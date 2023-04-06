using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public static class PBSmoothing
    {
        public const int smoothingGroupNone = 0;
        public const int smoothRangeMin = 1;
        public const int smoothRangeMax = 24;

        public static void ApplySmoothingGroup(PBMesh pbMesh, MeshSelection selection, float angleThreshold)
        {
            selection = selection.ToFaces(false);

            ProBuilderMesh mesh = pbMesh.ProBuilderMesh;
            IList<Face> faces = new List<Face>();
            
            mesh.GetFaces(selection.GetFaces(pbMesh).ToArray(), faces);
            Smoothing.ApplySmoothingGroups(mesh, faces, angleThreshold);
        }

        public static void SetGroup(PBMesh pbMesh, MeshSelection selection,  int index)
        {
            selection = selection.ToFaces(false);

            ProBuilderMesh mesh = pbMesh.ProBuilderMesh;
            IList<Face> faces = new List<Face>();
            mesh.GetFaces(selection.GetFaces(pbMesh).ToArray(), faces);

            foreach (Face face in faces)
            {
                face.smoothingGroup = index;
            }

            mesh.ToMesh();
            mesh.Refresh();
        }

        public static void ClearGroup(PBMesh pbMesh, MeshSelection selection)
        {
            SetGroup(pbMesh, selection, smoothingGroupNone);
        }

        public static MeshSelection ExpandSelection(MeshSelection selection)
        {
            selection = selection.ToFaces(false);

            foreach (PBMesh pbMesh in selection.GetSelectedMeshes().ToArray())
            {
                ProBuilderMesh mesh = pbMesh.ProBuilderMesh;
                IList<Face> selectedFaces = new List<Face>();
                mesh.GetFaces(selection.GetFaces(pbMesh).ToArray(), selectedFaces);

                HashSet<int> groupsHs = new HashSet<int>();
                for(int i = 0; i < selectedFaces.Count; ++i)
                {
                    Face face = selectedFaces[i];
                    if(!groupsHs.Contains(face.smoothingGroup))
                    {
                        groupsHs.Add(face.smoothingGroup);
                    }
                }


                IList<int> selectedIndices = new List<int>();
                IList<Face> faces = mesh.faces;
                for(int i = 0; i < faces.Count; ++i)
                {
                    Face face = faces[i];
                    if(groupsHs.Contains(face.smoothingGroup))
                    {
                        selectedIndices.Add(i);
                    }
                }

                selection.SelectedFaces[pbMesh.gameObject] = selectedIndices;
            }

            return selection;
        }
    }


}

