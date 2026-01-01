using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI.Rendering
{
    /// <summary>
    /// Extremely small software 3D viewer control. It is not meant to be fast,
    /// just good enough to preview LOD meshes with their atlas textures.
    /// </summary>
    public class SoftwareMeshViewerControl : Control
    {
        /// <summary>
        /// Selection event raised when the user picks one or more faces in the 3D view.
        /// The indices are mesh-vertex indices (matching Mesh.Vertices).
        /// </summary>
        public event EventHandler<MeshSelectionEventArgs>? MeshSelectionChanged;

        /// <summary>
        /// Small helper describing which vertices were just selected from the 3D preview.
        /// </summary>
        public sealed class MeshSelectionEventArgs : EventArgs
        {
            public IReadOnlyList<int> VertexIndices { get; }
            public bool Additive { get; }

            public MeshSelectionEventArgs(IReadOnlyList<int> vertexIndices, bool additive)
            {
                VertexIndices = vertexIndices;
                Additive = additive;
            }
        }

        private Mesh? _mesh;
        private Bitmap? _texture;
        private Bitmap? _backBuffer;
        private float[]? _depthBuffer;

        // Fast raster backbuffer (ARGB) to avoid per-pixel Bitmap.SetPixel.
        private int[]? _colorBuffer;

        // Cached texture pixels (Format32bppArgb) for fast sampling.
        private int[]? _texturePixels;
        private int _texturePixelsW;
        private int _texturePixelsH;

        // Cached projected vertices to reduce per-frame allocations.
        private ScreenVertex[]? _screenVertsCache;

        // Coalesced/throttled redraw requests (useful during UV drags).
        private readonly System.Windows.Forms.Timer _renderTimer;
        private volatile bool _renderPending;

        private float _yaw = 0.6f;
        private float _pitch = -0.3f;
        private float _distance = 4.0f;
        private float _fovDegrees = 45.0f;

        private bool _isDragging;
        private Point _lastMouse;
        private MouseButtons _dragButton;

        // Indices of vertices currently selected from the 3D view.
        private readonly HashSet<int> _selectedVertexIndices = new HashSet<int>();

        // 3D view: rotation/navigation gizmo (no translate gizmo).
        private bool _isNavGizmoDragging;
        private Point _navGizmoDragStartMouse;
        private float _navGizmoStartYaw;
        private float _navGizmoStartPitch;
        private Rectangle _navGizmoBounds;


        private enum NavGizmoAxis { None, X, Y, Z }
        private NavGizmoAxis _navGizmoDragAxis = NavGizmoAxis.None;
        // When true, right-click picks faces and drives the UV editor.
        private bool _editMode = true;

        // When true, the viewer renders a simple wireframe instead of fully
        // shaded triangles. This improves responsiveness for complex meshes and
        // mirrors the "wireframe" shading mode available in many DCC tools. Use
        // the W key to toggle this mode. Defaults to false (shaded mode).
        private bool _wireframe = false;
        // Adjacency helpers for selection: map vertices to faces and map
        // quantised positions to the set of vertices that share that position.
        // This allows picking a triangle in the 3D view and expanding the
        // selection to all connected faces *and* any duplicated back‑side
        // vertices that occupy the same position.
        private Dictionary<int, List<int>>? _vertexToFaces;
        private Dictionary<(int, int, int), List<int>>? _positionToVertices;
        private Dictionary<int, (int, int, int)>? _vertexToPositionKey;

        // Cached matrices for rendering/picking. These are computed each frame
        // in RenderMesh() but are also useful for gizmo hit-testing.
        private Matrix4x4 _lastWorldViewProj;
        private bool _hasLastWorldViewProj;


        public Mesh? Mesh
        {
            get => _mesh;
            set
            {
                _mesh = value;
                // Reset camera when a new mesh is loaded so it fits nicely
                _yaw = 0.6f;
                _pitch = -0.3f;
                _distance = 3.0f;

                // Clear any previous selection when a new mesh is assigned.
                _selectedVertexIndices.Clear();

                // Rebuild adjacency tables so selection expansion can find
                // connected faces and back‑side vertices that share positions.
                RebuildSelectionAdjacency();

                _isNavGizmoDragging = false;
                _navGizmoBounds = Rectangle.Empty;

                Invalidate();
            }
        }

        public Bitmap? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                RebuildTextureCache();
                Invalidate();
            }
        }


                public float Yaw
        {
            get => _yaw;
            set
            {
                _yaw = value;
                Invalidate();
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = Math.Clamp(value, -1.5f, 1.5f);
                Invalidate();
            }
        }

        public float Distance
        {
            get => _distance;
            set
            {
                _distance = Math.Clamp(value, 0.5f, 100.0f);
                Invalidate();
            }
        }

        /// <summary>
        /// When enabled, right-click in the 3D view will pick faces / vertices.
        /// </summary>
        public bool EditMode
        {
            get => _editMode;
            set
            {
                _editMode = value;
                Invalidate();
            }
        }

        
        /// <summary>
        /// Rebuilds adjacency structures used for expanding 3D picks into full
        /// face patches and their back‑side counterparts.
        /// </summary>
        private void RebuildSelectionAdjacency()
        {
            _vertexToFaces = null;
            _positionToVertices = null;
            _vertexToPositionKey = null;

            if (_mesh == null || _mesh.Vertices.Length == 0 || _mesh.Indices.Length < 3)
                return;

            var verts = _mesh.Vertices;
            int vertexCount = verts.Length;
            int[] indices = _mesh.Indices;
            int faceCount = indices.Length / 3;
            if (faceCount == 0)
                return;

            var vertexToFaces = new Dictionary<int, List<int>>(vertexCount);
            for (int v = 0; v < vertexCount; v++)
            {
                vertexToFaces[v] = new List<int>();
            }

            for (int face = 0; face < faceCount; face++)
            {
                int baseIndex = face * 3;
                for (int k = 0; k < 3; k++)
                {
                    int vi = indices[baseIndex + k];
                    if ((uint)vi >= (uint)vertexCount)
                        continue;
                    vertexToFaces[vi].Add(face);
                }
            }

            const float quantiseScale = 1000.0f;
            var positionToVertices = new Dictionary<(int, int, int), List<int>>();
            var vertexToPositionKey = new Dictionary<int, (int, int, int)>(vertexCount);
            for (int v = 0; v < vertexCount; v++)
            {
                var pos = verts[v].Position;
                int qx = (int)Math.Round(pos.X * quantiseScale);
                int qy = (int)Math.Round(pos.Y * quantiseScale);
                int qz = (int)Math.Round(pos.Z * quantiseScale);
                var key = (qx, qy, qz);
                if (!positionToVertices.TryGetValue(key, out var list))
                {
                    list = new List<int>();
                    positionToVertices[key] = list;
                }
                list.Add(v);
                vertexToPositionKey[v] = key;
            }

            _vertexToFaces = vertexToFaces;
            _positionToVertices = positionToVertices;
            _vertexToPositionKey = vertexToPositionKey;
        }

        /// <summary>
        /// Starting from a single picked triangle, expand to all connected faces
        /// and any duplicate back‑side vertices that share positions with those
        /// faces. Returns the set of vertex indices that should be selected.
        /// </summary>
        private HashSet<int> ExpandSelectionFromTriangle(int v0, int v1, int v2)
        {
            var result = new HashSet<int>();

            if (_mesh == null ||
                _vertexToFaces == null ||
                _positionToVertices == null ||
                _vertexToPositionKey == null)
            {
                if (v0 >= 0) result.Add(v0);
                if (v1 >= 0) result.Add(v1);
                if (v2 >= 0) result.Add(v2);
                return result;
            }

            int vertexCount = _mesh.Vertices.Length;
            int[] indices = _mesh.Indices;
            int faceCount = indices.Length / 3;

            var pendingFaces = new Queue<int>();
            var visitedFaces = new HashSet<int>();

            void EnqueueFacesForVertex(int v)
            {
                if ((uint)v >= (uint)vertexCount)
                    return;

                if (_vertexToFaces.TryGetValue(v, out var facesForVertex))
                {
                    foreach (int f in facesForVertex)
                    {
                        if (visitedFaces.Add(f))
                            pendingFaces.Enqueue(f);
                    }
                }

                if (_vertexToPositionKey.TryGetValue(v, out var key) &&
                    _positionToVertices.TryGetValue(key, out var samePosVertices))
                {
                    foreach (int v2Index in samePosVertices)
                    {
                        if (result.Add(v2Index) &&
                            _vertexToFaces.TryGetValue(v2Index, out var facesForV2))
                        {
                            foreach (int f2 in facesForV2)
                            {
                                if (visitedFaces.Add(f2))
                                    pendingFaces.Enqueue(f2);
                            }
                        }
                    }
                }
            }

            if (v0 >= 0) { result.Add(v0); EnqueueFacesForVertex(v0); }
            if (v1 >= 0) { result.Add(v1); EnqueueFacesForVertex(v1); }
            if (v2 >= 0) { result.Add(v2); EnqueueFacesForVertex(v2); }

            while (pendingFaces.Count > 0)
            {
                int face = pendingFaces.Dequeue();
                if (face < 0 || face >= faceCount)
                    continue;

                int baseIndex = face * 3;
                for (int k = 0; k < 3; k++)
                {
                    int v = indices[baseIndex + k];
                    if (!result.Add(v))
                        continue;

                    EnqueueFacesForVertex(v);
                }
            }

            return result;
        }

public SoftwareMeshViewerControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.LightGray;

            // Enable focus on click so we can receive key events such as W for
            // toggling wireframe mode. Controls in WinForms do not receive
            // keyboard events unless they are focusable and have focus.
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;

            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += (_, __) =>
            {
                _renderTimer.Stop();
                if (_renderPending && !IsDisposed)
                {
                    _renderPending = false;
                    Invalidate();
                }
            };
        }

        /// <summary>
        /// Coalesces rapid redraw requests (e.g., UV drags) into a ~60 FPS repaint cadence.
        /// </summary>
        public void RequestRender()
        {
            if (IsDisposed)
                return;

            _renderPending = true;
            if (!_renderTimer.Enabled)
                _renderTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            if (_mesh == null || _mesh.Vertices.Length == 0 || _mesh.Indices.Length == 0 || _texture == null)
            {
                e.Graphics.Clear(BackColor);
                using var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString("Load a mesh (.obj) and atlas texture to preview.", Font, Brushes.Black, ClientRectangle, fmt);
                return;
            }

            EnsureBuffers();

            using (var g = Graphics.FromImage(_backBuffer!))
            {
                g.Clear(BackColor);
            }

            Array.Fill(_depthBuffer!, float.PositiveInfinity);

            RenderMesh();

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.DrawImage(_backBuffer!, ClientRectangle);

            // Overlay: navigation/rotation gizmo (top-right).
            DrawNavigationGizmo(e.Graphics);
        }

        private void EnsureBuffers()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            if (w <= 0 || h <= 0)
                return;

            if (_backBuffer == null || _backBuffer.Width != w || _backBuffer.Height != h)
            {
                _backBuffer?.Dispose();
                _backBuffer = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                _depthBuffer = new float[w * h];
                _colorBuffer = new int[w * h];
            }
            else
            {
                if (_depthBuffer == null || _depthBuffer.Length != w * h)
                    _depthBuffer = new float[w * h];
                if (_colorBuffer == null || _colorBuffer.Length != w * h)
                    _colorBuffer = new int[w * h];
            }
        }

        private void RebuildTextureCache()
        {
            _texturePixels = null;
            _texturePixelsW = 0;
            _texturePixelsH = 0;

            if (_texture == null)
                return;

            Bitmap src = _texture;
            Bitmap? tmp = null;
            try
            {
                if (src.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    tmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(tmp))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, tmp.Width, tmp.Height));
                    }
                    src = tmp;
                }

                _texturePixelsW = src.Width;
                _texturePixelsH = src.Height;
                _texturePixels = new int[_texturePixelsW * _texturePixelsH];

                var rect = new Rectangle(0, 0, _texturePixelsW, _texturePixelsH);
                var data = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < _texturePixelsH; y++)
                    {
                        IntPtr rowPtr = IntPtr.Add(data.Scan0, y * data.Stride);
                        Marshal.Copy(rowPtr, _texturePixels, y * _texturePixelsW, _texturePixelsW);
                    }
                }
                finally
                {
                    src.UnlockBits(data);
                }
            }
            catch
            {
                _texturePixels = null;
                _texturePixelsW = 0;
                _texturePixelsH = 0;
            }
            finally
            {
                tmp?.Dispose();
            }
        }

        private void BlitColorBufferToBackBuffer()
        {
            if (_backBuffer == null || _colorBuffer == null)
                return;

            int w = _backBuffer.Width;
            int h = _backBuffer.Height;
            if (w <= 0 || h <= 0 || _colorBuffer.Length < w * h)
                return;

            var rect = new Rectangle(0, 0, w, h);
            var data = _backBuffer.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    IntPtr rowPtr = IntPtr.Add(data.Scan0, y * data.Stride);
                    Marshal.Copy(_colorBuffer, y * w, rowPtr, w);
                }
            }
            finally
            {
                _backBuffer.UnlockBits(data);
            }
        }
