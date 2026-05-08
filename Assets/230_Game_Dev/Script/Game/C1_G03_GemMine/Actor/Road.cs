using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DoDoEng.Game.C1_G03
{
    public enum RoadType { RB, RT, LR, TB, LT, LB }
    public enum RoadMode { Grayed, Colored };

    public class Road : MonoBehaviour,
        IBYDPooledObject<Road>,
        IMapObject
    {
        // Properties
        public bool ConnectedToPlayer { get; set; }
        public RoadType RoadType { get; private set; }
        public RoadMode RoadMode { get; private set; }

        // Methods
        public void Setup(RoadType roadType)
        {
            LOG.Function(this, $"{roadType}");

            RoadType = roadType;
            updateImage();
        }
        public void Place(Vector3 position, RoadMode roadMode, bool animation = true)
        {
            LOG.Function(this, $"{position} {roadMode}");

            RoadMode = roadMode;
            updateImage();

            var clip = roadMode == RoadMode.Colored ? placeConnectedCLIP : placedisconnectedCLIP;
            AudioMGR.One.PlayEffect(clip);
            transform.position = position;
            if (animation)
                anim.SetTrigger("Show");
            else anim.SetTrigger("ShowNow");

        }
        public void Remove(bool animation = true)
        {
            LOG.Function(this);

            if (animation)
            {
                AudioMGR.One.PlayEffect(removeCLIP);

                anim.SetTrigger("Hide");
                DOVirtual.DelayedCall(hideDuration, () => Pool.Release(this));

            }
            else Pool.Release(this);
        }
        public void SwitchTo(RoadMode roadMode)
        {
            RoadMode = roadMode;
            updateImage();
        }



        // Functions
        private void updateImage()
        {
            switch (RoadMode)
            {
                case RoadMode.Grayed:
                    roadImageColor.SetActiveAll(false);
                    roadImageGray.SetActiveOnly((int)RoadType);
                    colorFxGO.SetActive(false);
                    break;

                case RoadMode.Colored:
                    roadImageColor.SetActiveOnly((int)RoadType);
                    roadImageGray.SetActiveAll(false);
                    colorFxGO.SetActive(true);
                    break;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] roadImageColor = null;
        [SerializeField] private GameObject[] roadImageGray = null;
        [SerializeField] private Animator anim = null;
        [SerializeField] private GameObject colorFxGO = null;
        [Header("★ Config")]
        [SerializeField] private float hideDuration = 1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip placeConnectedCLIP = null;
        [SerializeField] private AudioClip placedisconnectedCLIP = null;
        [SerializeField] private AudioClip removeCLIP = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }



        // Interface : IBYDPooledObject<T>
        public IObjectPool<Road> Pool { get; set; }

        // Interface : IMapObject
        MapObject IMapObject.MapObject => MapObject.Road;
        Direction[] IMapObject.Connected => connected[RoadType];

        // Static Constructor
        public static RoadType RoadTypeOf(params Direction[] directions)
        {
            if (directions.Contain(Direction.R) && directions.Contain(Direction.B)) return RoadType.RB;
            if (directions.Contain(Direction.R) && directions.Contain(Direction.T)) return RoadType.RT;
            if (directions.Contain(Direction.L) && directions.Contain(Direction.R)) return RoadType.LR;
            if (directions.Contain(Direction.T) && directions.Contain(Direction.B)) return RoadType.TB;
            if (directions.Contain(Direction.L) && directions.Contain(Direction.T)) return RoadType.LT;
            if (directions.Contain(Direction.L) && directions.Contain(Direction.B)) return RoadType.LB;

            return RoadType.LB;
        }
        static Dictionary<RoadType, Direction[]> connected;
        static Road()
        {
            connected = new Dictionary<RoadType, Direction[]>();
            connected[RoadType.RB] = new[] { Direction.R, Direction.B };
            connected[RoadType.RT] = new[] { Direction.R, Direction.T };
            connected[RoadType.LR] = new[] { Direction.L, Direction.R };
            connected[RoadType.TB] = new[] { Direction.T, Direction.B };
            connected[RoadType.LT] = new[] { Direction.L, Direction.T };
            connected[RoadType.LB] = new[] { Direction.L, Direction.B };
        }
    }
}