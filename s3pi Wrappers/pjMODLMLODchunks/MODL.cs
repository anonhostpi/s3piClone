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

namespace s3pi.GenericRCOLResource
{
    public class MODL : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;
        const string TAG = "MODL";

        #region Attributes
        uint tag = (uint)FOURCC(TAG);
        uint version = 258;
        BoundingBox objectBoundingBox;
        BoundingBoxList extraBoundingBoxes;
        uint unknown1;
        uint unknown2;
        MLODReferenceList mlodReferences;
        #endregion

        #region Constructors
        public MODL(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public MODL(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public MODL(int APIversion, EventHandler handler, MODL basis)
            : base(APIversion, handler, null)
        {
            this.version = basis.version;
            this.objectBoundingBox = new BoundingBox(requestedApiVersion, handler, basis.objectBoundingBox);
            if (basis.version >= 0x0102)
            {
                this.extraBoundingBoxes = new BoundingBoxList(handler, basis.extraBoundingBoxes);
                this.unknown1 = basis.unknown1;
                this.unknown2 = basis.unknown2;
            }
            this.mlodReferences = new MLODReferenceList(handler, basis.mlodReferences);
        }

        public MODL(int APIversion, EventHandler handler, uint version, BoundingBox objectBoundingBox, IEnumerable<MLODReference> mlodReferences)
            : this(APIversion, handler, version, objectBoundingBox, null, 0, 0, mlodReferences)
        {
            if (checking) if (version >= 0x00000102)
                    throw new ArgumentException("extraBoundingBoxes, unknown1 and unknown2 required for version >= 0x0102.");
        }

        public MODL(int APIversion, EventHandler handler, uint version, BoundingBox objectBoundingBox,
            IEnumerable<BoundingBox>extraBoundingBoxes, uint unknown1, uint unknown2,
            IEnumerable<MLODReference> mlodReferences)
            : base(APIversion, handler, null)
        {
            this.version = version;
            this.objectBoundingBox = new BoundingBox(requestedApiVersion, handler, objectBoundingBox);
            if (version >= 0x0102)
            {
                this.extraBoundingBoxes = new BoundingBoxList(handler, extraBoundingBoxes);
                this.unknown1 = unknown1;
                this.unknown2 = unknown2;
            }
            this.mlodReferences = new MLODReferenceList(handler, mlodReferences);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x01661233; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC(TAG))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            version = r.ReadUInt32();
            int count = r.ReadInt32();
            objectBoundingBox = new BoundingBox(requestedApiVersion, handler, s);
            if (version >= 0x0102)
            {
                extraBoundingBoxes = new BoundingBoxList(handler, s);
                unknown1 = r.ReadUInt32();
                unknown2 = r.ReadUInt32();
            }
            mlodReferences = new MLODReferenceList(handler, count, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            if (mlodReferences == null) mlodReferences = new MLODReferenceList(handler);
            w.Write(mlodReferences.Count);
            
            if (objectBoundingBox == null) objectBoundingBox = new BoundingBox(requestedApiVersion, handler);
            objectBoundingBox.UnParse(ms);
            
            if (version >= 0x0102)
            {
                if (extraBoundingBoxes == null) extraBoundingBoxes = new BoundingBoxList(handler);
                extraBoundingBoxes.UnParse(ms);

                w.Write(unknown1);
                w.Write(unknown2);
            }

            mlodReferences.UnParse(ms);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new MODL(requestedApiVersion, handler, this); }

        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (version < 0x0102)
                {
                    res.Remove("ExtraBoundingBoxes");
                    res.Remove("Unknown1");
                    res.Remove("Unknown2");
                }
                return res;
            }
        }
        #endregion

        #region Sub-types
        public class MLODReference : AHandlerElement, IEquatable<MLODReference>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            GenericRCOLResource.ChunkReference mlodIndex;
            uint unknown1;
            LODType lod;
            uint vector1;
            uint vector2;
            #endregion

