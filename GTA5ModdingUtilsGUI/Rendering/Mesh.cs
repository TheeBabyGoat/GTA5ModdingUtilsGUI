using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

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

        /// <summary>
        /// Recomputes <see cref="Center"/> and <see cref="BoundingRadius"/> from the current vertex positions.
        /// This is useful after interactive edits.
        /// </summary>
        public void RecalculateBoundsPublic()
        {
            RecalculateBounds();
        }

        /// <summary>
        /// Deletes the specified vertices and any faces connected to them.
        /// </summary>
        public void DeleteVertices(HashSet<int> vertexIndicesToDelete)
        {
            if (vertexIndicesToDelete == null || vertexIndicesToDelete.Count == 0)
                return;

            // 1. Identify faces to keep (all 3 vertices must NOT be in the delete list)
            var facesToKeep = new List<int>();
            for (int i = 0; i < Indices.Length; i += 3)
            {
                int i0 = Indices[i];
                int i1 = Indices[i + 1];
                int i2 = Indices[i + 2];

                if (!vertexIndicesToDelete.Contains(i0) &&
                    !vertexIndicesToDelete.Contains(i1) &&
                    !vertexIndicesToDelete.Contains(i2))
                {
                    facesToKeep.Add(i0);
                    facesToKeep.Add(i1);
                    facesToKeep.Add(i2);
                }
            }

            // 2. Rebuild Vertices array and create a map from OldIndex -> NewIndex
            var oldToNewIndex = new Dictionary<int, int>();
            var newVertices = new List<Vertex>();

            for (int i = 0; i < Vertices.Length; i++)
            {
                if (!vertexIndicesToDelete.Contains(i))
                {
                    oldToNewIndex[i] = newVertices.Count;
                    newVertices.Add(Vertices[i]);
                }
            }

            // 3. Rebuild Indices array using the new vertex indices
            var newIndices = new List<int>();
            foreach (int oldIdx in facesToKeep)
            {
                if (oldToNewIndex.TryGetValue(oldIdx, out int newIdx))
                {
                    newIndices.Add(newIdx);
                }
            }

            // 4. Update mesh data
            Vertices = newVertices.ToArray();
            Indices = newIndices.ToArray();
            RecalculateBounds();
        }

        /// <summary>
        /// Creates a new Mesh containing only the faces that are fully selected.
        /// </summary>
        public Mesh CloneSubset(HashSet<int> vertexIndicesToClone)
        {
            // Identify faces where ALL vertices are in the selection
            var facesToClone = new List<int>();
            for (int i = 0; i < Indices.Length; i += 3)
            {
                int i0 = Indices[i];
                int i1 = Indices[i + 1];
                int i2 = Indices[i + 2];

                if (vertexIndicesToClone.Contains(i0) &&
                    vertexIndicesToClone.Contains(i1) &&
                    vertexIndicesToClone.Contains(i2))
                {
                    facesToClone.Add(i0);
                    facesToClone.Add(i1);
                    facesToClone.Add(i2);
                }
            }

            if (facesToClone.Count == 0)
                return new Mesh(); // Empty mesh

            // Map old indices to new 0..N range
            var oldToNew = new Dictionary<int, int>();
            var newVerts = new List<Vertex>();
            var newIndices = new List<int>();

            foreach (int oldIdx in facesToClone)
            {
                if (!oldToNew.ContainsKey(oldIdx))
                {
                    oldToNew[oldIdx] = newVerts.Count;
                    newVerts.Add(Vertices[oldIdx]);
                }
                newIndices.Add(oldToNew[oldIdx]);
            }

            var m = new Mesh
            {
                Vertices = newVerts.ToArray(),
                Indices = newIndices.ToArray()
            };
            m.RecalculateBounds();
            return m;
        }

        /// <summary>
        /// Merges another mesh into this one.
        /// </summary>
        public void Append(Mesh other)
        {
            if (other == null || other.Vertices.Length == 0) return;

            int baseIndex = Vertices.Length;

            var combinedVerts = new Vertex[Vertices.Length + other.Vertices.Length];
            Array.Copy(Vertices, 0, combinedVerts, 0, Vertices.Length);
            Array.Copy(other.Vertices, 0, combinedVerts, Vertices.Length, other.Vertices.Length);

            var combinedIndices = new int[Indices.Length + other.Indices.Length];
            Array.Copy(Indices, 0, combinedIndices, 0, Indices.Length);

            for (int i = 0; i < other.Indices.Length; i++)
            {
                combinedIndices[Indices.Length + i] = other.Indices[i] + baseIndex;
            }

            Vertices = combinedVerts;
            Indices = combinedIndices;
            RecalculateBounds();
        }

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

        public void SaveToObj(string path, string? objectName = null)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

            var culture = CultureInfo.InvariantCulture;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            writer.WriteLine("# Exported by GTA5ModdingUtilsGUI LOD Atlas Mesh Preview");
            if (!string.IsNullOrWhiteSpace(objectName))
            {
                writer.WriteLine($"o {SanitizeObjName(objectName!)}");
            }

            // Positions
            for (int i = 0; i < Vertices.Length; i++)
            {
                var p = Vertices[i].Position;
                writer.WriteLine(string.Format(culture, "v {0} {1} {2}", p.X, p.Y, p.Z));
            }

            // UVs
            for (int i = 0; i < Vertices.Length; i++)
            {
                var uv = Vertices[i].TexCoord;
                writer.WriteLine(string.Format(culture, "vt {0} {1}", uv.X, uv.Y));
            }

            // Faces: OBJ is 1-based. We bind v/vt by using the same index for both.
            for (int i = 0; i + 2 < Indices.Length; i += 3)
            {
                int a = Indices[i] + 1;
                int b = Indices[i + 1] + 1;
                int c = Indices[i + 2] + 1;
                writer.WriteLine($"f {a}/{a} {b}/{b} {c}/{c}");
            }
        }

        private static string SanitizeObjName(string name)
        {
            var chars = name.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsWhiteSpace(chars[i]))
                    chars[i] = '_';
            }
            return new string(chars);
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