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
    class TerrainPaintBrushCatalogResource : TerrainGeometryWaterBrushCatalogResource
    {
        #region Attributes
        TGIBlock brushTexture = null;
        uint unknown15;
        CategoryType category = CategoryType.None;
        #endregion

        #region Constructors
        public TerrainPaintBrushCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public TerrainPaintBrushCatalogResource(int APIversion, Stream unused, TerrainPaintBrushCatalogResource basis)
            : base(APIversion, null, basis)
        {
            this.brushTexture = (TGIBlock)basis.brushTexture.Clone(OnResourceChanged);
            this.unknown15 = basis.unknown15;
            this.category = basis.category;
        }
        public TerrainPaintBrushCatalogResource(int APIversion, uint unknown2, Common common,
            byte unknown3, byte unknown4, uint unknown5, byte unknown6, byte unknown7, uint unknown8, uint unknown9, uint unknown10,
            TGIBlock brushShape, byte[] unknown11, float unknown12, float unknown13, byte[] unknown14,
            TGIBlock brushTexture)
            : base(APIversion, 2, unknown2, common, unknown3, unknown4, unknown5, unknown6, unknown7, unknown8, unknown9, unknown10,
            brushShape, unknown11, unknown12, unknown13, unknown14)
        {
            this.brushTexture = (TGIBlock)brushTexture.Clone(OnResourceChanged);
        }
        public TerrainPaintBrushCatalogResource(int APIversion, uint unknown2, Common common,
            byte unknown3, byte unknown4, uint unknown5, byte unknown6, byte unknown7, uint unknown8, uint unknown9, uint unknown10,
            TGIBlock brushShape, byte[] unknown11, float unknown12, float unknown13, byte[] unknown14,
            TGIBlock brushTexture, uint unknown15, CategoryType category)
            : base(APIversion, 4, unknown2, common, unknown3, unknown4, unknown5, unknown6, unknown7, unknown8, unknown9, unknown10,
            brushShape, unknown11, unknown12, unknown13, unknown14)
        {
            this.brushTexture = (TGIBlock)brushTexture.Clone(OnResourceChanged);
            this.unknown15 = unknown15;
            this.category = category;
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            base.Parse(s);
            this.brushTexture = new TGIBlock(requestedApiVersion, OnResourceChanged, s);

            if (version >= 4)
            {
                BinaryReader r = new BinaryReader(s);
                this.unknown15 = r.ReadUInt32();
                this.category = (CategoryType)r.ReadUInt32();
            }

            if (checking) if (this.GetType().Equals(typeof(TerrainPaintBrushCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            if (brushTexture == null) brushTexture = new TGIBlock(requestedApiVersion, OnResourceChanged, 0, 0, 0);
            brushTexture.UnParse(s);

            if (version >= 4)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown15);
                w.Write((uint)category);
            }

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
                if (this.version < 0x00000004)
                {
                    res.Remove("Unknown15");
                    res.Remove("Category");
                }
                return res;
            }
        }
        #endregion

        #region Sub-classes
        public enum CategoryType : uint
        {
            None = 0x00,
            Grass = 0x01,
            Flowers = 0x02,
            Rock = 0x03,
            Dirt_Sand = 0x04,
            Other = 0x05,
        }
        #endregion

        #region Content Fields
        [ElementPriority(35)]
        public TGIBlock BrushTexture
        {
            get { return brushTexture; }
            set { if (brushTexture != value) { brushTexture = new TGIBlock(requestedApiVersion, OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } }
        }
        [ElementPriority(36)]
        public uint Unknown15 { get { if (version < 0x00000004) throw new InvalidOperationException(); return unknown15; } set { if (version < 0x00000004) throw new InvalidOperationException(); if (unknown15 != value) { unknown15 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(37)]
        public CategoryType Category { get { if (version < 0x00000004) throw new InvalidOperationException(); return category; } set { if (version < 0x00000004) throw new InvalidOperationException(); if (category != value) { category = value; OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
