using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public interface ISaveAssetDialog
    {
        Sprite AssetIcon
        {
            get;
            set;
        }

        UnityObject Asset
        {
            get;
            set;
        }

        bool SelectSavedAssets
        {
            get;
            set;
        }


        event Action<ISaveAssetDialog, UnityObject> SaveCompleted;
    }
}

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SaveAssetViewModel : HierarchicalDataViewModel<ProjectItem>, ISaveAssetDialog
    {
        public event Action<ISaveAssetDialog, UnityObject> SaveCompleted;

        private Sprite m_assetIcon;
        [Binding]
        public Sprite AssetIcon
        {
            get { return m_assetIcon; }
            set 
            { 
                if(m_assetIcon != value)
                {
                    m_assetIcon = value;
                    RaisePropertyChanged(nameof(AssetIcon));
                }
            }
        }

        public UnityObject Asset
        {
            get;
            set;
        }
        public bool SelectSavedAssets
        {
            get;
            set;
        }

        private bool m_activateInputField;
        [Binding]
        public bool ActivateInputField
        {
            get { return m_activateInputField; }
            private set
            {
                if (m_activateInputField != value)
                {
                    m_activateInputField = value;
                    RaisePropertyChanged(nameof(ActivateInputField));
                    m_activateInputField = false;
                }
            }
        }

        private string m_assetName;
        [Binding]
        public string AssetName
        {
            get { return m_assetName; }
            set
            {
                if (m_assetName != value)
                {
                    m_assetName = value;
                    RaisePropertyChanged(nameof(AssetName));

                    if(m_parentDialog != null)
                    {
                        m_parentDialog.IsOkInteractable = IsAssetNameValid();
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
                if (m_parentDialog == null)
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

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<ISaveAssetDialog>(this);
        }

        protected override void Start()
        {
            base.Start();

            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_SaveAssetsDialog_Save", "Save"),
                CancelText = Localization.GetString("ID_RTEditor_SaveAssetsDialog_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.IsOkInteractable = IsAssetNameValid();
            m_parentDialog.Ok += OnOk;

            m_rootItems = new[] { m_project.State.RootFolder };

            BindData();
            SelectedItems = m_rootItems;
            Expand(m_rootItems[0]);

            ActivateInputField = true;
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }

            m_rootItems = null;
            m_project = null;

            IOC.UnregisterFallback<ISaveAssetDialog>(this);

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
            if (selectedItem == null)
            {
                return;
            }

            if (!selectedItem.IsFolder)
            {
                AssetName = selectedItem.Name;
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
            if (parent == null)
            {
                return m_rootItems;
            }

            Guid assetTypeGuid = m_project.Utils.ToGuid(Asset.GetType());

            return parent.Children.Where(projectItem => projectItem.IsFolder).OrderBy(projectItem => projectItem.Name)
                    .Union(parent.Children.Where(projectItem => !projectItem.IsFolder && projectItem.GetTypeGuid() == assetTypeGuid).OrderBy(projectItem => projectItem.Name));
        }

        public override bool HasChildren(ProjectItem parent)
        {
            if (parent == null)
            {
                return true;
            }

            Guid assetTypeGuid = m_project.Utils.ToGuid(Asset.GetType());
            return parent.Children != null && parent.Children.Count(projectItem => projectItem.IsFolder || projectItem.GetTypeGuid() == assetTypeGuid) > 0;
        }

        #endregion

        #region Methods
        private bool IsAssetNameValid()
        {
            return !string.IsNullOrEmpty(AssetName);
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
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_UnableToSaveAsset", "Unable to save asset"),
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_UnableToSaveAssetInPlayMode", "Unable to save asset in play mode"));
                return false;
            }

            if (string.IsNullOrEmpty(AssetName))
            {
                return false;
            }

            if (AssetName != null && AssetName.Length > 0 && (!char.IsLetter(AssetName[0]) || AssetName[0] == '-'))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_AssetNameIsInvalid", "Asset name is invalid"),
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_AssetNameShouldStartWith", "Asset name should start with letter"));
                return false;
            }

            if (!ProjectItem.IsValidName(AssetName))
            {
                WindowManager.MessageBox(
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_AssetNameIsInvalid", "Asset name is invalid"),
                    Localization.GetString("ID_RTEditor_SaveAssetsDialog_AssetNameInvalidCharacters", "Asset name contains invalid characters"));
                return false;
            }

            ProjectItem selectedItem = SelectedItems.First();
            if (!selectedItem.IsFolder)
            {
                if (AssetName.ToLower() == selectedItem.Name.ToLower())
                {
                    Overwrite(selectedItem);
                    return false;
                }
                else
                {
                    ProjectItem folder = selectedItem.Parent;
                    return SaveAssetToFolder(folder);
                }
            }
            else
            {
                ProjectItem folder = selectedItem;
                return SaveAssetToFolder(folder);
            }
        }

        protected virtual void Overwrite(ProjectItem selectedItem)
        {
            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_SaveAssetsDialog_AssetWithSameNameExists", "Asset with same name already exits"),
                Localization.GetString("ID_RTEditor_SaveAssetsDialog_DoYouWantToOverwriteIt", "Do you want to overwrite it?"),
                async (sender, yes) =>
                {
                    m_parentDialog.Close(null);

                    ProjectItem parent = selectedItem.Parent;

                    IRuntimeEditor editor = Editor;
                    IWindowManager wm = WindowManager;
                    editor.IsBusy = true;
                    try
                    {
                        await m_project.Safe.DeleteAsync(new[] { selectedItem });
                        SaveNewAssetToFolder(parent);
                    }
                    catch (Exception e)
                    {
                        wm.MessageBox(Localization.GetString("ID_RTEditor_SaveAssetsDialog_UnableToDeleteAsset", "Unable to delete asset"), e.Message);
                        Debug.LogException(e);
                        RaseSaveCompleted(null);
                    }
                    finally
                    {
                        editor.IsBusy = false;
                    }
                },
            (sender, no) => ActivateInputField = true,
            Localization.GetString("ID_RTEditor_SaveAssetsDialog_Yes", "Yes"),
            Localization.GetString("ID_RTEditor_SaveAssetsDialog_No", "No"));
        }

        protected virtual bool SaveAssetToFolder(ProjectItem folder)
        {
            if (folder.Children != null && folder.Children.Any(p => p.Name.ToLower() == AssetName.ToLower() && m_project.Utils.IsScene(p)))
            {
                Overwrite(folder.Children.Where(p => p.Name.ToLower() == AssetName.ToLower() && m_project.Utils.IsScene(p)).First());
                return false;
            }

            SaveNewAssetToFolder(folder);
            return true;
        }

        private async void SaveNewAssetToFolder(ProjectItem folder)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            editor.IsBusy = true;

            Asset.name = AssetName;

            var projectLock = await project.LockAsync();
            try
            {
                ProjectItem[] saveResult = await project.SaveAsync(new[] { folder }, new[] { new byte[0] }, new[] { Asset }, null, SelectSavedAssets);
                UnityObject[] unityObject = await project.LoadAsync(saveResult);

                if (unityObject.Length > 0)
                {
                    Destroy(Asset);
                    Asset = unityObject[0];
                }

                RaseSaveCompleted(Asset);
            }
            catch (Exception e)
            {
                wm.MessageBox(Localization.GetString("ID_RTEditor_SaveAssetsDialog_UnableToSaveAsset", "Unable to save asset"), e.Message);
                Debug.LogError(e);
                RaseSaveCompleted(null);
            }
            finally
            {
                editor.IsBusy = false;
                projectLock.Dispose();
            }
        }

        private void RaseSaveCompleted(UnityObject result)
        {
            SaveCompleted?.Invoke(this, result);
        }

        #endregion

    }
}
