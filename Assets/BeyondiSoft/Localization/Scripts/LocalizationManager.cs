using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

///////////////////////////////////////////////////////////////////////////////
public class LangCode
{
    public SystemLanguage lang;
    public string nation;
    public string code;

    public LangCode(SystemLanguage lang, string nation, string code)
    {
        this.lang = lang;
        this.nation = nation;
        this.code = code;
    }

    public override string ToString()
    {
        return String.Format("[{0}] {1}({2})", code, nation, Enum.GetName(typeof(SystemLanguage), lang));
    }
}

[Serializable]
public class LangList
{
    public SystemLanguage[] list;
    public SystemLanguage def;
    public SystemLanguage key;
    public string[] sheets;
    public LangList(List<SystemLanguage> list, SystemLanguage def, SystemLanguage key, List<string> sheets)
    {
        this.list = list.ToArray();
        this.def = def;
        this.key = key;
        this.sheets = sheets.ToArray();
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

///////////////////////////////////////////////////////////////////////////////
public class LocalizationManager
{
    public static string LOCALIZATION_DIR = "Assets/Resources/Localization/";
    public static string LOCALIZATION_SHEETS_DIR = "Assets/Resources/Localization/Sheets/";
    public static string SETTINGS_FILE = "settings.json";

    static LocalizationManager instance = null;
    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null) instance = new LocalizationManager();
            return instance;
        }
    }

    string _lang = "unknown";
    public string Lang
    {
        get
        {
            return _lang;
        }
    }

    List<SystemLanguage> _LANGUAGES_ = new List<SystemLanguage>();
    public List<SystemLanguage> LANGUAGES
    {
        get
        {
            return _LANGUAGES_.ToList();
        }
    }
    SystemLanguage _DEFAULT_LANGUAGE_;
    public SystemLanguage DEFAULT_LANGUAGE
    {
        get
        {
            return _DEFAULT_LANGUAGE_;
        }
    }
    SystemLanguage _KEY_LANGUAGE_;
    public SystemLanguage KEY_LANGUAGE
    {
        get
        {
            return _KEY_LANGUAGE_;
        }
    }
    public SystemLanguage _CURRENT_LANGUAGE_;
    public SystemLanguage CURRENT_LANGUAGE
    {
        get
        {
            return _CURRENT_LANGUAGE_;
        }
    }
    string[] _SHEETS_;
    public string[] SHEETS
    {
        get
        {
            return _SHEETS_.ToArray();
        }
    }

    LocalizationManager()
    {
        SystemLanguage systemLanguage = EnableLanguage(Application.systemLanguage);
        Debug.LogFormat("System language: {0}", systemLanguage);

        LangList langs = GetData();
        if (langs != null)
        {
            _DEFAULT_LANGUAGE_ = langs.def;
            if (!_LANGUAGES_.Contains(_DEFAULT_LANGUAGE_)) _LANGUAGES_.Add(_DEFAULT_LANGUAGE_);
            _KEY_LANGUAGE_ = langs.key;
            if (!_LANGUAGES_.Contains(_KEY_LANGUAGE_)) _LANGUAGES_.Add(_KEY_LANGUAGE_);
            foreach (SystemLanguage m in langs.list)
            {
                if (!_LANGUAGES_.Contains(m)) _LANGUAGES_.Add(m);
            }
            _SHEETS_ = langs.sheets;
        }
        else
        {
            _KEY_LANGUAGE_ = _DEFAULT_LANGUAGE_ = systemLanguage;
            _LANGUAGES_.Add(_DEFAULT_LANGUAGE_);
        }
        LoadTextData();

        string language = Code(_DEFAULT_LANGUAGE_);
        if (PlayerPrefs.HasKey("beyondi_localization"))
        {
            language = PlayerPrefs.GetString("beyondi_localization");
        }
        else
        {
            foreach (SystemLanguage lang in _LANGUAGES_)
            {
                if (lang == systemLanguage)
                {
                    language = Code(systemLanguage);
                    break;
                }
            }
        }
        SetLanguage(language);


        ///////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            string cnavasName = @"Localizer(EDITOR)";
            GameObject oldCanvas = GameObject.Find(cnavasName);
            if (oldCanvas != null) GameObject.Destroy(oldCanvas);

            GameObject canvasLocalizer = new GameObject(cnavasName);
            Canvas canvas = canvasLocalizer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvasLocalizer.AddComponent<CanvasScaler>();
            canvasLocalizer.AddComponent<GraphicRaycaster>();

            GameObject btnObj = new GameObject("BUTTON");
            btnObj.transform.parent = canvasLocalizer.transform;
            Image image = btnObj.AddComponent<Image>();
            image.color = Color.grey;
            Button btn = btnObj.AddComponent<Button>();
            GameObject btnText = new GameObject("Text");
            btnText.transform.parent = btnObj.transform;
            Text text = btnText.AddComponent<Text>();
            
            Font font;
            try {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            text.font = font;

            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 20;
            RectTransform textRect = btnText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 50);
            btnRect.pivot = new Vector2(0, 1);
            btnRect.anchoredPosition = new Vector2(0, 0);
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(0, 1);
            btn.onClick.AddListener(Next);

            ChangeLocale += () =>
            {
                text.text = Lang;
            };
            text.text = Lang;
        }
