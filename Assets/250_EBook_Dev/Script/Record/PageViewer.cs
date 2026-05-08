using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Record
{
    public class PageViewer : MonoBehaviour
    {
        // Methods
        public void Setup(RecordData[] recordData, Sprite[] pageImages)
        {
            LOG.Info($"Setup()", this);

            recordInfos = buildInfo(recordData, pageImages);
            changeTo(1);
        }
        public void ChangeTo(int recordNo)
        {
            LOG.Info($"ChangeTo() | {recordNo}", this);

            changeTo(recordNo);
        }



        // Fields
        private RecordInfo[] recordInfos = null;

        // Functions
        private RecordInfo[] buildInfo(RecordData[] recordData, Sprite[] pageImages)
        {
            var list = new List<RecordInfo>();
            foreach (var rd in recordData)
            {
                list.Add(new RecordInfo
                {
                    PageSPR = pageImages[rd.LayerNo - 1],
                    Sentence = rd.Sentence
                });
            }
            return list.ToArray();
        }
        private void changeTo(int recordNo)
        {
            pageIMG.sprite = recordInfos[recordNo - 1].PageSPR;
            sentenceTMP.text = recordInfos[recordNo - 1].Sentence.Replace("\\n", "\n");
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image pageIMG = null;
        [SerializeField] private TextMeshProUGUI sentenceTMP = null;

        // Unity Messages
        private void Awake()
        {
            var spriteAsset = Resources.Load<TMP_SpriteAsset>(EBookSingleBase.PATH_Avatar_TMP_SpritePath);
            sentenceTMP.spriteAsset = spriteAsset;
        }
        private void Start()
        {
        }



        // Inner class
        private class RecordInfo
        {
            public Sprite PageSPR;
            public string Sentence;
        }
    }
}