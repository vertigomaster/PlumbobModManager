using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

//TODO: eventually swap out Vector3 w/ something else if we want to use beyond Unity
// using UnityEngine;

namespace IDEK.Tools.DataStructures.Trees
{
    public interface ISpatial { }
    public interface ISpatial<T> : ISpatial
    {
        T Position { get; }
    }

    public static class OctreeExtensions
    {


        /// <summary>
        /// Gets the elementPosition of a data sourcePoint.
        /// </summary>
        /// <param name="point">The data sourcePoint.</param>
        /// <returns>The elementPosition of the data sourcePoint.</returns>
        public static TSpaceType GetPosition<TSpaceType>(this SparseOctree<ISpatial<TSpaceType>> tree, ISpatial<TSpaceType> dataPoint) where TSpaceType : ISpatial
        {
            return dataPoint.Position;
        }

        //public static Vector3 GetPosition(this SparseOctree<Vector3> tree, )
    }

    public class Octree<T>
    {

    }

    public enum DepthModeEnum
    {
        StaticDepth,
        DeriveFromMinSideLength
    }

    /// <summary>
    /// Represents a generic sparse octree.
    /// </summary>
    public class SparseOctree<T> : Octree<T>, ISerializable
    {
        #region Enums


        #endregion

        #region Fields

        private int? _cachedMaxDepth;
        private float? _cachedMinSideLength;

        #endregion

        #region Properties

        public SparseOctreeNode<T> Root { get; protected set; }

        public float MinSideLength 
        {
            get => _CalcMinSideLength();
            set => _SetMinSideLength(value);
        }
        
        public int MaxDepth 
        {
            get => _CalcMaxDepth();
            set => _SetMaxDepth(value);
        }

        #endregion

        #region Constructors

        public SparseOctree()
        {
            Root = new();
        }

        public SparseOctree(Vector3 center, Vector3 size)
        {
            Root = new SparseOctreeNode<T>(center, size);
        }

        #endregion

        #region Public Methods

        [Serializable]
        public class OctreeBuildParameters
        {
#if ODIN_INSPECTOR
            [ShowInInspector, ReadOnly]
#endif
            public Vector3 TotalSize { get; set; } //not serialized for Unity editor

            public DepthModeEnum DepthMode { get; set; } = DepthModeEnum.StaticDepth;

#if ODIN_INSPECTOR
            [ShowIf("@DepthMode == DepthModeEnum.DeriveFromMinSideLength")]
#endif
            private float _minSideLength;

#if ODIN_INSPECTOR
            [ShowIf("@DepthMode == DepthModeEnum.StaticDepth")]
#endif
            private int _maxDepth = 5;

            public float? MinSideLength
            {
                get => DepthMode == DepthModeEnum.DeriveFromMinSideLength ? _minSideLength : null;
                set => _minSideLength = value.Value;
            }

            public int? MaxDepth
            {
                get => DepthMode == DepthModeEnum.StaticDepth ? _maxDepth : null;
                set => _maxDepth = value.Value;
            }

            public OctreeBuildParameters Clone()
            {
                return new OctreeBuildParameters
                {
                    TotalSize = this.TotalSize,
                    DepthMode = this.DepthMode,
                    _minSideLength = this._minSideLength,
                    _maxDepth = this._maxDepth
                };
            }
        }

        /// <summary>
        /// Builds a sparse octree from the list of data points.
        /// </summary>
        /// <param name="parameters">The parameters for building the octree.</param>
        /// <returns>The root currentNode of the octree.</returns>
        public static TRuntimeType BuildOctree<TRuntimeType>(List<T> points, OctreeBuildParameters parameters) where TRuntimeType : SparseOctree<T>, new()
        {
            TRuntimeType octree = new();
            octree.Root = new(Vector3.Zero, parameters.TotalSize);

            if (parameters.MaxDepth != null && parameters.MinSideLength != null)
            {
                ConsoleLog.LogError("Cannot set both maxDepth and minSideLength at the same time.");
                throw new InvalidOperationException("Cannot set both maxDepth and minSideLength at the same time.");
            }

            if (parameters.MaxDepth != null)
            {
                octree.MaxDepth = parameters.MaxDepth.Value;
            }
            else if (parameters.MinSideLength != null)
            {
                octree.MinSideLength = parameters.MinSideLength.Value;
            }
            else
            {
                ConsoleLog.LogError("Must set either maxDepth or minSideLength.");
                throw new InvalidOperationException("Must set either maxDepth or minSideLength.");
                return null;
            }
            
            for(int i = 0; i < points.Count; i++)
            {
                ConsoleLog.Log("Build Octree | Inserting Element #" + i);
                T point = points[i];
                octree.InsertElement(point);
            }
            return octree;
        }

