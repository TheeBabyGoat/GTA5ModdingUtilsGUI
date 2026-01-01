GTA5 Modding Utils GUI
======================

Graphical front‑end for the `gta5-modding-utils` Python tools, plus helpers
for custom meshes and live UV editing.

First‑time setup
----------------

1. Follow the original README in `gta5-modding-utils-main`:
   - Install **Python 3.11**.
   - From a CMD prompt, run `python --version` or `py -3.11 --version` and make
     sure it reports Python 3.11.x.
   - Windows can override the python launcher via *App execution aliases*.
     Go to **Settings → Apps → Advanced app settings → App execution aliases**
     and turn off the Python entries if they interfere.
   - Create and activate the environment from `environment.yml` so all required
     packages (numpy, matplotlib, scikit-learn, shapely, etc.) are available.   
   - Also install and additional package `glob2` by running:
     ```
     python -m pip install glob2 
     ```2. In the GUI:
   - Set gta5-modding-utils to the path of your folder containing the enviorment. For example,
     `C:\Users\You\gta5-mod-utils\`).
   - Pick your **Input** and **Output** folders.
   - Choose a **Prefix**. This is the base name used for generated files and
     directories.

Running the main toolchain
--------------------------

The checkboxes in the main window map directly to command‑line flags for
`main.py` in `gta5-modding-utils-main` (see the *Commands* tab in Help for the
exact mapping).

Typical workflow:

1. Enable the workers you want to run, for example:
   - Vegetation
   - Entropy
   - Reducer (with Resolution and Adapt scaling)
   - Clustering (with Prefix / Excluded lists)
   - Static collision, LOD map, Clear LOD, Reflection, Sanitizer, Statistics
2. Fill in advanced options where needed (reducer resolution, number of
   clusters, clustering prefix and exclusions, polygon JSON, etc.).
3. Click **Run**. The log window at the bottom mirrors the console output
   from `main.py`. Any matplotlib windows created by the Python scripts
   will still pop up as usual.

LOD Distance Overrides
----------------------

The **LOD Distance Overrides** panel lets you set a hard `lodDist` value for
common vegetation buckets (Cacti, Trees, Bushes, Palms) when generating LODs.

Internally, the tool still computes a base LOD distance from each HD entity’s
bounding box, bounding sphere, and scale. If overrides are enabled and the
value for a category is > 0, that value is used instead of the computed one:

    finalLodDistance = overrideValue   (when Enable is checked and overrideValue > 0)
    finalLodDistance = baseLodDistance (otherwise)

Controls
--------

- Enable
  - Turns the override system on or off.
  - Unchecked: all categories use the computed distances.
  - Checked: categories with a value > 0 override the computed distance.

- Cacti / Trees / Bushes / Palms
  - Absolute LOD distance to apply for that category.
  - Set to **0** to keep the tool’s computed default for that category.

Effects in-game
---------------

- Higher values
  - Vegetation stays visible from further away (less pop-in).
  - May increase the number of objects rendered at distance (performance cost).

- Lower values
  - Vegetation disappears or swaps to lower-detail models closer to the camera.
  - Can improve performance, especially in dense areas.

Example
-------

To force trees to stay visible until ~200 units from the camera, enable the
panel and set:

- Trees = 200

Leave other categories at 0 if you do not want to override them.


LOD Atlas Mesh Preview & UV editor
----------------------------------

The **LOD Atlas Mesh Preview** window lets you preview a mesh (.obj) mapped
onto the atlas and perform Blender‑style UV edits.

Key features:

- **3D edit mode** toggle: when enabled, right‑clicking a face in the 3D view
  selects the corresponding UVs in the 2D editor.
- **UV mode** dropdown: choose between **Move**, **Scale** and **Rotate** to
  decide how the current UV selection is transformed.
- **Export OBJ**: writes the current UV edits to a new OBJ file.
- **Camera controls**:
  - Left‑drag: orbit around the mesh.
  - Middle‑drag: constrained rotation / orbit for comfortable inspection.
  - Scroll wheel: zoom.

UV editor controls (quick reference)
------------------------------------

- Left click: select vertex (hold **Shift** to multi‑select).
- Left‑drag on a selection: transform (Move / Scale / Rotate depending on
  the current **UV mode**).
- Drag on empty space: box‑select.
- **Ctrl+A**: select all.
- **Esc**: clear the current selection.

Themes
------

The GUI supports multiple color themes so it can better match your workflow:

- Open **Settings → Theme** to choose a preset.
- Most windows (main form, settings, 3D preview,
  etc.) follow the currently selected theme.
- The actual **mesh preview** and **UV editor** drawing areas intentionally
  stay neutral (light gray / checkerboard) so textures remain easy to read
  regardless of theme.

Notes
-----

- The original Python project is copied into `gta5-modding-utils-main` inside
  the C# project and is placed next to the executable at build time so that
  the GUI can call `main.py` directly.
- The GUI does not change or replace the Python logic; it only collects your
  options and runs the same steps you would on the command line.
