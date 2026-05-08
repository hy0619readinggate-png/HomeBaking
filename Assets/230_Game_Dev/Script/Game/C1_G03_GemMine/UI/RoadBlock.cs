using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G03
{
    [RequireComponent(typeof(Animator))]
    public class RoadBlock : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private bool isDrag = false;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoadType roadType = RoadType.RB;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Function(this);

            isDrag = true;
            cg.blocksRaycasts = false;

            CameraControl.One.SuppressPan = true;
            BlockDropZone.One.EnableDrop = true;
            RoadFloatBlock.One.Show(roadType);
            RoadFloatBlock.One.Locate(eventData);
            Map.One.ShowGuide(true);

            var pScreen = RoadFloatBlock.One.ScreenPositionOfCenter;
            var pWorld = Camera.main.ScreenToWorldPoint(pScreen);
            Map.One.OverOn(pWorld);

            anim.SetTrigger("Pressed");
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                RoadFloatBlock.One.Locate(eventData);

                var pScreen = RoadFloatBlock.One.ScreenPositionOfCenter;
                var pWorld = Camera.main.ScreenToWorldPoint(pScreen);
                Map.One.OverOn(pWorld);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Function(this);

            if (isDrag)
            {
                if (eventData.used)
                {
                    var pScreen = RoadFloatBlock.One.ScreenPositionOfCenter;
                    var pPosition = Camera.main.ScreenToWorldPoint(pScreen);
                    Map.One.PlaceRoad(pPosition, roadType);
                }

                CameraControl.One.SuppressPan = false;
                BlockDropZone.One.EnableDrop = false;
                RoadFloatBlock.One.Hide();
                Map.One.ShowGuide(false);
                Map.One.OverOff();

                isDrag = false;
                cg.blocksRaycasts = true;
            }
        }
    }
}