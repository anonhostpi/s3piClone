/***************************************************************************
 *  Copyright (C) 2010 by Peter L Jones                                    *
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

namespace CASPartResource
{
    [Flags]
    public enum FacialRegionFlags : uint
    {
        None = 0x000,
        Eyes = 0x001,
        Nose = 0x002,
        Mouth = 0x004,
        TranslateMouth = 0x008,
        Ears = 0x010,
        TranslateEyes = 0x020,
        Face = 0x040,
        Head = 0x080,
        Brow = 0x100,
        Jaw = 0x200,
        Body = 0x400,
        Eyelashes = 0x800,
    }

    [Flags]
    public enum AgeGenderFlags : uint
    {
        None = 0x00000000,

        Baby = 0x00000001,
        Toddler = 0x00000002,
        Child = 0x00000004,
        Teen = 0x00000008,

        YoungAdult = 0x00000010,
        Adult = 0x00000020,
        Elder = 0x00000040,
        //AgeMask=0x0000007F,
        Unknown07 = 0x00000080,

        Unknown08 = 0x00000100,
        Unknown09 = 0x00000200,
        Unknown0A = 0x00000400,
        Unknown0B = 0x00000800,

        Male = 0x00001000,
        Female = 0x00002000,
        //GenderMask=0x00003000,
        Unknown0E = 0x00004000,
        Unknown0F = 0x00008000,

        Unknown10 = 0x00010000,
        Unknown11 = 0x00020000,
        Unknown12 = 0x00040000,
        Unknown13 = 0x00080000,

        Unknown14 = 0x00100000,
        Unknown15 = 0x00200000,
        Unknown16 = 0x00400000,
        Unknown17 = 0x00800000,

        Unknown18 = 0x01000000,
        Unknown19 = 0x02000000,
        Unknown1A = 0x04000000,
        Unknown1B = 0x08000000,

        Unknown1C = 0x10000000,
        Unknown1D = 0x20000000,
        Unknown1E = 0x40000000,
        Unknown1F = 0x80000000,
    }

    [Flags]
    public enum DataTypeFlags : uint
    {
        Hair = 0x00000001,
        Scalp = 0x00000002,
        FaceOverlay = 0x00000004,
        Body = 0x00000008,
        Accessory = 0x00000010,
    }

    [Flags]
    public enum ClothingCategoryFlags : uint
    {
        None = 0x00000000,
        Naked = 0x00000001,
        Everyday = 0x00000002,
        Formalwear = 0x00000004,
        Sleepwear = 0x00000008,
        Swimwear = 0x00000010,
        Athletic = 0x00000020,
        Singed = 0x00000040,
        Career = 0x00000100,
        //CategoryMask = 0x000007FF,
        //All = 0x0000FFFF,

        ValidForRandom = 0x00200000,
        IsHat = 0x00400000,
        ValidForMaternity = 0x00800000,
        IsHiddenInCAS = 0x01000000,
    }

    [Flags]
    public enum UnknownFlags : uint
    {
        None = 0x00000000,

        Unknown00 = 0x00000001,
        Unknown01 = 0x00000002,
        Unknown02 = 0x00000004,
        Unknown03 = 0x00000008,

        Unknown04 = 0x00000010,
        Unknown05 = 0x00000020,
        Unknown06 = 0x00000040,
        Unknown07 = 0x00000080,

        Unknown08 = 0x00000100,
        Unknown09 = 0x00000200,
        Unknown0A = 0x00000400,
        Unknown0B = 0x00000800,

        Unknown0C = 0x00001000,
        Unknown0D = 0x00002000,
        Unknown0E = 0x00004000,
        Unknown0F = 0x00008000,

        Unknown10 = 0x00010000,
        Unknown11 = 0x00020000,
        Unknown12 = 0x00040000,
        Unknown13 = 0x00080000,

        Unknown14 = 0x00100000,
        Unknown15 = 0x00200000,
        Unknown16 = 0x00400000,
        Unknown17 = 0x00800000,

        Unknown18 = 0x01000000,
        Unknown19 = 0x02000000,
        Unknown1A = 0x04000000,
        Unknown1B = 0x08000000,

        Unknown1C = 0x10000000,
        Unknown1D = 0x20000000,
        Unknown1E = 0x40000000,
        Unknown1F = 0x80000000,
    }

    public enum ClothingType : uint
    {
        Hair = 1,
        Scalp = 2,
        Face = 3,
        Body = 4,
        Top = 5,
        Bottom = 6,
        Shoes = 7,
        Earrings = 11,
        GlassesF = 12,
        Bracelets = 13,
        RingL = 14,
        RingR = 15,
        Beard = 16,
        Lipstick = 17,
        Eyeshadow = 18,
        Eyeliner = 19,
        Blush = 20,
        Makeup = 21,
        Eyebrow = 22,
        Glove = 24,
        Socks = 25,
        Mascara = 26,
        Weathering = 29,
        EarringL = 30,
        EarringR = 31,
    }
}