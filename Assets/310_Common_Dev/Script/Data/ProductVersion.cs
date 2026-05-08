using Cysharp.Threading.Tasks;
using Jose;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1;

namespace DoDoEng.Common
{
    public class ProductVersion
    {
        // Methods
        public static ProductVersion One
        {
            get
            {
                if (one == null)
                    one = new ProductVersion();

                return one;
            }
        }


        // Definitions
        // Google 서비스 계정 JSON 구조
        [Serializable]
        public class ServiceAccountKey
        {
            public string type;
            public string project_id;
            public string private_key_id;
            public string private_key;
            public string client_email;
            public string client_id;
            public string auth_uri;
            public string token_uri;
            public string auth_provider_x509_cert_url;
            public string client_x509_cert_url;
        }

        // OAuth 토큰 응답 구조
        [Serializable]
        public class GoogleOAuthResponse
        {
            public string access_token;
            public string token_type;
            public int expires_in;
        }

        // Play Store API 응답 구조
        [Serializable]
        public class EditResponse
        {
            public string id;  // Edit ID
        }

        [Serializable]
        public class TrackResponse
        {
            public string track;
            public Release[] releases;
        }

        [Serializable]
        public class Release
        {
            public string name;
            public string[] versionCodes;
            public string status;
        }

        // Properties
        public string Version => version;

        // Methods
        public async UniTask LoadVersion()
        {
#if UNITY_IOS
            await LoadVersionIOS();
#else
            await LoadVersionAndroid();
#endif
        }
        public async UniTask LoadVersionAndroid()
        {
            string PACKAGE_NAME = Application.identifier;
            string GOOGLE_OAUTH_URL = "https://oauth2.googleapis.com/token";
            string GOOGLE_API_BASE = "https://androidpublisher.googleapis.com/androidpublisher/v3/applications/";

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payload = new
            {
                iss = "googleplay-api-service@hidodo-6273e.iam.gserviceaccount.com",
                scope = "https://www.googleapis.com/auth/androidpublisher",
                aud = GOOGLE_OAUTH_URL,
                exp = now + 3600, // 1시간 후 만료
                iat = now
            };

            RSA privateKey;
            string privateKeyPem = "-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC6eheWVxsJrYUw\nEc59QBvq1SsnsnQ38KAY0yzDwrC6nYZFO81rPuBoQWE4/N35cpboyZZXg4R6H+KP\n3dRwb9TVn9r6f0O02ql21k/u1EAyaX66bDKEBDKfSSRyxZJ+sc/H63ijM1+q5D/t\nAilaVLj9O6ApteKnp2btTLVAXZexlyMHUIGmaMjXtw4Cdg/7w8cqwMf1VvOjrtVR\nRA2BmVB//jUgp13bg4vWZGLI6isoACKEYVvp6tBz1yMftqcI4rYcWWF+ZA08V4sv\ncLFH7x2GI9Gx2naBrKY+SvxRqsskB/dc7O9Nm++ZYKH1P1GgOBBzdE6eKk99Fi3M\nVeEfyipfAgMBAAECggEAVCBMj9iQLBeKJJuD6mGHtrOgmnIZmDsIHur2IQKuNCJo\ntsc57w4Ysy/7qnf5tFWL201GdAizNV05Gupasrbg79cENUpAw1B6b1BUE0zSAWwy\nbevuBjzWkaGvvc+APYP+VxTWAdplaHU8mbIF8eiS5DuIEAksTbJp7thSVM1kaAsf\nYA2PrFULKgMME6HWUxhFaz6ozmx92Ba1eBwsLLegDRU2FvQ/nv6UrWyUXbjLXD5+\nH9dhjA7lO9PVXcyq2OwMVdHTxTxII1Qu63LNASf02ibNSKcBCURxJeJWxM1aYme3\nc8cCUuru8C8MFa8AW4CTKceZZ3DjM066qixBY+CwEQKBgQDrqNF3l/oKWmklwrWS\n6+lYWQU65isUcCHAjm0f9LP56lNN7s2Ki3o5KB1JzhDyN5kigVAGjIPsNL+RSGGi\nSCKUEN2VCIiZCVXElbZo8Cqx6GF8RgnYFv3Vwx+6ibd1OpNIzw7Gl4N4oVt6XIqF\noLlpTnCgQTTCHDpwjGPDNdE4LQKBgQDKkoa7ARwGNUfCRDzg1eMw4T+GXY4XuDsJ\nPPI3pJZmWr5bg0pqtMBeVc+GMl/sZ8nPCTymJ4Y5ebOWVFg3dhIwMbOC+W7LQZFi\nlGELDsRZ3X1HEebOCWHRjU30d51zCqJ4QHkqEAwnSGOraJtkZvlkMXnlMplMTHWV\n3NlOG0YYOwKBgGcJzmSbJ7HFLDpdcyTFT6bYbYf+QZHTIX7fB2m0hcjdIwVtR6Ov\nLoa1OnEdz50IoEVNAx+J6tipi6VLX50kKzJQWYIjVA6N3VlyGGqzsAdP0ugSe5Vn\nIB+KDGJ0eqmgLevk1NISR2LEMopC0jJxPwCDUo1PVXEWBJtXVvtwmYUVAoGAWL0J\nYkAhK6MJdVN5K7DmnlH+BvlStpdQ3UIihTeHXv0faG1CGy5lGq4Sg6HrnV7168kL\nFWo5BVKH1jHKlzkUzKHpFlX4L/fgHUQMotBsOUWqjIiOB/HaNPbmkFIZ2fxtoan0\nqfVvrYGrj7n6YGgvlbGKyVl6CA9ybxzY4LtX4GMCgYBatPy1vkHZyLu49m8hZZVC\nmKrwwsuyzkxZGKLl73aWPzgVptc0WAT4MyTAK0wXrDJJSJs+MoeiJT1NGoRwuYP6\n7YkK1oPIdqnGjPnHvvWxJBya7DHLvWayY9pqMV32ub600K8nSjjbiSn5xsIcIYbU\nO1gDbZN6JVccBVCiwKDHLQ==\n-----END PRIVATE KEY-----\n";

            using (var reader = new StringReader(privateKeyPem))
            {
                var pemReader = new PemReader(reader);
                //object keyObject = pemReader.ReadObject();
                var pemObject = pemReader.ReadPemObject();
                Asn1Sequence instance = Asn1Sequence.GetInstance(pemObject.Content);
                object keyObject = PrivateKeyFactory.CreateKey(PrivateKeyInfo.GetInstance(instance));

                if (keyObject is AsymmetricCipherKeyPair keyPair)
                {
                    privateKey = RSA.Create(DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private));
                }
                else if (keyObject is RsaPrivateCrtKeyParameters rsaKey)
                {
                    privateKey = RSA.Create(DotNetUtilities.ToRSAParameters(rsaKey));
                }
                else
                {
                    throw new Exception("지원되지 않는 RSA 키 형식입니다.");
                }
            }

