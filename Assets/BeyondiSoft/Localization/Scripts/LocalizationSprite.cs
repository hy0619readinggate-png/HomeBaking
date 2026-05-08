using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationSprite : Localization
{
    [SerializeField]
    string path;

    string localePath;

    SpriteRenderer spriteRenderer;
    Image image;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer) return;

        image = gameObject.GetComponent<Image>();
        if (image) return;

        Debug.LogErrorFormat("Localization ERROR({0}): spriteRenderer 또는 Image 컴포넌트가 없습니다.", Hierarcy);
        return;
    }

    Sprite ChangeSprite(Sprite sprite)
    {
        if (!sprite) return null;
        string path = localePath + sprite.name;
        Sprite sp = Resources.Load<Sprite>(path);
        if (!sp)
        {
            Debug.LogErrorFormat("Localization ERROR({0}): Sprite {1}/{2} 데이타를 찾을 수 없음.", Hierarcy, path, LocalizationManager.Instance.Lang);
            sp = sprite;
        }
        return sp;
    }

    protected override void ChangeLocale()
    {
        if (!spriteRenderer && !image) return;

        localePath = "Localization/" + LocalizationManager.Instance.Lang + "/";
        if (!string.IsNullOrEmpty(path)) localePath += path + "/";

        if (spriteRenderer)
        {
            spriteRenderer.sprite = ChangeSprite(spriteRenderer.sprite);
        }
        else
        if (image)
        {
            image.sprite = ChangeSprite(image.sprite);
        }
    }
}
