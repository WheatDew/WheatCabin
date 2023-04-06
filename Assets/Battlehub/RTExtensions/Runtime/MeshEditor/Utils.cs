using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.MeshTools
{
    public static class Utils
    {
        public enum CullingMode
        {
            Front,
            Back
        }

        public struct RaycastHit
        {
            public float Distance;
            public Vector3 Point;
            public Vector3 Normal;
            public int Index;

            public RaycastHit(float distance, Vector3 point, Vector3 normal, int index)
            {
                Distance = distance;
                Point = point;
                Normal = normal;
                Index = index;
            }
        }

        public static bool FaceRaycast(Ray worldRay, Mesh mesh, Transform meshTransform, out RaycastHit hit, CullingMode cullingMode = CullingMode.Back)
        {
            // Transform ray into model space
            worldRay.origin -= meshTransform.position; // Why doesn't worldToLocalMatrix apply translation?
            worldRay.origin = meshTransform.worldToLocalMatrix * worldRay.origin;
            worldRay.direction = meshTransform.worldToLocalMatrix * worldRay.direction;

            Vector3[] positions = mesh.vertices;
            int[] indexes = mesh.triangles;

            float outDistance = Mathf.Infinity;
            int outHitFace = -1;
            Vector3 outNrm = Vector3.zero;
            
            for (int i = 0; i < indexes.Length; i += 3)
            {

                Vector3 a = positions[indexes[i + 0]];
                Vector3 b = positions[indexes[i + 1]];
                Vector3 c = positions[indexes[i + 2]];

                Vector3 nrm = Vector3.Cross(b - a, c - a);
                float dot = Vector3.Dot(worldRay.direction, nrm);

                bool skip = false;

                switch (cullingMode)
                {
                    case CullingMode.Front:
                        if (dot < 0f) skip = true;
                        break;

                    case CullingMode.Back:
                        if (dot > 0f) skip = true;
                        break;
                }

                var dist = 0f;

                Vector3 point;
                if (!skip && MathHelper.RayIntersectsTriangle(worldRay, a, b, c, out dist, out point))
                {
                    if (dist > outDistance || dist > outDistance)
                    {
                        continue;
                    }

                    outNrm = nrm;
                    outHitFace = i;
                    outDistance = dist;
                }
            }

            hit = new RaycastHit(outDistance,
                    worldRay.GetPoint(outDistance),
                    outNrm,
                    outHitFace);

            return outHitFace > -1;
        }

    }
}
