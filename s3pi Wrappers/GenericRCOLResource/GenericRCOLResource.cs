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
using System.Reflection;

namespace s3pi.GenericRCOLResource
{
    /// <summary>
    /// A resource wrapper that understands generic RCOL resources
    /// </summary>
    public class GenericRCOLResource : AResource
    {
        static bool checking = s3pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        uint version;
        uint dataType;
        uint unused;
        ChunkEntryList blockList;
        CountedTGIBlockList resources;
        #endregion

        #region Constructors
        public GenericRCOLResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            version = r.ReadUInt32();
            dataType = r.ReadUInt32();
            unused = r.ReadUInt32();
            int countResources = r.ReadInt32();
            int countChunks = r.ReadInt32();
            TGIBlock[] chunks = new TGIBlock[countChunks];
            for (int i = 0; i < countChunks; i++) chunks[i] = new TGIBlock(0, OnResourceChanged, "ITG", s);
            resources = new CountedTGIBlockList(OnResourceChanged, "ITG", countResources, s);

            RCOLIndexEntry[] index = new RCOLIndexEntry[countChunks];
            for (int i = 0; i < countChunks; i++) { index[i].Position = r.ReadUInt32(); index[i].Length = r.ReadInt32(); }

            blockList = new ChunkEntryList(requestedApiVersion, OnResourceChanged, s, chunks, index);
        }

        protected override Stream UnParse()
        {
            long rcolIndexPos;

            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(version);
            w.Write(dataType);
            w.Write(unused);
            if (resources == null) resources = new CountedTGIBlockList(OnResourceChanged, "ITG");
            w.Write(resources.Count);
            if (blockList == null) blockList = new ChunkEntryList(OnResourceChanged);
            w.Write(blockList.Count);
            foreach (ChunkEntry ce in blockList) ce.TGIBlock.UnParse(ms);
            resources.UnParse(ms);

            rcolIndexPos = ms.Position;
            RCOLIndexEntry[] index = new RCOLIndexEntry[blockList.Count];
            for (int i = 0; i < blockList.Count; i++) { w.Write((uint)0); w.Write((uint)0); } // Pad for the index

            int j = 0;
            foreach (ChunkEntry ce in blockList)
            {
                byte[] data = ce.RCOLBlock.AsBytes;
                while (w.BaseStream.Position % 4 != 0) w.Write((byte)0);
                index[j].Position = (uint)ms.Position;
                index[j].Length = data.Length;
                w.Write(data);
                j++;
            }

            ms.Position = rcolIndexPos;
            foreach (RCOLIndexEntry entry in index) { w.Write(entry.Position); w.Write(entry.Length); }

            return ms;
        }
        #endregion

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Sub-types
        internal struct RCOLIndexEntry
        {
            public uint Position;
            public int Length;
        }

        public class ChunkEntry : AHandlerElement, IEquatable<ChunkEntry>
        {
            const Int32 recommendedApiVersion = 1;

            #region Attributes
            TGIBlock tgiBlock;
            ARCOLBlock rcolBlock;
            #endregion

            public ChunkEntry(int APIversion, EventHandler handler, ChunkEntry basis)
                : this(APIversion, handler, basis.tgiBlock, basis.rcolBlock) { }
            public ChunkEntry(int APIversion, EventHandler handler, TGIBlock tgiBlock, ARCOLBlock rcolBlock)
                : base(APIversion, handler)
            {
                this.tgiBlock = (TGIBlock)tgiBlock.Clone(handler);
                this.rcolBlock = (ARCOLBlock)rcolBlock.Clone(handler);
            }

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new ChunkEntry(requestedApiVersion, handler, this); }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ChunkEntry> Members

            public bool Equals(ChunkEntry other) { return tgiBlock == other.tgiBlock && rcolBlock == other.rcolBlock; }

            #endregion

            public TGIBlock TGIBlock { get { return tgiBlock; } set { if (tgiBlock != value) { tgiBlock = new TGIBlock(0, handler, value); OnElementChanged(); } } }
            public ARCOLBlock RCOLBlock { get { return rcolBlock; } set { if (rcolBlock != value) { rcolBlock = (ARCOLBlock)rcolBlock.Clone(handler); OnElementChanged(); } } }

