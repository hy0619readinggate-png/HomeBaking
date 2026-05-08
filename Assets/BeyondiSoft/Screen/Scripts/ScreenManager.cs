using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// 사용자 화면비 기준해상도 설정 - 기본이 되는 해상도를 넣으면 그 비율에 맞춘다.
public class ScreenManager : MonoBehaviour
{
    [SerializeField] Vector2Int _SizeWide = new Vector2Int(2796, 1290);
    static Vector2Int SizeWide;
    [SerializeField] Vector2Int _SizeNormal = new Vector2Int(1920, 1080);
    static Vector2Int SizeNormal;


    static int dW = Screen.width;
    static int dH = Screen.height;

    void OnPreCull() => GL.Clear(true, true, Color.black);

    static ScreenManager _instance = null;
    void Awake()
    {
        if (_instance != null) return;
        _instance = this;

        SizeWide = _SizeWide;
        SizeNormal = _SizeNormal;
        Debug.LogFormat("DEVICE SCREEN SIZE: {0}, {1}", dW, dH);
        Debug.LogFormat("SCREEN WIDTH LIMIT: {0}", SizeWide);
        Debug.LogFormat("SCREEN NORMAL LIMIT: {0}", SizeNormal);

        SetResolution();
    }

    public static bool isChangeScreen()
    {
        return dW != Screen.width || dH != Screen.height;
    }

    static bool bFoldable = false;
    static bool bCheckFoldable = false;

    public static bool IsFoldableDevice()
    {
        if (!bCheckFoldable)
        {
            bCheckFoldable = true;
            if (Application.isMobilePlatform)
            {
                bFoldable = Regex.IsMatch(SystemInfo.deviceName, @" FOLD", RegexOptions.IgnoreCase);
            }
        }
        return bFoldable;
    }

    // 기기의 실제 화면비
    static float ScreenRate()
    {
        dW = Screen.width;
        dH = Screen.height;
        return (float)dW / dH;
    }

    static float TargetRate(int w, int h)
    {
        return (float)w / h;
    }
    static float TargetRate(Vector2Int v)
    {
        return TargetRate(v.x, v.y);
    }

    static void _setResolutionAndroidFoldable()
    {
        float tRate = TargetRate(SizeWide); // 목표화면비: iPhone 14 Pro MAX - 19.5:9 기준
        float sRate = ScreenRate(); // 실 화면비

        if (tRate < sRate)
        {
            float rW = tRate / sRate;
            Camera.main.rect = new Rect((1f - rW) / 2f, 0f, rW, 1f);
        }
        else
        {
            float rH = sRate / tRate;
            Camera.main.rect = new Rect(0f, (1f - rH) / 2f, 1f, rH);
        }
    }

    static void _setResolution()
    {
        float tRate1 = TargetRate(SizeWide); // 가로가 너무 넓은 화면 대응, iPhone 14 Pro MAX - 19.5:9 기준 - Z Flip
        float tRate2 = TargetRate(SizeNormal); // 세로가 너무 높은 화면 대응, 16:9
        float sRate = ScreenRate(); // 실 화면비

        if (tRate1 < sRate)
        {
            float rW = tRate1 / sRate;
            Camera.main.rect = new Rect((1f - rW) / 2f, 0f, rW, 1f);
        }
        else if (tRate2 > sRate)
        {
            float rH = sRate / tRate2;
            Camera.main.rect = new Rect(0f, (1f - rH) / 2f, 1f, rH);
        }
    }

    public static void SetResolution()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (IsFoldableDevice())
        {
            _setResolutionAndroidFoldable();
        }
        else
        {
            _setResolution();
        }
#else
        _setResolution();
#endif
    }

    public delegate void SetCameraPosDelegate();
    public static SetCameraPosDelegate SetCameraPos;
    public delegate void GetCameraPosDelegate();
    public static GetCameraPosDelegate GetCameraPos;
    public static void UpdateCheck()
    {
        if (isChangeScreen())
        {
            SetResolution();
            SetCameraPos?.Invoke();
        }
        GetCameraPos?.Invoke();
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        UpdateCheck();
#endif
    }
}
