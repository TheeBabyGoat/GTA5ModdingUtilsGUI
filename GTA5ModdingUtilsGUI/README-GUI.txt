GTA5 Modding Utils GUI
======================

`gta5-modding-utils` Python tools.

How to use
----------

1. Make sure you have followed the original README in `gta5-modding-utils-main`:
   - Install Python 3.11.
   - Verify that `py -3.11` works from a CMD prompt. Use the CMD prompt "python --version" you should see Python 3.11.x.
    If not, adjust your PATH or use the full path to the python executable.
   - Windows has a setting that overrides this, so disable that setting if needed. In Settings > Apps > Advanced app settings > App execution aliases
     "Look for Python and toggle them off".
   - Create and activate the environment from `environment.yml` so all required packages
     (numpy, matplotlib, scikit-learn, shapely, etc.) are available.
   - Run this command from a CMD prompt from your gta5-modding-utils-main folder this will install all needed packages:
        py -3.11 -m pip install numpy matplotlib natsort scipy transforms3d scikit-learn shapely miniball Pillow

2. Run the GUI application:
   - Browse to your gta5-modding-utils environment and select it's root folder.
   - Select the input folder with your `.ymap.xml` files.
   - Optionally select an explicit output folder (otherwise `<input>/generated` is used).
   - Enter a project prefix (same rules as the original script).
   - Choose which steps to run (vegetation, entropy, reducer, clustering, LOD map, etc.).
   - Configure advanced options if needed (reducer resolution, fixed number of clusters,
     clustering prefix and exclusions, polygon JSON, ...).
   - Click **Run**.

3. The log window at the bottom shows the console output of `main.py`. Any matplotlib windows
   opened by the Python scripts will still appear as usual. When the script finishes, check
   the selected output folder for the generated files.

Notes
-----

- The original Python project has been copied into the `gta5-modding-utils-main` folder inside
  the C# project. At build time it is copied next to the executable so the GUI can call `main.py`
  directly.
- The GUI does not change or replace the Python logic; it only makes it easier to run the various
  processing steps without manually typing command-line arguments.