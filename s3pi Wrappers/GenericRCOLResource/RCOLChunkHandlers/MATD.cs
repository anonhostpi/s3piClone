/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
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

namespace s3pi.GenericRCOLResource
{
    public class MATD : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint tag = (uint)FOURCC("MATD");
        uint version = 0x00000103;
        uint materialNameHash;
        ShaderType shader = 0;
        MTRL mtrl = null;
        uint unknown1;
        uint unknown2;
        MTNF mtnf = null;
        #endregion

        #region Constructors
        public MATD(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public MATD(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public MATD(int APIversion, EventHandler handler, MATD basis)
            : base(APIversion, handler, null)
        {
            this.version = basis.version;
            this.materialNameHash = basis.materialNameHash;
            this.shader = basis.shader;
            if (version < 0x00000103)
            {
                mtrl = basis.mtrl != null
                    ? new MTRL(requestedApiVersion, OnRCOLChanged, basis.mtrl)
                    : new MTRL(requestedApiVersion, OnRCOLChanged);
            }
            else
            {
                this.unknown1 = basis.unknown1;
                this.unknown2 = basis.unknown2;
                mtnf = basis.mtnf != null
                    ? new MTNF(requestedApiVersion, OnRCOLChanged, basis.mtnf)
                    : new MTNF(requestedApiVersion, OnRCOLChanged);
            }
        }

        public MATD(int APIversion, EventHandler handler,
            uint version, uint materialNameHash, ShaderType shader, MTRL mtrl)
            : base(APIversion, handler, null)
        {
            this.version = version;
            this.materialNameHash = materialNameHash;
            this.shader = shader;
            if (checking) if (version >= 0x00000103)
                    throw new ArgumentException("version must be < 0x0103 for MTRLs");
            this.mtrl = mtrl != null
                ? new MTRL(requestedApiVersion, OnRCOLChanged, mtrl)
                : new MTRL(requestedApiVersion, OnRCOLChanged);
        }

        public MATD(int APIversion, EventHandler handler,
            uint version, uint materialNameHash, ShaderType shader, uint unknown1, uint unknown2, MTNF mtnf)
            : base(APIversion, handler, null)
        {
            this.version = version;
            this.materialNameHash = materialNameHash;
            this.shader = shader;
            if (checking) if (version < 0x00000103)
                    throw new ArgumentException("version must be >= 0x0103 for MTNFs");
            this.unknown1 = unknown1;
            this.unknown2 = unknown2;
            this.mtnf = mtnf != null
                ? new MTNF(requestedApiVersion, OnRCOLChanged, mtnf)
                : new MTNF(requestedApiVersion, OnRCOLChanged);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "MATD"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x01D0E75D; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC("MATD"))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: 'MATD'; at 0x{1:X8}", FOURCC(tag), s.Position));
            version = r.ReadUInt32();
            materialNameHash = r.ReadUInt32();
            shader = (ShaderType)r.ReadUInt32();
            uint length = r.ReadUInt32();
            long start;
            if (version < 0x00000103)
            {
                start = s.Position;
                mtrl = new MTRL(requestedApiVersion, OnRCOLChanged, s);
            }
            else
            {
                unknown1 = r.ReadUInt32();
                unknown2 = r.ReadUInt32();
                start = s.Position;
                mtnf = new MTNF(requestedApiVersion, OnRCOLChanged, s);
            }

            if (checking) if (start + length != s.Position)
                    throw new InvalidDataException(string.Format("Invalid length 0x{0:X8} at 0x{1:X8}", length, s.Position));
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);
            w.Write(materialNameHash);
            w.Write((uint)shader);
            long lenPos = ms.Position;
            w.Write((uint)0);//length
            long pos;
            if (version < 0x00000103)
            {
                pos = ms.Position;
                if (mtrl == null) mtrl = new MTRL(requestedApiVersion, OnRCOLChanged);
                mtrl.UnParse(ms);
            }
            else
            {
                w.Write(unknown1);
                w.Write(unknown2);
                pos = ms.Position;
                if (mtnf == null) mtnf = new MTNF(requestedApiVersion, OnRCOLChanged);
                mtnf.UnParse(ms);
            }

            long endPos = ms.Position;
            ms.Position = lenPos;
            w.Write((uint)(endPos - pos));
            ms.Position = endPos;

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new MATD(requestedApiVersion, handler, this); }

