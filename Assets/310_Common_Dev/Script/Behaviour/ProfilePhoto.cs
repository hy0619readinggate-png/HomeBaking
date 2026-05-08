using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class ProfilePhoto : MonoBehaviour
    {
        // Properties
        public Texture2D Photo => photo;
        public bool VisibleButton
        {
            get => photoBT != null ? photoBT.gameObject.activeSelf : false;
            set => photoBT?.gameObject.SetActive(value);
        }

        // Events
        public event Action<Texture2D> OnChangePhoto;

        // Methods
        public void Clear()
        {
            SetPhoto(null);
        }
        public async UniTask<bool> ChangePhoto()
        {
            bool result = false;
            bool selecting = true;
            while (true)
            {
                NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
                {
                    if (path != null)
                    {
                        LOG.Function(this, $"| path={path}");
                        var texture = NativeGallery.LoadImageAtPath(path, -1, false);
                        //var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        SetPhoto(texture);

                        OnChangePhoto?.Invoke(texture);
                        result = true;
                    }
                    selecting = false;
                }, "Select an image", "image/*");

                LOG.Function(this, "Permission result: " + permission);

                if (permission == NativeGallery.Permission.Granted)
                {
                    await UniTask.WaitWhile(() => selecting);
                    break;
                }
                else
                {
                    if (!await SystemUI.One.ShowPopupFilePermission())
                        break;
                }
            }

            return result;
        }
        public void SetPhoto(Texture2D photo)
        {
            LOG.Function(this, $"| photo={photo}");
            this.photo = photo;
            sampleGO.SetActive(photo == null);
            photoIMG.texture = photo;
            photoIMG.gameObject.SetActive(photo != null);

            if (photo != null)
            {
                RectTransform rt = photoIMG.rectTransform;
                RectTransform parentRt = rt.parent.GetComponent<RectTransform>();

                float imageRatio = (float)photo.width / photo.height;
                float parentRatio = parentRt.rect.width / parentRt.rect.height;

                if (imageRatio > parentRatio)
                {
                    // 가로를 부모에 맞추고 세로를 조정
                    rt.sizeDelta = new Vector2(parentRt.rect.width, parentRt.rect.width / imageRatio);
                }
                else
                {
                    // 세로를 부모에 맞추고 가로를 조정
                    rt.sizeDelta = new Vector2(parentRt.rect.height * imageRatio, parentRt.rect.height);
                }
            }
        }

        // Fields
        private Texture2D photo;

        // Functions



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject sampleGO = null;
        [SerializeField] private RawImage photoIMG = null;
        [SerializeField] private Button photoBT = null;

        // Unity Messages
        private void Awake()
        {
            photoBT?.onClick.AddListener(() => ChangePhoto().Forget());
        }
        private void Start()
        {
        }
    }
}