using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class DirectionalIndicator : MonoBehaviour
    {
        // Methods
        public void Show()
        {
            LOG.Function(this);

            vfxs.ForEach(v => v.Play());
            go.SetActive(true);
        }
        public void Hide()
        {
            LOG.Function(this);

            vfxs.ForEach(v => v.Stop());
            go.SetActive(false);
        }
        public void UpdateAngle(float angle)
        {
            var h = Camera.main.orthographicSize;
            var w = Camera.main.aspect * h;
            var sin = Mathf.Sin(angle * Mathf.Deg2Rad);
            var cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            var r1 = Mathf.Abs(h / sin);
            var x1 = r1 * cos;

            var r2 = Mathf.Abs(w / cos);
            var y2 = r2 * sin;

            var xx = Mathf.Clamp(x1, -w, w);
            var yy = Mathf.Clamp(y2, -h, h);

            var x = snap ? getSnapPosition(xx, 0, w) : xx;
            var y = snap ? getSnapPosition(yy, -h, h) : yy;

            transform.localPosition = new Vector3(x, y, 0);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Function
        private float getSnapPosition(float value, float stand, float gab)
        {
            return Mathf.Round((value - stand) / gab) * gab + stand;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ParticleSystem[] vfxs = null;
        [SerializeField] private GameObject go = null;
        [Header("★ Config")]
        [SerializeField] private bool snap = true;
        // Unity Messages
        private void Awake()
        {
            vfxs.ForEach(v => v.Stop());
            go.SetActive(false);
        }
        private void Start()
        {
        }
    }
}