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
    public class WallCatalogResource : CatalogResourceTGIBlockList
    {
        #region Attributes
        uint unknown2;
        uint unknown3;
        byte unknown4;
        uint unknown5;
        byte unknown6;
        uint unknown7;
        byte unknown8;
        uint unknown9;
        byte[] unknown10 = new byte[4];
        uint unknown11;
        uint unknown12;
        uint unknown13;
        uint unknown14;
        uint unknown15;
        uint unknown16;
        byte[] unknown17 = new byte[8];
        #endregion

        #region Constructors
        public WallCatalogResource(int APIversion, Stream s) : base(APIversion, s) { }
        public WallCatalogResource(int APIversion, Stream unused, WallCatalogResource basis)
            : base(APIversion, basis.version, basis.list)
        {
            this.unknown2 = basis.unknown2;
            this.common = new Common(requestedApiVersion, OnResourceChanged, basis.common);
            this.unknown3 = basis.unknown3;
            this.unknown4 = basis.unknown4;
            this.unknown5 = basis.unknown5;
            this.unknown6 = basis.unknown6;
            this.unknown7 = basis.unknown7;
            this.unknown8 = basis.unknown8;
            this.unknown9 = basis.unknown9;
            this.unknown10 = (byte[])basis.unknown10.Clone();
            this.unknown11 = basis.unknown11;
            this.unknown12 = basis.unknown12;
            this.unknown13 = basis.unknown13;
            this.unknown14 = basis.unknown14;
            this.unknown15 = basis.unknown15;
            this.unknown16 = basis.unknown16;
            this.unknown17 = (byte[])basis.unknown17.Clone();
        }
        public WallCatalogResource(int APIversion, uint version, uint unknown2, Common common,
            uint unknown3, byte unknown4, uint unknown5, byte unknown6, uint unknown7, byte unknown8, uint unknown9,
            byte[] unknown10, uint unknown11, uint unknown12, uint unknown13, uint unknown14, uint unknown15, uint unknown16, byte[] unknown17,
            TGIBlockList ltgib)
            : base(APIversion, version, ltgib)
        {
            this.unknown2 = unknown2;
            this.common = new Common(requestedApiVersion, OnResourceChanged, common);
            this.unknown3 = unknown3;
            this.unknown4 = unknown4;
            this.unknown5 = unknown5;
            this.unknown6 = unknown6;
            this.unknown7 = unknown7;
            this.unknown8 = unknown8;
            this.unknown9 = unknown9;
            if (unknown10.Length != this.unknown10.Length) throw new ArgumentLengthException("unknown10", this.unknown10.Length);
            this.unknown10 = (byte[])unknown10.Clone();
            this.unknown11 = unknown11;
            this.unknown12 = unknown12;
            this.unknown13 = unknown13;
            this.unknown14 = unknown14;
            this.unknown15 = unknown15;
            this.unknown16 = unknown16;
            if (unknown17.Length != this.unknown17.Length) throw new ArgumentLengthException("unknown17", this.unknown17.Length);
            this.unknown17 = (byte[])unknown17.Clone();
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
            this.unknown7 = r.ReadUInt32();
            this.unknown8 = r.ReadByte();
            this.unknown9 = r.ReadUInt32();
            this.unknown10 = r.ReadBytes(4);
            if (checking) if (unknown10.Length != 4)
                    throw new InvalidDataException(String.Format("unknown10: read {0} bytes; expected 4 at 0x{1:X8}.", unknown10.Length, s.Position));
            this.unknown11 = r.ReadUInt32();
            this.unknown12 = r.ReadUInt32();
            this.unknown13 = r.ReadUInt32();
            this.unknown14 = r.ReadUInt32();
            this.unknown15 = r.ReadUInt32();
            this.unknown16 = r.ReadUInt32();
            this.unknown17 = r.ReadBytes(8);
            if (checking) if (unknown17.Length != 8)
                    throw new InvalidDataException(String.Format("unknown17: read {0} bytes; expected 8 at 0x{1:X8}.", unknown17.Length, s.Position));

            list = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);

            if (checking) if (this.GetType().Equals(typeof(WallCatalogResource)) && s.Position != s.Length)
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
            w.Write(unknown8);
            w.Write(unknown9);
            w.Write(unknown10);
            w.Write(unknown11);
            w.Write(unknown12);
            w.Write(unknown13);
            w.Write(unknown14);
            w.Write(unknown15);
            w.Write(unknown16);
            w.Write(unknown17);

            base.UnParse(s);

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
        public uint Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(27)]
        public byte Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(28)]
        public uint Unknown9 { get { return unknown9; } set { if (unknown9 != value) { unknown9 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(29)]
        public byte[] Unknown10
        {
            get { return (byte[])unknown10.Clone(); }
            set
            {
                if (value.Length != this.unknown10.Length) throw new ArgumentLengthException("Unknown10", this.unknown10.Length);
                if (!ArrayCompare(unknown10, value)) { unknown10 = (byte[])value.Clone(); OnResourceChanged(this, new EventArgs()); }
            }
        }
        [ElementPriority(30)]
        public uint Unknown11 { get { return unknown11; } set { if (unknown11 != value) { unknown11 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(31)]
        public uint Unknown12 { get { return unknown12; } set { if (unknown12 != value) { unknown12 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(32)]
        public uint Unknown13 { get { return unknown13; } set { if (unknown13 != value) { unknown13 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(33)]
        public uint Unknown14 { get { return unknown14; } set { if (unknown14 != value) { unknown14 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(34)]
        public uint Unknown15 { get { return unknown15; } set { if (unknown15 != value) { unknown15 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(35)]
        public uint Unknown16 { get { return unknown16; } set { if (unknown16 != value) { unknown16 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(36)]
        public byte[] Unknown17
        {
            get { return (byte[])unknown17.Clone(); }
            set
            {
                if (value.Length != this.unknown17.Length) throw new ArgumentLengthException("Unknown17", this.unknown17.Length);
                if (!ArrayCompare(unknown17, value)) { unknown17 = (byte[])value.Clone(); OnResourceChanged(this, new EventArgs()); }
            }
        }
        #endregion
    }
}
