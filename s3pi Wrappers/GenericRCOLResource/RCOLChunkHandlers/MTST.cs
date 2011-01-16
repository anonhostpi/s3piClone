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
    public class MTST : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint tag = (uint)FOURCC("MTST");
        uint version = 0x00000200;

        uint fnv32 = 0;
        GenericRCOLResource.ChunkReference index;
        EntryList list = null;
        #endregion

        #region Constructors
        public MTST(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public MTST(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public MTST(int APIversion, EventHandler handler, MTST basis)
            : this(APIversion, handler, basis.fnv32, basis.index, basis.list) { }
        public MTST(int APIversion, EventHandler handler, uint fnv32, GenericRCOLResource.ChunkReference index, IEnumerable<Entry> list)
            : base(APIversion, handler, null)
        {
            this.fnv32 = fnv32;
            this.index = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, index);
            this.list = new EntryList(OnRCOLChanged, list);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return "MTST"; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x02019972; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC("MTST"))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: 'MTST'; at 0x{1:X8}", FOURCC(tag), s.Position));
            version = r.ReadUInt32();
            if (checking) if (version != 0x00000200)
                    throw new InvalidDataException(String.Format("Invalid Version read: 0x{0:X8}; expected 0x00000200; at 0x{1:X8}", version, s.Position));

            fnv32 = r.ReadUInt32();
            index = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
            list = new EntryList(OnRCOLChanged, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            w.Write(fnv32);
            if (index == null) this.index = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, 0);
            index.UnParse(ms);
            if (list == null) this.list = new EntryList(OnRCOLChanged);
            list.UnParse(ms);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new MTST(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class Entry : AHandlerElement, IEquatable<Entry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            GenericRCOLResource.ChunkReference index;
            uint fnv32 = 0;
            #endregion

            #region Constructors
            public Entry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Entry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Entry(int APIversion, EventHandler handler, Entry basis) : this(APIversion, handler, basis.index, basis.fnv32) { }
            public Entry(int APIversion, EventHandler handler, GenericRCOLResource.ChunkReference index, uint fnv32) : base(APIversion, handler)
            {
                this.index = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, index);
                this.fnv32 = fnv32;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s) { index = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s); fnv32 = new BinaryReader(s).ReadUInt32(); }

            internal void UnParse(Stream s) { index.UnParse(s); new BinaryWriter(s).Write(fnv32); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new Entry(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(Entry other) { return this.index == other.index && this.fnv32 == other.fnv32; }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public GenericRCOLResource.ChunkReference Index { get { return index; } set { if (index != value) { new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public UInt32 FNV32 { get { return fnv32; } set { if (fnv32 != value) { fnv32 = value; OnElementChanged(); } } }

            public string Value { get { return String.Format("Index: {0}; FNV32: 0x{1:X8}", index.Value, fnv32); } }
            #endregion
        }
        public class EntryList : DependentList<Entry>
        {
            #region Constructors
            public EntryList(EventHandler handler) : base(handler) { }
            public EntryList(EventHandler handler, Stream s) : base(handler, s) { }
            public EntryList(EventHandler handler, IEnumerable<Entry> le) : base(handler, le) { }
            #endregion

            #region Data I/O
            protected override Entry CreateElement(Stream s) { return new Entry(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Entry element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new Entry(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public GenericRCOLResource.ChunkReference Index { get { return index; } set { if (index != value) { new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public uint FNV32 { get { return fnv32; } set { if (fnv32 != value) { fnv32 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public EntryList Entries { get { return list; } set { if (list != value) { list = new EntryList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");

                s += String.Format("\nIndex: 0x{0:X8}\nFNV32: 0x{1:X8}", index, fnv32);

                s += "\n--\nEntry List:";
                for (int i = 0; i < list.Count; i++)
                    s += String.Format("\n  [{0}]: {1}", i, list[i].Value);
                s += "\n--";

                return s;
                /**/
            }
        }
        #endregion
    }
}
