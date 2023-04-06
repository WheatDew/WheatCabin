using TMPro;
using UnityEngine.UI;

namespace Battlehub.RTEditor.UI
{
    public static class AutoUIControlsExtensions 
    {
        public static void SetText(this Button button, string text)
        {
            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if(tmp != null)
            {
                tmp.text = text;
            }
        }

        public static string GetText(this Button button)
        {
            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if(tmp != null)
            {
                return tmp.text;
            }
            return null;
        }
    }
}
