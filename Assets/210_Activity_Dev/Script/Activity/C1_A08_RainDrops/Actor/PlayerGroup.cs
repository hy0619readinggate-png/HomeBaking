using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    public class PlayerGroup : MonoBehaviour
    {
        // Properties
        public Player Current { get; private set; }
        public Player[] All => players;

        // Methods
        public void SwitchTo(int id)
        {
            LOG.Info($"SwitchTo() | {id}", this);

            players.SetActiveOnly(id - 1);
            Current = players.SingleOrDefault(p => p.gameObject.activeSelf);
        }



        // Fields : caching
        private Player[] players_ = null;
        private Player[] players => players_ ??= GetComponentsInChildren<Player>(true);
    }
}