using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.Models;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.ViewModels
{
    [Binding]
    public class MobileHeaderViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool m_isCreating;
        [Binding]
        public bool IsCreating
        {
            get { return m_isCreating; }
            set
            {
                if(m_isCreating != value)
                {
                    m_isCreating = value;
                    RaisePropertyChanged(nameof(IsCreating));
                }
            }
        }

        private bool m_canDelete;
        [Binding]
        public bool CanDelete
        {
            get { return m_canDelete; }
            set
            {
                if (m_canDelete != value)
                {
                    m_canDelete = value;
                    RaisePropertyChanged(nameof(CanDelete));
                }
            }
        }

        private bool m_canUndo;
        [Binding]
        public bool CanUndo
        {
            get { return m_canUndo; }
            set
            {
                if (m_canUndo != value)
                {
                    m_canUndo = value;
                    RaisePropertyChanged(nameof(CanUndo));
                }
            }
        }

        private bool m_canRedo;
        [Binding]
        public bool CanRedo
        {
            get { return m_canRedo; }
            set
            {
                if(m_canRedo != value)
                {
                    m_canRedo = value;
                    RaisePropertyChanged(nameof(CanRedo));
                }
            }
        }

        private bool m_isPlaying;
        [Binding]
        public bool IsPlaying
        {
            get { return m_isPlaying; }
            set
            {
                if (m_isPlaying != value)
                {
                    m_isPlaying = value;
                    RaisePropertyChanged(nameof(IsPlaying));

                    m_rte.IsPlaying = value;
                }
            }
        }

        private bool m_canSave;
        [Binding]
        public bool CanSave
        {
            get { return m_canSave; }
            set
            {
                if(m_canSave != value)
                {
                    m_canSave = value;
                    RaisePropertyChanged(nameof(CanSave));
                }
            }
        }

        private bool m_isMainMenuOpened;
        [Binding]
        public bool IsMainMenuOpened
        {
            get { return m_isMainMenuOpened; }
            set
            {
                if(m_isMainMenuOpened != value)
                {
                    m_isMainMenuOpened = value;
                    RaisePropertyChanged(nameof(IsMainMenuOpened));

                    m_mobileEditorModel.IsMainMenuOpened = value;
                }
            }
        }

        private IRuntimeEditor m_rte;
        private IMobileEditorModel m_mobileEditorModel;
        private void Start()
        {
            m_rte = IOC.Resolve<IRuntimeEditor>();
            m_rte.PlaymodeStateChanged += OnPlaymodeStateChanged;
            m_rte.Selection.SelectionChanged += OnSelectionChanged;
            m_rte.Undo.UndoCompleted += OnUndoRedoStateChanged;
            m_rte.Undo.RedoCompleted += OnUndoRedoStateChanged;
            m_rte.Undo.StateChanged += OnUndoRedoStateChanged;
           
            IsPlaying = m_rte.IsPlaying;
            CanDelete = m_rte.Selection.activeGameObject != null;
            CanUndo = m_rte.Undo.CanUndo;
            CanRedo = m_rte.Undo.CanRedo;
            CanSave = m_rte.Undo.CanUndo;
            
            m_mobileEditorModel = IOC.Resolve<IMobileEditorModel>();
            if(m_mobileEditorModel != null)
            {
                m_mobileEditorModel.IsMainMenuOpenedChanged += OnIsMainMenuOpenedChanged;
                IsMainMenuOpened = m_mobileEditorModel.IsMainMenuOpened;
            }
        }
        
        private void OnDestroy()
        {
            if(m_rte != null)
            {
                if (m_rte.Selection != null)
                {
                    m_rte.Selection.SelectionChanged -= OnSelectionChanged;
                }

                if (m_rte.Undo != null)
                {
                    m_rte.Undo.UndoCompleted -= OnUndoRedoStateChanged;
                    m_rte.Undo.RedoCompleted -= OnUndoRedoStateChanged;
                    m_rte.Undo.StateChanged -= OnUndoRedoStateChanged;
                }

                m_rte.PlaymodeStateChanged -= OnPlaymodeStateChanged;
                m_rte = null;
            }

            if(m_mobileEditorModel != null)
            {
                m_mobileEditorModel.IsMainMenuOpenedChanged -= OnIsMainMenuOpenedChanged;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            CanDelete = m_rte.Selection.activeGameObject != null;
        }

        private void OnUndoRedoStateChanged()
        {
            CanUndo = m_rte.Undo.CanUndo;
            CanRedo = m_rte.Undo.CanRedo;
            CanSave = m_rte.Undo.CanUndo;
        }

        private void OnPlaymodeStateChanged()
        {
            IsPlaying = m_rte.IsPlaying;
        }

        private void OnIsMainMenuOpenedChanged(object sender, ValueChangedArgs<bool> e)
        {
            IsMainMenuOpened = e.NewValue;
        }

        [Binding]
        public void OnDelete()
        {
            m_rte.Delete(m_rte.Selection.gameObjects);
        }

        [Binding]
        public void OnUndo()
        {
            m_rte.Undo.Undo();
        }

        [Binding]
        public void OnRedo()
        {
            m_rte.Undo.Redo();
        }

        [Binding]
        public void OnSave()
        {
            m_rte.SaveScene();   
        }

        [Binding]
        public void OnOpenMainMenu()
        {
            IsMainMenuOpened = true;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

