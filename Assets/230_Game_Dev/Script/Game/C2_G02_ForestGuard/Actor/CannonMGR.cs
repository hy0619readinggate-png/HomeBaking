using beyondi.Behaviour;
using DoDoEng.Activity.C1_A11;
using DoDoEng.Common;

namespace DoDoEng.Game.C2_G02
{
    public class CannonMGR : BYDSingleton<CannonMGR>
    {
        // Methods
        public void ResurrectAll()
        {
            LOG.Info($"ResurrectAll()", this);

            foreach (var c in cannons)
            {
                if (!c.IsAlive)
                    c.Resurrect();
            }
        }

        // Methods
        public bool IsCannonAlive(int lane)
        {
            return cannons[lane - 1].IsAlive;
        }

        // Methods
        public void DEV_Fire(int lane, BulletData bulletData)
        {
            cannons[lane - 1].DEV_Fire(bulletData);
        }



        // Fields : caching
        private Cannon[] cannons_ = null;
        private Cannon[] cannons => cannons_ ??= GetComponentsInChildren<Cannon>(true);



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }
    }
}