using Battlehub.RTCommon;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Battlehub.UIControls;
using UnityEngine.EventSystems;
using System.Linq;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.RTSL;
using System;

using UnityObject = UnityEngine.Object;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.Views;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class AnimationView : RuntimeWindow
    {
        [SerializeField]
        private Toggle m_previewToggle = null;

        [SerializeField]
        private Toggle m_recordToggle = null;

        [SerializeField]
        private Button m_firstFrameButton = null;

        [SerializeField]
        private Button m_prevFrameButton = null;

        [SerializeField]
        private Toggle m_playToggle = null;

        [SerializeField]
        private Button m_nextFrameButton = null;

        [SerializeField]
        private Button m_lastFrameButton = null;

        [SerializeField]
        private TMP_InputField m_frameInput = null;

        [SerializeField]
        private TMP_Dropdown m_animationsDropDown = null;

        [SerializeField]
        private TMP_InputField m_samplesInput = null;

        [SerializeField]
        private Button m_addKeyframeButton = null;

        [SerializeField]
        private Button m_addEventButton = null;

        [SerializeField]
        private Toggle m_dopesheetToggle = null;

#pragma warning disable CS0414
        [SerializeField]
        private CanvasGroup m_group = null;
#pragma warning restore CS0414

        private AnimationPropertiesView m_propertiesView;
        private AnimationTimelineView m_timelineView;
        private AnimationCreateView m_animationCreateView;

        private bool m_wasInPreviewMode;
        private float m_normalizedTime;
        private bool m_isEditing;
        private int m_currentSample;
        private byte[][] m_state;

        private List<RuntimeAnimationClip> m_clips = new List<RuntimeAnimationClip>();

        private RuntimeAnimationClip CurrentClip
        {
            get { return m_propertiesView.Clip; }
            set
            {
                if (m_propertiesView.Clip != value)
                {
                    SetCurrentClip(value);
                }
            }
        }

        private void SetCurrentClip(RuntimeAnimationClip value)
        {
            //m_timelineView.SetSample(0);
            m_propertiesView.Clip = value;
            m_timelineView.Clip = value;
            if (m_target != null)
            {
                m_target.ClipIndex = m_target.Clips.IndexOf(value);
                m_target.Refresh();
            }

            InitLocalEulerUpdater();
        }

        private GameObject m_targetGameObject;
        public GameObject TargetGameObject
        {
            get { return m_targetGameObject; }
            set
            {
                m_targetGameObject = value;
                UpdateTargetAnimation();
                UpdateCreateViewState();
            }
        }

        private RuntimeAnimation m_target;
        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                if (m_target != value && m_target != null)
                {
                    if (!Editor.IsPlaying)
                    {
                        m_target.IsPlaying = false;
                    }

                    //m_target.IsInPreviewMode = false;
                    //m_timelineView.SetSample(0);
                }

                if (m_target != null)
                {
                    m_target.ClipsChanged -= OnAnimationClipsChanged;
                    m_target.ClipIndexChanged -= OnAnimationClipIndexChanged;
                    m_localEulerUpdater = null;
                }
                m_target = value;
                if (m_target != null)
                {
                    m_target.ClipsChanged += OnAnimationClipsChanged;
                    m_target.ClipIndexChanged += OnAnimationClipIndexChanged;
                }

                m_propertiesView.Target = m_target;
                m_timelineView.Target = m_target;
                OnAnimationClipsChanged();
            }
        }

        private LocalEulerAnglesUpdater m_localEulerUpdater;
        private void InitLocalEulerUpdater()
        {
            m_localEulerUpdater = null;
            if (Target != null && CurrentClip != null)
            {
                m_localEulerUpdater = new LocalEulerAnglesUpdater(Target, CurrentClip);
            }
        }

        private ILocalization m_localization;
        private IInspectorModel m_inspector;
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Animation;
            base.AwakeOverride();

            m_localization = IOC.Resolve<ILocalization>();
            m_inspector = IOC.Resolve<IInspectorModel>();
            m_inspector.BeginEdit += OnInspectorBeginEdit;
            m_inspector.EndEdit += OnInspectorEndEdit;

            m_propertiesView = GetComponentInChildren<AnimationPropertiesView>(true);
            m_propertiesView.BeforePropertiesAdded += OnBeforePropertiesAdded;
            m_propertiesView.PropertiesAdded += OnPropertiesAdded;
            m_propertiesView.BeforePropertiesRemoved += OnBeforePropertiesRemoved;
            m_propertiesView.PropertiesRemoved += OnPropertiesRemoved;
            m_propertiesView.PropertyExpanded += OnPropertyExpanded;
            m_propertiesView.PropertyCollapsed += OnPropertyCollapsed;
            m_propertiesView.PropertyBeginEdit += OnPropertyBeginEdit;
            m_propertiesView.PropertyValueChanged += OnPropertyValueChanged;
            m_propertiesView.PropertyEndEdit += OnPropertyEndEdit;

            m_timelineView = GetComponentInChildren<AnimationTimelineView>(true);
            m_timelineView.IsDopesheet = m_dopesheetToggle.isOn;
            m_timelineView.ClipBeginModify += OnClipBeginModify;
            m_timelineView.ClipModified += OnClipModified;

            m_animationCreateView = GetComponentInChildren<AnimationCreateView>(true);
            m_animationCreateView.Click += OnCreateClick;

            Editor.Object.ComponentAdded += OnComponentAdded;

            UnityEventHelper.AddListener(m_previewToggle, toggle => toggle.onValueChanged, OnPreviewToggleValueChanged);
            UnityEventHelper.AddListener(m_recordToggle, toggle => toggle.onValueChanged, OnRecordToggleValueChanged);
            UnityEventHelper.AddListener(m_firstFrameButton, button => button.onClick, OnFirstFrameButtonClick);
            UnityEventHelper.AddListener(m_prevFrameButton, button => button.onClick, OnPrevFrameButtonClick);
            UnityEventHelper.AddListener(m_playToggle, toggle => toggle.onValueChanged, OnPlayToggleValueChanged);
            UnityEventHelper.AddListener(m_nextFrameButton, button => button.onClick, OnNextFrameButtonClick);
            UnityEventHelper.AddListener(m_lastFrameButton, button => button.onClick, OnLastFrameButtonClick);
            UnityEventHelper.AddListener(m_frameInput, input => input.onEndEdit, OnFrameInputEndEdit);
            UnityEventHelper.AddListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);
            UnityEventHelper.AddListener(m_samplesInput, input => input.onEndEdit, OnSamplesInputEndEdit);
            UnityEventHelper.AddListener(m_addKeyframeButton, button => button.onClick, OnAddKeyframeButtonClick);
            UnityEventHelper.AddListener(m_addEventButton, button => button.onClick, OnAddEventButtonClick);
            UnityEventHelper.AddListener(m_dopesheetToggle, toggle => toggle.onValueChanged, OnDopesheetToggleValueChanged);
        }



        protected virtual void Start()
        {
            if (!GetComponent<AnimationViewImpl>())
            {
                gameObject.AddComponent<AnimationViewImpl>();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if (Editor != null)
            {
                SaveCurrentClip();

                if (Editor.Object != null)
                {
                    Editor.Object.ComponentAdded -= OnComponentAdded;
                }
            }

            if (m_propertiesView != null)
            {
                m_propertiesView.BeforePropertiesAdded -= OnBeforePropertiesAdded;
                m_propertiesView.PropertiesAdded -= OnPropertiesAdded;
                m_propertiesView.BeforePropertiesRemoved -= OnBeforePropertiesRemoved;
                m_propertiesView.PropertiesRemoved -= OnPropertiesRemoved;
                m_propertiesView.PropertyExpanded -= OnPropertyExpanded;
                m_propertiesView.PropertyCollapsed -= OnPropertyCollapsed;
                m_propertiesView.PropertyBeginEdit -= OnPropertyBeginEdit;
                m_propertiesView.PropertyValueChanged -= OnPropertyValueChanged;
                m_propertiesView.PropertyEndEdit -= OnPropertyEndEdit;
            }

            if (m_timelineView != null)
            {
                m_timelineView.ClipBeginModify -= OnClipBeginModify;
                m_timelineView.ClipModified -= OnClipModified;
            }

            if (m_animationCreateView != null)
            {
                m_animationCreateView.Click -= OnCreateClick;
            }

            if (m_target != null)
            {
                m_target.ClipsChanged -= OnAnimationClipsChanged;
                m_target.ClipIndexChanged -= OnAnimationClipIndexChanged;
            }

            UnityEventHelper.RemoveListener(m_previewToggle, toggle => toggle.onValueChanged, OnPreviewToggleValueChanged);
            UnityEventHelper.RemoveListener(m_recordToggle, toggle => toggle.onValueChanged, OnRecordToggleValueChanged);
            UnityEventHelper.RemoveListener(m_firstFrameButton, button => button.onClick, OnFirstFrameButtonClick);
            UnityEventHelper.RemoveListener(m_prevFrameButton, button => button.onClick, OnPrevFrameButtonClick);
            UnityEventHelper.RemoveListener(m_playToggle, toggle => toggle.onValueChanged, OnPlayToggleValueChanged);
            UnityEventHelper.RemoveListener(m_nextFrameButton, button => button.onClick, OnNextFrameButtonClick);
            UnityEventHelper.RemoveListener(m_lastFrameButton, button => button.onClick, OnLastFrameButtonClick);
            UnityEventHelper.RemoveListener(m_frameInput, input => input.onEndEdit, OnFrameInputEndEdit);
            UnityEventHelper.RemoveListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);
            UnityEventHelper.RemoveListener(m_samplesInput, input => input.onEndEdit, OnSamplesInputEndEdit);
            UnityEventHelper.RemoveListener(m_addKeyframeButton, button => button.onClick, OnAddKeyframeButtonClick);
            UnityEventHelper.RemoveListener(m_addEventButton, button => button.onClick, OnAddEventButtonClick);
            UnityEventHelper.RemoveListener(m_dopesheetToggle, toggle => toggle.onValueChanged, OnDopesheetToggleValueChanged);

            if (m_inspector != null)
            {
                m_inspector.BeginEdit -= OnInspectorBeginEdit;
                m_inspector.EndEdit -= OnInspectorEndEdit;
                m_inspector = null;
            }

            m_localization = null;
        }

        protected virtual void Update()
        {
            UpdateOverride();
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (activeTool is BaseHandle)
            {
                if (!m_isEditing)
                {
                    BeginEdit();
                    m_isEditing = true;
                }
            }
            else
            {
                if (m_isEditing && !m_inspector.IsEditing)
                {
                    EndEdit();
                    m_isEditing = false;
                }
            }


            bool isInPreviewModel = m_target != null && m_target.IsInPreviewMode;
            if (m_previewToggle != null && m_previewToggle.isOn != isInPreviewModel)
            {
                m_previewToggle.isOn = isInPreviewModel;
            }

            bool isPlaying = m_target != null && m_target.IsPlaying;
            if (m_playToggle.isOn != isPlaying)
            {
                m_playToggle.isOn = isPlaying;
            }

            if (m_currentSample != m_timelineView.CurrentSample)
            {
                m_currentSample = m_timelineView.CurrentSample;
                m_frameInput.text = m_currentSample.ToString();
            }

            object t = Target;
            if (t != null && Target == null)
            {
                Target = null;
                UpdateCreateViewState();
            }


            if (m_localEulerUpdater != null && !m_inspector.IsEditing && !m_isEditing)
            {
                m_localEulerUpdater.Update(m_timelineView.CurrentSample);
            }
        }

        private void OnInspectorBeginEdit(object sender, InspectorModelEventArgs e)
        {
            if (m_isEditing)
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

            BeginEdit();
            m_isEditing = true;
        }

        private void OnInspectorEndEdit(object sender, InspectorModelEventArgs e)
        {
            UnityObject activeTool = Editor.Tools.ActiveTool;
            if (m_isEditing && activeTool == null)
            {
                EndEdit();
                m_isEditing = false;
            }
        }

        private void BeginEdit()
        {
            if (m_target)
            {
                if (m_recordToggle.isOn)
                {
                    m_timelineView.SyncTargetWithDopesheet();
                }    
                    
                m_wasInPreviewMode = m_target.IsInPreviewMode;
                m_normalizedTime = m_target.NormalizedTime;
                m_target.IsInPreviewMode = false;
            }
        }

        private void EndEdit()
        {
            if (m_recordToggle.isOn)
            {
                RecordAllProperties();
            }

            if (m_wasInPreviewMode)
            {
                if (m_target != null)
                {
                    m_target.IsInPreviewMode = true;
                    m_target.NormalizedTime = m_normalizedTime;
                }
            }
        }

        private void OnAnimationClipsChanged()
        {
            SaveCurrentClip();

            if (m_target != null)
            {
                m_clips = m_target.Clips.Where(rtClip => rtClip != null).ToList();
            }
            else
            {
                m_clips.Clear();
            }

            if (m_target == null || m_clips.Count == 0)
            {
                if (m_previewToggle != null)
                {
                    m_previewToggle.isOn = false;
                }

                if (m_playToggle != null)
                {
                    m_playToggle.isOn = false;
                }

                if (m_frameInput != null)
                {
                    m_frameInput.text = "0";
                    m_currentSample = 0;
                }

                if (m_samplesInput != null)
                {
                    m_samplesInput.text = "60";
                }

                if (m_dopesheetToggle != null)
                {
                    m_dopesheetToggle.isOn = true;
                }

                if (m_animationsDropDown != null)
                {
                    m_animationsDropDown.ClearOptions();
                }

                CurrentClip = null;
            }
            else
            {
                if(m_previewToggle != null)
                {
                    m_previewToggle.SetIsOnWithoutNotify(m_target.IsInPreviewMode);
                }
                
                if (m_animationsDropDown != null)
                {
                    UnityEventHelper.RemoveListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);

                    m_animationsDropDown.ClearOptions();
                    List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

                    for (int i = 0; i < m_clips.Count; ++i)
                    {
                        options.Add(new TMP_Dropdown.OptionData(m_clips[i].name));
                    }

                    m_animationsDropDown.options = options;

                    if (!m_clips.Contains(CurrentClip))
                    {
                        CurrentClip = m_clips.First();
                        m_animationsDropDown.value = 0;
                    }
                    else
                    {
                        m_animationsDropDown.value = m_clips.IndexOf(CurrentClip);
                    }
                    UnityEventHelper.AddListener(m_animationsDropDown, dropdown => dropdown.onValueChanged, OnAnimationsDropdownValueChanged);
                }
                else
                {
                    if (!m_clips.Contains(CurrentClip))
                    {
                        CurrentClip = m_clips.First();
                    }
                }

                
            }

            if (m_group != null)
            {
                m_group.blocksRaycasts = CurrentClip != null;
                m_group.alpha = CurrentClip != null ? 1.0f : 0.5f;
                
                if (CurrentClip == null && EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            UpdateCreateViewState();
            InitLocalEulerUpdater();
        }

        private void OnAnimationClipIndexChanged()
        {
            InitLocalEulerUpdater();
        }

        private void OnComponentAdded(ExposeToEditor obj, Component addedComponent)
        {
            if (TargetGameObject == obj.gameObject)
            {
                if (addedComponent is RuntimeAnimation)
                {
                    UpdateTargetAnimation();
                    UpdateCreateViewState();
                }

                m_propertiesView.OnComponentAdded(addedComponent);
            }
        }

        private void OnCreateClick()
        {
            IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SaveAsset.ToString(), m_localization.GetString("ID_RTEditor_AnimationView_SaveAnimationClip", "Save Animation Clip"),
                (sender, args) => { });

            ISaveAssetDialog saveAssetDialog = IOC.Resolve<ISaveAssetDialog>();
            RuntimeAnimationClip clip = ScriptableObject.CreateInstance<RuntimeAnimationClip>();

            clip.name = m_localization.GetString("ID_RTEditor_AnimationView_NewAnimationClip", "New Animation Clip");

            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            saveAssetDialog.Asset = clip;
            saveAssetDialog.AssetIcon = settings.SelectedTheme.GetIcon($"RTEAsset_{typeof(RuntimeAnimationClip).FullName}");
            saveAssetDialog.SaveCompleted += OnSaveCompleted;
        }

        private void OnSaveCompleted(ISaveAssetDialog sender, UnityObject asset)
        {
            sender.SaveCompleted -= OnSaveCompleted;

            if (asset == null)
            {
                return;
            }

            RuntimeAnimationClip clip = (RuntimeAnimationClip)asset;
            GameObject go = TargetGameObject;

            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null)
            {
                Editor.Undo.BeginRecord();
                if (!exposeToEditor.GetComponent<RuntimeAnimation>())
                {
                    Editor.Undo.AddComponent(exposeToEditor, typeof(RuntimeAnimation));
                }
                Editor.Undo.CreateRecord(redoRecord =>
                {
                    SetAnimationClip(clip, go);
                    return true;
                },
                undoRecord =>
                {
                    RuntimeAnimation animation = exposeToEditor.GetComponent<RuntimeAnimation>();
                    if (animation != null)
                    {
                        animation.Clips = null;
                    }
                    UpdateTargetAnimation();
                    m_propertiesView.Target = null;
                    m_timelineView.Target = null;
                    UpdateCreateViewState();
                    return true;
                });
                Editor.Undo.EndRecord();
            }
            else
            {
                if (!go.GetComponent<RuntimeAnimation>())
                {
                    go.AddComponent<RuntimeAnimation>();
                }
            }

            SetAnimationClip(clip, go);
        }

        private void SetAnimationClip(RuntimeAnimationClip clip, GameObject go)
        {
            RuntimeAnimation animation = go.GetComponent<RuntimeAnimation>();
            animation.Clips = new List<RuntimeAnimationClip> { clip };
            animation.ClipIndex = 0;

            UpdateTargetAnimation();
            m_propertiesView.Target = animation;
            m_timelineView.Target = animation;

            CurrentClip = clip;

            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null && animation != null)
            {
                exposeToEditor.ReloadComponentEditor(animation, true);
            }

            animation.Refresh();
            UpdateCreateViewState();
        }

        private void OnBeforePropertiesAdded(object sender, EventArgs e)
        {
            OnClipBeginModify();
        }

        private void OnPropertiesAdded(AnimationPropertiesView.ItemsArg args)
        {
            m_timelineView.AddRows(args.Rows, args.Items);
        }

        private void OnBeforePropertiesRemoved(object sender, EventArgs e)
        {
            OnClipBeginModify();
        }

        private void OnPropertiesRemoved(AnimationPropertiesView.ItemsArg args)
        {
            m_timelineView.RemoveRows(args.Rows, args.Items);
        }

        private void OnPropertyExpanded(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.ExpandRow(args.Row, args.Item);
        }

        private void OnPropertyCollapsed(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.CollapseRow(args.Row, args.Item);
        }

        private void OnPropertyBeginEdit(AnimationPropertiesView.ItemArg args)
        {
            BeginEdit();
            m_timelineView.BeginSetKeyframeValues(true);
        }

        private void OnPropertyValueChanged(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.SetKeyframeValue(args.Row, args.Item);
        }

        private void OnPropertyEndEdit(AnimationPropertiesView.ItemArg args)
        {
            m_timelineView.EndSetKeyframeValues(true);
            EndEdit();
        }

        private void OnPreviewToggleValueChanged(bool value)
        {
            if (m_target != null)
            {
                m_target.IsInPreviewMode = value;

                if (m_target.IsInPreviewMode)
                {
                    m_timelineView.SyncTargetWithDopesheet();
                }
            }
        }

        private void OnRecordToggleValueChanged(bool value)
        {
            if (value)
            {
                Debug.Log("Recording...");
            }
        }

        private void OnFirstFrameButtonClick()
        {
            m_timelineView.FirstSample();
        }

        private void OnPrevFrameButtonClick()
        {
            m_timelineView.PrevSample();
        }

        private void OnPlayToggleValueChanged(bool value)
        {
            if (Target != null)
            {
                Target.IsPlaying = value;
            }
        }

        private void OnNextFrameButtonClick()
        {
            m_timelineView.NextSample();
        }

        private void OnLastFrameButtonClick()
        {
            m_timelineView.LastSample();
        }

        private void OnFrameInputEndEdit(string value)
        {
            m_timelineView.SetSample(int.Parse(value));
        }

        private void OnAnimationsDropdownValueChanged(int value)
        {
            if (m_target != null)
            {
                m_timelineView.SetSample(0);
                Target.Sample();
                Target.ClipIndex = Target.Clips.IndexOf(m_clips[value]);
                SaveCurrentClip();
                CurrentClip = m_clips[value];
            }
        }

        private void OnSamplesInputEndEdit(string value)
        {

        }

        private void OnAddKeyframeButtonClick()
        {
            RuntimeAnimationProperty[] properties = m_propertiesView.SelectedProps;
            if (properties.Length == 0)
            {
                properties = m_propertiesView.Props.Where(p => p.ComponentType != null && (p.Children == null || p.Children.Count == 0)).ToArray();
            }

            RecordProperties(properties);
        }


        private void OnAddEventButtonClick()
        {

        }

        private void OnDopesheetToggleValueChanged(bool value)
        {
            m_timelineView.IsDopesheet = value;
        }

        private void UpdateTargetAnimation()
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

        private void UpdateCreateViewState()
        {
            if (TargetGameObject == null || TargetGameObject.isStatic)
            {
                m_animationCreateView.gameObject.SetActive(false);
            }
            else
            {
                if (Target == null)
                {
                    m_animationCreateView.Text = string.Format(m_localization.GetString("ID_RTEditor_AnimationView_Condition2", "To begin animating {0}, create a RuntimeAnimation and a RuntimeAnimation Clip"), TargetGameObject.name);
                }

                if (m_clips.Count == 0)
                {
                    m_animationCreateView.Text = string.Format(m_localization.GetString("ID_RTEditor_AnimationView_Condition1", "To begin animating {0}, create a RuntimeAnimation Clip"), TargetGameObject.name);
                }

                if (Target != null && m_clips.Count > 0)
                {
                    m_animationCreateView.gameObject.SetActive(false);
                }
                else
                {
                    m_animationCreateView.gameObject.SetActive(true);
                }
            }
        }

        private void RecordAllProperties()
        {
            RecordProperties(m_propertiesView.Props.Where(p => p.ComponentType != null && (p.Children == null || p.Children.Count == 0)).ToArray());
        }

        private void RecordProperties(RuntimeAnimationProperty[] properties)
        {
            m_timelineView.BeginSetKeyframeValues(true);
            for (int i = 0; i < properties.Length; ++i)
            {
                RuntimeAnimationProperty property = properties[i];
                if (property.ComponentIsNull)
                {
                    continue;
                }

                if (property.Children == null || property.Children.Count == 0)
                {
                    int index = m_propertiesView.IndexOf(property);
                    m_timelineView.SetKeyframeValue(index, property);
                }
                else
                {
                    foreach (RuntimeAnimationProperty childProperty in property.Children)
                    {
                        int index = m_propertiesView.IndexOf(childProperty);
                        m_timelineView.SetKeyframeValue(index, childProperty);
                    }
                }
            }

            m_timelineView.EndSetKeyframeValues(true);
        }

        private void SaveCurrentClip()
        {
            if (CurrentClip != null)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                if (editor != null)
                {
                    editor.SaveAssets(new[] { CurrentClip });
                }
            }
        }


        private void OnClipBeginModify()
        {
            if (Target != null)
            {
                //Debug.Log("OnClipBeginModify");
                m_state = SaveState();
            }
        }

        private void OnClipModified()
        {
            if (Target != null)
            {
                //Debug.Log("OnClipModified");
                Target.Refresh();

                byte[][] newState = SaveState();
                byte[][] oldState = m_state;

                m_state = null;

                Editor.Undo.CreateRecord(redoRecord =>
                {
                    LoadState(newState);
                    UpdateTargetAnimation();
                    SetCurrentClip(CurrentClip);

                    return true;
                },
                undoRecord =>
                {
                    LoadState(oldState);
                    UpdateTargetAnimation();
                    SetCurrentClip(CurrentClip);

                    return true;
                });

                InitLocalEulerUpdater();
            }
        }

        private byte[][] SaveState()
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

        private void LoadState(byte[][] state)
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

        private Type GetSurrogateType(Type type)
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


    }
}

