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
using System.Reflection;

namespace s3pi.Interfaces
{
    /// <summary>
    /// A resource contained in a package.
    /// </summary>
    public abstract class AResource : AApiVersionedFields, IResource
    {
        #region Attributes
        /// <summary>
        /// Resource data <see cref="System.IO.Stream"/>
        /// </summary>
        protected Stream stream = null;

        /// <summary>
        /// Indicates the resource stream may no longer reflect the resource content
        /// </summary>
        protected bool dirty = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s"><see cref="System.IO.Stream"/> to use, or null to create from scratch.</param>
        protected AResource(int APIversion, Stream s)
        {
            requestedApiVersion = APIversion;
            stream = s;
        }
        #endregion

        #region AApiVersionedFields
        /// <summary>
        /// A <see cref="List{String}"/> of available field names on object
        /// </summary>
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region IResource Members
        /// <summary>
        /// The resource content as a <see cref="System.IO.Stream"/>.
        /// </summary>
        public virtual Stream Stream
        {
            get
            {
                if (dirty || s3pi.Settings.Settings.AsBytesWorkaround)
                {
                    stream = UnParse();
                    dirty = false;
                    //Console.WriteLine(this.GetType().Name + " flushed.");
                }
                stream.Position = 0;
                return stream;
            }
        }
        /// <summary>
        /// The resource content as a <see cref="byte"/> array
        /// </summary>
        public virtual byte[] AsBytes
        {
            get
            {
                MemoryStream s = this.Stream as MemoryStream;
                if (s != null) return s.ToArray();

                stream.Position = 0;
                return (new BinaryReader(stream)).ReadBytes((int)stream.Length);
            }
        }

        /// <summary>
        /// Raised if the resource is changed
        /// </summary>
        public event EventHandler ResourceChanged;

        #endregion

        #region Sub-classes
        /// <summary>
        /// Abstract extension to <see cref="AHandlerList{T}"/> adding support for <see cref="System.IO.Stream"/> IO
        /// and partially implementing <see cref="IGenericAdd"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of list element</typeparam>
        /// <seealso cref="AHandlerList{T}"/>
        /// <seealso cref="IGenericAdd"/>
        public abstract class DependentList<T> : AHandlerList<T>, IGenericAdd
            where T : IEquatable<T>
        {
            /// <summary>
            /// Holds the <see cref="EventHandler"/> delegate to invoke if an element in the <see cref="DependentList{T}"/> changes.
            /// </summary>
            /// <remarks>Work around for list event handler triggering during stream constructor and other places.</remarks>
            protected EventHandler elementHandler = null;

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="DependentList{T}"/> class
            /// that is empty.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            protected DependentList(EventHandler handler, long size = -1) : base(handler, size) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="DependentList{T}"/> class
            /// filled with the content of <paramref name="ilt"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="ilt">The <see cref="IList{T}"/> to use as the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            /// <remarks>Does not throw an exception if <paramref name="ilt"/>.Count is greater than <paramref name="size"/>.
            /// An exception will be thrown on any attempt to add further items unless the Count is reduced first.</remarks>
            protected DependentList(EventHandler handler, IList<T> ilt, long size = -1) : base(handler, ilt, size) { }

            // Add stream-based constructors and support
            /// <summary>
            /// Initializes a new instance of the <see cref="DependentList{T}"/> class
            /// filled from <see cref="System.IO.Stream"/> <paramref name="s"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> to read for the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            /// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
            protected DependentList(EventHandler handler, Stream s, long size = -1) : base(null, size) { elementHandler = handler; Parse(s); this.handler = handler; }
            #endregion

            #region Data I/O
            /// <summary>
            /// Read list entries from a stream
            /// </summary>
            /// <param name="s">Stream containing list entries</param>
            /// <remarks>This method bypasses <see cref="DependentList{T}.Add(object[])"/>
            /// because <see cref="CreateElement(Stream, out bool)"/> must take care of the same issues.</remarks>
            protected virtual void Parse(Stream s) { base.Clear(); bool inc = true; for (uint i = ReadCount(s); i > 0; i = (uint)(i - (inc ? 1 : 0))) base.Add(CreateElement(s, out inc)); }
            /// <summary>
            /// Return the number of elements to be created.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> being processed.</param>
            /// <returns>The number of elements to be created.</returns>
            protected virtual uint ReadCount(Stream s) { return (new BinaryReader(s)).ReadUInt32(); }
            /// <summary>
            /// Create a new element from the <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="s">Stream containing element data.</param>
            /// <returns>A new element.</returns>
            protected abstract T CreateElement(Stream s);
            /// <summary>
            /// Create a new element from the <see cref="System.IO.Stream"/> and indicates whether it counts towards the number of elements to be created.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> containing element data.</param>
            /// <param name="inc">Whether this call towards the number of elements to be created.</param>
            /// <returns>A new element.</returns>
            protected virtual T CreateElement(Stream s, out bool inc) { inc = true; return CreateElement(s); }

            /// <summary>
            /// Write list entries to a stream
            /// </summary>
            /// <param name="s">Stream to receive list entries</param>
            public virtual void UnParse(Stream s) { WriteCount(s, (uint)Count); foreach (T element in this) WriteElement(s, element); }
            /// <summary>
            /// Write the count of list elements to the stream.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="count"/> to.</param>
            /// <param name="count">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
            protected virtual void WriteCount(Stream s, uint count) { (new BinaryWriter(s)).Write(count); }
            /// <summary>
            /// Write an element to the stream.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="element"/> to.</param>
            /// <param name="element">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
            protected abstract void WriteElement(Stream s, T element);
            #endregion

            #region IGenericAdd
            /// <summary>
            /// Add a default element to a <see cref="DependentList{T}"/>.
            /// </summary>
            /// <exception cref="NotImplementedException">Lists of abstract classes will fail
            /// with a NotImplementedException.</exception>
            /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
            public abstract void Add();

            /// <summary>
            /// Adds an entry to an <see cref="DependentList{T}"/>.
            /// </summary>
            /// <param name="fields">
            /// Either the object to add or the generic type&apos;s constructor arguments.
            /// </param>
            /// <returns>True on success</returns>
            /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
            public virtual bool Add(params object[] fields)
            {
                if (fields == null) return false;
                Type elementType = typeof(T);
                if (fields.Length == 1 && elementType.IsAssignableFrom(fields[0].GetType()) && !typeof(AHandlerElement).IsAssignableFrom(elementType))
                {
                    base.Add((T)fields[0]);
                    return true;
                }

                if (elementType.IsAbstract) elementType = GetElementType(fields);

                Type[] types = new Type[2 + fields.Length];
                types[0] = typeof(int);
                types[1] = typeof(EventHandler);
                for (int i = 0; i < fields.Length; i++) types[2 + i] = fields[i].GetType();

                object[] args = new object[2 + fields.Length];
                args[0] = (int)0;
                args[1] = elementHandler;
                Array.Copy(fields, 0, args, 2, fields.Length);

                System.Reflection.ConstructorInfo ci = elementType.GetConstructor(types);
                if (ci == null) return false;
                base.Add((T)(elementType.GetConstructor(types).Invoke(args)));
                return true;
            }

            /// <summary>
            /// Return the type to get the constructor from, for the given set of fields.
            /// </summary>
            /// <param name="fields">Constructor parameters</param>
            /// <returns>Class on which to invoke constructor</returns>
            /// <remarks><paramref name="fields"/>[0] could be an instance of the abstract class: it should provide a constructor that accepts a "template"
            /// object and creates a new instance on that basis.</remarks>
            protected virtual Type GetElementType(params object[] fields) { throw new NotImplementedException(); }
            #endregion
        }

