using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public class PBComplexShape : MonoBehaviour
    {
        private PBComplexShapeSelection m_selection;
        protected PBComplexShapeSelection Selection
        {
            get { return m_selection; }
            set { m_selection = value; }
        }

        private ProBuilderMesh m_targetMesh;
        protected ProBuilderMesh TargetMesh
        {
            get { return m_targetMesh; }
        }

        private PBMesh m_target;
        protected PBMesh Target
        {
            get { return m_target; }
        }

        private bool m_isEditing;
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if (m_isEditing != value)
                {
                    m_isEditing = value;
                    if (m_isEditing)
                    {
                        m_target.RaiseSelected(true);
                        BeginEdit();
                    }
                    else
                    {
                        EndEdit();
                        m_target.RaiseUnselected();
                    }
                }
            }
        }

        public int VertexCount
        {
            get { return m_selection.Positions.Count; }
        }

        private int m_stage;
        public int Stage
        {
            get { return m_stage; }
            set { m_stage = value; }
        }

        private List<Vector3> m_positions = new List<Vector3>();
        public List<Vector3> Positions
        {
            get { return m_positions; }
            set
            {
                m_positions.Clear();
                if (value != null)
                {
                    for (int i = 0; i < value.Count; ++i)
                    {
                        m_positions.Add(value[i]);
                    }
                }
                if (IsEditing)
                {
                    if (m_selection != null)
                    {
                        m_selection.Clear();
                        if (value != null)
                        {
                            for (int i = 0; i < value.Count; ++i)
                            {
                                m_selection.Add(value[i]);
                            }
                        }
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get { return m_selection == null ? -1 : m_selection.SelectedIndex; }
            set
            {
                m_selection.Unselect();
                if (value >= 0)
                {
                    m_selection.Select(value);
                }
            }
        }

        public Vector3 SelectedPosition
        {
            get { return m_selection.Positions[SelectedIndex]; }
            set
            {
                IList<Vector3> positions = m_selection.Positions;
                positions[m_selection.SelectedIndex] = value;
                m_positions[m_selection.SelectedIndex] = value;
                Refresh();
            }
        }

        protected virtual void Awake()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            m_target = GetComponent<PBMesh>();
            if (!m_target)
            {
                m_target = gameObject.AddComponent<PBMesh>();
            }

            m_targetMesh = m_target.GetComponent<ProBuilderMesh>();
            if (IsEditing)
            {
                BeginEdit();
            }
        }
        protected virtual void OnDestroy()
        {
            EndEdit();
        }

        private void BeginEdit()
        {
            m_selection = gameObject.GetComponent<PBComplexShapeSelection>();
            if (m_selection == null)
            {
                m_selection = CreateSelectionObject();
            }
            m_selection.enabled = true;
            m_isEditing = true;
            Positions = Positions.ToList();
            if (m_selection.Positions.Count == 0)
            {
                m_selection.Add(Vector3.zero);
                m_positions.Add(Vector3.zero);
            }
        }

        private void EndEdit()
        {
            if (m_selection != null)
            {
                m_selection.Unselect();
                m_selection.enabled = false;
            }

            m_isEditing = false;
        }


        public bool Click(Camera camera, Vector3 pointer, int layerMask = 0)
        {
            if (!m_isEditing)
            {
                return false;
            }

            SceneSelection selection = new SceneSelection();
            float result = PBUtility.PickVertex(camera, pointer, 20, m_selection.Transform, m_selection.Positions, ref selection);

            if (result != Mathf.Infinity)
            {
                if (m_selection.Positions.Count >= 3)
                {
                    m_selection.Unselect();
#if PROBUILDER_4_4_0_OR_NEWER
                    m_selection.Select(selection.vertexes[0]);
#else
                    m_selection.Select(selection.vertex);
#endif
                    return true;
                }
            }
            else
            {
                if (Stage == 0)
                {
                    Ray ray = camera.ScreenPointToRay(pointer);

                    bool foundHitPoint = false;
                    Vector3 hitPoint = Vector3.zero;

                    if (Physics.Raycast(ray, out RaycastHit hit, int.MaxValue, layerMask))
                    {
                        hitPoint = hit.point;
                        foundHitPoint = true;
                    }
                    else
                    {
                        Plane plane = new Plane(m_selection.transform.up, m_selection.transform.position);
                        if (plane.Raycast(ray, out float enter))
                        {
                            hitPoint = ray.GetPoint(enter);
                            foundHitPoint = true;
                        }
                    }

                    if (foundHitPoint)
                    {
                        hitPoint = m_selection.Transform.InverseTransformPoint(hitPoint);
                        m_selection.Add(hitPoint);
                        m_positions.Add(hitPoint);
                        CreateShape();
                    }
                }
            }

            return false;
        }

        public void Refresh()
        {
            CreateShape();
            m_selection.Refersh();
            #if PROBUILDER_4_4_0_OR_NEWER
            m_targetMesh.RefereshColliders();
            #endif
        }

        protected virtual void CreateShape()
        {
            m_target.CreateShapeFromPolygon(m_selection.Positions, 0.001f, false);
        }

        protected virtual PBComplexShapeSelection CreateSelectionObject()
        {
            return gameObject.AddComponent<PBComplexShapeSelection>();
        }

        public MeshEditorState GetState(bool recordUV)
        {
            MeshEditorState state = new MeshEditorState();
            state.State.Add(m_targetMesh.gameObject, new MeshState(m_targetMesh.positions.ToArray(), m_targetMesh.faces.ToArray(), m_targetMesh.textures.ToArray(), recordUV));
            return state;
        }

        public void SetState(MeshEditorState state)
        {
            ProBuilderMesh[] meshes = state.State.Keys.Select(key => key.GetComponent<ProBuilderMesh>()).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                MeshState meshState = state.State[mesh.gameObject];
                mesh.Rebuild(meshState.Positions, meshState.Faces.Select(f => f.ToFace()).ToArray(), meshState.Textures);
            }

            m_target.RaiseChanged(false, true);
        }
    }
}