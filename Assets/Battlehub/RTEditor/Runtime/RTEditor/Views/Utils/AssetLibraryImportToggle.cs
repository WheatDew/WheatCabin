using UnityEngine;

using Battlehub.UIControls;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using Battlehub.RTSL.Interface;

namespace Battlehub.RTEditor
{
    public class AssetLibraryImportToggle : MonoBehaviour
    {
        private VirtualizingTreeView m_treeView;
        private VirtualizingTreeViewItem m_treeViewItem;
        private Toggle m_toggle;

        private ProjectItem m_target;
        public ProjectItem Target
        {
            get { return m_target; }
            set
            {
                m_target = value;
                if(m_treeView != null)
                {
                    m_toggle.isOn = m_treeView.IsItemSelected(m_target);
                }
            }
        }

        private void Start()
        {
            m_treeViewItem = GetComponentInParent<VirtualizingTreeViewItem>();
            m_treeView = GetComponentInParent<VirtualizingTreeView>();
            m_toggle = GetComponent<Toggle>();
            m_toggle.isOn = m_treeView.IsItemSelected(m_treeViewItem.Item);
            m_toggle.onValueChanged.AddListener(OnValueChanged);

            VirtualizingItemContainer.Selected += OnSelected;
            VirtualizingItemContainer.Unselected += OnUnselected;

            if(Target != null)
            {
                m_toggle.isOn = m_treeView.IsItemSelected(m_target);
            }
        }

        private void OnDestroy()
        {
            VirtualizingItemContainer.Selected -= OnSelected;
            VirtualizingItemContainer.Unselected -= OnUnselected;

            if (m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void OnSelected(object sender, System.EventArgs e)
        {
            if (sender == (object)m_treeViewItem)
            {
                m_toggle.isOn = true;
            }
        }

        private void OnUnselected(object sender, System.EventArgs e)
        {
            if (sender == (object)m_treeViewItem)
            {
                m_toggle.isOn = false;
            }
        }

        private void OnValueChanged(bool value)
        {
            if(value == m_treeView.IsItemSelected(m_treeViewItem.Item))
            {
                return;
            }

            HashSet<object> selection;
            if (m_treeView.SelectedItems != null)
            {
                selection = new HashSet<object>(m_treeView.SelectedItems.OfType<object>().ToList());
            }
            else
            {
                selection = new HashSet<object>();
            }

            if (value)
            {
                OnSelected(m_treeViewItem.TreeViewItemData.Item, selection);
            }
            else
            {
                OnUnselected(m_treeViewItem.TreeViewItemData.Item, selection);
            }
            m_treeView.SelectedItems = selection;
        }

        private void OnSelected(object item, HashSet<object> selection)
        {
            if(!selection.Contains(item))
            {
                selection.Add(item);
            }
            
            SelectDescendants(item, selection);
            SelectAncestors(item, selection);
        }

        private void OnUnselected(object item, HashSet<object> selection)
        {
            selection.Remove(item);
            UnselecteDescenants(item, selection);
            UnselectAncestors(item, selection);
        }

        private static void SelectAncestors(object item, HashSet<object> selection)
        {
            ProjectItem projectItem = (ProjectItem)item;
            while (projectItem.Parent != null)
            {
                projectItem = projectItem.Parent;
                if (!selection.Contains(projectItem))
                {
                    if (projectItem.Children.All(c => selection.Contains(c)))
                    {
                        selection.Add(projectItem);
                    }
                }
            }
        }

        private static void UnselectAncestors(object item, HashSet<object> selection)
        {
            ProjectItem projectItem = (ProjectItem)item;
            while (projectItem.Parent != null)
            {
                projectItem = projectItem.Parent;
                //if (projectItem.Children.All(c => !selection.Contains(c)))
                if (projectItem.Children.Any(c => !selection.Contains(c)))
                {
                    selection.Remove(projectItem);
                }
            }
        }

        private static void SelectDescendants(object item, HashSet<object> selection)
        {
            ProjectItem projectItem = (ProjectItem)item;
            if (projectItem.Children != null)
            {
                for (int i = 0; i < projectItem.Children.Count; ++i)
                {
                    if(!selection.Contains(projectItem.Children[i]))
                    {
                        selection.Add(projectItem.Children[i]);
                    }
                    
                    SelectDescendants(projectItem.Children[i], selection);
                }
            }
        }

        private static void UnselecteDescenants(object item, HashSet<object> selection)
        {
            ProjectItem projectItem = (ProjectItem)item;
            if (projectItem.Children != null)
            {
                for (int i = 0; i < projectItem.Children.Count; ++i)
                {
                    selection.Remove(projectItem.Children[i]);
                    UnselecteDescenants(projectItem.Children[i], selection);
                }
            }
        }

       
    }
}


