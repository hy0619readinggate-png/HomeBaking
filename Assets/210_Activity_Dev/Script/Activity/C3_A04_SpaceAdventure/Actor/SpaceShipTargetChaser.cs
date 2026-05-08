using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class SpaceShipTargetChaser : MonoBehaviour
    {
        // Methods
        public void StartChase(Transform tr)
        {
            LOG.Function(this);

            stopDelay();
            delayTW = DOVirtual.DelayedCall(5, () => { targetTR = tr; });
        }
        public void FinishChase()
        {
            LOG.Function(this);

            stopDelay();
            targetTR = null;
        }



        // Fields
        private bool isShownIndicator = false;
        private Transform targetTR = null;
        private Tween delayTW;

        // Functions
        private bool isOutOfScreen()
        {
            var pos = Camera.main.WorldToViewportPoint(targetTR.position);
            return
                pos.x < 0 - extraBoundary || pos.x > 1 + extraBoundary ||
                pos.y < 0 - extraBoundary || pos.y > 1 + extraBoundary;
        }
        private void stopDelay()
        {
            if (delayTW != null)
            {
                delayTW.Kill();
                delayTW = null;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private DirectionalIndicator indicator = null;
        [Header("★ Config")]
        [SerializeField] private float extraBoundary = 0.1f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (targetTR != null)
            {
                var isOutside = isOutOfScreen();
                if (isShownIndicator)
                {
                    if (!isOutside)
                    {
                        isShownIndicator = false;
                        indicator.Hide();
                    }
                }
                else
                {
                    if (isOutside)
                    {
                        isShownIndicator = true;
                        indicator.Show();
                    }
                }
            }
            else
            {
                if (isShownIndicator)
                {
                    isShownIndicator = false;
                    indicator.Hide();
                }
            }

            //if (isShownIndicator)
            if (targetTR != null)
            {
                var direction = targetTR.position - transform.position;
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                indicator.UpdateAngle(angle);
            }
        }


    }
}