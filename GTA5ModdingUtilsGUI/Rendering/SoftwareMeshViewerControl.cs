using System;
using System.Drawing;
using System.Collections.Generic;
using System.Numerics;
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

        private float _yaw = 0.6f;
        private float _pitch = -0.3f;
        private float _distance = 4.0f;
        private float _fovDegrees = 45.0f;

        private bool _isDragging;
        private Point _lastMouse;
        private MouseButtons _dragButton;

        // Indices of vertices currently selected from the 3D view.
        private readonly HashSet<int> _selectedVertexIndices = new HashSet<int>();

        // When true, right-click picks faces and drives the UV editor.
        private bool _editMode = true;

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

                Invalidate();
            }
        }

        public Bitmap? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
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

        public SoftwareMeshViewerControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.LightGray;
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
        }

        private void EnsureBuffers()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            if (_backBuffer == null || _backBuffer.Width != w || _backBuffer.Height != h)
            {
                _backBuffer?.Dispose();
                _backBuffer = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _depthBuffer = new float[w * h];
            }
            else if (_depthBuffer == null || _depthBuffer.Length != w * h)
            {
                _depthBuffer = new float[w * h];
            }
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

            var screenVerts = new ScreenVertex[vertexCount];

            float aspect = (float)width / Math.Max(1, height);
            float fovRad = _fovDegrees * (float)Math.PI / 180f;

            float scale = 1.0f / Math.Max(0.0001f, _mesh.BoundingRadius);
            float distance = _distance;

            var world =
                Matrix4x4.CreateTranslation(-_mesh.Center) *
                Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateRotationY(_yaw) *
                Matrix4x4.CreateRotationX(_pitch);

            var view = Matrix4x4.CreateLookAt(
                new Vector3(0, 0, distance),
                Vector3.Zero,
                Vector3.UnitY);

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

            var screenVerts = new ScreenVertex[vertexCount];

            float aspect = (float)width / Math.Max(1, height);
            float fovRad = _fovDegrees * (float)Math.PI / 180f;

            // Normalize the mesh so its bounding sphere fits roughly into a unit radius.
            // This makes the preview more robust regardless of the mesh's original scale.
            float scale = 1.0f / Math.Max(0.0001f, _mesh.BoundingRadius);
            float distance = _distance;

            var world =
                Matrix4x4.CreateTranslation(-_mesh.Center) *
                Matrix4x4.CreateScale(scale) *
                Matrix4x4.CreateRotationY(_yaw) *
                Matrix4x4.CreateRotationX(_pitch);

            var view = Matrix4x4.CreateLookAt(
                new Vector3(0, 0, distance),
                Vector3.Zero,
                Vector3.UnitY);

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
                    // Flip V for GDI+ coordinates (bitmap origin is top-left)
                    VOverW = (1.0f - v.TexCoord.Y) * invW
                };
            }

            int texW = _texture.Width;
            int texH = _texture.Height;

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

                var v0 = screenVerts[i0];
                var v1 = screenVerts[i1];
                var v2 = screenVerts[i2];

                // Backface cull in screen space (disabled so both sides render)
                // float cross = (v1.X - v0.X) * (v2.Y - v0.Y) - (v1.Y - v0.Y) * (v2.X - v0.X);
                // if (cross >= 0) continue;

                RasterizeTriangle(v0, v1, v2, texW, texH);
            }

            // Overlay the currently selected face (or faces) as an orange outline,
            // so it is obvious which triangle is being edited.
            if (_selectedVertexIndices.Count > 0)
            {
                using (var g = Graphics.FromImage(_backBuffer!))
                using (var pen = new Pen(Color.Orange, 2.0f))
                {
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
        }

        private void RasterizeTriangle(ScreenVertex v0, ScreenVertex v1, ScreenVertex v2, int texW, int texH)
        {
            if (_backBuffer == null || _depthBuffer == null || _texture == null)
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


                    Color sample = _texture.GetPixel(tx, ty);
                    _backBuffer.SetPixel(x, y, sample);
                }
            }
        }

        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
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
                    if (!additive)
                        _selectedVertexIndices.Clear();

                    _selectedVertexIndices.Add(v0);
                    _selectedVertexIndices.Add(v1);
                    _selectedVertexIndices.Add(v2);

                    Invalidate();

                    var indices = new List<int> { v0, v1, v2 };
                    MeshSelectionChanged?.Invoke(this, new MeshSelectionEventArgs(indices, additive));
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == _dragButton)
            {
                _isDragging = false;
                _dragButton = MouseButtons.None;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backBuffer?.Dispose();
                _texture?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
