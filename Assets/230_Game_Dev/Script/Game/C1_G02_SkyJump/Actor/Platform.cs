using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class Platform : MonoBehaviour
    {
        // Methods
        public void AttachPlayer(Player player)
        {
            LOG.Info($"AttachPlayer()", this);

            playerAttached = player;
            player.Attach(transform);
        }
        public void DetachPlayer()
        {
            LOG.Info($"DetachPlayer()", this);

            if (playerAttached != null)
            {
                if (gameObject.activeInHierarchy)
                    playerAttached.Detach();

                playerAttached = null;
            }
        }

        // Events
        public event Action<Player> OnPlayerEnter;
        public event Action<Player> OnPlayerExit;



        // Fields
        private Player playerAttached = null;

        // Functions
        private void reorderCoverZ(float z)
        {
            if (coverTR != null)
            {
                var pos = coverTR.localPosition;
                coverTR.localPosition = new Vector3(pos.x, pos.y, z);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform coverTR = null;
        [Header("★ Config")]
        [SerializeField] private bool stepDown = true;
        [SerializeField] private float stepDownAmount = -0.3f;
        [SerializeField] private float stepDownDuration = 0.3f;
        [SerializeField] private float stepUpDuration = 0.1f;
        [Header("★ Config - cover")]
        [SerializeField] private float coverFrontZ = -2f;
        [SerializeField] private float coverBackZ = -0.1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip arriveCLIP = null;

        // Unity Messages
        private void Awake()
        {
            reorderCoverZ(coverBackZ);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null && player.IsGround)
            {
                reorderCoverZ(coverFrontZ);
                AttachPlayer(player);

                if (arriveCLIP != null)
                    AudioMGR.One.PlayEffect(arriveCLIP);

                if (stepDown)
                {
                    var y = transform.position.y;
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOMoveY(y + stepDownAmount, stepDownDuration));
                    seq.Append(transform.DOMoveY(y, stepUpDuration));
                    seq.OnComplete(() => OnPlayerEnter?.Invoke(player));
                }
                else OnPlayerEnter?.Invoke(player);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null && playerAttached != null)
            {
                DetachPlayer();
                reorderCoverZ(coverBackZ);

                OnPlayerExit?.Invoke(player);
            }
        }
    }
}