using Battlehub.MeshTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public static class RaycastHelper 
    {
        public static GameObject Raycast(Ray pointer)
        {
            RaycastHit hit;
            if (Physics.Raycast(pointer, out hit))
            {
                if (hit.collider.GetComponentsInChildren<Renderer>().Length > 0)
                {
                    return hit.collider.gameObject;
                }
            }
            return null;
        }


        public static int Raycast(Ray pointer, out Renderer result)
        {
            result = null;
            RaycastHit hit;
            if (!Physics.Raycast(pointer, out hit))
            {
                return -1;
            }

            int triangleIndex = -1;
            Mesh mesh = null;
            GameObject[] gameObjects = hit.collider.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
            foreach (GameObject obj in gameObjects)
            {
                MeshFilter f = obj.GetComponent<MeshFilter>();
                if (f == null || f.sharedMesh == null || f.sharedMesh.GetTopology(0) != MeshTopology.Triangles)
                {
                    continue;
                }

                int[] tris = f.sharedMesh.triangles;
                int range = tris.Length / 3;
                if (triangleIndex < hit.triangleIndex && hit.triangleIndex <= triangleIndex + range)
                {
                    mesh = f.sharedMesh;
                    result = f.GetComponent<Renderer>();
                    if (result != null)
                    {
                        break;
                    }
                }
                triangleIndex += range;
            }

            if (result == null)
            {
                return -1;
            }

            triangleIndex++;
            triangleIndex = hit.triangleIndex - triangleIndex;

            int lookupIdx0 = mesh.triangles[triangleIndex * 3];
            int lookupIdx1 = mesh.triangles[triangleIndex * 3 + 1];
            int lookupIdx2 = mesh.triangles[triangleIndex * 3 + 2];

            int subMeshCount = mesh.subMeshCount;
            for (int materialIdx = 0; materialIdx < subMeshCount; ++materialIdx)
            {
                int[] tris = mesh.GetTriangles(materialIdx);
                for (var t = 0; t < tris.Length; t += 3)
                {
                    if (tris[t] == lookupIdx0 && tris[t + 1] == lookupIdx1 && tris[t + 2] == lookupIdx2)
                    {
                        return materialIdx;
                    }
                }
            }

            Debug.Log("Triangle was not found");
            return -1;
        }

        public static int GetMaterialIndex(Ray pointer, out MeshRenderer result)
        {
            result = null;
            
            RayMeshIntersection intersection = new RayMeshIntersection();
            if(intersection.Raycast(pointer))
            {
                result = intersection.GameObject.GetComponent<MeshRenderer>();
                return intersection.SubmeshIndex;
            }

            return -1;
        }

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
                if ((obj.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }
                selectionParents[i] = obj.transform.parent;
                obj.transform.SetParent(null, true);
            }

            Matrix4x4 targetToLocal = target.transform.worldToLocalMatrix;

            List<CombineInstance> colliderCombine = new List<CombineInstance>();
            List<Mesh> meshes = new List<Mesh>();
            foreach (GameObject obj in gameObjects)
            {
                if((obj.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }

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
                removeColliderRotation[0].transform = Matrix4x4.identity;// targetRotationMatrix;

                finalColliderMesh = new Mesh();
                finalColliderMesh.name = target.name + "Collider";
                finalColliderMesh.CombineMeshes(removeColliderRotation);
            }

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject obj = gameObjects[i];
                if ((obj.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }
                obj.transform.SetParent(selectionParents[i], true);
            }

            return finalColliderMesh;
        }

        //based on https://github.com/garettbass/UnityExtensions.MeshIntersection
        public struct RayMeshIntersection
        {
            public GameObject GameObject;
            public float Distance;
            public Vector3 Position;
            public Vector3 Normal;
            public int SubmeshIndex;
            public int Triangle;
            public bool Found;
            public void Reset() { this = new RayMeshIntersection(); }
            public bool Raycast(Ray worldRay)
            {
                var meshRenderers = Object.FindObjectsOfType<MeshRenderer>();
                return Raycast(worldRay, meshRenderers);
            }

            public bool Raycast(Ray worldRay, IEnumerable<MeshRenderer> meshRenderers)
            {
                Reset();
                var meshIntersection = default(RayMeshIntersection);
                var nearestDistance = Mathf.Infinity;
                foreach (var meshRenderer in meshRenderers)
                {
                    if (meshIntersection.Raycast(worldRay, meshRenderer) &&
                        meshIntersection.Distance < nearestDistance)
                    {
                        nearestDistance = meshIntersection.Distance;
                        this = meshIntersection;
                    }
                }
                return Found;
            }

            public bool Raycast(Ray worldRay, MeshRenderer meshRenderer)
            {
                if (meshRenderer.bounds.IntersectRay(worldRay))
                {
                    MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    return Raycast(worldRay, meshFilter);
                }
                Reset();
                return false;
            }

            public bool Raycast(Ray worldRay, MeshFilter meshFilter)
            {
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    Transform meshTransform = meshFilter.transform;
                    if (Raycast(worldRay, mesh, meshTransform))
                    {
                        GameObject = meshFilter.gameObject;
                        return true;
                    }
                }
                Reset();
                return false;
            }

            public bool Raycast(Ray worldRay, Mesh mesh, Transform meshTransform)
            {
                if (meshTransform != null)
                {
                    var meshMatrix = meshTransform.localToWorldMatrix;
                    return Raycast(worldRay, mesh, meshMatrix);
                }
                Reset();
                return false;
            }

            public bool Raycast(Ray worldRay, Mesh mesh, Matrix4x4 meshMatrix)
            {
                Reset();
                Distance = Mathf.Infinity;
                Vector3 meshScale = meshMatrix.lossyScale;
                float normalScale =
                    (meshScale.x * meshScale.y * meshScale.z) < 0
                    ? -1f : +1f;

                var vertices = mesh.vertices;
                for (int s = 0; s < mesh.subMeshCount; ++s)
                {
                    var indices = mesh.GetTriangles(s);
                    var n = indices.Length;
                    for (int i = 0; i < n;)
                    {
                        var tri = i;
                        var i0 = indices[i++];
                        var i1 = indices[i++];
                        var i2 = indices[i++];

                        var v0 = vertices[i0];
                        var v1 = vertices[i1];
                        var v2 = vertices[i2];

                        v0 = meshMatrix.MultiplyPoint(v0);
                        v1 = meshMatrix.MultiplyPoint(v1);
                        v2 = meshMatrix.MultiplyPoint(v2);

                        var plane = new Plane(v0, v1, v2);

                        if (!plane.Raycast(worldRay, out float hitDistance))
                        {
                            continue;
                        }

                        if (hitDistance < 0 || hitDistance > Distance)
                        {
                            continue;
                        }

                        var hitPosition = worldRay.GetPoint(hitDistance);
                        var r = hitPosition - v0;

                        var edge0 = v2 - v0;
                        var edge1 = v1 - v0;

                        var dot00 = Vector3.Dot(edge0, edge0);
                        var dot01 = Vector3.Dot(edge0, edge1);
                        var dot11 = Vector3.Dot(edge1, edge1);

                        var coeff = 1f / (dot00 * dot11 - dot01 * dot01);

                        var dot02 = Vector3.Dot(edge0, r);
                        var dot12 = Vector3.Dot(edge1, r);

                        var u = coeff * (dot11 * dot02 - dot01 * dot12);
                        var v = coeff * (dot00 * dot12 - dot01 * dot02);

                        if ((u >= 0) && (v >= 0) && ((u + v) < 1))
                        {
                            Found = true;
                            Distance = hitDistance;
                            Position = hitPosition;
                            Normal = plane.normal * normalScale;
                            Triangle = tri;
                            SubmeshIndex = s;
                        }
                    }
                }

                return Found;
            }
        }
    }
}

