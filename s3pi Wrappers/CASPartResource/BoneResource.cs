/***************************************************************************
 *  Copyright (C) 2013 by Peter L Jones                                    *
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
using System.Linq;
using s3pi.Interfaces;

namespace CASPartResource
{
    public class BoneResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint version;
        BoneList bones;
        #endregion

        public BoneResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        private void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            version = br.ReadUInt32();

            string[] names = new string[br.ReadInt32()];
            for (int l = 0; l < names.Length; l++)
                names[l] = System.Text.BigEndianUnicodeString.Read(s);

            int i = br.ReadInt32();
            if (checking && i != names.Length)
                throw new InvalidDataException(String.Format("Unequal counts for bone names and matrices.  Bone name count {0}, matrix count {1}.  Position {2:X8}.",
                    names.Length, i, s.Position));

            Matrix4x3[] matrices = new Matrix4x3[i];
            for (int l = 0; l < matrices.Length; l++)
                matrices[l] = new Matrix4x3(0, null, s);

            bones = new BoneList(OnResourceChanged, names.Zip(matrices, (name, matrix) => new Bone(requestedApiVersion, null, name, matrix)));
        }

        protected override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(s);

            bw.Write(version);

            if (bones == null) bones = new BoneList(OnResourceChanged);

            bw.Write(bones.Count);
            foreach (var bone in bones)
                System.Text.BigEndianUnicodeString.Write(s, bone.Name);

            bw.Write(bones.Count);
            foreach (var bone in bones)
                bone.Matrix.UnParse(s);

            return s;
        }
        #endregion

        #region Sub-types
        public class MatrixRow : AHandlerElement, IEquatable<MatrixRow>
        {
            #region Attributes
            float v1;
            float v2;
            float v3;
            #endregion

            #region Constructors
            public MatrixRow(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public MatrixRow(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public MatrixRow(int APIversion, EventHandler handler, MatrixRow basis) : this(APIversion, handler, basis.v1, basis.v2, basis.v3) { }
            public MatrixRow(int APIversion, EventHandler handler, float v1, float v2, float v3) : base(APIversion, handler) { this.v1 = v1; this.v2 = v2; this.v3 = v3; }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader br = new BinaryReader(s);
                v1 = br.ReadSingle();
                v2 = br.ReadSingle();
                v3 = br.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter bw = new BinaryWriter(s);
                bw.Write(v1);
                bw.Write(v2);
                bw.Write(v3);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<MatrixRow> Members

            public bool Equals(MatrixRow other)
            {
                return
                    v1.Equals(other.v1)
                    && v2.Equals(other.v2)
                    && v3.Equals(other.v3)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as MatrixRow != null ? this.Equals(obj as MatrixRow) : false;
            }
            public override int GetHashCode()
            {
                return
                    v1.GetHashCode()
                    ^ v2.GetHashCode()
                    ^ v3.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public float V1 { get { return v1; } set { if (!v1.Equals(value)) { v1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float V2 { get { return v2; } set { if (!v2.Equals(value)) { v2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float V3 { get { return v3; } set { if (!v3.Equals(value)) { v3 = value; OnElementChanged(); } } }

            public string Value { get { return "{ " + ValueBuilder.Replace("\n", "; ") + " }"; } }
            #endregion
        }

        public class Matrix4x3 : AHandlerElement, IEquatable<Matrix4x3>
        {
            #region Attributes
            MatrixRow row1;
            MatrixRow row2;
            MatrixRow row3;
            MatrixRow row4;
            #endregion

            #region Constructors
            public Matrix4x3(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Matrix4x3(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public Matrix4x3(int APIversion, EventHandler handler, Matrix4x3 basis) : this(APIversion, handler, basis.row1, basis.row2, basis.row3, basis.row4) { }
            public Matrix4x3(int APIversion, EventHandler handler, MatrixRow row1, MatrixRow row2, MatrixRow row3, MatrixRow row4) : base(APIversion, handler)
            {
                this.row1 = new MatrixRow(requestedApiVersion, handler, row1);
                this.row2 = new MatrixRow(requestedApiVersion, handler, row2);
                this.row3 = new MatrixRow(requestedApiVersion, handler, row3);
                this.row4 = new MatrixRow(requestedApiVersion, handler, row4);
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                row1 = new MatrixRow(requestedApiVersion, handler, s);
                row2 = new MatrixRow(requestedApiVersion, handler, s);
                row3 = new MatrixRow(requestedApiVersion, handler, s);
                row4 = new MatrixRow(requestedApiVersion, handler, s);
            }

            internal void UnParse(Stream s)
            {
                row1.UnParse(s);
                row2.UnParse(s);
                row3.UnParse(s);
                row4.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Matrix4x3> Members

            public bool Equals(Matrix4x3 other)
            {
                return
                    row1.Equals(other.row1)
                    && row2.Equals(other.row2)
                    && row3.Equals(other.row3)
                    && row4.Equals(other.row4)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as Matrix4x3 != null ? this.Equals(obj as Matrix4x3) : false;
            }
            public override int GetHashCode()
            {
                return
                    row1.GetHashCode()
                    ^ row2.GetHashCode()
                    ^ row3.GetHashCode()
                    ^ row4.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public MatrixRow Row1 { get { return row1; } set { if (!row1.Equals(value)) { row1 =new MatrixRow(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public MatrixRow Row2 { get { return row2; } set { if (!row2.Equals(value)) { row2 = new MatrixRow(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public MatrixRow Row3 { get { return row3; } set { if (!row3.Equals(value)) { row3 = new MatrixRow(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public MatrixRow Row4 { get { return row4; } set { if (!row4.Equals(value)) { row4 = new MatrixRow(requestedApiVersion, handler, value); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class Bone : AHandlerElement, IEquatable<Bone>
        {
            #region Attributes
            string name;
            Matrix4x3 matrix;
            #endregion

            #region Constructors
            public Bone(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public Bone(int APIversion, EventHandler handler, Bone basis) : this(APIversion, handler, basis.name, basis.matrix) { }
            public Bone(int APIversion, EventHandler handler, string name, Matrix4x3 matrix) : base(APIversion, handler) { this.name = name; this.matrix = matrix; }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Bone> Members

            public bool Equals(Bone other)
            {
                return
                    name.Equals(other.name)
                    && matrix.Equals(other.matrix)
                    ;
            }
            public override bool Equals(object obj)
            {
                return obj as MatrixRow != null ? this.Equals(obj as MatrixRow) : false;
            }
            public override int GetHashCode()
            {
                return
                    name.GetHashCode()
                    ^ matrix.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public string Name { get { return name; } set { if (!name.Equals(value)) { name = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public Matrix4x3 Matrix { get { return matrix; } set { if (!matrix.Equals(value)) { matrix = new Matrix4x3(requestedApiVersion, handler, value); OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
            #endregion
        }

        public class BoneList : DependentList<Bone>
        {
            #region Constructors
            public BoneList(EventHandler handler) : base(handler) { }
            public BoneList(EventHandler handler, IEnumerable<Bone> le) : base(handler, le) { }
            #endregion

            #region Data I/O (or not)
            protected override int ReadCount(Stream s) { throw new InvalidOperationException("BoneList cannot be automatically parsed."); }
            protected override Bone CreateElement(Stream s) { throw new InvalidOperationException("BoneList cannot be automatically parsed."); }
            protected override void WriteCount(Stream s, int count) { throw new InvalidOperationException("BoneList cannot be automatically un-parsed."); }
            protected override void WriteElement(Stream s, Bone element) { throw new InvalidOperationException("BoneList cannot be automatically un-parsed."); }
            #endregion
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (!version.Equals(value)) { version = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(2)]
        public BoneList Bones { get { return bones; } set { if (!bones.Equals(value)) { bones = new BoneList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for BlendGeometryResource wrapper
    /// </summary>
    public class BoneResourceResourceHandler : AResourceHandler
    {
        public BoneResourceResourceHandler()
        {
            this.Add(typeof(BoneResource), new List<string>(new string[] { "0x00AE6C67" }));
        }
    }
}