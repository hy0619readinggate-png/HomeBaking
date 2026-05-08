/*
// 플랫폼 코드 확인시 사용
#define UNITY_IOS
//*/

using System.Runtime.InteropServices;

public class MyNativeBindings
{
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern string GetSettingsURL();

    [DllImport("__Internal")]
    public static extern void OpenSettings();
#endif
}