            string jwtToken = JWT.Encode(payload, privateKey, JwsAlgorithm.RS256);

            var payload2 = new
            {
                grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                assertion = jwtToken
            };
            string token = null;
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload2), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(GOOGLE_OAUTH_URL, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    GoogleOAuthResponse authResponse = JsonConvert.DeserializeObject<GoogleOAuthResponse>(responseBody);
                    token = authResponse.access_token;
                    LOG.Info($"OAuth 토큰 발급 성공: {token}", this);
                }
                else
                {
                    LOG.Warning("OAuth 인증 실패: " + responseBody, this);
                }
            }

            string editId = null;
            string url = $"{GOOGLE_API_BASE}{PACKAGE_NAME}/edits";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    EditResponse editData = JsonConvert.DeserializeObject<EditResponse>(responseBody);
                    LOG.Info($"Edit 생성 성공: {editData.id}", this);
                    editId = editData.id;
                }
                else
                {
                    LOG.Warning("Edit 생성 실패: " + responseBody, this);
                }
            }

            if (!string.IsNullOrEmpty(editId))
            {
                string url2 = $"{GOOGLE_API_BASE}{PACKAGE_NAME}/edits/{editId}/tracks/production";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = await client.GetAsync(url2);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        TrackResponse trackData = JsonConvert.DeserializeObject<TrackResponse>(responseBody);
                        if (trackData.releases != null && trackData.releases.Length > 0)
                        {
                            string latestVersion = trackData.releases[0].name;
                            LOG.Info($"Play Store 최신 버전: {latestVersion}", this);
                            var idx1 = latestVersion.IndexOf("(");
                            var idx2 = latestVersion.IndexOf(")");
                            version = latestVersion.Substring(idx1 + 1, idx2 - idx1 - 1);
                        }
                    }
                    else
                    {
                        LOG.Warning("배포 트랙 정보 가져오기 실패: " + responseBody, this);
                    }
                }
            }
        }
        public async UniTask LoadVersionIOS()
        {
            string BUNDLE_ID = "6467715927";
            string url = $"https://itunes.apple.com/lookup?id={BUNDLE_ID}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    JObject json = JObject.Parse(responseBody);
                    if (json["resultCount"].Value<int>() > 0)
                    {
                        string latestVersion = json["results"][0]["version"].ToString();
                        LOG.Info($"App Store 최신 버전: {latestVersion}", this);
                        version = latestVersion;
                    }
                    else
                    {
                        LOG.Warning("앱 정보를 찾을 수 없습니다. 번들 ID를 확인하세요.", this);
                    }
                }
                else
                {
                    LOG.Warning("App Store 요청 실패: " + responseBody, this);
                }
            }
        }

        // Methods

        // Methods : ctor.
        public ProductVersion()
        {
        }



        // Fields
        private static ProductVersion one = null;
        private string version = "";

        // Static ctor.
        static ProductVersion()
        {
        }
    }
}