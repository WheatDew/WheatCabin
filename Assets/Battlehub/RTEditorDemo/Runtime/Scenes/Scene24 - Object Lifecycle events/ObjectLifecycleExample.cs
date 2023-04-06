using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene24
{
    /// <summary>
    /// The runtime editor provides object life cycle events through the IRuntimeObjects interface.
    /// </summary>
    public class ObjectLifecycleExample : MonoBehaviour
    {
        private IRTE m_editor;

        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Object.Awaked += OnObjectAwaked;
            m_editor.Object.Enabled += OnObjectEnabled;
            m_editor.Object.Started += OnObjectStarted;
            m_editor.Object.Disabled += OnObjectDisabled;
            m_editor.Object.Destroying += OnObjectDestroying;
            m_editor.Object.Destroyed += OnObjectDestroyed;

            // This event occurs when the object is not completely destroyed,
            // and the destruction operation is written to the undo stack and can be undone in the future
            m_editor.Object.MarkAsDestroyedChanging += OnMarkAsDestroyedChanging;
            m_editor.Object.MarkAsDestroyedChanged += OnMarkAsDestroyedChanged;

            m_editor.Object.NameChanged += OnObjectNameChanged;
            m_editor.Object.ParentChanged += OnObjectParentChanged;
            m_editor.Object.TransformChanged += OnObjectTransformChanged;
            
            m_editor.Object.ComponentAdded += OnComponentAdded;
            m_editor.Object.ComponentDestroyed += OnComponentDestroyed;
        }

        private void OnDestroy()
        {
            if (m_editor != null && m_editor.Object != null)
            {
                m_editor.Object.Awaked -= OnObjectAwaked;
                m_editor.Object.Enabled -= OnObjectEnabled;
                m_editor.Object.Started -= OnObjectStarted;
                m_editor.Object.Disabled -= OnObjectDisabled;
                m_editor.Object.Destroying -= OnObjectDestroying;
                m_editor.Object.Destroyed -= OnObjectDestroyed;

                m_editor.Object.MarkAsDestroyedChanging -= OnMarkAsDestroyedChanging;
                m_editor.Object.MarkAsDestroyedChanged -= OnMarkAsDestroyedChanged;

                m_editor.Object.NameChanged -= OnObjectNameChanged;
                m_editor.Object.ParentChanged -= OnObjectParentChanged;
                m_editor.Object.TransformChanged -= OnObjectTransformChanged;

                m_editor.Object.ComponentAdded -= OnComponentAdded;
                m_editor.Object.ComponentDestroyed -= OnComponentDestroyed;
            }
            m_editor = null;
        }
   
        private void OnObjectAwaked(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Awaked");
        }

        private void OnObjectEnabled(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Enabled");
        }

        private void OnObjectStarted(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Started");
        }

        private void OnObjectNameChanged(ExposeToEditor obj)
        {
            Debug.Log($"name changed to {obj.name}");
        }

        private void OnObjectParentChanged(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue)
        {
            Debug.Log($"{obj.name} Parent Changed from {(oldValue != null ? oldValue.name : "null")} to {(newValue != null ? newValue.name : "null")} ");
        }

        private void OnObjectTransformChanged(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} transform changed");
        }

        private void OnMarkAsDestroyedChanging(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Mark As Destroyed Changing");
        }

        private void OnMarkAsDestroyedChanged(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Mark As Destroyed: {obj.MarkAsDestroyed}");
        }

        private void OnObjectDisabled(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Disabled");
        }

        private void OnObjectDestroying(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Destroying");
        }

        private void OnObjectDestroyed(ExposeToEditor obj)
        {
            Debug.Log($"{obj.name} Destroyed");
        }

        private void OnComponentAdded(ExposeToEditor obj, Component component)
        {
            Debug.Log($"{component.GetType().Name} added to {obj.name}");
        }

        private void OnComponentDestroyed(ExposeToEditor obj, Component component)
        {
            Debug.Log($"{component.GetType().Name} destroyed on {obj.name}");
        }   
    }
}
