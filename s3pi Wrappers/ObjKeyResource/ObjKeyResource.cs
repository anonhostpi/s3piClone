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

namespace ObjKeyResource
{
    /// <summary>
    /// A resource wrapper that understands Catalog Entry resources
    /// </summary>
    public class ObjKeyResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s3pi.Settings.Settings.Checking;

        #region Attributes
        uint format = 7;
        ComponentList components;
        ComponentDataList componentData;
        byte unknown1;
        TGIBlockList tgiBlocks;
        #endregion

        public ObjKeyResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            long tgiPosn, tgiSize;
            BinaryReader r = new BinaryReader(s);

            format = r.ReadUInt32();
            tgiPosn = r.ReadUInt32() + s.Position;
            tgiSize = r.ReadUInt32();

            components = new ComponentList(OnResourceChanged, s);
            componentData = new ComponentDataList(OnResourceChanged, s);
            unknown1 = r.ReadByte();

            tgiBlocks = new TGIBlockList(OnResourceChanged, s, tgiPosn, tgiSize);
        }

        protected override Stream UnParse()
        {
            long posn;
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(format);

            posn = s.Position;
            w.Write((uint)0);
            w.Write((uint)0);

            if (components == null) components = new ComponentList(OnResourceChanged);
            components.UnParse(s);
            if (componentData == null) componentData = new ComponentDataList(OnResourceChanged);
            componentData.UnParse(s);
            w.Write(unknown1);

            if (tgiBlocks == null) tgiBlocks = new TGIBlockList(OnResourceChanged);
            tgiBlocks.UnParse(s, posn);

            s.Flush();

            return s;
        }
        #endregion

        #region Sub-classes
        public enum Component : uint
        {
            Animation = 0xee17c6ad,
            Effect = 0x80d91e9e,
            Footprint = 0xc807312a,
            Lighting = 0xda6c50fd,
            Location = 0x461922c8,
            LotObject = 0x6693c8b3,
            Model = 0x2954e734,
            Physics = 0x1a8feb14,
            Sacs = 0x3ae9a8e7,
            Script = 0x23177498,
            Sim = 0x22706efa,
            Slot = 0x2ef1e401,
            Steering = 0x61bd317c,
            Transform = 0x54cb7ebb,
            Tree = 0xc602cd31,
            VisualState = 0x50b3d17c,
        }

        static Dictionary<Component, string> ComponentDataMap;

        public class ComponentElement : AHandlerElement, IEquatable<ComponentElement>
        {
            Component element;
            public ComponentElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public ComponentElement(int APIversion, EventHandler handler, ComponentElement basis) : this(APIversion, handler, basis.element) { }
            public ComponentElement(int APIversion, EventHandler handler, uint value) : base(APIversion, handler) { element = (Component)value; }
            public ComponentElement(int APIversion, EventHandler handler, Component element) : base(APIversion, handler) { this.element = element; }

            static ComponentElement()
            {
                ComponentDataMap = new Dictionary<Component, string>();
                ComponentDataMap.Add(Component.Sim, "simOutfitKey");
                ComponentDataMap.Add(Component.Script, "scriptClass");
                ComponentDataMap.Add(Component.Model, "modelKey");
                ComponentDataMap.Add(Component.Steering, "steeringInstance");
                ComponentDataMap.Add(Component.Tree, "modelKey");
                ComponentDataMap.Add(Component.Footprint, "footprintKey");
            }

            #region AHandlerElement Members
            public override AHandlerElement Clone(EventHandler handler) { return new ComponentElement(requestedApiVersion, handler, this); }

            public override int RecommendedApiVersion { get { return 1; } }

            public override List<string> ContentFields { get { return AApiVersionedFields.GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ComponentElement> Members

            public bool Equals(ComponentElement other) { return ((uint)element).Equals((uint)other.element); }

            public override bool Equals(object obj)
            {
                return obj as ComponentElement != null ? this.Equals(obj as ComponentElement) : false;
            }

            public override int GetHashCode()
            {
                return element.GetHashCode();
            }

            #endregion

            public TypedValue Data(ComponentDataList list, TGIBlockList tgiBlocks)
            {
                if (!ComponentDataMap.ContainsKey(element)) return null;
                if (!list.ContainsKey(ComponentDataMap[element])) return null;
                ComponentDataType cd = list[ComponentDataMap[element]];
                System.Reflection.PropertyInfo pi = cd.GetType().GetProperty("Data");
                if (pi == null || !pi.CanRead) return null;
                if (element == Component.Footprint || element == Component.Model || element == Component.Tree)
                    return new TypedValue(typeof(TGIBlock), tgiBlocks[(int)pi.GetValue(cd, null)], "X");
                else
                    return new TypedValue(pi.PropertyType, pi.GetValue(cd, null), "X");
            }

            public Component Element { get { return element; } set { if (element != value) { element = value; OnElementChanged(); } } }
            public string Value { get { return "0x" + ((uint)element).ToString("X8") + " (" + (Enum.IsDefined(typeof(Component), element) ? element + "" : "undefined") + ")"; } }
        }

        public class ComponentList : DependentList<ComponentElement>
        {
            #region Constructors
            public ComponentList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public ComponentList(EventHandler handler, IEnumerable<ComponentElement> luint) : base(handler, luint, Byte.MaxValue) { }
            internal ComponentList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override ComponentElement CreateElement(Stream s) { return new ComponentElement(0, elementHandler, (new BinaryReader(s)).ReadUInt32()); }

            protected override void WriteCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }
            protected override void WriteElement(Stream s, ComponentElement element) { (new BinaryWriter(s)).Write((uint)element.Element); }
            #endregion

            public bool HasComponent(Component component) { return Find(component) != null; }

            public ComponentElement Find(Component component)
            {
                foreach (ComponentElement ce in this)
                    if (ce.Element == component) return ce;
                return null;
            }

            public override void Add() { this.Add(new ComponentElement(0, null)); }
        }

        public abstract class ComponentDataType : AHandlerElement, IComparable<ComponentDataType>, IEqualityComparer<ComponentDataType>, IEquatable<ComponentDataType>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            protected string key;
            protected byte controlCode;
            #endregion

            #region Constructors
            protected ComponentDataType(int APIversion, EventHandler handler, string key, byte controlCode)
                : base(APIversion, handler) { this.key = key; this.controlCode = controlCode; }

            public static ComponentDataType CreateComponentData(int APIversion, EventHandler handler, Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                string key = new string(r.ReadChars(r.ReadInt32()));
                byte controlCode = r.ReadByte();
                switch (controlCode)
                {
                    case 0x00: return new CDTString(APIversion, handler, key, controlCode, new string(r.ReadChars(r.ReadInt32())));
                    case 0x01: return new CDTResourceKey(APIversion, handler, key, controlCode, r.ReadInt32());
                    case 0x02: return new CDTAssetResourceName(APIversion, handler, key, controlCode, r.ReadInt32());
                    case 0x03: return new CDTSteeringInstance(APIversion, handler, key, controlCode, new string(r.ReadChars(r.ReadInt32())));
                    case 0x04: return new CDTUInt32(APIversion, handler, key, controlCode, r.ReadUInt32());
                    default:
                        if (checking) throw new InvalidDataException(String.Format("Unknown control code 0x{0:X2} at position 0x{1:X8}", controlCode, s.Position));
                        return null;
                }
            }
            #endregion

            #region Data I/O
            internal virtual void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(key.Length);
                w.Write(key.ToCharArray());
                w.Write(controlCode);
            }
            #endregion

            #region AHandlerElement Members

            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return AApiVersionedFields.GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IComparable<Key> Members

            public abstract int CompareTo(ComponentDataType other);

            #endregion

            #region IEqualityComparer<Key> Members

            public bool Equals(ComponentDataType x, ComponentDataType y) { return x.Equals(y); }

            public abstract int GetHashCode(ComponentDataType obj);

            #endregion

            #region IEquatable<Key> Members

            public bool Equals(ComponentDataType other) { return this.CompareTo(other) == 0; }

            public override bool Equals(object obj)
            {
                return obj as ComponentDataType != null ? this.Equals(obj as ComponentDataType) : false;
            }

            public override int GetHashCode()
            {
                return key.GetHashCode() ^ controlCode.GetHashCode();
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public string Key { get { return key; } set { if (key != value) { key = value; OnElementChanged(); } } }

            public virtual string Value { get { return this.GetType().Name + " -- Key: \"" + key + "\"; Control code: 0x" + controlCode.ToString("X2"); } }
            #endregion
        }
        [ConstructorParameters(new object[] { "", (byte)0x00, "", })]
        public class CDTString : ComponentDataType
        {
            #region Attributes
            protected string data;
            #endregion

            #region Constructors
            public CDTString(int APIversion, EventHandler handler, CDTString basis)
                : this(APIversion, handler, basis.key, basis.controlCode, basis.data) { }
            public CDTString(int APIversion, EventHandler handler, string key, byte controlCode, string data)
                : base(APIversion, handler, key, controlCode) { this.data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                BinaryWriter w = new BinaryWriter(s);
                w.Write(data.Length);
                w.Write(data.ToCharArray());
            }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new CDTString(requestedApiVersion, handler, this); }

            public override int CompareTo(ComponentDataType other)
            {
                if (this.GetType() != other.GetType()) return -1;
                CDTString oth = (CDTString)other;
                int res = key.CompareTo(oth.key); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return data.CompareTo(oth.data);
            }

            public override int GetHashCode(ComponentDataType obj) { return key.GetHashCode() ^ controlCode ^ data.GetHashCode(); }

            public string Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }

            public override string Value { get { return base.Value + "; Data: " + "\"" + data + "\""; } }
        }
        [ConstructorParameters(new object[] { "", (byte)0x01, (Int32)0, })]
        public class CDTResourceKey : ComponentDataType
        {
            #region Attributes
            protected int data;
            #endregion

            #region Constructors
            public CDTResourceKey(int APIversion, EventHandler handler, CDTResourceKey basis)
                : this(APIversion, handler, basis.key, basis.controlCode, basis.data) { }
            public CDTResourceKey(int APIversion, EventHandler handler, string key, byte controlCode, int data)
                : base(APIversion, handler, key, controlCode) { this.data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                new BinaryWriter(s).Write(data);
            }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new CDTResourceKey(requestedApiVersion, handler, this); }

            public override int CompareTo(ComponentDataType other)
            {
                if (this.GetType() != other.GetType()) return -1;
                CDTResourceKey oth = (CDTResourceKey)other;
                int res = key.CompareTo(oth.key); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return data.CompareTo(oth.data);
            }

            public override int GetHashCode(ComponentDataType obj) { return key.GetHashCode() ^ controlCode ^ data; }

            public int Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }

            public override string Value { get { return base.Value + "; Data: " + "0x" + data.ToString("X8"); } }
        }
        [ConstructorParameters(new object[] { "", (byte)0x02, (Int32)0, })]
        public class CDTAssetResourceName : CDTResourceKey
        {
            public CDTAssetResourceName(int APIversion, EventHandler handler, CDTAssetResourceName basis)
                : base(APIversion, handler, basis) { }
            public CDTAssetResourceName(int APIversion, EventHandler handler, string key, byte controlCode, int data)
                : base(APIversion, handler, key, controlCode, data) { }

            public override AHandlerElement Clone(EventHandler handler) { return new CDTAssetResourceName(requestedApiVersion, handler, this); }
        }
        [ConstructorParameters(new object[] { "", (byte)0x03, "", })]
        public class CDTSteeringInstance : CDTString
        {
            #region Constructors
            public CDTSteeringInstance(int APIversion, EventHandler handler, CDTSteeringInstance basis)
                : base(APIversion, handler, basis) { }
            public CDTSteeringInstance(int APIversion, EventHandler handler, string key, byte controlCode, string data)
                : base(APIversion, handler, key, controlCode, data) { }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new CDTSteeringInstance(requestedApiVersion, handler, this); }
        }
        [ConstructorParameters(new object[] { "", (byte)0x04, (UInt32)0, })]
        public class CDTUInt32 : ComponentDataType
        {
            #region Attributes
            uint data;
            #endregion

            #region Constructors
            public CDTUInt32(int APIversion, EventHandler handler, CDTUInt32 basis)
                : this(APIversion, handler, basis.key, basis.controlCode, basis.data) { }
            public CDTUInt32(int APIversion, EventHandler handler, string key, byte controlCode, uint data)
                : base(APIversion, handler, key, controlCode) { this.data = data; }
            #endregion

            #region Data I/O
            internal override void UnParse(Stream s)
            {
                base.UnParse(s);
                new BinaryWriter(s).Write(data);
            }
            #endregion

            public override AHandlerElement Clone(EventHandler handler) { return new CDTUInt32(requestedApiVersion, handler, this); }

            public override int CompareTo(ComponentDataType other)
            {
                if (this.GetType() != other.GetType()) return -1;
                CDTUInt32 oth = (CDTUInt32)other;
                int res = key.CompareTo(oth.key); if (res != 0) return res;
                res = controlCode.CompareTo(oth.controlCode); if (res != 0) return res;
                return data.CompareTo(oth.data);
            }

            public override int GetHashCode(ComponentDataType obj) { return (int)(key.GetHashCode() ^ controlCode ^ data); }

            public uint Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }

            public override string Value { get { return base.Value + "; Data: " + "0x" + data.ToString("X8"); } }
        }

        public class ComponentDataList : DependentList<ComponentDataType>
        {
            #region Constructors
            public ComponentDataList(EventHandler handler) : base(handler, Byte.MaxValue) { }
            public ComponentDataList(EventHandler handler, IEnumerable<ComponentDataType> luint) : base(handler, luint, Byte.MaxValue) { }
            internal ComponentDataList(EventHandler handler, Stream s) : base(handler, s, Byte.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte(); }
            protected override ComponentDataType CreateElement(Stream s) { return ComponentDataType.CreateComponentData(0, elementHandler, s); }

            protected override void WriteCount(Stream s, int count) { (new BinaryWriter(s)).Write((byte)count); }
            protected override void WriteElement(Stream s, ComponentDataType element) { element.UnParse(s); }
            #endregion

            protected override Type GetElementType(params object[] fields)
            {
                if (fields.Length == 1 && typeof(ComponentDataType).IsAssignableFrom(fields[0].GetType())) return fields[0].GetType();

                if (fields.Length != 3) throw new ArgumentException();

                switch ((byte)fields[1])
                {
                    case 0x00: return typeof(CDTString);
                    case 0x01: return typeof(CDTResourceKey);
                    case 0x02: return typeof(CDTAssetResourceName);
                    case 0x03: return typeof(CDTSteeringInstance);
                    case 0x04: return typeof(CDTUInt32);
                }
                throw new ArgumentException(String.Format("Unknown control code 0x{0:X2}", (byte)fields[1]));
            }

            public bool ContainsKey(string key) { return Find(x => x.Key.Equals(key)) != null; }

            public ComponentDataType this[string key]
            {
                get
                {
                    ComponentDataType cd = this.Find(x => x.Key.Equals(key));
                    if (cd != null) return cd;
                    throw new KeyNotFoundException();
                }
                set { this[IndexOf(this[key])] = value; }
            }

            public override void Add() { throw new NotImplementedException(); }
        }

        #endregion

        #region Content Fields
        public uint Format { get { return format; } set { if (format != value) { format = value; OnResourceChanged(this, new EventArgs()); } } }
        public ComponentList Components { get { return components; } set { if (components != value) { components = new ComponentList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        public ComponentDataList ComponentData { get { return componentData; } set { if (componentData != value) { componentData = new ComponentDataList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }
        public byte Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnResourceChanged(this, new EventArgs()); } } }
        public TGIBlockList TGIBlocks { get { return tgiBlocks; } set { if (tgiBlocks != value) { tgiBlocks = new TGIBlockList(OnResourceChanged, value); OnResourceChanged(this, new EventArgs()); } } }

        public string Value
        {
            get
            {
                return ValueBuilder;
                /*
                string s = "";
                s += String.Format("Format: 0x{0:X8}", format);

                s += "\n\nComponents:";
                foreach (ComponentElement c in components)
                {
                    s += "\n  " + c.Value;
                    TypedValue tv = c.Data(componentData, tgiBlocks);
                    if (tv != null)
                        s += "; Data: " + tv;
                }

                s += String.Format("\n\nUnknown1: 0x{0:X2}", unknown1);

                s += "\n\nComponent Data:";
                foreach (ComponentDataType cdt in componentData)
                    s += "\n  " + cdt.Value;

                s += "\n\nTGI Blocks:";
                for (int i = 0; i < tgiBlocks.Count; i++)
                    s += "\n  [0x" + i.ToString("X8") + "]: " + tgiBlocks[i];
                return s;
                /**/
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for CatalogResource wrapper
    /// </summary>
    public class ObjKeyResourceHandler : AResourceHandler
    {
        public ObjKeyResourceHandler()
        {
            this.Add(typeof(ObjKeyResource), new List<string>(new string[] { "0x02DC343F" }));
        }
    }
}
