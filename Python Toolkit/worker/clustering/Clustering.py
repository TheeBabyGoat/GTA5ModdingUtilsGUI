
import math
import os
import re
import warnings
from typing import Dict, List, Optional

import numpy as np
from natsort import natsorted
from PIL import Image
from matplotlib import pyplot
import matplotlib.patheffects as PathEffects

from sklearn.exceptions import ConvergenceWarning

from common.Util import Util
from common.PlotManager import PlotManager
from common.ymap.Ymap import Ymap
from common.ytyp.YtypItem import YtypItem
from common.ytyp.YtypParser import YtypParser

# Globally silence scikit-learn's ConvergenceWarning. We clamp the
# number of clusters ourselves, so this warning is just noise for the UI.
warnings.filterwarnings("ignore", category=ConvergenceWarning)


class Clustering:
    """Worker that clusters vegetation entities into new ymaps.

    This implementation is compatible with the original gta5-modding-utils
    API but adds:
      * Integration with the shared PlotManager overview figure.
      * Global suppression of scikit-learn ConvergenceWarning.
    """

    # Type hints (for readability only)
    inputDir: str
    outputDir: str
    defaultYmapPart: Optional[str]
    ymapTemplate: str
    ytypItems: Dict[str, YtypItem]
    prefix: str
    numCluster: Optional[int]
    polygon: Optional[List[List[float]]]
    clusteringPrefix: Optional[str]
    clusteringExcluded: List[str]

    # Constants that control the automatic map hierarchy mode
    GROUP_MAX_EXTEND = 1800
    MAX_EXTEND = 600

    # Regex that captures complete <Item> blocks with <position .../> info.
    # group(0) -> whole entity block
    # group(1..3) -> x, y, z as strings
    _PATTERN = re.compile(
        r"[\t ]*<Item.*?>.*?"
        r"<position\s+[^>]*x=\"([^\"]+)\"\s+[^>]*y=\"([^\"]+)\"\s+[^>]*z=\"([^\"]+)\"[^>]*/>.*?"
        r"</Item>\s*",
        re.DOTALL,
    )

    def __init__(
        self,
        inputDir: str,
        outputDir: str,
        prefix: str,
        numCluster: Optional[int],
        polygon: Optional[List[List[float]]],
        clusteringPrefix: Optional[str],
        clusteringExcluded: Optional[List[str]],
    ):
        self.inputDir = inputDir
        self.outputDir = outputDir
        self.prefix = prefix
        self.numCluster = numCluster
        self.polygon = polygon
        self.clusteringPrefix = clusteringPrefix
        self.clusteringExcluded = [] if clusteringExcluded is None else clusteringExcluded

    # ------------------------------------------------------------------
    # High-level orchestration
    # ------------------------------------------------------------------
    def run(self):
        print("running clustering...")
        self.readYtyps()
        self.readYmapTemplate()
        self.createOutputDir()
        self.processFiles()
        self.fixMapExtents()
        self.copyOthers()
        print("clustering DONE")

    # ------------------------------------------------------------------
    # Setup / IO helpers
    # ------------------------------------------------------------------
    def readYtyps(self):
        self.ytypItems = YtypParser.readYtypDirectory(
            os.path.join(os.path.dirname(__file__), "../..", "resources", "ytyp")
        )

    def createOutputDir(self):
        if os.path.exists(self.outputDir):
            raise ValueError("Output dir " + self.outputDir + " must not exist")
        os.makedirs(self.outputDir)

    def readYmapTemplate(self):
        template_path = os.path.join(
            os.path.dirname(__file__), "templates", "template.ymap.xml"
        )
        with open(template_path, "r") as f:
            self.ymapTemplate = f.read()

        self.defaultYmapPart = self.getYmapPartAfterEntitiesAndBeforeBlock(
            self.ymapTemplate
        )
        if self.defaultYmapPart is None or not self.defaultYmapPart:
            # If the entities block cannot be parsed reliably, just disable the
            # "mapsHavingNotOnlyEntities" detection. Clustering still works.
            self.defaultYmapPart = None

    def getYmapPartAfterEntitiesAndBeforeBlock(self, ymap: str) -> Optional[str]:
        """Extracts the middle part of the ymap (between first and last <Item>)."""

        startIndex = ymap.find("\n        <Item>")
        if startIndex < 0:
            startIndex = ymap.find("\n      <Item>")
            if startIndex < 0:
                return None

        endIndex = ymap.rfind("\n        <Item>")
        if endIndex < 0:
            endIndex = ymap.rfind("\n      <Item>")
            if endIndex < 0:
                return None

        # 14 is the "magic" offset used by the original code to cut just after
        # the opening <Item> tag. We keep it for compatibility with existing
        # template files.
        return ymap[startIndex + 14 : endIndex]

    # ------------------------------------------------------------------
    # Hierarchy computation helpers
    # ------------------------------------------------------------------
    def _calculateMapHierarchy(
        self, points: List[List[float]], hierarchy: List[List[int]]
    ) -> List[List[int]]:
        level = len(hierarchy[0])

        if level == 0:
            maxExtends = Clustering.GROUP_MAX_EXTEND
            unevenClusters = False
        elif level == 1:
            maxExtends = Clustering.MAX_EXTEND
            unevenClusters = False
        else:
            return hierarchy

        absIndices: List[List[int]] = []
        pointsOfParent: List[List[List[float]]] = []

        for i in range(len(points)):
            parentIndex = 0 if len(hierarchy[i]) == 0 else hierarchy[i][0]
            while parentIndex >= len(pointsOfParent):
                absIndices.append([])
                pointsOfParent.append([])
            absIndices[parentIndex].append(i)
            pointsOfParent[parentIndex].append(points[i])

        for parentIndex in range(len(pointsOfParent)):
            clustering, _unused = Util.performClustering(
                pointsOfParent[parentIndex],
                -1,
                maxExtends,
                unevenClusters,
            )

            for c in range(len(clustering)):
                i = absIndices[parentIndex][c]
                hierarchy[i].insert(0, clustering[c])

        return self._calculateMapHierarchy(points, hierarchy)

    def calculateMapHierarchy(self, points: List[List[float]]) -> List[List[int]]:
        if len(points) == 0:
            return []

        hierarchy: List[List[int]] = [[] for _ in range(len(points))]
        return self._calculateMapHierarchy(points, hierarchy)

    # ------------------------------------------------------------------
    # Cluster / map naming helpers
    # ------------------------------------------------------------------
    def getClusterName(
        self,
        group: int,
        cluster: int,
        numGroups: int,
        numClusters: int,
    ) -> str:
        letters = ""
        if numGroups > 1:
            numLetters = math.ceil(math.log(numGroups, 26))
            n = group
            while n > 0:
                letters = chr(97 + (n % 26)) + letters
                n = math.floor(n / 26)
            letters = letters.rjust(numLetters, "a")

        if numClusters > 1:
            numDigits = math.ceil(math.log(numClusters, 10))
            digits = str(cluster).zfill(numDigits)
        else:
            digits = ""

        return letters + ("_" if letters and digits else "") + digits

    # ------------------------------------------------------------------
    # Core processing
    # ------------------------------------------------------------------
    def processFiles(self):
        coords: List[List[float]] = []
        mapsHavingNotOnlyEntities: List[str] = []
        mapsNeededToCopy: List[str] = []
        mapNames: List[str] = []

        # Read all input maps and collect entity coordinates
        for filename in natsorted(os.listdir(self.inputDir)):
            if not filename.endswith(".ymap.xml"):
                continue

            mapName = Util.getMapnameFromFilename(filename)
            if mapName in self.clusteringExcluded:
                mapsNeededToCopy.append(mapName)
                continue

            mapNames.append(mapName)

            print("\treading " + filename)
            with open(os.path.join(self.inputDir, filename), "r") as f:
                content = f.read()

            part = self.getYmapPartAfterEntitiesAndBeforeBlock(content)
            if (
                self.defaultYmapPart is not None
                and part is not None
                and part != self.defaultYmapPart
            ):
                mapsHavingNotOnlyEntities.append(mapName)

            for match in re.finditer(Clustering._PATTERN, content):
                coords.append(
                    [
                        float(match.group(1)),
                        float(match.group(2)),
                        float(match.group(3)),
                    ]
                )

        if not coords:
            return

        print(
            "\tperforming clustering of "
            + str(len(mapNames))
            + " ymap files and in total "
            + str(len(coords))
            + " entities"
        )

        coords_array = np.asarray(coords, dtype=float)
        num_distinct_points = int(len(np.unique(coords_array, axis=0)))

        # Clamp requested number of clusters to distinct points to avoid
        # meaningless KMeans warnings.
        effective_num_clusters: Optional[int] = self.numCluster
        if self.numCluster and num_distinct_points < self.numCluster:
            print(
                f"\t[WARN] Clustering: requested {self.numCluster} clusters but only "
                f"{num_distinct_points} distinct points found. "
                f"Using {num_distinct_points} clusters instead."
            )
            effective_num_clusters = num_distinct_points

        # Compute hierarchy / cluster assignment
        if self.polygon:
            clusters = Util.performClusteringFixedPolygon(coords, self.polygon)
            hierarchy = [[i, 0] for i in clusters]
        elif effective_num_clusters:
            clusters, _unused, _furthest = Util.performClusteringFixedNumClusters(
                coords, effective_num_clusters
            )
            hierarchy = [[i, 0] for i in clusters]
        else:
            hierarchy = self.calculateMapHierarchy(coords)

        # Build grouped entity buffers
        outputFiles: Dict[int, Dict[int, str]] = {}
        mapPrefix = self.getMapPrefix(mapNames)

        for h in hierarchy:
            cluster = h[0]
            group = h[1]
            if group not in outputFiles:
                outputFiles[group] = {}
            if cluster not in outputFiles[group]:
                outputFiles[group][cluster] = ""

        i = 0
        for filename in natsorted(os.listdir(self.inputDir)):
            if not filename.endswith(".ymap.xml"):
                continue

            mapName = Util.getMapnameFromFilename(filename)
            if mapName in mapsNeededToCopy:
                continue

            with open(os.path.join(self.inputDir, filename), "r") as f:
                content = f.read()

            for match in re.finditer(Clustering._PATTERN, content):
                cluster = hierarchy[i][0]
                group = hierarchy[i][1]
                outputFiles[group][cluster] += match.group(0)
                i += 1

        self.writeClusteredYmap(mapPrefix, outputFiles)

        # Copy excluded maps as-is with an "_excluded" suffix
        for mapName in mapsNeededToCopy:
            newMapName = Util.findAvailableMapName(
                self.outputDir, mapName, "_excluded", False
            )
            Util.copyFile(
                self.inputDir,
                self.outputDir,
                Util.getFilenameFromMapname(mapName),
                Util.getFilenameFromMapname(newMapName),
            )

        # For maps with additional content, emit *_no_entities variants
        for mapName in mapsHavingNotOnlyEntities:
            content = Util.readFile(
                os.path.join(self.inputDir, Util.getFilenameFromMapname(mapName))
            )
            newMapName = Util.findAvailableMapName(
                self.outputDir, mapName, "_no_entities", False
            )
            content = Ymap.replaceParent(content, None)
            content = Ymap.replaceName(content, newMapName)
            content = re.sub("[\S\s]*", "", content)
            Util.writeFile(
                os.path.join(self.outputDir, Util.getFilenameFromMapname(newMapName)),
                content,
            )

        # Feed the overview plot
        self.plotClusterResult(coords, hierarchy)

    # ------------------------------------------------------------------
    # Output creation helpers
    # ------------------------------------------------------------------
    def writeClusteredYmap(
        self,
        mapPrefix: str,
        clusteredEntities: Dict[int, Dict[int, str]],
    ):
        numGroups = len(clusteredEntities)
        for group, clusters_in_group in clusteredEntities.items():
            numClusters = len(clusters_in_group)
            for cluster, entities in clusters_in_group.items():
                clusterName = self.getClusterName(
                    group, cluster, numGroups, numClusters
                )
                mapName = mapPrefix.rstrip("_") + (
                    "_" if clusterName else ""
                ) + clusterName
                ymapContent = self.createYmapContent(mapName, entities)
                with open(
                    os.path.join(self.outputDir, Util.getFilenameFromMapname(mapName)),
                    "w",
                ) as file:
                    file.write(ymapContent)

    def createYmapContent(self, mapName: str, entities: str) -> str:
        return (
            self.ymapTemplate.replace("${NAME}", mapName)
            .replace("${TIMESTAMP}", Util.getNowInIsoFormat())
            .replace("${ENTITIES}\n", entities)
        )

    # ------------------------------------------------------------------
    # Overview plotting
    # ------------------------------------------------------------------
    def plotClusterResult(
        self,
        coords: List[List[float]],
        hierarchy: List[List[int]],
    ):
        """Render clustering result into the shared PlotManager overview."""

        if not coords or not hierarchy:
            return

        # Build group -> cluster -> indices mapping
        numTotalClusters = 0
        groups: Dict[int, Dict[int, List[int]]] = {}
        for idx, h in enumerate(hierarchy):
            cluster = h[0]
            group = h[1]
            if group not in groups:
                groups[group] = {}
            if cluster not in groups[group]:
                groups[group][cluster] = []
                numTotalClusters += 1
            groups[group][cluster].append(idx)

        numGroups = len(groups)

        X = np.asarray(coords, dtype=float)
        ax = PlotManager.get_axes("clustering", "Clustering – vegetation clusters")
        PlotManager.setup_world_background(ax)

        cmap = pyplot.cm.get_cmap("gist_ncar", numTotalClusters + 4)
        color_index = 1  # avoid very dark colors at beginning

        for group, clusters_in_group in groups.items():
            numClusters = len(clusters_in_group)
            for cluster, indices in clusters_in_group.items():
                row_ix = np.asarray(indices, dtype=int)

                # white halo for visibility
                ax.scatter(
                    X[row_ix, 0],
                    X[row_ix, 1],
                    marker=".",
                    s=96,
                    edgecolors="none",
                    color="#ffffff",
                    zorder=3,
                )
                ax.scatter(
                    X[row_ix, 0],
                    X[row_ix, 1],
                    marker=".",
                    s=64,
                    edgecolors="none",
                    color=cmap(color_index),
                    zorder=4,
                )

                clusterName = self.getClusterName(group, cluster, numGroups, numClusters)
                if clusterName:
                    annotate = ax.annotate(
                        clusterName,
                        xy=(np.mean(X[row_ix, 0]), np.mean(X[row_ix, 1])),
                        ha="center",
                        va="center",
                        zorder=5,
                    )
                    annotate.set_path_effects(
                        [PathEffects.withStroke(linewidth=4, foreground="w")]
                    )

                color_index += 1

        PlotManager.autoscale_to_points(ax, X[:, :2])

        # Do NOT call pyplot.show(); the GUI controls the figure lifecycle.

    # ------------------------------------------------------------------
    # Misc helpers
    # ------------------------------------------------------------------
    def getMapPrefix(self, mapNames: List[str]) -> str:
        if self.clusteringPrefix is not None:
            return self.clusteringPrefix

        prefixes = Util.determinePrefixBundles(mapNames)
        if len(prefixes) == 1:
            return prefixes[0]
        else:
            return self.prefix

    def fixMapExtents(self):
        """Adapt extents and parent/name of the output maps."""
        print("\tfixing map extents")
        for filename in natsorted(os.listdir(self.outputDir)):
            if not filename.endswith(".ymap.xml"):
                continue

            with open(os.path.join(self.outputDir, filename), "r") as file:
                content = file.read()

            content = Ymap.replaceName(content, filename.lower()[:-9])
            content = Ymap.replaceParent(content, None)
            content = Ymap.fixMapExtents(content, self.ytypItems)

            with open(os.path.join(self.outputDir, filename), "w") as file:
                file.write(content)

    def copyOthers(self):
        """Copy non-ymap files from input directory to output directory."""
        Util.copyFiles(
            self.inputDir,
            self.outputDir,
            lambda filename: not filename.endswith(".ymap.xml"),
        )
