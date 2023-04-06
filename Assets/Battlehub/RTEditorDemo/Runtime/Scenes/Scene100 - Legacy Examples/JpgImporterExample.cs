using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public class JpgImporterExample : ProjectFileImporterAsync
    {
        public override string FileExt
        {
            get { return ".jpg"; }
        }

        public override string IconPath
        {
            get { return "Importers/Jpg"; }
        }

        public override Type TargetType
        {
            get { return typeof(Texture2D); }
        }

        public override async Task ImportAsync(string filePath, string targetPath, IProjectAsync project, CancellationToken cancelToken)
        {
            byte[] bytes = filePath.Contains("://") ? 
                await DownloadBytesAsync(filePath) : 
                File.ReadAllBytes(filePath); 
            
            Texture2D texture = new Texture2D(4, 4);
            try
            {
                if (texture.LoadImage(bytes, false))
                {
                    IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
                    byte[] preview = previewUtility.CreatePreviewData(texture);

                    using (await project.LockAsync())
                    {
                        await project.SaveAsync(targetPath, texture, preview);
                    }
                }
                else
                {
                    throw new FileImporterException($"Unable to load image {filePath}");
                }
            }
            catch (Exception e)
            {
                throw new FileImporterException(e.Message, e);
            }
            finally
            {
                UnityObject.Destroy(texture);
            }
        }
    }
}
