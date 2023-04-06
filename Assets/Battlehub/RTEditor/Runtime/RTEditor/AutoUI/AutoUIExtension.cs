
namespace Battlehub.RTEditor.UI
{
    public partial class AutoUIExtension : EditorExtension
    {
        private AutoUI m_autoUI;

        protected override void OnInit()
        {
            m_autoUI = new AutoUI();
            
        }

        protected override void OnCleanup()
        {
            m_autoUI.Dispose();
        }

       
        /*
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Example();
            }
        }

        private void Example()
        {
           // m_autoUI.CreateDialog(new MasterDetailsViewModelExample(), new DataModelExample());
            m_autoUI.CreateDialog(200, 400, false, new SimpleProceduralExample());
        }
        */

    }
}
