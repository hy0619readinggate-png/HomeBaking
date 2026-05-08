using NaughtyAttributes;
using System;
using UnityEngine;

namespace DoDoEng.Game.C2_G01
{
    [CreateAssetMenu(fileName = "C2_G01_ConfigSO", menuName = "DoDoEng/ConfigSO/C2_G01_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public RoundConfig[] RoundConfigs => roundConfigs;
        [AllowNesting][Label("문제 수")] public int ProblemCount = 3;
        



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    [Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("라운드")] public int Round;
        [AllowNesting][Label("음악 목록")] public Music[] Musics = null;
    }

    [Serializable]
    public class Music
    {
        public AudioClip MusicCLIP;
        [AllowNesting][Label("BPM(분당 비트수)")] public float BPM = 100;
        [AllowNesting][Label("스크롤 속도(초당 픽셀수")] public float Speed = 300;
        [AllowNesting][Label("초반 스킵 시간(초)")] public float BeginningSkipTime = 10;
        [AllowNesting][Label("후반 스킵 시간(초)")] public float EndingSkipTime = 1;
        [AllowNesting][Label("피버 시간(초)")] public int FeverTime = 5;
        [AllowNesting][Label("비트 배율")] public int BeatSetp = 1;
        [AllowNesting][Label("문제 대기 비트 수")] public int ReadyBeatCount = 4;
        [AllowNesting][Label("오답 텍스트 비트 수")] public int WrongTextBeatCount = 3;

        public float BeatInterval => 60 / BPM;
        public float BeatDistance => BeatInterval * Speed;
        public float BeatSkipOffset => BeginningSkipTime * Speed;
        public float Duration => MusicCLIP.length;
    }
}