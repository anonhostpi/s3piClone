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

namespace CASPartResource
{
    /// <summary>
    /// A resource wrapper that understands CAS Part resources
    /// </summary>
    public class CASPartResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint version = 18;
        XMLEntryList xmlEntries;
        string unknown1 = "";
        float sortPriority;
        byte unknown2;
        ClothingType clothing;
        DataTypeFlags dataType;
        AgeGenderFlags ageGender;
        ClothingCategoryFlags clothingCategory;
        byte casPart1Index;
        byte casPart2Index;
        byte blendInfoFatIndex;
        byte blendInfoFitIndex;
        byte blendInfoThinIndex;
        byte blendInfoSpecialIndex;
        uint unknown3;
        ByteEntryList vpxyIndexes;
        OuterEntryList outerEntries;
        ByteEntryList diffuse1Indexes;
        ByteEntryList specular1Indexes;
        ByteEntryList diffuse2Indexes;
        ByteEntryList specular2Indexes;
        ByteEntryList bondIndexes;
        string unknown4 = "";
        CountedTGIBlockList tgiBlocks;

        #endregion

        public CASPartResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            int tgiPosn;

            BinaryReader r = new BinaryReader(s);

            version = r.ReadUInt32();
            tgiPosn = r.ReadInt32() + 8;
            xmlEntries = new XMLEntryList(OnResourceChanged, s);
            unknown1 = BigEndianUnicodeString.Read(s);
            sortPriority = r.ReadSingle();
            unknown2 = r.ReadByte();
            clothing = (ClothingType)r.ReadUInt32();
            dataType = (DataTypeFlags)r.ReadUInt32();
            ageGender = (AgeGenderFlags)r.ReadUInt32();
            clothingCategory = (ClothingCategoryFlags)r.ReadUInt32();
            casPart1Index = r.ReadByte();
            casPart2Index = r.ReadByte();
            blendInfoFatIndex = r.ReadByte();
            blendInfoFitIndex = r.ReadByte();
            blendInfoThinIndex = r.ReadByte();
            blendInfoSpecialIndex = r.ReadByte();
            unknown3 = r.ReadUInt32();
            vpxyIndexes = new ByteEntryList(OnResourceChanged, s);
            outerEntries = new OuterEntryList(OnResourceChanged, s);
            diffuse1Indexes = new ByteEntryList(OnResourceChanged, s);
            specular1Indexes = new ByteEntryList(OnResourceChanged, s);
            diffuse2Indexes = new ByteEntryList(OnResourceChanged, s);
            specular2Indexes = new ByteEntryList(OnResourceChanged, s);
            bondIndexes = new ByteEntryList(OnResourceChanged, s);
            unknown4 = BigEndianUnicodeString.Read(s);

            if (checking) if (tgiPosn != s.Position)
                    throw new InvalidDataException(String.Format("Position of TGIBlock read: 0x{0:X8}, actual: 0x{1:X8}",
                        tgiPosn, s.Position));

