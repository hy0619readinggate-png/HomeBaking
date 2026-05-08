using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class CharacterGroup : MonoBehaviour
    {
        // Properties
        public Character Current { get; private set; }

        // Methods
        public void SwitchTo(int pNO)
        {
            LOG.Info($"SwitchTo() | {pNO}", this);

            foreach (var (ch, idx) in characters.Select((ch, idx) => (ch, idx)))
                ch.gameObject.SetActive(pNO == idx + 1);

            Current = characters.SingleOrDefault(ch => ch.gameObject.activeSelf);
            Current.PlayAnimation(Current.DefaultIdle, false);
        }
        public void ShowCompleteVfx(bool show)
        {
            LOG.Info($"ShowCompleteVfx() | {show}", this);

            vfxComplete1GO.SetActive(show);
            vfxComplete2PS.gameObject.SetActive(show);

            Current.PlayAnimation(CharacterAnimation.Correct1, Current.DefaultIdle);
        }
        public void PlayCompleteSFX()
        {
            LOG.Info($"PlayCompleteSFX()", this);

            Current.PlayCompleteSFX();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character[] characters = null;
        [SerializeField] private GameObject vfxComplete1GO = null;
        [SerializeField] private ParticleSystem vfxComplete2PS = null;

        // Unity Messages
        private void Awake()
        {
            vfxComplete1GO.SetActive(false);
            vfxComplete2PS.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}