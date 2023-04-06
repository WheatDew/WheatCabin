using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class SliderOverride : Slider
    {
        public UnityEvent onEndEdit;

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            onEndEdit.Invoke();
        }
    }

}
