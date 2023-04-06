using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public class PBFaceEditor :  PBBaseEditor
    {
        private PBFaceSelection m_faceSelection;

        private int[][] m_initialIndexes;
        private Vector3[][] m_initialPositions;
        private Vector3 m_initialPostion;
        private Quaternion m_initialRotation;
        
        public override bool HasSelection
        {
            get { return m_faceSelection.FacesCount > 0; }
        }

        public override Vector3 Position
        {
            get
            {
                if(UVEditingMode)
                {
                    return m_faceSelection.LastPosition;
                }

                return CenterMode ? m_faceSelection.CenterOfMass : m_faceSelection.LastPosition;
            }
            set { MoveTo(value); }
        }

        public override Vector3 Normal
        {
            get
            {
                if(UVEditingMode)
                {
                    return m_faceSelection.LastNormal;
                }

                return (CenterMode && m_faceSelection.FacesCount > 1) ? Vector3.forward : m_faceSelection.LastNormal;
            }
        }    
        
        public override Quaternion Rotation
        {
            get
            {
                if(m_faceSelection.LastMesh == null || !UVEditingMode && CenterMode && m_faceSelection.FacesCount > 1)
                {
                    return Quaternion.identity;
                }

                MeshSelection selection = GetSelection();
                if(selection == null)
                {
                    return Quaternion.identity;
                }
                IList<int> faces;
                if(!selection.SelectedFaces.TryGetValue(m_faceSelection.LastMesh.gameObject, out faces))
                {
                    return Quaternion.identity;
                }

                if(faces == null || faces.Count == 0)
                {
                    return Quaternion.identity;
                }

                IList<int> distinctIndexes = m_faceSelection.LastMesh.faces[faces.Last()].distinctIndexes;
                return HandleUtility.GetRotation(m_faceSelection.LastMesh, distinctIndexes);
                //return HandleUtility.GetFaceRotation(m_faceSelection.LastMesh, HandleOrientation.ActiveElement, new[] { m_faceSelection.LastMesh.faces[faces.Last()] });
            }
        }

        public override GameObject Target
        {
            get { return m_faceSelection.LastMesh != null ? m_faceSelection.LastMesh.gameObject : null; }
        }

        private void Awake()
        {
            m_faceSelection = gameObject.AddComponent<PBFaceSelection>();
        }

        private void OnDestroy()
        {
            if(m_faceSelection != null)
            {
                Destroy(m_faceSelection);
            }
        }

        public override void Hover(Camera camera, Vector3 pointer)
        {

        }

        private bool m_isMoveInProgress;
        private Dictionary<ProBuilderMesh, IList<Face>> m_meshes;
        public override void BeginMove()
        {
            m_isMoveInProgress = true;
            m_meshes = new Dictionary<ProBuilderMesh, IList<Face>>();
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = new List<Face>();
                mesh.GetFaces(m_faceSelection.GetFaces(mesh), faces);
                m_meshes.Add(mesh, faces);
            }

            if(UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = false;
            }
        }

        public override void EndMove()
        {
            m_isMoveInProgress = false;
            m_meshes = null;

            if (UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = true;
            }

#if PROBUILDER_4_4_0_OR_NEWER
            m_faceSelection.Meshes.RefreshColliders();
#endif
        }

        private void MoveTo(Vector3 to)
        {
            if (m_faceSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 from = Position;
            Vector3 offset = to - from;

            bool wasMoveInProgress = m_isMoveInProgress;
            if(!wasMoveInProgress)
            {
                BeginMove();
            }

            foreach(KeyValuePair<ProBuilderMesh, IList<Face>> kvp in m_meshes)
            {
                ProBuilderMesh mesh = kvp.Key;
                Vector3 localOffset = mesh.transform.InverseTransformVector(offset);

                IList<Face> faces = kvp.Value;
                mesh.TranslateVertices(faces, localOffset);

                mesh.ToMesh();
                mesh.Refresh();
            }
            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass + offset,
                m_faceSelection.LastPosition + offset);

            RaisePBMeshesChanged(true);

            if(!wasMoveInProgress)
            {
                EndMove();
            }
        }

        public override void BeginRotate(Quaternion initialRotation)
        {
            m_initialPostion = m_faceSelection.LastPosition;
            m_initialPositions = new Vector3[m_faceSelection.MeshesCount][];
            m_initialIndexes = new int[m_faceSelection.MeshesCount][];
            m_initialRotation = Quaternion.Inverse(initialRotation);

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                int[] indexes = m_faceSelection.GetIndexes(mesh).ToArray();
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_faceSelection.GetIndexes(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }

            if (UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = false;
            }
        }

        public override void EndRotate()
        {
            m_initialPositions = null;
            m_initialIndexes = null;

            if (UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = true;
            }

#if PROBUILDER_4_4_0_OR_NEWER
            m_faceSelection.Meshes.RefreshColliders();
#endif
        }

        public override void Rotate(Quaternion rotation)
        {
            if (m_faceSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * m_initialRotation * (position - center);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass,
                center + rotation * m_initialRotation * (m_initialPostion - center));

            RaisePBMeshesChanged(true);
        }


        public override void BeginScale()
        {
            m_initialPostion = m_faceSelection.LastPosition;

            m_initialPositions = new Vector3[m_faceSelection.MeshesCount][];
            m_initialIndexes = new int[m_faceSelection.MeshesCount][];

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_faceSelection.GetIndexes(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }

            if (UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = false;
            }
        }

        public override void EndScale()
        {
            m_initialPositions = null;
            m_initialIndexes = null;

            if (UVEditingMode)
            {
                m_faceSelection.IsRendererEnabled = true;
            }

#if PROBUILDER_4_4_0_OR_NEWER
            m_faceSelection.Meshes.RefreshColliders();
#endif
        }

        public override void Scale(Vector3 scale, Quaternion rotation)
        {
            if (m_faceSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (position - center), scale);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass,
                center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (m_initialPostion - center), scale));

            RaisePBMeshesChanged(true);
        }

        public override MeshSelection Select(Camera camera, Vector3 pointer, bool shift, bool ctrl, bool depthTest)
        {
            MeshSelection selection = null;
            MeshAndFace result = PBUtility.PickFace(camera, pointer);
            if(result.face != null)
            {
                if(ctrl)
                {
                    int submeshIndex = result.face.submeshIndex;
                    IList<Face> faces = result.mesh.faces;
                    List<int> sameMaterialFaces = new List<int>();

                    bool wasSelected = false;
                    bool wasUnselected = false;
                    
                    m_faceSelection.BeginChange();
                    for (int i = 0; i < faces.Count; ++i)
                    {
                        Face face = faces[i];
                        if (face.submeshIndex == submeshIndex)
                        {
                            sameMaterialFaces.Add(i);

                            if (!m_faceSelection.IsSelected(result.mesh, i))
                            {
                                m_faceSelection.Add(result.mesh, i);
                                wasUnselected = true;
                            }
                            else
                            {
                                wasSelected = true;
                            }
                        }
                    }

                    if(wasSelected && !wasUnselected)
                    {
                        for(int i = 0; i < sameMaterialFaces.Count; ++i)
                        {
                            m_faceSelection.Remove(result.mesh, sameMaterialFaces[i]);
                        }

                        selection = new MeshSelection();
                        selection.UnselectedFaces.Add(result.mesh.gameObject, sameMaterialFaces.ToArray());
                    }
                    else
                    {
                        selection = new MeshSelection();
                        selection.SelectedFaces.Add(result.mesh.gameObject, sameMaterialFaces.ToArray());
                    }

                    m_faceSelection.EndChange();
                }
                else
                {
                    int faceIndex = result.mesh.faces.IndexOf(result.face);
                    if (m_faceSelection.IsSelected(result.mesh, faceIndex))
                    {
                        if (shift)
                        {
                            m_faceSelection.Remove(result.mesh, faceIndex);
                            selection = new MeshSelection();
                            selection.UnselectedFaces.Add(result.mesh.gameObject, new[] { faceIndex });
                        }
                        else
                        {
                            //selection = ReadSelection();
                            selection = null;
                        }
                    }
                    else
                    {
                        if (shift)
                        {
                            selection = new MeshSelection();
                        }
                        else
                        {
                            selection = ReadSelection();
                            m_faceSelection.Clear();
                        }

                        m_faceSelection.Add(result.mesh, faceIndex);
                        selection.SelectedFaces.Add(result.mesh.gameObject, new[] { faceIndex });
                    }
                }
            }
            else
            {
                if (!shift)
                {
                    selection = ReadSelection();
                    if (selection.UnselectedFaces.Count == 0)
                    {
                        selection = null;
                    }
                    m_faceSelection.Clear(); 
                }
            }
            return selection;
        }

        private MeshSelection ReadSelection()
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                if(mesh == null)
                {
                    continue;
                }

                IList<int> faces = m_faceSelection.GetFaces(mesh);
                if (faces != null && faces.Count > 0)
                {
                    selection.UnselectedFaces.Add(mesh.gameObject, faces.ToArray());
                }
            }
            return selection;
        }

        public override MeshSelection Select(Camera camera, Rect rect, Rect uiRootRect, GameObject[] gameObjects, bool depthTest, MeshEditorSelectionMode mode)
        {
            MeshSelection selection = new MeshSelection();
            m_faceSelection.BeginChange();

            Dictionary<ProBuilderMesh, HashSet<Face>> result = PBUtility.PickFaces(camera, rect, uiRootRect, gameObjects, depthTest);
            if (mode == MeshEditorSelectionMode.Add)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;
                    IList<Face> faces = mesh.faces;
                    IList<int> faceIndices = kvp.Value.Select(f => faces.IndexOf(f)).ToArray();
                    IList<int> notSelected = faceIndices.Where(f => !m_faceSelection.IsSelected(mesh, f)).ToArray();
                    foreach (int face in notSelected)
                    {
                        m_faceSelection.Add(mesh, face);
                    }

                    selection.SelectedFaces.Add(mesh.gameObject, notSelected);
                }
            }
            else if(mode == MeshEditorSelectionMode.Substract)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;
                    IList<Face> faces = mesh.faces;
                    IList<int> faceIndices = kvp.Value.Select(f => faces.IndexOf(f)).ToArray();
                    IList<int> selected = faceIndices.Where(f => m_faceSelection.IsSelected(mesh, f)).ToArray();
                    foreach (int face in selected)
                    {
                        m_faceSelection.Remove(mesh, face);
                    }
                    selection.UnselectedFaces.Add(mesh.gameObject, selected);
                }
            }
            else if(mode == MeshEditorSelectionMode.Difference)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;
                    IList<Face> faces = mesh.faces;
                    IList<int> faceIndices = kvp.Value.Select(f => faces.IndexOf(f)).ToArray();

                    IList<int> selected = faceIndices.Where(f => m_faceSelection.IsSelected(mesh, f)).ToArray();
                    IList<int> notSelected = faceIndices.Where(f => !m_faceSelection.IsSelected(mesh, f)).ToArray();

                    foreach (int face in selected)
                    {
                        m_faceSelection.Remove(mesh, face);
                    }

                    foreach(int face in notSelected)
                    {
                        m_faceSelection.Add(mesh, face);
                    }

                    selection.UnselectedFaces.Add(mesh.gameObject, selected);
                    selection.SelectedFaces.Add(mesh.gameObject, notSelected);
                }
            }

            m_faceSelection.EndChange();

            if(selection.SelectedFaces.Count == 0 && selection.UnselectedFaces.Count == 0)
            {
                selection = null;
            }

            return selection;
        }

        public override MeshSelection Select(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);

            m_faceSelection.BeginChange();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces.ToArray())
            {
                IList<int> faces = kvp.Value;
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                for(int i = faces.Count - 1; i >= 0; i--)
                {
                    int face = faces[i];
                    if(m_faceSelection.IsSelected(mesh, face))
                    {
                        faces.Remove(face);
                    }
                    else
                    {
                        m_faceSelection.Add(mesh, face);
                    }
                }

                if(faces.Count == 0)
                {
                    selection.SelectedFaces.Remove(kvp.Key);
                }
            }
            m_faceSelection.EndChange();
            if(selection.SelectedFaces.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override MeshSelection Unselect(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection.Invert();

            m_faceSelection.BeginChange();
            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.UnselectedFaces.ToArray())
            {
                IList<int> faces = kvp.Value;
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                for (int i = faces.Count - 1; i >= 0; i--)
                {
                    int face = faces[i];
                    if (!m_faceSelection.IsSelected(mesh, face))
                    {
                        faces.Remove(face);
                    }
                    else
                    {
                        m_faceSelection.Remove(mesh, face);
                    }
                }

                if (faces.Count == 0)
                {
                    selection.UnselectedFaces.Remove(kvp.Key);
                }
            }
            m_faceSelection.EndChange();
            if (selection.UnselectedFaces.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override void SetSelection(MeshSelection selection)
        {
            if (m_faceSelection != null)
            {
                m_faceSelection.Clear();
            }

            if (selection != null)
            {
                selection = selection.ToFaces(false);

                m_faceSelection.BeginChange();

                foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
                {
                    ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                    if(mesh == null)
                    {
                        continue;
                    }

                    PBMesh pbMesh = kvp.Key.GetComponent<PBMesh>();
                    if (pbMesh.IsMarkedAsDestroyed)
                    {
                        continue;
                    }
                    foreach (int face in kvp.Value)
                    {
                        m_faceSelection.Add(mesh, face);
                    }
                }

                m_faceSelection.EndChange();
            }
        }

        public override MeshSelection GetSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach (ProBuilderMesh mesh in m_faceSelection.Meshes)
            {
                selection.SelectedFaces.Add(mesh.gameObject, m_faceSelection.GetFaces(mesh).ToArray());
            }
            if (selection.SelectedFaces.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override MeshSelection ClearSelection()
        {
            MeshSelection meshSelection = null;
            if (m_faceSelection != null)
            {
                meshSelection = ReadSelection();
                m_faceSelection.Clear();
            }
            return meshSelection;
        }

        public override MeshEditorState GetState(bool recordUV)
        {
            MeshEditorState state = new MeshEditorState();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                state.State.Add(mesh.gameObject, new MeshState(mesh.positions.ToArray(), mesh.faces.ToArray(), mesh.textures.ToArray(), recordUV));
            }
            return state;
        }

        public override void SetState(MeshEditorState state)
        {
            m_faceSelection.BeginChange();

            ProBuilderMesh[] meshes = state.State.Keys.Select(k => k.GetComponent<ProBuilderMesh>()).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<int> faces = m_faceSelection.GetFaces(mesh).ToArray();
                for (int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Remove(mesh, faces[i]);
                }

                MeshState meshState = state.State[mesh.gameObject];
                mesh.Rebuild(meshState.Positions, meshState.Faces.Select(f => f.ToFace()).ToArray(), meshState.Textures);

                for (int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Add(mesh, faces[i]);
                }
            }

            m_faceSelection.EndChange();
        }

        public override void Extrude(float distance)
        {
            m_faceSelection.BeginChange();

            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<int> faceIndexes = m_faceSelection.GetFaces(mesh).ToArray();
                for (int i = 0; i < faceIndexes.Count; ++i)
                {
                    m_faceSelection.Remove(mesh, faceIndexes[i]);
                }

                IList<Face> faces = new List<Face>();
                mesh.GetFaces(faceIndexes, faces);

                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, distance);

                for (int i = 0; i < faceIndexes.Count; ++i)
                {
                    m_faceSelection.Add(mesh, faceIndexes[i]);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }

            m_faceSelection.EndChange();

            if (distance != 0.0f)
            {
                m_faceSelection.Synchronize(
                    m_faceSelection.GetCenterOfMass(),
                    m_faceSelection.LastPosition + m_faceSelection.LastNormal * distance);
            }

            RaisePBMeshesChanged(false);
        }

        public override void Delete()
        {
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();   
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = new List<Face>();
                mesh.GetFaces(m_faceSelection.GetFaces(mesh), faces);
                mesh.DeleteFaces(faces);
                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public override void Subdivide()
        {
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();

            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = new List<Face>();
                mesh.GetFaces(m_faceSelection.GetFaces(mesh), faces);
                ConnectElements.Connect(mesh, faces);
                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public override void Merge()
        {
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();

            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = new List<Face>();
                mesh.GetFaces(m_faceSelection.GetFaces(mesh), faces);
                Merge(mesh, faces);
                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        private static Face Merge(ProBuilderMesh target, IEnumerable<Face> faces)
        {
            int mergedCount = faces != null ? faces.Count() : 0;

            if (mergedCount < 1)
                return null;

            Face first = faces.First();

            Face mergedFace = new Face();
            mergedFace.SetIndexes(faces.SelectMany(x => x.indexes).ToArray());
            mergedFace.submeshIndex = first.submeshIndex;
            mergedFace.uv = first.uv;
            mergedFace.smoothingGroup = first.smoothingGroup;
            mergedFace.textureGroup = first.textureGroup;
            mergedFace.manualUV = first.manualUV;

            Face[] rebuiltFaces = new Face[target.faces.Count - mergedCount + 1];

            int n = 0;

            HashSet<Face> skip = new HashSet<Face>(faces);

            foreach (Face f in target.faces)
            {
                if (!skip.Contains(f))
                    rebuiltFaces[n++] = f;
            }

            rebuiltFaces[n] = mergedFace;

            target.faces = rebuiltFaces;

            CollapseCoincidentVertices(target, new Face[] { mergedFace });

            return mergedFace;
        }

        /// <summary>
        /// Condense co-incident vertex positions per-face. vertices must already be marked as shared in the sharedIndexes
        /// array to be considered. This method is really only useful after merging faces.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        internal static void CollapseCoincidentVertices(ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            SharedVertex.GetSharedVertexLookup(mesh.sharedVertices, lookup);
            Dictionary<int, int> matches = new Dictionary<int, int>();

            foreach (Face face in faces)
            {
                matches.Clear();

                int[] indexes = face.indexes.ToArray();
                for (int i = 0; i < indexes.Length; i++)
                {
                    int common = lookup[face.indexes[i]];

                    if (matches.ContainsKey(common))
                        indexes[i] = matches[common];
                    else
                        matches.Add(common, indexes[i]);
                }
                face.SetIndexes(indexes);

                face.Reverse();
                face.Reverse();
            }

            MeshValidation.RemoveUnusedVertices(mesh);
        }

        private void RaisePBMeshesChanged(bool positionsOnly)
        {
            foreach (PBMesh mesh in m_faceSelection.PBMeshes)
            {
                mesh.RaiseChanged(positionsOnly, false);
            }
        }
    }
}

