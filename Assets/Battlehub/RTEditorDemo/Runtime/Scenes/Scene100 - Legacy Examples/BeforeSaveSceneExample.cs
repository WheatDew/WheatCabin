using Battlehub.RTCommon;
using Battlehub.UIControls;

namespace Battlehub.RTEditor
{
    public class BeforeSaveSceneExample : EditorExtension
    {
        private IRuntimeEditor m_editor;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.BeforeSceneSave += OnBeforeSceneSave;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if(m_editor != null)
            {
                m_editor.BeforeSceneSave -= OnBeforeSceneSave;
            }
        }

        private void OnBeforeSceneSave(CancelArgs args)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            args.Cancel = true;
            wm.MessageBox("Unable to save scene", "Unable to save scene");
        }
    }
}

