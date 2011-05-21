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

namespace s3pi.Interfaces
{
    public abstract class ARCOLBlock : AHandlerElement, IRCOLBlock, IEquatable<ARCOLBlock>
    {
        const int recommendedApiVersion = 1;

        protected int requestedAPIversion;
        protected Stream stream;

        public ARCOLBlock(int APIversion, EventHandler handler, Stream s)
            : base(APIversion, handler)
        {
            stream = s;
            if (stream == null) { stream = UnParse(); OnElementChanged(); }
            stream.Position = 0;
            Parse(stream);
        }

        protected abstract void Parse(Stream s);

        #region AApiVersionedFields
        static List<string> ARCOLBlockBanlist = new List<string>(new string[] {
            "Tag", "ResourceType", "Data",
        });
        protected override List<string> ValueBuilderFields
        {
            get
            {
                List<string> fields = base.ValueBuilderFields;
                fields.RemoveAll(ARCOLBlockBanlist.Contains);
                return fields;
            }
        }
        #endregion

        #region AHandlerElement
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        public abstract override AHandlerElement Clone(EventHandler handler);
        #endregion

        #region IRCOLBlock Members
        [ElementPriority(2)]
        public abstract string Tag { get; }
        [ElementPriority(3)]
        public abstract uint ResourceType { get; }
        public abstract Stream UnParse();
        #endregion

        #region IResource Members
        /// <summary>
        /// The resource content as a Stream
        /// </summary>
        public virtual Stream Stream
        {
            get
            {
                if (dirty || s3pi.Settings.Settings.AsBytesWorkaround)
                {
                    stream = UnParse();
                    dirty = false;
                    //Console.WriteLine(this.GetType().Name + " flushed.");
                }
                stream.Position = 0;
                return stream;
            }
        }

        /// <summary>
        /// The resource content as a byte array
        /// </summary>
        [ElementPriority(0)]
        public virtual byte[] AsBytes
        {
            get
            {
                MemoryStream s = this.Stream as MemoryStream;
                if (s != null) return s.ToArray();

                stream.Position = 0;
                return (new BinaryReader(stream)).ReadBytes((int)stream.Length);
            }
            set { MemoryStream ms = new MemoryStream(value); Parse(ms); OnRCOLChanged(this, EventArgs.Empty); }
        }

        //disable "Never used" warning, as this is used by library users rather than the library itself.
#pragma warning disable 67
        /// <summary>
        /// Raised if the resource is changed
        /// </summary>
        public event EventHandler ResourceChanged;
#pragma warning restore 67

        #endregion

        #region IEquatable<ARCOLBlock> Members

        public virtual bool Equals(ARCOLBlock other) { return this.AsBytes.Equals<byte>(other.AsBytes); }

        #endregion

        /// <summary>
        /// Used to indicate the RCOL has changed
        /// </summary>
        protected virtual void OnRCOLChanged(object sender, EventArgs e) { OnElementChanged(); }

        /// <summary>
        /// To allow editor import/export
        /// </summary>
        [ElementPriority(1)]
        public virtual BinaryReader Data
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
                OnRCOLChanged(this, EventArgs.Empty);
            }
        }
    }
}
