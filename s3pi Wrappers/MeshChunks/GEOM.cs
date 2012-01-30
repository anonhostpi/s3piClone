/***************************************************************************
 *  Based on earlier work.                                                 *
 *  Copyright (C) 2011 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This is free software: you can redistribute it and/or modify           *
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
 *  along with this software.  If not, see <http://www.gnu.org/licenses/>. *
 ***************************************************************************/
using System.IO;
using s3pi.Interfaces;
using s3pi.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using s3pi.GenericRCOLResource;
using System.Linq;
using System.Collections;

namespace meshExpImp.ModelBlocks
{
    public class GEOM : ARCOLBlock
    {
#if OLDVERSION
        private const int kRecommendedApiVersion = 1;
        public override uint ResourceType { get { return 0x015A1849; } }
        public override string Tag { get { return "GEOM"; } }

        private UInt32 mVersion;
        private MATD.ShaderType mShader;
        private MATD.MTNF mMaterialBlock;
        private UInt32 mMergeGroup;
        private UInt32 mSortOrder;
        private VertexDataFormat mVertexFormat;
        private VertexList mVertices;
        private IndexList mIndices;
        private UInt32 mSkinControllerIndex;
        private UIntList mJoints;
        private TGIBlockList mReferences;

        public GEOM(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public GEOM(int APIversion, EventHandler handler, GEOM basis) : this(APIversion, handler, new IndexList(handler, basis.mIndices), new UIntList(handler, basis.mJoints), basis.Shader, basis.MaterialBlock, basis.mMergeGroup, new TGIBlockList(handler, basis.mReferences), basis.mSkinControllerIndex, basis.mSortOrder, basis.mVersion, new VertexDataFormat(0, handler, basis.mVertexFormat), new VertexList(handler, basis, basis.mVertices)) { }
        public GEOM(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }

        public GEOM(int APIversion, EventHandler handler, IndexList indices, UIntList joints, MATD.ShaderType shader, MATD.MTNF material, uint mergeGroup, TGIBlockList references, uint skinControllerIndex, uint sortOrder, uint version, VertexDataFormat vertexFormat, VertexList vertices)
            : base(APIversion, handler, null)
        {
            mIndices = indices;
            mJoints = joints;
            mShader = shader;
            mMaterialBlock = material;
            mMergeGroup = mergeGroup;
            mReferences = references;
            mSkinControllerIndex = skinControllerIndex;
            mSortOrder = sortOrder;
            mVersion = version;
            mVertexFormat = vertexFormat;
            mVertices = vertices;
        }

        protected override void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            string tag = FOURCC(br.ReadUInt32());
            if (Settings.Checking && tag != Tag)
                throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", tag, Tag, s.Position));

            mVersion = br.ReadUInt32();
            if (Settings.Checking && mVersion != 5)
                throw new InvalidDataException(String.Format("Invalid Version read: '{0}'; expected: '{1}'; at 0x{2:X8}", mVersion, 5, s.Position));

            long tgiOffset = br.ReadUInt32() + s.Position;
            long tgiSize = br.ReadUInt32();

            mShader = (MATD.ShaderType)br.ReadUInt32();
            mMaterialBlock = mShader == 0 ? new MATD.MTNF(0, handler) : new MATD.MTNF(0, handler, s);
            mMergeGroup = br.ReadUInt32();
            mSortOrder = br.ReadUInt32();

            int vertexCount = br.ReadInt32();

            mVertexFormat = new VertexDataFormat(0, handler, s);

            var verts = new List<Vertex>();
            for (int i = 0; i < vertexCount; i++)
                verts.Add(new Vertex(0, handler, s, this));
            mVertices = new VertexList(handler, this, verts);
            if (br.ReadUInt32() != 0x01 && Settings.Checking)
                throw new InvalidDataException("Expected 0x01 at 0x" + (s.Position - 1).ToString("X8"));
            mIndices = new IndexList(handler, s);
            mSkinControllerIndex = br.ReadUInt32();
            mJoints = new UIntList(handler, s);
            mReferences = new TGIBlockList(handler, s, tgiOffset, tgiSize);
        }

        public override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((uint)FOURCC(Tag));
            bw.Write(mVersion);
            long tgiOffsetPtr = s.Position;
            s.Seek(4, SeekOrigin.Current);
            long startOffset = s.Position;
            s.Seek(4, SeekOrigin.Current);
            if (mMaterialBlock == null) mMaterialBlock = new MATD.MTNF(0, handler);
            bw.Write((UInt32)mShader);
            if (mShader != 0x00000000)
            {
                var mtnfBytes = mMaterialBlock.AsBytes;
                bw.Write(mtnfBytes.Length);
                bw.Write(mtnfBytes);
            }
            bw.Write(mMergeGroup);
            bw.Write(mSortOrder);
            if (mVertices == null) mVertices = new VertexList(handler, this);
            bw.Write(mVertices.Count);
            if (mVertexFormat == null) mVertexFormat = new VertexDataFormat(0, handler);
            mVertexFormat.UnParse(s);
            for (int i = 0; i < mVertices.Count; i++)
                mVertices[i].UnParse(s);
            bw.Write(1U);
            if (mIndices == null) mIndices = new IndexList(handler);
            mIndices.UnParse(s);
            bw.Write(mSkinControllerIndex);
            if (mJoints == null) mJoints = new UIntList(handler);
            mJoints.UnParse(s);
            if (mReferences == null) mReferences = new TGIBlockList(handler);
            long tgiOffset = s.Position;
            mReferences.UnParse(s);
            long endOffset = s.Position;
            long tgiSize = endOffset - tgiOffset;
            s.Seek(tgiOffsetPtr, SeekOrigin.Begin);
            bw.Write((uint)(tgiOffset - startOffset));
            bw.Write((uint)tgiSize);
            s.Seek(endOffset, SeekOrigin.Begin);
            return s;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new GEOM(0, handler, this); }

        protected override List<string> ValueBuilderFields
        {
            get
            {
                var f = base.ValueBuilderFields;
                f.Remove("Vertices");
                f.Remove("Indices");
                return f;
            }
        }
        public override List<string> ContentFields
        {
            get
            {
                var f = base.ContentFields;
                if (mShader == 0)
                {
                    f.Remove("MaterialBlock");
                }
                return f;
            }
        }

        #region Sub-types
        public enum VertexElementUsage
        {
            Position = 1,
            Normal = 2,
            UV = 3,
            Assignment = 4,
            Weights = 5,
            Tangent = 6,
            Colour = 7,
            VertexId = 10
        }

        public enum VertexElementType
        {
            Float = 1,
            Byte = 2,
            ARGB = 3,
            UInt32 = 4
        }

        public class IndexList : SimpleList<UInt32>
        {
            private byte mFormat = 2;

            public IndexList(EventHandler handler) : base(handler) { }
            public IndexList(EventHandler handler, Stream s) : base(handler, s, null, null) { }
            public IndexList(EventHandler handler, IEnumerable<uint> ilt) : base(handler, ilt) { }

