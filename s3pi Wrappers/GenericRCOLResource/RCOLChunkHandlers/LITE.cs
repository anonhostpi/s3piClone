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

namespace s3pi.GenericRCOLResource
{
    public class LITE : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;
        const string TAG = "LITE";

        #region Attributes
        uint tag = (uint)FOURCC(TAG);
        uint version = 4;
        uint unknown1 = 0x84;
        ushort unknown2 = 0;
        LongSectionList longSections = null;
        ShortSectionList shortSections = null;
        #endregion

        #region Constructors
        public LITE(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public LITE(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public LITE(int APIversion, EventHandler handler, LITE basis)
            : base(APIversion, handler, null)
        {
            this.version = basis.version;
            this.unknown1 = basis.unknown1;
            this.longSections = new LongSectionList(handler, basis.longSections);
            this.shortSections = new ShortSectionList(handler, basis.shortSections);
            this.unknown2 = basis.unknown2;
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x03B4C61D; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC(TAG))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            version = r.ReadUInt32();
            unknown1 = r.ReadUInt32();
            byte lsCount = r.ReadByte();
            byte ssCount = r.ReadByte();
            unknown2 = r.ReadUInt16();
            longSections = new LongSectionList(handler, lsCount, s);
            shortSections = new ShortSectionList(handler, ssCount, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);
            w.Write(unknown1);
            if (longSections == null) longSections = new LongSectionList(handler);
            w.Write((byte)longSections.Count);
            if (shortSections == null) shortSections = new ShortSectionList(handler);
            w.Write((byte)shortSections.Count);
            w.Write(unknown2);
            longSections.UnParse(ms);
            shortSections.UnParse(ms);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new LITE(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class LongSection : AHandlerElement, IEquatable<LongSection>
        {
            const int recommendedApiVersion = 1;

            public enum LightSourceType : uint
            {
                Unknown = 0x00,
                Point = 0x03,
                Spot = 0x04,
                Window = 0x07,
                Area = 0x09,
            }

            #region Attributes
            LightSourceType lightSource = LightSourceType.Unknown;
            float xTiles = 0;
            float yMetres = 0;
            float zTiles = 0;
            float red = 0;
            float green = 0;
            float blue = 0;
            float intensity = 0; // Point, Spot, Area
            float yRotation = 0; // Spot
            float zRotation = 0; // Spot
            float xRotation = 0; // Spot
            float coneAngle = 0; // Spot
            float f12_unknown = 0;
            float f13_unknown = 0;
            float width = 0; // Window, Area
            float height = 0; // Window, Area
            float[] floats16_31_unknown = new float[16];
            #endregion

            #region Constructors
            public LongSection(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public LongSection(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public LongSection(int APIversion, EventHandler handler, LongSection basis)
                : this(APIversion, handler, basis.lightSource
                , basis.xTiles, basis.yMetres, basis.zTiles
                , basis.red, basis.green, basis.blue, basis.intensity
                , basis.yRotation, basis.zRotation, basis.xRotation, basis.coneAngle
                , basis.f12_unknown, basis.f13_unknown, basis.width, basis.height
                , basis.floats16_31_unknown
                ) { }

            public LongSection(int APIversion, EventHandler handler, LightSourceType sectionType
                , float xTiles, float yMetres, float zTiles
                , float red, float green, float blue, float intensity
                , float yaw, float roll, float pitch, float aperture
                , float f12_unknown, float f13_unknown, float width, float height
                , float[] floats16_31_unknown
                )
                : base(APIversion, handler)
            {
                this.lightSource = sectionType;
                this.xTiles = xTiles;
                this.yMetres = yMetres;
                this.zTiles = zTiles;
                this.red = red;
                this.green = green;
                this.blue = blue;
                this.intensity = intensity;
                this.yRotation = yaw;
                this.zRotation = roll;
                this.xRotation = pitch;
                this.coneAngle = aperture;
                this.f12_unknown = f12_unknown;
                this.f13_unknown = f13_unknown;
                this.width = width;
                this.height = height;
                if (checking) if (floats16_31_unknown.Length != 16)
                        throw new ArgumentException("Array length must be 16");
                this.floats16_31_unknown = (float[])floats16_31_unknown.Clone();
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                lightSource = (LightSourceType)r.ReadUInt32();
                xTiles = r.ReadSingle();
                yMetres = r.ReadSingle();
                zTiles = r.ReadSingle();
                red = r.ReadSingle();
                green = r.ReadSingle();
                blue = r.ReadSingle();
                intensity = r.ReadSingle();
                yRotation = r.ReadSingle();
                zRotation = r.ReadSingle();
                xRotation = r.ReadSingle();
                coneAngle = r.ReadSingle();
                f12_unknown = r.ReadSingle();
                f13_unknown = r.ReadSingle();
                width = r.ReadSingle();
                height = r.ReadSingle();
                for (int i = 0; i < floats16_31_unknown.Length; i++)
                    floats16_31_unknown[i] = r.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)lightSource);
                w.Write(xTiles);
                w.Write(yMetres);
                w.Write(zTiles);
                w.Write(red);
                w.Write(green);
                w.Write(blue);
                w.Write(intensity);
                w.Write(yRotation);
                w.Write(zRotation);
                w.Write(xRotation);
                w.Write(coneAngle);
                w.Write(f12_unknown);
                w.Write(f13_unknown);
                w.Write(width);
                w.Write(height);
                for (int i = 0; i < floats16_31_unknown.Length; i++)
                    w.Write(floats16_31_unknown[i]);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    switch (lightSource)
                    {
                        case LightSourceType.Point:
                            res.Remove("F07_unknown");
                            res.Remove("YRotation");
                            res.Remove("ZRotation");
                            res.Remove("XRotation");
                            res.Remove("ConeAngle");
                            res.Remove("Width");
                            res.Remove("Height");
                            break;
                        case LightSourceType.Spot:
                            res.Remove("F07_unknown");
                            res.Remove("F08_unknown");
                            res.Remove("F09_unknown");
                            res.Remove("F10_unknown");
                            res.Remove("F11_unknown");
                            res.Remove("Width");
                            res.Remove("Height");
                            break;
                        case LightSourceType.Window:
                            res.Remove("Intensity");
                            res.Remove("YRotation");
                            res.Remove("ZRotation");
                            res.Remove("XRotation");
                            res.Remove("ConeAngle");
                            res.Remove("F14_unknown");
                            res.Remove("F15_unknown");
                            break;
                        case LightSourceType.Area:
                            res.Remove("F07_unknown");
                            res.Remove("YRotation");
                            res.Remove("ZRotation");
                            res.Remove("XRotation");
                            res.Remove("ConeAngle");
                            res.Remove("F14_unknown");
                            res.Remove("F15_unknown");
                            break;
                        default:
                            res.Remove("Intensity");
                            res.Remove("YRotation");
                            res.Remove("ZRotation");
                            res.Remove("XRotation");
                            res.Remove("ConeAngle");
                            res.Remove("Width");
                            res.Remove("Height");
                            break;
                    }
                    return res;
                }
            }

            public override AHandlerElement Clone(EventHandler handler) { return new LongSection(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<LongSection> Members

            public bool Equals(LongSection other)
            {
                return lightSource.Equals(other.lightSource)
                    && xTiles.Equals(other.xTiles)
                    && yMetres.Equals(other.yMetres)
                    && zTiles.Equals(other.zTiles)
                    && red.Equals(other.red)
                    && green.Equals(other.green)
                    && blue.Equals(other.blue)
                    && intensity.Equals(other.intensity)
                    && yRotation.Equals(other.yRotation)
                    && zRotation.Equals(other.zRotation)
                    && xRotation.Equals(other.xRotation)
                    && coneAngle.Equals(other.coneAngle)
                    && f12_unknown.Equals(other.f12_unknown)
                    && f13_unknown.Equals(other.f13_unknown)
                    && width.Equals(other.width)
                    && height.Equals(other.height)
                    && ArrayCompare(floats16_31_unknown, other.floats16_31_unknown)
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(0)]
            public LightSourceType LightSource { get { return lightSource; } set { if (lightSource != value) { lightSource = value; OnElementChanged(); } } }
            [ElementPriority(1)]
            public float XOffset { get { return xTiles; } set { if (xTiles != value) { xTiles = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float YOffset { get { return yMetres; } set { if (yMetres != value) { yMetres = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float ZOffset { get { return zTiles; } set { if (zTiles != value) { zTiles = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public float Red { get { return red; } set { if (red != value) { red = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public float Green { get { return green; } set { if (green != value) { green = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public float Blue { get { return blue; } set { if (blue != value) { blue = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public float Intensity { get { return intensity; } set { if (intensity != value) { intensity = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public float F07_unknown { get { return intensity; } set { if (intensity != value) { intensity = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public float YRotation { get { return yRotation; } set { if (yRotation != value) { yRotation = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public float F08_unknown { get { return yRotation; } set { if (yRotation != value) { yRotation = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public float ZRotation { get { return zRotation; } set { if (zRotation != value) { zRotation = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public float F09_unknown { get { return zRotation; } set { if (zRotation != value) { zRotation = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public float XRotation { get { return xRotation; } set { if (xRotation != value) { xRotation = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public float F10_unknown { get { return xRotation; } set { if (xRotation != value) { xRotation = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public float ConeAngle { get { return coneAngle; } set { if (coneAngle != value) { coneAngle = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public float F11_unknown { get { return coneAngle; } set { if (coneAngle != value) { coneAngle = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public float F12_unknown { get { return f12_unknown; } set { if (f12_unknown != value) { f12_unknown = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public float F13_unknown { get { return f13_unknown; } set { if (f13_unknown != value) { f13_unknown = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public float Width { get { return width; } set { if (width != value) { width = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public float F14_unknown { get { return width; } set { if (width != value) { width = value; OnElementChanged(); } } }
            [ElementPriority(15)]
            public float Height { get { return height; } set { if (height != value) { height = value; OnElementChanged(); } } }
            [ElementPriority(16)]
            public float F15_unknown { get { return height; } set { if (height != value) { height = value; OnElementChanged(); } } }
            [ElementPriority(17)]
            public float F16_unknown { get { return floats16_31_unknown[0]; } set { setFloatN(0, value); } }
            [ElementPriority(18)]
            public float F17_unknown { get { return floats16_31_unknown[1]; } set { setFloatN(1, value); } }
            [ElementPriority(19)]
            public float F18_unknown { get { return floats16_31_unknown[2]; } set { setFloatN(2, value); } }
            [ElementPriority(20)]
            public float F19_unknown { get { return floats16_31_unknown[3]; } set { setFloatN(3, value); } }
            [ElementPriority(21)]
            public float F20_unknown { get { return floats16_31_unknown[4]; } set { setFloatN(4, value); } }
            [ElementPriority(22)]
            public float F21_unknown { get { return floats16_31_unknown[5]; } set { setFloatN(5, value); } }
            [ElementPriority(23)]
            public float F22_unknown { get { return floats16_31_unknown[6]; } set { setFloatN(6, value); } }
            [ElementPriority(24)]
            public float F23_unknown { get { return floats16_31_unknown[7]; } set { setFloatN(7, value); } }
            [ElementPriority(25)]
            public float F24_unknown { get { return floats16_31_unknown[8]; } set { setFloatN(8, value); } }
            [ElementPriority(26)]
            public float F25_unknown { get { return floats16_31_unknown[9]; } set { setFloatN(9, value); } }
            [ElementPriority(27)]
            public float F26_unknown { get { return floats16_31_unknown[10]; } set { setFloatN(10, value); } }
            [ElementPriority(28)]
            public float F27_unknown { get { return floats16_31_unknown[11]; } set { setFloatN(11, value); } }
            [ElementPriority(29)]
            public float F28_unknown { get { return floats16_31_unknown[12]; } set { setFloatN(12, value); } }
            [ElementPriority(30)]
            public float F29_unknown { get { return floats16_31_unknown[13]; } set { setFloatN(13, value); } }
            [ElementPriority(31)]
            public float F30_unknown { get { return floats16_31_unknown[14]; } set { setFloatN(14, value); } }
            [ElementPriority(32)]
            public float F31_unknown { get { return floats16_31_unknown[15]; } set { setFloatN(15, value); } }
            void setFloatN(int n, float value) { if (floats16_31_unknown[n] != value) { floats16_31_unknown[n] = value; OnElementChanged(); } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    foreach (string field in ContentFields)
                        if (!field.Equals("Value"))
                            s += "\n" + field + ": " + this[field];
                    return s.TrimStart('\n');
                    /**/
                }
            }
            #endregion
        }

        public class LongSectionList : DependentList<LongSection>
        {
            int count;

            #region Constructors
            public LongSectionList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public LongSectionList(EventHandler handler, int count, Stream s) : base(null, Byte.MaxValue) { this.count = count; elementHandler = handler; Parse(s); this.handler = handler; }
            public LongSectionList(EventHandler handler, IEnumerable<LongSection> llp) : base(handler, llp, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return count; }
            protected override void WriteCount(Stream s, int count) { }

            protected override LongSection CreateElement(Stream s) { return new LongSection(0, elementHandler, s); }

            protected override void WriteElement(Stream s, LongSection element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new LongSection(0, null)); }
        }

        public class ShortSection : AHandlerElement, IEquatable<ShortSection>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            uint unknown1 = 0;
            float[] floats1_13_unknown = new float[13];
            #endregion

            #region Constructors
            public ShortSection(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public ShortSection(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public ShortSection(int APIversion, EventHandler handler, ShortSection basis)
                : this(APIversion, handler, basis.unknown1, basis.floats1_13_unknown) { }
            public ShortSection(int APIversion, EventHandler handler, uint unknown1, float[] floats1_13_unknown)
                : base(APIversion, handler)
            {
                this.unknown1 = unknown1;
                if (checking) if (floats1_13_unknown.Length != 13)
                        throw new ArgumentException("Array length must be 13");
                this.floats1_13_unknown = (float[])floats1_13_unknown.Clone();
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadUInt32();
                for (int i = 0; i < floats1_13_unknown.Length; i++)
                    floats1_13_unknown[i] = r.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
                for (int i = 0; i < floats1_13_unknown.Length; i++)
                    w.Write(floats1_13_unknown[i]);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new ShortSection(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<ShortSection> Members

            public bool Equals(ShortSection other) { return unknown1.Equals(other.unknown1) && ArrayCompare(floats1_13_unknown, other.floats1_13_unknown) ; }

            #endregion

            #region Content Fields
            [ElementPriority(0)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(1)]
            public float F01_unknown { get { return floats1_13_unknown[0]; } set { setFloatN(0, value); } }
            [ElementPriority(2)]
            public float F02_unknown { get { return floats1_13_unknown[1]; } set { setFloatN(1, value); } }
            [ElementPriority(3)]
            public float F03_unknown { get { return floats1_13_unknown[2]; } set { setFloatN(2, value); } }
            [ElementPriority(4)]
            public float F04_unknown { get { return floats1_13_unknown[3]; } set { setFloatN(3, value); } }
            [ElementPriority(5)]
            public float F05_unknown { get { return floats1_13_unknown[4]; } set { setFloatN(4, value); } }
            [ElementPriority(6)]
            public float F06_unknown { get { return floats1_13_unknown[5]; } set { setFloatN(5, value); } }
            [ElementPriority(7)]
            public float F07_unknown { get { return floats1_13_unknown[6]; } set { setFloatN(6, value); } }
            [ElementPriority(8)]
            public float F08_unknown { get { return floats1_13_unknown[7]; } set { setFloatN(7, value); } }
            [ElementPriority(9)]
            public float F09_unknown { get { return floats1_13_unknown[8]; } set { setFloatN(8, value); } }
            [ElementPriority(10)]
            public float F10_unknown { get { return floats1_13_unknown[9]; } set { setFloatN(9, value); } }
            [ElementPriority(11)]
            public float F11_unknown { get { return floats1_13_unknown[10]; } set { setFloatN(10, value); } }
            [ElementPriority(12)]
            public float F12_unknown { get { return floats1_13_unknown[11]; } set { setFloatN(11, value); } }
            [ElementPriority(13)]
            public float F13_unknown { get { return floats1_13_unknown[12]; } set { setFloatN(12, value); } }
            void setFloatN(int n, float value) { if (floats1_13_unknown[n] != value) { floats1_13_unknown[n] = value; OnElementChanged(); } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    foreach (string field in ContentFields)
                        if (!field.Equals("Value"))
                            s += "\n" + field + ": " + this[field];
                    return s.TrimStart('\n');
                    /**/
                }
            }
            #endregion
        }

        public class ShortSectionList : DependentList<ShortSection>
        {
            int count;

            #region Constructors
            public ShortSectionList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public ShortSectionList(EventHandler handler, int count, Stream s) : base(null, Byte.MaxValue) { this.count = count; elementHandler = handler; Parse(s); this.handler = handler; }
            public ShortSectionList(EventHandler handler, IEnumerable<ShortSection> lss) : base(handler, lss, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return count; }
            protected override void WriteCount(Stream s, int count) { }

            protected override ShortSection CreateElement(Stream s) { return new ShortSection(0, elementHandler, s); }

            protected override void WriteElement(Stream s, ShortSection element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new ShortSection(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public ushort Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public LongSectionList LongSections { get { return longSections; } set { if (longSections != value) { longSections = new LongSectionList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public ShortSectionList ShortSections { get { return shortSections; } set { if (shortSections != value) { shortSections = new ShortSectionList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");
                s += "\nUnknown1: 0x" + unknown1.ToString("X8");
                s += "\nUnknown2: 0x" + unknown1.ToString("X4");

                string fmt;
                s += String.Format("\nLong Sections ({0:X}):", longSections.Count);
                fmt = "\n--[{0:X" + longSections.Count.ToString("X").Length + "}]--\n{1}\n--";
                for (int i = 0; i < longSections.Count; i++) s += String.Format(fmt, i, longSections[i].Value);
                s += "\n----";

                s += String.Format("\nShort Sections ({0:X}):", shortSections.Count);
                fmt = "\n--[{0:X" + shortSections.Count.ToString("X").Length + "}]--\n{1}\n--";
                for (int i = 0; i < shortSections.Count; i++) s += String.Format(fmt, i, shortSections[i].Value);

                return s;
                /**/
            }
        }
        #endregion
    }
}
