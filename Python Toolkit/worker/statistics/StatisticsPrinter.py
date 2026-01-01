import os
import re

import numpy as np
from matplotlib import pyplot
from natsort import natsorted

from common.PlotManager import PlotManager
from common.ymap.LodLevel import LodLevel
from common.ytyp.YtypItem import YtypItem
from common.ytyp.YtypParser import YtypParser


class StatisticsPrinter:
    countProps: dict[str, dict[str, int]]
    inputDir: str
    ytypItems: dict[str, YtypItem]

    def __init__(self, inputDir: str):
        self.inputDir = inputDir

    def run(self):
        self.readYtypItems()
        self.countProps = {}
        self.processFiles()

    def readYtypItems(self):
        self.ytypItems = YtypParser.readYtypDirectory(os.path.join(os.path.dirname(__file__), "..", "..", "resources", "ytyp"))

    def processFiles(self):
        for filename in natsorted(os.listdir(self.inputDir)):
            if not filename.endswith(".ymap.xml") or filename.endswith("_lod.ymap.xml"):
                continue

            f = open(os.path.join(self.inputDir, filename), 'r')
            content = f.read()

            expression = '<Item type="CEntityDef">' + \
                         '\\s*<archetypeName>([^<]+)</archetypeName>' + \
                         '(?:\\s*<[^/].*>)*?' + \
                         '\\s*<lodLevel>(?:' + LodLevel.HD + "|" + LodLevel.ORPHAN_HD + ')</lodLevel>' + \
                         '(?:\\s*<[^/].*>)*?' + \
                         '\\s*</Item>'

            for match in re.finditer(expression, content):
                archetypeName = match.group(1).lower()

                if archetypeName in self.ytypItems:
                    ytypName = self.ytypItems[archetypeName].parent
                else:
                    ytypName = "others"

                # if not tree.startswith("prop_s_pine_") and not tree.startswith("prop_tree_") and not tree.startswith("prop_w_r_cedar_") and not tree.startswith("test_tree_"):
                #	continue

                if ytypName not in self.countProps:
                    self.countProps[ytypName] = {}

                if archetypeName not in self.countProps[ytypName]:
                    self.countProps[ytypName][archetypeName] = 0

                self.countProps[ytypName][archetypeName] += 1

        totalCount = 0
        ytypCounts = {}
        for ytyp in natsorted(list(self.countProps.keys())):
            ytypCounts[ytyp] = 0
            print(ytyp + ":")
            for prop in natsorted(list(self.countProps[ytyp])):
                num = self.countProps[ytyp][prop]
                ytypCounts[ytyp] += num
                print("\t" + prop + ":\t\t" + str(num))
            totalCount += ytypCounts[ytyp]
            print("\t----------------------------------------------")
            print("\t" + ytyp + " total:\t\t" + str(ytypCounts[ytyp]) + "\n")

        print("\nsummary:")
        for ytyp in natsorted(list(ytypCounts.keys())):
            print(ytyp + ":\t\t" + str(ytypCounts[ytyp]))
        print("----------------------------------------------")
        print("total:\t\t" + str(totalCount))

        # Visualization: bar chart of total instances per ytyp
        if ytypCounts:
            labels = list(natsorted(list(ytypCounts.keys())))
            values = [ytypCounts[y] for y in labels]

            ax = PlotManager.get_axes("statistics", "Statistics")
            pyplot.sca(ax)
            ax.clear()
            ax.set_title("Total instances per ytyp")

            indices = np.arange(len(labels))

            # Use a narrower bar width when there are only a few labels so the bars
            # do not visually "fill" the entire axis when there is only one ytyp.
            if len(indices) <= 3:
                width = 0.4
            elif len(indices) <= 6:
                width = 0.6
            else:
                width = 0.8

            ax.bar(indices, values, width=width)
            ax.set_xticks(indices)
            ax.set_xticklabels(labels, rotation=75)
            ax.set_ylabel("Instance count")
            ax.set_xlabel("ytyp")

            # Ensure there is some horizontal padding so bars do not touch the axes
            # frame even when there is only a single label.
            if len(indices) > 0:
                ax.set_xlim(-0.5, len(indices) - 0.5)
