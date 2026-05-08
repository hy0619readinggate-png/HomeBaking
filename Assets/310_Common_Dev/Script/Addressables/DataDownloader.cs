using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DoDoEng.Common
{
    public class DataDownloader : BYDSingleton<DataDownloader>
    {
        // Methods
        public async UniTask DownloadData(List<IndexBase> idxList, IReportProgress reporter = null)
        {
            await DownloadData(idxList.Select(idx => idx.DownloadDataPath).ToList(), reporter);
            //using (LOG.Coroutine($"downloadData() | {idxList}", this))
            //{
            //    reporter?.Begin();
            //    float subPercent = 1f / idxList.Length;
            //    for (int i = 0; i < idxList.Length; i++)
            //    {
            //        var idx = idxList[i];
            //        try
            //        {
            //            // devBOX(epic) - 서버에 파일이 없는 경우, 404 Exception이 발생해야 하는데, 발생하지 않음.
            //            // 현재 무한 다운로드 상태로 있음. Addressable 업데이트 확인 필요
            //            // 현재 Addressable 버전은 1.21.14

            //            // Download
            //            var hDownload = Addressables.DownloadDependenciesAsync(idx.DownloadDataPath);
            //            var currentPercent = subPercent * i;
            //            while (hDownload.Status == AsyncOperationStatus.None)
            //            {
            //                var progress = currentPercent + (subPercent * hDownload.GetDownloadStatus().Percent);
            //                reporter?.ReportProgress(progress);

            //                await UniTask.Yield();
            //            }

            //            if (hDownload.OperationException != null)
            //                throw hDownload.OperationException;

            //            Addressables.Release(hDownload);

            //            reporter?.ReportProgress(currentPercent + subPercent);
            //        }
            //        catch (Exception e)
            //        {
            //            LOG.Warning($"{e.Message}", this);
            //        }
            //    }
            //    reporter?.ReportProgress(1);
            //    await UniTask.Delay(500);
            //    reporter?.End();
            //}
        }
        public async UniTask DownloadData(IndexBase idx, IReportProgress reporter = null)
        {
            var address = idx.DownloadDataPath;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.DownloadDataPath),
                $"idx.DownloadDataPath must not be empty!");

            await DownloadData(address, reporter);
        }
        public async UniTask<long> GetDataDownloadSize(IndexBase idx)
        {
            var address = idx.DownloadDataPath;

            Debug.Assert(
                !string.IsNullOrEmpty(idx.DownloadDataPath),
                $"idx.DownloadDataPath must not be empty!");

            return await GetDataDownloadSize(address);
        }
        public async UniTask<long> GetDataDownloadSize(string address)
        {
            using (LOG.Coroutine($"getDataDownloadSize() | {address}", this))
            {
                var hSize = Addressables.GetDownloadSizeAsync(address);
                await hSize.Task;

                var result = hSize.Result;

                var ex = hSize.OperationException;
                Addressables.Release(hSize);
                if (ex != null)
                    throw ex;

                LOG.Addressable($"Download Size : {result}", this);
                return result;
            }
        }
        public async UniTask<long> GetDataDownloadSize(List<IndexBase> idxList)
        {
            Debug.Assert(
                idxList.All(idx => !string.IsNullOrEmpty(idx.DownloadDataPath)),
                $"idx.DownloadDataPath must not be empty!");

            return await GetDataDownloadSize(idxList.Select(idx => idx.DownloadDataPath).ToList());
        }
        public async UniTask<long> GetDataDownloadSize(List<string> address)
        {
            using (LOG.Coroutine($"getDataDownloadSize() | {address}", this))
            {
                var hSize = Addressables.GetDownloadSizeAsync(address);
                await hSize.Task;

                var result = hSize.Result;

                var ex = hSize.OperationException;
                Addressables.Release(hSize);

                if (ex != null)
                    throw ex;

                LOG.Addressable($"Download Size : {result}", this);
                return result;
            }
        }
        public async UniTask DownloadData(string address, IReportProgress reporter = null)
        {
            using (LOG.Coroutine($"downloadData() | {address}", this))
            {
                try
                {
                    reporter?.Begin();

                    // devBOX(epic) - 서버에 파일이 없는 경우, 404 Exception이 발생해야 하는데, 발생하지 않음.
                    // 현재 무한 다운로드 상태로 있음. Addressable 업데이트 확인 필요
                    // 현재 Addressable 버전은 1.21.14

                    // Download
                    var hDownload = Addressables.DownloadDependenciesAsync(address);
                    while (hDownload.Status == AsyncOperationStatus.None)
                    {
                        var progress = hDownload.GetDownloadStatus().Percent;
                        reporter?.ReportProgress(progress);

                        await UniTask.Yield();
                    }

                    var ex = hDownload.OperationException;
                    Addressables.Release(hDownload);

                    if (ex != null)
                        throw ex;

                    reporter?.ReportProgress(1);
                    await UniTask.Delay(500);
                }
                finally
                {
                    reporter?.End();
                }
            }
        }
        public async UniTask DownloadData(List<string> addressList, IReportProgress reporter = null)
        {
            using (LOG.Coroutine($"downloadData() | {addressList}", this))
            {
                try
                {
                    reporter?.Begin();

                    // devBOX(epic) - 서버에 파일이 없는 경우, 404 Exception이 발생해야 하는데, 발생하지 않음.
                    // 현재 무한 다운로드 상태로 있음. Addressable 업데이트 확인 필요
                    // 현재 Addressable 버전은 1.21.14

                    // Download
                    var hDownload = Addressables.DownloadDependenciesAsync(addressList, Addressables.MergeMode.Union);
                    while (hDownload.Status == AsyncOperationStatus.None)
                    {
                        var progress = hDownload.GetDownloadStatus().Percent;
                        reporter?.ReportProgress(progress);

                        await UniTask.Yield();
                    }

                    var ex = hDownload.OperationException;
                    Addressables.Release(hDownload);

                    if (ex != null)
                        throw ex;

                    reporter?.ReportProgress(1);
                    await UniTask.Delay(500);
                }
                finally
                {
                    reporter?.End();
                }
            }
        }
    }

    public interface IReportProgress
    {
        void Begin();
        void End();
        void ReportProgress(float progress);
    }
}