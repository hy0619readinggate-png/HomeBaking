using TMPro;
using UnityEngine;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Game.C4_G03
{
    public class AnswerBox : MonoBehaviour
    {
        // Methods
        public void Show()
        {
            anim.SetTrigger(hashKeyShow);
        }
        public void Glow()
        {
            anim.SetTrigger(hashKeySuccess);
        }
        public void SetText(string answer)
        {
            answerText.text = answer;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        // Fields : Anim
        private readonly int hashKeyShow = Animator.StringToHash("Show");
        private readonly int hashKeySuccess = Animator.StringToHash("Success");



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI answerText = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}