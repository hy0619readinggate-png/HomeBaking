using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A02
{
    public class S1EntranceMGR : MonoBehaviour
    {
        // Properties
        public int GetCharacterID(int entranceID) => charIndices[entranceID - 1];
        public void Idle() => s1Entrances.ForEach(e => e.Character.Idle());

        // Methods
        public void Init(Transform floatTR)
        {
            LOG.Info($"Init()", this);

            s1Entrances.ForEach(e => e.ExampleText.Init(floatTR));
        }
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup() | {string.Join(",", exams.Select(t => t.Text))}", this);

            charIndices = UtilArray.Random(1, s1Entrances.Length);

            s1Entrances.ForEach((i, e) => e.Setup(exams[i], charIndices[i]));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Correct(int entranceID)
        {
            LOG.Info($"Correct() | {entranceID}", this);

            s1Entrances[entranceID - 1].Correct();
        }
        public void Wrong(int entranceID)
        {
            LOG.Info($"Wrong() | {entranceID}", this);

            s1Entrances.ForEach(e => e.Character.Wrong());
            s1Entrances[entranceID - 1].Wrong();
        }

        // Methods
        public Transform[] GetAnswerEntrancesTRs(string word)
        {
            LOG.Info($"GetAnswerEntrancesTRs() | {word}", this);

            s1Entrances.ForEach(e => LOG.Important($"{e.ExampleText.Text}", this));

            return s1Entrances
                        .Where(e => e.ExampleText.Text == word)
                        .Select(e => e.transform)
                        .ToArray();
        }



        // Fields : caching
        private S1Entrance[] s1Entrances_ = null;
        private S1Entrance[] s1Entrances => s1Entrances_ ??= GetComponentsInChildren<S1Entrance>(true);
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private int[] charIndices = null;



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            s1Entrances.AutoFillID();
        }
        private void Start()
        {
        }
    }
}