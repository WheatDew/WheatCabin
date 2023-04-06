using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public class PBEdgeEditor : PBBaseEditor
    {
        private PBEdgeSelection m_edgeSelection;
        private SceneSelection m_selection = new SceneSelection();
        
        public override bool HasSelection
        {
            get { return m_edgeSelection.EdgesCount > 0; }
        }

        public override Vector3 Position
        {
            get { return CenterMode ? m_edgeSelection.CenterOfMass : m_edgeSelection.LastPosition; }
            set { MoveTo(value); }
        }

        public override Vector3 Normal
        {
            get { return (!GlobalMode && m_edgeSelection.LastMesh != null) ? m_edgeSelection.LastMesh.transform.forward : Vector3.forward; }
        }

        public override Quaternion Rotation
        {
            get
            {
                if (GlobalMode || m_edgeSelection.LastMesh == null)
                {
                    return Quaternion.identity;
                }

                MeshSelection selection = GetSelection();
                if (selection == null)
                {
                    return Quaternion.identity;
                }

                selection = selection.ToFaces(false, false);

                IList<int> faceIndexes;
                if (selection.SelectedFaces.TryGetValue(m_edgeSelection.LastMesh.gameObject, out faceIndexes))
                {
                    if (faceIndexes.Count != 0)
                    {
                        return HandleUtility.GetRotation(m_edgeSelection.LastMesh, m_edgeSelection.LastMesh.faces[faceIndexes.Last()].distinctIndexes);
                    }
                }

                IList<Edge> edges;
                if (!selection.SelectedEdges.TryGetValue(m_edgeSelection.LastMesh.gameObject, out edges) || edges.Count == 0)
                {
                    return Quaternion.identity;
                }

                Face face = PBUtility.GetFace(m_edgeSelection.LastMesh, edges.Last());
                if (face == null)
                {
                    return Quaternion.identity;
                }

                return HandleUtility.GetRotation(m_edgeSelection.LastMesh, face.distinctIndexes);
            }
        }

        public override GameObject Target
        {
            get { return m_edgeSelection.LastMesh != null ? m_edgeSelection.LastMesh.gameObject : null; }
        }

        private void Awake()
        {
            m_edgeSelection = gameObject.AddComponent<PBEdgeSelection>();
        }

        private void OnDestroy()
        {
            if (m_edgeSelection != null)
            {
                Destroy(m_edgeSelection);
            }
        }

        public override void SetSelection(MeshSelection selection)
        {
            m_edgeSelection.Clear();
            m_selection.Clear();

            if (selection != null)
            {
                selection = selection.ToEdges(false);

                foreach (KeyValuePair<GameObject, IList<Edge>> kvp in selection.SelectedEdges)
                {
                    PBMesh pbMesh = kvp.Key.GetComponent<PBMesh>();
                    if(pbMesh.IsMarkedAsDestroyed)
                    {
                        continue;
                    }

                    m_edgeSelection.Add(kvp.Key.GetComponent<ProBuilderMesh>(), kvp.Value);
                }
            }
        }

        public override MeshSelection GetSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach (ProBuilderMesh mesh in m_edgeSelection.Meshes)
            {
                selection.SelectedEdges.Add(mesh.gameObject, m_edgeSelection.GetEdges(mesh).ToArray());
            }
            if (selection.SelectedEdges.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override MeshSelection ClearSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach (ProBuilderMesh mesh in m_edgeSelection.Meshes)
            {
                if(mesh == null || mesh.gameObject == null)
                {
                    continue;
                }

                selection.UnselectedEdges.Add(mesh.gameObject, m_edgeSelection.GetEdges(mesh).ToArray());
            }

            m_edgeSelection.Clear();
            m_selection.Clear();

            if (selection.UnselectedEdges.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override MeshSelection SelectHoles()
        {
            MeshSelection selection = new MeshSelection();
            foreach (ProBuilderMesh mesh in m_edgeSelection.Meshes)
            {
                HashSet<int> indexes = new HashSet<int>();
                for (int i = 0; i < mesh.vertexCount; ++i)
                {
                    indexes.Add(i);
                }

                List<List<Edge>> holes = PBElementSelection.FindHoles(mesh, indexes);
                selection.SelectedEdges.Add(mesh.gameObject, m_edgeSelection.GetCoincidentEdges(holes.SelectMany(e => e).Where(e => !m_edgeSelection.IsSelected(mesh, e))));
                m_edgeSelection.Add(mesh, selection.SelectedEdges[mesh.gameObject]);
            }

            if (!selection.HasEdges)
            {
                return null;
            }

            return selection;
        }

        public override void FillHoles()
        {
            int filled = 0;

            foreach (ProBuilderMesh mesh in m_edgeSelection.Meshes)
            {
                HashSet<int> indexes = new HashSet<int>();
                for (int i = 0; i < mesh.vertexCount; ++i)
                {
                    indexes.Add(i);
                }
                List<List<Edge>> holes = PBElementSelection.FindHoles(mesh, indexes);

                mesh.ToMesh();

                List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
                HashSet<Face> appendedFaces = new HashSet<Face>();

                foreach (List<Edge> hole in holes)
                {
                    List<int> holeIndexes;
                    Face face;

                    if (!hole.All(e => m_edgeSelection.IsSelected(mesh, e)))
                    {
                        continue;
                    }

                    //if (wholePath)
                    //{
                    //    // if selecting whole path and in edge mode, make sure the path contains
                    //    // at least one complete edge from the selection.
                    //    if (!hole.Any(x => common.Contains(x.edge.common.a) &&
                    //            common.Contains(x.edge.common.b)))
                    //        continue;

                    //    holeIndexes = hole.Select(x => x.edge.local.a).ToList();
                    //    face = AppendElements.CreatePolygon(mesh, holeIndexes, false);
                    //}
                    //else
                    {
                        //IEnumerable<WingedEdge> selected = hole.Where(x => common.Contains(x.edge.common.a));
                        //holeIndexes = selected.Select(x => x.edge.local.a).ToList();

                        //holeIndexes = hole.Select(x => x.edge.local.a).ToList();
                        //face = AppendElements.CreatePolygon(mesh, holeIndexes, true);

                        holeIndexes = hole.Select(x => x.a).ToList();
                        face = AppendElements.CreatePolygon(mesh, holeIndexes, true);
                    }

                    if (face != null)
                    {
                        filled++;
                        appendedFaces.Add(face);
                    }
                }

                mesh.SetSelectedFaces(appendedFaces);

                wings = WingedEdge.GetWingedEdges(mesh);

                // make sure the appended faces match the first adjacent face found
                // both in winding and face properties
                foreach (var appendedFace in appendedFaces)
                {
                    var wing = wings.FirstOrDefault(x => x.face == appendedFace);

                    if (wing == null)
                        continue;

                    using (var it = new WingedEdgeEnumerator(wing))
                    {
                        while (it.MoveNext())
                        {
                            if (it.Current == null)
                                continue;

                            var currentWing = it.Current;
                            var oppositeFace = it.Current.opposite != null ? it.Current.opposite.face : null;

                            if (oppositeFace != null && !appendedFaces.Contains(oppositeFace))
                            {
                                currentWing.face.submeshIndex = oppositeFace.submeshIndex;
                                currentWing.face.uv = new AutoUnwrapSettings(oppositeFace.uv);
                                PBSurfaceTopology.ConformOppositeNormal(currentWing.opposite);
                                break;
                            }
                        }
                    }
                }

                mesh.ToMesh();
                mesh.Refresh();
            }
        }

        public override MeshSelection Select(Camera camera, Vector3 pointer, bool shift, bool ctrl, bool depthTest)
        {
            MeshSelection selection = null;
            GameObject pickedObject = PBUtility.PickObject(camera, pointer);
            float result = PBUtility.PickEdge(camera, pointer, 20, pickedObject, m_edgeSelection.Meshes, depthTest, ref m_selection);

            if (result != Mathf.Infinity)
            {
#if PROBUILDER_4_4_0_OR_NEWER
                Edge selectedEdge = m_selection.edges[0];
#else
                Edge selectedEdge = m_selection.edge;
#endif

                if (m_edgeSelection.IsSelected(m_selection.mesh, selectedEdge))
                {
                    if (shift)
                    {
                        m_edgeSelection.FindCoincidentEdges(m_selection.mesh);

                        IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(new[] { selectedEdge });
                        m_edgeSelection.Remove(m_selection.mesh, edges);
                        selection = new MeshSelection();
                        selection.UnselectedEdges.Add(m_selection.mesh.gameObject, edges);
                    }
                    else
                    {
                        m_edgeSelection.FindCoincidentEdges(m_selection.mesh);

                        IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(new[] { selectedEdge });
                        selection = ReadSelection();
                        selection.UnselectedEdges[m_selection.mesh.gameObject] = selection.UnselectedEdges[m_selection.mesh.gameObject].Where(e => e != selectedEdge).ToArray();
                        selection.SelectedEdges.Add(m_selection.mesh.gameObject, edges);
                        m_edgeSelection.Clear();
                        m_edgeSelection.Add(m_selection.mesh, edges);
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
                        m_edgeSelection.Clear();
                    }

                    m_edgeSelection.FindCoincidentEdges(m_selection.mesh);

                    IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(new[] { selectedEdge });

                    m_edgeSelection.Add(m_selection.mesh, edges);
                    selection.SelectedEdges.Add(m_selection.mesh.gameObject, edges);
                }
            }
            else
            {
                if (!shift)
                {
                    selection = ReadSelection();
                    if (selection.UnselectedEdges.Count == 0)
                    {
                        selection = null;
                    }
                    m_edgeSelection.Clear();
                }
            }
            return selection;
        }

        private MeshSelection ReadSelection()
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = m_edgeSelection.Meshes.OrderBy(m => m == m_edgeSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetEdges(mesh);
                if (edges != null)
                {
                    selection.UnselectedEdges.Add(mesh.gameObject, edges.ToArray());
                }
            }
            return selection;
        }

        public override MeshSelection Select(Camera camera, Rect rect, Rect uiRootRect, GameObject[] gameObjects, bool depthTest, MeshEditorSelectionMode mode)
        {
            Dictionary<ProBuilderMesh, HashSet<Edge>> pickResult = PBUtility.PickEdges(camera, rect, uiRootRect, gameObjects, depthTest);
            if (pickResult.Count == 0)
            {
                return null;
            }

            MeshSelection selection = new MeshSelection();
            foreach (KeyValuePair<ProBuilderMesh, HashSet<Edge>> kvp in pickResult)
            {
                ProBuilderMesh mesh = kvp.Key;

                m_edgeSelection.FindCoincidentEdges(mesh);
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(kvp.Value);

                IList<Edge> selected = edges.Where(edge => m_edgeSelection.IsSelected(mesh, edge)).ToArray();
                IList<Edge> notSelected = edges.Where(edge => !m_edgeSelection.IsSelected(mesh, edge)).ToArray();

                if (mode == MeshEditorSelectionMode.Substract || mode == MeshEditorSelectionMode.Difference)
                {
                    selection.UnselectedEdges.Add(mesh.gameObject, selected);
                    m_edgeSelection.Remove(mesh, selected);
                }

                if (mode == MeshEditorSelectionMode.Add || mode == MeshEditorSelectionMode.Difference)
                {
                    selection.SelectedEdges.Add(mesh.gameObject, notSelected);
                    m_edgeSelection.Add(mesh, notSelected);
                }
            }
            return selection;
        }

        public override MeshSelection Select(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection = selection.ToEdges(false, false);

            foreach (KeyValuePair<GameObject, IList<Edge>> kvp in selection.SelectedEdges.ToArray())
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                m_edgeSelection.FindCoincidentEdges(mesh);
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(kvp.Value).ToList();

                for (int i = edges.Count - 1; i >= 0; i--)
                {
                    Edge edge = edges[i];
                    if (m_edgeSelection.IsSelected(mesh, edge))
                    {
                        edges.Remove(edge);
                    }
                }

                if (edges.Count == 0)
                {
                    selection.SelectedEdges.Remove(kvp.Key);
                }
                else
                {
                    m_edgeSelection.Add(mesh, edges);
                }
            }
            if (selection.SelectedEdges.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override MeshSelection Unselect(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection = selection.ToEdges(true, false);

            foreach (KeyValuePair<GameObject, IList<Edge>> kvp in selection.UnselectedEdges.ToArray())
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                m_edgeSelection.FindCoincidentEdges(mesh);
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(kvp.Value).ToList();

                for (int i = edges.Count - 1; i >= 0; i--)
                {
                    Edge edge = edges[i];
                    if (!m_edgeSelection.IsSelected(mesh, edge))
                    {
                        edges.Remove(edge);
                    }
                }

                if (edges.Count == 0)
                {
                    selection.UnselectedEdges.Remove(kvp.Key);
                }
                else
                {
                    m_edgeSelection.Remove(mesh, edges);
                }
            }
            if (selection.UnselectedEdges.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override void BeginMove()
        {
            base.BeginMove();
        }

        public override void EndMove()
        {
            base.EndMove();
#if PROBUILDER_4_4_0_OR_NEWER
            m_edgeSelection.Meshes.RefreshColliders();
#endif
        }
        private void MoveTo(Vector3 to)
        {
            if (m_edgeSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 from = Position;
            Vector3 offset = to - from;

            IEnumerable<ProBuilderMesh> meshes = m_edgeSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                Vector3 localOffset = mesh.transform.InverseTransformVector(offset);
                IEnumerable<Edge> edges = m_edgeSelection.GetEdges(mesh);
                mesh.TranslateVertices(edges, localOffset);

                mesh.ToMesh();
                mesh.Refresh();
            }

            ProBuilderMesh lastMesh = m_edgeSelection.LastMesh;
            if (lastMesh != null)
            {
                m_edgeSelection.Synchronize(
                    m_edgeSelection.CenterOfMass + offset,
                    m_edgeSelection.LastPosition + offset,
                    m_edgeSelection.LastNormal);
            }

            RaisePBMeshesChanged(true);
        }

        private int[][] m_initialIndexes;
        private Vector3[][] m_initialPositions;
        private Vector3 m_initialPostion;
        private Quaternion m_initialRotation;
        
        public override void BeginRotate(Quaternion initialRotation)
        {
            m_initialPostion = m_edgeSelection.LastPosition;
            m_initialPositions = new Vector3[m_edgeSelection.MeshesCount][];
            m_initialIndexes = new int[m_edgeSelection.MeshesCount][];
            m_initialRotation = Quaternion.Inverse(initialRotation);

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_edgeSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(m_edgeSelection.GetEdges(mesh));

                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(edges.Select(e => e.a).Union(edges.Select(e => e.b))).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }
        }

        public override void EndRotate()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
#if PROBUILDER_4_4_0_OR_NEWER
            m_edgeSelection.Meshes.RefreshColliders();
#endif
        }

        public override void Rotate(Quaternion rotation)
        {
            if (m_edgeSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_edgeSelection.Meshes;
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

            m_edgeSelection.Synchronize(
                m_edgeSelection.CenterOfMass,
                center + rotation * m_initialRotation * (m_initialPostion - center),
                rotation * m_initialRotation * m_edgeSelection.LastNormal);

            RaisePBMeshesChanged(true);
        }


        public override void BeginScale()
        {
            m_initialPostion = m_edgeSelection.LastPosition;
            m_initialPositions = new Vector3[m_edgeSelection.MeshesCount][];
            m_initialIndexes = new int[m_edgeSelection.MeshesCount][];

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_edgeSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(m_edgeSelection.GetEdges(mesh));

                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(edges.Select(e => e.a).Union(edges.Select(e => e.b))).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();

                meshIndex++;
            }
        }

        public override void EndScale()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
#if PROBUILDER_4_4_0_OR_NEWER
            m_edgeSelection.Meshes.RefreshColliders();
#endif
        }

        public override void Scale(Vector3 scale, Quaternion rotation)
        {
            if (m_edgeSelection.MeshesCount == 0)
            {
                return;
            }

            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_edgeSelection.Meshes;

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

            m_edgeSelection.Synchronize(
                m_edgeSelection.CenterOfMass,
                center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (m_initialPostion - center), scale),
                rotation * m_initialRotation * m_edgeSelection.LastNormal);

            RaisePBMeshesChanged(true);
        }

        public override MeshEditorState GetState(bool recordUV)
        {
            MeshEditorState state = new MeshEditorState();
            ProBuilderMesh[] meshes = m_edgeSelection.Meshes.OrderBy(m => m == m_edgeSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                state.State.Add(mesh.gameObject, new MeshState(mesh.positions.ToArray(), mesh.faces.ToArray(), mesh.textures.ToArray(), recordUV));
            }
            return state;
        }

        public override void SetState(MeshEditorState state)
        {
            ProBuilderMesh[] meshes = state.State.Keys.Select(k => k.GetComponent<ProBuilderMesh>()).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetCoincidentEdges(m_edgeSelection.GetEdges(mesh));
                if(edges != null)
                {
                    edges = edges.ToArray();
                    m_edgeSelection.Remove(mesh, edges);
                }
                
                MeshState meshState = state.State[mesh.gameObject];
                mesh.Rebuild(meshState.Positions, meshState.Faces.Select(f => f.ToFace()).ToArray(), meshState.Textures);

                if(edges != null)
                {
                    m_edgeSelection.Add(mesh, edges);
                }
            }
        }

        public override void Subdivide()
        {
            ProBuilderMesh[] meshes = m_edgeSelection.Meshes.OrderBy(m => m == m_edgeSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetEdges(mesh);
                ConnectElements.Connect(mesh, edges);

                mesh.Refresh();
                mesh.ToMesh();
            }
        }

        public override void Extrude(float distance = 0)
        {
            ProBuilderMesh[] meshes = m_edgeSelection.Meshes.OrderBy(m => m == m_edgeSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Edge> edges = m_edgeSelection.GetEdges(mesh).ToList();
                for(int i = edges.Count - 1; i >= 0; i--)
                {
                    Edge currentEdge = edges[i];

                    bool hasCoEdges = false;
                    IList<Edge> coEdges =  m_edgeSelection.GetCoincidentEdges(new[] { currentEdge });
                    for(int j = 0; j < coEdges.Count; ++j)
                    {
                        Edge coEdge = coEdges[j];
                        if(coEdge == currentEdge)
                        {
                            continue;
                        }

                        if(edges.Contains(coEdge))
                        {
                            hasCoEdges = true;
                            break;
                        }
                    }

                    if(hasCoEdges)
                    {
                        edges.RemoveAt(i);
                    }
                }
                m_edgeSelection.Remove(mesh);

                Edge[] newEdges = mesh.Extrude(edges, distance, false, true);

                mesh.ToMesh();
                mesh.Refresh();

                m_edgeSelection.Add(mesh, newEdges);
            }

            if (distance != 0.0f)
            {
                m_edgeSelection.Synchronize(
                    m_edgeSelection.CenterOfMass + m_edgeSelection.LastNormal * distance,
                    m_edgeSelection.LastPosition + m_edgeSelection.LastNormal * distance,
                    m_edgeSelection.LastNormal);
            }

            RaisePBMeshesChanged(false);
        }

        private void RaisePBMeshesChanged(bool positionsOnly)
        {
            foreach (PBMesh mesh in m_edgeSelection.PBMeshes)
            {
                mesh.RaiseChanged(positionsOnly, false);
            }
        }

        public override void Delete()
        {
            MeshSelection selection = GetSelection();
            selection = selection.ToFaces(false, true);

            foreach (KeyValuePair<GameObject, IList<int>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key.GetComponent<ProBuilderMesh>();
                mesh.DeleteFaces(kvp.Value);
                mesh.ToMesh();
                mesh.Refresh();
            }
        }
    }
}
