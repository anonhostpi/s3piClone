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
    public class StairsCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        MaterialList materialList = null;
        uint unknown2;
        byte unknown3;
        uint unknown4;
        byte unknown5;
        uint unknown6;
        byte unknown7;
        uint vpxy_index1;
        uint vpxy_index2;
        uint vpxy_index3;
        uint catalogRailing;
        uint catalogWall;
        uint catalogWallFloorPattern;
        uint catalogFence;
        #endregion

        #region Constructors
        public StairsCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public StairsCatalogResource(int APIversion, Stream unused, StairsCatalogResource basis)
            : base(APIversion, basis.version, basis.list)
        {
            this.materialList = (basis.version >= 0x00000003) ? new MaterialList(OnResourceChanged, basis.materialList) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown2 = basis.unknown2;
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.vpxy_index1 = basis.vpxy_index1;
            this.vpxy_index2 = basis.vpxy_index2;
            this.vpxy_index3 = basis.vpxy_index3;
            this.catalogRailing = basis.catalogRailing;
            this.catalogWall = basis.catalogWall;
            this.catalogWallFloorPattern = basis.catalogWallFloorPattern;
            this.catalogFence = basis.catalogFence;
        }
        public StairsCatalogResource(int APIversion, uint version, Common common,
        uint unknown2, byte unknown3, uint unknown4, byte unknown5, uint unknown6, byte unknown7, uint index1, uint index2, uint index3,
        uint catalogRailing, uint catalogWall, uint catalogWallFloorPattern, uint catalogFence,
        TGIBlockList ltgib)
            : this(APIversion, version, null, common, unknown2, unknown3, unknown4, unknown5, unknown6, unknown7, index1, index2, index3, catalogRailing, catalogWall, catalogWallFloorPattern, catalogFence, ltgib)
        {
            if (checking) if (version >= 0x00000003)
                    throw new InvalidOperationException(String.Format("Constructor requires MaterialList for version {0}", version));
        }
        public StairsCatalogResource(int APIversion, uint version, MaterialList materialList, Common common,
            uint unknown2, byte unknown3, uint unknown4, byte unknown5, uint unknown6, byte unknown7, uint index1, uint index2, uint index3,
            uint catalogRailing, uint catalogWall, uint catalogWallFloorPattern, uint catalogFence,
            TGIBlockList ltgib)
            : base(APIversion, version, ltgib)
        {
            this.materialList = materialList != null ? new MaterialList(OnResourceChanged, materialList) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, common);
            this.unknown2 = unknown2;
            this.unknown3 = unknown3;
            this.unknown4 = unknown4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.unknown7 = unknown7;
            this.vpxy_index1 = index1;
            this.vpxy_index2 = index2;
            this.vpxy_index3 = index3;
            this.catalogRailing = catalogRailing;
            this.catalogWall = catalogWall;
            this.catalogWallFloorPattern = catalogWallFloorPattern;
            this.catalogFence = catalogFence;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);

            this.materialList = (this.version >= 0x00000003) ? new MaterialList(OnResourceChanged, s) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.unknown2 = r.ReadUInt32();
            this.unknown3 = r.ReadByte();
            this.unknown4 = r.ReadUInt32();
            this.unknown5 = r.ReadByte();
            this.unknown6 = r.ReadUInt32();
            this.unknown7 = r.ReadByte();
            this.vpxy_index1 = r.ReadUInt32();
            this.vpxy_index2 = r.ReadUInt32();
            this.vpxy_index3 = r.ReadUInt32();
            this.catalogRailing = r.ReadUInt32();
            this.catalogWall = r.ReadUInt32();
            this.catalogWallFloorPattern = r.ReadUInt32();
            this.catalogFence = r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(RoofStyleCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);

            if (version >= 0x00000003)
            {
                if (materialList == null) materialList = new MaterialList(OnResourceChanged);
                materialList.UnParse(s);
            }
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
            w.Write(vpxy_index3);
            w.Write(catalogRailing);
            w.Write(catalogWall);
            w.Write(catalogWallFloorPattern);
            w.Write(catalogFence);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region AApiVersionedFields
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (this.version < 0x00000003) res.Remove("Materials");
                return res;
            }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public MaterialList Materials
        {
            get { if (version < 0x00000003) throw new InvalidOperationException(); return materialList; }
            set { if (version < 0x00000003) throw new InvalidOperationException(); if (materialList != value) { materialList = value == null ? null : new MaterialList(OnResourceChanged, value); } OnResourceChanged(this, new EventArgs()); }
        }
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
        [ElementPriority(29), TGIBlockListContentField("TGIBlocks")]
        public uint VPXYIndex3 { get { return vpxy_index3; } set { if (vpxy_index3 != value) { vpxy_index3 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogRailingIndex { get { return catalogRailing; } set { if (catalogRailing != value) { catalogRailing = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogWallIndex { get { return catalogWall; } set { if (catalogWall != value) { catalogWall = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogWallFloorPatternIndex { get { return catalogWallFloorPattern; } set { if (catalogWallFloorPattern != value) { catalogWallFloorPattern = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(33), TGIBlockListContentField("TGIBlocks")]
        public uint CatalogFenceIndex { get { return catalogFence; } set { if (catalogFence != value) { catalogFence = value; OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
