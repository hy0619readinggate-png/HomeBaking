using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A01;

namespace DoDoEng.Activity.C1_A01
{
    public class C1_A01_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            using (LOG.Coroutine($"Build() | {ActIDX}", this))
            {
                LOG.Assert(curriculum.ProblemCount == 1, "problem count must be 1", this);

                // 문제 데이터 추출
                tables = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
                LOG.Assert(tables.Length == 1, "tables data must be == 1", this);

                // 문제 데이터 생성
                var problem1 = new ProblemData
                {
                    Text = tables[0].Text,
                    TracingPB = await loadPrefab(tables[0].PrefabsTracing1),
                    DecoPB = await loadPrefab(tables[0].PrefabsDeco1),
                    Text1CLIP = await loadSound(tables[0].SoundText1),
                    Text2CLIP = await loadSound(tables[0].SoundText3),
                };
                var problem2 = new ProblemData
                {
                    Text = tables[0].Text,
                    TracingPB = await loadPrefab(tables[0].PrefabsTracing2),
                    DecoPB = await loadPrefab(tables[0].PrefabsDeco2),
                    Text1CLIP = await loadSound(tables[0].SoundText2),
                    Text2CLIP = await loadSound(tables[0].SoundText3),
                };

                // LMS(ActivityProgress) 빈칸수 설정
                BlanksCount = 2;

                return new ProblemData[] { problem1, problem2 };
            }
        }
    }

    public class ProblemData
    {
        public string Text;
        public GameObject TracingPB;
        public GameObject DecoPB;
        public AudioClip Text1CLIP;
        public AudioClip Text2CLIP;

        public override string ToString()
        {
            return $"<color=red>ProblemData [{Text} | {TracingPB.name} | {Text1CLIP.name} | {Text2CLIP.name}]</color>";
        }
    }
}