/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  Based on an idea by atavera                                            *
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

namespace s3pi.Extensions
{
    public class ExtList : Dictionary<string, List<string>>
    {
        static ExtList e = null;
        static ExtList() { e = new ExtList(); }
        public static ExtList Ext { get { return e; } }

        ExtList()
        {
            string path = Path.GetDirectoryName(typeof(ExtList).Assembly.Location);
            StreamReader sr = new StreamReader(Path.Combine(path, "Extensions.txt"));
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                if (s.StartsWith(";")) continue;
                List<string> t = new List<string>(s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                if (t.Count < 2) continue;
                string t0 = t[0];
                t.RemoveAt(0);
                this.Add(t0, t);
            }
        }
    }
}
