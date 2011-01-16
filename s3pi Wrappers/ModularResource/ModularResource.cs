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

namespace ModularResource
{
    public class ModularResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        ushort unknown1;
        ushort unknown2;
        TGIIndexList tgiIndexes;
        TGIBlockList tgiBlocks;
        #endregion

        public ModularResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            long tgiPosn, tgiSize;
            BinaryReader r = new BinaryReader(s);

            unknown1 = r.ReadUInt16();
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();
            unknown2 = r.ReadUInt16();
            tgiIndexes = new TGIIndexList(OnResourceChanged, s);
            tgiBlocks = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);
        }

        protected override Stream UnParse()
        {
            long pos;
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(unknown1);
            pos = ms.Position;
            w.Write((uint)0);//tgiOffset
            w.Write((uint)0);//tgiSize
            w.Write(unknown2);
            if (tgiIndexes == null) tgiIndexes = new TGIIndexList(OnResourceChanged);
            tgiIndexes.UnParse(ms);
            if (tgiBlocks == null) tgiBlocks = new TGIBlockList(OnResourceChanged);
            tgiBlocks.UnParse(ms, pos);

            return ms;
        }
        #endregion

        #region Sub-classes
        public class TGIIndexList : SimpleList<UInt32>
        {
            #region Constructors
            public TGIIndexList(EventHandler handler) : base(handler, ReadUInt32, WriteUInt32, ushort.MaxValue, ReadListCount, WriteListCount) { }
            internal TGIIndexList(EventHandler handler, Stream s) : base(handler, s, ReadUInt32, WriteUInt32, ushort.MaxValue, ReadListCount, WriteListCount) { }
            public TGIIndexList(EventHandler handler, IEnumerable<uint> ltgii) : base(handler, ltgii, ReadUInt32, WriteUInt32, ushort.MaxValue, ReadListCount, WriteListCount) { }
            public TGIIndexList(EventHandler handler, IEnumerable<HandlerElement<uint>> ltgii) : base(handler, ltgii, ReadUInt32, WriteUInt32, ushort.MaxValue, ReadListCount, WriteListCount) { }
            #endregion

            #region Data I/O
            static int ReadListCount(Stream s) { return (new BinaryReader(s)).ReadUInt16(); }
            static void WriteListCount(Stream s, int count) { (new BinaryWriter(s)).Write((UInt16)count); }
            static UInt32 ReadUInt32(Stream s) { return (new BinaryReader(s)).ReadUInt32(); }
            static void WriteUInt32(Stream s, UInt32 value) { (new BinaryWriter(s)).Write(value); }
            #endregion
       }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public ushort Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public ushort Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public TGIIndexList TGIIndexes { get { return tgiIndexes; } set { if (tgiIndexes != value) { tgiIndexes = new TGIIndexList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public TGIBlockList TGIBlocks { get { return tgiBlocks; } set { if (tgiBlocks != value) { tgiBlocks = new TGIBlockList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public String Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";
                string fmt;

                s += "Unknown1: 0x" + unknown1.ToString("X4");
                s += "\nUnknown2: 0x" + unknown2.ToString("X4");

                s += "\nTGIIndexes:";
                fmt = "\n  [{0:X" + tgiIndexes.Count.ToString("X").Length + "}]: {1:X8}";
                for (int i = 0; i < tgiIndexes.Count; i++) s += string.Format(fmt, i, tgiIndexes[i]);
                s += "\n--";

                s += "\nTGIBlocks:";
                fmt = "\n  [{0:X" + tgiBlocks.Count.ToString("X").Length + "}]: {1}";
                for (int i = 0; i < tgiBlocks.Count; i++) s += string.Format(fmt, i, tgiBlocks[i].Value);
                s += "\n--";
                return s;
                /**/
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for ModularResource wrapper
    /// </summary>
    public class ModularResourceHandler : AResourceHandler
    {
        public ModularResourceHandler()
        {
            this.Add(typeof(ModularResource), new List<string>(new string[] { "0xCF9A4ACE", }));
        }
    }
}
