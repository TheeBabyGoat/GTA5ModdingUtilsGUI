
#!/usr/bin/env python

import argparse
import os
import sys

from worker.texture_variants import TextureVariantGenerator


def main(argv=None):
    parser = argparse.ArgumentParser(
        description="Generate seasonal variants (spring/fall/winter) for a vegetation texture."
    )
    parser.add_argument("--input", required=True, help="Path to source texture image")
    parser.add_argument("--outputDir", required=True, help="Output directory for generated variants")
    parser.add_argument(
        "--seasons",
        required=True,
        help="Comma-separated list of seasons to generate (spring,fall,winter)",
    )

    args = parser.parse_args(argv)

    seasons = [s.strip() for s in args.seasons.split(",") if s.strip()]

    gen = TextureVariantGenerator()
    try:
        gen.generate_variants(args.input, args.outputDir, seasons)
    except Exception as exc:
        # Propagate a non-zero exit code and print the error so the GUI can show it.
        print(f"Error while generating texture variants: {exc}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
