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
    public class FenceCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        MaterialList materialList = null;
        uint unknown2;
        byte unknown3;
        uint unknown4;
        byte unknown5;
        uint unknown6;
        byte unknown7;
        uint modelVPXYIndex;
        uint diagonalVPXYIndex;
        uint postVPXYIndex;
        uint tileSpacing;
        byte canWalkOver;
        byte risesAboveWall;
        uint wallIndex;
        #endregion

        #region Constructors
        public FenceCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public FenceCatalogResource(int APIversion, Stream unused, FenceCatalogResource basis)
            : base(APIversion, basis.version, basis.list)
        {
            this.materialList = (basis.version >= 0x00000007) ? new MaterialList(OnResourceChanged, basis.materialList) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown2 = basis.unknown2;
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.modelVPXYIndex = basis.modelVPXYIndex;
            this.diagonalVPXYIndex = basis.diagonalVPXYIndex;
            this.postVPXYIndex = basis.postVPXYIndex;
            this.tileSpacing = basis.tileSpacing;
            this.canWalkOver = basis.canWalkOver;
        }
        public FenceCatalogResource(int APIversion, uint version, Common common, uint unknown2, byte unknown3, uint unknown4,
            byte unknown5, uint unknown6, byte unknown7, uint unknown8, uint unknown9, uint unknown10, uint unknown11, byte unknown12,
            TGIBlockList ltgib)
            : this(APIversion, version, null, common, unknown2, unknown3, unknown4, unknown5, unknown6, unknown7, unknown8, unknown9, unknown10, unknown11, unknown12, ltgib)
        {
            if (checking) if (version >= 0x00000007)
                    throw new InvalidOperationException(String.Format("Constructor requires MaterialList for version {0}", version));
        }
        public FenceCatalogResource(int APIversion, uint version, MaterialList materialList, Common common, uint unknown2, byte unknown3, uint unknown4,
            byte unknown5, uint unknown6, byte unknown7, uint unknown8, uint unknown9, uint unknown10, uint unknown11, byte unknown12,
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
            this.modelVPXYIndex = unknown8;
            this.diagonalVPXYIndex = unknown9;
            this.postVPXYIndex = unknown10;
            this.tileSpacing = unknown11;
            this.canWalkOver = unknown12;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);
            this.materialList = (this.version >= 0x00000007) ? new MaterialList(OnResourceChanged, s) : null;
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.unknown2 = r.ReadUInt32();
            this.unknown3 = r.ReadByte();
            this.unknown4 = r.ReadUInt32();
            this.unknown5 = r.ReadByte();
            this.unknown6 = r.ReadUInt32();
            this.unknown7 = r.ReadByte();
            this.modelVPXYIndex = r.ReadUInt32();
            this.diagonalVPXYIndex = r.ReadUInt32();
            this.postVPXYIndex = r.ReadUInt32();
            this.tileSpacing = r.ReadUInt32();
            this.canWalkOver = r.ReadByte();
            if (this.version >= 0x00000008)
            {
                this.risesAboveWall = r.ReadByte();
                this.wallIndex = r.ReadUInt32();
            }

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(FenceCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);
            if (version >= 0x00000007)
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
            w.Write(modelVPXYIndex);
            w.Write(diagonalVPXYIndex);
            w.Write(postVPXYIndex);
            w.Write(tileSpacing);
            w.Write(canWalkOver);
            if (this.version >= 0x00000008)
            {
                w.Write(risesAboveWall);
                w.Write(wallIndex);
            }

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
            get {
                List<string> res = base.ContentFields;
                if (this.version < 0x00000008)
                {
                    res.Remove("RisesAboveWall");
                    res.Remove("WallIndex");
                    if (this.version < 0x00000007)
                    {
                        res.Remove("Materials");
                    }
                }
                return res;
            }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public MaterialList Materials
        {
            get { if (version < 0x00000007) throw new InvalidOperationException(); return materialList; }
            set { if (version < 0x00000007) throw new InvalidOperationException(); if (materialList != value) { materialList = value == null ? null : new MaterialList(OnResourceChanged, value); } OnResourceChanged(this, new EventArgs()); }
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
        public uint ModelVPXYIndex { get { return modelVPXYIndex; } set { if (modelVPXYIndex != value) { modelVPXYIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28), TGIBlockListContentField("TGIBlocks")]
        public uint DiagonalVPXYIndex { get { return diagonalVPXYIndex; } set { if (diagonalVPXYIndex != value) { diagonalVPXYIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29), TGIBlockListContentField("TGIBlocks")]
        public uint PostVPXYIndex { get { return postVPXYIndex; } set { if (postVPXYIndex != value) { postVPXYIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30)]
        public uint TileSpacing { get { return tileSpacing; } set { if (tileSpacing != value) { tileSpacing = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31)]
        public byte CanWalkOver { get { return canWalkOver; } set { if (canWalkOver != value) { canWalkOver = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32), TGIBlockListContentField("TGIBlocks")]
        public byte RisesAboveWall
        {
            get { if (version < 0x00000008) throw new InvalidOperationException(); return risesAboveWall; }
            set { if (version < 0x00000008) throw new InvalidOperationException(); if (risesAboveWall != value) { risesAboveWall = value; OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(33), TGIBlockListContentField("TGIBlocks")]
        public uint WallIndex
        {
            get { if (version < 0x00000008) throw new InvalidOperationException(); return wallIndex; }
            set { if (version < 0x00000008) throw new InvalidOperationException(); if (wallIndex != value) { wallIndex = value; OnResourceChanged(this, new EventArgs()); } }
        }
        #endregion
    }
}
