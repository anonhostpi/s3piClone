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
    public class VPXY : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint tag = (uint)FOURCC("VPXY");
        uint version = 4;
        EntryList entryList;
        byte tc02 = 0x02;
        BoundingBox bounds;
        byte[] unused = new byte[4];
        byte modular;
        int ftptIndex;
        TGIBlockList tgiBlockList;
        #endregion

        #region Constructors
        public VPXY(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public VPXY(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public VPXY(int APIversion, EventHandler handler, VPXY basis)
            : this(APIversion, handler,
            basis.version, basis.entryList, basis.tc02, basis.bounds, basis.unused, basis.modular, basis.ftptIndex,
            basis.tgiBlockList) { }
        public VPXY(int APIversion, EventHandler handler,
            uint version, IEnumerable<Entry> entryList, byte tc02, BoundingBox bounds, byte[] unused, byte modular, int ftptIndex,
            IEnumerable<TGIBlock> tgiBlockList)
            : base(APIversion, handler, null)
        {
            this.version = version;
            if (checking) if (version != 4)
                    throw new ArgumentException(String.Format("Invalid Version: 0x{0:X8}; expected 0x00000004", version));
            this.entryList = new EntryList(OnRCOLChanged, entryList);
            this.tc02 = tc02;
            if (checking) if (tc02 != 0x02)
                    throw new ArgumentException(String.Format("Invalid TC02: 0x{0:X2}; expected 0x02", tc02));
            this.bounds = new BoundingBox(requestedApiVersion, handler, bounds);
            this.unused = (byte[])unused.Clone();
            if (checking) if (unused.Length != 4)
                    throw new ArgumentLengthException("Unused", 4);
            this.modular = modular;
            if (modular != 0)
                this.ftptIndex = ftptIndex;
            this.tgiBlockList = new TGIBlockList(OnRCOLChanged, tgiBlockList);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "VPXY"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x736884F1; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC("VPXY"))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: 'VPXY'; at 0x{1:X8}", FOURCC(tag), s.Position));
            version = r.ReadUInt32();
            if (checking) if (version != 4)
                    throw new InvalidDataException(String.Format("Invalid Version read: 0x{0:X8}; expected 0x00000004; at 0x{1:X8}", version, s.Position));

            long tgiPosn = r.ReadUInt32() + s.Position;
            long tgiSize = r.ReadUInt32();

            entryList = new EntryList(OnRCOLChanged, s);
            tc02 = r.ReadByte();
            if (checking) if (tc02 != 2)
                    throw new InvalidDataException(String.Format("Invalid TC02 read: 0x{0:X2}; expected 0x02; at 0x{1:X8}", tc02, s.Position));
            bounds = new BoundingBox(requestedApiVersion, handler, s);
            unused = r.ReadBytes(4);
            if (checking) if (unused.Length != 4)
                    throw new EndOfStreamException(String.Format("Unused: expected 4 bytes, read {0}.", unused.Length));
            modular = r.ReadByte();
            if (modular != 0)
                ftptIndex = r.ReadInt32();
            else
                ftptIndex = 0;

            tgiBlockList = new TGIBlockList(OnRCOLChanged, s, tgiPosn, tgiSize);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            long pos = ms.Position;
            w.Write((uint)0); // tgiOffset
            w.Write((uint)0); // tgiSize

            if (entryList == null) entryList = new EntryList(OnRCOLChanged);
            entryList.UnParse(ms);

            w.Write(tc02);

            if (bounds == null) bounds = new BoundingBox(requestedApiVersion, handler);
            bounds.UnParse(ms);

            w.Write(unused);
            w.Write(modular);
            if (modular != 0)
                w.Write(ftptIndex);

            if (tgiBlockList == null) tgiBlockList = new TGIBlockList(OnRCOLChanged);
            tgiBlockList.UnParse(ms, pos);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new VPXY(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class IntList : SimpleList<Int32>
        {
            #region Constructors
            public IntList(EventHandler handler) : base(handler, ReadInt32, WriteInt32, byte.MaxValue, ReadListCount, WriteListCount) { }
            public IntList(EventHandler handler, Stream s) : base(handler, s, ReadInt32, WriteInt32, byte.MaxValue, ReadListCount, WriteListCount) { }
            public IntList(EventHandler handler, IEnumerable<Int32> ltgi) : base(handler, ltgi, ReadInt32, WriteInt32, byte.MaxValue, ReadListCount, WriteListCount) { }
            #endregion

            #region Data I/O
            static int ReadListCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            static void WriteListCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }
            static int ReadInt32(Stream s) { return new BinaryReader(s).ReadInt32(); }
            static void WriteInt32(Stream s, int value) { new BinaryWriter(s).Write(value); }
            #endregion
        }

        public abstract class Entry : AHandlerElement, IEquatable<Entry>
        {
            const int recommendedApiVersion = 1;

            #region Constructors
            protected Entry(int APIversion, EventHandler handler) : base(APIversion, handler) { }

            public static Entry CreateEntry(int APIversion, EventHandler handler, Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                byte entryType = r.ReadByte();
                if (entryType == 0x00) return new Entry00(APIversion, handler, 0, r.ReadByte(), new IntList(handler, s));
                if (entryType == 0x01) return new Entry01(APIversion, handler, 1, r.ReadInt32());
                throw new InvalidDataException(String.Format("Unknown EntryType 0x{0:X2} at 0x{1:X8}", entryType, s.Position));
            }
            #endregion

            #region Data I/O
            internal abstract void UnParse(Stream s);
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Entry> Members

            public abstract bool Equals(Entry other);

            #endregion

            public abstract string Value { get; }
        }
        public class Entry00 : Entry
        {
            byte entryID;
            IntList tgiIndexes;

            public Entry00(int APIversion, EventHandler handler, Entry00 basis)
                : this(APIversion, handler, 0, basis.entryID, basis.tgiIndexes) { }
            public Entry00(int APIversion, EventHandler handler, byte entryType, byte entryID, IEnumerable<int> tgiIndexes)
                : base(APIversion, handler) { this.entryID = entryID; this.tgiIndexes = new IntList(handler, tgiIndexes); }

            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)0x00);
                w.Write(entryID);
                if (tgiIndexes == null) tgiIndexes = new IntList(handler);
                tgiIndexes.UnParse(s);
            }

            public override bool Equals(Entry other)
            {
                return other.GetType() == this.GetType() &&
                    (other as Entry00).entryID == entryID && (other as Entry00).tgiIndexes == tgiIndexes;
            }

            public override AHandlerElement Clone(EventHandler handler) { return new Entry00(requestedApiVersion, handler, this); }

            #region Content Fields
            public byte EntryID { get { return entryID; } set { if (entryID != value) { entryID = value; if (handler != null) handler(this, EventArgs.Empty); } } }
            public IntList TGIIndexes { get { return tgiIndexes; } set { if (tgiIndexes != value) { tgiIndexes = new IntList(handler, value); if (handler != null) handler(this, EventArgs.Empty); } } }

            public override string Value
            {
                get
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("EntryID: 0x" + entryID.ToString("X2") + String.Format("; TGIIndexes ({0:X}): ", tgiIndexes.Count));
                    string fmt = "[{0:X" + tgiIndexes.Count.ToString("X").Length + "}]: 0x{1:X8}; ";
                    for (int i = 0; i < tgiIndexes.Count; i++) sb.Append(String.Format(fmt, i, tgiIndexes[i]));
                    return sb.ToString().TrimEnd(';', ' ');
                }
            }
            #endregion
        }
        public class Entry01 : Entry
        {
            int tgiIndex;
            public Entry01(int APIversion, EventHandler handler, Entry01 basis) : this(APIversion, handler, 1, basis.tgiIndex) { }
            public Entry01(int APIversion, EventHandler handler, byte entryType, int tgiIndex) : base(APIversion, handler) { this.tgiIndex = tgiIndex; }
            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)0x01);
                w.Write(tgiIndex);
            }

            public override bool Equals(Entry other) { return other.GetType() == this.GetType() && (other as Entry01).tgiIndex == tgiIndex; }

            public override AHandlerElement Clone(EventHandler handler) { return new Entry01(requestedApiVersion, handler, this); }

            #region Content Fields
            public Int32 TGIIndex { get { return tgiIndex; } set { if (tgiIndex != value) { tgiIndex = value; if (handler != null) handler(this, EventArgs.Empty); } } }

            public override string Value { get { return "TGIIndex: 0x" + tgiIndex.ToString("X8") + ""; } }
            #endregion
        }

        public class EntryList : DependentList<Entry>
        {
            #region Constructors
            public EntryList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public EntryList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public EntryList(EventHandler handler, IEnumerable<Entry> le) : base(handler, le, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override void WriteCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }

            protected override Entry CreateElement(Stream s) { return Entry.CreateEntry(0, elementHandler, s); }

            protected override void WriteElement(Stream s, Entry element) { element.UnParse(s); }
            #endregion

            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(Entry).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();

                switch ((byte)fields[0])
                {
                    case 0x00: return typeof(Entry00);
                    case 0x01: return typeof(Entry01);
                }
                throw new ArgumentException(String.Format("Unknown entry type 0x{0:X2}", (byte)fields[0]));
            }

            public override void Add() { throw new NotImplementedException(); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } /*set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } }/**/ }
        [ElementPriority(12)]
        public EntryList Entries { get { return entryList; } set { if (entryList != value) { entryList = new EntryList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public byte TC02 { get { return tc02; } /*set { if (tc02 != value) { tc02 = value; OnRCOLChanged(this, EventArgs.Empty); } }/**/ }
        [ElementPriority(14)]
        public BoundingBox Bounds
        {
            get { return bounds; }
            set
            {
                if (bounds != value) { bounds = new BoundingBox(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); }
            }
        }
        [ElementPriority(15)]
        public byte[] Unused
        {
            get { return (byte[])unused.Clone(); }
            set
            {
                if (value.Length != this.unused.Length) throw new ArgumentLengthException("Unused", this.unused.Length);
                if (!unused.Equals<byte>(value)) { unused = value == null ? null : (byte[])value.Clone(); OnRCOLChanged(this, EventArgs.Empty); }
            }
        }
        [ElementPriority(16)]
        public bool Modular { get { return modular != 0; } set { if (Modular != value) { modular = (byte)(value ? 0x01 : 0x00); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public int FTPTIndex
        {
            get { return ftptIndex; }
            set { if (modular == 0) throw new InvalidOperationException(); if (ftptIndex != value) { ftptIndex = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }
        public TGIBlockList TGIBlocks
        {
            get { return tgiBlockList; }
            set { if (tgiBlockList != value) { tgiBlockList = new TGIBlockList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string fmt;
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");

                s += String.Format("\nEntry List ({0:X}):", entryList.Count);
                fmt = "\n  [{0:X" + entryList.Count.ToString("X").Length + "}]: {1}";
                for (int i = 0; i < entryList.Count; i++) s += String.Format(fmt, i, entryList[i].Value);
                s += "\n----";

                s += "\nTC02: 0x" + tc02.ToString("X2");
                s += "\nBounds: " + bounds.Value;
                s += "\nUnused: " + this["Unused"];
                s += "\nModular: " + modular;
                if (Modular)
                    s += "\n" + "FTPTIndex: 0x" + ftptIndex.ToString("X8");

                s += String.Format("\nTGI Blocks ({0:X}):", tgiBlockList.Count);
                fmt = "\n  [{0:X" + tgiBlockList.Count.ToString("X").Length + "}]: {1}";
                for (int i = 0; i < tgiBlockList.Count; i++) s += string.Format(fmt, i, tgiBlockList[i].Value);
                s += "\n----";

                return s;
                /**/
            }
        }
        #endregion
    }
}
