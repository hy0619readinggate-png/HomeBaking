using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DoDoEng.Common
{
    public class AudioMGR : BYDSingleton<AudioMGR>
    {
        // Definition
        public enum AudioChannel { Narration, BGM, SFX, SFX_LongLasting, SFX_UI, MyVoice, Ambient }

        // Definition : Exposed Parameter
        private const string EP_MASTER_VOL = "MasterVolume";
        private const string EP_BGM_VOL = "BGMVolume";
        private const string EP_SFX_VOL = "SFXVolume";
        private const string EP_MYVOICE_VOL = "MyVoiceVolume";
        private const string EP_AMBIENT_VOL = "AmbientVolume";

        // Properties
        public float BgmVolume
        {
            get
            {
                return bgmVolumeRatio;
            }
            set
            {
                bgmVolumeRatio = value;

                var vol = Mathf.Lerp(
                    bgmVolumeMin,
                    bgmVolumeMax,
                    bgmVolumeRatio);
                mixer.SetFloat(EP_BGM_VOL, vol);
            }
        }
        public float SfxVolume
        {
            get
            {
                return sfxVolumeRatio;
            }
            set
            {
                sfxVolumeRatio = value;

                var vol = Mathf.Lerp(
                    sfxVolumeMin,
                    sfxVolumeMax,
                    sfxVolumeRatio);
                mixer.SetFloat(EP_SFX_VOL, vol);
            }
        }
        public float MyVoiceVolume
        {
            get
            {
                return myVoiceVolumeRatio;
            }
            set
            {
                myVoiceVolumeRatio = value;

                var vol = Mathf.Lerp(
                    myVoiceVolumeMin,
                    myVoiceVolumeMax,
                    myVoiceVolumeRatio);
                mixer.SetFloat(EP_MYVOICE_VOL, vol);
            }
        }
        public float AMBVolume
        {
            get
            {
                return ambVolumeRatio;
            }
            set
            {
                ambVolumeRatio = value;

                var vol = Mathf.Lerp(
                    ambVolumeMin,
                    ambVolumeMax,
                    ambVolumeRatio);
                mixer.SetFloat(EP_AMBIENT_VOL, vol);
            }
        }
        public float Pitch
        {
            get => pitch;
            set
            {
                channels.Where(c => c.Key != AudioChannel.SFX_UI).ForEach(c => c.Value.pitch = value);
                pitch = value;
            }
        }
        public bool IsPlayingEffectLL => channels[AudioChannel.SFX_LongLasting].isPlaying;

        // Methods
        public Coroutine PlayNarrationAndWait(AudioClip clip)
        {
            LOG.Audio($"PlayNarrationAndWait() | {clip.name}", this);

            stopNarrationSequences();
            return playAndWait(clip, AudioChannel.Narration);
        }
        public void PlayNarration(AudioClip clip, float delay = 0)
        {
            LOG.Audio($"PlayNarration() | {clip}", this);

            stopNarrationSequences();
            if (clip != null)
            {
                if (delay == 0)
                    play(clip, AudioChannel.Narration);
                else DOVirtual.DelayedCall(delay, () => play(clip, AudioChannel.Narration));
            }
            else LOG.Warning($"clip is null!", this);
        }
        public void PlayNarration(params AudioClip[] clips)
        {
            stopNarrationSequences();
            crPlayNarrationSequences = StartCoroutine(coPlayNarrationSequences(clips));
        }
        public void StopNarration()
        {
            LOG.Audio($"StopNarration()", this);

            stopNarrationSequences();
            stop(AudioChannel.Narration);
        }
        public Coroutine PlayMyVocieAndWait(AudioClip clip)
        {
            LOG.Audio($"PlayMyVocieAndWait() | {clip.name}", this);

            return playAndWait(clip, AudioChannel.MyVoice);
        }
        public void PlayMyVocie(AudioClip clip, float delay = 0)
        {
            LOG.Audio($"PlayMyVocie() | {clip}", this);

            if (clip != null)
            {
                if (delay == 0)
                    play(clip, AudioChannel.MyVoice);
                else DOVirtual.DelayedCall(delay, () => play(clip, AudioChannel.MyVoice));
            }
            else LOG.Warning($"clip is null!", this);
        }
        public void StopMyVoice()
        {
            LOG.Audio($"StopMyVoice()", this);

            stop(AudioChannel.MyVoice);
        }
        public void PlayEffect(AudioClip clip)
        {
            LOG.Audio($"PlayEffect() | {clip}", this);

            if (clip != null)
                playOneShot(clip, AudioChannel.SFX);
            //else LOG.Warning($"clip is null!", this);
        }
        public void PlayEffect(SfxMoment type)
        {
            LOG.Audio($"PlaySFX() | {type}", this);

            var clip = clipOf(type);
            if (clip != null)
                PlayEffect(clipOf(type));
            //else LOG.Warning($"no clip for {type}", this);
        }
        public void PlayEffectLL(AudioClip clip, bool loop = false)
        {
            LOG.Audio($"PlayEffectLL() | {clip}, {loop}", this);

            if (clip != null)
                play(clip, AudioChannel.SFX_LongLasting, loop);
            //else LOG.Warning($"clip is null!", this);
        }
        public void StopEffectLL(bool fadeOut = false, float duration = 0.5f)
        {
            LOG.Audio($"StopEffectLL()", this);

            if (fadeOut)
                stopWithFade(AudioChannel.SFX_LongLasting, duration);
            else stop(AudioChannel.SFX_LongLasting);
        }
        public void PlayEffectUI(AudioClip clip)
        {
            LOG.Audio($"PlayEffectUI() | {clip}", this);

            if (clip != null)
                playOneShot(clip, AudioChannel.SFX_UI);
            //else LOG.Warning($"clip is null!", this);
        }
        public void PlayEffectUI(SfxMoment type)
        {
            LOG.Audio($"PlayEffectUI() | {type}", this);

            var clip = clipOf(type);
            if (clip != null)
                PlayEffectUI(clipOf(type));
            //else LOG.Warning($"no clip for {type}", this);
        }
        public void PlayBGM(AudioClip clip, float volume = 1)
        {
            LOG.Audio($"PlayBGM() | {clip} {volume}", this);

            if (clip != null)
                play(clip, AudioChannel.BGM, true, volume);
            else LOG.Warning($"clip is null!", this);
        }
        public void StopBGM(bool fadeOut = false, float duration = 0.5f)
        {
            LOG.Audio($"StopBGM()", this);

            if (fadeOut)
                stopWithFade(AudioChannel.BGM, duration);
            else stop(AudioChannel.BGM);
        }
        public void PlayAmbient(AudioClip clip, float volume = 1)
        {
            LOG.Audio($"PlayAmbient() | {clip} {volume}", this);

            if (clip != null)
                play(clip, AudioChannel.Ambient, true, volume);
            else LOG.Warning($"clip is null!", this);
        }
        public void StopAmbient(bool fadeOut = false, float duration = 0.5f)
        {
            LOG.Audio($"StopAmbient()", this);

            if (fadeOut)
                stopWithFade(AudioChannel.Ambient, duration);
            else stop(AudioChannel.Ambient);
        }
        public void StopAll()
        {
            LOG.Audio($"StopAll()", this);

            stopNarrationSequences();
            stop(AudioChannel.Narration);
            stop(AudioChannel.SFX);
            stop(AudioChannel.SFX_LongLasting);
            stop(AudioChannel.BGM);
            stop(AudioChannel.Ambient);
        }

        // Methods
        public void FadeOut(float duration)
        {
            LOG.Audio($"FadeOut() | {duration}", this);

            mixer.DOSetFloat(EP_MASTER_VOL, -80f, duration);
        }
        public void FadeIn(float duration)
        {
            LOG.Audio($"FadeIn() | {duration}", this);

            mixer.DOSetFloat(EP_MASTER_VOL, 0, duration);
        }



        // Fields
        private Dictionary<AudioChannel, AudioSource> channels = new Dictionary<AudioChannel, AudioSource>();
        private float bgmVolumeRatio = 1;
        private float sfxVolumeRatio = 1;
        private float myVoiceVolumeRatio = 1;
        private float ambVolumeRatio = 1;
        private float pitch = 1;

        // Fields
        private Coroutine crPlayNarrationSequences = null;

        // Functions
        private void addChannel(AudioChannel channel, bool ignorePause = false)
        {
            var mixerGroup = mixerConfigs.Single(m => m.channel == channel)?.mixerGroup;
            if (mixerGroup == null)
                LOG.Warning($"Not found mixer group for {channel}", this);

            var source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mixerGroup;
            source.pitch = pitch;
            source.ignoreListenerPause = ignorePause;

            channels[channel] = source;
        }
        private void stopNarrationSequences()
        {
            this.StopCoroutineSafe(ref crPlayNarrationSequences);
        }

        // Functions
        private Coroutine playAndWait(AudioClip clip, AudioChannel channel)
        {
            return StartCoroutine(coPlayAndWait(clip, channel));
        }
        private void play(AudioClip clip, AudioChannel channel, bool loop = false, float volume = 1)
        {
            var source = channels[channel];
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.Play();
        }
        private void playOneShot(AudioClip clip, AudioChannel channel)
        {
            var source = channels[channel];
            source.PlayOneShot(clip);
        }
        private void stop(AudioChannel channel)
        {
            channels[channel]?.Stop();
        }
        private void stopWithFade(AudioChannel channel, float duration)
        {
            var source = channels[channel];
            source?
                .DOFade(0, duration)
                .OnComplete(() => source.Stop());
        }
        private AudioClip clipOf(SfxMoment type)
        {
            return sfxCollection.SfxConfigs.SingleOrDefault(c => c.moment == type)?.clip;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AudioMixer mixer = null;
        [SerializeField] private MixerConfig[] mixerConfigs;
        [SerializeField] private SfxCollectionSO sfxCollection = null;
        [Header("★ Config")]
        [SerializeField] private float bgmVolumeMin = -80f;
        [SerializeField] private float bgmVolumeMax = 0f;
        [SerializeField] private float sfxVolumeMin = -80f;
        [SerializeField] private float sfxVolumeMax = 0f;
        [SerializeField] private float ambVolumeMin = -80f;
        [SerializeField] private float ambVolumeMax = 0f;
        [SerializeField] private float myVoiceVolumeMin = -80f;
        [SerializeField] private float myVoiceVolumeMax = 20f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            addChannel(AudioChannel.BGM);
            addChannel(AudioChannel.Narration);
            addChannel(AudioChannel.SFX);
            addChannel(AudioChannel.SFX_LongLasting);
            addChannel(AudioChannel.SFX_UI, true);
            addChannel(AudioChannel.MyVoice);
            addChannel(AudioChannel.Ambient);
        }
        private void Start()
        {
            BgmVolume = 1;
            SfxVolume = 1;

            // #1798 내 목소리 다시 듣기 볼륨 조절
            MyVoiceVolume = 1;

        }

        // Unity Coroutine
        IEnumerator coPlayAndWait(AudioClip clip, AudioChannel channel)
        {
            stop(channel);
            yield return null;

            var source = channels[channel];
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length);
        }
        IEnumerator coPlayNarrationSequences(AudioClip[] clips)
        {
            stop(AudioChannel.Narration);
            yield return null;

            foreach (var clip in clips)
            {
                var source = channels[AudioChannel.Narration];
                source.clip = clip;
                source.Play();
                yield return new WaitForSeconds(clip.length);
            }
        }

    }

    [Serializable]
    public class MixerConfig
    {
        public AudioMGR.AudioChannel channel;
        public AudioMixerGroup mixerGroup;
    }
}