using beyondi.Util;
using DoDoEng.Common;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public enum Direction { L, R, T, B };
    public class MapNavigator : MonoBehaviour
    {
        // Properties
        public Cell PlayerCell => playerCell;

        // Methods
        public void Build(Grid grid, int cols, int rows)
        {
            LOG.Function(this);

            mapObject = new MapData<IMapObject>(cols, rows);
            mapVisit = new MapData<bool>(cols, rows);

            var cell = new Vector3Int();
            for (var r = 0; r < mapObject.Rows; r++)
            {
                for (var c = 0; c < mapObject.Cols; c++)
                {
                    cell.x = c;
                    cell.y = r;

                    var center = grid.GetCellCenterWorld(cell);
                    var col = Physics2D.OverlapPoint(center);
                    var obj = col?.GetComponent<IMapObject>();
                    mapObject[c, r] = obj ?? TheEmpty;
                }
            }
            for (var r = 0; r < mapObject.Rows; r++)
            {
                mapObject[0, r] = TheWall;
                mapObject[cols - 1, r] = TheWall;
            }
            for (var c = 0; c < mapObject.Cols; c++)
            {
                mapObject[c, 0] = TheWall;
                mapObject[c, rows - 1] = TheWall;
            }
        }
        public void Build(Grid grid, int cols, int rows, Gem[] gems, ObstacleArea[] obstacleAreas)
        {
            LOG.Function(this, $"cols:{cols} rows:{rows} gems:{gems.Length} oArea:{obstacleAreas.Length}");

            playerCell = null;

            mapObject = new MapData<IMapObject>(cols, rows);
            mapVisit = new MapData<bool>(cols, rows);

            for (var r = 0; r < mapObject.Rows; r++)
            {
                for (var c = 0; c < mapObject.Cols; c++)
                    mapObject[c, r] = TheEmpty;
            }
            for (var r = 0; r < mapObject.Rows; r++)
            {
                mapObject[0, r] = TheWall;
                mapObject[cols - 1, r] = TheWall;
            }
            for (var c = 0; c < mapObject.Cols; c++)
            {
                mapObject[c, 0] = TheWall;
                mapObject[c, rows - 1] = TheWall;
            }

            foreach (var gem in gems)
            {
                var cell = grid.WorldToCell(gem.transform.position);

                LOG.Info($"GEM : {cell.x},{cell.y} => {gem.Word}", this);

                mapObject[cell.x, cell.y] = gem;
            }

            foreach (var area in obstacleAreas)
            {
                var cell = grid.WorldToCell(area.transform.position);

                LOG.Info($"OBSTACLE : {cell.x},{cell.y}", this);

                mapObject[cell.x, cell.y] = area;
            }
        }

        // Methods
        public bool CanPlaceRoadBlock(Cell cell)
        {
            if (cell.X < 0) return false;
            if (cell.Y < 0) return false;
            if (cell.X >= mapObject.Cols) return false;
            if (cell.Y >= mapObject.Rows) return false;

            var type = mapObject[cell]?.MapObject ?? MapObject.None;
            return type == MapObject.None || type == MapObject.Road;
        }
        public void PlacePlayer(Cell cell)
        {
            LOG.Function(this, $"{cell.X},{cell.Y}");

            if (playerCell != null)
                mapObject[playerCell] = TheEmpty;

            playerCell = cell;
            mapObject[cell] = ThePlayer;
        }
        public void PlaceRoad(Cell cell, Road road, out bool connectedToPlayerOrGem, out Road replacedRoad)
        {
            LOG.Function(this, $"{cell} {road.gameObject.name}");

            replacedRoad = mapObject[cell] as Road;
            if (replacedRoad != null)
                replacedRoad.Remove(false);

            mapObject[cell] = road;

            connectedToPlayerOrGem = checkConnectionToPlayerOrGem(cell);
        }
        public Road RemoveRoad(Cell cell)
        {
            LOG.Function(this, $"{cell}");

            var road = mapObject[cell] as Road;
            mapObject[cell] = TheEmpty;

            return road;
        }
        public void ClearRoads()
        {
            LOG.Function(this);

            mapObject.Convert<Road>(TheEmpty);
            Dump();
        }
        public void UpdateConnection()
        {
            LOG.Function(this);

            updateConnection();
        }

        // Methods
        public Cell[] FindPathAlongRoad(out Gem gem)
        {
            return findPathAlongRoad(out gem);
        }
        public Cell[] FindPathTo(Cell targetCell)
        {
            return AStarAlgorithm.FindPath(mapObject, playerCell, targetCell);
        }
        public Direction FindMineDirection(Cell gemCell)
        {
            var dirs = new Direction[] { Direction.L, Direction.R };
            var validDirs = dirs.Where(d => gemCell + d != playerCell).ToArray();
            return UtilArray.ExtractOne(validDirs);
        }

        // Methods
        public void Dump() => mapObject?.Dump();



        // Fields
        private static IMapObject TheWall = new Wall();
        private static IMapObject TheEmpty = new Empty();
        private static IMapObject ThePlayer = new Player();

        // Fields
        private MapData<IMapObject> mapObject = null;
        private MapData<bool> mapVisit = null;
        private Cell playerCell = null;

        // Functions
        private bool checkConnectionToPlayerOrGem(Cell cellStart)
        {
            var queue = new Queue<Cell>();
            queue.Enqueue(cellStart);

            mapVisit.Clear();
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var obj = mapObject[cell];
                if (obj.MapObject == MapObject.Player)
                    return true;

                if (obj.MapObject == MapObject.Gem)
                    return true;

                mapVisit[cell] = true;
                foreach (var dir in obj.Connected)
                {
                    var cellNext = cell + dir;
                    if (mapVisit[cellNext] == false &&
                        mapObject[cellNext].CanEnterFrom(dir.Opposite()))
                        queue.Enqueue(cellNext);
                }

                InfiniteLoopDetector.Run();
            }

            return false;
        }
        private void updateConnection()
        {
            foreach (var roadCell in mapObject.CellOf<Road>())
            {
                var road = mapObject[roadCell] as Road;
                var connected = checkConnectionToPlayerOrGem(roadCell);
                road.SwitchTo(connected ? RoadMode.Colored : RoadMode.Grayed);
            }
        }
        private void updateConnectionOld()
        {
            var roads = mapObject.GetAll<Road>();
            roads.ForEach(r => r.SwitchTo(RoadMode.Grayed));

            var queue = new Queue<Cell>();
            queue.Enqueue(playerCell);

            mapVisit.Clear();
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var obj = mapObject[cell];
                var road = obj as Road;
                if (road != null)
                    road.SwitchTo(RoadMode.Colored);

                mapVisit[cell] = true;
                foreach (var dir in obj.Connected)
                {
                    var cellNext = cell + dir;
                    if (mapVisit[cellNext] == false &&
                        mapObject[cellNext].CanEnterFrom(dir.Opposite()))
                        queue.Enqueue(cellNext);
                }

                InfiniteLoopDetector.Run();
            }
        }
        private Cell[] findPathAlongRoad(out Gem gemFound)
        {
            var stack = new Stack<Cell>();
            stack.Push(playerCell);

            mapVisit.Clear();
            while (stack.Count > 0)
            {
                var cell = stack.Peek();
                var obj = mapObject[cell];
                var gem = obj as Gem;
                if (gem != null)
                {
                    gemFound = gem;
                    return stack.Reverse().SkipLast(1).ToArray(); // 보석 위치는 제외
                }
                mapVisit[cell] = true;

                var forwarded = false;
                foreach (var dir in obj.Connected)
                {
                    var cellNext = cell + dir;
                    if (mapVisit[cellNext] == false &&
                        mapObject[cellNext].CanEnterFrom(dir.Opposite()) &&
                        isOnRoad(cell, cellNext))
                    {
                        stack.Push(cellNext);

                        forwarded = true;
                        break;
                    }
                }

                if (!forwarded)
                    stack.Pop();

                InfiniteLoopDetector.Run();
            }

            gemFound = null;
            return null;
        }
        private bool isOnRoad(Cell cell1, Cell cell2)
        {
            return
                mapObject[cell1].MapObject == MapObject.Road ||
                mapObject[cell2].MapObject == MapObject.Road;

        }
    }
}