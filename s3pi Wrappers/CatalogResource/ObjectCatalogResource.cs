/***************************************************************************
 *  Copyright (C) 2010 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using s3pi.Interfaces;
using System.Text;

namespace CatalogResource
{
    public class ObjectCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        MaterialList materialList = null;
        string instanceName = "";//Version>=0x16
        uint objkIndex;
        ObjectType objectTypeFlags;
        WallPlacement wallPlacementFlags;
        Movement movementFlags;
        uint unknown22;//Version>=0x1a
        uint cutoutTilesPerLevel;
        uint levels;
        MTDoorList mtDoorList = null;
        byte isScriptEnabled;
        uint diagonalIndex;
        uint ambienceTypeHash;
        RoomCategory roomCategoryFlags;
        FunctionCategory functionCategoryFlags;
        FunctionSubCategory functionSubCategoryFlags;
        RoomSubCategory roomSubCategoryFlags;
        BuildCategory buildCategoryFlags;
        uint surfaceCutoutDDSIndex;
        uint floorCutoutDDSIndex;//Version>=0x17
        uint floorCutoutLevelOffset;//Version>=0x17
        float floorCutoutBoundsLength;//Version>=0x17
        UIntList buildableShellDisplayStateHashes;//Version>=0x18
        uint levelBelowOBJDIndex;//Version>=0x19
        SlotPlacement slotPlacementFlags;
        string surfaceType = "";
        string sourceMaterial = "";
        Moodlet moodletGiven;
        int moodletScore;
        uint unknown21;
        TopicRating[] topicRatings = new TopicRating[5];
        uint fallbackIndex;
        #endregion

        #region Constructors
        public ObjectCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public ObjectCatalogResource(int APIversion, Stream unused, ObjectCatalogResource basis)
            : base(APIversion, basis.version, basis.common, basis.list)
        {
            this.instanceName = (this.version >= 0x00000016) ? basis.instanceName : null;
            this.materialList = new MaterialList(OnResourceChanged, basis.materialList);
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.objkIndex = basis.objkIndex;
            this.objectTypeFlags = basis.objectTypeFlags;
            this.wallPlacementFlags = basis.wallPlacementFlags;
            this.movementFlags = basis.movementFlags;
            this.cutoutTilesPerLevel = basis.cutoutTilesPerLevel;
            this.levels = basis.levels;
            this.mtDoorList = new MTDoorList(OnResourceChanged, basis.mtDoorList);
            this.isScriptEnabled = basis.isScriptEnabled;
            this.diagonalIndex = basis.diagonalIndex;
            this.ambienceTypeHash = basis.ambienceTypeHash;
            this.roomCategoryFlags = basis.roomCategoryFlags;
            this.functionCategoryFlags = basis.functionCategoryFlags;
            this.functionSubCategoryFlags = basis.functionSubCategoryFlags;
            this.roomSubCategoryFlags = basis.roomSubCategoryFlags;
            this.buildCategoryFlags = basis.buildCategoryFlags;
            this.surfaceCutoutDDSIndex = basis.surfaceCutoutDDSIndex;
            this.slotPlacementFlags = basis.slotPlacementFlags;
            this.surfaceType = basis.surfaceType;
            this.sourceMaterial = basis.sourceMaterial;
            this.moodletGiven = basis.moodletGiven;
            this.moodletScore = basis.moodletScore;
            this.unknown21 = basis.unknown21;
            this.topicRatings = (TopicRating[])basis.topicRatings.Clone();
            this.fallbackIndex = basis.fallbackIndex;
        }

        // Version <0x16
        public ObjectCatalogResource(int APIversion,
            uint version, IEnumerable<Material> materialList,
            Common common, uint objkIndex,
            ObjectType objectTypeFlags, WallPlacement wallPlacementFlags, Movement movementFlags, uint cutoutTilesPerLevel,
            uint levels, IEnumerable<MTDoor> mtDoorList, byte isScriptEnabled, uint diagonalIndex, uint hash,
            uint roomFlags, uint functionCategoryFlags, ulong subFunctionFlags, ulong subRoomFlags, uint buildCategoryFlags, uint sinkDDSIndex,
            uint slotPlacementFlags, string materialGrouping1, string materialGrouping2, Moodlet moodletGiven, int moodletScore, uint unknown21,
            TopicRating[] topicRatings, uint fallbackIndex, TGIBlockList ltgib)
            : this(APIversion,
            version, materialList,
            "",
            common, objkIndex,
            objectTypeFlags, wallPlacementFlags, movementFlags, cutoutTilesPerLevel,
            levels, mtDoorList, isScriptEnabled, diagonalIndex, hash,
            roomFlags, functionCategoryFlags, subFunctionFlags, subRoomFlags, buildCategoryFlags, sinkDDSIndex,
            0, 0, 0,
            new UIntList(null),
            0,
            slotPlacementFlags, materialGrouping1, materialGrouping2, moodletGiven, moodletScore, unknown21,
            topicRatings, fallbackIndex, ltgib)
        {
            if (checking) if (version >= 0x00000016)
                    throw new InvalidOperationException(String.Format("Constructor requires Unknown1 for version {0}", version));
        }
        // Version <0x17
        public ObjectCatalogResource(int APIversion,
            uint version, IEnumerable<Material> materialList,
            string unknown1,
            Common common, uint objkIndex,
            ObjectType objectTypeFlags, WallPlacement wallPlacementFlags, Movement movementFlags, uint cutoutTilesPerLevel,
            uint levels, IEnumerable<MTDoor> mtDoorList, byte isScriptEnabled, uint diagonalIndex, uint hash,
            uint roomFlags, uint functionCategoryFlags, ulong subFunctionFlags, ulong subRoomFlags, uint buildCategoryFlags, uint sinkDDSIndex,
            uint slotPlacementFlags, string materialGrouping1, string materialGrouping2, Moodlet moodletGiven, int moodletScore, uint unknown21,
            TopicRating[] topicRatings, uint fallbackIndex, TGIBlockList ltgib)
            : this(APIversion,
            version, materialList,
            unknown1,
            common, objkIndex,
            objectTypeFlags, wallPlacementFlags, movementFlags, cutoutTilesPerLevel,
            levels, mtDoorList, isScriptEnabled, diagonalIndex, hash,
            roomFlags, functionCategoryFlags, subFunctionFlags, subRoomFlags, buildCategoryFlags, sinkDDSIndex,
            0, 0, 0,
            new UIntList(null),
            0,
            slotPlacementFlags, materialGrouping1, materialGrouping2, moodletGiven, moodletScore, unknown21,
            topicRatings, fallbackIndex, ltgib)
        {
            if (checking) if (version >= 0x00000017)
                    throw new InvalidOperationException(String.Format("Constructor requires Unknown16, Unknown17 and Unknown18 for version {0}", version));
        }
        // Version <0x18
        public ObjectCatalogResource(int APIversion,
            uint version, IEnumerable<Material> materialList,
            string unknown1,
            Common common, uint objkIndex,
            ObjectType objectTypeFlags, WallPlacement wallPlacementFlags, Movement movementFlags, uint cutoutTilesPerLevel,
            uint levels, IEnumerable<MTDoor> mtDoorList, byte isScriptEnabled, uint diagonalIndex, uint hash,
            uint roomFlags, uint functionCategoryFlags, ulong subFunctionFlags, ulong subRoomFlags, uint buildCategoryFlags, uint sinkDDSIndex,
            uint unknown16, uint unknown17, float unknown18,
            uint slotPlacementFlags, string materialGrouping1, string materialGrouping2, Moodlet moodletGiven, int moodletScore, uint unknown21,
            TopicRating[] topicRatings, uint fallbackIndex, TGIBlockList ltgib)
            : this(APIversion,
            version, materialList,
            unknown1,
            common, objkIndex,
            objectTypeFlags, wallPlacementFlags, movementFlags, cutoutTilesPerLevel,
            levels, mtDoorList, isScriptEnabled, diagonalIndex, hash,
            roomFlags, functionCategoryFlags, subFunctionFlags, subRoomFlags, buildCategoryFlags, sinkDDSIndex,
            unknown16, unknown17, unknown18,
            new UIntList(null),
            0,
            slotPlacementFlags, materialGrouping1, materialGrouping2, moodletGiven, moodletScore, unknown21,
            topicRatings, fallbackIndex, ltgib)
        {
            if (checking) if (version >= 0x00000018)
                    throw new InvalidOperationException(String.Format("Constructor requires TGIIndexes for version {0}", version));
        }
        // Version <0x19
        public ObjectCatalogResource(int APIversion,
            uint version, IEnumerable<Material> materialList,
            string unknown1,
            Common common, uint objkIndex,
            ObjectType objectTypeFlags, WallPlacement wallPlacementFlags, Movement movementFlags, uint cutoutTilesPerLevel,
            uint levels, IEnumerable<MTDoor> mtDoorList, byte isScriptEnabled, uint diagonalIndex, uint hash,
            uint roomFlags, uint functionCategoryFlags, ulong subFunctionFlags, ulong subRoomFlags, uint buildCategoryFlags, uint sinkDDSIndex,
            uint unknown16, uint unknown17, float unknown18,
            UIntList tgiIndexes,
            uint slotPlacementFlags, string materialGrouping1, string materialGrouping2, Moodlet moodletGiven, int moodletScore, uint unknown21,
            TopicRating[] topicRatings, uint fallbackIndex, TGIBlockList ltgib)
            : this(APIversion,
            version, materialList,
            unknown1,
            common, objkIndex,
            objectTypeFlags, wallPlacementFlags, movementFlags, cutoutTilesPerLevel,
            levels, mtDoorList, isScriptEnabled, diagonalIndex, hash,
            roomFlags, functionCategoryFlags, subFunctionFlags, subRoomFlags, buildCategoryFlags, sinkDDSIndex,
            unknown16, unknown17, unknown18,
            tgiIndexes,
            0,
            slotPlacementFlags, materialGrouping1, materialGrouping2, moodletGiven, moodletScore, unknown21,
            topicRatings, fallbackIndex, ltgib)
        {
            if (checking) if (version >= 0x00000019)
                    throw new InvalidOperationException(String.Format("Constructor requires IndexV19 for version {0}", version));
        }
        public ObjectCatalogResource(int APIversion,
            uint version, IEnumerable<Material> materialList,
            string unknown1,
            Common common, uint objkIndex,
            ObjectType objectTypeFlags, WallPlacement wallPlacementFlags, Movement movementFlags, uint cutoutTilesPerLevel,
            uint levels, IEnumerable<MTDoor> mtDoorList, byte isScriptEnabled, uint diagonalIndex, uint hash,
            uint roomFlags, uint functionCategoryFlags, ulong subFunctionFlags, ulong subRoomFlags, uint buildCategoryFlags, uint sinkDDSIndex,
            uint unknown16, uint unknown17, float unknown18,
            UIntList tgiIndexes,
            uint indexV19,
            uint slotPlacementFlags, string materialGrouping1, string materialGrouping2, Moodlet moodletGiven, int moodletScore, uint unknown21,
            TopicRating[] topicRatings, uint fallbackIndex, TGIBlockList ltgib)
            : base(APIversion, version, common, ltgib)
        {
            this.materialList = new MaterialList(OnResourceChanged, materialList);
            this.instanceName = unknown1;
            this.common = new Common(requestedApiVersion, OnResourceChanged, common);
            this.objkIndex = objkIndex;
            this.objectTypeFlags = objectTypeFlags;
            this.wallPlacementFlags = wallPlacementFlags;
            this.movementFlags = movementFlags;
            this.cutoutTilesPerLevel = cutoutTilesPerLevel;
            this.levels = levels;
            this.mtDoorList = new MTDoorList(OnResourceChanged, mtDoorList);
            this.isScriptEnabled = isScriptEnabled;
            this.diagonalIndex = diagonalIndex;
            this.ambienceTypeHash = hash;
            this.roomCategoryFlags = (RoomCategory)roomFlags;
            this.functionCategoryFlags = (FunctionCategory)functionCategoryFlags;
            this.functionSubCategoryFlags = (FunctionSubCategory)subFunctionFlags;
            this.roomSubCategoryFlags = (RoomSubCategory)subRoomFlags;
            this.buildCategoryFlags = (BuildCategory)buildCategoryFlags;
            this.surfaceCutoutDDSIndex = sinkDDSIndex;
            this.floorCutoutDDSIndex = unknown16;
            this.floorCutoutLevelOffset = unknown17;
            this.floorCutoutBoundsLength = unknown18;
            this.buildableShellDisplayStateHashes = new UIntList(OnResourceChanged, tgiIndexes);
            this.levelBelowOBJDIndex = indexV19;
            this.slotPlacementFlags = (SlotPlacement)slotPlacementFlags;
            this.surfaceType = materialGrouping1;
            this.sourceMaterial = materialGrouping2;
            this.moodletGiven = moodletGiven;
            this.moodletScore = moodletScore;
            this.unknown21 = unknown21;
            if (checking) if (topicRatings.Length != 5)
                    throw new ArgumentLengthException("TopicRatings", this.topicRatings.Length);
            this.topicRatings = (TopicRating[])topicRatings.Clone();
            this.fallbackIndex = fallbackIndex;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            base.Parse(s);
            this.materialList = new MaterialList(OnResourceChanged, s);
            this.instanceName = (this.version >= 0x00000016) ? BigEndianUnicodeString.Read(s) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.objkIndex = r.ReadUInt32();
            this.objectTypeFlags = (ObjectType)r.ReadUInt32();
            this.wallPlacementFlags = (WallPlacement)r.ReadUInt32();
            this.movementFlags = (Movement)r.ReadUInt32();
            if (this.version >= 0x0000001a)
            {
                this.Unknown22 = r.ReadUInt32();
            }
            this.cutoutTilesPerLevel = r.ReadUInt32();
            this.levels = r.ReadUInt32();
            this.mtDoorList = new MTDoorList(OnResourceChanged, s);
            this.isScriptEnabled = r.ReadByte();
            this.diagonalIndex = r.ReadUInt32();
            this.ambienceTypeHash = r.ReadUInt32();
            this.roomCategoryFlags = (RoomCategory)r.ReadUInt32();
            this.functionCategoryFlags = (FunctionCategory)r.ReadUInt32();
            this.functionSubCategoryFlags = (FunctionSubCategory)r.ReadUInt64();
            this.roomSubCategoryFlags = (RoomSubCategory)r.ReadUInt64();
            this.buildCategoryFlags = (BuildCategory)r.ReadUInt32();
            this.surfaceCutoutDDSIndex = r.ReadUInt32();
            if (this.version >= 0x00000017)
            {
                this.floorCutoutDDSIndex = r.ReadUInt32();
                this.floorCutoutLevelOffset = r.ReadUInt32();
                this.floorCutoutBoundsLength = r.ReadSingle();
                if (this.version >= 0x00000018)
                {
                    buildableShellDisplayStateHashes = new UIntList(OnResourceChanged, s);
                    if (this.version >= 0x00000019)
                    {
                        levelBelowOBJDIndex = r.ReadUInt32();
                    }
                }
            }
            this.slotPlacementFlags = (SlotPlacement)r.ReadUInt32();
            this.surfaceType = BigEndianUnicodeString.Read(s);
            this.sourceMaterial = BigEndianUnicodeString.Read(s);
            this.moodletGiven = (Moodlet)r.ReadUInt32();
            this.moodletScore = r.ReadInt32();
            this.unknown21 = r.ReadUInt32();
            for (int i = 0; i < topicRatings.Length; i++)
                topicRatings[i] = new TopicRating(requestedApiVersion, OnResourceChanged, s);
            this.fallbackIndex = r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(ObjectCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);

            if (materialList == null) materialList = new MaterialList(OnResourceChanged);
            materialList.UnParse(s);
            if (this.version >= 0x00000016) BigEndianUnicodeString.Write(s, instanceName);
            if (common == null) common = new Common(requestedApiVersion, OnResourceChanged);
            common.UnParse(s);
            w.Write(objkIndex);
            w.Write((uint)objectTypeFlags);
            w.Write((uint)wallPlacementFlags);
            w.Write((uint)movementFlags);
            if (this.version >= 0x0000001a)
            {
                w.Write(unknown22);
            }
            w.Write(cutoutTilesPerLevel);
            w.Write(levels);
            if (mtDoorList == null) mtDoorList = new MTDoorList(OnResourceChanged);
            mtDoorList.UnParse(s);
            w.Write(isScriptEnabled);
            w.Write(diagonalIndex);
            w.Write(ambienceTypeHash);
            w.Write((uint)roomCategoryFlags);
            w.Write((uint)functionCategoryFlags);
            w.Write((ulong)functionSubCategoryFlags);
            w.Write((ulong)roomSubCategoryFlags);
            w.Write((uint)buildCategoryFlags);
            w.Write(surfaceCutoutDDSIndex);
            if (this.version >= 0x00000017)
            {
                w.Write(floorCutoutDDSIndex);
                w.Write(floorCutoutLevelOffset);
                w.Write(floorCutoutBoundsLength);
                if (this.version >= 0x00000018)
                {
                    if (buildableShellDisplayStateHashes == null) buildableShellDisplayStateHashes = new UIntList(OnResourceChanged);
                    buildableShellDisplayStateHashes.UnParse(s);
                    if (this.version >= 0x00000019)
                    {
                        w.Write(levelBelowOBJDIndex);
                    }
                }
            }
            w.Write((uint)slotPlacementFlags);
            BigEndianUnicodeString.Write(s, surfaceType);
            BigEndianUnicodeString.Write(s, sourceMaterial);
            w.Write((uint)moodletGiven);
            w.Write(moodletScore);
            w.Write(unknown21);
            for (int i = 0; i < topicRatings.Length; i++)
            {
                if (topicRatings[i] == null) topicRatings[i] = new TopicRating(0, OnResourceChanged);
                topicRatings[i].UnParse(s);
            }
            w.Write(fallbackIndex);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region AApiVersionedFields
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (this.version < 0x0000001a)
                {
                    res.Remove("Unknown22");
                    if (this.version < 0x00000019)
                    {
                        res.Remove("LevelBelowOBJDIndex");
                        if (this.version < 0x00000018)
                        {
                            res.Remove("BuildableShellDisplayStateHashes");
                            if (this.version < 0x00000017)
                            {
                                res.Remove("FloorCutoutDDSIndex");
                                res.Remove("FloorCutoutLevelOffset");
                                res.Remove("FloorCutoutBoundsLength");
                                if (this.version < 0x00000016)
                                    res.Remove("InstanceName");
                            }
                        }
                    }
                }
                return res;
            }
        }
        #endregion

        #region Sub-classes
        [Flags]
        public enum ObjectType : uint
        {
            Unknown00 = 0x00000001,
            AutomaticallyBuyAnotherAfterPlacing = 0x00000002,
            HidesFloorOnPlacement = 0x00000004,
            IsDoor = 0x00000008,

            IsWindow = 0x00000010,
            IsGate = 0x00000020,
            HideWhenWallDown = 0x00000040,
            RabbitHole = 0x00000080,

            IsDiagonal = 0x00000100,
            ForceToFullGrid = 0x00000200,
            RequireFloorAboveIfOutside = 0x00000400,
            IsFireplace = 0x00000800,

            IsChimney = 0x00001000,
            IsFlora = 0x00002000,
            IsColumn = 0x00004000,
            TakeParentAlongWhenPicked = 0x00008000,

            LiveDraggingEnabled = 0x00010000,
            AllowOnSlope = 0x00020000,
            LargeObject = 0x00040000,
            FloatsOnWater = 0x00080000,

            IsGarageDoor = 0x00100000,
            IsMailbox = 0x00200000,
            IgnorePatternSound = 0x00400000,
            IsRoadBridge = 0x00800000,

            AllowWallObjectOnGround = 0x01000000,
            HasFloorCutout = 0x02000000,
            BuildableShell = 0x04000000,
            ElevationFromCeiling = 0x08000000,

            CanDepressTerrain = 0x10000000,
            IgnorePlatformElevation = 0x20000000,
            CantBePlacedOnPlatform = 0x40000000,
            IsShellDoor = 0x80000000,
        }

        [Flags]
        public enum WallPlacement : uint
        {
            WallAtMinXEdge = 0x00000001,
            WallAtMinZEdge = 0x00000002,
            WallAtMaxXEdge = 0x00000004,
            WallAtMaxZEdge = 0x00000008,

            WallAt01To10Diag = 0x00000010,
            WallAt00To11Diag = 0x00000020,
            NoWallAtMinXEdge = 0x00000040,
            NoWallAtMinZEdge = 0x00000080,

            NoWallAtMaxXEdge = 0x00000100,
            NoWallAtMaxZEdge = 0x00000200,
            NoWallAt01To10Diag = 0x00000400,
            NoWallAt00To11Diag = 0x00000800,

            FlagsApplyToFences = 0x00001000,
            ProhibitsFenceArch = 0x00002000,
            OnWall = 0x00004000,
            IntersectsObjectsOffWall = 0x00008000,

            ApplyCutoutDiagonalShift = 0x00010000,
            CanBeMovedUpDownOnWall = 0x00020000,
            CannotBeMovedUpDownOnWall = 0x00040000,
            //Unknown13 = 0x00080000,

            //Unknown14 = 0x00100000,
            //Unknown15 = 0x00200000,
            //Unknown16 = 0x00400000,
            //Unknown17 = 0x00800000,

            //Unknown18 = 0x01000000,
            //Unknown19 = 0x02000000,
            //Unknown1A = 0x04000000,
            //Unknown1B = 0x08000000,

            //Unknown1C = 0x10000000,
            //Unknown1D = 0x20000000,
            //Unknown1E = 0x40000000,
            //Unknown1F = 0x80000000,
        }

        [Flags]
        public enum Movement : uint
        {
            Unknown00 = 0x00000001,
            StaysAfterEvict = 0x00000002,
            HandToolCannotMoveIt = 0x00000004,
            HandToolCannotDeleteIt = 0x00000008,

            HandToolCannotDuplicateIt = 0x00000010,
            HandToolCanDuplicateWhenHiddenInCatalog = 0x00000020,
            HandToolSkipRecursivePickupTests = 0x00000040,
            GhostsCannotFloatThrough = 0x00000080,

            //Unknown08 = 0x00000100,
        }

        [Flags]
        public enum RoomCategory : uint
        {
            Unused1 = 0x00000001,
            Living = 0x00000002,
            Dining = 0x00000004,
            Kitchen = 0x00000008,

            Nursery = 0x00000010,
            Bathroom = 0x00000020,
            Bedroom = 0x00000040,
            Study = 0x00000080,

            Outside = 0x00000100,
            Community = 0x00000200,
            Residential = 0x00000400,
            Pool = 0x00000800,

            Default = 0x80000000,

            //All         = 0xFFFFFFFF,
        }

        [Flags]
        public enum RoomSubCategory : ulong
        {
            //Low DWORD
            Unused1 = 0x00000001,
            Dishwashers = 0x00000002,
            SmallApps = 0x00000004,
            Fridges = 0x00000008,

            Trash = 0x00000010,
            Alarms = 0x00000020,
            Phones = 0x00000040,
            TVs = 0x00000080,

            SmokeAlarm = 0x00000100,
            Unused10 = 0x00000200,
            Audio = 0x00000400,
            Computers = 0x00000800,

            HobbiesSkills = 0x00001000,
            IndoorActivities = 0x00002000,
            LivingChairs = 0x00004000,
            OfficeChairs = 0x00008000,

            Stoves = 0x00010000,
            EatingOut = 0x00020000,
            OutdoorActivities = 0x00040000,
            CeilingLights = 0x00080000,

            FloorLamps = 0x00100000,
            TableLamps = 0x00200000,
            WallLamps = 0x00400000,
            OutdoorLights = 0x00800000,

            Showers = 0x01000000,
            Sinks = 0x02000000,
            Toilets = 0x04000000,
            Tubs = 0x08000000,

            Accents = 0x10000000,
            LawnDecor = 0x20000000,
            WallArtAdult = 0x40000000,
            Plants = 0x80000000,

            //High DWORD
            Mirrors = 0x0000000100000000,
            VideoGames = 0x0000000200000000,
            WallArtKids = 0x0000000400000000,
            Bookshelves = 0x0000000800000000,

            Cabinets = 0x0000001000000000,
            Dressers = 0x0000002000000000,
            DiningChairs = 0x0000004000000000,
            Sofas = 0x0000008000000000,

            OutdoorSeating = 0x0000010000000000,
            RoofDecorations = 0x0000020000000000,
            Beds = 0x0000040000000000,
            BarStools = 0x0000080000000000,

            CoffeeTables = 0x0000100000000000,
            Counters = 0x0000200000000000,
            Desks = 0x0000400000000000,
            EndTables = 0x0000800000000000,

            DiningTables = 0x0001000000000000,
            Furniture = 0x0002000000000000,
            Toys = 0x0004000000000000,
            Transport = 0x0008000000000000,

            Bars = 0x0010000000000000,
            Clocks = 0x0020000000000000,
            WindowDecor = 0x0040000000000000,
            KidsDecor = 0x0080000000000000,

            MiscDecor = 0x0100000000000000,
            Rugs = 0x0200000000000000,
            Laundry = 0x0400000000000000,
            Unused60 = 0x0800000000000000,

            Unused61 = 0x1000000000000000,
            Unused62 = 0x2000000000000000,
            Unused63 = 0x4000000000000000,
            Default = 0x8000000000000000,
        }

        [Flags]
        public enum FunctionCategory : uint
        {
            Unused1 = 0x00000001,
            Appliances = 0x00000002,
            Electronics = 0x00000004,
            Entertainment = 0x00000008,

            Unused5 = 0x00000010,
            Lighting = 0x00000020,
            Plumbing = 0x00000040,
            Decor = 0x00000080,

            Kids = 0x00000100,
            Storage = 0x00000200,
            Unused11 = 0x00000400,
            Comfort = 0x00000800,

            Surfaces = 0x00001000,
            Vehicles = 0x00002000,
            Unused15 = 0x00004000,
            Unused16 = 0x00008000,

            //
            //
            //
            //

            //
            //
            //
            //

            //
            //
            //
            //

            Unused29 = 0x10000000,
            Unused30 = 0x20000000,
            Debug = 0x40000000,
            Default = 0x80000000,
        }

        [Flags]
        public enum FunctionSubCategory : ulong
        {
            //Low DWORD
            Unused1 = 0x00000001,
            AppliancesMisc = 0x00000002,
            AppliancesSmall = 0x00000004,
            AppliancesLarge = 0x00000008,

            DebugTombObjects = 0x00000010,
            DebugFishSpawners = 0x00000020,
            DebugPlantSeedSpawners = 0x00000040,
            ElectronicsTVs = 0x00000080,

            ElectronicsMisc = 0x00000100,
            DebugRockGemMetalSpawners = 0x00000200,
            ElectronicsAudio = 0x00000400,
            ElectronicsComputers = 0x00000800,

            EntertainmentHobbiesSkills = 0x00001000,
            EntertainmentSports = 0x00002000,
            ComfortLivingChairs = 0x00004000,
            ComfortDeskChairs = 0x00008000,

            DebugInsectSpawners = 0x00010000,
            EntertainmentParties = 0x00020000,
            EntertainmentMisc = 0x00040000,
            LightingCeiling = 0x00080000,

            LightingFloor = 0x00100000,
            LightingTable = 0x00200000,
            LightingWall = 0x00400000,
            LightingOutdoor = 0x00800000,

            ComfortLoungeChairs = 0x01000000,
            PlumbingSinks = 0x02000000,
            PlumbingToilets = 0x04000000,
            PlumbingShowersAndTubs = 0x08000000,

            DecorMisc = 0x10000000,
            DecorSculptures = 0x20000000,
            DecorWallArt = 0x40000000,
            DecorPlants = 0x80000000,

            //High DWORD
            DecorMirrors = 0x0000000100000000,
            Unused34 = 0x0000000200000000,
            DebugMisc = 0x0000000400000000,
            StorageBookshelves = 0x0000000800000000,

            SurfacesDisplays = 0x0000001000000000,
            StorageDressers = 0x0000002000000000,
            ComfortDiningChairs = 0x0000004000000000,
            ComfortSofas = 0x0000008000000000,

            ComfortMisc = 0x0000010000000000,
            DecorRoof = 0x0000020000000000,
            ComfortBeds = 0x0000040000000000,
            Unused44 = 0x0000080000000000,

            SurfacesCoffeeTables = 0x0000100000000000,
            SurfacesCounters = 0x0000200000000000,
            SurfacesDesks = 0x0000400000000000,
            SurfacesEndTables = 0x0000800000000000,

            SurfacesDiningTables = 0x0001000000000000,
            KidsFurniture = 0x0002000000000000,
            KidsToys = 0x0004000000000000,
            VehiclesCars = 0x0008000000000000,

            VehiclesBicycles = 0x0010000000000000,
            SurfacesCabinets = 0x0020000000000000,
            DecorWindowDecor = 0x0040000000000000,
            KidsMisc = 0x0080000000000000,

            MiscLighting = 0x0100000000000000,
            MiscPlumbing = 0x0200000000000000,
            MiscStorage = 0x0400000000000000,
            MiscSurfaces = 0x0800000000000000,

            MiscVehicles = 0x1000000000000000,
            DecorRugs = 0x2000000000000000,
            Unused63 = 0x4000000000000000,
            Default = 0x8000000000000000,
        }

        [Flags]
        public enum BuildCategory : uint
        {
            Unused1 = 0x00000001,
            Door = 0x00000002,
            Window = 0x00000004,
            Gate = 0x00000008,

            Column = 0x00000010,
            RabbitHole = 0x00000020,
            Fireplace = 0x00000040,
            Chimney = 0x00000080,

            Arch = 0x00000100,
            Flower = 0x00000200,
            Shrub = 0x00000400,
            Tree = 0x00000800,

            Rug = 0x00001000,
            Rock = 0x00002000,
            Shell = 0x00004000,
            Landmark = 0x00008000,

            Elevator = 0x00010000,

            //
            //
            //
            //

            //
            //
            //
            //

            //
            //
            //
            //


            //
            //
            //
            Default = 0x80000000,
        }

        [Flags]
        public enum SlotPlacement : uint
        {
            //CheckFlags = 0xc3f38, 

            None = 0x01,
            //
            //
            Small = 0x08,

            Medium = 0x10,
            Large = 0x20,
            //
            //

            Sim = 0x0100,
            Chair = 0x0200,
            CounterSink = 0x0400,
            EndTable = 0x0800,

            Stool = 0x1000,
            CounterAppliance = 0x2000,
            //
            //

            //
            //
            Functional = 0x40000,
            Decorative = 0x80000,

            Upgrade = 0x1000000,
            //MatchFlags    = 0x2000000,
            Vertical = 0x2000000,
            PlacementOnly = 0x4000000,
            //

            //RotationFlags = 0x30000000,
            CardinalRotation = 0x10000000,
            FullRotation = 0x20000000,
            AlwaysUp = 0x40000000,
            //
        }

        public enum Moodlet : uint
        {
            Unused00 = 0x00000000,
            Seating,
            Sleeping,
            Music,
        }

        public enum TopicCategory : uint
        {
            EndOfTopics = 0x00000000,
            Environment,
            Hunger,
            Bladder,
            Energy,
            StressRelief,
            Fun,
            Hygiene,
            Logic,
            Charisma,
            Cooking,
            Athletic,
            Painting,
            Guitar,
            Handiness,
            GroupActivity,
            Upgradable,
            LearnCookingFaster,
            ChildOnly,
            unused13,
            Gardening,
            Fishing,
            SelfCleaning,
            NeverBreaks,
            Portable,
            Speed,
            Inventing,
            Sculpting,
            PlayPiano,
            PlayBass,
            PlayDrums,
        }

        public class TopicRating : AHandlerElement, IEquatable<TopicRating>, IComparable<TopicRating>
        {
            #region Attributes
            TopicCategory topic;
            int rating;
            #endregion

            #region Constructors
            public TopicRating(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public TopicRating(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public TopicRating(int APIversion, EventHandler handler, TopicRating basis)
                : this(APIversion, handler, basis.topic, basis.rating) { }
            public TopicRating(int APIversion, EventHandler handler, TopicCategory topic, int rating)
                : base(APIversion, handler)
            {
                this.topic = topic;
                this.rating = rating;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.topic = (TopicCategory)r.ReadUInt32();
                this.rating = r.ReadInt32();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)topic);
                w.Write(rating);
            }
            #endregion

            #region IComparable<TopicRating>
            public int CompareTo(TopicRating other)
            {
                int res = topic.CompareTo(other.topic); if (res != 0) return res;
                return rating.CompareTo(other.rating);
            }
            #endregion

            #region IEquatable<TopicRating> Members

            public bool Equals(TopicRating other) { return this.CompareTo(other) == 0; }

            public override bool Equals(object obj)
            {
                return obj as TopicRating != null ? this.Equals((TopicRating)obj) : false;
            }

            public override int GetHashCode()
            {
                return topic.GetHashCode() ^ rating.GetHashCode();
            }

            #endregion

            #region AHandlerElement
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override AHandlerElement Clone(EventHandler handler) { return new TopicRating(requestedApiVersion, handler, this); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public TopicCategory Topic { get { return topic; } set { if (topic != value) { topic = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public int Rating { get { return rating; } set { if (rating != value) { rating = value; OnElementChanged(); } } }

            public String Value
            {
                get
                {
                    if (topic == TopicCategory.EndOfTopics)
                        return "--- " + topic + " ---";
                    else
                    {
                        if (rating == 0) return "" + topic;
                        else
                        {
                            if (rating == 11) return "+ " + topic;
                            else return topic + ": " + rating;
                        }
                    }
                }
            }
            #endregion
        }

        public class MTDoor : AHandlerElement, IEquatable<MTDoor>
        {
            #region Attributes
            float[] unknown1 = new float[4];
            uint levelOffset;
            uint wallMaskIndex;
            #endregion

            #region Constructors
            public MTDoor(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public MTDoor(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public MTDoor(int APIversion, EventHandler handler, MTDoor basis)
                : this(APIversion, handler, basis.unknown1, basis.levelOffset, basis.wallMaskIndex) { }
            public MTDoor(int APIversion, EventHandler handler, float[] unknown1, uint unknownX, uint wallMaskIndex)
                : base(APIversion, handler)
            {
                if (unknown1.Length != this.unknown1.Length) throw new ArgumentLengthException("unknown1", this.unknown1.Length);
                this.unknown1 = (float[])unknown1.Clone();
                this.levelOffset = unknownX;
                this.wallMaskIndex = wallMaskIndex;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.unknown1 = new float[4]; for (int i = 0; i < unknown1.Length; i++) unknown1[i] = r.ReadSingle();
                this.levelOffset = r.ReadUInt32();
                this.wallMaskIndex = r.ReadUInt32();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                foreach (float f in unknown1) w.Write(f);
                w.Write(levelOffset);
                w.Write(wallMaskIndex);
            }
            #endregion

            #region IEquatable<MTDoorEntry> Members

            public bool Equals(MTDoor other)
            {
                for (int i = 0; i < unknown1.Length; i++) if (unknown1[i] != other.unknown1[i]) return false;
                return (levelOffset == other.levelOffset && wallMaskIndex == other.wallMaskIndex);
            }

            public override bool Equals(object obj)
            {
                return obj as MTDoor != null ? this.Equals((MTDoor)obj) : false;
            }

            public override int GetHashCode()
            {
                return unknown1.GetHashCode() ^ levelOffset.GetHashCode() ^ wallMaskIndex.GetHashCode();
            }

            #endregion

            #region AHandlerElement
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override AHandlerElement Clone(EventHandler handler) { return new MTDoor(requestedApiVersion, handler, this); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public float[] Unknown1
            {
                get { return (float[])unknown1.Clone(); }
                set
                {
                    if (value.Length != unknown1.Length) throw new ArgumentLengthException("unknown1", this.unknown1.Length);
                    if (!unknown1.Equals<float>(value)) { unknown1 = (float[])value.Clone(); OnElementChanged(); }
                }
            }
            [ElementPriority(2)]
            public uint LevelOffset { get { return levelOffset; } set { if (levelOffset != value) { levelOffset = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint WallMaskIndex { get { return wallMaskIndex; } set { if (wallMaskIndex != value) { wallMaskIndex = value; OnElementChanged(); } } }

            public String Value { get { return ValueBuilder; } }
            #endregion
        }

        public class MTDoorList : DependentList<MTDoor>
        {
            #region Constructors
            public MTDoorList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public MTDoorList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public MTDoorList(EventHandler handler, IEnumerable<MTDoor> mtDoorList) : base(handler, mtDoorList, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override MTDoor CreateElement(Stream s) { return new MTDoor(0, elementHandler, s); }
            protected override void WriteCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }
            protected override void WriteElement(Stream s, MTDoor element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new MTDoor(0, null)); }
        }
        #endregion

        #region Content Fields
        //--insert Version: ElementPriority(1)
        [ElementPriority(12)]
        public MaterialList Materials { get { return materialList; } set { if (materialList != value) { materialList = value == null ? null : new MaterialList(OnResourceChanged, value); } OnResourceChanged(this, new EventArgs()); } }
        [ElementPriority(13)]
        public string InstanceName
        {
            get { if (version < 0x00000016) throw new InvalidOperationException(); return instanceName; }
            set { if (version < 0x00000016) throw new InvalidOperationException(); if (instanceName != value) { instanceName = value; OnResourceChanged(this, new EventArgs()); } }
        }
        //--insert CommonBlock: ElementPriority(11)
        [ElementPriority(21), TGIBlockListContentField("TGIBlocks")]
        public uint OBJKIndex { get { return objkIndex; } set { if (objkIndex != value) { objkIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(22)]
        public ObjectType ObjectTypeFlags { get { return objectTypeFlags; } set { if (objectTypeFlags != value) { objectTypeFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(23)]
        public WallPlacement WallPlacementFlags { get { return wallPlacementFlags; } set { if (wallPlacementFlags != value) { wallPlacementFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(24)]
        public Movement MovementFlags { get { return movementFlags; } set { if (movementFlags != value) { movementFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(25)]
        public uint Unknown22 {
            get { if (version < 0x0000001a) throw new InvalidOperationException(); return unknown22; }
            set { if (version < 0x0000001a) throw new InvalidOperationException(); if (unknown22 != value) { unknown22 = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(26)]
        public uint CutoutTilesPerLevel { get { return cutoutTilesPerLevel; } set { if (cutoutTilesPerLevel != value) { cutoutTilesPerLevel = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(27)]
        public uint Levels { get { return levels; } set { if (levels != value) { levels = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28)]
        public MTDoorList MTDoors { get { return mtDoorList; } set { if (mtDoorList != value) { mtDoorList = value == null ? null : new MTDoorList(OnResourceChanged, value); } OnResourceChanged(this, new EventArgs()); } }
        [ElementPriority(29)]
        public bool IsScriptEnabled { get { return isScriptEnabled != 0; } set { if (IsScriptEnabled != value) { isScriptEnabled = (byte)(value ? 1 : 0); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30), TGIBlockListContentField("TGIBlocks")]
        public uint DiagonalIndex { get { return diagonalIndex; } set { if (diagonalIndex != value) { diagonalIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31)]
        public uint AmbienceTypeHash { get { return ambienceTypeHash; } set { if (ambienceTypeHash != value) { ambienceTypeHash = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32)]
        public RoomCategory RoomCategoryFlags { get { return roomCategoryFlags; } set { if (roomCategoryFlags != value) { roomCategoryFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(33)]
        public FunctionCategory FunctionCategoryFlags { get { return functionCategoryFlags; } set { if (functionCategoryFlags != value) { functionCategoryFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(34)]
        public FunctionSubCategory FunctionSubCategoryFlags { get { return functionSubCategoryFlags; } set { if (functionSubCategoryFlags != value) { functionSubCategoryFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(35)]
        public RoomSubCategory RoomSubCategoryFlags { get { return roomSubCategoryFlags; } set { if (roomSubCategoryFlags != value) { roomSubCategoryFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(36)]
        public BuildCategory BuildCategoryFlags { get { return buildCategoryFlags; } set { if (buildCategoryFlags != value) { buildCategoryFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(37), TGIBlockListContentField("TGIBlocks")]
        public uint SurfaceCutoutDDSIndex { get { return surfaceCutoutDDSIndex; } set { if (surfaceCutoutDDSIndex != value) { surfaceCutoutDDSIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(38), TGIBlockListContentField("TGIBlocks")]
        public uint FloorCutoutDDSIndex
        {
            get { if (version < 0x00000017) throw new InvalidOperationException(); return floorCutoutDDSIndex; }
            set { if (version < 0x00000017) throw new InvalidOperationException(); if (floorCutoutDDSIndex != value) { floorCutoutDDSIndex = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(39)]
        public uint FloorCutoutLevelOffset
        {
            get { if (version < 0x00000017) throw new InvalidOperationException(); return floorCutoutLevelOffset; }
            set { if (version < 0x00000017) throw new InvalidOperationException(); if (floorCutoutLevelOffset != value) { floorCutoutLevelOffset = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(40)]
        public float FloorCutoutBoundsLength
        {
            get { if (version < 0x00000017) throw new InvalidOperationException(); return floorCutoutBoundsLength; }
            set { if (version < 0x00000017) throw new InvalidOperationException(); if (floorCutoutBoundsLength != value) { floorCutoutBoundsLength = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(41)]
        public UIntList BuildableShellDisplayStateHashes
        {
            get { if (version < 0x00000018) throw new InvalidOperationException(); return buildableShellDisplayStateHashes; }
            set { if (version < 0x00000018) throw new InvalidOperationException(); if (buildableShellDisplayStateHashes != value) { buildableShellDisplayStateHashes = value == null ? null : new UIntList(OnResourceChanged, value); } OnResourceChanged(this, new EventArgs()); }
        }
        [ElementPriority(42), TGIBlockListContentField("TGIBlocks")]
        public uint LevelBelowOBJDIndex
        {
            get { if (version < 0x00000019) throw new InvalidOperationException(); return levelBelowOBJDIndex; }
            set { if (version < 0x00000019) throw new InvalidOperationException(); if (levelBelowOBJDIndex != value) { levelBelowOBJDIndex = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(43)]
        public SlotPlacement SlotPlacementFlags { get { return slotPlacementFlags; } set { if (slotPlacementFlags != value) { slotPlacementFlags = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(44)]
        public string SurfaceType { get { return surfaceType; } set { if (surfaceType != value) { surfaceType = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(45)]
        public string SourceMaterial { get { return sourceMaterial; } set { if (sourceMaterial != value) { sourceMaterial = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(46)]
        public Moodlet MoodletGiven { get { return moodletGiven; } set { if (moodletGiven != value) { moodletGiven = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(47)]
        public int MoodletScore { get { return moodletScore; } set { if (moodletScore != value) { moodletScore = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(48)]
        public uint Unknown21 { get { return unknown21; } set { if (unknown21 != value) { unknown21 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(49)]
        public TopicRating[] TopicRatings
        {
            get { return topicRatings; }
            set
            {
                if (value.Length != this.topicRatings.Length) throw new ArgumentLengthException("TopicRatings", this.topicRatings.Length);
                if (!topicRatings.Equals<TopicRating>(value)) { topicRatings = value == null ? null : (TopicRating[])value.Clone(); OnResourceChanged(this, new EventArgs()); }
            }
        }
        [ElementPriority(50), TGIBlockListContentField("TGIBlocks")]
        public uint FallbackIndex { get { return fallbackIndex; } set { if (fallbackIndex != value) { fallbackIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        //--insert TGIBlockList: no ElementPriority
        #endregion
    }
}
