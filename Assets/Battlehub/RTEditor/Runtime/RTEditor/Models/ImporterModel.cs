using UnityEngine;
using System.Collections;
using Battlehub.RTCommon;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Battlehub.RTSL.Interface;
using Battlehub.RTEditor.ViewModels;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;

namespace Battlehub.RTEditor
{
    [Obsolete("Use IFileImporterAsync")]
    public interface IFileImporter : IFileImporterDescription
    {
        IEnumerator Import(string filePath, string targetPath);
    }

    [Obsolete("Use FileImporterAsync")]
    public abstract class FileImporter : IFileImporter
    {
        public abstract string FileExt { get; }

        public abstract string IconPath { get; }

        public virtual int Priority
        {
            get { return 0; }
        }

        public abstract IEnumerator Import(string filePath, string targetPath);
    }


    [Serializable]
    public class FileImporterException : Exception
    {
        public FileImporterException() { }
        public FileImporterException(string message) : base(message) { }
        public FileImporterException(string message, Exception inner) : base(message, inner) { }
        protected FileImporterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public abstract class FileImporterAsync : IFileImporterAsync
    {
        public abstract string FileExt { get; }

        public abstract string IconPath { get; }

        public virtual int Priority
        {
            get { return 0; }
        }

        public abstract Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken);
    }


    [Serializable]
    public class UnityWebRequestException : Exception
    {
        public UnityWebRequestException() { }
        public UnityWebRequestException(string message) : base(message) { }
        public UnityWebRequestException(string message, Exception inner) : base(message, inner) { }
        protected UnityWebRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public abstract class ProjectFileImporterAsync : FileImporterAsync
    {
        public abstract Type TargetType
        {
            get;
        }

        public override Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            ProjectItem target = project.Utils.Get(targetPath, TargetType);
            if (target != null)
            {
                targetPath = project.Utils.GetUniquePath(targetPath, TargetType, target.Parent);
            }

            return ImportAsync(filePath, targetPath, project, cancelToken);
        }

        public abstract Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken);

        protected Task<byte[]> DownloadBytesAsync(string filePath)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            IOC.Resolve<IRTE>().StartCoroutine(CoDownloadBytes(filePath, tcs));
            return tcs.Task;
        }

        private IEnumerator CoDownloadBytes(string filePath, TaskCompletionSource<byte[]> tcs)
        {
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                tcs.SetException(new UnityWebRequestException(www.error));
            }
            else
            {
                tcs.SetResult(www.downloadHandler.data);
            }
        }
    }
}

namespace Battlehub.RTEditor.Models
{
    public class ImporterModel : MonoBehaviour, IImporterModel
    {
        private readonly Dictionary<string, IFileImporterDescription> m_extToFileImporter = new Dictionary<string, IFileImporterDescription>();

        public string[] Extensions
        {
            get;
            private set;
        }

        public Sprite[] Icons
        {
            get;
            private set;
        }

        protected virtual void Awake()
        {
            if (!IOC.IsFallbackRegistered<IImporterModel>())
            {
                IOC.RegisterFallback<IImporterModel>(this);
            }


            Dictionary<string, Sprite> extToIcon = new Dictionary<string, Sprite>();
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in KnownAssemblies.Names)
            {
                var asName = new AssemblyName();
                asName.Name = assemblyName;

                try
                {
                    Assembly asm = Assembly.Load(asName);
                    assemblies.Add(asm);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            Type[] importerTypes = assemblies.SelectMany(asm => asm.GetTypes().Where(t => t != null && t.IsClass && typeof(IFileImporterDescription).IsAssignableFrom(t))).ToArray();
            foreach (Type importerType in importerTypes)
            {
                if (importerType.IsAbstract)
                {
                    continue;
                }

                try
                {
                    IFileImporterDescription fileImporter = (IFileImporterDescription)Activator.CreateInstance(importerType);

                    string ext = fileImporter.FileExt;
                    ext = ext.ToLower();

                    if (!ext.StartsWith("."))
                    {
                        ext = "." + ext;
                    }

                    if (m_extToFileImporter.ContainsKey(ext))
                    {
                        int priority = fileImporter.Priority;
                        if (fileImporter is IFileImporterAsync)
                        {
                            priority++;
                        }

                        if (m_extToFileImporter[ext].Priority > priority)
                        {
                            continue;
                        }
                    }
                    m_extToFileImporter[ext] = fileImporter;
                    extToIcon[ext] = Resources.Load<Sprite>(fileImporter.IconPath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to instantiate File Importer " + e.ToString());
                }
            }

            Extensions = extToIcon.Keys.ToArray();
            Icons = extToIcon.Values.ToArray();
        }

        protected virtual void OnDestroy()
        {
            IOC.UnregisterFallback<IImporterModel>(this);

            m_extToFileImporter.Clear();
            Extensions = null;
            Icons = null;
        }

        public IFileImporterAsync GetImporter(string ext)
        {
            if (!m_extToFileImporter.TryGetValue(ext.ToLower(), out IFileImporterDescription importer))
            {
                return null;
            }

            if (importer is IFileImporterAsync)
            {
                return (IFileImporterAsync)importer;
            }

            return null;
        }

        public IFileImporterDescription GetImporterDescription(string ext)
        {
            if (!m_extToFileImporter.TryGetValue(ext.ToLower(), out IFileImporterDescription importer))
            {
                return null;
            }

            return importer;
        }

        public Task ImportAsync(string path, string ext, CancellationToken cancelToken)
        {
            IFileImporterDescription importer = GetImporterDescription(ext);
            if (importer is IFileImporterAsync)
            {
                return ImportAsync(path, (IFileImporterAsync)importer, cancelToken);
            }

#pragma warning disable CS0618
            if (importer is IFileImporter)
            {
                return Import(path, (IFileImporter)importer);
            }
#pragma warning restore CS0618

            throw new ArgumentException($"Importer for {path} not found");
        }

        public Task ImportAsync(string path, IFileImporterAsync importer, CancellationToken cancelToken)
        {
            string targetPath = GetTargetPath(path);
            return importer.ImportAsync(path, targetPath, cancelToken);
        }

#pragma warning disable CS0618
        private Task Import(string path, IFileImporter importer)
#pragma warning restore CS0618
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            IRTE rte = IOC.Resolve<IRTE>();
            rte.StartCoroutine(CoImport(path, importer, tcs));
            return tcs.Task;
        }

#pragma warning disable CS0618
        private IEnumerator CoImport(string path, IFileImporter importer, TaskCompletionSource<object> tcs)
#pragma warning restore CS0618
        {
            string targetPath = GetTargetPath(path);

            IRTE rte = IOC.Resolve<IRTE>();
            yield return rte.StartCoroutine(importer.Import(path, targetPath));
            tcs.SetResult(true);
        }

        private static string GetTargetPath(string path)
        {
            ProjectItem folder;
            IProjectTreeModel projectTreeModel = IOC.Resolve<IProjectTreeModel>();
            if (projectTreeModel != null && projectTreeModel.SelectedItem != null)
            {
                folder = projectTreeModel.SelectedItem;
            }
            else
            {
                IProjectTree projectTree = IOC.Resolve<IProjectTree>();
                if (projectTree != null)
                {
                    folder = projectTree.SelectedItem;
                }
                else
                {
                    folder = IOC.Resolve<IProjectAsync>().State.RootFolder;
                }
            }

            string targetPath = System.IO.Path.GetFileNameWithoutExtension(path);
            targetPath = folder.RelativePath(false) + "/" + targetPath;
            targetPath = targetPath.TrimStart('/');
            return targetPath;
        }
    }

}
