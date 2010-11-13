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

namespace System.Collections.Generic
{
#pragma warning disable 1591
    [Obsolete()]
    public abstract class ADependentList<T, U> : List<T>, IDependentList<T, U>
    {
        #region Attributes
        protected U parent;
        protected long maxSize = -1;
        #endregion

        #region Constructors
        protected ADependentList(U parent) : base() { this.parent = parent; }
        protected ADependentList(U parent, IList<T> lt) : base(lt) { this.parent = parent; }
        protected ADependentList(U parent, long size) : base() { this.parent = parent; maxSize = size; }
        protected ADependentList(U parent, long size, IList<T> lt) : base(lt)
        {
            if (size >= 0 && lt.Count > size) throw new ArgumentOutOfRangeException("lt", "Size of list supplied must not exceed maximum list size supplied.");
            this.parent = parent; maxSize = size;
        }
        #endregion

        #region ICloneableWithParent Members

        /// <summary>
        /// Creates a copy of the list with the given parent
        /// </summary>
        /// <returns>Object of the same type as the list</returns>
        public object Clone(object newParent) { return Clone((U)newParent); }

        /// <summary>
        /// Creates a copy of the list with the given parent
        /// </summary>
        /// <returns>Object of the same type as the list</returns>
        public abstract object Clone(U newParent);

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a copy of the list with the same parent
        /// </summary>
        /// <returns>Object of the same type as the list</returns>
        public object Clone() { return Clone(parent); }

        #endregion

        #region IList<T> Members
        public virtual new void Insert(int index, T item) { if (maxSize >= 0 && Count == maxSize) throw new InvalidOperationException(); base.Insert(index, item); }
        #endregion

        #region ICollection<T> Members
        public virtual new void Add(T item) { if (maxSize >= 0 && Count == maxSize) throw new InvalidOperationException(); base.Add(item); }
        #endregion

        public long MaxSize { get { return maxSize; } }
    }
#pragma warning restore 1591
}