#endif

        ///////////////////////////////////////////////////////////////////////
        {
            _language = LANGUAGES;
            while (_language.Contains(_CURRENT_LANGUAGE_) && _language[0] != _CURRENT_LANGUAGE_) NextLang();
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    static List<SystemLanguage> _language; // = new List<SystemLanguage>();
    SystemLanguage NextLang()
    {
        SystemLanguage lang = _language[0];
        _language.RemoveAt(0);
        _language.Add(lang);
        return _language[0];
    }

    public void Next()
    {
        SetLanguage(NextLang());
    }
    ///////////////////////////////////////////////////////////////////////////

    public Dictionary<SystemLanguage, LangCode> DicLangNationCode = new Dictionary<SystemLanguage, LangCode>()
    {
        {SystemLanguage.Afrikaans, new LangCode(SystemLanguage.Afrikaans, "아프리칸스어", "af")},
        {SystemLanguage.Arabic, new LangCode(SystemLanguage.Arabic, "아랍어", "ar")},
        {SystemLanguage.Basque, new LangCode(SystemLanguage.Basque, "바스크어", "eu")},
        {SystemLanguage.Belarusian, new LangCode(SystemLanguage.Belarusian, "벨라루스어", "be")},
        {SystemLanguage.Bulgarian,new LangCode(SystemLanguage.Bulgarian, "불가리아어", "bg")},
        {SystemLanguage.Catalan, new LangCode(SystemLanguage.Catalan, "카탈로니아어", "ca")},
        {SystemLanguage.Chinese, new LangCode(SystemLanguage.Chinese, "중국어", "zh")},
        {SystemLanguage.ChineseSimplified, new LangCode(SystemLanguage.Chinese, "중국어", "zh")},
        {SystemLanguage.ChineseTraditional, new LangCode(SystemLanguage.Chinese, "중국어", "zh")},
        {SystemLanguage.Czech, new LangCode(SystemLanguage.Czech, "체코어", "cs")},
        {SystemLanguage.Danish, new LangCode(SystemLanguage.Danish, "덴마크어", "da")},
        {SystemLanguage.Dutch, new LangCode(SystemLanguage.Dutch, "네덜란드어", "nl")},

        {SystemLanguage.English, new LangCode(SystemLanguage.English, "영어", "en")},
        {SystemLanguage.Estonian, new LangCode(SystemLanguage.Estonian, "에스토니아어", "et")},
        {SystemLanguage.Faroese, new LangCode(SystemLanguage.Faroese, "페로어", "fo")},
        {SystemLanguage.Finnish, new LangCode(SystemLanguage.Finnish, "핀란드어", "fi")},
        {SystemLanguage.French, new LangCode(SystemLanguage.French, "프랑스어", "fr")},
        {SystemLanguage.German, new LangCode(SystemLanguage.German, "독일어", "de")},
        {SystemLanguage.Greek, new LangCode(SystemLanguage.Greek, "그리스어", "el")},
        {SystemLanguage.Hebrew, new LangCode(SystemLanguage.Hebrew, "히브리어", "he")},
        {SystemLanguage.Hungarian, new LangCode(SystemLanguage.Hungarian, "헝가리어", "hu")},
        {SystemLanguage.Indonesian, new LangCode(SystemLanguage.Indonesian, "인도네시안어", "id")},

        {SystemLanguage.Italian, new LangCode(SystemLanguage.Italian, "이탈리아어", "it")},
        {SystemLanguage.Japanese, new LangCode(SystemLanguage.Japanese, "일본어", "ja")},
        {SystemLanguage.Korean, new LangCode(SystemLanguage.Korean, "한국어", "ko")},
        {SystemLanguage.Latvian, new LangCode(SystemLanguage.Latvian, "라트비아어", "lv")},
        {SystemLanguage.Lithuanian, new LangCode(SystemLanguage.Lithuanian, "리투아니아어", "lt")},
        {SystemLanguage.Norwegian, new LangCode(SystemLanguage.Norwegian, "노르웨이어", "no")},
        {SystemLanguage.Polish, new LangCode(SystemLanguage.Polish, "폴란드어", "pl")},
        {SystemLanguage.Portuguese, new LangCode(SystemLanguage.Portuguese, "포르투갈어", "pt")},
        {SystemLanguage.Romanian, new LangCode(SystemLanguage.Romanian, "루마니아어", "ro")},
        {SystemLanguage.Russian, new LangCode(SystemLanguage.Russian, "러시아어", "ru")},

        {SystemLanguage.Slovak, new LangCode(SystemLanguage.Slovak, "슬로바키아어", "sk")},
        {SystemLanguage.Slovenian, new LangCode(SystemLanguage.Slovenian, "슬로베니아어", "sl")},
        {SystemLanguage.Spanish, new LangCode(SystemLanguage.Spanish, "스페인어", "es")},
        {SystemLanguage.Swedish, new LangCode(SystemLanguage.Swedish, "스웨덴어", "sv")},
        {SystemLanguage.Thai, new LangCode(SystemLanguage.Thai, "태국어", "th")},
        {SystemLanguage.Turkish, new LangCode(SystemLanguage.Turkish, "터키어", "tr")},
        {SystemLanguage.Ukrainian, new LangCode(SystemLanguage.Ukrainian, "우크라이나어", "uk")},
        {SystemLanguage.Vietnamese, new LangCode(SystemLanguage.Vietnamese, "베트남어", "vi")}
    };

    public SystemLanguage EnableLanguage(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Unknown: // 알 수 없는 - 영어로 통합
            case SystemLanguage.Icelandic: // 아이슬란드어 - 영어로 통합
            case SystemLanguage.SerboCroatian: // 여기도 - 영어로 통합
                return SystemLanguage.English; // 알 수 없거나 잡다한 것들은 영어로 통일
            case SystemLanguage.ChineseSimplified: // 중국어 간체
            case SystemLanguage.ChineseTraditional: // 중국어 번체
                return SystemLanguage.Chinese; // 중국어는 중국어로
        }
        return lang;
    }

    public LangList GetData()
    {
        LangList langList = null;

        /*
        if (File.Exists(FilePath))
        {
            // 이 방식은 모바일기기에서는 사용불가
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                string json = streamReader.ReadToEnd();
                langList = JsonConvert.DeserializeObject<LangList>(json);
            }
            Debug.LogFormat("GET LOCALIZATION SETTINGS: {0}", langList);
        }
        */

        TextAsset textData = Resources.Load<TextAsset>(String.Format("Localization/{0}", Path.GetFileNameWithoutExtension(SETTINGS_FILE)));
        if (textData)
        {
            string json = textData.ToString();
            langList = JsonConvert.DeserializeObject<LangList>(json);
            Debug.LogFormat("GET LOCALIZATION SETTINGS: {0}", langList);
        }

        return langList;
    }

    string Code(SystemLanguage lang)
    {
        SystemLanguage key = EnableLanguage(lang);
        return DicLangNationCode[key].code;
    }
    SystemLanguage DeCode(string code)
    {
        foreach (SystemLanguage key in DicLangNationCode.Keys)
        {
            if (DicLangNationCode[EnableLanguage(key)].code == code) return key;
        }
        return _DEFAULT_LANGUAGE_;
    }


    public delegate void _delegateChangeLocale();
    public _delegateChangeLocale ChangeLocale;

    void SetLanguage(string language)
    {
        if (String.IsNullOrEmpty(language)) return;
        if (language == _lang) return;
        Debug.LogFormat("CHANGE LOCALE: {0} -> {1}", _lang, language);

        _lang = language;
        PlayerPrefs.SetString("beyondi_localization", _lang);

        _CURRENT_LANGUAGE_ = DeCode(language);
        ChangeLocale?.Invoke();
        while (_language != null && _language.Contains(_CURRENT_LANGUAGE_) && _language[0] != _CURRENT_LANGUAGE_) NextLang();
    }

    public void SetLanguage(SystemLanguage lang)
    {
        Debug.LogFormat("SetLanguage({0})", lang);
        SetLanguage(Code(lang));
    }

    ///////////////////////////////////////////////////////////////////////////
    [Serializable]
    class lang
    {
        public string af; //Afrikaans
        public string ar; //Arabic
        public string eu; //Basque
        public string be; //Belarusian
        public string bg; //Bulgarian
        public string ca; //Catalan
        public string zh; //Chinese, ChineseSimplified, ChineseTraditional
        public string cs; //Czech
        public string da; //Danish
        public string nl; //Dutch

        public string en; //English, Unknown
        public string et; //Estonian
        public string fo; //Faroese
        public string fi; //Finnish
        public string fr; //French
        public string de; //German
        public string el; //Greek
        public string he; //Hebrew
        public string hu; //Hungarian
        public string id; //Indonesian

        public string it; //Italian
        public string ja; //Japanese
        public string ko; //Korean
        public string lv; //Latvian
        public string lt; //Lithuanian
        public string no; //Norwegian
        public string pl; //Polish
        public string pt; //Portuguese
        public string ro; //Romanian
        public string ru; //Russian

        public string sk; //Slovak
        public string sl; //Slovenian
        public string es; //Spanish
        public string sv; //Swedish
        public string th; //Thai
        public string tr; //Turkish
        public string uk; //Ukrainian
        public string vi; //Vietnamese
    }

    [Serializable]
    class JsonObj
    {
        public lang[] data;
    }

    string LangKey(lang m, SystemLanguage key)
    {
        //Debug.Log(key);
        switch (key)
        {
            case SystemLanguage.Afrikaans: return m.af;
            case SystemLanguage.Arabic: return m.ar;
            case SystemLanguage.Basque: return m.eu;
            case SystemLanguage.Belarusian: return m.be;
            case SystemLanguage.Bulgarian: return m.bg;
            case SystemLanguage.Catalan: return m.ca;
            case SystemLanguage.Chinese: return m.zh;
            case SystemLanguage.Czech: return m.cs;
            case SystemLanguage.Danish: return m.da;
            case SystemLanguage.Dutch: return m.nl;

            case SystemLanguage.English: return m.en;
            case SystemLanguage.Estonian: return m.et;
            case SystemLanguage.Faroese: return m.fo;
            case SystemLanguage.Finnish: return m.fi;
            case SystemLanguage.French: return m.fr;
            case SystemLanguage.German: return m.de;
            case SystemLanguage.Greek: return m.el;
            case SystemLanguage.Hebrew: return m.he;
            case SystemLanguage.Hungarian: return m.hu;
            case SystemLanguage.Indonesian: return m.id;

            case SystemLanguage.Italian: return m.it;
            case SystemLanguage.Japanese: return m.ja;
            case SystemLanguage.Korean: return m.ko;
            case SystemLanguage.Latvian: return m.lv;
            case SystemLanguage.Lithuanian: return m.lt;
            case SystemLanguage.Norwegian: return m.no;
            case SystemLanguage.Polish: return m.pl;
            case SystemLanguage.Portuguese: return m.pt;
            case SystemLanguage.Romanian: return m.ro;
            case SystemLanguage.Russian: return m.ru;

            case SystemLanguage.Slovak: return m.sk;
            case SystemLanguage.Slovenian: return m.sl;
            case SystemLanguage.Spanish: return m.es;
            case SystemLanguage.Swedish: return m.sv;
            case SystemLanguage.Thai: return m.th;
            case SystemLanguage.Turkish: return m.tr;
            case SystemLanguage.Ukrainian: return m.uk;
            case SystemLanguage.Vietnamese: return m.vi;

            default: return m.ko;
        }
    }

    Dictionary<string, Dictionary<string, lang>> _DIC_ = new Dictionary<string, Dictionary<string, lang>>();
    void LoadTextData()
    {
        foreach (string sheet in SHEETS)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(String.Format("{0}{1}", LOCALIZATION_SHEETS_DIR.Split("Resources/")[1], sheet));
            if (textAsset == null) return;
            Debug.LogFormat("Google sheet({0}): {1}", sheet, textAsset.ToString());
            Dictionary<string, lang> dic = _DIC_[sheet] = new Dictionary<string, lang>();
            string json = String.Format("{{\"data\":{0}}}", textAsset.ToString());
            lang[] data = JsonConvert.DeserializeObject<JsonObj>(json).data;
            foreach (lang m in data)
            {
                string key = LangKey(m, _KEY_LANGUAGE_);
                if (key == null) continue;
                dic[key] = m;
            }
        }
        Debug.Log(JsonConvert.SerializeObject(_DIC_));
    }

    public string GetText(string key, string sheet)
    {
        if (!_DIC_.ContainsKey(sheet))
        {
            Debug.LogWarningFormat("Localization ERROR: 텍스트 시트({0})가 존재하지 않습니다.", sheet);
            return key;
        }

        Dictionary<string, lang> dic = _DIC_[sheet];
        if (!dic.ContainsKey(key))
        {
            Debug.LogWarningFormat("Localization ERROR: 텍스트 키워드 ({0})가 존재하지 않습니다.", key);
            return key;
        }

        lang m = dic[key];
        switch (Lang)
        {
            case "ko": //Korean
                return m.ko;
            case "en": //English, Unknown
                return m.en;
            case "af": //Afrikaans
                return m.af;
            case "ar": //Arabic
                return m.ar;
            case "eu": //Basque
                return m.eu;
            case "be": //Belarusian
                return m.be;
            case "bg": //Bulgarian
                return m.bg;
            case "ca": //Catalan
                return m.ca;
            case "zh": //Chinese, ChineseSimplified, ChineseTraditional
                return m.zh;
            case "cs": //Czech
                return m.cs;
            case "da": //Danish
                return m.da;
            case "nl": //Dutch
                return m.nl;
            case "et": //Estonian
                return m.et;
            case "fo": //Faroese
                return m.fo;
            case "fi": //Finnish
                return m.fi;
            case "fr": //French
                return m.fr;
            case "de": //German
                return m.de;
            case "el": //Greek
                return m.el;
            case "he": //Hebrew
                return m.he;
            case "hu": //Hungarian
                return m.hu;
            case "id": //Indonesian
                return m.id;
            case "it": //Italian
                return m.it;
            case "ja": //Japanese
                return m.ja;
            case "lv": //Latvian
                return m.lv;
            case "lt": //Lithuanian
                return m.lt;
            case "no": //Norwegian
                return m.no;
            case "pl": //Polish
                return m.pl;
            case "pt": //Portuguese
                return m.pt;
            case "ro": //Romanian
                return m.ro;
            case "ru": //Russian
                return m.ru;
            case "sk": //Slovak
                return m.sk;
            case "sl": //Slovenian
                return m.sl;
            case "es": //Spanish
                return m.es;
            case "sv": //Swedish
                return m.sv;
            case "th": //Thai
                return m.th;
            case "tr": //Turkish
                return m.tr;
            case "uk": //Ukrainian
                return m.uk;
            case "vi": //Vietnamese
                return m.vi;
        }

        return LangKey(m, _DEFAULT_LANGUAGE_);
    }
}
