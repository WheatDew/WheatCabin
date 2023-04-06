using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene26
{
    public class RegisterCustomHandleExample : RuntimeWindowExtension
    {
        private readonly Dictionary<RuntimeWindow, CustomHandleExample> m_windowToHandle = new Dictionary<RuntimeWindow, CustomHandleExample>();

        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            GameObject go = new GameObject("CustomHandle");
            go.transform.SetParent(transform, false);
            go.gameObject.SetActive(false);

            CustomHandleExample handle = go.AddComponent<CustomHandleExample>();
            handle.Window = window;

            m_windowToHandle.Add(window, handle);
        }

        protected override void Cleanup(RuntimeWindow window)
        {
            base.Cleanup(window);
             
            if (m_windowToHandle.TryGetValue(window, out CustomHandleExample handle))
            {
                Destroy(handle.gameObject);
                m_windowToHandle.Remove(window);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                foreach (RuntimeWindow window in m_windowToHandle.Keys)
                {
                    IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                    selectionComponent.CustomHandle = m_windowToHandle[window];
                }

                IRTE rte = IOC.Resolve<IRTE>();
                rte.Tools.Current = RuntimeTool.Custom;
            }
        }

       
    }
}
