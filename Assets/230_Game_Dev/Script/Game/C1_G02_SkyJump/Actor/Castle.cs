using beyondi.Coroutine;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    [RequireComponent(typeof(Platform))]
    public class Castle : MonoBehaviour, ICompletable
    {
        // Fields : caching
        private Platform platform_ = null;
        private Platform platform => platform_ ??= GetComponent<Platform>();

        // Fields
        private bool playerArrived = false;

        // Event Handlers
        private void platform_OnPlayerEnter(Player player)
        {
            LOG.Info($"platform_OnPlayerEnter()", this);

            player.AutoJump = false;
            playerArrived = true;
        }
        private void platform_OnPlayerExit(Player player)
        {
            LOG.Info($"platform_OnPlayerExit()", this);
        }



        // Unity Messages
        private void Awake()
        {
            playerArrived = false;
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            platform.OnPlayerEnter += platform_OnPlayerEnter;
            platform.OnPlayerExit += platform_OnPlayerExit;
        }
        private void OnDisable()
        {
            platform.OnPlayerEnter -= platform_OnPlayerEnter;
            platform.OnPlayerExit -= platform_OnPlayerExit;
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => playerArrived;
    }
}