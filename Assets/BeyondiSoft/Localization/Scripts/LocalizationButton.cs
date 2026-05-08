using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationButton : Localization
{
    [SerializeField]
    string path;

    string localePath;

    Button button;

    void Awake()
    {
        button = gameObject.GetComponent<Button>();
        if (!button)
        {
            Debug.LogErrorFormat("Localization ERROR({0}): Button 컴포넌트가 없습니다.", Hierarcy);
            return;
        }
    }

    Sprite ChangeSprite(Sprite sprite)
    {
        if (!sprite) return null;
        string path = localePath + sprite.name;
        Sprite sp = Resources.Load<Sprite>(path);
        if (!sp)
        {
            Debug.LogErrorFormat("Localization ERROR({0}): Button {1}/{2} 데이타를 찾을 수 없음.", Hierarcy, path, LocalizationManager.Instance.Lang);
            sp = sprite;
        }
        return sp;
    }

    protected override void ChangeLocale()
    {
        if (!button) return;

        localePath = "Localization/" + LocalizationManager.Instance.Lang + "/";
        if (!string.IsNullOrEmpty(path)) localePath += path + "/";

        button.image.sprite = ChangeSprite(button.image.sprite);

        SpriteState spriteState = button.spriteState;
        {
            spriteState.highlightedSprite = ChangeSprite(spriteState.highlightedSprite);
            spriteState.pressedSprite = ChangeSprite(spriteState.pressedSprite);
            spriteState.selectedSprite = ChangeSprite(spriteState.selectedSprite);
            spriteState.disabledSprite = ChangeSprite(spriteState.disabledSprite);
        }
        button.spriteState = spriteState;
    }
}
