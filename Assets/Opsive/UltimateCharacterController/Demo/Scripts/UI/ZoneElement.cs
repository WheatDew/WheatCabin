/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A ZoneElement is the selectable item within the ScrollRect.
    /// </summary>
    public class ZoneElement : Selectable, IPointerClickHandler
    {
        [Tooltip("A reference to the element text.")]
        [SerializeField] protected Shared.UI.Text m_Text;

        private ZoneSelection m_ZoneSelection;
        private int m_Index;
        private Image m_Image;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        /// <param name="demoZone">The DemoZone that the elemnt represents.</param>
        /// <param name="zoneSelection">The parent ZoneSelection component.</param>
        /// <param name="index">The index of the DemoZone.</param>
        public void Initialize(DemoManager.DemoZone demoZone, ZoneSelection zoneSelection, int index)
        {
            m_ZoneSelection = zoneSelection;
            m_Index = index;
            m_Image = GetComponentInChildren<Image>();

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.position = new Vector3(0, -(index * rectTransform.sizeDelta.y) - rectTransform.sizeDelta.y / 2);
            rectTransform.SetParent(zoneSelection.ZoneScrollRect.content, false);

            m_Text.text = demoZone.Header;
        }

        /// <summary>
        /// The pointer has entered the element.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            m_Image.color = currentSelectionState == SelectionState.Selected ? colors.selectedColor : colors.normalColor;
            m_ZoneSelection.FocusZone(m_Index);
        }

        /// <summary>
        /// The element has been selected.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            m_Image.color = colors.selectedColor;
            m_ZoneSelection.FocusZone(m_Index);
        }

        /// <summary>
        /// The element has been deselected.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            m_Image.color = colors.normalColor;
        }

        /// <summary>
        /// The pointer has clicked on the element.
        /// </summary>
        /// <param name="eventData">The event that triggered the event.</param>
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            m_ZoneSelection.LoadZone(m_Index);
        }
    }
}