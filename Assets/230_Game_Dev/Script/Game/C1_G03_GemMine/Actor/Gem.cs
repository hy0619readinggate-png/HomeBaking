using beyondi.Util;
using DoDoEng.Activity.C1_A02;
using DoDoEng.Common;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.Game.C1_G03
{
    public class Gem : MonoBehaviour, IMapObject
    {
        // Properties
        public string Word { get; private set; }
        public bool IsMined { get; private set; }
        public bool IsCompleted { get; private set; }

        // Methods
        public void Setup(GemData gemData, int mapVariation)
        {
            LOG.Info($"{nameof(Setup)}() | {gemData}", this);

            IsCompleted = false;
            IsMined = false;

            Word = gemData.Word;
            image.sprite = gemData.WordSprite;
            gemVariationGO.ForEach((i, go) => go.SetActive(i + 1 == gemData.Variation));
            mapVariationGO.ForEach((i, go) => go.SetActive(i + 1 == mapVariation));

            DEV_Order = gemData.DEV_Order;
            gameObject.name = $"Gem(Example) - {gemData.Word}";
        }
        public void Revive()
        {
            LOG.Function(this);

            if (!IsCompleted)
            {
                IsMined = false;
                reviveTL.time = 0;
                reviveTL.Play();
            }
        }
        public Coroutine StartMine(Direction dir, bool correct)
        {
            LOG.Info($"StartMine() | {dir}, {correct}", this);

            IsMined = true;
            IsCompleted = correct;

            crMine = StartCoroutine(coMine(dir, correct));
            return crMine;
        }
        public void FinishMine()
        {
            LOG.Info($"FinishMine()", this);

            this.StopCoroutineSafe(ref crMine);
        }



        // Fields : caching
        private TimelineSignal correctLSIG_ = null;
        private TimelineSignal correctLSIG => correctLSIG_ ??= correctLTL.GetComponent<TimelineSignal>();
        private TimelineSignal correctRSIG_ = null;
        private TimelineSignal correctRSIG => correctRSIG_ ??= correctRTL.GetComponent<TimelineSignal>();

        // Fields
        private static Direction[] allDirections => new Direction[]
        {
            Direction.L, Direction.R, Direction.B, Direction.T
        };
        private Coroutine crMine = null;
        private string DEV_Order = null;

        // Event Handlers
        private void correctSIG_OnSignal(string signal)
        {
            LOG.Info($"correctSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
            {
                var clip = UtilArray.ExtractOne(gemDiscoveryCLIP);
                AudioMGR.One.PlayEffect(clip);
            }    
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [SerializeField] private GameObject[] mapVariationGO = null;
        [SerializeField] private GameObject[] gemVariationGO = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] gemDiscoveryCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector correctLTL = null;
        [SerializeField] private PlayableDirector correctRTL = null;
        [SerializeField] private PlayableDirector wrongLTL = null;
        [SerializeField] private PlayableDirector wrongRTL = null;
        [SerializeField] private PlayableDirector reviveTL = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            correctLSIG.OnSignal += correctSIG_OnSignal;
            correctRSIG.OnSignal += correctSIG_OnSignal;
        }
        private void OnDisable()
        {
            correctLSIG.OnSignal -= correctSIG_OnSignal;
            correctRSIG.OnSignal -= correctSIG_OnSignal;
        }
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            drawString($"{DEV_Order}", transform.position, Color.yellow, new Vector2(0.5f, 0.5f), 20f);
#endif
        }

        // Unity Coroutine
        IEnumerator coMine(Direction dir, bool correct)
        {
            using (LOG.Coroutine($"coMine()", this))
            {
                PlayableDirector tl;

                if (dir == Direction.L)
                    tl = correct ? correctLTL : wrongLTL;
                else tl = correct ? correctRTL : wrongRTL;

                tl.time = 0;
                tl.Play();
                yield return new WaitForSeconds((float)tl.duration);
            }
        }



        // Interface : IMapObject
        MapObject IMapObject.MapObject => MapObject.Gem;
        Direction[] IMapObject.Connected => IsMined ? null : allDirections;



        // Static functions
        static private void drawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
        {
#if UNITY_EDITOR
            var view = SceneView.currentDrawingSceneView;
            if (!view)
                return;

            var screenPosition = view.camera.WorldToScreenPoint(worldPosition);
            if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight ||
                screenPosition.x < 0 || screenPosition.x > view.camera.pixelWidth ||
                screenPosition.z < 0)
                return;

            var pixelRatio =
                HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x -
                HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;

            Handles.BeginGUI();
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = (int)textSize,
                normal = new GUIStyleState() { textColor = textColor },
                alignment = TextAnchor.MiddleCenter
            };
            var size = style.CalcSize(new GUIContent(text)) * pixelRatio;
            var alignedPosition =
                ((Vector2)screenPosition +
                size * ((anchor + Vector2.left + Vector2.up))) * (Vector2.right + Vector2.down) +
                Vector2.up * view.camera.pixelHeight;
            GUI.Box(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
            //GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
            Handles.EndGUI();
#endif
        }
    }
}