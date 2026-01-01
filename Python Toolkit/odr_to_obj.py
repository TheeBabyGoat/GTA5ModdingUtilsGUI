#!/usr/bin/env python3
"""Convert OpenFormats .odr (+ referenced .mesh) to Wavefront OBJ.

Designed for the Modding Utility round-trip workflow:
  ODR/MESH -> OBJ (UV edit) -> OBJ -> ODR/MESH

Assumptions:
- Input .odr is OpenFormats text exported from CodeWalker/OpenFormats.
- Referenced .mesh is in the same directory as the .odr (typical layout).
- Mesh format matches the templates used by gta5-modding-utils (Geometry blocks, Indices/Vertices).

If --outObj is omitted, output defaults to <odr_base>_uv.obj next to the .odr.
"""

from __future__ import annotations

import argparse
import os
import re
import sys
from typing import List, Tuple


_MESH_REF_RE = re.compile(r"\b([^\s{}]+\.mesh)\b")


def _read_text(path: str) -> str:
    with open(path, "r", encoding="utf-8", errors="replace") as f:
        return f.read()


def _find_mesh_filename_in_odr(odr_text: str) -> str:
    """Return the first referenced .mesh filename from the ODR."""
    # The template used by this project includes a line like: "<name>.mesh 0" inside LodGroup/High.
    # We simply grab the first occurrence of something ending with .mesh.
    m = _MESH_REF_RE.search(odr_text)
    if not m:
        raise RuntimeError("Could not find referenced .mesh filename inside the .odr")
    return m.group(1)


def _parse_vertices_block(vertices_text: str, flip_v: bool) -> Tuple[List[Tuple[float, float, float]], List[Tuple[float, float]], List[Tuple[float, float, float]]]:
    positions: List[Tuple[float, float, float]] = []
    uvs: List[Tuple[float, float]] = []
    normals: List[Tuple[float, float, float]] = []

    for raw in vertices_text.splitlines():
        line = raw.strip()
        if not line:
            continue
        if "/" not in line:
            continue

        parts = [p.strip() for p in line.split("/")]
        if len(parts) < 4:
            # Unexpected vertex format
            continue

        # Position
        p = parts[0].split()
        if len(p) < 3:
            continue
        x, y, z = float(p[0]), float(p[1]), float(p[2])

        # Normal
        n = parts[1].split()
        if len(n) < 3:
            continue
        nx, ny, nz = float(n[0]), float(n[1]), float(n[2])

        # UV (last part)
        t = parts[3].split()
        if len(t) < 2:
            continue
        u, v = float(t[0]), float(t[1])
        if flip_v:
            v = 1.0 - v

        positions.append((x, y, z))
        normals.append((nx, ny, nz))
        uvs.append((u, v))

    return positions, uvs, normals


def _parse_indices_block(indices_text: str) -> List[int]:
    # Indices are integers separated by whitespace/newlines.
    nums = re.findall(r"-?\d+", indices_text)
    return [int(n) for n in nums]


def _parse_mesh_geometries(mesh_text: str, flip_v: bool):
    """Yield geometries as (positions, uvs, normals, indices)."""

    # Non-greedy capture of Geometry blocks, and their Indices/Vertices contents.
    geom_re = re.compile(
        r"Geometry\s*\{.*?"
        r"Indices\s+\d+\s*\{(?P<indices>.*?)\}\s*"
        r"Vertices\s+\d+\s*\{(?P<verts>.*?)\}\s*"
        r"\}",
        re.DOTALL,
    )

    any_found = False
    for m in geom_re.finditer(mesh_text):
        any_found = True
        indices_text = m.group("indices")
        verts_text = m.group("verts")

        positions, uvs, normals = _parse_vertices_block(verts_text, flip_v=flip_v)
        indices = _parse_indices_block(indices_text)

        if not positions or not indices:
            continue

        yield positions, uvs, normals, indices

    if not any_found:
        raise RuntimeError("No Geometry blocks found in .mesh; unsupported format?")


def _write_obj(out_path: str, geometries) -> None:
    os.makedirs(os.path.dirname(out_path) or ".", exist_ok=True)

    with open(out_path, "w", encoding="utf-8") as f:
        f.write("# Exported by odr_to_obj.py\n")

        v_offset = 0  # OBJ is 1-based; we'll add 1 later.

        for gi, (positions, uvs, normals, indices) in enumerate(geometries):
            f.write(f"\no geom_{gi}\n")
            f.write(f"g geom_{gi}\n")

            for (x, y, z) in positions:
                f.write(f"v {x:.8f} {y:.8f} {z:.8f}\n")
            for (u, v) in uvs:
                f.write(f"vt {u:.8f} {v:.8f}\n")
            for (nx, ny, nz) in normals:
                f.write(f"vn {nx:.8f} {ny:.8f} {nz:.8f}\n")

            # Faces: indices are in triangle list order
            if len(indices) % 3 != 0:
                # Still write what we can
                pass

            for i in range(0, len(indices) - 2, 3):
                a = indices[i] + 1 + v_offset
                b = indices[i + 1] + 1 + v_offset
                c = indices[i + 2] + 1 + v_offset
                # Use the same index for v/vt/vn since buffers are aligned
                f.write(f"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}\n")

            v_offset += len(positions)


def main(argv: List[str]) -> int:
    ap = argparse.ArgumentParser(description="Convert OpenFormats .odr/.mesh to Wavefront .obj")
    ap.add_argument("--odr", required=True, help="Path to input .odr")
    ap.add_argument("--outObj", default="", help="Path to output .obj (optional)")
    ap.add_argument(
        "--noFlipV",
        action="store_true",
        help="Do not flip V (by default V is flipped: v = 1 - v) to match typical OBJ UV editing expectations.",
    )

    args = ap.parse_args(argv)

    odr_path = os.path.abspath(args.odr)
    if not os.path.isfile(odr_path):
        print(f"error: ODR not found: {odr_path}", file=sys.stderr)
        return 2

    odr_dir = os.path.dirname(odr_path)
    odr_base = os.path.splitext(os.path.basename(odr_path))[0]

    out_obj = args.outObj.strip()
    if not out_obj:
        out_obj = os.path.join(odr_dir, odr_base + "_uv.obj")
    out_obj = os.path.abspath(out_obj)

    flip_v = not args.noFlipV

    odr_text = _read_text(odr_path)
    mesh_name = _find_mesh_filename_in_odr(odr_text)
    mesh_path = os.path.join(odr_dir, mesh_name)
    if not os.path.isfile(mesh_path):
        # Some exports may include a relative path; try resolving as-is
        alt = os.path.abspath(os.path.join(odr_dir, mesh_name.replace("/", os.sep).replace("\\", os.sep)))
        if os.path.isfile(alt):
            mesh_path = alt
        else:
            print(f"error: referenced .mesh not found: {mesh_path}", file=sys.stderr)
            return 3

    mesh_text = _read_text(mesh_path)

    geoms = list(_parse_mesh_geometries(mesh_text, flip_v=flip_v))
    if not geoms:
        print("error: no usable geometry extracted from .mesh", file=sys.stderr)
        return 4

    _write_obj(out_obj, geoms)

    print(f"wrote: {out_obj}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
