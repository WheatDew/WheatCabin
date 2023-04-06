using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Views
{
    public class ManageProjectsView : View
    {
        [NonSerialized]
        public UnityEvent CreateProject = new UnityEvent();

        [NonSerialized]
        public UnityEvent DestroyProject = new UnityEvent();

        [SerializeField]
        private Button m_createProjectButton = null;

        [SerializeField]
        private Button m_destroyProjectButton = null;

        protected override void Awake()
        {
            base.Awake();
            UnityEventHelper.AddListener(m_createProjectButton, btn => btn.onClick, OnCreateClick);
            UnityEventHelper.AddListener(m_destroyProjectButton, btn => btn.onClick, OnDestroyClick);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnityEventHelper.RemoveListener(m_createProjectButton, btn => btn.onClick, OnCreateClick);
            UnityEventHelper.RemoveListener(m_destroyProjectButton, btn => btn.onClick, OnDestroyClick);
        }

        private void OnCreateClick()
        {
            CreateProject?.Invoke();
        }

        private void OnDestroyClick()
        {
            DestroyProject?.Invoke();
        }
    }
}
