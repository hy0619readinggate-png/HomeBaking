using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;

namespace beyondi.Util
{
    public class UtilMedia
    {
        public static IEnumerator PlayTimeline(PlayableDirector director)
        {
            director.Play();
            while (director.state != PlayState.Paused)
                yield return null;
        }
        public static IEnumerator PlayAudioClip(AudioSource src, AudioClip clip)
        {
            if (clip == null)
                yield break;

            src.clip = clip;
            src.Play();
            yield return new WaitForSeconds(clip.length);
        }


        public static string MakeClipToWav(AudioClip clip, string filePath, bool bNormalize = false, float mormalVolume = 0)
        {
            float[] floats = new float[clip.samples * clip.channels];
            clip.GetData(floats, 0);

            if (bNormalize)
            {
                float maxVolume = GetMaxVoume(clip);
                float meanVolume = Mathf.Approximately(maxVolume, 0) ? 0 : mormalVolume / maxVolume;
                Debug.Log($"오디오 MAX VOLUME: {mormalVolume}dB / {maxVolume}dB = {meanVolume}");
                for (int i = 0; i < floats.Length; ++i)
                {
                    floats[i] *= meanVolume;
                }
            }

            byte[] bytes = new byte[floats.Length * 2];
            for (int ii = 0; ii < floats.Length; ii++)
            {
                short uint16 = (short)(floats[ii] * short.MaxValue);
                byte[] vs = BitConverter.GetBytes(uint16);
                bytes[ii * 2] = vs[0];
                bytes[ii * 2 + 1] = vs[1];
            }

            byte[] wav = new byte[44 + bytes.Length];
            byte[] header = {
                0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00,
                0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
                0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61
            };

            Buffer.BlockCopy(header, 0, wav, 0, header.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(36 + bytes.Length), 0, wav, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels), 0, wav, 22, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency), 0, wav, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, wav, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels * 2), 0, wav, 32, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, wav, 40, 4);
            Buffer.BlockCopy(bytes, 0, wav, 44, bytes.Length);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)));
            File.WriteAllBytes(filePath, wav);

            return filePath;
        }
        public static float GetMaxVoume(AudioClip clip)
        {
            float[] floats = new float[clip.samples * clip.channels];
            clip.GetData(floats, 0);

            float maxVolume = 0;
            foreach (float f in floats) maxVolume = Mathf.Max(maxVolume, Mathf.Abs(f));
            return maxVolume;
        }
    }
}
