using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.TreeGenerator.Algorithm.Model;
using MCollections;

#if PoESkillTree_UseShortDistanceIndex == false
///<summary>
/// Unsigned Int type
///</summary>
using UnsignedIDType = System.UInt32;
///<summary>
/// Signed Int type
///</summary>
using NodeIDType = System.Int32;
#else
///<summary>
/// Unsigned Short type
///</summary>
using UnsignedIDType = System.UInt16;
///<summary>
/// Unsigned Short type
///</summary>
using NodeIDType = System.UInt16;
#endif

namespace PoESkillTree.TreeGenerator.Algorithm
{
#if PoESkillTree_EnableExtraGeneratorPath
    public class SecondaryPathList : List<ushort>
    {
        UnsignedIDType pathCost;
        ///// <summary>
        /////  Total Toggled Calculated Stat CvC
        ///// </summary>
        //float CalcStatCvc;
        ///// <summary>
        /////  Total Target PseudoStat+Normal Stat CvC
        ///// </summary>
        //float NormalStatCvc;
        ///// <summary>
        /////  Combination of CalcStatCvc+NormalStatCvc
        ///// </summary>
        //float TotalStatCvc;
    }
    public class SecondaryPathStore : HashSet<SecondaryPathList>
    {
        UnsignedIDType pathCost;
        HashSet<SecondaryPathList> SecondaryPaths;
    }
    /// <summary>
    ///  If PoESkillTree_EnableExtraGeneratorPath is enabled(not implemented yet), <br></br>
    ///  then value also stores alternative paths between start and end node(plus distance cost and stat score of possible path)
    /// </summary>
    public class PathList : List<ushort>
    {
        UnsignedIDType pathCost;
        SecondaryPathStore SecondaryPaths;
        ///// <summary>
        /////  Total Toggled Calculated Stat CvC
        ///// </summary>
        //float CalcStatCvc;
        ///// <summary>
        /////  Total Target PseudoStat+Normal Stat CvC
        ///// </summary>
        //float NormalStatCvc;
        ///// <summary>
        /////  Combination of CalcStatCvc+NormalStatCvc
        ///// </summary>
        //float TotalStatCvc;
    }
#endif

#if PoESkillTree_EnableExtraGeneratorPath==false
    public readonly struct DistanceLookup
    {
        private readonly
#if PoESkillTree_LinkDistancesByID
        MCollections.IndexedDictionary<NodeIDType, MCollections.IndexedDictionary<NodeIDType, UnsignedIDType>>
#else
        UnsignedIDType[][]
#endif
        _distances;

        public readonly NodeIDType CacheSize;

        public UnsignedIDType this[NodeIDType a, NodeIDType b] => _distances[a][b];

#if PoESkillTree_LinkDistancesByID
        public DistanceLookup(NodeIDType cacheSize, MCollections.IndexedDictionary<NodeIDType, MCollections.IndexedDictionary<NodeIDType, UnsignedIDType>>distances)
        {
            CacheSize = cacheSize;
            _distances = distances;
        }
#endif

        public DistanceLookup(NodeIDType cacheSize, UnsignedIDType[][] distances)
        {
            CacheSize = cacheSize;
#if PoESkillTree_LinkDistancesByID
            //Convert Array into Dictionary
            _distances = new IndexedDictionary<int, IndexedDictionary<int, uint>>();
            for (int XIndex=0; XIndex < distances.Length;++XIndex)
            {
                _distances.Add(XIndex, new IndexedDictionary<int, uint>());
                for (int YIndex = 0; YIndex < distances[XIndex].Length; ++YIndex)
                {
                    _distances[XIndex].Add(YIndex, distances[XIndex][YIndex]);
                }
            }
#else
            _distances = distances;
#endif
        }
    }
#endif

        public readonly struct ShortestPathLookup
    {
        private readonly
#if PoESkillTree_LinkDistancesByID
        Dictionary<NodeIDType, Dictionary<NodeIDType,
#if PoESkillTree_EnableExtraGeneratorPath
        PathList
#else
        List<ushort>
#endif
        >>
#else
        ushort[][][]
#endif
        _paths;

        public
#if PoESkillTree_EnableExtraGeneratorPath
#else
        IReadOnlyList<ushort>
#endif
        this[NodeIDType a, NodeIDType b]
            => _paths[a][b];

        public ShortestPathLookup(
#if PoESkillTree_LinkDistancesByID
        Dictionary<NodeIDType, Dictionary<NodeIDType,
#if PoESkillTree_EnableExtraGeneratorPath
        PathList
#else
        List<ushort>
#endif
        >>
#else
        ushort[][][]
#endif
        paths)
        {
            _paths = paths;
        }
    }

