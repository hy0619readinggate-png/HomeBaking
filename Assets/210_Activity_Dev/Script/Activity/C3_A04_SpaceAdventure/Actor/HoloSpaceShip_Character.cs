using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public enum CharacterMode { Empty, Fill, Active };
    public class HoloSpaceShip_Character : MonoBehaviour
    {
        // Properties
        public CharacterMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                updateMode();
            }
        }

        // Methods
        public void Setup(char c)
        {
            LOG.Info($"Setup() | {c}", this);

            labelEmpty.text = c.ToString();
            labelFill.text = c.ToString();
            labelActive.text = c.ToString();
        }
        public void Scatter(int variation)
        {
            if (variation == 0)
                StartCoroutine(coScatterZoom());
            else StartCoroutine(coScatterShake());
        }



        // Fields
        private CharacterMode mode = CharacterMode.Fill;
        private CanvasGroup cg = null;

        // Functions
        private void updateMode()
        {
            labelEmpty.gameObject.SetActive(mode == CharacterMode.Empty);
            labelFill.gameObject.SetActive(mode == CharacterMode.Fill);
            labelActive.gameObject.SetActive(mode == CharacterMode.Active);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI labelEmpty = null;
        [SerializeField] private TextMeshProUGUI labelFill = null;
        [SerializeField] private TextMeshProUGUI labelActive = null;
        [Header("★ Config - zoom")]
        [SerializeField] private float zoom1Duration = 1f;
        [SerializeField] private Range zoom1Dist = new Range(0.5f, 1f);
        [SerializeField] private float zoom1Scale = 1.1f;
        [SerializeField] private Range zoom1Angle = new Range(-30, +30);
        [SerializeField] private Ease zoom1Ease = Ease.OutQuad;
        [SerializeField] private float zoom2Duration = 0.5f;
        [SerializeField] private float zoom2Dist = 50f;
        [SerializeField] private Ease zoom2Ease = Ease.InQuad;
        [Header("★ Config - shake")]
        [SerializeField] private float shake1Duration = 0.5f;
        [SerializeField] private float shake1PosStrength = 10;
        [SerializeField] private int shake1PosVibrato = 20;
        [SerializeField] private float shake1RotStrength = 10f;
        [SerializeField] private int shake1RotVibrato = 20;
        [SerializeField] private float shake2Duration = 0.5f;
        [SerializeField] private float shake2Dist = 20f;
        [SerializeField] private Ease shake2Ease = Ease.OutQuad;

        // Unity Messages
        private void Awake()
        {
            cg = GetComponent<CanvasGroup>();
            if (cg == null)
                cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            updateMode();
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coScatterShake()
        {
            using (LOG.Coroutine($"coScatterShake()", this))
            {
                // 1
                transform.DOShakePosition(shake1Duration, shake1PosStrength, shake1PosVibrato, 90, false, false, ShakeRandomnessMode.Harmonic);
                yield return transform
                    .DOShakeRotation(shake1Duration, shake1RotStrength, shake1RotVibrato, 90, false, ShakeRandomnessMode.Harmonic)
                    .WaitForCompletion();

                // 2
                var direction = Random.insideUnitCircle;
                var from = (Vector2)transform.position;
                var to = from + direction * shake2Dist;

                cg.DOFade(0, shake2Duration)
                  .SetEase(shake2Ease);

                yield return transform
                    .DOMove(to, shake2Duration)
                    .SetEase(shake2Ease)
                    .WaitForCompletion();

                Destroy(gameObject);
            }
        }
        IEnumerator coScatterZoom()
        {
            using (LOG.Coroutine($"coScatterZoom()", this))
            {
                var direction = Random.insideUnitCircle;
                var theta = zoom1Angle.RandomValue();
                var from = (Vector2)transform.position;
                var to1 = from + direction * zoom1Dist.RandomValue();
                var to2 = from + direction * zoom2Dist;

                // 1
                transform
                    .DOPunchScale(Vector3.one * zoom1Scale, zoom1Duration, 1)
                    .SetEase(zoom1Ease);

                transform
                    .DORotate(new Vector3(0, 0, theta), zoom1Duration)
                    .SetEase(zoom1Ease);

                yield return transform
                    .DOMove(to1, zoom1Duration)
                    .SetEase(zoom1Ease)
                    .WaitForCompletion();

                // 2
                cg.DOFade(0, zoom2Duration).SetEase(zoom2Ease);

                yield return transform
                    .DOMove(to2, zoom2Duration)
                    .SetEase(zoom2Ease)
                    .WaitForCompletion();

                Destroy(gameObject);
            }
        }
    }
}