using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.DockPanels;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.Controls
{
    public class MobileContextPanelToggle : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_toggle;

        [SerializeField]
        private string m_windowName;

        [SerializeField]
        private float m_regionMinHeight = 30.0f;

        [SerializeField]
        private float m_contentPadding = 8.0f;

        private Transform m_window;

        private IWindowManager m_wm;

        private void Awake()
        {
            if(m_toggle == null)
            {
                m_toggle = GetComponent<Toggle>();
            }

            m_toggle.onValueChanged.AddListener(OnValueChanged);

            m_wm = IOC.Resolve<IWindowManager>();
        }

        private void OnDisable()
        {
            if (m_wm != null)
            {
                if (m_window != null)
                {
                    m_wm.WindowDestroyed -= OnWindowDestroyed;
                    m_wm.DestroyWindow(m_window);
                    m_window = null;

                    if(m_toggle != null)
                    {
                        m_toggle.SetIsOnWithoutNotify(false);
                        m_toggle.interactable = true;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
                m_toggle = null;
            }

            if(m_window != null)
            {
                m_wm.WindowDestroyed -= OnWindowDestroyed;
                m_wm = null;
            }
            
        }

        private void OnValueChanged(bool value)
        {
            if (value)
            {
                m_window = m_wm.CreateDropdown(m_windowName, (RectTransform)m_toggle.transform, true);
                if (m_window != null)
                {
                    InitWindow();

                    FitRegionToContentSize();

                    m_wm.WindowDestroyed += OnWindowDestroyed;
                }
            }
            else
            {
                m_wm.WindowDestroyed -= OnWindowDestroyed;
            }
        }

        private void OnWindowDestroyed(Transform window)
        {
            if(m_window == window)
            {
                m_window = null;
                m_wm.WindowDestroyed -= OnWindowDestroyed;
                if (m_toggle != null)
                {
                    m_toggle.SetIsOnWithoutNotify(false);
                    m_toggle.interactable = false;

                    if(isActiveAndEnabled)
                    {
                        StartCoroutine(ReEnable());
                    }
                    else
                    {
                        m_toggle.interactable = false;
                    }
                }
            }
        }

        private IEnumerator ReEnable()
        {
            if(Input.GetMouseButtonDown(0))
            {
                yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            }

            m_toggle.interactable = true;
        }

        private void InitWindow()
        {
            Template sourceTemplate = GetComponent<Template>();
            Template targetTemplate = m_window.GetComponentInChildren<Template>(true);
            if (sourceTemplate != null && targetTemplate != null)
            {
                targetTemplate.InitChildBindings(sourceTemplate.GetViewModel());
                targetTemplate.gameObject.SetActive(true);
            }
        }

        private void FitRegionToContentSize()
        {
            ContentSizeFitter contentSizeFitter = m_window.GetComponentInChildren<ContentSizeFitter>();
            if (contentSizeFitter != null && contentSizeFitter.verticalFit == ContentSizeFitter.FitMode.PreferredSize)
            {
                Region region = Region.FindRegion(m_window);
                ScrollRect scrollRect = m_window.GetComponentInChildren<ScrollRect>();
                CanvasGroup canvasGroup = scrollRect.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0;
                }
                
                RectTransform regionRT = (RectTransform)region.transform;
                RectTransform contentRT = (RectTransform)contentSizeFitter.transform;

                RectTransformChangeListener rtcl = contentSizeFitter.gameObject.AddComponent<RectTransformChangeListener>();
                RectTransformChanged onRectTranformChaged = null;
                onRectTranformChaged = () =>
                {
                    regionRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentRT.rect.height + m_contentPadding);
                    region.MinHeight = m_regionMinHeight;
                    region.Root.RefreshDropdownRegion(m_window, (RectTransform)m_toggle.transform);

                    if (contentRT.rect.height > 0)
                    {
                        rtcl.RectTransformChanged -= onRectTranformChaged;
                        if(canvasGroup != null)
                        {
                            StartCoroutine(SetAlpha(canvasGroup, 1));
                        }
                    }
                };

                rtcl.RectTransformChanged += onRectTranformChaged;
            }
        }

        private IEnumerator SetAlpha(CanvasGroup group, float alpha)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            group.alpha = alpha;
        }
    }
}
