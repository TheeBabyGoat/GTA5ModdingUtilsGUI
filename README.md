# GTA5ModdingUtilsGUI
This release ships a Windows desktop front-end for the original gta5-modding-utils Python toolkit.
Instead of typing long command lines, you can point the GUI at a folder of .ymap.xml files, pick which processing steps to run, and watch the live log output while the Python scripts do the heavy lifting.

What this GUI does

Wraps the original main.py from gta5-modding-utils in a simple Windows Forms app.

Lets you browse and configure:

Path to your gta5-modding-utils checkout (or use the bundled copy).

Input folder with .ymap.xml files.

Output folder (auto-defaults to generated inside the input directory if left empty).

Project prefix used for generated files.

Exposes the main processing steps as checkboxes under “Steps to run”:

Vegetation – vegetation creator.

Entropy – entropy / analysis step.

Reducer – reducer stage with optional:

Reducer resolution (numeric up/down).

Adapt scaling toggle.

Clustering – automatically group entities into new maps with:

Number of clusters (optional override).

Polygon (JSON list) – restrict clustering to a specific area.

Clustering prefix – custom name prefix for generated maps.

Excluded maps (comma) – skip specific maps from clustering.

Static col – generate static collision.

LOD map – create LOD/SLOD maps.

Clear LOD – clear existing LOD from inputs.

Reflection – reflection generation (requires LOD map).

Sanitizer – clean and normalize maps.

Statistics – summary/diagnostic statistics.

Provides an “Advanced options” panel for all extra parameters (polygon JSON, clustering prefix/exclusions, reducer settings, etc.).

Shows a live log window that mirrors the Python console output (stdout + stderr), so you can see exactly what main.py is doing.

Includes built-in Help / Readme and Credits windows:

Quick setup overview (Python environment, dependencies).

Link to the original gta5-modding-utils GitHub repository.

Requirements

Windows 10/11 (64-bit)

.NET 6 Desktop Runtime (for running the compiled WinForms app).

Python 3.11 (64-bit)

A conda environment created from the included environment.yml:

conda env create -f environment.yml
conda activate gta5-modding-utils-env


This installs the required packages (numpy, scikit-learn, shapely, matplotlib, natsort, transforms3d, miniball, etc.).

How to use

Set up Python / environment

Install Python 3.11 or Miniconda (64-bit).

Create and activate the environment from environment.yml as shown above.

Make sure python (or py -3.11) from a normal CMD prompt launches the same interpreter used by the environment.

Launch the GUI

Run GTA5ModdingUtilsGUI.exe.

On first launch, read the setup overview on the intro screen if needed.

Configure paths

In “Gta5-Modding-Utils”, either:

Leave empty to use the bundled gta5-modding-utils-main folder next to the executable, or

Browse to your own checkout that contains main.py.

Select Input folder containing your .ymap.xml files.

Optionally select an Output folder (otherwise generated under the input will be used).

Set a Project prefix for all generated files.

Choose processing steps

In “Steps to run”, tick the operations you want (Vegetation, Entropy, Reducer, Clustering, Static col, LOD map, Clear LOD, Reflection, Sanitizer, Statistics).

Open “Advanced options” to:

Set reducer resolution / adapt scaling.

Configure clustering polygon, number of clusters, prefix and exclusions.

Run & review output

Click Run to start the pipeline.

Watch the log window for progress, warnings, or errors.

When it completes, check the selected output folder for generated maps and assets.

Notes

The underlying Python logic is unchanged – this GUI only builds the command line and forwards parameters to main.py.

You can still run gta5-modding-utils purely from the command line; the GUI just makes day-to-day workflows faster and less error-prone.

Credits for the original tool go to the gta5-modding-utils author(s); this project only provides the desktop front-end.

https://github.com/Larcius/gta5-modding-utils