        /// <summary>
        /// An implementation of AResourceKey that supports storing the Type, Group and Instance in any order.
        /// </summary>
        public class TGIBlock : AResourceKey, IEquatable<TGIBlock>
        {
            #region Attributes
            const int recommendedApiVersion = 1;
            string order = "TGI";
            #endregion

            #region Constructors
            /// <summary>
            /// Options for the order of the Type, Group and Instance elements of a TGIBlock
            /// </summary>
            public enum Order
            {
                /// <summary>
                /// Type, Group, Instance
                /// </summary>
                TGI,
                /// <summary>
                /// Type, Instance, Group
                /// </summary>
                TIG,
                /// <summary>
                /// Group, Type, Instance
                /// </summary>
                GTI,
                /// <summary>
                /// Group, Instance, Type
                /// </summary>
                GIT,
                /// <summary>
                /// Instance, Type, Group
                /// </summary>
                ITG,
                /// <summary>
                /// Instance, Group, Type
                /// </summary>
                IGT,
            }
            void ok(string v) { ok((Order)Enum.Parse(typeof(Order), v)); }
            void ok(Order v) { if (!Enum.IsDefined(typeof(Order), v)) throw new ArgumentException("Invalid value " + v, "order"); }

            /// <summary>
            /// Initialize a new TGIBlock
            /// with the order and values
            /// based on <paramref name="basis"/>.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="basis">The TGIBlock to use for the <see cref="Order"/> and <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, TGIBlock basis) : this(APIversion, handler, basis.order, (IResourceKey)basis) { }

            /// <summary>
            /// Initialize a new TGIBlock
            /// with the default order ("TGI").
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            public TGIBlock(int APIversion, EventHandler handler) : base(APIversion, handler, 0, 0, 0) { }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, string order) : this(APIversion, handler) { ok(order); this.order = order; }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, Order order) : this(APIversion, handler) { ok(order); this.order = "" + order; }