        /// <summary>
        /// Checks if a elementPosition is within a certain distance of the BOUNDARY of the given octree node. 
        /// </summary>
        /// <param name="position">The elementPosition to check.</param>
        /// <param name="node">The octree currentNode.</param>
        /// <param name="distance">The distance to check.</param>
        /// <returns>True if the elementPosition is within the distance of the currentNode (or within the node boundary directly), otherwise false.</returns>
        public static bool IsPositionWithinDistanceOfNodeBoundary(Vector3 position, SparseOctreeNode<T> node, float distance)
        {
            Vector3 halfSize = node.Extents;
            Vector3 min = node.Center - halfSize;
            Vector3 max = node.Center + halfSize;

            bool result = (position.X >= min.X - distance && position.X <= max.X + distance) &&
                   (position.Y >= min.Y - distance && position.Y <= max.Y + distance) &&
                   (position.Z >= min.Z - distance && position.Z <= max.Z + distance);

            ConsoleLog.Log($"Verdict: {result} | Min: " + min + " | Max: " + max + " | Position: " + position + " | Distance: " + distance);
            
            return result;
        }

        /// <summary>
        /// The only valid way to insert new points is via the root.
        /// </summary>
        /// <param name="element"></param>
        public void InsertElement(T element) => _InsertElementAtNode(Root, element);

        #endregion

        #region Private Methods

        #region Parameter Changes

        private int _CalcMaxDepth()
        {
            if (_cachedMaxDepth.HasValue)
                return _cachedMaxDepth.Value;

            Vector3 currentSize = Root.Size;
            int depth = 0;

            while(currentSize.MinComponent() > MinSideLength)
            {
                currentSize /= 2;
                depth++;
            }

            _cachedMaxDepth = depth;
            return depth;
        }

        private float _CalcMinSideLength()
        {
            if (_cachedMinSideLength.HasValue)
                return _cachedMinSideLength.Value;

            Vector3 currentSize = Root.Size;
            for(int i = 0; i < MaxDepth; i++)
            {
                currentSize /= 2;
            }

            float minSideLength = currentSize.MinComponent();
            _cachedMinSideLength = minSideLength;
            return minSideLength;
        }

        private void _SetMinSideLength(float minSideLength)
        {
            _cachedMinSideLength = minSideLength;
            _cachedMaxDepth = null;
        }

        private void _SetMaxDepth(int maxDepth)
        {
            _cachedMaxDepth = maxDepth;
            _cachedMinSideLength = null;
        }

        #endregion

        private static TSpatialUnit _GetPosition<TSpatialUnit>(T element)
        {
            return element switch
            {
                TSpatialUnit pointVector => pointVector,
                ISpatial<TSpatialUnit> spatialPoint => spatialPoint.Position,
                _ => throw new InvalidOperationException("Unsupported element type! Must be either a Vector3 or a type implementing ISpatial<T>!")
            };
        }

        /// <summary>
        /// Inserts a data sourcePoint into the octree.
        /// </summary>
        /// <param name="node">The current currentNode.</param>
        /// <param name="element">The data sourcePoint to insert.</param>
        private void _InsertElementAtNode(SparseOctreeNode<T> startingNode, T element)
        {
            //PROBLEM: If two elements are too close together, we end up doing millions of operations to try and put them in distinct cells; otherwise they just keep swapping into the next depth down over and over
            //and if they're at the same position, it'll never end.
            //SOLUTION: Re-implement min-size/max-depth and allow multiple elements, but only at the bottom-most depth
            SparseOctreeNode<T> currentNode = startingNode;

            //technically not needed, since we have a return statement,
            //but using while(true) tends to give people a heart attack, lol 
            bool elementInserted = false;

            //TODO: this is looping infinitely, unfortunately. Something's gone quite wrong somewhere.
            while(!elementInserted)
            {
                //TODO: somehow rework this to use a queue instead of recursive calls; the callstack can't take it
                if(currentNode.IsLeaf) //at bottom. if empty, fill it in. Otherwise, subdivide and fill in the next appropriate level.
                {
                    if (currentNode.Depth >= MaxDepth || currentNode.IsEmpty)
                    {
                        currentNode.InsertElement(element);
                        elementInserted = true;
                        return;
                    }

                     //filled already; we have to set up child nodes
                    //not a leaf anymore! keep going for non-leaf
                    _SubdivideNodeAndElements(currentNode);
                }

                //tunnel down and try again
                int index = _CalcBestIndexForElement(currentNode, element);
                currentNode = currentNode.Children[index];
            } 
        }

