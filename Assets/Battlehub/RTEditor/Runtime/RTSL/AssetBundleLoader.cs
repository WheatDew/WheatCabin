using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if USE_GOOGLE_DRIVE
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Download;
using System.Threading;
using System.Linq;

using Battlehub;
using Battlehub.Utils;
#endif

namespace Battlehub.RTSL
{
    public delegate void LoadAssetBundleHandler(AssetBundle bundle);
    public delegate void ListAssetBundlesHandler(string[] bundleNames);
    public interface IAssetBundleLoader
    {
        string AssetBundlesPath
        {
            get;
            set;
        }
        void GetAssetBundles(ListAssetBundlesHandler assetBundles);
        void Load(string name, LoadAssetBundleHandler callback);
    }

    #if USE_GOOGLE_DRIVE
    public class GoogleDriveAssetBundleLoader : IAssetBundleLoader
    {
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = Application.productName;
        static string PersistentDataPath = Application.persistentDataPath;
        static string StreamingAssetsPath = Application.streamingAssetsPath;
        static bool IsPlaying = Application.isPlaying;

        public DriveService GetService()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(StreamingAssetsPath + "/credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(PersistentDataPath + "/" + credPath, true)).Result;
            }

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private IList<Google.Apis.Drive.v3.Data.File> m_files;
        public void GetAssetBundles(ListAssetBundlesHandler callback)
        {
            List<string> result = new List<string>();

            QueueWorkItem(() =>
            {
                try
                {
                    DriveService service = GetService();
                    GetFiles(service);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
                
                if (m_files != null && m_files.Count > 0)
                {
                    foreach (var file in m_files)
                    {
                        string name = Path.GetFileName(file.Name);
                        if (!result.Contains(name))
                        {
                            result.Add(name);
                        }
                    }
                }
                else
                {
                    Debug.LogFormat("No files found.");
                }

                Dispatch(() =>
                {
                    callback(result.ToArray());
                });
            });

        }

        private void GetFiles(DriveService service)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            m_files = listRequest.Execute().Files;
        }

        public void Load(string bundleName, LoadAssetBundleHandler callback)
        {
            QueueWorkItem(() =>
            {
                DriveService service = GetService();
                if (m_files == null)
                {
                    GetFiles(service);
                    if (m_files == null)
                    {
                        Callback(callback, null);
                        return;
                    }
                }

                var file = m_files.Where(f => f.Name == bundleName).FirstOrDefault();
                if (file == null)
                {
                    Callback(callback, null);
                    return;
                }

                var googleDriveRequest = service.Files.Get(file.Id);
                var stream = new MemoryStream();
                IDownloadProgress progress = googleDriveRequest.DownloadWithStatus(stream);
                switch (progress.Status)
                {
                    case DownloadStatus.Completed:
                        {
                            Dispatch(() =>
                            {
                                AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
                                if (request.isDone)
                                {
                                    AssetBundle bundle = request.assetBundle;
                                    Callback(callback, bundle);
                                }
                                else
                                {
                                    Action<AsyncOperation> completed = null;
                                    completed = result =>
                                    {
                                        AssetBundle bundle = request.assetBundle;
                                        request.completed -= completed;
                                        Callback(callback, bundle);
                                    };
                                    request.completed += completed;
                                }
                            });
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Debug.LogError("Download failed.");
                            Callback(callback, null);
                            break;
                        }
                }
            });
        }

        private static void QueueWorkItem(Action action)
        {
            if(IsPlaying)
            {
                ThreadPool.QueueUserWorkItem(arg =>
                {
                    action();
                });
            }
            else
            {
                action();
            }
        }

        private static void Dispatch(Action action)
        {
            if(IsPlaying)
            {
                if (Dispatcher.Current != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        action();
                    });
                }
                else
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }

        private static void Callback(LoadAssetBundleHandler callback, AssetBundle bundle)
        {
            if (IsPlaying)
            {
                if (Dispatcher.Current != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        callback(bundle);
                    });
                }
                else
                {
                    callback(bundle);
                }
            }
            else
            {
                callback(bundle);
            }
        }
    }
    #endif
    public class AssetBundleLoader : IAssetBundleLoader
    {
        private string m_assetBundlesPath = Application.streamingAssetsPath;
        public string AssetBundlesPath
        {
            get { return m_assetBundlesPath; }
            set { m_assetBundlesPath = value; }
        }

        public void GetAssetBundles(ListAssetBundlesHandler callback)
        {
            List<string> result = new List<string>();

            if (Directory.Exists(AssetBundlesPath))
            {
                string[] manifestFiles = Directory.GetFiles(AssetBundlesPath, "*.manifest");
                for (int i = 0; i < manifestFiles.Length; ++i)
                {
                    string assetBundleFile = Path.GetDirectoryName(manifestFiles[i]) + "/" + Path.GetFileNameWithoutExtension(manifestFiles[i]);
                    if (File.Exists(assetBundleFile))
                    {
                        result.Add(Path.GetFileName(assetBundleFile));
                    }
                }
            }
            else
            {
                Debug.Log("StreamingAssets folder does not exist. No asset bundles to load.");
            }

            callback(result.ToArray());
        }

        public void Load(string bundleName, LoadAssetBundleHandler callback)
        {
            if (!File.Exists(AssetBundlesPath + "/" + bundleName))
            {
                callback(null);
                return;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(AssetBundlesPath + "/" + bundleName);
            if (request.isDone)
            {
                AssetBundle bundle = request.assetBundle;
                if (callback != null)
                {
                    callback(bundle);
                }
            }
            else
            {
                Action<AsyncOperation> completed = null;
                completed = result =>
                {
                    AssetBundle bundle = request.assetBundle;
                    if (callback != null)
                    {
                        callback(bundle);
                    }
                    request.completed -= completed;
                };
                request.completed += completed;
            }
        }
    }
}