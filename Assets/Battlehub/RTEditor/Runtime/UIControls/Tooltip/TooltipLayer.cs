using UnityEngine;

namespace Battlehub.UIControls.TooltipControl
{
    public class TooltipLayer : MonoBehaviour
    {
        public RectTransform RectTransform
        {
            get { return (RectTransform)transform; }
        }
    }

}
