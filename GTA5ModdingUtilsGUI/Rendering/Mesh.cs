using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace GTA5ModdingUtilsGUI.Rendering
{
    /// <summary>
    /// Very small mesh representation and OBJ loader used only by the atlas preview window.
    /// This is intentionally simple and does not try to be a full-featured 3D asset loader.
    /// </summary>
    public class Mesh
    {
        public struct Vertex
        {
            public Vector3 Position;
            public Vector2 TexCoord;
        }

        public Vertex[] Vertices { get; private set; } = Array.Empty<Vertex>();
        public int[] Indices { get; private set; } = Array.Empty<int>();
        public Vector3 Center { get; private set; }
        public float BoundingRadius { get; private set; }

        public static Mesh LoadFromObj(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("Mesh file not found", path);

            using var reader = new StreamReader(path);

            var positions = new List<Vector3>();
            var texCoords = new List<Vector2>();
            var verts = new List<Vertex>();
            var indices = new List<int>();

            var culture = CultureInfo.InvariantCulture;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        if (parts.Length >= 4 &&
                            float.TryParse(parts[1], NumberStyles.Float, culture, out float vx) &&
                            float.TryParse(parts[2], NumberStyles.Float, culture, out float vy) &&
                            float.TryParse(parts[3], NumberStyles.Float, culture, out float vz))
                        {
                            positions.Add(new Vector3(vx, vy, vz));
                        }
                        break;

                    case "vt":
                        if (parts.Length >= 3 &&
                            float.TryParse(parts[1], NumberStyles.Float, culture, out float vu) &&
                            float.TryParse(parts[2], NumberStyles.Float, culture, out float vv))
                        {
                            texCoords.Add(new Vector2(vu, vv));
                        }
                        break;

                    case "f":
                        if (parts.Length < 4) break; // need at least a triangle

                        int firstIndex = -1;
                        int prevIndex = -1;

                        for (int i = 1; i < parts.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(parts[i])) continue;

                            var comps = parts[i].Split('/');
                            if (comps.Length == 0 || comps[0].Length == 0) continue;

                            if (!int.TryParse(comps[0], NumberStyles.Integer, culture, out int vi))
                            {
                                continue;
                            }

                            int ti = 0;
                            if (comps.Length > 1 && comps[1].Length > 0)
                            {
                                int.TryParse(comps[1], NumberStyles.Integer, culture, out ti);
                            }

                            // OBJ indices are 1-based, and can be negative (relative from the end).
                            if (vi < 0) vi = positions.Count + vi;
                            else vi -= 1;

                            if (ti < 0) ti = texCoords.Count + ti;
                            else if (ti > 0) ti -= 1;

                            var pos = (vi >= 0 && vi < positions.Count) ? positions[vi] : Vector3.Zero;
                            var uv = (ti >= 0 && ti < texCoords.Count) ? texCoords[ti] : Vector2.Zero;

                            verts.Add(new Vertex { Position = pos, TexCoord = uv });
                            int idx = verts.Count - 1;

                            if (firstIndex == -1)
                            {
                                firstIndex = idx;
                            }
                            else if (prevIndex != -1)
                            {
                                // Fan triangulation for polygons with more than 3 vertices
                                indices.Add(firstIndex);
                                indices.Add(prevIndex);
                                indices.Add(idx);
                            }

                            prevIndex = idx;
                        }

                        break;
                }
            }

            var mesh = new Mesh
            {
                Vertices = verts.ToArray(),
                Indices = indices.ToArray()
            };
            mesh.RecalculateBounds();
            return mesh;
        }

        private void RecalculateBounds()
        {
            if (Vertices.Length == 0)
            {
                Center = Vector3.Zero;
                BoundingRadius = 1.0f;
                return;
            }

            Vector3 min = Vertices[0].Position;
            Vector3 max = Vertices[0].Position;

            foreach (var v in Vertices)
            {
                min = Vector3.Min(min, v.Position);
                max = Vector3.Max(max, v.Position);
            }

            Center = (min + max) * 0.5f;

            float radius = 0.0f;
            foreach (var v in Vertices)
            {
                float d = Vector3.Distance(Center, v.Position);
                if (d > radius) radius = d;
            }

            BoundingRadius = radius > 0.0001f ? radius : 1.0f;
        }
    }
}
