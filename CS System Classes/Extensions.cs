/***************************************************************************
 *  Copyright (C) 2011 by Peter L Jones                                    *
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
using System.Collections;

namespace System
{
    /// <summary>
    /// Useful Extension Methods not provided by Linq (and without deferred execution).
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert all elements of an <c>Array</c> to <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TOut">The output element type.</typeparam>
        /// <param name="array">The input array</param>
        /// <returns>An <c>TOut[]</c> array containing converted input elements.</returns>
        /// <exception cref="InvalidCastException">The element type of <paramref name="array"/> does not provide the <c>IConvertible</c> interface.</exception>
        public static TOut[] Cast<TOut>(this Array array) { return array.Cast<TOut>(0, array.Length); }
        /// <summary>
        /// Convert elements of an <c>Array</c> to <typeparamref name="TOut"/>,
        /// starting at <paramref name="start"/>.
        /// </summary>
        /// <typeparam name="TOut">The output element type.</typeparam>
        /// <param name="array">The input array</param>
        /// <param name="start">The offset into <paramref name="array"/> from which to start creating the output.</param>
        /// <returns>An <c>TOut[]</c> array containing converted input elements.</returns>
        /// <exception cref="InvalidCastException">The element type of <paramref name="array"/> does not provide the <c>IConvertible</c> interface.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="start"/> is outside the bounds of <paramref name="array"/>.</exception>
        public static TOut[] Cast<TOut>(this Array array, int start) { return array.Cast<TOut>(start, array.Length - start); }
        /// <summary>
        /// Convert elements of an <c>Array</c> to <typeparamref name="TOut"/>,
        /// starting at <paramref name="start"/> for <paramref name="length"/> elements.
        /// </summary>
        /// <typeparam name="TOut">The output element type.</typeparam>
        /// <param name="array">The input array</param>
        /// <param name="start">The offset into <paramref name="array"/> from which to start creating the output.</param>
        /// <param name="length">The number of elements in the output.</param>
        /// <returns>An <c>TOut[]</c> array containing converted input elements.</returns>
        /// <exception cref="InvalidCastException">The element type of <paramref name="array"/> does not provide the <c>IConvertible</c> interface.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="start"/> is outside the bounds of <paramref name="array"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> has an invalid value.</exception>
        public static TOut[] Cast<TOut>(this Array array, int start, int length)
        {
            if (!typeof(IConvertible).IsAssignableFrom(array.GetType().GetElementType()))
                throw new InvalidCastException(array.GetType().GetElementType().Name + " is not IConvertible");

            if (start > array.Length)
                throw new IndexOutOfRangeException("'start' exceeds array length");

            if (length > array.Length - start)
                throw new ArgumentException("'length' exceeds constrained element count");

            TOut[] res = new TOut[length];

            for (int i = 0; i < res.Length; i++)
                res[i] = (TOut)System.Convert.ChangeType(((IList)array)[i + start], typeof(TOut));

            return res;
        }
    }
}
