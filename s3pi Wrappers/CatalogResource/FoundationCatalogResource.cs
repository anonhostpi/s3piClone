/***************************************************************************
 *  Copyright (C) 2010 by Peter L Jones                                    *
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
    public class FoundationCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        FoundationType foundation;
        uint wallCatalogIndex;
        uint floorCatalogIndex;
        uint index3;
        uint index4;
        uint unknown5;
        uint unknown6;
        ShapeType shape;
        #endregion

        #region Constructors
        public FoundationCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public FoundationCatalogResource(int APIversion, Stream unused, FoundationCatalogResource basis)
            : this(APIversion,
            basis.version, basis.common,
            basis.foundation,
            basis.wallCatalogIndex,
            basis.floorCatalogIndex,
            basis.index3,
            basis.index4,
            basis.unknown5,
            basis.unknown6,
            basis.shape,
            basis.list) { }
        public FoundationCatalogResource(int APIversion,
            uint version, Common common,
            FoundationType foundation,
            uint wallCatalogIndex,
            uint floorCatalogIndex,
            uint index3,
            uint index4,
            uint unknown5,
            uint unknown6,
            ShapeType shape,
            TGIBlockList ltgib)
            : base(APIversion, version, common, ltgib)
        {
            this.foundation = foundation;
            this.wallCatalogIndex = wallCatalogIndex;
            this.floorCatalogIndex = floorCatalogIndex;
            this.index3 = index3;
            this.index4 = index4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.shape = shape;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            base.Parse(s);
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.foundation = (FoundationType)r.ReadUInt32();
            this.wallCatalogIndex = r.ReadUInt32();
            this.floorCatalogIndex = r.ReadUInt32();
            this.index3 = r.ReadUInt32();
            this.index4 = r.ReadUInt32();
            this.unknown5 = r.ReadUInt32();
            this.unknown6 = r.ReadUInt32();
            this.shape = (ShapeType)r.ReadUInt32();

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(FoundationCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);
            if (common == null) common = new Common(requestedApiVersion, OnResourceChanged);
            common.UnParse(s);
            w.Write((uint)foundation);
            w.Write(wallCatalogIndex);
            w.Write(floorCatalogIndex);
            w.Write(index3);
            w.Write(index4);
            w.Write(unknown5);
            w.Write(unknown6);
            w.Write((uint)shape);

            base.UnParse(s);

            w.Flush();

            return s;
        }
        #endregion

        #region Sub-classes
        public enum FoundationType : uint
        {
            Uninitialized = 0x00000000,
            Deck = 0x00000001,
            Foundation = 0x00000002,
            Pool = 0x00000003,
            Frieze = 0x00000004,
            Platform = 0x00000005,
            Fountain = 0x00000006,
        }
        public enum ShapeType : uint
        {
            Rectangle = 0x00000000,
            Diamond = 0x00000001,
        }
        #endregion

        #region Content Fields
        //--insert Version: ElementPriority(1)
        //--insert CommonBlock: ElementPriority(11)
        [ElementPriority(21), TGIBlockListContentField("TGIBlocks")]
        public FoundationType Foundation { get { return foundation; } set { if (foundation != value) { foundation = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(22), TGIBlockListContentField("TGIBlocks")]
        public uint WallCatalogIndex { get { return wallCatalogIndex; } set { if (wallCatalogIndex != value) { wallCatalogIndex = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(23), TGIBlockListContentField("TGIBlocks")]
        public uint FloorCatalogIndex { get { return floorCatalogIndex; } set { if (floorCatalogIndex != value) { floorCatalogIndex = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(24), TGIBlockListContentField("TGIBlocks")]
        public uint Index3 { get { return index3; } set { if (index3 != value) { index3 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(25), TGIBlockListContentField("TGIBlocks")]
        public uint Index4 { get { return index4; } set { if (index4 != value) { index4 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(26)]
        public uint Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(26)]
        public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(28)]
        public ShapeType Shape { get { return shape; } set { if (shape != value) { shape = value; OnResourceChanged(this, EventArgs.Empty); } } }
        //--insert TGIBlockList: no ElementPriority
        #endregion
    }
}
