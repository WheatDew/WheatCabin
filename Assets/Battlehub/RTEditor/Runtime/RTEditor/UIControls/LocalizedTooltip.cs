using Battlehub.RTCommon;
using Battlehub.UIControls.TooltipControl;

namespace Battlehub.RTEditor
{
    public class LocalizedTooltip : Tooltip
    {
        public override string Text
        {
            get { return IOC.Resolve<ILocalization>().GetString(base.Text); }
            set { base.Text = value; }
        }
    }

}
