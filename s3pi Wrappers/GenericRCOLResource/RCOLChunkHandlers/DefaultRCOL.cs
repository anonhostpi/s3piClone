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

namespace s3pi.GenericRCOLResource
{
    public class DefaultRCOL : ARCOLBlock
    {
        byte[] data = new byte[0];

        // This ARCOLBlock does not support CreateRCOLBlock
        public DefaultRCOL(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public DefaultRCOL(int APIversion, EventHandler handler, DefaultRCOL basis) : base(APIversion, null, null) { this.handler = handler; data = (byte[])basis.data.Clone(); }

        protected override void Parse(System.IO.Stream s) { data = new byte[s.Length]; s.Read(data, 0, (int)s.Length); }

        public override AHandlerElement Clone(EventHandler handler) { return new DefaultRCOL(requestedApiVersion, handler, this); }

        [ElementPriority(2)]
        public override string Tag { get { return "*"; } } // For RCOLDealer

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0xFFFFFFFF; } }

        public override System.IO.Stream UnParse() { MemoryStream ms = new MemoryStream(); ms.Write(data, 0, data.Length); return ms; }

        public string Value { get { return "Tag: " + FOURCC(BitConverter.ToUInt32(data, 0)) + "\nLength: " + data.Length; } }
    }
}