    /// <summary>
    /// Exception that is thrown if an operation can't be continued because the
    /// graph is disconnected.
    /// </summary>
    public class GraphNotConnectedException : Exception
    {
    }

    /// <summary>
    ///  Calculates and caches distances between nodes. Only relies on adjacency
    ///  information stored in the nodes.
    /// </summary>
    public class DistanceCalculator
    {

#if PoESkillTree_EnableExtraGeneratorPath==false
        private
#if PoESkillTree_LinkDistancesByID
        MCollections.IndexedDictionary<NodeIDType, MCollections.IndexedDictionary<NodeIDType, UnsignedIDType>>
#else
        UnsignedIDType[][]
#endif
        _distances;
#endif

        /// <summary>
        ///  StartNodeIndex(X),EndNodeIndex(Y), List of Shortest Path(Value)
        /// </summary>
        private
#if PoESkillTree_LinkDistancesByID
        Dictionary<NodeIDType, Dictionary<NodeIDType,
#if PoESkillTree_EnableExtraGeneratorPath
        PathList
#else
        List<ushort>
#endif
        >>
#else
        ushort[][][]
#endif
        _paths;

        /// <summary>
        /// The GraphNodes of which distances and paths are cached.
        /// The index in the Array equals their <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        public
#if PoESkillTree_LinkDistancesByID
        Dictionary<NodeIDType, GraphNode>//Remove value if keyed to null
#else
        GraphNode[]
#endif
        _nodes;

        /// <summary>
        /// Total number of within calculator cache<br></br>
        /// Node Ids use NodeIDType, so instead limiting path start and end node values as NodeIDType instead of as int fields
        /// </summary>
        public NodeIDType CacheSize { get; private set; }

        /// <summary>
        /// Retrieves the path distance from one node to another.
        /// CalculateFully must have been called or an exception will be thrown.
        /// </summary>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        /// <remarks>
        ///  If the nodes are not connected, 0 will be returned.
        ///  If at least one of the nodes is greater or equals CacheSize, a IndexOutOfRangeException will be thrown.
        /// </remarks>
        public UnsignedIDType this[NodeIDType a, NodeIDType b]
        {
            get => _distances[a][b];
            private set => _distances[a][b] = _distances[b][a] = value;
        }

        public DistanceLookup DistanceLookup
            => new DistanceLookup(CacheSize, _distances);

        public ShortestPathLookup ShortestPathLookup
            => new ShortestPathLookup(_paths);

        /// <summary>
        ///  Retrieves the shortest path from one node to another.
        /// </summary>
        /// <param name="a">The first graph node. (not null)</param>
        /// <param name="b">The second graph node. (not null)</param>
        /// <returns>The shortest path from a to b, not containing either and ordered from a to b or b to a.</returns>
        /// <remarks>
        ///  If the nodes are not connected, null will be returned.
        ///  If at least one of the nodes is greater or equals CacheSize, a IndexOutOfRangeException will be thrown.
        /// </remarks>
        public IReadOnlyList<ushort> GetShortestPath(NodeIDType a, NodeIDType b)
            => _paths[a][b];

        /// <summary>
        /// Sets the shortest path between the given two nodes.
        /// </summary>
        private void SetShortestPath(NodeIDType a, NodeIDType b,
#if PoESkillTree_LinkDistancesByID
#if PoESkillTree_EnableExtraGeneratorPath
        PathList
#else
        List<ushort>
#endif
#else
        ushort[]
#endif
        path)
        {
            _paths[a][b] = _paths[b][a] = path;
        }

        /// <summary>
        /// Sets the shortest path between the given two nodes to be no path at all
        /// </summary>
        private void SetShortestPathToDeadEnd(NodeIDType a, NodeIDType b)
        {
            _paths[a][b] = _paths[b][a] = new
#if PoESkillTree_LinkDistancesByID
#if PoESkillTree_EnableExtraGeneratorPath
            PathList();
#else
            List<ushort>();
#endif
#else
            ushort[0];
#endif
        }

