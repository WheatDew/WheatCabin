using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class ShowDropDown : MonoBehaviour
    {
        private Button m_showDropDownButton = null;
        private DockPanel m_dockPanels;

        private void Start()
        {
            m_dockPanels = GetComponentInParent<DockPanel>();

            m_showDropDownButton = GetComponent<Button>();
            m_showDropDownButton.onClick.AddListener(OnShowDropDownClick);
        }

        private void OnDestroy()
        {
            if(m_showDropDownButton != null)
            {
                m_showDropDownButton.onClick.RemoveListener(OnShowDropDownClick);
            }
        }

        private void OnShowDropDownClick()
        {
            RectTransform content = new GameObject("Empty").AddComponent<RectTransform>();
            Image image = content.gameObject.AddComponent<Image>();
            image.color = new Color32(0x27, 0x27, 0x27, 0xFF);

            Tab tab = Instantiate(m_dockPanels.TabPrefab);
            Vector2 size = new Vector2(300, 400);

            RectTransform anchor = (RectTransform)m_showDropDownButton.transform;
            m_dockPanels.AddDropdownRegion(tab, content, anchor, size, false);
        }
    }
}
