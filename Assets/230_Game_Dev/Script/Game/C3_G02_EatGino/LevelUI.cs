using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C3_G02
{
    public class LevelUI : MonoBehaviour
    {
        // Methods
        public void SetJellyCount(int foundJellyNum)
        {
            correctJellyCount.text = string.Format("{0}/{1}", foundJellyNum, C3_G02_Main.Instance.JellyCount);
        }
        public void SetJellyIllust(int idx)
        {
            for (int i = 0; i < jellyIllusts.Length; i++)
                jellyIllusts[i].SetActive(i == idx);
        }
        public void ResetJellyCount()
        {
            correctJellyCount.text = string.Format("{0}/{1}", 0, C3_G02_Main.Instance.JellyCount);
        }
        public void PlaySpeaker(AudioClip wordSound)
        {
            StartCoroutine(coPlayQuiz(wordSound));
        }
        public void SetSpeaker(bool isActive)
        {
            speaker.enabled = isActive;
        }
        public void PlayCompleteFX()
        {
            completeFX.SetTrigger(hashKey_Complete);
        }


        // Fields
        private AudioClip quizSound = null;
        // Fields : Anim
        private readonly int hashKey_Complete = Animator.StringToHash("Complete");

        // Functions
        private void playQuizWord()
        {
            AudioMGR.One.PlayNarration(quizSound);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI correctJellyCount = null;
        [SerializeField] private Button speaker = null;
        [SerializeField] private Animator completeFX = null;
        [SerializeField] private GameObject[] jellyIllusts = null;
        [Header("★ Configs")]
        [SerializeField] private float quizDelay = 2.0f;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {
            speaker.onClick.AddListener(playQuizWord);
        }

        // Unity Coroutines
        IEnumerator coPlayQuiz(AudioClip wordSound)
        {
            quizSound = wordSound;
            yield return new WaitForSeconds(quizDelay);

            AudioMGR.One.PlayNarration(quizSound);
        }
    }
}