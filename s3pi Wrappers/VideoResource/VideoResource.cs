/***************************************************************************
 *  Copyright (C) 2012 by Peter L Jones                                    *
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
using VideoResource.Properties;

namespace VideoResource
{
    public class VideoResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint fourCC = (uint)FOURCC("SCHl");
        byte[] preamble;
        byte[] video;
        #endregion

        public VideoResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        private void Parse(System.IO.Stream stream)
        {
            BinaryReader r = new BinaryReader(stream);

            fourCC = r.ReadUInt32();
            if (FOURCC(fourCC) != "SCHl")
                throw new InvalidDataException(String.Format("Unexpected FOURCC '{0}' found.  Expected 'SCHl'.", FOURCC(fourCC)));

            int posn = r.ReadInt32();
            preamble = r.ReadBytes(posn - (sizeof(uint) + sizeof(int)));

            long i = stream.Length - stream.Position;
            video = r.ReadBytes((int)i);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(fourCC);

            if (preamble == null) preamble = new byte[0];
            w.Write(preamble.Length + (sizeof(uint) + sizeof(int)));
            w.Write(preamble);

            if (video == null) video = new byte[0];
            w.Write(video);

            return ms;
       }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint FourCC { get { return fourCC; } set { if (fourCC != value) { fourCC = value; OnResourceChanged(this, new EventArgs()); } } }
        [ElementPriority(2)]
        public BinaryReader Preamble
        {
            get { return new BinaryReader(new MemoryStream(preamble)); }
            set
            {
                if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; preamble = value.ReadBytes((int)value.BaseStream.Length); }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024 * 1024];
                    for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                        ms.Write(buffer, 0, read);
                    preamble = ms.ToArray();
                }
                OnResourceChanged(this, new EventArgs());
            }
        }
        [ElementPriority(3)]
        public BinaryReader Video
        {
            get { return new BinaryReader(new MemoryStream(video)); }
            set
            {
                if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; video = value.ReadBytes((int)value.BaseStream.Length); }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024 * 1024];
                    for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                        ms.Write(buffer, 0, read);
                    video = ms.ToArray();
                }
                OnResourceChanged(this, new EventArgs());
            }
        }

        protected override List<string> ValueBuilderFields
        {
            get
            {
                var fields = base.ValueBuilderFields;
                fields.Remove("Preamble");
                fields.Remove("Video");
                return fields;
            }
        }

        public string Value { get { return ValueBuilder; } }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for VideoResource wrapper
    /// </summary>
    public class VideoResourceHandler : AResourceHandler
    {
        public VideoResourceHandler()
        {
            this.Add(typeof(VideoResource), new List<string>(new string[] { "0xB1CC1AF6", }));
        }
    }
}
