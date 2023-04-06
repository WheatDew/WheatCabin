using Battlehub.RTCommon;
using UnityEngine;
using System.Linq;
namespace Battlehub.RTHandles
{
    public class RuntimeHighlightComponent : MonoBehaviour
    {
        private IRTE m_editor;
        private IRenderersCache m_cache;

        private bool m_updateRenderers;
        private Renderer[] m_allRenderers;
        private Renderer[] m_pickedRenderers;
        private IRuntimeSelectionComponent m_selectionComponent;
        private CameraMovementTracker m_tracker;

        private Color32[] m_texPixels;
        private Vector2Int m_texSize;

        private void Start()
        {
            IOC.Register("HighlightRenderers", m_cache = gameObject.AddComponent<RenderersCache>());

            m_editor = IOC.Resolve<IRTE>();
            m_tracker = new CameraMovementTracker();
            m_tracker.Editor = m_editor;
            m_tracker.ActiveWindow = m_editor.ActiveWindow;
            if (m_tracker.ActiveWindow != null && m_editor.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                m_selectionComponent = m_tracker.ActiveWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
            }

            m_editor.Object.Enabled += OnObjectEnabled;
            m_editor.Object.Disabled += OnObjectDisabled;
            m_editor.Object.ComponentAdded += OnComponentAdded;

            m_editor.Selection.SelectionChanged += OnSelectionChanged;
            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
        }

        private void OnDestroy()
        {
            IOC.Unregister("HighlightRenderers", m_cache);

            if (m_editor != null)
            {
                if (m_editor.Object != null)
                {
                    m_editor.Object.Enabled -= OnObjectEnabled;
                    m_editor.Object.Disabled -= OnObjectDisabled;
                    m_editor.Object.ComponentAdded -= OnComponentAdded;
                }

                if (m_editor.Selection != null)
                {
                    m_editor.Selection.SelectionChanged -= OnSelectionChanged;
                }

                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
            }
        }

        private void Update()
        {
            if (m_selectionComponent == null)
            {
                return;
            }


            bool wasMoving = m_tracker.IsMoving;
            m_tracker.Track();
            if (!m_tracker.IsMoving)
            {
                m_updateRenderers |= wasMoving;
                if (m_updateRenderers)
                {
                    m_updateRenderers = false;
                    m_allRenderers = m_editor.Object.Get(true).Where(go => go.ActiveSelf).SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray();
                    m_texPixels = m_selectionComponent.BoxSelection.BeginPick(out m_texSize, m_allRenderers);
                    m_pickedRenderers = null;
                }

                BaseHandle handle = null;
                switch (m_editor.Tools.Current)
                {
                    case RuntimeTool.Move:
                        handle = m_selectionComponent.PositionHandle;
                        break;
                    case RuntimeTool.Rotate:
                        handle = m_selectionComponent.RotationHandle;
                        break;
                    case RuntimeTool.Scale:
                        handle = m_selectionComponent.ScaleHandle;
                        break;
                    case RuntimeTool.Rect:
                        handle = m_selectionComponent.RectTool;
                        break;
                    case RuntimeTool.Custom:
                        handle = m_selectionComponent.CustomHandle;
                        break;
                }

                if (IsPointerOver(handle) || m_editor.Tools.ActiveTool == m_selectionComponent.BoxSelection && m_selectionComponent.BoxSelection != null)
                {
                    m_pickedRenderers = null;
                    m_cache.Clear();
                    return;
                }

                bool updateCache = false;
                Renderer[] renderers = m_selectionComponent.BoxSelection.EndPick(m_texPixels, m_texSize, m_allRenderers);

                if (m_pickedRenderers == null || !AreEqual(m_pickedRenderers, renderers))
                {
                    m_pickedRenderers = renderers;
                    if (m_editor.Tools.SelectionMode == SelectionMode.Root)
                    {
                        renderers = RuntimeSelectionUtil.GetRoots(renderers)
                            .OfType<GameObject>()
                            .SelectMany(go => go.GetComponentsInChildren<Renderer>())
                            .ToArray();
                    }
                    updateCache = true;
                }

                if (updateCache)
                {
                    m_cache.Clear();
                    m_cache.Add(renderers, true, true);
                }
            }
            else
            {
                m_pickedRenderers = null;
                m_cache.Clear();
            }
        }

        private bool AreEqual(Renderer[] pickedRenderers, Renderer[] renderers)
        {
            if (pickedRenderers.Length != renderers.Length)
            {
                return false;
            }

            for (int i = 0; i < pickedRenderers.Length; ++i)
            {
                if (pickedRenderers[i] != renderers[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsPointerOver(BaseHandle handle)
        {
            return handle != null && handle.SelectedAxis != RuntimeHandleAxis.None;
        }

        private void OnObjectEnabled(ExposeToEditor obj)
        {
            m_updateRenderers = true;
        }

        private void OnObjectDisabled(ExposeToEditor obj)
        {
            m_updateRenderers = true;
        }

        private void OnComponentAdded(ExposeToEditor obj, Component arg)
        {
            m_updateRenderers = true;
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            m_updateRenderers = true;
        }

        private void OnActiveWindowChanged(RuntimeWindow window)
        {
            if (m_editor.ActiveWindow != null && m_editor.ActiveWindow.WindowType == RuntimeWindowType.Scene)
            {
                m_updateRenderers = true;
                m_tracker.ActiveWindow = m_editor.ActiveWindow;
                m_selectionComponent = m_editor.ActiveWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
            }
            else
            {
                m_tracker.ActiveWindow = null;
                m_selectionComponent = null;
                m_allRenderers = null;
            }
        }

        private class CameraMovementTracker
        {
            private Ray m_prevRay;
            private RuntimeWindow m_activeWindow;
            public RuntimeWindow ActiveWindow
            {
                get { return m_activeWindow; }
                set
                {
                    if (m_activeWindow != value)
                    {
                        m_activeWindow = value;
                        if (m_activeWindow != null)
                        {
                            m_prevRay = Ray;
                        }
                    }
                }
            }

            private Ray Ray
            {
                get { return new Ray(m_activeWindow.Camera.transform.position, m_activeWindow.Camera.transform.forward); }
            }

            private IRTE m_editor;
            public IRTE Editor
            {
                get { return m_editor; }
                set { m_editor = value; }
            }

            private float m_cooldownTime;
            private bool m_isMoving;
            public bool IsMoving
            {
                get { return m_isMoving; }
            }

            public void Track()
            {
                if (m_activeWindow == null)
                {
                    return;
                }

                Ray ray = Ray;
                if (m_prevRay.origin != ray.origin || m_prevRay.direction != ray.direction || Editor.Tools.IsViewing || !m_activeWindow.IsPointerOver)
                {
                    m_isMoving = true;
                    m_prevRay = ray;
                    m_cooldownTime = Time.time + 0.2f;
                }
                else
                {
                    if (m_cooldownTime <= Time.time)
                    {
                        m_isMoving = false;
                    }
                }
            }
        }
    }
}