        public override List<string> ContentFields
        {
            get
            {
                List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                if (version < 0x00000103)
                {
                    res.Remove("Unknown1");
                    res.Remove("Unknown2");
                    res.Remove("Mtnf");
                }
                else
                {
                    res.Remove("Mtrl");
                }
                return res;
            }
        }
        #endregion

        #region Sub-types
        public enum ShaderType : uint
        {
            None = 0,
            additive = 0x5af16731,
            BasinWater = 0x6aad2ad5,
            BrushedMetal = 0x3fd7990d,
            BurntTile = 0x690fdf06,
            CasSilk = 0x0072aa53,
            CasSimEyes = 0xb51ec997,
            CasSimHair = 0xfcf80ce1,
            CasSimHairSimple = 0xa7b368fb,
            CasSkin = 0x01772897,
            Counters = 0xa4172f62,
            DropShadow = 0xc09c7582,
            ExteriorCeiling = 0xd2ac4914,
            ExteriorWalls = 0xcd677552,
            Fence = 0x67107fe8,
            FlatMirror = 0xa68d9e29,
            Floors = 0xbc84d000,
            FloorsVisualizer = 0x2b1f3aec,
            Foliage = 0x4549e22e,
            FullBright = 0x14fa335e,
            Gemstones = 0xa063c1d0,
            Ghost = 0x2b1f3aec,
            GhostEyes = 0x8c88b4a8,
            GhostHair = 0x00c394a6,
            GlassForFences = 0x52986c62,
            GlassForObjects = 0x492eca7c,
            GlassForObjectsTranslucent = 0x849cf021,
            GlassForPortals = 0x81dd204d,
            GlassForRabbitHoles = 0x265ffaa1,
            ImpostorColorDefault = 0xed4fb30e,
            ImpostorColorGlow = 0x9661e300,
            ImposterLightingDefault = 0x5f03f969,
            ImpostorLightingGlow = 0x05954911,
            Instanced = 0x0cb82eb8,
            InstancedImpostorColor = 0xe7abde9c,
            LotImposter = 0x68601de3,
            LotPondWater = 0xe1386384,
            LotTerrain = 0x11d0b721,
            LotTerrainImposterMaker = 0xaee088f0,
            Occluder = 0x071fd3d4,
            OutdoorProp = 0x4d26bec0,
            Painting = 0xaa495821,
            Particle = 0x6da87a9b,
            ParticleAnim = 0x460e93f4,
            ParticleLight = 0xd9a8e549,
            phong_alpha = 0xfc5fc212,
            Phong = 0xb9105a6d,
            PickCASSim = 0x26d1704a,
            PickCounters = 0xce0c0dc1,
            PickDefault = 0x9017b045,
            PickInstanced = 0xb7178269,
            PickRug = 0x18120028,
            PickSim = 0x301464c3,
            PickTerrain = 0x0f49bea1,
            PickWater = 0xc107590f,
            PickWalls = 0xb81ad379,
            Plumbob = 0xdef16564,
            Ponds = 0x79c38597,
            PreviewWallsAndFloors = 0x213d6300,
            RabbitHoleHighDetail = 0x8d346bbc,
            RabbitHoleMediumDetail = 0xaede7105,
            Roads = 0x5e0ac22e,
            RoadsCompositor = 0x7c8b3791,
            Roofs = 0x4c0628aa,
            RoofImpostorLighting = 0xcb14114c,
            Rug = 0x2a72b9a1,
            ShadowMapMerged = 0xe2918799,
            SimEyes = 0xcf8a70b4,
            SimEyelashes = 0x9d9da161,
            SimHair = 0x84fd7152,
            SimHairVisualizer = 0x109defb6,
            SimSilk = 0x53881019,
            Simple = 0x723aa6e7,
            SimSkin = 0x548394b9,
            SimSkinThumbnail = 0x9eff872b,
            SimSkinVisualizer = 0x969921ad,
            Stairs = 0x4ce2f497,
            StandingWater = 0x70fde012,
            StaticTerrain = 0xe05b91aa,
            StaticTerrainLowLOD = 0x413d7051,
            Subtractive = 0x0b272cc5,
            TerrainLightFog = 0x69eb86e4,
            TerrainVisualization = 0xc589e244,
            TreeBillboard = 0xedd106f2,
            TreeShadowCompositor = 0x974fba48,
            ThumbnailShadowPlane = 0xd32eec7b,
            VertexColor = 0xb39101ac,
            Walls = 0x974fba48,
        }
        public enum FieldType : uint
        {
            None = 0,
            AlphaMap = 0xc3faac4f,
            AlphaMaskThreshold = 0xe77a2b60,
            Ambient = 0x04a5daa3,
            AmbientOcclusionMap = 0xb01cba60,
            AmbientUVSelector = 0x797f8e81,
            AnimDir = 0x3f89c2ef,
            AnimSpeed = 0xd600cb63,
            AutoRainbow = 0x5f7800ea,
            BackFaceDiffuseContribution = 0xd641a1b1,
            bHasDetailmap = 0xe9008abe,
            bHasNormalMap = 0x5e99ee74,
            bIsTerrainRoad = 0xa4a17516,
            BloomFactor = 0x4168508b,
            BounceAmountMeters = 0xd8542d8b,
            ContourSmoothing = 0x1e27dccd,
            CounterMatrixRow1 = 0x1ef8655d,
            CounterMatrixRow2 = 0x1ef8655e,
            CutoutValidHeights = 0x6d43d7b7,
            DetailMap = 0x9205daa8,
            DetailUVScale = 0xcd985a0b,
            Diffuse = 0x637daa05,
            DiffuseMap = 0x6cc0fd85,
            DiffuseUVScale = 0x2d4e507e,
            DiffuseUVSelector = 0x91eebaff,
            DimmingCenterHeight = 0x01adace0,
            DimmingRadius = 0x32dfa298,
            DirtOverlay = 0x48372e62,
            DropShadowAtlas = 0x22ad8507,
            DropShadowStrength = 0x1b1ab4d5,
            EdgeDarkening = 0x8c27d8c9,
            Emission = 0x3bd441a0,
            EmissionMap = 0xf303d152,
            EmissiveBloomMultiplier = 0x490e6eb4,
            EmissiveLightMultiplier = 0x8ef71c85,
            FadeDistance = 0x957210ea,
            FresnelOffset = 0xfb66a8cb,
            ImposterTexture = 0xbdcf71c5,
            ImposterTextureAOandSI = 0x15c9d298,
            ImposterTextureWater = 0xbf3fb9fa,
            ImpostorDetailTexture = 0x56e1c6b2,
            ImpostorWater = 0x277cf8eb,
            Layer2Shift = 0x92692cb2,
            LayerOffset = 0x80d9bfe1,
            LightMapScale = 0x4f7dcb9b,
            MaskHeight = 0x849cdadc,
            MaskWidth = 0x707f712f,
            NoiseMap = 0xe19fd579,
            NoiseMapScale = 0x5e86dea1,
            NormalMap = 0x6e56548a,
            NormalMapScale = 0x3c45e334,
            NormalMapUVSelector = 0x415368b4,
            NormalUVScale = 0xba2d1ab9,
            PosOffset = 0x790ebf2c,
            PosScale = 0x487648e5,
            Reflective = 0x73c9923e,
            RefractionDistortionScale = 0xc3c472a1,
            RevealMap = 0xf3f22ac4,
            RippleDistanceScale = 0xccb35b98,
            RippleHeights = 0x6a07d7e1,
            RippleSpeed = 0x52dec070,
            RoadDetailMap = 0x28392dc6,
            RoadNormalMap = 0xbca022bc,
            RoadTexture = 0x53521204,
            RoomLightMap = 0xe7ca9166,
            RotationSpeed = 0x32003ad4,
            RugSort = 0x906997a9,
            ShadowAlphaTest = 0xfeb1f9cb,
            Shininess = 0xf755f7ff,
            SparkleCube = 0x1d90c086,
            SparkleSpeed = 0xba13921e,
            SpecStyle = 0x9554d40f,
            Specular = 0x2ce11842,
            SpecularMap = 0xad528a60,
            SpecularUVScale = 0xf12e27c3,
            SpecularUVSelector = 0xb63546ac,
            TerrainLightMap = 0x5fd5b006,
            TextureSpeedScale = 0x583df357,
            Transparency = 0x05d22fd3,
            Transparent = 0x988403f9,
            UVOffset = 0x57582869,
            UVScale = 0x159ba53e,
            UVScales = 0x420520e9,
            UVScrollSpeed = 0xf2eea6ec,
            UVTiling = 0x773cab85,
            VertexColorScale = 0xa2fd73ca,
            WaterScrollSpeedLayer1 = 0xafa11436,
            WaterScrollSpeedLayer2 = 0xafa11435,
            WindSpeed = 0x66e9b6bc,
            WindStrength = 0xbc4a2544,
            Unknown1 = 0x209b1c8e,
            Unknown2 = 0xdaa9532d,
            Unknown3 = 0x29bcdd1f,
            Unknown4 = 0x2eb8e9d4,
            HaloRamp = 0x84f6e0fb,
            HaloBlur = 0xc3ad4f50,
            HaloHighColor = 0xd4043258,
        }
        public enum DataType : uint
        {
            dtUnknown = 0,
            dtFloat = 1,
            dtInt = 2,
            dtTexture = 4,
        }