            /// <summary>
            /// Initialize a new TGIBlock
            /// with the default order ("TGI") and specified values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="resourceType">The resource type value.</param>
            /// <param name="resourceGroup">The resource group value.</param>
            /// <param name="instance">The resource instance value.</param>
            public TGIBlock(int APIversion, EventHandler handler, uint resourceType, uint resourceGroup, ulong instance)
                : base(APIversion, handler, resourceType, resourceGroup, instance) { }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            /// <param name="resourceType">The resource type value.</param>
            /// <param name="resourceGroup">The resource group value.</param>
            /// <param name="instance">The resource instance value.</param>
            public TGIBlock(int APIversion, EventHandler handler, string order, uint resourceType, uint resourceGroup, ulong instance)
                : this(APIversion, handler, resourceType, resourceGroup, instance) { ok(order); this.order = order; }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            /// <param name="resourceType">The resource type value.</param>
            /// <param name="resourceGroup">The resource group value.</param>
            /// <param name="instance">The resource instance value.</param>
            public TGIBlock(int APIversion, EventHandler handler, Order order, uint resourceType, uint resourceGroup, ulong instance)
                : this(APIversion, handler, resourceType, resourceGroup, instance) { ok(order); this.order = "" + order; }

            /// <summary>
            /// Initialize a new TGIBlock
            /// with the default order ("TGI") and specified <see cref="IResourceKey"/> values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, IResourceKey rk) : base(APIversion, handler, rk) { }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and <see cref="IResourceKey"/> values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, string order, IResourceKey rk) : this(APIversion, handler, rk) { ok(order); this.order = order; }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and <see cref="IResourceKey"/> values.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">The <see cref="Order"/> to use to store the <see cref="IResourceKey"/> values.</param>
            /// <param name="rk">The <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, Order order, IResourceKey rk) : this(APIversion, handler, rk) { ok(order); this.order = "" + order; }

            /// <summary>
            /// Initialize a new TGIBlock
            /// with the default order ("TGI") and values read from the specified <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and values read from the specified <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">A <see cref="string"/> representing the <see cref="Order"/> of the <see cref="IResourceKey"/> values.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, string order, Stream s) : base(APIversion, handler) { ok(order); this.order = order; Parse(s); }
            /// <summary>
            /// Initialize a new TGIBlock
            /// with the specified order and values read from the specified <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="APIversion">The requested API version.</param>
            /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
            /// <param name="order">The <see cref="Order"/> of the <see cref="IResourceKey"/> values.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> from which to read the <see cref="IResourceKey"/> values.</param>
            public TGIBlock(int APIversion, EventHandler handler, Order order, Stream s) : base(APIversion, handler) { ok(order); this.order = "" + order; Parse(s); }
            #endregion

