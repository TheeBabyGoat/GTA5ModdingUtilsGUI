import math
import shutil
import os
import re
import json
import numpy as np
import transforms3d
from matplotlib import pyplot
from numpy.linalg import norm
from re import Match
from typing import IO, Optional, Tuple, Dict, Any
from natsort import natsorted
from dataclasses import dataclass

from common.BoundingGeometry import BoundingGeometry
from common.Box import Box
from common.PlotManager import PlotManager
from common.Sphere import Sphere
from common.Util import Util
from common.texture.UV import UV
from common.texture.UVMap import UVMap
from common.ymap.ContentFlag import ContentFlag
from common.ymap.EntityItem import EntityItem
from common.ymap.Flag import Flag
from common.ymap.LodLevel import LodLevel
from common.ymap.PriorityLevel import PriorityLevel
from common.ymap.Ymap import Ymap
from common.ytyp.YtypItem import YtypItem
from common.ytyp.YtypParser import YtypParser
from worker.lod_map_creator.LodCandidate import LodCandidate
from worker.lod_map_creator.Manifest import Manifest


class LodMapCreator:
    inputDir: str
    outputDir: str

    prefix: str
    bundlePrefixes: list[str]
    clearLod: bool
    createReflection: bool

    contentTemplateYtypItem: str
    contentTemplateMesh: str
    contentTemplateMeshAabb: str
    contentTemplateMeshGeometry: str
    contentTemplateOdr: str
    contentTemplateOdrShaderTreeLod: str
    contentTemplateOdrShaderTreeLod2: str
    contentTemplateOdrShaderAlpha: str
    contentTemplateEntitySlod: str
    contentTemplateSlod2Map: str

    ytypItems: dict[str, YtypItem]
    reflYtypItems: dict[str, IO]
    slodYtypItems: dict[str, IO]
    slodCandidates: dict[str, UVMap]
    foundLod: bool
    foundSlod: bool

    # Cached key sets for BUILT-IN candidates only (lowercased). We keep these so
    # that helper-mesh export (custom_meshes/) is not suppressed when we later add
    # dynamic custom candidates at runtime.
    _builtinLodKeysLower: set[str]
    _builtinSlodKeysLower: set[str]

    lodCandidates: dict[str, LodCandidate]

    MAX_NUM_CHILDREN_IN_DRAWABLE_DICTIONARY = 63

    TEXTURE_DICTIONARY_LOD = "vegetation_lod"
    TEXTURE_DICTIONARY_SLOD = "vegetation_slod"

    def prepareLodCandidates(self):
        lodCandidates = {
            # cacti
            "prop_cactus_01a": LodCandidate(0.265625),
            "prop_cactus_01b": LodCandidate(0.5),
            "prop_cactus_01c": LodCandidate(0.53125),
            "prop_cactus_01d": LodCandidate(0.4375),
            "prop_cactus_01e": LodCandidate(0.5),
            "prop_joshua_tree_01a": LodCandidate(0.453125),
            "prop_joshua_tree_01b": LodCandidate(0.5),
            "prop_joshua_tree_01c": LodCandidate(0.53125),
            "prop_joshua_tree_01d": LodCandidate(0.40625),
            "prop_joshua_tree_01e": LodCandidate(0.46875),
            "prop_joshua_tree_02a": LodCandidate(0.546875),
            "prop_joshua_tree_02b": LodCandidate(0.609375),
            "prop_joshua_tree_02c": LodCandidate(0.484375),
            "prop_joshua_tree_02d": LodCandidate(0.734375, None, UV(0, 0), UV(0.5, 1), None, None, None, UV(0.5, 0), UV(1, 1), 0.3125),
            "prop_joshua_tree_02e": LodCandidate(0.546875),
            # trees
            "prop_tree_birch_01": LodCandidate(0.546875, 0.6484375, UV(0, 0), UV(0.5, 1), UV(0.5, 0), UV(1, 1), UV(0.7734375, 0.5078125)),
            "prop_tree_birch_02": LodCandidate(0.421875, 0.4765625, UV(0, 0), UV(0.5, 1), UV(0.5, 0), UV(1, 1), UV(0.765625, 0.3671875)),
            "prop_tree_birch_03": LodCandidate(0.546875),
            "prop_tree_birch_03b": LodCandidate(0.5625),
            "prop_tree_birch_04": LodCandidate(0.5625, 0.3515625, UV(0, 0), UV(0.5, 1), UV(0.5, 1), UV(1, 0), UV(0.7734375, 0.453125)),
            "prop_tree_maple_02": LodCandidate(0.421875),
            "prop_tree_maple_03": LodCandidate(0.5),
            "prop_tree_cedar_02": LodCandidate(0.5078125, 0.40104166666, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_tree_cedar_03": LodCandidate(0.5234375, 0.46875, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_tree_cedar_04": LodCandidate(0.484375, 0.34375, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1), UV(0.484375, 0.87890625)),
            "prop_tree_cedar_s_01": LodCandidate(0.484375, 0.66875, UV(0, 0), UV(1, 0.625), UV(0, 0.625), UV(1, 1), UV(0.46875, 0.8203125)),
            "prop_tree_cedar_s_02": LodCandidate(0.5),
            "prop_tree_cedar_s_04": LodCandidate(0.5, 0.67307692307, UV(0, 0), UV(1, 0.8125), UV(0, 1), UV(1, 0.8125)),
            "prop_tree_cedar_s_05": LodCandidate(0.46875),
            "prop_tree_cedar_s_06": LodCandidate(0.5),
            "prop_tree_cypress_01": LodCandidate(0.5, 0.66666666666, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_tree_eng_oak_01": LodCandidate(0.5, 0.375, UV(0, 0), UV(0.5, 1), UV(0.5, 0), UV(1, 1), UV(0.75, 0.53125)),
            "prop_tree_eucalip_01": LodCandidate(0.5, 0.28125, UV(0, 0), UV(0.3125, 1), UV(0.65625, 0), UV(1, 1), None, UV(0.3125, 0), UV(0.65625, 1), 0.5),
            "prop_tree_jacada_01": LodCandidate(0.484375, 0.421875, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_jacada_02": LodCandidate(0.515625, 0.34375, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_oak_01": LodCandidate(0.46875, 0.453125, UV(0, 0), UV(1, 0.328125), UV(0, 0.65625), UV(1, 1), None, UV(0, 0.328125), UV(1, 0.65625), 0.5078125),
            "prop_tree_olive_01": LodCandidate(0.5, 0.375, UV(0, 0), UV(1, 0.5), UV(0, 1), UV(1, 0.5)),
            "prop_tree_pine_01": LodCandidate(0.515625, 0.50625, UV(0, 0), UV(1, 0.625), UV(0, 1), UV(1, 0.625), UV(0.515625, 0.79296875)),
            "prop_tree_pine_02": LodCandidate(0.546875, 0.63125, UV(0, 0), UV(1, 0.625), UV(0, 1), UV(1, 0.625), UV(0.5, 0.80078125)),
            "prop_tree_fallen_pine_01": LodCandidate(0.609375, None, UV(0, 0), UV(1, 1)),
            "prop_s_pine_dead_01": LodCandidate(0.40625, 0.4875, UV(0, 0), UV(1, 0.625), UV(0, 1), UV(1, 0.625), UV(0.53125, 0.8515625)),
            "prop_w_r_cedar_01": LodCandidate(0.515625, 0.67708333333, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_w_r_cedar_dead": LodCandidate(0.59375, 0.425, UV(0, 0), UV(1, 0.625), UV(0, 1), UV(1, 0.625), UV(0.53125, 0.78125)),
            "test_tree_cedar_trunk_001": LodCandidate(0.5234375, 0.54807692307, UV(0, 0), UV(1, 0.8125), UV(0, 1), UV(1, 0.8125)),
            "test_tree_forest_trunk_01": LodCandidate(0.515625, 0.54807692307, UV(0, 0), UV(1, 0.8125), UV(0, 0.8125), UV(1, 1)),
            "test_tree_forest_trunk_04": LodCandidate(0.453125),
            # trees2
            "prop_tree_lficus_02": LodCandidate(0.4453125, 0.359375, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_lficus_03": LodCandidate(0.46875, 0.21875, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_lficus_05": LodCandidate(0.46875, 0.203125, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_lficus_06": LodCandidate(0.453125, 0.25, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_tree_mquite_01": LodCandidate(0.46875),
            "prop_desert_iron_01": LodCandidate(0.46875),
            "prop_rio_del_01": LodCandidate(0.53125),
            "prop_rus_olive": LodCandidate(0.484375, 0.546875, UV(0, 0), UV(1, 0.5), UV(0, 0.5), UV(1, 1)),
            "prop_rus_olive_wint": LodCandidate(0.556),
            # bushes
            "prop_bush_lrg_02": LodCandidate(0.46875),
            "prop_bush_lrg_02b": LodCandidate(0.5),
            "prop_bush_lrg_04b": LodCandidate(0.375, 0.46875, UV(0, 0), UV(0.59375, 0.5), UV(0, 0.5), UV(1, 1), None, UV(0.59375, 0), UV(1, 0.5), 0.4423076923),
            "prop_bush_lrg_04c": LodCandidate(0.38157894736, 0.453125, UV(0, 0), UV(0.59375, 0.5), UV(0, 0.5), UV(1, 1), None, UV(0.59375, 0), UV(1, 0.5), 0.4423076923),
            "prop_bush_lrg_04d": LodCandidate(0.38970588235, 0.484375, UV(0, 0), UV(0.53125, 0.5), UV(0, 0.5), UV(0.75, 1), None, UV(0.53125, 0), UV(1, 0.5), 0.5),
            # palms
            "prop_palm_fan_02_b": LodCandidate(0.515625, 0.13125, UV(0, 0), UV(1, 0.625), UV(0, 1), UV(1, 0.625), UV(0.484375, 0.8125)),
            "prop_palm_fan_03_c": LodCandidate(0.5, 0.08854166666, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_palm_fan_03_d": LodCandidate(0.484375, 0.08173076923, UV(0, 0), UV(1, 0.8125), UV(0, 0.8125), UV(1, 1), UV(0.484375, 0.90625)),
            "prop_palm_fan_04_b": LodCandidate(0.484375, 0.20625, UV(0, 0), UV(1, 0.625), UV(0, 0.625), UV(1, 1), UV(0.46875, 0.80859375)),
            "prop_palm_fan_04_c": LodCandidate(0.5, 0.140625, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_palm_fan_04_d": LodCandidate(0.421875, 0.12019230769, UV(0, 0), UV(1, 0.8125), UV(0, 0.8125), UV(1, 1)),
            "prop_palm_huge_01a": LodCandidate(0.484375, 0.05092592592, UV(0, 0), UV(1, 0.84375), UV(0, 1), UV(1, 0.84375), UV(0.484375, 0.921875), UV(0, 0), UV(1, 70/512), None, (432-70)/432),
            "prop_palm_huge_01b": LodCandidate(0.4765625, 0.04166666666, UV(0, 0), UV(1, 0.84375), UV(0, 1), UV(1, 0.84375), UV(0.484375, 0.921875), UV(0, 0), UV(1, 67/512), None, (432-67)/432),
            "prop_palm_med_01b": LodCandidate(0.515625, 0.17613636363, UV(0, 0), UV(1, 0.6875), UV(0, 1), UV(1, 0.6875), UV(0.546875, 0.84375)),
            "prop_palm_med_01c": LodCandidate(0.515625, 0.16666666666, UV(0, 0), UV(1, 0.75), UV(0, 0.75), UV(1, 1)),
            "prop_palm_med_01d": LodCandidate(0.5, 0.11057692307, UV(0, 0), UV(1, 0.8125), UV(0, 0.8125), UV(1, 1)),
            "prop_palm_sm_01a": LodCandidate(0.46875),
            "prop_palm_sm_01d": LodCandidate(0.515625),
            "prop_palm_sm_01e": LodCandidate(0.515625),
            "prop_palm_sm_01f": LodCandidate(0.515625),
            "prop_veg_crop_tr_01": LodCandidate(0.484375),
            "prop_veg_crop_tr_02": LodCandidate(0.5)
        }
        # for each lodCandidate set its diffuseSampler as lod_<archetype name>
        for name in lodCandidates:
            lodCandidates[name].diffuseSampler = "lod_" + name.lower()

        # add other Props that should use the same UV mapping
        lodCandidates["prop_palm_med_01a"] = lodCandidates["prop_palm_med_01b"]
        lodCandidates["prop_fan_palm_01a"] = lodCandidates["prop_palm_fan_02_a"] = lodCandidates["prop_palm_fan_02_b"]
        lodCandidates["prop_palm_fan_04_a"] = lodCandidates["prop_palm_fan_04_b"]
        lodCandidates["prop_palm_fan_03_a"] = lodCandidates["prop_palm_fan_03_b"] = lodCandidates["prop_palm_fan_03_c_graff"] = \
            lodCandidates["prop_palm_fan_03_c"]
        lodCandidates["prop_palm_fan_03_d_graff"] = lodCandidates["prop_palm_fan_03_d"]

        # Cache BUILT-IN keys (before we inject any user-defined candidates or
        # custom override placeholders). This is used to avoid suppressing helper
        # mesh export when we later add dynamic custom candidates.
        self._builtinLodKeysLower = {k.lower() for k in lodCandidates.keys()}

        # Merge in user-defined candidates / OBJ overrides
        self._merge_custom_lod_candidates_into(lodCandidates)
        self._merge_custom_override_placeholders_into(lodCandidates)

        self.lodCandidates = lodCandidates

    def prepareSlodCandidates(self):
        # ensure that lodCandidates are prepared
        if self.lodCandidates is None:
            self.prepareLodCandidates()

        slodCandidates = {
            # trees2
            "prop_tree_lficus_02": UVMap("trees2", UV(0 / 2, 0 / 4), UV(1 / 2, 1 / 4), UV(0 / 2, 0 / 4), UV(1 / 2, 1 / 4)),
            "prop_tree_lficus_03": UVMap("trees2", UV(1 / 2, 0 / 4), UV(2 / 2, 1 / 4), UV(1 / 2, 0 / 4), UV(2 / 2, 1 / 4)),
            "prop_tree_lficus_05": UVMap("trees2", UV(0 / 2, 1 / 4), UV(1 / 2, 2 / 4), UV(0 / 2, 1 / 4), UV(1 / 2, 2 / 4)),
            "prop_tree_lficus_06": UVMap("trees2", UV(1 / 2, 1 / 4), UV(2 / 2, 2 / 4), UV(1 / 2, 1 / 4), UV(2 / 2, 2 / 4)),
            "prop_tree_mquite_01": UVMap("trees2", UV(0 / 2, 2 / 4), UV(1 / 2, 3 / 4)),
            "prop_rio_del_01": UVMap("trees2", UV(1 / 2, 2 / 4), UV(2 / 2, 3 / 4)),
            "prop_rus_olive": UVMap("trees2", UV(0 / 2, 3 / 4), UV(1 / 2, 4 / 4), UV(0 / 2, 2 / 4), UV(1 / 2, 3 / 4)),
            "prop_rus_olive_wint": UVMap("trees2", UV(1 / 2, 3 / 4), UV(2 / 2, 4 / 4)),
            # trees
            "prop_s_pine_dead_01": UVMap("trees", UV(10 / 16, 6 / 16), UV(12 / 16, 11 / 16), UV(0 / 16, 12 / 16), UV(3 / 16, 14 / 16)),
            "prop_tree_birch_01": UVMap("trees", UV(8 / 16, 6 / 16), UV(10 / 16, 10 / 16), UV(0 / 16, 3 / 16), UV(3 / 16, 6 / 16)),
            "prop_tree_birch_02": UVMap("trees", UV(6 / 16, 3 / 16), UV(9 / 16, 6 / 16), UV(3 / 16, 3 / 16), UV(6 / 16, 6 / 16)),
            "prop_tree_birch_03": UVMap("trees", UV(4 / 16, 3 / 16), UV(6 / 16, 6 / 16)),
            "prop_tree_birch_03b": UVMap("trees", UV(2 / 16, 3 / 16), UV(4 / 16, 5 / 16)),
            "prop_tree_birch_04": UVMap("trees", UV(9 / 16, 3 / 16), UV(12 / 16, 6 / 16), UV(6 / 16, 3 / 16), UV(9 / 16, 6 / 16)),
            "prop_tree_cedar_02": UVMap("trees", UV(4 / 16, 11 / 16), UV(6 / 16, 16 / 16), UV(6 / 16, 6 / 16), UV(9 / 16, 9 / 16)),
            "prop_tree_cedar_03": UVMap("trees", UV(6 / 16, 10 / 16), UV(8 / 16, 16 / 16), UV(9 / 16, 6 / 16), UV(12 / 16, 9 / 16)),
            "prop_tree_cedar_04": UVMap("trees", UV(8 / 16, 10 / 16), UV(10 / 16, 16 / 16), UV(12 / 16, 6 / 16), UV(15 / 16, 9 / 16)),
            "prop_tree_cedar_s_01": UVMap("trees", UV(6 / 16, 6 / 16), UV(8 / 16, 10 / 16)),
            "prop_tree_cedar_s_04": UVMap("trees", UV(3 / 16, 8 / 16), UV(4 / 16, 12 / 16), UV(3 / 16, 6 / 16), UV(3 / 16, 6 / 16)),
            "prop_tree_cedar_s_05": UVMap("trees", UV(3 / 16, 8 / 16), UV(4 / 16, 12 / 16)),
            "prop_tree_cedar_s_06": UVMap("trees", UV(6 / 16, 0 / 16), UV(7 / 16, 3 / 16)),
            "prop_tree_cypress_01": UVMap("trees", UV(4 / 16, 6 / 16), UV(6 / 16, 11 / 16)),
            "prop_tree_eng_oak_01": UVMap("trees", UV(10 / 16, 0 / 16), UV(13 / 16, 3 / 16), UV(9 / 16, 0 / 16), UV(12 / 16, 3 / 16)),
            "prop_tree_eucalip_01": UVMap("trees", UV(0 / 16, 8 / 16), UV(3 / 16, 12 / 16), UV(9 / 16, 3 / 16), UV(12 / 16, 6 / 16)),
            "prop_tree_fallen_pine_01": UVMap("trees", UV(12 / 16, 3 / 16), UV(14 / 16, 8 / 16)),
            "prop_tree_jacada_01": UVMap("trees", UV(0 / 16, 0 / 16), UV(3 / 16, 3 / 16), UV(0 / 16, 0 / 16), UV(3 / 16, 3 / 16)),
            "prop_tree_jacada_02": UVMap("trees", UV(3 / 16, 0 / 16), UV(6 / 16, 3 / 16), UV(3 / 16, 0 / 16), UV(6 / 16, 3 / 16)),
            "prop_tree_maple_02": UVMap("trees", UV(0 / 16, 3 / 16), UV(2 / 16, 5 / 16)),
            "prop_tree_maple_03": UVMap("trees", UV(0 / 16, 5 / 16), UV(2 / 16, 8 / 16)),
            "prop_tree_oak_01": UVMap("trees", UV(13 / 16, 0 / 16), UV(16 / 16, 3 / 16), UV(12 / 16, 0 / 16), UV(15 / 16, 3 / 16)),
            "prop_tree_olive_01": UVMap("trees", UV(7 / 16, 0 / 16), UV(10 / 16, 3 / 16), UV(6 / 16, 0 / 16), UV(9 / 16, 3 / 16)),
            "prop_tree_pine_01": UVMap("trees", UV(0 / 16, 12 / 16), UV(2 / 16, 16 / 16), UV(0 / 16, 9 / 16), UV(3 / 16, 12 / 16)),
            "prop_tree_pine_02": UVMap("trees", UV(2 / 16, 12 / 16), UV(4 / 16, 16 / 16), UV(3 / 16, 9 / 16), UV(6 / 16, 12 / 16)),
            "prop_w_r_cedar_01": UVMap("trees", UV(10 / 16, 11 / 16), UV(12 / 16, 16 / 16), UV(6 / 16, 9 / 16), UV(9 / 16, 12 / 16)),
            "prop_w_r_cedar_dead": UVMap("trees", UV(14 / 16, 3 / 16), UV(16 / 16, 8 / 16), UV(3 / 16, 12 / 16), UV(6 / 16, 15 / 16)),
            "test_tree_cedar_trunk_001": UVMap("trees", UV(12 / 16, 8 / 16), UV(14 / 16, 16 / 16), UV(9 / 16, 9 / 16), UV(12 / 16, 12 / 16)),
            "test_tree_forest_trunk_01": UVMap("trees", UV(14 / 16, 8 / 16), UV(16 / 16, 16 / 16), UV(12 / 16, 9 / 16), UV(15 / 16, 12 / 16)),
            "test_tree_forest_trunk_04": UVMap("trees", UV(2 / 16, 5 / 16), UV(4 / 16, 8 / 16)),
            # bushes
            "prop_bush_lrg_04b": UVMap("bushes", UV(0.5, 0), UV(1, 0.5), UV(0.5, 0), UV(1, 0.5)),
            "prop_bush_lrg_04c": UVMap("bushes", UV(0, 0.5), UV(0.5, 1), UV(0, 0.5), UV(0.5, 1)),
            "prop_bush_lrg_04d": UVMap("bushes", UV(0.5, 0.5), UV(1, 1), UV(0.5, 0.5), UV(1, 1)),
            # palms
            "prop_palm_sm_01e": UVMap("palms", UV(0 / 4, 0 / 4), UV(1 / 4, 2 / 4)),
            "prop_palm_fan_02_b": UVMap("palms", UV(0 / 4, 2 / 4), UV(1 / 4, 4 / 4), UV(0, 0), UV(0.5, 0.5), 0.23692810457),
            "prop_palm_fan_03_c": UVMap("palms", UV(1 / 4, 0 / 4), UV(2 / 4, 4 / 4), UV(0.5, 0), UV(1, 0.5), 0.14356435643),
            "prop_palm_fan_03_d": UVMap("palms", UV(2 / 4, 0 / 4), UV(3 / 4, 4 / 4), UV(0, 0.5), UV(0.5, 1), 0.13046937151),
            "prop_palm_huge_01a": UVMap("palms", UV(3 / 4, 0 / 4), UV(4 / 4, 4 / 4), UV(0.5, 0.5), UV(1, 1), 0.09644268774)
        }
        # for each missing topZ in slodCandidates get planeZ from lodCandidates and set that value
        for name in slodCandidates:
            if slodCandidates[name].topZ is None:
                slodCandidates[name].topZ = self.lodCandidates[name].planeZ

        # add other Props that should use the same UV mapping
        slodCandidates["prop_palm_sm_01d"] = slodCandidates["prop_palm_sm_01f"] = slodCandidates["prop_palm_med_01a"] = slodCandidates["prop_palm_med_01b"] = \
            slodCandidates["prop_palm_med_01c"] = slodCandidates["prop_palm_sm_01e"]
        slodCandidates["prop_fan_palm_01a"] = slodCandidates["prop_palm_fan_02_a"] = slodCandidates["prop_palm_sm_01a"] = \
            slodCandidates["prop_palm_fan_04_a"] = slodCandidates["prop_palm_fan_04_b"] = slodCandidates["prop_palm_fan_02_b"]
        slodCandidates["prop_palm_fan_03_a"] = slodCandidates["prop_palm_fan_03_b"] = slodCandidates["prop_palm_fan_03_c_graff"] = \
            slodCandidates["prop_palm_fan_04_c"] = slodCandidates["prop_palm_fan_03_c"]
        slodCandidates["prop_palm_med_01d"] = slodCandidates["prop_palm_fan_03_d_graff"] = slodCandidates["prop_palm_fan_04_d"] = \
            slodCandidates["prop_palm_fan_03_d"]
        slodCandidates["prop_palm_huge_01b"] = \
            slodCandidates["prop_palm_huge_01a"]

        # Cache BUILT-IN keys (before we inject any user-defined or auto-generated
        # custom candidates). This is used to avoid suppressing helper mesh export
        # when we later add dynamic custom candidates.
        self._builtinSlodKeysLower = {k.lower() for k in slodCandidates.keys()}

        # Merge in user-defined and auto-generated custom candidates.
        self._merge_custom_slod_candidates_into(slodCandidates)

        # Ensure topZ is always defined when a top card is enabled.
        for name, uvmap in slodCandidates.items():
            if uvmap.topMin is None or uvmap.topMax is None:
                continue
            if uvmap.topZ is not None:
                continue
            cand = self.lodCandidates.get(name)
            uvmap.topZ = float(cand.planeZ) if (cand is not None and cand.planeZ is not None) else 0.25

        self.slodCandidates = slodCandidates

    VERTEX_DECLARATION_TREE_LOD = "N209731BE"
    VERTEX_DECLARATION_TREE_LOD2 = "N5A9A1E1A"

    REFL_LOD_DISTANCE = 1000
    LOD_DISTANCE = 500
    SLOD_DISTANCE = 1000
    SLOD2_DISTANCE = 2500
    SLOD3_DISTANCE = 5000
    SLOD4_DISTANCE = 15000

    NUM_CHILDREN_MAX_VALUE = 255  # TODO confirm following claim: must be <= 255 since numChildren is of size 1 byte
    LOD_DISTANCES_MAX_DIFFERENCE_LOD = 80
    ENTITIES_EXTENTS_MAX_DIAGONAL_LOD = 100
    ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD1 = 300  # also used for reflection models
    ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD2 = 900
    ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD3 = 1800
    ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD4 = 3600

    USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE = 2
    USE_NO_TOP_TEMPLATE_FOR_LEVEL_AND_ABOVE = 3

    unitBox = Box.createUnitBox()
    unitSphere = Sphere.createUnitSphere()

    # only entities with a lodDistance (according to hd entity) greater or equal this value are considered for SLOD1 to 4 model
    MIN_HD_LOD_DISTANCE_FOR_SLOD1 = Util.calculateLodDistance(unitBox, unitSphere, [5] * 3, True)
    MIN_HD_LOD_DISTANCE_FOR_SLOD2 = Util.calculateLodDistance(unitBox, unitSphere, [10] * 3, True)
    MIN_HD_LOD_DISTANCE_FOR_SLOD3 = Util.calculateLodDistance(unitBox, unitSphere, [15] * 3, True)
    MIN_HD_LOD_DISTANCE_FOR_SLOD4 = Util.calculateLodDistance(unitBox, unitSphere, [20] * 3, True)

    def __init__(self, inputDir: str, outputDir: str, prefix: str,
                 clearLod: bool, createReflection: bool, lodMultipliers=None, lodDistanceOverrides=None):
        self.inputDir = inputDir
        self.outputDir = outputDir
        self.prefix = prefix
        self.clearLod = clearLod
        self.createReflection = createReflection
        # Optional per-category absolute LOD distance overrides coming from the CLI / UI.
        # Expected keys: 'cacti', 'trees', 'bushes', 'palms'.
        # Values are absolute lodDist values; a value <= 0 is ignored.
        self.lodDistanceOverrides = lodDistanceOverrides or {}

        # Legacy (deprecated): optional per-category LOD multipliers.
        # Kept for backwards compatibility; GUI now prefers absolute overrides.
        self.lodMultipliers = lodMultipliers or {}

        # YTYP item dictionaries used when writing LOD / reflection models.
        # These MUST exist before we start creating reflection LOD maps.
        self.slodYtypItems = {}
        self.reflYtypItems = {}

        self.foundLod = False
        self.foundSlod = False


        # Custom meshes chosen via the GUI (Custom Meshes tab). These are
        # archetype names for which the tools may want to generate or export
        # additional helper meshes. We load them once from custom_meshes.json.
        self.customMeshes = self._load_custom_meshes()

        # Custom OBJ overrides (optional). The GUI writes custom_mesh_overrides.json.
        # When present, LOD generation will use the imported OBJ geometry instead of
        # procedurally generated billboard planes for that archetype.
        self.customMeshOverrides = self._load_custom_mesh_overrides()
        self.customLodCandidates = self._load_custom_lod_candidates()
        self.customSlodCandidates = self._load_custom_slod_candidates()
        self._overrideMeshCache: dict[str, 'ObjMeshData'] = {}

        # Built-in candidate caches (populated during prepareLodCandidates/prepareSlodCandidates)
        self._builtinLodKeysLower = set()
        self._builtinSlodKeysLower = set()
        self.customSlods = self._load_custom_slods()

    def _load_custom_slod_candidates(self) -> dict[str, UVMap]:
        """Load optional additional SLOD candidates from slod_custom_candidates.json.

        This file is *optional*. When present, it should map archetype names to a
        minimal UVMap description. This primarily exists to let users opt-in to a
        top card ("_top" sampler) and/or tweak topZ for a given archetype.

        Expected format (values are in normalized 0..1 UV space):
            {
              "prop_example": {
                "diffuseSamplerSuffix": "prop_example",   // optional; defaults to key
                "frontMin": [0, 0],
                "frontMax": [1, 1],
                "topMin": [0, 0],                          // optional
                "topMax": [1, 1],                          // optional
                "topZ": 0.25                               // optional
              }
            }
        """
        try:
            root_dir = self._resolve_tool_root()
            json_path = os.path.join(root_dir, "slod_custom_candidates.json")
            if not os.path.exists(json_path):
                return {}

            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)

            if not isinstance(data, dict):
                print(f"warning: slod_custom_candidates.json must be an object; got {type(data)}")
                return {}

            def _uv(v: Any) -> UV:
                if isinstance(v, (list, tuple)) and len(v) == 2:
                    return UV(float(v[0]), float(v[1]))
                raise ValueError(f"invalid UV value: {v}")

            result: dict[str, UVMap] = {}
            for raw_name, cfg in data.items():
                if not isinstance(raw_name, str) or not isinstance(cfg, dict):
                    continue

                name = raw_name.strip().lower()
                if not name:
                    continue

                suffix = str(cfg.get("diffuseSamplerSuffix", name)).strip()
                if not suffix:
                    suffix = name

                frontMin = _uv(cfg.get("frontMin", [0.0, 0.0]))
                frontMax = _uv(cfg.get("frontMax", [1.0, 1.0]))

                topMin = cfg.get("topMin", None)
                topMax = cfg.get("topMax", None)
                if topMin is not None and topMax is not None:
                    topMin = _uv(topMin)
                    topMax = _uv(topMax)
                else:
                    topMin = None
                    topMax = None

                topZ = cfg.get("topZ", None)
                topZ = None if topZ is None else float(topZ)

                result[name] = UVMap(suffix, frontMin, frontMax, topMin, topMax, topZ)

            print(f"loaded {len(result)} custom SLOD candidate(s) from {os.path.basename(json_path)}")
            return result
        except Exception as e:
            print(f"warning: failed to load slod_custom_candidates.json: {e}")
            return {}
    
    def _merge_custom_slod_candidates_into(self, slodCandidates: dict[str, UVMap]) -> None:
        """Merge user-defined and auto-generated SLOD candidates into slodCandidates.

        - Explicit mappings from slod_custom_candidates.json are applied first.
        - For any remaining custom meshes (custom_meshes.json) and OBJ overrides,
          we generate a default per-archetype UVMap using the full texture space.
          By default, this is *front-only* (no _top texture required).
        """
        # 1) Explicit user-defined candidates
        custom = getattr(self, 'customSlodCandidates', None) or {}
        for name, uvmap in custom.items():
            if name in slodCandidates:
                print(f"info: overriding existing SLOD candidate for '{name}' via slod_custom_candidates.json")
            slodCandidates[name] = uvmap

        # 2) Auto-generate defaults for custom meshes / overrides
        names: set[str] = set()
        names |= set(getattr(self, 'customMeshes', set()) or set())
        names |= set((getattr(self, 'customMeshOverrides', {}) or {}).keys())
        names |= set(custom.keys())

        for raw in sorted(names):
            name = (raw or "").strip().lower()
            if not name:
                continue
            if name in slodCandidates:
                continue

            # Default: full UV range, front-only.
            # Users can opt in to a top card by adding an entry to slod_custom_candidates.json.
            topZ = None
            cand = (getattr(self, 'lodCandidates', None) or {}).get(name)
            if cand is not None and cand.planeZ is not None:
                topZ = float(cand.planeZ)
            else:
                topZ = 0.25

            slodCandidates[name] = UVMap(name, UV(0.0, 0.0), UV(1.0, 1.0), None, None, topZ)

    def _load_custom_meshes(self) -> set[str]:
        """Load custom mesh archetype names from custom_meshes.json.

        The GUI writes this file as a JSON array of strings (one per archetype).
        We normalise everything to lowercase for simpler matching. If the file
        is missing or invalid, an empty set is returned.
        """
        try:
            root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
            json_path = os.path.join(root_dir, "custom_meshes.json")
            if not os.path.exists(json_path):
                return set()

            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)

            # Accept either a plain list or a small dict wrapper.
            if isinstance(data, dict):
                items = data.get("meshes", [])
            else:
                items = data

            result: set[str] = set()
            for item in items:
                if isinstance(item, str):
                    name = item.strip()
                    if name:
                        result.add(name.lower())

            print(f"loaded {len(result)} custom mesh name(s) from {os.path.basename(json_path)}")
            return result
        except Exception as e:
            print(f"warning: failed to load custom_meshes.json: {e}")
            return set()


    def _load_custom_slods(self) -> set[str]:
        """Load custom slod archetype names from custom_slods.json."""
        try:
            root_dir = self._resolve_tool_root()
            json_path = os.path.join(root_dir, "custom_slods.json")
            if not os.path.exists(json_path):
                return set()

            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)

            if isinstance(data, list):
                result = set()
                for item in data:
                    if isinstance(item, str):
                        name = item.strip()
                        if name:
                            result.add(name.lower())
                print(f"loaded {len(result)} custom slod name(s) from {os.path.basename(json_path)}")
                return result
            return set()
        except Exception as e:
            print(f"warning: failed to load custom_slods.json: {e}")
            return set()

    def _resolve_tool_root(self) -> str:
        # Project root (two levels up from worker/lod_map_creator)
        return os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


    def _load_custom_mesh_overrides(self) -> dict[str, dict]:
        """Load custom OBJ override mappings from custom_mesh_overrides.json.

        Expected formats:
          1) Dict format (preferred):
             {
               "prop_example": {"obj": "custom_mesh_overrides/prop_example.obj", "diffuseSampler": "lod_prop_example"}
             }
          2) List format (legacy):
             [{"name": "prop_example", "obj": "..."}, ...]

        Returns a dict keyed by lowercase archetype name.
        """
        try:
            root_dir = self._resolve_tool_root()
            json_path = os.path.join(root_dir, "custom_mesh_overrides.json")
            if not os.path.exists(json_path):
                return {}

            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)

            result: dict[str, dict] = {}
            if isinstance(data, dict):
                items = data.items()
                for k, v in items:
                    if not isinstance(k, str) or not isinstance(v, dict):
                        continue
                    name = k.strip().lower()
                    obj_rel = v.get("obj")
                    if not isinstance(obj_rel, str) or not obj_rel.strip():
                        continue
                    entry = dict(v)
                    entry["obj"] = obj_rel.strip()
                    # Default sampler if not provided
                    if not isinstance(entry.get("diffuseSampler"), str) or not entry.get("diffuseSampler").strip():
                        entry["diffuseSampler"] = f"lod_{name}"
                    result[name] = entry
            elif isinstance(data, list):
                for item in data:
                    if not isinstance(item, dict):
                        continue
                    k = item.get("name")
                    obj_rel = item.get("obj")
                    if not isinstance(k, str) or not isinstance(obj_rel, str):
                        continue
                    name = k.strip().lower()
                    if not name or not obj_rel.strip():
                        continue
                    entry = dict(item)
                    entry["obj"] = obj_rel.strip()
                    if not isinstance(entry.get("diffuseSampler"), str) or not entry.get("diffuseSampler").strip():
                        entry["diffuseSampler"] = f"lod_{name}"
                    result[name] = entry

            print(f"loaded {len(result)} custom mesh OBJ override(s) from {os.path.basename(json_path)}")
            return result
        except Exception as e:
            print(f"warning: failed to load custom_mesh_overrides.json: {e}")
            return {}


    def _load_custom_lod_candidates(self) -> dict[str, LodCandidate]:
        """Load optional additional LOD candidates from lod_custom_candidates.json.

        This is the non-OBJ workflow: you define UV rectangles and other LodCandidate
        parameters in JSON and the generator will still build billboard planes.

        This file is optional and can be authored manually if you want billboard-plane generation without an OBJ override.
        """
        try:
            root_dir = self._resolve_tool_root()
            json_path = os.path.join(root_dir, "lod_custom_candidates.json")
            if not os.path.exists(json_path):
                return {}

            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)

            if not isinstance(data, dict):
                return {}

            def _uv(obj, default):
                if not isinstance(obj, dict):
                    return default
                u = obj.get("u")
                v = obj.get("v")
                if isinstance(u, (int, float)) and isinstance(v, (int, float)):
                    return UV(float(u), float(v))
                return default

            result: dict[str, LodCandidate] = {}
            for name, cfg in data.items():
                if not isinstance(name, str) or not isinstance(cfg, dict):
                    continue
                key = name.strip().lower()
                if not key:
                    continue

                textureOrigin = float(cfg.get("textureOrigin", 0.5))
                planeZ = cfg.get("planeZ", 0.5)
                planeZ = None if planeZ is None else float(planeZ)

                uvFrontMin = _uv(cfg.get("uvFrontMin"), UV(0, 0))
                uvFrontMax = _uv(cfg.get("uvFrontMax"), UV(1, 1))

                uvTopMin = cfg.get("uvTopMin")
                uvTopMax = cfg.get("uvTopMax")
                uvTopMin = None if uvTopMin is None else _uv(uvTopMin, UV(0, 0))
                uvTopMax = None if uvTopMax is None else _uv(uvTopMax, UV(1, 1))

                uvSideMin = cfg.get("uvSideMin")
                uvSideMax = cfg.get("uvSideMax")
                uvSideMin = None if uvSideMin is None else _uv(uvSideMin, UV(0, 0))
                uvSideMax = None if uvSideMax is None else _uv(uvSideMax, UV(1, 1))

                textureOriginSide = cfg.get("textureOriginSide")
                textureOriginSide = None if textureOriginSide is None else float(textureOriginSide)

                sideOffsetZ = float(cfg.get("sideOffsetZ", 0.0))

                cand = LodCandidate(textureOrigin, planeZ, uvFrontMin, uvFrontMax,
                                   uvTopMin, uvTopMax, None,
                                   uvSideMin, uvSideMax, textureOriginSide, sideOffsetZ)
                diffuseSampler = cfg.get("diffuseSampler")
                if isinstance(diffuseSampler, str) and diffuseSampler.strip():
                    cand.diffuseSampler = diffuseSampler.strip()
                else:
                    cand.diffuseSampler = f"lod_{key}"

                result[key] = cand

            if result:
                print(f"loaded {len(result)} custom LOD candidate(s) from {os.path.basename(json_path)}")
            return result
        except Exception as e:
            print(f"warning: failed to load lod_custom_candidates.json: {e}")
            return {}


    def _merge_custom_lod_candidates_into(self, lodCandidates: dict[str, LodCandidate]) -> None:
        custom = getattr(self, 'customLodCandidates', None) or {}
        for name, cand in custom.items():
            # Do not clobber built-ins unless user explicitly wants it
            if name in lodCandidates:
                continue
            lodCandidates[name] = cand


    def _merge_custom_override_placeholders_into(self, lodCandidates: dict[str, LodCandidate]) -> None:
        overrides = getattr(self, 'customMeshOverrides', None) or {}
        for name, entry in overrides.items():
            if name in lodCandidates:
                # Ensure sampler is updated if user specified
                sampler = entry.get('diffuseSampler')
                if isinstance(sampler, str) and sampler.strip():
                    lodCandidates[name].diffuseSampler = sampler.strip()
                continue

            cand = LodCandidate()
            sampler = entry.get('diffuseSampler')
            cand.diffuseSampler = sampler.strip() if isinstance(sampler, str) and sampler.strip() else f"lod_{name}"
            lodCandidates[name] = cand


    @dataclass
    class ObjMeshData:
        vertices: list[list[float]]
        normals: list[list[float]]
        uvs: list[list[float]]
        indices: list[int]


    def _parse_obj_mesh(self, obj_path: str) -> 'ObjMeshData':
        """Parse a Wavefront OBJ file into a simple indexed triangle mesh.

        - Supports v / vt / vn and f with triangles or polygons (fan triangulation).
        - If normals are missing, per-face normals are generated.
        - V coordinate is flipped (Blender-friendly) to match the tool's UV convention.
        """
        positions: list[list[float]] = []
        texcoords: list[list[float]] = []
        normals: list[list[float]] = []

        out_vertices: list[list[float]] = []
        out_uvs: list[list[float]] = []
        out_normals: list[list[float]] = []
        out_indices: list[int] = []

        # Cache only when vn exists; when normals are generated per-face we avoid sharing.
        unique: dict[tuple[int, int, int], int] = {}

        def _fix_index(i: int, n: int) -> int:
            # OBJ is 1-based; negative indices are relative to end
            if i > 0:
                return i - 1
            return n + i

        def _get_uv(vt_i: int | None) -> list[float]:
            if vt_i is None:
                return [0.0, 0.0]
            u, v = texcoords[vt_i]
            return [u, 1.0 - v]

        def _safe_normal(nv):
            try:
                return Util.normalize(nv)
            except Exception:
                return [0.0, 0.0, 1.0]

        with open(obj_path, 'r', encoding='utf-8', errors='ignore') as f:
            for raw in f:
                line = raw.strip()
                if not line or line.startswith('#'):
                    continue
                if line.startswith('v '):
                    parts = line.split()
                    if len(parts) >= 4:
                        positions.append([float(parts[1]), float(parts[2]), float(parts[3])])
                elif line.startswith('vt '):
                    parts = line.split()
                    if len(parts) >= 3:
                        texcoords.append([float(parts[1]), float(parts[2])])
                elif line.startswith('vn '):
                    parts = line.split()
                    if len(parts) >= 4:
                        normals.append([float(parts[1]), float(parts[2]), float(parts[3])])
                elif line.startswith('f '):
                    parts = line.split()[1:]
                    if len(parts) < 3:
                        continue

                    face = []
                    for p in parts:
                        v_i = vt_i = vn_i = None
                        comps = p.split('/')
                        if len(comps) >= 1 and comps[0]:
                            v_i = _fix_index(int(comps[0]), len(positions))
                        if len(comps) >= 2 and comps[1]:
                            vt_i = _fix_index(int(comps[1]), len(texcoords))
                        if len(comps) >= 3 and comps[2]:
                            vn_i = _fix_index(int(comps[2]), len(normals))
                        if v_i is None or v_i < 0 or v_i >= len(positions):
                            continue
                        face.append((v_i, vt_i, vn_i))

                    if len(face) < 3:
                        continue

                    # Triangulate polygon (fan)
                    tri_indices = [(0, i, i + 1) for i in range(1, len(face) - 1)]
                    for a, b, c in tri_indices:
                        v0i, vt0i, vn0i = face[a]
                        v1i, vt1i, vn1i = face[b]
                        v2i, vt2i, vn2i = face[c]

                        p0 = positions[v0i]
                        p1 = positions[v1i]
                        p2 = positions[v2i]

                        # Face normal if needed
                        fn = None
                        if vn0i is None or vn1i is None or vn2i is None or len(normals) == 0:
                            e1 = np.subtract(p1, p0)
                            e2 = np.subtract(p2, p0)
                            fn = np.cross(e1, e2).tolist()
                            fn = _safe_normal(fn)

                        def _emit(v_i, vt_i, vn_i):
                            if vn_i is not None and len(normals) > 0:
                                key = (v_i, -1 if vt_i is None else vt_i, vn_i)
                                if key in unique:
                                    return unique[key]
                                idx = len(out_vertices)
                                unique[key] = idx
                                out_vertices.append(positions[v_i])
                                out_uvs.append(_get_uv(vt_i))
                                out_normals.append(_safe_normal(normals[vn_i]))
                                return idx
                            else:
                                # Per-face normal => do not share
                                idx = len(out_vertices)
                                out_vertices.append(positions[v_i])
                                out_uvs.append(_get_uv(vt_i))
                                out_normals.append(fn if fn is not None else [0.0, 0.0, 1.0])
                                return idx

                        i0 = _emit(v0i, vt0i, vn0i)
                        i1 = _emit(v1i, vt1i, vn1i)
                        i2 = _emit(v2i, vt2i, vn2i)
                        out_indices.extend([i0, i1, i2])

        if not out_vertices or not out_indices:
            raise ValueError(f"OBJ has no usable geometry: {obj_path}")

        return LodMapCreator.ObjMeshData(out_vertices, out_normals, out_uvs, out_indices)


    def _get_obj_mesh_cached(self, archetype_lower: str, obj_path: str) -> 'ObjMeshData':
        cache = getattr(self, '_overrideMeshCache', None)
        if cache is None:
            self._overrideMeshCache = {}
            cache = self._overrideMeshCache

        key = archetype_lower
        if key in cache:
            return cache[key]

        mesh = self._parse_obj_mesh(obj_path)
        cache[key] = mesh
        return mesh


    def convert_obj_to_openformats(self,
                                  obj_path: str,
                                  out_dir: str,
                                  base_name: str | None = None,
                                  diffuse_sampler: str | None = None,
                                  overwrite: bool = True) -> tuple[str, str]:
        """Convert a Wavefront OBJ to OpenFormats (.mesh + .odr).

        This is a lightweight helper intended for the GUI workflow:
        - User UV-maps an OBJ
        - Tool converts it back to OpenFormats so it can be imported in OpenIV / used in LOD pipelines

        The output is a single-geometry drawable using the TREE_LOD vertex declaration and
        the TREE_LOD shader template. UV V is flipped on import (handled by _parse_obj_mesh).

        Returns (mesh_path, odr_path).
        """
        if not obj_path or not os.path.exists(obj_path):
            raise ValueError(f"OBJ not found: {obj_path}")

        # Ensure templates are loaded
        if not hasattr(self, 'contentTemplateMesh') or not getattr(self, 'contentTemplateMesh', None):
            self.readTemplates()

        if not base_name:
            base_name = os.path.splitext(os.path.basename(obj_path))[0]

        base = base_name.strip().lower()
        if not base:
            raise ValueError("base_name resolves to empty")

        if not diffuse_sampler or not diffuse_sampler.strip():
            diffuse_sampler = f"lod_{base}"
        diffuse_sampler = diffuse_sampler.strip()

        os.makedirs(out_dir, exist_ok=True)

        mesh_filename = base + '.mesh'
        odr_filename = base + '.odr'
        mesh_out = os.path.join(out_dir, mesh_filename)
        odr_out = os.path.join(out_dir, odr_filename)

        if not overwrite:
            if os.path.exists(mesh_out) or os.path.exists(odr_out):
                raise FileExistsError(f"Output exists (overwrite disabled): {mesh_out} / {odr_out}")

        mesh = self._parse_obj_mesh(obj_path)

        # Bounds and translation (center to origin), matching the LOD generator.
        totalBoundingGeometry = BoundingGeometry()
        totalBoundingGeometry.extendByPoints(mesh.vertices)
        totalBoundingSphere = totalBoundingGeometry.getSphere()
        center = totalBoundingSphere.center
        translation = np.multiply(center, [-1]).tolist()

        totalBoundingBox = totalBoundingGeometry.getBox().getTranslated(translation)
        totalBoundingSphere = totalBoundingSphere.getTranslated(translation)

        vnut_str = LodMapCreator.convertVerticesNormalsTextureUVsAsStr(
            mesh.vertices, mesh.normals, mesh.uvs, translation
        )

        bounds = self.createAabb(totalBoundingBox)

        geometries = (
            self.contentTemplateMeshGeometry
            .replace('${SHADER_INDEX}', '0')
            .replace('${VERTEX_DECLARATION}', self.VERTEX_DECLARATION_TREE_LOD)
            .replace('${INDICES.NUM}', str(len(mesh.indices)))
            .replace('${INDICES}', self.createIndicesStr(mesh.indices))
            .replace('${VERTICES.NUM}', str(len(mesh.vertices)))
            .replace("${VERTICES}\n", vnut_str)
        )

        contentModelMesh = (
            self.contentTemplateMesh
            .replace("${BOUNDS}\n", bounds)
            .replace("${GEOMETRIES}\n", geometries)
        )

        with open(mesh_out, 'w', encoding='utf-8') as fmesh:
            fmesh.write(contentModelMesh)

        shaders = self.contentTemplateOdrShaderTreeLod.replace('${DIFFUSE_SAMPLER}', diffuse_sampler)

        contentModelOdr = (
            self.contentTemplateOdr
            .replace('${BBOX.MIN.X}', Util.floatToStr(totalBoundingBox.min[0]))
            .replace('${BBOX.MIN.Y}', Util.floatToStr(totalBoundingBox.min[1]))
            .replace('${BBOX.MIN.Z}', Util.floatToStr(totalBoundingBox.min[2]))
            .replace('${BBOX.MAX.X}', Util.floatToStr(totalBoundingBox.max[0]))
            .replace('${BBOX.MAX.Y}', Util.floatToStr(totalBoundingBox.max[1]))
            .replace('${BBOX.MAX.Z}', Util.floatToStr(totalBoundingBox.max[2]))
            .replace('${BSPHERE.CENTER.X}', Util.floatToStr(totalBoundingSphere.center[0]))
            .replace('${BSPHERE.CENTER.Y}', Util.floatToStr(totalBoundingSphere.center[1]))
            .replace('${BSPHERE.CENTER.Z}', Util.floatToStr(totalBoundingSphere.center[2]))
            .replace('${BSPHERE.RADIUS}', Util.floatToStr(totalBoundingSphere.radius))
            .replace('${MESH_FILENAME}', mesh_filename)
            .replace("${SHADERS}\n", shaders)
        )

        with open(odr_out, 'w', encoding='utf-8') as fodr:
            fodr.write(contentModelOdr)

        return mesh_out, odr_out

    def _peek_custom_override_sampler_if_present(self, entity: EntityItem) -> str | None:
        """Return the diffuseSampler for a valid custom OBJ override (if present).

        This mirrors the basic validation done by _append_custom_override_mesh_if_present()
        but does not load or parse the OBJ.
        """
        overrides = getattr(self, 'customMeshOverrides', None) or {}
        name = entity.archetypeName.lower()
        entry = overrides.get(name)
        if not entry:
            return None

        obj_rel = entry.get('obj')
        if not isinstance(obj_rel, str) or not obj_rel.strip():
            return None

        root_dir = self._resolve_tool_root()
        obj_path = obj_rel.strip()
        if not os.path.isabs(obj_path):
            obj_path = os.path.join(root_dir, obj_path)

        if not os.path.exists(obj_path):
            return None

        sampler = entry.get('diffuseSampler')
        if not isinstance(sampler, str) or not sampler.strip():
            sampler = f"lod_{name}"
        return sampler.strip()
    def _append_custom_override_mesh_if_present(self, entity: EntityItem,
                                                groupToVertices: dict,
                                                groupToNormals: dict,
                                                groupToTextureUVs: dict,
                                                groupToIndices: dict,
                                                groupKey: str | None = None,
                                                groupKeyToSampler: dict | None = None) -> bool:
        overrides = getattr(self, 'customMeshOverrides', None) or {}
        name = entity.archetypeName.lower()
        entry = overrides.get(name)
        if not entry:
            return False

        obj_rel = entry.get('obj')
        if not isinstance(obj_rel, str) or not obj_rel.strip():
            return False

        root_dir = self._resolve_tool_root()
        obj_path = obj_rel.strip()
        if not os.path.isabs(obj_path):
            obj_path = os.path.join(root_dir, obj_path)

        if not os.path.exists(obj_path):
            print(f"warning: custom OBJ override for '{name}' not found: {obj_path}")
            return False

        sampler = entry.get('diffuseSampler')
        if not isinstance(sampler, str) or not sampler.strip():
            sampler = f"lod_{name}"
        sampler = sampler.strip()

        key = groupKey.strip() if isinstance(groupKey, str) and groupKey.strip() else sampler

        if groupKeyToSampler is not None:
            prev = groupKeyToSampler.get(key)
            if prev is None:
                groupKeyToSampler[key] = sampler
            elif prev != sampler:
                print(f"warning: group '{key}' has multiple samplers ('{prev}' vs '{sampler}'); keeping '{prev}'")
                sampler = prev

        if key not in groupToVertices:
            groupToVertices[key] = []
            groupToNormals[key] = []
            groupToTextureUVs[key] = []
            groupToIndices[key] = []

        mesh = self._get_obj_mesh_cached(name, obj_path)
        base_offset = len(groupToVertices[key])

        for v, n, uv in zip(mesh.vertices, mesh.normals, mesh.uvs):
            groupToVertices[key].append(entity.applyTransformationTo(v))
            groupToNormals[key].append(Util.applyRotation(n, entity.rotation))
            groupToTextureUVs[key].append(uv)

        groupToIndices[key].extend([i + base_offset for i in mesh.indices])
        return True
    def _get_lod_distance_override(self, archetypeName: str):
        """Return an absolute lodDist override for the given archetype name.

        Category matching is heuristic (substring-based) and mirrors the logic
        previously used for multipliers. If no override is configured (or the
        configured value is <= 0), None is returned.
        """
        overrides = getattr(self, 'lodDistanceOverrides', None)
        if not overrides:
            return None

        name = archetypeName.lower()

        def _valid(v):
            try:
                return v is not None and float(v) > 0
            except Exception:
                return False

        # Cacti-like
        if 'cacti' in name or 'cactus' in name or 'yucca' in name:
            v = overrides.get('cacti')
            return float(v) if _valid(v) else None

        # Palms
        if 'palm' in name:
            v = overrides.get('palms')
            return float(v) if _valid(v) else None

        # Bushes / shrubs
        if 'bush' in name or 'shrub' in name:
            v = overrides.get('bushes')
            return float(v) if _valid(v) else None

        # Default bucket: trees / general vegetation
        v = overrides.get('trees')
        return float(v) if _valid(v) else None


    def _get_lod_multiplier(self, archetypeName: str) -> float:
        """
        Return a per-category LOD distance multiplier for the given archetype name.
        Categories are matched heuristically based on substrings in the archetype name.
        If no category matches or no multipliers were provided, 1.0 is returned.
        """
        # Use getattr so we don't crash even if older code paths didn't
        # initialise the attribute for some reason.
        lodMultipliers = getattr(self, "lodMultipliers", None)
        if not lodMultipliers:
            return 1.0

        name = archetypeName.lower()

        # Cacti-like
        if "cacti" in name or "cactus" in name or "yucca" in name:
            return self.lodMultipliers.get("cacti", 1.0)

        # Palms
        if "palm" in name:
            return self.lodMultipliers.get("palms", 1.0)

        # Bushes / shrubs
        if "bush" in name or "shrub" in name:
            return self.lodMultipliers.get("bushes", 1.0)

        # Default bucket: trees / general vegetation
        return self.lodMultipliers.get("trees", 1.0)

    def run(self):
        if self.clearLod:
            print("clearing lod map...")
        else:
            print("running lod map creator...")

        self.determinePrefixBundles()
        self.readTemplates()

        if self.clearLod:
            self.lodCandidates = {}
            self.slodCandidates = {}
        else:
            self.prepareLodCandidates()
            self.prepareSlodCandidates()

        self.createOutputDir()
        self.readYtypItems()
        # Export helper meshes for selected custom archetypes
        self.createCustomMeshesForArchetypes(self.customMeshes)
        self.processFiles()
        self.addLodAndSlodModelsToYtypDict(False)
        if self.createReflection:
            self.addLodAndSlodModelsToYtypDict(True)

        self.fixMapExtents(False)
        self.createManifest(False)
        if self.createReflection:
            self.fixMapExtents(True)
            self.createManifest(True)

        self.copyOthers()
        self.copyTextureDictionaries()

        if self.clearLod:
            print("clearing lod map DONE")
        else:
            print("lod map creator DONE")


    def runCustomMeshesOnly(self):
        """Create helper meshes only, without generating full LOD maps.

        This is used when the GUI 'Custom meshes' step is enabled without the
        full LOD Map step. It reads templates and YTYP data, creates the
        output directories, and writes helper meshes for all archetypes listed
        in custom_meshes.json.
        """
        print("running custom mesh creator (custom meshes only)...")

        # We still need templates and ytyp items so we can build proper
        # mesh/odr files, but we skip LOD/SLOD candidate preparation and
        # map processing.
        self.readTemplates()
        self.createOutputDir()
        self.readYtypItems()
        self.createCustomMeshesForArchetypes(self.customMeshes)

        print("custom mesh creator DONE")

    def getYtypName(self, mapPrefix: str, slodLevel: int, reflection: bool) -> str:
        if reflection:
            return mapPrefix + "_refl"
        elif slodLevel <= 1:
            return mapPrefix + "_lod"
        elif slodLevel <= 3:
            return mapPrefix + "_slod2"
        else:
            return self.prefix + "_slod" + str(slodLevel)

    def createOutputDir(self):
        if os.path.exists(self.outputDir):
            raise ValueError("Output dir " + self.outputDir + " must not exist")

        os.makedirs(self.outputDir)
        os.mkdir(self.getOutputDirMaps(False))
        os.mkdir(self.getOutputDirMeshes(False))
        os.mkdir(self.getOutputDirModels(False))
        os.mkdir(self.getOutputDirMetadata(False))
        os.mkdir(self.getOutputDirCustomMeshes())
        
        # [ADD THIS LINE]
        os.mkdir(self.getOutputDirCustomSlods())
        
        if self.createReflection:
            os.mkdir(self.getOutputDirMaps(True))
            os.mkdir(self.getOutputDirMeshes(True))
            os.mkdir(self.getOutputDirModels(True))
            os.mkdir(self.getOutputDirMetadata(True))

    # [ADD THIS METHOD]
    def getOutputDirCustomSlods(self) -> str:
        """Dedicated folder for SLOD helper meshes."""
        return os.path.join(self.outputDir, "custom_slod_meshes")

    def getOutputDirMaps(self, reflection: bool) -> str:
        directory = ("refl_" if reflection else "") + "maps"
        return os.path.join(self.outputDir, directory)

    def getOutputDirMetadata(self, reflection: bool) -> str:
        directory = ("refl" if reflection else "slod") + "_metadata"
        return os.path.join(self.outputDir, directory)

    def getOutputDirModels(self, reflection: bool) -> str:
        directory = "refl" if reflection else "slod"
        return os.path.join(self.outputDir, directory)

    def getOutputDirMeshes(self, reflection: bool) -> str:
        directory = "_" + ("refl" if reflection else "slod") + "_meshes"
        return os.path.join(self.outputDir, directory)


    def getOutputDirCustomMeshes(self) -> str:
        """Dedicated folder for helper meshes (not part of LOD/SLOD)."""
        return os.path.join(self.outputDir, "custom_meshes")

    def readTemplates(self):
        templatesDir = os.path.join(os.path.dirname(__file__), "templates")

        f = open(os.path.join(templatesDir, "template_lod_ytyp_item.xml"), 'r')
        self.contentTemplateYtypItem = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_slod_entity.ymap.xml"), 'r')
        self.contentTemplateEntitySlod = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_slod2.ymap.xml"), 'r')
        self.contentTemplateSlod2Map = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_slod.mesh"), 'r')
        self.contentTemplateMesh = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_aabb.mesh.part"), 'r')
        self.contentTemplateMeshAabb = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_geometry.mesh.part"), 'r')
        self.contentTemplateMeshGeometry = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_slod.odr"), 'r')
        self.contentTemplateOdr = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_shader_tree_lod.odr.part"), 'r')
        self.contentTemplateOdrShaderTreeLod = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_shader_tree_lod2.odr.part"), 'r')
        self.contentTemplateOdrShaderTreeLod2 = f.read()
        f.close()

        f = open(os.path.join(templatesDir, "template_shader_alpha.odr.part"), 'r')
        self.contentTemplateOdrShaderAlpha = f.read()
        f.close()

    def readYtypItems(self):
        self.ytypItems = YtypParser.readYtypDirectory(os.path.join(os.path.dirname(__file__), "..", "..", "resources", "ytyp"))

    def replaceFlagsAndContentFlags(self, content: str, flags: int, contentFlags: int) -> str:
        # TODO deal with existing flags, e.g. "Scripted (1)"
        return re.sub(
            '(<flags\\s+value=")[^"]*("\\s*/>\\s*<contentFlags\\s+value=")[^"]*("\\s*/>)',
            "\\g<1>" + str(flags) + "\\g<2>" + str(contentFlags) + "\\g<3>", content
        )

    def replParentIndexAndLodDistance(self, matchobj: Match, entities: list[EntityItem], mutableIndex: list[int], hdToLod: dict[int, int], offsetParentIndex: int) -> str:
        archetypeName = matchobj.group(2).lower()
        if (self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE > 0 and archetypeName in self.lodCandidates) or \
                (self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE <= 0 and archetypeName in self.slodCandidates):
            index = mutableIndex[0]
            parentIndex = hdToLod[index] + offsetParentIndex
            entity = entities[index]
            lodDistance = Util.floatToStr(entity.lodDistance)
            mutableIndex[0] += 1
        else:
            parentIndex = -1
            lodDistance = matchobj.group(4)

        return matchobj.group(1) + str(parentIndex) + matchobj.group(3) + lodDistance + matchobj.group(5)

    def replacePlaceholders(self, template: str, name: str, textureDictionary: str, drawableDictionary: str, bbox: Box, bsphere: Sphere, hdDistance: float, lodDistance: float) -> str:
        return template \
            .replace("${NAME}", name) \
            .replace("${TEXTURE_DICTIONARY}", textureDictionary) \
            .replace("${DRAWABLE_DICTIONARY}", drawableDictionary) \
            .replace("${LOD_DISTANCE}", Util.floatToStr(lodDistance)) \
            .replace("${HD_TEXTURE_DISTANCE}", Util.floatToStr(hdDistance)) \
            .replace("${BSPHERE.CENTER.X}", Util.floatToStr(bsphere.center[0])) \
            .replace("${BSPHERE.CENTER.Y}", Util.floatToStr(bsphere.center[1])) \
            .replace("${BSPHERE.CENTER.Z}", Util.floatToStr(bsphere.center[2])) \
            .replace("${BSPHERE.RADIUS}", Util.floatToStr(bsphere.radius)) \
            .replace("${BBOX.MIN.X}", Util.floatToStr(bbox.min[0])) \
            .replace("${BBOX.MIN.Y}", Util.floatToStr(bbox.min[1])) \
            .replace("${BBOX.MIN.Z}", Util.floatToStr(bbox.min[2])) \
            .replace("${BBOX.MAX.X}", Util.floatToStr(bbox.max[0])) \
            .replace("${BBOX.MAX.Y}", Util.floatToStr(bbox.max[1])) \
            .replace("${BBOX.MAX.Z}", Util.floatToStr(bbox.max[2]))

    def createIndicesForRectangles(self, numRectangles: int) -> list[int]:
        indices = []
        for i in range(numRectangles):
            offset = 4 * i
            indices.append(offset)
            indices.append(offset + 1)
            indices.append(offset + 2)
            indices.append(offset + 2)
            indices.append(offset + 3)
            indices.append(offset)
        return indices

    def createAabb(self, bbox: Box) -> str:
        return self.contentTemplateMeshAabb \
            .replace("${BBOX.MIN.X}", Util.floatToStr(bbox.min[0])) \
            .replace("${BBOX.MIN.Y}", Util.floatToStr(bbox.min[1])) \
            .replace("${BBOX.MIN.Z}", Util.floatToStr(bbox.min[2])) \
            .replace("${BBOX.MAX.X}", Util.floatToStr(bbox.max[0])) \
            .replace("${BBOX.MAX.Y}", Util.floatToStr(bbox.max[1])) \
            .replace("${BBOX.MAX.Z}", Util.floatToStr(bbox.max[2]))

    @staticmethod
    def findClosestRatio(ratioInput: float, ratioCandidate1: float, ratioCandidate2: float) -> (float, int):
        options = [ratioCandidate1, ratioCandidate2]
        argmin = np.abs(np.asarray(options) - ratioInput).argmin()
        return options[argmin], argmin

    @staticmethod
    def createLodModelVertexNormalTextureUVStr(vertex: list[float], normal: list[float], uv: list[float]):
        colors = "255 0 255 255"
        return "				" + Util.vectorToStr(vertex) + " / " + Util.vectorToStr(Util.normalize(normal)) + " / " + colors + " / " + Util.vectorToStr(uv) + "\n"

    def createIndicesStr(self, indices: list[int]) -> str:
        indicesStr = ""
        i = 0
        for index in indices:
            if i % 15 == 0:
                if i > 0:
                    indicesStr += "\n"
                indicesStr += "				"
            else:
                indicesStr += " "
            indicesStr += str(index)
            i += 1

        return indicesStr

    @staticmethod
    def appendFrontPlaneIndicesForLod(indices: list[int], offset: int):
        indicesTemplate = [
            0, 1, 2,
            2, 3, 0
        ]
        for i in range(2):
            indices += [x + offset for x in indicesTemplate]
            offset += max(indicesTemplate) + 1

        indicesTemplate.reverse()
        for i in range(2):
            indices += [x + offset for x in indicesTemplate]
            offset += max(indicesTemplate) + 1

    @staticmethod
    def appendTopPlaneIndicesForLod(indices: list[int], offset: int):
        indicesTemplateTop = [
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 1
        ]
        indices += [x + offset for x in indicesTemplateTop]

    @staticmethod
    def appendTopPlaneIndicesForReflLod(indices: list[int], offset: int):
        # clockwise order because in reflection that top plane is needed for viewing from below and not above
        indicesTemplateTop = [
            0, 2, 1,
            0, 3, 2
        ]
        indices += [x + offset for x in indicesTemplateTop]

    def appendFrontPlaneVerticesForLod(self, vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], entity: EntityItem, planeIntersection: list[float]):
        bbox = self.ytypItems[entity.archetypeName].boundingBox
        height = bbox.getSizes()[2]

        lodCandidate = self.lodCandidates[entity.archetypeName]
        uvFrontMin = lodCandidate.uvFrontMin
        uvFrontMax = lodCandidate.uvFrontMax
        uvSideMin = lodCandidate.getUvSideMin()
        uvSideMax = lodCandidate.getUvSideMax()
        sideOffsetZ = lodCandidate.sideOffsetZ
        minZ = bbox.min[2] + height * max(0.0, sideOffsetZ)
        maxZ = bbox.max[2] - height * min(0.0, sideOffsetZ)

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], planeIntersection[1], bbox.min[2]], [-1, -0.1, 0], [uvFrontMin.u, uvFrontMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], planeIntersection[1], bbox.min[2]], [1, -0.1, 0], [uvFrontMax.u, uvFrontMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], planeIntersection[1], bbox.max[2]], [1, 0, 1], [uvFrontMax.u, uvFrontMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], planeIntersection[1], bbox.max[2]], [-1, 0, 1], [uvFrontMin.u, uvFrontMin.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.min[1], minZ], [0.1, -1, 0], [uvSideMin.u, uvSideMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.max[1], minZ], [0.1, 1, 0], [uvSideMax.u, uvSideMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.max[1], maxZ], [0, 1, 1], [uvSideMax.u, uvSideMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.min[1], maxZ], [0, -1, 1], [uvSideMin.u, uvSideMin.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], planeIntersection[1], bbox.min[2]], [-1, 0.1, 0], [uvFrontMin.u, uvFrontMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], planeIntersection[1], bbox.min[2]], [1, 0.1, 0], [uvFrontMax.u, uvFrontMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], planeIntersection[1], bbox.max[2]], [1, 0, 1], [uvFrontMax.u, uvFrontMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], planeIntersection[1], bbox.max[2]], [-1, 0, 1], [uvFrontMin.u, uvFrontMin.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.min[1], minZ], [-0.1, -1, 0], [uvSideMin.u, uvSideMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.max[1], minZ], [-0.1, 1, 0], [uvSideMax.u, uvSideMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.max[1], maxZ], [0, 1, 1], [uvSideMax.u, uvSideMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], bbox.min[1], maxZ], [0, -1, 1], [uvSideMin.u, uvSideMin.v])

    def appendDiagonalPlaneVerticesForLod(self, vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], entity: EntityItem, planeIntersection: list[float]):
        bbox = self.ytypItems[entity.archetypeName].boundingBox

        lodCandidate = self.lodCandidates[entity.archetypeName]

        uvFrontMin = lodCandidate.uvFrontMin
        uvFrontMax = lodCandidate.uvFrontMax
        uvSideMin = lodCandidate.getUvSideMin()
        uvSideMax = lodCandidate.getUvSideMax()

        distanceLeftToIntersection = planeIntersection[0] - bbox.min[0]
        distanceRightToIntersection = bbox.max[0] - planeIntersection[0]
        distanceBottomToIntersection = planeIntersection[1] - bbox.min[1]
        distanceTopToIntersection = bbox.max[1] - planeIntersection[1]

        vectorRightTop = self.calculateVectorOnEllipseAtDiagonal(distanceRightToIntersection, distanceTopToIntersection, 0)
        vectorLeftBottom = self.calculateVectorOnEllipseAtDiagonal(distanceLeftToIntersection, distanceBottomToIntersection, 2)
        lengthVectorRightTop = norm(vectorRightTop)
        lengthVectorLeftBottom = norm(vectorLeftBottom)
        ratio = lengthVectorRightTop / (lengthVectorLeftBottom + lengthVectorRightTop)
        desiredRatio, option = LodMapCreator.findClosestRatio(ratio, lodCandidate.textureOrigin, lodCandidate.textureOriginSide())
        uvDiagonal1Min = uvFrontMin if option == 0 else uvSideMin
        uvDiagonal1Max = uvFrontMax if option == 0 else uvSideMax
        if ratio > desiredRatio:
            adapt = desiredRatio / (1 - desiredRatio) * lengthVectorLeftBottom / lengthVectorRightTop
            vectorRightTop = [vectorRightTop[0] * adapt, vectorRightTop[1] * adapt]
        else:
            adapt = (1 - desiredRatio) / desiredRatio * lengthVectorRightTop / lengthVectorLeftBottom
            vectorLeftBottom = [vectorLeftBottom[0] * adapt, vectorLeftBottom[1] * adapt]

        vectorLeftTop = self.calculateVectorOnEllipseAtDiagonal(distanceLeftToIntersection, distanceTopToIntersection, 1)
        vectorRightBottom = self.calculateVectorOnEllipseAtDiagonal(distanceRightToIntersection, distanceBottomToIntersection, 3)
        lengthVectorLeftTop = norm(vectorLeftTop)
        lengthVectorRightBottom = norm(vectorRightBottom)
        ratio = lengthVectorRightBottom / (lengthVectorLeftTop + lengthVectorRightBottom)
        desiredRatio, option = LodMapCreator.findClosestRatio(ratio, lodCandidate.textureOrigin, lodCandidate.textureOriginSide())
        uvDiagonal2Min = uvFrontMin if option == 0 else uvSideMin
        uvDiagonal2Max = uvFrontMax if option == 0 else uvSideMax
        if ratio > desiredRatio:
            adapt = desiredRatio / (1 - desiredRatio) * lengthVectorLeftTop / lengthVectorRightBottom
            vectorRightBottom = [vectorRightBottom[0] * adapt, vectorRightBottom[1] * adapt]
        else:
            adapt = (1 - desiredRatio) / desiredRatio * lengthVectorRightBottom / lengthVectorLeftTop
            vectorLeftTop = [vectorLeftTop[0] * adapt, vectorLeftTop[1] * adapt]

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightTop[0], planeIntersection[1] + vectorRightTop[1], bbox.min[2]], [0.9, 1, 0], [uvDiagonal1Min.u, uvDiagonal1Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftBottom[0], planeIntersection[1] + vectorLeftBottom[1], bbox.min[2]], [-1, -0.9, 0], [uvDiagonal1Max.u, uvDiagonal1Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftBottom[0], planeIntersection[1] + vectorLeftBottom[1], bbox.max[2]], [-1, -1, 1], [uvDiagonal1Max.u, uvDiagonal1Min.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightTop[0], planeIntersection[1] + vectorRightTop[1], bbox.max[2]], [1, 1, 1], [uvDiagonal1Min.u, uvDiagonal1Min.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightBottom[0], planeIntersection[1] + vectorRightBottom[1], bbox.min[2]], [1, 0.9, 0], [uvDiagonal2Min.u, uvDiagonal2Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftTop[0], planeIntersection[1] + vectorLeftTop[1], bbox.min[2]], [-0.9, 1, 0], [uvDiagonal2Max.u, uvDiagonal2Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftTop[0], planeIntersection[1] + vectorLeftTop[1], bbox.max[2]], [-1, 1, 1], [uvDiagonal2Max.u, uvDiagonal2Min.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightBottom[0], planeIntersection[1] + vectorRightBottom[1], bbox.max[2]], [1, 1, 1], [uvDiagonal2Min.u, uvDiagonal2Min.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightTop[0], planeIntersection[1] + vectorRightTop[1], bbox.min[2]], [1, 0.9, 0], [uvDiagonal1Min.u, uvDiagonal1Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftBottom[0], planeIntersection[1] + vectorLeftBottom[1], bbox.min[2]], [-0.9, -1, 0], [uvDiagonal1Max.u, uvDiagonal1Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftBottom[0], planeIntersection[1] + vectorLeftBottom[1], bbox.max[2]], [-1, -1, 1], [uvDiagonal1Max.u, uvDiagonal1Min.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightTop[0], planeIntersection[1] + vectorRightTop[1], bbox.max[2]], [1, 1, 1], [uvDiagonal1Min.u, uvDiagonal1Min.v])

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightBottom[0], planeIntersection[1] + vectorRightBottom[1], bbox.min[2]], [0.9, -1, 0], [uvDiagonal2Min.u, uvDiagonal2Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftTop[0], planeIntersection[1] + vectorLeftTop[1], bbox.min[2]], [-1, 0.9, 0], [uvDiagonal2Max.u, uvDiagonal2Max.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorLeftTop[0], planeIntersection[1] + vectorLeftTop[1], bbox.max[2]], [-1, 1, 1], [uvDiagonal2Max.u, uvDiagonal2Min.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0] + vectorRightBottom[0], planeIntersection[1] + vectorRightBottom[1], bbox.max[2]], [1, -1, 1], [uvDiagonal2Min.u, uvDiagonal2Min.v])

    def appendTopPlaneVerticesForLod(self, vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], entity: EntityItem, planeIntersection: list[float]):
        bbox = self.ytypItems[entity.archetypeName].boundingBox
        sizes = bbox.getSizes()

        lodCandidate = self.lodCandidates[entity.archetypeName]

        uvTopMin = lodCandidate.uvTopMin
        uvTopMax = lodCandidate.uvTopMax
        uvTopCenter = lodCandidate.getUvTopCenter()

        planeTopMinZ = bbox.min[2] + sizes[2] * (1 - lodCandidate.planeZ)
        # planeTopMaxZ = min(bbox.max[2] - min(sizes) * 0.1, planeTopMinZ + min(sizes[0], sizes[1]) / 4)
        planeTopMaxZ = max(bbox.min[2] + min(sizes) * 0.2, planeTopMinZ - 0.15 * min(sizes[0], sizes[1]))

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [planeIntersection[0], planeIntersection[1], planeTopMaxZ], [0, 0, 1], [uvTopCenter.u, uvTopCenter.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], bbox.min[1], planeTopMinZ], [-1, -1, 0.1], [uvTopMin.u, uvTopMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], bbox.min[1], planeTopMinZ], [1, -1, 0.1], [uvTopMax.u, uvTopMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], bbox.max[1], planeTopMinZ], [1, 1, 0.1], [uvTopMax.u, uvTopMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], bbox.max[1], planeTopMinZ], [-1, 1, 0.1], [uvTopMin.u, uvTopMin.v])

    def appendTopPlaneVerticesForReflLod(self, vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], entity: EntityItem):
        bbox = self.ytypItems[entity.archetypeName].boundingBox
        sizes = bbox.getSizes()

        lodCandidate = self.lodCandidates[entity.archetypeName]

        uvTopMin = lodCandidate.uvTopMin
        uvTopMax = lodCandidate.uvTopMax

        planeTopZ = bbox.min[2] + sizes[2] * (1 - lodCandidate.planeZ)

        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], bbox.min[1], planeTopZ], [-1, -1, 0.1], [uvTopMin.u, uvTopMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], bbox.min[1], planeTopZ], [1, -1, 0.1], [uvTopMax.u, uvTopMax.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.max[0], bbox.max[1], planeTopZ], [1, 1, 0.1], [uvTopMax.u, uvTopMin.v])
        self.appendVertexForLod(vertices, normals, textureUVs, entity, [bbox.min[0], bbox.max[1], planeTopZ], [-1, 1, 0.1], [uvTopMin.u, uvTopMin.v])

    def appendVertexForLod(self, vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], entity: EntityItem, vertex: list[float], normal: list[float], uv: list[float]):
        vertices.append(entity.applyTransformationTo(vertex))
        normals.append(Util.applyRotation(normal, entity.rotation))
        textureUVs.append(uv)

    @staticmethod
    def convertVerticesNormalsTextureUVsAsStr(vertices: list[list[float]], normals: list[list[float]], textureUVs: list[list[float]], translation: list[float]) -> str:
        result = ""
        for i in range(len(vertices)):
            translatedVertex = np.add(vertices[i], translation).tolist()
            result += LodMapCreator.createLodModelVertexNormalTextureUVStr(translatedVertex, normals[i], textureUVs[i])
        return result

    @staticmethod
    def convertVerticesTextureUVsAsStrForSlod(vertices: list[list[float]], sizes: list[list[float]], textureUVs: list[list[UV]], translation: list[float]) -> str:
        result = ""
        for i in range(len(vertices)):
            translatedVertex = np.add(vertices[i], translation).tolist()

            uvMin = textureUVs[i][0]
            uvMax = textureUVs[i][1]

            # for a bit more variety randomly flip/mirror the texture
            # yields same result for multiple runs and different SLOD levels (therefore do not use translatedVertex)
            xyHash = Util.hashFloat(vertices[i][0]) ^ Util.hashFloat(vertices[i][1])
            if xyHash % 2 == 0:
                temp = uvMin.u
                uvMin = UV(uvMax.u, uvMin.v)
                uvMax = UV(temp, uvMax.v)

            result += LodMapCreator.createSlodModelVertexNormalTextureUVStr(translatedVertex, [-1, -0.1, 0], sizes[i], [0, 1], [uvMin.u, uvMax.v])
            result += LodMapCreator.createSlodModelVertexNormalTextureUVStr(translatedVertex, [1, -0.1, 0], sizes[i], [1, 1], [uvMax.u, uvMax.v])
            result += LodMapCreator.createSlodModelVertexNormalTextureUVStr(translatedVertex, [1, 0, 1], sizes[i], [1, 0], [uvMax.u, uvMin.v])
            result += LodMapCreator.createSlodModelVertexNormalTextureUVStr(translatedVertex, [-1, 0, 1], sizes[i], [0, 0], [uvMin.u, uvMin.v])

        return result

    @staticmethod
    def createSlodModelVertexNormalTextureUVStr(center: list[float], normal: list[float], size: list[float], uvPosition: list[float], uv: list[float]):
        colorsFront = "255 0 255 255 / 0 0 255 0"
        return "				" + Util.vectorToStr(center) + " / " + Util.vectorToStr(Util.normalize(normal)) + " / " + colorsFront + " / " + Util.vectorToStr(uvPosition) + " / " + Util.vectorToStr(uv) + " / " + Util.vectorToStr(size) + " / " + Util.vectorToStr([1, 1]) + "\n"

    def createLodOrSlodModel(self, nameWithoutSlodLevel: str, slodLevel: int, drawableDictionary: str, entities: list[EntityItem], parentIndex: int, numChildren: int, mapPrefix: str, reflection: bool) -> EntityItem:
        if reflection:
            lodName = nameWithoutSlodLevel
            slodLevel = 3
            numChildren = 0
        elif slodLevel > 0:
            lodName = nameWithoutSlodLevel + "_slod" + str(slodLevel)
        else:
            lodName = nameWithoutSlodLevel + "_lod"

        if slodLevel < self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE or reflection:
            return self.createLodModel(lodName, slodLevel, drawableDictionary, entities, parentIndex, numChildren, mapPrefix, reflection)
        else:
            return self.createSlodModel(lodName, slodLevel, drawableDictionary, entities, parentIndex, numChildren, mapPrefix)
    def createLodModel(self, lodName: str, slodLevel: int, drawableDictionary: str, entities: list[EntityItem], parentIndex: int, numChildren: int, mapPrefix: str, reflection: bool) -> EntityItem:
        self.foundLod = True

        # Classic behavior groups geometries by diffuse sampler. With a single-texture atlas
        # (one sampler name for many archetypes), that collapses to one geometry. To keep
        # Larcius-like "many models" in the drawable while retaining the atlas, we split
        # geometries by archetype *only when* the cluster resolves to exactly one sampler.
        uniqueSamplers: set[str] = set()
        maxHdEntityLodDistance = 0

        for e in entities:
            maxHdEntityLodDistance = max(maxHdEntityLodDistance, e.lodDistance)

            s = self._peek_custom_override_sampler_if_present(e)
            if s is not None:
                uniqueSamplers.add(s)
                continue

            cand = self.lodCandidates.get(e.archetypeName)
            if cand is None:
                continue
            uniqueSamplers.add(cand.diffuseSampler)

        splitByArchetype = (len(uniqueSamplers) == 1)

        groupToVertices: dict = {}
        groupToNormals: dict = {}
        groupToTextureUVs: dict = {}
        groupToIndices: dict = {}
        groupKeyToSampler: dict[str, str] = {}

        for entity in entities:
            # Custom OBJ override path.
            if splitByArchetype:
                gkey = entity.archetypeName.lower()
                if self._append_custom_override_mesh_if_present(
                    entity,
                    groupToVertices, groupToNormals, groupToTextureUVs, groupToIndices,
                    groupKey=gkey, groupKeyToSampler=groupKeyToSampler
                ):
                    continue
            else:
                if self._append_custom_override_mesh_if_present(entity, groupToVertices, groupToNormals, groupToTextureUVs, groupToIndices):
                    continue

            lodCandidate = self.lodCandidates.get(entity.archetypeName)
            if lodCandidate is None:
                continue

            diffuseSampler = lodCandidate.diffuseSampler
            key = entity.archetypeName.lower() if splitByArchetype else diffuseSampler
            groupKeyToSampler.setdefault(key, diffuseSampler)

            if key not in groupToVertices:
                groupToVertices[key] = []
                groupToNormals[key] = []
                groupToTextureUVs[key] = []
                groupToIndices[key] = []

            bbox = self.ytypItems[entity.archetypeName].boundingBox
            sizes = bbox.getSizes()
            distanceLeftToIntersection = sizes[0] * lodCandidate.textureOrigin
            distanceBottomToIntersection = sizes[1] * lodCandidate.textureOriginSide()
            planeIntersection = [bbox.min[0] + distanceLeftToIntersection, bbox.min[1] + distanceBottomToIntersection]

            LodMapCreator.appendFrontPlaneIndicesForLod(groupToIndices[key], len(groupToVertices[key]))
            self.appendFrontPlaneVerticesForLod(groupToVertices[key], groupToNormals[key], groupToTextureUVs[key], entity, planeIntersection)

            if lodCandidate.hasDiagonal(bbox, entity.scale):
                LodMapCreator.appendFrontPlaneIndicesForLod(groupToIndices[key], len(groupToVertices[key]))
                self.appendDiagonalPlaneVerticesForLod(groupToVertices[key], groupToNormals[key], groupToTextureUVs[key], entity, planeIntersection)

            if not lodCandidate.hasTop(bbox, entity.scale):
                pass
            elif reflection:
                LodMapCreator.appendTopPlaneIndicesForReflLod(groupToIndices[key], len(groupToVertices[key]))
                self.appendTopPlaneVerticesForReflLod(groupToVertices[key], groupToNormals[key], groupToTextureUVs[key], entity)
            elif slodLevel < self.USE_NO_TOP_TEMPLATE_FOR_LEVEL_AND_ABOVE:
                LodMapCreator.appendTopPlaneIndicesForLod(groupToIndices[key], len(groupToVertices[key]))
                self.appendTopPlaneVerticesForLod(groupToVertices[key], groupToNormals[key], groupToTextureUVs[key], entity, planeIntersection)

        totalBoundingGeometry = BoundingGeometry()
        for key in groupToVertices:
            totalBoundingGeometry.extendByPoints(groupToVertices[key])

        totalBoundingSphere = totalBoundingGeometry.getSphere()
        center = totalBoundingSphere.center
        translation = np.multiply(center, [-1]).tolist()

        totalBoundingBox = totalBoundingGeometry.getBox().getTranslated(translation)
        totalBoundingSphere = totalBoundingSphere.getTranslated(translation)

        bounds = self.createAabb(totalBoundingBox)

        # Deduplicate shaders by sampler name.
        samplerToShaderIndex: dict[str, int] = {}
        shaders = ""
        geometries = ""
        nextShaderIndex = 0

        for key in groupToVertices:
            sampler = groupKeyToSampler.get(key)
            if sampler is None:
                sampler = next(iter(uniqueSamplers)) if uniqueSamplers else "lod_default"

            if sampler not in samplerToShaderIndex:
                samplerToShaderIndex[sampler] = nextShaderIndex
                if reflection:
                    shaders += self.contentTemplateOdrShaderAlpha.replace("${DIFFUSE_SAMPLER}", sampler)
                else:
                    shaders += self.contentTemplateOdrShaderTreeLod.replace("${DIFFUSE_SAMPLER}", sampler)
                nextShaderIndex += 1

            indices = groupToIndices[key]

            boundingGeometry = BoundingGeometry(groupToVertices[key])
            boundingBox = boundingGeometry.getBox().getTranslated(translation)

            verticesNormalsTextureUVsStr = LodMapCreator.convertVerticesNormalsTextureUVsAsStr(
                groupToVertices[key], groupToNormals[key], groupToTextureUVs[key], translation
            )

            bounds += self.createAabb(boundingBox)

            geometries += self.contentTemplateMeshGeometry                 .replace("${SHADER_INDEX}", str(samplerToShaderIndex[sampler]))                 .replace("${VERTEX_DECLARATION}", self.VERTEX_DECLARATION_TREE_LOD)                 .replace("${INDICES.NUM}", str(len(indices)))                 .replace("${INDICES}", self.createIndicesStr(indices))                 .replace("${VERTICES.NUM}", str(len(groupToVertices[key])))                 .replace("${VERTICES}\n", verticesNormalsTextureUVsStr)

        contentModelMesh = self.contentTemplateMesh             .replace("${BOUNDS}\n", bounds)             .replace("${GEOMETRIES}\n", geometries)

        fileModelMesh = open(os.path.join(self.getOutputDirMeshes(reflection), lodName.lower() + ".mesh"), 'w')
        fileModelMesh.write(contentModelMesh)
        fileModelMesh.close()

        contentModelOdr = self.contentTemplateOdr             .replace("${BBOX.MIN.X}", Util.floatToStr(totalBoundingBox.min[0]))             .replace("${BBOX.MIN.Y}", Util.floatToStr(totalBoundingBox.min[1]))             .replace("${BBOX.MIN.Z}", Util.floatToStr(totalBoundingBox.min[2]))             .replace("${BBOX.MAX.X}", Util.floatToStr(totalBoundingBox.max[0]))             .replace("${BBOX.MAX.Y}", Util.floatToStr(totalBoundingBox.max[1]))             .replace("${BBOX.MAX.Z}", Util.floatToStr(totalBoundingBox.max[2]))             .replace("${BSPHERE.CENTER.X}", Util.floatToStr(totalBoundingSphere.center[0]))             .replace("${BSPHERE.CENTER.Y}", Util.floatToStr(totalBoundingSphere.center[1]))             .replace("${BSPHERE.CENTER.Z}", Util.floatToStr(totalBoundingSphere.center[2]))             .replace("${BSPHERE.RADIUS}", Util.floatToStr(totalBoundingSphere.radius))             .replace("${MESH_FILENAME}", lodName.lower() + ".mesh")             .replace("${SHADERS}\n", shaders)

        fileModelOdr = open(os.path.join(self.getOutputDirMeshes(reflection), lodName.lower() + ".odr"), 'w')
        fileModelOdr.write(contentModelOdr)
        fileModelOdr.close()

        itemLodDistance = self.getLodDistance(slodLevel)
        if reflection:
            childLodDistance = 0
            itemLodDistance = self.REFL_LOD_DISTANCE
        elif slodLevel > 0:
            childLodDistance = self.getLodDistance(slodLevel - 1)
        else:
            childLodDistance = maxHdEntityLodDistance

        self.writeYtypItem(lodName, LodMapCreator.TEXTURE_DICTIONARY_LOD, drawableDictionary, totalBoundingBox, totalBoundingSphere, childLodDistance, itemLodDistance, mapPrefix, slodLevel, reflection)

        return self.createEntityItem(lodName, center, childLodDistance, itemLodDistance, parentIndex, numChildren, slodLevel, reflection)

    def calculateVectorOnEllipseAtDiagonal(self, semiaxisX: float, semiaxisY: float, quadrant: int) -> list[float]:
        coordinate = semiaxisX * semiaxisY / math.sqrt(semiaxisX**2 + semiaxisY**2)
        return [coordinate * (1 if quadrant == 0 or quadrant == 3 else -1), coordinate * (1 if quadrant == 0 or quadrant == 1 else -1)]

    def createDrawableDictionary(self, name: str, entities: list[EntityItem], reflection: bool):
        if len(entities) == 0:
            return

        relPath = os.path.relpath(self.getOutputDirMeshes(reflection), self.getOutputDirModels(reflection))

        file = open(os.path.join(self.getOutputDirModels(reflection), name.lower() + ".odd"), 'w')
        file.write("Version 165 32\n{\n")
        for entity in entities:
            file.write("\t")
            file.write(os.path.join(relPath, entity.archetypeName.lower() + ".odr"))
            file.write("\n")
        file.write("}\n")
        file.close()

    def appendSlodTop(self, verticesTop: list[list[float]], normalsTop: list[list[float]], textureUVsTop: list[list[float]], size: list[float], center: list[float], rotation: list[float], uvMap: UVMap):
        # in terms of absolute z-coordinate this is sizeZ * (1 - topZ) + transformedBboxEntityMin[2]
        # which yields relative (to centerTransformed) z-coordinate sizeZ * (1 - topZ) - sizeZ / 2 <=>  sizeZ * ((1 - topZ) - 1 / 2)
        planeZ = size[1] * (0.5 - uvMap.topZ)

        vertices = [
            [-size[0] / 2, -size[0] / 2, planeZ],
            [size[0] / 2, -size[0] / 2, planeZ],
            [size[0] / 2, size[0] / 2, planeZ],
            [-size[0] / 2, size[0] / 2, planeZ]
        ]

        normals = [
            [-1, -1, 0.1],
            [1, -1, 0.1],
            [1, 1, 0.1],
            [-1, 1, 0.1]
        ]

        rotZ, unused, unused = transforms3d.euler.quat2euler(rotation, axes='rzyx')
        onlyZRotationQuaternion = transforms3d.euler.euler2quat(rotZ, 0, 0, axes='rzyx')
        for i in range(4):
            rotatedVertex = Util.applyRotation(vertices[i], onlyZRotationQuaternion)
            translatedVertex = np.add(rotatedVertex, center).tolist()
            rotatedNormal = Util.applyRotation(normals[i], onlyZRotationQuaternion)

            verticesTop.append(translatedVertex)
            normalsTop.append(rotatedNormal)

        textureUVsTop += [
            [uvMap.topMin.u, uvMap.topMax.v],
            [uvMap.topMax.u, uvMap.topMax.v],
            [uvMap.topMax.u, uvMap.topMin.v],
            [uvMap.topMin.u, uvMap.topMin.v]
        ]

    def createSlodModel(self, name: str, slodLevel: int, drawableDictionary: str, entities: list[EntityItem], parentIndex: int, numChildren: int, mapPrefix: str) -> EntityItem:
        self.foundSlod = True

        verticesFront = {}
        sizesFront = {}
        textureUVsFront = {}
        verticesTop = {}
        normalsTop = {}
        textureUVsTop = {}
        bbox = {}

        maxHdEntityLodDistance = 0

        for entity in entities:
            maxHdEntityLodDistance = max(maxHdEntityLodDistance, entity.lodDistance)

            uvMap = self.slodCandidates[entity.archetypeName]

            diffuseSampler = uvMap.getDiffuseSampler()

            if diffuseSampler not in verticesFront:
                verticesFront[diffuseSampler] = []
                sizesFront[diffuseSampler] = []
                textureUVsFront[diffuseSampler] = []
                verticesTop[diffuseSampler] = []
                normalsTop[diffuseSampler] = []
                textureUVsTop[diffuseSampler] = []
                bbox[diffuseSampler] = Box.createReversedInfinityBox()

            bboxEntity = self.ytypItems[entity.archetypeName].boundingBox

            size = bboxEntity.getScaled(entity.scale).getSizes()
            sizeXY = (size[0] + size[1]) / 2
            sizeZ = size[2]

            centerTransformed = entity.applyTransformationTo(bboxEntity.getCenter())

            transformedBboxEntityMin = np.subtract(centerTransformed, [sizeXY / 2, sizeXY / 2, sizeZ / 2]).tolist()
            transformedBboxEntityMax = np.add(centerTransformed, [sizeXY / 2, sizeXY / 2, sizeZ / 2]).tolist()

            bbox[diffuseSampler].extendByPoint(transformedBboxEntityMin)
            bbox[diffuseSampler].extendByPoint(transformedBboxEntityMax)

            size2D = [sizeXY, sizeZ]
            sizesFront[diffuseSampler].append(size2D)
            verticesFront[diffuseSampler].append(centerTransformed)
            textureUVsFront[diffuseSampler].append([uvMap.frontMin, uvMap.frontMax])

            if slodLevel < self.USE_NO_TOP_TEMPLATE_FOR_LEVEL_AND_ABOVE and uvMap.topMin is not None and uvMap.topMax is not None:
                assert uvMap.topZ is not None
                self.appendSlodTop(verticesTop[diffuseSampler], normalsTop[diffuseSampler], textureUVsTop[diffuseSampler], size2D, centerTransformed, entity.rotation, uvMap)

        totalBoundingGeometry = BoundingGeometry()

        for diffuseSampler in verticesTop:
            totalBoundingGeometry.extendByPoints(verticesTop[diffuseSampler])

        for diffuseSampler in verticesFront:
            for i in range(len(verticesFront[diffuseSampler])):
                size2D = sizesFront[diffuseSampler][i]
                size3D = [size2D[0], size2D[0], size2D[1]]
                minVertex = np.subtract(verticesFront[diffuseSampler][i], size3D).tolist()
                maxVertex = np.add(verticesFront[diffuseSampler][i], size3D).tolist()
                totalBoundingGeometry.extendByPoints([minVertex, maxVertex])

        totalBoundingSphere = totalBoundingGeometry.getSphere()
        center = totalBoundingSphere.center
        translation = np.multiply(center, [-1]).tolist()

        totalBoundingBox = totalBoundingGeometry.getBox().getTranslated(translation)
        totalBoundingSphere = totalBoundingSphere.getTranslated(translation)

        bounds = self.createAabb(totalBoundingBox)

        shaders = ""
        geometries = ""
        shaderIndex = 0
        for diffuseSampler in verticesFront:
            numFrontPlanes = len(verticesFront[diffuseSampler])
            indicesFront = self.createIndicesForRectangles(numFrontPlanes)

            bbox[diffuseSampler] = bbox[diffuseSampler].getTranslated(translation)
            verticesFrontStr = LodMapCreator.convertVerticesTextureUVsAsStrForSlod(verticesFront[diffuseSampler], sizesFront[diffuseSampler], textureUVsFront[diffuseSampler], translation)

            shaders += self.contentTemplateOdrShaderTreeLod2.replace("${DIFFUSE_SAMPLER}", diffuseSampler)

            bounds += self.createAabb(bbox[diffuseSampler])

            geometries += self.contentTemplateMeshGeometry \
                .replace("${SHADER_INDEX}", str(shaderIndex)) \
                .replace("${VERTEX_DECLARATION}", self.VERTEX_DECLARATION_TREE_LOD2) \
                .replace("${INDICES.NUM}", str(numFrontPlanes * 6)) \
                .replace("${INDICES}", self.createIndicesStr(indicesFront)) \
                .replace("${VERTICES.NUM}", str(numFrontPlanes * 4)) \
                .replace("${VERTICES}\n", verticesFrontStr)
            shaderIndex += 1

            if len(verticesTop[diffuseSampler]) > 0:
                assert len(verticesTop[diffuseSampler]) % 4 == 0
                numTopPlanes = int(len(verticesTop[diffuseSampler]) / 4)
                indicesTop = self.createIndicesForRectangles(numTopPlanes)
                verticesTopStr = LodMapCreator.convertVerticesNormalsTextureUVsAsStr(verticesTop[diffuseSampler], normalsTop[diffuseSampler], textureUVsTop[diffuseSampler], translation)

                shaders += self.contentTemplateOdrShaderTreeLod.replace("${DIFFUSE_SAMPLER}", diffuseSampler + "_top")

                bounds += self.createAabb(bbox[diffuseSampler])

                geometries += self.contentTemplateMeshGeometry \
                    .replace("${SHADER_INDEX}", str(shaderIndex)) \
                    .replace("${VERTEX_DECLARATION}", self.VERTEX_DECLARATION_TREE_LOD) \
                    .replace("${INDICES.NUM}", str(numTopPlanes * 6)) \
                    .replace("${INDICES}", self.createIndicesStr(indicesTop)) \
                    .replace("${VERTICES.NUM}", str(len(verticesTop[diffuseSampler]))) \
                    .replace("${VERTICES}\n", verticesTopStr)
                shaderIndex += 1

        contentModelMesh = self.contentTemplateMesh \
            .replace("${BOUNDS}\n", bounds) \
            .replace("${GEOMETRIES}\n", geometries)

        fileModelMesh = open(os.path.join(self.getOutputDirMeshes(False), name.lower() + ".mesh"), 'w')
        fileModelMesh.write(contentModelMesh)
        fileModelMesh.close()

        contentModelOdr = self.contentTemplateOdr \
            .replace("${BBOX.MIN.X}", Util.floatToStr(totalBoundingBox.min[0])) \
            .replace("${BBOX.MIN.Y}", Util.floatToStr(totalBoundingBox.min[1])) \
            .replace("${BBOX.MIN.Z}", Util.floatToStr(totalBoundingBox.min[2])) \
            .replace("${BBOX.MAX.X}", Util.floatToStr(totalBoundingBox.max[0])) \
            .replace("${BBOX.MAX.Y}", Util.floatToStr(totalBoundingBox.max[1])) \
            .replace("${BBOX.MAX.Z}", Util.floatToStr(totalBoundingBox.max[2])) \
            .replace("${BSPHERE.CENTER.X}", Util.floatToStr(totalBoundingSphere.center[0])) \
            .replace("${BSPHERE.CENTER.Y}", Util.floatToStr(totalBoundingSphere.center[1])) \
            .replace("${BSPHERE.CENTER.Z}", Util.floatToStr(totalBoundingSphere.center[2])) \
            .replace("${BSPHERE.RADIUS}", Util.floatToStr(totalBoundingSphere.radius)) \
            .replace("${MESH_FILENAME}", name.lower() + ".mesh") \
            .replace("${SHADERS}\n", shaders)

        fileModelOdr = open(os.path.join(self.getOutputDirMeshes(False), name.lower() + ".odr"), 'w')
        fileModelOdr.write(contentModelOdr)
        fileModelOdr.close()

        itemLodDistance = self.getLodDistance(slodLevel)
        if slodLevel == 0:
            childLodDistance = maxHdEntityLodDistance
        else:
            childLodDistance = self.getLodDistance(slodLevel - 1)

        self.writeYtypItem(name, LodMapCreator.TEXTURE_DICTIONARY_SLOD, drawableDictionary, totalBoundingBox, totalBoundingSphere, childLodDistance, itemLodDistance, mapPrefix, slodLevel, False)

        return self.createEntityItem(name, center, childLodDistance, itemLodDistance, parentIndex, numChildren, slodLevel, False)


    
    def createCustomMeshesForArchetypes(self, custom_names: set[str], output_dir: str = None, sampler_prefix: str = "lod_") -> None:
        """Create simple helper meshes for each archetype listed in custom_names.

        For each archetype we build three simple cards derived from its AABB:
          - a front billboard (facing +Y)
          - a side billboard (facing +X)
          - a horizontal top card (facing +Z)
        """
        if not custom_names:
            return

        # [MODIFIED LOGIC START]
        if output_dir is None:
            output_dir = self.getOutputDirCustomMeshes()
        
        os.makedirs(output_dir, exist_ok=True)
        # [MODIFIED LOGIC END]

        # Build lookup maps for case-insensitive matching against ytyp items
        ytyp_key_by_lower = {k.lower(): k for k in self.ytypItems.keys()}

        lod_keys_lower = getattr(self, "_builtinLodKeysLower", None)
        if not lod_keys_lower:
            lod_keys_lower = {k.lower() for k in getattr(self, "lodCandidates", {}).keys()}

        slod_keys_lower = getattr(self, "_builtinSlodKeysLower", None)
        if not slod_keys_lower:
            slod_keys_lower = {k.lower() for k in getattr(self, "slodCandidates", {}).keys()}

        for name in sorted(custom_names):
            aname = name.lower().strip()

            if aname in lod_keys_lower or aname in slod_keys_lower:
                continue

            ytyp_key = ytyp_key_by_lower.get(aname)
            if not ytyp_key:
                print(f"warning: custom mesh archetype '{aname}' not found in ytyp items; skipping")
                continue

            bbox = self.ytypItems[ytyp_key].boundingBox
            sizes = bbox.getSizes()

            minX, minY, minZ = bbox.min
            maxX, maxY, maxZ = bbox.max
            centerX = minX + sizes[0] * 0.5
            centerY = minY + sizes[1] * 0.5
            centerZ = minZ + sizes[2] * 0.5

            verts: list[list[float]] = []
            normals: list[list[float]] = []
            uvs: list[list[float]] = []

            plane_uvs = [
                [0.0, 1.0],
                [1.0, 1.0],
                [1.0, 0.0],
                [0.0, 0.0],
            ]

            # 1) Front billboard
            verts.extend([
                [minX, centerY, minZ], [maxX, centerY, minZ], [maxX, centerY, maxZ], [minX, centerY, maxZ],
            ])
            normals.extend([[0.0, 1.0, 0.0]] * 4)
            uvs.extend(plane_uvs)

            # 2) Side billboard
            verts.extend([
                [centerX, minY, minZ], [centerX, maxY, minZ], [centerX, maxY, maxZ], [centerX, minY, maxZ],
            ])
            normals.extend([[1.0, 0.0, 0.0]] * 4)
            uvs.extend(plane_uvs)

            # 3) Horizontal top card
            verts.extend([
                [minX, minY, centerZ], [maxX, minY, centerZ], [maxX, maxY, centerZ], [minX, maxY, centerZ],
            ])
            normals.extend([[0.0, 0.0, 1.0]] * 4)
            uvs.extend(plane_uvs)

            indices: list[int] = []
            num_rects = len(verts) // 4
            for i in range(num_rects):
                offset = 4 * i
                indices.extend([offset, offset + 1, offset + 2, offset + 2, offset + 3, offset])

            totalBoundingGeometry = BoundingGeometry()
            totalBoundingGeometry.extendByPoints(verts)
            totalBoundingSphere = totalBoundingGeometry.getSphere()
            center = totalBoundingSphere.center
            translation = np.multiply(center, [-1]).tolist()

            totalBoundingBox = totalBoundingGeometry.getBox().getTranslated(translation)
            totalBoundingSphere = totalBoundingSphere.getTranslated(translation)

            vnut_str = LodMapCreator.convertVerticesNormalsTextureUVsAsStr(verts, normals, uvs, translation)
            bounds = self.createAabb(totalBoundingBox)

            geometries = (
                self.contentTemplateMeshGeometry
                .replace("${SHADER_INDEX}", "0")
                .replace("${VERTEX_DECLARATION}", self.VERTEX_DECLARATION_TREE_LOD)
                .replace("${INDICES.NUM}", str(len(indices)))
                .replace("${INDICES}", self.createIndicesStr(indices))
                .replace("${VERTICES.NUM}", str(len(verts)))
                .replace("${VERTICES}\n", vnut_str)
            )

            contentModelMesh = (
                self.contentTemplateMesh
                .replace("${BOUNDS}\n", bounds)
                .replace("${GEOMETRIES}\n", geometries)
            )

            base = f"{aname}"
            mesh_filename = base + ".mesh"
            odr_filename = base + ".odr"

            with open(os.path.join(output_dir, mesh_filename), "w") as fmesh:
                fmesh.write(contentModelMesh)

            # [MODIFIED SHADER GENERATION]
            # Use the passed sampler_prefix (defaults to "lod_", but can be "slod_")
            shaders = self.contentTemplateOdrShaderTreeLod.replace("${DIFFUSE_SAMPLER}", f"{sampler_prefix}{aname}")

            contentModelOdr = (
                self.contentTemplateOdr
                .replace("${BBOX.MIN.X}", Util.floatToStr(totalBoundingBox.min[0]))
                .replace("${BBOX.MIN.Y}", Util.floatToStr(totalBoundingBox.min[1]))
                .replace("${BBOX.MIN.Z}", Util.floatToStr(totalBoundingBox.min[2]))
                .replace("${BBOX.MAX.X}", Util.floatToStr(totalBoundingBox.max[0]))
                .replace("${BBOX.MAX.Y}", Util.floatToStr(totalBoundingBox.max[1]))
                .replace("${BBOX.MAX.Z}", Util.floatToStr(totalBoundingBox.max[2]))
                .replace("${BSPHERE.CENTER.X}", Util.floatToStr(totalBoundingSphere.center[0]))
                .replace("${BSPHERE.CENTER.Y}", Util.floatToStr(totalBoundingSphere.center[1]))
                .replace("${BSPHERE.CENTER.Z}", Util.floatToStr(totalBoundingSphere.center[2]))
                .replace("${BSPHERE.RADIUS}", Util.floatToStr(totalBoundingSphere.radius))
                .replace("${MESH_FILENAME}", mesh_filename)
                .replace("${SHADERS}\n", shaders)
            )

            with open(os.path.join(output_dir, odr_filename), "w") as fodr:
                fodr.write(contentModelOdr)

            print(f"custom mesh written: {odr_filename}")

    def writeYtypItem(self, name: str, textureDictionary: str, drawableDictionary: str, totalBoundingBox: Box, totalBoundingSphere: Sphere, childLodDistance: float,
            itemLodDistance: float, mapPrefix: str, slodLevel: int, reflection: bool):

        if reflection:
            ytypItemsDict = self.reflYtypItems
        else:
            ytypItemsDict = self.slodYtypItems

        ytypName = self.getYtypName(mapPrefix, slodLevel, reflection)
        if ytypName in ytypItemsDict:
            ytypItems = ytypItemsDict[ytypName]
        else:
            ytypItems = open(os.path.join(self.getOutputDirMetadata(reflection), ytypName + ".ytyp.xml"), 'w')
            ytypItems.write("""<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<CMapTypes>
  <extensions/>
  <archetypes>
""")
            ytypItemsDict[ytypName] = ytypItems

        item = self.replacePlaceholders(self.contentTemplateYtypItem, name, textureDictionary, drawableDictionary, totalBoundingBox, totalBoundingSphere,
            childLodDistance, itemLodDistance)

        ytypItems.write(item)

    def getLodDistance(self, slodLevel: int) -> int:
        if slodLevel == 0:
            return LodMapCreator.LOD_DISTANCE
        elif slodLevel == 1:
            return LodMapCreator.SLOD_DISTANCE
        elif slodLevel == 2:
            return LodMapCreator.SLOD2_DISTANCE
        elif slodLevel == 3:
            return LodMapCreator.SLOD3_DISTANCE
        elif slodLevel == 4:
            return LodMapCreator.SLOD4_DISTANCE
        else:
            raise Exception("unknown slod level " + str(slodLevel))

    def getLodLevel(self, slodLevel: int) -> str:
        if slodLevel == 0:
            return LodLevel.LOD
        elif slodLevel == 1:
            return LodLevel.SLOD1
        elif slodLevel == 2:
            return LodLevel.SLOD2
        elif slodLevel == 3:
            return LodLevel.SLOD3
        elif slodLevel == 4:
            return LodLevel.SLOD4
        else:
            raise Exception("unknown slod level " + str(slodLevel))

    def getFlags(self, slodLevel: int, hasParent: bool, reflection: bool) -> (str, int):
        if slodLevel == 0:
            flags = Flag.FLAGS_LOD
        elif slodLevel == 1:
            flags = Flag.FLAGS_SLOD1
        elif slodLevel == 2:
            flags = Flag.FLAGS_SLOD2
        elif slodLevel == 3:
            flags = Flag.FLAGS_SLOD3
        elif slodLevel == 4:
            flags = Flag.FLAGS_SLOD4
        else:
            raise Exception("unknown slod level " + str(slodLevel))

        if hasParent and slodLevel % 2 == 1:
            flags |= Flag.LOD_IN_PARENT

        if reflection:
            flags |= Flag.ONLY_RENDER_IN_REFLECTIONS
        else:
            flags |= Flag.DONT_RENDER_IN_REFLECTIONS

        return flags

    def createEntityItem(self, name: str, center: list[float], childLodDistance: int, itemLodDistance: int, parentIndex: int, numChildren: int, slodLevel: int, reflection: bool) -> EntityItem:
        lodLevel = self.getLodLevel(slodLevel)
        flags = self.getFlags(slodLevel, parentIndex >= 0, reflection)
        return EntityItem(name, center, [1, 1, 1], [1, 0, 0, 0], itemLodDistance, childLodDistance, parentIndex, numChildren, lodLevel, flags)

    def fixHdOrOrphanHdLodLevelsAndSplitAccordingly(self, content: str) -> (str, Optional[str]):
        matchEntities = re.search("<entities>\\n", content, re.M)
        if matchEntities is None:
            return None, None, content, ""

        hdEntities = ""
        orphanHdEntities = ""
        for match in re.finditer('([\\t ]*<Item type="CEntityDef">' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<flags value=")([^"]+)("\\s*/>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<parentIndex value="([^"]+)"\\s*/>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<lodLevel>)[^<]+(</lodLevel>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<priorityLevel>)([^<]+)(</priorityLevel>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*</Item>)', content):

            isOrphanHd = (match.group(4) == "-1")

            flags = int(match.group(2))
            if isOrphanHd:
                # TODO pruefen ob korrekt
                flags |= Flag.FLAGS_ORPHANHD_DEFAULT
                flags &= ~Flag.FLAGS_ORPHANHD_EXCLUDE_DEFAULT
            else:
                flags |= Flag.FLAGS_HD_DEFAULT
                flags &= ~Flag.FLAGS_HD_EXCLUDE_DEFAULT

            entity = match.group(1) + str(flags) + match.group(3) + (LodLevel.ORPHAN_HD if isOrphanHd else LodLevel.HD) + match.group(5) + \
                     (match.group(6) if isOrphanHd else PriorityLevel.REQUIRED) + match.group(7)

            if isOrphanHd:
                orphanHdEntities += entity + "\n"
            else:
                hdEntities += entity + "\n"

        start = matchEntities.end()
        end = re.search("[\\t ]+</entities>[\\S\\s]*?\\Z", content, re.M).start()

        orphanHdEntities = None if orphanHdEntities == "" else orphanHdEntities
        hdEntities = None if hdEntities == "" else hdEntities

        return orphanHdEntities, hdEntities, content[:start], content[end:]

    def resetParentIndexAndNumChildren(self, content: str) -> str:
        result = re.sub('(<parentIndex value=")[^"]+("/>)', '\\g<1>-1\\g<2>', content)
        result = re.sub('(<numChildren value=")[^"]+("/>)', '\\g<1>0\\g<2>', result)
        return result

    def determinePrefixBundles(self):
        mapNames = []
        for filename in natsorted(os.listdir(self.inputDir)):
            if filename.endswith(".ymap.xml") and not filename.endswith("_lod.ymap.xml") and not filename.endswith("_slod2.ymap.xml"):
                mapNames.append(Util.getMapnameFromFilename(filename))

        self.bundlePrefixes = Util.determinePrefixBundles(mapNames)

    def processFiles(self):
        for mapPrefix in self.bundlePrefixes:
            self.processFilesWithPrefix(mapPrefix)

        self.finalizeYtypItems(False)
        if self.createReflection:
            self.finalizeYtypItems(True)

    def finalizeYtypItems(self, reflection: bool):
        if reflection:
            ytypItemsDict = self.reflYtypItems
        else:
            ytypItemsDict = self.slodYtypItems

        for ytypName, ytypItems in ytypItemsDict.items():
            self.finalizeSpecificYtypItems(ytypItems, ytypName)

    def finalizeSpecificYtypItems(self, ytypItems: IO, ytypName: str):
        ytypItems.write("""  </archetypes>
  <name>""" + ytypName + """</name>
  <dependencies/>
  <compositeEntityTypes/>
</CMapTypes>""")

        ytypItems.close()

    def processFilesWithPrefix(self, mapPrefix: str):
        hdEntitiesWithLod = []
        lodCoords = []
        lodDistances = []
        for filename in natsorted(os.listdir(self.inputDir)):
            if not filename.endswith(".ymap.xml") or not filename.startswith(mapPrefix.lower()):
                continue

            mapName = Util.getMapnameFromFilename(filename)

            if os.path.exists(os.path.join(self.getOutputDirMaps(False), Util.getFilenameFromMapname(mapName))):
                print("\twarning: skipping " + filename + " since such a map was created by this script")
                continue

            print("\tprocessing " + filename)

            fileNoLod = open(os.path.join(self.inputDir, filename), 'r')
            contentNoLod = fileNoLod.read()
            fileNoLod.close()

            contentNoLod = self.resetParentIndexAndNumChildren(contentNoLod)
            contentNoLod = Ymap.replaceName(contentNoLod, mapName)

            fileNoLod = open(os.path.join(self.getOutputDirMaps(False), filename), 'w')
            fileNoLod.write(contentNoLod)
            fileNoLod.close()

            pattern = re.compile('[\t ]*<Item type="CEntityDef">' +
                                 '\\s*<archetypeName>([^<]+)</archetypeName>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<position x="([^"]+)" y="([^"]+)" z="([^"]+)"\\s*/>' +
                                 '\\s*<rotation x="([^"]+)" y="([^"]+)" z="([^"]+)" w="([^"]+)"\\s*/>' +
                                 '\\s*<scaleXY value="([^"]+)"\\s*/>' +
                                 '\\s*<scaleZ value="([^"]+)"\\s*/>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*<lodDist value="([^"]+)"\\s*/>' +
                                 '(?:\\s*<[^/].*>)*?' +
                                 '\\s*</Item>[\r\n]+')

            for matchobj in re.finditer(pattern, contentNoLod):
                archetypeName = matchobj.group(1).lower()
                if archetypeName not in self.ytypItems or \
                        (self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE > 0 and archetypeName not in self.lodCandidates) or \
                        (self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE <= 0 and archetypeName not in self.slodCandidates):
                    continue

                archetype = self.ytypItems[archetypeName]
                position = [float(matchobj.group(2)), float(matchobj.group(3)), float(matchobj.group(4))]
                rotation = [float(matchobj.group(8)), -float(matchobj.group(5)), -float(matchobj.group(6)), -float(matchobj.group(7))]  # order is w, -x, -y, -z
                scale = [float(matchobj.group(9)), float(matchobj.group(9)), float(matchobj.group(10))]
                lodDistance = Util.calculateLodDistance(archetype.boundingBox, archetype.boundingSphere, scale, True)
                # Apply an absolute category-specific lodDist override (if provided).
                lodOverride = self._get_lod_distance_override(archetypeName)
                if lodOverride is not None:
                    lodDistance = lodOverride
                else:
                    # Legacy multiplier support (deprecated).
                    lodMultiplier = self._get_lod_multiplier(archetypeName)
                    lodDistance *= lodMultiplier
                entity = EntityItem(archetypeName, position, scale, rotation, lodDistance)

                hdEntitiesWithLod.append(entity)

                lodCoords.append(position)
                lodDistances.append(lodDistance)

        hierarchy = self.calculateLodHierarchy(lodCoords, lodDistances)

        # Visualization: show where HD entities with LOD are located for this prefix.
        if lodCoords:
            ax = PlotManager.get_axes("lod_map", "LOD map / reflection")
            pyplot.sca(ax)
            PlotManager.setup_world_background(ax)

            coords_np = np.array(lodCoords)[:, :2]
            distances_np = np.array(lodDistances)
            sc = pyplot.scatter(coords_np[:, 0], coords_np[:, 1], c=distances_np, s=10, edgecolors='none')
            PlotManager.autoscale_to_points(ax, coords_np)
            # Attach a per-tab colorbar that is only visible on the LOD map tab
            PlotManager.set_colorbar("lod_map", sc, "LOD distance")


        minLodDistances = [
            0,
            LodMapCreator.MIN_HD_LOD_DISTANCE_FOR_SLOD1,
            LodMapCreator.MIN_HD_LOD_DISTANCE_FOR_SLOD2,
            LodMapCreator.MIN_HD_LOD_DISTANCE_FOR_SLOD3,
            LodMapCreator.MIN_HD_LOD_DISTANCE_FOR_SLOD4,
        ]

        entitiesForReflModels = {}
        entitiesForLodModels = [{}, {}, {}, {}, {}]
        hierarchyMappingFromPreviousLevel = [{}, {}, {}, {}, {}]
        lodNumChildren = [{}, {}, {}, {}, {}]

        for i in range(len(hdEntitiesWithLod)):
            hdEntity = hdEntitiesWithLod[i]
            h = hierarchy[i]

            hierarchyMappingFromPreviousLevel[0][i] = h[0]
            lodNumChildren[0][h[0]] = lodNumChildren[0].get(h[0], 0) + 1
            if h[0] not in entitiesForLodModels[0]:
                entitiesForLodModels[0][h[0]] = []
            entitiesForLodModels[0][h[0]].append(hdEntity)

            for lodLevel in range(len(entitiesForLodModels)):
                if lodLevel == 0:
                    # for lod level 0 the HD entity must be used in currentHierarchy
                    currentHierarchy = i
                    nextLodLevel = 0
                elif lodLevel == 1:
                    # for lod level 1 the LOD entity must be used in currentHierarchy which is not in h[1] because
                    # hierarchy level 1 is only used to separate by lod distance but does not result in the LOD level itself
                    currentHierarchy = h[0]
                    nextLodLevel = lodLevel + 1

                    # add entity to reflection model
                    if h[nextLodLevel] not in entitiesForReflModels:
                        entitiesForReflModels[h[nextLodLevel]] = []
                    entitiesForReflModels[h[nextLodLevel]].append(hdEntity)
                else:
                    currentHierarchy = h[lodLevel]
                    nextLodLevel = lodLevel + 1

                if self.USE_SLOD_TEMPLATE_FOR_LEVEL_AND_ABOVE <= lodLevel and hdEntity.archetypeName not in self.slodCandidates:
                    continue

                if hdEntity.lodDistance < minLodDistances[lodLevel]:
                    continue

                if currentHierarchy not in hierarchyMappingFromPreviousLevel[lodLevel]:
                    hierarchyMappingFromPreviousLevel[lodLevel][currentHierarchy] = h[nextLodLevel]
                    lodNumChildren[lodLevel][h[nextLodLevel]] = lodNumChildren[lodLevel].get(h[nextLodLevel], 0) + 1

                if h[nextLodLevel] not in entitiesForLodModels[lodLevel]:
                    entitiesForLodModels[lodLevel][h[nextLodLevel]] = []
                entitiesForLodModels[lodLevel][h[nextLodLevel]].append(hdEntity)

        if self.SLOD3_DISTANCE == self.SLOD4_DISTANCE:
            entitiesForLodModels[4] = {}
            hierarchyMappingFromPreviousLevel[4] = {}
            lodNumChildren[4] = {}

        if self.createReflection:
            self.createReflLodMapsModels(entitiesForReflModels, mapPrefix.lower().rstrip("_"))

        numSlod1Entities = self.createLodSlodMapsModels(entitiesForLodModels, hierarchyMappingFromPreviousLevel, lodNumChildren, mapPrefix.lower().rstrip("_"))

        self.adaptHdMapsForPrefix(mapPrefix, hdEntitiesWithLod, hierarchyMappingFromPreviousLevel[0], numSlod1Entities)

    def createReflLodMapsModels(self, entitiesForReflLodModels: dict[int, list[EntityItem]], prefix: str):
        reflDrawableDictionary = prefix + "_refl_children"
        drawableDictionariesReflEntities = [[]]

        index = 0
        for key in sorted(entitiesForReflLodModels):
            reflName = prefix + "_refl_" + str(index)

            if len(drawableDictionariesReflEntities[-1]) >= LodMapCreator.MAX_NUM_CHILDREN_IN_DRAWABLE_DICTIONARY:
                drawableDictionariesReflEntities.append([])

            drawableDictionariesReflEntities[-1].append(self.createLodOrSlodModel(
                reflName, 1,
                reflDrawableDictionary + "_" + str(len(drawableDictionariesReflEntities) - 1),
                entitiesForReflLodModels[key],
                -1, 0,
                prefix, True
            ))

            index += 1

        for reflEntitiesIndex in range(len(drawableDictionariesReflEntities)):
            self.createDrawableDictionary(reflDrawableDictionary + "_" + str(reflEntitiesIndex), drawableDictionariesReflEntities[reflEntitiesIndex], True)

        reflEntities = []
        for drawableDictionaryReflEntities in drawableDictionariesReflEntities:
            reflEntities += drawableDictionaryReflEntities

        if len(reflEntities) > 0:
            mapName = prefix + "_refl"
            self.writeLodOrSlodMap(mapName, None, ContentFlag.SLOD | ContentFlag.SLOD2, reflEntities, True)

    def createLodSlodMapsModels(self, entitiesForLodModels: list[dict[int, list[EntityItem]]], hierarchyMappingFromPreviousLevel: list[dict[int, int]], lodNumChildren: list[dict[int, int]], prefix: str) -> int:
        lodDrawableDictionary = prefix + "_lod_children"
        slod1DrawableDictionary = prefix + "_slod1_children"
        slod2DrawableDictionary = prefix + "_slod2_children"
        slod3DrawableDictionary = prefix + "_slod3_children"
        slod4DrawableDictionary = prefix + "_slod4_children"

        lodEntities = [[]]
        slod1Entities = [[]]
        slod2Entities = []
        slod3Entities = []
        slod4Entities = []

        index = 0
        slod4KeyToIndex = {}
        for key in sorted(entitiesForLodModels[4]):
            slodName = prefix + "_" + str(index)
            slod4Entities.append(self.createLodOrSlodModel(
                slodName, 4,
                slod4DrawableDictionary,
                entitiesForLodModels[4][key],
                -1, lodNumChildren[4][key],
                prefix, False
            ))
            slod4KeyToIndex[key] = index
            index += 1

        index = 0
        slod3KeyToIndex = {}
        for key in sorted(entitiesForLodModels[3]):
            slodName = prefix + "_" + str(index)
            parentIndex = self.getParentIndexForKey(key, hierarchyMappingFromPreviousLevel[4], slod4KeyToIndex, 0)
            slod3Entities.append(self.createLodOrSlodModel(
                slodName, 3,
                slod3DrawableDictionary,
                entitiesForLodModels[3][key],
                parentIndex, lodNumChildren[3][key],
                prefix, False
            ))
            slod3KeyToIndex[key] = index
            index += 1

        index = 0
        slod2KeyToIndex = {}
        for key in sorted(entitiesForLodModels[2]):
            slodName = prefix + "_" + str(index)
            parentIndex = self.getParentIndexForKey(key, hierarchyMappingFromPreviousLevel[3], slod3KeyToIndex, 0)
            slod2Entities.append(self.createLodOrSlodModel(
                slodName, 2,
                slod2DrawableDictionary,
                entitiesForLodModels[2][key],
                parentIndex, lodNumChildren[2][key],
                prefix, False
            ))
            slod2KeyToIndex[key] = index
            index += 1

        index = 0
        slod1KeyToIndex = {}
        parentIndexOffset = len(slod3Entities)
        for key in sorted(entitiesForLodModels[1]):
            slodName = prefix + "_" + str(index)
            parentIndex = self.getParentIndexForKey(key, hierarchyMappingFromPreviousLevel[2], slod2KeyToIndex, parentIndexOffset)

            if len(slod1Entities[-1]) >= LodMapCreator.MAX_NUM_CHILDREN_IN_DRAWABLE_DICTIONARY:
                slod1Entities.append([])

            slod1Entities[-1].append(self.createLodOrSlodModel(
                slodName, 1,
                slod1DrawableDictionary + "_" + str(len(slod1Entities) - 1),
                entitiesForLodModels[1][key],
                parentIndex, lodNumChildren[1][key],
                prefix, False
            ))

            slod1KeyToIndex[key] = index
            index += 1

        for key in sorted(entitiesForLodModels[0]):
            lodName = prefix + "_" + str(key)
            parentIndex = self.getParentIndexForKey(key, hierarchyMappingFromPreviousLevel[1], slod1KeyToIndex, 0)

            if len(lodEntities[-1]) >= LodMapCreator.MAX_NUM_CHILDREN_IN_DRAWABLE_DICTIONARY:
                lodEntities.append([])

            lodEntities[-1].append(self.createLodOrSlodModel(
                lodName, 0,
                lodDrawableDictionary + "_" + str(len(lodEntities) - 1),
                entitiesForLodModels[0][key],
                parentIndex, lodNumChildren[0][key],
                prefix, False
            ))

        for lodEntitiesIndex in range(len(lodEntities)):
            self.createDrawableDictionary(lodDrawableDictionary + "_" + str(lodEntitiesIndex), lodEntities[lodEntitiesIndex], False)

        for slod1EntitiesIndex in range(len(slod1Entities)):
            self.createDrawableDictionary(slod1DrawableDictionary + "_" + str(slod1EntitiesIndex), slod1Entities[slod1EntitiesIndex], False)

        self.createDrawableDictionary(slod2DrawableDictionary, slod2Entities, False)
        self.createDrawableDictionary(slod3DrawableDictionary, slod3Entities, False)
        self.createDrawableDictionary(slod4DrawableDictionary, slod4Entities, False)

        if len(slod4Entities) == 0:
            slod4MapName = None
        else:
            slod4MapName = prefix + "_slod4"
            self.writeLodOrSlodMap(slod4MapName, None, ContentFlag.SLOD | ContentFlag.SLOD2, slod4Entities, False)

        if len(slod2Entities) == 0:
            slod2MapName = None
        else:
            slod2MapName = prefix + "_slod2"
            self.writeLodOrSlodMap(slod2MapName, slod4MapName, ContentFlag.SLOD | ContentFlag.SLOD2, slod3Entities + slod2Entities, False)

        slod1AndLodEntities = []
        numSlod1Entities = 0
        for slod1s in slod1Entities:
            slod1AndLodEntities += slod1s
            numSlod1Entities += len(slod1s)

        for lods in lodEntities:
            slod1AndLodEntities += lods

        if len(slod1AndLodEntities) > 0:
            lodMapName = prefix + "_lod"
            self.writeLodOrSlodMap(lodMapName, slod2MapName, ContentFlag.LOD | ContentFlag.SLOD, slod1AndLodEntities, False)

        return numSlod1Entities

    def getParentIndexForKey(self, key: int, keyToParentKey: dict[int, int], parentKeyToIndex: dict[int, int], parentIndexOffset: int):
        if key not in keyToParentKey:
            return -1
        else:
            return parentKeyToIndex[keyToParentKey[key]] + parentIndexOffset

    def adaptHdMapsForPrefix(self, mapPrefix: str, hdEntities: list[EntityItem], hdToLod: dict[int, int], offsetParentIndex: int):
        mutableIndex = [0]
        for filename in natsorted(os.listdir(self.inputDir)):
            if not filename.endswith(".ymap.xml") or not filename.startswith(mapPrefix.lower()):
                continue

            pathNoLod = os.path.join(self.getOutputDirMaps(False), filename)
            fileNoLod = open(pathNoLod, 'r')
            contentNoLod = fileNoLod.read()
            fileNoLod.close()
            os.remove(pathNoLod)

            # fix parentIndex in hd map to match lod map
            contentNoLod = re.sub('(\\s*<Item type="CEntityDef">' +
                                  '\\s*<archetypeName>([^<]+)</archetypeName>' +
                                  '(?:\\s*<[^/].*>)*?' +
                                  '\\s*<parentIndex value=")[^"]+("\\s*/>' +
                                  '(?:\\s*<[^/].*>)*?' +
                                  '\\s*<lodDist value=")([^"]+)("\\s*/>' +
                                  '(?:\\s*<[^/].*>)*?' +
                                  '\\s*</Item>)', lambda match: self.replParentIndexAndLodDistance(match, hdEntities, mutableIndex, hdToLod, offsetParentIndex), contentNoLod, flags=re.M)

            orphanHdEntities, hdEntitiesContent, contentBeforeEntities, contentAfterEntities = self.fixHdOrOrphanHdLodLevelsAndSplitAccordingly(contentNoLod)

            if hdEntitiesContent is None and orphanHdEntities is None:
                contentNoLod = Ymap.replaceParent(contentNoLod, None)
                fileHd = open(pathNoLod, 'w')
                fileHd.write(contentNoLod)
                fileHd.close()
                return

            if hdEntitiesContent is not None:
                mapNameLod = mapPrefix.lower().rstrip("_") + "_lod"
                contentHd = contentBeforeEntities + hdEntitiesContent + contentAfterEntities
                contentHd = Ymap.replaceParent(contentHd, mapNameLod)
                fileHd = open(pathNoLod, 'w')
                fileHd.write(contentHd)
                fileHd.close()

            if orphanHdEntities is not None:
                mapName = Util.getMapnameFromFilename(filename)
                mapNameStrm = Util.findAvailableMapName(self.getOutputDirMaps(False), mapName, "_strm", not self.clearLod)

                if hdEntitiesContent is None:
                    contentHd = contentBeforeEntities + orphanHdEntities + contentAfterEntities
                    contentHd = Ymap.replaceParent(contentHd, None)
                    fileHd = open(os.path.join(self.getOutputDirMaps(False), Util.getFilenameFromMapname(mapNameStrm)), 'w')
                    fileHd.write(contentHd)
                    fileHd.close()
                else:
                    self.writeStrmMap(mapNameStrm, orphanHdEntities)

    def writeLodOrSlodMap(self, mapName: str, parentMap: Optional[str], contentFlags: int, entities: list[EntityItem], reflection: bool):
        contentEntities = self.createEntitiesContent(entities)
        self.writeMap(mapName, parentMap, 2, contentFlags, contentEntities, reflection)

    def writeStrmMap(self, mapName: str, contentEntities: str):
        self.writeMap(mapName, None, 0, ContentFlag.HD, contentEntities, False)

    def writeMap(self, mapName: str, parentMap: Optional[str], flags: int, contentFlags: int, contentEntities: str, reflection: bool):
        content = self.contentTemplateSlod2Map \
            .replace("${NAME}", mapName) \
            .replace("${FLAGS}", str(flags)) \
            .replace("${CONTENT_FLAGS}", str(contentFlags)) \
            .replace("${TIMESTAMP}", Util.getNowInIsoFormat()) \
            .replace("${ENTITIES}\n", contentEntities)

        content = Ymap.replaceParent(content, parentMap)

        mapsDir = self.getOutputDirMaps(reflection)

        fileMap = open(os.path.join(mapsDir, Util.getFilenameFromMapname(mapName)), 'w')
        fileMap.write(content)
        fileMap.close()

    def createEntitiesContent(self, entities: list[EntityItem]):
        contentEntities = ""
        for entity in entities:
            contentEntities += self.contentTemplateEntitySlod \
                .replace("${POSITION.X}", Util.floatToStr(entity.position[0])) \
                .replace("${POSITION.Y}", Util.floatToStr(entity.position[1])) \
                .replace("${POSITION.Z}", Util.floatToStr(entity.position[2])) \
                .replace("${NAME}", entity.archetypeName) \
                .replace("${NUM_CHILDREN}", str(entity.numChildren)) \
                .replace("${PARENT_INDEX}", str(entity.parentIndex)) \
                .replace("${LOD_LEVEL}", entity.lodLevel) \
                .replace("${CHILD.LOD_DISTANCE}", Util.floatToStr(entity.childLodDist)) \
                .replace("${LOD_DISTANCE}", Util.floatToStr(entity.lodDistance)) \
                .replace("${FLAGS}", str(entity.flags))
        return contentEntities

    def calculateLodHierarchy(self, points: list[list[float]], lodDistances: list[float]) -> list[list[int]]:
        if len(points) == 0:
            return []

        hierarchy = []
        for i in range(len(points)):
            hierarchy.append([])

        return self._calculateLodHierarchy(points, lodDistances, hierarchy)

    def _calculateLodHierarchy(self, points: list[list[float]], lodDistances: list[float], hierarchy: list[list[int]]) -> list[list[int]]:
        numMaxChildren = -1
        clusterWithRespectToPosition = True
        level = len(hierarchy[0])
        if level == 0:
            maxExtends = LodMapCreator.ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD4
        elif level == 1:
            maxExtends = LodMapCreator.ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD3
        elif level == 2:
            maxExtends = LodMapCreator.ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD2
        elif level == 3:
            maxExtends = LodMapCreator.ENTITIES_EXTENTS_MAX_DIAGONAL_SLOD1
        elif level == 4:
            maxExtends = LodMapCreator.LOD_DISTANCES_MAX_DIFFERENCE_LOD
            clusterWithRespectToPosition = False
        elif level == 5:
            maxExtends = LodMapCreator.ENTITIES_EXTENTS_MAX_DIAGONAL_LOD
            numMaxChildren = LodMapCreator.NUM_CHILDREN_MAX_VALUE
        else:
            return hierarchy

        absIndices = []
        pointsOfParent = []
        for i in range(len(points)):
            parentIndex = 0 if len(hierarchy[i]) == 0 else hierarchy[i][0]

            while parentIndex >= len(pointsOfParent):
                absIndices.append([])
                pointsOfParent.append([])

            absIndices[parentIndex].append(i)
            if clusterWithRespectToPosition:
                sample = points[i]
            else:
                sample = [lodDistances[i]]
            pointsOfParent[parentIndex].append(sample)

        clusterOffset = 0
        for parentIndex in range(len(pointsOfParent)):
            clustering, unused = Util.performClustering(pointsOfParent[parentIndex], numMaxChildren, maxExtends)

            for c in range(len(clustering)):
                i = absIndices[parentIndex][c]
                hierarchy[i].insert(0, clustering[c] + clusterOffset)

            clusterOffset += max(clustering) + 1

        return self._calculateLodHierarchy(points, lodDistances, hierarchy)

    def copyOthers(self):
        # copy other files
        Util.copyFiles(self.inputDir, self.getOutputDirMaps(False),
            lambda filename: not filename.endswith(".ymap.xml") or not filename.startswith(tuple(each.lower() for each in self.bundlePrefixes)))

    # adapt extents and set current datetime
    def fixMapExtents(self, reflection: bool):
        print("\tfixing map extents")

        mapsDir = self.getOutputDirMaps(reflection)

        for filename in natsorted(os.listdir(mapsDir)):
            if not filename.endswith(".ymap.xml"):
                continue

            file = open(os.path.join(mapsDir, filename), 'r')
            content = file.read()
            file.close()

            content = Ymap.fixMapExtents(content, self.ytypItems)

            file = open(os.path.join(mapsDir, filename), 'w')
            file.write(content)
            file.close()

    def createManifest(self, reflection: bool) -> None:
        print("\tcreating manifest")
        mapsDir = self.getOutputDirMaps(reflection)
        metadataDir = self.getOutputDirMetadata(reflection)
        manifest = Manifest(self.ytypItems, mapsDir, metadataDir)
        manifest.parseYmaps()
        manifest.writeManifest()

    def addLodAndSlodModelsToYtypDict(self, reflection: bool) -> None:
        self.ytypItems |= YtypParser.readYtypDirectory(self.getOutputDirMetadata(reflection))

    def copyTextureDictionaries(self):
        texturesDir = os.path.join(os.path.dirname(__file__), "textures")
        root_dir = self._resolve_tool_root()

        def _pick_source(dictionary_name: str) -> str:
            # Allow user to drop a custom vegetation_lod.ytd / vegetation_slod.ytd into the tool root.
            override_path = os.path.join(root_dir, dictionary_name + ".ytd")
            if os.path.exists(override_path):
                return override_path
            return os.path.join(texturesDir, dictionary_name + ".ytd")

        if self.foundLod:
            shutil.copyfile(_pick_source(LodMapCreator.TEXTURE_DICTIONARY_LOD),
                            os.path.join(self.getOutputDirModels(False), LodMapCreator.TEXTURE_DICTIONARY_LOD + ".ytd"))
        if self.foundSlod:
            shutil.copyfile(_pick_source(LodMapCreator.TEXTURE_DICTIONARY_SLOD),
                            os.path.join(self.getOutputDirModels(False), LodMapCreator.TEXTURE_DICTIONARY_SLOD + ".ytd"))

    def runCustomSlodsOnly(self):
        """Create helper meshes only (for Custom SLODs), without generating full LOD maps."""
        print("running custom slod helper creator (custom slods only)...")

        self.readTemplates()
        self.createOutputDir()
        self.readYtypItems()
        
        base_dir = self.getOutputDirCustomSlods()

        # Generate helpers for slod1, slod2, slod3, and slod4 folders
        for i in range(1, 5):
            level_name = f"slod{i}"
            output_dir = os.path.join(base_dir, level_name)
            # We maintain "slod_" as the sampler prefix for all levels
            self.createCustomMeshesForArchetypes(self.customSlods, output_dir, "slod_")

        print("custom slod helper creator DONE")