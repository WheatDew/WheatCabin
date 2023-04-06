using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.MeshTools
{
    public static partial class MeshUtils
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

            //combine colliders
            List<CombineInstance> colliderCombine = new List<CombineInstance>();
            List<Mesh> meshes = new List<Mesh>();
            foreach (GameObject obj in gameObjects)
            {
                MeshFilter f = obj.GetComponent<MeshFilter>();
                if (f != null && f.sharedMesh != null)
                {
                    meshes.AddRange(Separate(f.sharedMesh));
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

                for(int i = 0; i < colliderCombine.Count; ++i)
                {
                    Object.Destroy(colliderCombine[i].mesh);
                }

                CombineInstance[] removeColliderRotation = new CombineInstance[1];
                removeColliderRotation[0].mesh = colliderMesh;
                removeColliderRotation[0].transform = targetRotationMatrix;

                finalColliderMesh = new Mesh(); 
                finalColliderMesh.name = target.name + "Collider";
                finalColliderMesh.CombineMeshes(removeColliderRotation);

                Object.DestroyImmediate(colliderMesh);
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                obj.transform.SetParent(selectionParents[i], true);
            }

            return finalColliderMesh;
        }
    }
}
