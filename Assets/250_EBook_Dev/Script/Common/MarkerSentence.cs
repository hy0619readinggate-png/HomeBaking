using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using NaughtyAttributes;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace DoDoEng.EBook
{
    public class MarkerSentence : MonoBehaviour
    {
        // Properties
        public int LayerNo => layerNo;
        public int SentenceNo => sentenceNo;

        // Methods
        public void Setup(SentenceData stData, int layerNo, bool separateByLine)
        {
            LOG.Info($"Setup() | {layerNo} | {stData}, {separateByLine}", this);

            this.layerNo = layerNo;
            this.separateByLine = separateByLine;
            sentenceNo = stData.SentenceNo;
            sentence = stData.Sentence;

            if (tmpNormal == null)
                throw new Exception($"Normal TextMeshPro is not exists for Layer:{layerNo} Sentece:{sentenceNo}");

            if (tmpHilight == null)
                throw new Exception($"Hilight TextMeshPro is not exists for Layer:{layerNo} Sentece:{sentenceNo}");

            tmpNormal.text = makeNormalDisplayString();
            tmpHilight.text = "";

            tmpNormal.gameObject.SetActive(true);
            tmpHilight.gameObject.SetActive(true);
        }
        public void SetupTMP(TMP_SpriteAsset spriteAsset)
        {
            tmpNormal.spriteAsset = spriteAsset;
            tmpHilight.spriteAsset = spriteAsset;
        }
        public void ShowSentence(bool visible)
        {
            LOG.Info($"ShowSentence() | {visible}", this);

            tmpNormal.gameObject.SetActive(visible);
            tmpHilight.gameObject.SetActive(visible);
        }
        public void HilightOn(int startSeqNo, int finishSeqNo)
        {
            LOG.Info($"HilightOn() | {startSeqNo} to {finishSeqNo}", this);

            if (separateByLine)
                tmpHilight.text = makeHilightDisplayStringByLine(startSeqNo, finishSeqNo - 1);
            else tmpHilight.text = makeHilightDisplayString(startSeqNo, finishSeqNo - 1);
        }
        public void HilightOff()
        {
            LOG.Info($"HilightOff()", this);

            tmpHilight.text = "";
        }



        // Fields : caching
        private TextMeshProUGUI tmpNormal_ = null;
        private TextMeshProUGUI tmpNormal => tmpNormal_ ??= GetComponentsInChildren<TextMeshProUGUI>(true)[0];
        private TextMeshProUGUI tmpHilight_ = null;
        private TextMeshProUGUI tmpHilight => tmpHilight_ ??= GetComponentsInChildren<TextMeshProUGUI>(true)[1];

        // Fields
        private const string patternTag = @"<color=#.*?>(.*?)</color>";
        private const string patternNL = @"[[](?<t1>(<.+>)?)\n(?<t2>(<.+>)?)[]]";
        private string sentence = null;

        // Functions
        private string makeNormalDisplayString()
        {
            // MyVoice 문장 구분을 위한 특수 기호([\n])를 무시함
            var s1 = sentence.Replace("[\\n]", "");

            // 표시되는 세미콜론은 대괄호 사이([;])에 넣어져 있음. Split 되지 않게 다른 문자([|])로 변경
            var s2 = s1.Replace("[;]", "[|]");

            var sequences = s2.Split(';');

            // 분리기호인 세미콜론으로 분리후 표시되는 세미콜론 특수기호를 세미콜론으로 다시 변경함
            sequences = sequences.Select(s => s.Replace("[|]", ";")).ToArray();

            return string.Join(" ", sequences).Replace("\\n", "\n").Replace("\\r", "\n");
        }
        private string makeHilightDisplayString(int startHilightSeqIdx = 0, int finishHilightSeqIdx = 0)
        {
            // 전처리
            var s1 = sentence.Replace("[;]", "[|]"); // 표시되는 세미콜론은 대괄호 사이([;])에 넣어져 있음. Split 되지 않게 다른 문자([|])로 변경
            var s2 = Regex.Replace(s1, patternTag, @"$1"); // <color> 태그는 삭제되어야 TMP의 vertex color 적용을 할 수 있음.
            var s3 = s2.Replace("[\\n]", ""); // 라인 단위 처리를 위한 기호를 무시

            // Sequence로 분리
            var sequences = s3.Split(';');
            sequences = sequences.Select(s => s.Replace("[|]", ";")).ToArray(); // 분리기호인 세미콜론으로 분리후 표시되는 세미콜론 특수기호를 세미콜론으로 다시 변경함

            // 앞부분 안보이게
            if (startHilightSeqIdx > 1)
            {
                sequences[0] = $"<color=#FFFFFF00>{sequences[0]}";
                sequences[startHilightSeqIdx - 2] = $"{sequences[startHilightSeqIdx - 2]}</color>";
            }
            // 뒷부분 안보이게
            if (finishHilightSeqIdx < sequences.Length - 1)
            {
                var s = finishHilightSeqIdx + 1;
                var f = sequences.Length - 1;
                
                sequences[s] = $"<color=#FFFFFF00>{sequences[s]}";
                sequences[f] = $"{sequences[f]}</color>";
            }

            return string.Join(" ", sequences).Replace("\\n", "\n").Replace("\\r", "\n");
        }
        private string makeHilightDisplayStringByLine(int startHilightSeqIdx = 0, int finishHilightSeqIdx = 0)
        {
            // 전처리
            var s1 = sentence.Replace("[;]", "[|]"); // 표시되는 세미콜론은 대괄호 사이([;])에 넣어져 있음. Split 되지 않게 다른 문자([|])로 변경
            var s2 = Regex.Replace(s1, patternTag, @"${text}"); // <color> 태그는 삭제되어야 TMP의 vertex color 적용을 할 수 있음.
            var s3 = s2.Replace(";", " "); // 라인단위로 처리하므로, 세미콜론은 제외

            // 라인단위로 Sequence 분리
            var sequences = s3.Split("\\n");
            sequences = sequences.Select(s => s.Replace("[|]", ";")).ToArray(); // 분리기호인 세미콜론으로 분리후 표시되는 세미콜론 특수기호를 세미콜론으로 다시 변경함

            // 앞부분 안보이게
            if (startHilightSeqIdx > 1)
            {
                sequences[0] = $"<color=#FFFFFF00>{sequences[0]}";
                sequences[startHilightSeqIdx - 2] = $"{sequences[startHilightSeqIdx - 2]}</color>";
            }
            // 뒷부분 안보이게
            if (finishHilightSeqIdx < sequences.Length - 1)
            {
                var s = finishHilightSeqIdx + 1;
                var f = sequences.Length - 1;

                sequences[s] = $"<color=#FFFFFF00>{sequences[s]}";
                sequences[f] = $"{sequences[f]}</color>";
            }

            var s4 = string.Join("\n", sequences).Replace("\\r", "\n");
            var s5 = Regex.Replace(s4, patternNL, @"${t1}${t2}");

            return s5;
        }



        // Unity Inspectors 
        [HorizontalLine(color: EColor.Red)]
        [SerializeField][ReadOnly] private int layerNo;
        [SerializeField][ReadOnly] private bool separateByLine;
        [SerializeField][ReadOnly] private int sentenceNo;
        [HorizontalLine(color: EColor.Red)]
        [SerializeField][OnValueChanged("devHilightOn")] private int devHilightSeqNo = 1;

        // Unity Inspectors : button
        [Button("(DEV) HilightOn", EButtonEnableMode.Playmode)]
        private void devHilightOn()
        {
            HilightOn(1, devHilightSeqNo);
        }
        [Button("(DEV) HilightOff", EButtonEnableMode.Playmode)]
        private void devHilightOff()
        {
            HilightOff();
        }

        // Unity Messages
        private void OnDrawGizmos()
        {

#if UNITY_EDITOR
            var rt = GetComponent<RectTransform>();

            var v = new Vector3[4];
            rt.GetWorldCorners(v);

            drawString($"{LayerNo}-{SentenceNo}", v[1], Color.yellow, new Vector2(1, 0), 13f);
#endif
        }



        // Static functions
        static private void drawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
        {
#if UNITY_EDITOR
            var view = SceneView.currentDrawingSceneView;
            if (!view)
                return;

            var screenPosition = view.camera.WorldToScreenPoint(worldPosition);
            if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight ||
                screenPosition.x < 0 || screenPosition.x > view.camera.pixelWidth ||
                screenPosition.z < 0)
                return;

            var pixelRatio =
                HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x -
                HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;

            Handles.BeginGUI();
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = (int)textSize,
                normal = new GUIStyleState() { textColor = textColor },
                alignment = TextAnchor.MiddleCenter
            };
            var size = style.CalcSize(new GUIContent(text)) * pixelRatio;
            var alignedPosition =
                ((Vector2)screenPosition +
                size * ((anchor + Vector2.left + Vector2.up))) * (Vector2.right + Vector2.down) +
                Vector2.up * view.camera.pixelHeight;
            GUI.Box(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
            //GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
            Handles.EndGUI();
#endif
        }
    }
}