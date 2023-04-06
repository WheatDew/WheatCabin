using System;
using UnityEngine;
using UnityEngine.UI;

using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using System.Collections;

using UnityObject = UnityEngine.Object;
using System.Linq;

namespace Battlehub.RTEditor
{
    public class ProjectItemView : MonoBehaviour
    {
        private IProjectAsync m_project;

        [SerializeField]
        private Image m_imgPreview = null;

        private Texture2D m_texture;
        private Sprite m_sprite;

        private ProjectItem m_projectItem;
        public ProjectItem ProjectItem
        {
            get { return m_projectItem; }
            set
            {
                if (m_projectItem != null)
                {
                    m_projectItem.PreviewDataChanged -= OnPreviewDataChanged;
                }

                m_projectItem = value;

                if (m_project == null)
                {
                    return;
                }

                UpdateImage();

                if (m_projectItem != null && !m_projectItem.IsFolder)
                {
                    m_projectItem.PreviewDataChanged += OnPreviewDataChanged;
                }
            }
        }

        private void OnPreviewDataChanged(object sender, EventArgs e)
        {
            if(m_imgPreview == null)
            {
                Debug.LogWarning("ImgPreview is null");
                return;
            }
            UpdateImage();
        }

        private void UpdateImage()
        {
            if(m_project == null)
            {
                m_project = IOC.Resolve<IProjectAsync>();
                if(m_project == null)
                {
                    Debug.LogError("Project is null");
                }
            }

            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            if (m_texture != null)
            {
                Destroy(m_texture);
                m_texture = null;
            }
            if(m_sprite != null)
            {
                Destroy(m_sprite);
                m_sprite = null;
            }
            if (m_projectItem == null)
            {
                m_imgPreview.sprite = null;
            }
            else if (m_projectItem.IsFolder)
            {
                m_imgPreview.sprite = settings.SelectedTheme.GetIcon("RTEAsset_FolderLarge");
            }
            else if (m_project.Utils.IsScene(m_projectItem))
            {
                m_imgPreview.sprite = settings.SelectedTheme.GetIcon("RTEAsset_SceneLarge");
            }
            else
            {
                Type assetItemType = m_project.Utils.ToType(m_projectItem);
                if (m_projectItem.GetPreview() == null || assetItemType == null)
                {
                    m_imgPreview.sprite = settings.SelectedTheme.GetIcon("None");
                }
                else if (m_projectItem.GetPreview().Length == 0)
                {
                    m_imgPreview.sprite = settings.SelectedTheme.GetIcon($"RTEAsset_{assetItemType.FullName}");
                    if (m_imgPreview.sprite == null)
                    {
                        m_imgPreview.sprite = settings.SelectedTheme.GetIcon("RTEAsset_Object");
                    }
                }
                else
                {
                    m_texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    m_texture.LoadImage(m_projectItem.GetPreview());
                    m_imgPreview.sprite = Sprite.Create(m_texture, new Rect(0, 0, m_texture.width, m_texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }

        private void Awake()
        {
            m_project = IOC.Resolve<IProjectAsync>();
           
            UpdateImage();

            if (m_projectItem != null && !m_projectItem.IsFolder)
            {
                m_projectItem.PreviewDataChanged += OnPreviewDataChanged;
            }
        }

        private void OnDestroy()
        {
            if(m_texture != null)
            {
                Destroy(m_texture);
                m_texture = null;
            }
            if(m_sprite != null)
            {
                Destroy(m_sprite);
                m_sprite = null;
            }
            if (m_projectItem != null)
            {
                m_projectItem.PreviewDataChanged -= OnPreviewDataChanged;
            }
        }

        [Obsolete("Use overload without 'project' parameter")] //12.11.2020
        public static IEnumerator CoCreatePreviews(ProjectItem[] items, IProject project, IResourcePreviewUtility resourcePreview, Action done = null)
        {
            return CoCreatePreviews(items, resourcePreview, done);
        }

        [Obsolete("Use IResourcePreviewUtilityExtensions.CoCreatePreviews method")] //09.02.2021
        public static IEnumerator CoCreatePreviews(ProjectItem[] projectItems,  IResourcePreviewUtility resourcePreview, Action done = null)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            if (resourcePreview == null)
            {
                if(done != null)
                {
                    done();
                }
                yield break;
            }

            IRTE rte = IOC.Resolve<IRTE>();
            
            if(rte.Selection.activeObject != null)
            {
                object id = project.Utils.ToPersistentID(rte.Selection.activeObject);
                ProjectItem selectedProjectItem = projectItems.Where(projectItem => !projectItem.IsFolder && !(projectItem is ImportAssetItem) && project.Utils.ToPersistentID(projectItem).Equals(id)).FirstOrDefault();
                if(selectedProjectItem != null)
                {
                    selectedProjectItem.SetPreview(null);
                }
            }

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                ImportAssetItem importItem = projectItems[i] as ImportAssetItem;
                if (importItem != null)
                {
                    if (/*importItem.Preview == null &&*/ importItem.Object != null)
                    {
                        importItem.SetPreview(resourcePreview.CreatePreviewData(importItem.Object));
                    }
                }
                else
                {
                    if (!projectItem.IsFolder)
                    {
                        UnityObject obj = null;
                        if (projectItem.GetPreview() == null)
                        {
                            obj = project.Utils.FromProjectItem<UnityObject>(projectItem);
                        }

                        if (obj != null)
                        {
                            projectItem.SetPreview(resourcePreview.CreatePreviewData(obj));
                        }
                    }
                }

                if(i % 10 == 0)
                {
                    yield return new WaitForSeconds(0.005f);
                }
            }

            if(done != null)
            {
                done();
            }
        }
    }
}

