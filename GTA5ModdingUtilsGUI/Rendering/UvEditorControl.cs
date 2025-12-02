
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

        private bool _isBoxSelecting;
        private Point _boxStart;
        private Point _boxEnd;

        private bool _isPanning;
        private bool _isDraggingSelection;
        private Point _lastMouse;
        private Point _dragStartMouse;

        private float _zoom = 1.0f;
        private float _panX;
        private float _panY;

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
                ResetView();
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
            _dragStartUvs.Clear();

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
            string help =
                "UV editor controls:\n" +
                "· Left click: select vertex (Shift to multi-select)\n" +
                "· Left drag: transform selection (mode: G=Move, S=Scale, R=Rotate)\n" +
                "· Middle drag: pan\n" +
                "· Drag on empty space: box select\n" +
                "· Ctrl+A: select all, Esc: clear selection";

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

                if (haveSelection && (hit >= 0 || ctrl))
                {
                    // Prepare for transforming the current selection (move / scale / rotate).
                    _isDraggingSelection = true;
                    _dragStartMouse = e.Location;
                    _dragStartUvs.Clear();

                    if (_mesh != null)
                    {
                        var verts = _mesh.Vertices;

                        Vector2 sum = Vector2.Zero;
                        int count = 0;

                        foreach (int idx in _selectedVertices)
                        {
                            if ((uint)idx >= verts.Length)
                                continue;

                            var uv = verts[idx].TexCoord;
                            _dragStartUvs[idx] = uv;
                            sum += uv;
                            count++;
                        }

                        _transformPivotUv = count > 0 ? sum / Math.Max(1, count) : Vector2.Zero;
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
                }
                else if (_isDraggingSelection)
                {
                    // Finalize drag / transform.
                    _isDraggingSelection = false;
                }
            }

            if (!(_isPanning || _isDraggingSelection || _isBoxSelecting))
            {
                Capture = false;
            }

            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

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
                Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                _transformMode = TransformMode.Scale;
                Invalidate();
            }
            else if (e.KeyCode == Keys.R)
            {
                _transformMode = TransformMode.Rotate;
                Invalidate();
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                if (_mesh != null && _mesh.Vertices.Length > 0)
                {
                    _selectedVertices.Clear();
                    for (int i = 0; i < _mesh.Vertices.Length; i++)
                        _selectedVertices.Add(i);

                    Invalidate();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _selectedVertices.Clear();
                _isDraggingSelection = false;
                _isBoxSelecting = false;
                _dragStartUvs.Clear();
                Invalidate();
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

        #endregion
    }
}