            #region Constructors
            public MLODReference(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public MLODReference(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public MLODReference(int APIversion, EventHandler handler, MLODReference basis)
                : this(APIversion, handler,
                basis.mlodIndex, basis.unknown1, basis.lod, basis.vector1, basis.vector2) { }
            public MLODReference(int APIversion, EventHandler handler,
                GenericRCOLResource.ChunkReference mlodIndex, uint unknown2, LODType lod, uint vector1, uint vector2)
                : base(APIversion, handler)
            {
                this.mlodIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, mlodIndex);
                this.unknown1 = unknown2;
                this.lod = lod;
                this.vector1 = vector1;
                this.vector2 = vector2;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.mlodIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                this.unknown1 = r.ReadUInt32();
                this.lod = (LODType)r.ReadUInt32();
                this.vector1 = r.ReadUInt32();
                this.vector2 = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                mlodIndex.UnParse(s);
                w.Write(unknown1);
                w.Write((uint)lod);
                w.Write(vector1);
                w.Write(vector2);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new MLODReference(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<MLODReference> Members

            public bool Equals(MLODReference other)
            {
                return mlodIndex == other.mlodIndex && unknown1 == other.unknown1 && lod == other.lod
                    && vector1 == other.vector1 && vector2 == other.vector2;
            }

            #endregion

            #region Sub-types
            public enum LODType : uint
            {
                LT00 = 0x00000000,
                LT01 = 0x00000001,
                LT10 = 0x00010000,
                LT11 = 0x00010001,
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public GenericRCOLResource.ChunkReference MLODIndex { get { return mlodIndex; } set { if (mlodIndex != value) { mlodIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public LODType LOD { get { return lod; } set { if (lod != value) { lod = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public uint Vector1 { get { return vector1; } set { if (vector1 != value) { vector1 = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public uint Vector2 { get { return vector2; } set { if (vector2 != value) { vector2 = value; OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    s += "MLODIndex: " + mlodIndex.Value;
                    s += "\nUnknown1: 0x" + unknown1.ToString("X8");
                    s += "\nLOD: " + this["LOD"];
                    s += "\nVector1: 0x" + vector1.ToString("X8");
                    s += "\nVector2: 0x" + vector2.ToString("X8");
                    return s;
                    /**/
                }
            }
            #endregion
        }

        public class MLODReferenceList : DependentList<MLODReference>
        {
            int outerCount;

            #region Constructors
            public MLODReferenceList(EventHandler handler) : base(handler) { }
            public MLODReferenceList(EventHandler handler, int count, Stream s) : base(handler) { outerCount = count; Parse(s); }
            public MLODReferenceList(EventHandler handler, IEnumerable<MLODReference> lpp) : base(handler, lpp) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return outerCount; }
            protected override void WriteCount(Stream s, int count) { }//written by containing class

            protected override MLODReference CreateElement(Stream s) { return new MLODReference(0, elementHandler, s); }
            protected override void WriteElement(Stream s, MLODReference element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new MLODReference(0, null)); }
        }

        public class BoundingBoxList : DependentList<BoundingBox>
        {
            #region Constructors
            public BoundingBoxList(EventHandler handler) : base(handler) { }
            public BoundingBoxList(EventHandler handler, Stream s) : base(handler, s) { }
            public BoundingBoxList(EventHandler handler, IEnumerable<BoundingBox> lpp) : base(handler, lpp) { }
            #endregion

            #region Data I/O
            protected override BoundingBox CreateElement(Stream s) { return new BoundingBox(0, elementHandler, s); }
            protected override void WriteElement(Stream s, BoundingBox element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new BoundingBox(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public BoundingBox ObjectBoundingBox { get { return objectBoundingBox; } set { if (objectBoundingBox != value) { objectBoundingBox = new BoundingBox(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public BoundingBoxList ExtraBoundingBoxes { get { return extraBoundingBoxes; } set { if (extraBoundingBoxes != value) { extraBoundingBoxes = new BoundingBoxList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(6)]
        public MLODReferenceList MlodReferences { get { return mlodReferences; } set { if (mlodReferences != value) { mlodReferences = new MLODReferenceList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string fmt;
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");
                s += "\nObjectBoundingBox: " + objectBoundingBox.Value;

                if (version >= 0x0102)
                {
                    s += String.Format("\nExtraBoundingBoxes ({0:X}):", extraBoundingBoxes.Count);
                    fmt = "\n  [{0:X" + extraBoundingBoxes.Count.ToString("X").Length + "}]: {1}";
                    for (int i = 0; i < extraBoundingBoxes.Count; i++) s += String.Format(fmt, i, extraBoundingBoxes[i]);
                    s += "\n----";

                    s += "\nUnknown1: 0x" + unknown1.ToString("X8");
                    s += "\nUnknown2: 0x" + unknown2.ToString("X8");
                }

                s += String.Format("\nMlodReferences ({0:X}):",mlodReferences.Count);
                fmt = "\n--[{0:X" + mlodReferences.Count.ToString("X").Length + "}]--\n{1}\n--";
                for (int i = 0; i < mlodReferences.Count; i++) s += String.Format(fmt, i, mlodReferences[i].Value);
                s += "\n----";

                return s;
                /**/
            }
        }
        #endregion
    }
}
