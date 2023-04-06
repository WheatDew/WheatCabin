using System;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls.Binding
{
    public abstract class ControlBinding : AbstractMemberBinding
    {
        public EventBindingSlim[] m_eventBindings;

        public OneWayPropertyBindingSlim[] m_oneWayPropertyBindings;

        public TwoWayPropertyBindingSlim[] m_twoWayPropertyBindings;

        public abstract Component TargetControl
        {
            get;
        }

        public override void Connect()
        {
            if(m_eventBindings != null)
            {
                for (int i = 0; i < m_eventBindings.Length; ++i)
                {
                    m_eventBindings[i].Connect(this);
                }
            }
            
            if(m_oneWayPropertyBindings != null)
            {
                for (int i = 0; i < m_oneWayPropertyBindings.Length; ++i)
                {
                    m_oneWayPropertyBindings[i].Connect(this);
                }
            }
            
            if(m_twoWayPropertyBindings != null)
            {
                for (int i = 0; i < m_twoWayPropertyBindings.Length; ++i)
                {
                    m_twoWayPropertyBindings[i].Connect(this);
                }
            }
        }

        public override void Disconnect()
        {
            if (m_eventBindings != null)
            {
                for (int i = 0; i < m_eventBindings.Length; ++i)
                {
                    m_eventBindings[i].Disconnect();
                }
            }

            if(m_oneWayPropertyBindings != null)
            {
                for (int i = 0; i < m_oneWayPropertyBindings.Length; ++i)
                {
                    m_oneWayPropertyBindings[i].Disconnect();
                }
            }
            
            if(m_twoWayPropertyBindings != null)
            {
                for (int i = 0; i < m_twoWayPropertyBindings.Length; ++i)
                {
                    m_twoWayPropertyBindings[i].Disconnect();
                }
            }
        }

        #region EventBindingSlim

        [Serializable]
        public class EventBindingSlim 
        {
            /// <summary>
            /// Name of the method in the view model to bind to.
            /// </summary>
            public string ViewModelMethodName
            {
                get { return viewModelMethodName; }
                set { viewModelMethodName = value; }
            }

            [SerializeField]
            private string viewModelMethodName;

            /// <summary>
            /// Name of the event in the view to bind to.
            /// </summary>
            public string ViewEventName
            {
                get { return viewEventName; }
                set { viewEventName = value; }
            }

            [SerializeField]
            private string viewEventName;

            public EventBindingSlim()
            {
            }

            public EventBindingSlim(string viewEventName, string viewModelMethodName)
            {
                this.viewEventName = viewEventName;
                this.viewModelMethodName = viewModelMethodName;
            }

            /// <summary>
            /// Watches a Unity event for updates.
            /// </summary>
            private UnityEventWatcher eventWatcher;

            public void Connect(ControlBinding parentBinding)
            {
                string methodName;
                object viewModel;
                parentBinding.ParseViewModelEndPointReference(
                    viewModelMethodName,
                    out methodName,
                    out viewModel
                );

                var viewModelMethod = viewModel.GetType().GetMethod(methodName, new Type[0]);

                string eventName;
                Component view;
                parentBinding.ParseViewEndPointReference(viewEventName, out eventName, out view);

                eventWatcher = new UnityEventWatcher(view, eventName,
                    () =>
                    {
                        if (viewModelMethod != null)
                        {
                            viewModelMethod.Invoke(viewModel, new object[0]);
                        }
                    });
            }

            public void Disconnect()
            {
                if (eventWatcher != null)
                {
                    eventWatcher.Dispose();
                    eventWatcher = null;
                }
            }
        }

        #endregion

        #region OneWayPropertyBindingSlim
        [Serializable]
        public class OneWayPropertyBindingSlim
        {
            [SerializeField]
            private string m_viewPropertyName;
            public string ViewPropertyName
            {
                get { return m_viewPropertyName; }
                set { m_viewPropertyName = value; }
            }

            
            [SerializeField]
            private string m_viewModelPropertyName = null;
            public string ViewModelPropertyName
            {
                get { return m_viewModelPropertyName; }
                set { m_viewModelPropertyName = value; }
            }

            [SerializeField]
            private string m_viewAdapterTypeName = null;
            public string ViewAdapterTypeName
            {
                get { return m_viewAdapterTypeName; }
                set { m_viewAdapterTypeName = value; }
            }

            [SerializeField]
            private AdapterOptions m_viewAdapterOptions = null;
            public AdapterOptions ViewAdapterOptions
            {
                get { return m_viewAdapterOptions; }
                set { m_viewAdapterOptions = value; }
            }

            private PropertyWatcher m_viewModelWatcher;

            public void Connect(ControlBinding parentBinding)
            {
                string propertyName;
                Component view;
                parentBinding.ParseViewEndPointReference(m_viewPropertyName, out propertyName, out view);

               
                var viewModelEndPoint = parentBinding.MakeViewModelEndPoint(m_viewModelPropertyName, null, null);
                var propertySync = new PropertySync(
                    viewModelEndPoint,
                    new PropertyEndPoint(
                        view,
                        propertyName,
                        CreateAdapter(m_viewAdapterTypeName),
                        m_viewAdapterOptions,
                        "view",
                        parentBinding.TargetControl
                    ),
                    null,
                    parentBinding.TargetControl
                );

                m_viewModelWatcher = viewModelEndPoint.Watch(
                    () => propertySync.SyncFromSource()
                );

                propertySync.SyncFromSource();
            }

            public void Disconnect()
            {
                if (m_viewModelWatcher != null)
                {
                    m_viewModelWatcher.Dispose();
                    m_viewModelWatcher = null;
                }
            }
        }
        #endregion

        #region TwoWayPropertyBindingSlim

        [Serializable]
        public class TwoWayPropertyBindingSlim
        {
            [SerializeField]
            private string m_viewPropertyName;
            public string ViewPropertyName
            {
                get { return m_viewPropertyName; }
                set { m_viewPropertyName = value; }
            }


            [SerializeField]
            private string m_viewEventName;
            public string ViewEventName
            {
                get { return m_viewEventName; }
                set { m_viewEventName = value; }
            }


            [SerializeField]
            private string m_viewModelPropertyName = null;
            public string ViewModelPropertyName
            {
                get { return m_viewModelPropertyName; }
                set { m_viewModelPropertyName = value; }
            }

            [SerializeField]
            private string m_viewAdapterTypeName = null;
            public string ViewAdapterTypeName
            {
                get { return m_viewAdapterTypeName; }
                set { m_viewAdapterTypeName = value; }
            }

            [SerializeField]
            private AdapterOptions m_viewAdapterOptions = null;
            public AdapterOptions ViewAdapterOptions
            {
                get { return m_viewAdapterOptions; }
                set { m_viewAdapterOptions = value; }
            }

            [SerializeField]
            private string m_viewModelAdapterTypeName;

            public string ViewModelAdapterTypeName
            {
                get { return m_viewModelAdapterTypeName; }
                set { m_viewModelAdapterTypeName = value; }
            }

           
            [SerializeField]
            private AdapterOptions viewModelAdapterOptions;
            public AdapterOptions ViewModelAdapterOptions
            {
                get { return viewModelAdapterOptions; }
                set { viewModelAdapterOptions = value; }
            }

            private PropertyWatcher m_viewModelWatcher;
            private UnityEventWatcher m_unityEventWatcher;

            public void Connect(ControlBinding parentBinding)
            {
                string propertyName;
                Component view;
                parentBinding.ParseViewEndPointReference(m_viewPropertyName, out propertyName, out view);

                var viewModelEndPoint = parentBinding.MakeViewModelEndPoint(m_viewModelPropertyName, m_viewModelAdapterTypeName, viewModelAdapterOptions);
                var propertySync = new PropertySync(
                    viewModelEndPoint,
                    new PropertyEndPoint(
                        view,
                        propertyName,
                        CreateAdapter(m_viewAdapterTypeName),
                        m_viewAdapterOptions,
                        "view",
                        parentBinding.TargetControl
                    ),
                    null,
                    parentBinding.TargetControl
                );

                m_viewModelWatcher = viewModelEndPoint.Watch(
                    () => propertySync.SyncFromSource()
                );


                string eventName;
                string eventComponentType;
                ParseEndPointReference(m_viewEventName, out eventName, out eventComponentType);

                var eventView = parentBinding.GetComponent(eventComponentType);

                m_unityEventWatcher = new UnityEventWatcher(
                    eventView,
                    eventName,
                    () => propertySync.SyncFromDest()
                );

                propertySync.SyncFromSource();
            }

            public void Disconnect()
            {
                if (m_viewModelWatcher != null)
                {
                    m_viewModelWatcher.Dispose();
                    m_viewModelWatcher = null;
                }

                if (m_unityEventWatcher != null)
                {
                    m_unityEventWatcher.Dispose();
                    m_unityEventWatcher = null;
                }
            }
        }
        #endregion
    }
}
