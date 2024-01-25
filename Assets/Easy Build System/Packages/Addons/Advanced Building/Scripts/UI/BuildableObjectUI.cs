using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding.UI
{
    public class BuildableObjectUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_CanvasGroup;

        [SerializeField] RectTransform m_RectTransform;

        [Serializable]
        public class RequiredResourceSlot
        {
            [SerializeField] CarriableObject.ResourceType m_Resource;
            public CarriableObject.ResourceType Resource { get { return m_Resource; } }

            [SerializeField] GameObject m_Slot;
            public GameObject Slot { get { return m_Slot; } }

            [SerializeField] Text m_AmountText;
            public Text AmountText { get { return m_AmountText; } }
        }

        [SerializeField] RequiredResourceSlot[] m_RequiredResourceSlots;

        [SerializeField] Vector3 m_CustomItemOffset;

        void Update()
        {
            if (InteractionController.Instance.Interactable == null)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            if (InteractionController.Instance.Interactable.InteractableType != InteractableType.BUILDABLE)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            BuildableObject carriableObject = (BuildableObject)InteractionController.Instance.Interactable;

            List<BuildableObject.RequiredResource> slots = carriableObject.RequiredResources.ToList();

            for (int i = 0; i < m_RequiredResourceSlots.Length; i++)
            {
                BuildableObject.RequiredResource required = slots.Find(x => x.ResourceType == m_RequiredResourceSlots[i].Resource);

                if (required != null)
                {
                    m_RequiredResourceSlots[i].Slot.SetActive(true);

                    int requiredAmount = carriableObject.GetResourceCount(m_RequiredResourceSlots[i].Resource);
                    int currentAmount = carriableObject.GetCurrentResourceCount(m_RequiredResourceSlots[i].Resource);

                    m_RequiredResourceSlots[i].AmountText.text = currentAmount + "/" + requiredAmount;
                }
                else
                {
                    m_RequiredResourceSlots[i].Slot.SetActive(false);
                }
            }

            m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 1f, 15f * Time.deltaTime);

            if (carriableObject == null)
            {
                return;
            }

            float boundsMedian = GetMedian(carriableObject.MeshBounds.extents.x, carriableObject.MeshBounds.extents.y, carriableObject.MeshBounds.extents.z);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(carriableObject.MeshBounds.center + Camera.main.transform.right * boundsMedian);
            PositionAtScreenPoint(m_RectTransform, screenPosition + m_CustomItemOffset);
        }

        float GetMedian(params float[] values)
        {
            float sum = 0f;

            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum / values.Length;
        }

        void PositionAtScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, screenPosition, null, out Vector2 position))
                rectTransform.anchoredPosition = position;
        }
    }
}