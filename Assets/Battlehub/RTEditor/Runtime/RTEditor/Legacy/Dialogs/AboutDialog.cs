using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class AboutDialog : RuntimeWindow
    {
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.About;
            base.AwakeOverride();
        }
    }
}

