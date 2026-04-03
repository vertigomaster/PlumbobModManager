using IDEK.Tools.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace IDEK.Tools.DataStructures.Trees
{
    /// <summary>
    /// Represents a node in the sparse octree.
    /// </summary>
    public class SparseOctreeNode<T> : ISerializable
    {
        #region Fields

        [Obsolete("Have to go back to a list to handle trees with finite depth.")]
        private T _element;

        protected List<T> _elements = new();

        #endregion

        #region Properties

        public int Capacity => 8; //Because octree (2^3)
        public Vector3 Center { get; private set; }
        public Vector3 Size { get; private set; }
        public Vector3 Extents => Size / 2f;

        [Obsolete("Have to go back to a list to handle trees with finite depth. See Elements")]
        /// <summary>
        /// Can only be set if node is a leaf. 
        /// This should really only be set by internal mechanisms. To insert elements
        /// </summary>
        public T Element
        {
            get => _elements.FirstOrDefault();
            protected internal set
            {
                ConsoleLog.LogError("Element is deprecated, please use Elements. This function will set the first element of Elements list for loose backwards compat, but issues may arise. Please update your implementations as soon as possible!");
                if (!IsLeaf)
                {
                    throw new InvalidOperationException("Cannot assign an element to a non-leaf node; the element must be propogated down.");
                }

                _elements[0] = value;
            }
        }

        /// <summary>
        /// This list generally only contains 0 or 1 elements, 
        /// unless Depth >= max depth, at which point any extra elements have to be shoved into it.
        /// </summary>
        public IReadOnlyList<T> Elements => _elements;

        public SparseOctreeNode<T>[] Children { get; protected internal set; }
        public int Depth { get; protected internal set; } = 0;

        public bool IsEmpty => _elements.Count <= 0;

        /// <summary>
        /// A leaf if no children array, or if said array is empty
        /// </summary>
        public bool IsLeaf => Children == null;

        #endregion

        #region Constructors

        public SparseOctreeNode()
        {
            Center = Vector3.Zero;
            Size = Vector3.One;
        }

        public SparseOctreeNode(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        #endregion

        #region Operations

        public void EmptyOut()
        {
            _elements.Clear();
        }

        public void InsertElement(T newElement)
        {
            _elements.Add(newElement);
        }

        #endregion

        //TODO: Make Serialization Reversible
        #region Serialization

        /// <summary>
        /// Serializes the node to a binary stream.
        /// </summary>
        /// <param name="stream">The stream to serialize to.</param>
        [Obsolete("Not Yet Functional")]
        public void Serialize(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                writer.Write(Center.X);
                writer.Write(Center.Y);
                writer.Write(Center.Z);

                writer.Write(Size.X);
                writer.Write(Size.Y);
                writer.Write(Size.Z);

                writer.Write(Depth);

                writer.Write(IsLeaf);

                if (IsLeaf)
                {
                    if (Element is ISerializable serializableElement)
                        serializableElement.Serialize(stream);

                    return;
                }

                foreach (SparseOctreeNode<T> child in Children)
                {
                    child?.Serialize(stream);
                }
            }
        }

        /// <summary>
        /// Deserializes a node from a binary stream.
        /// </summary>
        /// <param name="stream">The stream to deserialize from.</param>
        [Obsolete("Not Yet Functional")]
        public void Deserialize(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                Center = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Size = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                Depth = reader.ReadInt32();

                bool isLeaf = reader.ReadBoolean();
                if (isLeaf)
                {
                    Element = Activator.CreateInstance<T>();
                    if (Element is ISerializable serializableElement)
                    {
                        serializableElement.Deserialize(stream);
                    }

                    return;
                }

                Children = new SparseOctreeNode<T>[Capacity];
                for (int i = 0; i < Capacity; i++)
                {
                    SparseOctreeNode<T> child = new();
                    child.Deserialize(stream);
                    Children[i] = child;
                }
            }
        }

        #endregion
    }
}
