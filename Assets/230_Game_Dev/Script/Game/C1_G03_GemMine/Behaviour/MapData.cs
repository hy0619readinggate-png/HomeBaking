using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class MapData<T>
    {
        // Properties
        public T this[Cell cell]
        {
            get => data[cell.X, cell.Y];
            set => data[cell.X, cell.Y] = value;
        }
        public T this[int x, int y]
        {
            get => data[x, y];
            set => data[x, y] = value;
        }
        public int Cols => data.GetLength(0);
        public int Rows => data.GetLength(1);

        // Methods : ctor.
        public MapData(int cols, int rows)
        {
            data = new T[cols, rows];
        }

        // Methods
        public TObj[] GetAll<TObj>()
        {
            return data.OfType<TObj>().Where(obj => obj != null).ToArray();
        }
        public IEnumerable<Cell> CellOf<TObj>() where TObj : class
        {
            for (var r = 0; r < Rows; r++)
            {
                for (var c = 0; c < Cols; c++)
                {
                    var obj = this[c, r] as TObj;
                    if (obj != null)
                        yield return new Cell(c, r);
                }
            }
        }
        public void Convert<TObj>(T target) where TObj : class
        {
            foreach (var cell in CellOf<TObj>())
                this[cell] = target;
        }
        public void Clear()
        {
            for (var r = 0; r < Rows; r++)
            {
                for (var c = 0; c < Cols; c++)
                    data[c, r] = default(T);
            }
        }



        // Fields
        private T[,] data = null;
    }

    public interface IMapObject
    {
        MapObject MapObject { get; }
        Direction[] Connected { get; }
    }
    public class Wall : IMapObject
    {
        MapObject IMapObject.MapObject => MapObject.Obstacle;
        Direction[] IMapObject.Connected => null;
    }
    public class Empty : IMapObject
    {
        MapObject IMapObject.MapObject => MapObject.None;
        Direction[] IMapObject.Connected => null;
    }
    public class Player : IMapObject
    {
        MapObject IMapObject.MapObject => MapObject.Player;
        Direction[] IMapObject.Connected => new[] { Direction.L, Direction.R, Direction.T, Direction.B };
    }

    public class PathResult
    {
        public Cell LastCellOfPath => PathCell.Last();
        public Vector3 LastPointOfPath => PathPoint.Last();
        public Cell LastBeforeCellOfPath => PathCell.SkipLast(1).Last();

        public Cell[] PathCell;
        public Vector3[] PathPoint;
        public Gem Gem;
        public Cell GemCell;
    }

    public class Cell
    {
        // Properties
        public int X { get; set; }
        public int Y { get; set; }

        // Methods : ctor.
        public Cell() { }
        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public Cell(Vector3Int v3)
        {
            X = v3.x;
            Y = v3.y;
        }

        // Methods
        public void Set(int x, int y)
        {
            X = x; Y = y;
        }
        public Direction DirectionTo(Cell to)
        {
            if (to.X - this.X > 0) return Direction.R;
            if (to.X - this.X < 0) return Direction.L;
            if (to.Y - this.Y > 0) return Direction.T;
            if (to.Y - this.Y < 0) return Direction.B;

            LOG.Warning($"The beginning and end must be different!!!", this);
            return Direction.B;
        }

        // Methods : static
        public static Direction DirectionOf(Cell from, Cell to)
        {
            return from.DirectionTo(to);
        }


        // Override
        public override string ToString()
        {
            return $"{X:D2}x{Y:D2}";
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Cell)) return false;
            if (X != ((Cell)obj).X) return false;
            if (Y != ((Cell)obj).Y) return false;

            return true;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        // Overload
        public static implicit operator Cell(Vector3Int v3) => new Cell(v3);
        public static implicit operator Vector3Int(Cell cell) => new Vector3Int(cell.X, cell.Y);
        public static Cell operator +(Cell cell, Direction dir)
        {
            return dir switch
            {
                C1_G03.Direction.L => new Cell(cell.X - 1, cell.Y),
                C1_G03.Direction.R => new Cell(cell.X + 1, cell.Y),
                C1_G03.Direction.T => new Cell(cell.X, cell.Y + 1),
                C1_G03.Direction.B => new Cell(cell.X, cell.Y - 1),
                _ => cell
            };
        }
        public static bool operator ==(Cell cell1, Cell cell2)
        {
            if (ReferenceEquals(cell1, cell2)) return true;
            if (ReferenceEquals(cell1, null)) return false;
            if (ReferenceEquals(cell2, null)) return false;

            return cell1.Equals(cell2);
        }
        public static bool operator !=(Cell obj1, Cell obj2) => !(obj1 == obj2);
    }

    public static class Extensions
    {
        public static Direction Opposite(this Direction dir)
        {
            return dir switch
            {
                Direction.L => Direction.R,
                Direction.R => Direction.L,
                Direction.T => Direction.B,
                Direction.B => Direction.T,
                _ => Direction.L
            };
        }
        public static void Dump(this MapData<IMapObject> mapData)
        {
            LOG.VeryImportant<MapData<IMapObject>>($"MapData : {mapData.Cols}x{mapData.Rows}");

            var cell = new Cell();
            var row = new string[mapData.Cols];
            for (var r = mapData.Rows - 1; r >= 0; r--)
            {
                for (var c = 0; c < mapData.Cols; c++)
                {
                    cell.Set(c, r);

                    var obj = mapData[cell];
                    if (obj != null)
                    {
                        if (obj.MapObject == MapObject.None)
                            row[c] = "_";
                        else row[c] = obj.MapObject.ToString().Substring(0, 1);
                    }
                    else row[c] = " ";
                }

                var rowString = string.Join(" ", row);
                LOG.VeryImportant<MapData<IMapObject>>($"{r:D3} : {rowString}");
            }
        }
        public static bool CanEnterFrom(this IMapObject mapObject, Direction dir)
        {
            return mapObject.Connected?.Contain(dir) ?? false;
        }
    }
}
