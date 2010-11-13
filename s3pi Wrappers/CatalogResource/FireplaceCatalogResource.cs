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
    public class FireplaceCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        uint unknown2;
        byte unknown3;
        uint unknown4;
        byte unknown5;
        uint unknown6;
        byte unknown7;
        byte unknown8;
        uint index1;
        uint index2;
        uint index3;
        uint index4;
        uint index5;
        uint index6;
        uint index7;
        #endregion

        #region Constructors
        public FireplaceCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public FireplaceCatalogResource(int APIversion, Stream unused, FireplaceCatalogResource basis)
            : base(APIversion, basis.version, basis.list)
        {
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown2 = basis.unknown2;
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.unknown8 = basis.unknown8;
            this.index1 = basis.index1;
            this.index2 = basis.index2;
            this.index3 = basis.index3;
            this.index4 = basis.index4;
            this.index5 = basis.index5;
            this.index6 = basis.index6;
            this.index7 = basis.index7;
        }
        public FireplaceCatalogResource(int APIversion, uint version, Common common, uint unknown2, byte unknown3, uint unknown4, byte unknown5,
            uint unknown6, byte unknown7, byte unknown8, uint index1, uint index2, uint index3, uint index4, uint index5, uint index6, uint index7,
            TGIBlockList ltgib)
            : base(APIversion, version, ltgib)
        {
            this.common = new Common(requestedApiVersion, OnResourceChanged, common);
            this.unknown2 = unknown2;
            this.unknown3 = unknown3;
            this.unknown4 = unknown4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.unknown7 = unknown7;
            this.unknown8 = unknown8;
            this.index1 = index1;
            this.index2 = index2;
            this.index3 = index3;
            this.index4 = index4;
            this.index5 = index5;
            this.index6 = index6;
            this.index7 = index7;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.unknown2 = r.ReadUInt32();
            this.unknown3 = r.ReadByte();
            this.unknown4 = r.ReadUInt32();
            this.unknown5 = r.ReadByte();
            this.unknown6 = r.ReadUInt32();
            this.unknown7 = r.ReadByte();
            this.unknown8 = r.ReadByte();
            this.index1 = r.ReadUInt32();
            this.index2 = r.ReadUInt32();
            this.index3 = r.ReadUInt32();
            this.index4 = r.ReadUInt32();
            this.index5 = r.ReadUInt32();
            this.index6 = r.ReadUInt32();
            this.index7 = r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(FireplaceCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);

            if (common == null) common = new Common(requestedApiVersion, OnResourceChanged);
            common.UnParse(s);

            w.Write(unknown2);
            w.Write(unknown3);
            w.Write(unknown4);
            w.Write(unknown5);
            w.Write(unknown6);
            w.Write(unknown7);
            w.Write(unknown8);
            w.Write(index1);
            w.Write(index2);
            w.Write(index3);
            w.Write(index4);
            w.Write(index5);
            w.Write(index6);
            w.Write(index7);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region Content Fields
        [ElementPriority(21)]
        public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(22)]
        public byte Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(23)]
        public uint Unknown4 { get { return unknown4; } set { if (unknown4 != value) { unknown4 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(24)]
        public byte Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(25)]
        public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(26)]
        public byte Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(27)]
        public byte Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28), TGIBlockListContentField("TGIBlocks")]
        public uint Index1 { get { return index1; } set { if (index1 != value) { index1 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29), TGIBlockListContentField("TGIBlocks")]
        public uint Index2 { get { return index2; } set { if (index2 != value) { index2 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30), TGIBlockListContentField("TGIBlocks")]
        public uint Index3 { get { return index3; } set { if (index3 != value) { index3 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31), TGIBlockListContentField("TGIBlocks")]
        public uint Index4 { get { return index4; } set { if (index4 != value) { index4 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32), TGIBlockListContentField("TGIBlocks")]
        public uint Index5 { get { return index5; } set { if (index5 != value) { index5 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(33), TGIBlockListContentField("TGIBlocks")]
        public uint Index6 { get { return index6; } set { if (index6 != value) { index6 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(34), TGIBlockListContentField("TGIBlocks")]
        public uint Index7 { get { return index7; } set { if (index7 != value) { index7 = value; OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
