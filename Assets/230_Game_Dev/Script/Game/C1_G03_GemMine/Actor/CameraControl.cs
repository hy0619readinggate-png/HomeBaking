using beyondi.Behaviour;
using Com.LuisPedroFonseca.ProCamera2D;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    [RequireComponent(typeof(ProCamera2D))]
    public class CameraControl : BYDSingleton<CameraControl>
    {
        // Properties
        public bool SuppressPan
        {
            get => !proCamPan.AllowPan;
            set => proCamPan.AllowPan = !value;
        }

        // Methods
        public void EnablePan(bool enable)
        {
            LOG.Function(this, $"{enable}");

            enablePan = enable;

            proCam.enabled = true;
            proCamPan.enabled = enablePan;
        }
        public void FollowPlayer()
        {
            LOG.Function(this);

            proCam.enabled = true;
            proCam.AddCameraTarget(playerTR);

            proCamPan.enabled = false;
        }
        public void UnfollowPlayer()
        {
            LOG.Function(this);

            proCam.RemoveCameraTarget(playerTR);

            proCamPan.enabled = enablePan;
        }
        public void CenterOnPlayer()
        {
            LOG.Function(this);

            proCamPan.enabled = false;

            proCam.AddCameraTarget(playerTR);
            proCam.CenterOnTargets();
            proCam.RemoveCameraTarget(playerTR);
        }


        // Fields
        private bool enablePan = false;

        // Fields : caching
        private ProCamera2D proCam_ = null;
        private ProCamera2D proCam => proCam_ ??= GetComponent<ProCamera2D>();
        private ProCamera2DPanAndZoom proCamPan_ = null;
        private ProCamera2DPanAndZoom proCamPan => proCamPan_ ??= GetComponent<ProCamera2DPanAndZoom>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform playerTR = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            enablePan = false;

            proCam.enabled = false;
            proCamPan.enabled = false;
            proCamPan.AllowPan = true;
        }
        private void Start()
        {
            // 레터박스때문에 다시 한번 계산이 필요함 by veramocor
            proCam.CalculateScreenSize();
        }
    }
}