import os
from typing import Dict, Optional

import numpy as np
from matplotlib import pyplot
from matplotlib.widgets import RadioButtons
from PIL import Image


class PlotManager:
    """
    Centralized helper that manages a single figure with multiple "tabs"
    (one Axes per tool) and provides helpers for drawing the GTA V map
    background and autoscaling to a set of 2D points.

    It also manages optional per-tab colorbars so that, for example,
    only the LOD-related plots show a LOD distance gradient.
    """
    _fig = None
    _axes: Dict[str, "pyplot.Axes"] = {}
    _radio_ax = None
    _radio: Optional[RadioButtons] = None
    _colorbars: Dict[str, "pyplot.Colorbar"] = {}

    # ------------------------------------------------------------------
    # Figure / axes management
    # ------------------------------------------------------------------
    @staticmethod
    def _ensure_figure():
        if PlotManager._fig is None:
            PlotManager._fig = pyplot.figure("GTA5 Modding Utils – Overview")
            # Give the window a reasonable default size
            try:
                PlotManager._fig.set_size_inches(9, 7, forward=True)
            except Exception:
                # Some backends do not support set_size_inches; ignore.
                pass

    @staticmethod
    def get_axes(name: str, title: Optional[str] = None):
        """
        Returns the Axes associated with the given logical name.
        If it does not yet exist, it is created and registered.
        """
        PlotManager._ensure_figure()

        if name in PlotManager._axes:
            ax = PlotManager._axes[name]
            if title:
                ax.set_title(title)
        else:
            # Central plotting area.
            # Leave extra space at the bottom so rotated tick labels or
            # long numbers are not cut off.
            ax = PlotManager._fig.add_axes([0.08, 0.22, 0.68, 0.70])
            if title:
                ax.set_title(title)
            PlotManager._axes[name] = ax

        # Make this axes visible and hide the others
        for key, other in PlotManager._axes.items():
            other.set_visible(key == name)

        PlotManager._rebuild_radio(name)
        PlotManager._update_colorbar_visibility(name)
        return ax

    @staticmethod
    def _rebuild_radio(active_name: str):
        """
        (Re)builds the radio button widget used as a tab selector on the right.
        """
        labels = list(PlotManager._axes.keys())
        if not labels:
            return

        PlotManager._ensure_figure()

        if PlotManager._radio_ax is None:
            # [left, bottom, width, height]
            PlotManager._radio_ax = PlotManager._fig.add_axes([0.82, 0.35, 0.15, 0.3])
        else:
            PlotManager._radio_ax.cla()

        PlotManager._radio_ax.set_title("Plots", fontsize=9)

        PlotManager._radio = RadioButtons(
            PlotManager._radio_ax,
            labels,
            active=labels.index(active_name),
        )

        def on_clicked(label: str):
            for key, ax in PlotManager._axes.items():
                ax.set_visible(key == label)
            PlotManager._update_colorbar_visibility(label)
            # Request a redraw of the figure; use draw_idle when available
            try:
                canvas = PlotManager._fig.canvas
                if hasattr(canvas, 'draw_idle'):
                    canvas.draw_idle()
                else:
                    canvas.draw()
            except Exception:
                # As a last resort, fall back to pyplot.draw (always exists)
                pyplot.draw()

        PlotManager._radio.on_clicked(on_clicked)

    # ------------------------------------------------------------------
    # Colorbar management
    # ------------------------------------------------------------------
    @staticmethod
    def _update_colorbar_visibility(active_name: str):
        """
        Ensures that only the colorbar associated with the active plot
        (if any) is visible.
        """
        for name, cb in list(PlotManager._colorbars.items()):
            cb.ax.set_visible(name == active_name)

    @staticmethod
    def set_colorbar(name: str, mappable, label: Optional[str] = None):
        """
        Attach or update a colorbar specific to the given plot name.

        The colorbar will only be visible when that plot is the
        currently active "tab". Pass mappable=None to hide the
        colorbar for that plot.
        """
        PlotManager._ensure_figure()

        if mappable is None:
            cb = PlotManager._colorbars.get(name)
            if cb is not None:
                cb.ax.set_visible(False)
            PlotManager._update_colorbar_visibility(name)
            return

        if name in PlotManager._colorbars:
            cb = PlotManager._colorbars[name]
            # Update the underlying ScalarMappable
            try:
                cb.update_normal(mappable)
            except Exception:
                pass
            cb.ax.set_visible(True)
        else:
            # Place colorbar below the main axes, spanning the same width
            # [left, bottom, width, height]
            cax = PlotManager._fig.add_axes([0.08, 0.14, 0.68, 0.02])
            cb = pyplot.colorbar(mappable, cax=cax, orientation="horizontal")
            PlotManager._colorbars[name] = cb

        if label is not None:
            cb.set_label(label)

        PlotManager._update_colorbar_visibility(name)

    # ------------------------------------------------------------------
    # Background utilities
    # ------------------------------------------------------------------
    @staticmethod
    def _get_img_dir() -> str:
        """
        Returns the directory that contains the world map background images.
        """
        base_dir = os.path.dirname(__file__)
        # worker/clustering/img resides alongside common/ as ../worker/clustering/img
        return os.path.abspath(os.path.join(base_dir, "..", "worker", "clustering", "img"))

    @staticmethod
    def setup_world_background(ax):
        """
        Clears the given axes (keeping its title) and draws the GTA V world map
        background (Los Santos + Cayo Perico), including grid lines.
        """
        if ax is None:
            return

        title = ax.get_title()
        ax.clear()
        if title:
            ax.set_title(title)

        ax.minorticks_on()
        ax.grid(which="major", alpha=0.8)
        ax.grid(which="minor", alpha=0.4)

        # Background imagery
        img_dir = PlotManager._get_img_dir()

        cayo_path = os.path.join(img_dir, "map_cayo.jpg")
        if os.path.exists(cayo_path):
            img_cayo = Image.open(cayo_path)
            ax.imshow(img_cayo, extent=(3500, 5900, -6300, -4000))

        map_path = os.path.join(img_dir, "map.jpg")
        if os.path.exists(map_path):
            img = Image.open(map_path)
            ax.imshow(img, extent=(-4000, 4500, -4000, 8000))

        ax.set_aspect("equal")

    # ------------------------------------------------------------------
    # Autoscaling helper
    # ------------------------------------------------------------------
    @staticmethod
    def autoscale_to_points(ax, coords: np.ndarray):
        """
        Sets the axis limits so that all given 2D points are visible with a margin.
        coords must be an (N, 2) ndarray.
        """
        if ax is None or coords is None or coords.size == 0:
            return

        min_coords = np.min(coords, axis=0)
        max_coords = np.max(coords, axis=0)
        size = max_coords - min_coords
        # keep it robust even for degenerate cases
        margin = max(size[0] * 0.03, size[1] * 0.03, 50.0)
        ax.axis([
            min_coords[0] - margin,
            max_coords[0] + margin,
            min_coords[1] - margin,
            max_coords[1] + margin,
        ])
