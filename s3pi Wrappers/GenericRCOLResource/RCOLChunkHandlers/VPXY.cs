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
        float[] boundingBox = new float[6];
        byte[] unused = new byte[4];
        byte modular;
        int ftptIndex;
        AResource.TGIBlockList tgiBlockList;
        #endregion

        public VPXY(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public VPXY(int APIversion, EventHandler handler, VPXY basis)
            : this(APIversion, handler,
            basis.version, basis.entryList, basis.tc02, basis.boundingBox, basis.unused, basis.modular, basis.ftptIndex, basis.tgiBlockList) { }
        public VPXY(int APIversion, EventHandler handler,
            uint version, EntryList entryList, byte tc02, float[] boundingBox, byte[] unused, byte modular, int ftptIndex, IList<AResource.TGIBlock> tgiBlockList)
            : base(APIversion, null, null)
        {
            this.handler = handler;
            this.version = version;
            if (checking) if (version != 4)
                    throw new ArgumentException(String.Format("Invalid Version: 0x{0:X8}; expected 0x00000004", version));
            this.entryList = new EntryList(OnRCOLChanged, entryList);
            this.tc02 = tc02;
            if (checking) if (tc02 != 0x02)
                    throw new ArgumentException(String.Format("Invalid TC02: 0x{0:X2}; expected 0x02", tc02));
            this.boundingBox = (float[])boundingBox.Clone();
            this.unused = (byte[])unused.Clone();
            if (checking) if (unused.Length != 4)
                    throw new ArgumentLengthException("Unused", 4);
            this.modular = modular;
            if (modular != 0)
                this.ftptIndex = ftptIndex;
            this.tgiBlockList = new AResource.TGIBlockList(OnRCOLChanged, tgiBlockList);
        }
        public VPXY(int APIversion, EventHandler handler)
            : base(APIversion, null, null)
        {
            this.handler = handler;
            entryList = new EntryList(OnRCOLChanged);
            tgiBlockList = new AResource.TGIBlockList(OnRCOLChanged);
        }

        #region ARCOLBlock
        public override string Tag { get { return "VPXY"; } }

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
            for (int i = 0; i < boundingBox.Length; i++) boundingBox[i] = r.ReadSingle();
            unused = r.ReadBytes(4);
            if (checking) if (unused.Length != 4)
                    throw new EndOfStreamException(String.Format("Unused: expected 4 bytes, read {0}.", unused.Length));
            modular = r.ReadByte();
            if (modular != 0)
                ftptIndex = r.ReadInt32();
            else
                ftptIndex = 0;

            tgiBlockList = new AResource.TGIBlockList(OnRCOLChanged, s, tgiPosn, tgiSize);
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
            foreach (float f in boundingBox) w.Write(f);
            w.Write(unused);
            w.Write(modular);
            if (modular != 0)
                w.Write(ftptIndex);

            if (tgiBlockList == null) tgiBlockList = new AResource.TGIBlockList(OnRCOLChanged);
            tgiBlockList.UnParse(ms, pos);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new VPXY(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class IntList : AResource.SimpleList<Int32>
        {
            static string fmt = "0x{1:X8}; ";
            #region Constructors
            public IntList(EventHandler handler) : base(handler, ReadInt32, WriteInt32, fmt, byte.MaxValue, ReadListCount, WriteListCount) { }
            public IntList(EventHandler handler, Stream s) : base(handler, s, ReadInt32, WriteInt32, fmt, byte.MaxValue, ReadListCount, WriteListCount) { }
            public IntList(EventHandler handler, IList<HandlerElement<Int32>> ltgi) : base(handler, ltgi, ReadInt32, WriteInt32, fmt, byte.MaxValue, ReadListCount, WriteListCount) { }
            public IntList(EventHandler handler, IList<Int32> ltgi) : base(handler, ltgi, ReadInt32, WriteInt32, fmt, byte.MaxValue, ReadListCount, WriteListCount) { }
            #endregion

            #region Data I/O
            static uint ReadListCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            static void WriteListCount(Stream s, uint count) { (new BinaryWriter(s)).Write((byte)count); }
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
            public Entry00(int APIversion, EventHandler handler, byte entryType, byte entryID, IList<HandlerElement<int>> tgiIndexes)
                : base(APIversion, handler) { this.entryID = entryID; this.tgiIndexes = new IntList(handler, tgiIndexes); }
            public Entry00(int APIversion, EventHandler handler, byte entryType, byte entryID, IList<int> tgiIndexes)
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
                    return "EntryID: 0x" + entryID.ToString("X2") + "; TGIIndexes: " + tgiIndexes.Value;
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

        public class EntryList : AResource.DependentList<Entry>
        {
            #region Constructors
            public EntryList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public EntryList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            public EntryList(EventHandler handler, IList<Entry> le) : base(handler, le, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override uint ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override void WriteCount(Stream s, uint count) { (new BinaryWriter(s)).Write((byte)count); }

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
        public uint Version { get { return version; } /*set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } }/**/ }
        public EntryList Entries { get { return entryList; } set { if (entryList != value) { entryList = new EntryList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        public byte TC02 { get { return tc02; } /*set { if (tc02 != value) { tc02 = value; OnRCOLChanged(this, EventArgs.Empty); } }/**/ }
        public float[] BoundingBox
        {
            get { return (float[])boundingBox.Clone(); }
            set
            {
                if (value.Length != this.boundingBox.Length) throw new ArgumentLengthException("BoundingBox", this.boundingBox.Length);
                if (!ArrayCompare(boundingBox, value)) { boundingBox = value == null ? null : (float[])value.Clone(); OnRCOLChanged(this, EventArgs.Empty); }
            }
        }
        public byte[] Unused
        {
            get { return (byte[])unused.Clone(); }
            set
            {
                if (value.Length != this.unused.Length) throw new ArgumentLengthException("Unused", this.unused.Length);
                if (!ArrayCompare(unused, value)) { unused = value == null ? null : (byte[])value.Clone(); OnRCOLChanged(this, EventArgs.Empty); }
            }
        }
        public bool Modular { get { return modular != 0; } set { if (Modular != value) { modular = (byte)(value ? 0x01 : 0x00); OnRCOLChanged(this, EventArgs.Empty); } } }
        public int FTPTIndex
        {
            get { return ftptIndex; }
            set { if (modular == 0) throw new InvalidOperationException(); if (ftptIndex != value) { ftptIndex = value; OnRCOLChanged(this, EventArgs.Empty); } }
        }
        public AResource.TGIBlockList TGIBlocks
        {
            get { return tgiBlockList; }
            set { if (tgiBlockList != value) { tgiBlockList = new AResource.TGIBlockList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value
        {
            get
            {
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");

                s += "\n--\nEntry List:";
                for (int i = 0; i < entryList.Count; i++)
                    s += "\n[" + i + "]: " + entryList[i].Value;

                s += "\nTC02: 0x" + tc02.ToString("X2");
                s += "\nBoundingBox: " + this["BoundingBox"];
                s += "\nUnused: " + this["Unused"];
                s += "\nModular: " + modular;
                if (Modular)
                    s += "\n" + "FTPTIndex: 0x" + ftptIndex.ToString("X8");

                s += "\n--TGI Blocks:\n" + tgiBlockList.Value + "--";
                return s;
            }
        }
        #endregion
    }
}
