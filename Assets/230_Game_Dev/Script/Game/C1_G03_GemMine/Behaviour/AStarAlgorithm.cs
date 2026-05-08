using beyondi.Util;
using System;
using System.Collections.Generic;

namespace DoDoEng.Game.C1_G03
{
    public class AStarAlgorithm
    {
        // Methods
        public static Cell[] FindPath(MapData<IMapObject> map, Cell start, Cell end)
        {
            var openSet = new PriorityQueue<AStarNode>();
            var closedSet = new HashSet<Cell>();

            var nodeStart = new AStarNode(start, null, 0, heuristic(start, end));
            openSet.Enqueue(nodeStart);

            while (openSet.Count > 0)
            {
                var cNode = openSet.Dequeue();
                if (isAdjacentTo(cNode.Position, end) && countOfPath(cNode) > 2) // 바로 인접한 경우는 직접 가지 않도록
                    return reconstructPath(cNode, end);

                var cCell = cNode.Position;
                closedSet.Add(cCell);
                foreach (var nCell in neighborsOf(map, cCell))
                {
                    if (closedSet.Contains(nCell))
                        continue;

                    var tentativeGScore = cNode.GScore + 1;
                    var nNode = new AStarNode(nCell, cNode, tentativeGScore, heuristic(nCell, end));
                    if (!openSet.Contains(nNode) || tentativeGScore < nNode.GScore)
                        openSet.Enqueue(nNode);
                }

                InfiniteLoopDetector.Run(1000);
            }

            return null;
        }



        // Functions
        static int heuristic(Cell a, Cell b)
        {
            // Manhattan distance as a heuristic
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

            // Chebyshev distance
            //return Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }
        static Cell[] neighborsOf(MapData<IMapObject> map, Cell cell)
        {
            var list = new List<Cell>();
            foreach (var dir in UtilEnum.GetValues<Direction>())
            {
                var cellN = cell + dir;
                var objN = map[cellN];

                if (objN.MapObject == MapObject.None ||
                    objN.MapObject == MapObject.Road)
                    list.Add(cellN);
            }

            return list.ToArray();
        }
        static Cell[] reconstructPath(AStarNode node, Cell target)
        {
            var path = new List<Cell>();
            while (node != null)
            {
                path.Insert(0, node.Position); // Add to the beginning of the list to reverse the order
                node = node.Parent;
            }
            path.Add(target);

            return path.ToArray();
        }
        static int countOfPath(AStarNode node)
        {
            var count = 0;
            while (node != null)
            {
                count++;
                node = node.Parent;
            }

            return count;
        }
        static bool isAdjacentTo(Cell cell, Cell cellTarget)
        {
            foreach (var dir in UtilEnum.GetValues<Direction>())
            {
                var cellN = cell + dir;
                if (cellN == cellTarget)
                    return true;
            }

            return false;
        }
    }



    public class AStarNode : IComparable<AStarNode>
    {
        // Properties
        public Cell Position { get; }
        public AStarNode Parent { get; }
        public int GScore { get; }
        public int HScore { get; }
        public int FScore => GScore + HScore;

        // Methods : ctor.
        public AStarNode(Cell position, AStarNode parent, int gScore, int hScore)
        {
            Position = position;
            Parent = parent;
            GScore = gScore;
            HScore = hScore;
        }



        // IComparable
        public int CompareTo(AStarNode other)
        {
            return FScore.CompareTo(other.FScore);
        }
    }

    public class PriorityQueue<T> where T : IComparable<T>
    {
        // Properties
        public int Count => heap.Count;

        // Methods : ctor.
        public PriorityQueue()
        {
            heap = new List<T>();
        }

        // Methods
        public void Enqueue(T item)
        {
            heap.Add(item);

            var currentIndex = heap.Count - 1;
            while (currentIndex > 0)
            {
                var parentIndex = (currentIndex - 1) / 2;
                if (heap[currentIndex].CompareTo(heap[parentIndex]) >= 0)
                    break;

                swap(currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
        }
        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("PriorityQueue is empty.");

            var result = heap[0];
            var lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);

            var currentIndex = 0;
            while (true)
            {
                var leftChildIndex = 2 * currentIndex + 1;
                var rightChildIndex = 2 * currentIndex + 2;

                if (leftChildIndex >= heap.Count)
                    break;

                var smallerChildIndex = leftChildIndex;
                if (rightChildIndex < heap.Count && heap[rightChildIndex].CompareTo(heap[leftChildIndex]) < 0)
                    smallerChildIndex = rightChildIndex;

                if (heap[currentIndex].CompareTo(heap[smallerChildIndex]) <= 0)
                    break;

                swap(currentIndex, smallerChildIndex);
                currentIndex = smallerChildIndex;
            }

            return result;
        }
        public bool Contains(T item)
        {
            return heap.Contains(item);
        }



        // Fields
        private List<T> heap;

        // Functions
        private void swap(int index1, int index2)
        {
            T temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }
    }
}
