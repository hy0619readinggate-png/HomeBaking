using beyondi.Util;
using DoDoEng.Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class WordBox : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup() | {problem}", this);

            current = 0;
            buildWords(problem);
        }
        public void ShowLocation()
        {
            LOG.Info($"ShowLocation()", this);

            hilight(current);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            var tr = fill(current);
            var ps = Instantiate(answerPS, answerPS.transform.parent);
            ps.gameObject.SetActive(true);
            ps.transform.position = tr.position;

            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            current++;
            if (current < textGroup.Length)
                hilight(current);
        }



        // Fields
        private TextGroup[] textGroup;
        private int current;

        // Functions
        private void buildWords(ProblemData problemData)
        {
            Util.RemoveAllChildren(parentTR);

            var list = new List<TextGroup>();
            var locations = problemData.AlphbetLocations;
            for (var i = 0; i < problemData.Text.Length; i++)
            {
                var ch = problemData.Text[i].ToString();

                if (locations.Contain(i + 1))
                {
                    var tmNormal = Instantiate(templateNormalTXT, parentTR);
                    tmNormal.text = ch;
                    tmNormal.gameObject.SetActive(false);

                    var tmHilight = Instantiate(templateHilightTXT, parentTR);
                    tmHilight.text = ch;
                    tmHilight.gameObject.SetActive(false);

                    var tmGray = Instantiate(templateGrayTXT, parentTR);
                    tmGray.text = ch;
                    tmGray.gameObject.SetActive(true);

                    list.Add(new TextGroup { Normal = tmNormal, Hilight = tmHilight, Gray = tmGray });
                }
                else
                {
                    var tm = Instantiate(templateNormalTXT, parentTR);
                    tm.text = ch;
                    tm.gameObject.SetActive(true);
                }
            }

            textGroup = list.ToArray();
        }
        private void hilight(int idx)
        {
            textGroup[idx].Normal.gameObject.SetActive(false);
            textGroup[idx].Hilight.gameObject.SetActive(true);
            textGroup[idx].Gray.gameObject.SetActive(false);
        }
        private Transform fill(int idx)
        {
            textGroup[idx].Normal.gameObject.SetActive(true);
            textGroup[idx].Hilight.gameObject.SetActive(false);
            textGroup[idx].Gray.gameObject.SetActive(false);

            return textGroup[idx].Hilight.transform;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform parentTR = null;
        [SerializeField] private TextMeshProUGUI templateNormalTXT = null;
        [SerializeField] private TextMeshProUGUI templateHilightTXT = null;
        [SerializeField] private TextMeshProUGUI templateGrayTXT = null;
        [SerializeField] private ParticleSystem answerPS = null;

        // Unity Messages
        private void Awake()
        {
            answerPS.gameObject.SetActive(false);
            templateNormalTXT.gameObject.SetActive(false);
            templateHilightTXT.gameObject.SetActive(false);
            templateGrayTXT.gameObject.SetActive(false);
        }
        private void Start()
        {

        }
    }

    public struct TextGroup
    {
        public TextMeshProUGUI Normal;
        public TextMeshProUGUI Hilight;
        public TextMeshProUGUI Gray;
    }

}