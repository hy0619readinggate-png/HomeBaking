using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public enum ItemType { Clock, Scanner }

    public class Item : MonoBehaviour
    {


        // Properties
        public ItemType ItemType => _Type;
        public float ClockTime => clockTime;
        public float ScanTime => scanTime;



        // Methods
        public void Init()
        {
            clockTime = C3_G02_Main.Instance.ClockTime;
            scanTime = C3_G02_Main.Instance.ScanTime;
        }
        public void Show()
        {
            if (isShowing)
                return;

            isShowing = true;
        }
        public void Appear()
        {
            anim.SetTrigger(hashKey_Show);
        }
        public void Hide()
        {
            anim.SetTrigger(hashKey_Hit);
            Destroy(gameObject);
        }



        // Fields
        private bool isShowing = false;
        private float clockTime = 15f;
        private float scanTime = 10f;

        // Fields : ani
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Hit = Animator.StringToHash("Hit");


        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ItemType _Type = ItemType.Clock;
        [SerializeField] private Animator anim;
    }

}