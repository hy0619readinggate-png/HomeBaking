using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

[Serializable]
class EditorSettings
{
    public string spreadSheetKey;
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}


public class LocalizationEditorWindow : EditorWindow
{
    int idxLangDef;
    SystemLanguage defLanguage;
    int idxLangKey;
    SystemLanguage keyLanguage;
    List<string> listSelectLang = new List<string>();

    Dictionary<string, SystemLanguage> dicSystemLanguage = new Dictionary<string, SystemLanguage>();
    Dictionary<SystemLanguage, bool> dicCheck = new Dictionary<SystemLanguage, bool>();
    List<string> sheetNames = new List<string>();

    void OnEnable()
    {
        Debug.Log("OnEnable");

        // 언어 데이타 생성
        LangList data = GetData();

        List<SystemLanguage> langs = new List<SystemLanguage>();
        if (data != null && data.list.Length > 0) langs.AddRange(data.list);

        foreach (LangCode lc in LocalizationManager.Instance.DicLangNationCode.Values)
        {
            SystemLanguage key = LocalizationManager.Instance.EnableLanguage(lc.lang);
            if (dicCheck.ContainsKey(key)) continue;
            dicCheck[key] = langs.Contains(key);
            dicSystemLanguage[LocalizationManager.Instance.DicLangNationCode[key].ToString()] = key;
        }

        defLanguage = data != null ? data.def : SystemLanguage.Korean;
        idxLangDef = langs.IndexOf(defLanguage);

        keyLanguage = data != null ? data.key : SystemLanguage.Korean;
        idxLangKey = langs.IndexOf(keyLanguage);

        if (data == null) SaveData();

        EditorSettings editorSettings = GetGoogleSheetData();
        if (editorSettings == null) SaveGoogleSheetData();
    }


    string InitDir
    {
        get
        {
            //if (!LocalizationManager.LOCALIZATION_DIR.EndsWith("/")) LocalizationManager.LOCALIZATION_DIR += "/";
            //Directory.CreateDirectory(LocalizationManager.LOCALIZATION_DIR);
            if (!LocalizationManager.LOCALIZATION_SHEETS_DIR.EndsWith("/")) LocalizationManager.LOCALIZATION_SHEETS_DIR += "/";
            Directory.CreateDirectory(LocalizationManager.LOCALIZATION_SHEETS_DIR);
            return LocalizationManager.LOCALIZATION_DIR;
        }
    }
    string FilePath
    {
        get
        {
            return InitDir + LocalizationManager.SETTINGS_FILE;
        }
    }

    void SaveData()
    {
        List<SystemLanguage> list = new List<SystemLanguage>();
        foreach (var m in dicCheck)
        {
            if (m.Value) list.Add(m.Key);
        }
        string json = JsonConvert.SerializeObject(new LangList(list, defLanguage, keyLanguage, sheetNames));
        if (SaveFile(FilePath, json)) Debug.LogFormat("SAVE LOCALIZATION SETTINGS: {0}", json);
    }

