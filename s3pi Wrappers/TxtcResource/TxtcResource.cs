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

namespace TxtcResource
{
    /// <summary>
    /// A resource wrapper that understands Texture Compositor resources
    /// </summary>
    public class TxtcResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        ContentType root;
        #endregion

        public TxtcResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s) { root = new ContentType(requestedApiVersion, OnResourceChanged, s); }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            if (root == null) root = new ContentType(requestedApiVersion, OnResourceChanged);
            root.UnParse(ms);
            return ms;
        }
        #endregion

        #region Sub-classes
        public enum PatternSizeType : uint
        {
            Default = 0x00,
            Large = 0x01,
        }
        //Copied from CASPartFlags - TODO: resolve!
        [Flags]
        public enum DataTypeFlags : uint
        {
            Hair = 0x00000001,
            Scalp = 0x00000002,
            FaceOverlay = 0x00000004,
            Body = 0x00000008,
            Accessory = 0x00000010,
        }

        public class ContentType : AHandlerElement, IEquatable<ContentType>
        {
            const int recommendedApiVersion = 1;
            void SetTGIBlocks() { if (superBlocks != null) superBlocks.ParentTGIBlocks = tgiBlocks; if (entries != null) entries.ParentTGIBlocks = tgiBlocks; }
            internal void SetTGIBlocks(DependentList<TGIBlock> parentTGIBlocks) { if (superBlocks != null) superBlocks.ParentTGIBlocks = parentTGIBlocks; if (entries != null) entries.ParentTGIBlocks = parentTGIBlocks; }

            #region Attributes
            uint version;
            SuperBlockList superBlocks;//if version >= 7
            PatternSizeType patternSize;
            DataTypeFlags dataType;
            byte unknown3;
            byte unknown4;//if version >= 8
            EntryBlockList entries;
            CountedTGIBlockList tgiBlocks;
            #endregion

            #region Constructors
            public ContentType(int APIversion, EventHandler handler) : base(APIversion, handler)
            {
                superBlocks = new SuperBlockList(handler);
                entries = new EntryBlockList(handler);
                tgiBlocks = new CountedTGIBlockList(handler, byte.MaxValue);

                SetTGIBlocks();
            }
            public ContentType(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public ContentType(int APIversion, EventHandler handler, ContentType basis)
                : this(APIversion, handler,
                basis.version, basis.superBlocks, basis.patternSize, basis.dataType, basis.unknown3, basis.unknown4, basis.entries, basis.tgiBlocks
                ) { }
            public ContentType(int APIversion, EventHandler handler,
                uint version, SuperBlockList superBlocks, PatternSizeType patternSize, DataTypeFlags dataType, byte unknown3, byte unknown4, EntryBlockList entries, CountedTGIBlockList tgiBlocks
                )
                : base(APIversion, handler)
            {
                this.version = version;
                this.superBlocks = superBlocks == null ? null : new SuperBlockList(handler, superBlocks);
                this.patternSize = patternSize;
                this.dataType = dataType;
                this.unknown3 = unknown3;
                this.unknown4 = unknown4;
                this.entries = new EntryBlockList(handler, entries);
                this.tgiBlocks = tgiBlocks == null ? null : new CountedTGIBlockList(handler, tgiBlocks);

                SetTGIBlocks();
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                version = r.ReadUInt32();
                long tgiPos = r.ReadUInt32() + s.Position;
                if (version >= 7)
                    superBlocks = new SuperBlockList(handler, s);
                patternSize = (PatternSizeType)r.ReadUInt32();
                dataType = (DataTypeFlags)r.ReadUInt32();
                unknown3 = r.ReadByte();
                int count = r.ReadInt32();
                if (version >= 8)
                    unknown4 = r.ReadByte();
                entries = new EntryBlockList(handler, count, s);
                if (checking) if (tgiPos != s.Position)
                        throw new InvalidDataException(string.Format("TGI Block found at 0x{0:X8}; expected position 0x{1:X8}", s.Position, tgiPos));
                tgiBlocks = new CountedTGIBlockList(handler, "IGT", r.ReadByte(), s, Byte.MaxValue);

                SetTGIBlocks();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(version);
                long osetPos = s.Position;
                w.Write((uint)0);
                if (version >= 7)
                {
                    if (superBlocks == null) superBlocks = new SuperBlockList(handler);
                    superBlocks.UnParse(s);
                }
                w.Write((uint)patternSize);
                w.Write((uint)dataType);
                w.Write(unknown3);
                if (entries == null) entries = new EntryBlockList(handler);
                w.Write(entries.Count);
                if (version >= 8)
                    w.Write(unknown4);
                entries.UnParse(s);
                if (tgiBlocks == null) tgiBlocks = new CountedTGIBlockList(handler, "IGT", Byte.MaxValue);
                long tgiPosn = s.Position;
                w.Write((byte)tgiBlocks.Count);
                tgiBlocks.UnParse(s);

                long posn = s.Position;
                s.Position = osetPos;
                w.Write((uint)(tgiPosn - osetPos - sizeof(uint)));
                s.Position = posn;
            }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new ContentType(requestedApiVersion, handler, this); }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    if (version < 8)
                    {
                        res.Remove("Unknown4");
                        if (version < 7)
                        {
                            res.Remove("SuperBlocks");
                        }
                    }
                    return res;
                }
            }
            #endregion

            #region IEquatable<ContentType> Members

            public bool Equals(ContentType other)
            {
                return version == other.version
                    && superBlocks.Equals(other.superBlocks)
                    && patternSize.Equals(other.patternSize)
                    && dataType.Equals(other.dataType)
                    && unknown3.Equals(other.unknown3)
                    && unknown4.Equals(other.unknown4)
                    && entries.Equals(other.entries)
                    && tgiBlocks.Equals(other.tgiBlocks)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as ContentType != null ? this.Equals(obj as ContentType) : false;
            }

            public override int GetHashCode()
            {
                return version.GetHashCode()
                    ^ superBlocks.GetHashCode()
                    ^ patternSize.GetHashCode()
                    ^ dataType.GetHashCode()
                    ^ unknown3.GetHashCode()
                    ^ unknown4.GetHashCode()
                    ^ entries.GetHashCode()
                    ^ tgiBlocks.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint Version { get { return version; } set { if (version != value) { version = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public SuperBlockList SuperBlocks
            {
                get { if (version < 0x00000007) throw new InvalidOperationException(); return superBlocks; }
                set { if (version < 0x00000007) throw new InvalidOperationException(); if (superBlocks != value) { superBlocks = new SuperBlockList(handler, value) { ParentTGIBlocks = tgiBlocks }; OnElementChanged(); } }
            }
            [ElementPriority(3)]
            public PatternSizeType PatternSize { get { return patternSize; } set { if (patternSize != value) { patternSize = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public DataTypeFlags DataType { get { return dataType; } set { if (dataType != value) { dataType = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public byte Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public byte Unknown4
            {
                get { if (version < 0x00000008) throw new InvalidOperationException(); return unknown4; }
                set { if (version < 0x00000008) throw new InvalidOperationException(); if (unknown4 != value) { unknown4 = value; OnElementChanged(); } }
            }
            [ElementPriority(7)]
            public EntryBlockList Entries { get { return entries; } set { if (entries != value) { entries = new EntryBlockList(handler, value) { ParentTGIBlocks = tgiBlocks }; OnElementChanged(); } } }
            [ElementPriority(8)]
            public CountedTGIBlockList TGIBlocks { get { return tgiBlocks; } set { if (tgiBlocks != value) { tgiBlocks = new CountedTGIBlockList(handler, "IGT", value); SetTGIBlocks(); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class SuperBlock : AHandlerElement, IEquatable<SuperBlock>
        {
            const int recommendedApiVersion = 1;

            DependentList<TGIBlock> _ParentTGIBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get { return _ParentTGIBlocks; }
                set { if (_ParentTGIBlocks != value) { _ParentTGIBlocks = value; fabric.SetTGIBlocks(value); } }
            }
            public override List<string> ContentFields { get { List<string> res = GetContentFields(requestedApiVersion, this.GetType()); res.Remove("ParentTGIBlocks"); return res; } }

            #region Attributes
            byte tgiIndex;
            ContentType fabric;
            byte unknown1;
            byte unknown2;
            byte unknown3;
            #endregion

            #region Constructors
            public SuperBlock(int APIversion, EventHandler handler) : base(APIversion, handler) { fabric = new ContentType(requestedApiVersion, handler); }
            public SuperBlock(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public SuperBlock(int APIversion, EventHandler handler, SuperBlock basis)
                : this(APIversion, handler, basis.tgiIndex, basis.fabric) { }
            public SuperBlock(int APIversion, EventHandler handler, byte tgiIndex, ContentType fabric)
                : base(APIversion, handler)
            {
                this.tgiIndex = tgiIndex;
                this.fabric = new ContentType(requestedApiVersion, handler, fabric);
            }
            #endregion

            #region Data I/O
            // http://simswiki.info/wiki.php?title=Sims_3:0x033A1435
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                tgiIndex = r.ReadByte();
                uint offset = r.ReadUInt32();
                long posn2 = s.Position;

                fabric = new ContentType(requestedApiVersion, handler, s);
                unknown1 = r.ReadByte();
                unknown2 = r.ReadByte();
                unknown3 = r.ReadByte();

                if (posn2 + offset != s.Position)
                    throw new InvalidDataException(
                        "Unexpected data around position 0x" + posn2.ToString("X8") +
                        "; read 0x" + (s.Position - posn2).ToString("X8") + " bytes" +
                        "; expected 0x" + offset.ToString("X8") + " bytes."
                        );
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(tgiIndex);

                long before = s.Position;
                w.Write((uint)0);

                long posn = s.Position;
                fabric.UnParse(s);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
                long after = s.Position;

                uint offset = (uint)(after - posn);

                s.Position = before;
                w.Write(offset);
                s.Position = after;
            }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new SuperBlock(requestedApiVersion, handler, this) { ParentTGIBlocks = ParentTGIBlocks }; }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            #endregion

            #region IEquatable<SuperBlock> Members

            public bool Equals(SuperBlock other)
            {
                return tgiIndex == other.tgiIndex
                    && fabric.Equals(other.fabric)
                    && unknown1.Equals(other.unknown1)
                    && unknown2.Equals(other.unknown2)
                    && unknown3.Equals(other.unknown3)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as SuperBlock != null ? this.Equals(obj as SuperBlock) : false;
            }

            public override int GetHashCode()
            {
                return tgiIndex.GetHashCode()
                    ^ fabric.GetHashCode()
                    ^ unknown1.GetHashCode()
                    ^ unknown2.GetHashCode()
                    ^ unknown3.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1), TGIBlockListContentField("ParentTGIBlocks")]
            public byte TGIIndex { get { return tgiIndex; } set { if (tgiIndex != value) { tgiIndex = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ContentType Fabric { get { return fabric; } set { if (!fabric.Equals(value)) { fabric = new ContentType(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public byte Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public byte Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class SuperBlockList : DependentList<SuperBlock>
        {
            private DependentList<TGIBlock> _ParentTGIBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get { return _ParentTGIBlocks; }
                set { if (_ParentTGIBlocks != value) { _ParentTGIBlocks = value; foreach (var i in this) i.ParentTGIBlocks = _ParentTGIBlocks; } }
            }

            public SuperBlockList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public SuperBlockList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public SuperBlockList(EventHandler handler, IEnumerable<SuperBlock> lsb) : base(handler, lsb, Byte.MaxValue) { }

            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override SuperBlock CreateElement(Stream s) { return new SuperBlock(0, elementHandler, s) { ParentTGIBlocks = ParentTGIBlocks }; }

            protected override void WriteCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }
            protected override void WriteElement(Stream s, SuperBlock element) { element.UnParse(s); }

            public override void Add() { this.Add(new SuperBlock(0, null) { ParentTGIBlocks = ParentTGIBlocks }); }
        }

        #region Entry
        public abstract class Entry : AHandlerElement, IEquatable<Entry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            protected uint property;
            private Type enumType;
            protected byte unknown;
            protected byte dataType;
            #endregion

            #region Constructors
            public Entry(int APIversion, EventHandler handler, uint property, Type enumType, byte unknown, byte dataType)
                : base(APIversion, handler)
            {
                if (checking) if (enumType != null && !Enum.IsDefined(enumType, property))
                        throw new InvalidDataException(String.Format("Unexpected property ID 0x{0:X8}", property));
                this.property = property; this.enumType = enumType; this.unknown = unknown; this.dataType = dataType;
            }

            public static Entry CreateEntry(int APIversion, EventHandler handler, Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                uint property = r.ReadUInt32();
                if (property == 0)
                    return new EntryNull(APIversion, handler);

                byte unknown = r.ReadByte();
                byte dataType = r.ReadByte();
                switch (dataType)
                {
                    // bytes
                    case 0x00: return new EntryBoolean(APIversion, handler, (EntryBoolean.BooleanProperties)property, unknown, dataType, r.ReadByte());
                    case 0x01: return new EntrySByte(APIversion, handler, property, unknown, dataType, r.ReadSByte());
                    case 0x05: return new EntryByte(APIversion, handler, property, unknown, dataType, r.ReadByte());
                    case 0x0C: return new EntryTGIIndex(APIversion, handler, (EntryTGIIndex.TGIIndexProperties)property, unknown, dataType, r.ReadByte());
                    // words
                    case 0x02: return new EntryInt16(APIversion, handler, property, unknown, dataType, r.ReadInt16());
                    case 0x06: return new EntryUInt16(APIversion, handler, property, unknown, dataType, r.ReadUInt16());
                    // dwords
                    case 0x03: return new EntryInt32(APIversion, handler, (EntryInt32.Int32Properties)property, unknown, dataType, r.ReadInt32());
                    case 0x07: return new EntryUInt32(APIversion, handler, (EntryUInt32.UInt32Properties)property, unknown, dataType, r.ReadUInt32());
                    // qwords
                    case 0x04: return new EntryInt64(APIversion, handler, property, unknown, dataType, r.ReadInt64());
                    case 0x08: return new EntryUInt64(APIversion, handler, property, unknown, dataType, r.ReadUInt64());
                    // float
                    case 0x09: return new EntrySingle(APIversion, handler, (EntrySingle.SingleProperties)property, unknown, dataType, r.ReadSingle());
                    // rectangle
                    case 0x0A: return new EntryRectangle(APIversion, handler, (EntryRectangle.RectangleProperties)property, unknown, dataType, r);
                    // vector
                    case 0x0B: return new EntryVector(APIversion, handler, (EntryVector.VectorProperties)property, unknown, dataType, r);
                    // String
                    case 0x0D: return new EntryString(APIversion, handler, (EntryString.StringProperties)property, unknown, dataType, new String(r.ReadChars(r.ReadUInt16())));
                    default:
                        if (checking)
                            throw new InvalidDataException(String.Format("Unsupported data type 0x{0:X2} at 0x{1:X8}", dataType, s.Position));
                        break;
                }
                return null;
            }
            #endregion

            #region Data I/O
            internal virtual void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(property);
                if (property == 0)
                    return;
                w.Write(unknown);
                w.Write(dataType);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return AApiVersionedFields.GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(Entry other)
            {
                MemoryStream thisMS = new MemoryStream();
                UnParse(thisMS);
                MemoryStream otherMS = new MemoryStream();
                other.UnParse(otherMS);
                return thisMS.ToArray().Equals<byte>(otherMS.ToArray());
            }

            public override bool Equals(object obj)
            {
                return obj as Entry != null && this.Equals(obj as Entry);
            }

            public override int GetHashCode()
            {
                MemoryStream thisMS = new MemoryStream();
                UnParse(thisMS);
                return thisMS.ToArray().GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(2)]
            public byte Unknown { get { return unknown; } set { if (unknown != value) { unknown = value; OnElementChanged(); } } }
            //-this is linked to the subclass of Entry, so should not be editable
            //[ElementPriority(3)]
            //public byte DataType { get { return dataType; } set { if (dataType != value) { dataType = value; OnElementChanged(); } } }

            protected abstract string EntryValue { get; }
            public virtual string Value
            {
                get
                {
                    return ((enumType == null)
                        ? new TypedValue(typeof(uint), property, "X")
                        : new TypedValue(enumType, Enum.ToObject(enumType, property), "X"))
                        + "; " + EntryValue;
                }
            }
            #endregion
        }

        public class EntryNull : Entry
        {
            public EntryNull(int APIversion, EventHandler handler, EntryNull basis)
                : base(APIversion, handler, 0, null, 0, 0) { throw new NotImplementedException(); }
            public EntryNull(int APIversion, EventHandler handler)
                : base(APIversion, handler, 0, null, 0, 0) { }
            internal override void UnParse(Stream s) { throw new NotImplementedException(); }
            public override AHandlerElement Clone(EventHandler handler) { throw new NotImplementedException(); }
            protected override string EntryValue { get { return null; } }
            public override string Value { get { throw new NotImplementedException(); } }
        }
        [ConstructorParameters(new object[] { BooleanProperties.UIVisible, (byte)0, (byte)0x00, (byte)0, })]
        public class EntryBoolean : Entry
        {
            public enum BooleanProperties : uint
            {
                //EntryBoolean
                Unknown0xD49E8879 = 0xD49E8879,
                UIVisible = 0xD92A4C8B,
                EnableFiltering = 0xE27FE962,
                EnableBlending = 0xFBF310C7,
            }

            byte data;
            public EntryBoolean(int APIversion, EventHandler handler, EntryBoolean basis)
                : this(APIversion, handler, (BooleanProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryBoolean(int APIversion, EventHandler handler, BooleanProperties property, byte unknown, byte dataType, byte data)
                : base(APIversion, handler, (uint)property, typeof(BooleanProperties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryBoolean(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public BooleanProperties Property
            {
                get { return (BooleanProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(BooleanProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public byte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "" + (data != 0); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X2"); } }
        }
        [ConstructorParameters(new object[] { (uint)1, (byte)0, (byte)0x01, (sbyte)0, })]
        public class EntrySByte : Entry
        {
            sbyte data;
            public EntrySByte(int APIversion, EventHandler handler, EntrySByte basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntrySByte(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, sbyte data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntrySByte(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public sbyte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X2"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X2"); } }
        }
        [ConstructorParameters(new object[] { (uint)5, (byte)0, (byte)0x05, (byte)0, })]
        public class EntryByte : Entry
        {
            byte data;
            public EntryByte(int APIversion, EventHandler handler, EntryByte basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryByte(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, byte data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryByte(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public byte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X2"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X2"); } }
        }
        [ConstructorParameters(new object[] { TGIIndexProperties.MaskKey, (byte)0, (byte)0x0C, (byte)0, })]
        public class EntryTGIIndex : Entry
        {
            public enum TGIIndexProperties : uint
            {
                //EntryTGIIndex
                MaskKey = 0x49DE3B16,
                DefaultFabric = 0xDCFF6D7B,
                ImageKey = 0xF6CC8471,
            }

            public DependentList<TGIBlock> ParentTGIBlocks { get; set; }
            public override List<string> ContentFields { get { List<string> res = base.ContentFields; res.Remove("ParentTGIBlocks"); return res; } }

            byte data;
            public EntryTGIIndex(int APIversion, EventHandler handler, EntryTGIIndex basis)
                : this(APIversion, handler, (TGIIndexProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryTGIIndex(int APIversion, EventHandler handler, TGIIndexProperties property, byte unknown, byte dataType, byte data)
                : base(APIversion, handler, (uint)property, typeof(TGIIndexProperties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryTGIIndex(requestedApiVersion, handler, this) { ParentTGIBlocks = ParentTGIBlocks }; }
            [ElementPriority(1)]
            public TGIIndexProperties Property
            {
                get { return (TGIIndexProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(TGIIndexProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4), TGIBlockListContentField("ParentTGIBlocks")]
            public byte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            //protected override string EntryValue { get { return "0x" + data.ToString("X2"); } }
            protected override string EntryValue { get { return string.Format("0x{0:X2} ({1})", data, ParentTGIBlocks == null || data >= ParentTGIBlocks.Count ? "unknown" : ParentTGIBlocks[data]); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X2"); } }
        }
        [ConstructorParameters(new object[] { (uint)2, (byte)0, (byte)0x02, (Int16)0, })]
        public class EntryInt16 : Entry
        {
            Int16 data;
            public EntryInt16(int APIversion, EventHandler handler, EntryInt16 basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryInt16(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, Int16 data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryInt16(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Int16 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X4"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X4"); } }
        }
        [ConstructorParameters(new object[] { (uint)6, (byte)0, (byte)0x06, (UInt16)0, })]
        public class EntryUInt16 : Entry
        {
            UInt16 data;
            public EntryUInt16(int APIversion, EventHandler handler, EntryUInt16 basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryUInt16(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, UInt16 data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryUInt16(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt16 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X4"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X4"); } }
        }
        [ConstructorParameters(new object[] { Int32Properties.DestinationBlend, (byte)0, (byte)0x03, (Int32)0, })]
        public class EntryInt32 : Entry
        {
            public enum Int32Properties : uint
            {
                DestinationBlend = 0x048F7567,
                SkipShaderModel = 0x06A775CE,
                MinShaderModel = 0x2EDF5F53,
                ColorWrite = 0xB07B3B93,
                SourceBlend = 0xE055EE36,
            }

            // see http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.blend.aspx
            public enum Blend : int
            {
                Zero,
                One,
                SourceColor,
                InverseSourceColor,
                SourceAlpha,
                InverseSourceAlpha,
                DestinationAlpha,
                InverseDestinationAlpha,
                DestinationColor,
                InverseDestinationColor,
                SourceAlphaSaturation,
                BlendFactor,
                InverseBlendFactor,
            }

            public enum ShaderModel : int
            {
                SM_1_0 = 0x00000000,
                SM_1_1 = 0x00000001,
                SM_2_0 = 0x00000002,
                SM_2_1 = 0x00000003,
                SM_Highest = 0x7FFFFFFF,
            }

            [Flags]
            public enum ColorWriteChannels : int
            {
                Red = 0x01,
                Green = 0x02,
                Blue = 0x04,
                Alpha = 0x08,
                // -
                //None = 0x00,
                //Color = 0x07,
                //All = 0x0F,
            }

            Int32 data;
            public EntryInt32(int APIversion, EventHandler handler, EntryInt32 basis)
                : this(APIversion, handler, (Int32Properties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryInt32(int APIversion, EventHandler handler, Int32Properties property, byte unknown, byte dataType, Int32 data)
                : base(APIversion, handler, (uint)property, typeof(Int32Properties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryInt32(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public Int32Properties Property
            {
                get { return (Int32Properties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(Int32Properties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Int32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue
            {
                get
                {
                    switch ((Int32Properties)property)
                    {
                        case Int32Properties.DestinationBlend:
                        case Int32Properties.SourceBlend:
                            return new TypedValue(typeof(Blend), Enum.ToObject(typeof(Blend), data), "X") + "";
                        case Int32Properties.SkipShaderModel:
                            return new TypedValue(typeof(ShaderModel), Enum.ToObject(typeof(ShaderModel), data), "X") + "";
                        case Int32Properties.ColorWrite:
                            return new TypedValue(typeof(ColorWriteChannels), Enum.ToObject(typeof(ColorWriteChannels), data), "X") + "";
                        default:
                            return "0x" + data.ToString("X8");
                    }
                }
            }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X8"); } }
        }
        [ConstructorParameters(new object[] { UInt32Properties.Width, (byte)0, (byte)0x07, (UInt32)0, })]
        public class EntryUInt32 : Entry
        {
            public enum UInt32Properties : uint
            {
                MaskSource = 0x10DA0B6A,
                Width = 0x182E64EB,
                SkipDetailLevel = 0x331178DF,
                Height = 0x4C47D5C0,
                DefaultColor = 0x64399EC5,
                ID = 0x687720A6,
                ImageSource = 0x8A7006DB,
                RenderTarget = 0xA2C91332,
                MinDetailLevel = 0xAE5FE82A,
                Color = 0xB01748DA,
            }

            public enum DetailLevel : uint
            {
                Lowest = 0x00000000,
                Highest = 0x00000003,
            }

            public enum StepType : uint
            {
                DrawFabric = 0x034210A5,
                ChannelSelect = 0x1E363B9B,
                SkinTone = 0x43B554E3,
                HairTone = 0x5D7C85D4,
                RemappedChannelSelect = 0x890805DB,
                ColorFill = 0x9CD1269D,
                DrawImage = 0xA15200B1,
                CASPickData = 0xC6B6AC1F,
                SetTarget = 0xD6BD8695,
                HSVtoRGB = 0xDC0984B9,
            }

            public enum RenderTarget : uint
            {
                RenderTarget_A = 0x21E9CD2,
                RenderTarget_B = 0x21E9CD4,
            }


            UInt32 data;
            public EntryUInt32(int APIversion, EventHandler handler, EntryUInt32 basis)
                : this(APIversion, handler, (UInt32Properties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryUInt32(int APIversion, EventHandler handler, UInt32Properties property, byte unknown, byte dataType, UInt32 data)
                : base(APIversion, handler, (uint)property, typeof(UInt32Properties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryUInt32(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public UInt32Properties Property
            {
                get { return (UInt32Properties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(UInt32Properties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public UInt32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue
            {
                get
                {
                    switch ((UInt32Properties)property)
                    {
                        case UInt32Properties.SkipDetailLevel:
                        case UInt32Properties.MinDetailLevel:
                            return new TypedValue(typeof(DetailLevel), Enum.ToObject(typeof(DetailLevel), data), "X") + "";
                        case UInt32Properties.ID:
                            return new TypedValue(typeof(StepType), Enum.ToObject(typeof(StepType), data), "X") + "";
                        case UInt32Properties.RenderTarget:
                            return new TypedValue(typeof(RenderTarget), Enum.ToObject(typeof(RenderTarget), data), "X") + "";
                        default:
                            return "0x" + data.ToString("X8");
                    }
                }
            }
        }
        [ConstructorParameters(new object[] { (uint)4, (byte)0, (byte)0x04, (UInt64)0, })]
        public class EntryInt64 : Entry
        {
            Int64 data;
            public EntryInt64(int APIversion, EventHandler handler, EntryInt64 basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryInt64(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, Int64 data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write((uint)property); w.Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryInt64(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Int64 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X16"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X16"); } }
        }
        [ConstructorParameters(new object[] { (uint)8, (byte)0, (byte)0x08, (UInt64)0, })]
        public class EntryUInt64 : Entry
        {
            UInt64 data;
            public EntryUInt64(int APIversion, EventHandler handler, EntryUInt64 basis)
                : this(APIversion, handler, basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryUInt64(int APIversion, EventHandler handler, uint property, byte unknown, byte dataType, UInt64 data)
                : base(APIversion, handler, property, null, unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write((uint)property); w.Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryUInt64(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public uint Property { get { return property; } set { if (property != value) { property = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt64 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return "0x" + data.ToString("X16"); } }
            //public override string Value { get { return base.Value + "; Data: 0x" + data.ToString("X16"); } }
        }
        [ConstructorParameters(new object[] { SingleProperties.MaskBias, (byte)0, (byte)0x09, (Single)0, })]
        public class EntrySingle : Entry
        {
            public enum SingleProperties : uint
            {
                MaskBias = 0x3A3260E6,
                Rotation = 0x49F996DB,
            }

            Single data;
            public EntrySingle(int APIversion, EventHandler handler, EntrySingle basis)
                : this(APIversion, handler, (SingleProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntrySingle(int APIversion, EventHandler handler, SingleProperties property, byte unknown, byte dataType, Single data)
                : base(APIversion, handler, (uint)property, typeof(SingleProperties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(data); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntrySingle(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public SingleProperties Property
            {
                get { return (SingleProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(SingleProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return data.ToString("F4"); } }
            //public override string Value { get { return base.Value + "; Data: " + data.ToString(); } }
        }
        [ConstructorParameters(new object[] { RectangleProperties.SourceRectangle, (byte)0, (byte)0x0A, new Single[] { 0, 0, 0, 0 }, })]
        public class EntryRectangle : Entry
        {
            public enum RectangleProperties : uint
            {
                SourceRectangle = 0xA3AAFC98,
                DestinationRectangle = 0xE1D6D01F,
            }

            Single[] data = new Single[4];
            public EntryRectangle(int APIversion, EventHandler handler, RectangleProperties property, byte unknown, byte dataType, BinaryReader r)
                : this(APIversion, handler, property, unknown, dataType, new Single[4]) { for (int i = 0; i < data.Length; i++) data[i] = r.ReadSingle(); }
            public EntryRectangle(int APIversion, EventHandler handler, EntryRectangle basis)
                : this(APIversion, handler, (RectangleProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryRectangle(int APIversion, EventHandler handler, RectangleProperties property, byte unknown, byte dataType, Single[] data)
                : base(APIversion, handler, (uint)property, typeof(RectangleProperties), unknown, dataType) { Array.Copy(data, this.data, Math.Max(data.Length, this.data.Length)); }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); for (int i = 0; i < data.Length; i++) w.Write(data[i]); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryRectangle(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public RectangleProperties Property
            {
                get { return (RectangleProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(RectangleProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single[] Data { get { return (Single[])data.Clone(); } set { if (value.Length != this.data.Length) throw new ArgumentLengthException(); if (!data.Equals<float>(value)) { data = (Single[])value.Clone(); OnElementChanged(); } } }
            protected override string EntryValue { get { return String.Format("{0:F4}, {1:F4}; {2:F4}, {3:F4}", data[0], data[1], data[2], data[3]); } }
            //public override string Value { get { return base.Value + "; Data: " + (new TypedValue(data.GetType(), data, "X")); } }
        }
        [ConstructorParameters(new object[] { VectorProperties.MaskSelect, (byte)0, (byte)0x0B, new Single[] { 0, 0, 0, 0 }, })]
        public class EntryVector : Entry
        {
            public enum VectorProperties : uint
            {
                MaskSelect = 0x1F091259,
                HSVShift = 0xB67C2EF8,
                ChannelSelect = 0xD0E69002,
            }

            Single[] data = new Single[4];
            public EntryVector(int APIversion, EventHandler handler, VectorProperties property, byte unknown, byte dataType, BinaryReader r)
                : this(APIversion, handler, property, unknown, dataType, new Single[4]) { for (int i = 0; i < data.Length; i++) data[i] = r.ReadSingle(); }
            public EntryVector(int APIversion, EventHandler handler, EntryVector basis)
                : this(APIversion, handler, (VectorProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryVector(int APIversion, EventHandler handler, VectorProperties property, byte unknown, byte dataType, Single[] data)
                : base(APIversion, handler, (uint)property, typeof(VectorProperties), unknown, dataType) { Array.Copy(data, this.data, Math.Max(data.Length, this.data.Length)); }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); for (int i = 0; i < data.Length; i++) w.Write(data[i]); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryVector(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public VectorProperties Property
            {
                get { return (VectorProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(VectorProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public Single[] Data { get { return (Single[])data.Clone(); } set { if (value.Length != this.data.Length) throw new ArgumentLengthException(); if (!data.Equals<float>(value)) { data = (Single[])value.Clone(); OnElementChanged(); } } }
            protected override string EntryValue { get { return String.Format("{0:F4}, {1:F4}, {2:F4}, {3:F4}", data[0], data[1], data[2], data[3]); } }
            //public override string Value { get { return base.Value + "; Data: " + (new TypedValue(data.GetType(), data, "X")); } }
        }
        [ConstructorParameters(new object[] { StringProperties.Description, (byte)0, (byte)0x0D, "", })]
        public class EntryString : Entry
        {
            public enum StringProperties : uint
            {
                Description = 0x6B7119C1,
            }
            String data;
            public EntryString(int APIversion, EventHandler handler, EntryString basis)
                : this(APIversion, handler, (StringProperties)basis.property, basis.unknown, basis.dataType, basis.data) { }
            public EntryString(int APIversion, EventHandler handler, StringProperties property, byte unknown, byte dataType, String data)
                : base(APIversion, handler, (uint)property, typeof(StringProperties), unknown, dataType) { this.data = data; }
            internal override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write((UInt16)data.Length); w.Write(data.ToCharArray()); }
            public override AHandlerElement Clone(EventHandler handler) { return new EntryString(requestedApiVersion, handler, this); }
            [ElementPriority(1)]
            public StringProperties Property
            {
                get { return (StringProperties)property; }
                set
                {
                    if (property != (uint)value)
                    {
                        if (checking) if (!Enum.IsDefined(typeof(StringProperties), property))
                                throw new ArgumentException(String.Format("Unexpected property ID 0x{0:X8}", property));
                        property = (uint)value;
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(4)]
            public String Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            protected override string EntryValue { get { return data; } }
            //public override string Value { get { return base.Value + "; Data: \"" + data + "\""; } }
        }

        public class EntryList : DependentList<Entry>
        {
            private DependentList<TGIBlock> _ParentTGIBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get { return _ParentTGIBlocks; }
                set { if (_ParentTGIBlocks != value) { _ParentTGIBlocks = value; foreach (EntryTGIIndex i in this.FindAll(e => e is EntryTGIIndex)) i.ParentTGIBlocks = _ParentTGIBlocks; } }
            }

            public EntryList(int APIversion, EventHandler handler, Stream s) : base(null, -1) { elementHandler = handler; Parse(APIversion, s); this.handler = handler; }
            public EntryList(EventHandler handler) : base(handler) { }
            public EntryList(EventHandler handler, IEnumerable<Entry> le) : base(handler, le) { }

            protected void Parse(int requestedApiVersion, Stream s)
            {
                for (Entry e = Entry.CreateEntry(requestedApiVersion, elementHandler, s); !(e is EntryNull); e = Entry.CreateEntry(requestedApiVersion, elementHandler, s))
                {
                    this.Add(e);
                    if (e is EntryTGIIndex) (e as EntryTGIIndex).ParentTGIBlocks = ParentTGIBlocks;
                }
            }

            protected override int ReadCount(Stream s) { throw new InvalidOperationException(); }
            protected override Entry CreateElement(Stream s) { throw new InvalidOperationException(); }

            protected override void WriteCount(Stream s, int count) { } // List owner must do this, if required
            protected override void WriteElement(Stream s, Entry element) { element.UnParse(s); }

            public override void Add() { throw new NotImplementedException(); }

            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(Entry).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();

                uint property = (uint)fields[0];
                if (property == 0) return typeof(EntryNull);

                switch ((byte)fields[2])
                {
                    // bytes
                    case 0x00: return typeof(EntryBoolean);
                    case 0x01: return typeof(EntrySByte);
                    case 0x05: return typeof(EntryByte);
                    case 0x0C: return typeof(EntryTGIIndex);
                    // words
                    case 0x02: return typeof(EntryInt16);
                    case 0x06: return typeof(EntryUInt16);
                    // dwords
                    case 0x03: return typeof(EntryInt32);
                    case 0x07: return typeof(EntryUInt32);
                    // qwords
                    case 0x04: return typeof(EntryInt64);
                    case 0x08: return typeof(EntryUInt64);
                    // float
                    case 0x09: return typeof(EntrySingle);
                    // rectangle
                    case 0x0A: return typeof(EntryRectangle);
                    // vector
                    case 0x0B: return typeof(EntryVector);
                    // String
                    case 0x0D: return typeof(EntryString);
                }
                throw new InvalidDataException(String.Format("Unsupported data type 0x{0:X2}", (byte)fields[2]));
            }
        }
        #endregion

        public class EntryBlock : AHandlerElement, IEquatable<EntryBlock>
        {
            const int recommendedApiVersion = 1;
            DependentList<TGIBlock> _ParentTGIBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get { return _ParentTGIBlocks; }
                set { if (_ParentTGIBlocks != value) { _ParentTGIBlocks = value; if (theList != null) theList.ParentTGIBlocks = ParentTGIBlocks; } }
            }
            public override List<string> ContentFields { get { List<string> res = GetContentFields(requestedApiVersion, this.GetType()); res.Remove("ParentTGIBlocks"); return res; } }

            EntryList theList;

            public EntryBlock(int APIversion, EventHandler handler) : base(APIversion, handler) { theList = new EntryList(handler); }
            public EntryBlock(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public EntryBlock(int APIversion, EventHandler handler, EntryBlock basis) : base(APIversion, handler) { theList = new EntryList(handler, basis.theList); }

            void Parse(Stream s) { theList = new EntryList(requestedApiVersion, handler, s); }

            internal void UnParse(Stream s) { theList.UnParse(s); (new BinaryWriter(s)).Write((uint)0); }

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new EntryBlock(requestedApiVersion, handler, this) { ParentTGIBlocks = ParentTGIBlocks }; }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            #endregion

            #region IEquatable<EntryBlock> Members

            public bool Equals(EntryBlock other) { return theList.Equals(other.theList); }
            public override bool Equals(object obj)
            {
                return obj as EntryBlock != null ? this.Equals(obj as EntryBlock) : false;
            }
            public override int GetHashCode()
            {
                return theList.GetHashCode();
            }

            #endregion

            #region Content Fields
            public EntryList Entries { get { return theList; } set { if (theList != value) { theList = new EntryList(handler, value) { ParentTGIBlocks = ParentTGIBlocks }; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class EntryBlockList : DependentList<EntryBlock>
        {
            private DependentList<TGIBlock> _ParentTGIBlocks;
            public DependentList<TGIBlock> ParentTGIBlocks
            {
                get { return _ParentTGIBlocks; }
                set { if (_ParentTGIBlocks != value) { _ParentTGIBlocks = value; foreach (var i in this) i.ParentTGIBlocks = _ParentTGIBlocks; } }
            }
            //public override List<string> ContentFields { get { List<string> res = GetContentFields(0, this.GetType()); res.Remove("ParentTGIBlocks"); return res; } }

            int blockCount;
            public EntryBlockList(EventHandler handler) : base(handler) { }
            public EntryBlockList(EventHandler handler, int blockCount, Stream s) : base(null) { elementHandler = handler; this.blockCount = blockCount; Parse(s); this.handler = handler; }
            public EntryBlockList(EventHandler handler, IEnumerable<EntryBlock> leb) : base(handler, leb) { }

            protected override int ReadCount(Stream s) { return blockCount; }
            protected override EntryBlock CreateElement(Stream s) { return new EntryBlock(0, elementHandler, s) { ParentTGIBlocks = ParentTGIBlocks }; }

            protected override void WriteCount(Stream s, int count) { } // List owner must do this
            protected override void WriteElement(Stream s, EntryBlock element) { element.UnParse(s); }

            public override void Add() { this.Add(new EntryBlock(0, null) { ParentTGIBlocks = ParentTGIBlocks }); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public ContentType Root { get { return root; } set { if (!root.Equals(value)) { root = new ContentType(requestedApiVersion, OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for TxtcResource wrapper
    /// </summary>
    public class TxtcResourceHandler : AResourceHandler
    {
        public TxtcResourceHandler()
        {
            this.Add(typeof(TxtcResource), new List<string>(new string[] { "0x033A1435", "0x0341ACC9", }));
        }
    }
}