        /// <summary>
        /// Returns the GraphNode with the specified <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        public GraphNode IndexToNode(NodeIDType index)
        {
            return _nodes[index];
        }

        /// <summary>
        /// Returns true iff the given nodes are connected.
        /// </summary>
        public bool AreConnected(GraphNode a, GraphNode b)//=> GetShortestPath(a.DistancesIndex, b.DistancesIndex) != null;
        {
#if PoESkillTree_UseShortDistanceIndex==false
            return GetShortestPath(a.DistancesIndex, b.DistancesIndex) != null;
#else
            return a.DistancesIndex==null||b.DistancesIndex==null?false:GetShortestPath((ushort)a.DistancesIndex, (ushort)b.DistancesIndex) != null;
#endif
        }

        /// <summary>
        /// Returns true iff the given nodes are connected.
        /// </summary>
        public bool AreConnectedById(NodeIDType a, NodeIDType b)//=> GetShortestPath(a, b) != null;
        {
            return GetShortestPath(a, b) != null;
        }

        /// <summary>
        /// Merges both nodes so that distances and paths to any of the two nodes are overwritten
        /// to the shortest distance and path to any of the two nodes or the nodes on the shortest path
        /// between them.
        /// Only the paths and distances from and to <paramref name="into"/> are updated.
        /// </summary>
        public void MergeInto(NodeIDType x, NodeIDType into)
        {
            var path = new HashSet<ushort>(GetShortestPath(x, into));
            this[x, into] = 0;
            SetShortestPathToDeadEnd(x, into);
            UnsignedIDType XSize; UnsignedIDType YSize;
            for (NodeIDType i = 0; i < CacheSize; ++i)
            {
                if (i == into || i == x) continue;

                var XPath = GetShortestPath(i, x).Where(n => !path.Contains(n))
#if PoESkillTree_LinkDistancesByID
                ;
#else
                .ToArray();
#endif
                var YPath = GetShortestPath(i, into).Where(n => !path.Contains(n))
#if PoESkillTree_LinkDistancesByID
                ;
#else
                .ToArray();
#endif
                XSize = (UnsignedIDType)
#if PoESkillTree_LinkDistancesByID
                XPath.Count();
#else
                XPath.Length;
#endif
                YSize = (UnsignedIDType)
#if PoESkillTree_LinkDistancesByID
                YPath.Count();
#else
                YPath.Length;
#endif
                if (XSize < YSize)
                {
                    this[i, into] = ++XSize;
                    SetShortestPath(i, into,
#if PoESkillTree_LinkDistancesByID
#if PoESkillTree_EnableExtraGeneratorPath
                    (PathList)
#else
                    (List<ushort>)
#endif
#endif
                    XPath);
                }
                else
                {
                    this[i, into] = ++YSize;
                    SetShortestPath(i, into,
#if PoESkillTree_LinkDistancesByID
#if PoESkillTree_EnableExtraGeneratorPath
                    (PathList)
#else
                    (List<ushort>)
#endif
#endif
                    YPath);
                }
            }
        }

        /// <summary>
        /// Calculates and caches all distances between the given nodes.
        /// Sets DistancesIndex of the nodes as incremental index in the cache starting from 0.
        /// </summary>
        public DistanceCalculator(IReadOnlyList<GraphNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");

            CacheSize = 
#if PoESkillTree_LinkDistancesByID && PoESkillTree_UseShortDistanceIndex
            (ushort)
#endif
            nodes.Count;
#if PoESkillTree_LinkDistancesByID//Potential fix for index breaking on node removal
            _nodes = new Dictionary<NodeIDType, GraphNode>(nodes.Count);
            _distances = new MCollections.IndexedDictionary<NodeIDType, MCollections.IndexedDictionary<NodeIDType, UnsignedIDType>>();
            _paths = new Dictionary<NodeIDType, Dictionary<NodeIDType, List<ushort>>>(nodes.Count);
#else
            _nodes = new GraphNode[CacheSize];
            _distances = new UnsignedIDType[CacheSize][];
            _paths = new ushort[CacheSize][][];
#endif
            for (NodeIDType i = 0; i < CacheSize; ++i)
            {
                nodes[
#if PoESkillTree_UseShortDistanceIndex
                (int)
#endif
                i].DistancesIndex = i;
#if PoESkillTree_LinkDistancesByID
                _nodes.Add(i, nodes[
#if PoESkillTree_UseShortDistanceIndex
                (int)
#endif
                 i]);
                _distances.Add(i, new MCollections.IndexedDictionary<NodeIDType, UnsignedIDType>());
                _paths.Add(i, new Dictionary<NodeIDType,
#if PoESkillTree_EnableExtraGeneratorPath
                 PathList
#else
                 List<ushort>
#endif
                 >(nodes.Count));
#else
                _nodes[i] = nodes[i];
                _distances[i] = new UnsignedIDType[CacheSize];
                _paths[i] = new ushort[CacheSize][];
#endif
            }

            foreach (var node in nodes)
            {
                Dijkstra(node);
            }
        }

