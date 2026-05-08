using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class Obstacle : MonoBehaviour, IMapObject
    {
        // Methods
        public void Setup(int mapVariation)
        {
            LOG.Function(this);

            if (mapVariationGO.Length > 0)
                mapVariationGO.ForEach((i, go) => go.SetActive(i + 1 == mapVariation));

        }
        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private GameObject[] mapVariationGO = null;



        // Interface : IMapObject
        MapObject IMapObject.MapObject => MapObject.Obstacle;
        Direction[] IMapObject.Connected => null;
    }
}