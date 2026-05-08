using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class ObstacleArea : MonoBehaviour, IMapObject
    {
        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }



        // Interface : IMapObject
        MapObject IMapObject.MapObject => MapObject.Obstacle;
        Direction[] IMapObject.Connected => null;
    }  
}