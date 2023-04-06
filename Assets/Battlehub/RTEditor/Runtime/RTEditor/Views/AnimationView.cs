using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;


namespace Battlehub.RTEditor.Views
{
    internal class LocalEulerAnglesUpdater
    {
        private RuntimeAnimationClip m_currentClip;
        private RuntimeAnimationProperty m_LocalEulerProp;
        private ExposeToEditor m_target;

        public LocalEulerAnglesUpdater(RuntimeAnimation target, RuntimeAnimationClip currentClip)
        {
            m_currentClip = currentClip;
            m_LocalEulerProp = currentClip.Properties.Where(p => p.AnimationPropertyPath == "m_LocalEuler").FirstOrDefault();
            m_target = target.GetComponent<ExposeToEditor>();
            if (m_target == null)
            {
                throw new ArgumentException("target.GetComponent<ExposeToEditor> == null");
            }
        }

        public void Update(int currentSample)
        {
            if (m_LocalEulerProp == null || m_currentClip == null || m_target == null)
            {
                return;
            }

            float time = currentSample / m_currentClip.Clip.frameRate;

            Vector3 localEuler = m_target.LocalEuler;
            localEuler.x = m_LocalEulerProp.Children[0].Curve.Evaluate(time);
            localEuler.y = m_LocalEulerProp.Children[1].Curve.Evaluate(time);
            localEuler.z = m_LocalEulerProp.Children[2].Curve.Evaluate(time);
            m_target.LocalEuler = localEuler;
        }
    }


    public class AnimationView : View
    {
        private ILocalization m_localization;

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

        [SerializeField]
        private CanvasGroup m_group = null;

        private AnimationPropertiesView m_propertiesView;
        private AnimationTimelineView m_timelineView;
        private AnimationCreateView m_animationCreateView;

        private bool m_wasInPreviewMode;
        private float m_normalizedTime;
        private int m_currentSample;
        
        private List<RuntimeAnimationClip> m_clips = new List<RuntimeAnimationClip>();

        [NonSerialized]
        public UnityEvent CurrentClipChanged = new UnityEvent();
        public RuntimeAnimationClip CurrentClip
        {
            get { return m_propertiesView.Clip; }
            set
            {
                bool clipChanged = m_propertiesView.Clip != value;
                SetCurrentClip(value);
                
                if(clipChanged)
                {
                    CurrentClipChanged?.Invoke();
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

        [NonSerialized]
        public UnityEvent TargetGameObjectChanged = new UnityEvent();

        private GameObject m_targetGameObject;
        public GameObject TargetGameObject
        {
            get { return m_targetGameObject; }
            set
            {
                m_targetGameObject = value;
                UpdateTargetAnimation();
                UpdateCreateViewState();

                TargetGameObjectChanged?.Invoke();
            }
        }

        [NonSerialized]
        public UnityEvent TargetChanged = new UnityEvent();

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

                TargetChanged?.Invoke();
            }
        }

        private bool m_isEditing;
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if(m_isEditing != value)
                {
                    m_isEditing = value;
                    if(m_isEditing)
                    {
                        BeginEdit();
                    }
                    else
                    {
                        EndEdit();
                    }
                }
            }

        }

        [NonSerialized]
        public UnityEvent ClipBeginModify = new UnityEvent();
        [NonSerialized]
        public UnityEvent ClipModified = new UnityEvent();
        [NonSerialized]
        public UnityEvent SaveCurrentClip = new UnityEvent();

     
        private LocalEulerAnglesUpdater m_localEulerUpdater;
        private void InitLocalEulerUpdater()
        {
            m_localEulerUpdater = null;
            if(Target != null && CurrentClip != null)
            {
                m_localEulerUpdater = new LocalEulerAnglesUpdater(Target, CurrentClip);
            }
        }

        protected override void Awake()
        {
            base.Awake();
         
            m_localization = IOC.Resolve<ILocalization>();

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

        protected override void OnDestroy()
        {
            
            if (Editor != null)
            {
                OnSaveCurrentClip();

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
            
            base.OnDestroy();
        }

        protected virtual void Update()
        {
            bool isInPreviewMode = m_target != null && m_target.IsInPreviewMode;
            if (m_previewToggle != null && m_previewToggle.isOn != isInPreviewMode)
            {
                m_previewToggle.isOn = isInPreviewMode;
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


            if(m_localEulerUpdater != null && !IsEditing)
            {
                m_localEulerUpdater.Update(m_timelineView.CurrentSample);
            }
        }

        private void BeginEdit()
        {
            if (m_target)
            {
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
            OnSaveCurrentClip();

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
                if (m_previewToggle != null)
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
            }
        }

        private void OnRecordToggleValueChanged(bool value)
        {
            if (value)
            {
                Debug.Log("Recording...");
            }
            else
            {
                Debug.Log("Record Completed");
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
                OnSaveCurrentClip();
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

        private void OnClipBeginModify()
        {
            ClipBeginModify?.Invoke();
        }

        private void OnClipModified()
        {
            ClipModified?.Invoke();

            InitLocalEulerUpdater();
        }

        private void OnSaveCurrentClip()
        {
            SaveCurrentClip?.Invoke();
        }
    }
}
