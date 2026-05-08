using Cysharp.Threading.Tasks;
using DoDoEng.Launcher;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class EventPopup : PopupBase<SimplePopupResult>
    {
        // Properties
        public bool IsNoWatchAgain => noAgainTGL.isOn;

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(string title, string imageUrl, string moreUrl)
        {
            LOG.Function(this, $"| title={title} | imageUrl={imageUrl} | moreUrl={moreUrl}");

            this.moreUrl = moreUrl;
            moreBT.gameObject.SetActive(!string.IsNullOrEmpty(moreUrl));
            var texture = await API.One.DownloadImage(imageUrl);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)); ;
            noAgainTGL.isOn = false;

            return await showPopup();
        }

        // Fields
        private string moreUrl = "";



        // Unity Inspectors
        [SerializeField] private Image image = null;
        [SerializeField] private Button moreBT = null;
        [SerializeField] private Toggle noAgainTGL = null;

        // Unity Messages
        private void Awake()
        {
            moreBT.onClick.AddListener(() => Application.OpenURL(moreUrl));
        }
    }
}