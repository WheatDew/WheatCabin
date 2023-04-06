using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTSL.Interface;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileGameObjectViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private GameObject[] m_selectedObjects;
        [Binding]
        public GameObject[] SelectedGameObjects
        {
            get { return m_selectedObjects; }
            set
            {
                if(m_selectedObjects != value)
                {
                    m_selectedObjects = value;
                    if(m_selectedObjects != null && m_selectedObjects.Length > 0)
                    {
                        Name = GameObjectEditorUtils.GetObjectName(m_selectedObjects);
                    }
                    else
                    {
                        Name = string.Empty;
                    }

                    RaisePropertyChanged(nameof(CanCreatePrefab));
                    RaisePropertyChanged(nameof(CanGroup));
                    RaisePropertyChanged(nameof(CanGroupLocal));
                    RaisePropertyChanged(nameof(CanUngroup));
                }
                
            }
        }

        private string m_name;
        [Binding]
        public string Name
        {
            get { return m_name; }
            set 
            {
                if(m_name != value)
                {
                    m_name = value;
                    RaisePropertyChanged(nameof(Name));
                    GameObjectEditorUtils.EndEditName(Name, SelectedGameObjects);
                }
            }
        }


        private LayersInfo m_layers;
        [Binding]
        public LayersInfo Layers
        {
            get { return m_layers; }
            set
            {
                if(m_layers != value)
                {
                    m_layers = value;
                    RaisePropertyChanged(nameof(Layers));
                }
            }
        }

        [Binding]
        public bool CanCreatePrefab
        {
            get { return m_selectedObjects != null && m_selectedObjects.Length == 1; }
        }

        [Binding]
        public bool CanGroup
        {
            get { return m_groupingModel.CanGroup(SelectedGameObjects); }
        }

        [Binding]
        public bool CanGroupLocal
        {
            get { return m_groupingModel.CanGroup(SelectedGameObjects); }
        }

        [Binding]
        public bool CanUngroup
        {
            get { return m_groupingModel.CanUngroup(SelectedGameObjects); }
        }

        private IGroupingModel m_groupingModel;
        private ILocalization m_localization;
        private IWindowManager m_wm;
        private IRuntimeEditor m_editor;

        private void Awake()
        {
            m_groupingModel = IOC.Resolve<IGroupingModel>();
            m_localization = IOC.Resolve<ILocalization>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_editor = IOC.Resolve<IRuntimeEditor>();
        }

        private void OnDestroy()
        {
            m_groupingModel = null;
            m_localization = null;
            m_wm = null;
            m_editor = null;
        }


        [Binding]
        public async void OnCreatePrefab()
        {
            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
            IProjectAsync project = IOC.Resolve<IProjectAsync>();

            ExposeToEditor selectedObject = SelectedGameObjects[0].GetComponent<ExposeToEditor>();

            await m_editor.CreatePrefabAsync(
                projectTree != null && projectTree.SelectedItem != null ? projectTree.SelectedItem : project.State.RootFolder, 
                selectedObject, null);
        }

        [Binding]
        public void OnCreateGroup()
        {
            CreateGroup();
        }

        [Binding]
        public void OnCreateGroupLocal()
        {
            CreateGroup("Local");
        }

        [Binding]
        public void OnUngroup()
        {
            m_groupingModel.UngroupAndRecord(SelectedGameObjects);
        }

        private void CreateGroup(string arg = null)
        {
            IGroupingModel groupingModel = m_groupingModel;
            GameObject[] selectedGameObjects = SelectedGameObjects;
            m_wm.Prompt(
                m_localization.GetString("ID_RTEditor_Peek_EnterGroupName", "Enter Group Name"),
                m_localization.GetString("ID_RTEditor_Peek_DefaultGroupName", "Group"),
                (sender, args) =>
                {
                    string groupName = args.Text;
                    groupingModel.GroupAndRecord(selectedGameObjects, groupName, arg == "Local");
                },
                (sender, args) => { });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

}
