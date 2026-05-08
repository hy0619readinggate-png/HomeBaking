//using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public enum PetCode
    {
        C401, // °­¾ÆÁö
        C402, // °í¾çÀÌ
        C403, // ÇÜ½ºÅÍ
        C209, // ÇÏ´Ã ´Ù¶÷Áã
        C210, // °ø·æ
        C211, // Æë±Ï
        R404, // È£¶ûÀÌ
        R405, // °í½¿µµÄ¡
        S212, // ³ª¹«´Ãº¸
        S407, // ´Ù¶÷Áã
        S408, // µå·¡°ï
        S406 // °ÅºÏÀÌ
    }
    public class UserDataPet
    {
        // Definitions
        public static int MAX => 30;
        private string[] sampleName = { "¸ù", "¹Ö", "ÄÚ", "Áî", "Ä¡", "¹«", "·ç", "È£", "¹Ì", "Æþ" };

        // Properties
        public int ID = 0;
        public int IdxKind = 0;
        public int Level => (int)Mathf.Floor(Affection) + 1;
        public float Affection
        {
            get => affection;
            set
            {
                affection = value;
            }
        }
        public string Name = "";
        public bool New {  get; set; }

        public UserDataPet() { }
        public UserDataPet(int id, int idxKind, float affection = 0, bool @new = false)
        {
            ID = id;
            IdxKind = idxKind;
            Affection = affection;
            Name = sampleName[Random.Range(0, sampleName.Length)] + sampleName[Random.Range(0, sampleName.Length)];
            New = @new;
        }

        // Fields
        private float affection = 0;
    }
}