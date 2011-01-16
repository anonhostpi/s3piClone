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
    public class MLOD : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;
        const string TAG = "MLOD";

        #region Attributes
        uint tag = (uint)FOURCC(TAG);
        uint version = 0x00000202;
        LODGroupList lodGroups;
        #endregion

        #region Constructors
        public MLOD(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public MLOD(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public MLOD(int APIversion, EventHandler handler, MLOD basis)
            : base(APIversion, handler, null)
        {
            this.version = basis.version;
            this.lodGroups = new LODGroupList(handler, basis.version, basis.lodGroups);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x01D10F34; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC(TAG))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            version = r.ReadUInt32();
            lodGroups = new LODGroupList(handler, version, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            if (lodGroups == null) lodGroups = new LODGroupList(handler, 0x00000202);
            lodGroups.UnParse(ms);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new MLOD(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class LODGroup : AHandlerElement, IEquatable<LODGroup>
        {
            const int recommendedApiVersion = 1;

            uint mlodVersion;

            #region Attributes
            uint groupNameHash;
            GenericRCOLResource.ChunkReference matdMtstIndex;
            GenericRCOLResource.ChunkReference vrtfIndex;
            GenericRCOLResource.ChunkReference vbufIndex;
            GenericRCOLResource.ChunkReference ibufIndex;
            uint vbufType;//???????
            ulong vbufOffset;//???????
            ulong ibufOffset;//???????
            uint vbufCount;//???????
            uint ibufCount;//???????
            BoundingBox boundingBox;
            GenericRCOLResource.ChunkReference skinIndex;
            UIntList boneNameHashes;
            GenericRCOLResource.ChunkReference matdIndex;
            GeoStateList geoStates;
            float unknown1;
            float unknown2;
            float unknown3;
            float unknown4;
            float unknown5;
            #endregion

            #region Constructors
            public LODGroup(int APIversion, EventHandler handler, uint mlodVersion) : base(APIversion, handler) { this.mlodVersion = mlodVersion; }
            public LODGroup(int APIversion, EventHandler handler, uint mlodVersion, Stream s) : this(APIversion, handler, mlodVersion) { Parse(s); }
            public LODGroup(int APIversion, EventHandler handler, LODGroup basis)
                : this(APIversion, handler,
                basis.mlodVersion,
                basis.groupNameHash, basis.matdMtstIndex, basis.vrtfIndex, basis.vbufIndex, basis.ibufIndex,
                basis.vbufType,//???????
                basis.vbufOffset,//???????
                basis.ibufOffset,//???????
                basis.vbufCount,//???????
                basis.ibufCount,//???????
                basis.boundingBox, basis.skinIndex, basis.boneNameHashes, basis.matdIndex, basis.geoStates,
                basis.unknown1, basis.unknown2, basis.unknown3, basis.unknown4, basis.unknown5
                ) { }
            public LODGroup(int APIversion, EventHandler handler,
                uint mlodVersion,
                uint groupNameHash,
                GenericRCOLResource.ChunkReference matdMtstIndex,
                GenericRCOLResource.ChunkReference vrtfIndex,
                GenericRCOLResource.ChunkReference vbufIndex,
                GenericRCOLResource.ChunkReference ibufIndex,
                uint vbufType,//???????
                ulong vbufOffset,//???????
                ulong ibufOffset,//???????
                uint vbufCount,//???????
                uint ibufCount,//???????
                BoundingBox boundingBox,
                GenericRCOLResource.ChunkReference skinIndex,
                UIntList boneNameHashes,
                GenericRCOLResource.ChunkReference matdIndex,
                GeoStateList geoStates
                ) : this(APIversion, handler,
                mlodVersion,
                    groupNameHash, matdMtstIndex, vrtfIndex, vbufIndex, ibufIndex,
                    vbufType,//???????
                    vbufOffset,//???????
                    ibufOffset,//???????
                    vbufCount,//???????
                    ibufCount,//???????
                    boundingBox, skinIndex, boneNameHashes, matdIndex, geoStates,
                    0, 0, 0, 0, 0
                    )
            {
                if (checking) if (mlodVersion >= 0x00000202)
                        throw new InvalidOperationException(String.Format("Constructor requires unknown1-5 for version {0}", 0));
            }
            public LODGroup(int APIversion, EventHandler handler,
                uint mlodVersion,
                uint groupNameHash,
                GenericRCOLResource.ChunkReference matdMtstIndex,
                GenericRCOLResource.ChunkReference vrtfIndex,
                GenericRCOLResource.ChunkReference vbufIndex,
                GenericRCOLResource.ChunkReference ibufIndex,
                uint vbufType,//???????
                ulong vbufOffset,//???????
                ulong ibufOffset,//???????
                uint vbufCount,//???????
                uint ibufCount,//???????
                BoundingBox boundingBox,
                GenericRCOLResource.ChunkReference skinIndex,
                UIntList boneNameHashes,
                GenericRCOLResource.ChunkReference matdIndex,
                GeoStateList geoStates,
                float unknown1, float unknown2, float unknown3, float unknown4, float unknown5
                ) : base(APIversion, handler)
            {
                this.mlodVersion = mlodVersion;

                this.groupNameHash = groupNameHash;
                this.matdMtstIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, matdMtstIndex);
                this.vrtfIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, vrtfIndex);
                this.vbufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, vbufIndex);
                this.ibufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, ibufIndex);
                this.vbufType = vbufType;//???????
                this.vbufOffset = vbufOffset;//???????
                this.ibufOffset = ibufOffset;//???????
                this.vbufCount = vbufCount;//???????
                this.ibufCount = ibufCount;//???????
                this.boundingBox = new BoundingBox(requestedApiVersion, handler, boundingBox);
                this.skinIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, skinIndex);
                this.boneNameHashes = new UIntList(handler, boneNameHashes);
                this.matdIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, matdIndex);
                this.geoStates = new GeoStateList(handler, geoStates);

                if (mlodVersion >= 0x00000202)
                {
                    this.unknown1 = unknown1;
                    this.unknown2 = unknown2;
                    this.unknown3 = unknown3;
                    this.unknown4 = unknown4;
                    this.unknown5 = unknown5;
                }
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                long endPos = r.ReadInt32() + s.Position;

                groupNameHash = r.ReadUInt32();
                matdMtstIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                vrtfIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                vbufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                ibufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                vbufType = r.ReadUInt32();//???????
                vbufOffset = r.ReadUInt64();//???????
                ibufOffset = r.ReadUInt64();//???????
                vbufCount = r.ReadUInt32();//???????
                ibufCount = r.ReadUInt32();//???????
                boundingBox = new BoundingBox(requestedApiVersion, handler, s);
                skinIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                boneNameHashes = new UIntList(handler, s);
                matdIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, s);
                geoStates = new GeoStateList(handler, s);
                if (mlodVersion >= 0x0202)
                {
                    unknown1 = r.ReadSingle();
                    unknown2 = r.ReadSingle();
                    unknown3 = r.ReadSingle();
                    unknown4 = r.ReadSingle();
                    unknown5 = r.ReadSingle();
                }

                if (checking) if (endPos != s.Position)
                        throw new InvalidDataException(String.Format("Expected end of LODGroup: 0x{0:X8}, actual: 0x{1:X8}", endPos, s.Position));
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                long lenPos = s.Position;
                w.Write((int)0); // number of bytes after here in this LODGroup

                w.Write(groupNameHash);
                matdMtstIndex.UnParse(s);
                vrtfIndex.UnParse(s);
                vbufIndex.UnParse(s);
                ibufIndex.UnParse(s);
                w.Write(vbufType);//???????
                w.Write(vbufOffset);//???????
                w.Write(ibufOffset);//???????
                w.Write(vbufCount);//???????
                w.Write(ibufCount);//???????
                boundingBox.UnParse(s);
                skinIndex.UnParse(s);
                boneNameHashes.UnParse(s);
                matdIndex.UnParse(s);
                geoStates.UnParse(s);
                if (mlodVersion >= 0x0202)
                {
                    w.Write(unknown1);
                    w.Write(unknown2);
                    w.Write(unknown3);
                    w.Write(unknown4);
                    w.Write(unknown5);
                }

                int size = (int)(s.Position - lenPos - sizeof(int));
                s.Position = lenPos;
                w.Write(size);
                s.Position += lenPos;
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    if (0 < 0x0202)
                    {
                        res.Remove("Unknown1");
                        res.Remove("Unknown2");
                        res.Remove("Unknown3");
                        res.Remove("Unknown4");
                        res.Remove("Unknown5");
                    }
                    return res;
                }
            }

            public override AHandlerElement Clone(EventHandler handler) { return new LODGroup(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<MLODReference> Members

            public bool Equals(LODGroup other)
            {
                bool res =
                    groupNameHash.Equals(other.groupNameHash)
                    && matdMtstIndex.Equals(other.matdMtstIndex)
                    && vbufIndex.Equals(other.vbufIndex)
                    && ibufIndex.Equals(other.ibufIndex)
                    && vbufType.Equals(other.vbufType)
                    && vbufOffset.Equals(other.vbufOffset)
                    && ibufOffset.Equals(other.ibufOffset)
                    && vbufCount.Equals(other.vbufCount)
                    && ibufCount.Equals(other.ibufCount)
                    && boundingBox.Equals(other.boundingBox)
                    && skinIndex.Equals(other.skinIndex)
                    && boneNameHashes.Equals(other.boneNameHashes)
                    && geoStates.Equals(other.geoStates)
                    ;
                if (res && mlodVersion >= 0x0202) res = res
                    && unknown1.Equals(other.unknown1)
                    && unknown2.Equals(other.unknown2)
                    && unknown3.Equals(other.unknown3)
                    && unknown4.Equals(other.unknown4)
                    && unknown5.Equals(other.unknown5)
                    ;
                return res;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint GroupNameHash { get { return groupNameHash; } set { if (groupNameHash != value) { groupNameHash = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public GenericRCOLResource.ChunkReference MatdMtstIndex { get { return matdMtstIndex; } set { if (matdMtstIndex != value) { matdMtstIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public GenericRCOLResource.ChunkReference VrtfIndex { get { return vrtfIndex; } set { if (vrtfIndex != value) { vrtfIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public GenericRCOLResource.ChunkReference VbufIndex { get { return vbufIndex; } set { if (vbufIndex != value) { vbufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public GenericRCOLResource.ChunkReference IbufIndex { get { return ibufIndex; } set { if (ibufIndex != value) { ibufIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(6)]
            public uint VbufType { get { return vbufType; } set { if (vbufType != value) { vbufType = value; OnElementChanged(); } } }//???????
            [ElementPriority(7)]
            public ulong VbufOffset { get { return vbufOffset; } set { if (vbufOffset != value) { vbufOffset = value; OnElementChanged(); } } }//???????
            [ElementPriority(8)]
            public ulong IbufOffset { get { return ibufOffset; } set { if (ibufOffset != value) { ibufOffset = value; OnElementChanged(); } } }//???????
            [ElementPriority(9)]
            public uint VbufCount { get { return vbufCount; } set { if (vbufCount != value) { vbufCount = value; OnElementChanged(); } } }//???????
            [ElementPriority(10)]
            public uint IbufCount { get { return ibufCount; } set { if (ibufCount != value) { ibufCount = value; OnElementChanged(); } } }//???????
            [ElementPriority(11)]
            public BoundingBox BoundingBox { get { return boundingBox; } set { if (boundingBox != value) { boundingBox = new BoundingBox(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(12)]
            public GenericRCOLResource.ChunkReference SkinIndex { get { return skinIndex; } set { if (skinIndex != value) { skinIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(13)]
            public UIntList BoneNameHashes { get { return boneNameHashes; } set { if (boneNameHashes != value) { boneNameHashes = new UIntList(handler, value); OnElementChanged(); } } }
            [ElementPriority(14)]
            public GenericRCOLResource.ChunkReference MatdIndex { get { return matdIndex; } set { if (matdIndex != value) { matdIndex = new GenericRCOLResource.ChunkReference(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(15)]
            public GeoStateList GeoStates { get { return geoStates; } set { if (geoStates != value) { geoStates = new GeoStateList(handler, value); OnElementChanged(); } } }
            [ElementPriority(16)]
            public float Unknown1 { get { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); return unknown1; } set { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(17)]
            public float Unknown2 { get { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); return unknown2; } set { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(18)]
            public float Unknown3 { get { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); return unknown3; } set { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(19)]
            public float Unknown4 { get { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); return unknown4; } set { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); if (unknown4 != value) { unknown4 = value; OnElementChanged(); } } }
            [ElementPriority(20)]
            public float Unknown5 { get { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); return unknown5; } set { if (mlodVersion < 0x00000202) throw new InvalidOperationException(); if (unknown5 != value) { unknown5 = value; OnElementChanged(); } } }

            public virtual String Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    foreach (string f in this.ContentFields)
                    {
                        if (f.Equals("Value")) continue;

                        TypedValue tv = this[f];

                        if (typeof(UIntList).IsAssignableFrom(tv.Type))
                        {
                            s += String.Format("\n{0} ({1:X}):", f, boneNameHashes.Count);
                            string fmt = "\n  [{0:X" + boneNameHashes.Count.ToString("X").Length + "}]: {1:X8}";
                            for (int i = 0; i < boneNameHashes.Count; i++) s += string.Format(fmt, i, boneNameHashes[i]);
                            s += "\n----";
                        }
                        else if (typeof(GeoStateList).IsAssignableFrom(tv.Type))
                        {
                            s += String.Format("\n{0} ({1:X}):", f, geoStates.Count);
                            string fmt = "\n--[{0:X" + geoStates.Count.ToString("X").Length + "}]--\n{1}\n--";
                            for (int i = 0; i < geoStates.Count; i++) s += string.Format(fmt, i, geoStates[i].Value);
                            s += "\n----";
                        }
                        else if (typeof(BoundingBox).IsAssignableFrom(tv.Type)) s += "\n" + f + ": " + boundingBox.Value;
                        else if (typeof(GenericRCOLResource.ChunkReference).IsAssignableFrom(tv.Type)) s += "\n" + f + ": " + ((GenericRCOLResource.ChunkReference)tv.Value).Value;
                        else if (typeof(AApiVersionedFields).IsAssignableFrom(tv.Type) && ((AApiVersionedFields)tv.Value).ContentFields.Contains("Value"))
                        {
                            s += "\n" + f + ":" + "\n" + ((AApiVersionedFields)tv.Value)["Value"] + "\n----";
                        }
                        else s += "\n" + f + ": " + tv;
                    }
                    return s.TrimStart('\n');
                    /**/
                }
            }
            #endregion
        }

        public class LODGroupList : DependentList<LODGroup>
        {
            uint mlodVersion;
            #region Constructors
            public LODGroupList(EventHandler handler, uint mlodVersion) : base(handler) { }
            public LODGroupList(EventHandler handler, uint mlodVersion, Stream s) : base(null) { this.mlodVersion = mlodVersion; elementHandler = handler; Parse(s); this.handler = handler; }
            public LODGroupList(EventHandler handler, uint mlodVersion, IEnumerable<LODGroup> lpp) : base(handler, lpp) { }
            #endregion

            #region Data I/O
            protected override LODGroup CreateElement(Stream s) { return new LODGroup(0, elementHandler, mlodVersion, s); }
            protected override void WriteElement(Stream s, LODGroup element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new LODGroup(0, null, mlodVersion)); }
        }

        public class GeoState : AHandlerElement, IEquatable<GeoState>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            uint stateNameHash;
            int firstIbufNum;
            int firstVbufNum;
            int ibufCount;
            int vbufCount;
            #endregion

            #region Constructors
            public GeoState(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public GeoState(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public GeoState(int APIversion, EventHandler handler, GeoState basis)
                : this(APIversion, handler,
                basis.stateNameHash,
                basis.firstIbufNum,
                basis.firstVbufNum,
                basis.ibufCount,
                basis.vbufCount
                ) { }
            public GeoState(int APIversion, EventHandler handler,
                uint stateNameHash,
                int firstIbufNum,
                int firstVbufNum,
                int ibufCount,
                int vbufCount
                )
                : base(APIversion, handler)
            {
                this.stateNameHash = stateNameHash;
                this.firstIbufNum = firstIbufNum;
                this.firstVbufNum = firstVbufNum;
                this.ibufCount = ibufCount;
                this.vbufCount = vbufCount;
            }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.stateNameHash = r.ReadUInt32();
                this.firstIbufNum = r.ReadInt32();
                this.firstVbufNum = r.ReadInt32();
                this.ibufCount = r.ReadInt32();
                this.vbufCount = r.ReadInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(stateNameHash);
                w.Write(firstIbufNum);
                w.Write(firstVbufNum);
                w.Write(ibufCount);
                w.Write(vbufCount);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new GeoState(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<BoxPoint> Members

            public bool Equals(GeoState other)
            {
                return stateNameHash.Equals(other.stateNameHash)
                    && firstIbufNum.Equals(other.firstIbufNum)
                    && firstVbufNum.Equals(other.firstVbufNum)
                    && ibufCount.Equals(other.ibufCount)
                    && vbufCount.Equals(other.vbufCount)
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint StateNameHash { get { return stateNameHash; } set { if (stateNameHash != value) { stateNameHash = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public int FirstIbufNum { get { return firstIbufNum; } set { if (firstIbufNum != value) { firstIbufNum = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public int FirstVbufNum { get { return firstVbufNum; } set { if (firstVbufNum != value) { firstVbufNum = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public int IbufCount { get { return ibufCount; } set { if (ibufCount != value) { ibufCount = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public int VbufCount { get { return vbufCount; } set { if (vbufCount != value) { vbufCount = value; OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    string s = "";
                    s += "StateNameHash: 0x" + stateNameHash.ToString("X8");
                    s += "\nFirstIbufNum: 0x" + firstIbufNum.ToString("X8");
                    s += "\nFirstVbufNum: 0x" + firstVbufNum.ToString("X8");
                    s += "\nIbufCount: 0x" + ibufCount.ToString("X8");
                    s += "\nVbufCount: 0x" + vbufCount.ToString("X8");
                    return s;
                    /**/
                }
            }
            #endregion
        }

        public class GeoStateList : DependentList<GeoState>
        {
            #region Constructors
            public GeoStateList(EventHandler handler) : base(handler) { }
            public GeoStateList(EventHandler handler, Stream s) : base(handler, s) { }
            public GeoStateList(EventHandler handler, IEnumerable<GeoState> lpp) : base(handler, lpp) { }
            #endregion

            #region Data I/O
            protected override GeoState CreateElement(Stream s) { return new GeoState(0, elementHandler, s); }
            protected override void WriteElement(Stream s, GeoState element) { element.UnParse(s); }
            #endregion

            public override void Add() { this.Add(new GeoState(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public LODGroupList LODGroups { get { return lodGroups; } set { if (lodGroups != value) { lodGroups = new LODGroupList(handler, version, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");
                s += String.Format("\nLODGroups ({0:X}):", lodGroups.Count);
                string fmt = "\n--[{0:X" + lodGroups.Count.ToString("X").Length + "}]--\n{1}\n--";
                for (int i = 0; i < lodGroups.Count; i++) s += String.Format(fmt, i, lodGroups[i].Value);
                s += "\n----";

                return s;
                /**/
            }
        }
        #endregion
    }
}
