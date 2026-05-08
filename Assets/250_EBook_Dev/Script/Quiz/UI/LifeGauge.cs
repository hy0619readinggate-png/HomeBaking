using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    public class LifeGauge : MonoBehaviour
    {
        // Properties
        public int Life => life;

        // Methods
        public void Setup(int lifeCount)
        {
            LOG.Info($"Setup() | {lifeCount}", this);

            life = lifeCount;

            if (lifeCount > 1)
            {
                gameObject.SetActive(true);
                for (var i = 0; i < heartANIM.Length; i++)
                {
                    var valid = i < lifeCount;
                    heartANIM[i].gameObject.SetActive(valid);
                    if (valid)
                        heartANIM[i].SetTrigger("Enable");
                }
            }
            else gameObject.SetActive(false);
        }
        public void Decrease()
        {
            LOG.Info($"Decrease()", this);

            life--;
            if (gameObject.activeSelf && life >= 0)
            {
                AudioMGR.One.PlayEffect(brokenSFX);
                heartANIM[life].SetTrigger("Break");
            }
        }



        // Fields
        private int life = 0;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator[] heartANIM = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip brokenSFX = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}