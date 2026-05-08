using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.Common
{
    public class UILife : MonoBehaviour
    {
        // Properties
        public int CurrentLife => life;

        // Methods
        public void Setup()
        {
            LOG.Info($"Setup()", this);

            life = lifeANIM.Length;
            lifeANIM.ForEach(an => an.SetBool("On", true));
        }
        public void Decrease()
        {
            LOG.Info($"Decrease()", this);

            if (life > 0)
            {
                life--;
                lifeANIM[life].SetBool("On", false);
            }
        }



        // Fields
        private int life;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator[] lifeANIM = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}