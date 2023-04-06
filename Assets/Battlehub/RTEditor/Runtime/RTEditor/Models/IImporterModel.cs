using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IFileImporterDescription
    {
        string FileExt
        {
            get;
        }

        string IconPath
        {
            get;
        }

        int Priority
        {
            get;
        }
    }

    public interface IFileImporterAsync : IFileImporterDescription
    {
        Task ImportAsync(string filePath, string targetPath, CancellationToken cancelToken);
    }
}

namespace Battlehub.RTEditor.Models
{
    public interface IImporterModel
    {
        string[] Extensions
        {
            get;
        }

        Sprite[] Icons
        {
            get;
        }

        IFileImporterAsync GetImporter(string ext);
        Task ImportAsync(string path, string ext, CancellationToken cancelToken = default);
        Task ImportAsync(string path, IFileImporterAsync importer, CancellationToken cancelToken = default);
    }
}