        /// <summary>
        /// Removes the given nodes from the cache.
        /// Resets DistancesIndex of removedNodes to -1 and of remainingNodes to be
        /// incremental without holes again.
        /// O(|removedNodes| + |remainingNodes|^2)
        /// </summary>
        /// <returns>List of the remaining node. Ordered by their distance index.</returns>
        public List<GraphNode> RemoveNodes(IEnumerable<GraphNode> removedNodes)
        {
            if (removedNodes == null) throw new ArgumentNullException("removedNodes");

            var removed = new bool[CacheSize];
            foreach (var node in removedNodes)
            {
#if PoESkillTree_UseShortDistanceIndex
                if (node.DistancesIndex == null)
                    continue;
#endif
#if PoESkillTree_LinkDistancesByID

#endif
                removed[
#if PoESkillTree_UseShortDistanceIndex
                (int)
#endif
                node.DistancesIndex] = true;
#if PoESkillTree_UseShortDistanceIndex==false
                node.DistancesIndex = -1;
#else
                node.DistancesIndex = null;
#endif
            }
            var remainingNodes = new List<GraphNode>();
            for (NodeIDType i = 0; i < CacheSize; ++i)
            {
                if (!removed[i])
                    remainingNodes.Add(IndexToNode(i));
            }

#if PoESkillTree_LinkDistancesByID == false
            var oldDistances = _distances;
            var oldPaths = _paths;
            CacheSize = remainingNodes.Count;
            _distances = new UnsignedIDType[CacheSize][];
            _paths = new ushort[CacheSize][][];

            for (var i = 0; i < CacheSize; ++i)
            {
                _distances[i] = new UnsignedIDType[CacheSize];
                _paths[i] = new ushort[CacheSize][];
                var oldi = remainingNodes[i].DistancesIndex;
                for (var j = 0; j < CacheSize; ++j)
                {
                    var oldj = remainingNodes[j].DistancesIndex;
                    _distances[i][j] = oldDistances[oldi][oldj];
                    _paths[i][j] = oldPaths[oldi][oldj];
                }
            }

            _nodes = new GraphNode[CacheSize];
            for (var i = 0; i < CacheSize; ++i)
            {
                remainingNodes[i].DistancesIndex = i;
                _nodes[i] = remainingNodes[i];
            }
#endif

            return remainingNodes;
        }

        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node and calculate distances and shortest paths to all reachable relevant nodes.
        /// </summary>
        /// <param name="start">The starting node. (not null)</param>
        private void Dijkstra(GraphNode start)
        {
            if (start == null) throw new ArgumentNullException("start");

            AddEdgeWithNoLength(start, start, null!);

            // The last newly found nodes.
            var front = new HashSet<GraphNode>() { start };
            // The already visited nodes.
            var visited = new HashSet<GraphNode>() { start };
            // The dictionary of the predecessors of the visited nodes.
            var predecessors = new Dictionary<ushort, ushort>();
            // The traversed distance from the starting node in edges.
            NodeIDType distFromStart = 0;

            while (front.Count > 0)
            {
                var newFront = new HashSet<GraphNode>();

                foreach (var node in front)
                {
                    foreach (var adjacentNode in node.Adjacent)
                    {
                        if (visited.Contains(adjacentNode))
                            continue;

                        predecessors[adjacentNode.Id] = node.Id;
                        
                        if (adjacentNode.DistancesIndex >= 0)
                            AddEdge(start, adjacentNode, distFromStart, predecessors);

                        newFront.Add(adjacentNode);
                        visited.Add(adjacentNode);
                    }
                }

                front = newFront;
                ++distFromStart;
            }
        }


