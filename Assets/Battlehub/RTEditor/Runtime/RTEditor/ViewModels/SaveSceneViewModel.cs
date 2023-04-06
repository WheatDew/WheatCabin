using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SaveSceneViewModel : HierarchicalDataViewModel<ProjectItem>
    {
        private bool m_activateInputField;
        [Binding]
        public bool ActivateInputField
        {
            get { return m_activateInputField; }
            private set
            {
                if(m_activateInputField != value)
                {
                    m_activateInputField = value;
                    RaisePropertyChanged(nameof(ActivateInputField));
                    m_activateInputField = false;
                }
            }
        }

        private string m_sceneName;
        [Binding]
        public string SceneName
        {
            get { return m_sceneName; }
            set
            {
                if(m_sceneName != value)
                {
                    m_sceneName = value;
                    RaisePropertyChanged(nameof(SceneName));

                    if(m_parentDialog != null)
                    {
                        m_parentDialog.IsOkInteractable = IsSceneNameValid();
                    }
                }
            }
        }

        private DialogViewModel m_parentDialog;
        [Binding]
        public DialogViewModel ParentDialog
        {
            get 
            {
                if(m_parentDialog == null)
                {
                    m_parentDialog = new DialogViewModel();
                }
                return m_parentDialog; 
            }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }


        private ProjectItem[] m_rootItems = new ProjectItem[0];
        protected ProjectItem[] RootItems
        {
            get { return m_rootItems; }
        }

        protected override void Start()
        {
            base.Start();

            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_SaveSceneDialog_Save", "Save"),
                CancelText = Localization.GetString("ID_RTEditor_SaveSceneDialog_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkInteractable = IsSceneNameValid();
            }

            m_rootItems = new[] { m_project.State.RootFolder };

            BindData();
            SelectedItems = m_rootItems;
            Expand(m_rootItems[0]);

            ActivateInputField = true;
        }

        protected override void OnDestroy()
        {
            if(m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            m_rootItems = null;
            m_project = null;

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            args.Cancel = !TryToApplyChanges();
        }
        #endregion

        #region Bound UnityEvent Handlers
        protected override void OnSelectedItemsChanged(IEnumerable<ProjectItem> unselectedObjects, IEnumerable<ProjectItem> selectedObjects)
        {
            ProjectItem selectedItem = selectedObjects != null ? selectedObjects.FirstOrDefault() : null;
            if(selectedItem == null)
            {
                return;
            }

            if(m_project.Utils.IsScene(selectedItem))
            {
                SceneName = selectedItem.Name;
            }

            ActivateInputField = true;
        }

        public override void OnItemDoubleClick()
        {
            Expand(TargetItem);

            ActivateInputField = true;
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanDrag;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanUnselectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;
            flags &= ~HierarchicalDataFlags.CanEdit;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectItem item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<ProjectItem> GetChildren(ProjectItem parent)
        {
            if(parent == null)
            {
                return m_rootItems;
            }
            
            return parent.Children.Where(projectItem => projectItem.IsFolder).OrderBy(projectItem => projectItem.Name)
                    .Union(parent.Children.Where(projectItem => m_project.Utils.IsScene(projectItem)).OrderBy(projectItem => projectItem.Name));
        }

        public override bool HasChildren(ProjectItem parent)
        {
            if(parent == null)
            {
                return true;
            }
            return parent.Children != null && parent.Children.Count(projectItem => projectItem.IsFolder || m_project.Utils.IsScene(projectItem)) > 0;
        }

        #endregion

        #region Methods
        private bool IsSceneNameValid()
        {
            return !string.IsNullOrEmpty(SceneName);
        }

        protected virtual bool TryToApplyChanges()
        {
            if (!HasSelectedItems)
            {
                return false;
            }

            if (Editor.IsPlaying)
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScene", "Unable to save scene"),
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScenePlayMode", "Unable to save scene in play mode"));
                return false;
            }

            if (string.IsNullOrEmpty(SceneName))
            {
                return false;
            }

            if (SceneName != null && SceneName.Length > 0 && (!char.IsLetter(SceneName[0]) || SceneName[0] == '-'))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameIsInvalid", "Scene name is invalid"),
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameShouldStartWith", "Scene name should start with letter"));
                return false;
            }

            if (!ProjectItem.IsValidName(SceneName))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameIsInvalid", "Scene name is invalid"),
                    Localization.GetString("ID_RTEditor_SaveSceneDialog_SceneNameInvalidCharacters", "Scene name contains invalid characters"));
                return false;
            }

            ProjectItem selectedItem = SelectedItems.First();
            if (m_project.Utils.IsScene(selectedItem))
            {
                if (SceneName.ToLower() == selectedItem.Name.ToLower())
                {
                    Overwrite(selectedItem);
                    return false;
                }
                else
                {
                    ProjectItem folder = selectedItem.Parent;
                    return SaveSceneToFolder(folder);
                }
            }
            else
            {
                ProjectItem folder = selectedItem;
                return SaveSceneToFolder(folder);
            }
        }

        protected virtual void Overwrite(ProjectItem selectedItem)
        {
            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_SaveSceneDialog_SceneWithSameNameExists", "Scene with same name already exits"),
                Localization.GetString("ID_RTEditor_SaveSceneDialog_DoYouWantToOverwriteIt", "Do you want to overwrite it?"),
                async (sender, yes) =>
                {
                    m_parentDialog.Close(null);
                    IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                    await editor.OverwriteSceneAsync(selectedItem);
                },
            (sender, no) => ActivateInputField = true,
                Localization.GetString("ID_RTEditor_SaveSceneDialog_Yes", "Yes"),
                Localization.GetString("ID_RTEditor_SaveSceneDialog_No", "No"));
        }

        protected virtual bool SaveSceneToFolder(ProjectItem folder)
        {
            if (folder.Children != null && folder.Children.Any(p => p.Name.ToLower() == SceneName.ToLower() && m_project.Utils.IsScene(p)))
            {
                Overwrite(folder.Children.Where(p => p.Name.ToLower() == SceneName.ToLower() && m_project.Utils.IsScene(p)).First());
                return false;
            }

            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.SaveSceneToFolder(folder, SceneName, error =>
            {
                if (error.HasError)
                {
                    IWindowManager windowManager = IOC.Resolve<IWindowManager>();
                    windowManager.MessageBox(Localization.GetString("ID_RTEditor_SaveSceneDialog_UnableToSaveScene", "Unable to save scene"), error.ErrorText);
                }
            });

            return true;
        }

        #endregion
    }
}
