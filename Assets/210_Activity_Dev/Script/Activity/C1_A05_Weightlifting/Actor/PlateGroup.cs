using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A05
{
    [RequireComponent(typeof(Animator))]
    public class PlateGroup : MonoBehaviour
    {
        // Properties
        public Transform[] AnswerPlateTRs => plates
                                                .Where(p => p.IsAvaliable && p.IsAnswer)
                                                .Select(p => p.AffPos)
                                                .ToArray();

        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            plates.ForEach(p => p.Init());
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            foreach (var (plate, idx) in plates.Select((p, i) => (p, i)))
                plate.Setup(exams[idx]);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            AudioMGR.One.PlayEffect(hideCLIP);
            anim.SetTrigger("Down");
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Plate[] plates_ = null;
        private Plate[] plates => plates_ ??= GetComponentsInChildren<Plate>(true);



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip hideCLIP = null;


        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {

        }
    }
}