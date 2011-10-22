/***************************************************************************
 *  Copyright (C) 2011 by Peter L Jones                                    *
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

namespace RigResource
{
    public class RigResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        RigFormat rigFormat = 0;
        //RawGranny
        byte[] granny2Data = null;
        //WrappedGranny - not done
        //Clear
        uint major = 0x00000004;
        uint minor = 0x00000002;
        BoneList bones = null;
        string skeletonName = null;//major >= 4
        IKChainList ikChains = null;
        #endregion

        public RigResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { rigFormat = RigFormat.Clear; stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            uint dw1 = r.ReadUInt32();
            uint dw2 = r.ReadUInt32();
            s.Position = 0;

            if (dw1 == 0x8EAF13DE && dw2 == 0x00000000)
            {
                rigFormat = RigFormat.WrappedGranny;
                //ParseWrappedGranny(s);
                ParseRawGranny(s);
            }
            else if ((dw1 == 0x00000003 || dw1 == 0x00000004) && ((dw2 == 0x00000001 || dw2 == 0x00000002)))
            {
                rigFormat = RigFormat.Clear;
                ParseClear(s);
            }
            else
            {
                rigFormat = RigFormat.RawGranny;
                ParseRawGranny(s);
            }
        }

        private void ParseRawGranny(Stream s)
        {
            granny2Data = new byte[s.Length];
            s.Read(granny2Data, 0, granny2Data.Length);
        }

        private void ParseWrappedGranny(Stream s)
        {
            throw new NotImplementedException();
        }

        private void ParseClear(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            major = r.ReadUInt32();
            minor = r.ReadUInt32();
            bones = new BoneList(OnResourceChanged, s);
            if (major >= 4)
                skeletonName = new String(r.ReadChars(r.ReadInt32()));
            ikChains = new IKChainList(OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            switch (rigFormat)
            {
                case RigFormat.WrappedGranny:
                    return UnParseRawGranny();
                case RigFormat.RawGranny:
                    //return UnParseWrappedGranny();
                    return UnParseRawGranny();
                case RigFormat.Clear:
                    return UnParseClear();
            }
            throw new InvalidOperationException("Unknown RIG format: " + rigFormat);
        }

        private Stream UnParseRawGranny()
        {
            MemoryStream s = new MemoryStream();
            if (granny2Data == null) granny2Data = new byte[0];
            s.Write(granny2Data, 0, granny2Data.Length);
            s.Flush();
            return s;
        }

        private Stream UnParseWrappedGranny()
        {
            throw new NotImplementedException();
        }

        private Stream UnParseClear()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(major);
            w.Write(minor);

            if (bones == null) bones = new BoneList(OnResourceChanged);
            bones.UnParse(s);

            if (major >= 4)
            {
                if (skeletonName == null) skeletonName = "";
                w.Write(skeletonName.Length);
                w.Write(skeletonName.ToCharArray());
            }

            if (ikChains == null) ikChains = new IKChainList(OnResourceChanged);
            ikChains.UnParse(s);

            return s;
        }
        #endregion

        #region Sub-Types
        enum RigFormat
        {
            RawGranny,
            WrappedGranny,
            Clear,
        }

        public class Bone : AHandlerElement, IEquatable<Bone>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Vertex position;
            Quaternion orientation;
            Vertex scaling;
            string name;
            int opposingBoneIndex;
            int parentBoneIndex;
            uint hash;
            uint unknown2;
            #endregion

            #region Constructors
            public Bone(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Bone(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Bone(int APIversion, EventHandler handler, Bone basis)
                : this(APIversion, handler, basis.position, basis.orientation, basis.scaling,
                basis.name, basis.opposingBoneIndex, basis.parentBoneIndex, basis.hash, basis.unknown2) { }
            public Bone(int APIversion, EventHandler handler,
                Vertex position, Quaternion quaternion, Vertex scaling,
                string name, int opposingBoneIndex, int parentBoneIndex, uint hash, uint unknown2)
                : base(APIversion, handler)
            {
                this.position = new Vertex(requestedApiVersion, handler, position);
                this.orientation = new Quaternion(requestedApiVersion, handler, quaternion);
                this.scaling = new Vertex(requestedApiVersion, handler, scaling);
                this.name = name;
                this.opposingBoneIndex = opposingBoneIndex;
                this.parentBoneIndex = parentBoneIndex;
                this.hash = hash;
                this.unknown2 = unknown2;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                position = new Vertex(requestedApiVersion, handler, s);
                orientation = new Quaternion(requestedApiVersion, handler, s);
                scaling = new Vertex(requestedApiVersion, handler, s);
                name = new String(r.ReadChars(r.ReadInt32()));
                opposingBoneIndex = r.ReadInt32();
                parentBoneIndex = r.ReadInt32();
                hash = r.ReadUInt32();
                unknown2 = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                if (position == null) position = new Vertex(requestedApiVersion, handler);
                position.UnParse(s);
                if (orientation == null) orientation = new Quaternion(requestedApiVersion, handler);
                orientation.UnParse(s);
                if (scaling == null) scaling = new Vertex(requestedApiVersion, handler);
                scaling.UnParse(s);
                if (name == null) name = "";
                w.Write(name.Length);
                w.Write(name.ToCharArray());
                w.Write(opposingBoneIndex);
                w.Write(parentBoneIndex);
                w.Write(hash);
                w.Write(unknown2);
            }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new Bone(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Bone>
            public bool Equals(Bone other)
            {
                return position.Equals(other.position) && orientation.Equals(other.orientation) && scaling.Equals(other.scaling) && name.Equals(other.name)
                    && opposingBoneIndex.Equals(other.opposingBoneIndex) && parentBoneIndex.Equals(other.parentBoneIndex) && hash.Equals(other.hash) && unknown2.Equals(other.unknown2);
            }

            public override bool Equals(object obj)
            {
                return obj as Bone != null && this.Equals(obj as Bone);
            }

            public override int GetHashCode()
            {
                return position.GetHashCode() ^ orientation.GetHashCode() ^ scaling.GetHashCode() ^ name.GetHashCode()
                    ^ opposingBoneIndex.GetHashCode() ^ parentBoneIndex.GetHashCode() ^ hash.GetHashCode() ^ unknown2.GetHashCode();
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public Vertex Position { get { return position; } set { if (!position.Equals(value)) { position = new Vertex(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public Quaternion Orientation { get { return orientation; } set { if (!orientation.Equals(value)) { orientation = new Quaternion(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public Vertex Scaling { get { return scaling; } set { if (!scaling.Equals(value)) { scaling = new Vertex(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public String Name { get { return name; } set { if (name != value) { name = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public int OpposingBoneIndex { get { return opposingBoneIndex; } set { if (opposingBoneIndex != value) { opposingBoneIndex = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public int ParentBoneIndex { get { return parentBoneIndex; } set { if (parentBoneIndex != value) { parentBoneIndex = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public uint Hash { get { return hash; } set { if (hash != value) { hash = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class BoneList : DependentList<Bone>
        {
            public BoneList(EventHandler handler) : base(handler) { }
            public BoneList(EventHandler handler, Stream s) : base(handler, s) { }
            public BoneList(EventHandler handler, IEnumerable<Bone> lb) : base(handler, lb) { }

            protected override Bone CreateElement(Stream s) { return new Bone(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Bone element) { element.UnParse(s); }

            public override void Add() { this.Add(new Bone(0, null, new Vertex(0, null), new Quaternion(0, null), new Vertex(0, null), "", 0, 0, 0, 0)); }
        }

        public enum IKType : uint
        {
            IKTypeRoot = 0x00000001,
            IKTypeLimb = 0x00000003,
        }
        public abstract class IKElement : AHandlerElement, IEquatable<IKElement>
        {
            const int recommendedApiVersion = 1;

            public IKElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public static IKElement CreateEntry(int APIversion, EventHandler handler, Stream s)
            {
                IKType iktype = (IKType)new BinaryReader(s).ReadUInt32();
                switch (iktype)
                {
                    case IKType.IKTypeRoot:
                        return new IKRoot(APIversion, handler, s);
                    case IKType.IKTypeLimb:
                        return new IKLimb(APIversion, handler, s);
                    default:
                        throw new InvalidDataException(String.Format("Unsupported IKType 0x{0:X2} at 0x{1:X8}", iktype, s.Position));
                }
            }
            internal abstract void UnParse(Stream s);

            #region AHandlerElement Members
            //-- public override AHandlerElement Clone(EventHandler handler);
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<IKElement>
            public abstract bool Equals(IKElement other);
            public abstract override bool Equals(object other);
            public abstract override int GetHashCode();
            #endregion

            public string Value { get { return "-" + this.GetType().Name + "-\n" + this.ValueBuilder; } }
        }
        [ConstructorParameters(new object[] {
            (uint)0,
            (uint)0, (uint)0, (uint)0, (uint)0, (uint)0, (uint)0,
            (uint)0, (uint)0, (uint)0, (uint)0, (uint)0,
            (uint)0, (uint)0, })]
        public class IKRoot : IKElement, IEquatable<IKRoot>
        {
            #region Attributes
            uint rootBind;
            uint unknown1;
            uint unknown2;
            uint unknown3;
            uint unknown4;
            uint unknown5;
            uint unknown6;
            uint unknown7;
            uint unknown8;
            uint unknown9;
            uint unknown10;
            uint unknown11;
            uint slotOffset;
            uint root;
            #endregion

            #region Constructors
            public IKRoot(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public IKRoot(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public IKRoot(int APIversion, EventHandler handler, IKRoot basis)
                : this(APIversion, handler,
                    basis.rootBind,
                    basis.unknown1, basis.unknown2, basis.unknown3, basis.unknown4, basis.unknown5, basis.unknown6,
                    basis.unknown7, basis.unknown8, basis.unknown9, basis.unknown10, basis.unknown11,
                    basis.slotOffset, basis.root) { }
            public IKRoot(int APIversion, EventHandler handler,
                uint rootBind, uint unknown1, uint unknown2, uint unknown3, uint unknown4, uint unknown5, uint unknown6,
                uint unknown7, uint unknown8, uint unknown9, uint unknown10, uint unknown11, uint slotOffset, uint root)
                : base(APIversion, handler)
            {
                this.rootBind = rootBind;
                this.unknown1 = unknown1;
                this.unknown2 = unknown2;
                this.unknown3 = unknown3;
                this.unknown4 = unknown4;
                this.unknown5 = unknown5;
                this.unknown6 = unknown6;
                this.unknown7 = unknown7;
                this.unknown8 = unknown8;
                this.unknown9 = unknown9;
                this.unknown10 = unknown10;
                this.unknown11 = unknown11;
                this.slotOffset = slotOffset;
                this.root = root;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.rootBind = r.ReadUInt32();
                this.unknown1 = r.ReadUInt32();
                this.unknown2 = r.ReadUInt32();
                this.unknown3 = r.ReadUInt32();
                this.unknown4 = r.ReadUInt32();
                this.unknown5 = r.ReadUInt32();
                this.unknown6 = r.ReadUInt32();
                this.unknown7 = r.ReadUInt32();
                this.unknown8 = r.ReadUInt32();
                this.unknown9 = r.ReadUInt32();
                this.unknown10 = r.ReadUInt32();
                this.unknown11 = r.ReadUInt32();
                this.slotOffset = r.ReadUInt32();
                this.root = r.ReadUInt32();
            }

            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)IKType.IKTypeRoot);
                w.Write(rootBind);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
                w.Write(unknown4);
                w.Write(unknown5);
                w.Write(unknown6);
                w.Write(unknown7);
                w.Write(unknown8);
                w.Write(unknown9);
                w.Write(unknown10);
                w.Write(unknown11);
                w.Write(slotOffset);
                w.Write(root);
            }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new IKRoot(requestedApiVersion, handler, this); }

            #region IEquatable<IKRoot>
            public bool Equals(IKRoot other)
            {
                return rootBind == other.rootBind
                    && unknown1 == other.unknown1
                    && unknown2 == other.unknown2
                    && unknown3 == other.unknown3
                    && unknown4 == other.unknown4
                    && unknown5 == other.unknown5
                    && unknown6 == other.unknown6
                    && unknown7 == other.unknown7
                    && unknown8 == other.unknown8
                    && unknown9 == other.unknown9
                    && unknown10 == other.unknown10
                    && unknown11 == other.unknown11
                    && slotOffset == other.slotOffset
                    && root == other.root;
            }

            public override bool Equals(IKElement other)
            {
                return other as IKRoot != null && this.Equals(other as IKRoot);
            }

            public override bool Equals(object other)
            {
                return other as IKRoot != null && this.Equals(other as IKRoot);
            }

            public override int GetHashCode()
            {
                return (int)(rootBind ^ unknown1 ^ unknown2 ^ unknown3 ^ unknown4 ^ unknown5 ^ unknown6
                    ^ unknown7 ^ unknown8 ^ unknown9 ^ unknown10 ^ unknown11 ^ slotOffset ^ root);
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint RootBind { get { return rootBind; } set { if (rootBind != value) { rootBind = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public uint Unknown4 { get { return unknown4; } set { if (unknown4 != value) { unknown4 = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public uint Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public uint Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public uint Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public uint Unknown9 { get { return unknown9; } set { if (unknown9 != value) { unknown9 = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public uint Unknown10 { get { return unknown10; } set { if (unknown10 != value) { unknown10 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public uint Unknown11 { get { return unknown11; } set { if (unknown11 != value) { unknown11 = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public uint SlotOffset { get { return slotOffset; } set { if (slotOffset != value) { slotOffset = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public uint Root { get { return root; } set { if (root != value) { root = value; OnElementChanged(); } } }
            #endregion
        }
        [ConstructorParameters(new object[] {
            (uint)0, (uint)0, (uint)0,
            (uint)0, (uint)0, (uint)0, (uint)0, (uint)0, (uint)0,
            (uint)0, (uint)0, (uint)0, (uint)0, (uint)0,
            (uint)0, (uint)0, (uint)0, (uint)0, })]
        public class IKLimb : IKElement, IEquatable<IKLimb>
        {
            #region Attributes
            uint start;
            uint mid;
            uint end;
            uint unknown1;
            uint unknown2;
            uint unknown3;
            uint unknown4;
            uint unknown5;
            uint unknown6;
            uint unknown7;
            uint unknown8;
            uint unknown9;
            uint unknown10;
            uint unknown11;
            uint poleVector;
            uint slotInfo;
            uint slotOffset;
            uint root;
            #endregion

            #region Constructors
            public IKLimb(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public IKLimb(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public IKLimb(int APIversion, EventHandler handler, IKLimb basis)
                : this(APIversion, handler,
                    basis.start, basis.mid, basis.end,
                    basis.unknown1, basis.unknown2, basis.unknown3, basis.unknown4, basis.unknown5, basis.unknown6,
                    basis.unknown7, basis.unknown8, basis.unknown9, basis.unknown10, basis.unknown11, basis.poleVector, basis.slotInfo, basis.slotOffset, basis.root) { }
            public IKLimb(int APIversion, EventHandler handler,
                uint start, uint mid, uint end, uint unknown1, uint unknown2, uint unknown3, uint unknown4, uint unknown5, uint unknown6,
                uint unknown7, uint unknown8, uint unknown9, uint unknown10, uint unknown11, uint poleVector, uint slotInfo, uint slotOffset, uint root)
                : base(APIversion, handler)
            {
                this.start = start;
                this.mid = mid;
                this.end = end;
                this.unknown1 = unknown1;
                this.unknown2 = unknown2;
                this.unknown3 = unknown3;
                this.unknown4 = unknown4;
                this.unknown5 = unknown5;
                this.unknown6 = unknown6;
                this.unknown7 = unknown7;
                this.unknown8 = unknown8;
                this.unknown9 = unknown9;
                this.unknown10 = unknown10;
                this.unknown11 = unknown11;
                this.poleVector = poleVector;
                this.slotInfo = slotInfo;
                this.slotOffset = slotOffset;
                this.root = root;
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.start = r.ReadUInt32();
                this.mid = r.ReadUInt32();
                this.end = r.ReadUInt32();
                this.unknown1 = r.ReadUInt32();
                this.unknown2 = r.ReadUInt32();
                this.unknown3 = r.ReadUInt32();
                this.unknown4 = r.ReadUInt32();
                this.unknown5 = r.ReadUInt32();
                this.unknown6 = r.ReadUInt32();
                this.unknown7 = r.ReadUInt32();
                this.unknown8 = r.ReadUInt32();
                this.unknown9 = r.ReadUInt32();
                this.unknown10 = r.ReadUInt32();
                this.unknown11 = r.ReadUInt32();
                this.poleVector = r.ReadUInt32();
                this.slotInfo = r.ReadUInt32();
                this.slotOffset = r.ReadUInt32();
                this.root = r.ReadUInt32();
            }

            internal override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)IKType.IKTypeLimb);
                w.Write(start);
                w.Write(mid);
                w.Write(end);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
                w.Write(unknown4);
                w.Write(unknown5);
                w.Write(unknown6);
                w.Write(unknown7);
                w.Write(unknown8);
                w.Write(unknown9);
                w.Write(unknown10);
                w.Write(unknown11);
                w.Write(poleVector);
                w.Write(slotInfo);
                w.Write(slotOffset);
                w.Write(root);
            }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new IKLimb(requestedApiVersion, handler, this); }

            #region IEquatable<IKLimb>
            public bool Equals(IKLimb other)
            {
                return start == other.start
                    && mid == other.mid
                    && end == other.end
                    && unknown1 == other.unknown1
                    && unknown2 == other.unknown2
                    && unknown3 == other.unknown3
                    && unknown4 == other.unknown4
                    && unknown5 == other.unknown5
                    && unknown6 == other.unknown6
                    && unknown7 == other.unknown7
                    && unknown8 == other.unknown8
                    && unknown9 == other.unknown9
                    && unknown10 == other.unknown10
                    && unknown11 == other.unknown11
                    && poleVector == other.poleVector
                    && slotInfo == other.slotInfo
                    && slotOffset == other.slotOffset
                    && root == other.root;
            }

            public override bool Equals(IKElement other)
            {
                return other as IKLimb != null && this.Equals(other as IKLimb);
            }

            public override bool Equals(object other)
            {
                return other as IKLimb != null && this.Equals(other as IKLimb);
            }

            public override int GetHashCode()
            {
                return (int)(start ^ mid ^ end ^ unknown1 ^ unknown2 ^ unknown3 ^ unknown4 ^ unknown5 ^ unknown6
                    ^ unknown7 ^ unknown8 ^ unknown9 ^ unknown10 ^ unknown11 ^ poleVector ^ slotInfo ^ slotOffset ^ root);
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint Start { get { return start; } set { if (start != value) { start = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint Mid { get { return mid; } set { if (mid != value) { mid = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public uint End { get { return end; } set { if (end != value) { end = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public uint Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public uint Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public uint Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public uint Unknown4 { get { return unknown4; } set { if (unknown4 != value) { unknown4 = value; OnElementChanged(); } } }
            [ElementPriority(8)]
            public uint Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public uint Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public uint Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public uint Unknown8 { get { return unknown8; } set { if (unknown8 != value) { unknown8 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public uint Unknown9 { get { return unknown9; } set { if (unknown9 != value) { unknown9 = value; OnElementChanged(); } } }
            [ElementPriority(13)]
            public uint Unknown10 { get { return unknown10; } set { if (unknown10 != value) { unknown10 = value; OnElementChanged(); } } }
            [ElementPriority(14)]
            public uint Unknown11 { get { return unknown11; } set { if (unknown11 != value) { unknown11 = value; OnElementChanged(); } } }
            [ElementPriority(15)]
            public uint PoleVector { get { return poleVector; } set { if (poleVector != value) { poleVector = value; OnElementChanged(); } } }
            [ElementPriority(16)]
            public uint SlotInfo { get { return slotInfo; } set { if (slotInfo != value) { slotInfo = value; OnElementChanged(); } } }
            [ElementPriority(17)]
            public uint SlotOffset { get { return slotOffset; } set { if (slotOffset != value) { slotOffset = value; OnElementChanged(); } } }
            [ElementPriority(18)]
            public uint Root { get { return root; } set { if (root != value) { root = value; OnElementChanged(); } } }
            #endregion
        }

        public class IKChainList : DependentList<IKElement>
        {
            #region Constructors
            public IKChainList(EventHandler handler) : base(handler) { }
            public IKChainList(EventHandler handler, Stream s) : base(handler, s) { }
            public IKChainList(EventHandler handler, IEnumerable<IKElement> le) : base(handler, le) { }
            #endregion

            #region Data I/O
            protected override IKElement CreateElement(Stream s) { return IKElement.CreateEntry(0, handler, s); }

            protected override void WriteElement(Stream s, IKElement element) { element.UnParse(s); }
            #endregion

            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(IKElement).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();

                switch (fields.Length)
                {
                    case 14: return typeof(IKRoot);
                    case 18: return typeof(IKLimb);
                }
                throw new ArgumentException(String.Format("Unexpected argument count {0}", fields.Length));
            }

            public override void Add() { throw new NotImplementedException(); }
        }
        #endregion

        public override List<string> ContentFields
        {
            get
            {
                switch (rigFormat)
                {
                    case RigFormat.RawGranny:
                        return new List<string>(new string[] { "RawGranny", });
                    case RigFormat.WrappedGranny:
                        return new List<string>(new string[] { "RawGranny", });
                    case RigFormat.Clear:
                        List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                        res.Remove("RawGranny");
                        if (major < 4)
                        {
                            res.Remove("SkeletonName");
                        }
                        return res;
                }
                throw new InvalidOperationException("Unknown RIG format: " + rigFormat);
            }
        }

        #region Content Fields
        [ElementPriority(1)]
        public BinaryReader RawGranny
        {
            get { return new BinaryReader(UnParse()); }
            set
            {
                if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; Parse(value.BaseStream); }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024 * 1024];
                    for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                        ms.Write(buffer, 0, read);
                    Parse(ms);
                }
                OnResourceChanged(this, EventArgs.Empty);
            }
        }

        [ElementPriority(1)]
        public uint Major { get { return major; } set { if (major != value) { major = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public uint Minor { get { return minor; } set { if (minor != value) { minor = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public BoneList Bones { get { return bones; } set { if (!bones.Equals(value)) { bones = new BoneList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public String SkeletonName { get { return skeletonName; } set { if (skeletonName != value) { skeletonName = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public IKChainList IKChains { get { return ikChains; } set { if (!ikChains.Equals(value)) { ikChains = new IKChainList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion

    }

    /// <summary>
    /// ResourceHandler for RigResource wrapper
    /// </summary>
    public class RigResourceResourceHandler : AResourceHandler
    {
        public RigResourceResourceHandler()
        {
            this.Add(typeof(RigResource), new List<string>(new string[] { "0x8EAF13DE" }));
        }
    }
}
