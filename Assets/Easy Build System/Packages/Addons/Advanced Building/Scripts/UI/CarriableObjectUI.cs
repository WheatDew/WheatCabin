using UnityEngine;
using UnityEngine.UI;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding.UI
{
    public class CarriableObjectUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup m_CanvasGroup;

        [SerializeField] RectTransform m_RectTransform;

        [SerializeField] Text m_Text;

        [SerializeField] Vector3 m_CustomItemOffset;

        void Update()
        {
            if (InteractionController.Instance.Interactable == null)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            if (InteractionController.Instance.Interactable.InteractableType != InteractableType.CARRIABLE)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 0f, 15f * Time.deltaTime);
                return;
            }

            CarriableObject carriableObject = (CarriableObject)InteractionController.Instance.Interactable;

            m_Text.text = FirstLetterToUpper(carriableObject.CarriableType.ToString());

            m_CanvasGroup.alpha = Mathf.Lerp(m_CanvasGroup.alpha, 1f, 15f * Time.deltaTime);

            float boundsMedian = GetMedian(carriableObject.MeshBounds.extents.x, carriableObject.MeshBounds.extents.y, carriableObject.MeshBounds.extents.z);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(carriableObject.MeshBounds.center + Camera.main.transform.right * boundsMedian);
            PositionAtScreenPoint(m_RectTransform, screenPosition + m_CustomItemOffset);
        }

        string FirstLetterToUpper(string str)
        {
            str = str.ToLower();

            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
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