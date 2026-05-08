using System.Collections.Generic;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{
    public class MonsterGenerator : MonoBehaviour
    {

        // Methods
        public void AllMoveFast()
        {
            foreach (var mon in monsterList)
            {
                if (mon != null)
                    mon.MoveFast();
            }
        }



        // Fields
        private List<Monster> monsterList = new List<Monster>();
        //private Coroutine deathAllCoroutine = null;



        // Functions
        private void create(BubbleMonster bubble)
        {
            var pf = MonsterMGR.One.GetMonsterPrefab(bubble.MonsterIndex);

            var go = Instantiate(pf, _SpawnTR);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var mon = go.GetComponent<Monster>();
            if (mon != null)
                monsterList.Add(mon);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform _SpawnTR = null;
        //[SerializeField] private GameObject _ColliderGO = null;

        // Unity Messages
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                var bubble = collision.attachedRigidbody.GetComponent<BubbleMonster>();
                if (bubble != null)
                {
                    bubble.SetHeightLimit(_SpawnTR.position.y, create);
                }
            }

        }

        // Coroutine
        //IEnumerator coDeathAllMonster(bool delay)
        //{
        //    foreach (var mon in monsterList)
        //    {
        //        if (mon != null)
        //            mon.MoveFast();

        //        if (delay)
        //            yield return new WaitForSeconds(0.1f);
        //    }

        //    monsterList.Clear();

        //    deathAllCoroutine = null;
        //    yield return null;
        //}
    }
}