    LangList GetData()
    {
        LangList langList = null;
        if (File.Exists(FilePath))
        {
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                string json = streamReader.ReadToEnd();
                langList = JsonConvert.DeserializeObject<LangList>(json);
            }
            Debug.LogFormat("GET LOCALIZATION SETTINGS: {0}", langList);
        }
        return langList;
    }

    string editorPath = "./Assets/BeyondiSoft/Localization/Editor/setting.json";
    EditorSettings GetGoogleSheetData()
    {
        EditorSettings editorSettings = null;
        if (File.Exists(editorPath))
        {
            using (StreamReader streamReader = new StreamReader(editorPath))
            {
                string json = streamReader.ReadToEnd();
                editorSettings = JsonConvert.DeserializeObject<EditorSettings>(json);
                spreadSheetKey = editorSettings.spreadSheetKey;
            }
            Debug.LogFormat("GET EDITOR SETTINGS: {0}", editorSettings);
        }
        return editorSettings;
    }

    void SaveGoogleSheetData()
    {
        EditorSettings editorSettings = new EditorSettings();
        editorSettings.spreadSheetKey = spreadSheetKey;
        string json = JsonConvert.SerializeObject(editorSettings);
        if (SaveFile(editorPath, json)) Debug.LogFormat("SAVE EDITOR SETTINGS: {0}", json);
    }

    Vector2 scrollPosition;
    void OnGUI_A()
    {
        GUILayout.Label("");
        GUILayout.Label("1. 언어선택");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUI.skin.scrollView, GUILayout.Width(600), GUILayout.Height(300));
        Dictionary<SystemLanguage, bool> dic = new Dictionary<SystemLanguage, bool>();
        foreach (LangCode lang in LocalizationManager.Instance.DicLangNationCode.Values)
        {
            SystemLanguage key = LocalizationManager.Instance.EnableLanguage(lang.lang);
            if (dic.ContainsKey(key)) continue; dic[key] = true;
            if (!dicCheck.ContainsKey(key)) continue;

            LangCode lc = LocalizationManager.Instance.DicLangNationCode[key];
            bool b = GUILayout.Toggle(dicCheck[lc.lang], lc.ToString());
            if (dicCheck[lc.lang] != b)
            {
                dicCheck[lc.lang] = b;
                SaveData();
                if (b) // 선택된 경우 이미지용 디렉토리를 만든다.
                {
                    Directory.CreateDirectory(InitDir + lc.code);
                }
            }
        }
        EditorGUILayout.EndScrollView();

        string strLangDef = null;
        if (idxLangDef >= 0 && listSelectLang.Count > idxLangDef)
        {
            strLangDef = listSelectLang[idxLangDef];
        }

        string strLangKey = null;
        if (idxLangKey >= 0 && listSelectLang.Count > idxLangKey)
        {
            strLangKey = listSelectLang[idxLangKey];
        }

        listSelectLang.Clear();
        int n = 0;
        foreach (var m in dicCheck)
        {
            if (!m.Value) continue;
            LangCode lc = LocalizationManager.Instance.DicLangNationCode[m.Key];
            string key = lc.ToString();
            listSelectLang.Add(key);
            if (strLangKey != null && strLangKey == key)
            {
                idxLangKey = n;
            }
            if (strLangDef != null && strLangDef == key)
            {
                idxLangDef = n;
            }
            ++n;
        }

        GUILayout.Label("");
        int b2 = EditorGUILayout.Popup("2. 기본 언어", idxLangDef, listSelectLang.ToArray());
        if (idxLangDef != b2)
        {
            idxLangDef = b2;
            defLanguage = dicSystemLanguage[listSelectLang[idxLangDef]];
            SaveData();
        }

        GUILayout.Label("");
        int b3 = EditorGUILayout.Popup("3. 키 언어", idxLangKey, listSelectLang.ToArray());
        if (idxLangKey != b3)
        {
            idxLangKey = b3;
            keyLanguage = dicSystemLanguage[listSelectLang[idxLangKey]];
            SaveData();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    /*
    // 이하 코드는 에셋스토어의 "Google sheet to Json" 에셋을 가져와 일부 내용을 
    // 프로젝트에 맞춰 수정한 것임.
    // 20230209, 박진형
    */
    const string CLIENT_ID = "";
    const string CLIENT_SECRET = "";
    string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    const string appName = "BeyondiLocalization";
    string spreadSheetKey = "";

    static List<string> allowedDataTypes = new List<string>() { "string", "int", "bool", "float", "string[]", "int[]", "bool[]", "float[]" };
    float progress = 100;
    string progressMessage = "";

    void OnGUI_B()
    {
        progress = 100;
        progressMessage = "";
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

        HorizontalLine(DEFAULT_COLOR, 1, new Vector2(30, 30));
        GUILayout.Label("Google Spread Sheet", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        string newSpreadSheetKey = EditorGUILayout.TextField("spread sheet key", spreadSheetKey, GUILayout.Width(550));
        if (spreadSheetKey != newSpreadSheetKey)
        {
            spreadSheetKey = newSpreadSheetKey;
            SaveGoogleSheetData();
        }

        string url = string.Format("https://docs.google.com/spreadsheets/d/{0}", spreadSheetKey);
        if (GUILayout.Button("열기") && !string.IsNullOrEmpty(spreadSheetKey))
        {
            Application.OpenURL(url);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("");
        if (GUILayout.Button("\n다운로드\n"))
        {
            progress = 0;
            DownloadToJson();
        }
        GUI.backgroundColor = UnityEngine.Color.white;
        if ((progress < 100) && (progress > 0))
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100))
            {
                progress = 100;
                EditorUtility.ClearProgressBar();
            }
        }
        else
        {
            EditorUtility.ClearProgressBar();
        }
    }

    void DownloadToJson()
    {
        if (string.IsNullOrEmpty(spreadSheetKey))
        {
            Debug.LogError("spreadSheetKey can not be null!");
            return;
        }
        Debug.Log("Start downloading from key: " + spreadSheetKey);

        progressMessage = "Authenticating...";
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = GetCredential(),
            ApplicationName = appName
        });

        progress = 5;
        EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);
        progressMessage = "Get list of spreadsheets...";
        EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);

        Spreadsheet spreadSheetData = service.Spreadsheets.Get(spreadSheetKey).Execute();
        IList<Sheet> sheets = spreadSheetData.Sheets;
        if ((sheets == null) || (sheets.Count <= 0))
        {
            Debug.LogError("Not found any data!");
            progress = 100;
            EditorUtility.ClearProgressBar();
            return;
        }

        progress = 15;

        List<string> ranges = new List<string>();
        foreach (Sheet sheet in sheets)
        {
            ranges.Add(sheet.Properties.Title);
        }

        SpreadsheetsResource.ValuesResource.BatchGetRequest request = service.Spreadsheets.Values.BatchGet(spreadSheetKey);
        request.Ranges = ranges;
        BatchGetValuesResponse response = request.Execute();

        sheetNames.Clear();
        foreach (ValueRange valueRange in response.ValueRanges)
        {
            string sheetName = valueRange.Range.Split('!')[0];
            progressMessage = string.Format("Processing {0}...", sheetName);
            EditorUtility.DisplayCancelableProgressBar("Processing", progressMessage, progress / 100);
            if (CreateJsonFile(sheetName, LocalizationManager.LOCALIZATION_SHEETS_DIR, valueRange)) sheetNames.Add(sheetName);
            progress += 85 / response.ValueRanges.Count;
        }
        progress = 100;
        AssetDatabase.Refresh();

        Debug.Log("Download completed.");
        SaveData();
    }

    bool CreateJsonFile(string fileName, string outputDirectory, ValueRange valueRange)
    {
        fileName += ".json";
        try
        {
            IDictionary<int, string> propertyNames = new Dictionary<int, string>(); //Dictionary of (column index, property name of that column)
            IDictionary<int, string> dataTypes = new Dictionary<int, string>(); //Dictionary of (column index, data type of that column)
            IDictionary<int, Dictionary<int, string>> values = new Dictionary<int, Dictionary<int, string>>(); //Dictionary of (row index, dictionary of (column index, value in cell))
            int rowIndex = 0;
            foreach (IList<object> row in valueRange.Values)
            {
                int columnIndex = 0;
                foreach (string cellValue in row)
                {
                    string value = cellValue;
                    if (rowIndex == 0)
                    {
                        propertyNames.Add(columnIndex, value);
                    }
                    else if (rowIndex == 1)
                    {
                        dataTypes.Add(columnIndex, value);
                    }
                    else
                    {
                        if (!values.ContainsKey(rowIndex - 2))
                        {
                            values.Add(rowIndex - 2, new Dictionary<int, string>());
                        }
                        values[rowIndex - 2].Add(columnIndex, value);
                    }

                    columnIndex++;
                }

                rowIndex++;
            }

            List<object> datas = new List<object>();
            foreach (int rowId in values.Keys)
            {
                bool thisRowHasError = false;
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (int columnId in propertyNames.Keys)
                {
                    if (thisRowHasError) break;
                    if ((!dataTypes.ContainsKey(columnId)) || (!allowedDataTypes.Contains(dataTypes[columnId]))) continue;
                    if (!values[rowId].ContainsKey(columnId))
                    {
                        values[rowId].Add(columnId, "");
                    }

                    string strVal = values[rowId][columnId];

                    switch (dataTypes[columnId])
                    {
                        case "string":
                            {
                                data.Add(propertyNames[columnId], strVal);
                                break;
                            }
                        case "int":
                            {
                                int val = 0;
                                if (!string.IsNullOrEmpty(strVal))
                                {
                                    try
                                    {
                                        val = int.Parse(strVal);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                        thisRowHasError = true;
                                        continue;
                                    }
                                }
                                data.Add(propertyNames[columnId], val);
                                break;
                            }
                        case "bool":
                            {
                                bool val = false;
                                if (!string.IsNullOrEmpty(strVal))
                                {
                                    try
                                    {
                                        val = bool.Parse(strVal);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                        continue;
                                    }
                                }
                                data.Add(propertyNames[columnId], val);
                                break;
                            }
                        case "float":
                            {
                                float val = 0f;
                                if (!string.IsNullOrEmpty(strVal))
                                {
                                    try
                                    {
                                        val = float.Parse(strVal);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                        continue;
                                    }
                                }
                                data.Add(propertyNames[columnId], val);
                                break;
                            }
                        case "string[]":
                            {
                                string[] valArr = strVal.Split(new char[] { ',' });
                                data.Add(propertyNames[columnId], valArr);
                                break;
                            }
                        case "int[]":
                            {
                                string[] strValArr = strVal.Split(new char[] { ',' });
                                int[] valArr = new int[strValArr.Length];
                                if (string.IsNullOrEmpty(strVal.Trim()))
                                {
                                    valArr = new int[0];
                                }
                                bool error = false;
                                for (int i = 0; i < valArr.Length; i++)
                                {
                                    int val = 0;
                                    if (!string.IsNullOrEmpty(strValArr[i]))
                                    {
                                        try
                                        {
                                            val = int.Parse(strValArr[i]);
                                        }
                                        catch (System.Exception e)
                                        {
                                            Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                            error = true;
                                            break;
                                        }
                                    }
                                    valArr[i] = val;
                                }
                                if (error) continue;
                                data.Add(propertyNames[columnId], valArr);
                                break;
                            }
                        case "bool[]":
                            {
                                string[] strValArr = strVal.Split(new char[] { ',' });
                                bool[] valArr = new bool[strValArr.Length];
                                if (string.IsNullOrEmpty(strVal.Trim()))
                                {
                                    valArr = new bool[0];
                                }
                                bool error = false;
                                for (int i = 0; i < valArr.Length; i++)
                                {
                                    bool val = false;
                                    if (!string.IsNullOrEmpty(strValArr[i]))
                                    {
                                        try
                                        {
                                            val = bool.Parse(strValArr[i]);
                                        }
                                        catch (System.Exception e)
                                        {
                                            Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                            error = true;
                                            break;
                                        }
                                    }
                                    valArr[i] = val;
                                }
                                if (error) continue;
                                data.Add(propertyNames[columnId], valArr);
                                break;
                            }
                        case "float[]":
                            {
                                string[] strValArr = strVal.Split(new char[] { ',' });
                                float[] valArr = new float[strValArr.Length];
                                if (string.IsNullOrEmpty(strVal.Trim()))
                                {
                                    valArr = new float[0];
                                }
                                bool error = false;
                                for (int i = 0; i < valArr.Length; i++)
                                {
                                    float val = 0f;
                                    if (!string.IsNullOrEmpty(strValArr[i]))
                                    {
                                        try
                                        {
                                            val = float.Parse(strValArr[i]);
                                        }
                                        catch (System.Exception e)
                                        {
                                            Debug.LogError(string.Format("There is exception when parse value of property {0} of {1} class.\nDetail: {2}", propertyNames[columnId], fileName, e.ToString()));
                                            error = true;
                                            break;
                                        }
                                    }
                                    valArr[i] = val;
                                }
                                if (error) continue;
                                data.Add(propertyNames[columnId], valArr);
                                break;
                            }
                        default:
                            break;
                    }
                }

                if (!thisRowHasError)
                {
                    datas.Add(data);
                }
                else
                {
                    Debug.LogError("There's error!");
                    return false;
                }
            }

            SaveFile(fileName, outputDirectory, JsonConvert.SerializeObject(datas));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(string.Format("{0}: {1}", fileName, e));
            return false;
        }
    }

    UserCredential GetCredential()
    {
        MonoScript ms = MonoScript.FromScriptableObject(this);
        string scriptFilePath = AssetDatabase.GetAssetPath(ms);
        FileInfo fi = new FileInfo(scriptFilePath);
        string scriptFolder = fi.Directory.ToString();
        scriptFolder.Replace('\\', '/');
        Debug.Log("Save Credential to: " + scriptFolder);

        UserCredential credential = null;
        ClientSecrets clientSecrets = new ClientSecrets();
        clientSecrets.ClientId = CLIENT_ID;
        clientSecrets.ClientSecret = CLIENT_SECRET;
        try
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, Scopes, "user", CancellationToken.None, new FileDataStore(scriptFolder, true)).Result;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

        return credential;
    }

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;

        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        Debug.LogError("certificate chain is not valid");
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }

    ///////////////////////////////////////////////////////////////////////////
    bool SaveFile(string fileName, string outputDirectory, string text)
    {
        if (!outputDirectory.EndsWith("/")) outputDirectory += "/";
        Directory.CreateDirectory(outputDirectory);
        return SaveFile(outputDirectory + fileName, text);
    }
    bool SaveFile(string path, string text)
    {
        try
        {
            using (StreamWriter strmWriter = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                strmWriter.Write(text);
                strmWriter.Close();
            }
            Debug.LogFormat("SaveFile SUCCESS: {0}", path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogFormat("SaveFile FAIL: {0}, {1}", path, e);
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    [MenuItem("비온디/로컬라이징")]

    static void ShowWindow()
    {
        LocalizationEditorWindow window = EditorWindow.GetWindow(typeof(LocalizationEditorWindow)) as LocalizationEditorWindow;
        window.maxSize = new Vector2(600, 600);
        window.minSize = window.maxSize;
        window.titleContent = new GUIContent("비온디 로컬라이징");
    }

    void OnGUI()
    {
        OnGUI_A();
        OnGUI_B();
    }

    public static readonly UnityEngine.Color DEFAULT_COLOR = new UnityEngine.Color(0f, 0f, 0f, 0.3f);
    public static readonly Vector2 DEFAULT_LINE_MARGIN = new Vector2(2f, 2f);
    public const float DEFAULT_LINE_HEIGHT = 1f;

    public static void HorizontalLine(UnityEngine.Color color, float height, Vector2 margin)
    {
        GUILayout.Space(margin.x);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);
        GUILayout.Space(margin.y);
    }
}
