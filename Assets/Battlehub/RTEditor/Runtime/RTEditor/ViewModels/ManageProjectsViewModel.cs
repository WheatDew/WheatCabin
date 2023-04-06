using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class ManageProjectsViewModel : HierarchicalDataViewModel<ProjectInfo>
    {
        #region ProjectInfoViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ProjectInfo without modifying the ProjectInfo itself.
        /// </summary>
        [Binding]
        internal class ProjectInfoViewModel
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            private ProjectInfoViewModel() { Debug.Assert(false); }
        }
        #endregion

        [SerializeField]
        private InputViewModel m_inputDialog = null;

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

        private ProjectInfo[] m_projectInfo = new ProjectInfo[0];
        protected ProjectInfo[] ProjectInfo
        {
            get { return m_projectInfo; }
        }

        protected override async void Start()
        {
            base.Start();

            m_project = IOC.Resolve<IProjectAsync>();

            ParentDialog.DialogSettings = new DialogViewModel.Settings
            {
                OkText = Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Open", "Open"),
                CancelText = Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"),
                IsOkVisible = true,
                IsCancelVisible = true,
            };

            m_parentDialog.Ok += OnOk;

            await LoadProjectsAsync();
        }

        protected override void OnDestroy()
        {
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
                m_parentDialog = null;
            }
            m_project = null;

            base.OnDestroy();
        }

        #region Dialog Event Handlers
        private async void OnOk(object sender, DialogViewModel.CancelEventArgs args)
        {
            if (!HasSelectedItems)
            {
                args.Cancel = true;
            }
            await OpenProjectAsync();
        }
        #endregion

        #region Bound UnityEvent Handlers

        public override void OnItemDoubleClick()
        {
            ParentDialog.Close(true);
        }

        [Binding]
        public virtual void OnCreateProject()
        {
            InputViewModel input = Instantiate(m_inputDialog);
            input.gameObject.SetActive(true);

            WindowManager.Dialog(Localization.GetString("ID_RTEditor_ProjectsDialog_CreateProject", "Create Project"), input.transform,
                async (sender, args) =>
                {
                    string projectName = input.Text;
                    if (string.IsNullOrEmpty(projectName))
                    {
                        args.Cancel = true;
                        return;
                    }

                    if (m_projectInfo != null && m_projectInfo.Any(p => p.Name.ToLower() == projectName.ToLower()))
                    {
                        WindowManager.MessageBox(
                            Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToCreateProject", "Unable to create project"),
                            Localization.GetString("ID_RTEditor_ProjectsDialog_ProjectWithSameNameExists", "Project with the same name already exists"));
                        args.Cancel = true;
                        return;
                    }

                    try
                    {
                        Editor.IsBusy = true;

                        ProjectInfo newProjectInfo = await m_project.Safe.CreateProjectAsync(projectName);
                        m_projectInfo = m_projectInfo.Union(new[] { newProjectInfo }).OrderBy(p => p.Name).ToArray();

                        SelectedItem = null;

                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, newProjectInfo));
                        int index = Array.IndexOf(m_projectInfo, newProjectInfo);
                        if (index == 0)
                        {
                            ProjectInfo sibling = m_projectInfo.FirstOrDefault();
                            if (sibling != null)
                            {
                                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(newProjectInfo, sibling));
                            }
                        }
                        else
                        {
                            ProjectInfo sibling = m_projectInfo.ElementAt(index - 1);
                            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.NextSiblingsChanged(newProjectInfo, sibling));
                        }

                        ScrollIntoView = true;
                        SelectedItem = newProjectInfo;
                        ScrollIntoView = false;
                    }
                    catch (Exception e)
                    {
                        WindowManager.MessageBox(Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToCreateProject", "Unable to create project"), e.Message);
                        args.Cancel = true;
                        Debug.LogException(e);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                },
                Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Create", "Create"),
                (sender, args) => { },
                Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"));
        }

        [Binding]
        public virtual void OnDestroyProject()
        {
            ProjectInfo selectedProject = SelectedItem;
            if (selectedProject == null)
            {
                return;
            }

            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_ProjectsDialog_DeleteProject", "Delete Project"),
                string.Format(Localization.GetString("ID_RTEditor_ProjectsDialog_AreYouSureDeleteProject", "Delete {0} project?"), selectedProject.Name),
                async (sender, args) =>
                {
                    ProjectInfo[] projectInfo = m_projectInfo.ToArray();
                    int index = Array.IndexOf(projectInfo, selectedProject);

                    try
                    {
                        Editor.IsBusy = true;
                        await m_project.Safe.DeleteProjectAsync(selectedProject.Name);

                        m_projectInfo = m_projectInfo.Where(p => p != selectedProject).ToArray();
                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(null, selectedProject));
                        
                        if (index == (projectInfo.Length - 1))
                        {
                            SelectedItem = m_projectInfo[index - 1];
                        }
                        else
                        {
                            SelectedItem = m_projectInfo[index];
                        }
                    }
                    catch (Exception e)
                    {
                        WindowManager.MessageBox("Unable to delete project", e.Message);
                        args.Cancel = true;
                        Debug.LogException(e);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                },
            (sender, args) => { },
            Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Delete", "Delete"),
            Localization.GetString("ID_RTEditor_ProjectsDialog_Btn_Cancel", "Cancel"));
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

        public override HierarchicalDataItemFlags GetItemFlags(ProjectInfo item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        public override IEnumerable<ProjectInfo> GetChildren(ProjectInfo parent)
        {
            return m_projectInfo;
        }

        #endregion

        #region Methods
        protected virtual async Task LoadProjectsAsync()
        {
            try
            {
                ParentDialog.IsInteractable = false;
                Editor.IsBusy = true;

                ProjectInfo[] projectInfo = await m_project.Safe.GetProjectsAsync();
                m_projectInfo = projectInfo.OrderBy(p => p.Name).ToArray();

                BindData();
                
                if(m_project.State.ProjectInfo != null)
                {
                    SelectedItem = m_projectInfo.Where(p => p.Name == m_project.State.ProjectInfo.Name).FirstOrDefault();
                }

                if(SelectedItem == null)
                {
                    SelectedItem = m_projectInfo.FirstOrDefault();
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                ParentDialog.IsInteractable = true;
                Editor.IsBusy = false;
            }
        }

        protected virtual async Task OpenProjectAsync()
        {
            IRTE editor = Editor;
            try
            {
                editor.IsPlaying = false;
                editor.IsBusy = true;
                await m_project.Safe.OpenProjectAsync(SelectedItem.Name);
            }
            catch (Exception e)
            {
                WindowManager.MessageBox(Localization.GetString("ID_RTEditor_ProjectsDialog_UnableToOpenProject", "Unable to open project"), e.Message);
                Debug.LogException(e);
            }
            finally
            {
                editor.IsBusy = false;
            }
        }
        #endregion
    }
}