private Matrix4x4 BuildViewMatrix(float distance)
{
    // Z-up orbit camera:
    //  - yaw   : azimuth around global Z (vertical) axis
    //  - pitch : elevation (clamped) while keeping the camera "up" locked to +Z
    //
    // This keeps Z as the world up-axis (GTA-style) and prevents view roll so the
    // top of the viewport remains aligned with +Z.
    float cosPitch = MathF.Cos(_pitch);
    float sinPitch = MathF.Sin(_pitch);
    float cosYaw = MathF.Cos(_yaw);
    float sinYaw = MathF.Sin(_yaw);

    // When yaw=0 and pitch=0, the camera sits on -Y looking toward the origin, with +Z as up.
    Vector3 camPos = new Vector3(
        distance * cosPitch * sinYaw,
        -distance * cosPitch * cosYaw,
        distance * sinPitch);

    return Matrix4x4.CreateLookAt(
        camPos,
        Vector3.Zero,
        Vector3.UnitZ);
}
private Matrix4x4 BuildWorldViewProj(int width, int height)
        {
            if (_mesh == null)
                return Matrix4x4.Identity;

            float aspect = (float)width / Math.Max(1, height);
            float fovRad = _fovDegrees * (float)Math.PI / 180f;

            float scale = 1.0f / Math.Max(0.0001f, _mesh.BoundingRadius);
            float distance = _distance;

            // Keep the mesh fixed in world space (except centering + scaling) and orbit the camera.
            // This ensures viewport rotations behave like a camera orbit and do not "tip" the mesh.
            var world =
                Matrix4x4.CreateTranslation(-_mesh.Center) *
                Matrix4x4.CreateScale(scale);

            var view = BuildViewMatrix(distance);

            var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                fovRad,
                aspect,
                0.1f,
                1000.0f);

            return world * view * proj;
        }

        private bool TryProjectToScreen(Vector3 pos, Matrix4x4 worldViewProj, int width, int height, out PointF screen)
        {
            Vector4 clip = Vector4.Transform(new Vector4(pos, 1.0f), worldViewProj);
            if (Math.Abs(clip.W) < 1e-6f)
                clip.W = 1e-6f;
            float invW = 1.0f / clip.W;
            float ndcX = clip.X * invW;
            float ndcY = clip.Y * invW;

            float sx = (ndcX * 0.5f + 0.5f) * (width - 1);
            float sy = (1.0f - (ndcY * 0.5f + 0.5f)) * (height - 1);
            screen = new PointF(sx, sy);
            return true;
        }

        private Vector3 GetSelectionPivotLocal()
        {
            if (_mesh == null || _selectedVertexIndices.Count == 0)
                return Vector3.Zero;

            Vector3 sum = Vector3.Zero;
            int count = 0;
            foreach (int idx in _selectedVertexIndices)
            {
                if ((uint)idx >= (uint)_mesh.Vertices.Length)
                    continue;
                sum += _mesh.Vertices[idx].Position;
                count++;
            }
            return count > 0 ? (sum / count) : Vector3.Zero;
        }

        private Rectangle GetNavigationGizmoBounds()
        {
            const int size = 80;
            const int margin = 8;

            int w = Math.Max(1, ClientSize.Width);
            int x = w - size - margin;
            int y = margin;
            return new Rectangle(x, y, size, size);
        }

        private bool HitTestNavigationGizmo(Point p)
        {
            // Use cached bounds from the last paint when available, so hit tests
            // remain consistent even if we get mouse events between repaints.
            Rectangle r = _navGizmoBounds;
            if (r.Width <= 0 || r.Height <= 0)
                r = GetNavigationGizmoBounds();
            return r.Contains(p);
        }


        private NavGizmoAxis HitTestNavigationGizmoAxis(Point p)
        {
            // Determine whether the user started dragging on a specific axis
            // inside the navigation gizmo. This allows axis-constrained orbiting
            // (e.g., dragging X spins around the mesh instead of introducing pitch).
            Rectangle r = _navGizmoBounds;
            if (r.Width <= 0 || r.Height <= 0)
                r = GetNavigationGizmoBounds();

            float cx = r.Left + r.Width * 0.5f;
            float cy = r.Top + r.Height * 0.5f;
            float radius = Math.Min(r.Width, r.Height) * 0.38f;

            PointF mp = new PointF(p.X, p.Y);
            PointF cp = new PointF(cx, cy);

            // Near the center we prefer free orbit.
            float centerDist = PointDistance(mp, cp);
            if (centerDist <= 12.0f)
                return NavGizmoAxis.None;

            Matrix4x4 view = BuildViewMatrix(1.0f);

            Vector3 ax = Vector3.TransformNormal(Vector3.UnitX, view);
            Vector3 ay = Vector3.TransformNormal(Vector3.UnitY, view);
            Vector3 az = Vector3.TransformNormal(Vector3.UnitZ, view);

            var axes = new (NavGizmoAxis axis, Vector3 v)[]
            {
                (NavGizmoAxis.X, ax),
                (NavGizmoAxis.Y, ay),
                (NavGizmoAxis.Z, az),
            };

            float best = float.MaxValue;
            NavGizmoAxis bestAxis = NavGizmoAxis.None;

            foreach (var a in axes)
            {
                float ex = cx + a.v.X * radius;
                float ey = cy - a.v.Y * radius;
                PointF ep = new PointF(ex, ey);

                float dLine = DistancePointToSegment(mp, cp, ep);
                float dEnd = PointDistance(mp, ep);
                float d = Math.Min(dLine, dEnd);

                if (d < best)
                {
                    best = d;
                    bestAxis = a.axis;
                }
            }

            const float threshold = 8.0f;
            return best <= threshold ? bestAxis : NavGizmoAxis.None;
        }

        private static float PointDistance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        private static float DistancePointToSegment(PointF p, PointF a, PointF b)
        {
            // Project p onto the segment a->b and return the distance.
            Vector2 ap = new Vector2(p.X - a.X, p.Y - a.Y);
            Vector2 ab = new Vector2(b.X - a.X, b.Y - a.Y);

            float abLenSq = ab.LengthSquared();
            if (abLenSq <= 1e-6f)
                return ap.Length();

            float t = Vector2.Dot(ap, ab) / abLenSq;
            t = Math.Clamp(t, 0.0f, 1.0f);

            Vector2 proj = new Vector2(a.X, a.Y) + (ab * t);
            Vector2 diff = new Vector2(p.X, p.Y) - proj;
            return diff.Length();
        }

        private void BeginNavigationGizmoDrag(Point mouse)
        {
            _isNavGizmoDragging = true;
            _navGizmoDragStartMouse = mouse;
            _navGizmoStartYaw = _yaw;
            _navGizmoStartPitch = _pitch;
            _navGizmoDragAxis = HitTestNavigationGizmoAxis(mouse);
            Capture = true;
            Cursor = Cursors.Hand;
        }

        private void UpdateNavigationGizmoDrag(Point mouse)
        {
            if (!_isNavGizmoDragging)
                return;

            int dx = mouse.X - _navGizmoDragStartMouse.X;
            int dy = mouse.Y - _navGizmoDragStartMouse.Y;

            float sens = 0.01f;

            switch (_navGizmoDragAxis)
{
    case NavGizmoAxis.X:
    case NavGizmoAxis.Z:
        // Spin/orbit around the mesh without changing elevation (yaw only).
        // With a Z-up camera, this is a rotation around the global +Z axis.
        _yaw = _navGizmoStartYaw + dx * sens;
        Pitch = _navGizmoStartPitch;
        break;

    case NavGizmoAxis.Y:
        // Elevation change only (pitch only).
        _yaw = _navGizmoStartYaw;
        Pitch = _navGizmoStartPitch + (-dy * sens);
        break;

    case NavGizmoAxis.None:
    default:
        // Free orbit.
        _yaw = _navGizmoStartYaw + dx * sens;
        Pitch = _navGizmoStartPitch + (-dy * sens);
        break;
}

Invalidate();}

        private void EndNavigationGizmoDrag()
        {
            if (!_isNavGizmoDragging)
                return;

            _isNavGizmoDragging = false;
            Capture = false;
            Cursor = Cursors.Default;
            _navGizmoDragAxis = NavGizmoAxis.None;
            Invalidate();
        }

        private void DrawNavigationGizmo(Graphics g)
        {
            Rectangle r = GetNavigationGizmoBounds();
            _navGizmoBounds = r;

            float cx = r.Left + r.Width * 0.5f;
            float cy = r.Top + r.Height * 0.5f;
            float radius = Math.Min(r.Width, r.Height) * 0.38f;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var bgBrush = new SolidBrush(Color.FromArgb(150, 18, 18, 18));
            using var circlePen = new Pen(Color.FromArgb(160, 200, 200, 200), 1.0f);
            g.FillEllipse(bgBrush, cx - radius - 10, cy - radius - 10, (radius + 10) * 2, (radius + 10) * 2);
            g.DrawEllipse(circlePen, cx - radius - 10, cy - radius - 10, (radius + 10) * 2, (radius + 10) * 2);

            // Render a small axis triad oriented by the current camera rotation.
            // We transform world axes into camera space (inverse yaw/pitch), then
            // project into 2D with a simple orthographic mapping.
            Matrix4x4 view = BuildViewMatrix(1.0f);

            Vector3 ax = Vector3.TransformNormal(Vector3.UnitX, view);
            Vector3 ay = Vector3.TransformNormal(Vector3.UnitY, view);
            Vector3 az = Vector3.TransformNormal(Vector3.UnitZ, view);

            var axes = new (string name, Vector3 v, Pen pen)[]
            {
                ("X", ax, new Pen(Color.Red, 2.0f)),
                ("Y", ay, new Pen(Color.LimeGreen, 2.0f)),
                ("Z", az, new Pen(Color.DodgerBlue, 2.0f)),
            };

            // Draw back-most axes first for a subtle depth cue.
            Array.Sort(axes, (a, b) => a.v.Z.CompareTo(b.v.Z));

            using var labelFont = new Font(Font.FontFamily, 7.0f, FontStyle.Bold);
            using var labelBrush = new SolidBrush(Color.Gainsboro);

            foreach (var a in axes)
            {
                // Map camera-space (X,Y) to screen. Y is inverted because screen Y grows down.
                float ex = cx + a.v.X * radius;
                float ey = cy - a.v.Y * radius;

                g.DrawLine(a.pen, cx, cy, ex, ey);
                g.FillEllipse(Brushes.White, ex - 2, ey - 2, 4, 4);
                g.DrawString(a.name, labelFont, labelBrush, ex + 2, ey + 2);
                a.pen.Dispose();
            }

            // Center dot.
            g.FillEllipse(Brushes.White, cx - 2, cy - 2, 4, 4);
        }

        private struct ScreenVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float InvW;
            public float UOverW;
            public float VOverW;
        }

        
        /// <summary>
        /// Very small CPU-side triangle picker. It mirrors the math used in <see cref="RenderMesh"/>
        /// to project vertices to screen space, then performs a barycentric hit test in that space
        /// and returns the front-most triangle under the given pixel (if any).
        /// </summary>
        private bool TryPickTriangle(Point screenPoint, out int v0Index, out int v1Index, out int v2Index)
        {
            v0Index = v1Index = v2Index = -1;

            if (_mesh == null || _mesh.Vertices.Length == 0 || _mesh.Indices.Length < 3)
                return false;

            int width = Math.Max(1, ClientSize.Width);
            int height = Math.Max(1, ClientSize.Height);

            var vertices = _mesh.Vertices;
            int vertexCount = vertices.Length;
            if (vertexCount == 0)
                return false;

            if (_screenVertsCache == null || _screenVertsCache.Length != vertexCount)
                _screenVertsCache = new ScreenVertex[vertexCount];

            var screenVerts = _screenVertsCache;

            float aspect = (float)width / Math.Max(1, height);
            float fovRad = _fovDegrees * (float)Math.PI / 180f;

            float scale = 1.0f / Math.Max(0.0001f, _mesh.BoundingRadius);
            float distance = _distance;

            var world =
                Matrix4x4.CreateTranslation(-_mesh.Center) *
                Matrix4x4.CreateScale(scale);

            var view = BuildViewMatrix(distance);
var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                fovRad,
                aspect,
                0.1f,
                1000.0f);

            var worldViewProj = world * view * proj;

            for (int i = 0; i < vertexCount; i++)
            {
                var v = vertices[i];
                var p = new Vector4(v.Position, 1.0f);
                Vector4 clip = Vector4.Transform(p, worldViewProj);
                if (Math.Abs(clip.W) < 1e-6f) clip.W = 1e-6f;
                float invW = 1.0f / clip.W;

                float ndcX = clip.X * invW;
                float ndcY = clip.Y * invW;
                float ndcZ = clip.Z * invW;

                float sx = (ndcX * 0.5f + 0.5f) * (width - 1);
                float sy = (1.0f - (ndcY * 0.5f + 0.5f)) * (height - 1);

                screenVerts[i] = new ScreenVertex
                {
                    X = sx,
                    Y = sy,
                    Z = ndcZ,
                    InvW = invW,
                    UOverW = v.TexCoord.X * invW,
                    VOverW = (1.0f - v.TexCoord.Y) * invW
                };
            }

            float bestDepth = float.PositiveInfinity;
            bool found = false;

            float px = screenPoint.X + 0.5f;
            float py = screenPoint.Y + 0.5f;

            for (int i = 0; i < _mesh.Indices.Length; i += 3)
            {
                int i0 = _mesh.Indices[i];
                int i1 = _mesh.Indices[i + 1];
                int i2 = _mesh.Indices[i + 2];

                if ((uint)i0 >= screenVerts.Length ||
                    (uint)i1 >= screenVerts.Length ||
                    (uint)i2 >= screenVerts.Length)
                {
                    continue;
                }

                var sv0 = screenVerts[i0];
                var sv1 = screenVerts[i1];
                var sv2 = screenVerts[i2];

                float denom = ((sv1.Y - sv2.Y) * (sv0.X - sv2.X) + (sv2.X - sv1.X) * (sv0.Y - sv2.Y));
                if (Math.Abs(denom) < 1e-8f)
                    continue;
                float invDenom = 1.0f / denom;

                float w0 = ((sv1.Y - sv2.Y) * (px - sv2.X) + (sv2.X - sv1.X) * (py - sv2.Y)) * invDenom;
                if (w0 < 0) continue;
                float w1 = ((sv2.Y - sv0.Y) * (px - sv2.X) + (sv0.X - sv2.X) * (py - sv2.Y)) * invDenom;
                if (w1 < 0) continue;
                float w2 = 1.0f - w0 - w1;
                if (w2 < 0) continue;

                float depth = w0 * sv0.Z + w1 * sv1.Z + w2 * sv2.Z;

                if (depth < bestDepth)
                {
                    bestDepth = depth;
                    v0Index = i0;
                    v1Index = i1;
                    v2Index = i2;
                    found = true;
                }
            }

            return found;
        }

        private void RenderMesh()
        {
            if (_backBuffer == null || _depthBuffer == null || _mesh == null || _texture == null)
                return;

            int width = _backBuffer.Width;
            int height = _backBuffer.Height;

            var vertices = _mesh.Vertices;
            int vertexCount = vertices.Length;

            if (_screenVertsCache == null || _screenVertsCache.Length != vertexCount)
                _screenVertsCache = new ScreenVertex[vertexCount];

            var screenVerts = _screenVertsCache;

            float aspect = (float)width / Math.Max(1, height);
            float fovRad = _fovDegrees * (float)Math.PI / 180f;

            // Normalize the mesh so its bounding sphere fits roughly into a unit radius.
            float scale = 1.0f / Math.Max(0.0001f, _mesh.BoundingRadius);
            float distance = _distance;

            var world =
                Matrix4x4.CreateTranslation(-_mesh.Center) *
                Matrix4x4.CreateScale(scale);

            var view = BuildViewMatrix(distance);
var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                fovRad,
                aspect,
                0.1f,
                1000.0f);

            var worldViewProj = world * view * proj;

            _lastWorldViewProj = worldViewProj;
            _hasLastWorldViewProj = true;

            // Project all vertices to screen space once. We cache W for UV interpolation.
            for (int i = 0; i < vertexCount; i++)
            {
                var v = vertices[i];
                var p = new Vector4(v.Position, 1.0f);
                Vector4 clip = Vector4.Transform(p, worldViewProj);
                if (Math.Abs(clip.W) < 1e-6f) clip.W = 1e-6f;
                float invW = 1.0f / clip.W;

                float ndcX = clip.X * invW;
                float ndcY = clip.Y * invW;
                float ndcZ = clip.Z * invW;

                float sx = (ndcX * 0.5f + 0.5f) * (width - 1);
                float sy = (1.0f - (ndcY * 0.5f + 0.5f)) * (height - 1);

                screenVerts[i] = new ScreenVertex
                {
                    X = sx,
                    Y = sy,
                    Z = ndcZ,
                    InvW = invW,
                    UOverW = v.TexCoord.X * invW,
                    // Flip V for bitmap coordinates
                    VOverW = (1.0f - v.TexCoord.Y) * invW
                };
            }

            if (_wireframe)
            {
                // Wireframe mode: draw only triangle outlines. Use a simple
                // constant colour derived from the texture average to give a sense
                // of shading without filling every pixel. Sampling every triangle
                // would be expensive; instead sample a single texel from the
                // atlas as a base colour.
                using (var g = Graphics.FromImage(_backBuffer!))
                {
                    g.Clear(BackColor);

                    // Sample the center of the texture to derive a neutral tone
                    int txCenter = _texture.Width / 2;
                    int tyCenter = _texture.Height / 2;
                    Color baseColour = Color.Gray;
                    if (_texturePixels != null && _texturePixelsW > 0 && _texturePixelsH > 0)
                    {
                        int cx = Math.Clamp(txCenter, 0, _texturePixelsW - 1);
                        int cy = Math.Clamp(tyCenter, 0, _texturePixelsH - 1);
                        baseColour = Color.FromArgb(_texturePixels[cy * _texturePixelsW + cx]);
                    }
                    else
                    {
                        try
                        {
                            baseColour = _texture.GetPixel(txCenter, tyCenter);
                        }
                        catch
                        {
                            baseColour = Color.Gray;
                        }
                    }
                    // Dim the colour so outlines are visible on the light background
                    baseColour = Color.FromArgb(160, baseColour);
                    using var pen = new Pen(baseColour, 1.0f);
                    // Draw each triangle
                    for (int i = 0; i < _mesh.Indices.Length; i += 3)
                    {
                        int i0 = _mesh.Indices[i];
                        int i1 = _mesh.Indices[i + 1];
                        int i2 = _mesh.Indices[i + 2];

                        if ((uint)i0 >= (uint)screenVerts.Length ||
                            (uint)i1 >= (uint)screenVerts.Length ||
                            (uint)i2 >= (uint)screenVerts.Length)
                        {
                            continue;
                        }

                        var v0 = screenVerts[i0];
                        var v1 = screenVerts[i1];
                        var v2 = screenVerts[i2];

                        g.DrawLine(pen, v0.X, v0.Y, v1.X, v1.Y);
                        g.DrawLine(pen, v1.X, v1.Y, v2.X, v2.Y);
                        g.DrawLine(pen, v2.X, v2.Y, v0.X, v0.Y);
                    }

                    // Highlight selected faces in orange so they stand out
                    if (_selectedVertexIndices.Count > 0)
                    {
                        using var selPen = new Pen(Color.Orange, 2.0f);
                        for (int i = 0; i < _mesh.Indices.Length; i += 3)
                        {
                            int i0 = _mesh.Indices[i];
                            int i1 = _mesh.Indices[i + 1];
                            int i2 = _mesh.Indices[i + 2];
                            if (!_selectedVertexIndices.Contains(i0) ||
                                !_selectedVertexIndices.Contains(i1) ||
                                !_selectedVertexIndices.Contains(i2))
                            {
                                continue;
                            }
                            var v0 = screenVerts[i0];
                            var v1 = screenVerts[i1];
                            var v2 = screenVerts[i2];
                            g.DrawLine(selPen, v0.X, v0.Y, v1.X, v1.Y);
                            g.DrawLine(selPen, v1.X, v1.Y, v2.X, v2.Y);
                            g.DrawLine(selPen, v2.X, v2.Y, v0.X, v0.Y);
                        }
                    }

					// Intentionally no transform gizmo in the 3D preview.
					// (Selection transforms are handled in the 2D UV/texture view.)
                }

                return;
            }

            // Shaded mode: rasterize triangles into the backbuffer via simple
            // software rendering with depth buffering and UV interpolation.
            if (_colorBuffer == null)
                return;

            if (_texturePixels == null || _texturePixelsW != _texture.Width || _texturePixelsH != _texture.Height)
                RebuildTextureCache();

            int texW = _texturePixelsW > 0 ? _texturePixelsW : _texture.Width;
            int texH = _texturePixelsH > 0 ? _texturePixelsH : _texture.Height;

            Array.Fill(_colorBuffer, BackColor.ToArgb());
            for (int i = 0; i < _mesh.Indices.Length; i += 3)
            {
                int i0 = _mesh.Indices[i];
                int i1 = _mesh.Indices[i + 1];
                int i2 = _mesh.Indices[i + 2];

                if ((uint)i0 >= (uint)screenVerts.Length ||
                    (uint)i1 >= (uint)screenVerts.Length ||
                    (uint)i2 >= (uint)screenVerts.Length)
                {
                    continue;
                }

                var sv0 = screenVerts[i0];
                var sv1 = screenVerts[i1];
                var sv2 = screenVerts[i2];

                RasterizeTriangle(sv0, sv1, sv2, texW, texH);
            }

            BlitColorBufferToBackBuffer();

            // Draw orange outline on selected faces so the user can see what is
            // currently being edited in the UV editor.
            if (_selectedVertexIndices.Count > 0)
            {
                using var g = Graphics.FromImage(_backBuffer!);
                using var pen = new Pen(Color.Orange, 2.0f);
                for (int i = 0; i < _mesh.Indices.Length; i += 3)
                {
                    int i0 = _mesh.Indices[i];
                    int i1 = _mesh.Indices[i + 1];
                    int i2 = _mesh.Indices[i + 2];

                    if (!_selectedVertexIndices.Contains(i0) ||
                        !_selectedVertexIndices.Contains(i1) ||
                        !_selectedVertexIndices.Contains(i2))
                    {
                        continue;
                    }
                    var v0 = screenVerts[i0];
                    var v1 = screenVerts[i1];
                    var v2 = screenVerts[i2];
                    g.DrawLine(pen, v0.X, v0.Y, v1.X, v1.Y);
                    g.DrawLine(pen, v1.X, v1.Y, v2.X, v2.Y);
                    g.DrawLine(pen, v2.X, v2.Y, v0.X, v0.Y);
                }
            }
        }

        private void RasterizeTriangle(ScreenVertex v0, ScreenVertex v1, ScreenVertex v2, int texW, int texH)
        {
            if (_backBuffer == null || _depthBuffer == null || _texture == null || _colorBuffer == null)
                return;

            int width = _backBuffer.Width;
            int height = _backBuffer.Height;

            int minX = (int)Math.Floor(Math.Min(v0.X, Math.Min(v1.X, v2.X)));
            int maxX = (int)Math.Ceiling(Math.Max(v0.X, Math.Max(v1.X, v2.X)));
            int minY = (int)Math.Floor(Math.Min(v0.Y, Math.Min(v1.Y, v2.Y)));
            int maxY = (int)Math.Ceiling(Math.Max(v0.Y, Math.Max(v1.Y, v2.Y)));

            if (minX < 0) minX = 0;
            if (minY < 0) minY = 0;
            if (maxX >= width) maxX = width - 1;
            if (maxY >= height) maxY = height - 1;

            float denom = ((v1.Y - v2.Y) * (v0.X - v2.X) + (v2.X - v1.X) * (v0.Y - v2.Y));
            if (Math.Abs(denom) < 1e-8f) return;
            float invDenom = 1.0f / denom;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    float w0 = ((v1.Y - v2.Y) * (px - v2.X) + (v2.X - v1.X) * (py - v2.Y)) * invDenom;
                    if (w0 < 0) continue;
                    float w1 = ((v2.Y - v0.Y) * (px - v2.X) + (v0.X - v2.X) * (py - v2.Y)) * invDenom;
                    if (w1 < 0) continue;
                    float w2 = 1.0f - w0 - w1;
                    if (w2 < 0) continue;

                    float invW = w0 * v0.InvW + w1 * v1.InvW + w2 * v2.InvW;
                    if (invW <= 0) continue;

                    float depth = w0 * v0.Z + w1 * v1.Z + w2 * v2.Z;

                    int idx = y * width + x;
                    if (depth >= _depthBuffer[idx]) continue;
                    _depthBuffer[idx] = depth;

                    
                    float uOverW = w0 * v0.UOverW + w1 * v1.UOverW + w2 * v2.UOverW;
                    float vOverW = w0 * v0.VOverW + w1 * v1.VOverW + w2 * v2.VOverW;

                    float u = uOverW / invW;
                    float v = vOverW / invW;

                    // Wrap UVs into [0,1) so meshes with tiled or out-of-range
                    // texture coordinates still render correctly in the preview.
                    u = u - (float)Math.Floor(u);
                    v = v - (float)Math.Floor(v);

                    int tx = (int)(u * (texW - 1));
                    int ty = (int)(v * (texH - 1));

                    // Clamp to the valid range to guard against tiny precision issues.
                    if (tx < 0) tx = 0;
                    else if (tx >= texW) tx = texW - 1;
                    if (ty < 0) ty = 0;
                    else if (ty >= texH) ty = texH - 1;


                    int sampleArgb;
                    if (_texturePixels != null && _texturePixelsW > 0 && _texturePixelsH > 0 && (uint)tx < (uint)_texturePixelsW && (uint)ty < (uint)_texturePixelsH)
                    {
                        sampleArgb = _texturePixels[ty * _texturePixelsW + tx];
                    }
                    else
                    {
                        Color sample = _texture.GetPixel(tx, ty);
                        sampleArgb = sample.ToArgb();
                    }
                    // Apply a simple depth‑based diffuse shading so surfaces further
                    // from the camera appear slightly darker. This approximates
                    // Lambert shading without per-pixel normals. NDC Z ranges
                    // roughly from -1 (near) to +1 (far).
                    float normDepth = (depth + 1.0f) * 0.5f;
                    if (normDepth < 0) normDepth = 0;
                    else if (normDepth > 1) normDepth = 1;
                    float shade = 0.3f + 0.7f * (1.0f - normDepth);

                    int a = (sampleArgb >> 24) & 0xFF;
                    int r0 = (sampleArgb >> 16) & 0xFF;
                    int g0 = (sampleArgb >> 8) & 0xFF;
                    int b0 = sampleArgb & 0xFF;

                    int r = (int)(r0 * shade);
                    int gCol = (int)(g0 * shade);
                    int b = (int)(b0 * shade);

                    _colorBuffer[idx] = (a << 24) | (r << 16) | (gCol << 8) | b;
                }
            }
        }

        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Acquire keyboard focus when the user clicks in the 3D view. Without
            // focus, keyboard events (e.g. W to toggle wireframe) will not be
            // routed to this control.
            Focus();

            if (e.Button == MouseButtons.Left)
            {
                // If the user clicks the navigation/rotation gizmo, rotate via the gizmo
                // instead of beginning a normal orbit drag.
                if (HitTestNavigationGizmo(e.Location))
                {
                    BeginNavigationGizmoDrag(e.Location);
                    return;
                }

                _isDragging = true;
                _dragButton = e.Button;
                _lastMouse = e.Location;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                _isDragging = true;
                _dragButton = e.Button;
                _lastMouse = e.Location;
            }
            else if (e.Button == MouseButtons.Right && _mesh != null && _editMode)
            {
                bool additive = (ModifierKeys & Keys.Shift) == Keys.Shift;

                if (TryPickTriangle(e.Location, out int v0, out int v1, out int v2))
                {
                    var expanded = ExpandSelectionFromTriangle(v0, v1, v2);

                    if (!additive)
                        _selectedVertexIndices.Clear();

                    foreach (int v in expanded)
                    {
                        _selectedVertexIndices.Add(v);
                    }

                    Invalidate();

                    var indices = new List<int>(expanded);
                    MeshSelectionChanged?.Invoke(this, new MeshSelectionEventArgs(indices, additive));
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && _isNavGizmoDragging)
            {
                EndNavigationGizmoDrag();
                return;
            }

            if (e.Button == _dragButton)
            {
                _isDragging = false;
                _dragButton = MouseButtons.None;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isNavGizmoDragging)
            {
                UpdateNavigationGizmoDrag(e.Location);
                return;
            }

            // Cursor affordance when hovering the navigation gizmo.
            if (!_isDragging && HitTestNavigationGizmo(e.Location))
            {
                Cursor = Cursors.Hand;
            }
            else if (!_isDragging)
            {
                Cursor = Cursors.Default;
            }

            if (_isDragging)
            {
                int dx = e.X - _lastMouse.X;
                int dy = e.Y - _lastMouse.Y;
                _lastMouse = e.Location;

                if (_dragButton == MouseButtons.Left)
                {
                    // Free orbit: yaw + pitch
                    _yaw += dx * 0.01f;
                    Pitch += -dy * 0.01f;
                }
                else if (_dragButton == MouseButtons.Middle)
                {
                    // Constrained orbit: rotate only around the vertical axis
                    // so dragging left/right spins the model around a fixed axis.
                    _yaw += dx * 0.01f;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            float zoomFactor = e.Delta > 0 ? 0.9f : 1.1f;
            Distance *= zoomFactor;
        }

        /// <summary>
        /// Toggle wireframe rendering on W key. Other keys can be added in a
        /// similar manner. Keyboard focus is acquired on mouse down so that
        /// this event fires when expected.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.W)
            {
                _wireframe = !_wireframe;
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderTimer?.Stop();
                _renderTimer?.Dispose();
                _backBuffer?.Dispose();
                _texture?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
