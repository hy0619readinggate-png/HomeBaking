using UnityEngine;

public class KeyboardUIAdjuster : MonoBehaviour
{
    private RectTransform panel;
    private Vector3 originalPosition;
    private float keyboardHeight = 0f;

    void Start()
    {
        panel = GetComponent<RectTransform>();
        originalPosition = panel.position;
    }

    void Update()
    {
        if (TouchScreenKeyboard.visible)
        {
            float newKeyboardHeight = GetKeyboardHeight();
            if (Mathf.Abs(newKeyboardHeight - keyboardHeight) > 1f)
            {
                keyboardHeight = newKeyboardHeight;
                AdjustUIPosition();
            }
        }
        else if (keyboardHeight > 0f)
        {
            keyboardHeight = 0f;
            panel.position = originalPosition; // 키보드 내려가면 UI 원래 위치로 복귀
        }

        keyboardHeight = 0.1f;
        AdjustUIPosition();
    }

    void AdjustUIPosition()
    {
        Vector3 newPosition = originalPosition + new Vector3(0, keyboardHeight, 0);
        panel.position = newPosition;
    }

    float GetKeyboardHeight()
    {
        return Screen.height - TouchScreenKeyboard.area.y;
    }
}