        /// <summary>
        /// Adds the distance and shortest path between from and to to the
        /// respective dictionaries if not already present.
        /// </summary>
        private void AddEdge(GraphNode from, GraphNode to, NodeIDType distFromStart, IDictionary<ushort, ushort> predecessors)
        {
            NodeIDType length =
#if PoESkillTree_UseShortDistanceIndex
            (ushort)//Because of C#'s spec using int operator for ushort operations
#endif
            (distFromStart + 1);

#if PoESkillTree_UseShortDistanceIndex
            if (from.DistancesIndex == null || to.DistancesIndex == null)
                return;
#endif
            var i1 =
#if PoESkillTree_UseShortDistanceIndex
            (ushort)
#endif
            from.DistancesIndex;
            var i2 =
#if PoESkillTree_UseShortDistanceIndex
            (ushort)
#endif
            to.DistancesIndex;
            if (_paths[i1][i2] != null) return;

            if (distFromStart > 0)
            {
                var path = GenerateShortestPath(from.Id, to.Id, predecessors, length);
                this[i1, i2] = (UnsignedIDType) length;
                SetShortestPath(i1, i2, path);
            }
            else
            {
                this[i1, i2] = 0;
                SetShortestPathToDeadEnd(i1, i2);
            }
        }


        /// <summary>
        /// Adds the distance and shortest path between from and to to the
        /// respective dictionaries if not already present.
        /// </summary>
        private void AddEdgeWithNoLength(GraphNode from, GraphNode to, IDictionary<ushort, ushort> predecessors)
        {
#if PoESkillTree_UseShortDistanceIndex
            if (from.DistancesIndex == null || to.DistancesIndex == null)
                return;
#endif
            var i1 =
#if PoESkillTree_UseShortDistanceIndex
            (ushort)
#endif
            from.DistancesIndex;
            var i2 =
#if PoESkillTree_UseShortDistanceIndex
            (ushort)
#endif
            to.DistancesIndex;
            if (_paths[i1][i2] != null) return;

            this[i1, i2] = 0;
            SetShortestPathToDeadEnd(i1, i2);
        }

        ///// <summary>
        ///// Adds the distance and shortest path between from and to to the
        ///// respective dictionaries if not already present.
        ///// </summary>
        //private void AddEdgeWithLength(GraphNode from, GraphNode to, NodeIDType distFromStart, IDictionary<ushort, ushort> predecessors)
        //{
        //    var length = distFromStart + 1;
            
        //    var i1 = from.DistancesIndex;
        //    var i2 = to.DistancesIndex;
        //    if (_paths[i1][i2] != null) return;

        //    var path = GenerateShortestPath(from.Id, to.Id, predecessors, length);
        //    this[i1, i2] = (UnsignedIDType) length;
        //    SetShortestPath(i1, i2, path);
        //}

        /// <summary>
        /// Generates the shortest path from target to start by reading it out of the predecessors-dictionary.
        /// The dictionary must have a path from target to start stored.
        /// </summary>
        /// <param name="start">The starting node</param>
        /// <param name="target">The target node</param>
        /// <param name="predecessors">Dictonary with the predecessor of every node</param>
        /// <param name="length">Length of the shortest path</param>
        /// <returns>The shortest path from start to target, not including either. The Array is ordered from target to start</returns>
        private static
#if PoESkillTree_LinkDistancesByID
        List<ushort>
#else
        ushort[]
#endif
        GenerateShortestPath(ushort start, ushort target, IDictionary<ushort, ushort> predecessors, NodeIDType length)
        {
#if PoESkillTree_LinkDistancesByID
            var path = new List<ushort>(length - 1);
#else
            var path = new ushort[length - 1];
#endif
            var i = 0;
            for (var node = predecessors[target]; node != start; node = predecessors[node], ++i)
            {
                path[i] = node;
            }
            return path;
        }
    }
}
