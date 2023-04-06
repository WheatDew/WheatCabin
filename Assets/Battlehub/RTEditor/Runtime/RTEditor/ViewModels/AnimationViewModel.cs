using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL;
using Battlehub.RTSL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class AnimationViewModel : ViewModel
    {
        private IInspectorModel m_inspector;

        private byte[][] m_state;

        private GameObject m_targetGameObject;

        [Binding]
        public GameObject TargetGameObject
        {
            get { return m_targetGameObject; }
            set
            {
                if(m_targetGameObject != value)
                {
                    m_targetGameObject = value;
                    RaisePropertyChanged(nameof(TargetGameObject));
                }
            }
        }

        private RuntimeAnimation m_target;

        [Binding]
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set 
            {
                if(m_target != value)
                {
                    m_target = value;
                    RaisePropertyChanged(nameof(Target));
                }
            }
            
        }

        private RuntimeAnimationClip m_currentClip;

        [Binding]
        public RuntimeAnimationClip CurrentClip
        {
            get { return m_currentClip; }
            set
            {
                if(m_currentClip != value)
                {
                    m_currentClip = value;
                    RaisePropertyChanged(nameof(CurrentClip));
                }
            }
        }

        private bool m_isEditing;

        [Binding]
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if(m_isEditing != value)
                {
                    m_isEditing = value;
                    RaisePropertyChanged(nameof(IsEditing));
                }
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            m_inspector = IOC.Resolve<IInspectorModel>();
            if (m_inspector != null)
            {
                m_inspector.BeginEdit += OnInspectorBeginEdit;
                m_inspector.EndEdit += OnInspectorEndEdit;
            }

            Editor.Selection.SelectionChanged += OnSelectionChanged;
            OnSelectionChanged(null);
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            if(m_inspector != null)
            {
                m_inspector.BeginEdit -= OnInspectorBeginEdit;
                m_inspector.EndEdit -= OnInspectorEndEdit;
                m_inspector = null;
            }

            Editor.Selection.SelectionChanged -= OnSelectionChanged;
        }

        protected virtual void Update()
        {
            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (activeTool is BaseHandle)
            {
                IsEditing = true;
            }
            else
            {
                if (!m_inspector.IsEditing)
                {
                    IsEditing = false;
                }
            }
        }
  
        #region Bound Unity EventHandlers

        [Binding]
        public virtual void OnSaveCurrentClip()
        {
            if (CurrentClip != null)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if (editor != null && !editor.IsPlaying)
                {
                    editor.SaveAssets(new[] { CurrentClip });
                }
            }
        }

        [Binding]
        public void OnClipBeginModify()
        {
            if (Target != null)
            {
                m_state = SaveState();
            }
        }

        [Binding]
        public void OnClipModified()
        {
            if (Target != null)
            {
                Target.Refresh();

                byte[][] newState = SaveState();
                byte[][] oldState = m_state;

                m_state = null;

                Editor.Undo.CreateRecord(redoRecord =>
                {
                    LoadState(newState);
                    UpdateTargetAnimation();
                    RaisePropertyChanged(nameof(CurrentClip));
                    
                    return true;
                },
                undoRecord =>
                {
                    LoadState(oldState);
                    UpdateTargetAnimation();
                    RaisePropertyChanged(nameof(CurrentClip));
                    
                    return true;
                });
            }
        }

        #endregion

        #region Methods
        private void OnInspectorBeginEdit(object sender, InspectorModelEventArgs e)
        {
            if (IsEditing)
            {
                return;
            }

            if (CurrentClip == null || CurrentClip.Properties == null || CurrentClip.Properties.Count == 0)
            {
                return;
            }

            Component targetComponent = e.TargetComponents.FirstOrDefault();
            if (targetComponent == null)
            {
                return;
            }

            bool canBeginEdit = false;
            foreach (RuntimeAnimationProperty property in CurrentClip.Properties)
            {
                if (property.ComponentType == targetComponent.GetType())
                {
                    canBeginEdit = true;
                }
            }

            if (!canBeginEdit)
            {
                return;
            }

            IsEditing = true;
        }
        private void OnInspectorEndEdit(object sender, InspectorModelEventArgs e)
        {
            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (IsEditing && activeTool == null)
            {
                IsEditing = false;
            }
        }

        protected virtual void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            TargetGameObject = Editor.Selection.activeGameObject;
        }

        protected void UpdateTargetAnimation()
        {
            if (TargetGameObject != null)
            {
                RuntimeAnimation animation = TargetGameObject.GetComponent<RuntimeAnimation>();
                Target = animation;
            }
            else
            {
                Target = null;
            }
        }


        protected virtual byte[][] SaveState()
        {
            ISerializer serializer = IOC.Resolve<ISerializer>();
            Type animType = GetSurrogateType(typeof(RuntimeAnimation));
            Type clipType = GetSurrogateType(typeof(RuntimeAnimationClip));

            if (serializer == null || animType == null || clipType == null)
            {
                return new byte[0][];
            }

            IList<RuntimeAnimationClip> clips = Target.Clips;
            byte[][] state = new byte[1 + clips.Count][];

            IPersistentSurrogate animationSurrogate = (IPersistentSurrogate)Activator.CreateInstance(animType);
            IPersistentSurrogate clipSurrogate = (IPersistentSurrogate)Activator.CreateInstance(clipType);

            animationSurrogate.ReadFrom(Target);
            state[0] = serializer.Serialize(animationSurrogate);
            for (int i = 0; i < clips.Count; ++i)
            {
                RuntimeAnimationClip clip = clips[i];
                clipSurrogate.ReadFrom(clip);
                state[1 + i] = serializer.Serialize(clipSurrogate);
            }

            return state;
        }

        protected virtual void LoadState(byte[][] state)
        {
            ISerializer serializer = IOC.Resolve<ISerializer>();
            Type animType = GetSurrogateType(typeof(RuntimeAnimation));
            Type clipType = GetSurrogateType(typeof(RuntimeAnimationClip));

            if (serializer == null || animType == null || clipType == null)
            {
                return;
            }

            IPersistentSurrogate animationSurrogate = (IPersistentSurrogate)serializer.Deserialize(state[0], animType);
            animationSurrogate.WriteTo(Target);

            IList<RuntimeAnimationClip> clips = Target.Clips;
            for (int i = 0; i < clips.Count; ++i)
            {
                RuntimeAnimationClip clip = clips[i];
                if (clip == null)
                {
                    clips[i] = clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();
                }

                IPersistentSurrogate clipSurrogate = (IPersistentSurrogate)serializer.Deserialize(state[1 + i], clipType);
                clipSurrogate.WriteTo(clip);
            }
        }

        protected Type GetSurrogateType(Type type)
        {
            ITypeMap typeMap = IOC.Resolve<ITypeMap>();
            if (typeMap == null)
            {
                return null;
            }

            Type persistentType = typeMap.ToPersistentType(type);
            if (persistentType == null)
            {
                return null;
            }

            return persistentType;
        }

        #endregion
    }
}
