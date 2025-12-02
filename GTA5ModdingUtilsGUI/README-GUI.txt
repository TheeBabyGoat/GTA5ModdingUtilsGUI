GTA5 Modding Utils GUI
======================

Graphical front‑end for the `gta5-modding-utils` Python tools, plus helpers
for custom vegetation LOD / SLOD atlases and live UV editing.

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
     ```
   - • REPLACE LodMapCreator.py with the provided edited copy gta5-modding-utils-main\\worker\\lod_map_creator\n" +
        • This is needed to allow the JSON file to communicate with the script.

2. In the GUI:
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

Custom assets – LOD / SLOD Atlas Helper
---------------------------------------

The **LOD / SLOD Atlas Helper** button in the *Advanced* section opens a tool
for building JSON data that describes how your custom props use a LOD atlas
texture (for example `vegetation_lod.ytd`).

In the Atlas Helper:

1. Choose your **atlas texture** (PNG/JPG/DDS) and the **props / YTYP XML**.
2. Set the **Atlas grid** (rows and columns) so it matches your atlas layout.
3. For each prop in the grid:
   - Set the **Row** and **Column** (0‑based tile index in the atlas).
   - Adjust **Texture origin (0–1)** and **Plane Z (0–1)** if you need
     per‑prop tweaks.
4. Click **3D Preview…** to open the mesh preview & UV editor and fine‑tune
   a single mesh visually.
5. When you are satisfied, click **Generate** to write the JSON file that you
   can then import into your LOD texture workflow.

LOD Atlas Mesh Preview & UV editor
----------------------------------

The **LOD Atlas Mesh Preview** window lets you preview a mesh (.obj) mapped
onto the atlas and perform Blender‑style UV edits.

Key features:

- **3D edit mode** toggle: when enabled, right‑clicking a face in the 3D view
  selects the corresponding UVs in the 2D editor.
- **UV mode** dropdown: choose between **Move**, **Scale** and **Rotate** to
  decide how the current UV selection is transformed.
- **Live Texture origin / Plane Z**:
  - The numeric fields update the preview immediately.
  - When you click **Save & Close**, the values are written back to the Atlas
    Helper grid for the selected prop.
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
- Most windows (main form, settings, LOD / SLOD Atlas Helper, 3D preview,
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
