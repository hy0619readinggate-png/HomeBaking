using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public class IceCream : MonoBehaviour
    {
        // Properties
        public int ColorID { get; private set; }

        // Methods
        public void Setup(int colorID)
        {
            LOG.Info($"Setup() | {colorID}", this);

            ColorID = colorID;
            for (var i = 0; i < imageGO.Length; i++)
                imageGO[i].SetActive(i == colorID - 1);
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);

            if (anim != null)
                anim.SetTrigger("Drop");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] imageGO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}