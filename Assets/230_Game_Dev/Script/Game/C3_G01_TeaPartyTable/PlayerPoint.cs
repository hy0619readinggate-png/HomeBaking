using System;
using UnityEngine;

namespace DoDoEng.Game.C3_G01
{
    public class PlayerPoint : MonoBehaviour
    {
        //Properties
        public Vector3 PointPosition => transform.position;

        //Events
        [HideInInspector] public Action<Vector3> OnPlayerMove;

    }
}