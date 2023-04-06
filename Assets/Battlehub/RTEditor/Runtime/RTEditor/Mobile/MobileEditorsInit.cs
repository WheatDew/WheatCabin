using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileEditorsInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_vector3Editor = null;

        [SerializeField]
        private GameObject m_vector2Editor = null;

        protected override void OnInit()
        {
            base.OnInit();

            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            if(m_vector3Editor != null)
            {
                editorsMap.RemoveMapping(typeof(Vector3));
                editorsMap.AddMapping(typeof(Vector3), m_vector3Editor, true, true);
                EnableStyling(m_vector3Editor);
            }

            if(m_vector2Editor != null)
            {
                editorsMap.RemoveMapping(typeof(Vector2));
                editorsMap.AddMapping(typeof(Vector2), m_vector2Editor, true, true);
                EnableStyling(m_vector2Editor);
            }   
        }



    }

}
