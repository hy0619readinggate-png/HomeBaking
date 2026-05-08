using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DoDoEng.Common
{
    public class DataLoader : BYDSingleton<DataLoader>
    {
        // Methods
        public async UniTask<GameObject> LoadPrefab(string address)
        {
            LOG.Addressable($"LoadPrefab() | {address}", this);

            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LOG.Warning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'", this);
                throw handle.OperationException;
            }

            return handle.Result;
        }
        public async UniTask<AudioClip> LoadSound(string address)
        {
            LOG.Addressable($"LoadSound() | {address}", this);

            var handle = Addressables.LoadAssetAsync<AudioClip>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LOG.Warning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'", this);
                throw handle.OperationException;
            }

            return handle.Result;
        }
        public async UniTask<Sprite> LoadSprite(string address)
        {
            LOG.Addressable($"LoadSprite() | {address}", this);

            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LOG.Warning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'", this);
                throw handle.OperationException;
            }

            return handle.Result;
        }
        public async UniTask<Texture> LoadTexture(string address)
        {
            LOG.Addressable($"LoadTexture() | {address}", this);

            var handle = Addressables.LoadAssetAsync<Texture>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LOG.Warning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'", this);
                throw handle.OperationException;
            }

            return handle.Result;
        }
        public async UniTask<string> LoadJson(string address)
        {
            LOG.Addressable($"LoadJson() | {address}", this);

            var handle = Addressables.LoadAssetAsync<TextAsset>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LOG.Warning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'", this);
                throw handle.OperationException;
            }

            return handle.Result.text;
        }

        // Methods
        public void ReleaseHandles()
        {
            LOG.Info($"ReleaseHandles()", this);

            foreach (var h in handles)
                Addressables.Release(h);
            handles.Clear();
        }



        // Fields
        private List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
    }

    public static class DoDoDataLoaderExtension
    {
        // General
        public static async UniTask<GameObject> LoadPrefab(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Prefab/{name}.prefab";
            return await DataLoader.One.LoadPrefab(address);
        }
        public static async UniTask<AudioClip> LoadSound(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Sound/{name}.mp3";
            return await DataLoader.One.LoadSound(address);
        }
        public static async UniTask<Sprite> LoadSprite(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Sprite/{name}.png";
            return await DataLoader.One.LoadSprite(address);
        }
        public static async UniTask<Texture> LoadTexture(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Texture/{name}.png";
            return await DataLoader.One.LoadTexture(address);
        }

        // Quiz
        public static async UniTask<string> LoadQuizJson(this IndexBase idx)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Quiz.json";
            return await DataLoader.One.LoadJson(address);
        }
        public static async UniTask<AudioClip> LoadQuizSound(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Quiz/{name}.mp3";
            return await DataLoader.One.LoadSound(address);
        }
        public static async UniTask<Sprite> LoadQuizSprite(this IndexBase idx, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.AddressablePath),
                $"idx.AddressablePath must not be empty!");

            var address = $"{idx.AddressablePath}/Quiz/{name}.png";
            return await DataLoader.One.LoadSprite(address);
        }

        // Thumbnail
        public static async UniTask<Sprite> LoadThumbnail(this IndexBase idx)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(idx.ThumbnailPath),
                $"idx.ThumbnailPath must not be empty!");

            return await DataLoader.One.LoadSprite(idx.ThumbnailPath);
        }
    }
}