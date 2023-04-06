using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Demo
{
    public class CreatePreviewExample : MonoBehaviour
    {
        [SerializeField]
        private Image m_image = null;
        [SerializeField]
        private Object m_object = null;

        private Texture2D m_previewTexture;
        private Sprite m_previewSprite;

        private void Start()
        {
            CreatePreview();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreatePreview();
            }
        }

        private void CreatePreview()
        {
            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();

            if (m_previewTexture != null)
            {
                Destroy(m_previewTexture);
            }

            if (m_previewSprite != null)
            {
                Destroy(m_previewSprite);
            }

            if (previewUtil.CanCreatePreview(m_object))
            {
                m_previewTexture = previewUtil.CreatePreview(m_object);
                m_previewSprite = Sprite.Create(m_previewTexture,
                    new Rect(0, 0, m_previewTexture.width, m_previewTexture.height),
                    new Vector2(0.5f, 0.5f));

                m_image.sprite = m_previewSprite;
            }
        }

        private void OnDestroy()
        {
            if (m_previewSprite != null)
            {
                Destroy(m_previewSprite);
            }

            if (m_previewTexture != null)
            {
                Destroy(m_previewTexture);
            }
        }
    }
}
