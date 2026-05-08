using System.Collections;
using UnityEngine;

#pragma warning disable 0414

namespace DoDoEng.Activity.C1_A10
{
    public class Test01 : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform startTR = null;
        [SerializeField] private Transform finishTR = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(coTest());
            }
        }

        // Unity Coroutine
        IEnumerator coTest()
        {
            PlayerController.One.EnableInteraction(false);
            yield return null;

            Dodo.One.TeleportTo(startTR.position);
            yield return null;

            yield return Dodo.One.MoveAndWait(finishTR.position);
            yield return null;

            PlayerController.One.EnableInteraction(true);
            yield return null;
        }
    }
}