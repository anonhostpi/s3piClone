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

namespace System
{
    /// <summary>
    /// Defines an object as having a parent object that can be changed on Clone()
    /// </summary>
    public interface ICloneableWithParent : ICloneable
    {
        /// <summary>
        /// Return a clone of the object with a new parent object.
        /// </summary>
        /// <param name="newParent">The new parent object</param>
        /// <returns>A clone of the object with <paramref name="newParent"/> as its new parent object.</returns>
        object Clone(object newParent);
    }
}
