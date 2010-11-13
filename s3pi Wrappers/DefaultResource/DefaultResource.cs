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

namespace s3pi.DefaultResource
{
    /// <summary>
    /// A resource wrapper that "does nothing"
    /// </summary>
    public class DefaultResource : AResource
    {
        const Int32 recommendedApiVersion = 1;

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        #endregion

        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public DefaultResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = new MemoryStream(); dirty = true; } }

        protected override Stream UnParse() { throw new NotImplementedException(); }

        /// <summary>
        /// The resource content as a Stream
        /// </summary>
        public override Stream Stream { get { stream.Position = 0; return stream; } }
    }

    /// <summary>
    /// ResourceHandler for DefaultResource wrapper
    /// </summary>
    public class DefaultResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary
        /// "*" is used by WrapperDealer to indicate the "default wrapper" - i.e. one that will happily wrap any resource
        /// </summary>
        public DefaultResourceHandler()
        {
            this.Add(typeof(DefaultResource), new List<string>(new string[] { "*" }));
        }
    }
}
