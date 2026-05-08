using UnityEngine;
using TMPro;


namespace DoDoEng.Game.C3_G01
{
    public class LetterUI : MonoBehaviour
    {

        // Methods
        public void Setup(bool empty, string value)
        {
            foreach (var txt in _Texts)
                txt.text = value;

            isEmpty = empty;
        }
        public void Glow()
        {
            _Animator.SetTrigger(hashKey_Glow);
        }
        public void Correct()
        {
            _Animator.SetTrigger(hashKey_Correct);
        }



        // Fields
        private readonly int hashKey_Empty = Animator.StringToHash("Empty");
        private readonly int hashKey_Fill = Animator.StringToHash("Fill");
        private readonly int hashKey_Glow = Animator.StringToHash("Glow");
        private readonly int hashKey_Correct = Animator.StringToHash("Correct");

        private bool isEmpty = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _Animator = null;
        [SerializeField] private TextMeshProUGUI[] _Texts = null;

        // Unity Messages
        private void OnEnable()
        {
            if (isEmpty)
                _Animator.SetTrigger(hashKey_Empty);
            else _Animator.SetTrigger(hashKey_Fill);
        }
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }

}