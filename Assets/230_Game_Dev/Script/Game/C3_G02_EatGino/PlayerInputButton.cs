using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Game.C3_G02
{
    [RequireComponent(typeof(EventTrigger))]
    public class PlayerInputButton : MonoBehaviour
    {
        // Methods
        public void EnableButton()
        {
            isButtonActive = true;
            button.enabled = true;
        }
        public void DisableButton()
        {
            isButtonActive = false;
            button.enabled = false;
        }

        // Event
        public event Action<bool> OnDown = null;



        // Fields
        private bool isButtonActive = false;
        // Fields : cachings
        private EventTrigger eventTrigger_ = null;
        private EventTrigger eventTrigger => eventTrigger_ ??= GetComponent<EventTrigger>();
        private Button button_ = null;
        private Button button => button_ ??= GetComponent<Button>();



        // Event Handlers
        private void pointerDown(BaseEventData data)
        {
            if (isButtonActive) OnDown?.Invoke(true);
        }
        private void pointerUp(BaseEventData data)
        {
            if (isButtonActive) OnDown?.Invoke(false);
        }



        // Unity Messages
        private void Awake()
        {
            EventTrigger.Entry down = new EventTrigger.Entry();
            down.eventID = EventTriggerType.PointerDown;
            down.callback.AddListener(pointerDown);

            EventTrigger.Entry up = new EventTrigger.Entry();
            up.eventID = EventTriggerType.PointerUp;
            up.callback.AddListener(pointerUp);

            eventTrigger.triggers.Add(down);
            eventTrigger.triggers.Add(up);
        }
    }

}