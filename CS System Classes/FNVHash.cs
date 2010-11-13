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

namespace System.Security.Cryptography
{
    /// <summary>
    /// Base class implementing <see cref="System.Security.Cryptography.HashAlgorithm"/>.
    /// For full documentation, refer to http://www.sims2wiki.info/wiki.php?title=FNV
    /// </summary>
    public abstract class FNVHash : HashAlgorithm
    {
        ulong prime;
        ulong offset;
        /// <summary>
        /// Algorithm result, needs casting to appropriate size by concrete classes (because I'm lazy)
        /// </summary>
        protected ulong hash;
        /// <summary>
        /// Initialise the hash algorithm
        /// </summary>
        /// <param name="prime">algorithm-specific value</param>
        /// <param name="offset">algorithm-specific value</param>
        protected FNVHash(ulong prime, ulong offset) { this.prime = prime; this.offset = offset; hash = offset; }

        /// <summary>
        /// Method for hashing a string
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>FNV hash of string</returns>
        public byte[] ComputeHash(string value) { return this.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(value.ToLowerInvariant())); }

        /// <summary>
        /// Nothing to initialize
        /// </summary>
        public override void Initialize() { }

        /// <summary>
        /// Implements the algorithm
        /// </summary>
        /// <param name="array">The input to compute the hash code for.</param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (int i = ibStart; i < ibStart + cbSize; i++) { hash *= prime; hash ^= array[i]; }
        }

        /// <summary>
        /// Returns the computed hash code.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal() { HashValue = BitConverter.GetBytes(hash); return HashValue; }
    }

    /// <summary>
    /// FNV32 hash routine
    /// </summary>
    public class FNV32 : FNVHash
    {
        /// <summary>
        /// Initialise the hash algorithm
        /// </summary>
        public FNV32() : base((uint)0x01000193, (uint)0x811C9DC5) { }
        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
        public override byte[] Hash { get { return BitConverter.GetBytes((uint)hash); } }
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize { get { return 32; } }
        /// <summary>
        /// Get the FNV32 hash for a string of text
        /// </summary>
        /// <param name="text">the text to get the hash for</param>
        /// <returns>the hash value</returns>
        public static uint GetHash(string text) { return BitConverter.ToUInt32(new System.Security.Cryptography.FNV32().ComputeHash(text), 0); }
    }

    /// <summary>
    /// FNV64 hash routine
    /// </summary>
    public class FNV64 : FNVHash
    {
        /// <summary>
        /// Initialise the hash algorithm
        /// </summary>
        public FNV64() : base((ulong)0x00000100000001B3, (ulong)0xCBF29CE484222325) { }
        /// <summary>
        /// Gets the value of the computed hash code.
        /// </summary>
        public override byte[] Hash { get { return BitConverter.GetBytes(hash); } }
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize { get { return 64; } }
        /// <summary>
        /// Get the FNV64 hash for a string of text
        /// </summary>
        /// <param name="text">the text to get the hash for</param>
        /// <returns>the hash value</returns>
        public static ulong GetHash(string text) { return BitConverter.ToUInt64(new System.Security.Cryptography.FNV64().ComputeHash(text), 0); }
    }
}
