
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace GTA5ModdingUtilsGUI.Rendering
{
    /// <summary>
    /// Simple 2D UV editor that works alongside <see cref="SoftwareMeshViewerControl"/>.
    /// It lets you inspect and edit the texture coordinates of the currently loaded mesh.
    /// The control is intentionally lightweight and focused on basic grab-style editing:
    ///
    /// - Left click to select a vertex (Shift+click to add/remove from the selection).
    /// - Left drag a selection to move it.
    /// - Middle mouse drag to pan the view.
    /// - Mouse wheel to zoom in/out.
    ///
    /// Any UV changes are immediately reflected in the 3D preview because both controls
    /// share the same <see cref="Mesh"/> instance.
    /// </summary>
    public class UvEditorControl : Control
    {
        private Mesh? _mesh;
        private Bitmap? _texture;

        private readonly HashSet<int> _selectedVertices = new HashSet<int>();

        private enum TransformMode
        {
            Move,
            Scale,
            Rotate
        }

        private TransformMode _transformMode = TransformMode.Move;
        private Vector2 _transformPivotUv;

        // 2D transform gizmo (mode-based). This lives in the UV editor (2D preview)
        // and provides constrained move/scale/rotate handles similar to DCC tools.
        private enum GizmoHandle
        {
            None,
            MoveX,
            MoveY,
            MoveXY,
            ScaleX,
            ScaleY,
            ScaleUniform,
            Rotate
        }

        private bool _isGizmoDragging;
        private GizmoHandle _activeGizmoHandle = GizmoHandle.None;
        private Point _gizmoDragStartMouse;
        private Vector2 _gizmoDragStartPivotUv;
        private Vector2 _gizmoStartMouseVecUv;
        private float _gizmoStartMouseVecLen;
        private float _gizmoStartAngle;

        private bool _isBoxSelecting;
        private Point _boxStart;
        private Point _boxEnd;

        private bool _isPanning;
        private bool _isDraggingSelection;
        private Point _lastMouse;
        private Point _dragStartMouse;
        private Vector2 _dragStartPivotUv;

        private float _zoom = 1.0f;
        private float _panX;
        private float _panY;

        // Grid size used for snapping UVs. This can be adjusted to any power-of-two
        // fraction (e.g. 0.5f, 0.25f, 0.125f) to fit your workflow. A smaller value
        // yields finer snapping.
        private float _snapGridSize = 0.0625f; // 1/16th of the UV space
        // Margin applied when packing a selection into the unit square. This
        // reserves some empty space around the packed island so that it does
        // not butt up directly against the 0..1 edges or other islands when
        // working with atlas textures. The final packed UVs will be scaled
        // by (1 - 2*PackMargin) and offset by PackMargin.
        private float _packMargin = 0.02f;

        /// <summary>
        /// When true, pack islands will snap into a grid arrangement. This ensures
        /// that separate UV islands (connected components in the index buffer)
        /// are laid out into distinct cells within the 0..1 square. You can
        /// invoke PackAllIslands() via the U key. Islands are sorted by area
        /// before packing to improve space utilisation.
        /// </summary>
        private bool _enableIslandPacking = true;

        // Original UVs captured when a drag starts so we can apply deltas.
        private readonly Dictionary<int, Vector2> _dragStartUvs = new Dictionary<int, Vector2>();

        /// <summary>
        /// Raised whenever any UVs are changed by the user.
        /// </summary>
        public event EventHandler? UvChanged;

        /// <summary>
        /// Mesh whose UVs are being edited. This is usually the same instance
        /// that the 3D SoftwareMeshViewerControl is rendering.
        /// </summary>
        public Mesh? Mesh
        {
            get => _mesh;
            set
            {
                _mesh = value;
                _selectedVertices.Clear();
                _dragStartUvs.Clear();
                _isGizmoDragging = false;
                _activeGizmoHandle = GizmoHandle.None;
                ResetView();
                RecalculateTransformPivot();
                Invalidate();
            }
        }


        /// <summary>
        /// Replace or extend the current vertex selection.
        /// This is used by the 3D preview when the user picks faces there.
        /// </summary>
        public void SetSelectedVertices(IEnumerable<int> indices, bool additive)
        {
            if (!additive)
            {
                _selectedVertices.Clear();
            }

            if (indices != null)
            {
                foreach (int idx in indices)
                {
                    if (idx < 0)
                        continue;

                    // Clamp selection to the actual mesh vertex range when available.
                    if (_mesh != null && (uint)idx >= _mesh.Vertices.Length)
                        continue;

                    _selectedVertices.Add(idx);
                }
            }

            // Reset transient interaction state when selection comes from the 3D view.
            _isBoxSelecting = false;
            _isDraggingSelection = false;
            _isGizmoDragging = false;
            _activeGizmoHandle = GizmoHandle.None;
            _dragStartUvs.Clear();

            RecalculateTransformPivot();

            // Force a redraw so the newly selected vertices light up immediately.
            Invalidate();
        }

        /// <summary>
        /// Current transform mode for the UV editor ("Move", "Scale" or "Rotate").
        /// This is used by the LodAtlasPreviewForm drop-down.
        /// </summary>
        public string TransformModeName
        {
            get => _transformMode.ToString();
            set
            {
                if (Enum.TryParse<TransformMode>(value, out var mode))
                {
                    _transformMode = mode;
                    Invalidate();
                }
            }
        }



        /// <summary>
        /// Texture displayed in the background of the UV editor.
        /// </summary>
        public Bitmap? Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                Invalidate();
            }
        }

        public UvEditorControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(32, 32, 32);
        }

        private void ResetView()
        {
            _zoom = 1.0f;
            _panX = 0.0f;
            _panY = 0.0f;
        }

        #region Coordinate helpers

        // Map UV (0..1, V up) to screen space (pixels, Y down).
        private PointF UvToScreen(Vector2 uv)
        {
            int w = Math.Max(1, ClientSize.Width);
            int h = Math.Max(1, ClientSize.Height);

            float x = uv.X * _zoom * w + _panX;
            float y = (1.0f - uv.Y) * _zoom * h + _panY;
            return new PointF(x, y);
        }

        // Inverse of UvToScreen.
        private Vector2 ScreenToUv(Point p)
        {
            int w = Math.Max(1, ClientSize.Width);
            int h = Math.Max(1, ClientSize.Height);

            float u = (p.X - _panX) / (w * _zoom);
            float v = 1.0f - (p.Y - _panY) / (h * _zoom);
            return new Vector2(u, v);
        }

        private RectangleF GetUnitUvRect()
        {
            PointF tl = UvToScreen(new Vector2(0.0f, 1.0f)); // (0,1)
            PointF br = UvToScreen(new Vector2(1.0f, 0.0f)); // (1,0)
            return RectangleF.FromLTRB(tl.X, tl.Y, br.X, br.Y);
        }

        #endregion

        
        private Rectangle GetBoxSelectionRectangle()
        {
            int x1 = Math.Min(_boxStart.X, _boxEnd.X);
            int y1 = Math.Min(_boxStart.Y, _boxEnd.Y);
            int x2 = Math.Max(_boxStart.X, _boxEnd.X);
            int y2 = Math.Max(_boxStart.Y, _boxEnd.Y);

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        private void RecalculateTransformPivot()
        {
            if (_mesh == null || _selectedVertices.Count == 0 || _mesh.Vertices.Length == 0)
            {
                _transformPivotUv = Vector2.Zero;
                return;
            }

            var verts = _mesh.Vertices;
            Vector2 sum = Vector2.Zero;
            int count = 0;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                sum += verts[idx].TexCoord;
                count++;
            }

            _transformPivotUv = count > 0 ? (sum / count) : Vector2.Zero;
        }

        // Returns a "UV-space" vector from the pivot to the mouse, with V up.
        private static Vector2 MouseVectorUv(Point mouse, PointF pivotScreen)
        {
            float dx = mouse.X - pivotScreen.X;
            float dy = mouse.Y - pivotScreen.Y;
            return new Vector2(dx, -dy);
        }

        private static float DistancePointToSegmentSquared(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;
            float abLenSq = ab.LengthSquared();
            if (abLenSq <= 1e-6f)
                return ap.LengthSquared();

            float t = Vector2.Dot(ap, ab) / abLenSq;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            Vector2 c = a + ab * t;
            Vector2 d = p - c;
            return d.LengthSquared();
        }

        private bool TryHitTestGizmo(Point mouse, out GizmoHandle handle)
        {
            handle = GizmoHandle.None;
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;

            PointF pivot = UvToScreen(_transformPivotUv);
            Vector2 m = new Vector2(mouse.X, mouse.Y);
            Vector2 c = new Vector2(pivot.X, pivot.Y);

            // Common center "free" handle.
            const float centerHalf = 7.0f;
            if (Math.Abs(mouse.X - pivot.X) <= centerHalf && Math.Abs(mouse.Y - pivot.Y) <= centerHalf)
            {
                handle = _transformMode switch
                {
                    TransformMode.Move => GizmoHandle.MoveXY,
                    TransformMode.Scale => GizmoHandle.ScaleUniform,
                    TransformMode.Rotate => GizmoHandle.Rotate,
                    _ => GizmoHandle.None
                };
                return handle != GizmoHandle.None;
            }

            const float axisLen = 45.0f;
            const float hitPx = 7.0f;
            float hitSq = hitPx * hitPx;

            // UV axis directions in screen space: +U is +X, +V is -Y.
            Vector2 xEnd = c + new Vector2(axisLen, 0);
            Vector2 yEnd = c + new Vector2(0, -axisLen);

            if (_transformMode == TransformMode.Move)
            {
                // Arrow shaft hit test.
                if (DistancePointToSegmentSquared(m, c, xEnd) <= hitSq)
                {
                    handle = GizmoHandle.MoveX;
                    return true;
                }
                if (DistancePointToSegmentSquared(m, c, yEnd) <= hitSq)
                {
                    handle = GizmoHandle.MoveY;
                    return true;
                }
            }
            else if (_transformMode == TransformMode.Scale)
            {
                // Handle squares.
                const float hs = 6.0f;
                var rx = new RectangleF(xEnd.X - hs, xEnd.Y - hs, hs * 2, hs * 2);
                var ry = new RectangleF(yEnd.X - hs, yEnd.Y - hs, hs * 2, hs * 2);
                var runi = new RectangleF((c.X + axisLen) - hs, (c.Y - axisLen) - hs, hs * 2, hs * 2);

                if (rx.Contains(mouse.X, mouse.Y))
                {
                    handle = GizmoHandle.ScaleX;
                    return true;
                }
                if (ry.Contains(mouse.X, mouse.Y))
                {
                    handle = GizmoHandle.ScaleY;
                    return true;
                }
                if (runi.Contains(mouse.X, mouse.Y))
                {
                    handle = GizmoHandle.ScaleUniform;
                    return true;
                }
            }
            else if (_transformMode == TransformMode.Rotate)
            {
                // Ring hit test.
                const float r = 52.0f;
                float dist = Vector2.Distance(m, c);
                if (Math.Abs(dist - r) <= hitPx)
                {
                    handle = GizmoHandle.Rotate;
                    return true;
                }
            }

            return false;
        }

        private void BeginGizmoDrag(GizmoHandle handle, Point mouse)
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return;

            _isGizmoDragging = true;
            _activeGizmoHandle = handle;
            _gizmoDragStartMouse = mouse;

            _dragStartUvs.Clear();
            var verts = _mesh.Vertices;

            // Capture original UVs.
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                _dragStartUvs[idx] = verts[idx].TexCoord;
            }

            RecalculateTransformPivot();
            _gizmoDragStartPivotUv = _transformPivotUv;

            // Cache mouse vector/angle for rotate/scale.
            PointF pivotScreen = UvToScreen(_transformPivotUv);
            _gizmoStartMouseVecUv = MouseVectorUv(mouse, pivotScreen);
            _gizmoStartMouseVecLen = Math.Max(1e-3f, _gizmoStartMouseVecUv.Length());
            _gizmoStartAngle = (float)Math.Atan2(_gizmoStartMouseVecUv.Y, _gizmoStartMouseVecUv.X);

            Capture = true;
        }

        private void UpdateGizmoDrag(Point mouse)
        {
            if (!_isGizmoDragging || _mesh == null || _dragStartUvs.Count == 0)
                return;

            int w = Math.Max(1, ClientSize.Width);
            int h = Math.Max(1, ClientSize.Height);

            var verts = _mesh.Vertices;

            // Pixel delta -> UV delta.
            int dx = mouse.X - _gizmoDragStartMouse.X;
            int dy = mouse.Y - _gizmoDragStartMouse.Y;
            float du = (float)dx / (w * _zoom);
            float dv = -(float)dy / (h * _zoom);

            // Rotate/scale are based on the current vector from the pivot to the mouse.
            PointF pivotScreen = UvToScreen(_transformPivotUv);
            Vector2 curMouseVecUv = MouseVectorUv(mouse, pivotScreen);

            float scaleX = 1.0f;
            float scaleY = 1.0f;
            float angle = 0.0f;

            switch (_activeGizmoHandle)
            {
                case GizmoHandle.MoveX:
                    dv = 0.0f;
                    break;
                case GizmoHandle.MoveY:
                    du = 0.0f;
                    break;
                case GizmoHandle.MoveXY:
                    break;
                case GizmoHandle.ScaleX:
                    {
                        float start = Math.Abs(_gizmoStartMouseVecUv.X) < 1e-4f ? 1e-4f : _gizmoStartMouseVecUv.X;
                        float cur = Math.Abs(curMouseVecUv.X) < 1e-4f ? 1e-4f : curMouseVecUv.X;
                        scaleX = Math.Clamp(cur / start, 0.05f, 50.0f);
                    }
                    break;
                case GizmoHandle.ScaleY:
                    {
                        float start = Math.Abs(_gizmoStartMouseVecUv.Y) < 1e-4f ? 1e-4f : _gizmoStartMouseVecUv.Y;
                        float cur = Math.Abs(curMouseVecUv.Y) < 1e-4f ? 1e-4f : curMouseVecUv.Y;
                        scaleY = Math.Clamp(cur / start, 0.05f, 50.0f);
                    }
                    break;
                case GizmoHandle.ScaleUniform:
                    {
                        float curLen = Math.Max(1e-3f, curMouseVecUv.Length());
                        float s = Math.Clamp(curLen / _gizmoStartMouseVecLen, 0.05f, 50.0f);
                        scaleX = scaleY = s;
                    }
                    break;
                case GizmoHandle.Rotate:
                    {
                        float curAngle = (float)Math.Atan2(curMouseVecUv.Y, curMouseVecUv.X);
                        angle = curAngle - _gizmoStartAngle;
                    }
                    break;
            }

            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            foreach (var kvp in _dragStartUvs)
            {
                int idx = kvp.Key;
                if ((uint)idx >= verts.Length)
                    continue;

                Vector2 baseUv = kvp.Value;
                Vector2 newUv = baseUv;

                if (_activeGizmoHandle == GizmoHandle.MoveX || _activeGizmoHandle == GizmoHandle.MoveY || _activeGizmoHandle == GizmoHandle.MoveXY)
                {
                    newUv = new Vector2(baseUv.X + du, baseUv.Y + dv);
                }
                else if (_activeGizmoHandle == GizmoHandle.Rotate)
                {
                    Vector2 off = baseUv - _transformPivotUv;
                    Vector2 rot;
                    rot.X = off.X * cos - off.Y * sin;
                    rot.Y = off.X * sin + off.Y * cos;
                    newUv = _transformPivotUv + rot;
                }
                else if (_activeGizmoHandle == GizmoHandle.ScaleX || _activeGizmoHandle == GizmoHandle.ScaleY || _activeGizmoHandle == GizmoHandle.ScaleUniform)
                {
                    Vector2 off = baseUv - _transformPivotUv;
                    off.X *= scaleX;
                    off.Y *= scaleY;
                    newUv = _transformPivotUv + off;
                }

                // Clamp into a generous range so out-of-range UVs remain editable.
                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);

                var v = verts[idx];
                v.TexCoord = newUv;
                verts[idx] = v;
            }

            // Keep the gizmo anchored to the selection while translating.
            // For rotate/scale we intentionally keep the pivot fixed during the drag.
            if (_activeGizmoHandle == GizmoHandle.MoveX || _activeGizmoHandle == GizmoHandle.MoveY || _activeGizmoHandle == GizmoHandle.MoveXY)
            {
                _transformPivotUv = _gizmoDragStartPivotUv + new Vector2(du, dv);
            }

            UvChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        private void EndGizmoDrag()
        {
            if (!_isGizmoDragging)
                return;

            _isGizmoDragging = false;
            _activeGizmoHandle = GizmoHandle.None;
            Capture = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        private void DrawTransformGizmo(Graphics g)
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return;

            PointF pivot = UvToScreen(_transformPivotUv);
            float cx = pivot.X;
            float cy = pivot.Y;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            const float axisLen = 45.0f;
            const float handleSize = 6.0f;

            using var penX = new Pen(Color.Red, 2.0f);
            using var penY = new Pen(Color.LimeGreen, 2.0f);
            using var penActive = new Pen(Color.Gold, 3.0f);
            using var centerBrush = new SolidBrush(Color.FromArgb(220, Color.White));
            using var centerPen = new Pen(Color.Black, 1.0f);

            // Center handle.
            g.FillRectangle(centerBrush, cx - 4, cy - 4, 8, 8);
            g.DrawRectangle(centerPen, cx - 4, cy - 4, 8, 8);

            if (_transformMode == TransformMode.Move)
            {
                PointF xEnd = new PointF(cx + axisLen, cy);
                PointF yEnd = new PointF(cx, cy - axisLen);

                Pen px = _activeGizmoHandle == GizmoHandle.MoveX ? penActive : penX;
                Pen py = _activeGizmoHandle == GizmoHandle.MoveY ? penActive : penY;

                g.DrawLine(px, pivot, xEnd);
                g.DrawLine(py, pivot, yEnd);

                // Arrowheads.
                DrawArrowHead(g, px, pivot, xEnd);
                DrawArrowHead(g, py, pivot, yEnd);
            }
            else if (_transformMode == TransformMode.Scale)
            {
                PointF xEnd = new PointF(cx + axisLen, cy);
                PointF yEnd = new PointF(cx, cy - axisLen);
                PointF uEnd = new PointF(cx + axisLen, cy - axisLen);

                g.DrawLine(penX, pivot, xEnd);
                g.DrawLine(penY, pivot, yEnd);

                var brX = _activeGizmoHandle == GizmoHandle.ScaleX ? Brushes.Gold : Brushes.White;
                var brY = _activeGizmoHandle == GizmoHandle.ScaleY ? Brushes.Gold : Brushes.White;
                var brU = _activeGizmoHandle == GizmoHandle.ScaleUniform ? Brushes.Gold : Brushes.White;

                g.FillRectangle(brX, xEnd.X - handleSize, xEnd.Y - handleSize, handleSize * 2, handleSize * 2);
                g.FillRectangle(brY, yEnd.X - handleSize, yEnd.Y - handleSize, handleSize * 2, handleSize * 2);
                g.FillRectangle(brU, uEnd.X - handleSize, uEnd.Y - handleSize, handleSize * 2, handleSize * 2);

                g.DrawRectangle(Pens.Black, xEnd.X - handleSize, xEnd.Y - handleSize, handleSize * 2, handleSize * 2);
                g.DrawRectangle(Pens.Black, yEnd.X - handleSize, yEnd.Y - handleSize, handleSize * 2, handleSize * 2);
                g.DrawRectangle(Pens.Black, uEnd.X - handleSize, uEnd.Y - handleSize, handleSize * 2, handleSize * 2);
            }
            else if (_transformMode == TransformMode.Rotate)
            {
                const float r = 52.0f;
                using var ringPen = new Pen(_activeGizmoHandle == GizmoHandle.Rotate ? Color.Gold : Color.White, _activeGizmoHandle == GizmoHandle.Rotate ? 2.5f : 1.5f);
                g.DrawEllipse(ringPen, cx - r, cy - r, r * 2, r * 2);
            }
        }

        private static void DrawArrowHead(Graphics g, Pen pen, PointF from, PointF to)
        {
            var dir = new Vector2(to.X - from.X, to.Y - from.Y);
            float len = dir.Length();
            if (len < 1e-3f)
                return;
            dir /= len;
            var perp = new Vector2(-dir.Y, dir.X);
            const float arrowLen = 10.0f;
            const float arrowWidth = 5.0f;
            var tip = new Vector2(to.X, to.Y);
            var basePt = tip - dir * arrowLen;
            var a = basePt + perp * arrowWidth;
            var b = basePt - perp * arrowWidth;
            g.DrawLine(pen, to, new PointF(a.X, a.Y));
            g.DrawLine(pen, to, new PointF(b.X, b.Y));
        }

        #region Rendering

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            g.Clear(BackColor);

            RectangleF uvRect = GetUnitUvRect();

            // Checkerboard behind the texture so transparent areas are visible.
            DrawCheckerboard(g, uvRect);

            // Draw the texture if one is available.
            if (_texture != null)
            {
                g.DrawImage(_texture, uvRect);
            }

            // Outline the 0..1 UV rectangle.
            using (var borderPen = new Pen(Color.DimGray, 1.0f))
            {
                g.DrawRectangle(borderPen, uvRect.X, uvRect.Y, uvRect.Width, uvRect.Height);
            }

            // Draw all UV triangles for the current mesh.
            if (_mesh != null && _mesh.Vertices.Length > 0 && _mesh.Indices.Length > 0)
            {
                var verts = _mesh.Vertices;
                var indices = _mesh.Indices;

                using var edgePen = new Pen(Color.FromArgb(200, 200, 220), 1.0f);

                for (int i = 0; i < indices.Length; i += 3)
                {
                    if (i + 2 >= indices.Length)
                        break;

                    int i0 = indices[i];
                    int i1 = indices[i + 1];
                    int i2 = indices[i + 2];

                    if ((uint)i0 >= verts.Length ||
                        (uint)i1 >= verts.Length ||
                        (uint)i2 >= verts.Length)
                    {
                        continue;
                    }

                    PointF p0 = UvToScreen(verts[i0].TexCoord);
                    PointF p1 = UvToScreen(verts[i1].TexCoord);
                    PointF p2 = UvToScreen(verts[i2].TexCoord);

                    g.DrawLine(edgePen, p0, p1);
                    g.DrawLine(edgePen, p1, p2);
                    g.DrawLine(edgePen, p2, p0);
                }

                // Draw vertices on top so they are easy to select.
                foreach (int vi in System.Linq.Enumerable.Range(0, verts.Length))
                {
                    PointF p = UvToScreen(verts[vi].TexCoord);
                    float size = 5.0f;
                    RectangleF r = new RectangleF(p.X - size * 0.5f, p.Y - size * 0.5f, size, size);

                    bool selected = _selectedVertices.Contains(vi);
                    using var brush = new SolidBrush(selected ? Color.Orange : Color.LightBlue);
                    using var pen = new Pen(Color.Black, 1.0f);

                    g.FillEllipse(brush, r);
                    g.DrawEllipse(pen, r);
                }
            }

            // Mode-based transform gizmo for the current selection.
            if (_selectedVertices.Count > 0 && !_isBoxSelecting)
            {
                DrawTransformGizmo(g);
            }

            
            // Draw box selection rectangle if active.
            if (_isBoxSelecting)
            {
                using var boxPen = new Pen(Color.FromArgb(160, Color.LightGreen))
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                };
                Rectangle r = GetBoxSelectionRectangle();
                if (r.Width > 0 && r.Height > 0)
                {
                    g.DrawRectangle(boxPen, r);
                }
            }

            // Small help text in the bottom-left corner.
            // Describe the extended hotkeys available in this editor. In addition to
            // Blender‑inspired move/scale/rotate modes (G/S/R), we now offer
            // quick packing of the current selection into the unit square (P),
            // a 90° rotation around the selection centroid (Q), and snapping
            // selected UVs to a fixed grid (T). These shortcuts bring some of
            // Blender's quality‑of‑life tools into this lightweight UV editor.
            string help =
                "UV editor controls:\n" +
                "· Left click: select vertex (Shift to multi-select)\n" +
                "· Left drag: transform selection (mode: G=Move, S=Scale, R=Rotate)\n" +
                "· Middle drag: pan\n" +
                "· Drag on empty space: box select\n" +
                "· Ctrl+A: select all, Esc: clear selection\n" +
                "· P: pack selection into 0..1 (with margin)\n" +
                "· Q: rotate 90°\n" +
                "· E: rotate 180°\n" +
                "· X: flip horizontally\n" +
                "· Y: flip vertically\n" +
                "· T: snap to grid\n" +
                "· U: pack all islands";

            using var helpBrush = new SolidBrush(Color.FromArgb(180, Color.Gainsboro));
            using var helpFont = new Font(Font.FontFamily, 7.0f);
            var helpSize = g.MeasureString(help, helpFont);
            var helpRect = new RectangleF(
                4,
                ClientSize.Height - helpSize.Height - 4,
                helpSize.Width,
                helpSize.Height);

            g.FillRectangle(new SolidBrush(Color.FromArgb(120, 16, 16, 16)), helpRect);
            g.DrawString(help, helpFont, helpBrush, helpRect.Location);
        }
        private void DrawCheckerboard(Graphics g, RectangleF rect)
        {
            int cellSize = 12;
            using var light = new SolidBrush(Color.FromArgb(60, 60, 60));
            using var dark = new SolidBrush(Color.FromArgb(40, 40, 40));

            bool yToggle = false;
            for (int y = (int)rect.Top; y < rect.Bottom; y += cellSize)
            {
                bool xToggle = yToggle;
                for (int x = (int)rect.Left; x < rect.Right; x += cellSize)
                {
                    var brush = xToggle ? light : dark;
                    g.FillRectangle(brush, x, y, cellSize, cellSize);
                    xToggle = !xToggle;
                }

                yToggle = !yToggle;
            }
        }

        #endregion

        #region Interaction

                protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Make sure we can receive keyboard shortcuts (G/S/R/A/Escape).
            Focus();

            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = true;
                _lastMouse = e.Location;
                Capture = true;
            }
            else if (e.Button == MouseButtons.Left)
            {
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;

                // If there is an existing selection, allow the user to grab the
                // mode-based transform gizmo directly (Move/Scale/Rotate). This
                // takes precedence over changing the selection.
                if (!shift && !ctrl && _selectedVertices.Count > 0 && TryHitTestGizmo(e.Location, out var gizmo) && gizmo != GizmoHandle.None)
                {
                    _isBoxSelecting = false;
                    _isDraggingSelection = false;
                    BeginGizmoDrag(gizmo, e.Location);
                    _lastMouse = e.Location;
                    Invalidate();
                    return;
                }

                int hit = HitTestVertex(e.Location, 10.0f);

                if (shift)
                {
                    if (hit >= 0)
                    {
                        // Toggle selection.
                        if (_selectedVertices.Contains(hit))
                            _selectedVertices.Remove(hit);
                        else
                            _selectedVertices.Add(hit);
                    }
                }
                else if (!ctrl)
                {
                    // Replace selection unless Ctrl is held (Ctrl+drag reuses the existing selection).
                    _selectedVertices.Clear();
                    if (hit >= 0)
                    {
                        _selectedVertices.Add(hit);
                    }
                }

                bool haveSelection = _selectedVertices.Count > 0;

                RecalculateTransformPivot();

                if (haveSelection && (hit >= 0 || ctrl))
                {
                    // Prepare for transforming the current selection (move / scale / rotate).
                    _isDraggingSelection = true;
                    _dragStartMouse = e.Location;
                    _dragStartPivotUv = _transformPivotUv;
                    _dragStartUvs.Clear();

                    if (_mesh != null)
                    {
                        var verts = _mesh.Vertices;

                        foreach (int idx in _selectedVertices)
                        {
                            if ((uint)idx >= verts.Length)
                                continue;
                            _dragStartUvs[idx] = verts[idx].TexCoord;
                        }
                    }

                    _isBoxSelecting = false;
                }
                else
                {
                    // No vertex under the cursor (or no active selection): start a box selection.
                    _isDraggingSelection = false;
                    _dragStartUvs.Clear();
                    _isBoxSelecting = true;
                    _boxStart = _boxEnd = e.Location;
                }

                _lastMouse = e.Location;
                Capture = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (_isGizmoDragging)
                {
                    EndGizmoDrag();
                    return;
                }

                if (_isBoxSelecting)
                {
                    // Finalize box selection.
                    _isBoxSelecting = false;

                    if (_mesh != null)
                    {
                        var verts = _mesh.Vertices;
                        Rectangle selectionRect = GetBoxSelectionRectangle();

                        _selectedVertices.Clear();
                        for (int i = 0; i < verts.Length; i++)
                        {
                            PointF p = UvToScreen(verts[i].TexCoord);
                            if (selectionRect.Contains(Point.Round(p)))
                            {
                                _selectedVertices.Add(i);
                            }
                        }
                    }

                    RecalculateTransformPivot();
                }
                else if (_isDraggingSelection)
                {
                    // Finalize drag / transform.
                    _isDraggingSelection = false;
                    RecalculateTransformPivot();
                }
            }

            if (!(_isPanning || _isDraggingSelection || _isBoxSelecting || _isGizmoDragging))
            {
                Capture = false;
            }

            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isGizmoDragging)
            {
                UpdateGizmoDrag(e.Location);
                return;
            }

            // Cursor feedback when hovering a gizmo handle (only when idle).
            if (!(_isPanning || _isBoxSelecting || _isDraggingSelection) && _selectedVertices.Count > 0)
            {
                if (TryHitTestGizmo(e.Location, out var h) && h != GizmoHandle.None)
                {
                    Cursor = h switch
                    {
                        GizmoHandle.MoveX => Cursors.SizeWE,
                        GizmoHandle.MoveY => Cursors.SizeNS,
                        GizmoHandle.MoveXY => Cursors.SizeAll,
                        GizmoHandle.ScaleX => Cursors.SizeWE,
                        GizmoHandle.ScaleY => Cursors.SizeNS,
                        GizmoHandle.ScaleUniform => Cursors.SizeNWSE,
                        GizmoHandle.Rotate => Cursors.Hand,
                        _ => Cursors.Default
                    };
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }

            if (_isPanning)
            {
                int dx = e.X - _lastMouse.X;
                int dy = e.Y - _lastMouse.Y;
                _lastMouse = e.Location;

                _panX += dx;
                _panY += dy;
                Invalidate();
            }
            else if (_isBoxSelecting)
            {
                _boxEnd = e.Location;
                Invalidate();
            }
            else if (_isDraggingSelection && _mesh != null && _dragStartUvs.Count > 0)
            {
                int dx = e.X - _dragStartMouse.X;
                int dy = e.Y - _dragStartMouse.Y;

                int w = Math.Max(1, ClientSize.Width);
                int h = Math.Max(1, ClientSize.Height);

                var verts = _mesh.Vertices;

                switch (_transformMode)
                {
                    case TransformMode.Move:
                        {
                            float du = (float)dx / (w * _zoom);
                            float dv = -(float)dy / (h * _zoom);

                            foreach (var kvp in _dragStartUvs)
                            {
                                int idx = kvp.Key;
                                if ((uint)idx >= verts.Length)
                                    continue;

                                Vector2 baseUv = kvp.Value;
                                baseUv.X += du;
                                baseUv.Y += dv;

                                baseUv.X = Math.Clamp(baseUv.X, -4.0f, 4.0f);
                                baseUv.Y = Math.Clamp(baseUv.Y, -4.0f, 4.0f);

                                var v = verts[idx];
                                v.TexCoord = baseUv;
                                verts[idx] = v;
                            }

                            // Keep the gizmo anchored to the translating selection.
                            _transformPivotUv = _dragStartPivotUv + new Vector2(du, dv);
                        }
                        break;

                    case TransformMode.Scale:
                        {
                            // Horizontal mouse movement controls uniform scale.
                            float scale = 1.0f + (float)dx / (w * 0.5f);
                            scale = Math.Clamp(scale, 0.1f, 10.0f);

                            foreach (var kvp in _dragStartUvs)
                            {
                                int idx = kvp.Key;
                                if ((uint)idx >= verts.Length)
                                    continue;

                                Vector2 baseUv = kvp.Value;
                                Vector2 offset = baseUv - _transformPivotUv;
                                offset *= scale;
                                Vector2 newUv = _transformPivotUv + offset;

                                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);

                                var v = verts[idx];
                                v.TexCoord = newUv;
                                verts[idx] = v;
                            }
                        }
                        break;

                    case TransformMode.Rotate:
                        {
                            // Horizontal mouse movement controls rotation angle.
                            float angle = dx * 0.01f; // radians
                            float cos = (float)Math.Cos(angle);
                            float sin = (float)Math.Sin(angle);

                            foreach (var kvp in _dragStartUvs)
                            {
                                int idx = kvp.Key;
                                if ((uint)idx >= verts.Length)
                                    continue;

                                Vector2 baseUv = kvp.Value;
                                Vector2 offset = baseUv - _transformPivotUv;

                                Vector2 rotated;
                                rotated.X = offset.X * cos - offset.Y * sin;
                                rotated.Y = offset.X * sin + offset.Y * cos;

                                Vector2 newUv = _transformPivotUv + rotated;

                                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);

                                var v = verts[idx];
                                v.TexCoord = newUv;
                                verts[idx] = v;
                            }
                        }
                        break;
                }

                UvChanged?.Invoke(this, EventArgs.Empty);

                Invalidate();
            }
        }

protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta == 0)
                return;

            float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
            float newZoom = _zoom * zoomFactor;
            newZoom = Math.Clamp(newZoom, 0.1f, 20.0f);

            if (Math.Abs(newZoom - _zoom) < 1e-4f)
                return;

            // Optional: zoom around the mouse position, so the point under the cursor stays put.
            Vector2 uvUnderMouseBefore = ScreenToUv(e.Location);

            _zoom = newZoom;

            Vector2 uvUnderMouseAfter = uvUnderMouseBefore;
            PointF screenBefore = UvToScreen(uvUnderMouseBefore);
            PointF screenAfter = UvToScreen(uvUnderMouseAfter);

            _panX += (screenBefore.X - screenAfter.X);
            _panY += (screenBefore.Y - screenAfter.Y);

            Invalidate();
        }

        
        protected override bool IsInputKey(Keys keyData)
        {
            // Let the control receive arrow keys etc. directly if we ever use them.
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.G)
            {
                _transformMode = TransformMode.Move;
                if (_isGizmoDragging)
                    EndGizmoDrag();
                Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                _transformMode = TransformMode.Scale;
                if (_isGizmoDragging)
                    EndGizmoDrag();
                Invalidate();
            }
            else if (e.KeyCode == Keys.R)
            {
                _transformMode = TransformMode.Rotate;
                if (_isGizmoDragging)
                    EndGizmoDrag();
                Invalidate();
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                if (_mesh != null && _mesh.Vertices.Length > 0)
                {
                    _selectedVertices.Clear();
                    for (int i = 0; i < _mesh.Vertices.Length; i++)
                        _selectedVertices.Add(i);

                    if (_isGizmoDragging)
                        EndGizmoDrag();
                    _isDraggingSelection = false;
                    _isBoxSelecting = false;
                    _dragStartUvs.Clear();

                    RecalculateTransformPivot();

                    Invalidate();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _selectedVertices.Clear();
                _isDraggingSelection = false;
                _isBoxSelecting = false;
                _isGizmoDragging = false;
                _activeGizmoHandle = GizmoHandle.None;
                _dragStartUvs.Clear();
                RecalculateTransformPivot();
                Invalidate();
            }
            // Pack the current selection into the 0..1 UV square. This uniformly
            // scales and translates the selected UVs so they fit within the unit
            // rectangle while preserving aspect ratio. Inspired by Blender's
            // packing tools, this is useful when working with atlas layouts.
            else if (e.KeyCode == Keys.P)
            {
                if (PackSelectionToUnitSquare())
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Rotate the selection 90 degrees around its centroid. Blender exposes
            // cardinal rotations for islands; this shortcut brings similar
            // functionality to this editor.
            else if (e.KeyCode == Keys.Q)
            {
                if (RotateSelection90())
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Snap the selection onto a fixed grid. This rounds each UV to the
            // nearest multiple of the configured grid size (_snapGridSize). Use
            // this to align vertices cleanly without manually tweaking values.
            else if (e.KeyCode == Keys.T)
            {
                if (SnapSelectionToGrid())
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Rotate selection by 180 degrees around its centroid. This
            // effectively flips both axes at once. Key: E.
            else if (e.KeyCode == Keys.E)
            {
                if (RotateSelection180())
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Flip selection horizontally around its centroid. Key: X.
            else if (e.KeyCode == Keys.X)
            {
                if (FlipSelection(horizontal: true))
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Flip selection vertically around its centroid. Key: Y.
            else if (e.KeyCode == Keys.Y)
            {
                if (FlipSelection(horizontal: false))
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
            // Pack all UV islands into a grid so they occupy distinct cells in
            // the unit square. Key: U.
            else if (e.KeyCode == Keys.U)
            {
                if (PackAllIslands())
                {
                    UvChanged?.Invoke(this, EventArgs.Empty);
                    RecalculateTransformPivot();
                    Invalidate();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // We do not own _texture, so do not dispose it here.
                _mesh = null;
                _texture = null;
                _selectedVertices.Clear();
                _dragStartUvs.Clear();
            }

            base.Dispose(disposing);
        }

        // Hit test for the closest vertex in screen space.
        private int HitTestVertex(Point location, float maxDistancePixels)
        {
            if (_mesh == null || _mesh.Vertices.Length == 0)
                return -1;

            var verts = _mesh.Vertices;
            float maxDistSq = maxDistancePixels * maxDistancePixels;
            int bestIndex = -1;
            float bestDistSq = maxDistSq;

            for (int i = 0; i < verts.Length; i++)
            {
                PointF p = UvToScreen(verts[i].TexCoord);
                float dx = (float)location.X - p.X;
                float dy = (float)location.Y - p.Y;
                float distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// Packs the currently selected UVs into the [0,1] square. The UVs are
        /// translated so that the minimum U/V becomes 0, then uniformly scaled
        /// so the larger extent fits within 1.0 while preserving aspect ratio.
        /// Returns true if any UVs were modified. If the selection is empty or
        /// degenerate (zero area), nothing is changed.
        /// </summary>
        private bool PackSelectionToUnitSquare()
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;

            var verts = _mesh.Vertices;

            // Compute bounding box of the selection
            float minU = float.PositiveInfinity;
            float minV = float.PositiveInfinity;
            float maxU = float.NegativeInfinity;
            float maxV = float.NegativeInfinity;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;

                Vector2 uv = verts[idx].TexCoord;
                if (uv.X < minU) minU = uv.X;
                if (uv.Y < minV) minV = uv.Y;
                if (uv.X > maxU) maxU = uv.X;
                if (uv.Y > maxV) maxV = uv.Y;
            }

            float width = maxU - minU;
            float height = maxV - minV;
            if (width <= 1e-6f && height <= 1e-6f)
            {
                // Degenerate selection; nothing to pack
                return false;
            }

            // Uniform scaling based on the larger dimension. This preserves the
            // aspect ratio of the selection while fitting it inside [0,1].
            float scale = 1.0f / Math.Max(width, height);

            bool changed = false;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;

                var uv = verts[idx].TexCoord;
                // Translate to origin and scale uniformly
                float u = (uv.X - minU) * scale;
                float v = (uv.Y - minV) * scale;
                // Apply a margin by shrinking the selection and offsetting it
                float margin = Math.Clamp(_packMargin, 0.0f, 0.45f);
                float shrink = Math.Max(0.0f, 1.0f - 2.0f * margin);
                u = u * shrink + margin;
                v = v * shrink + margin;
                // Clamp into a generous range (-4..4) so off‑grid UVs don't explode
                u = Math.Clamp(u, -4.0f, 4.0f);
                v = Math.Clamp(v, -4.0f, 4.0f);
                if (Math.Abs(u - uv.X) > 1e-6f || Math.Abs(v - uv.Y) > 1e-6f)
                {
                    changed = true;
                    var vtx = verts[idx];
                    vtx.TexCoord = new Vector2(u, v);
                    verts[idx] = vtx;
                }
            }

            return changed;
        }

        /// <summary>
        /// Rotates the selected UVs by 90 degrees around their centroid. The
        /// rotation is counter‑clockwise in UV space (U right, V up). Returns
        /// true if any UVs were modified. Does nothing if the selection is
        /// empty.
        /// </summary>
        private bool RotateSelection90()
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;

            var verts = _mesh.Vertices;

            // Compute centroid of selection
            Vector2 sum = Vector2.Zero;
            int count = 0;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                sum += verts[idx].TexCoord;
                count++;
            }
            if (count == 0)
                return false;

            Vector2 pivot = sum / count;

            bool changed = false;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;

                Vector2 uv = verts[idx].TexCoord;
                Vector2 offset = uv - pivot;
                // 90° CCW rotation: (x, y) -> (-y, x)
                Vector2 rotated = new Vector2(-offset.Y, offset.X);
                Vector2 newUv = pivot + rotated;
                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);
                if (Math.Abs(newUv.X - uv.X) > 1e-6f || Math.Abs(newUv.Y - uv.Y) > 1e-6f)
                {
                    changed = true;
                    var vtx = verts[idx];
                    vtx.TexCoord = newUv;
                    verts[idx] = vtx;
                }
            }

            return changed;
        }

        /// <summary>
        /// Snaps the selected UVs to the nearest multiple of the configured
        /// _snapGridSize. This rounds each coordinate independently. Returns
        /// true if any UVs were modified. Does nothing if the selection is empty.
        /// </summary>
        private bool SnapSelectionToGrid()
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;

            var verts = _mesh.Vertices;
            float grid = Math.Max(1e-6f, _snapGridSize);

            bool changed = false;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                Vector2 uv = verts[idx].TexCoord;
                float u = (float)Math.Round(uv.X / grid) * grid;
                float v = (float)Math.Round(uv.Y / grid) * grid;
                u = Math.Clamp(u, -4.0f, 4.0f);
                v = Math.Clamp(v, -4.0f, 4.0f);
                if (Math.Abs(u - uv.X) > 1e-6f || Math.Abs(v - uv.Y) > 1e-6f)
                {
                    changed = true;
                    var vtx = verts[idx];
                    vtx.TexCoord = new Vector2(u, v);
                    verts[idx] = vtx;
                }
            }
            return changed;
        }

        /// <summary>
        /// Rotates the selection by 180 degrees around its centroid. Equivalent
        /// to flipping horizontally and vertically simultaneously. Returns true
        /// if any UVs changed.
        /// </summary>
        private bool RotateSelection180()
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;

            var verts = _mesh.Vertices;

            // Compute centroid
            Vector2 sum = Vector2.Zero;
            int count = 0;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                sum += verts[idx].TexCoord;
                count++;
            }
            if (count == 0)
                return false;
            Vector2 pivot = sum / count;

            bool changed = false;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                Vector2 uv = verts[idx].TexCoord;
                Vector2 offset = uv - pivot;
                Vector2 newUv = pivot - offset; // 180° rotation
                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);
                if (Math.Abs(newUv.X - uv.X) > 1e-6f || Math.Abs(newUv.Y - uv.Y) > 1e-6f)
                {
                    changed = true;
                    var vtx = verts[idx];
                    vtx.TexCoord = newUv;
                    verts[idx] = vtx;
                }
            }
            return changed;
        }

        /// <summary>
        /// Flips the selection around its centroid. If horizontal is true, the
        /// U coordinate is mirrored; otherwise the V coordinate is mirrored.
        /// Returns true if any UVs changed.
        /// </summary>
        private bool FlipSelection(bool horizontal)
        {
            if (_mesh == null || _selectedVertices.Count == 0)
                return false;
            var verts = _mesh.Vertices;
            Vector2 sum = Vector2.Zero;
            int count = 0;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                sum += verts[idx].TexCoord;
                count++;
            }
            if (count == 0)
                return false;
            Vector2 pivot = sum / count;
            bool changed = false;
            foreach (int idx in _selectedVertices)
            {
                if ((uint)idx >= verts.Length)
                    continue;
                Vector2 uv = verts[idx].TexCoord;
                Vector2 newUv = uv;
                if (horizontal)
                {
                    // Mirror U across pivot
                    newUv.X = pivot.X + (pivot.X - uv.X);
                }
                else
                {
                    // Mirror V across pivot
                    newUv.Y = pivot.Y + (pivot.Y - uv.Y);
                }
                newUv.X = Math.Clamp(newUv.X, -4.0f, 4.0f);
                newUv.Y = Math.Clamp(newUv.Y, -4.0f, 4.0f);
                if (Math.Abs(newUv.X - uv.X) > 1e-6f || Math.Abs(newUv.Y - uv.Y) > 1e-6f)
                {
                    changed = true;
                    var vtx = verts[idx];
                    vtx.TexCoord = newUv;
                    verts[idx] = vtx;
                }
            }
            return changed;
        }

        /// <summary>
        /// Packs all UV islands (connected components in the mesh index buffer)
        /// into a grid layout within the 0..1 UV square. Each island is
        /// individually scaled and positioned so that no islands overlap. This
        /// prevents stacking of UV shells when working with complex meshes.
        /// Returns true if any UVs were modified. This operates on the entire
        /// mesh rather than just the current selection.
        /// </summary>
        private 
        /// <summary>
        /// Packs all UV islands (connected components in the mesh index buffer)
        /// into a grid layout while preserving each island's original scale.
        /// This is used by the U hotkey to "unstack" cards: we only translate
        /// islands so they no longer overlap, but we do not resize them.
        ///
        /// NOTE: Because no scaling is applied, the packed layout is not
        /// guaranteed to remain inside the 0..1 range if there are many large
        /// islands. This is intentional – artists can choose to pack to 0..1
        /// using P on a selection if desired.
        /// </summary>
        bool PackAllIslands()
        {
            if (_mesh == null || _mesh.Vertices.Length == 0 || _mesh.Indices.Length < 3)
                return false;

            var verts = _mesh.Vertices;
            int vertexCount = verts.Length;
            int[] indices = _mesh.Indices;
            int faceCount = indices.Length / 3;
            if (faceCount == 0)
                return false;

            // Union‑find structure to group connected vertices into islands
            // based on shared edges.
            var parent = new int[vertexCount];
            for (int i = 0; i < vertexCount; i++) parent[i] = i;

            Func<int, int> find = null!;
            find = (int x) =>
            {
                if (parent[x] != x) parent[x] = find(parent[x]);
                return parent[x];
            };
            Action<int, int> union = (int a, int b) =>
            {
                int ra = find(a);
                int rb = find(b);
                if (ra != rb) parent[rb] = ra;
            };

            // Union vertices that share an edge within a triangle.
            for (int i = 0; i < indices.Length; i += 3)
            {
                int i0 = indices[i];
                int i1 = indices[i + 1];
                int i2 = indices[i + 2];
                if ((uint)i0 < (uint)vertexCount && (uint)i1 < (uint)vertexCount) union(i0, i1);
                if ((uint)i1 < (uint)vertexCount && (uint)i2 < (uint)vertexCount) union(i1, i2);
                if ((uint)i2 < (uint)vertexCount && (uint)i0 < (uint)vertexCount) union(i2, i0);
            }

            // Group vertex indices by root parent.
            var islandDict = new Dictionary<int, List<int>>();
            for (int i = 0; i < vertexCount; i++)
            {
                int root = find(i);
                if (!islandDict.TryGetValue(root, out var list))
                {
                    list = new List<int>();
                    islandDict[root] = list;
                }
                list.Add(i);
            }

            if (islandDict.Count <= 1)
            {
                // Nothing to pack if there is only one island.
                return false;
            }

            // Build list of islands with bounding boxes & area (for sorting).
            var islands = new List<(int root, float minU, float minV, float maxU, float maxV, float area, List<int> verts)>();
            float maxWidth = 0.0f;
            float maxHeight = 0.0f;

            foreach (var kvp in islandDict)
            {
                var idxList = kvp.Value;
                float minU = float.PositiveInfinity, minV = float.PositiveInfinity;
                float maxU = float.NegativeInfinity, maxV = float.NegativeInfinity;

                foreach (int idx in idxList)
                {
                    Vector2 uv = verts[idx].TexCoord;
                    if (uv.X < minU) minU = uv.X;
                    if (uv.Y < minV) minV = uv.Y;
                    if (uv.X > maxU) maxU = uv.X;
                    if (uv.Y > maxV) maxV = uv.Y;
                }

                float width = maxU - minU;
                float height = maxV - minV;
                float area = Math.Max(0.0f, width) * Math.Max(0.0f, height);

                maxWidth = Math.Max(maxWidth, width);
                maxHeight = Math.Max(maxHeight, height);

                islands.Add((kvp.Key, minU, minV, maxU, maxV, area, idxList));
            }

            // Sort islands so larger ones are laid out first.
            islands.Sort((a, b) => b.area.CompareTo(a.area));
            int islandCount = islands.Count;

            // Decide how many columns/rows we need in the grid.
            int cols = (int)Math.Ceiling(Math.Sqrt(islandCount));
            int rows = (int)Math.Ceiling(islandCount / (float)cols);

            // Cell size is derived from the largest island size plus margins,
            // so that islands can be translated without scaling.
            float margin = Math.Clamp(_packMargin, 0.0f, 0.45f);
            float cellW = maxWidth + 2.0f * margin;
            float cellH = maxHeight + 2.0f * margin;
            if (cellW <= 0.0f) cellW = 1.0f;
            if (cellH <= 0.0f) cellH = 1.0f;

            bool changed = false;

            for (int idxIsland = 0; idxIsland < islands.Count; idxIsland++)
            {
                var island = islands[idxIsland];
                float islandWidth = island.maxU - island.minU;
                float islandHeight = island.maxV - island.minV;
                if (islandWidth <= 1e-6f || islandHeight <= 1e-6f)
                    continue; // degenerate

                int col = idxIsland % cols;
                int row = idxIsland / cols;

                // Translate island into its grid cell, keeping its original size.
                float baseU = col * cellW + margin;
                float baseV = row * cellH + margin;
                float offsetU = baseU - island.minU;
                float offsetV = baseV - island.minV;

                foreach (int vIndex in island.verts)
                {
                    Vector2 uv = verts[vIndex].TexCoord;
                    float newU = uv.X + offsetU;
                    float newV = uv.Y + offsetV;

                    // Do not clamp to 0..1 here – we want to preserve scale
                    // and allow artists to work with tiles outside this range.
                    if (Math.Abs(newU - uv.X) > 1e-6f || Math.Abs(newV - uv.Y) > 1e-6f)
                    {
                        changed = true;
                        var vtx = verts[vIndex];
                        vtx.TexCoord = new Vector2(newU, newV);
                        verts[vIndex] = vtx;
                    }
                }
            }

            return changed;
        }


        #endregion
    }
}
