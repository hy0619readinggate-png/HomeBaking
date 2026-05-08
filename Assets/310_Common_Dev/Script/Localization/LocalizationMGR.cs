using beyondi.Util;
using System.Linq;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine;

namespace DoDoEng.Common
{
    public enum LocalizationLocale
    {
        ko,
        en,
        vi
    }
    public sealed class LocalizationMGR
    {
        // Singleton
        private static LocalizationMGR one;
        public static LocalizationMGR One
        {
            get
            {
                if (one == null) one = new LocalizationMGR();
                return one;
            }
        }

        // Definitions
        // Properties
        public LocalizationLocale Locale { get; set; } = LocalizationLocale.ko;
        public CultureInfo Culture
        {
            get
            {
                if (Locale == LocalizationLocale.ko)
                    return new CultureInfo("ko-KR");
                else if (Locale == LocalizationLocale.en)
                    return new CultureInfo("en-US");
                else if (Locale == LocalizationLocale.vi)
                    return new CultureInfo("vi-VN");
                return null;
            }
        }

        // Methods
        public string GetText(string id, params object[] args)
        {
            if (dic.ContainsKey(id))
            {
                string text = "";
                if (Locale == LocalizationLocale.ko)
                    text = dic[id].Korean;
                else if (Locale == LocalizationLocale.en)
                    text = dic[id].English;
                else if (Locale == LocalizationLocale.vi)
                    text = dic[id].Vietnam;

                if (args.Length > 0)
                    text = string.Format(text, args);
                return Regex.Unescape(text.Replace("\n", ""));
            }
            return null;
        }
        public string Select(string ko, string en, string vi)
        {
            if (Locale == LocalizationLocale.ko)
                return Regex.Unescape(ko.Replace("\n", ""));
            else if (Locale == LocalizationLocale.en)
                return Regex.Unescape(en.Replace("\n", ""));
            else if (Locale == LocalizationLocale.vi)
                return Regex.Unescape(vi.Replace("\n", ""));
            return null;
        }
        public void LoadLanguageTable()
        {
            using (LOG.Coroutine($"LoadLanguageTable()", this))
            {
                // ¿¢¼¿ ·Îµå
                //string address = $"C1_Tables/00_Language.xlsx";
                //var hLoad = Addressables.LoadAssetAsync<XlsxAsset>(address);
                //await hLoad.Task;

                //if (hLoad.OperationException != null)
                //    throw hLoad.OperationException;

                //var xlsx = hLoad.Result;

                //Addressables.Release(hLoad);

                var xlsx = Resources.Load<XlsxAsset>("Tables/00_Language");

                // ¿¢¼¿ µ¥ÀÌÅÍ Ã³¸®
                var workbook = new WorkBook(xlsx.Bytes);
                var sheet = workbook["language"];
                sheet.Rows.ForEach(r =>
                {
                    while (r.Cells.Count < 5)
                    {
                        InfiniteLoopDetector.Run(1000);
                        r.Cells.Add(new Cell());
                    }
                });
                var tables = sheet.Convert<LanguageTable>().ToArray();
                if (tables.Length == 0)
                {
                    var msg = $"No table is exist.";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }

                //foreach (var t in tables)
                //    LOG.Addressable($"TABLE : {t}", this);

                dic = new Dictionary<string, LanguageTable>();
                tables.ForEach(t =>
                {
                    dic[t.ID] = t;
                });
            }
        }



        // Fields
        private Dictionary<string, LanguageTable> dic = new Dictionary<string, LanguageTable>();
    }

    // Table
    [Serializable, Table(1, 2, SafeMode = true)]
    public class LanguageTable
    {
        [Column("A")] public string ID;
        [Column("C")] public string Korean;
        [Column("D")] public string English;
        [Column("E")] public string Vietnam;

        public override string ToString()
        {
            return $"{ID} | {Korean} | {English} | {Vietnam}";
        }
    }
}