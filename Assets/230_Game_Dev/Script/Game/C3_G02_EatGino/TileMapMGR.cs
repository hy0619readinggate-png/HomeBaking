using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace DoDoEng.Game.C3_G02
{
    public class TileMapMGR : MonoBehaviour
    {

        // Methods
        public bool HasTile(Vector3 pos)
        {
            var tPos = _TileMap.WorldToCell(pos);
            return _TileMap.HasTile(tPos);
        }
        public void RemoveTile(Vector3 pos)
        {
            if (removeCoroutine != null)
            {
                StopCoroutine(removeCoroutine);
                removeCoroutine = null;

                var tPos = _TileMap.WorldToCell(prevRemovePos);
                _TileMap.SetTile(tPos, null);

                OnTileRemoved?.Invoke(prevRemovePos);
            }


            if (removeCoroutine == null)
            {
                prevRemovePos = pos;
                removeCoroutine = StartCoroutine(coRemove(pos));
            }
        }
        public void InitTile(Vector3 pos)
        {
            var tPos = _TileMap.WorldToCell(pos);
            var start = _TileMap.WorldToCell(_Start.position);
            var end = _TileMap.WorldToCell(_End.position);

            for (int i = (int)start.x; i < (int)end.x; i++)
            {
                for (int k = (int)start.y; k < (int)end.y; k++)
                    _TileMap.SetTile(new Vector3Int(i, k, 0), _Tile);
            }

            _TileMap.SetTile(tPos, null);
        }



        // Event
        public event Action<Vector3> OnTileRemoved;



        // Fields
        private Coroutine removeCoroutine = null;

        // Fields : ani
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");

        private Vector3 prevRemovePos = Vector3.zero;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Tilemap _TileMap = null;
        [SerializeField] private TileBase _Tile = null;
        [SerializeField] private Animator _TileFXANI = null;
        [SerializeField] private Transform _Start = null;
        [SerializeField] private Transform _End = null;
        [Header("★ Config")]
        [SerializeField] private float _TileDelayTime = 0.4f;


        // Unity Coroutine
        IEnumerator coRemove(Vector3 pos)
        {
            _TileFXANI.SetTrigger(hashKey_Hide);

            yield return new WaitForSeconds(_TileDelayTime);

            _TileFXANI.transform.position = pos;
            _TileFXANI.SetTrigger(hashKey_Show);

            var tPos = _TileMap.WorldToCell(pos);
            _TileMap.SetTile(tPos, null);
            yield return null;


            OnTileRemoved?.Invoke(pos);

            removeCoroutine = null;
        }
    }
}