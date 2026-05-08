using UnityEditor;

namespace DoDoEng
{
    // 빌드시 keystore 암호를 자동으로 입력
    // 하드코딩된 암호는 위험할 수도 있음 => 팀시티에서 세팅 필요
    // https://discussions.unity.com/t/publishing-settings-keystore-password-not-saving/112052

    [InitializeOnLoad]
    public class Android_PreloadSigningAlias
    {
        static Android_PreloadSigningAlias()
        {
            PlayerSettings.Android.keystorePass = "rgaone2017";
            PlayerSettings.Android.keyaliasName = "hidodo";
            PlayerSettings.Android.keyaliasPass = "rgaone2017";
        }
    }
}