            protected override void Parse(Stream s)
            {
                mFormat = new BinaryReader(s).ReadByte();
                base.Parse(s);
            }

            public override void UnParse(Stream s)
            {
                mFormat = (byte)(Count > 0 ? (((IEnumerable<UInt32>)this).Max() > ushort.MaxValue ? 4 : 2) : 2);
                new BinaryWriter(s).Write(mFormat);
                base.UnParse(s);
            }

            protected override HandlerElement<UInt32> CreateElement(Stream s)
            {
                var br = new BinaryReader(s);
                switch (mFormat)
                {
                    case 0x02:
                        return new HandlerElement<UInt32>(0, elementHandler, br.ReadUInt16());
                    case 0x04:
                        return new HandlerElement<UInt32>(0, elementHandler, br.ReadUInt32());
                    default:
                        throw new Exception("Unknown index format " + mFormat);
                }
            }

            protected override void WriteElement(Stream s, HandlerElement<UInt32> element)
            {
                var bw = new BinaryWriter(s);
                switch (mFormat)
                {
                    case 0x02:
                        bw.Write((UInt16)element);
                        break;
                    case 0x04:
                        bw.Write((UInt32)element);
                        break;
                    default:
                        throw new Exception("Unknown index format " + mFormat);
                }
            }
        }

        public class VertexList : DependentList<Vertex>
        {
            private GEOM mRoot;
            public VertexList(EventHandler handler, GEOM root) : base(handler) { mRoot = root; }

            public VertexList(EventHandler handler, GEOM root, IEnumerable<Vertex> ilt)
                : base(handler, ilt)
            {
                mRoot = root;
                foreach (var e in ilt)
                {
                    base.Add(new object[] { e, root });
                }
            }

            public override void Add() { Add(new object[] { mRoot }); }
            protected override Vertex CreateElement(Stream s) { throw new NotSupportedException(); }
            protected override void WriteElement(Stream s, Vertex element) { throw new NotSupportedException(); }
            public override void UnParse(Stream s) { throw new NotSupportedException(); }
            protected override void Parse(Stream s) { throw new NotSupportedException(); }
        }