            #region Data I/O
            /// <summary>
            /// Used by the <see cref="TGIBlock"/> constructor to inialise a new <see cref="TGIBlock"/> from a <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> containing <see cref="TGIBlock"/> values in known order.</param>
            protected void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                foreach (char c in order)
                    switch (c)
                    {
                        case 'T': resourceType = r.ReadUInt32(); break;
                        case 'G': resourceGroup = r.ReadUInt32(); break;
                        case 'I': instance = r.ReadUInt64(); break;
                    }
            }

            /// <summary>
            /// Writes the <see cref="TGIBlock"/> to the specified <see cref="System.IO.Stream"/> in known order.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <see cref="TGIBlock"/> values to.</param>
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                foreach (char c in order)
                    switch (c)
                    {
                        case 'T': w.Write(resourceType); break;
                        case 'G': w.Write(resourceGroup); break;
                        case 'I': w.Write(instance); break;
                    }
            }
            #endregion

            #region AHandlerElement
            /// <summary>
            /// The list of available field names on this API object
            /// </summary>
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            /// <summary>
            /// The best supported version of the API available
            /// </summary>
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            /// <summary>
            /// Get a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.
            /// </summary>
            /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
            /// <returns>Return a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.</returns>
            public override AHandlerElement Clone(EventHandler handler) { return new TGIBlock(requestedApiVersion, handler, this); }
            #endregion

            #region IEquatable<TGIBlock> Members

            /// <summary>
            /// Indicates whether the current <see cref="TGIBlock"/> instance is equal to another <see cref="TGIBlock"/> instance.
            /// </summary>
            /// <param name="other">An <see cref="TGIBlock"/> instance to compare with this instance.</param>
            /// <returns>true if the current instance is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
            public bool Equals(TGIBlock other) { return this.Equals((IResourceKey)other); }

            #endregion

            #region Content Fields
            /// <summary>
            /// A display-ready string representing the <see cref="TGIBlock"/>.
            /// </summary>
            public String Value { get { return this.ToString(); } }
            #endregion
        }

        /// <summary>
        /// A TGIBlock list class where the count of elements is separate from the stored list
        /// </summary>
        public class CountedTGIBlockList : DependentList<TGIBlock>
        {
            uint origCount; // count at the time the list was constructed, used to Parse() list from stream
            string order = "TGI";

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// that is empty
            /// with <see cref="TGIBlock.Order"/> of "TGI".
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, long size = -1) : this(handler, "TGI", size) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with the content of <paramref name="ilt"/>
            /// with <see cref="TGIBlock.Order"/> of "TGI".
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="ilt">The <see cref="IList{TGIBlock}"/> to use as the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, IList<TGIBlock> ilt, long size = -1) : this(handler, "TGI", ilt, size) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with <paramref name="count"/> elements from <see cref="System.IO.Stream"/> <paramref name="s"/>
            /// with <see cref="TGIBlock.Order"/> of "TGI".
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="count">The number of list elements to read.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> to read for the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, uint count, Stream s, long size = -1) : this(handler, "TGI", count, s, size) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// that is empty
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">The <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, TGIBlock.Order order, long size = -1) : this(handler, "" + order, size) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with the content of <paramref name="ilt"/>
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">The <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="ilt">The <see cref="IList{TGIBlock}"/> to use as the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, TGIBlock.Order order, IList<TGIBlock> ilt, long size = -1) : this(handler, "" + order, ilt, size) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with <paramref name="count"/> elements from <see cref="System.IO.Stream"/> <paramref name="s"/>
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">The <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="count">The number of list elements to read.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> to read for the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, TGIBlock.Order order, uint count, Stream s, long size = -1) : this(handler, "" + order, count, s, size) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// that is empty
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">A string representing the <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, string order, long size = -1) : base(handler, size) { this.order = order; }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with the content of <paramref name="ilt"/>
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">A string representing the <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="ilt">The <see cref="IList{TGIBlock}"/> to use as the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, string order, IList<TGIBlock> ilt, long size = -1) : base(handler, ilt, size) { this.order = order; }
            /// <summary>
            /// Initializes a new instance of the <see cref="CountedTGIBlockList"/> class
            /// filled with <paramref name="count"/> elements from <see cref="System.IO.Stream"/> <paramref name="s"/>
            /// with the specified <see cref="TGIBlock.Order"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="order">A string representing the <see cref="TGIBlock.Order"/> of the <see cref="TGIBlock"/> values.</param>
            /// <param name="count">The number of list elements to read.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> to read for the initial content of the list.</param>
            /// <param name="size">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
            public CountedTGIBlockList(EventHandler handler, string order, uint count, Stream s, long size = -1) : base(null, size) { this.origCount = count; this.order = order; elementHandler = handler; Parse(s); this.handler = handler; }
            #endregion

            #region Data I/O
            /// <summary>
            /// Create a new element from the <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="s">Stream containing element data.</param>
            /// <returns>A new element.</returns>
            protected override TGIBlock CreateElement(Stream s) { return new TGIBlock(0, elementHandler, order, s); }
            /// <summary>
            /// Write an element to the stream.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="element"/> to.</param>
            /// <param name="element">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
            protected override void WriteElement(Stream s, TGIBlock element) { element.UnParse(s); }

            /// <summary>
            /// Return the number of elements to be created.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> being processed -- ignored.</param>
            /// <returns>The number of elements to be created, as provided to the <see cref="CountedTGIBlockList"/> constructor.</returns>
            protected override uint ReadCount(Stream s) { return origCount; }
            /// <summary>
            /// This list does not manage a count within the <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="s">Ignored.</param>
            /// <param name="count">Ignored.</param>
            protected override void WriteCount(Stream s, uint count) { }
            #endregion

            /// <summary>
            /// Add a new default element to the list.
            /// </summary>
            public override void Add() { base.Add(new TGIBlock(0, null, order)); } // Need to pass "order"

            /// <summary>
            /// Adds a new <see cref="TGIBlock"/> to the list using the values of the specified <see cref="TGIBlock"/>.
            /// </summary>
            /// <param name="item">The <see cref="TGIBlock"/> to use as a basis for the new <see cref="TGIBlock"/>.</param>
            /// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
            /// <remarks>A new element is created rather than using the element passed
            /// as the order (TGI/ITG/etc) may be different.</remarks>
            public override void Add(TGIBlock item) { base.Add(new TGIBlock(0, elementHandler, order, item)); }

            /// <summary>
            /// Adds a new TGIBlock to the list using the values of the IResourceKey.
            /// </summary>
            /// <param name="rk">The ResourceKey values to use for the TGIBlock.</param>
            /// <remarks>A new element is created rather than using the element passed
            /// as the order (TGI/ITG/etc) may be different.</remarks>
            public void Add(IResourceKey rk) { base.Add(new TGIBlock(0, elementHandler, order, rk)); }

            /// <summary>
            /// Inserts a new <see cref="TGIBlock"/> to the list at the specified index using the values of the specified <see cref="TGIBlock"/>.
            /// </summary>
            /// <param name="index">The zero-based index at which the new <see cref="TGIBlock"/> should be inserted.</param>
            /// <param name="item">The <see cref="TGIBlock"/> to use as a basis for the new <see cref="TGIBlock"/>.</param>
            /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index
            /// in the <see cref="CountedTGIBlockList"/>.</exception>
            /// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="System.NotSupportedException">The <see cref="CountedTGIBlockList"/> is read-only.</exception>
            /// <remarks>A new element is created rather than using the element passed
            /// as the order (TGI/ITG/etc) may be different.</remarks>
            public override void Insert(int index, TGIBlock item) { base.Insert(index, new TGIBlock(0, elementHandler, order, item)); }

            #region Content Fields
            /// <summary>
            /// A displayable string representing the list.
            /// </summary>
            public String Value { get { string s = ""; for (int i = 0; i < Count; i++) s += string.Format("0x{0:X8}: {1}\n", i, this[i].Value); return s; } }
            #endregion
        }

        /// <summary>
        /// A TGIBlock list class where the count and size of the list are stored separately (but managed by this class)
        /// </summary>
        public class TGIBlockList : DependentList<TGIBlock>
        {
            bool addEight = false;

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="TGIBlockList"/> class
            /// that is empty.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="addEight">When true, invoke fudge factor in parse/unparse</param>
            public TGIBlockList(EventHandler handler, bool addEight = false) : base(handler) { this.addEight = addEight; }
            /// <summary>
            /// Initializes a new instance of the <see cref="TGIBlockList"/> class
            /// filled with the content of <paramref name="ilt"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="ilt">The <see cref="IList{TGIBlock}"/> to use as the initial content of the list.</param>
            /// <param name="addEight">When true, invoke fudge factor in parse/unparse</param>
            public TGIBlockList(EventHandler handler, IList<TGIBlock> ilt, bool addEight = false) : base(handler, ilt) { this.addEight = addEight; }
            /// <summary>
            /// Initializes a new instance of the <see cref="TGIBlockList"/> class
            /// filled with elements from <see cref="System.IO.Stream"/> <paramref name="s"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="s">The <see cref="System.IO.Stream"/> to read for the initial content of the list.</param>
            /// <param name="tgiPosn">Position in the <see cref="System.IO.Stream"/> where the list of <see cref="TGIBlock"/>s starts.</param>
            /// <param name="tgiSize">Size (in bytes) of the stored list.</param>
            /// <param name="addEight">When true, invoke fudge factor in parse/unparse</param>
            public TGIBlockList(EventHandler handler, Stream s, long tgiPosn, long tgiSize, bool addEight = false) : base(null) { elementHandler = handler; this.addEight = addEight; Parse(s, tgiPosn, tgiSize); this.handler = handler; }
            #endregion

            #region Data I/O
            /// <summary>
            /// Create a new element from the <see cref="System.IO.Stream"/>.
            /// </summary>
            /// <param name="s">Stream containing element data.</param>
            /// <returns>A new element.</returns>
            protected override TGIBlock CreateElement(Stream s) { return new TGIBlock(0, elementHandler, s); }
            /// <summary>
            /// Write an element to the stream.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="element"/> to.</param>
            /// <param name="element">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
            protected override void WriteElement(Stream s, TGIBlock element) { element.UnParse(s); }

            /// <summary>
            /// Read list entries from a stream
            /// </summary>
            /// <param name="s">Stream containing list entries</param>
            /// <param name="tgiPosn">Position in the <see cref="System.IO.Stream"/> where the list of <see cref="TGIBlock"/>s starts.</param>
            /// <param name="tgiSize">Size (in bytes) of the stored list.</param>
            protected void Parse(Stream s, long tgiPosn, long tgiSize)
            {
                bool checking = true;
                if (checking) if (tgiPosn != s.Position)
                        throw new InvalidDataException(String.Format("Position of TGIBlock read: 0x{0:X8}, actual: 0x{1:X8}",
                            tgiPosn, s.Position));

                if (tgiSize > 0) Parse(s);

                if (checking) if (tgiSize != s.Position - tgiPosn + (addEight ? 8 : 0))
                        throw new InvalidDataException(String.Format("Size of TGIBlock read: 0x{0:X8}, actual: 0x{1:X8}; at 0x{2:X8}",
                            tgiSize, s.Position - tgiPosn, s.Position));
            }

            /// <summary>
            /// Write list entries to a stream
            /// </summary>
            /// <param name="s">Stream to receive list entries</param>
            /// <param name="ptgiO">Position in <see cref="System.IO.Stream"/> to write list position and size values.</param>
            public void UnParse(Stream s, long ptgiO)
            {
                BinaryWriter w = new BinaryWriter(s);

                long tgiPosn = s.Position;
                UnParse(s);
                long pos = s.Position;

                s.Position = ptgiO;
                w.Write((uint)(tgiPosn - ptgiO - sizeof(uint)));
                w.Write((uint)(pos - tgiPosn + (addEight ? 8 : 0)));

                s.Position = pos;
            }
            #endregion

            /// <summary>
            /// Add a new default element to the list
            /// </summary>
            public override void Add() { this.Add(new TGIBlock(0, null)); }

            #region Content Fields
            /// <summary>
            /// A displayable string representing the list.
            /// </summary>
            public String Value { get { string s = ""; for (int i = 0; i < Count; i++) s += string.Format("0x{0:X8}: {1}\n", i, this[i].Value); return s; } }
            #endregion
        }

        /// <summary>
        /// A flexible generic list that implements <see cref="DependentList{T}"/> for
        /// a simple data type (such as <see cref="UInt32"/>).
        /// </summary>
        /// <typeparam name="T">A simple data type (such as <see cref="UInt32"/>).</typeparam>
        /// <example>
        /// The following method shows a way to create a list of UInt32 values, with a UInt32 entry count
        /// stored in the stream immediately before the list.
        /// <code>
        /// <![CDATA[
        /// SimpleList<UInt32> ReadUInt32List(EventHandler e, Stream s)
        /// {
        ///     return new SimpleList<UInt32>(e,
        ///         s => new BinaryReader(s).ReadUInt32(),
        ///         (s, value) => new BinaryWriter(s).Write(value));
        /// }
        /// ]]>
        /// </code>
        /// For more complex cases, or where repeated use of the same kind of <see cref="SimpleList{T}"/> is needed,
        /// it can be worthwhile extending the class, as shown below.  This example is for a list of byte values prefixed
        /// by a one byte count.  It shows that the list length can also be specified (here using <c>Byte.MaxValue</c>
        /// <code>
        /// <![CDATA[
        /// public class ByteList : AResource.SimpleList<Byte>
        /// {
        ///     static string fmt = "0x{1:X2}; ";
        ///     
        ///     public ByteList(EventHandler handler) : base(handler, ReadByte, WriteByte, fmt, Byte.MaxValue, ReadListCount, WriteListCount) { }
        ///     public ByteList(EventHandler handler, Stream s) : base(handler, s, ReadByte, WriteByt, fmte, Byte.MaxValue, ReadListCount, WriteListCount) { }
        ///     public ByteList(EventHandler handler, IList<HandlerElement<Byte>> le) : base(handler, le, ReadByte, WriteByte fmt,, Byte.MaxValue, ReadListCount, WriteListCount) { }
        ///     
        ///     static uint ReadListCount(Stream s) { return new BinaryReader(s).ReadByte(); }
        ///     static void WriteListCount(Stream s, uint count) { new BinaryWriter(s).Write((byte)count); }
        ///     static byte ReadByte(Stream s) { return new BinaryReader(s).ReadByte(); }
        ///     static void WriteByte(Stream s, byte value) { new BinaryWriter(s).Write(value); }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="HandlerElement{T}"/>
        public class SimpleList<T> : DependentList<HandlerElement<T>>
            where T: struct, IEquatable<T>
        {
            /// <summary>
            /// Create a new element of type <typeparamref name="T"/> from a <see cref="Stream"/>.
            /// </summary>
            /// <param name="s">The <see cref="Stream"/> from which to read the element data.</param>
            /// <returns>A new element of type <typeparamref name="T"/>.</returns>
            public delegate T CreateElementMethod(Stream s);
            /// <summary>
            /// Write an element of type <typeparamref name="T"/> to a <see cref="Stream"/>.
            /// </summary>
            /// <param name="s">The <see cref="Stream"/> to which to write the value.</param>
            /// <param name="value">The value of type <typeparamref name="T"/> to write.</param>
            public delegate void WriteElementMethod(Stream s, T value);
            /// <summary>
            /// Return the number of list elements to read.
            /// </summary>
            /// <param name="s">A <see cref="Stream"/> that may contain the number of elements.</param>
            /// <returns>The number of list elements to read.</returns>
            public delegate uint ReadCountMethod(Stream s);
            /// <summary>
            /// Store the number of elements in the list.
            /// </summary>
            /// <param name="s">A <see cref="Stream"/> to which list elements will be written after the count.</param>
            /// <param name="count">The number of list elements.</param>
            public delegate void WriteCountMethod(Stream s, uint count);

            CreateElementMethod createElement;
            WriteElementMethod writeElement;
            string valFormat = "0x{1:X8}\n";
            ReadCountMethod readCount;
            WriteCountMethod writeCount;

            #region Constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleList{T}"/> class
            /// that is empty.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="createElement">Optional; the method to create a new element in the list from a stream.  If null, return default{T}.</param>
            /// <param name="writeElement">Optional; the method to create a new element in the list from a stream.  No operation if null.</param>
            /// <param name="valFormat">Optional, default is <c>"0x{1:X8}\n"</c>; the method to create a new element in the list from a stream.</param>
            /// <param name="size">Optional maximum number of elements in the list.</param>
            /// <param name="readCount">Optional; default is to read a <see cref="UInt32"/> from the <see cref="Stream"/>.</param>
            /// <param name="writeCount">Optional; default is to write a <see cref="UInt32"/> to the <see cref="Stream"/>.</param>
            public SimpleList(EventHandler handler, CreateElementMethod createElement = null, WriteElementMethod writeElement = null, string valFormat = "0x{1:X8}\n", long size = -1, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : base(handler, size) { this.createElement = createElement; this.writeElement = writeElement; this.valFormat = valFormat; this.readCount = readCount; this.writeCount = writeCount; }
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleList{T}"/> class
            /// from <paramref name="iList"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="iList">The source to use as the initial content of the list.</param>
            /// <param name="createElement">Optional; the method to create a new element in the list from a stream.  If null, return default{T}.</param>
            /// <param name="writeElement">Optional; the method to create a new element in the list from a stream.  No operation if null.</param>
            /// <param name="valFormat">Optional, default is <c>"0x{1:X8}\n"</c>; the method to create a new element in the list from a stream.</param>
            /// <param name="size">Optional maximum number of elements in the list.</param>
            /// <param name="readCount">Optional; default is to read a <see cref="UInt32"/> from the <see cref="Stream"/>.</param>
            /// <param name="writeCount">Optional; default is to write a <see cref="UInt32"/> to the <see cref="Stream"/>.</param>
            public SimpleList(EventHandler handler, IList<HandlerElement<T>> iList, CreateElementMethod createElement = null, WriteElementMethod writeElement = null, string valFormat = "0x{1:X8}\n", long size = -1, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : base(handler, iList, size) { this.createElement = createElement; this.writeElement = writeElement; this.valFormat = valFormat; this.readCount = readCount; this.writeCount = writeCount; }
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleList{T}"/> class
            /// from <paramref name="iList"/>, wrapping each entry in a <see cref="HandlerElement{T}"/> instance.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="iList">The source to use as the initial content of the list.</param>
            /// <param name="createElement">Optional; the method to create a new element in the list from a stream.  If null, return default{T}.</param>
            /// <param name="writeElement">Optional; the method to create a new element in the list from a stream.  No operation if null.</param>
            /// <param name="valFormat">Optional, default is <c>"0x{1:X8}\n"</c>; the method to create a new element in the list from a stream.</param>
            /// <param name="size">Optional maximum number of elements in the list.</param>
            /// <param name="readCount">Optional; default is to read a <see cref="UInt32"/> from the <see cref="Stream"/>.</param>
            /// <param name="writeCount">Optional; default is to write a <see cref="UInt32"/> to the <see cref="Stream"/>.</param>
            public SimpleList(EventHandler handler, IList<T> iList, CreateElementMethod createElement = null, WriteElementMethod writeElement = null, string valFormat = "0x{1:X8}\n", long size = -1, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : this(null, createElement, writeElement, valFormat, size, readCount, writeCount) { elementHandler = handler; this.AddRange(iList); this.handler = handler; }
            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleList{T}"/> class
            /// from <paramref name="s"/>.
            /// </summary>
            /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
            /// <param name="s">The <see cref="Stream"/> to read for the initial content of the list.</param>
            /// <param name="createElement">Required; the method to create a new element in the list from a stream.</param>
            /// <param name="writeElement">Required; the method to create a new element in the list from a stream.</param>
            /// <param name="valFormat">Optional, default is <c>"0x{1:X8}\n"</c>; the method to create a new element in the list from a stream.</param>
            /// <param name="size">Optional maximum number of elements in the list.</param>
            /// <param name="readCount">Optional; default is to read a <see cref="UInt32"/> from the <see cref="Stream"/>.</param>
            /// <param name="writeCount">Optional; default is to write a <see cref="UInt32"/> to the <see cref="Stream"/>.</param>
            public SimpleList(EventHandler handler, Stream s, CreateElementMethod createElement, WriteElementMethod writeElement, string valFormat = "0x{1:X8}\n", long size = -1, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : this(null, createElement, writeElement, valFormat, size, readCount, writeCount) { elementHandler = handler; Parse(s); this.handler = handler; }
            #endregion

            #region Data I/O
            /// <summary>
            /// Return the number of elements to be created.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> being processed.</param>
            /// <returns>The number of elements to be created.</returns>
            protected override uint ReadCount(Stream s) { return readCount == null ? base.ReadCount(s) : readCount(s); }
            /// <summary>
            /// Write the count of list elements to the stream.
            /// </summary>
            /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="count"/> to.</param>
            /// <param name="count">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
            protected override void WriteCount(Stream s, uint count) { if (writeCount == null) base.WriteCount(s, count); else writeCount(s, count); }

            /// <summary>
            /// Creates an new list element of type <typeparamref name="T"/> by reading <paramref name="s"/>.
            /// </summary>
            /// <param name="s"><see cref="Stream"/> containing data.</param>
            /// <returns>New list element.</returns>
            protected override HandlerElement<T> CreateElement(Stream s) { return new HandlerElement<T>(0, elementHandler, createElement == null ? default(T) : createElement(s)); }
            /// <summary>
            /// Writes the value of a list element to <paramref name="s"/>.
            /// </summary>
            /// <param name="s"><see cref="Stream"/> containing data.</param>
            /// <param name="element">List element for which to write the value to the <seealso cref="Stream"/>.</param>
            protected override void WriteElement(Stream s, HandlerElement<T> element) { if (writeElement != null) writeElement(s, element.Val); }
            #endregion

            /// <summary>
            /// Add a default element to a <see cref="SimpleList{T}"/>.
            /// </summary>
            /// <exception cref="NotImplementedException">Lists of abstract classes will fail
            /// with a NotImplementedException.</exception>
            /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
            public override void Add() { this.Add(new HandlerElement<T>(0, elementHandler)); }

            /// <summary>
            /// Adds an entry to a <see cref="SimpleList{T}"/>.
            /// </summary>
            /// <param name="item">The object to add.</param>
            /// <returns>True on success</returns>
            /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
            public void Add(T item) { base.Add(new HandlerElement<T>(0, elementHandler, item)); }

            /// <summary>
            /// Adds the elements of the specified collection to the end of the <see cref="SimpleList{T}"/>.
            /// </summary>
            /// <param name="collection">The collection whose elements should be added to the end of the <see cref="SimpleList{T}"/>.
            /// The collection itself cannot be null, but it can contain elements that are null, if type <typeparamref name="T"/> is a reference type.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
            /// <exception cref="System.InvalidOperationException">Thrown when list size would be exceeded.</exception>
            /// <exception cref="System.NotSupportedException">The <see cref="SimpleList{T}"/> is read-only.</exception>
            /// <remarks>Calls <see cref="Add(T)"/> for each item in <paramref name="collection"/>.</remarks>
            public void AddRange(IEnumerable<T> collection)
            {
                int newElements = new List<T>(collection).Count;
                if (maxSize >= 0 && Count >= maxSize - newElements) throw new InvalidOperationException();

                //Note that the following is required to allow for implementation specific processing on items added to the list:
                EventHandler h = handler;
                handler = null;
                foreach (T item in collection) this.Add(item);
                handler = h;

                OnListChanged();
            }

            /// <summary>
            /// Inserts an item to the <see cref="SimpleList{T}"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which item should be inserted.</param>
            /// <param name="item">The object to insert into the <see cref="SimpleList{T}"/>.</param>
            /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="SimpleList{T}"/>.</exception>
            /// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
            /// <exception cref="System.NotSupportedException">The <see cref="SimpleList{T}"/> is read-only.</exception>
            public void Insert(int index, T item) { base.Insert(index, new HandlerElement<T>(0, elementHandler, item)); }

            /// <summary>
            /// Inserts the elements of a collection into the <see cref="SimpleList{T}"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
            /// <param name="collection">The collection whose elements should be inserted into the <see cref="SimpleList{T}"/>.
            /// The collection itself cannot be null, but it can contain elements that are null, if type <typeparamref name="T"/> is a reference type.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">
            /// <paramref name="index"/> is less than 0.
            /// -or-
            /// <paramref name="index"/> is greater than <see cref="SimpleList{T}"/>.Count.
            /// </exception>
            /// <exception cref="System.InvalidOperationException">Thrown when list size would be exceeded.</exception>
            /// <exception cref="System.NotSupportedException">The <see cref="SimpleList{T}"/> is read-only.</exception>
            /// <remarks>Calls <see cref="Insert(int, T)"/> for each item in <paramref name="collection"/>.</remarks>
            public void InsertRange(int index, IEnumerable<T> collection)
            {
                int newElements = new List<T>(collection).Count;
                if (maxSize >= 0 && Count >= maxSize - newElements) throw new InvalidOperationException();

                //Note that the following is required to allow for implementation specific processing on items inserted into the list:
                EventHandler h = handler;
                handler = null;
                foreach (T item in collection) this.Insert(index++, item);
                handler = h;

                OnListChanged();
            }

            /// <summary>
            /// Removes the first occurrence of an entry from the <see cref="SimpleList{T}"/> with the value given.
            /// </summary>
            /// <param name="item">The value to remove from the <see cref="SimpleList{T}"/>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="SimpleList{T}"/>
            /// otherwise, false. This method also returns false if item is not found in
            /// the original <see cref="SimpleList{T}"/>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
            public bool Remove(T item) { return base.Remove(this.Find(e => e.Val.Equals(item))); }

            #region Content Fields
            /// <summary>
            /// Return the list content, formated for display only, using the element format supplied on the list constructor.
            /// </summary>
            public String Value { get { string fmt = "[{0:X" + Count.ToString("X").Length + "}]: " + valFormat; string s = ""; for (int i = 0; i < Count; i++) s += string.Format(fmt, i, this[i].Val); return s; } }
            #endregion
        }
        #endregion

        /// <summary>
        /// AResource classes must supply an <see cref="UnParse()"/> method that serializes the class to a <see cref="System.IO.Stream"/> that is returned.
        /// </summary>
        /// <returns><see cref="System.IO.Stream"/> containing serialized class data.</returns>
        protected abstract Stream UnParse();

        /// <summary>
        /// AResource classes must use this to indicate the resource has changed.
        /// </summary>
        /// <param name="sender">The resource (or sub-class) that has changed.</param>
        /// <param name="e">(Empty) event data object.</param>
        protected virtual void OnResourceChanged(object sender, EventArgs e)
        {
            dirty = true;
            //Console.WriteLine(this.GetType().Name + " dirtied.");
            if (ResourceChanged != null) ResourceChanged(sender, e);
        }
    }
}
