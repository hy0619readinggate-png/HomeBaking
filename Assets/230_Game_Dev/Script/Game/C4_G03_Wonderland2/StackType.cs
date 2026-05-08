using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C4_G03
{
    public class StackType : MonoBehaviour
    {
        // Properties
        public List<Block> IntroGlowBlocks => introGlowBlocks;

        // Methods
        public void SetRestChunkRndIdx(List<string> restChunks)
        {
            rndRestIdx = UtilArray.Random(0, restChunks.Count - 1, restChunks.Count);
        }
        public void SetStackType(int addBlockCount)
        {
            var rndIdx = UnityEngine.Random.Range(0, stackTypeValue.Length);
            var tmpList = new List<Transform>();
            var rndStackType = stackTypeValue[rndIdx].Split(',');
            var trIdx = 0;

            for (int i = 0; i < rndStackType.Length; i++)
            {
                if (rndStackType[i] == "1")
                {
                    blockPf.Appear(emptyPosition[trIdx]);
                    stagePosition.Add(emptyPosition[trIdx]);
                }
                else if (rndStackType[i] == "2")
                {
                    tmpList.Add(emptyPosition[trIdx]);
                }
                trIdx++;
            }


            var addition = UtilArray.Random(0, tmpList.Count - 1, addBlockCount);

            for (int k = 0; k < addBlockCount; k++)
            {
                blockPf.Appear(tmpList[addition[k]]);
                stagePosition.Add(tmpList[addition[k]]);
            }

            foreach (Transform ep in emptyPosition) ep.GetComponent<Slot>().SetSlot(true);
            foreach (Transform sp in stagePosition) sp.GetComponent<Slot>().SetSlot(false);
        }
        public void SetMustCorrectBlock(int mustCorrectNum, List<string> chunks, List<string> restChunks, SentenceData sd)
        {
            List<Transform> answerLineBlock = new();
            List<int> answerLineBlockIdx = new();

            for (int i = 0; i < answerPosition.Length; i++)
            {
                if (answerPosition[i].GetComponentInChildren<Block>() != null)
                {
                    answerLineBlock.Add(answerPosition[i]);
                    answerLineBlockIdx.Add(i);
                }
            }
            var idx = 0;
            foreach (Transform answer in answerLineBlock)
            {
                answer.GetComponentInChildren<Block>().Setup(restChunks[rndRestIdx[idx]], sd);
                idx++;
            }

            if (mustCorrectNum > 0)
            {
                int[] rndIdx = UtilArray.Random(0, answerLineBlockIdx.Count - 1, mustCorrectNum);

                for (int i = 0; i < mustCorrectNum; i++)
                {
                    var introGlowBlock = answerLineBlock[rndIdx[i]].GetComponentInChildren<Block>();
                    introGlowBlock.Setup(chunks[answerLineBlockIdx[rndIdx[i]]], sd);

                    restChunks.Add(restChunks[rndIdx[i]]);
                    introGlowBlock.IsCorrect = true;
                    glowConditions[answerLineBlockIdx[rndIdx[i]]] = 1;

                    introGlowBlocks.Add(introGlowBlock);
                    selectedIdx.Add(answerLineBlockIdx[rndIdx[i]]);
                }
            }
        }
        public void SetBlock(List<string> restChunks, List<string> currChunks, SentenceData sd)
        {
            foreach (Transform t in answerPosition)
                if (stagePosition.Contains(t)) stagePosition.Remove(t);

            var rndIdx = UtilArray.Random(0, stagePosition.Count - 1, stagePosition.Count);
            int idx = 0;

            for (int i = 0; i < currChunks.Count; i++)
            {
                if (!selectedIdx.Contains(i))
                {
                    stagePosition[rndIdx[idx]].GetComponentInChildren<Block>().Setup(currChunks[i], sd);
                    idx++;
                }
            }

            for (int j = 0; j < stagePosition.Count - currChunks.Count + selectedIdx.Count; j++)
            {
                var rndIdxSelect = j + currChunks.Count - selectedIdx.Count;
                var rnd = rndIdx[rndIdxSelect];
                var stagePos = stagePosition[rnd];
                var block = stagePos.GetComponentInChildren<Block>();

                var restRnd = rndRestIdx[j + currChunks.Count];

                block.Setup(restChunks[restRnd], sd);
            }

            foreach(Transform t in stagePosition)
            {
                if(t.GetComponentInChildren<Block>() != null)
                    t.GetComponentInChildren<Block>().OnBlockReturned += StackType_OnBlockReturned;
            }
        }
        public void SetGlowBox(int hashIdx)
        {
            switch (hashIdx)
            {
                case 0:
                    DOVirtual.DelayedCall(0.5f, () => answerBox.Show());
                    break;
                case 1:
                    answerBox.Glow();
                    break;
            }
        }
        public void SetAnswer(List<string> answer)
        {
            answerChunks = answer;
        }
        public void Setup(Sprite sprite, AudioClip audioclip)
        {
            _ProblemBoardIMG.sprite = sprite;
            sentenceAudioClip = audioclip;
        }
        public void ShowProblemBoard()
        {
            _ProblemBoardANI.SetTrigger(hashKeyShow);

            AudioMGR.One.PlayNarration(sentenceAudioClip);
        }
        public void MoveProblemBoard()
        {
            _ProblemBoardANI.SetTrigger(hashKeyMove);
        }


        // Function
        private void getStackType()
        {
            if (stackTextAsset == null)
            {

            }
            else
            {
                stackTypeValue = stackTextAsset.text.Split("\n");
            }
        }
        private void PlayProblemBoard()
        {
            AudioMGR.One.PlayNarration(sentenceAudioClip);
        }
        // Events
        public event Action OnClear;



        // Event Handlers
        private void slot_drop(bool isDropped)
        {
            StartCoroutine(crDrop(isDropped));
        }
        private void StackType_OnBlockReturned(int idx)
        {
            glowConditions[idx] = 1;
        }

        // Fields
        private bool isClear = false;
        private string[] stackTypeValue = null;
        private int[] glowConditions = null;
        private int[] rndRestIdx = null;

        private List<Transform> stagePosition = new();
        private List<Block> introGlowBlocks = new();
        private List<string> answerChunks = null;
        private List<int> selectedIdx = new();
        private AudioClip sentenceAudioClip = null;

        // Fields : Anim
        private readonly int hashKeyShow = Animator.StringToHash("Show");
        private readonly int hashKeyMove = Animator.StringToHash("Move");


        // Unity Inspectors
        [Header("★ Puzzles")]
        [SerializeField] private Transform[] emptyPosition = null;
        [SerializeField] private Transform[] answerPosition = null;
        [SerializeField] private Block blockPf = null;
        [Header("★ Bindings")]
        [SerializeField] private AnswerBox answerBox = null;
        [SerializeField] private Animator _ProblemBoardANI = null;
        [SerializeField] private Image _ProblemBoardIMG = null;
        [SerializeField] private Button _ProblemBoardBTN = null;
        [Header("★ Configs")]
        [SerializeField] private TextAsset stackTextAsset = null;
        [SerializeField] private int typeIdx = 0;

        // Unity Messages
        private void Awake()
        {
            getStackType();
            _ProblemBoardBTN.onClick.AddListener(PlayProblemBoard);

            glowConditions = new int[answerPosition.Length];
        }
        private void Start()
        {
            for(int i = 0; i < answerPosition.Length; i++)
                answerPosition[i].GetComponent<Slot>().AnswerSlotIdx = i;
        }
        private void OnEnable()
        {
            foreach (Transform t in emptyPosition)
                t.GetComponent<Slot>().OnDrop += slot_drop;

            StartCoroutine(crCheckGlow());
        }

        private void OnDisable()
        {
            foreach (Transform t in emptyPosition)
                t.GetComponent<Slot>().OnDrop -= slot_drop;
        }

        // Coroutines
        IEnumerator crCheckGlow()
        {
            yield return null;

            while (!isClear)
            {
                for(int i = 0; i < answerPosition.Length; i++)
                {
                    if (answerPosition[i].GetComponentInChildren <Block>() == null)
                        glowConditions[i] = 0;
                }
                yield return null;
            }
        }
        IEnumerator crDrop(bool isDropped)
        {
            if (isDropped)
            {
                for (int i = 0; i < emptyPosition.Length; i++)
                    if (emptyPosition[i].GetComponentInChildren<Block>() != null)
                        emptyPosition[i].GetComponent<Slot>().CheckDrop();

                yield return new WaitForEndOfFrame();

                for (int j = 0; j < emptyPosition.Length; j++)
                {
                    var block = emptyPosition[j].GetComponentInChildren<Block>();
                    if (block != null)
                        block.IsCorrect = false;
                }

                yield return new WaitForEndOfFrame();

                int correctAnswerNum = 0;
                var answerNum = answerPosition.Length;

                for (int k = 0; k < answerNum; k++)
                {
                    var block = answerPosition[k].GetComponentInChildren<Block>();
                    yield return null;

                    if (block != null)
                    {
                        LOG.Important("Word : " + block.Word + " | Chunk : " + answerChunks[k], this);
                        yield return null;

                        if (block.Word.Equals(answerChunks[k]))
                        {
                            block.IsCorrect = true;

                            if (glowConditions[k] == 0)
                            {
                                block.Glow();
                                glowConditions[k] = 1;
                            }
                            correctAnswerNum += 1;
                        }
                        else
                        {
                            block.IsCorrect = false;
                            glowConditions[k] = 0;
                            correctAnswerNum -= 1;
                        }
                    }
                    else
                    {
                        LOG.Important("Word : NULL | Chunk : " + answerChunks[k], this);
                        glowConditions[k] = 0;
                    }
                    yield return null;
                }

                if (correctAnswerNum == answerNum)
                {
                    isClear = true;

                    answerBox.Glow();
                    answerBox.SetText(C4_G03_Main.Instance.AnswerSentence);

                    foreach (Transform tr in answerPosition) tr.GetComponentInChildren<Block>().HideText();

                    foreach (var block in emptyPosition)
                    {
                        if (block.GetComponentInChildren<Block>() != null)
                            block.GetComponentInChildren<Block>().enabled = false;
                    }

                    for (int i = typeIdx * 2; i < emptyPosition.Length; i++)
                    {
                        if (emptyPosition[i].GetComponentInChildren<Block>() != null)
                            emptyPosition[i].GetComponentInChildren<Block>().gameObject.SetActive(false);
                    }

                    for (int j = 0; j < typeIdx; j++) emptyPosition[j].GetComponentInChildren<Block>().Erase();
                    yield return new WaitForSeconds(1.0f);

                    AudioMGR.One.PlayNarration(sentenceAudioClip);
                    yield return null;

                    OnClear?.Invoke();
                }
            }
        }
    }
}