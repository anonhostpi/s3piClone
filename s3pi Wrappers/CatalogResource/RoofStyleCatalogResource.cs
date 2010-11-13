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
    public class RoofStyleCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        uint unknown2;
        byte unknown3;
        uint unknown4;
        byte unknown5;
        byte unknown6;
        uint unknown7;
        uint unknown8;
        uint catalogRoofPattern;
        uint catalogWallStyle;
        float unknown9;
        uint unknown10;
        #endregion

        #region Constructors
        public RoofStyleCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public RoofStyleCatalogResource(int APIversion, Stream unused, RoofStyleCatalogResource basis)
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
            this.catalogRoofPattern = basis.catalogRoofPattern;
            this.catalogWallStyle = basis.catalogWallStyle;
            this.unknown9 = basis.unknown9;
            this.unknown10 = basis.unknown10;
        }
        public RoofStyleCatalogResource(int APIversion, uint version, Common common,
            uint unknown2, byte unknown3, uint unknown4, byte unknown5, byte unknown6, uint unknown7, uint unknown8,
            uint catalogRoofStyle, uint catalogWallStyle, float unknown9, uint unknown10,
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
            this.catalogRoofPattern = catalogRoofStyle;
            this.catalogWallStyle = catalogWallStyle;
            this.unknown9 = unknown9;
            this.unknown10 = unknown10;
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
            this.unknown6 = r.ReadByte();
            this.unknown7 = r.ReadUInt32();
            this.unknown8 = r.ReadUInt32();
            this.catalogRoofPattern = r.ReadUInt32();
            this.catalogWallStyle = r.ReadUInt32();
            this.unknown9 = r.ReadSingle();
            this.unknown10 = r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(RoofStyleCatalogResource)) && s.Position != s.Length)
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
            w.Write(catalogRoofPattern);
            w.Write(catalogWallStyle);
            w.Write(unknown9);
            w.Write(unknown10);

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
        public byte Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(26)]
        public uint Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(27)]
        public uint Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogRoofPattern { get { return catalogRoofPattern; } set { if (catalogRoofPattern != value) { catalogRoofPattern = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogWallStyle { get { return catalogWallStyle; } set { if (catalogWallStyle != value) { catalogWallStyle = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30)]
        public float Unknown9 { get { return unknown9; } set { if (unknown9 != value) { unknown9 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31)]
        public uint Unknown10 { get { return unknown10; } set { if (unknown10 != value) { unknown10 = value; OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
