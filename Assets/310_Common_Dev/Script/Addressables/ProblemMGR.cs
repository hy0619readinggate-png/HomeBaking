using beyondi.Util;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Common
{
    public abstract class ProblemMGR<TCurriculum, TTableData, TProblemData> : MonoBehaviour
    {
        // Properties
        public TProblemData[] Problems => problems;
        public TProblemData Current => problems[pNO - 1];
        public int Count => problems.Length;
        public int PNO => pNO;

        // Methods
        public async UniTask Build(IndexBase index, TCurriculum curriculum, TTableData[] tables)
        {
            LOG.Info($"Build() | {index}", this);

            idx = index;

            pNO = 1;
            problems = await onBuild(curriculum, tables);
            problems.ForEach(p => LOG.Info($"{p}", this));
        }
        public void Goto(int pNO)
        {
            this.pNO = pNO;
        }
        public bool Next()
        {
            if (pNO < Count)
            {
                pNO++;
                return true;
            }
            else return false;
        }



        // Fields
        private IndexBase idx;
        private TProblemData[] problems;
        private int pNO = 0;

        // Fields
        private Dictionary<string, AudioClip> dictAudio = new Dictionary<string, AudioClip>();
        private Dictionary<string, Sprite> dictSprite = new Dictionary<string, Sprite>();

        // Functions
        protected IndexBase IDX => idx;
        protected async UniTask<GameObject> loadPrefab(string path)
        {
            return await idx.LoadPrefab(path);
        }
        protected async UniTask<AudioClip> loadSound(string path)
        {
            if (!dictAudio.ContainsKey(path))
                dictAudio[path] = await idx.LoadSound(path);
            return dictAudio[path];
        }
        protected async UniTask<Sprite> loadSprite(string path)
        {
            if (!dictSprite.ContainsKey(path))
                dictSprite[path] = await idx.LoadSprite(path);
            return dictSprite[path];
        }

        // Virtual
        protected virtual async UniTask<TProblemData[]> onBuild(TCurriculum curriculum, TTableData[] tables)
        {
            await UniTask.Yield();
            return null;
        }
    }
}