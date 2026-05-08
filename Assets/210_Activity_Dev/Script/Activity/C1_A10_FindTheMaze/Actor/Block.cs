using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class Block : MonoBehaviour
    {
        // Definitions
        private static float BLOCK_WIDTH = 1.89f;
        private static float BLOCK_HEIGHT = 1.54f;

        // Fields
        private static Block[,] blocks = new Block[8, 11];



        // Properties
        public int BlockType { get; private set; } = -1;

        // Methods
        public static void RandomizeBlocks(Transform tr)
        {
            Array.Clear(blocks, 0, blocks.Length);

            var allBlocks = tr.GetComponentsInChildren<Block>(true);
            foreach (var block in allBlocks)
            {
                blocks[block.Col, block.Row] = block;
            }

            for (var i = 0; i < blocks.GetLength(0); i++)
                for (var j = 0; j < blocks.GetLength(1); j++)
                    blocks[i, j]?.randomize();
        }


        // Properties
        public int Col => (int)(transform.localPosition.x / BLOCK_WIDTH);
        public int Row => (int)(transform.localPosition.y / BLOCK_HEIGHT);

        // Methods
        private void randomize()
        {
            var leftBlock = Col > 0 ? blocks[Col - 1, Row] : null;
            var leftBlockType = leftBlock?.BlockType ?? -1;

            var downBlock = Row > 0 ? blocks[Col, Row - 1] : null;
            var downBlockType = downBlock?.BlockType ?? -1;

            BlockType = getBlockType(leftBlockType, downBlockType);
            transform.GetChildren().ToArray().SetActiveOnly(BlockType);

            var childBlock = transform.GetChild(BlockType);
            if (childBlock.childCount == 0)
                return;

            var flowerColorCount = childBlock.GetChild(0).childCount;
            if (flowerColorCount == 0)
                return;

            var randomFlowerColorIdx = UtilArray.RandomOne(0, flowerColorCount - 1);
            foreach (var flower in childBlock.GetChildren())
                flower.GetChildren().ToArray().SetActiveOnly(randomFlowerColorIdx);
        }
        private int getBlockType(params int[] exceptTypes)
        {
            var pool1 = UtilArray.Random(0, transform.childCount - 1);
            var pool2 = pool1.Where(d => !exceptTypes.Contains(d)).ToArray();

            if (pool2.Length == 0)
                return UtilArray.ExtractOne(pool1);
            else return UtilArray.ExtractOne(pool2);
        }




        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}