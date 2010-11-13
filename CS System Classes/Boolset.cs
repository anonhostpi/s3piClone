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

namespace System
{
    /// <summary>
    /// A Boolset provides an easy way to handle (unsigned) integer types as a set of bitwise flags.
    /// </summary>
    public class Boolset : IEquatable<Boolset>, IEquatable<ulong>, IEquatable<string>,
        IEqualityComparer<Boolset>, IEqualityComparer<ulong>, IEqualityComparer<string>,
        IEnumerable<bool>
    {
        private bool[] bitset = null;

        private Boolset(int size, ulong val)
        {
            bitset = new bool[size];
            for (int i = 0; i < size; i++)
                bitset[i] = (val & ((ulong)1 << i)) != 0;
        }

        public Boolset(ulong val) : this(64, val) { }

        public Boolset(uint val) : this(32, val) { }

        public Boolset(ushort val) : this(16, val) { }

        public Boolset(byte val) : this(8, val) { }

        public Boolset(string val)
        {
            bitset = new bool[val.Length];
            int j = 0;
            for (int i = val.Length - 1; i >= 0; i--)
                bitset[j++] = !val.Substring(i, 1).Equals("0");
        }


        public static implicit operator Boolset(ulong o) { return new Boolset(o); }

        public static implicit operator Boolset(uint o) { return new Boolset(o); }

        public static implicit operator Boolset(ushort o) { return new Boolset(o); }

        public static implicit operator Boolset(byte o) { return new Boolset(o); }

        public static implicit operator Boolset(string o) { return new Boolset(o); }


        private static ulong doOperator(Boolset t, int l)
        {
            ulong val = 0;
            for (int i = 0; i < l && i < t.bitset.Length; i++)
                val += (ulong)(t[i] ? 1 : 0) << i;
            return val;
        }

        public static implicit operator ulong(Boolset t) { return (ulong)doOperator(t, 64); }

        public static implicit operator uint(Boolset t) { return (uint)doOperator(t, 32); }

        public static implicit operator ushort(Boolset t) { return (ushort)doOperator(t, 16); }

        public static implicit operator byte(Boolset t) { return (byte)doOperator(t, 8); }

        public static implicit operator string(Boolset t)
        {
            string s = "";
            for (int i = 0; i < t.bitset.Length; i++)
                s = (t.bitset[i] ? "1" : "0") + s;
            return s;
        }


        public override string ToString() { return this; }


        public event EventHandler BoolsetChanged;
        protected virtual void OnBoolsetChanged(object sender, EventArgs e) { if (BoolsetChanged != null) BoolsetChanged(sender, e); }

        public bool this[int i]
        {
            get
            {
                if (i > bitset.Length)
                    throw new ArgumentOutOfRangeException();
                return bitset[i];
            }

            set
            {
                if (i > bitset.Length)
                    throw new ArgumentOutOfRangeException();
                if (bitset[i] == value) return;
                bitset[i] = value;
                OnBoolsetChanged(this, new EventArgs());
                /*
                 *   set: val |= 1 << bit;
                 * clear: val -= (val & (1 << bit))
                 */
            }

        }

        public int Length { get { return bitset.Length; } }

        public bool Matches(string mask)
        {
            // right-hand end of mask is low end of bitset
            int mcnt = mask.Length - 1;
            bool matched = true;
            int i = 0;
            while (matched && mcnt > 0 && i < bitset.Length)
            {
                if (mask[mcnt].Equals('0'))
                    matched = !bitset[i];
                else if (mask[mcnt].Equals('1'))
                    matched = bitset[i];
                mcnt--;
                i++;
            }
            return matched;
        }

        public void flip(string bits)
        {
            if (bits.Length > bitset.Length) throw new ArgumentOutOfRangeException();
            for (int i = 0; i < bits.Length; i++) if (!bits[i].Equals("0")) flip(i);
        }
        
        public void flip(int[] bits) { foreach (int bit in bits) flip(bit); }

        public void flip(int bit) { bitset[bit] = !bitset[bit]; }

        #region IEquatable<*> Members

        public bool Equals(Boolset other) { return ((ulong)this).Equals((ulong)other); }

        public bool Equals(ulong other) { return ((ulong)this).Equals(other); }

        public bool Equals(string other) { return ((Boolset)this).Equals((Boolset)other); }

        #endregion

        #region IEqualityComparer<*> Members

        public bool Equals(Boolset x, Boolset y) { return x.Equals(y); }

        public int GetHashCode(Boolset obj) { return ((ulong)obj).GetHashCode(); }

        public bool Equals(ulong x, ulong y) { return x.Equals(y); }

        public int GetHashCode(ulong obj) { return obj.GetHashCode(); }

        public bool Equals(string x, string y) { return ((Boolset)x).Equals((Boolset)y); }

        public int GetHashCode(string obj) { return ((Boolset)obj).GetHashCode(); }

        #endregion

        #region IEnumerable<bool> Members

        public IEnumerator<bool> GetEnumerator() { return (IEnumerator<bool>)bitset.GetEnumerator(); }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return bitset.GetEnumerator(); }

        #endregion
    }
}
