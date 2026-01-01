#!/usr/bin/env python3
"""Convert a Wavefront OBJ mesh into OpenFormats (.mesh + .odr).

This is intended for the GUI workflow (Custom Meshes tab):
- import / UV-edit an OBJ
- convert back to OpenFormats so it can be imported into OpenIV (or used as a reference in LOD pipelines)

The output uses the same OpenFormats templates as the LOD Map generator.
"""

import argparse
import os
import sys

from worker.lod_map_creator.LodMapCreator import LodMapCreator


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description="Convert OBJ to OpenFormats ODR/MESH")
    ap.add_argument("--obj", required=True, help="Path to .obj file")
    ap.add_argument("--outDir", default=None, help="Output directory (default: OBJ directory)")
    ap.add_argument("--name", default=None, help="Base output name (default: OBJ base name)")
    ap.add_argument("--diffuseSampler", default=None, help="Diffuse sampler name (default: lod_<name>)")
    ap.add_argument("--noOverwrite", action="store_true", help="Fail if outputs already exist")

    args = ap.parse_args(argv)

    obj_path = os.path.abspath(args.obj)
    if not os.path.exists(obj_path):
        print(f"ERROR: OBJ not found: {obj_path}")
        return 2

    out_dir = args.outDir
    if out_dir is None or not str(out_dir).strip():
        out_dir = os.path.dirname(obj_path)
    out_dir = os.path.abspath(out_dir)

    base_name = args.name
    sampler = args.diffuseSampler

    # LodMapCreator is used only as a helper for template loading + OBJ parsing + OpenFormats writing.
    helper = LodMapCreator(inputDir='.', outputDir='.', prefix='', clearLod=False, createReflection=False)

    try:
        mesh_path, odr_path = helper.convert_obj_to_openformats(
            obj_path=obj_path,
            out_dir=out_dir,
            base_name=base_name,
            diffuse_sampler=sampler,
            overwrite=not args.noOverwrite,
        )
    except Exception as e:
        print("ERROR:", str(e))
        return 3

    print("WROTE_MESH:", mesh_path)
    print("WROTE_ODR:", odr_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
