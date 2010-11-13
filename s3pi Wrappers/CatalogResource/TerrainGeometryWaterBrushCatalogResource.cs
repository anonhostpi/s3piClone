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
    public class TerrainGeometryWaterBrushCatalogResource : CatalogResource
    {
        #region Attributes
        uint unknown2;
        uint unknown3;
        byte unknown4;
        uint unknown5;
        byte unknown6;
        byte unknown7;
        uint brushIndex;
        uint unknown9;
        uint unknown10;
        TGIBlock brushShape = null;
        byte[] unknown11 = new byte[4];
        float unknown12;
        float unknown13;
        byte[] unknown14 = new byte[5];
        #endregion

        #region Constructors
        public TerrainGeometryWaterBrushCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public TerrainGeometryWaterBrushCatalogResource(int APIversion, Stream unused, TerrainGeometryWaterBrushCatalogResource basis)
            : base(APIversion, basis.version)
        {
            this.unknown2 = basis.unknown2;
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.brushIndex = basis.brushIndex;
            this.unknown9 = basis.unknown9;
            this.unknown10 = basis.unknown10;
            this.brushShape = (TGIBlock)basis.brushShape.Clone(OnResourceChanged);
            this.unknown11 = (byte[])basis.unknown11.Clone();
            this.unknown12 = basis.unknown12;
            this.unknown13 = basis.unknown13;
            this.unknown14 = (byte[])basis.unknown14.Clone();
        }
        public TerrainGeometryWaterBrushCatalogResource(int APIversion, uint version, uint unknown2, Common common,
            byte unknown3, byte unknown4, uint unknown5, byte unknown6, byte unknown7, uint unknown8, uint unknown9, uint unknown10,
            TGIBlock brushShape, byte[] unknown11, float unknown12, float unknown13, byte[] unknown14)
            : base(APIversion, version)
        {
            this.unknown2 = unknown2;
            this.common = new Common(requestedApiVersion, OnResourceChanged, common);
            this.unknown3 = unknown3;
            this.unknown4 = unknown4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.unknown7 = unknown7;
            this.brushIndex = unknown8;
            this.unknown9 = unknown9;
            this.unknown10 = unknown10;
            this.brushShape = (TGIBlock)brushShape.Clone(OnResourceChanged);
            if (unknown11.Length != this.unknown11.Length) throw new ArgumentLengthException("unknown11", this.unknown11.Length);
            this.unknown11 = (byte[])unknown11.Clone();
            this.unknown12 = unknown12;
            this.unknown13 = unknown13;
            if (unknown14.Length != this.unknown14.Length) throw new ArgumentLengthException("unknown14", this.unknown14.Length);
            this.unknown14 = (byte[])unknown14.Clone();
        }
        #endregion

        #region Data I/O
        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            base.Parse(s);
            this.unknown2 = r.ReadUInt32();
            this.common = new Common(requestedApiVersion, OnResourceChanged, s);
            this.unknown3 = r.ReadUInt32();
            this.unknown4 = r.ReadByte();
            this.unknown5 = r.ReadUInt32();
            this.unknown6 = r.ReadByte();
            this.unknown7 = r.ReadByte();
            this.brushIndex = r.ReadUInt32();
            this.unknown9 = r.ReadUInt32();
            this.unknown10 = r.ReadUInt32();
            this.brushShape = new TGIBlock(requestedApiVersion, OnResourceChanged, s);
            this.unknown11 = r.ReadBytes(4);
            if (checking) if (this.unknown11.Length != 4)
                    throw new InvalidDataException(String.Format("unknown11: read {0} bytes; expected 4 at 0x{1:X8}.", unknown11.Length, s.Position));
            this.unknown12 = r.ReadSingle();
            this.unknown13 = r.ReadSingle();
            this.unknown14 = r.ReadBytes(5);
            if (checking) if (this.unknown14.Length != 5)
                    throw new InvalidDataException(String.Format("unknown14: read {0} bytes; expected 5 at 0x{1:X8}.", unknown14.Length, s.Position));

            if (checking) if (this.GetType().Equals(typeof(TerrainGeometryWaterBrushCatalogResource)) && s.Position != s.Length)
                    throw new InvalidDataException(String.Format("Data stream length 0x{0:X8} is {1:X8} bytes longer than expected at {2:X8}",
                        s.Length, s.Length - s.Position, s.Position));
        }

        protected override Stream UnParse()
        {
            Stream s = base.UnParse();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(unknown2);
            if (common == null) common = new Common(requestedApiVersion, OnResourceChanged);
            common.UnParse(s);
            w.Write(unknown3);
            w.Write(unknown4);
            w.Write(unknown5);
            w.Write(unknown6);
            w.Write(unknown7);
            w.Write(brushIndex);
            w.Write(unknown9);
            w.Write(unknown10);
            if (brushShape == null) brushShape = new TGIBlock(requestedApiVersion, OnResourceChanged, 0, 0, 0);
            brushShape.UnParse(s);
            w.Write(unknown11);
            w.Write(unknown12);
            w.Write(unknown13);
            w.Write(unknown14);

            w.Flush();

            return s;
        }
        #endregion

        #region Content Fields
        [ElementPriority(21)]
        public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(22)]
        public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(23)]
        public byte Unknown4 { get { return unknown4; } set { if (unknown4 != value) { unknown4 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(24)]
        public uint Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(25)]
        public byte Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(26)]
        public byte Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(27)]
        public uint BrushIndex { get { return brushIndex; } set { if (brushIndex != value) { brushIndex = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28)]
        public uint Unknown9 { get { return unknown9; } set { if (unknown9 != value) { unknown9 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29)]
        public uint Unknown10 { get { return unknown10; } set { if (unknown10 != value) { unknown10 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(30)]
        public TGIBlock BrushShape
        {
            get { return brushShape; }
            set
            {
                if (brushShape != value)
                {
                    brushShape = new TGIBlock(requestedApiVersion, OnResourceChanged, value);
                    OnResourceChanged(this, new EventArgs());
                }
            }
        }
        [ElementPriority(31)]
        public byte[] Unknown11 { get { return (byte[])unknown11.Clone(); } set { if (!ArrayCompare(unknown11, value)) { unknown11 = (byte[])value.Clone(); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32)]
        public float Unknown12 { get { return unknown12; } set { if (unknown12 != value) { unknown12 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(33)]
        public float Unknown13 { get { return unknown13; } set { if (unknown13 != value) { unknown13 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(34)]
        public byte[] Unknown14 { get { return (byte[])unknown14.Clone(); } set { if (!ArrayCompare(unknown14, value)) { unknown14 = (byte[])value.Clone(); OnResourceChanged(this, new EventArgs()); } } }
        #endregion
    }
}
