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
            None = 0x00000000,
            Subtractive = 0x0B272CC5,
            Instanced = 0x0CB82EB8,
            FullBright = 0x14FA335E,
            PreviewWallsAndFloors = 0x213D6300,
            ShadowMap = 0x21FE207D,
            GlassForRabbitHoles = 0x265FFAA1,
            ImpostorWater = 0x277CF8EB,
            Rug = 0x2A72B9A1,
            Trampoline = 0x3939E094,
            Foliage = 0x4549E22E,
            ParticleAnim = 0x460E93F4,
            SolidPhong = 0x47C6638C,
            GlassForObjects = 0x492ECA7C,
            Stairs = 0x4CE2F497,
            OutdoorProp = 0x4D26BEC0,
            GlassForFences = 0x52986C62,
            SimSkin = 0x548394B9,
            Additive = 0x5AF16731,
            SimGlass = 0x5EDA9CDE,
            Fence = 0x67107FE8,
            LotImposter = 0x68601DE3,
            BasinWater = 0x6AAD2AD5,
            StandingWater = 0x70FDE012,
            BuildingWindow = 0x7B036C01,
            Roof = 0x7BD05F63,
            GlassForPortals = 0x81DD204D,
            GlassForObjectsTranslucent = 0x849CF021,
            SimHair = 0x84FD7152,
            Landmark = 0x8A60B969,
            RabbitHoleHighDetail = 0x8D346BBC,
            CASRoom = 0x94B9A835,
            SimEyelashes = 0x9D9DA161,
            Gemstones = 0xA063C1D0,
            Counters = 0xA4172F62,
            FlatMirror = 0xA68D9E29,
            Painting = 0xAA495821,
            RabbitHoleMediumDetail = 0xAEDE7105,
            Phong = 0xB9105A6D,
            Floors = 0xBC84D000,
            DropShadow = 0xC09C7582,
            SimEyes = 0xCF8A70B4,
            Plumbob = 0xDEF16564,
            SculptureIce = 0xE5D98507,
            PhongAlpha = 0xFC5FC212,
            ParticleJet = 0xFF5E6908,
        }

        // At some point AHandlerElement will gain IResource...
        public class MTRL : AHandlerElement, IEquatable<MTRL>, IResource
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
                this.sdList = basis.sdList == null ? null : new ShaderDataList(handler, basis.sdList);
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
                this.sdList = new ShaderDataList(handler, s, start, null);
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

            public override bool Equals(object obj)
            {
                return obj as MTRL != null ? this.Equals(obj as MTRL) : false;
            }

            public override int GetHashCode()
            {
                return mtrlUnknown1.GetHashCode() ^ mtrlUnknown2.GetHashCode() ^ mtrlUnknown3.GetHashCode() ^ sdList.GetHashCode();
            }

            #endregion

            #region IResource
            public Stream Stream
            {
                get
                {
                    MemoryStream ms = new MemoryStream();
                    UnParse(ms);
                    ms.Position = 0;
                    return ms;
                }
            }

            public byte[] AsBytes
            {
                get { return ((MemoryStream)Stream).ToArray(); }
                set { MemoryStream ms = new MemoryStream(value); Parse(ms); OnElementChanged(); }
            }

            public event EventHandler ResourceChanged;
            protected override void OnElementChanged()
            {
                dirty = true;
                if (handler != null) handler(this, EventArgs.Empty);
                if (ResourceChanged != null) ResourceChanged(this, EventArgs.Empty);
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint MTRLUnknown1 { get { return mtrlUnknown1; } set { if (mtrlUnknown1 != value) { mtrlUnknown1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort MTRLUnknown2 { get { return mtrlUnknown2; } set { if (mtrlUnknown2 != value) { mtrlUnknown2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ushort MTRLUnknown3 { get { return mtrlUnknown3; } set { if (mtrlUnknown3 != value) { mtrlUnknown3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public ShaderDataList SData { get { return sdList; } set { if (sdList != value) { sdList = value == null ? null : new ShaderDataList(handler, value); OnElementChanged(); } } }

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
        public class MTNF : AHandlerElement, IEquatable<MTNF>, IResource
        {
            const int recommendedApiVersion = 1;

            uint mtnfUnknown1;
            ShaderDataList sdList = null;

            public MTNF(int APIversion, EventHandler handler, MTNF basis)
                : base(APIversion, handler)
            {
                this.mtnfUnknown1 = basis.mtnfUnknown1;
                this.sdList = basis.sdList == null ? null : new ShaderDataList(handler, basis.sdList);
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
            public override bool Equals(object obj)
            {
                return obj as MTNF != null ? this.Equals(obj as MTNF) : false;
            }
            public override int GetHashCode()
            {
                return mtnfUnknown1.GetHashCode() ^ sdList.GetHashCode();
            }

            #endregion

            #region IResource
            public Stream Stream
            {
                get
                {
                    MemoryStream ms = new MemoryStream();
                    UnParse(ms);
                    ms.Position = 0;
                    return ms;
                }
            }

            public byte[] AsBytes
            {
                get { return ((MemoryStream)Stream).ToArray(); }
                set { MemoryStream ms = new MemoryStream(value); Parse(ms); OnElementChanged(); }
            }

            public event EventHandler ResourceChanged;
            protected override void OnElementChanged()
            {
                dirty = true;
                if (handler != null) handler(this, EventArgs.Empty);
                if (ResourceChanged != null) ResourceChanged(this, EventArgs.Empty);
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint MTNFUnknown1 { get { return mtnfUnknown1; } set { if (mtnfUnknown1 != value) { mtnfUnknown1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ShaderDataList SData { get { return sdList; } set { if (sdList != value) { sdList = value == null ? null : new ShaderDataList(handler, value); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public enum FieldType : uint
        {
            None = 0x00000000,
            AlignAcrossDirection = 0x01885886, // Float
            DimmingCenterHeight = 0x01ADACE0, // Float
            Transparency = 0x05D22FD3, // Float
            BlendSourceMode = 0x0995E96C, // Float
            SharpSpecControl = 0x11483F01, // Float
            RotateSpeedRadsSec = 0x16BF7A44, // Float
            AlignToDirection = 0x17B78AF6, // Float
            DropShadowStrength = 0x1B1AB4D5, // Float
            ContourSmoothing = 0x1E27DCCD, // Float
            reflectivity = 0x29BCDD1F, // Float
            BlendOperation = 0x2D13B939, // Float
            RotationSpeed = 0x32003AD4, // Float
            DimmingRadius = 0x32DFA298, // Float
            IsSolidObject = 0x3BBF99CF, // Float
            NormalMapScale = 0x3C45E334, // Float
            NoAutomaticDaylightDimming = 0x3CB5FA70, // Float
            FramesPerSecond = 0x406ADE00, // Float
            BloomFactor = 0x4168508B, // Float
            EmissiveBloomMultiplier = 0x490E6EB4, // Float
            RippleSpeed = 0x52DEC070, // Float
            UseLampColor = 0x56B220CD, // Float
            TextureSpeedScale = 0x583DF357, // Float
            NoiseMapScale = 0x5E86DEA1, // Float
            AutoRainbow = 0x5F7800EA, // Float
            DebouncePower = 0x656025DF, // Float
            SpeedStretchFactor = 0x66479028, // Float
            WindSpeed = 0x66E9B6BC, // Float
            DaytimeOnly = 0x6BB389BC, // Float
            FramesRandomStartFactor = 0x7211F24F, // Float
            DeflectionThreshold = 0x7D621D61, // Float
            LifetimeSeconds = 0x84212733, // Float
            NormalBumpScale = 0x88C64AE2, // Float
            DeformerOffset = 0x8BDF4746, // Float
            EdgeDarkening = 0x8C27D8C9, // Float
            OverrideFactor = 0x8E35CCC0, // Float
            EmissiveLightMultiplier = 0x8EF71C85, // Float
            SharpSpecThreshold = 0x903BE4D3, // Float
            RugSort = 0x906997A9, // Float
            Layer2Shift = 0x92692CB2, // Float
            SpecStyle = 0x9554D40F, // Float
            FadeDistance = 0x957210EA, // Float
            BlendDestMode = 0x9BDECB37, // Float
            LightingEnabled = 0xA15E4594, // Float
            OverrideSpeed = 0xA3D6342E, // Float
            VisibleOnlyAtNight = 0xAC5D0A82, // Float
            UseDiffuseForAlphaTest = 0xB597FA7F, // Float
            SparkleSpeed = 0xBA13921E, // Float
            WindStrength = 0xBC4A2544, // Float
            HaloBlur = 0xC3AD4F50, // Float
            RefractionDistortionScale = 0xC3C472A1, // Float
            DiffuseMapUVChannel = 0xC45A5F41, // Float
            SpecularMapUVChannel = 0xCB053686, // Float
            ParticleCount = 0xCC31B828, // Float
            RippleDistanceScale = 0xCCB35B98, // Float
            DivetScale = 0xCE8C8311, // Float
            ForceAmount = 0xD4D51D02, // Float
            AnimSpeed = 0xD600CB63, // Float
            BackFaceDiffuseContribution = 0xD641A1B1, // Float
            BounceAmountMeters = 0xD8542D8B, // Float
            index_of_refraction = 0xDAA9532D, // Float
            BloomScale = 0xE29BA4AC, // Float
            AlphaMaskThreshold = 0xE77A2B60, // Float
            LightingDirectScale = 0xEF270EE4, // Float
            AlwaysOn = 0xF019641D, // Float
            Shininess = 0xF755F7FF, // Float
            FresnelOffset = 0xFB66A8CB, // Float
            BouncePower = 0xFBA6B898, // Float
            ShadowAlphaTest = 0xFEB1F9CB, // Float
            DiffuseUVScale = 0x2D4E507E, // Float2
            RippleHeights = 0x6A07D7E1, // Float2
            CutoutValidHeights = 0x6D43D7B7, // Float2
            UVTiling = 0x773CAB85, // Float2
            SizeScaleEnd = 0x891A3133, // Float2
            SizeScaleStart = 0x9A6C2EC8, // Float2
            WaterScrollSpeedLayer2 = 0xAFA11435, // Float2
            WaterScrollSpeedLayer1 = 0xAFA11436, // Float2
            NormalUVScale = 0xBA2D1AB9, // Float2
            DetailUVScale = 0xCD985A0B, // Float2
            SpecularUVScale = 0xF12E27C3, // Float2
            UVScrollSpeed = 0xF2EEA6EC, // Float2
            Ambient = 0x04A5DAA3, // Float3
            OverrideDirection = 0x0C12DED8, // Float3
            OverrideVelocity = 0x14677578, // Float3
            CounterMatrixRow1 = 0x1EF8655D, // Float3
            CounterMatrixRow2 = 0x1EF8655E, // Float3
            ForceDirection = 0x29881F55, // Float3
            Specular = 0x2CE11842, // Float3
            HaloLowColor = 0x2EB8E8D4, // Float3
            Emission = 0x3BD441A0, // Float3
            NormalMapUVSelector = 0x415368B4, // Float3
            UVScales = 0x420520E9, // Float3
            LightMapScale = 0x4F7DCB9B, // Float3
            Diffuse = 0x637DAA05, // Float3
            Reflective = 0x73C9923E, // Float3
            AmbientUVSelector = 0x797F8E81, // Float3
            HighlightColor = 0x90F8DCF0, // Float3
            DiffuseUVSelector = 0x91EEBAFF, // Float3
            Transparent = 0x988403F9, // Float3
            VertexColorScale = 0xA2FD73CA, // Float3
            SpecularUVSelector = 0xB63546AC, // Float3
            EmissionMapUVSelector = 0xBC823DDC, // Float3
            HaloHighColor = 0xD4043258, // Float3
            RootColor = 0xE90599F6, // Float3
            ForceVector = 0xEBA4727B, // Float3
            PositionTweak = 0xEF36D180, // Float3
            TimelineLength = 0x0081AE98, // Float4
            UVScale = 0x159BA53E, // Float4
            FrameData = 0x1E5B2324, // Float4
            AnimDir = 0x3F89C2EF, // Float4
            PosScale = 0x487648E5, // Float4
            Births = 0x568E0367, // Float4
            UVOffset = 0x57582869, // Float4
            PosOffset = 0x790EBF2C, // Float4
            AverageColor = 0x449A3A67, // Int
            MaskWidth = 0x707F712F, // Int
            MaskHeight = 0x849CDADC, // Int
            SparkleCube = 0x1D90C086, // Texture
            DropShadowAtlas = 0x22AD8507, // Texture
            DirtOverlay = 0x48372E62, // Texture
            OverlayTexture = 0x4DC0C8BC, // Texture
            JetTexture = 0x52CE211B, // Texture
            ColorRamp = 0x581835D6, // Texture
            DiffuseMap = 0x6CC0FD85, // Texture
            SelfIlluminationMap = 0x6E067554, // Texture
            NormalMap = 0x6E56548A, // Texture
            HaloRamp = 0x84F6E0FB, // Texture
            DetailMap = 0x9205DAA8, // Texture
            SpecularMap = 0xAD528A60, // Texture
            AmbientOcclusionMap = 0xB01CBA60, // Texture
            AlphaMap = 0xC3FAAC4F, // Texture
            MultiplyMap = 0xCD869A45, // Texture
            SpecCompositeTexture = 0xD652FADE, // Texture
            NoiseMap = 0xE19FD579, // Texture
            RoomLightMap = 0xE7CA9166, // Texture
            EmissionMap = 0xF303D152, // Texture
            RevealMap = 0xF3F22AC4, // Texture
            ImposterTextureAOandSI = 0x15C9D298, // TextureKey
            ImpostorDetailTexture = 0x56E1C6B2, // TextureKey
            ImposterTexture = 0xBDCF71C5, // TextureKey
            ImposterTextureWater = 0xBF3FB9FA, // TextureKey
        }

        public enum DataType : uint
        {
            dtUnknown = 0,
            dtFloat = 1,
            dtInt = 2,
            dtTexture = 4,
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
                return (ShaderData)basis.GetType().GetConstructor(new Type[] { typeof(int), typeof(EventHandler), basis.GetType(), })
                    .Invoke(new object[] { APIversion, handler, basis, });
            }

            internal int ByteCount() { return CountFromType; }

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
            public override bool Equals(object obj)
            {
                return obj as ShaderData != null ? this.Equals(obj as ShaderData) : false;
            }
            public override abstract int GetHashCode();

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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data.GetHashCode();
            }

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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data0.GetHashCode() ^ data1.GetHashCode();
            }

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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data0.GetHashCode() ^ data1.GetHashCode() ^ data2.GetHashCode();
            }

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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data0.GetHashCode() ^ data1.GetHashCode() ^ data2.GetHashCode() ^ data3.GetHashCode();
            }

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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data.GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public Int32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, (uint)0, })]
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
            public ElementTextureRef(int APIversion, EventHandler handler, FieldType field, uint chunkRef) : base(APIversion, handler, field) { this.data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, chunkRef); }
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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data.GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public GenericRCOLResource.ChunkReference Data { get { return data; } set { if (data != value) { data = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] { (FieldType)0, (uint)0, (uint)0, (ulong)0, })]
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
            public ElementTextureKey(int APIversion, EventHandler handler, FieldType field, uint resourceType, uint resourceGroup, ulong instance) : base(APIversion, handler, field) { this.data = new TGIBlock(requestedApiVersion, handler, resourceType, resourceGroup, instance); }
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
            public override int GetHashCode()
            {
                return field.GetHashCode() ^ data.GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(11)]
            public IResourceKey Data { get { return data; } set { if (!data.Equals(value)) { data = new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class ShaderDataList : DependentList<ShaderData>
        {
            static List<Type> ShaderDataNestedTypes;
            static ShaderDataList() { ShaderDataNestedTypes = new List<Type>(typeof(ShaderData).DeclaringType.GetNestedTypes()); }

            internal long dataPos = -1;

            #region Constructors
            public ShaderDataList(EventHandler handler) : base(handler) { }
            public ShaderDataList(EventHandler handler, Stream s, long start, Nullable<int> expectedDataLen) : base(null) { elementHandler = handler; Parse(s, start, expectedDataLen); this.handler = handler; }
            public ShaderDataList(EventHandler handler, IEnumerable<ShaderData> lsd) : base(handler, lsd) { }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { throw new NotSupportedException(); }
            internal void Parse(Stream s, long start, Nullable<int> expectedDataLen)
            {
                int dataLen = 0;
                for (int i = ReadCount(s); i > 0; i--)
                {
                    this.Add(ShaderData.CreateEntry(0, elementHandler, s, start));
                    dataLen += this[Count - 1].ByteCount() * 4;
                }
                if (checking) if (expectedDataLen != null && expectedDataLen != dataLen)
                        throw new InvalidDataException(string.Format("Expected 0x{0:X8} bytes of data, read 0x{1:X8} at 0x{2:X8}", expectedDataLen, dataLen, s.Position));
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

                List<Type> types = new List<Type>(new Type[] { typeof(int), typeof(EventHandler), });
                for (int i = 0; i < fields.Length; i++) types.Add(fields[i].GetType());

                return ShaderDataNestedTypes.Find(type =>
                {
                    if (!type.IsSubclassOf(typeof(ShaderData))) return false;
                    System.Reflection.ConstructorInfo ci = type.GetConstructor(types.ToArray());
                    if (ci == null) return false;
                    System.Reflection.ParameterInfo[] api = ci.GetParameters();
                    for (int i = 0; i < types.Count; i++) if (types[i] != api[i].ParameterType) return false;
                    return true;
                });
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
