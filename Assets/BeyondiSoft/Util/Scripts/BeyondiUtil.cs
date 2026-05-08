using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BeyondiUtil
{
#if UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string _localeRegionCode();
#endif

    static string countryCode;
    public static string CountryCode()
    {
        if (!string.IsNullOrEmpty(countryCode)) return countryCode;

        countryCode = "KR";
#if UNITY_EDITOR
        Debug.Log("EDITOR"); // 윈도우즈에서는 많이 복잡하다 그냥 기본값을 쓰자.
#elif UNITY_ANDROID
        Debug.Log("ANDROID");
        AndroidJavaClass locClass = new AndroidJavaClass("com.beyondisoft.plugin.Locale");
        AndroidJavaObject locInstance = locClass.CallStatic<AndroidJavaObject>("instance");
        if (locInstance != null)
        {
            countryCode = locInstance.Call<string>("CountryCode");
        }
#elif UNITY_IOS
        Debug.Log("iOS");
        countryCode = _localeRegionCode();
#endif

        Debug.LogFormat("System country: {0}", countryCode);
        return countryCode;
    }
}
