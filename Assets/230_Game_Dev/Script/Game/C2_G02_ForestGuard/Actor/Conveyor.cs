using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public class Conveyor : MonoBehaviour
    {
        // Methods
        public void TurnOn()
        {
            LOG.Info($"TurnOn()", this);

            logs.ForEach(log => log.TurnOn());
        }
        public void TurnOff()
        {
            LOG.Info($"TurnOff()", this);

            logs.ForEach(log => log.TurnOff());
        }
        public void Damage()
        {
            LOG.Function(this);

            logs.ForEach(log => log.Damage());
        }



        // Fields : caching
        private ConveyorLog[] logs_ = null;
        private ConveyorLog[] logs => logs_ ??= GetComponentsInChildren<ConveyorLog>(true);



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}