        public class Vertex : AHandlerElement,
                              IEquatable<Vertex>
        {
            private GEOM mRoot;
            private Vector3 mPosition;
            private Vector3 mNormal;
            private Vector2 mUV;
            private UByte4 mAssignments;
            private Vector4 mWeights;
            private Vector3 mTangent;
            private UByte4 mColour;
            private UInt32 mVertexId;

            public Vertex(int APIversion, EventHandler handler, GEOM root)
                : base(APIversion, handler)
            {
                mRoot = root;
                mPosition = new Vector3(0, handler);
                mNormal = new Vector3(0, handler);
                mUV = new Vector2(0, handler);
                mAssignments = new UByte4(0, handler);
                mWeights = new Vector4(0, handler);
                mColour = new UByte4(0, handler);
                mTangent = new Vector3(0, handler);
            }

            public Vertex(int APIversion, EventHandler handler, Vertex basis, GEOM root)
                : base(APIversion, handler)
            {
                mRoot = root;
                mPosition = new Vector3(0, handler, basis.mPosition);
                mNormal = new Vector3(0, handler, basis.mNormal);
                mUV = new Vector2(0, handler, basis.mUV);
                mAssignments = new UByte4(0, handler, basis.mAssignments);
                mWeights = new Vector4(0, handler, basis.mWeights);
                mColour = new UByte4(0, handler, basis.mColour);
                mTangent = new Vector3(0, handler, basis.mTangent);
            }

            public Vertex(int APIversion, EventHandler handler, Stream s, GEOM root) : this(APIversion, handler, root) { Parse(s); }

            public string Value
            {
                get
                {
                    return ValueBuilder;
                    /*
                    StringBuilder sb = new StringBuilder();
                    foreach (var f in ContentFields)
                    {
                        sb.AppendFormat("{0}:\t{1}\n", f, this[f]);
                    }
                    return sb.ToString();
                    /**/
                }
            }

            [ElementPriority(1)]
            public Vector3 Position
            {
                get { return mPosition; }
                set
                {
                    if (mPosition != value)
                    {
                        mPosition = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(2)]
            public Vector3 Normal
            {
                get { return mNormal; }
                set
                {
                    if (mNormal != value)
                    {
                        mNormal = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(3)]
            public Vector2 UV
            {
                get { return mUV; }
                set
                {
                    if (mUV != value)
                    {
                        mUV = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(4)]
            public UByte4 Assignments
            {
                get { return mAssignments; }
                set
                {
                    if (mAssignments != value)
                    {
                        mAssignments = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(5)]
            public Vector4 Weights
            {
                get { return mWeights; }
                set
                {
                    if (mWeights != value)
                    {
                        mWeights = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(6)]
            public Vector3 Tangent
            {
                get { return mTangent; }
                set
                {
                    if (mTangent != value)
                    {
                        mTangent = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(7)]
            public UByte4 Colour
            {
                get { return mColour; }
                set
                {
                    if (mColour != value)
                    {
                        mColour = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(8)]
            public UInt32 VertexId
            {
                get { return mVertexId; }
                set
                {
                    if (mVertexId != value)
                    {
                        mVertexId = value;
                        OnElementChanged();
                    }
                }
            }

            protected void Parse(Stream s)
            {
                foreach (var format in mRoot.VertexFormat.Elements)
                {
                    switch (format.Usage)
                    {
                        case VertexElementUsage.Position:
                            mPosition = new Vector3(0, handler, s);
                            break;
                        case VertexElementUsage.Normal:
                            mNormal = new Vector3(0, handler, s);
                            break;
                        case VertexElementUsage.UV:
                            mUV = new Vector2(0, handler, s);
                            break;
                        case VertexElementUsage.Assignment:
                            mAssignments = new UByte4(0, handler, s);
                            break;
                        case VertexElementUsage.Weights:
                            mWeights = new Vector4(0, handler, s);
                            break;
                        case VertexElementUsage.Tangent:
                            mTangent = new Vector3(0, handler, s);
                            break;
                        case VertexElementUsage.Colour:
                            mColour = new UByte4(0, handler, s);
                            break;
                        case VertexElementUsage.VertexId:
                            mVertexId = new BinaryReader(s).ReadUInt32();
                            break;
                    }
                }
            }

            public void UnParse(Stream s)
            {
                foreach (var format in mRoot.VertexFormat.Elements)
                {
                    switch (format.Usage)
                    {
                        case VertexElementUsage.Position:
                            mPosition.UnParse(s);
                            break;
                        case VertexElementUsage.Normal:
                            mNormal.UnParse(s);
                            break;
                        case VertexElementUsage.UV:
                            mUV.UnParse(s);
                            break;
                        case VertexElementUsage.Assignment:
                            mAssignments.UnParse(s);
                            break;
                        case VertexElementUsage.Weights:
                            mWeights.UnParse(s);
                            break;
                        case VertexElementUsage.Tangent:
                            mTangent.UnParse(s);
                            break;
                        case VertexElementUsage.Colour:
                            mColour.UnParse(s);
                            break;
                        case VertexElementUsage.VertexId:
                            new BinaryWriter(s).Write(mVertexId);
                            break;
                    }
                }
            }

            public override AHandlerElement Clone(EventHandler handler) { return new Vertex(0, handler, this, mRoot); }

            public override List<string> ContentFields
            {
                get
                {
                    List<string> fields = new List<string> { "Value" };
                    foreach (var format in mRoot.VertexFormat.Elements)
                    {
                        switch (format.Usage)
                        {
                            case VertexElementUsage.Position:
                                fields.Add("Position");
                                break;
                            case VertexElementUsage.Normal:
                                fields.Add("Normal");
                                break;
                            case VertexElementUsage.UV:
                                fields.Add("UV");
                                break;
                            case VertexElementUsage.Assignment:
                                fields.Add("Assignments");
                                break;
                            case VertexElementUsage.Weights:
                                fields.Add("Weights");
                                break;
                            case VertexElementUsage.Tangent:
                                fields.Add("Tangent");
                                break;
                            case VertexElementUsage.Colour:
                                fields.Add("Colour");
                                break;
                            case VertexElementUsage.VertexId:
                                fields.Add("VertexId");
                                break;
                        }
                    }
                    return fields;
                }
            }

            public override int RecommendedApiVersion { get { return kRecommendedApiVersion; } }

            public bool Equals(Vertex other)
            {
                return
                    mRoot.Equals(other.mRoot)
                    && mPosition.Equals(other.mPosition)
                    && mNormal.Equals(other.mNormal)
                    && mUV.Equals(other.mUV)
                    && mAssignments.Equals(other.mAssignments)
                    && mWeights.Equals(other.mWeights)
                    && mTangent.Equals(other.mTangent)
                    && mColour.Equals(other.mColour)
                    && mVertexId.Equals(other.mVertexId);
            }

            public override bool Equals(object obj)
            {
                return obj as Vertex != null ? this.Equals(obj as Vertex) : false;
            }

            public override int GetHashCode()
            {
                return
                    mRoot.GetHashCode()
                    ^ mPosition.GetHashCode()
                    ^ mNormal.GetHashCode()
                    ^ mUV.GetHashCode()
                    ^ mAssignments.GetHashCode()
                    ^ mWeights.GetHashCode()
                    ^ mTangent.GetHashCode()
                    ^ mColour.GetHashCode()
                    ^ mVertexId.GetHashCode();
            }
        }

        public class VertexElementFormatList : DependentList<VertexElementFormat>
        {
            public VertexElementFormatList(EventHandler handler) : base(handler) { }
            public VertexElementFormatList(EventHandler handler, Stream s) : base(handler, s) { }
            public VertexElementFormatList(EventHandler handler, IEnumerable<VertexElementFormat> ilt) : base(handler, ilt) { }
            public override void Add() { base.Add(new object[] { }); }
            protected override VertexElementFormat CreateElement(Stream s) { return new VertexElementFormat(0, handler, s); }
            protected override void WriteElement(Stream s, VertexElementFormat element) { element.UnParse(s); }
        }

        // This and VertexElementFormatList need merging...
        public class VertexDataFormat : AHandlerElement
        {
            private VertexElementFormatList mElements;
            public VertexDataFormat(int APIversion, EventHandler handler) : base(APIversion, handler) { mElements = new VertexElementFormatList(handler); }
            public VertexDataFormat(int APIversion, EventHandler handler, VertexDataFormat basis) : base(APIversion, handler) { mElements = new VertexElementFormatList(handler, basis.Elements); }
            public VertexDataFormat(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public string Value { get { return ValueBuilder; } }

            [ElementPriority(1)]
            public VertexElementFormatList Elements
            {
                get { return mElements; }
                set
                {
                    if (mElements != value)
                    {
                        mElements = value;
                        OnElementChanged();
                    }
                }
            }

            protected void Parse(Stream s) { mElements = new VertexElementFormatList(handler, s); }
            public void UnParse(Stream s) { mElements.UnParse(s); }
            public override AHandlerElement Clone(EventHandler handler) { return new VertexDataFormat(0, handler, this); }
            public override List<string> ContentFields { get { return GetContentFields(base.requestedApiVersion, GetType()); } }
            public override int RecommendedApiVersion { get { return kRecommendedApiVersion; } }
        }

        public class VertexElementFormat : AHandlerElement,
                                           IEquatable<VertexElementFormat>,
                                           IComparable<VertexElementFormat>
        {
            protected VertexElementUsage mUsage;
            protected VertexElementType mType;
            protected Byte mSize;
            public VertexElementFormat(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public VertexElementFormat(int APIversion, EventHandler handler, VertexElementFormat basis) : this(APIversion, handler, basis.Type, basis.Size, basis.Usage) { }
            public VertexElementFormat(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }

            public VertexElementFormat(int APIversion, EventHandler handler, VertexElementType vertexElementType, byte size, VertexElementUsage vertexElementUsage)
                : base(APIversion, handler)
            {
                mType = vertexElementType;
                mSize = size;
                mUsage = vertexElementUsage;
            }

            [ElementPriority(1)]
            public VertexElementUsage Usage
            {
                get { return mUsage; }
                set
                {
                    if (mUsage != value)
                    {
                        mUsage = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(2)]
            public VertexElementType Type
            {
                get { return mType; }
                set
                {
                    if (mType != value)
                    {
                        mType = value;
                        OnElementChanged();
                    }
                }
            }

            [ElementPriority(3)]
            public byte Size
            {
                get { return mSize; }
                set
                {
                    if (mSize != value)
                    {
                        mSize = value;
                        OnElementChanged();
                    }
                }
            }

            public string Value { get { return String.Format("{0}:{1} ({2:X2})", Usage, Type, Size); } }

            protected void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                mUsage = (VertexElementUsage)br.ReadUInt32();
                mType = (VertexElementType)br.ReadUInt32();
                mSize = br.ReadByte();
            }

            public void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                bw.Write((uint)mUsage);
                bw.Write((uint)mType);
                bw.Write((byte)mSize);
            }

            public override AHandlerElement Clone(EventHandler handler) { return new VertexElementFormat(0, handler, this); }
            public override List<string> ContentFields { get { return GetContentFields(base.requestedApiVersion, GetType()); } }

            public override int RecommendedApiVersion { get { return kRecommendedApiVersion; } }

            public bool Equals(VertexElementFormat other)
            {
                return
                    mUsage.Equals(other.mUsage)
                    && mType.Equals(other.mType)
                    && mSize.Equals(other.mSize)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as VertexElementFormat != null ? this.Equals(obj as VertexElementFormat) : false;
            }
            public override int GetHashCode()
            {
                return
                    mUsage.GetHashCode()
                    ^ mType.GetHashCode()
                    ^ mSize.GetHashCode()
                ;
            }

            public int CompareTo(VertexElementFormat other) { return mUsage.CompareTo(other.Usage); }
        }
        #endregion

        [ElementPriority(1)]
        public uint Version
        {
            get { return mVersion; }
            set
            {
                if (mVersion != value)
                {
                    mVersion = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(2)]       

        public MATD.ShaderType Shader
        {
            get { return mShader; }
            set
            {
                if (mShader != value)
                {
                    mShader = value;
                    OnElementChanged();
                }
            }
        }

        [ElementPriority(3)]
        public MATD.MTNF MaterialBlock
        {
            get { return mMaterialBlock; }
            set
            {
                if (mMaterialBlock != value)
                {
                    mMaterialBlock = value;
                    OnElementChanged();
                }
            }
        }

        [ElementPriority(4)]
        public uint MergeGroup
        {
            get { return mMergeGroup; }
            set
            {
                if (mMergeGroup != value)
                {
                    mMergeGroup = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(5)]
        public uint SortOrder
        {
            get { return mSortOrder; }
            set
            {
                if (mSortOrder != value)
                {
                    mSortOrder = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(6)]
        public VertexDataFormat VertexFormat
        {
            get { return mVertexFormat; }
            set
            {
                if (mVertexFormat != value)
                {
                    mVertexFormat = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(6)]
        public VertexList Vertices
        {
            get { return mVertices; }
            set
            {
                if (mVertices != value)
                {
                    mVertices = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(7)]
        public IndexList Indices
        {
            get { return mIndices; }
            set
            {
                if (mIndices != value)
                {
                    mIndices = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(8), TGIBlockListContentField("References")]
        public UInt32 SkinControllerIndex
        {
            get { return mSkinControllerIndex; }
            set
            {
                if (mSkinControllerIndex != value)
                {
                    mSkinControllerIndex = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(9)]
        public UIntList Joints
        {
            get { return mJoints; }
            set
            {
                if (mJoints != value)
                {
                    mJoints = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        [ElementPriority(10)]
        public TGIBlockList References
        {
            get { return mReferences; }
            set
            {
                if (mReferences != value)
                {
                    mReferences = value;
                    OnRCOLChanged(this, new EventArgs());
                }
            }
        }

        public String Value { get { return ValueBuilder; } }
#endif

        static bool checking = s3pi.Settings.Settings.Checking;
        public override uint ResourceType { get { return 0x015A1849; } }
        public override string Tag { get { return "GEOM"; } }

        #region Attributes
        uint tag = (uint)FOURCC("GEOM");
        uint version = 0x00000005;
        MATD.ShaderType shader;
        MATD.MTNF mtnf = null;
        uint mergeGroup;
        uint sortOrder;
        VertexFormatList vertexFormats;
        VertexDataList vertexData;
        FaceList faces;
        int skinIndex;
        UIntList boneHashes;

        TGIBlockList tgiBlockList;
        #endregion

        #region Constructors
        public GEOM(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public GEOM(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public GEOM(int APIversion, EventHandler handler, GEOM basis)
            : this(APIversion, handler,
            basis.version, basis.shader, basis.mtnf, basis.mergeGroup, basis.sortOrder,
            basis.vertexFormats, basis.vertexData,
            basis.faces, basis.skinIndex, basis.boneHashes,
            basis.tgiBlockList) { }
        public GEOM(int APIversion, EventHandler handler,
            uint version, MATD.ShaderType shader, MATD.MTNF mtnf, uint mergeGroup, uint sortOrder,
            IEnumerable<VertexFormat> vertexFormats, IEnumerable<VertexDataElement> vertexData,
            IEnumerable<Face> facePoints, int skinIndex, IEnumerable<uint> boneHashes,
            IEnumerable<TGIBlock> tgiBlockList)
            : base(APIversion, handler, null)
        {
            this.version = version;
            this.shader = shader;
            if (shader != 0 && mtnf == null)
                throw new ArgumentException("Must supply MTNF when applying a Shader.");
            this.mtnf = shader == 0 ? null : new MATD.MTNF(requestedApiVersion, handler, mtnf);
            this.mergeGroup = mergeGroup;
            this.sortOrder = sortOrder;
            this.vertexFormats = new VertexFormatList(handler, vertexFormats);
            this.vertexData = new VertexDataList(handler, vertexData, this.vertexFormats);
            this.faces = new FaceList(handler, facePoints);
            this.skinIndex = skinIndex;
            this.boneHashes = new UIntList(handler, boneHashes);
            this.tgiBlockList = new TGIBlockList(handler, tgiBlockList);
        }
        #endregion

        #region ARCOLBlock
        public override AHandlerElement Clone(EventHandler handler) { return new GEOM(requestedApiVersion, handler, this); }

        public override List<string> ContentFields
        {
            get
            {
                List<string> res = base.ContentFields;
                if (shader == 0)
                    res.Remove("Mtnf");
                return res;
            }
        }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC("GEOM"))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: 'GEOM'; at 0x{1:X8}", FOURCC(tag), s.Position));
            version = r.ReadUInt32();
            if (checking) if (version != 0x00000005)
                    throw new InvalidDataException(String.Format("Invalid Version read: '{0}'; expected: '0x00000005'; at 0x{1:X8}", version, s.Position));

            long tgiPosn = r.ReadUInt32() + s.Position;
            long tgiSize = r.ReadUInt32();

            shader = (MATD.ShaderType)r.ReadUInt32();
            if (shader != 0)
            {
                uint size = r.ReadUInt32();
                long posn = s.Position;
                mtnf = new MATD.MTNF(requestedApiVersion, handler, s);
                if (checking) if (s.Position != posn + size)
                    throw new InvalidDataException(String.Format("MTNF chunk size invalid; expected 0x{0:X8} bytes, read 0x{1:X8} bytes; at 0x{2:X8}",
                        size, s.Position - posn, s.Position));
            }
            else mtnf = null;

            mergeGroup = r.ReadUInt32();
            sortOrder = r.ReadUInt32();

            int numVertices = r.ReadInt32();//now write that down...
            vertexFormats = new VertexFormatList(handler, s);
            vertexData = new VertexDataList(handler, s, numVertices, vertexFormats);//...as you'll be needing it

            int numFacePointSizes = r.ReadInt32();
            if (checking) if (numFacePointSizes != 1)
                    throw new InvalidDataException(String.Format("Expected number of face point sizes to be 1, read {0}, at 0x{1:X8}", numFacePointSizes, s.Position));

            byte facePointSize = r.ReadByte();
            if (checking) if (facePointSize != 2)
                    throw new InvalidDataException(String.Format("Expected face point size to be 2, read {0}, at 0x{1:X8}", facePointSize, s.Position));

            faces = new FaceList(handler, s);
            skinIndex = r.ReadInt32();
            boneHashes = new UIntList(handler, s);

            tgiBlockList = new TGIBlockList(OnRCOLChanged, s, tgiPosn, tgiSize);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            long pos = ms.Position;
            w.Write((uint)0); // tgiOffset
            w.Write((uint)0); // tgiSize

            w.Write((uint)shader);
            if (shader != 0)
            {
                if (mtnf == null) mtnf = new MATD.MTNF(requestedApiVersion, handler);
                byte[] mtnfData = mtnf.AsBytes;
                w.Write(mtnfData.Length);
                w.Write(mtnfData);
            }

            w.Write(mergeGroup);
            w.Write(sortOrder);

            if (vertexData == null) w.Write(0);
            else w.Write(vertexData.Count);
            if (vertexFormats == null) vertexFormats = new VertexFormatList(handler);
            vertexFormats.UnParse(ms);
            if (vertexData == null) vertexData = new VertexDataList(handler, vertexFormats);
            vertexData.UnParse(ms);
            w.Write((int)1);
            w.Write((byte)2);
            if (faces == null) faces = new FaceList(handler);
            faces.UnParse(ms);
            w.Write(skinIndex);
            if (boneHashes == null) boneHashes = new UIntList(handler);
            boneHashes.UnParse(ms);

            if (tgiBlockList == null) tgiBlockList = new TGIBlockList(OnRCOLChanged);
            tgiBlockList.UnParse(ms, pos);

            return ms;
        }

        private byte ReadByte(Stream s) { return new BinaryReader(s).ReadByte(); }
        private void WriteByte(Stream s, byte element) { new BinaryWriter(s).Write(element); }
        #endregion

        #region Sub-Types

        #region VertexFormat
        public enum UsageType : uint
        {
            Position = 0x01,
            Normal = 0x02,
            UV = 0x03,
            BoneAssignment = 0x04,
            Weights = 0x05,
            TangentNormal = 0x06,
            Color = 0x07,
            VertexID = 0x0A,
        }
        static uint[] expectedDataType = new uint[] {
            /*Unknown*/ 0,
            /*Position*/ 1,
            /*Normal*/ 1,
            /*UV*/ 1,
            /*BoneAssignment*/ 2,
            /*Weights*/ 1, 
            /*TangentNormal*/ 1,
            /*Color*/ 3,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        static byte[] expectedElementSize = new byte[] {
            /*Unknown*/ 0,
            /*Position*/ 12,
            /*Normal*/ 12,
            /*UV*/ 8,
            /*BoneAssignment*/ 4,
            /*Weights*/ 16, 
            /*TangentNormal*/ 12,
            /*Color*/ 4,
            /*Unknown*/ 0,
            /*Unknown*/ 0,
            /*VertexID*/ 4,
            /**/
        };
        public class VertexFormat : AHandlerElement, IEquatable<VertexFormat>
        {
            const int recommendedApiVersion = 1;

            UsageType usage;

            public VertexFormat(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public VertexFormat(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public VertexFormat(int APIversion, EventHandler handler, VertexFormat basis)
                : this(APIversion, handler, basis.usage) { }
            public VertexFormat(int APIversion, EventHandler handler, UsageType usage)
                : base(APIversion, handler)
            {
                this.usage = usage;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                usage = (UsageType)r.ReadUInt32();
                if (checking) if (usage == 0 || (uint)usage >= expectedDataType.Length)
                    throw new InvalidDataException(string.Format("Unexpected usage code 0x{0:X8} at 0x{1:X8}", (uint)usage, s.Position));

                uint dataType = r.ReadUInt32();
                if (checking) if (dataType != expectedDataType[(uint)usage])
                    throw new InvalidDataException(string.Format("Expected data type 0x{0:X8}, read 0x{1:X8}, at 0x{2:X8}", expectedDataType[(uint)usage], dataType, s.Position));

                byte elementSize = r.ReadByte();
                if (checking) if (elementSize != expectedElementSize[(uint)usage])
                    throw new InvalidDataException(String.Format("Expected element size 0x{0:X2}, read 0x{1:X2}, at {2:X8}", expectedElementSize[(uint)usage], elementSize, s.Position));
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((uint)usage);
                w.Write(expectedDataType[(uint)usage]);
                w.Write(expectedElementSize[(uint)usage]);
            }

            #region AHandlerElement
            public override AHandlerElement Clone(EventHandler handler) { return new VertexFormat(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<VertexFormat>
            public bool Equals(VertexFormat other)
            {
                return this.usage.Equals(other.usage);
            }

            public override bool Equals(object obj) { return obj is VertexFormat && Equals(obj as VertexFormat); }

            public override int GetHashCode() { return usage.GetHashCode(); }
            #endregion

            [ElementPriority(1)]
            public UsageType Usage { get { return usage; } set { if (!usage.Equals(value)) { usage = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
        }
        public class VertexFormatList : DependentList<VertexFormat>
        {
            #region Constructors
            public VertexFormatList(EventHandler handler) : base(handler) { }
            public VertexFormatList(EventHandler handler, Stream s) : base(handler, s) { }
            public VertexFormatList(EventHandler handler, IEnumerable<VertexFormat> le) : base(handler, le) { }
            #endregion

            protected override VertexFormat CreateElement(Stream s) { return new VertexFormat(0, elementHandler, s); }
            protected override void WriteElement(Stream s, VertexFormat element) { element.UnParse(s); }

            public override void Add() { this.Add(new VertexFormat(0, elementHandler)); }
        }
        #endregion

        #region VertexElement
        public abstract class VertexElement : AHandlerElement, IEquatable<VertexElement>
        {
            const int recommendedApiVersion = 1;

            protected VertexElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            protected VertexElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }

            protected abstract void Parse(Stream s);
            internal abstract void UnParse(Stream s);

            #region AHandlerElement
            //public abstract AHandlerElement Clone(EventHandler handler);
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public abstract bool Equals(VertexElement other);

            public virtual string Value { get { return string.Join("; ", ValueBuilder.Split('\n')); } }
        }
        public class PositionElement : VertexElement
        {
            protected float x, y, z;

            public PositionElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public PositionElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public PositionElement(int APIversion, EventHandler handler, PositionElement basis) : this(APIversion, handler, basis.x, basis.y, basis.z) { }
            public PositionElement(int APIversion, EventHandler handler, float x, float y, float z) : base(APIversion, handler) { this.x = x; this.y = y; this.z = z; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); x = r.ReadSingle(); y = r.ReadSingle(); z = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(x); w.Write(y); w.Write(z); }

            public override AHandlerElement Clone(EventHandler handler) { return new PositionElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { PositionElement o = other as PositionElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is PositionElement && this.Equals(obj as PositionElement); }
            public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode(); }

            [ElementPriority(1)]
            public float X { get { return x; } set { if (!x.Equals(value)) { x = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float Y { get { return y; } set { if (!y.Equals(value)) { y = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float Z { get { return z; } set { if (!z.Equals(value)) { z = value; OnElementChanged(); } } }
        }
        public class NormalElement : PositionElement
        {
            public NormalElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public NormalElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public NormalElement(int APIversion, EventHandler handler, NormalElement basis) : this(APIversion, handler, basis.x, basis.y, basis.z) { }
            public NormalElement(int APIversion, EventHandler handler, float x, float y, float z) : base(APIversion, handler) { this.x = x; this.y = y; this.z = z; }

            public override AHandlerElement Clone(EventHandler handler) { return new NormalElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { NormalElement o = other as NormalElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is NormalElement && this.Equals(obj as NormalElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class UVElement : VertexElement
        {
            protected float u, v;

            public UVElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public UVElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public UVElement(int APIversion, EventHandler handler, UVElement basis) : this(APIversion, handler, basis.u, basis.v) { }
            public UVElement(int APIversion, EventHandler handler, float u, float v) : base(APIversion, handler) { this.u = u; this.v = v;  }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); u = r.ReadSingle(); v = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(u); w.Write(v);  }

            public override AHandlerElement Clone(EventHandler handler) { return new UVElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { UVElement o = other as UVElement; return o != null && u.Equals(o.u) && v.Equals(o.v); }
            public override bool Equals(object obj) { return obj is UVElement && this.Equals(obj as UVElement); }
            public override int GetHashCode() { return u.GetHashCode() ^ v.GetHashCode(); }

            [ElementPriority(1)]
            public float U { get { return u; } set { if (!u.Equals(value)) { u = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float V { get { return v; } set { if (!v.Equals(value)) { v = value; OnElementChanged(); } } }
        }
        public class BoneAssignmentElement : VertexElement
        {
            protected uint id;

            public BoneAssignmentElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public BoneAssignmentElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public BoneAssignmentElement(int APIversion, EventHandler handler, BoneAssignmentElement basis) : this(APIversion, handler, basis.id) { }
            public BoneAssignmentElement(int APIversion, EventHandler handler, uint id) : base(APIversion, handler) { this.id = id; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); id = r.ReadUInt32(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(id); }

            public override AHandlerElement Clone(EventHandler handler) { return new BoneAssignmentElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { BoneAssignmentElement o = other as BoneAssignmentElement; return o != null && id.Equals(o.id); }
            public override bool Equals(object obj) { return obj is BoneAssignmentElement && this.Equals(obj as BoneAssignmentElement); }
            public override int GetHashCode() { return id.GetHashCode(); }

            [ElementPriority(1)]
            public uint ID { get { return id; } set { if (!id.Equals(value)) { id = value; OnElementChanged(); } } }
        }
        public class WeightsElement : VertexElement
        {
            protected float w1, w2, w3, w4;

            public WeightsElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public WeightsElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public WeightsElement(int APIversion, EventHandler handler, WeightsElement basis) : this(APIversion, handler, basis.w1, basis.w2, basis.w3, basis.w4) { }
            public WeightsElement(int APIversion, EventHandler handler, float w1, float w2, float w3, float w4) : base(APIversion, handler) { this.w1 = w1; this.w2 = w2; this.w3 = w3; this.w4 = w4; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); w1 = r.ReadSingle(); w2 = r.ReadSingle(); w3 = r.ReadSingle(); w4 = r.ReadSingle(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(w1); w.Write(w2); w.Write(w3); w.Write(w4); }

            public override AHandlerElement Clone(EventHandler handler) { return new WeightsElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { WeightsElement o = other as WeightsElement; return o != null && w1.Equals(o.w1) && w2.Equals(o.w2) && w3.Equals(o.w3) && w3.Equals(o.w4); }
            public override bool Equals(object obj) { return obj is WeightsElement && this.Equals(obj as WeightsElement); }
            public override int GetHashCode() { return w1.GetHashCode() ^ w2.GetHashCode() ^ w3.GetHashCode() ^ w4.GetHashCode(); }

            [ElementPriority(1)]
            public float W1 { get { return w1; } set { if (!w1.Equals(value)) { w1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float W2 { get { return w2; } set { if (!w2.Equals(value)) { w2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float W3 { get { return w3; } set { if (!w3.Equals(value)) { w3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public float W4 { get { return w4; } set { if (!w4.Equals(value)) { w4 = value; OnElementChanged(); } } }
        }
        public class TangentNormalElement : PositionElement
        {
            public TangentNormalElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public TangentNormalElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public TangentNormalElement(int APIversion, EventHandler handler, TangentNormalElement basis) : this(APIversion, handler, basis.x, basis.y, basis.z) { }
            public TangentNormalElement(int APIversion, EventHandler handler, float x, float y, float z) : base(APIversion, handler) { this.x = x; this.y = y; this.z = z; }

            public override AHandlerElement Clone(EventHandler handler) { return new TangentNormalElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { TangentNormalElement o = other as TangentNormalElement; return o != null && x.Equals(o.x) && y.Equals(o.y) && z.Equals(o.z); }
            public override bool Equals(object obj) { return obj is TangentNormalElement && this.Equals(obj as TangentNormalElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class ColorElement : VertexElement
        {
            uint argb;

            public ColorElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public ColorElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public ColorElement(int APIversion, EventHandler handler, ColorElement basis) : this(APIversion, handler, basis.argb) { }
            public ColorElement(int APIversion, EventHandler handler, uint argb) : base(APIversion, handler) { this.argb = argb; }

            protected override void Parse(Stream s) { BinaryReader r = new BinaryReader(s); argb = r.ReadUInt32(); }
            internal override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(argb); }

            public override AHandlerElement Clone(EventHandler handler) { return new ColorElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { ColorElement o = other as ColorElement; return o != null && argb.Equals(o.argb); }
            public override bool Equals(object obj) { return obj is PositionElement && this.Equals(obj as PositionElement); }
            public override int GetHashCode() { return argb.GetHashCode(); }
        }
        public class VertexIDElement : BoneAssignmentElement
        {
            public VertexIDElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public VertexIDElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
            public VertexIDElement(int APIversion, EventHandler handler, VertexIDElement basis) : this(APIversion, handler, basis.id) { }
            public VertexIDElement(int APIversion, EventHandler handler, uint id) : base(APIversion, handler) { this.id = id; }

            public override AHandlerElement Clone(EventHandler handler) { return new VertexIDElement(requestedApiVersion, handler, this); }
            public override bool Equals(VertexElement other) { VertexIDElement o = other as VertexIDElement; return o != null && id.Equals(o.id); }
            public override bool Equals(object obj) { return obj is VertexIDElement && this.Equals(obj as VertexIDElement); }
            public override int GetHashCode() { return base.GetHashCode(); }
        }
        public class ElementList : DependentList<VertexElement>
        {
            public DependentList<VertexFormat> ParentVertexFormats { get; private set; }

            #region Constructors
            public ElementList(EventHandler handler) : base(handler) { }
            public ElementList(EventHandler handler, Stream s, DependentList<VertexFormat> parentVertexFormats)
                : base(null)
            {
                this.ParentVertexFormats = parentVertexFormats;
                elementHandler = handler;
                foreach (var fmt in parentVertexFormats)
                {
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: this.Add(new PositionElement(0, handler, s)); break;
                        case UsageType.Normal: this.Add(new NormalElement(0, handler, s)); break;
                        case UsageType.UV: this.Add(new UVElement(0, handler, s)); break;
                        case UsageType.BoneAssignment: this.Add(new BoneAssignmentElement(0, handler, s)); break;
                        case UsageType.Weights: this.Add(new WeightsElement(0, handler, s)); break;
                        case UsageType.TangentNormal: this.Add(new TangentNormalElement(0, handler, s)); break;
                        case UsageType.Color: this.Add(new ColorElement(0, handler, s)); break;
                        case UsageType.VertexID: this.Add(new VertexIDElement(0, handler, s)); break;
                    }
                }
                this.handler = handler;
            }
            public ElementList(EventHandler handler, IEnumerable<VertexElement> ilt, DependentList<VertexFormat> parentVertexFormats)
                : base(null)
            {
                this.ParentVertexFormats = parentVertexFormats;
                elementHandler = handler;
                foreach (var fmt in parentVertexFormats)
                {
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: this.Add(ilt.FirstOrDefault(t => t is PositionElement) ?? new PositionElement(0, handler)); break;
                        case UsageType.Normal: this.Add(ilt.FirstOrDefault(t => t is NormalElement) ?? new NormalElement(0, handler)); break;
                        case UsageType.UV: this.Add(ilt.FirstOrDefault(t => t is UVElement) ?? new UVElement(0, handler)); break;
                        case UsageType.BoneAssignment: this.Add(ilt.FirstOrDefault(t => t is BoneAssignmentElement) ?? new BoneAssignmentElement(0, handler)); break;
                        case UsageType.Weights: this.Add(ilt.FirstOrDefault(t => t is WeightsElement) ?? new WeightsElement(0, handler)); break;
                        case UsageType.TangentNormal: this.Add(ilt.FirstOrDefault(t => t is TangentNormalElement) ?? new TangentNormalElement(0, handler)); break;
                        case UsageType.Color: this.Add(ilt.FirstOrDefault(t => t is ColorElement) ?? new ColorElement(0, handler)); break;
                        case UsageType.VertexID: this.Add(ilt.FirstOrDefault(t => t is VertexIDElement) ?? new VertexIDElement(0, handler)); break;
                    }
                }
                this.handler = handler;
            }
            #endregion

            protected override VertexElement CreateElement(Stream s) { throw new NotImplementedException(); }

            public override void UnParse(Stream s)
            {
                foreach (var fmt in ParentVertexFormats)
                {
                    VertexElement vtx = null;
                    switch (fmt.Usage)
                    {
                        case UsageType.Position: vtx = this.Find(e => e is PositionElement); break;
                        case UsageType.Normal: vtx = this.Find(e => e is NormalElement); break;
                        case UsageType.UV: vtx = this.Find(e => e is UVElement); break;
                        case UsageType.BoneAssignment: vtx = this.Find(e => e is BoneAssignmentElement); break;
                        case UsageType.Weights: vtx = this.Find(e => e is WeightsElement); break;
                        case UsageType.TangentNormal: vtx = this.Find(e => e is TangentNormalElement); break;
                        case UsageType.Color: vtx = this.Find(e => e is ColorElement); break;
                        case UsageType.VertexID: vtx = this.Find(e => e is VertexIDElement); break;
                    }
                    if (vtx == null)
                        throw new InvalidOperationException();
                    vtx.UnParse(s);
                }
            }

            protected override void WriteElement(Stream s, VertexElement element) { throw new NotImplementedException(); }

            public override void Add() { throw new NotImplementedException(); }
            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1) return fields[0].GetType();
                if (fields.Length == 3 && typeof(int).IsAssignableFrom(fields[0].GetType()) && typeof(EventHandler).IsAssignableFrom(fields[1].GetType()))
                    return fields[2].GetType();
                throw new ArgumentException();
            }

            public VertexElement this[UsageType usage]
            {
                get
                {
                    if (!ParentVertexFormats.Exists(x => x.Usage.Equals(usage)))
                        throw new IndexOutOfRangeException();
                    switch (usage)
                    {
                        case UsageType.Position: return this.Find(x => x is PositionElement);
                        case UsageType.Normal: return this.Find(x => x is NormalElement);
                        case UsageType.UV: return this.Find(x => x is UVElement);
                        case UsageType.BoneAssignment: return this.Find(x => x is BoneAssignmentElement);
                        case UsageType.Weights: return this.Find(x => x is WeightsElement);
                        case UsageType.TangentNormal: return this.Find(x => x is TangentNormalElement);
                        case UsageType.Color: return this.Find(x => x is ColorElement);
                        case UsageType.VertexID: return this.Find(x => x is VertexIDElement);
                    }
                    throw new ArgumentException();
                }
                set
                {
                    VertexElement vtx = this[usage];
                    if (vtx != null && vtx.Equals(value)) return;

                    int index = this.IndexOf(vtx);
                    if (value.GetType().Equals(vtx.GetType()))
                        this[index] = (VertexElement)vtx.Clone(handler);
                    else
                        throw new ArgumentException();
                }
            }
        }
        public class VertexDataElement : AHandlerElement, IEquatable<VertexDataElement>
        {
            const int recommendedApiVersion = 1;

            ElementList elementList;
            DependentList<VertexFormat> parentVertexFormats;

            public VertexDataElement(int APIversion, EventHandler handler, DependentList<VertexFormat> parentVertexFormats) : base(APIversion, handler) { this.parentVertexFormats = parentVertexFormats; }
            public VertexDataElement(int APIversion, EventHandler handler, Stream s, DependentList<VertexFormat> parentVertexFormats) : base(APIversion, handler) { this.parentVertexFormats = parentVertexFormats; Parse(s); }
            public VertexDataElement(int APIversion, EventHandler handler, VertexDataElement basis) : this(APIversion, handler, basis.elementList, basis.parentVertexFormats) { }
            public VertexDataElement(int APIversion, EventHandler handler, DependentList<VertexElement> elementList, DependentList<VertexFormat> parentVertexFormats)
                : base(APIversion, handler)
            {
                this.parentVertexFormats = parentVertexFormats;//reference!
                this.elementList = new ElementList(handler, elementList, parentVertexFormats);
            }

            private void Parse(Stream s) { elementList = new ElementList(handler, s, parentVertexFormats); }
            internal void UnParse(Stream s) { elementList.UnParse(s); }

            #region AHandlerElement
            public override AHandlerElement Clone(EventHandler handler) { return new VertexDataElement(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public bool Equals(VertexDataElement other) { return elementList.Equals(other.elementList); }
            public override bool Equals(object obj) { return obj is VertexDataElement && this.Equals(obj as VertexDataElement); }
            public override int GetHashCode() { return elementList.GetHashCode(); }

            public ElementList Vertex
            {
                get { return elementList; }
                set { if (!elementList.Equals(value)) { elementList = new ElementList(handler, value, parentVertexFormats); OnElementChanged(); } }
            }

            public string Value
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var fmt in parentVertexFormats)
                    {
                        sb.AppendLine(fmt.Usage.ToString() + ": " + elementList[fmt.Usage].Value);
                    }
                    return sb.ToString();
                }
            }
        }

        public class VertexDataList : DependentList<VertexDataElement>
        {
            int origCount;
            DependentList<VertexFormat> parentVertexFormats;

            #region Constructors
            public VertexDataList(EventHandler handler, DependentList<VertexFormat> parentVertexFormats) : base(handler) { this.parentVertexFormats = parentVertexFormats; }
            public VertexDataList(EventHandler handler, Stream s, int origCount, DependentList<VertexFormat> parentVertexFormats) : base(null) { this.origCount = origCount; this.parentVertexFormats = parentVertexFormats; elementHandler = handler; Parse(s); this.handler = handler; }
            public VertexDataList(EventHandler handler, IEnumerable<VertexDataElement> ilt, DependentList<VertexFormat> parentVertexFormats) : base(null) { this.parentVertexFormats = parentVertexFormats; elementHandler = handler; foreach (var t in ilt) this.Add(t); this.handler = handler; }
            #endregion

            protected override int ReadCount(Stream s) { return origCount; }
            protected override VertexDataElement CreateElement(Stream s) { return new VertexDataElement(0, elementHandler, s, parentVertexFormats); }

            protected override void WriteCount(Stream s, int count) { }
            protected override void WriteElement(Stream s, VertexDataElement element) { element.UnParse(s); }

            public override void Add() { this.Add(new VertexDataElement(0, elementHandler, parentVertexFormats)); }
        }
        #endregion

        public class Face : AHandlerElement, IEquatable<Face>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            ushort vertexDataIndex0;
            ushort vertexDataIndex1;
            ushort vertexDataIndex2;
            #endregion

            public Face(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Face(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Face(int APIversion, EventHandler handler, Face basis)
                : this(APIversion, handler, basis.vertexDataIndex0, basis.vertexDataIndex1, basis.vertexDataIndex2) { }
            public Face(int APIversion, EventHandler handler, ushort vertexDataIndex0, ushort vertexDataIndex1, ushort vertexDataIndex2)
                : base(APIversion, handler)
            {
                this.vertexDataIndex0 = vertexDataIndex0;
                this.vertexDataIndex1 = vertexDataIndex1;
                this.vertexDataIndex2 = vertexDataIndex2;
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                vertexDataIndex0 = r.ReadUInt16();
                vertexDataIndex1 = r.ReadUInt16();
                vertexDataIndex2 = r.ReadUInt16();
            }
            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(vertexDataIndex0);
                w.Write(vertexDataIndex1);
                w.Write(vertexDataIndex2);
            }

            #region AHandlerElement
            public override AHandlerElement Clone(EventHandler handler) { return new Face(requestedApiVersion, handler, this); }
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<VertexFormat>
            public bool Equals(Face other)
            {
                return this.vertexDataIndex0.Equals(other.vertexDataIndex0)
                    && this.vertexDataIndex1.Equals(other.vertexDataIndex1)
                    && this.vertexDataIndex2.Equals(other.vertexDataIndex2);
            }

            public override bool Equals(object obj) { return obj is VertexFormat && Equals(obj as VertexFormat); }

            public override int GetHashCode() { return vertexDataIndex0.GetHashCode() ^ vertexDataIndex1.GetHashCode() ^ vertexDataIndex2.GetHashCode(); }
            #endregion

            [ElementPriority(1)]
            public ushort VertexDataIndex0 { get { return vertexDataIndex0; } set { if (vertexDataIndex0 != value) { vertexDataIndex0 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ushort VertexDataIndex1 { get { return vertexDataIndex1; } set { if (vertexDataIndex1 != value) { vertexDataIndex1 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public ushort VertexDataIndex2 { get { return vertexDataIndex2; } set { if (vertexDataIndex2 != value) { vertexDataIndex2 = value; OnElementChanged(); } } }

            public string Value { get { return string.Join("; ", ValueBuilder.Split('\n')); } }
        }
        public class FaceList : DependentList<Face>
        {
            #region Constructors
            public FaceList(EventHandler handler) : base(handler) { }
            public FaceList(EventHandler handler, Stream s) : base(handler, s) {}
            public FaceList(EventHandler handler, IEnumerable<Face> le) : base(handler, le) { }
            #endregion

            protected override int ReadCount(Stream s) { return base.ReadCount(s) / 3; }
            protected override Face CreateElement(Stream s) { return new Face(0, elementHandler, s); }
            protected override void WriteCount(Stream s, int count) { base.WriteCount(s, (int)(count * 3)); }
            protected override void WriteElement(Stream s, Face element) { element.UnParse(s); }

            public override void Add() { this.Add(new Face(0, elementHandler)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public MATD.ShaderType Shader { get { return shader; } set { if (shader != value) { shader = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public MATD.MTNF Mtnf
        {
            get { return mtnf; }
            set { if ((shader == 0 && value != null) || (shader != 0 && value == null)) throw new ArgumentException(); if (!mtnf.Equals(value)) { mtnf = new MATD.MTNF(requestedApiVersion, OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(4)]
        public uint MergeGroup { get { return mergeGroup; } set { if (mergeGroup != value) { mergeGroup = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public uint SortOrder { get { return sortOrder; } set { if (sortOrder != value) { sortOrder = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(6)]
        public VertexFormatList VertexFormats
        {
            get { return vertexFormats; }
            set { if (!vertexFormats.Equals(value)) { vertexFormats = new VertexFormatList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(7)]
        public VertexDataList VertexData
        {
            get { return vertexData; }
            set { if (!vertexData.Equals(value)) { vertexData = new VertexDataList(OnRCOLChanged, value, this.vertexFormats); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(8)]
        public FaceList Faces
        {
            get { return faces; }
            set { if (!faces.Equals(value)) { faces = new FaceList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }
        [ElementPriority(9), TGIBlockListContentField("TGIBlocks")]
        public int SkinIndex { get { return skinIndex; } set { if (skinIndex != value) { skinIndex = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(10)]
        public UIntList BoneHashes
        {
            get { return boneHashes; }
            set { if (!boneHashes.Equals(value)) { boneHashes = new UIntList(OnRCOLChanged, value); OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public TGIBlockList TGIBlocks
        {
            get { return tgiBlockList; }
            set { if (!tgiBlockList.Equals(value)) { tgiBlockList = new TGIBlockList(OnRCOLChanged, value); /*hand me down*/ OnRCOLChanged(this, EventArgs.Empty); } }
        }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }
}