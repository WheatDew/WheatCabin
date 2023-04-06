using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RuntimeEditorInput : RTEBaseInput
    {
        [SerializeField]
        protected KeyCode SaveSceneKey = KeyCode.S;

        private IRuntimeEditor m_editor;

        protected override void Start()
        {
            base.Start();
            m_editor = IOC.Resolve<IRuntimeEditor>();
        }

        protected virtual bool SaveSceneAction()
        {
            return Input.GetKeyDown(SaveSceneKey) && Input.GetKey(ModifierKey);
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            if(m_editor.IsOpened)
            {
                if (SaveSceneAction())
                {
                    m_editor.SaveScene();
                }
            }
        }
    }
}