            byte count = r.ReadByte();
            tgiBlocks = new CountedTGIBlockList(OnResourceChanged, "IGT", count, s);
        }

        protected override Stream UnParse()
        {
            long posn, tgiPosn, end;
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(version);
            posn = s.Position;
            w.Write((int)0); //offset

            if (xmlEntries == null) xmlEntries = new XMLEntryList(OnResourceChanged);
            xmlEntries.UnParse(s);

            BigEndianUnicodeString.Write(s, unknown1);
            w.Write(sortPriority);
            w.Write(unknown2);
            w.Write((uint)clothing);
            w.Write((uint)dataType);
            w.Write((uint)ageGender);
            w.Write((uint)clothingCategory);
            w.Write(casPart1Index);
            w.Write(casPart2Index);
            w.Write(blendInfoFatIndex);
            w.Write(blendInfoFitIndex);
            w.Write(blendInfoThinIndex);
            w.Write(blendInfoSpecialIndex);
            w.Write(unknown3);

            if (vpxyIndexes == null) vpxyIndexes = new ByteEntryList(OnResourceChanged); vpxyIndexes.UnParse(s);
            if (outerEntries == null) outerEntries = new OuterEntryList(OnResourceChanged); outerEntries.UnParse(s);
            if (diffuse1Indexes == null) diffuse1Indexes = new ByteEntryList(OnResourceChanged); diffuse1Indexes.UnParse(s);
            if (specular1Indexes == null) specular1Indexes = new ByteEntryList(OnResourceChanged); specular1Indexes.UnParse(s);
            if (diffuse2Indexes == null) diffuse2Indexes = new ByteEntryList(OnResourceChanged); diffuse2Indexes.UnParse(s);
            if (specular2Indexes == null) specular2Indexes = new ByteEntryList(OnResourceChanged); specular2Indexes.UnParse(s);
            if (bondIndexes == null) bondIndexes = new ByteEntryList(OnResourceChanged); bondIndexes.UnParse(s);
            BigEndianUnicodeString.Write(s, unknown4);

            tgiPosn = s.Position;
            if (tgiBlocks == null) tgiBlocks = new CountedTGIBlockList(OnResourceChanged, "IGT");
            w.Write((byte)tgiBlocks.Count);
            tgiBlocks.UnParse(s);

            end = s.Position;

            s.Position = posn;
            w.Write((int)(tgiPosn - posn - sizeof(int)));
            s.Position = end;

            s.Flush();
            return s;
        }
        #endregion

        #region Sub-types
        public class XMLEntry : AHandlerElement, IEquatable<XMLEntry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            string xml;
            uint unknown1;
            #endregion

            #region Constructors
            public XMLEntry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public XMLEntry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public XMLEntry(int APIversion, EventHandler handler, XMLEntry basis) : this(APIversion, handler, basis.xml, basis.unknown1) { }
            public XMLEntry(int APIversion, EventHandler handler, string xml, uint unknown1) : base(APIversion, handler) { this.xml = xml; this.unknown1 = unknown1; }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                xml = System.Text.Encoding.Unicode.GetString(r.ReadBytes(r.ReadInt32() * 2));
                unknown1 = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(xml.Length);
                w.Write(System.Text.Encoding.Unicode.GetBytes(xml));
                w.Write(unknown1);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            public override AHandlerElement Clone(EventHandler handler) { return new XMLEntry(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<XMLEntry> Members

            public bool Equals(XMLEntry other)
            {
                return
                    this.xml == other.xml
                    && this.unknown1 == other.unknown1
                    ;
            }

            #endregion

            #region Content Fields
            //[ElementPriority(1)]
            //public string Xml { get { return xml; } set { if (xml != value) { xml = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }

            [ElementPriority(99)]
            public TextReader XmlFile
            {
                get { return new StringReader(xml); }
                set
                {
                    string temp = value.ReadToEnd();
                    if (xml != temp) { xml = temp; OnElementChanged(); }
                }
            }

            public string Value
            {
                get
                {
                    return "Xml: " + (xml.Length > 160 ? xml.Substring(0, 157) + "..." : xml) +
                        "\nUnknown1: " + this["Unknown1"];
                }
            }
            #endregion
        }
        public class XMLEntryList : DependentList<XMLEntry>
        {
            #region Constructors
            public XMLEntryList(EventHandler handler) : base(handler) { }
            public XMLEntryList(EventHandler handler, Stream s) : base(handler, s) { }
            public XMLEntryList(EventHandler handler, IEnumerable<XMLEntry> le) : base(handler, le) { }
            #endregion

            #region Data I/O
            protected override XMLEntry CreateElement(Stream s) { return new XMLEntry(0, elementHandler, s); }
            protected override void WriteElement(Stream s, XMLEntry element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new XMLEntry(0, null)); }
        }

        public class ByteEntryList : SimpleList<byte>
        {
            #region Constructors
            public ByteEntryList(EventHandler handler) : base(handler, ReadByte, WriteByte, byte.MaxValue, ReadListCount, WriteListCount) { }
            public ByteEntryList(EventHandler handler, Stream s) : base(handler, s, ReadByte, WriteByte, byte.MaxValue, ReadListCount, WriteListCount) { }
            public ByteEntryList(EventHandler handler, IEnumerable<byte> le) : base(handler, le, ReadByte, WriteByte, byte.MaxValue, ReadListCount, WriteListCount) { }
            #endregion

            #region Data I/O
            static int ReadListCount(Stream s) { return new BinaryReader(s).ReadByte(); }
            static void WriteListCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            static byte ReadByte(Stream s) { return new BinaryReader(s).ReadByte(); }
            static void WriteByte(Stream s, byte value) { new BinaryWriter(s).Write(value); }
            #endregion
        }

        public class InnerEntry : AHandlerElement, IEquatable<InnerEntry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            uint unknown1;
            uint unknown2;
            uint unknown3;
            #endregion

            #region Constructors
            public InnerEntry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public InnerEntry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public InnerEntry(int APIversion, EventHandler handler, InnerEntry basis) : this(APIversion, handler, basis.unknown1, basis.unknown2, basis.unknown3) { }
            public InnerEntry(int APIversion, EventHandler handler, uint unknown1, uint unknown2, uint unknown3)
                : base(APIversion, handler) { this.unknown1 = unknown1; this.unknown2 = unknown2; this.unknown3 = unknown3; }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadUInt32();
                unknown2 = r.ReadUInt32();
                unknown3 = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            public override AHandlerElement Clone(EventHandler handler) { return new InnerEntry(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<InnerEntry> Members

            public bool Equals(InnerEntry other)
            {
                return
                    this.unknown1 == other.unknown1
                    && this.unknown2 == other.unknown2
                    && this.unknown3 == other.unknown3
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    System.Text.StringBuilder sb = new StringBuilder();
                    foreach (string field in ContentFields)
                        if (!field.Equals("Value"))
                            sb.Append(string.Format("{0}: {1}; ", field, this[field]));
                    return sb.ToString().TrimEnd(';', ' ');
                }
            }
            #endregion
        }
        public class InnerEntryList : DependentList<InnerEntry>
        {
            #region Constructors
            public InnerEntryList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public InnerEntryList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public InnerEntryList(EventHandler handler, IEnumerable<InnerEntry> le) : base(handler, le, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return new BinaryReader(s).ReadByte(); }
            protected override void WriteCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            protected override InnerEntry CreateElement(Stream s) { return new InnerEntry(0, elementHandler, s); }
            protected override void WriteElement(Stream s, InnerEntry element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new InnerEntry(0, null)); }
        }

        public class OuterEntry : AHandlerElement, IEquatable<OuterEntry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            byte outerEntryNum;
            uint unknown1;
            InnerEntryList innerEntries;
            #endregion

            #region Constructors
            public OuterEntry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public OuterEntry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public OuterEntry(int APIversion, EventHandler handler, OuterEntry basis)
                : this(APIversion, handler, basis.outerEntryNum, basis.unknown1, basis.innerEntries) { }
            public OuterEntry(int APIversion, EventHandler handler, byte outerEntryNum, uint unknown1, IEnumerable<InnerEntry> le)
                : base(APIversion, handler)
            {
                this.outerEntryNum = outerEntryNum;
                this.unknown1 = unknown1;
                this.innerEntries = new InnerEntryList(handler, le);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                outerEntryNum = r.ReadByte();
                unknown1 = r.ReadUInt32();
                innerEntries = new InnerEntryList(handler, s);
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(outerEntryNum);
                w.Write(unknown1);
                if (innerEntries == null) innerEntries = new InnerEntryList(handler);
                innerEntries.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override AHandlerElement Clone(EventHandler handler) { return new OuterEntry(requestedApiVersion, handler, this); }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(OuterEntry other)
            {
                return
                    this.outerEntryNum == other.outerEntryNum
                    && this.unknown1 == other.unknown1
                    && this.innerEntries.Equals(other.innerEntries)
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public byte OuterEntryNum { get { return outerEntryNum; } set { if (outerEntryNum != value) { outerEntryNum = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public InnerEntryList InnerEntries { get { return innerEntries; } set { if (!innerEntries.Equals(value)) { innerEntries = new InnerEntryList(handler, value); OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    s += string.Format("OuterEntryNum: {0}; Unknown1: {1}; InnerEntries:", outerEntryNum, unknown1);
                    for (int i = 0; i < innerEntries.Count; i++)
                        s += string.Format("\n  [{0}]: {1}", i, innerEntries[i].Value);
                    return s;
                    /**/
                }
            }
            #endregion
        }
        public class OuterEntryList : DependentList<OuterEntry>
        {
            #region Constructors
            public OuterEntryList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public OuterEntryList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public OuterEntryList(EventHandler handler, IEnumerable<OuterEntry> le) : base(handler, le, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return new BinaryReader(s).ReadByte(); }
            protected override void WriteCount(Stream s, int count) { new BinaryWriter(s).Write((byte)count); }
            protected override OuterEntry CreateElement(Stream s) { return new OuterEntry(0, elementHandler, s); }
            protected override void WriteElement(Stream s, OuterEntry element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new OuterEntry(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(2)]
        public XMLEntryList XmlEntries { get { return xmlEntries; } set { if (xmlEntries.Equals(value)) { xmlEntries = new XMLEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(3)]
        public string Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(4)]
        public float SortPriority { get { return sortPriority; } set { if (sortPriority != value) { sortPriority = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(5)]
        public byte Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(6)]
        public ClothingType Clothing { get { return clothing; } set { if (clothing != value) { clothing = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(7)]
        public DataTypeFlags DataType { get { return dataType; } set { if (dataType != value) { dataType = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(8)]
        public AgeGenderFlags AgeGender { get { return ageGender; } set { if (ageGender != value) { ageGender = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(9)]
        public ClothingCategoryFlags ClothingCategory { get { return clothingCategory; } set { if (clothingCategory != value) { clothingCategory = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(10), TGIBlockListContentField("TGIBlocks")]
        public byte CasPart1Index { get { return casPart1Index; } set { if (casPart1Index != value) { casPart1Index = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(11), TGIBlockListContentField("TGIBlocks")]
        public byte CasPart2Index { get { return casPart2Index; } set { if (casPart2Index != value) { casPart2Index = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(12), TGIBlockListContentField("TGIBlocks")]
        public byte BlendInfoFatIndex { get { return blendInfoFatIndex; } set { if (blendInfoFatIndex != value) { blendInfoFatIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(13), TGIBlockListContentField("TGIBlocks")]
        public byte BlendInfoFitIndex { get { return blendInfoFitIndex; } set { if (blendInfoFitIndex != value) { blendInfoFitIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(14), TGIBlockListContentField("TGIBlocks")]
        public byte BlendInfoThinIndex { get { return blendInfoThinIndex; } set { if (blendInfoThinIndex != value) { blendInfoThinIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(15), TGIBlockListContentField("TGIBlocks")]
        public byte BlendInfoSpecialIndex { get { return blendInfoSpecialIndex; } set { if (blendInfoSpecialIndex != value) { blendInfoSpecialIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(16)]
        public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(17), DataGridExpandable]
        public ByteEntryList VPXYIndexes { get { return vpxyIndexes; } set { if (vpxyIndexes.Equals(value)) { vpxyIndexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(18)]
        public OuterEntryList OuterEntries { get { return outerEntries; } set { if (outerEntries.Equals(value)) { outerEntries = new OuterEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(19), DataGridExpandable]
        public ByteEntryList Diffuse1Indexes { get { return diffuse1Indexes; } set { if (diffuse1Indexes.Equals(value)) { diffuse1Indexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(20), DataGridExpandable]
        public ByteEntryList Specular1Indexes { get { return specular1Indexes; } set { if (specular1Indexes.Equals(value)) { specular1Indexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(21), DataGridExpandable]
        public ByteEntryList Diffuse2Indexes { get { return diffuse2Indexes; } set { if (diffuse2Indexes.Equals(value)) { diffuse2Indexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(22), DataGridExpandable]
        public ByteEntryList Specular2Indexes { get { return specular2Indexes; } set { if (specular2Indexes.Equals(value)) { specular2Indexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(23), DataGridExpandable]
        public ByteEntryList BONDIndexes { get { return bondIndexes; } set { if (bondIndexes.Equals(value)) { bondIndexes = new ByteEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }

        [ElementPriority(24)]
        public CountedTGIBlockList TGIBlocks { get { return tgiBlocks; } set { if (!tgiBlocks.Equals(value)) { tgiBlocks = new CountedTGIBlockList(OnResourceChanged, "IGT", value); OnResourceChanged(this, new EventArgs()); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";

                foreach (string field in ContentFields)
                    if (field.Equals("Value") || field.Equals("AsBytes") || field.Equals("Stream")) continue;
                    else if (field.EndsWith("Index"))
                    {
                        s += String.Format("\n{0}: {1} ({2})", field, this[field], tgiBlocks[Convert.ToInt32(this[field].Value)]);
                    }
                    else if (field.EndsWith("Indexes"))
                    {
                        s += String.Format("\n--\n{0}:", field);
                        ByteEntryList list = this[field].Value as ByteEntryList;
                        string fmt = "\n  [{0:X" + list.Count.ToString("X").Length + "}]: 0x{1:X2} ({2})";
                        for (int i = 0; i < list.Count; i++)
                            s += String.Format(fmt, i, list[i], tgiBlocks[list[i]]);
                        s += "\n----";
                    }
                    else if (field.Equals("XmlEntries"))
                    {
                        s += "\n--\nXmlEntries:";
                        string fmt = "\n  [{0:X" + xmlEntries.Count.ToString("X").Length + "}]: {1}";
                        for (int i = 0; i < xmlEntries.Count; i++)
                            s += String.Format(fmt, i, xmlEntries[i].Value);
                        s += "\n----";
                    }
                    else if (field.Equals("OuterEntries"))
                    {
                        s += "\n--\nOuterEntries:";
                        string fmt = "\n--[{0:X" + outerEntries.Count.ToString("X").Length + "}]--{1}";
                        for (int i = 0; i < outerEntries.Count; i++)
                            s += String.Format(fmt, i, outerEntries[i].Value);
                        s += "\n----";
                    }
                    else if (field.Equals("TGIBlocks"))
                    {
                        s += "\n--\nTGIBlocks:";
                        string fmt = "\n  [{0:X" + tgiBlocks.Count.ToString("X").Length + "}]: {1}";
                        for (int i = 0; i < tgiBlocks.Count; i++)
                            s += String.Format(fmt, i, tgiBlocks[i].Value);
                        s += "\n----";
                    }
                    else
                    {
                        s += String.Format("\n{0}: {1}", field, this[field]);
                    }

                return s;
                /**/
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for CASPartResource wrapper
    /// </summary>
    public class CASPartResourceHandler : AResourceHandler
    {
        public CASPartResourceHandler()
        {
            this.Add(typeof(CASPartResource), new List<string>(new string[] { "0x034AEECB", }));
        }
    }
}
