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

namespace CatalogResource
{
    /// <summary>
    /// A resource wrapper that understands Catalog Entry resources
    /// </summary>
    public abstract class CatalogResource : AResource
    {
        protected const Int32 recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        protected static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        protected uint version;
        protected Common common = null;
        #endregion

        #region Constructors
        protected CatalogResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = this.UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; this.Parse(stream); }
        protected CatalogResource(int APIversion, uint version) : base(APIversion, null) { this.version = version; }
        #endregion

        #region Data I/O
        protected virtual void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            version = r.ReadUInt32();
        }

        protected override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            new BinaryWriter(s).Write(version);
            return s;
        }
        #endregion

        #region Sub-classes
        public class Common : AHandlerElement
        {
            #region Attributes
            uint version;
            ulong nameGUID;
            ulong descGUID;
            string name = "";
            string desc = "";
            float price;
            float unknown2;
            byte[] unknown3 = new byte[4];
            BuildBuyProductStatus buildBuyProductStatusFlags;
            ulong pngInstance;
            byte unknown4;
            byte unknown5;
            uint unknown6;
            #endregion

            #region Constructors
            internal Common(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }

            public Common(int APIversion, EventHandler handler, Common basis)
                : this(APIversion, handler,
                basis.version, basis.nameGUID, basis.descGUID, basis.name, basis.desc, basis.price, basis.unknown2, basis.unknown3
                , (byte)basis.buildBuyProductStatusFlags, basis.pngInstance) { }

            public Common(int APIversion, EventHandler handler) : base(APIversion, handler) { }

            public Common(int APIversion, EventHandler handler, uint version, ulong nameGUID, ulong descGUID, string name, string desc, float price, float unknown2,
                byte[] unknown3, byte buildBuyProductStatusFlags, ulong pngInstance)
                : base(APIversion, handler)
            {
                this.version = version;
                this.nameGUID = nameGUID;
                this.descGUID = descGUID;
                this.name = name;
                this.desc = desc;
                this.price = price;
                this.unknown2 = unknown2;
                if (unknown3.Length != this.unknown3.Length) throw new ArgumentLengthException("unknown3", this.unknown3.Length);
                this.unknown3 = (byte[])unknown3.Clone();
                this.buildBuyProductStatusFlags = (BuildBuyProductStatus)buildBuyProductStatusFlags;
                this.pngInstance = pngInstance;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                BinaryReader r2 = new BinaryReader(s, System.Text.Encoding.BigEndianUnicode);

                version = r.ReadUInt32();
                nameGUID = r.ReadUInt64();
                descGUID = r.ReadUInt64();
                name = r2.ReadString();
                //name = System.Text.Encoding.BigEndianUnicode.GetString(r.ReadBytes(r.ReadByte()));
                desc = r2.ReadString();
                //desc = System.Text.Encoding.BigEndianUnicode.GetString(r.ReadBytes(r.ReadByte()));
                price = r.ReadSingle();
                unknown2 = r.ReadSingle();
                unknown3 = r.ReadBytes(4);
                if (checking) if (unknown3.Length != 4)
                        throw new InvalidDataException(String.Format("unknown3: read {0} bytes; expected 4; at 0x{1:X8}", unknown3.Length, s.Position));
                buildBuyProductStatusFlags = (BuildBuyProductStatus)r.ReadByte();
                pngInstance = r.ReadUInt64();
                if (version >= 0x0000000D)
                {
                    unknown4 = r.ReadByte();
                    if (version >= 0x0000000E)
                    {
                        unknown5 = r.ReadByte();
                        if (version >= 0x0000000F)
                        {
                            unknown6 = r.ReadUInt32();
                        }
                    }
                }
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(version);
                w.Write(nameGUID);
                w.Write(descGUID);
                Write7BitStr(s, name, System.Text.Encoding.BigEndianUnicode);
                //w.Write((byte)(name.Length * 2));
                //w.Write(System.Text.Encoding.BigEndianUnicode.GetBytes(name));
                Write7BitStr(s, desc, System.Text.Encoding.BigEndianUnicode);
                //w.Write((byte)(desc.Length * 2));
                //w.Write(System.Text.Encoding.BigEndianUnicode.GetBytes(desc));
                w.Write(price);
                w.Write(unknown2);
                w.Write(unknown3);
                w.Write((byte)buildBuyProductStatusFlags);
                w.Write(pngInstance);
                if (version >= 0x0000000D)
                {
                    w.Write(unknown4);
                    if (version >= 0x0000000E)
                    {
                        w.Write(unknown5);
                        if (version >= 0x0000000F)
                        {
                            w.Write(unknown6);
                        }
                    }
                }
            }
            #endregion

            #region AHandlerElement
            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());;
                    if (version < 0x0000000F)
                    {
                        res.Remove("Unknown6");
                        if (version < 0x0000000E)
                        {
                            res.Remove("Unknown5");
                            if (this.version < 0x0000000D)
                                res.Remove("Unknown4");
                        }
                    }
                    return res;
                }
            }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override AHandlerElement Clone(EventHandler handler) { return new Common(requestedApiVersion, handler, this); }
            #endregion

            #region Sub-classes
            [Flags]
            public enum BuildBuyProductStatus : byte
            {
                ShowInCatalog = 0x01,
                ProductForTesting = 0x02,
                ProductInDevelopment = 0x04,
                ShippingProduct = 0x08,

                DebugProduct = 0x10,
                ProductionProduct = 0x20,
                ObjProductMadeUsingNewEntryScheme = 0x40,
                //
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint Version { get { return version; } set { if (version != value) { version = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ulong NameGUID { get { return nameGUID; } set { if (nameGUID != value) { nameGUID = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ulong DescGUID { get { return descGUID; } set { if (descGUID != value) { descGUID = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public string Name { get { return name; } set { if (name != value) { name = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public string Desc { get { return desc; } set { if (desc != value) { desc = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public float Price { get { return price; } set { if (price != value) { price = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public float Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public byte[] Unknown3
            {
                get { return (byte[])unknown3.Clone(); }
                set
                {
                    if (value.Length != this.unknown3.Length) throw new ArgumentLengthException("Unknown3", this.unknown3.Length);
                    bool same = true;
                    for (int i = 0; same && i < value.Length; i++) same = unknown3[i] == value[i];
                    if (!same)
                    {
                        unknown3 = (byte[])value.Clone();
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(9)]
            public BuildBuyProductStatus BuildBuyProductStatusFlags { get { return buildBuyProductStatusFlags; } set { if (buildBuyProductStatusFlags != value) { buildBuyProductStatusFlags = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public ulong PngInstance { get { return pngInstance; } set { if (pngInstance != value) { pngInstance = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public byte Unknown4
            {
                get { if (version < 0x0000000D) throw new InvalidOperationException(); return unknown4; }
                set { if (version < 0x0000000D) throw new InvalidOperationException(); if (unknown4 != value) { unknown4 = value; OnElementChanged(); } }
            }
            [ElementPriority(12)]
            public byte Unknown5
            {
                get { if (version < 0x0000000E) throw new InvalidOperationException(); return unknown5; }
                set { if (version < 0x0000000E) throw new InvalidOperationException(); if (unknown5 != value) { unknown5 = value; OnElementChanged(); } }
            }
            [ElementPriority(13)]
            public uint Unknown6
            {
                get { if (version < 0x0000000F) throw new InvalidOperationException(); return unknown6; }
                set { if (version < 0x0000000F) throw new InvalidOperationException(); if (unknown6 != value) { unknown6 = value; OnElementChanged(); } }
            }

            public String Value
            {
                get
                {
                    string s = "";
                    foreach (string f in this.ContentFields)
                    {
                        if (f.Equals("Value")) continue;
                        s += String.Format("{0}: {1}\n", f, "" + this[f]);
                    }
                    return s;
                }
            }
            #endregion
        }

        #region TypeCode
        public abstract class TypeCode : AHandlerElement,
            IComparable<TypeCode>, IEqualityComparer<TypeCode>, IEquatable<TypeCode>
        {
            #region Attributes
            protected byte[] prefix = new byte[0];
            #endregion

            #region Constructors
            protected TypeCode(int APIversion, EventHandler handler, byte[] pfx) : base(APIversion, handler) { prefix = pfx == null ? null : (byte[])pfx.Clone(); }
            protected TypeCode(int APIversion, EventHandler handler, Stream s, byte[] pfx) : this(APIversion, handler, pfx) { Parse(s); }

            public static TypeCode CreateTypeCode(int APIversion, EventHandler handler, Stream s, byte[] prefix)
            {
                switch (prefix[1])
                {
                    case 0x01: return new TC01_String(APIversion, handler, s, prefix);
                    case 0x02: return new TC02_ARGB(APIversion, handler, s, prefix);
                    case 0x03: return new TC03_TGIIndex(APIversion, handler, s, prefix);
                    case 0x04: return new TC04_Single(APIversion, handler, s, prefix);
                    case 0x05: return new TC05_XY(APIversion, handler, s, prefix);
                    case 0x06: return new TC06_XYZ(APIversion, handler, s, prefix);
                    case 0x07: return new TC07_Boolean(APIversion, handler, s, prefix);
                }
                throw new InvalidDataException(String.Format("Unknown TypeCode 0x{0:X2} at 0x{1:X8}", prefix[1], s.Position));
            }
            #endregion

            #region Data I/O
            protected abstract void Parse(Stream s);
            internal virtual void UnParse(Stream s)
            {
                if (prefix == null) return;
                BinaryWriter w = new BinaryWriter(s);
                foreach (byte b in prefix) w.Write(b);
            }
            #endregion

            #region IComparable<TypeCode> Members

            public abstract int CompareTo(TypeCode other);

            #endregion

            #region IEqualityComparer<TypeCode> Members

            public virtual bool Equals(TypeCode x, TypeCode y) { if (x.GetType() != y.GetType()) return false; return x.CompareTo(y) == 0; }

            public virtual int GetHashCode(TypeCode obj) { return ((Object)obj).GetHashCode(); }

            public override int GetHashCode() { return GetHashCode(this); }

            #endregion

            #region IEquatable<TypeCode> Members

            public bool Equals(TypeCode other) { return Equals(this, other); }

            #endregion

            #region AApiVersionedFields
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    if (prefix == null) res.Remove("ControlCode");
                    return res;
                }
            }
            #endregion

            #region ContentFields
            [ElementPriority(0)]
            public byte ControlCode { get { return prefix[0]; } set { if (prefix[0] != value) { prefix[0] = value; handler(this, new EventArgs()); } } }

            public String Value
            {
                get
                {
                    string s = "";
                    foreach (string f in this.ContentFields)
                    {
                        if (f.Equals("Value")) continue;
                        if (f.Equals("ControlCode") && prefix == null) continue;
                        s += String.Format("{0}: {1}\n", f, "" + this[f]);
                    }
                    return s;
                }
            }
            #endregion
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x01, }, (byte)0, })]
        public class TC01_String : TypeCode
        {
            static List<string> stringTable = new List<string>(StringTableSingleton.Table);
            #region Attributes
            bool hasString;
            bool twoBytes;
            byte byteValue;
            string stringValue;
            #endregion

            #region Constructors
            internal TC01_String(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC01_String(int APIversion, EventHandler handler, TC01_String basis)
                : base(APIversion, handler, basis.prefix)
            {
                this.hasString = basis.hasString;
                this.twoBytes = basis.twoBytes;
                this.byteValue = basis.byteValue;
                this.stringValue = basis.stringValue == null ? null : (string)basis.stringValue.Clone();
            }

            public TC01_String(int APIversion, EventHandler handler, byte[] prefix, string stringValue)
                : base(APIversion, handler, prefix) { setStringValue(stringValue); }
            public TC01_String(int APIversion, EventHandler handler, byte[] prefix, byte byteValue)
                : base(APIversion, handler, prefix) { setByteValue(byteValue); }
            #endregion

            private void setStringValue(string value)
            {
                int index = stringTable.IndexOf(value);

                if (index < 0)
                {
                    if (value.Length > 0xFF)
                        throw new ArgumentLengthException("value", 0xFF);
                    hasString = true;
                    twoBytes = value.Length > 0x3F;
                    stringValue = value;
                    byteValue = (byte)stringValue.Length;
                }
                else setByteValue((byte)index);
            }

            private void setByteValue(byte value)
            {
                if (value >= stringTable.Count)
                    throw new ArgumentOutOfRangeException("value", value, "Value must not exceed length of table: " + stringTable.Count);
                hasString = false;
                twoBytes = value > 0x3F;
                byteValue = value;
            }

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                byte flags = r.ReadByte();
                twoBytes = (flags & 0x40) != 0;
                hasString = (flags & 0x80) != 0;

                byteValue = twoBytes ? r.ReadByte() : (byte)(flags & 0x3F);
                if (hasString) stringValue = new string(r.ReadChars(byteValue));
            }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);

                byte flags = 0x00;
                if (twoBytes) flags |= 0x40;
                if (hasString) flags |= 0x80;
                if (!twoBytes) flags |= byteValue;
                w.Write(flags);

                if (twoBytes) w.Write(byteValue);
                if (hasString) w.Write(stringValue.ToCharArray());
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC01_String tc = other as TC01_String;
                if (tc == null) return -1;
                return Data.CompareTo(tc.Data);
            }

            public override int GetHashCode(TypeCode obj) { return Data.GetHashCode(); }

            public override AHandlerElement Clone(EventHandler handler) { return new TC01_String(requestedApiVersion, handler, this); }

            public string Data
            {
                get { return hasString ? stringValue : stringTable[byteValue]; }
                set { if (value == Data) return; setStringValue(value); OnElementChanged(); }
            }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x02, }, (byte)0, (byte)0, (byte)0, (byte)0, })]
        public class TC02_ARGB : TypeCode
        {
            #region Attributes
            byte red;
            byte green;
            byte blue;
            byte alpha;
            #endregion

            #region Constructors
            internal TC02_ARGB(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC02_ARGB(int APIversion, EventHandler handler, TC02_ARGB basis)
                : this(APIversion, handler, basis.prefix, basis.red, basis.green, basis.blue, basis.alpha) { }
            public TC02_ARGB(int APIversion, EventHandler handler, byte[] prefix, byte r, byte g, byte b, byte a)
                : base(APIversion, handler, prefix) { red = r; green = g; blue = b; alpha = a; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                red = r.ReadByte();
                green = r.ReadByte();
                blue = r.ReadByte();
                alpha = r.ReadByte();
            }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(red);
                w.Write(green);
                w.Write(blue);
                w.Write(alpha);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC02_ARGB tc = other as TC02_ARGB;
                if (tc == null) return -1;
                return GetHashCode(this).CompareTo(GetHashCode(tc));
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC02_ARGB tc = obj as TC02_ARGB;
                if (tc == null) base.GetHashCode(obj);
                return (((tc.red << 8) + tc.green << 8) + tc.blue << 8) + tc.alpha;
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC02_ARGB(requestedApiVersion, handler, this); }

            [ElementPriority(1)]
            public byte Red { get { return red; } set { if (red != value) { red = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public byte Green { get { return green; } set { if (green != value) { green = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public byte Blue { get { return blue; } set { if (blue != value) { blue = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Alpha { get { return alpha; } set { if (alpha != value) { alpha = value; OnElementChanged(); } } }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x03, }, (byte)0, })]
        public class TC03_TGIIndex : TypeCode
        {
            #region Attributes
            byte tgiIndex;
            #endregion

            #region Constructors
            internal TC03_TGIIndex(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC03_TGIIndex(int APIversion, EventHandler handler, TC03_TGIIndex basis)
                : this(APIversion, handler, basis.prefix, basis.tgiIndex) { }
            public TC03_TGIIndex(int APIversion, EventHandler handler, byte[] prefix, byte tgiIndex)
                : base(APIversion, handler, prefix) { this.tgiIndex = tgiIndex; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { tgiIndex = (new BinaryReader(s)).ReadByte(); }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(tgiIndex);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC03_TGIIndex tc = other as TC03_TGIIndex;
                if (tc == null) return -1;
                return tgiIndex.CompareTo(tc.tgiIndex);
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC03_TGIIndex tc = obj as TC03_TGIIndex;
                if (tc == null) base.GetHashCode(obj);
                return tgiIndex.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC03_TGIIndex(requestedApiVersion, handler, this); }

            public byte TGIIndex { get { return tgiIndex; } set { if (tgiIndex != value) { tgiIndex = value; OnElementChanged(); } } }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x04, }, (float)0, })]
        public class TC04_Single : TypeCode
        {
            #region Attributes
            float unknown1;
            #endregion

            #region Constructors
            internal TC04_Single(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC04_Single(int APIversion, EventHandler handler, TC04_Single basis)
                : this(APIversion, handler, basis.prefix, basis.unknown1) { }
            public TC04_Single(int APIversion, EventHandler handler, byte[] prefix, float unknown1)
                : base(APIversion, handler, prefix) { this.unknown1 = unknown1; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { unknown1 = (new BinaryReader(s)).ReadSingle(); }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC04_Single tc = other as TC04_Single;
                if (tc == null) return -1;
                return unknown1.CompareTo(tc.unknown1);
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC04_Single tc = obj as TC04_Single;
                if (tc == null) base.GetHashCode(obj);
                return unknown1.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC04_Single(requestedApiVersion, handler, this); }

            public float Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x05, }, (float)0, (float)0, })]
        public class TC05_XY : TypeCode
        {
            #region Attributes
            float unknown1;
            float unknown2;
            #endregion

            #region Constructors
            internal TC05_XY(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC05_XY(int APIversion, EventHandler handler, TC05_XY basis)
                : this(APIversion, handler, basis.prefix, basis.unknown1, basis.unknown2) { }
            public TC05_XY(int APIversion, EventHandler handler, byte[] prefix, float unknown1, float unknown2)
                : base(APIversion, handler, prefix) { this.unknown1 = unknown1; this.unknown2 = unknown2; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadSingle();
                unknown2 = r.ReadSingle();
            }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
                w.Write(unknown2);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC05_XY tc = other as TC05_XY;
                if (tc == null) return -1;
                return GetHashCode(this).CompareTo(GetHashCode(tc));
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC05_XY tc = obj as TC05_XY;
                if (tc == null) base.GetHashCode(obj);
                return tc.unknown1.GetHashCode() ^ tc.unknown2.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC05_XY(requestedApiVersion, handler, this); }

            public float Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            public float Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x06, }, (float)0, (float)0, (float)0, })]
        public class TC06_XYZ : TypeCode
        {
            #region Attributes
            float unknown1;
            float unknown2;
            float unknown3;
            #endregion

            #region Constructors
            internal TC06_XYZ(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC06_XYZ(int APIversion, EventHandler handler, TC06_XYZ basis)
                : this(APIversion, handler, basis.prefix, basis.unknown1, basis.unknown2, basis.unknown3) { }
            public TC06_XYZ(int APIversion, EventHandler handler, byte[] prefix, float unknown1, float unknown2, float unknown3)
                : base(APIversion, handler, prefix) { this.unknown1 = unknown1; this.unknown2 = unknown2; this.unknown3 = unknown3; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadSingle();
                unknown2 = r.ReadSingle();
                unknown3 = r.ReadSingle();
            }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC06_XYZ tc = other as TC06_XYZ;
                if (tc == null) return -1;
                return GetHashCode(this).CompareTo(GetHashCode(tc));
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC06_XYZ tc = obj as TC06_XYZ;
                if (tc == null) base.GetHashCode(obj);
                return tc.unknown1.GetHashCode() ^ tc.unknown2.GetHashCode() ^ tc.unknown3.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC06_XYZ(requestedApiVersion, handler, this); }

            public float Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            public float Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            public float Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
        }

        [ConstructorParameters(new object[] { new byte[] { 0, 0x07, }, (byte)0, })]
        public class TC07_Boolean : TypeCode
        {
            #region Attributes
            byte unknown1;
            #endregion

            #region Constructors
            internal TC07_Boolean(int APIversion, EventHandler handler, Stream s, byte[] prefix) : base(APIversion, handler, s, prefix) { }

            public TC07_Boolean(int APIversion, EventHandler handler, TC07_Boolean basis)
                : this(APIversion, handler, basis.prefix, basis.unknown1) { }
            public TC07_Boolean(int APIversion, EventHandler handler, byte[] prefix, byte unknown1)
                : base(APIversion, handler, prefix) { this.unknown1 = unknown1; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { unknown1 = (new BinaryReader(s)).ReadByte(); }

            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                (new BinaryWriter(s)).Write(unknown1);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC07_Boolean tc = other as TC07_Boolean;
                if (tc == null) return -1;
                return unknown1.CompareTo(tc.unknown1);
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC07_Boolean tc = obj as TC07_Boolean;
                if (tc == null) base.GetHashCode(obj);
                return unknown1.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC07_Boolean(requestedApiVersion, handler, this); }

            public byte Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
        }

        /*[ConstructorParameters(new object[] { (byte)0x2F, (byte)0, (uint)0, })]
        public class TypeCode2F : TypeCode
        {
            #region Attributes
            byte unknown1;
            uint unknown2;
            #endregion

            #region Constructors
            internal TypeCode2F(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s, null) { }

            public TypeCode2F(int APIversion, EventHandler handler, TypeCode2F basis)
                : this(APIversion, handler, 0x2F, basis.unknown1, basis.unknown2) { }
            public TypeCode2F(int APIversion, EventHandler handler, byte tc, byte unknown1, uint unknown2)
                : base(APIversion, handler, null) { this.unknown1 = unknown1; this.unknown2 = unknown2; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadByte();
                unknown2 = r.ReadUInt32();
            }

            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)0x2F);
                w.Write(unknown1);
                w.Write(unknown2);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TypeCode2F tc = other as TypeCode2F;
                if (tc == null) return -1;
                return GetHashCode(this).CompareTo(GetHashCode(tc));
            }

            public override int GetHashCode(TypeCode obj)
            {
                TypeCode2F tc = obj as TypeCode2F;
                if (tc == null) base.GetHashCode(obj);
                return unknown1.GetHashCode() ^ unknown2.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TypeCode2F(requestedApiVersion, handler, this); }

            public byte Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
        }/**/

        [ConstructorParameters(new object[] { (byte)0x40, (int)0, })]
        public class TC_Padding : TypeCode
        {
            #region Attributes
            int length;
            #endregion

            #region Constructors
            internal TC_Padding(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s, null) { }

            public TC_Padding(int APIversion, EventHandler handler, TC_Padding basis)
                : this(APIversion, handler, 0x40, basis.length) { }
            private TC_Padding(int APIversion, EventHandler handler, byte tc, int length)
                : base(APIversion, handler, null) { this.length = length; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                length = 1;
                while (r.PeekChar() == 0x40) { length++; r.ReadChar(); }
            }

            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                for (int i = 0; i < length; i++) w.Write((byte)0x40);
            }
            #endregion

            public override int CompareTo(TypeCode other)
            {
                TC_Padding tc = other as TC_Padding;
                if (tc == null) return -1;
                return length.CompareTo(tc.length);
            }

            public override int GetHashCode(TypeCode obj)
            {
                TC_Padding tc = obj as TC_Padding;
                if (tc == null) base.GetHashCode(obj);
                return length.GetHashCode();
            }

            public override AHandlerElement Clone(EventHandler handler) { return new TC_Padding(requestedApiVersion, handler, this); }

            public int Length { get { return length; } set { if (length != value) { length = value; OnElementChanged(); } } }
        }

        public class TypeCodeList : AResource.DependentList<TypeCode>
        {
            #region Constructors
            public TypeCodeList(EventHandler handler) : base(handler) { }
            public TypeCodeList(EventHandler handler, IList<TypeCode> ltc) : base(handler, ltc) { }
            public TypeCodeList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region Data I/O
            protected override TypeCode CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override TypeCode CreateElement(Stream s, out bool inc)
            {
                BinaryReader r = new BinaryReader(s);
                byte controlCode = r.ReadByte();
                switch (controlCode)
                {
                    case 0x40: inc = false; return new TC_Padding(0, handler, s);
                    //case 0x2F: inc = true; return new TypeCode2F(0, handler, s);
                    default: inc = true; return TypeCode.CreateTypeCode(0, elementHandler, s, new byte[] { controlCode, r.ReadByte() });
                }
            }

            protected override void WriteCount(Stream s, uint count) { foreach (TypeCode tc in this) if (tc is TC_Padding) count--; (new BinaryWriter(s)).Write(count); }
            protected override void WriteElement(Stream s, TypeCode element) { element.UnParse(s); }
            #endregion

            #region Content Fields
            public String Value { get { string s = ""; for (int i = 0; i < Count; i++) s += string.Format("\n--{0}: {1}--\n", i, this[i].GetType().Name) + this[i].Value; return s; } }
            #endregion

            public override void Add() { throw new NotImplementedException(); }

            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(TypeCode).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();

                if (fields[0].GetType().Equals(typeof(byte)))
                {
                    if ((byte)fields[0] == 0x40) return typeof(TC_Padding);
                    //else if ((byte)fields[0] == 0x2F) return typeof(TypeCode2F);
                }
                else if (fields[0].GetType().Equals(typeof(byte[])))
                {
                    switch (((byte[])fields[0])[1])
                    {
                        case 0x01: return typeof(TC01_String);
                        case 0x02: return typeof(TC02_ARGB);
                        case 0x03: return typeof(TC03_TGIIndex);
                        case 0x04: return typeof(TC04_Single);
                        case 0x05: return typeof(TC05_XY);
                        case 0x06: return typeof(TC06_XYZ);
                        case 0x07: return typeof(TC07_Boolean);
                    }
                    throw new InvalidDataException(String.Format("Unknown TypeCode 0x{0:X2}", ((byte[])fields[0])[1]));
                }
                throw new ArgumentException();
            }
        }
        #endregion

        public class MaterialBlock : AHandlerElement,
            IComparable<MaterialBlock>, IEqualityComparer<MaterialBlock>, IEquatable<MaterialBlock>
        {
            #region Attributes
            byte xmlindex;
            TC01_String unknown1 = null;
            TC01_String unknown2 = null;
            TypeCodeList tcList = null;
            MaterialBlockList mbList = null;
            #endregion

            #region Constructors
            public MaterialBlock(int APIversion, EventHandler handler)
                : base(APIversion, handler)
            {
                unknown1 = new TC01_String(0, null, null, 0);
                unknown2 = new TC01_String(0, null, null, 0);
                tcList = new TypeCodeList(handler);
                mbList = new MaterialBlockList(handler);
            }

            internal MaterialBlock(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }

            public MaterialBlock(int APIversion, EventHandler handler, MaterialBlock basis)
                : this(APIversion, handler, basis.xmlindex, basis.unknown1, basis.unknown2, basis.tcList, basis.mbList) { }

            public MaterialBlock(int APIversion, EventHandler handler, byte xmlindex, TC01_String unknown1, TC01_String unknown2,
                IList<TypeCode> ltc, IList<MaterialBlock> lmb)
                : base(APIversion, handler)
            {
                this.handler = handler;
                this.xmlindex = xmlindex;
                this.unknown1 = (TC01_String)unknown1.Clone(handler);
                this.unknown2 = (TC01_String)unknown2.Clone(handler);
                tcList = new TypeCodeList(handler, ltc);
                mbList = new MaterialBlockList(handler, lmb);
            }
            #endregion

            #region Data I/O
            protected void Parse(Stream s)
            {
                this.xmlindex = (new BinaryReader(s)).ReadByte();
                this.unknown1 = new TC01_String(requestedApiVersion, handler, s, null);
                this.unknown2 = new TC01_String(requestedApiVersion, handler, s, null);
                this.tcList = new TypeCodeList(handler, s);
                this.mbList = new MaterialBlockList(handler, s);
            }

            public void UnParse(Stream s)
            {
                (new BinaryWriter(s)).Write(xmlindex);
                unknown1.UnParse(s);
                unknown2.UnParse(s);
                tcList.UnParse(s);
                mbList.UnParse(s);
            }
            #endregion

            #region IComparable<MaterialBlock> Members

            public int CompareTo(MaterialBlock other)
            {
                int res = xmlindex.CompareTo(other.xmlindex); if (res != 0) return res;
                res = unknown1.CompareTo(other.unknown1); if (res != 0) return res;
                res = unknown2.CompareTo(other.unknown2); if (res != 0) return res;
                res = tcList.Count.CompareTo(other.tcList.Count); if (res != 0) return res;
                for (int i = 0; i < tcList.Count; i++) { res = tcList[i].CompareTo(other.tcList[i]); if (res != 0) return res; }
                res = mbList.Count.CompareTo(other.mbList.Count); if (res != 0) return res;
                for (int i = 0; i < mbList.Count; i++) { res = mbList[i].CompareTo(other.mbList[i]); if (res != 0) return res; }
                return 0;
            }

            #endregion

            #region IEqualityComparer<MaterialBlock> Members

            public bool Equals(MaterialBlock x, MaterialBlock y) { return x.Equals(y); }

            public int GetHashCode(MaterialBlock obj) { return obj.GetHashCode(); }

            public override int GetHashCode()
            {
                int hc = xmlindex.GetHashCode() ^ unknown1.GetHashCode() ^ unknown2.GetHashCode();
                foreach (TypeCode tc in tcList) hc ^= tc.GetHashCode();
                foreach (MaterialBlock mb in mbList) hc ^= mb.GetHashCode();
                return hc;
            }

            #endregion

            #region IEquatable<MaterialBlock> Members

            public bool Equals(MaterialBlock other) { return this.CompareTo(other) == 0; }

            #endregion

            #region ICloneable Members

            public object Clone() { return new MaterialBlock(requestedApiVersion, handler, this); }

            #endregion

            #region AHandlerElement
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override AHandlerElement Clone(EventHandler handler) { return new MaterialBlock(requestedApiVersion, handler, this); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public byte XMLIndex { get { return xmlindex; } set { if (xmlindex != value) { xmlindex = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public TC01_String Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public TC01_String Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public TypeCodeList TypeCodes { get { return tcList; } set { if (tcList != (value as TypeCodeList)) { tcList = new TypeCodeList(handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public MaterialBlockList MaterialBlocks { get { return mbList; } set { if (mbList != (value as MaterialBlockList)) { mbList = new MaterialBlockList(handler, value); OnElementChanged(); } } }

            public String Value
            {
                get
                {
                    string s = "";
                    foreach (string f in this.ContentFields)
                    {
                        if (f.Equals("Value")) continue;
                        TypedValue tv = this[f];
                        string h = String.Format("\n---------\n---------\n{0}: {1}\n---------\n", tv.Type.Name, f);
                        string t = "---------\n";
                        if (typeof(TC01_String).IsAssignableFrom(tv.Type)) s += h + (tv.Value as TC01_String).Value + t;
                        else if (typeof(TypeCodeList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as TypeCodeList).Value + t;
                        else if (typeof(MaterialBlockList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as MaterialBlockList).Value + t;
                        else s += string.Format("{0}: {1}\n", f, "" + tv);
                    }
                    return s;
                }
            }
            #endregion
        }

        public class MaterialBlockList : AResource.DependentList<MaterialBlock>
        {
            #region Constructors
            public MaterialBlockList(EventHandler handler) : base(handler) { }
            public MaterialBlockList(EventHandler handler, IList<MaterialBlock> lmb) : base(handler, lmb) { }
            internal MaterialBlockList(EventHandler handler, Stream s) : base(handler, s) { }
            #endregion

            #region Data I/O
            protected override MaterialBlock CreateElement(Stream s) { return new MaterialBlock(0, elementHandler, s); }
            protected override void WriteElement(Stream s, MaterialBlock element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new MaterialBlock(0, null)); }

            #region Content Fields
            public String Value { get { string s = ""; for (int i = 0; i < Count; i++) s += string.Format("\n--{0}--\n", i) + this[i].Value; return s; } }
            #endregion
        }

        public class Material : AHandlerElement,
            IComparable<Material>, IEqualityComparer<Material>, IEquatable<Material>
        {
            #region Attributes
            byte materialType;
            uint unknown1;
            ushort unknown2;
            MaterialBlock mb = null;
            TGIBlockList list = null;
            uint unknown3;
            #endregion

            #region Constructors
            public Material(int APIversion, EventHandler handler)
                : base(APIversion, handler)
            {
                mb = new MaterialBlock(requestedApiVersion, handler);
                list = new TGIBlockList(handler);
            }
            internal Material(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Material(int APIversion, EventHandler handler, Material basis)
                : base(APIversion, handler)
            {
                this.materialType = basis.materialType;
                this.unknown1 = basis.unknown1;
                this.unknown2 = basis.unknown2;
                this.mb = (MaterialBlock)basis.mb.Clone(handler);
                this.list = new TGIBlockList(handler, basis.list);
                this.unknown3 = basis.unknown3;
            }
            public Material(int APIversion, EventHandler handler, byte materialType, uint unknown1, ushort unknown2,
                MaterialBlock mb, IList<TGIBlock> ltgib, uint unknown3)
                : base(APIversion, handler)
            {
                this.materialType = materialType;
                this.unknown1 = unknown1;
                this.unknown2 = unknown2;
                this.mb = (MaterialBlock)mb.Clone(handler);
                this.list = new TGIBlockList(handler, ltgib);
                this.unknown3 = unknown3;
            }
            #endregion

            #region Data I/O
            protected virtual void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                materialType = r.ReadByte();
                if (materialType != 1) unknown1 = r.ReadUInt32();
                long oset = r.ReadUInt32() + s.Position;
                unknown2 = r.ReadUInt16();
                long tgiPosn = r.ReadUInt32() + s.Position;
                long tgiSize = r.ReadUInt32();

                mb = new MaterialBlock(requestedApiVersion, handler, s);

                list = new TGIBlockList(handler, s, tgiPosn, tgiSize);

                if (checking) if (oset != s.Position)
                        throw new InvalidDataException(String.Format("Position of final DWORD read: 0x{0:X8}, actual: 0x{1:X8}",
                            oset, s.Position));

                unknown3 = r.ReadUInt32();
            }

            public virtual void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                long pOset, ptgiO, pos;

                w.Write(materialType);
                if (materialType != 1) w.Write(unknown1);

                pOset = s.Position;
                w.Write((uint)0); // oset

                w.Write(unknown2);

                ptgiO = s.Position;
                w.Write((uint)0); // tgiOffset
                w.Write((uint)0); // tgiSize

                mb.UnParse(s);

                list.UnParse(s, ptgiO);

                pos = s.Position;
                s.Position = pOset;
                w.Write((uint)(pos - pOset - sizeof(uint)));

                s.Position = pos;
                w.Write(unknown3);
            }
            #endregion

            #region IComparable<Material> Members

            public int CompareTo(Material other)
            {
                int res = materialType.CompareTo(other.materialType); if (res != 0) return res;
                res = unknown2.CompareTo(other.unknown2); if (res != 0) return res;
                res = mb.CompareTo(other.mb); if (res != 0) return res;
                return unknown3.CompareTo(other.unknown3);
            }

            #endregion

            #region IEqualityComparer<Material> Members

            public bool Equals(Material x, Material y) { return x.Equals(y); }

            public int GetHashCode(Material obj) { return obj.GetHashCode(); }

            public override int GetHashCode() { return materialType.GetHashCode() ^ unknown2.GetHashCode() ^ mb.GetHashCode() ^ unknown3.GetHashCode(); }

            #endregion

            #region IEquatable<Material> Members

            public bool Equals(Material other) { return this.CompareTo(other) == 0; }

            #endregion

            #region ICloneable Members

            public object Clone() { return new Material(requestedApiVersion, handler, this); }

            #endregion

            #region AHandlerElement
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override AHandlerElement Clone(EventHandler handler) { return new Material(requestedApiVersion, handler, this); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public byte MaterialType { get { return materialType; } set { if (materialType != value) { materialType = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown1 { get { return unknown1; } set { if (materialType == 1) throw new InvalidOperationException(); if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ushort Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public MaterialBlock MaterialBlock { get { return mb; } set { if (mb != value) { mb = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public TGIBlockList TGIBlocks
            {
                get { return list; }
                set
                {
                    if (list != (value as TGIBlockList))
                    {
                        list = new TGIBlockList(handler, value);
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(6)]
            public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }

            public String Value
            {
                get
                {
                    string s = "";
                    foreach (string f in this.ContentFields)
                    {
                        if (f.Equals("Value")) continue;
                        TypedValue tv = this[f];
                        string h = String.Format("\n---------\n---------\n{0}: {1}\n---------\n", tv.Type.Name, f);
                        string t = "---------\n";
                        if (typeof(MaterialBlock).IsAssignableFrom(tv.Type)) s += h + (tv.Value as MaterialBlock).Value + t;
                        else if (typeof(TGIBlockList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as TGIBlockList).Value + t;
                        else s += string.Format("{0}: {1}\n", f, "" + tv);
                    }
                    return s;
                }
            }
            #endregion
        }

        public class MaterialList : AResource.DependentList<Material>
        {
            #region Constructors
            internal MaterialList(EventHandler handler) : base(handler) { }
            internal MaterialList(EventHandler handler, Stream s) : base(handler, s) { }
            public MaterialList(EventHandler handler, IList<Material> lme) : base(handler, lme) { }
            #endregion

            #region Data I/O
            protected override Material CreateElement(Stream s) { return new Material(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Material element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new Material(0, null)); }

            #region Content Fields
            public String Value { get { string s = ""; for (int i = 0; i < Count; i++) s += string.Format("\n--{0}--\n", i) + this[i].Value; return s; } }
            #endregion
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(11)]
        public Common CommonBlock { get { return common; } set { if (common != value) { common = new Common(requestedApiVersion, OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }

        public virtual String Value
        {
            get
            {
                string s = "";
                string hdr = "\n---------\n---------\n{0}: {1}\n---------\n";
                string t = "---------\n";
                s += String.Format(hdr, "Common", "CommonBlock") + this.CommonBlock.Value + t;
                foreach (string f in this.ContentFields)
                {
                    if (f.Equals("Value") || f.Equals("Stream") || f.Equals("AsBytes")) continue;
                    TypedValue tv = this[f];
                    string h = String.Format(hdr, tv.Type.Name, f);
                    if (tv.Type.HasElementType && typeof(AApiVersionedFields).IsAssignableFrom(tv.Type.GetElementType())) // it's an array
                            s += h + tv + "\n" + t;
                    else if (typeof(Common).IsAssignableFrom(tv.Type)) { }
                    else if (typeof(WallFloorPatternCatalogResource.WallFloorPatternMaterialList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as WallFloorPatternCatalogResource.WallFloorPatternMaterialList).Value + t;
                    else if (typeof(MaterialList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as MaterialList).Value + t;
                    else if (typeof(TGIBlockList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as TGIBlockList).Value + t;
                    else if (typeof(ObjectCatalogResource.MTDoorList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as ObjectCatalogResource.MTDoorList).Value + t;
                    else if (typeof(ObjectCatalogResource.UIntList).IsAssignableFrom(tv.Type)) s += h + (tv.Value as ObjectCatalogResource.UIntList).Value + t;
                    else s += string.Format("{0}: {1}\n", f, "" + tv);
                }
                return s;
            }
        }
        #endregion
    }

    /// <summary>
    /// A CatalogResource wrapper that contains a TGIBlockList
    /// </summary>
    public abstract class CatalogResourceTGIBlockList : CatalogResource
    {
        #region Attributes
        protected long tgiPosn, tgiSize;
        protected TGIBlockList list = null;
        #endregion

        #region Constructors
        public CatalogResourceTGIBlockList(int APIversion, Stream s) : base(APIversion, s) { }
        public CatalogResourceTGIBlockList(int APIversion, uint version, IList<TGIBlock> tgibl) : base(APIversion, version) { this.list = new TGIBlockList(OnResourceChanged, tgibl); }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            base.Parse(s);
            BinaryReader r = new BinaryReader(s);
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();
        }

        private long pos;
        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);
            pos = s.Position;
            w.Write((uint)0); // tgiOffset
            w.Write((uint)0); // tgiSize
            return s;
        }

        protected virtual void UnParse(Stream s)
        {
            if (list == null) list = new TGIBlockList(OnResourceChanged);
            list.UnParse(s, pos);
        }
        #endregion

        #region Content Fields
        public TGIBlockList TGIBlocks { get { return list; } set { if (list != value) { list = new TGIBlockList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for CatalogResource wrapper
    /// </summary>
    public class CatalogResourceHandler : AResourceHandler
    {
        public CatalogResourceHandler()
        {
            this.Add(typeof(FenceCatalogResource), new List<string>(new string[] { "0x0418FE2A" }));
            this.Add(typeof(FireplaceCatalogResource), new List<string>(new string[] { "0x04F3CC01" }));
            this.Add(typeof(ObjectCatalogResource), new List<string>(new string[] { "0x319E4F1D" }));
            this.Add(typeof(ProxyProductCatalogResource), new List<string>(new string[] { "0x04AC5D93" }));
            this.Add(typeof(RailingCatalogResource), new List<string>(new string[] { "0x04C58103" }));
            this.Add(typeof(RoofStyleCatalogResource), new List<string>(new string[] { "0x91EDBD3E" }));
            this.Add(typeof(RoofPatternCatalogResource), new List<string>(new string[] { "0xF1EDBD86" }));
            this.Add(typeof(StairsCatalogResource), new List<string>(new string[] { "0x049CA4CD" }));
            this.Add(typeof(TerrainPaintBrushCatalogResource), new List<string>(new string[] { "0x04ED4BB2" }));
            this.Add(typeof(TerrainGeometryWaterBrushCatalogResource), new List<string>(new string[] { "0x04B30669", "0x060B390C" }));
            this.Add(typeof(WallFloorPatternCatalogResource), new List<string>(new string[] { "0x515CA4CD" }));
            this.Add(typeof(WallCatalogResource), new List<string>(new string[] { "0x9151E6BC" }));

        }
    }
}
