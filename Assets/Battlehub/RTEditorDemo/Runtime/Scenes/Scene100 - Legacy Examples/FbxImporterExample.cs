/*
using Battlehub.MeshTools;
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TriLib;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class FbxImporterExample : FileImporter
    {
        public override string FileExt
        {
            get { return ".fbx"; }
        }

        public override string IconPath
        {
            get { return "Importers/Fbx"; }
        }

        private bool m_loaded;

        public override IEnumerator Import(string filePath, string targetPath)
        {
            m_loaded = false;

            using (var assetLoader = new AssetLoaderAsync())
            {
                try
                {
                    var assetLoaderOptions = DefaultAssetLoaderOptions(1.0f);

                    assetLoaderOptions.AutoPlayAnimations = true;
                    assetLoader.LoadFromFile(filePath, assetLoaderOptions, null, delegate (GameObject loadedGameObject)
                    {
                        IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                        IProject project = IOC.Resolve<IProject>();
                        ProjectItem targetFolder = project.GetFolder(Path.GetDirectoryName(targetPath));

                        loadedGameObject.SetActive(false);
                        loadedGameObject.name = Path.GetFileName(targetPath);
                        

                       
                        Bounds bounds = ContentMeshUtil.CalculateBounds(loadedGameObject);
                        ContentMeshUtil.SetPivotPoint(loadedGameObject, bounds.center + Vector3.down * bounds.extents.y);
                        loadedGameObject = ContentMeshUtil.Flatten(loadedGameObject);

                        ExposeToEditor exposeToEditor = loadedGameObject.AddComponent<ExposeToEditor>();

                        exposeToEditor.AddColliders = false;
                        exposeToEditor.BoundsType = BoundsType.Custom;
                        exposeToEditor.CustomBounds = exposeToEditor.CalculateBounds();
                        exposeToEditor.CustomBounds.center = Vector3.up * exposeToEditor.CustomBounds.extents.y;

                        MeshCollider collider = loadedGameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = ContentMeshUtil.CreateColliderMesh(loadedGameObject.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray());

                        editor.CreatePrefab(targetFolder, exposeToEditor, true, assetItems =>
                        {
                            m_loaded = true;
                            UnityEngine.Object.Destroy(loadedGameObject);
                        });
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            yield return new WaitUntil(() => m_loaded);
        }



        private static AssetLoaderOptions DefaultAssetLoaderOptions(float scale)
        {
            var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
            assetLoaderOptions.RotationAngles = new Vector3(0, 0, 0);
            assetLoaderOptions.AddAssetUnloader = false;
            assetLoaderOptions.DontLoadCameras = true;
            assetLoaderOptions.DontLoadLights = true;
            assetLoaderOptions.DontLoadMaterials = false;
            assetLoaderOptions.DontLoadMetadata = false;
            assetLoaderOptions.AutoPlayAnimations = true;
            assetLoaderOptions.UseOriginalPositionRotationAndScale = true;
            //assetLoaderOptions.Use32BitsIndexFormat = true;
            assetLoaderOptions.Scale = scale;
            return assetLoaderOptions;
        }
    }

    public static class ContentMeshUtil
    {
        public static Mesh CreateColliderMesh(params GameObject[] gameObjects)
        {
            if (gameObjects == null)
            {
                throw new System.ArgumentNullException("gameObjects");
            }

            if (gameObjects.Length == 0)
            {
                return null;
            }

            GameObject target = gameObjects[0];

            //save parents and unparent selected objects
            Transform[] selectionParents = new Transform[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                selectionParents[i] = obj.transform.parent;
                obj.transform.SetParent(null, true);
            }

            Matrix4x4 targetRotationMatrix = Matrix4x4.TRS(Vector3.zero, target.transform.rotation, Vector3.one);// target.transform.localScale);
            Matrix4x4 targetToLocal = target.transform.worldToLocalMatrix;

            List<CombineInstance> colliderCombine = new List<CombineInstance>();
            List<Mesh> meshes = new List<Mesh>();
            foreach (GameObject obj in gameObjects)
            {
                MeshFilter f = obj.GetComponent<MeshFilter>();
                if (f != null && f.sharedMesh != null)
                {
                    meshes.AddRange(MeshUtils.Separate(f.sharedMesh));
                }
                for (int i = 0; i < meshes.Count; i++)
                {
                    Mesh mesh = meshes[i];
                    CombineInstance colliderCombineInstance = new CombineInstance();
                    colliderCombineInstance.mesh = mesh;
                    //convert to active selected object's local coordinate system
                    colliderCombineInstance.transform = targetToLocal * obj.transform.localToWorldMatrix;
                    colliderCombine.Add(colliderCombineInstance);
                }
                meshes.Clear();
            }

            Mesh finalColliderMesh = null;
            if (colliderCombine.Count != 0)
            {
                Mesh colliderMesh = new Mesh();
                colliderMesh.CombineMeshes(colliderCombine.ToArray());

                CombineInstance[] removeColliderRotation = new CombineInstance[1];
                removeColliderRotation[0].mesh = colliderMesh;
                removeColliderRotation[0].transform = targetRotationMatrix;

                finalColliderMesh = new Mesh();
                finalColliderMesh.name = target.name + "Collider";
                finalColliderMesh.CombineMeshes(removeColliderRotation);
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                obj.transform.SetParent(selectionParents[i], true);
            }

            return finalColliderMesh;
        }

        public static Bounds CalculateBounds(GameObject go, float minBoundsSize = 0.1f)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Vector3 scale = go.transform.localScale;
            go.transform.localScale = Vector3.one;

            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector2.one * minBoundsSize);
            }
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            go.transform.localScale = scale;
            return bounds;
        }

        public static void SetPivotPoint(GameObject go, Vector3 center)
        {
            GameObject pivot = new GameObject("PivotOffset");
            pivot.transform.SetParent(go.transform, false);
            pivot.transform.localPosition = center;

            for (int i = go.transform.childCount - 2; i >= 0; i--)
            {
                Transform child = go.transform.GetChild(i);
                child.transform.SetParent(pivot.transform, true);
            }

            pivot.transform.localPosition = Vector3.zero;
        }

        public static Mesh Transform(Mesh mesh, Matrix4x4 fromTransform, Matrix4x4 toTransform)
        {
            Vector3[] vertices = mesh.vertices;
            Matrix4x4 matrix = fromTransform * toTransform;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }

            mesh.vertices = vertices;
            return mesh;
        }

        public static GameObject Flatten(GameObject model)
        {
            MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters.Length == 1)
            {
                MeshFilter filter = meshFilters[0];
                filter.gameObject.SetActive(false);
                filter.transform.SetParent(null);

                Transform(filter.sharedMesh, filter.transform.localToWorldMatrix, Matrix4x4.identity);

                filter.transform.position = Vector3.zero;
                filter.transform.rotation = Quaternion.identity;
                filter.transform.localScale = Vector3.one;

                filter.sharedMesh.RecalculateBounds();

                UnityEngine.Object.Destroy(model);

                return filter.gameObject;
            }
            else
            {
                GameObject root = new GameObject();
                root.name = model.name;
                root.transform.position = model.transform.position;
                root.gameObject.SetActive(false);

                for (int i = 0; i < meshFilters.Length; ++i)
                {
                    MeshFilter filter = meshFilters[i];
                    Transform(filter.sharedMesh, filter.transform.localToWorldMatrix, root.transform.worldToLocalMatrix);
                    filter.transform.SetParent(root.transform, false);
                    filter.transform.position = Vector3.zero;
                    filter.transform.rotation = Quaternion.identity;
                    filter.transform.localScale = Vector3.one;
                    filter.sharedMesh.RecalculateBounds();
                }

                UnityEngine.Object.Destroy(model);

                root.transform.position = Vector3.zero;
                return root;
            }


        }
    }
}
*/