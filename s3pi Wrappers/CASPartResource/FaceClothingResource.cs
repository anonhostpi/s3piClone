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

namespace CASPartResource
{
    /// <summary>
    /// A resource wrapper that understands Face and Clothing resources
    /// </summary>
    public class FaceClothingResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint version = 8;
        string partName = "";
        uint unknown1 = 2;
        TGIBlock blendGeometry;
        CASEntryList casEntries;
        TGIBlockList tgiBlocks;
        #endregion

        public FaceClothingResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            long tgiPosn, tgiSize;
            BinaryReader r = new BinaryReader(s);
            BinaryReader r2 = new BinaryReader(s, System.Text.Encoding.BigEndianUnicode);

            version = r.ReadUInt32();
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();

            partName = r2.ReadString();
            unknown1 = r.ReadUInt32();
            blendGeometry = new TGIBlock(requestedApiVersion, OnResourceChanged, s);
            casEntries = new CASEntryList(OnResourceChanged, s);

            tgiBlocks = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize, true);
        }

        protected override Stream UnParse()
        {
            long posn;
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(version);

            posn = s.Position;
            w.Write((uint)0);
            w.Write((uint)0);

            Write7BitStr(s, partName, System.Text.Encoding.BigEndianUnicode);
            w.Write(unknown1);
            if (blendGeometry == null) blendGeometry = new TGIBlock(requestedApiVersion, OnResourceChanged);
            blendGeometry.UnParse(s);
            if (casEntries == null) casEntries = new CASEntryList(OnResourceChanged);
            casEntries.UnParse(s);

            if (tgiBlocks == null) tgiBlocks = new TGIBlockList(OnResourceChanged, true);
            tgiBlocks.UnParse(s, posn);

            s.Flush();

            return s;
        }
        #endregion

        #region Sub-types
        public class Entry : AHandlerElement, IEquatable<Entry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            AgeGenderFlags ageGender;
            float amount;
            int index;
            #endregion

            #region Constructors
            public Entry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Entry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Entry(int APIversion, EventHandler handler, Entry basis) : this(APIversion, handler, basis.ageGender, basis.amount, basis.index) { }
            public Entry(int APIversion, EventHandler handler, AgeGenderFlags ageGender, float amount, int index)
                : base(APIversion, handler) { this.ageGender = ageGender; this.amount = amount; this.index = index; }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                ageGender = (AgeGenderFlags)r.ReadUInt32();
                amount = r.ReadSingle();
                index = r.ReadInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)ageGender);
                w.Write(amount);
                w.Write(index);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            public override AHandlerElement Clone(EventHandler handler) { return new Entry(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(Entry other)
            {
                return
                    this.ageGender == other.ageGender
                    && this.amount == other.amount
                    && this.index == other.index
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public AgeGenderFlags AgeGender { get { return ageGender; } set { if (ageGender != value) { ageGender = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float Amount { get { return amount; } set { if (amount != value) { amount = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public int Index { get { return index; } set { if (index != value) { index = value; OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    string s = "";
                    foreach (string field in ContentFields)
                        if (!field.Equals("Value"))
                            s += string.Format("{0}: {1}; ", field, this[field]);
                    return s.Trim();
                }
            }
            #endregion
        }

        public class CASEntry : AHandlerElement, IEquatable<CASEntry>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            FacialRegionFlags facialRegion;
            uint andBone;
            uint useGeom;
            Entry geom;
            uint useBone;
            Entry bone;
            #endregion

            #region Constructors
            public CASEntry(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public CASEntry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public CASEntry(int APIversion, EventHandler handler, CASEntry basis)
                : this(APIversion, handler, basis.facialRegion, basis.andBone, basis.useGeom, basis.geom, basis.useBone, basis.bone) { }
            public CASEntry(int APIversion, EventHandler handler, FacialRegionFlags facialRegion, uint andBone, uint useGeom, Entry geom, uint useBone, Entry bone)
                : base(APIversion, handler)
            {
                this.facialRegion = facialRegion;
                this.andBone = andBone;
                this.useGeom = useGeom;
                this.geom = new Entry(requestedApiVersion, handler, geom);
                this.useBone = useBone;
                this.bone = new Entry(requestedApiVersion, handler, bone);
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                facialRegion = (FacialRegionFlags)r.ReadUInt32();
                andBone = r.ReadUInt32();
                if (andBone == 0)
                    useGeom = r.ReadUInt32();
                if (andBone != 0 || useGeom != 0)
                    geom = new Entry(requestedApiVersion, handler, s);
                if (andBone != 0)
                {
                    useBone = r.ReadUInt32();
                    if (useBone != 0)
                        bone = new Entry(requestedApiVersion, handler, s);
                }
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)facialRegion);
                w.Write(andBone);
                if (andBone == 0)
                    w.Write(useGeom);
                if (andBone != 0 || useGeom != 0)
                {
                    if (geom == null) geom = new Entry(requestedApiVersion, handler);
                    geom.UnParse(s);
                }
                if (andBone != 0)
                {
                    w.Write(useBone);
                    if (useBone != 0)
                    {
                        if (bone == null) bone = new Entry(requestedApiVersion, handler);
                        bone.UnParse(s);
                    }
                }
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override AHandlerElement Clone(EventHandler handler) { return new CASEntry(requestedApiVersion, handler, this); }
            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    if (andBone != 0)
                        res.Remove("UseGeom");
                    else
                        if (useGeom == 0)
                            res.Remove("Geom");
                    if (andBone == 0)
                    {
                        res.Remove("UseBone");
                        res.Remove("Bone");
                    }
                    else
                        if (useBone == 0)
                            res.Remove("Bone");
                    return res;
                }
            }
            #endregion

            #region IEquatable<Entry> Members

            public bool Equals(CASEntry other)
            {
                return
                    this.facialRegion == other.facialRegion
                    && this.andBone == other.andBone
                    && (this.andBone == 0 &&
                        this.useGeom == other.useGeom)
                    && this.geom.Equals(other.geom)
                    && (this.andBone > 0 &&
                        this.useBone == other.useBone
                        && this.bone.Equals(other.bone)
                    )
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public FacialRegionFlags FacialRegion { get { return facialRegion; } set { if (facialRegion != value) { facialRegion = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint AndBone { get { return andBone; } set { if (andBone != value) { andBone = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint UseGeom { get { return useGeom; } set { if (useGeom != value) { useGeom = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Entry Geom { get { return geom; } set { if (!geom.Equals(value)) { geom = new Entry(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public uint UseBone { get { return useBone; } set { if (useBone != value) { useBone = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public Entry Bone { get { return bone; } set { if (!bone.Equals(value)) { bone = new Entry(requestedApiVersion, handler, value); OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    string s = "";
                    foreach (string field in ContentFields)
                        if (field == "Value") continue;
                        else if (field == "Geom" || field == "Bone")
                            s += string.Format("\n{0}: {1}", field, (this[field].Value as Entry).Value);
                        else
                            s += string.Format("\n{0}: {1}", field, this[field]);
                    return s;
                }
            }
            #endregion
        }
        public class CASEntryList : AResource.DependentList<CASEntry>
        {
            #region Constructors
            public CASEntryList(EventHandler handler) : base(handler) { }
            public CASEntryList(EventHandler handler, Stream s) : base(handler, s) { }
            public CASEntryList(EventHandler handler, IList<CASEntry> le) : base(handler, le) { }
            #endregion

            #region Data I/O
            protected override CASEntry CreateElement(Stream s) { return new CASEntry(0, elementHandler, s); }
            protected override void WriteElement(Stream s, CASEntry element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new CASEntry(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(2)]
        public string PartName { get { return partName; } set { if (partName != value) { partName = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(3)]
        public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(4)]
        public TGIBlock BlendGeometry { get { return blendGeometry; } set { if (!blendGeometry.Equals(value)) { blendGeometry = new TGIBlock(requestedApiVersion, OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(5)]
        public CASEntryList CASEntries { get { return casEntries; } set { if (!casEntries.Equals(value)) { casEntries = new CASEntryList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(6)]
        public TGIBlockList TGIBlocks { get { return tgiBlocks; } set { if (!tgiBlocks.Equals(value)) { tgiBlocks = new TGIBlockList(OnResourceChanged, value, true); OnResourceChanged(this, new EventArgs()); } } }

        public string Value
        {
            get
            {
                string s = "";
                string fmt = "";

                s += "Version: " + this["Version"];
                s += "\nPartName: " + this["PartName"];
                s += "\nUnknown1: " + this["Unknown1"];
                s += "\nBlendGeometry: " + this["BlendGeometry"];

                s += "\n\nCAS Entries:";
                fmt = "\n-- 0x{0:X" + casEntries.Count.ToString("X").Length + "} --{1}";
                for (int i = 0; i < casEntries.Count; i++)
                    s += String.Format(fmt, i, casEntries[i].Value);

                s += "\n---\n\nTGI Blocks:";
                fmt = "\n  [0x{0:X" + tgiBlocks.Count.ToString("X").Length + "}]: {1}";
                for (int i = 0; i < tgiBlocks.Count; i++)
                    s += String.Format(fmt, i, tgiBlocks[i]);

                return s;
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for FaceClothingResource wrapper
    /// </summary>
    public class FaceClothingResourceHandler : AResourceHandler
    {
        public FaceClothingResourceHandler()
        {
            this.Add(typeof(FaceClothingResource), new List<string>(new string[] { "0x0358B08A", "0x062C8204", }));
        }
    }
}
