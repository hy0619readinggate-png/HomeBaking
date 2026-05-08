using System.Collections;
using UnityEngine;


namespace DoDoEng.Game.C3_G02
{
    public class ItemControl : MonoBehaviour
    {

        // Methods
        public void Show(Vector3 pos)
        {
            if (hasItem(pos, out Item item))
            {
                item?.Show();
            }
        }
        public bool HasItem(Vector3 pos)
        {
            if (hasItem(pos, out Item item))
                return true;

            return false;
        }
        public void GetItem(Vector3 pos)
        {
            if (hasItem(pos, out Item item))
                StartCoroutine(coGet(item));

        }
        public void HideScan()
        {
            _ScannerFXANI.SetTrigger(hashKey_Hide);
        }



        // Fields
        private Coroutine scanCoroutine = null;
        private float scanTime = 0f;

        // Fields : ani
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");



        // Functions
        private bool hasItem(Vector3 pos, out Item item)
        {
            item = getItem(pos);
            return item != null;
        }
        private Item getItem(Vector3 pos)
        {
            var col = Physics2D.OverlapPoint(pos);
            if (col != null)
            {
                var item = col.GetComponentInParent<Item>();
                if (item != null)
                    return item;
            }

            return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _ClockANI = null;
        [SerializeField] private Animator _ScannerANI = null;
        [SerializeField] private Animator _ScannerFXANI = null;



        // Unity Coroutine
        IEnumerator coGet(Item item)
        {
            //yield return new WaitForSeconds(0.5f);
            ItemType itemType = item.ItemType;

            float scanMaxTime = item.ScanTime;
            float timerMaxTime = item.ClockTime;
            Animator ani = null;

            switch (item.ItemType)
            {
                case ItemType.Clock: ani = _ClockANI; break;
                case ItemType.Scanner: ani = _ScannerANI; break;
                default:
                    break;
            }

            item.Hide();

            if (ani != null)
            {
                ani.SetTrigger(hashKey_Show);
                yield return null;
                yield return new WaitUntil(() => ani.GetCurrentAnimatorStateInfo(0).IsTag("Hide"));
            }


            // 아이템 활성화
            if (itemType == ItemType.Clock)
                C3_G02_Main.Instance.Clock(timerMaxTime);

            else if (itemType == ItemType.Scanner)
            {
                scanTime = 0f;

                if (scanCoroutine == null)
                    scanCoroutine = StartCoroutine(coScan(scanMaxTime));
            }
            yield return null;

        }
        IEnumerator coScan(float maxTime)
        {
            _ScannerFXANI.SetTrigger(hashKey_Show);
            yield return null;

            while (scanTime < maxTime)
            {
                scanTime += Time.deltaTime;
                yield return null;
            }

            _ScannerFXANI.SetTrigger(hashKey_Hide);
            yield return null;

            scanCoroutine = null;
        }
    }
}