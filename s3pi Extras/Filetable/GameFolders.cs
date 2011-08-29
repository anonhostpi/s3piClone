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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace s3pi.Filetable
{
    /// <summary>
    /// Provides access to the XML definition of game folder default locations and
    /// the resultant list of known games.
    /// </summary>
    public static class GameFolders
    {
        #region The first level element/attribute names...
        static readonly string ns = "{http://sims3.drealm.info/terms/gamefolders/1.0}";
        static readonly string docXName = ns + "gamefolders";
        static readonly string defaultrootfolderXName = ns + "defaultrootfolder";
        static readonly string rootfolderXName = ns + "rootfolder";
        static readonly string vendorAttributeName = "vendor";
        static readonly string osAttributeName = "os";
        static readonly string gameXName = ns + "game";
        #endregion

        static XDocument _gameFoldersXml;
        static XDocument GameFoldersXML
        {
            get
            {
                if (_gameFoldersXml == null)
                {
                    XDocument gameFoldersXml;
                    string iniFile = Path.Combine(Path.GetDirectoryName(typeof(GameFoldersForm).Assembly.Location), "GameFolders.xml");
                    gameFoldersXml = XDocument.Load(iniFile);

                    _gameFoldersXml = gameFoldersXml;
                }
                return _gameFoldersXml;
            }
        }
        static XElement XElementGameFolders { get { return GameFoldersXML.Element(docXName); } }

        static string _rootFolder = null;
        static string RootFolder
        {
            get
            {
                if (_rootFolder == null)
                {
                    string rootFolder = "/";
                    XElement root = null;
                    if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
                    {
                        string vendor = "Microsoft";
                        string os = "Win64";
                        if (System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)) == 4)
                        {
                            os = "Win32";
                        }

                        root = XElementGameFolders.Elements(rootfolderXName)
                            .Where(x => x.Attributes(vendorAttributeName).Any(y => y.Value == vendor))
                            .Where(x => x.Attributes(osAttributeName).Any(y => y.Value == os))
                            .FirstOrDefault();
                    }
                    if (root == null)
                        root = XElementGameFolders.Elements(defaultrootfolderXName).FirstOrDefault();
                    if (root != null && root.Value != null)
                        rootFolder = root.Value;

                    _rootFolder = rootFolder;
                }
                return _rootFolder;
            }
        }

        static List<Game> _games = null;
        /// <summary>
        /// The list of <see cref="Game"/>s defined in <see cref="GameFoldersXML"/>.
        /// </summary>
        public static List<Game> Games
        {
            get
            {
                if (_games == null)
                    _games = XElementGameFolders.Elements(gameXName).Select(g => new Game(g)).ToList();
                return _games;
            }
        }

        /// <summary>
        /// Return the <see cref="Game"/> with a <c>Name</c> element with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">String to match again <see cref="Game"/>s' <c>Name</c> elements.</param>
        /// <returns>The (first) <see cref="Game"/> with a <c>Name</c> element with the specified <paramref name="value"/>
        /// or <c>null</c> if none found.</returns>
        public static Game byName(string value) { return Games.Where(x => x.Name == value).FirstOrDefault(); }

        /// <summary>
        /// Return the <see cref="Game"/> with a <c>RGVersion</c> element with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Number to match again <see cref="Game"/>s' <c>RGVersion</c> elements.</param>
        /// <returns>The (first) <see cref="Game"/> with a <c>RGVersion</c> element with the specified <paramref name="value"/>
        /// or <c>null</c> if none found.</returns>
        public static Game byRGVersion(int value) { return Games.Where(x => x.RGVersion == value).FirstOrDefault(); }

        private static Dictionary<Game, string> installDirs = new Dictionary<Game,string>();
        /// <summary>
        /// A semi-colon delimited string of game name / install folder pairs, internally delimited by an equals sign.
        /// </summary>
        public static string InstallDirs
        {
            get { return string.Join(";", installDirs.Select(kvp => kvp.Key.Name + "=" + kvp.Value).ToArray()); }
            set
            {
                if (value == null) installDirs = new Dictionary<Game, string>();
                else installDirs = value.Split(';')
                    .Select(xy => xy.Split('='))
                    .Where(xy =>
                        xy.Length == 2 &&
                        GameFolders.byName(xy[0]) != null &&
                        Directory.Exists(xy[1]) &&
                        Path.GetFullPath(xy[1]) != Path.GetFullPath(Path.Combine(RootFolder, GameFolders.byName(xy[0]).DefaultInstallDir))
                        )
                    .ToDictionary(xy => GameFolders.byName(xy[0]), xy => xy[1]);
            }
        }

        /// <summary>
        /// Return the folder where the given <see cref="Game"/> is installed.
        /// This will either be a user-specified location or the <see cref="Game.DefaultInstallDir"/>
        /// relative to the <see cref="RootFolder"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> for which to determine install folder.</param>
        /// <returns>The install folder for <paramref name="game"/>.</returns>
        public static string InstallDir(Game game)
        {
            return installDirs.ContainsKey(game) && Directory.Exists(installDirs[game]) ? installDirs[game] : Path.Combine(RootFolder, game.DefaultInstallDir);
        }

        static List<Game> gamesEnabled = null;
        /// <summary>
        /// A semi-colon delimited string of EP and SP names that the user has stated should not be referenced by the vendor part of the
        /// <see cref="FileTable"/> view.
        /// </summary>
        /// <seealso cref="FileTable"/>
        public static string EPsDisabled
        {
            get { return string.Join(";", GameFolders.Games.Where(g => !gamesEnabled.Contains(g)).Select(g => g.Name).ToArray()); }
            set
            {
                string[] split = value.Split(';').Distinct().ToArray();
                gamesEnabled = new List<Game>(GameFolders.Games.Where(g => !g.Suppressed.HasValue || !split.Contains(g.Name)));
            }
        }

        /// <summary>
        /// Returns whether a specified <see cref="Game"/> is enabled.
        /// </summary>
        /// <param name="game">The <see cref="Game"/> to query.</param>
        /// <returns>Whether a specified <see cref="Game"/> is enabled.</returns>
        /// <remarks>A game is enabled if<br/>
        /// (a) Disabling it is disallowed;<br/>
        /// (b) Or no games are user-disabled (see <see cref="EPsDisabled"/>) and it is not suppressed;<br/>
        /// (c) Or it is not one of the user-disabled games (see <see cref="EPsDisabled"/>).</remarks>
        public static bool IsEnabled(Game game) { return !game.Suppressed.HasValue || (gamesEnabled != null ? gamesEnabled.Contains(game) : !game.Suppressed.Value); }
    }

    /// <summary>
    /// Represents a known game object.
    /// </summary>
    public class Game
    {
        XElement _game;
        string ns = "";

        /// <summary>
        /// Create a game object from the supplied <see cref="XElement"/>.
        /// </summary>
        /// <param name="game">An <see cref="XElement"/> describing the game.</param>
        public Game(XElement game)
        {
            _game = game;
            if (_game.GetDefaultNamespace().NamespaceName.Length > 0)
                ns = "{" + _game.GetDefaultNamespace().NamespaceName + "}";
        }

        int _rgversion = -1;
        /// <summary>
        /// The ResourceGroup Version number for the <see cref="Game"/>.
        /// </summary>
        public int RGVersion
        {
            get
            {
                if (_rgversion == -1)
                {
                    int rgversion = 0;
                    XAttribute XArgversion = _game.Attribute("rgversion");
                    if (XArgversion == null || XArgversion.Value == null) rgversion = 0;
                    else if (!int.TryParse(XArgversion.Value, out rgversion)) rgversion = 0;
                    if (rgversion < 0) rgversion = 0;
                    _rgversion = rgversion;
                }
                return _rgversion;
            }
        }

        string getElement(string elementName, string defaultValue)
        {
            XElement xe = _game.Element(ns + elementName);
            if (xe != null && xe.Value != null && xe.Value.Length > 0)
                defaultValue = xe.Value;
            return defaultValue;
        }

        string _name = null;
        /// <summary>
        /// The name by which the game is known for settings purposes.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null) _name = getElement("Name", "Unk");
                return _name;
            }
        }

        string _longname = null;
        /// <summary>
        /// The full name for the game.
        /// </summary>
        public string Longname
        {
            get
            {
                if (_longname == null) _longname = getElement("Longname", "Unknown");
                return _longname;
            }
        }

        string _defaultInstallDir = null;
        /// <summary>
        /// The default installation location, relative to <see cref="GameFolders.RootFolder"/>.
        /// </summary>
        public string DefaultInstallDir
        {
            get
            {
                if (_defaultInstallDir == null) _defaultInstallDir = getElement("DefaultInstallDir", "/");
                return _defaultInstallDir;
            }
        }

        /// <summary>
        /// The <see cref="GameFolders.InstallDir"/> for this <see cref="Game"/>.
        /// </summary>
        public string UserInstallDir { get { return GameFolders.InstallDir(this); } }

        int? _suppresed = null;
        /// <summary>
        /// Whether the supplied XML indicates this game is suppressed.
        /// </summary>
        /// <remarks>The value is null if the user should not be able to disable this game.</remarks>
        public bool? Suppressed
        {
            get
            {
                if (!_suppresed.HasValue)
                {
                    string suppresed = getElement("Suppressed", "false");
                    switch (suppresed)
                    {
                        case "true": _suppresed = 1; break;
                        case "not-allowed": _suppresed = -1; break;
                        default: _suppresed = 0; break;
                    }
                }
                return _suppresed == -1 ? (bool?)null : _suppresed != 0;
            }
        }

        /// <summary>
        /// The <see cref="GameFolders.IsEnabled"/> state for this <see cref="Game"/>.
        /// </summary>
        public bool Enabled { get { return GameFolders.IsEnabled(this); } }

        /// <summary>
        /// All packages for this <see cref="Game"/> used to contain game content
        /// (i.e. excluding DDS images and thumbnail images).
        /// </summary>
        /// <seealso cref="DDSImages"/>
        /// <seealso cref="Thumbnails"/>
        public List<string> GameContent
        {
            get
            {
                if (!Directory.Exists(UserInstallDir)) return new List<string>();

                List<string> paths = new List<string>(GetDeltaPackages());
                paths.AddRange(GetFBPackages("0"));
                return paths;
            }
        }

        /// <summary>
        /// All packages for this <see cref="Game"/> used to contain DDS images.
        /// </summary>
        /// <seealso cref="DDSImages"/>
        /// <seealso cref="Thumbnails"/>
        public List<string> DDSImages
        {
            get
            {
                List<string> paths = new List<string>(GetDeltaPackages());
                paths.AddRange(GetFBPackages("2"));
                return paths;
            }
        }

        /// <summary>
        /// All packages for this <see cref="Game"/> used to contain Thumbnail images.
        /// </summary>
        /// <seealso cref="GameContent"/>
        /// <seealso cref="DDSImages"/>
        public List<string> Thumbnails
        {
            get
            {
                string root = Path.Combine(UserInstallDir, "Thumbnails");
                if (Directory.Exists(root)) return Directory.GetFiles(root, "*Thumbnails.package").ToList();
                else return new List<string>();
            }
        }

        List<string> GetDeltaPackages()
        {
            List<string> paths = new List<string>();
            string root = Path.Combine(UserInstallDir, "GameData/Shared/DeltaPackages");
            if (Directory.Exists(root))
            {
                for (int i = 0x20; i > 1; i--)
                {
                    string p = string.Format("p{0:d2}", i);
                    string db = Path.Combine(root, p);
                    if (!Directory.Exists(db)) continue;
                    paths.InsertRange(0, Directory.GetFiles(db, "DeltaBuild_" + p + ".package"));
                }
            }
            return paths;
        }

        IEnumerable<string> GetFBPackages(string sfx)
        {
            string root = Path.Combine(UserInstallDir, "GameData/Shared/Packages");
            if (Directory.Exists(root))
                return RGVersion == 0
                    ? (new string[] { "Delta", "Full", })
                        .Select(pfx => Path.Combine(root, pfx + "Build" + sfx + ".package"))
                        .Where(path => File.Exists(path))
                    : Directory.GetFiles(root, "FullBuild*.package");
            else return new string[0];
        }
    }
}