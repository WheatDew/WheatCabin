using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile.Controls
{
    public class MobileContextPanelPositionUpdater : MonoBehaviour
    {
        private IRTE m_editor = null;
        private RuntimeWindow m_window;
        private Canvas m_canvas = null;

        [SerializeField]
        private RectTransform m_panelTransform = null;
        [SerializeField]
        private CanvasGroup m_panelCanvasGroup = null;
        [SerializeField]
        private MobileContextPanel m_panel = null;

        private RectTransformChangeListener m_rectTransformChangesListener;
        private TransformChangesTracker m_cameraTransform;
        private Rect m_cameraPixelRect;
        private bool m_isCameraOrthographic;
        private bool m_transformChanged = false;
        private bool m_updateAlpha = false;
        private float m_forceInvisible;
        
        private Bounds[] m_bounds;
        private Transform[] m_selectedTransforms;
        private IList<Renderer> m_selectedRenderers;

        [SerializeField]
        private float m_marginLeft = 10;
        public float MarginLeft
        {
            get { return m_marginLeft; }
            set { m_marginLeft = value; }
        }

        [SerializeField]
        private float m_marginRight = 10;
        public float MarginRight
        {
            get { return m_marginRight; }
            set { m_marginRight = value; }
        }

        [SerializeField]
        private float m_marginTop = 10;
        public float MarginTop
        {
            get { return m_marginTop; }
            set { m_marginTop = value; }
        }

        [SerializeField]
        private float m_marginBottom = 10;
        public float MarginBottom
        {
            get { return m_marginBottom; }
            set { m_marginBottom = value; }
        }

        [SerializeField]
        private float m_spacing = 10;
        public float Spacing
        {
            get { return m_spacing; }
            set { m_spacing = value; }
        }

        private Camera CanvasCamera
        {
            get { return m_canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_canvas.worldCamera; }
        }

        private void Awake()
        {
            m_panelCanvasGroup.alpha = 0;

            m_window = GetComponentInParent<RuntimeWindow>();
            m_canvas = GetComponentInParent<Canvas>();

            RectTransform parent = (RectTransform)m_panelTransform.parent;
            parent.pivot = Vector2.zero;

            m_rectTransformChangesListener = m_panelTransform.gameObject.AddComponent<RectTransformChangeListener>();
            m_rectTransformChangesListener.RectTransformChanged += OnPanelRectTransformChanged;

            m_editor = IOC.Resolve<IRTE>();
            m_editor.Object.TransformChanged += OnTransformChanged;
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            m_cameraTransform = new TransformChangesTracker(m_window.Camera.transform);
            m_cameraPixelRect = m_window.Camera.pixelRect;
        }

        private void Start()
        {
            UpdateArrays();
            m_transformChanged = true;
        }

        private void OnDestroy()
        {
            if (m_editor.Object != null)
            {
                m_editor.Object.TransformChanged -= OnTransformChanged;
            }

            if(m_editor.Selection != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if(m_rectTransformChangesListener != null)
            {
                m_rectTransformChangesListener.RectTransformChanged -= OnPanelRectTransformChanged;
                m_rectTransformChangesListener = null;
            }
           
            m_panelTransform = null;
            m_canvas = null;
            m_editor = null;

            m_selectedTransforms = null;
            m_selectedRenderers = null;
            m_bounds = null;
        }

        private void Update()
        {
            if (m_forceInvisible > 0 && !m_editor.Input.IsAnyKey() && m_editor.TouchInput.TouchCount == 0)
            {
                m_forceInvisible -= Time.deltaTime;
                m_transformChanged = true;
            }
            
            if(m_updateAlpha)
            {
                if (m_panelTransform.gameObject.activeSelf)
                {
                    m_panelCanvasGroup.alpha = 1;
                }
                else
                {
                    m_panelCanvasGroup.alpha = 0;
                }

                if(m_panelCanvasGroup.alpha <= 0 || m_panelCanvasGroup.alpha >= 1)
                {
                    m_updateAlpha = false;
                }
            }           

            if (m_cameraTransform.HasChanged || m_isCameraOrthographic != m_window.Camera.orthographic || m_cameraPixelRect != m_window.Camera.pixelRect)
            {
                m_cameraTransform.Reset();
                m_isCameraOrthographic = m_window.Camera.orthographic;
                m_cameraPixelRect = m_window.Camera.pixelRect;

                if(m_editor.Input.IsAnyKey() || m_editor.TouchInput.TouchCount > 0)
                {
                    m_panelCanvasGroup.alpha = 0;
                    m_forceInvisible = 0.3f;
                    m_updateAlpha = false;
                }
            }
            else
            {
                if (!m_transformChanged)
                {
                    return;
                }
            }

            m_transformChanged = false;

            if(m_forceInvisible <= 0)
            {
                UpdatePanelTransform();
                UpdateIsInView();
                m_updateAlpha = true;
            }
        }

        private void UpdateArrays()
        {
            m_selectedRenderers = m_editor.Selection.GetComponents<Renderer>();
            m_selectedTransforms = m_editor.Selection.GetExposedToEditor().Select(o => o.transform).ToArray();
            m_bounds = new Bounds[m_selectedTransforms.Length];
        }

        private void UpdateIsInView()
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_window.Camera);
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int i = 0; i < m_selectedTransforms.Length; ++i)
            {
                Transform selectedTransform = m_selectedTransforms[i];
                if(selectedTransform == null || (selectedTransform.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }

                bounds.center = selectedTransform.position;
                if (GeometryUtility.TestPlanesAABB(planes, bounds))
                {
                    m_panel.IsInView = true;
                    return;
                }
            }
          
            for (int i = 0; i < m_selectedRenderers.Count; ++i)
            {
                Renderer selectedRenderer = m_selectedRenderers[i];
                if(selectedRenderer == null || (selectedRenderer.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }
                if (GeometryUtility.TestPlanesAABB(planes, selectedRenderer.bounds))
                {
                    m_panel.IsInView = true;
                    return;
                }
            }

            m_panel.IsInView = false;
        }

        private void UpdatePanelTransform()
        {
            if(m_editor.Selection.activeGameObject == null)
            {
                return;     
            }

            for(int i = 0; i < m_selectedTransforms.Length; ++i)
            {
                m_bounds[i] = TransformUtility.CalculateBounds(m_selectedTransforms[i]);
            }

            Rect screenRect = TransformUtility.BoundsToScreenRect(m_window.Camera, m_bounds, true);

            if (TransformUtility.ScreenRectToLocalRectInRectangle((RectTransform)m_panelTransform.parent, screenRect, CanvasCamera, out Rect localRect))
            {
                RectTransform parent = (RectTransform)m_panelTransform.parent;
                Vector2 pos = localRect.center;
                float minY = localRect.center.y - Mathf.Max(localRect.height / 2, 50);

                pos.y = minY - m_spacing;
                pos.y -= m_panelTransform.rect.height;

                if (pos.y < m_marginBottom)
                {
                    pos.y = m_marginBottom;
                }

                if (pos.y > parent.rect.height - m_panelTransform.rect.height - m_marginTop)
                {
                    pos.y = parent.rect.height - m_panelTransform.rect.height - m_marginTop;
                }

                pos.x -= m_panelTransform.rect.width / 2;

                if (pos.x < m_marginLeft)
                {
                    pos.x = m_marginLeft;
                }

                if (pos.x > parent.rect.width - m_panelTransform.rect.width - m_marginRight)
                {
                    pos.x = parent.rect.width - m_panelTransform.rect.width - m_marginRight;
                }

                m_panelTransform.localPosition = pos;
            }
        }

        private void OnTransformChanged(ExposeToEditor obj)
        {
            if(m_transformChanged)
            {
                return;
            }

            if(m_editor.Selection.IsSelected(obj.gameObject))
            {
                m_transformChanged = true;
                m_forceInvisible = 0.3f;
                m_panelCanvasGroup.alpha = 0;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            UpdateArrays();
            m_transformChanged = true;
            m_forceInvisible = 0;
        }

        private void OnPanelRectTransformChanged()
        {
            if(!m_transformChanged)
            {
                UpdatePanelTransform();
            }

            m_transformChanged = true;
            m_forceInvisible = 0;
        }
    }
}

