using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Demo
{
    public class ShowPreviewsExample : MonoBehaviour
    {
        [SerializeField]
        private Transform m_panel = null;

        [SerializeField]
        private Image m_previewUIPrefab = null;

        [SerializeField]
        private Sprite m_defaultPreview = null;

        private void Awake()
        {
            if (m_previewUIPrefab == null)
            {
                m_previewUIPrefab = Resources.Load<Image>("ExampleScripts/PreviewUI");
            }

            if (m_panel == null)
            {
                m_panel = transform;
            }

            if (m_defaultPreview == null)
            {
                m_defaultPreview = Resources.Load<Sprite>("RTE_Object");
            }
        }

        private IEnumerator Start()
        {
            IProject project = IOC.Resolve<IProject>();
            yield return new WaitUntil(() => project.IsOpened);

            ProjectAsyncOperation<Preview[]> ao = project.GetPreviews(project.Root.Flatten(true, false).Cast<AssetItem>().ToArray());
            yield return ao;

            Preview[] previews = ao.Result;
            for(int i = 0; i < previews.Length; ++i)
            {
                Preview preview = ao.Result[i];
                Image previewUI = Instantiate(m_previewUIPrefab, m_panel);

                if (IsValidPreview(preview))
                {
                    Texture2D previewTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    previewTex.LoadImage(preview.PreviewData);
                    previewUI.sprite = Sprite.Create(previewTex,
                        new Rect(0, 0, previewTex.width, previewTex.height),
                        new Vector2(0.5f, 0.5f));
                }
                else
                {
                    previewUI.sprite = m_defaultPreview;
                }
            }
        }

        private static bool IsValidPreview(Preview preview)
        {
            return preview != null && preview.PreviewData.Length > 0;
        }
    }

}

