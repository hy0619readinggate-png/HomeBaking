using System.Collections;
using UnityEngine;

#pragma warning disable 0414

namespace DoDoEng.Activity.C1_A10
{
    public class Test02 : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform waitTR = null;
        [SerializeField] private Transform enterTR = null;
        [SerializeField] private Transform exitTR = null;
        [SerializeField] private Transform friendWaitTR = null;
        [SerializeField] private Friend[] friends = null;

        // Unity Messages
        private void Awake()
        {
            PlayerController.One.EnableInteraction(false);

            friends[0].Setup(1);
            friends[1].Setup(2);
            friends[2].Setup(3);

            Dodo.One.AddFriend(friends[0]);
            Dodo.One.AddFriend(friends[1]);


        }
        private void Start()
        {
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(coTest1());
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(coTest2());
            }
        }

        // Unity Coroutine
        IEnumerator coTest1()
        {
            Dodo.One.TeleportTo(waitTR.position);
            yield return null;

            yield return Dodo.One.MoveAndWait(enterTR.position);
            yield return null;
        }
        IEnumerator coTest2()
        {
            friends[2].TeleportTo(friendWaitTR.position);
            yield return null;

            var pos = Dodo.One.GetNextFriendPosition();
            yield return friends[2].MoveAndWait(pos);

            Dodo.One.AddFriend(friends[2]);
            yield return null;

            yield return Dodo.One.MoveAndWait(exitTR.position);
        }
    }
}
