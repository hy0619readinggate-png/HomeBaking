using DoDoEng.Common;
using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C2_G01
{
    public class NoteScroller : MonoBehaviour
    {
        // Properties
        public Music MusicForGizmos { get; set; } = null;

        // Methods
        public void StartScroll(float speed)
        {
            LOG.Info($"{nameof(StartScroll)}() | {speed}", this);

            transform.position = originalPos;
            scrollSpeed = speed;
            isScroll = true;
        }
        public void StopScroll()
        {
            LOG.Info($"{nameof(StopScroll)}()", this);

            isScroll = false;
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Fields
        private Vector3 originalPos;
        private bool isScroll = false;



        // Unity Inspectors
        [Header("★ DEV")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private int gizmosBeatCount = 100;
        [SerializeField][ReadOnly] private float scrollSpeed = 0;

        // Unity Messages
        private void Awake()
        {
            originalPos = transform.position;
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (isScroll)
            {
                rt.anchoredPosition = new Vector2(
                    rt.anchoredPosition.x,
                    rt.anchoredPosition.y - scrollSpeed * Time.deltaTime);
            }
        }
        private void OnDrawGizmos()
        {
            if (drawGizmos && MusicForGizmos != null)
            {
                var rt = GetComponent<RectTransform>();
                var x1 = rt.offsetMin.x;
                var x2 = rt.offsetMax.x;

                var distance = MusicForGizmos.BeatDistance;
                var skipOffset = MusicForGizmos.BeatSkipOffset;

                for (var i = 0; i < gizmosBeatCount / 2; i++)
                {
                    Gizmos.color = Color.gray;

                    var y = skipOffset - i * distance;
                    var from = transform.TransformPoint(new Vector3(x1, y));
                    var to = transform.TransformPoint(new Vector3(x2, y));
                    Gizmos.DrawLine(from, to);
                }

                for (var i = 0; i < gizmosBeatCount / 2; i++)
                {
                    Gizmos.color = Color.yellow;

                    var y = skipOffset + i * distance;
                    var from = transform.TransformPoint(new Vector3(x1, y));
                    var to = transform.TransformPoint(new Vector3(x2, y));
                    Gizmos.DrawLine(from, to);
                }
            }
        }
    }
}