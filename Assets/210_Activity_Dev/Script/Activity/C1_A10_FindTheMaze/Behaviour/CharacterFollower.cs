using DoDoEng.Activity.C3_A04;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    // 다음을 참고해서 썰매끌기 식으로 구현도 가능하지만, 따라다니는 애들이 벽에 걸리는 현상이 생기기도 해서, 사용하지 않음
    // https://gamedev.stackexchange.com/a/198199

    [RequireComponent(typeof(CharacterMotion))]
    public class CharacterFollower : MonoBehaviour
    {
        // Properties
        public bool IsFollowing => isFollowing;

        // Methods
        public void SetFollow(CharacterLeader leader, float distance)
        {
            LOG.Info($"SetFollow() {leader.gameObject.name} {distance}", this);

            this.leader = leader;
            this.distance = distance;
        }



        // Fields : caching
        private CharacterMotion motion_ = null;
        private CharacterMotion motion => motion_ ??= GetComponent<CharacterMotion>();

        // Fields
        private bool isFollowing = false;




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterLeader leader = null;
        [Header("★ Config")]
        [SerializeField] private float distance = 1;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if (leader != null)
            {
                var pos = leader.GetPosition(distance);

                if (isFollowing && pos == null)
                {
                    isFollowing = false;
                }
                if (!isFollowing && pos != null)
                {
                    transform.position = pos.Value;
                    isFollowing = true;
                }

                if (isFollowing)
                {
                    if (pos != null)
                        motion.MoveTo(pos.Value);
                    else motion.StopMoving();
                }
            }
        }
    }
}