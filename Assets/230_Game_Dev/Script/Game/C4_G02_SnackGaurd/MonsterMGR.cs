using UnityEngine;
using beyondi.Behaviour;

namespace DoDoEng.Game.C4_G02
{
    public class MonsterMGR : BYDSingleton<MonsterMGR>
    {

        // Methods
        public GameObject GetMonsterPrefab(int index)
        {
            return _MonstersPF[index];
        }
        public void AllMoveFast()
        {
            foreach (var generator in _Generators)
                generator.AllMoveFast();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] _MonstersPF = null;
        [SerializeField] private MonsterGenerator[] _Generators = null;
    }
}