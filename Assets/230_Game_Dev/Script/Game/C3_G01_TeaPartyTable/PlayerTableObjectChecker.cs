using System;
using UnityEngine;

namespace DoDoEng.Game.C3_G01
{
    public class PlayerTableObjectChecker : MonoBehaviour
    {

        // Event
        public event Action OnTriggerObstacle = null;
        public event Action<char?> OnTriggerText = null;
        public event Action OnTriggerBroom = null;
        public event Action OnTriggerBonus = null;


        // Unity Messages
        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null)
            {
                var tableObject = other.attachedRigidbody.GetComponent<TableObject>();
                if (tableObject != null)
                {
                    if (tableObject.IsText)
                        OnTriggerText?.Invoke(tableObject.Letter);
                    else
                    {
                        if (tableObject.IsBroom)
                        {
                            OnTriggerBroom?.Invoke();
                        }
                        else if (tableObject.IsMacaron)
                        {
                            OnTriggerBonus?.Invoke();
                        }
                        else
                        {
                            OnTriggerObstacle?.Invoke();
                        }
                    }
                }
            }
        }
    }

}