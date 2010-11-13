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
    public class RoofPatternCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        uint unknown2;
        byte unknown3;
        uint unknown4;
        byte unknown5;
        uint unknown6;
        byte unknown7;
        uint vpxy_index1;
        uint vpxy_index2;
        uint unknown8;
        #endregion

        #region Constructors
        public RoofPatternCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public RoofPatternCatalogResource(int APIversion, Stream unused, RoofPatternCatalogResource basis)
            : base(APIversion, basis.version, basis.list)
        {
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown2 = basis.unknown2;
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.vpxy_index1 = basis.vpxy_index1;
            this.vpxy_index2 = basis.vpxy_index2;
            this.unknown8 = basis.unknown8;
        }
        public RoofPatternCatalogResource(int APIversion, uint version, Common common,
            uint unknown2, byte unknown3, uint unknown4, byte unknown5, uint unknown6, byte unknown7, uint index1, uint index2, uint unknown8,
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
            this.vpxy_index1 = index1;
            this.vpxy_index2 = index2;
            this.unknown8 = unknown8;
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
            this.vpxy_index1 = r.ReadUInt32();
            this.vpxy_index2 = r.ReadUInt32();
            this.unknown8 = r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(RoofPatternCatalogResource)) && s.Position != s.Length)
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
            w.Write(vpxy_index1);
            w.Write(vpxy_index2);
            w.Write(unknown8);

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
        [ElementPriority(27), TGIBlockListContentField("TGIBlocks")]
        public uint VPXYIndex1 { get { return vpxy_index1; } set { if (vpxy_index1 != value) { vpxy_index1 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28), TGIBlockListContentField("TGIBlocks")]
        public uint VPXYIndex2 { get { return vpxy_index2; } set { if (vpxy_index2 != value) { vpxy_index2 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29)]
        public uint Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
