using TMPro;
using UnityEngine;

namespace Battlehub.UIControls.TooltipControl
{
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_text;

        [SerializeField]
        private CanvasGroup m_canvasGroup;

        [SerializeField]
        private RectTransform m_rectTransform;

        public RectTransform RectTransform
        {
            get { return m_rectTransform; }
        }

        public string Text
        {
            get { return m_text.text; }
            set { m_text.text = value; }
        }

        private float m_delay;
        private float m_targetAlpha;
        private RectTransform m_layer;

        private void Awake()
        {
            if(m_text == null)
            {
                m_text = GetComponentInChildren<TextMeshProUGUI>();
            }

            if(m_canvasGroup == null)
            {
                m_canvasGroup = GetComponentInChildren<CanvasGroup>();
            }

            if(m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }

            m_canvasGroup.alpha = 0.0f;
            m_targetAlpha = 1.0f;
        }

        private void Start()
        {
            TooltipLayer layer = GetComponentInParent<TooltipLayer>();
            if(layer != null)
            {
                m_layer = layer.RectTransform;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if(m_layer != null)
            {
                Fit();
            }
        }

        private void Update()
        {
            if(m_delay > 0)
            {
                m_delay -= Time.deltaTime;
                return;
            }
            
            float alpha = m_canvasGroup.alpha;
            alpha = Mathf.Lerp(alpha, m_targetAlpha, Time.deltaTime * 5);
            if(alpha < 0.01)
            {
                alpha = 0;
            }
            m_canvasGroup.alpha = alpha;
            if (alpha <= 0 && m_targetAlpha == 0)
            {
                Destroy(gameObject);
            }
        }

        public void Show()
        {
            if(m_canvasGroup.alpha < 0.01)
            {
                m_delay = 0.5f;
            }
            m_targetAlpha = 1;
        }

        public void Hide()
        {
            m_targetAlpha = 0; 
        }

        private void Fit()
        {
            Vector3 position = m_layer.InverseTransformPoint(RectTransform.position);
            Vector2 topLeft = -Vector2.Scale(m_layer.rect.size, m_layer.pivot);
            RectTransform rt = RectTransform;
            Vector2 size = rt.rect.size;

            if (position.x + size.x > topLeft.x + m_layer.rect.width)
            {
                position.x += topLeft.x + m_layer.rect.width - (position.x + size.x);
            }

            if (position.y - size.y < topLeft.y)
            {
                position.y -= (position.y - size.y) - topLeft.y;
            }

            transform.position = m_layer.TransformPoint(position);

            Vector3 lp = transform.localPosition;
            lp.z = 0;
            transform.localPosition = lp;
        }
    }
}

