using Battlehub.UIControls;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class ActivateInputFieldOnBeginEdit : MonoBehaviour
    {
        private VirtualizingItemContainer m_itemContainer;
        private TMP_InputField m_inputField;

        private void OnEnable()
        {
            m_inputField = GetComponent<TMP_InputField>();
            m_itemContainer = GetComponentInParent<VirtualizingItemContainer>();
            VirtualizingItemContainer.BeginEdit += OnBeginEdit;
        }

        private void OnDisable()
        {
            VirtualizingItemContainer.BeginEdit -= OnBeginEdit;
        }

        private void OnBeginEdit(object sender, System.EventArgs e)
        {
            if(!ReferenceEquals(sender, m_itemContainer))
            {
                return;
            }

            m_inputField.ActivateInputField();
            m_inputField.Select();
        }
    }
}

