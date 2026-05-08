using UnityEngine;
using beyondi.Behaviour;
using DoDoEng.Common;

namespace DoDoEng.Game.C4_G02
{
    public class Question : BYDSingleton<Question>
    {

        // Methods
        public void Setup(int answerIndex, string[] texts)
        {
            count = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                if (string.IsNullOrEmpty(texts[i]) == false)
                {
                    Texts[i].Setup(answerIndex == i, texts[i]);

                    count++;
                }
            }

            foreach (var pt in _ClearParticle)
                pt.SetActive(false);
        }
        public void Show()
        {
            AudioMGR.One.PlayEffect(_AudioClip);

            for (int i = 0; i < Texts.Length; i++)
                Texts[i].gameObject.SetActive(i < count);
        }
        public void Hide()
        {
            foreach (var text in Texts)
                text.gameObject.SetActive(false);
        }
        public void Success()
        {
            foreach (var text in Texts)
            {
                if (text.gameObject.activeSelf)
                    text.HideCover();
            }

            foreach (var pt in _ClearParticle)
                pt.SetActive(true);
        }



        // Fields
        private int count = 0;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AudioClip _AudioClip = null;
        [SerializeField] private QuestionText[] Texts = null;
        [SerializeField] private GameObject[] _ClearParticle = null;

        // Unity Messages
        private void Start()
        {
            foreach (var pt in _ClearParticle)
                pt.SetActive(false);

            foreach (var go in Texts)
                go.gameObject.SetActive(false);
        }
    }
}