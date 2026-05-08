using System.Collections.Generic;
using UnityEngine;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Game.C4_G03
{
    public class PuzzleSlot : MonoBehaviour
    {
        // Properties
        public StackType StackType => _stackType;
        // Methods
        public void SetRestChunkRndIdx(int chunkNum, List<string> restChunks)
        {
            var selectedType = puzzleType[chunkNum % 3];
            selectedType.SetRestChunkRndIdx(restChunks);
        }
        public void SetPuzzleType(int chunkNum, int blockCount)
        {
            var selectedType = puzzleType[chunkNum % 3];
            _stackType = selectedType;

            selectedType.gameObject.SetActive(true);
            selectedType.SetStackType(blockCount);
        }
        public void SetMustCorrectBlock(int chunkNum, int mustCorrectNum, List<string> chunks, List<string> restChunks, SentenceData sd)
        {
            var selectedType = puzzleType[chunkNum % 3];
            selectedType.SetMustCorrectBlock(mustCorrectNum, chunks, restChunks, sd);
        }
        public void SetBlock(int chunkNum, List<string> restChunks, List<string> currChunks, SentenceData sd)
        {
            var selectedType = puzzleType[chunkNum % 3];
            selectedType.SetBlock(restChunks, currChunks, sd);
        }
        public void SetGlowBox(int chunkNum, int hashIdx)
        {
            var selectedType = puzzleType[chunkNum % 3];
            selectedType.SetGlowBox(hashIdx);
        }
        public void SetAnswer(int chunkNum, List<string> answer)
        {
            var selectedType = puzzleType[chunkNum % 3];
            selectedType.SetAnswer(answer);
        }
        public void Setup(Sprite sprite, AudioClip audioClip)
        {
            _stackType?.Setup(sprite, audioClip);
        }
        public void ShowProblemBoard()
        {
            _stackType?.ShowProblemBoard();
        }
        public void MoveProblemBoard()
        {
            _stackType?.MoveProblemBoard();
        }



        // Fields
        private StackType _stackType;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private StackType[] puzzleType = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}