        /// <summary>
        /// Subdivides a currentNode into eight children, which also inherently distributes its elements.
        /// </summary>
        /// <param name="node">The currentNode to subdivide.</param>
        private void _SubdivideNodeAndElements(SparseOctreeNode<T> node)
        {
            //create our children array (doing so only as needed cuts down on memory usage significantly)
            node.Children = new SparseOctreeNode<T>[node.Capacity];

            //we subdivide into halves; 8 = 2^3 so each child has half the parent's side lengths
            Vector3 childSize = node.Size / 2f;

            _CreateChildNodes(node, childSize);

            //we have to move our element to the best-fitting leaf.
            _PushNodeElementsToLeaves(node);
        }

        /// <summary>
        /// Builds child nodes that are ready to be used. They are configured but empty.
        /// </summary>
        /// <param name="node"></param>
        /// <param="childSize"></param>
        /// <remarks>
        /// TODO: Find better way to not hardcode the counts so we can more easily
        /// extend the library to Quadtrees and n-dimensional spatial/sparse trees
        /// </remarks>
        private static void _CreateChildNodes(SparseOctreeNode<T> node, Vector3 childSize)
        {
            for(int i = 0; i < 8; i++)
            {
                Vector3 offset = _CalcOffsetFromIndex(childSize, i);

                node.Children[i] = new SparseOctreeNode<T>(node.Center + offset, childSize);
                node.Children[i].Depth = node.Depth + 1;
            }
        }

        private static Vector3 _CalcOffsetFromIndex(Vector3 childSize, int index)
        {
            //utilizing fun bit math to quickly and concisely route things
            return new(
                (index & 1) == 0 ? -childSize.X / 2f : childSize.X / 2f,
                (index & 2) == 0 ? -childSize.Y / 2f : childSize.Y / 2f,
                (index & 4) == 0 ? -childSize.Z / 2f : childSize.Z / 2f
            );
        }

        /// <summary>
        /// Intended as a deletion and re-insertion to resolve structural updates
        /// </summary>
        /// <param name="node"></param>
        private void _PushNodeElementsToLeaves(SparseOctreeNode<T> node)
        {
            if(node.IsLeaf) return; //already a leaf, nothing to push

            //we push them all down first before updating the parent Elements array.
            //This is only safe to do because everything is propogating downward, not upward.
            foreach(T element in node.Elements)
            {
                int childNodeIndexToPushElementTo = _CalcBestIndexForElement(node, element);
                
                //Usually the child is already a leaf, but this approach
                //ensure it works for non-leaf children.
                _InsertElementAtNode(node.Children[childNodeIndexToPushElementTo], element);
            }

            //removes the stale references from the parent
            node.EmptyOut(); 
        }

        /// <summary>
        /// Returns index of node�s child that best fits the given element
        /// </summary>
        /// <param name="node">The current currentNode.</param>
        /// <param name="element">The data sourcePoint to check.</param>
        /// <returns>The childNodeIndexToPushElement of the child currentNode.</returns>
        /// <remarks>
        /// Gets the childNodeIndexToPushElement of the child currentNode that contains the given elementPosition.
        /// </remarks>
        private static int _CalcBestIndexForElement(SparseOctreeNode<T> node, T element)
        {
            Vector3 elementPosition = _GetPosition<Vector3>(element);
            int index = 0;
            if (elementPosition.X > node.Center.X) index |= 1;
            if (elementPosition.Y > node.Center.Y) index |= 2;
            if (elementPosition.Z > node.Center.Z) index |= 4;
            return index;
        }


        #endregion

        #region Serialization

        /// <summary>
        /// Serializes the octree to a binary stream.
        /// </summary>
        /// <param name="stream">The stream to serialize to.</param>
        public void Serialize(Stream stream)
        {
            Root.Serialize(stream);
        }

        /// <summary>
        /// Deserializes the octree from a binary stream.
        /// </summary>
        /// <param="stream">The stream to deserialize from.</param>
        public void Deserialize(Stream stream)
        {
            SparseOctreeNode<T> newRoot = new();
            newRoot.Deserialize(stream);
            Root = newRoot;
        }

        #endregion
    }
}
