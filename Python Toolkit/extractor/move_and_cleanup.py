
import os
import shutil

def move_and_cleanup(source_dir, target_dir):
    if not os.path.exists(target_dir):
        os.makedirs(target_dir)

    for root, dirs, files in os.walk(source_dir, topdown=False):  # bottom-up to delete empty dirs later
        if root == source_dir:
            continue  # skip the root source dir itself

        for file in files:
            source_path = os.path.join(root, file)
            target_path = os.path.join(target_dir, file)

            # Move and overwrite
            shutil.move(source_path, target_path)
            print(f"Moved: {source_path} -> {target_path}")

        # After moving files, try to remove the folder if it's empty
        if not os.listdir(root):
            os.rmdir(root)
            print(f"Deleted empty folder: {root}")

# === EXAMPLE USAGE ===
source_directory = r"C:\Users\Crush\gta5-modding-utils-main\resources\extract"
target_directory = r"C:\Users\Crush\gta5-modding-utils-main\resources\models\cs_xref.rpf"

move_and_cleanup(source_directory, target_directory)
