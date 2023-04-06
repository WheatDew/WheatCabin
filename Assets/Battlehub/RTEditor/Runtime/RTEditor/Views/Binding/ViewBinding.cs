using Battlehub.RTEditor.Views;
using Battlehub.UIControls.Binding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityWeld.Binding.Internal;

namespace Battlehub.RTEditor.Binding
{
    public class ViewBinding : ControlBinding
    {
        private View m_view;

        public override Component TargetControl
        {
            get 
            {
                if(m_view == null)
                {
                    m_view = GetComponent<View>();
                }
                return m_view; 
            }
        }

        #region DragObjects

        [SerializeField]
        private string m_viewModelDragObjectsPropertyName = string.Empty;

        public string ViewModelDragObjectsPropertyName
        {
            get { return m_viewModelDragObjectsPropertyName; }
            set { m_viewModelDragObjectsPropertyName = value; }
        }

        private PropertySync m_dragObjectsPropertySync;

        private IEnumerable<object> m_dragObjects;
        public IEnumerable<object> DragObjects
        {
            get { return m_dragObjects; }
            set
            {
                if (m_dragObjects != value)
                {
                    m_dragObjects = value;
                    if (m_dragObjectsPropertySync != null)
                    {
                        m_dragObjectsPropertySync.SyncFromDest();
                    }
                }
            }
        }

        #endregion DragObjects

        #region CanDropObjects

        [SerializeField]
        private string m_viewModelCanDropObjectsPropertyName = string.Empty;

        public string ViewModelCanDropObjectsPropertyName
        {
            get { return m_viewModelCanDropObjectsPropertyName; }
            set { m_viewModelCanDropObjectsPropertyName = value; }
        }
        private PropertyWatcher m_viewModelCanDropObjectWatcher;

        #endregion


        public override void Connect()
        {
            m_view = GetComponent<View>();
            m_view.DragEnter.AddListener(OnDragEnter);

            base.Connect();
            if (!string.IsNullOrEmpty(m_viewModelDragObjectsPropertyName))
            {
                PropertyEndPoint dragItemsPropertyEndPoint = MakeViewModelEndPoint(m_viewModelDragObjectsPropertyName, null, null);
                m_dragObjectsPropertySync = new PropertySync(
                    dragItemsPropertyEndPoint,
                    new PropertyEndPoint(
                        this,
                        nameof(DragObjects),
                        null,
                        null,
                        "view",
                        this),
                    null,
                    this);
            }

            m_view.DragLeave.AddListener(OnDragLeave);
            m_view.Drop.AddListener(OnDrop);

            var viewModelEndPoint = MakeViewModelEndPoint(ViewModelCanDropObjectsPropertyName, null, null);
            var propertySync = new PropertySync(
                viewModelEndPoint,
                new PropertyEndPoint(
                    m_view,
                    nameof(View.CanDropExternalObjects),
                    CreateAdapter(null),
                    null,
                    "view",
                    m_view
                ),
                null, 
                this
            );

            m_viewModelCanDropObjectWatcher = viewModelEndPoint.Watch(
                () => propertySync.SyncFromSource()
            );

            // Copy the initial value over from the view-model.
            propertySync.SyncFromSource();
        }

        public override void Disconnect()
        {
            base.Disconnect();

            if(m_view != null)
            {
                m_view.DragEnter.RemoveListener(OnDragEnter);
                m_view.DragLeave.RemoveListener(OnDragLeave);
                m_view.Drop.RemoveListener(OnDrop);
                m_view = null;
            }

            if (m_viewModelCanDropObjectWatcher != null)
            {
                m_viewModelCanDropObjectWatcher.Dispose();
                m_viewModelCanDropObjectWatcher = null;
            }
        }

        private void OnDragEnter()
        {
            DragObjects = m_view.DragObjects;
        }

        private void OnDragLeave()
        {
            DragObjects = null;
        }

        private void OnDrop()
        {
            DragObjects = null;
        }
    }

}
