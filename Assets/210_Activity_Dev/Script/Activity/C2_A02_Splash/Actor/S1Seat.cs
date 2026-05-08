using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A02
{
    public class S1Seat : MonoBehaviour
    {
        // Properties
        public int[] CharacterIDs => characters.Select(c => c.CharacterID).ToArray();

        // Methods
        public void Setup()
        {
            LOG.Info($"Setup()", this);

            characters.ForEach(c => c.Empty());
            characters.ForEach(c => c.gameObject.SetActive(false));
        }
        public void Sit(int seatID, int characterID)
        {
            LOG.Info($"Sit() | {seatID}, {characterID}", this);

            characters[seatID - 1].Setup(characterID);
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            characters.Where(c => c.CharacterID > 0).ForEach(c => c.Idle());
        }
        public void IdleOnShip()
        {
            LOG.Info($"IdleOnShip()", this);

            characters.Where(c => c.CharacterID > 0).ForEach(c => c.IdleOnShip());
        }
        public void Cheese(int[] characterIds)
        {
            LOG.Info($"Cheese() | {string.Join(",", characterIds)}", this);

            foreach (var (c, i) in characters.Select((c, i) => (c, i)))
            {
                var active = characterIds[i] > 0;
                c.gameObject.SetActive(active);

                if (active)
                {
                    c.Setup(characterIds[i]);
                    c.Cheese();
                }
            }
        }
        public void PlayCheeseSound()
        {
            LOG.Info($"PlayCheeseSound()", this);

            foreach (var c in characters)
            {
                if (c.gameObject.activeSelf)
                    c.PlayCheeseSound();
            }
        }
        public void StopCheeseSound()
        {
            LOG.Info($"StopCheeseSound()", this);

            foreach (var c in characters)
                c.StopCheeseSound();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character[] characters = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}