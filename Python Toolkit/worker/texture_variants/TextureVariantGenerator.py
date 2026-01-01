
import os
import json
from typing import Iterable, List, Tuple

import numpy as np
from PIL import Image, ImageEnhance


class TextureVariantGenerator:
    """
    Generates seasonal variants (spring, fall, winter) for a vegetation atlas.

    Compared to the very first version, this implementation tries to be a bit
    smarter about:
      * Separating background from vegetation.
      * Detecting leafy pixels vs. branches.
      * Thinning out foliage in fall/winter so the atlas looks less dense.
      * Applying more season-appropriate color grading.

    The operations are deliberately conservative and always preserve the
    original image resolution.
    """

    _SUPPORTED_SEASONS = {"spring", "fall", "winter"}

    # How aggressively to remove leaves in each season (probability that a
    # detected leaf pixel will be significantly faded out).
    _LEAF_REMOVAL_PROB = {
        # Global thinning is deliberately mild; anchors do the heavy lifting.
        "spring": 0.0,
        "fall": 0.12,
        "winter": 0.18,
    }

    def generate_variants(self, input_path: str, output_dir: str, seasons: Iterable[str]) -> None:
        """
        Generate one or more seasonal variants for the given input texture.

        :param input_path: Path to the source texture (PNG/JPG/TGA/BMP, etc.).
        :param output_dir: Directory where the variants will be written.
        :param seasons: Iterable of season names ("spring", "fall", "winter").
        """
        if not os.path.isfile(input_path):
            raise FileNotFoundError(f"Source texture not found: {input_path}")

        os.makedirs(output_dir, exist_ok=True)

        base = Image.open(input_path).convert("RGBA")
        base_name, ext = os.path.splitext(os.path.basename(input_path))

        normalized: List[str] = []
        for s in seasons:
            s = (s or "").strip().lower()
            if s and s in self._SUPPORTED_SEASONS and s not in normalized:
                normalized.append(s)

        if not normalized:
            # Nothing to do – no valid seasons selected.
            return

        anchors = self._load_anchor_config(input_path)

        for season in normalized:
            variant = self._apply_preset(base, season, input_path=input_path, anchors=anchors)
            out_name = f"{base_name}_{season}{ext}"
            out_path = os.path.join(output_dir, out_name)
            variant.save(out_path)

    # ------------------------------------------------------------------ helpers

    @staticmethod
    def _find_background_color(rgb: np.ndarray) -> Tuple[int, int, int]:
        """
        Heuristic: quantize colors on a coarse grid and pick the most frequent
        color as the "background". Works well for atlas textures with a flat
        background and foliage only in some tiles.
        """
        # Downsample for speed.
        sample = rgb[::16, ::16, :]
        # Quantize to 16 levels per channel.
        q = (sample // 16).astype(np.uint8)
        flat = q.reshape(-1, 3)

        # Count occurrences.
        unique, counts = np.unique(flat, axis=0, return_counts=True)
        idx = int(np.argmax(counts))
        bg_q = unique[idx]
        # Map the quantized color back near the center of the bucket.
        bg_color = (bg_q.astype(np.int32) * 16) + 8
        return int(bg_color[0]), int(bg_color[1]), int(bg_color[2])

    @staticmethod
    def _compute_leaf_mask(rgb: np.ndarray, alpha: np.ndarray, bg_color: Tuple[int, int, int]) -> np.ndarray:
        """
        Roughly classify which pixels are likely "leaf" pixels:
          * Far enough away from the background color.
          * Greenish and reasonably saturated.
          * Not too transparent.
        """
        r = rgb[..., 0].astype(np.float32)
        g = rgb[..., 1].astype(np.float32)
        b = rgb[..., 2].astype(np.float32)

        # Background distance.
        br, bg, bb = bg_color
        dr = r - float(br)
        dg = g - float(bg)
        db = b - float(bb)
        dist2 = dr * dr + dg * dg + db * db

        # Consider everything close to the dominant background color as "background".
        background_mask = dist2 < (30.0 * 30.0)

        # Basic saturation estimate.
        maxc = np.maximum(np.maximum(r, g), b)
        minc = np.minimum(np.minimum(r, g), b)
        delta = maxc - minc
        sat = np.zeros_like(maxc)
        nonzero = maxc > 0.0
        sat[nonzero] = delta[nonzero] / maxc[nonzero]

        # Greenish pixels: green is the dominant channel.
        is_green_dominant = (g >= r) & (g >= b)

        # Alpha > threshold to ignore very faint pixels.
        alpha_mask = alpha.astype(np.float32) > 25.0

        leaf_mask = (
            (~background_mask)
            & is_green_dominant
            & (sat > 0.25)
            & (maxc > 50.0)
            & alpha_mask
        )
        return leaf_mask

    def _apply_preset(self, img: Image.Image, season: str, input_path: str, anchors) -> Image.Image:
        rgba = img.convert("RGBA")
        arr = np.array(rgba).astype(np.float32)
        rgb = arr[..., :3]
        alpha = arr[..., 3]

        bg_color = self._find_background_color(rgb.astype(np.uint8))
        leaf_mask = self._compute_leaf_mask(rgb, alpha, bg_color)

        # Deterministic randomness based on input path + season so that the
        # thinning pattern is stable across runs.
        seed = (hash((input_path, season)) & 0xFFFFFFFF)
        rng = np.random.default_rng(seed)

        if season == "spring":
            self._spring_variant(rgb, alpha, leaf_mask, bg_color)
        elif season == "fall":
            self._fall_variant(rgb, alpha, leaf_mask, bg_color, rng)
        elif season == "winter":
            self._winter_variant(rgb, alpha, leaf_mask, bg_color, rng)

        # Apply any anchor-based thinning after the basic per-season color and
        # thinning logic has been applied.
        self._apply_anchor_masks(rgb, alpha, leaf_mask, season, anchors, rng)

        # For winter, add a snow overlay on remaining foliage.
        if (season or "").lower() == "winter":
            self._apply_snow_overlay(rgb, alpha, leaf_mask, rng)

        # Clamp and rebuild RGBA image.
        rgb = np.clip(rgb, 0.0, 255.0).astype(np.uint8)
        alpha = np.clip(alpha, 0.0, 255.0).astype(np.uint8)

        out = np.zeros_like(arr, dtype=np.uint8)
        out[..., :3] = rgb
        out[..., 3] = alpha
        return Image.fromarray(out, mode="RGBA")

    

    # ------------------------------------------------------------------ anchor support

    def _anchor_config_path(self, input_path: str) -> str:
        """Return the JSON path that stores anchor data for a given texture."""
        directory = os.path.dirname(input_path)
        base = os.path.splitext(os.path.basename(input_path))[0]
        return os.path.join(directory, f"{base}_anchors.json")

    def _load_anchor_config(self, input_path: str):
        """Load anchor configuration for the given texture, if present.

        The expected JSON structure is:
        {
          "anchors": [
            {
              "x": 0.35,
              "y": 0.62,
              "radius": 0.08,
              "strength": 1.0,
              "seasons": ["fall", "winter"],
              "polygon": [
                {"x": 0.30, "y": 0.60},
                {"x": 0.40, "y": 0.60},
                {"x": 0.40, "y": 0.70},
                {"x": 0.30, "y": 0.70}
              ]
            }
          ]
        }

        """  # noqa: D401
        cfg_path = self._anchor_config_path(input_path)
        if not os.path.isfile(cfg_path):
            return []

        try:
            with open(cfg_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        except Exception:
            return []

        anchors = []
        for raw in data.get("anchors", []):
            if not isinstance(raw, dict):
                continue
            try:
                x = float(raw.get("x", 0.5))
                y = float(raw.get("y", 0.5))
                radius = float(raw.get("radius", 0.1))
                strength = float(raw.get("strength", 1.0))
            except Exception:
                continue

            seasons_raw = raw.get("seasons", [])
            if not isinstance(seasons_raw, (list, tuple)):
                seasons_raw = []

            seasons = []
            for s in seasons_raw:
                if isinstance(s, str):
                    s_lower = s.strip().lower()
                    if s_lower:
                        seasons.append(s_lower)

            # Optional polygon support: list of points in normalized [0, 1] space.
            polygon_clean = None
            poly_raw = raw.get("polygon")
            if isinstance(poly_raw, list):
                pts: List[Tuple[float, float]] = []
                for p in poly_raw:
                    try:
                        if isinstance(p, dict):
                            px = float(p.get("x", 0.0))
                            py = float(p.get("y", 0.0))
                        elif isinstance(p, (list, tuple)) and len(p) >= 2:
                            px = float(p[0])
                            py = float(p[1])
                        else:
                            continue
                        px = max(0.0, min(1.0, px))
                        py = max(0.0, min(1.0, py))
                        pts.append((px, py))
                    except Exception:
                        continue
                if len(pts) >= 3:
                    polygon_clean = pts

            anchors.append(
                {
                    "x": max(0.0, min(1.0, x)),
                    "y": max(0.0, min(1.0, y)),
                    "radius": max(0.0, min(1.0, radius)),
                    "strength": max(0.0, strength),
                    "seasons": seasons,
                    "polygon": polygon_clean,
                }
            )

        return anchors

    def _apply_anchor_masks(
        self,
        rgb: np.ndarray,
        alpha: np.ndarray,
        leaf_mask: np.ndarray,
        season: str,
        anchors,
        rng: np.random.Generator,
    ) -> None:
        """Apply anchor-based thinning for the given season.

        This version is intentionally strong so that the effect of anchors is
        clearly visible. For fall and winter, foliage pixels inside an anchor
        are fully removed (alpha = 0) at default strength = 1. For spring,
        anchors are ignored unless explicitly enabled for that season, and
        even then the effect is milder.
        """
        if not anchors:
            return

        h, w = alpha.shape
        if h == 0 or w == 0:
            return

        yy, xx = np.indices((h, w))
        season_l = (season or "").lower()

        for anchor in anchors:
            seasons = anchor.get("seasons") or []
            seasons_l = [str(s).lower() for s in seasons]
            if season_l not in seasons_l:
                continue

            strength = float(anchor.get("strength", 1.0))
            if strength <= 0.0:
                continue

            # Clamp strength early.
            strength = max(0.0, min(1.0, strength))

            # Determine anchor region: polygon (preferred) or circular fallback.
            region_mask = None

            polygon = anchor.get("polygon")
            if polygon:
                try:
                    poly_arr = np.asarray(polygon, dtype=np.float32)
                    if poly_arr.ndim == 2 and poly_arr.shape[0] >= 3 and poly_arr.shape[1] >= 2:
                        poly_x = poly_arr[:, 0] * float(w - 1)
                        poly_y = poly_arr[:, 1] * float(h - 1)
                        region_mask = self._polygon_mask(xx, yy, poly_x, poly_y)
                except Exception:
                    region_mask = None

            if region_mask is None:
                radius_norm = float(anchor.get("radius", 0.0))
                if radius_norm <= 0.0:
                    continue

                cx_norm = float(anchor.get("x", 0.5))
                cy_norm = float(anchor.get("y", 0.5))

                cx = cx_norm * (w - 1)
                cy = cy_norm * (h - 1)
                radius_px = radius_norm * float(max(w, h))
                if radius_px <= 1.0:
                    continue

                dist2 = (xx - cx) ** 2 + (yy - cy) ** 2
                region_mask = dist2 <= radius_px * radius_px

            if not np.any(region_mask):
                continue

            # Pixels inside the anchor region that are currently visible.
            mask = region_mask & (alpha > 0.0)
            if not np.any(mask):
                continue

            if season_l in ("fall", "winter") and strength >= 0.99:
                # For fall/winter at full strength, completely clear the foliage
                # inside the anchor region.
                alpha[mask] = 0.0
            else:
                # For other cases, just fade alpha by a factor.
                if season_l == "spring":
                    base_removal = 0.5
                elif season_l == "fall":
                    base_removal = 0.8
                elif season_l == "winter":
                    base_removal = 0.9
                else:
                    base_removal = 0.0

                if base_removal <= 0.0:
                    continue

                removal = base_removal * strength
                alpha_factor = 1.0 - removal
                alpha_factor = max(0.0, min(1.0, alpha_factor))

                alpha[mask] = alpha[mask] * alpha_factor

    @staticmethod
    def _polygon_mask(
        xx: np.ndarray,
        yy: np.ndarray,
        poly_x: np.ndarray,
        poly_y: np.ndarray,
    ) -> np.ndarray:
        """Return a boolean mask of points inside a polygon using the even–odd rule.

        xx, yy are 2D index grids (pixel coordinates).
        poly_x, poly_y are 1D arrays of polygon vertices in pixel coordinates.
        """
        if poly_x.size < 3:
            return np.zeros_like(xx, dtype=bool)

        # Flatten for vectorized computation.
        x = xx.ravel()
        y = yy.ravel()
        inside = np.zeros(x.shape, dtype=bool)

        n = poly_x.size
        for i in range(n):
            j = (i - 1) % n
            xi, yi = poly_x[i], poly_y[i]
            xj, yj = poly_x[j], poly_y[j]

            # Check if the edge crosses the horizontal line at each y.
            cond = ((yi > y) != (yj > y)) & (
                x < (xj - xi) * (y - yi) / ((yj - yi) + 1e-12) + xi
            )
            inside ^= cond

        return inside.reshape(xx.shape)

    def _apply_snow_overlay(
        self,
        rgb: np.ndarray,
        alpha: np.ndarray,
        leaf_mask: np.ndarray,
        rng: np.random.Generator,
        fraction: float = 0.12,
    ) -> None:
        """Add simple snow specs on remaining leaf pixels.

        A random subset of sufficiently opaque leaf pixels is turned into a
        bright, slightly bluish white. The randomness is deterministic because
        the RNG is seeded by the caller.
        """
        if fraction <= 0.0:
            return

        candidates = leaf_mask & (alpha > 80.0)
        idx = np.where(candidates)
        if idx[0].size == 0:
            return

        num_pixels = idx[0].size
        count = int(num_pixels * fraction)
        if count <= 0:
            return

        # Choose pixels without replacement.
        selected = rng.choice(num_pixels, size=count, replace=False)
        snow_idx = (idx[0][selected], idx[1][selected])

        r = rgb[..., 0]
        g = rgb[..., 1]
        b = rgb[..., 2]

        r[snow_idx] = 245.0
        g[snow_idx] = 245.0
        b[snow_idx] = 255.0
# ------------------------------------------------------------------ per-season logic

    def _spring_variant(self, rgb: np.ndarray, alpha: np.ndarray, leaf_mask: np.ndarray, bg_color: Tuple[int, int, int]) -> None:
        """
        Spring: keep density, make green foliage a bit fresher/brighter and
        slightly more saturated, without turning branches or background neon.
        """
        # Mild global color enhancement (avoid background drift by working only
        # on non-background foliage where possible).
        # For simplicity we apply a gentle brightness/saturation gain to
        # everything, then a stronger gain to leaves.
        rgb *= 1.03  # slight global lift

        # Stronger changes only on leaf pixels.
        r = rgb[..., 0]
        g = rgb[..., 1]
        b = rgb[..., 2]

        # Boost green channel a bit and saturation.
        r_leaf = r[leaf_mask]
        g_leaf = g[leaf_mask]
        b_leaf = b[leaf_mask]

        # brighten leaves and push towards a fresher green
        g_leaf = g_leaf * 1.10 + 4.0
        r_leaf = r_leaf * 0.95
        b_leaf = b_leaf * 0.95

        r[leaf_mask] = r_leaf
        g[leaf_mask] = g_leaf
        b[leaf_mask] = b_leaf

    def _fall_variant(self, rgb: np.ndarray, alpha: np.ndarray, leaf_mask: np.ndarray, bg_color: Tuple[int, int, int], rng: np.random.Generator) -> None:
        """
        Fall: warm color grading for foliage + moderate thinning of leaves.
        Leaves shift from green towards yellow/orange and some percentage fade
        out to give a sparser look.
        """
        r = rgb[..., 0]
        g = rgb[..., 1]
        b = rgb[..., 2]

        # Base: slightly warm the whole texture (branches as well).
        rgb *= np.array([1.03, 0.98, 0.93], dtype=np.float32)

        # Thinning: randomly fade out a fraction of leaf pixels.
        prob = self._LEAF_REMOVAL_PROB.get("fall", 0.35)
        if prob > 0.0:
            mask_idx = np.where(leaf_mask)
            if mask_idx[0].size > 0:
                rand_vals = rng.random(mask_idx[0].shape)
                to_fade = rand_vals < prob
                if np.any(to_fade):
                    fade_idx = (mask_idx[0][to_fade], mask_idx[1][to_fade])
                    # Reduce alpha strongly; keep a tiny bit so there is still
                    # some continuity if the engine uses alpha testing.
                    alpha[fade_idx] *= 0.25

        # Color shift for remaining leaves (those still reasonably opaque).
        effective_leaf_mask = leaf_mask & (alpha > 40.0)
        if np.any(effective_leaf_mask):
            rl = r[effective_leaf_mask]
            gl = g[effective_leaf_mask]
            bl = b[effective_leaf_mask]

            # Compute a simple "warm" mix: emphasize red/yellow, slightly
            # mute blue/green.
            # Start from a luminance-like value.
            lum = 0.3 * rl + 0.59 * gl + 0.11 * bl

            r_new = lum + 30.0  # a bit more red/orange
            g_new = lum + 5.0   # slightly less than red -> orange/yellow
            b_new = bl * 0.6    # dampen blue

            r[effective_leaf_mask] = r_new
            g[effective_leaf_mask] = g_new
            b[effective_leaf_mask] = b_new

    
    def _winter_variant(
        self,
        rgb: np.ndarray,
        alpha: np.ndarray,
        leaf_mask: np.ndarray,
        bg_color: Tuple[int, int, int],
        rng: np.random.Generator,
    ) -> None:
        """Winter: late-fall style color + moderate thinning.

        We keep some of the underlying leaf color (no full grayscale), cool the
        tones slightly, and thin foliage somewhat. Additional, stronger
        thinning and snow specs are handled by anchors and _apply_snow_overlay.
        """
        r = rgb[..., 0]
        g = rgb[..., 1]
        b = rgb[..., 2]

        # Mild global desaturation towards luminance to avoid overly vibrant
        # greens in winter.
        lum = 0.3 * r + 0.59 * g + 0.11 * b
        mix = 0.4  # 0 -> original, 1 -> grayscale
        rgb[..., 0] = mix * lum + (1.0 - mix) * r
        rgb[..., 1] = mix * lum + (1.0 - mix) * g
        rgb[..., 2] = mix * lum + (1.0 - mix) * b

        # Slight cool tint on non-background pixels.
        br, bgc, bb = bg_color
        dr = rgb[..., 0] - float(br)
        dg = rgb[..., 1] - float(bgc)
        db = rgb[..., 2] - float(bb)
        dist2 = dr * dr + dg * dg + db * db
        non_background = dist2 > (25.0 * 25.0)

        rgb[non_background, 0] *= 0.96  # slightly reduce red
        rgb[non_background, 2] *= 1.06  # slightly boost blue

        # Baseline thinning: less aggressive than before, anchors will do the
        # heavy lifting for specific clumps.
        prob = self._LEAF_REMOVAL_PROB.get("winter", 0.4)
        if prob > 0.0:
            mask_idx = np.where(leaf_mask)
            if mask_idx[0].size > 0:
                rand_vals = rng.random(mask_idx[0].shape)
                to_fade = rand_vals < prob
                if np.any(to_fade):
                    fade_idx = (mask_idx[0][to_fade], mask_idx[1][to_fade])
                    alpha[fade_idx] *= 0.4

