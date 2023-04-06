using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public partial class PBUVEditing
    {
        private static MethodInfo m_methodInfo;
        static PBUVEditing()
        {
            Type type = typeof(ProBuilderMesh).Assembly.GetType("UnityEngine.ProBuilder.MeshOperations.UVEditing");
            if (type == null)
            {
                Debug.LogWarning("AutoStitch is not supported");
                return;
            }

            m_methodInfo = type.GetMethod("AutoStitch", BindingFlags.Public | BindingFlags.Static);
            if (m_methodInfo == null)
            {
                Debug.LogWarning("AutoStitch method was not found");
                return;
            }
        }

        /// <summary>
        /// Provided two faces, this method will attempt to project @f2 and align its size, rotation, and position to match
        /// the shared edge on f1.  
        /// </summary>
        public static void AutoStitch(PBMesh mesh, int f1, int f2, int channel)
        {          
            if(m_methodInfo == null)
            {
                return;
            }
           
            ProBuilderMesh pbMesh = mesh.ProBuilderMesh;
            IList<Face> faces = new List<Face>(2);
            pbMesh.GetFaces(new List<int> { f1, f2 }, faces);

            m_methodInfo.Invoke(null, new object[] { pbMesh, faces[0], faces[1], channel });
        }
    }
}
