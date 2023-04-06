using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    /// <summary>
    /// This script is required to fix wrong input field text alignment (starting from unity 2019.1f)
    /// </summary>
    [DefaultExecutionOrder(101)]
    public class TMP_InputFieldAlignmentFix : MonoBehaviour
    {
        #pragma warning disable CS0414
        [SerializeField]
        private TMP_InputField m_inputField = null;
        #pragma warning restore CS0414

        private void Start()
        {
           //m_inputField.textComponent.verticalAlignment = VerticalAlignmentOptions.Top;
        }
    }
}
