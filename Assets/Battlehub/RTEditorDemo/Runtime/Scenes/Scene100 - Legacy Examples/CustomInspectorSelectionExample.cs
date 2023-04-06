using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class CustomInspectorSelectionExample : RuntimeWindowExtension
    {
        private IRuntimeSelection m_selection = new RuntimeSelection();

        public override string WindowTypeName
        {
            get { return RuntimeWindowType.Inspector.ToString(); }
        }

        protected override void Extend(RuntimeWindow window)
        {
            window.IOCContainer.Register(m_selection);
        }

        protected override void Cleanup(RuntimeWindow window)
        {
            window.IOCContainer.Unregister(m_selection);
        }

        private int m_index = 0;
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GameObject go =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = $"TestGo {m_index++}";
                m_selection.activeGameObject = go;
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                m_selection.activeGameObject = null;
            }
        }
    }
}

