using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class MethodCaller : PropertyEditor
    {
        private Button m_button;
        private void Start()
        {
            m_button = GetComponentInChildren<Button>(true);
            if(m_button != null)
            {
                m_button.onClick.AddListener(Click);
            }
        }

        private void OnDestroy()
        {
            if (m_button != null)
            {
                m_button.onClick.RemoveListener(Click);
            }
        }

        private void Click()
        {
            MethodInfo methodInfo = MemberInfo as MethodInfo;
            if(methodInfo != null && methodInfo.GetParameters().Length == 0)
            {
                methodInfo.Invoke(Accessor, null);
            }
            RaiseValueChanged();
        }
    }

}

