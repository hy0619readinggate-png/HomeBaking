using DoDoEng.EBook.Framework;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class DebugTableViewer : MonoBehaviour
    {
        // Methods
        public void ShowTable(TableVersion version, ActivityList curriculum, ActivityData[] tables)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VERSION : {version}");
            sb.AppendLine($"CURRICULUM : {curriculum}");
            foreach (var t in tables)
                sb.AppendLine($"TABLE : {t}");

            tablesTXT.text = sb.ToString();
        }
        public void ShowTable(TableVersion version, GameList curriculum, GameData[] tables)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VERSION : {version}");
            sb.AppendLine($"CURRICULUM : {curriculum}");
            foreach (var t in tables)
                sb.AppendLine($"TABLE : {t}");

            tablesTXT.text = sb.ToString();
        }
        public void ShowTable(TableVersion version, EbookList curriculum, TableVersion pageVersion, LayerData[] pages)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VERSION : {version}");
            sb.AppendLine($"CURRICULUM : {curriculum}");
            sb.AppendLine();
            sb.AppendLine($"VERSION : {pageVersion}");
            foreach (var p in pages)
                sb.AppendLine($"TABLE : {p}");

            tablesTXT.text = sb.ToString();
        }
        public void ShowTable(TableVersion version, MovieList curriculum)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"VERSION : {version}");
            sb.AppendLine($"CURRICULUM : {curriculum}");

            tablesTXT.text = sb.ToString();
        }
        public void ShowTable(EBookQuizData[] quizData)
        {
            var sb = new StringBuilder();
            foreach (var qd in quizData)
                sb.AppendLine($"QUIZc : {qd}");

            tablesTXT.text = sb.ToString();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Text tablesTXT = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}