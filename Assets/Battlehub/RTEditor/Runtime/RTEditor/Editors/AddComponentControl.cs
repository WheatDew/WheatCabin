using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AddComponentControl : MonoBehaviour
    {
        public event Action<Type> ComponentSelected;

        [SerializeField]
        private TMP_Dropdown m_dropDown = null;
        private TMP_InputField m_filter = null;
        private VirtualizingTreeView m_treeView = null;
        
        private Type[] m_cache;
        private string m_filterText;
        private bool m_isOpened;

        private IRTE m_editor;
        private ILocalization m_localization;

        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_localization = IOC.Resolve<ILocalization>();
        }

        private void Update()
        {
            bool isOpened = m_dropDown.transform.childCount == 3;

            if(m_isOpened != isOpened)
            {
                m_isOpened = isOpened;
                if(m_isOpened)
                {
                    OnOpened();
                }
                else
                {
                    OnClosed();
                }
            }

            if(m_isOpened)
            {
                IInput input = m_editor.Input;
                if (input.GetKeyDown(KeyCode.DownArrow))
                {
                    m_treeView.Select();
                    m_treeView.IsFocused = true;
                }
                else if(input.GetKeyDown(KeyCode.Return))
                {
                    if(m_treeView.SelectedItem != null)
                    {
                        Hide();
                    }
                }
            }
        }

        private void OnOpened()
        {
            Type[] editableTypes = IOC.Resolve<IEditorsMap>().GetEditableTypes();

            m_filter = GetComponentInChildren<TMP_InputField>();
            if(m_filter != null)
            {
                m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                m_filter.text = m_filterText;

                if (!m_editor.TouchInput.IsTouchSupported)
                {
                    m_filter.Select();
                }
            }

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            m_treeView.CanDrag = false;
            m_treeView.CanReparent = false;
            m_treeView.CanReorder = false;
            m_treeView.CanSelectAll = false;
            m_treeView.CanMultiSelect = false;

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemClick += OnItemClick;
            m_cache = editableTypes.Where(t => t.IsSubclassOf(typeof(Component))).OrderBy(t => t.Name).ToArray();
            InstantApply(m_filterText);
        }

        private void OnClosed()
        {
            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemClick -= OnItemClick;

            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Type type = (Type)e.Item;
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = m_localization.GetString(string.Format("ID_RTEditor_CD_{0}", type.Name), type.Name);
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            StartCoroutine(CoHide());
        }

        private IEnumerator CoHide()
        {
            yield return new WaitForEndOfFrame();
            if(m_treeView.SelectedItem != null)
            {
                Hide();
            }
        }

        private void Hide()
        {
            m_dropDown.Hide();
            if (ComponentSelected != null)
            {
                ComponentSelected((Type)m_treeView.SelectedItem);
            }
        }

        private void OnFilterValueChanged(string text)
        {
            m_filterText = text;
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSeconds(0.3f);
            InstantApply(filter);
        }

        private void InstantApply(string filter)
        {
            if (m_treeView != null)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    m_treeView.Items = m_cache;
                }
                else
                {
                    m_treeView.Items = m_cache.Where(item => 
                        item.Name.ToLower().Contains(filter.ToLower()) ||
                        m_localization.GetString(string.Format("ID_RTEditor_CD_{0}", item.Name), item.Name).ToLower().Contains(filter.ToLower()));
                }
            }
        }
    }
}

