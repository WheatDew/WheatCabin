#define USE_RTE
//#define TIMELINE_CONTROL_DEBUG

using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface ITimelineControl
    {
        event Action ClipBeginModify;
        event Action ClipModified;
        event Action SampleChanged;

        bool IsSelected
        {
            get;
        }

        bool MultiselectMode
        {
            get;
            set;
        }

        int CurrentSample
        {
            get;
        }

        int SamplesCount
        {
            get;
        }

        float NormalizedTime
        {
            get;
        }

        int VisibleRowsCount
        {
            get;
            set;
        }

        Dopesheet.DsAnimationClip Clip
        {
            set;
        }

        void SetNormalizedTime(float value, bool raiseEvent);
        void SetSample(int value);
        void NextSample();
        void PrevSample();
        void LastSample();
        void FirstSample();

        void ChangeInterval(Vector2 delta);
        void BeginSetKeyframeValues(bool refresh);
        void EndSetKeyframeValues(bool refresh = true);
        void SetKeyframeValue(float value, int row, int sample);
        void SetKeyframeValue(float value, int row);

        void RemoveKeyframes(int rowIndex);
        void RemoveSelectedKeyframes();

        void AddRow(bool isVisible, bool isNew, int parentRowIndex, float initialValue, AnimationCurve curve);
        void RemoveRow(int rowIndex);
        
        void Expand(int row, int count);
        void Collapse(int row, int count);
        void BeginRefresh();
        void Refresh(bool dictonaries = true, bool firstAndLastSample = true, bool curves = true);
    }

    [DefaultExecutionOrder(-61)]
    public class TimelineControl : Selectable, ITimelineControl
    {
        public event Action ClipBeginModify;
        public event Action ClipModified;
        public event Action SampleChanged;

        [SerializeField]
        private GameObject RenderCameraPrefab = null;
        [SerializeField]
        private RawImage m_output = null;
        [SerializeField]
        private TimelineTextPanel m_textPanel = null;
        [SerializeField]
        private TimelinePointer m_pointer = null;
        [SerializeField]
        private TimelineBoxSelection m_boxSelection = null;
        
        private Dopesheet m_dopesheet;
        private TimelineGrid m_timelineGrid;

        private Camera m_camera;
        private RenderTextureCamera m_rtCamera;

        private ScrollRect m_scrollRect;
        private RectTransformChangeListener m_rtListener;

        [SerializeField]
        private float m_fixedHeight = -1;
        private float FixedHeight
        {
            get
            {
                if(m_fixedHeight >= 0)
                {
                    return Mathf.Max(1, m_fixedHeight);
                }
                return m_fixedHeight;
            }
        }

        [SerializeField]
        private bool m_virtualizingTreeViewVerticalScrollStyle = true; 
        
        [SerializeField]
        private Color m_backgroundColor = new Color32(0x27, 0x27, 0x27, 0xFF);
        public Color BackgroundColor
        {
            get { return m_backgroundColor; }
            set 
            { 
                if(m_backgroundColor != value)
                {
                    m_backgroundColor = value;

                    if (m_camera != null)
                    {
                        IRenderPipelineCameraUtility cameraUtility = IOC.Resolve<IRenderPipelineCameraUtility>();
                        if (cameraUtility != null)
                        {
                            cameraUtility.SetBackgroundColor(m_camera, m_backgroundColor);
                        }
                        m_camera.backgroundColor = m_backgroundColor;
                    }

                    m_renderGraphics = true;
                }
            }
        }

        private Vector2 m_interval = Vector2.one;

        private TimelineGridParameters m_timelineGridParams;
        private DragAndDropListener m_hScrollbarListener;
        private DragAndDropListener m_vScrollbarListener;
        //private bool m_hScrollValue;
        //private bool m_vScrollValue;
        private bool m_renderGraphics;

        public bool IsSelected
        {
            get;
            private set;
        }

        public bool MultiselectMode
        {
            get;
            set;
        }

        public int CurrentSample
        {
            get
            {
                if(m_pointer != null)
                {
                    return m_pointer.GetSample();
                }
                return 0;
            }
        }

        public int SamplesCount
        {
            get
            {
                return m_dopesheet.Clip.SamplesCount;
            }
        }


        public float NormalizedTime
        {
            get
            {
                if(m_pointer != null)
                {
                    if(Clip.LastSample <= Clip.FirstSample)
                    {
                        return 0;
                    }

                    float sample = CurrentSample;

                    return sample / Clip.SamplesCount;
                }

                return 0;
            }
        }

        public int VisibleRowsCount
        {
            get { return m_timelineGridParams.HorLines - 1; }
            set
            {
                if (m_timelineGridParams == null)
                {
                    CreateDefaultTimelineGridParams();
                }

                m_timelineGridParams.HorLines = value + 1;
                SetTimelineGridParameters();
                m_renderGraphics = true;

                if (m_scrollRect != null)
                {
                    OnRectTransformChanged();
                }   
            }
        }

        public Dopesheet.DsAnimationClip Clip
        {
            private get
            {
                if(m_dopesheet == null)
                {
                    return null;
                }

                return m_dopesheet.Clip;
            }
            set
            {
                if(m_dopesheet == null)
                {
                    return;
                }

                if(m_dopesheet.Clip != null)
                {
                    m_dopesheet.Clip.Modified -= OnClipModified;
                }

                m_dopesheet.Clip = value;

                if (m_dopesheet.Clip != null)
                {
                    m_dopesheet.Clip.Modified += OnClipModified;
                }

                float colums = m_dopesheet.Clip.ColsCount - 1;
                m_interval.x = Mathf.Log(m_timelineGridParams.VertLinesSecondary * colums / (m_timelineGridParams.VertLines * m_timelineGridParams.VertLinesSecondary), m_timelineGridParams.VertLinesSecondary);
                ChangeInterval(Vector2.zero);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if(!Application.isPlaying)
            {
                return;
            }
            
            if (m_textPanel == null)
            {
                m_textPanel = GetComponentInChildren<TimelineTextPanel>(true);
            }

            if (m_pointer == null)
            {
                m_pointer = GetComponentInChildren<TimelinePointer>(true);
            }

            m_scrollRect = GetComponentInChildren<ScrollRect>(true);
            m_scrollRect.scrollSensitivity = 0;
            m_scrollRect.onValueChanged.AddListener(OnInitScrollRectValueChanged);

            m_hScrollbarListener = m_scrollRect.horizontalScrollbar.GetComponentInChildren<DragAndDropListener>(true);
            m_vScrollbarListener = m_scrollRect.verticalScrollbar.GetComponentInChildren<DragAndDropListener>(true);
            m_hScrollbarListener.Drop += OnHorizontalScrollbarDrop;
            m_hScrollbarListener.EndDrag += OnHorizontalScrollbarDrop;
            m_vScrollbarListener.Drop += OnVerticalScrolbarDrop;
            m_vScrollbarListener.EndDrag += OnVerticalScrolbarDrop;

            if (FixedHeight > -1)
            {
                ScrollbarResizer[] resizers = m_scrollRect.verticalScrollbar.GetComponentsInChildren<ScrollbarResizer>(true);
                for (int i = 0; i < resizers.Length; ++i)
                {
                    resizers[i].gameObject.SetActive(false);
                }
            }

            m_rtListener = m_scrollRect.gameObject.AddComponent<RectTransformChangeListener>();
            m_rtListener.RectTransformChanged += OnRectTransformChanged;

            if (m_output == null)
            {
                m_output = m_scrollRect.content.GetComponentInChildren<RawImage>(true);
            }

            GameObject cameraGo = RenderCameraPrefab != null ? Instantiate(RenderCameraPrefab) : new GameObject();
            cameraGo.name = "TimelineGraphicsCamera";
            cameraGo.SetActive(false);

#if USE_RTE
            IRTE editor = IOC.Resolve<IRTE>();
            cameraGo.transform.SetParent(editor.Root, false);
#endif
            m_camera = cameraGo.GetComponent<Camera>();
            if(m_camera == null)
            {
                m_camera = cameraGo.AddComponent<Camera>();
            }

            IRenderPipelineCameraUtility cameraUtility = IOC.Resolve<IRenderPipelineCameraUtility>();
            if(cameraUtility != null)
            {
                cameraUtility.EnablePostProcessing(m_camera, false);
                cameraUtility.SetBackgroundColor(m_camera, m_backgroundColor);
            }

            m_camera.enabled = false;
            m_camera.orthographic = true;
            m_camera.orthographicSize = 0.5f;
            m_camera.clearFlags = CameraClearFlags.SolidColor;
            m_camera.backgroundColor = m_backgroundColor;

            IRenderPipelineCameraUtility cameraUtil = IOC.Resolve<IRenderPipelineCameraUtility>();
            if (cameraUtil != null)
            {
                cameraUtil.ResetCullingMask(m_camera);
            }
            else
            {
                m_camera.cullingMask = 0;
            }
            
            m_rtCamera = cameraGo.AddComponent<RenderTextureCamera>();
            m_rtCamera.Fullscreen = false;
            m_rtCamera.Output = m_output;

            cameraGo.SetActive(true);
            m_rtCamera.enabled = false;

            if(m_timelineGridParams == null)
            {
                CreateDefaultTimelineGridParams();
                m_timelineGridParams.HorLines = 1;
                m_timelineGridParams.HorLinesSecondary = 2;
            }
            
            m_timelineGrid = m_output.GetComponent<TimelineGrid>();
            if (m_timelineGrid == null)
            {
                m_timelineGrid = m_output.gameObject.AddComponent<TimelineGrid>();
            }
            m_timelineGrid.Init(m_camera);


            m_dopesheet = m_output.gameObject.GetComponent<Dopesheet>();
            if (m_dopesheet == null)
            {
                m_dopesheet = m_output.gameObject.AddComponent<Dopesheet>();
            }
            m_dopesheet.Init(m_camera);
            SetTimelineGridParameters();

            Clip = new Dopesheet.DsAnimationClip();

            m_pointer.SampleChanged += OnTimelineSampleChanged;
            m_pointer.PointerDown += OnTimlineClick;
            m_pointer.BeginDrag += OnTimelineBeginDrag;
            m_pointer.Drag += OnTimelineDrag;
            m_pointer.Drop += OnTimelineDrop;

            if (m_boxSelection == null)
            {
                m_boxSelection = GetComponentInChildren<TimelineBoxSelection>();
            }

            if (m_boxSelection != null)
            {
                m_boxSelection.BeginSelection += OnBeginBoxSelection;
                m_boxSelection.Selection += OnBoxSelection;

            }
        }

        protected override void Start()
        {
            base.Start();
            if (!Application.isPlaying)
            {
                return;
            }
            if (GetComponent<TimelineControlInput>() == null)
            {
                gameObject.AddComponent<TimelineControlInput>();
            }
            RenderGraphics();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Application.isPlaying)
            {
                return;
            }
            if (m_rtListener != null)
            {
                m_rtListener.RectTransformChanged -= OnRectTransformChanged;
            }

            if (m_scrollRect != null)
            {
                m_scrollRect.onValueChanged.AddListener(OnInitScrollRectValueChanged);
                m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
            }
            if(m_hScrollbarListener != null)
            {
                m_hScrollbarListener.Drop -= OnHorizontalScrollbarDrop;
                m_hScrollbarListener.EndDrag -= OnHorizontalScrollbarDrop;
            }
            
            if(m_hScrollbarListener != null)
            {
                m_vScrollbarListener.Drop -= OnVerticalScrolbarDrop;
                m_vScrollbarListener.EndDrag -= OnVerticalScrolbarDrop;
            }
            
            if (m_camera != null)
            {
                Destroy(m_camera.gameObject);
            }

            if(m_pointer != null)
            {
                m_pointer.SampleChanged -= OnTimelineSampleChanged;
                m_pointer.PointerDown -= OnTimlineClick;
                m_pointer.BeginDrag -= OnTimelineBeginDrag;
                m_pointer.Drag -= OnTimelineDrag;
                m_pointer.Drop -= OnTimelineDrop;
            }

            if (m_boxSelection != null)
            {
                m_boxSelection.BeginSelection -= OnBeginBoxSelection;
                m_boxSelection.Selection -= OnBoxSelection;
            }

            if(m_dopesheet.Clip != null)
            {
                m_dopesheet.Clip.Modified -= OnClipModified;
            }
        }


        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            IsSelected = true;
        }


        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            IsSelected = false;
        }

        private void CreateDefaultTimelineGridParams()
        {
            m_timelineGridParams = new TimelineGridParameters();
            m_timelineGridParams.VertLines = 12;
            m_timelineGridParams.VertLinesSecondary = TimelineGrid.k_Lines;
            m_timelineGridParams.LineColor = new Color(1, 1, 1, 0.1f);
            m_timelineGridParams.FixedHeight = FixedHeight;
        }

        private void SetTimelineGridParameters()
        {
            if (m_timelineGrid != null)
            {
                m_timelineGrid.SetGridParameters(m_timelineGridParams);
            }

            if (m_dopesheet != null)
            {
                m_dopesheet.SetGridParameters(m_timelineGridParams);
            }

            if (m_textPanel != null)
            {
                m_textPanel.SetGridParameters(m_timelineGridParams.VertLines, m_timelineGridParams.VertLinesSecondary, Clip != null ? Clip.FrameRate  : 60);
            }

            if (m_pointer != null)
            {
                m_pointer.SetGridParameters(m_timelineGridParams);
            }
        }

        private bool TryGetKeyframeWithinRange(Vector2Int coord, int maxRange, out Vector2Int result)
        {
            result = coord;
            int range = 0;
            int row = coord.y;
            int col = coord.x;

            Dopesheet.DsRow dopesheetRow = Clip.GetRowByVisibleIndex(row);
            if (dopesheetRow != null)
            {
                row = dopesheetRow.Index;
                while (range <= maxRange)
                {
                    if (Clip.HasKeyframe(row, col - range))
                    {
                        result = new Vector2Int(col - range, row);
                        return true;
                    }

                    if (Clip.HasKeyframe(row, col + range))
                    {
                        result = new Vector2Int(col + range, row);
                        return true;
                    }

                    range++;
                }
            }

            return false;
        }

        private void OnTimelineSampleChanged(int sample)
        {
            if(SampleChanged != null)
            {
                SampleChanged();
            }
        }

        private void OnTimlineClick(TimelinePointer.PointerArgs args)
        {
            Select();

            Vector2Int coord = new Vector2Int(args.Col, args.Row);
            if (TryGetKeyframeWithinRange(coord, args.Range, out coord))
            {
                if (!Clip.IsSelected(coord.y, coord.x) && !MultiselectMode)
                {
                    UnselectAll();
                }

                Vector2Int min = coord;
                Vector2Int max = coord;
                Select(min, max);
            }
            else
            {
                if (!MultiselectMode)
                {
                    UnselectAll();
                }
            }
        }

        private bool HasUnselectedDescendants(Dopesheet.DsKeyframe kf)
        {
            if(kf.Row.Children != null)
            {
                for(int i = 0; i < kf.Row.Children.Count; ++i)
                {
                    if(Clip.HasKeyframe(kf.Row.Children[i].Index, kf.Col))
                    {
                        if (!Clip.IsSelected(kf.Row.Children[i].Index, kf.Col))
                        {
                            return true;
                        }
                        else
                        {
                            Dopesheet.DsKeyframe childKf = Clip.GetSelectedKeyframe(kf.Row.Children[i].Index, kf.Col);
                            if(HasUnselectedDescendants(childKf))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void OnTimelineBeginDrag()
        {
            Select();

            Dopesheet.DsAnimationClip clip = Clip;


            IList<Dopesheet.DsKeyframe> selectedKeyframes = clip.SelectedKeyframes;
            if(selectedKeyframes != null && selectedKeyframes.Count > 0)
            {
                BeginRefresh();
            }
            List<Dopesheet.DsKeyframe> keyframesWithUnselectedChildren = new List<Dopesheet.DsKeyframe>();
            for (int i = 0; i < selectedKeyframes.Count; ++i)
            {
                Dopesheet.DsKeyframe selectedKeyframe = selectedKeyframes[i];
                if(HasUnselectedDescendants(selectedKeyframe))
                {
                    Dopesheet.DsKeyframe keyframe = new Dopesheet.DsKeyframe(selectedKeyframe.Row, selectedKeyframe.Col, selectedKeyframe.Value);
                    keyframesWithUnselectedChildren.Add(keyframe);
                }
            }

            clip.AddKeyframes(keyframesWithUnselectedChildren.ToArray());
        }

        private void OnTimelineDrag(int delta)
        {
            Dopesheet.DsAnimationClip clip = Clip;

            IList<Dopesheet.DsKeyframe> selectedKeyframes = clip.SelectedKeyframes;
            for(int i = 0; i < selectedKeyframes.Count; ++i)
            {
                Dopesheet.DsKeyframe kf = selectedKeyframes[i];
                kf.Col += delta;   
            }

            clip.ResizeClip(selectedKeyframes);

            m_renderGraphics = true;
        }

        private void OnTimelineDrop()
        {
            Dopesheet.DsAnimationClip clip = Clip;

            Dopesheet.DsKeyframe[] selectedKeyframes = clip.SelectedKeyframes.ToArray();
            Dictionary<int, Dopesheet.DsKeyframe> selectedKfDictionary = new Dictionary<int, Dopesheet.DsKeyframe>();
            for (int i = 0; i < selectedKeyframes.Length; ++i)
            {
                Dopesheet.DsKeyframe keyframe = selectedKeyframes[i];
                if (keyframe.Col < 0)
                {
                    keyframe.Col = 0;
                }

                if (keyframe.Col >= m_pointer.ColumnsCount)
                {
                    keyframe.Col = m_pointer.ColumnsCount - 1;
                }

                int key = keyframe.Row.Index * clip.ColsCount + keyframe.Col;
                if (!selectedKfDictionary.ContainsKey(key))
                {
                    selectedKfDictionary.Add(key, keyframe);
                }
            }

            clip.RemoveKeyframes(false, selectedKeyframes);
            clip.ClearSelectedKeyframes();

            clip.AddKeyframes(selectedKfDictionary.Values.ToArray());
            clip.SelectKeyframes(selectedKfDictionary.Values.ToArray());

            clip.Refresh(true, true, true);

            m_renderGraphics = true;
        }

        private void OnBeginBoxSelection(TimelineBoxSelectionCancelArgs args)
        {
            Vector2Int coord;
            if(m_pointer.GetKeyframeCoordinate(args.LocalPoint, true, false, out coord))
            {
                if (TryGetKeyframeWithinRange(coord, m_pointer.Range, out coord))
                {
                    if (Clip.IsSelected(coord.y, coord.x))
                    {
                        args.Cancel = true;
                    }
                }
            }

            if(!args.Cancel)
            {
                m_pointer.IsDragInProgress = false;
            }
        }

        private void OnBoxSelection(Vector2Int min, Vector2Int max)
        {
            m_pointer.IsDragInProgress = false;

            Dopesheet.DsAnimationClip clip = Clip;
            int rows = clip.VisibleRowsCount;
            int cols = clip.ColsCount;

            min.y = Mathf.Clamp(min.y, 0, rows);
            //min.x = Mathf.Clamp(min.x, 0, cols - 1);
            max.y = Mathf.Clamp(max.y, 0, rows - 1);
            //max.x = Mathf.Clamp(max.x, 0, cols - 1);

            Dopesheet.DsRow minRow = clip.GetRowByVisibleIndex(min.y);
            if(minRow == null)
            {
                return;
            }
            min.y = minRow.Index;

            Dopesheet.DsRow maxRow = clip.GetRowByVisibleIndex(max.y);
            max.y = maxRow.Index;
            if(maxRow.Children != null)
            {
                max.y += maxRow.Children.Count;
            }

            Select(min, max);
        }

        private void UnselectAll()
        {
            Dopesheet.DsAnimationClip clip = Clip;
            clip.UnselectKeyframes(clip.SelectedKeyframes.ToArray());
            m_renderGraphics = true;
        }

        private void Select(Vector2Int min, Vector2Int max)
        {
            Dopesheet.DsAnimationClip clip = Clip;
            int rows = clip.RowsCount;
            int cols = clip.ColsCount;

            max.x = Mathf.Min(max.x, cols - 1);
            min.x = Mathf.Max(min.x, 0);

            List<Dopesheet.DsKeyframe> selectKeyframes = new List<Dopesheet.DsKeyframe>();
            for (int i = min.y; i <= max.y; i++)
            {
                for (int j = min.x; j <= max.x; j++)
                {
                    if (!clip.IsSelected(i, j))
                    {
                        Dopesheet.DsKeyframe kf = clip.GetKeyframe(i, j);
                        if(kf != null)
                        {
                            selectKeyframes.Add(kf);
                        }
                    }
                }
            }
            clip.SelectKeyframes(selectKeyframes.ToArray());
            m_renderGraphics = true;
        }

        private void OnInitScrollRectValueChanged(Vector2 value)
        {
            //This required to skip first scroll rect value change
            m_scrollRect.onValueChanged.RemoveListener(OnInitScrollRectValueChanged);
            m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        private void OnScrollRectValueChanged(Vector2 value)
        {
            RenderGraphics();
        }

        private void OnVerticalScrolbarDrop(UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void OnHorizontalScrollbarDrop(UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void OnRectTransformChanged()
        {
            Vector2 viewportSize = m_scrollRect.viewport.rect.size;

            if (viewportSize != m_output.rectTransform.sizeDelta)
            {
                m_output.rectTransform.sizeDelta = viewportSize;
                m_scrollRect.content.sizeDelta = viewportSize;
            }

            if (m_timelineGridParams.FixedHeight > -1)
            {
                m_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_timelineGridParams.FixedHeight * (m_timelineGridParams.HorLines - 1));
            }
        }

        public void ChangeInterval(Vector2 delta)
        {
            Vector2 newInterval = m_interval - delta;
            
            Vector2 maxInterval = new Vector2(
                3600 * 24, //seconds
                10000.0f
                );

            newInterval.x = Mathf.Clamp(newInterval.x, 1.0f, Mathf.Log(maxInterval.x, m_timelineGridParams.VertLinesSecondary)); //at 60 samples per second
            newInterval.y = Mathf.Clamp(newInterval.y, 1.0f, maxInterval.y); //TODO: handle negative values

            if (newInterval != m_interval)
            {
                m_interval = newInterval;
                m_renderGraphics = true;
            }
        }

        private bool m_raiseCurveModified = true;
        public void BeginSetKeyframeValues(bool refresh)
        {
            m_raiseCurveModified = false;
            if(refresh)
            {
                BeginRefresh();
            }
        }

        public void EndSetKeyframeValues(bool refresh)
        {
            m_raiseCurveModified = true;
            if(refresh)
            {
                Clip.Refresh();
            }
        }

        public void SetKeyframeValue(float value, int row)
        {
            SetKeyframeValue(value, row, CurrentSample);
        }

        public void SetKeyframeValue(float value, int row, int sample)
        {
            Dopesheet.DsKeyframe keyframe = Clip.GetKeyframe(row, sample);
            if(keyframe == null)
            {
                keyframe = Clip.GetSelectedKeyframe(row, sample);
            }
            if (keyframe == null)
            {
                Dopesheet.DsRow dopesheetRow = Clip.Rows[row];
                AddKeyframe(value, row, sample);

                dopesheetRow = dopesheetRow.Parent;
                while (dopesheetRow != null)
                {
                    AddKeyframe(0, dopesheetRow.Index, sample);
                    dopesheetRow = dopesheetRow.Parent;
                }
            }
            else
            {
                keyframe.Value = value;

                if(m_raiseCurveModified)
                {
                    Clip.RaiseCurveModified(keyframe.Row.Index);
                }
            }
        }

        private void AddKeyframe(float value, int row, int sample)
        {
            if(sample < 0 || row < 0)
            {
                return;
            }

            Dopesheet.DsRow dopesheetRow = Clip.Rows[row];
            Dopesheet.DsKeyframe newKeyframe = new Dopesheet.DsKeyframe(dopesheetRow, sample, value);
            Clip.ResizeClip(new[] { newKeyframe });

            if(!Clip.HasKeyframe(row, sample))
            {
                Clip.AddKeyframes(newKeyframe);
            }

            if(m_raiseCurveModified)
            {
                Clip.RefreshCurve(row);
            }
            
            m_renderGraphics = true;
        }

        public void RemoveSelectedKeyframes()
        {
            BeginRefresh();

            List<Dopesheet.DsKeyframe> keyframesList = Clip.SelectedKeyframes.ToList();
            HashSet<int> keyframesHs = new HashSet<int>(keyframesList.Select(kf => kf.Row.Index * Clip.ColsCount + kf.Col));
            List<Dopesheet.DsKeyframe> notRemovedKeyframesList = new List<Dopesheet.DsKeyframe>();

            for (int i = keyframesList.Count - 1; i >= 0; --i)
            {
                Dopesheet.DsKeyframe kf = keyframesList[i];
                if (kf.Row.Children != null)
                {
                    for (int j = 0; j < kf.Row.Children.Count; ++j)
                    {
                        Dopesheet.DsRow childRow = kf.Row.Children[j];
                        Dopesheet.DsKeyframe childKf = Clip.GetKeyframe(childRow.Index, kf.Col);
                        if (childKf != null && !keyframesHs.Contains(childKf.Row.Index * Clip.ColsCount + childKf.Col))
                        {
                            keyframesList.RemoveAt(i);
                            notRemovedKeyframesList.Add(kf);
                            break;
                        }
                    }
                }
            }

            if (notRemovedKeyframesList.Count > 0)
            {
                Dopesheet.DsKeyframe topRowKeyframe = keyframesList.Where(kf => kf.Row.Index == 0).FirstOrDefault();
                if (topRowKeyframe != null && !notRemovedKeyframesList.Contains(topRowKeyframe))
                {
                    notRemovedKeyframesList.Add(topRowKeyframe);
                    keyframesList.Remove(topRowKeyframe);
                }

                Clip.UnselectKeyframes(notRemovedKeyframesList.ToArray());
            }

            Clip.RemoveKeyframes(true, keyframesList.ToArray());
            Clip.Refresh(true, true, true);
            RenderGraphics();
        }

        public void RemoveKeyframes(int rowIndex)
        {
            Dopesheet.DsRow row = Clip.Rows[rowIndex];
            Clip.RemoveKeyframes(true, row.Keyframes.ToArray());
            Clip.RemoveKeyframes(true, row.SelectedKeyframes.ToArray());
        }

        public void AddRow(bool isVisible, bool isNew, int parentRowIndex, float initialValue, AnimationCurve curve)
        {
            Dopesheet.DsRow row = Clip.AddRow(isVisible, parentRowIndex, initialValue, curve);
            row.Curve = curve;

            if (isNew)
            {
                Dopesheet.DsKeyframe kf0 = new Dopesheet.DsKeyframe(row, 0, initialValue);
                if(Clip.ColsCount <= 1)
                {
                    Clip.AddKeyframes(kf0);
                }
                else
                {
                    Dopesheet.DsKeyframe kf1 = new Dopesheet.DsKeyframe(row, Clip.ColsCount - 1, initialValue);
                    Clip.AddKeyframes(kf0, kf1);
                }
                
                row.RefreshCurve(Clip.FrameRate);
            }
            
            if (isVisible)
            {
                VisibleRowsCount++;
            }
        }

        public void RemoveRow(int rowIndex)
        {
            bool isVisible = Clip.RemoveRow(rowIndex);
            if(isVisible)
            {
                VisibleRowsCount--;
            }
        }

        public void Expand(int row, int count)
        {
            VisibleRowsCount += count;
            Clip.Expand(row, count);
        }

        public void Collapse(int row, int count)
        {
            VisibleRowsCount -= count;
            Clip.Collapse(row, count);
        }

        public void BeginRefresh()
        {
            if (ClipBeginModify != null)
            {
                ClipBeginModify();
            }
        }

        public void Refresh(bool dictonaries = true, bool firstAndLastSample = true, bool curves = true)
        {
            if(Clip != null)
            {
                Clip.Refresh(dictonaries, firstAndLastSample, curves);
            }
        }

        public void SetNormalizedTime(float value, bool raiseEvent)
        {
            float sample = 0;
            if (Clip.LastSample > Clip.FirstSample)
            {
                sample = value * Clip.SamplesCount; //Clip.FirstSample + value * (Clip.LastSample - Clip.FirstSample);
            }

            m_pointer.SetSample(Mathf.RoundToInt(sample), raiseEvent);
        }

        public void SetSample(int value)
        {
            int sample = value;
            if(Clip != null)
            {
                sample = Mathf.Clamp(sample, Clip.FirstSample, Clip.LastSample);
            }

            m_pointer.SetSample(sample, true);
        }

        public void NextSample()
        {
            int sample = m_pointer.GetSample();
            if (sample < Clip.LastSample)
            {
                sample++;
                m_pointer.SetSample(sample, true);
            }
        }

        public void PrevSample()
        {
            int sample = m_pointer.GetSample();
            if (sample > Clip.FirstSample)
            {
                sample--;
                m_pointer.SetSample(sample, true);
            }
        }

        public void LastSample()
        {
            m_pointer.SetSample(Clip.LastSample, true);
        }

        public void FirstSample()
        {
            m_pointer.SetSample(Clip.FirstSample, true);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_rtCamera.TryResizeRenderTexture(false))
            {
                m_renderGraphics = true;
            }

            if(m_renderGraphics)
            {
                RenderGraphics();
                m_renderGraphics = false;
            }
        }

        private void RenderGraphics()
        {
            Vector2 viewportSize = m_scrollRect.viewport.rect.size;
            viewportSize.y = Mathf.Max(viewportSize.y, Mathf.Epsilon);

            Vector2 scrollOffset = new Vector2(
                    m_scrollRect.horizontalScrollbar.value,
                    m_scrollRect.verticalScrollbar.value);

            if (m_virtualizingTreeViewVerticalScrollStyle && FixedHeight > 0)
            {
                float possibleRowsCount = Mathf.RoundToInt(viewportSize.y / FixedHeight);
                float visibleRowsCount = Mathf.Min(VisibleRowsCount, possibleRowsCount);
                if(visibleRowsCount < VisibleRowsCount)
                {
                    float magicScrollFix = (0.5f / VisibleRowsCount);
                    int index = Mathf.RoundToInt(((1 - scrollOffset.y) + magicScrollFix) * Mathf.Max((VisibleRowsCount - visibleRowsCount), 0));

                    if(Mathf.Approximately(m_scrollRect.verticalScrollbar.size, 1))
                    {
                        scrollOffset.y = 1;
                    }
                    else
                    {
                        float deltaRow = 1.0f / (VisibleRowsCount - m_scrollRect.verticalScrollbar.size * VisibleRowsCount);
                        scrollOffset.y = 1 - (index * deltaRow);
                    }
                }
                else
                {
                    scrollOffset.y = 1;
                }
            }

            Vector2 scrollSize =  new Vector2(
                    m_scrollRect.horizontalScrollbar.size,
                    m_scrollRect.verticalScrollbar.size);
            
            Vector2 contentSize = m_scrollRect.content.sizeDelta;
            contentSize.y = Mathf.Max(contentSize.y, Mathf.Epsilon);

            Vector2 interval = m_interval;
            
            interval.x = Mathf.Pow(m_timelineGridParams.VertLinesSecondary, interval.x);
            interval.y = Mathf.Pow(m_timelineGridParams.HorLinesSecondary, interval.y);

            m_textPanel.UpdateGraphics(viewportSize.x, contentSize.x, scrollOffset.x, scrollSize.x, interval.x);
            m_timelineGrid.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);
            m_dopesheet.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);
            m_pointer.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);

            m_camera.enabled = true;
            m_camera.Render();
            m_camera.enabled = false;
        }

        private void OnClipModified()
        {
            if(ClipModified != null)
            {
                ClipModified();
            }
        }
    }
}