            public string Value
            {
                get
                {
                    string s = "";
                    s += "--- " + tgiBlock + ((rcolBlock.Equals("*")) ? "" : " - " + rcolBlock.Tag) + " ---";
                    if (AApiVersionedFields.GetContentFields(0, rcolBlock.GetType()).Contains("Value"))
                        s += "\n" + rcolBlock["Value"];
                    else foreach (string field in AApiVersionedFields.GetContentFields(0, rcolBlock.GetType()))
                        {
                            if (!(new List<string>(new string[] { "ResourceType", "Tag", "Value", "Stream", "AsBytes", })).Contains(field))
                                s += "\n  " + field + ": " + rcolBlock[field];
                        }
                    s += "\n----";
                    return s;
                }
            }
        }

        public class ChunkEntryList : DependentList<ChunkEntry>
        {
            internal ChunkEntryList(int requestedApiVersion, EventHandler handler, Stream s, TGIBlock[] chunks, RCOLIndexEntry[] index)
                : base(null, -1)
            {
                elementHandler = handler;

                BinaryReader r = new BinaryReader(s);
                for (int i = 0; i < index.Length; i++)
                {
                    s.Position = index[i].Position;
                    byte[] data = r.ReadBytes(index[i].Length);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(data, 0, data.Length);
                    ms.Position = 0;

                    this.Add(chunks[i], GenericRCOLResourceHandler.RCOLDealer(requestedApiVersion, elementHandler, chunks[i].ResourceType, ms));
                }

                this.handler = handler;
            }
            public ChunkEntryList(EventHandler handler) : base(handler) { }
            public ChunkEntryList(EventHandler handler, IEnumerable<ChunkEntry> ice) : base(handler, ice) { }

            protected override ChunkEntry CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, ChunkEntry element) { throw new NotImplementedException(); }

            internal EventHandler listEventHandler { set { handler = value; } }

            public override void Add() { throw new NotImplementedException(); }
        }

        public enum ReferenceType : byte
        {
            Public = 0x0, //This resource
            Private = 0x1,
            External = 0x2, //unused
            Delayed = 0x3, //Other resource
        }

        public class ChunkReference : AHandlerElement, IEquatable<ChunkReference>
        {
            const Int32 recommendedApiVersion = 1;

            #region Attributes
            uint chunkReference;
            #endregion

            #region Constructors
            public ChunkReference(int APIversion, EventHandler handler, Stream s)
                : base(APIversion, handler) { Parse(s); }
            public ChunkReference(int APIversion, EventHandler handler, ChunkReference basis)
                : this(APIversion, handler, basis.chunkReference) { }
            public ChunkReference(int APIversion, EventHandler handler, uint chunkReference)
                : base(APIversion, handler) { this.chunkReference = chunkReference; }
            #endregion

            #region Data I/O
            void Parse(Stream s) { this.chunkReference = new BinaryReader(s).ReadUInt32(); }

            public void UnParse(Stream s) { new BinaryWriter(s).Write(chunkReference); }
            #endregion

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new ChunkReference(requestedApiVersion, handler, this); }

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields
            {
                get
                {
                    List<string> res = GetContentFields(requestedApiVersion, this.GetType());
                    //if (chunkReference == 0) res.Remove("RefType");
                    return res;
                }
            }
            #endregion

            #region IEquatable<ChunkReference> Members

            public bool Equals(ChunkReference other) { return chunkReference == other.chunkReference; }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public int TGIBlockIndex { get { return (int)(chunkReference & 0x0FFFFFFF) - 1; } set { if (TGIBlockIndex != value) { if (value == -1) chunkReference = 0; else chunkReference = (chunkReference & 0xF0000000) | (uint)((value + 1) & 0x0FFFFFFF); OnElementChanged(); } } }
            [ElementPriority(2)]
            public ReferenceType RefType { get { return (ReferenceType)(chunkReference == 0 ? -1 : (byte)(chunkReference >> 28)); } set { if (RefType != value) { chunkReference = (((uint)value) << 28) | (chunkReference & 0x0FFFFFFF); OnElementChanged(); } } }

            public string Value { get { return chunkReference > 0 ? this["TGIBlockIndex"] + " (" + this["RefType"] + ")" : "(unset)"; } }
            #endregion
        }

        #endregion

        #region Content Fields
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        public uint DataType { get { return dataType; } set { if (dataType != value) { dataType = value; OnResourceChanged(this, EventArgs.Empty); } } }
        public uint Unused { get { return unused; } set { if (unused != value) { unused = value; OnResourceChanged(this, EventArgs.Empty); } } }
        public CountedTGIBlockList Resources { get { return resources; } set { if (resources != value) { resources = new CountedTGIBlockList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        public ChunkEntryList ChunkEntries { get { return blockList; } set { if (blockList != value) { blockList = new ChunkEntryList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                string s = "";
                s += "Version: 0x" + version.ToString("X8");
                s += "\nDataType: 0x" + dataType.ToString("X8");
                s += "\nUnused: 0x" + unused.ToString("X8");
                s += "\n--\nResources:";
                for (int i = 0; i < resources.Count; i++)
                    s += "\n[" + i + "]: " + resources[i];
                s += "\n----";
                s += "\nRCOL Blocks:";
                for (int i = 0; i < blockList.Count; i++)
                    s += "\n[" + i + "] " + blockList[i].Value;
                return s;
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for GenericRCOLResource wrapper
    /// </summary>
    public class GenericRCOLResourceHandler : AResourceHandler
    {
        static Dictionary<uint, Type> typeRegistry;
        static Dictionary<string, Type> tagRegistry;

        static List<string> resourceTypes;

        static GenericRCOLResourceHandler()
        {
            typeRegistry = new Dictionary<uint, Type>();
            tagRegistry = new Dictionary<string, Type>();

            string folder = Path.GetDirectoryName(typeof(GenericRCOLResourceHandler).Assembly.Location);
            foreach (string path in Directory.GetFiles(folder, "*.dll"))
            {
                //Protect load of DLL
                try
                {
                    Assembly dotNetDll = Assembly.LoadFile(path);
                    Type[] types = dotNetDll.GetTypes();
                    foreach (Type t in types)
                    {
                        if (!t.IsSubclassOf(typeof(ARCOLBlock))) continue;

                        //Protect instantiating class
                        try
                        {
                            ARCOLBlock arb = (ARCOLBlock)t.GetConstructor(new Type[] { typeof(int), typeof(EventHandler), typeof(Stream), }).Invoke(new object[] { 0, null, null });
                            if (arb == null) continue;

                            if (!typeRegistry.ContainsKey(arb.ResourceType)) typeRegistry.Add(arb.ResourceType, arb.GetType());
                            if (!tagRegistry.ContainsKey(arb.Tag)) tagRegistry.Add(arb.Tag, arb.GetType());
                        }
                        catch { }
                    }
                }
                catch { }
            }

            StreamReader sr = new StreamReader(Path.Combine(folder, "RCOLResources.txt"));
            resourceTypes = new List<string>();
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                if (s.StartsWith(";")) continue;
                string[] t = s.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                if (t[2].Equals("Y")) resourceTypes.Add(t[0]);
            }
        }

        public static ARCOLBlock CreateRCOLBlock(int APIversion, EventHandler handler, uint type)
        {
            Type[] types = new Type[] { typeof(int), typeof(EventHandler), };
            object[] args = new object[] { APIversion, handler, };
            if (GenericRCOLResourceHandler.typeRegistry.ContainsKey(type))
            {
                Type t = GenericRCOLResourceHandler.typeRegistry[type];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            if (GenericRCOLResourceHandler.tagRegistry.ContainsKey("*"))
            {
                Type t = GenericRCOLResourceHandler.tagRegistry["*"];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            return null;
        }

        public static ARCOLBlock RCOLDealer(int APIversion, EventHandler handler, uint type, Stream s)
        {
            Type[] types = new Type[] { typeof(int), typeof(EventHandler), typeof(Stream), };
            object[] args = new object[] { APIversion, handler, s };
            if (GenericRCOLResourceHandler.typeRegistry.ContainsKey(type))
            {
                Type t = GenericRCOLResourceHandler.typeRegistry[type];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            if (GenericRCOLResourceHandler.tagRegistry.ContainsKey("*"))
            {
                Type t = GenericRCOLResourceHandler.tagRegistry["*"];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            return null;
        }

        public static ARCOLBlock RCOLDealer(int APIversion, EventHandler handler, uint type, params object[] fields)
        {
            Type[] types = new Type[2 + fields.Length];
            types[0] = typeof(int);
            types[1] = typeof(EventHandler);
            for (int i = 0; i < fields.Length; i++) types[2 + i] = fields[i].GetType();

            object[] args = new object[2 + fields.Length];
            args[0] = APIversion;
            args[1] = handler;
            Array.Copy(fields, 0, args, 2, fields.Length);

            if (GenericRCOLResourceHandler.typeRegistry.ContainsKey(type))
            {
                Type t = GenericRCOLResourceHandler.typeRegistry[type];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            if (GenericRCOLResourceHandler.tagRegistry.ContainsKey("*"))
            {
                Type t = GenericRCOLResourceHandler.tagRegistry["*"];
                return (ARCOLBlock)t.GetConstructor(types).Invoke(args);
            }
            return null;
        }

        public GenericRCOLResourceHandler()
        {
            this.Add(typeof(GenericRCOLResource), new List<string>(resourceTypes.ToArray()));
        }
    }
}