        public class MTRL : AHandlerElement, IEquatable<MTRL>
        {
            const int recommendedApiVersion = 1;

            uint mtrlUnknown1;
            ushort mtrlUnknown2;
            ushort mtrlUnknown3;
            ShaderDataList sdList = null;

            public MTRL(int APIversion, EventHandler handler, MTRL basis)
                : base(APIversion, handler)
            {
                this.mtrlUnknown1 = basis.mtrlUnknown1;
                this.mtrlUnknown2 = basis.mtrlUnknown2;
                this.mtrlUnknown3 = basis.mtrlUnknown3;
                this.sdList = new ShaderDataList(handler, basis.sdList);
            }
            public MTRL(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public MTRL(int APIversion, EventHandler handler) : base(APIversion, handler) { }

            #region Data I/O
            private void Parse(Stream s)
            {
                long start = s.Position;
                BinaryReader r = new BinaryReader(s);
                uint mtrlTag = r.ReadUInt32();
                if (checking) if (mtrlTag != (uint)FOURCC("MTRL"))
                        throw new InvalidDataException(String.Format("Invalid mtrlTag read: '{0}'; expected: 'MTRL'; at 0x{1:X8}", FOURCC(mtrlTag), s.Position));
                mtrlUnknown1 = r.ReadUInt32();
                mtrlUnknown2 = r.ReadUInt16();
                mtrlUnknown3 = r.ReadUInt16();
                this.sdList = new ShaderDataList(handler, s, start, -1);
            }

            internal void UnParse(Stream s)
            {
                long start = s.Position;
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)FOURCC("MTRL"));
                w.Write(mtrlUnknown1);
                w.Write(mtrlUnknown2);
                w.Write(mtrlUnknown3);
                if (sdList == null) sdList = new ShaderDataList(handler);
                sdList.UnParse(s, start);
            }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new MTRL(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<MTRL> Members

            public bool Equals(MTRL other) { return mtrlUnknown1 == other.mtrlUnknown1 && mtrlUnknown2 == other.mtrlUnknown2 && mtrlUnknown3 == other.mtrlUnknown3 && sdList == other.sdList; }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint MTRLUnknown1 { get { return mtrlUnknown1; } set { if (mtrlUnknown1 != value) { mtrlUnknown1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort MTRLUnknown2 { get { return mtrlUnknown2; } set { if (mtrlUnknown2 != value) { mtrlUnknown2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ushort MTRLUnknown3 { get { return mtrlUnknown3; } set { if (mtrlUnknown3 != value) { mtrlUnknown3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public ShaderDataList SData { get { return sdList; } set { if (sdList != value) { sdList = new ShaderDataList(handler, value); OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    s += "MTRLUnknown1: 0x" + mtrlUnknown1.ToString("X8");
                    s += "\nMTRLUnknown2: 0x" + mtrlUnknown2.ToString("X4");
                    s += "\nMTRLUnknown3: 0x" + mtrlUnknown3.ToString("X4");

                    s += String.Format("\nSData ({0:X}):", sdList.Count);
                    string fmt = "\n  [{0:X" + sdList.Count.ToString("X").Length + "}]: {{{1}}}";
                    for (int i = 0; i < sdList.Count; i++)
                        s += String.Format(fmt, i, sdList[i].Value);
                    return s;
                    /**/
                }
            }
            #endregion
        }
        public class MTNF : AHandlerElement, IEquatable<MTNF>
        {
            const int recommendedApiVersion = 1;

            uint mtnfUnknown1;
            ShaderDataList sdList = null;

            public MTNF(int APIversion, EventHandler handler, MTNF basis)
                : base(APIversion, handler)
            {
                this.mtnfUnknown1 = basis.mtnfUnknown1;
                this.sdList = new ShaderDataList(handler, basis.sdList);
            }
            public MTNF(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public MTNF(int APIversion, EventHandler handler) : base(APIversion, handler) { }

            #region Data I/O
            private void Parse(Stream s)
            {
                long start = s.Position;
                BinaryReader r = new BinaryReader(s);
                uint mtnfTag = r.ReadUInt32();
                if (checking) if (mtnfTag != (uint)FOURCC("MTNF"))
                        throw new InvalidDataException(String.Format("Invalid mtnfTag read: '{0}'; expected: 'MTNF'; at 0x{1:X8}", FOURCC(mtnfTag), s.Position));
                mtnfUnknown1 = r.ReadUInt32();
                this.sdList = new ShaderDataList(handler, s, start, r.ReadInt32());
            }

            internal void UnParse(Stream s)
            {
                long start = s.Position;
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)FOURCC("MTNF"));
                w.Write(mtnfUnknown1);
                long dlPos = s.Position;
                w.Write((uint)0);//data length
                if (sdList == null) sdList = new ShaderDataList(handler);
                sdList.UnParse(s, start);

                long dlEnd = s.Position;
                s.Position = dlPos;
                w.Write((uint)(dlEnd - sdList.dataPos));
                s.Position = dlEnd;
            }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new MTNF(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<MTNF> Members

            public bool Equals(MTNF other) { return mtnfUnknown1 == other.mtnfUnknown1 && sdList == other.sdList; }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint MTNFUnknown1 { get { return mtnfUnknown1; } set { if (mtnfUnknown1 != value) { mtnfUnknown1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ShaderDataList SData { get { return sdList; } set { if (sdList != value) { sdList = new ShaderDataList(handler, value); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public abstract class ShaderData : AHandlerElement, IEquatable<ShaderData>
        {
            const int recommendedApiVersion = 1;

            protected FieldType field;
            long offsetPos = -1;

            #region Constructors
            protected ShaderData(int APIversion, EventHandler handler, FieldType field) : base(APIversion, handler) { this.field = field; }
            #endregion

            public static ShaderData CreateEntry(int APIversion, EventHandler handler, Stream s, long start)
            {
                BinaryReader r = new BinaryReader(s);
                FieldType field = (FieldType)r.ReadUInt32();
                DataType sdType = (DataType)r.ReadUInt32();
                int count = r.ReadInt32();
                uint offset = r.ReadUInt32();
                long pos = s.Position;
                s.Position = start + offset;
                try
                {
                    #region Determine entry type
                    switch (sdType)
                    {
                        case DataType.dtFloat:
                            switch (count)
                            {
                                case 1: return new ElementFloat(APIversion, handler, field, s);
                                case 2: return new ElementFloat2(APIversion, handler, field, s);
                                case 3: return new ElementFloat3(APIversion, handler, field, s);
                                case 4: return new ElementFloat4(APIversion, handler, field, s);
                            }
                            throw new InvalidDataException(String.Format("Invalid count #{0}' for DataType 0x{1:X8} at 0x{2:X8}", count, sdType, s.Position));
                        case DataType.dtInt:
                            switch (count)
                            {
                                case 1: return new ElementInt(APIversion, handler, field, s);
                            }
                            throw new InvalidDataException(String.Format("Invalid count #{0}' for DataType 0x{1:X8} at 0x{2:X8}", count, sdType, s.Position));
                        case DataType.dtTexture:
                            switch (count)
                            {
                                case 4: return new ElementTextureRef(APIversion, handler, field, s);
                                case 5: return new ElementTextureKey(APIversion, handler, field, s);
                            }
                            throw new InvalidDataException(String.Format("Invalid count #{0}' for DataType 0x{1:X8} at 0x{2:X8}", count, sdType, s.Position));
                    }
                    throw new InvalidDataException(String.Format("Unknown DataType 0x{0:X8} at 0x{1:X8}", sdType, s.Position));
                    #endregion
                }
                finally { s.Position = pos; }
            }
            public static ShaderData CreateEntry(int APIversion, EventHandler handler, ShaderData basis)
            {
                if (basis is ElementFloat) return new ElementFloat(APIversion, handler, basis as ElementFloat);
                if (basis is ElementFloat2) return new ElementFloat2(APIversion, handler, basis as ElementFloat2);
                if (basis is ElementFloat3) return new ElementFloat3(APIversion, handler, basis as ElementFloat3);
                if (basis is ElementFloat4) return new ElementFloat4(APIversion, handler, basis as ElementFloat4);
                if (basis is ElementInt) return new ElementInt(APIversion, handler, basis as ElementInt);
                if (basis is ElementTextureRef) return new ElementTextureRef(APIversion, handler, basis as ElementTextureRef);
                if (basis is ElementTextureKey) return new ElementTextureKey(APIversion, handler, basis as ElementTextureKey);
                throw new ArgumentException("Unknown element type.");
            }
            public static Type GetElementType(params object[] fields)
            {
                Type[] types = new Type[2 + fields.Length];
                types[0] = typeof(int);
                types[1] = typeof(EventHandler);
                for (int i = 0; i < types.Length; i++) types[i] = fields[2 + i].GetType();
                if (typeof(ElementFloat).GetConstructor(types) != null) return typeof(ElementFloat);
                if (typeof(ElementFloat2).GetConstructor(types) != null) return typeof(ElementFloat2);
                if (typeof(ElementFloat3).GetConstructor(types) != null) return typeof(ElementFloat3);
                if (typeof(ElementFloat4).GetConstructor(types) != null) return typeof(ElementFloat4);
                if (typeof(ElementInt).GetConstructor(types) != null) return typeof(ElementInt);
                if (typeof(ElementTextureRef).GetConstructor(types) != null) return typeof(ElementTextureRef);
                if (typeof(ElementTextureKey).GetConstructor(types) != null) return typeof(ElementTextureKey);
                return null;
            }

            #region Data I/O
            internal void UnParseHeader(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)field);
                w.Write((uint)DataTypeFromType);
                w.Write(CountFromType);
                offsetPos = s.Position;
                w.Write((uint)0);
            }

            internal void UnParseData(Stream s, long start)
            {
                if (checking) if (offsetPos < 0)
                        throw new InvalidOperationException();
                long pos = s.Position;
                s.Position = offsetPos;
                new BinaryWriter(s).Write((uint)(pos - start));
                s.Position = pos;
                UnParse(s);
            }

            protected abstract DataType DataTypeFromType { get; }
            protected abstract int CountFromType { get; }
            protected abstract void UnParse(Stream s);

            protected void ReadZeros(Stream s, int length) { while (length-- > 0) if (s.ReadByte() != 0) throw new InvalidDataException("Non-zero padding at 0x" + s.Position.ToString("X8")); }
            protected void WriteZeros(Stream s, int length) { while (length-- > 0) s.WriteByte(0); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Entry> Members

            public abstract bool Equals(ShaderData other);

            #endregion

            [ElementPriority(1)]
            public FieldType Field { get { return field; } set { if (field != value) { field = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder.Replace("\n", "; "); } }
        }
        [ConstructorParameters(new object[] { (FieldType)0, 0f, })]
        public class ElementFloat : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Single data;
            #endregion

            #region Constructors
            public ElementFloat(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementFloat(int APIversion, EventHandler handler, ElementFloat basis) : this(APIversion, handler, basis.field, basis.data) { }
            public ElementFloat(int APIversion, EventHandler handler, FieldType field, Single data) : base(APIversion, handler, field) { this.data = data; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { data = new BinaryReader(s).ReadSingle(); }

            protected override void UnParse(Stream s) { new BinaryWriter(s).Write(data); }
            protected override DataType DataTypeFromType { get { return DataType.dtFloat; } }
            protected override int CountFromType { get { return 1; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementFloat(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType()) && this.data == ((ElementFloat)other).data; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Single Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, 0f, 0f, })]
        public class ElementFloat2 : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Single data0;
            Single data1;
            #endregion

            #region Constructors
            public ElementFloat2(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementFloat2(int APIversion, EventHandler handler, ElementFloat2 basis) : this(APIversion, handler, basis.field, basis.data0, basis.data1) { }
            public ElementFloat2(int APIversion, EventHandler handler, FieldType field, Single data0, Single data1) : base(APIversion, handler, field) { this.data0 = data0; this.data1 = data1; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data0 = r.ReadSingle(); data1 = r.ReadSingle(); }

            protected override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data0); w.Write(data1); }
            protected override DataType DataTypeFromType { get { return DataType.dtFloat; } }
            protected override int CountFromType { get { return 2; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementFloat2(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType())
                && this.data0 == ((ElementFloat2)other).data0
                && this.data1 == ((ElementFloat2)other).data1
                ; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Single Data0 { get { return data0; } set { if (data0 != value) { data0 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public Single Data1 { get { return data1; } set { if (data1 != value) { data1 = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, 0f, 0f, 0f, })]
        public class ElementFloat3 : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Single data0;
            Single data1;
            Single data2;
            #endregion

            #region Constructors
            public ElementFloat3(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementFloat3(int APIversion, EventHandler handler, ElementFloat3 basis) : this(APIversion, handler, basis.field, basis.data0, basis.data1, basis.data2) { }
            public ElementFloat3(int APIversion, EventHandler handler, FieldType field, Single data0, Single data1, Single data2) : base(APIversion, handler, field) { this.data0 = data0; this.data1 = data1; this.data2 = data2; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data0 = r.ReadSingle(); data1 = r.ReadSingle(); data2 = r.ReadSingle(); }

            protected override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data0); w.Write(data1); w.Write(data2); }
            protected override DataType DataTypeFromType { get { return DataType.dtFloat; } }
            protected override int CountFromType { get { return 3; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementFloat3(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType())
                && this.data0 == ((ElementFloat3)other).data0
                && this.data1 == ((ElementFloat3)other).data1
                && this.data2 == ((ElementFloat3)other).data2
                ; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Single Data0 { get { return data0; } set { if (data0 != value) { data0 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public Single Data1 { get { return data1; } set { if (data1 != value) { data1 = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public Single Data2 { get { return data2; } set { if (data2 != value) { data2 = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, 0f, 0f, 0f, 0f, })]
        public class ElementFloat4 : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Single data0;
            Single data1;
            Single data2;
            Single data3;
            #endregion

            #region Constructors
            public ElementFloat4(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementFloat4(int APIversion, EventHandler handler, ElementFloat4 basis) : this(APIversion, handler, basis.field, basis.data0, basis.data1, basis.data2, basis.data3) { }
            public ElementFloat4(int APIversion, EventHandler handler, FieldType field, Single data0, Single data1, Single data2, Single data3) : base(APIversion, handler, field) { this.data0 = data0; this.data1 = data1; this.data2 = data2; this.data3 = data3; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data0 = r.ReadSingle(); data1 = r.ReadSingle(); data2 = r.ReadSingle(); data3 = r.ReadSingle(); }

            protected override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data0); w.Write(data1); w.Write(data2); w.Write(data3); }
            protected override DataType DataTypeFromType { get { return DataType.dtFloat; } }
            protected override int CountFromType { get { return 4; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementFloat4(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType())
                && this.data0 == ((ElementFloat4)other).data0
                && this.data1 == ((ElementFloat4)other).data1
                && this.data2 == ((ElementFloat4)other).data2
                && this.data3 == ((ElementFloat4)other).data3
                ; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Single Data0 { get { return data0; } set { if (data0 != value) { data0 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public Single Data1 { get { return data1; } set { if (data1 != value) { data1 = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public Single Data2 { get { return data2; } set { if (data2 != value) { data2 = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public Single Data3 { get { return data3; } set { if (data3 != value) { data3 = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, (int)0, })]
        public class ElementInt : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Int32 data;
            #endregion

            #region Constructors
            public ElementInt(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementInt(int APIversion, EventHandler handler, ElementInt basis) : this(APIversion, handler, basis.field, basis.data) { }
            public ElementInt(int APIversion, EventHandler handler, FieldType field, Int32 data) : base(APIversion, handler, field) { this.data = data; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { data = new BinaryReader(s).ReadInt32(); }

            protected override void UnParse(Stream s) { new BinaryWriter(s).Write(data); }
            protected override DataType DataTypeFromType { get { return DataType.dtInt; } }
            protected override int CountFromType { get { return 1; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementInt(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType()) && this.data == ((ElementInt)other).data; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Int32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, (GenericRCOLResource.ChunkReference)null, })]
        public class ElementTextureRef : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            GenericRCOLResource.ChunkReference data;
            #endregion

            #region Constructors
            public ElementTextureRef(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementTextureRef(int APIversion, EventHandler handler, ElementTextureRef basis) : this(APIversion, handler, basis.field, basis.data) { }
            public ElementTextureRef(int APIversion, EventHandler handler, FieldType field, GenericRCOLResource.ChunkReference data) : base(APIversion, handler, field) { this.data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, data); }
            #endregion

            #region Data I/O
            void Parse(Stream s) { data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s); ReadZeros(s, 12); }

            protected override void UnParse(Stream s) { if (data == null) data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0); data.UnParse(s); WriteZeros(s, 12); }
            protected override DataType DataTypeFromType { get { return DataType.dtTexture; } }
            protected override int CountFromType { get { return 4; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementTextureRef(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType()) && this.data == ((ElementTextureRef)other).data; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public GenericRCOLResource.ChunkReference Data { get { return data; } set { if (data != value) { data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, (IResourceKey)null, })]
        public class ElementTextureKey : ShaderData
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            TGIBlock data;
            #endregion

            #region Constructors
            public ElementTextureKey(int APIversion, EventHandler handler, FieldType field, Stream s) : base(APIversion, handler, field) { Parse(s); }
            public ElementTextureKey(int APIversion, EventHandler handler, ElementTextureKey basis) : this(APIversion, handler, basis.field, basis.data) { }
            public ElementTextureKey(int APIversion, EventHandler handler, FieldType field, IResourceKey data) : base(APIversion, handler, field) { this.data = new TGIBlock(requestedApiVersion, handler, data); }
            #endregion

            #region Data I/O
            void Parse(Stream s) { data = new TGIBlock(requestedApiVersion, handler, s); ReadZeros(s, 4); }

            protected override void UnParse(Stream s) { if (data == null) data = new TGIBlock(requestedApiVersion, handler, 0); data.UnParse(s); WriteZeros(s, 4); }
            protected override DataType DataTypeFromType { get { return DataType.dtTexture; } }
            protected override int CountFromType { get { return 5; } }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new ElementTextureKey(requestedApiVersion, handler, this); }

            #region IEquatable<Entry> Members

            public override bool Equals(ShaderData other) { return this.GetType().Equals(other.GetType()) && this.data == ((ElementTextureKey)other).data; }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public IResourceKey Data { get { return data; } set { if (data != value) { data = new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class ShaderDataList : DependentList<ShaderData>
        {
            internal long dataPos = -1;
            #region Constructors
            public ShaderDataList(EventHandler handler) : base(handler) { }
            public ShaderDataList(EventHandler handler, Stream s, long start, int dataLen) : base(null) { elementHandler = handler; Parse(s, start, dataLen); this.handler = handler; }
            public ShaderDataList(EventHandler handler, IEnumerable<ShaderData> lsd) : base(handler, lsd) { }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { throw new NotSupportedException(); }
            internal void Parse(Stream s, long start, int dataLen)
            {
                for (int i = ReadCount(s); i > 0; i--) this.Add(ShaderData.CreateEntry(0, elementHandler, s, start));
                s.Position += dataLen;
            }
            public override void UnParse(Stream s) { throw new NotSupportedException(); }
            internal void UnParse(Stream s, long start)
            {
                WriteCount(s, Count);
                foreach (var element in this) element.UnParseHeader(s);
                dataPos = s.Position;
                foreach (var element in this) element.UnParseData(s, start);
            }

            protected override ShaderData CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, ShaderData element) { throw new NotImplementedException(); }
            #endregion

            public override void Add() { throw new NotSupportedException(); }
            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(ShaderData).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();
                return ShaderData.GetElementType(fields);
            }
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint MaterialNameHash { get { return materialNameHash; } set { if (materialNameHash != value) { materialNameHash = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ShaderType Shader { get { return shader; } set { if (shader != value) { shader = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public MTRL Mtrl { get { return mtrl; } set { if (mtrl != value) { mtrl = new MTRL(requestedApiVersion, handler, mtrl); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public MTNF Mtnf { get { return mtnf; } set { if (mtnf != value) { mtnf = new MTNF(requestedApiVersion, handler, mtnf); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }
}
