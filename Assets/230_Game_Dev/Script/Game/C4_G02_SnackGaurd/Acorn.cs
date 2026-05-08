using DG.Tweening;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{
    public class Acorn : MonoBehaviour
    {

        // Methods
        public void Setup()
        {
            _HP = initialHP;
            prevHP = _HP;

            _ColliderGO.SetActive(true);
        }
        public void Show()
        {
            _Ani.SetTrigger(hashKey_Show);
        }
        public void Hide()
        {
            _Ani.SetTrigger(hashKey_Hide);
        }


        // Fields
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Eat = Animator.StringToHash("Eat");
        private readonly int hashKey_Finish = Animator.StringToHash("Finish");

        private int prevHP = 0;
        private int initialHP = 0;

        // Functions
        private void updateHP()
        {
            _Ani.SetTrigger(hashKey_Eat);

            _HP--;

            if (_HP <= 0)
            {
                _Ani.SetTrigger(hashKey_Finish);
                C4_G02_Main.Instance.GameOver();
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _Ani = null;
        [SerializeField] private GameObject _ColliderGO = null;
        [Header("★ Config")]
        [SerializeField] private int _HP = 0;

        // Unity Messages
        private void Start()
        {
            prevHP = _HP;
            initialHP = _HP;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var monster = collision.GetComponentInParent<Monster>();
            if (monster != null)
            {
                if (prevHP > 0)
                {
                    prevHP--;
                    monster.Eat(updateHP);
                }
                else
                {
                    _ColliderGO.SetActive(false);
                }
            }
        }
    }
}