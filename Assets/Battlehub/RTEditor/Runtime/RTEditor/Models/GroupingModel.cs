using Battlehub.MeshTools;
using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor.Models
{
    [DefaultExecutionOrder(-100)]
    public class GroupingModel : MonoBehaviour, IGroupingModel
    {
        protected virtual void Awake()
        {
            if (!IOC.IsFallbackRegistered<IGroupingModel>())
            {
                IOC.RegisterFallback<IGroupingModel>(this);
            }
        }
        protected virtual void OnDestroy()
        {
            IOC.UnregisterFallback<IGroupingModel>(this);
        }

        public virtual bool CanGroup(GameObject[] gameObjects)
        {
            return gameObjects != null && gameObjects.Length > 0;
        }

        public virtual void GroupAndRecord(GameObject[] gameObjects, string groupName, bool local = false)
        {
            if (!CanGroup(gameObjects))
            {
                throw new ArgumentException("Unable to group game objects", "gameObjects");
            }

            IRTE editor = IOC.Resolve<IRTE>();
            IRuntimeUndo undo = editor.Undo;
            IRuntimeSelection selection = editor.Selection;
            RuntimeTools tools = editor.Tools;
            undo.BeginRecord();
            try
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.BeginRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                GameObject group = Group(gameObjects, groupName, tools.PivotMode == RuntimePivotMode.Pivot ? selection.activeTransform : null, local ? selection.activeTransform.parent : null);
                undo.RegisterCreatedObjects(new[] { group.GetComponent<ExposeToEditor>() });

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.EndRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                selection.activeObject = group;
            }
            finally
            {
                undo.EndRecord();
            }
        }

        public virtual bool CanUngroup(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                if (CanUngroup(gameObjects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void UngroupAndRecord(GameObject[] groups)
        {
            if (!CanUngroup(groups))
            {
                throw new ArgumentException("Unable to ungroup game objects", "gameObjects");
            }

            IRTE editor = IOC.Resolve<IRTE>();
            IRuntimeUndo undo = editor.Undo;
            IRuntimeSelection selection = editor.Selection;

            undo.BeginRecord();
            try
            {
                GameObject[] gameObjects = BeginUngroup(groups);
                selection.objects = gameObjects;

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTransform = gameObjects[i].transform;
                    undo.BeginRecordTransform(goTransform, goTransform.parent, goTransform.GetSiblingIndex());
                }

                EndUngroup(groups);

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTransform = gameObjects[i].transform;
                    undo.EndRecordTransform(goTransform, goTransform.parent, goTransform.GetSiblingIndex());
                }

                
                undo.DestroyObjects(groups.Select(group => group.GetComponent<ExposeToEditor>()).ToArray());

            }
            finally
            {
                undo.EndRecord();
            }
        }

        public virtual bool CanMerge(GameObject[] gameObjects)
        {
            if (gameObjects == null)
            {
                return false;
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                MeshFilter meshFilter = gameObjects[i].GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void MergeAndRecord(GameObject[] gameObjects)
        {
            IRTE editor = IOC.Resolve<IRTE>();
            IRuntimeUndo undo = editor.Undo;
            IRuntimeSelection selection = editor.Selection;
            RuntimeTools tools = editor.Tools;
            undo.BeginRecord();
            try
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.BeginRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                GameObject merged = Merge(gameObjects, tools.PivotMode == RuntimePivotMode.Pivot ? selection.activeGameObject : null);

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.EndRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                undo.DestroyObjects(gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).ToArray());
                undo.RegisterCreatedObjects(new[] { merged.GetComponent<ExposeToEditor>() });
                selection.activeObject = merged;
            }
            finally
            {
                undo.EndRecord();
            }
        }

        public virtual void MergeOptimizedAndRecord(GameObject[] gameObjects)
        {
            IRTE rte = IOC.Resolve<IRTE>();
            IRuntimeUndo undo = rte.Undo;
            IRuntimeSelection selection = rte.Selection;
            RuntimeTools tools = rte.Tools;
            try
            {
                undo.BeginRecord();

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.BeginRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                CombineResult result = MergeOptimized(gameObjects, tools.PivotMode == RuntimePivotMode.Pivot ? selection.activeGameObject : null);
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    Transform goTranform = gameObjects[i].transform;
                    undo.EndRecordTransform(goTranform, goTranform.parent, goTranform.GetSiblingIndex());
                }

                undo.DestroyObjects(gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).ToArray());
                undo.RegisterCreatedObjects(new[] { result.GameObject.GetComponent<ExposeToEditor>() });
                selection.activeGameObject = result.GameObject;
            }
            finally
            {
                undo.EndRecord();
            }
        }

        public virtual bool CanSeparate(GameObject[] gameObjects)
        {
            if(gameObjects == null)
            {
                return false;
            }

            for(int i = 0; i < gameObjects.Length; ++i)
            {
                MeshFilter meshFilter = gameObjects[i].GetComponent<MeshFilter>();
                if(meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.subMeshCount > 1)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void SeparateAndRecord(GameObject[] gameObjects)
        {
            IRuntimeUndo undo = IOC.Resolve<IRTE>().Undo;
            undo.BeginRecord();
            try
            {
                GameObject[] targetClones = Separate(gameObjects, false);
                foreach(GameObject targetClone in targetClones)
                {
                    targetClone.SetActive(true);
                }
                undo.RegisterCreatedObjects(targetClones.Select(go => go.GetComponent<ExposeToEditor>()).ToArray());
                undo.DestroyObjects(gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).ToArray());

                IRuntimeSelection selection = IOC.Resolve<IRTE>().Selection;
                selection.objects = targetClones;
            }
            finally
            {
                undo.EndRecord();
            }
        }

        protected virtual GameObject Group(GameObject[] gameObjects, string groupName, Transform center, Transform parent)
        {
            if (!CanGroup(gameObjects))
            {
                throw new ArgumentException("Unable to group game objects", "gameObjects");
            }

            GameObject group = new GameObject();
            group.name = groupName;
            group.gameObject.AddComponent<ExposeToEditor>();
            if(parent != null)
            {
                group.transform.SetParent(parent, true);
            }

            if (center != null)
            {
                group.transform.position = center.position;
            }
            else
            {
                group.transform.position = TransformUtility.GetCommonCenter(gameObjects.Select(go => go.transform).ToArray());
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                gameObjects[i].transform.SetParent(group.transform, true);
            }

            return group;
        }

        protected virtual bool CanUngroup(GameObject gameObject)
        {
            int length = gameObject.GetComponents<Component>().Length;

            return length == 1 || length == 2 && gameObject.GetComponent<ExposeToEditor>() != null;
        }

        protected virtual GameObject[] BeginUngroup(GameObject[] groups)
        {
            return Ungroup(groups, false);
        }

        protected virtual GameObject[] EndUngroup(GameObject[] groups)
        {
            return Ungroup(groups, true);
        }

        protected virtual GameObject[] Ungroup(GameObject[] groups, bool ungroup = true)
        {
            if (!CanUngroup(groups))
            {
                throw new ArgumentException("Unable to ungroup game objects", "gameObjects");
            }

            List<GameObject> ungroupedList = new List<GameObject>();
            groups = groups.OrderBy(go => go.transform.CalculateDepth()).ToArray();

            for (int i = 0; i < groups.Length; ++i)
            {
                GameObject go = groups[i];
                if (CanUngroup(go))
                {
                    for (int j = go.transform.childCount - 1; j >= 0; j--)
                    {
                        Transform child = go.transform.GetChild(j);
                        if (ungroup)
                        {
                            child.transform.SetParent(go.transform.parent, true);
                        }
                        ungroupedList.Add(child.gameObject);
                    }
                }
            }

            return ungroupedList.ToArray();
        }

        protected virtual GameObject Merge(GameObject[] gameObjects, GameObject center = null)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                throw new ArgumentNullException("gameObjects");
            }

            GameObject target = center != null ? center : gameObjects[0];
            //save parents and unparent selected objects
            Transform[] parents = new Transform[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                parents[i] = obj.transform.parent;
                obj.transform.SetParent(null, true);
            }

            //GameObject targetClone = Instantiate(target);
            //MeshUtils.DestroyChildren(targetClone);

            //destroy colliders
            //Collider[] targetCloneColliders = targetClone.GetComponents<Collider>();
            //foreach (Collider collider in targetCloneColliders)
            //{
            //    Destroy(collider);
            //}

            GameObject targetClone = new GameObject();
            targetClone.name = target.name;
            targetClone.gameObject.AddComponent<ExposeToEditor>();

            string name = target.name;
            targetClone.name = name;

            Matrix4x4 targetRotationMatrix;
            Matrix4x4 targetToLocal;
            if (center != null)
            {
                targetRotationMatrix = Matrix4x4.TRS(Vector3.zero, target.transform.rotation, target.transform.localScale);
                targetToLocal = targetClone.transform.worldToLocalMatrix;
            }
            else
            {
                targetClone.transform.localScale = Vector3.one;
                targetClone.transform.rotation = Quaternion.identity;
                targetClone.transform.position = TransformUtility.GetCommonCenter(gameObjects.Select(go => go.transform).ToArray());
                targetRotationMatrix = Matrix4x4.identity;
                targetToLocal = Matrix4x4.TRS(-targetClone.transform.position, Quaternion.identity, Vector3.one);
            }

            //find all MeshFilters and SkinnedMeshRenderers
            List<MeshFilter> allMeshFilters = new List<MeshFilter>();
            List<SkinnedMeshRenderer> allSkinned = new List<SkinnedMeshRenderer>();

            foreach (GameObject obj in gameObjects)
            {
                MeshFilter[] filters = obj.GetComponents<MeshFilter>();
                allMeshFilters.AddRange(filters);

                SkinnedMeshRenderer[] skinned = obj.GetComponents<SkinnedMeshRenderer>();
                allSkinned.AddRange(skinned);
            }

            List<Material> materials = new List<Material>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            int vertexCount = 0;
            for (int i = 0; i < allMeshFilters.Count; ++i)
            {
                MeshFilter meshFilter = allMeshFilters[i];
                Mesh[] meshes = MeshUtils.Separate(meshFilter.sharedMesh);

                Material[] sharedMaterials;
                Renderer renderer = meshFilter.GetComponent<Renderer>();
                if (renderer == null)
                {
                    sharedMaterials = new Material[meshes.Length];
                }
                else
                {
                    sharedMaterials = renderer.sharedMaterials;
                    Array.Resize(ref sharedMaterials, meshes.Length);
                }

                for (int m = 0; m < sharedMaterials.Length; ++m)
                {
                    Material material = sharedMaterials[m];
                    materials.Add(material);

                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = meshes[m];
                    combineInstance.transform = targetToLocal * meshFilter.transform.localToWorldMatrix;
                    combineInstances.Add(combineInstance);

                    vertexCount += combineInstance.mesh.vertexCount;
                }
            }

            Mesh mesh = new Mesh();
            if (vertexCount > 65534)
            {
                mesh.indexFormat = IndexFormat.UInt32;
            }
            mesh.CombineMeshes(combineInstances.ToArray(), false);
            for (int i = 0; i < combineInstances.Count; ++i)
            {
                Destroy(combineInstances[i].mesh);
            }

            Mesh finalMesh = MeshUtils.RemoveRotation(mesh, targetRotationMatrix, false);
            finalMesh.name = name;

            Destroy(mesh);

            MeshFilter meshFiter = targetClone.GetComponent<MeshFilter>();
            if (meshFiter == null)
            {
                meshFiter = targetClone.AddComponent<MeshFilter>();
            }
            meshFiter.sharedMesh = finalMesh;

            MeshRenderer meshRenderer = targetClone.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = targetClone.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterials = materials.ToArray();

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                obj.transform.SetParent(parents[i], true);
            }

            ExposeToEditor exposeToEditor = targetClone.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null)
            {
                if (exposeToEditor.AddColliders && exposeToEditor.Colliders != null && exposeToEditor.Colliders.Length > 0)
                {
                    MeshCollider collider = exposeToEditor.Colliders[0] as MeshCollider;
                    if (collider != null)
                    {
                        collider.sharedMesh = finalMesh;
                    }
                }
            }
            else
            {
                MeshCollider targetCollider = targetClone.GetComponent<MeshCollider>();
                if (targetCollider == null)
                {
                    targetCollider = targetClone.AddComponent<MeshCollider>();
                }

                Rigidbody rigidBody = targetClone.GetComponent<Rigidbody>();
                if (rigidBody != null)
                {
                    targetCollider.sharedMesh = finalMesh;
                    if (!rigidBody.isKinematic)
                    {
                        targetCollider.convex = true;
                    }
                }
                else
                {
                    targetCollider.sharedMesh = finalMesh;
                }
            }

            return targetClone;
        }

        protected virtual CombineResult MergeOptimized(GameObject[] gameObjects, GameObject center)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                throw new ArgumentException("gameObjects", "gameObjects");
            }

            GameObject target = center != null ? center : gameObjects[0];

            //save parents and unparent selected objects
            Transform[] selectionParents = new Transform[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                selectionParents[i] = obj.transform.parent;
                obj.transform.SetParent(null, true);
            }


            //duplicate target and remove it's children
            //GameObject targetClone = Instantiate(target);
            //targetClone.name = target.name;
            //MeshUtils.DestroyChildren(targetClone);

            GameObject targetClone = new GameObject();
            targetClone.name = target.name;
            targetClone.gameObject.AddComponent<ExposeToEditor>();

            Matrix4x4 targetRotationMatrix;
            Matrix4x4 targetToLocal;
            if (center != null)
            {
                targetRotationMatrix = Matrix4x4.TRS(Vector3.zero, target.transform.rotation, target.transform.localScale);
                targetToLocal = targetClone.transform.worldToLocalMatrix;
            }
            else
            {
                targetClone.transform.localScale = Vector3.one;
                targetClone.transform.rotation = Quaternion.identity;
                targetClone.transform.position = TransformUtility.GetCommonCenter(gameObjects.Select(go => go.transform).ToArray());
                targetRotationMatrix = Matrix4x4.identity;
                targetToLocal = Matrix4x4.TRS(-targetClone.transform.position, Quaternion.identity, Vector3.one);
            }

            //find all MeshFilters and SkinnedMeshRenderers
            List<MeshFilter> allMeshFilters = new List<MeshFilter>();
            List<SkinnedMeshRenderer> allSkinned = new List<SkinnedMeshRenderer>();

            foreach (GameObject obj in gameObjects)
            {
                MeshFilter[] filters = obj.GetComponents<MeshFilter>();
                allMeshFilters.AddRange(filters);

                SkinnedMeshRenderer[] skinned = obj.GetComponents<SkinnedMeshRenderer>();
                allSkinned.AddRange(skinned);
            }

            //deactivate original object                  
            //target.SetActive(false);

            //combine colliders
            List<CombineInstance> colliderCombine = new List<CombineInstance>();

            foreach (GameObject obj in gameObjects)
            {
                List<Mesh> meshes = MeshUtils.GetColliderMeshes(obj);
                for (int i = 0; i < meshes.Count; i++)
                {
                    Mesh mesh = meshes[i];
                    CombineInstance colliderCombineInstance = new CombineInstance();
                    colliderCombineInstance.mesh = mesh;
                    //convert to active selected object's local coordinate system
                    colliderCombineInstance.transform = targetToLocal * obj.transform.localToWorldMatrix;
                    colliderCombine.Add(colliderCombineInstance);
                }
            }

            //copy original object name
            string name = target.name;
            targetClone.name = name;

            if (colliderCombine.Count != 0)
            {
                Mesh colliderMesh = new Mesh();
                colliderMesh.CombineMeshes(colliderCombine.ToArray());
                for(int i = 0; i < colliderCombine.Count; ++i)
                {
                    Destroy(colliderCombine[i].mesh);
                }

                CombineInstance[] removeColliderRotation = new CombineInstance[1];
                removeColliderRotation[0].mesh = colliderMesh;
                removeColliderRotation[0].transform = targetRotationMatrix;

                Mesh finalColliderMesh = new Mesh();
                finalColliderMesh.name = name + "Collider";
                finalColliderMesh.CombineMeshes(removeColliderRotation);

                Destroy(colliderMesh);

                ExposeToEditor exposeToEditor = targetClone.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null)
                {
                    if (exposeToEditor.AddColliders && exposeToEditor.Colliders != null && exposeToEditor.Colliders.Length > 0)
                    {
                        MeshCollider collider = exposeToEditor.Colliders[0] as MeshCollider;
                        if (collider != null)
                        {
                            collider.sharedMesh = finalColliderMesh;
                        }
                    }
                }
                else
                {
                    MeshCollider targetCollider = targetClone.GetComponent<MeshCollider>();
                    if (targetCollider == null)
                    {
                        targetCollider = targetClone.AddComponent<MeshCollider>();
                    }
                    Rigidbody rigidBody = targetClone.GetComponent<Rigidbody>();
                    if (rigidBody != null)
                    {
                        targetCollider.sharedMesh = finalColliderMesh;
                        if (!rigidBody.isKinematic)
                        {
                            targetCollider.convex = true;
                        }
                    }
                    else
                    {
                        targetCollider.sharedMesh = finalColliderMesh;
                    }
                }
            }


            CombineInstance[] meshCombine;
            Material[] materials;
            bool merge = MeshUtils.BuildCombineInstance(targetToLocal, allMeshFilters, allSkinned, out meshCombine, out materials);

            Mesh intermediateMesh = new Mesh();
            intermediateMesh.name = name;
            intermediateMesh.indexFormat = IndexFormat.UInt32;
            intermediateMesh.CombineMeshes(meshCombine, merge);

            //then remove rotation
            Mesh finalMesh = MeshUtils.RemoveRotation(intermediateMesh, targetRotationMatrix, merge);
            finalMesh.name = name;

            Destroy(intermediateMesh);

            targetClone.transform.rotation = Quaternion.identity;
            targetClone.transform.localScale = Vector3.one;

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                obj.transform.SetParent(selectionParents[i], true);
            }

            //restore parents
            if (target.transform.parent != null && target.transform.parent.gameObject.activeInHierarchy)
            {
                targetClone.transform.SetParent(target.transform.parent);
            }

            SkinnedMeshRenderer skinnedMeshRenderer = targetClone.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(skinnedMeshRenderer);
                }
                else
                {
                    DestroyImmediate(skinnedMeshRenderer);
                }
            }

            MeshFilter meshFiter = targetClone.GetComponent<MeshFilter>();
            if (meshFiter == null)
            {
                meshFiter = targetClone.AddComponent<MeshFilter>();
            }
            meshFiter.sharedMesh = finalMesh;

            MeshRenderer meshRenderer = targetClone.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = targetClone.AddComponent<MeshRenderer>();
            }
            meshRenderer.sharedMaterials = materials;

            return new CombineResult(targetClone, finalMesh);
        }

        protected virtual GameObject[] Separate(GameObject[] targets, bool activate)
        {
            targets = targets.OrderByDescending(target => target.transform.CalculateDepth()).ToArray();

            GameObject[] targetClones = new GameObject[targets.Length];

            for (int i = 0; i < targets.Length; ++i)
            {
                targetClones[i] = Separate(targets[i], activate);
            }

            return targetClones;
        }

        protected virtual GameObject Separate(GameObject target, bool activate)
        {
            GameObject targetClone = new GameObject();
            targetClone.gameObject.SetActive(activate);
            targetClone.name = target.name;
            targetClone.transform.position = target.transform.position;
            targetClone.transform.rotation = target.transform.rotation;
            targetClone.transform.localScale = target.transform.localScale;
            targetClone.AddComponent<ExposeToEditor>();


            MeshFilter filter = target.GetComponent<MeshFilter>();
            if (filter == null)
            {
                return targetClone;
            }

            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            Material[] materials = null;
            if (renderer != null)
            {
                materials = renderer.sharedMaterials;
            }

            Mesh mesh = filter.sharedMesh;
            Mesh[] submeshes = MeshUtils.Separate(mesh);

            for (int i = 0; i < submeshes.Length; ++i)
            {
                GameObject part = new GameObject();
                
                part.name = $"Part {i}";
                part.transform.SetParent(targetClone.transform, false);

                MeshFilter partFilter = part.AddComponent<MeshFilter>();
                partFilter.sharedMesh = submeshes[i];

                MeshRenderer partRenderer = part.AddComponent<MeshRenderer>();
                if (materials != null && i < materials.Length)
                {
                    partRenderer.sharedMaterial = materials[i];
                }
                else
                {
                    partRenderer.sharedMaterial = RenderPipelineInfo.DefaultMaterial;
                }

                Bounds bounds = part.CalculateBounds();
                Vector3 pivotOffset = bounds.center - targetClone.transform.position;
                pivotOffset = targetClone.transform.InverseTransformVector(pivotOffset);

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = partFilter.sharedMesh;
                combineInstance.transform = Matrix4x4.TRS(-pivotOffset, Quaternion.identity, Vector3.one);

                Mesh result = new Mesh();
                result.name = combineInstance.mesh.name;
                result.CombineMeshes(new[] { combineInstance });
                Destroy(combineInstance.mesh);
                partFilter.sharedMesh = result;
                part.transform.localPosition += pivotOffset;

                part.AddComponent<ExposeToEditor>();
            }

            return targetClone;
        }
    }

}
