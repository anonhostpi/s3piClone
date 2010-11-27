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
using System.Collections.Generic;
using System.IO;
using s3pi.Interfaces;

namespace s3pi.GenericRCOLResource
{
    public class RSLT : ARCOLBlock
    {
        static bool checking = s3pi.Settings.Settings.Checking;
        const string TAG = "RSLT";

        #region Attributes
        uint tag = (uint)FOURCC(TAG);
        uint version = 4;
        PartList routes;
        SevenFloatsList routeFloats;
        SlottedPartList containers;
        SevenFloatsList containerFloats;
        PartList effects;
        SevenFloatsList effectFloats;
        PartList inverseKineticsTargets;
        SevenFloatsList inverseKineticsTargetFloats;
        #endregion

        #region Constructors
        public RSLT(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public RSLT(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public RSLT(int APIversion, EventHandler handler, RSLT basis)
            : base(APIversion, handler, null)
        {
            this.version = basis.version;
            this.routes = new PartList(handler, basis.routes);
            this.routeFloats = new SevenFloatsList(handler, basis.routeFloats);
            this.containers = new SlottedPartList(handler, basis.containers);
            this.containerFloats = new SevenFloatsList(handler, basis.containerFloats);
            this.effects = new PartList(handler, basis.effects);
            this.effectFloats = new SevenFloatsList(handler, basis.effectFloats);
            this.inverseKineticsTargets = new PartList(handler, basis.inverseKineticsTargets);
            this.inverseKineticsTargetFloats = new SevenFloatsList(handler, basis.inverseKineticsTargetFloats);
        }
        #endregion

        #region ARCOLBlock
        public override string Tag { get { return TAG; } }

        public override uint ResourceType { get { return 0xD3044521; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC(TAG))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            version = r.ReadUInt32();

            int nRoutes = r.ReadInt32();
            int nContainers = r.ReadInt32();
            int nEffects = r.ReadInt32();
            int nInverseKineticsTargets = r.ReadInt32();
            int zero = r.ReadInt32();
            if (checking) if (zero != 0)
                    throw new InvalidDataException(string.Format("Expected zero, read 0x{0:X8} at 0x{1:X8}", zero, s.Position));

            routes = new PartList(handler, s, nRoutes);
            if (nRoutes == 0)
                routeFloats = new SevenFloatsList(handler);
            else
                routeFloats = new SevenFloatsList(handler, s);

            containers = new SlottedPartList(handler, s, nContainers);
            if (nContainers == 0)
                containerFloats = new SevenFloatsList(handler);
            else
                containerFloats = new SevenFloatsList(handler, s);
            
            effects = new PartList(handler, s, nEffects);
            if (nEffects == 0)
                effectFloats = new SevenFloatsList(handler);
            else
                effectFloats = new SevenFloatsList(handler, s);
            
            inverseKineticsTargets = new PartList(handler, s, nInverseKineticsTargets);
            if (nInverseKineticsTargets == 0)
                inverseKineticsTargetFloats = new SevenFloatsList(handler);
            else
                inverseKineticsTargetFloats = new SevenFloatsList(handler, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);

            if (routes == null) routes = new PartList(handler);
            w.Write(routes.Count);
            if (containers == null) containers = new SlottedPartList(handler);
            w.Write(containers.Count);
            if (effects == null) effects = new PartList(handler);
            w.Write(effects.Count);
            if (inverseKineticsTargets == null) inverseKineticsTargets = new PartList(handler);
            w.Write(inverseKineticsTargets.Count);
            w.Write((int)0);

            routes.UnParse(ms);
            if (routes.Count > 0) routeFloats.UnParse(ms);
            containers.UnParse(ms);
            if (containers.Count > 0) containerFloats.UnParse(ms);
            effects.UnParse(ms);
            if (effects.Count > 0) effectFloats.UnParse(ms);
            inverseKineticsTargets.UnParse(ms);
            if (inverseKineticsTargets.Count > 0) inverseKineticsTargetFloats.UnParse(ms);

            return ms;
        }

        public override AHandlerElement Clone(EventHandler handler) { return new RSLT(requestedApiVersion, handler, this); }
        #endregion

        #region Sub-types
        public class SevenFloats : AHandlerElement, IEquatable<SevenFloats>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            float unknown1 = 0f;
            float unknown2 = 0f;
            float unknown3 = 0f;
            float unknown4 = 0f;
            float unknown5 = 0f;
            float unknown6 = 0f;
            float unknown7 = 0f;
            #endregion

            #region Constructors
            public SevenFloats(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public SevenFloats(int APIversion, EventHandler handler, SevenFloats basis)
                : this(APIversion, handler, basis.unknown1, basis.unknown2, basis.unknown3, basis.unknown4, basis.unknown5, basis.unknown6, basis.unknown7) { }
            public SevenFloats(int APIversion, EventHandler handler,
                float unknown1, float unknown2, float unknown3, float unknown4, float unknown5, float unknown6, float unknown7)
                : base(APIversion, handler)
            {
                this.unknown1 = unknown1;
                this.unknown2 = unknown2;
                this.unknown3 = unknown3;
                this.unknown4 = unknown4;
                this.unknown5 = unknown5;
                this.unknown6 = unknown6;
                this.unknown7 = unknown7;
            }
            public SevenFloats(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                unknown1 = r.ReadSingle();
                unknown2 = r.ReadSingle();
                unknown3 = r.ReadSingle();
                unknown4 = r.ReadSingle();
                unknown5 = r.ReadSingle();
                unknown6 = r.ReadSingle();
                unknown7 = r.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(unknown1);
                w.Write(unknown2);
                w.Write(unknown3);
                w.Write(unknown4);
                w.Write(unknown5);
                w.Write(unknown6);
                w.Write(unknown7);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new SevenFloats(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<Route>
            public bool Equals(SevenFloats other)
            {
                return unknown1 == other.unknown1
                    && unknown2 == other.unknown2
                    && unknown3 == other.unknown3
                    && unknown4 == other.unknown4
                    && unknown5 == other.unknown5
                    && unknown6 == other.unknown6
                    && unknown7 == other.unknown7;
            }
            #endregion

            #region Content Fields
            public float Unknown1 { get { return unknown1; } set { if (unknown1 != value) { unknown1 = value; OnElementChanged(); } } }
            public float Unknown2 { get { return unknown2; } set { if (unknown2 != value) { unknown2 = value; OnElementChanged(); } } }
            public float Unknown3 { get { return unknown3; } set { if (unknown3 != value) { unknown3 = value; OnElementChanged(); } } }
            public float Unknown4 { get { return unknown4; } set { if (unknown1 != value) { unknown4 = value; OnElementChanged(); } } }
            public float Unknown5 { get { return unknown5; } set { if (unknown5 != value) { unknown5 = value; OnElementChanged(); } } }
            public float Unknown6 { get { return unknown6; } set { if (unknown6 != value) { unknown6 = value; OnElementChanged(); } } }
            public float Unknown7 { get { return unknown7; } set { if (unknown7 != value) { unknown7 = value; OnElementChanged(); } } }

            public virtual string Value
            {
                get
                {
                    return String.Format("Unknown1: {0}; ", unknown1) +
                        String.Format("Unknown2: {0}; ", unknown2) +
                        String.Format("Unknown3: {0}; ", unknown3) +
                        String.Format("Unknown4: {0}; ", unknown4) +
                        String.Format("Unknown5: {0}; ", unknown5) +
                        String.Format("Unknown6: {0}; ", unknown6) +
                        String.Format("Unknown7: {0}.", unknown7);
                }
            }
            #endregion
        }
        public class SevenFloatsList : AResource.DependentList<SevenFloats>
        {
            const int max = 1; // This implements the boolean nature of the list count

            #region Constructors
            public SevenFloatsList(EventHandler handler) : base(handler, max) { }
            public SevenFloatsList(EventHandler handler, Stream s) : base(null, max) { elementHandler = handler; Parse(s); this.handler = handler; }
            public SevenFloatsList(EventHandler handler, IEnumerable<SevenFloats> lsf) : base(handler, lsf, max) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s)
            {
                int c = base.ReadCount(s);
                if (checking) if (c > max)
                    throw new InvalidDataException(String.Format("Read 0x{0:X8}, expect less than 0x{1:X8}; position 0x{2:X16}", c, max, s.Position));
                return c;
            }
            protected override SevenFloats CreateElement(Stream s) { return new SevenFloats(0, elementHandler, s); }
            protected override void WriteElement(Stream s, SevenFloats element) { element.UnParse(s); }
            #endregion

            public override void Add() { if (Count < MaxSize) this.Add(new SevenFloats(0, null)); }
        }

        public class TransformElement : AHandlerElement, IEquatable<TransformElement>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            float rot1 = 0f;
            float rot2 = 0f;
            float rot3 = 0f;
            float pos = 0f;
            #endregion

            #region Constructors
            public TransformElement(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public TransformElement(int APIversion, EventHandler handler, TransformElement basis)
                : this(APIversion, handler, basis.rot1, basis.rot2, basis.rot3, basis.pos) { }
            public TransformElement(int APIversion, EventHandler handler, float rot1, float rot2, float rot3, float pos)
                : base(APIversion, handler) { this.rot1 = rot1; this.rot2 = rot2; this.rot3 = rot3; this.pos = pos; }
            public TransformElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                rot1 = r.ReadSingle();
                rot2 = r.ReadSingle();
                rot3 = r.ReadSingle();
                pos = r.ReadSingle();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(rot1);
                w.Write(rot2);
                w.Write(rot3);
                w.Write(pos);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new TransformElement(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<Route>
            public bool Equals(TransformElement other) { return rot1 == other.rot1 && rot2 == other.rot2 && rot3 == other.rot3 && pos == other.pos; }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public float Rot1 { get { return rot1; } set { if (rot1 != value) { rot1 = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public float Rot2 { get { return rot2; } set { if (rot2 != value) { rot2 = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public float Rot3 { get { return rot3; } set { if (rot3 != value) { rot3 = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public float Pos { get { return pos; } set { if (pos != value) { pos = value; OnElementChanged(); } } }

            public virtual string Value { get { return String.Format("Rot1: {0}; Rot2: {1}; Rot3: {2}; Pos: {3}", rot1, rot2, rot3, pos); } }
            #endregion
        }

        public class Part : AHandlerElement, IEquatable<Part>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            protected uint slotName;
            protected uint boneName;
            TransformElement tX;
            TransformElement tY;
            TransformElement tZ;
            #endregion

            #region Constructors
            public Part(int APIversion, EventHandler handler)
                : base(APIversion, handler)
            {
                tX = new TransformElement(requestedApiVersion, handler);
                tY = new TransformElement(requestedApiVersion, handler);
                tZ = new TransformElement(requestedApiVersion, handler);
            }
            public Part(int APIversion, EventHandler handler, Part basis)
                : this(APIversion, handler, basis.slotName, basis.boneName, basis.tX, basis.tY, basis.tZ) { }
            public Part(int APIversion, EventHandler handler,
                uint slotName, uint boneName, TransformElement tX, TransformElement tY, TransformElement tZ)
                : base(APIversion, handler)
            {
                this.slotName = slotName;
                this.boneName = boneName;
                this.tX = new TransformElement(requestedApiVersion, handler, tX);
                this.tY = new TransformElement(requestedApiVersion, handler, tY);
                this.tZ = new TransformElement(requestedApiVersion, handler, tZ);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

            public override AHandlerElement Clone(EventHandler handler) { return new Part(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<Route>
            public bool Equals(Part other)
            {
                return slotName.Equals(other.slotName)
                    && boneName.Equals(other.boneName)
                    && tX.Equals(other.tX)
                    && tY.Equals(other.tY)
                    && tZ.Equals(other.tZ)
                    ;
            }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public uint SlotName { get { return slotName; } set { if (slotName != value) { slotName = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public uint BoneName { get { return boneName; } set { if (boneName != value) { boneName = value; OnElementChanged(); } } }
            //[ElementPriority(3)] reserved for SlotPlacementFlags
            [ElementPriority(4)]
            public TransformElement X { get { return tX; } set { if (tX != value) { tX = new TransformElement(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public TransformElement Y { get { return tY; } set { if (tY != value) { tY = new TransformElement(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(6)]
            public TransformElement Z { get { return tZ; } set { if (tZ != value) { tZ = new TransformElement(requestedApiVersion, handler, value); OnElementChanged(); } } }

            public virtual string Value
            {
                get
                {
                    string s = "";
                    s += "SlotName: 0x" + slotName.ToString("X8");
                    s += "\nBoneName: 0x" + boneName.ToString("X8");
                    s += "\nX: " + tX.Value;
                    s += "\nY: " + tY.Value;
                    s += "\nZ: " + tZ.Value;
                    return s;
                }
            }
            #endregion
        }
        public class PartList : AResource.DependentList<Part>
        {
            #region Constructors
            public PartList(EventHandler handler) : base(handler) { }
            public PartList(EventHandler handler, Stream s, int count) : base(null) { elementHandler = handler; Parse(s, count); this.handler = handler; }
            public PartList(EventHandler handler, IEnumerable<Part> lsb) : base(handler, lsb) { }
            #endregion

            #region Data I/O
            protected void Parse(Stream s, int count)
            {
                uint[] slotNames = new uint[count];
                uint[] boneNames = new uint[count];
                BinaryReader r = new BinaryReader(s);
                for (int i = 0; i < slotNames.Length; i++) slotNames[i] = r.ReadUInt32();
                for (int i = 0; i < boneNames.Length; i++) boneNames[i] = r.ReadUInt32();
                for (int i = 0; i < count; i++) this.Add(new Part(0, elementHandler, slotNames[i], boneNames[i],
                    new TransformElement(0, elementHandler, s), new TransformElement(0, elementHandler, s), new TransformElement(0, elementHandler, s)));
            }
            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                for (int i = 0; i < Count; i++) w.Write(this[i].SlotName);
                for (int i = 0; i < Count; i++) w.Write(this[i].BoneName);
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].X == null) this[i].X = new TransformElement(0, handler);
                    this[i].X.UnParse(s);
                    if (this[i].Y == null) this[i].Y = new TransformElement(0, handler);
                    this[i].Y.UnParse(s);
                    if (this[i].Z == null) this[i].Z = new TransformElement(0, handler);
                    this[i].Z.UnParse(s);
                }
            }

            protected override Part CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, Part element) { throw new NotImplementedException(); }
            #endregion

            public override void Add() { this.Add(new Part(0, null)); }
        }

        //SlotPlacement flags taken from ObjectCatalogResource.cs
        [Flags]
        public enum SlotPlacement : uint
        {
            //CheckFlags = 0xc3f38, 

            None = 0x01,
            //
            //
            Small = 0x08,

            Medium = 0x10,
            Large = 0x20,
            //
            //

            Sim = 0x0100,
            Chair = 0x0200,
            CounterSink = 0x0400,
            EndTable = 0x0800,

            Stool = 0x1000,
            CounterAppliance = 0x2000,
            //
            //

            //
            //
            Functional = 0x40000,
            Decorative = 0x80000,

            Upgrade = 0x1000000,
            //MatchFlags    = 0x2000000,
            Vertical = 0x2000000,
            PlacementOnly = 0x4000000,
            //

            //RotationFlags = 0x30000000,
            CardinalRotation = 0x10000000,
            FullRotation = 0x20000000,
            AlwaysUp = 0x40000000,
            //
        }
        public class SlottedPart : Part, IEquatable<SlottedPart>
        {
            SlotPlacement slotPlacementFlags;
            public SlottedPart(int APIversion, EventHandler handler) : base(APIversion, handler) { }
            public SlottedPart(int APIversion, EventHandler handler, SlottedPart basis) : base(APIversion, handler, basis) { slotPlacementFlags = basis.slotPlacementFlags; }
            public SlottedPart(int APIversion, EventHandler handler, uint slotName, uint boneName, SlotPlacement slotPlacementFlags,
                TransformElement tX, TransformElement tY, TransformElement tZ)
                : base(APIversion, handler, slotName, boneName, tX, tY, tZ) { this.slotPlacementFlags = slotPlacementFlags; }

            public bool Equals(SlottedPart other) { return ((Part)this).Equals((Part)other) && slotPlacementFlags.Equals(other.slotPlacementFlags); }

            [ElementPriority(3)]
            public SlotPlacement SlotPlacementFlags { get { return slotPlacementFlags; } set { if (slotPlacementFlags != value) { slotPlacementFlags = value; OnElementChanged(); } } }
            public override string Value
            {
                get
                {
                    string s = base.Value;
                    s += "\nSlotPlacementFlags: " + this["SlotPlacementFlags"];
                    return s;
                }
            }
        }
        public class SlottedPartList : AResource.DependentList<SlottedPart>
        {
            #region Constructors
            public SlottedPartList(EventHandler handler) : base(handler) { }
            public SlottedPartList(EventHandler handler, Stream s, int count) : base(null) { elementHandler = handler; Parse(s, count); this.handler = handler; }
            public SlottedPartList(EventHandler handler, IEnumerable<SlottedPart> lsbp) : base(handler, lsbp) { }
            #endregion

            #region Data I/O
            protected void Parse(Stream s, int count)
            {
                uint[] slotNames = new uint[count];
                uint[] boneNames = new uint[count];
                uint[] flags = new uint[count];
                BinaryReader r = new BinaryReader(s);
                for (int i = 0; i < slotNames.Length; i++) slotNames[i] = r.ReadUInt32();
                for (int i = 0; i < boneNames.Length; i++) boneNames[i] = r.ReadUInt32();
                for (int i = 0; i < flags.Length; i++) flags[i] = r.ReadUInt32();
                for (int i = 0; i < count; i++) this.Add(new SlottedPart(0, elementHandler, slotNames[i], boneNames[i], (SlotPlacement)flags[i],
                    new TransformElement(0, elementHandler, s), new TransformElement(0, elementHandler, s), new TransformElement(0, elementHandler, s)));
            }
            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                for (int i = 0; i < Count; i++) w.Write(this[i].SlotName);
                for (int i = 0; i < Count; i++) w.Write(this[i].BoneName);
                for (int i = 0; i < Count; i++) w.Write((uint)this[i].SlotPlacementFlags);
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].X == null) this[i].X = new TransformElement(0, handler);
                    this[i].X.UnParse(s);
                    if (this[i].Y == null) this[i].Y = new TransformElement(0, handler);
                    this[i].Y.UnParse(s);
                    if (this[i].Z == null) this[i].Z = new TransformElement(0, handler);
                    this[i].Z.UnParse(s);
                }
            }

            protected override SlottedPart CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, SlottedPart element) { throw new NotImplementedException(); }
            #endregion

            public override void Add() { this.Add(new SlottedPart(0, null)); }
        }
        #endregion

        #region Content Fields
        [ElementPriority(0)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(1)]
        public PartList Routes { get { return routes; } set { if (routes != value) { routes = new PartList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        [DataGridExpandable]
        public SevenFloatsList RouteFloats { get { return routeFloats; } set { if (routeFloats != value) { routeFloats = new SevenFloatsList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public SlottedPartList Containers { get { return containers; } set { if (containers != value) { containers = new SlottedPartList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        [DataGridExpandable]
        public SevenFloatsList ContainerFloats { get { return containerFloats; } set { if (containerFloats != value) { containerFloats = new SevenFloatsList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public PartList Effects { get { return effects; } set { if (effects != value) { effects = new PartList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(6)]
        [DataGridExpandable]
        public SevenFloatsList EffectFloats { get { return effectFloats; } set { if (effectFloats != value) { effectFloats = new SevenFloatsList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(7)]
        public PartList InverseKineticsTargets { get { return inverseKineticsTargets; } set { if (inverseKineticsTargets != value) { inverseKineticsTargets = new PartList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(8)]
        [DataGridExpandable]
        public SevenFloatsList InverseKineticsTargetFloats { get { return inverseKineticsTargetFloats; } set { if (inverseKineticsTargetFloats != value) { inverseKineticsTargetFloats = new SevenFloatsList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }

        public string Value
        {
            get
            {
                string fmt;
                string s = "";
                s += "Tag: 0x" + tag.ToString("X8");
                s += "\nVersion: 0x" + version.ToString("X8");

                s += String.Format("\nRoutes ({0:X}):", routes.Count);
                fmt = "\n--[{0:X" + routes.Count.ToString("X").Length + "}]--\n{1}";
                for (int i = 0; i < routes.Count; i++) s += String.Format(fmt, i, routes[i].Value);
                s += "\n----";

                if (routeFloats.Count > 0) s += "\nRouteFloats: " + routeFloats[0].Value;

                s += String.Format("\nContainers ({0:X}):", containers.Count);
                fmt = "\n--[{0:X" + containers.Count.ToString("X").Length + "}]--\n{1}";
                for (int i = 0; i < containers.Count; i++) s += String.Format(fmt, i, containers[i].Value);
                s += "\n----";

                if (containerFloats.Count > 0) s += "\nContainerFloats: " + containerFloats[0].Value;

                s += String.Format("\nEffects ({0:X}):", effects.Count);
                fmt = "\n--[{0:X" + effects.Count.ToString("X").Length + "}]--\n{1}";
                for (int i = 0; i < effects.Count; i++) s += String.Format(fmt, i, effects[i].Value);
                s += "\n----";

                if (effectFloats.Count > 0) s += "\nEffectFloats: " + effectFloats[0].Value;

                s += String.Format("\nInverseKineticsTargets ({0:X}):", inverseKineticsTargets.Count);
                fmt = "\n--[{0:X" + inverseKineticsTargets.Count.ToString("X").Length + "}]--\n{1}";
                for (int i = 0; i < inverseKineticsTargets.Count; i++) s += String.Format(fmt, i, inverseKineticsTargets[i].Value);
                s += "\n----";

                if (inverseKineticsTargetFloats.Count > 0) s += "\nInverseKineticsTargetFloats: " + inverseKineticsTargetFloats[0].Value;

                return s;
            }
        }
        #endregion
    }
}
