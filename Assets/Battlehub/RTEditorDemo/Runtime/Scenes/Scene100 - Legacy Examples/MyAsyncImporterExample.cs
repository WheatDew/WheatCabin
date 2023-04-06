/*
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class MyAsyncImporterExample : FileImporter
    {
        public override string FileExt
        {
            get { return ".obj"; }
        }

        public override string IconPath
        {
            get { return "Importers/Obj"; }
        }

        public override IEnumerator Import(string filePath, string targetPath)
        {
            Task importTask = ImportAsync(filePath, targetPath);
            yield return new WaitForTask(importTask);
        }

        private static async Task ImportAsync(string filePath, string targetPath)
        {
            await Task.Delay(1000);

            (Vector3[] vertices, int[] tris) = await Task.Run(() => PrepareData(filePath));

            await CreatePrefab(filePath, targetPath, CreateGameObject(filePath, vertices, tris));
        }

        private static (Vector3[] importedVertices, int[] importedTris) PrepareData(string filePath)
        {
            Vector3[] importedVertices = new[]
            {
                new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
            };

            int[] importedTris = new int[]
            {
                0, 1, 2
            };

            return (importedVertices, importedTris);
        }

        private static GameObject CreateGameObject(string filePath, Vector3[] vertices, int[] tris)
        {
            GameObject go = new GameObject();
            go.name = Path.GetFileName(filePath);

            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = RenderPipelineInfo.DefaultMaterial;
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = new Mesh
            {
                vertices = vertices,
                triangles = tris,
            };
            filter.sharedMesh.RecalculateNormals();
            return go;
        }

        private static Task CreatePrefab(string filePath, string targetPath, GameObject loadedGameObject)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            IProject project = IOC.Resolve<IProject>();
            ProjectItem targetFolder = project.GetFolder(Path.GetDirectoryName(targetPath));

            loadedGameObject.SetActive(false);
            loadedGameObject.name = Path.GetFileName(filePath);

            Transform[] children = loadedGameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; ++i)
            {
                children[i].gameObject.AddComponent<ExposeToEditor>();
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            bool includeDependencies = true;
            editor.CreatePrefab(targetFolder, loadedGameObject.GetComponent<ExposeToEditor>(), includeDependencies, assetItems =>
            {
                Object.Destroy(loadedGameObject);
                tcs.SetResult(null);
            });

            return tcs.Task;
        }
    }
}
*/
