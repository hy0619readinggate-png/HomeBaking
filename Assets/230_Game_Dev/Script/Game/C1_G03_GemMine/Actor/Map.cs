using beyondi.Behaviour;
using beyondi.Coroutine;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using FlexFramework.Excel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public enum MapObject { None, Gem, Obstacle, Road, Player }

    [RequireComponent(typeof(MapNavigator))]
    public class Map : BYDSingleton<Map>, ICompletable
    {
        // Definitions
        public const int COLS = 18;
        public const int ROWS = 10;

        // Properties
        public Vector2 EntrancePosition => CellCenterOf(entranceCell);
        public Cell EntranceCell => entranceCell;

        // Methods
        public async UniTask Setup(RoundData roundData)
        {
            LOG.Info($"{nameof(Setup)}() | {roundData}", this);

            // 맵 베리에이션
            for (var i = 0; i < roundData.Maps.Length; i++)
            {
                var set = roundData.Maps[i];
                gridSet[i].SetChildActiveOnly(set - 1);
            }

            // 보석 세팅
            var gems = GetComponentsInChildren<Gem>();
            for (var i = 0; i < roundData.Gems.Length; i++)
                gems[i].Setup(roundData.Gems[i], roundData.MapVariation);

            // 장애물 세팅
            var obstacles = GetComponentsInChildren<Obstacle>();
            for (var i = 0; i < obstacles.Length; i++)
                obstacles[i].Setup(roundData.MapVariation);



            // 입구
            entranceCell = new Cell(roundData.Enterance - 1 + 1, ROWS - 1 + 1); // for Wall
            gateTR.position = new Vector3(
                CellCenterOf(entranceCell).x,
                gateTR.position.y,
                gateTR.position.z);

            // 물리 프레임 대기
            await UniTask.Delay(500);

            // 네비게이션
            // 빌드 방법을 아에 바꿈 (Physics를 사용하지 않도록)
            //navigator.Build(grid, COLS + 2, ROWS + 2); // +2 for Wall
            var oAreas = GetComponentsInChildren<ObstacleArea>();
            navigator.Build(grid, COLS + 2, ROWS + 2, gems, oAreas);
        }
        public void ShowGuide(bool show)
        {
            LOG.Function(this, $"{show}");

            guideANIM.SetTrigger(show ? "Show" : "Hide");
        }
        public void OverOn(Vector3 position)
        {
            var cell = CellOf(position);
            var canPlace = navigator.CanPlaceRoadBlock(cell);
            overTR.gameObject.SetActive(canPlace);
            overTR.position = CellCenterOf(cell);
        }
        public void OverOff()
        {
            overTR.gameObject.SetActive(false);
        }
        public void PlacePlayer(Cell cell)
        {
            LOG.Function(this);

            navigator.PlacePlayer(cell);
            navigator.Dump();
        }
        public void PlaceRoad(Vector3 position, RoadType roadType)
        {
            LOG.Function(this, $"{position}");

            var cell = CellOf(position);
            placeRoad(cell, roadType);
        }
        public void ClearRoads()
        {
            LOG.Info($"ClearRoads()", this);

            navigator.ClearRoads();
            currentPathToGem = null;
            undoMGR.Clear();

            StartCoroutine(coClearRoads());
        }
        public void Undo()
        {
            LOG.Function(this);

            var cmds = undoMGR.Undo();
            if (cmds == null)
                return;

            foreach (var cmd in cmds)
            {
                LOG.Info($"Undo() | {cmd}", this);
                switch (cmd.Type)
                {
                    case CommandType.Add:
                        removeRoad(cmd.Cell);
                        break;

                    case CommandType.Remove:
                        placeRoad(cmd.Cell, cmd.RoadType, true);
                        break;
                }
                LOG.Info($"{cmd}", this);
            }
        }

        // Methods
        public void ResetGems()
        {
            LOG.Function(this);

            var gems = GetComponentsInChildren<Gem>();
            var gemsMined = gems.Where(g => g.IsMined && !g.IsCompleted);
            gemsMined.ForEach(g => g.Revive());
        }

        // Methods
        public PathResult FindPathToGem()
        {
            var path = navigator.FindPathAlongRoad(out var targetGem);
            return new PathResult
            {
                PathCell = path,
                PathPoint = CellCenterOf(path),
                Gem = targetGem,
                GemCell = CellOf(targetGem.transform.position)
            };
        }
        public Direction FindMineDirection(Cell cell)
        {
            return navigator.FindMineDirection(cell);
        }

        // Methods : cell transform
        public Cell CellOf(Vector3 position)
        {
            return grid.WorldToCell(position);
        }
        public Vector3 CellCenterOf(Cell cell)
        {
            return grid.GetCellCenterWorld(cell);
        }
        public Vector3 CellCenterOf(Vector3 position)
        {
            var cell = CellOf(position);
            return CellCenterOf(cell);
        }
        public Vector3[] CellCenterOf(Cell[] cells)
        {
            return cells.Select(cell => CellCenterOf(cell)).ToArray();
        }

        // Methods : dev
        public void DEV_PlaceRoad_AnswerGem(string word)
        {
            LOG.Function(this);

            if (navigator.PlayerCell != null)
            {
                var playerPos = CellCenterOf(navigator.PlayerCell);
                var gems = GetComponentsInChildren<Gem>();
                var targetGem = gems.Single(g => g.Word == word);

                var targetCell = CellOf(targetGem.transform.position);
                this.StopCoroutineSafe(ref crPlaceRoadsTo);
                crPlaceRoadsTo = StartCoroutine(coPlaceRoadsTo(targetCell));
            }
            else LOG.Warning("No player is located.", this);
        }
        public void DEV_PlaceRoad_NearestGem()
        {
            LOG.Function(this);

            if (navigator.PlayerCell != null)
            {
                var playerPos = CellCenterOf(navigator.PlayerCell);
                var gems = GetComponentsInChildren<Gem>();
                var targetGem = gems
                    .Where(g => !g.IsMined)
                    .OrderBy(g => Vector2.Distance(g.transform.position, playerPos))
                    .First();

                var targetCell = CellOf(targetGem.transform.position);
                this.StopCoroutineSafe(ref crPlaceRoadsTo);
                crPlaceRoadsTo = StartCoroutine(coPlaceRoadsTo(targetCell));
            }
            else LOG.Warning("No player is located.", this);
        }
        public void DEV_PlaceRoad_FurthestGem()
        {
            LOG.Function(this);

            if (navigator.PlayerCell != null)
            {
                var playerPos = CellCenterOf(navigator.PlayerCell);
                var gems = GetComponentsInChildren<Gem>();
                var targetGem = gems
                    .Where(g => !g.IsMined)
                    .OrderByDescending(g => Vector2.Distance(g.transform.position, playerPos))
                    .First();

                var targetCell = CellOf(targetGem.transform.position);

                this.StopCoroutineSafe(ref crPlaceRoadsTo);
                crPlaceRoadsTo = StartCoroutine(coPlaceRoadsTo(targetCell));
            }
            else LOG.Warning("No player is located.", this);
        }



        // Fields : caching
        private MapNavigator navigator_ = null;
        private MapNavigator navigator => navigator_ ??= GetComponent<MapNavigator>();

        // Fields
        private Cell entranceCell;
        private Cell[] currentPathToGem;
        private UndoMGR undoMGR = new UndoMGR();
        private Coroutine crPlaceRoadsTo = null;

        // Functions
        private void placeRoad(Cell cell, RoadType roadType, bool byUndo = false)
        {
            var canPlace = navigator.CanPlaceRoadBlock(cell);
            if (canPlace)
            {
                var roadPos = CellCenterOf(cell);
                var road = roadPool.Get();
                road.gameObject.name = $"ROAD - {cell}";
                road.Setup(roadType);

                navigator.PlaceRoad(cell, road,
                    out var connectedToPlayerOrGem,
                    out var replacedRoad);
                navigator.UpdateConnection();

                road.Place(roadPos, connectedToPlayerOrGem ? RoadMode.Colored : RoadMode.Grayed, !byUndo);

                currentPathToGem = navigator.FindPathAlongRoad(out _);

                if (!byUndo)
                {
                    if (replacedRoad != null)
                        undoMGR.AddCommand(Command.OfRemove(cell, replacedRoad));
                    undoMGR.AddCommand(Command.OfAdd(cell, road));
                    undoMGR.AddCheckPoint();
                }
            }
        }
        private void removeRoad(Cell cell)
        {
            var road = navigator.RemoveRoad(cell);
            if (road != null)
                road.Remove(true);

            navigator.UpdateConnection();
            currentPathToGem = navigator.FindPathAlongRoad(out _);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] gridSet = null;
        [SerializeField] private Animator guideANIM = null;
        [SerializeField] private Grid grid;
        [SerializeField] private Transform overTR = null;
        [SerializeField] private RoadPool roadPool = null;
        [SerializeField] private Transform gateTR = null;

        [Header("★ Config")]
        [SerializeField] private float clearRoadInterval = 0.05f;


        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            guideANIM.gameObject.SetActive(true);
            overTR.gameObject.SetActive(false);
        }
        private void Start()
        {

        }
        private void OnDrawGizmos()
        {
            if (currentPathToGem != null)
            {
                var points = CellCenterOf(currentPathToGem);
                Gizmos.color = Color.red;
                Gizmos.DrawLineStrip(points, false);
            }
        }

        // Unity Coroutine
        IEnumerator coClearRoads()
        {
            using (LOG.Coroutine($"coClearRoads()", this))
            {
                foreach (var r in roadPool.GetComponentsInChildren<Road>())
                {
                    r.Remove();
                    yield return new WaitForSeconds(clearRoadInterval);
                }
            }
        }
        IEnumerator coPlaceRoadsTo(Cell targetCell)
        {
            using (LOG.Coroutine($"{nameof(coPlaceRoadsTo)}", this))
            {
                var path = navigator.FindPathTo(targetCell);
                var q = new Queue<Cell>(path);

                var prevCell = q.Dequeue();
                while (q.Count > 1)
                {
                    var currCell = q.Dequeue();
                    var nextCell = q.Peek();

                    var dir1 = Cell.DirectionOf(currCell, prevCell);
                    var dir2 = Cell.DirectionOf(currCell, nextCell);

                    var roadType = Road.RoadTypeOf(dir1, dir2);
                    placeRoad(currCell, roadType);

                    prevCell = currCell;

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => currentPathToGem?.Length > 1;
    }
}