using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DoDoEng.Launcher
{
    public enum APIServer
    {
        None = 0,

        Production,
        Test,
    }

    public class API
    {
        // Methods
        public static API One
        {
            get
            {
                if (one == null)
                    one = new API();

                return one;
            }
        }


        // Definitions
        public struct Result
        {
            public bool Success;
            public JObject Data;
            public JArray Datas;
        }
        public enum Method
        {
            Get,
            Post,
            Put,
            Delete
        }

        // Properties
        public string MediaHost => hostMedia;
        public string ParentSignInHost => hostParentSignIn;
        public string ParentInfoHost => hostParentInfo;

        // Methods
        public void SwitchTo(APIServer server)
        {
            if (currentServer != server)
            {
                LOG.LMS($"SwitchTo() | {server}", this);

                hostAPI = hostsAPI[server];
                hostMedia = hostsMedia[server];
                hostParentSignIn = hostsParent[server] + "/login/";
                hostParentInfo = hostsParent[server] + "/mypage/profile/";

                currentServer = server;
            }
        }

        // Methods
        public async UniTask<Result> Call(string token, string api, Method method = Method.Get, JObject body = null)
        {
            LOG.LMS($"Call() api: {api}, method: {method}, body:{body?.ToString(Newtonsoft.Json.Formatting.None)}", this);

            UnityWebRequest request = null;
            Result result = new()
            {
                Success = false,
                Data = new(),
                Datas = new(),
            };

            if (method == Method.Get)
            {
                request = UnityWebRequest.Get(hostAPI + api);
            }
            else if (method == Method.Post)
            {
                request = UnityWebRequest.Post(hostAPI + api, body == null ? "" : body.ToString(Newtonsoft.Json.Formatting.None), "application/json");
            }
            else if (method == Method.Put)
            {
                request = UnityWebRequest.Put(hostAPI + api, body == null ? "" : body.ToString(Newtonsoft.Json.Formatting.None));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            else if (method == Method.Delete)
            {
                request = UnityWebRequest.Delete(hostAPI + api);
            }

            if (request != null)
            {
                if (token != null)
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }
                request.SetRequestHeader("user-locale", LocalizationMGR.One.Locale.ToString());
                var userAgent = $"HiDODO/{Application.version};{SystemInfo.operatingSystem};{SystemInfo.deviceModel};hidodoapp";
                request.SetRequestHeader("User-Agent", userAgent);
                try
                {
                    var op = await request.SendWebRequest();

                    if (op.result == UnityWebRequest.Result.ConnectionError)
                    {
                        LOG.Error($"Call error: {op.error}", this);
                        result.Data.Add("error", op.error);
                    }
                    else
                    {
                        if (op.responseCode == 200 || op.responseCode == 201 || op.responseCode == 204)
                        {
                            LOG.LMS($"Call response: {op.downloadHandler?.text}", this);
                            result.Success = true;
                        }
                        else
                        {
                            LOG.Error($"Call code: {op.responseCode}, error: {op.error}, response: {op.downloadHandler?.text}", this);
                        }
                        string response = op.downloadHandler?.text;
                        if (!string.IsNullOrEmpty(response))
                        {
                            try
                            {
                                result.Data = JObject.Parse(op.downloadHandler.text);
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    result.Datas = JArray.Parse(op.downloadHandler.text);
                                }
                                catch (Exception ex2)
                                {
                                    LOG.Warning($"result.Data parsing error: {ex.Message}", this);
                                    LOG.Warning($"result.Datas parsing error: {ex2.Message}", this);
                                }
                            }
                        }
                    }
                }
                catch (UnityWebRequestException e)
                {
                    LOG.Debug($"Call error: {e.Message}", this);
                    try
                    {
                        result.Data = JObject.Parse(e.Text);
                    }
                    catch { }
                    //LOG.Debug($"Call error: {result.Data.ToString(Newtonsoft.Json.Formatting.None)}", this);
                }
            }
            else
            {
                LOG.Error($"Call error: request is null. (Method: {method})", this);
            }

            return result;
        }
        public async UniTask<bool> Upload(string token, string api, Method method, byte[] bytes)
        {
            LOG.LMS($"Upload() api: {api}, method: {method}", this);

            UnityWebRequest request = null;
            if (method == Method.Post)
            {
                WWWForm form = new WWWForm();
                form.AddBinaryData("uploadFile", bytes, "profile.png");
                request = UnityWebRequest.Post(hostAPI + api, form);
            }
            else if (method == Method.Put)
            {
                request = UnityWebRequest.Put(hostAPI + api, bytes);
            }

            if(request != null)
            {
                if (token != null)
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }
                request.SetRequestHeader("user-locale", LocalizationMGR.One.Locale.ToString());
                var userAgent = $"HiDODO/{Application.version};{SystemInfo.operatingSystem};{SystemInfo.deviceModel};hidodoapp";
                request.SetRequestHeader("User-Agent", userAgent);

                try
                {
                    var op = await request.SendWebRequest();
                    if (op.result != UnityWebRequest.Result.Success)
                    {
                        LOG.Warning($"Upload error: {op.error}", this);
                        return false;
                    }
                    else
                    {
                        LOG.LMS($"Upload completed: {op.downloadHandler.text}", this);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Warning($"Upload error: {ex.Message}", this);
                    return false;
                }
            }
            else
            {
                LOG.Error($"Call error: request is null. (Method: {method})", this);
            }

            return false;
        }
        public async UniTask<Texture2D> DownloadImage(string token, string api)
        {
            LOG.LMS($"DownloadImage() api: {api}", this);

            var request = UnityWebRequestTexture.GetTexture(hostAPI + api);
            if (token != null)
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }
            request.SetRequestHeader("user-locale", LocalizationMGR.One.Locale.ToString());
            var userAgent = $"HiDODO/{Application.version};{SystemInfo.operatingSystem};{SystemInfo.deviceModel};hidodoapp";
            request.SetRequestHeader("User-Agent", userAgent);

            try
            {
                var op = await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    LOG.Warning($"Download error: {op.error}", this);
                    return null;
                }
                else
                {
                    LOG.LMS($"Download completed.", this);
                    return DownloadHandlerTexture.GetContent(request);
                    //return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (Exception e)
            {
                LOG.Warning(e.Message, this);
            }
            return null;
        }
        public async UniTask<bool> Upload(string url, byte[] bytes)
        {
            LOG.LMS($"Upload({url})", this);

            UnityWebRequest request = UnityWebRequest.Put(url, bytes);

            try
            {
                var op = await request.SendWebRequest();
                if (op.result != UnityWebRequest.Result.Success)
                {
                    LOG.Warning($"Upload error: {op.error}", this);
                    return false;
                }
                else
                {
                    LOG.LMS($"Upload completed.", this);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LOG.Warning($"Upload error: {ex.Message}", this);
                return false;
            }
        }
        public async UniTask<byte[]> Download(string url)
        {
            LOG.LMS($"Download({url})", this);

            UnityWebRequest request = UnityWebRequest.Get(url);

            var op = await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                LOG.Warning($"Download error: {op.error}", this);
                return null;
            }
            else
            {
                LOG.LMS($"Download completed.", this);
                return request.downloadHandler.data;
            }
        }
        public async UniTask<Texture2D> DownloadImage(string url)
        {
            LOG.LMS($"DownloadImage({url})", this);

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                try
                {
                    var op = await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        LOG.Warning($"Download error: {op.error}", this);
                        return null;
                    }
                    else
                    {
                        LOG.LMS($"Download completed.", this);
                        return DownloadHandlerTexture.GetContent(request);
                    }
                }
                catch (Exception e)
                {
                    LOG.Warning(e.Message, this);
                }
            }
            return null;
        }

        // Methods : ctor.
        public API()
        {
            SwitchTo(APIServer.Production);
        }



        // Fields
        private static API one = null;
        private static Dictionary<APIServer, string> hostsAPI;
        private static Dictionary<APIServer, string> hostsMedia;
        private static Dictionary<APIServer, string> hostsParent;
        private string hostAPI = string.Empty;
        private string hostMedia = string.Empty;
        private string hostParentSignIn = string.Empty;
        private string hostParentInfo = string.Empty;
        private APIServer currentServer = APIServer.None;

        // Static ctor.
        static API()
        {
            hostsAPI = new Dictionary<APIServer, string>();
            hostsAPI[APIServer.Production] = "https://api.gohidodo.com";
            hostsAPI[APIServer.Test] = "https://api.dev.gohidodo.com";

            hostsMedia = new Dictionary<APIServer, string>();
            hostsMedia[APIServer.Production] = "https://content.gohidodo.com";
            hostsMedia[APIServer.Test] = "https://content.dev.gohidodo.com";

            hostsParent = new Dictionary<APIServer, string>();
            hostsParent[APIServer.Production] = "https://membership.gohidodo.com";
            hostsParent[APIServer.Test] = "https://membership.dev.gohidodo.com";
        }
    }
}