using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    public class UIHealthBar : MonoBehaviour
    {
        // Methods
        public void Setup(float health)
        {
            LOG.Info($"Setup() | {health}", this);

            totalHP = health;
            currentHP = totalHP;
        }
        public void UpdateHP(float health)
        {
            LOG.Info($"UpdateHP() | {health}", this);

            AudioMGR.One.PlayEffect(changeCLIP);
            currentHP = health;
        }



        // Fields
        private float totalHP = 1;
        private float currentHP = 1;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image progressIMG = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip changeCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float lerpRatio = 10;

        // Unity Messages
        private void Awake()
        {
            progressIMG.fillAmount = 1;
        }
        private void Start()
        {
        }
        private void Update()
        {
            var hpRatio = currentHP / totalHP;
            progressIMG.fillAmount = Mathf.Lerp(progressIMG.fillAmount, hpRatio, Time.deltaTime * lerpRatio);
        }
    }
}