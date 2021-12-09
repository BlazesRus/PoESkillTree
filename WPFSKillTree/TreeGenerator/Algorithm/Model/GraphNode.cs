﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.SkillTreeFiles;
using System.Diagnostics.CodeAnalysis;

namespace PoESkillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    ///  Represents a node (or a collection thereof) in the
    ///  simplified skill tree.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class GraphNode: IComparable<GraphNode>
    {
        private readonly ushort _id;
        /// <summary>
        /// Gets the Id of the corresponding SkillNode.
        /// </summary>
        public ushort Id { get { return _id; } }

#if DEBUG
        private string Name { get { return SkillTree.Skillnodes[_id].Name; } }
#endif

#if DEBUG
        private int distancesIndex;

        /// <summary>
        /// Gets or sets the index of the node by which it is represented in <see cref="DistanceLookup"/>
        /// and other classes.
        /// </summary>
        public int DistancesIndex 
        { 
            get=>distancesIndex;
            set
            {
                if (value != distancesIndex)
                {
                    distancesIndex = value;
                }
            }
        }
#else
        /// <summary>
        /// Gets or sets the index of the node by which it is represented in <see cref="DistanceLookup"/>
        /// and other classes.
        /// </summary>
        public int DistancesIndex { get; set; }
#endif

        private List<GraphNode> _adjacent = new List<GraphNode>();
        /// <summary>
        /// Gets the nodes adjacent to this node.
        /// </summary>
        public IReadOnlyCollection<GraphNode> Adjacent { get { return _adjacent;} }

        private readonly List<ushort> _nodes;
        /// <summary>
        /// Gets the (skill tree) nodes which are represented by this node.
        /// </summary>
        public IReadOnlyCollection<ushort> Nodes { get { return _nodes; } }

        /// <summary>
        /// Creates a new GraphNode representing a single skill tree node.
        /// </summary>
        public GraphNode(ushort id)
        {
            DistancesIndex = -1;
            _nodes = new List<ushort> {id};
            _id = id;
        }

        /// <summary>
        /// Creates a new GraphNode representing the given skill tree nodes.
        /// </summary>
        public GraphNode(IEnumerable<ushort> nodes)
        {
            DistancesIndex = -1;
            _nodes = new List<ushort>(nodes);
            if (!_nodes.Any()) throw new ArgumentException("Node enumerable must not be empty", "nodes");
            _id = _nodes.First();
        }

        /// <summary>
        /// Adds the given node to this node's neighbors.
        /// </summary>
        public void AddNeighbor(GraphNode other)
        {
            if (!_adjacent.Contains(other))
            {
                _adjacent.Add(other);
            }
        }

        /// <summary>
        /// Merges the given GraphNode with this node. The collections of adjacent GraphNode are unioned (with neither
        /// this nor the other node contained in the collection). The other GraphNode's represented skill tree nodes
        /// and the given path between them are added to the represented nodes ones of this GraphNode.
        /// </summary>
        public void MergeWith(GraphNode other, IEnumerable<ushort> path)
        {
            _adjacent = _adjacent.Union(other._adjacent).Where(n => n != this && n != other).ToList();
            _nodes.AddRange(other._nodes);
            _nodes.AddRange(path);
            other._adjacent.Clear();
            other._nodes.Clear();
        }

        public int CompareTo([AllowNull] GraphNode other)
        {//https://docs.microsoft.com/en-us/dotnet/api/system.nullable.compare?redirectedfrom=MSDN&view=net-6.0#System_Nullable_Compare__1_System_Nullable___0__System_Nullable___0__
            if (this == null)
                return other==null?0: Id*-1;
            else if (other == null)
                return Id;
            else
                return Id.CompareTo(other.Id);
        }
    }
}