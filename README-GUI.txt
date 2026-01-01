GTA5 Modding Utils GUI
======================

This Visual Studio solution provides a simple Windows Forms GUI wrapper around the original
`gta5-modding-utils` Python tools.

How to use
----------

1. Make sure you have followed the original README in `gta5-modding-utils-main`:
   - Install Miniconda or Python.
   - Create and activate the environment from `environment.yml` so all required packages
     (numpy, matplotlib, scikit-learn, shapely, etc.) are available.

2. Open `GTA5ModdingUtilsGUI.sln` in Visual Studio 2022 (or any version that supports .NET 6).

3. Build the solution (Debug or Release).

4. Run the GUI application:
   - Optionally browse to your `python.exe` from the gta5-modding-utils environment.
     If left empty, the app will just use `python` from your PATH.
   - Select the input folder with your `.ymap.xml` files.
   - Optionally select an explicit output folder (otherwise `<input>/generated` is used).
   - Enter a project prefix (same rules as the original script).
   - Choose which steps to run (vegetation, entropy, reducer, clustering, LOD map, etc.).
   - Configure advanced options if needed (reducer resolution, fixed number of clusters,
     clustering prefix and exclusions, polygon JSON, ...).
   - Click **Run**.

5. The log window at the bottom shows the console output of `main.py`. Any matplotlib windows
   opened by the Python scripts will still appear as usual. When the script finishes, check
   the selected output folder for the generated files.

Notes
-----

- The original Python project has been copied into the `gta5-modding-utils-main` folder inside
  the C# project. At build time it is copied next to the executable so the GUI can call `main.py`
  directly.
- The GUI does not change or replace the Python logic; it only makes it easier to run the various
  processing steps without manually typing command-line arguments.
