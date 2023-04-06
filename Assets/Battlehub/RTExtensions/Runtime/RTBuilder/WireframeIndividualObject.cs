using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTSL;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class WireframeIndividualObject : MonoBehaviour
    {
        private WireframeMesh m_wireframeMesh;
        private Renderer m_renderer;
        private bool m_wasEnabled;
        
        public WireframeMesh WireframeMesh
        {
            get { return m_wireframeMesh; }
        }

        private IRTE m_editor;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged += OnSelectionChanged;
            }
            
            PBMesh pbMesh = GetComponent<PBMesh>();
            if(pbMesh == null)
            {
                Destroy(this);
            }
            else
            {
                m_renderer = GetComponent<Renderer>();
                if (m_renderer != null)
                {
                    m_wasEnabled = m_renderer.enabled;
                    m_renderer.enabled = false;
                }

                CreateWireframeMesh(pbMesh);
            }
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
            if (m_wireframeMesh != null)
            {
                Destroy(m_wireframeMesh.gameObject);
                if(m_renderer != null)
                {
                    m_renderer.enabled = m_wasEnabled;
                }
            }
        }

        private void CreateWireframeMesh(PBMesh pbMesh)
        {
            GameObject wireframe = new GameObject("IndividualWireframe");
            wireframe.transform.SetParent(pbMesh.transform, false);
            wireframe.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            m_wireframeMesh = wireframe.AddComponent<WireframeMesh>();
            m_wireframeMesh.IsIndividual = true;
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            m_wireframeMesh.IsSelected = m_editor.Selection.IsSelected(gameObject);
        }
    }

}
