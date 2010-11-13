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

namespace s3pi.Interfaces
{
    /// <summary>
    /// Strings used in a number of places in the game.
    /// </summary>
    /// <remarks>Currently only in <c>CatalogResource</c>.</remarks>
    public static class StringTableSingleton
    {
        static string[] stringTable = null;
        /// <summary>
        /// Table of defined strings.
        /// </summary>
        public static String[] Table
        {
            get
            {
                if (stringTable == null) stringTable = new string[] {
                    /* 00 */  ""
                    /* 01 */, "filename"
                    /* 02 */, "X:"
                    /* 03 */, "-1"
                    /* 04 */, "assetRoot"
                    /* 05 */, "daeFileName"
                    /* 06 */, "daeFilePath"
                    /* 07 */, "Color"
                    /* 08 */, "ObjectRgbMask"
                    /* 09 */, "rgbmask"
                    /* 0a */, "specmap"
                    /* 0b */, "Background Image"
                    /* 0c */, "HSVShift Bg"
                    /* 0d */, "H Bg"
                    /* 0e */, "V Bg"
                    /* 0f */, "S Bg"
                    /* 10 */, "Base H Bg"
                    /* 11 */, "Base V Bg"
                    /* 12 */, "Base S Bg"
                    /* 13 */, "Mask"
                    /* 14 */, "Multiplier"
                    /* 15 */, "Dirt Layer"
                    /* 16 */, "1X Multiplier"
                    /* 17 */, "Specular"
                    /* 18 */, "Overlay"
                    /* 19 */, "Face"
                    /* 1a */, "partType"
                    /* 1b */, "gender"
                    /* 1c */, "bodyType"
                    /* 1d */, "age"
                    /* 1e */, "A"
                    /* 1f */, "M"
                    /* 20 */, "Stencil A"
                    /* 21 */, "Stencil B"
                    /* 22 */, "Stencil C"
                    /* 23 */, "Stencil D"
                    /* 24 */, "Stencil A Enabled"
                    /* 25 */, "Stencil B Enabled"
                    /* 26 */, "Stencil C Enabled"
                    /* 27 */, "Stencil D Enabled"
                    /* 28 */, "Stencil A Tiling"
                    /* 29 */, "Stencil B Tiling"
                    /* 2a */, "Stencil C Tiling"
                    /* 2b */, "Stencil D Tiling"
                    /* 2c */, "Stencil A Rotation"
                    /* 2d */, "Stencil B Rotation"
                    /* 2e */, "Stencil C Rotation"
                    /* 2f */, "Stencil D Rotation"
                    /* 30 */, "Pattern A"
                    /* 31 */, "Pattern B"
                    /* 32 */, "Pattern C"
                    /* 33 */, "Pattern A Enabled"
                    /* 34 */, "Pattern B Enabled"
                    /* 35 */, "Pattern C Enabled"
                    /* 36 */, "Pattern A Linked"
                    /* 37 */, "Pattern B Linked"
                    /* 38 */, "Pattern C Linked"
                    /* 39 */, "Pattern A Rotation"
                    /* 3a */, "Pattern B Rotation"
                    /* 3b */, "Pattern C Rotation"
                    /* 3c */, "Pattern A Tiling"
                    /* 3d */, "Pattern B Tiling"
                    /* 3e */, "Pattern C Tiling"
                    /* 40 */, ""
                    /* 41 */, "MaskWidth"
                    /* 42 */, "MaskHeight"
                    /* 43 */, "ObjectRgbaMask"
                    /* 44 */, "RndColors"
                    /* 45 */, "Flat Color"
                    /* 46 */, "Alpha"
                    /* 47 */, "Color 0"
                    /* 48 */, "Color 1"
                    /* 49 */, "Color 2"
                    /* 4a */, "Color 3"
                    /* 4b */, "Color 4"
                    /* 4c */, "Channel 1"
                    /* 4d */, "Channel 2"
                    /* 4e */, "Channel 3"
                    /* 4f */, "Pattern D"
                    /* 50 */, "Pattern D Tiling"
                    /* 51 */, "Pattern D Enabled"
                    /* 52 */, "Pattern D Linked"
                    /* 53 */, "Pattern D Rotation"
                    /* 54 */, "HSVShift 1"
                    /* 55 */, "HSVShift 2"
                    /* 56 */, "HSVShift 3"
                    /* 57 */, "Channel 1 Enabled"
                    /* 58 */, "Channel 2 Enabled"
                    /* 59 */, "Channel 3 Enabled"
                    /* 5a */, "Base H 1"
                    /* 5b */, "Base V 1"
                    /* 5c */, "Base S 1"
                    /* 5d */, "Base H 2"
                    /* 5e */, "Base V 2"
                    /* 5f */, "Base S 2"
                    /* 60 */, "Base H 3"
                    /* 61 */, "Base V 3"
                    /* 62 */, "Base S 3"
                    /* 63 */, "H 1"
                    /* 64 */, "S 1"
                    /* 65 */, "V 1"
                    /* 66 */, "H 2"
                    /* 67 */, "S 2"
                    /* 68 */, "V 2"
                    /* 69 */, "H 3"
                    /* 6a */, "V 3"
                    /* 6b */, "S 3"
                    /* 6c */, "true"
                    /* 6d */, "1,0,0,0"
                    /* 6e */, "defaultFlatColor"
                    /* 6f */, "solidColor_1"
                };
                return stringTable;
            }
        }
    }
}
