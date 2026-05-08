using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook
{
    public class BookController : MonoBehaviour
    {
        // Methods : setup
        public void Setup(GameObject bookPB, LayerData[] layers, bool separateByLine)
        {
            LOG.Info($"Setup()", this);

            currentLayerNo = 1;

            Util.RemoveAllChildren(bookRigTR);
            bookTR = Instantiate(bookPB, bookRigTR).transform;

            setupSentenceMarker(layers, separateByLine);

            Util.RemoveAllChildren(scrollContentTR);
            buildScrollPages(bookTR);

            var targetPos = getScrollPosition();
            scroll.content.localPosition = targetPos;
        }

        // Methods 
        public void MoveLayer(Direction direction)
        {
            LOG.Info($"MoveLayer() | {direction}", this);

            turnOffAllHilights();
            if (direction == Direction.Next)
            {
                //bookProFlip.FlipRightPage();
                currentLayerNo++;
                var targetPos = getScrollPosition();
                scroll.content.DOLocalMove(targetPos, scrollDuration);
            }
            else
            {
                //bookProFlip.FlipLeftPage();
                currentLayerNo--;
                var targetPos = getScrollPosition();
                scroll.content.DOLocalMove(targetPos, scrollDuration);
            }
        }
        public void ShowAllSentences(bool visible)
        {
            LOG.Info($"ShowAllSentences() | {visible}", this);

            markers.ForEach(m => m.ShowSentence(visible));
        }
        public void PlayHilight(SentenceData st)
        {
            LOG.Info($"PlayHilight() | {st}", this);

            currentHilightMarker = getMarkerSentence(st.LayerNo, st.SentenceNo);
            crPlayHilight = StartCoroutine(coPlayHilight(currentHilightMarker, st.Sequences));
        }
        public void StopHilight(bool hilightOff = false)
        {
            LOG.Info($"StopHilight()", this);

            this.StopCoroutineSafe(ref crPlayHilight);
            if (hilightOff)
                currentHilightMarker.HilightOff();
        }
        public void HilightOn(RecordData r)
        {
            LOG.Info($"HilightOn()", this);

            currentHilightMarker = getMarkerSentence(r.LayerNo, r.SentenceNo);
            currentHilightMarker.HilightOn(1, r.SequenceNo);
        }
        public void HilightOff()
        {
            LOG.Info($"HilightOff()", this);

            currentHilightMarker.HilightOff();
        }
        public void ShowLoading()
        {
            LOG.Info($"ShowLoading()", this);

            if (loadingGO != null)
                loadingGO.SetActive(true);
        }
        public void HideLoading()
        {
            LOG.Info($"HideLoading()", this);

            if (loadingGO != null)
                loadingGO.SetActive(false);
        }

        // Methods
        public void MoveTo(Transform newParentTR)
        {
            LOG.Function(this);

            transform.SetParent(newParentTR);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }
        public void MoveToOrigin()
        {
            LOG.Function(this);

            transform.SetParent(originParentTR);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }



        // Fields
        private Transform bookTR = null;
        private MarkerSentence[] markers;
        private RectTransform[] pagesRT = null;
        private int currentLayerNo = 1;
        private Transform originParentTR = null;

        // Fields : hilight
        private Coroutine crPlayHilight = null;
        private MarkerSentence currentHilightMarker = null;

        // Functions : setup
        private void setupSentenceMarker(LayerData[] layers, bool separateByLine)
        {
            var spriteAsset = Resources.Load<TMP_SpriteAsset>(EBookSingleBase.PATH_Avatar_TMP_SpritePath);

            var setupSucess = true;
            var markerList = new List<MarkerSentence>();
            foreach (var layer in layers)
            {
                foreach (var sentence in layer.Sentences)
                {
                    try
                    {
                        var stTR = findSentenceTransform(layer.LayerNo, sentence.SentenceNo);
                        var marker = stTR.gameObject.AddComponent<MarkerSentence>();
                        marker.Setup(sentence, layer.LayerNo, separateByLine);
                        marker.SetupTMP(spriteAsset);
                        markerList.Add(marker);
                    }
                    catch (Exception ex)
                    {
                        LOG.Error(ex.Message, this);
                        setupSucess = false;
                    }
                }
            }

            if (!setupSucess)
                throw new Exception("Setup is not completed!!!!!");

            markers = markerList.ToArray();
        }
        private Transform findSentenceTransform(int layerNo, int sentenceNo)
        {
            if (layerNo > bookTR.childCount)
                throw new Exception($"No Layer TRANSFORM is exists Layer:{layerNo}");

            if (sentenceNo > bookTR.GetChild(layerNo - 1).childCount)
                throw new Exception($"No Sentence TRANSFORM is exists Layer:{layerNo} Sentece:{sentenceNo}");

            return bookTR.GetChild(layerNo - 1).GetChild(sentenceNo - 1);
        }
        private Vector2 getScrollPosition()
        {
            var pageRT = pagesRT[currentLayerNo - 1];
            return scroll.GetSnapToPositionToBringChildIntoView(pageRT);
        }
        private void buildScrollPages(Transform bookTR)
        {
            var list = new List<RectTransform>();
            var pages = bookTR.GetChildren().ToArray();
            foreach (var page in pages)
            {
                page.SetParent(scrollContentTR);
                page.gameObject.SetActive(true);

                list.Add(page.GetComponent<RectTransform>());
            }

            pagesRT = list.ToArray();
        }

        // Functions
        private MarkerSentence getMarkerSentence(int layerNo, int sentenceNo)
        {
            return markers.SingleOrDefault(m => m.LayerNo == layerNo && m.SentenceNo == sentenceNo);
        }
        private MarkerSentence[] getAllMarkerSentence(int layerNo)
        {
            return markers.Where(m => m.LayerNo == layerNo).ToArray();
        }
        private void turnOffAllHilights()
        {
            var markers = getAllMarkerSentence(currentLayerNo);
            markers.ForEach(m => m.HilightOff());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform bookRigTR = null;
        [SerializeField] private ScrollRect scroll = null;
        [SerializeField] private Transform scrollContentTR = null;
        [SerializeField] private GameObject loadingGO = null;
        [Header("★ Config")]
        [SerializeField] private float scrollDuration = 0.4f;


        // Unity Messages
        private void Awake()
        {
            if (loadingGO != null)
                loadingGO?.SetActive(false);
        }
        private void Start()
        {
            originParentTR = transform.parent;
        }

        // Unity Coroutine
        IEnumerator coPlayHilight(MarkerSentence marker, SequenceData[] sequence)
        {
            using (LOG.Coroutine($"coPlayHilight()", this))
            {
                var elapsed = 0f;
                var q = new Queue<SequenceData>(sequence);
                while (q.Count > 0)
                {
                    if (elapsed > q.Peek().StartTimeSec)
                    {
                        var seq = q.Dequeue();
                        marker.HilightOn(1, seq.SequenceNo);
                    }

                    yield return null;
                    elapsed += Time.deltaTime;
                }

                // 테스트 패드에서 시간이 지날 수록 느려지는 문제가 있어서 위로 개선함.
                //foreach (var seq in sequence)
                //{
                //    yield return new WaitForSeconds(seq.StartTimeSec - elapsed);

                //    elapsed = seq.StartTimeSec;
                //    marker.HilightOn(1, seq.SequenceNo);
                //    yield return null;
                //}
            }
        }
    }


    public static class ScrollRectExtensions
    {
        public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            return result;
        }
    }
}