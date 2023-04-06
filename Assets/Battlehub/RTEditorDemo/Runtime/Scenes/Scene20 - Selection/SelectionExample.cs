using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene21
{
    /// <summary>
    // This example shows how to handle selection changes and select objects programmatically
    /// </summary>
    public class SelectionExample : MonoBehaviour
    {
        private IRTE m_editor;
        private int m_index = -1;

        public bool IsRootSelectionMode
        {
            get { return m_editor.Tools.SelectionMode == SelectionMode.Root; }
            set {  m_editor.Tools.SelectionMode = value ? SelectionMode.Root : SelectionMode.Part;  }
        }

        private void Awake()
        {
            //get reference to a runtime editor object
            m_editor = IOC.Resolve<IRTE>();

            //subscribe to the selection changed event
            m_editor.Selection.SelectionChanged += OnSelectionChanged;
        }


        private void OnDestroy()
        {
            if(m_editor.Selection != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
                m_editor = null;
            }
        }

        public void ClearSelection()
        {
            //unselect all 
            m_editor.Selection.objects = null;
        }

        public void SelectPrevious()
        {
            GameObject objectToSelect = GetPrevious();

            //select object
            m_editor.Selection.activeObject = objectToSelect;

        }

        public void SelectNext()
        {
            GameObject objectToSelect = GetNext();

            //select object
            m_editor.Selection.activeObject = objectToSelect;
        }


        /// <summary>
        /// Selection changed event handler
        /// </summary>
        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            Debug.Log($"{m_editor.Selection.Length} object(s) selected:");

            if (m_editor.Selection.Length == 0)
            {
                m_index = -1;
            }
            else
            {
                foreach (var obj in m_editor.Selection.objects)
                {
                    Debug.Log(obj.name);
                }
            }
        }

        private GameObject GetPrevious()
        {
            ExposeToEditor[] selectableObjects = m_editor.Object.Get(IsRootSelectionMode).ToArray();
            if (selectableObjects.Length > 0)
            {
                m_index = (m_index == 0 || m_index == -1) ? selectableObjects.Length - 1 : m_index - 1;
                return selectableObjects[m_index].gameObject;
            }
            return null;
        }

        private GameObject GetNext()
        {
            ExposeToEditor[] selectableObjects = m_editor.Object.Get(IsRootSelectionMode).ToArray();
            if (selectableObjects.Length > 0)
            {
                m_index = ++m_index % selectableObjects.Length;
                return selectableObjects[m_index].gameObject;
            }
            return null;
        }

    }
}

