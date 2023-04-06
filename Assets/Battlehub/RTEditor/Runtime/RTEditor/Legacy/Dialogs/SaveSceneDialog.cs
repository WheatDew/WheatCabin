using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTSL.Interface;

using TMPro;

namespace Battlehub.RTEditor
{
    [AddComponentMenu("") , /*System.Obsolete*/]
    public class SaveSceneDialog : RuntimeWindow
    {
        [SerializeField]
        private TMP_InputField Input = null;
        [SerializeField]
        private Sprite FolderIcon = null;
        [SerializeField]
        private Sprite SceneIcon = null;
    
        private Dialog m_parentDialog;
        private VirtualizingTreeView m_treeView = null;
        private IProjectAsync m_project;
        private IWindowManager m_windowManager;
        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.SaveScene;
            base.AwakeOverride();
            m_localization = IOC.Resolve<ILocalization>();
        }
        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.OkText = m_localization.GetString("ID_RTEditor_SaveSceneDialog_Save", "Save");
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SaveSceneDialog_Cancel", "Cancel");
            m_parentDialog.IsCancelVisible = true;

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            m_windowManager = IOC.Resolve<IWindowManager>();

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemExpanding += OnItemExpanding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;
            m_treeView.CanRemove = false;

            m_project = IOC.Resolve<IProjectAsync>();
            if (m_project == null)
            {
                Debug.LogError("ProjectManager.Instance is null");
                return;
            }

            m_treeView.Items = new[] { m_project.State.RootFolder };
            m_treeView.SelectedItem = m_project.State.RootFolder;
            m_treeView.Expand(m_project.State.RootFolder);
                      
            Input.ActivateInputField();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(CoActivateInputField());
        }

        private System.Collections.IEnumerator CoActivateInputField()
        {
            yield return new WaitForEndOfFrame();
            if (Input != null)
            {
                Input.ActivateInputField();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
          
            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk; 
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemExpanding -= OnItemExpanding;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
            }
        }


        private void OnOk(Dialog dialog, DialogCancelArgs args)
        {
            if (m_treeView.SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }

            if(Editor.IsPlaying)
            {
                m_windowManager.MessageBox(
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScene", "Unable to save scene") ,
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScenePlayMode", "Unable to save scene in play mode"));
                return;
            }

            if (string.IsNullOrEmpty(Input.text))
            {
                args.Cancel = true;
                Input.ActivateInputField();
                return;
            }

            if (Input.text != null && Input.text.Length > 0 && (!char.IsLetter(Input.text[0]) || Input.text[0] == '-'))
            {
                m_windowManager.MessageBox(
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameIsInvalid", "Scene name is invalid"), 
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameShouldStartWith", "Scene name should start with letter"));
                args.Cancel = true;
                return;
            }

            if (!ProjectItem.IsValidName(Input.text))
            {
                m_windowManager.MessageBox(
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameIsInvalid", "Scene name is invalid"),
                    m_localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameInvalidCharacters", "Scene name contains invalid characters"));
                args.Cancel = true;
                return;
            }

            ProjectItem selectedItem = (ProjectItem)m_treeView.SelectedItem;
            if (m_project.Utils.IsScene(selectedItem))
            {
                if (Input.text.ToLower() == selectedItem.Name.ToLower())
                {
                    Overwrite(selectedItem);
                    args.Cancel = true;
                }
                else
                {
                    ProjectItem folder = selectedItem.Parent;
                    SaveSceneToFolder(args, folder);
                }
            }
            else
            {
                ProjectItem folder = selectedItem;
                SaveSceneToFolder(args, folder);
            }
        }

        private void Overwrite(ProjectItem selectedItem)
        {
            m_windowManager.Confirmation(
                m_localization.GetString("ID_RTEditor_SaveSceneDialog_SceneWithSameNameExists", "Scene with same name already exits"),
                m_localization.GetString("ID_RTEditor_SaveSceneDialog_DoYouWantToOverwriteIt", "Do you want to overwrite it?"),
                async (sender, yes) =>
            {
                m_parentDialog.Close(null);
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                await editor.OverwriteSceneAsync(selectedItem);
            },
            (sender, no) => Input.ActivateInputField(),
                m_localization.GetString("ID_RTEditor_SaveSceneDialog_Yes", "Yes"),
                m_localization.GetString("ID_RTEditor_SaveSceneDialog_No", "No"));
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item.Name;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                if (m_project.Utils.IsScene(item))
                {
                    image.sprite = SceneIcon;
                }
                else
                {
                    image.sprite = FolderIcon;
                }
                image.gameObject.SetActive(true);
                e.HasChildren = item.Children != null && item.Children.Count(projectItem => projectItem.IsFolder || m_project.Utils.IsScene(projectItem)) > 0;
            }
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                e.Children = item.Children.Where(projectItem => projectItem.IsFolder).OrderBy(projectItem => projectItem.Name)
                    .Union(item.Children.Where(projectItem => m_project.Utils.IsScene(projectItem)).OrderBy(projectItem => projectItem.Name));
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            ProjectItem selectedItem = (ProjectItem)e.NewItem;
            if (selectedItem == null)
            {
                return;
            }
            if (m_project.Utils.IsScene(selectedItem))
            {
                Input.text = selectedItem.Name;
            }

            Input.ActivateInputField();
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(e.Items[0]);
            if (treeViewItem != null)
            {
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
            }

            Input.ActivateInputField();
        }


        private void SaveSceneToFolder(DialogCancelArgs args, ProjectItem folder)
        {
            if (folder.Children != null && folder.Children.Any(p => p.Name.ToLower() == Input.text.ToLower() && m_project.Utils.IsScene(p)))
            {
                Overwrite(folder.Children.Where(p => p.Name.ToLower() == Input.text.ToLower() && m_project.Utils.IsScene(p)).First());
                args.Cancel = true;
            }
            else
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                editor.SaveSceneToFolder(folder, Input.text, error =>
                {
                    if (error.HasError)
                    {
                        m_windowManager.MessageBox(m_localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScene", "Unable to save scene"), error.ErrorText);
                    }
                });
            }
        }
    }
}

