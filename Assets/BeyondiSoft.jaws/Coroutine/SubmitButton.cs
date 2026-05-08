using UnityEngine;
using UnityEngine.UI;

namespace beyondi.Coroutine
{
    [RequireComponent(typeof(Button))]
    public class SubmitButton : MonoBehaviour, ISubmitable
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            isPressed = false;
            btn.interactable = enable;
        }



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private bool isPressed = false;



        // Unity Messages
        private void Awake()
        {
            btn.interactable = false;
            btn.onClick.AddListener(() => isPressed = true);
        }
        private void Start()
        {
        }



        // Implementation : ISubmitable
        public bool IsSubmit => isPressed;
    }
}