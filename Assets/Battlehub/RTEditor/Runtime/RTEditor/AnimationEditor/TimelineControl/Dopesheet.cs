using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
    public class Dopesheet : MonoBehaviour
    {
        public class DsAnimationClip
        {
            public event Action Modified;

            public int VisibleRowsCount
            {
                get;
                private set;
            }

            public int RowsCount
            {
                get { return m_rows.Count; }
            }
            
            public int ColsCount
            {
                get;
                private set;
            }

            public int FrameRate
            {
                get;
                private set;
            }

            public int FirstSample
            {
                get;
                private set;
            }

            public int LastSample
            {
                get;
                private set;
            }

            public int SamplesCount
            {
                get { return ColsCount - 1; }
            }

            private readonly Dictionary<int, DsRow> m_visibleIndexToRow = new Dictionary<int, DsRow>();
            private readonly List<DsRow> m_rows = new List<DsRow>();

            private readonly List<DsKeyframe> m_keyframes = new List<DsKeyframe>();
            private readonly List<DsKeyframe> m_selectedKeyframes = new List<DsKeyframe>();

            private readonly Dictionary<int, DsKeyframe> m_kfDictionary = new Dictionary<int, DsKeyframe>();
            private readonly Dictionary<int, DsKeyframe> m_selectedKfDictionary = new Dictionary<int, DsKeyframe>();

            public IList<DsRow> Rows
            {
                get
                {
                    return m_rows;
                }
            }

            public IList<DsKeyframe> Keyframes
            {
                get { return m_keyframes; }
            }

            public IList<DsKeyframe> SelectedKeyframes
            {
                get { return m_selectedKeyframes; }
            }

            public DsRow GetRowByVisibleIndex(int visibleIndex)
            {
                DsRow result;
                if(!m_visibleIndexToRow.TryGetValue(visibleIndex, out result))
                {
                    return null;
                }
                return result;
            }
            
            public bool IsSelected(int row, int col)
            {
                int key = row * ColsCount + col;
                return m_selectedKfDictionary.ContainsKey(key);
            }

            public bool HasKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                return m_selectedKfDictionary.ContainsKey(key) || m_kfDictionary.ContainsKey(key);
            }

            public DsKeyframe GetSelectedKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                DsKeyframe result;
                if (!m_selectedKfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }

            public DsKeyframe GetKeyframe(int row, int col)
            {
                int key = row * ColsCount + col;
                DsKeyframe result;
                if(!m_kfDictionary.TryGetValue(key, out result))
                {
                    return null;
                }
                return result;
            }

            public void AddKeyframes(params DsKeyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    DsKeyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;
                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);
                    kf.Row.Keyframes.Add(kf);

                    if(kf.Col > LastSample)
                    {
                        LastSample = kf.Col;
                    }
                    if(kf.Col < FirstSample)
                    {
                        FirstSample = kf.Col;
                    }
                }
            }

            public void ClearSelectedKeyframes()
            {
                m_selectedKeyframes.Clear();
                m_selectedKfDictionary.Clear();

                foreach (DsRow row in m_rows)
                {
                    row.SelectedKeyframes.Clear();
                }
            }

            public void RemoveKeyframes(bool all, params DsKeyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    DsKeyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;
                    
                    if(all)
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                            kf.Row.Keyframes.Remove(kf);
                        }
                        else if (m_selectedKfDictionary.TryGetValue(key, out kf))
                        {
                            m_selectedKeyframes.Remove(kf);
                            m_selectedKfDictionary.Remove(key);
                            kf.Row.SelectedKeyframes.Remove(kf);
                        }
                    }
                    else
                    {
                        if (m_kfDictionary.TryGetValue(key, out kf))
                        {
                            m_keyframes.Remove(kf);
                            m_kfDictionary.Remove(key);
                            kf.Row.Keyframes.Remove(kf);
                        }
                    }  
                }
            }

            public void SelectKeyframes(params DsKeyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    DsKeyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    if (m_kfDictionary.TryGetValue(key, out kf))
                    {
                        SelectKeyframe(kf, key);
                        DsRow dopesheetRow = kf.Row;
                        while(dopesheetRow.Parent != null)
                        {
                            DsKeyframe parentKf;
                            int parentKey = dopesheetRow.Parent.Index * ColsCount + kf.Col;
                            if (m_kfDictionary.TryGetValue(parentKey, out parentKf))
                            {
                                SelectKeyframe(parentKf, parentKey);
                            }
                            dopesheetRow = dopesheetRow.Parent;
                        }

                        dopesheetRow = kf.Row;
                        if (dopesheetRow.Children != null)
                        {
                            List<DsKeyframe> childKeyframes = new List<DsKeyframe>();
                            for (int c = 0; c < dopesheetRow.Children.Count; ++c)
                            {
                                DsRow childRow = dopesheetRow.Children[c];
                                DsKeyframe childKeyframe = GetKeyframe(childRow.Index, kf.Col);
                                if (childKeyframe != null)
                                {
                                    childKeyframes.Add(childKeyframe);
                                }
                            }

                            SelectKeyframes(childKeyframes.ToArray());
                        }
                    }
                }
            }

            private void SelectKeyframe(DsKeyframe kf, int key)
            {
                m_keyframes.Remove(kf);
                m_kfDictionary.Remove(key);
                kf.Row.Keyframes.Remove(kf);

                m_selectedKfDictionary.Add(key, kf);
                m_selectedKeyframes.Add(kf);

                DsRow dopesheetRow = kf.Row;
                dopesheetRow.SelectedKeyframes.Add(kf);
            }

            public void UnselectKeyframes(params DsKeyframe[] keyframes)
            {
                for (int i = 0; i < keyframes.Length; ++i)
                {
                    DsKeyframe kf = keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    if (m_selectedKfDictionary.TryGetValue(key, out kf))
                    {
                        m_selectedKeyframes.Remove(kf);
                        m_selectedKfDictionary.Remove(key);
                        kf.Row.SelectedKeyframes.Remove(kf);
                    }
                    else
                    {
                        continue;
                    }

                    m_kfDictionary.Add(key, kf);
                    m_keyframes.Add(kf);
                    kf.Row.Keyframes.Add(kf);
                }
            }

            public DsRow AddRow(bool isVisible, int parentIndex, float initialValue, AnimationCurve curve)
            {
                DsRow row = new DsRow();
                row.IsVisible = isVisible;
                row.Index = m_rows.Count;
                m_rows.Add(row);
                
                if(parentIndex > -1)
                {
                    row.Parent = m_rows[parentIndex];
                    if(row.Parent.Children == null)
                    {
                        row.Parent.Children = new List<DsRow>();
                    }

                    row.Parent.Children.Add(row);
                }

                UpdateRowIndexes();
                return row;
            }

            public bool RemoveRow(int row)
            {
                DsRow dopesheetRow = m_rows[row];
                if(dopesheetRow.Parent != null)
                {
                    dopesheetRow.Parent.Children.Remove(dopesheetRow);
                    if(dopesheetRow.Parent.Children.Count == 0)
                    {
                        dopesheetRow.Parent.Children = null;
                    }
                }
                m_rows.RemoveAt(row);
                UpdateRowIndexes();
                return dopesheetRow.IsVisible;
            }

            public void Expand(int row, int count)
            {
                for(int i = row + 1; i <= row + count; ++i)
                {
                    m_rows[i].IsVisible = true;
                }
                UpdateRowIndexes();
            }

            public void Collapse(int row, int count)
            {
                for (int i = row + 1; i <= row + count; ++i)
                {
                    m_rows[i].IsVisible = false;
                }
                UpdateRowIndexes();
            }

            private void UpdateRowIndexes()
            {
                m_visibleIndexToRow.Clear();

                for (int i = 0; i < m_rows.Count; ++i)
                {
                    m_rows[i].Index = i;
                }

                VisibleRowsCount = 0;
                for (int i = 0; i < m_rows.Count; ++i)
                {
                    if (m_rows[i].IsVisible)
                    {
                        m_rows[i].VisibleIndex = VisibleRowsCount;
                        m_visibleIndexToRow.Add(VisibleRowsCount, m_rows[i]);
                        VisibleRowsCount++;
                    }
                    else
                    {
                        m_rows[i].VisibleIndex = -1;
                    }
                }
            }

            public void ResizeClip(IList<DsKeyframe> keyframes)
            {
                for (int i = 0; i < keyframes.Count; ++i)
                {
                    DsKeyframe kf = keyframes[i];
                    if (ColsCount <= kf.Col || RowsCount <= kf.Row.Index)
                    {
                        ColsCount = Mathf.Max(ColsCount, kf.Col + 1);
                        RefreshDictionaries();
                    }
                }
            }

            public void Refresh(bool dictonaries = true, bool firstAndLastSample = true, bool curves = true)
            {
                if (firstAndLastSample)
                {
                    RefreshFirstAndLastSample();
                }

                if (dictonaries)
                {
                    RefreshDictionaries();
                }

                if(curves)
                {
                    RefreshCurves();
                }
            }

            private void RefreshDictionaries()
            {
                m_kfDictionary.Clear();
                m_selectedKfDictionary.Clear();

                for (int i = 0; i < m_keyframes.Count; ++i)
                {
                    DsKeyframe kf = m_keyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    m_kfDictionary.Add(key, kf);
                }

                for (int i = 0; i < m_selectedKeyframes.Count; ++i)
                {
                    DsKeyframe kf = m_selectedKeyframes[i];
                    int key = kf.Row.Index * ColsCount + kf.Col;

                    m_selectedKfDictionary.Add(key, kf);
                }
            }

            private void RefreshFirstAndLastSample()
            {
                if (m_rows.Count == 0)
                {
                    FirstSample = 0;
                    LastSample = 0;
                }
                else
                {
                    int min = ColsCount - 1;
                    int max = 0;
                    for (int i = 0; i < m_keyframes.Count; ++i)
                    {
                        DsKeyframe keyframe = m_keyframes[i];
                        if (keyframe.Col < min)
                        {
                            min = keyframe.Col;
                        }
                        if (keyframe.Col > max)
                        {
                            max = keyframe.Col;
                        }
                    }

                    for (int i = 0; i < m_selectedKeyframes.Count; ++i)
                    {
                        DsKeyframe keyframe = m_selectedKeyframes[i];
                        if (keyframe.Col < min)
                        {
                            min = keyframe.Col;
                        }
                        if (keyframe.Col > max)
                        {
                            max = keyframe.Col;
                        }
                    }

                    FirstSample = min;
                    LastSample = max;
                    ColsCount = LastSample + 1;
                }
            }

            private void RefreshCurves()
            {
                for(int i = 0; i < m_rows.Count; ++i)
                {
                    DsRow row = m_rows[i];
                    row.RefreshCurve(FrameRate);
                }

                if(Modified != null)
                {
                    Modified();
                }
            }

            public void RefreshCurve(int index)
            {
                DsRow row = m_rows[index];
                row.RefreshCurve(FrameRate);

                if(Modified != null)
                {
                    Modified();
                }
            }

            public void RaiseCurveModified(int index)
            {
                if (Modified != null)
                {
                    Modified();
                }
            }

            public DsAnimationClip(int samplesCount = 60, int frameRate = 60)
            {
                ColsCount = samplesCount + 1;
                FrameRate = frameRate;  
            }

        }

        public class DsRow
        {
            public int Index;
            public int VisibleIndex;
            public readonly List<DsKeyframe> Keyframes = new List<DsKeyframe>();
            public readonly List<DsKeyframe> SelectedKeyframes = new List<DsKeyframe>();
            public bool IsVisible = true;

            public DsRow Parent;
            public List<DsRow> Children;

            public AnimationCurve Curve;
            public void RefreshCurve(float frameRate)
            {
                if(Curve != null)
                {
                    Keyframe[] keyframes = Curve.keys;
                    Array.Resize(ref keyframes, Keyframes.Count + SelectedKeyframes.Count);

                    DsKeyframe[] dsKeyframes = Keyframes.Union(SelectedKeyframes).OrderBy(k => k.Col).ToArray();
                    for(int i = 0; i < dsKeyframes.Length; ++i)
                    {
                        DsKeyframe dsKeyframe = dsKeyframes[i];
                        dsKeyframe.Index = i;

                        Keyframe keyframe = new Keyframe(dsKeyframe.Col / frameRate, dsKeyframe.Value);
                        keyframes[i] = keyframe;
                    }

                    Curve.keys = keyframes;
                }
            }
        }

        public class DsKeyframe
        {
            public DsRow Row;
            public int Col;
            public int Index = -1;

            private float m_value;
            public float Value
            {
                get { return m_value; }
                set
                {
                    m_value = value;
                    if(Index > -1)
                    {
                        Keyframe[] keyframes = Row.Curve.keys;
                        Keyframe keyframe = keyframes[Index];
                        keyframe.value = m_value;
                        keyframes[Index] = keyframe;
                        Row.Curve.keys = keyframes;
                    }
                }
            }
            
            public DsKeyframe(DsRow row, int col, float value)
            {
                Row = row;
                Col = col;
                Value = value;
            }
        }


        [SerializeField]
        private Mesh m_quad = null;

        [SerializeField]
        private Material m_material = null;

        [SerializeField]
        private Material m_selectionMaterial = null;

        private RTECamera m_rteCamera;
        private Camera m_camera;

        private TimelineGridParameters m_parameters = null;

        private const int k_batchSize = 512;
        private Matrix4x4[] m_matrices = new Matrix4x4[k_batchSize];

        private DsAnimationClip m_clip;
        public DsAnimationClip Clip
        {
            get { return m_clip; }
            set { m_clip = value; }
        }

        private void OnDestroy()
        {
            if(m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                Destroy(m_rteCamera);
                m_rteCamera = null;
            }
            
        }

        public void Init(Camera camera)
        {
            if(m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                Destroy(m_rteCamera);
                m_rteCamera = null;
            }

            m_camera = camera;
            m_rteCamera = m_camera.gameObject.AddComponent<RTECamera>();
            m_rteCamera.Event = CameraEvent.BeforeImageEffects;
            m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            Vector2 maxSupportedViewportSize = new Vector2(4096, 4096);
            SetGridParameters(parameters, maxSupportedViewportSize);
        }

        public void SetGridParameters(TimelineGridParameters parameters, Vector2 viewportSize)
        {
            m_parameters = parameters;
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval)
        {
            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            //m_commandBuffer.Clear();

            //int vLinesCount = m_parameters.VertLines;
            //int hLinesCount = m_parameters.HorLines;
            //Color lineColor = m_parameters.LineColor;

            int index = 0;
            //int cols = m_clip.Cols;
            //int rows = m_clip.RowsCount;

            float rowHeight = m_parameters.FixedHeight / viewportSize.y;
            float aspect = viewportSize.x / viewportSize.y;

            //int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            //int hLinesSq = m_parameters.HorLinesSecondary * m_parameters.HorLinesSecondary;

            //Vector2 contentScale = new Vector2(
            //    1.0f / normalizedSize.x,
            //    1.0f / normalizedSize.y);

            Vector3 offset = new Vector3(-0.5f, 0.5f - rowHeight * 0.5f, 1.0f);
            offset.x -= ((1 - normalizedSize.x) * normalizedOffset.x / normalizedSize.x);
            offset.x *= aspect;
            offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y);

            float px = interval.x * normalizedSize.x;
            float visibleColumns = m_parameters.VertLines * Mathf.Pow(m_parameters.VertLinesSecondary, Mathf.Log(px, m_parameters.VertLinesSecondary));
            //float offsetColumns = -(1 - 1 / normalizedSize.x) * normalizedOffset.x * visibleColumns;

            Vector3 keyframeScale = Vector3.one * rowHeight * 0.5f;
            UpdateKeyframes(index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
        }

        private class UpdateKeyframesArgs
        {
            public int Index;
            public float RowHeight;
            public float Aspect;
            public Vector3 Offset;
            public float VisibileColumns;
            public Vector3 KeyframeScale;
        }

        private UpdateKeyframesArgs m_updateKeyframeArgs = new UpdateKeyframesArgs();
        private void UpdateKeyframes(int index, float rowHeight, float aspect, Vector3 offset, float visibleColumns, Vector3 keyframeScale)
        {
            m_updateKeyframeArgs.Index = index;
            m_updateKeyframeArgs.RowHeight = rowHeight;
            m_updateKeyframeArgs.Aspect = aspect;
            m_updateKeyframeArgs.Offset = offset;
            m_updateKeyframeArgs.VisibileColumns = visibleColumns;
            m_updateKeyframeArgs.KeyframeScale = keyframeScale;

            m_rteCamera.RefreshCommandBuffer();
        }

        private void OnCommandBufferRefresh(IRTECamera camera)
        {
            int index = m_updateKeyframeArgs.Index;
            float rowHeight = m_updateKeyframeArgs.RowHeight;
            float aspect = m_updateKeyframeArgs.Aspect;
            Vector3 offset = m_updateKeyframeArgs.Offset;
            float visibleColumns = m_updateKeyframeArgs.VisibileColumns;
            Vector3 keyframeScale = m_updateKeyframeArgs.KeyframeScale;

            UpdateKeyframes(false, m_clip.Rows, m_material, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
            UpdateKeyframes(true, m_clip.Rows, m_selectionMaterial, index, rowHeight, aspect, offset, visibleColumns, keyframeScale);
        }

        private void UpdateKeyframes(bool selected, IList<DsRow> rows, Material material, int index, float rowHeight, float aspect, Vector3 offset, float visibleColumns, Vector3 keyframeScale)
        {
            int rowNumber = 0;
            for (int i = 0; i < rows.Count; ++i)
            {
                DsRow row = rows[i];
                if (row.IsVisible)
                {
                    List<DsKeyframe> keyframes = selected ? row.SelectedKeyframes : row.Keyframes;
                    for (int j = 0; j < keyframes.Count; ++j)
                    {
                        DsKeyframe keyframe = keyframes[j];
                        m_matrices[index] = Matrix4x4.TRS(
                            offset + new Vector3(aspect * keyframe.Col / visibleColumns, -rowHeight * rowNumber, 1),
                            Quaternion.Euler(0, 0, 45),
                            keyframeScale);

                        index++;
                        if (index == k_batchSize)
                        {
                            index = 0;
                            m_rteCamera.CommandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, k_batchSize);
                        }
                    }

                    rowNumber++;
                }
            }

            if (0 < index && index < k_batchSize)
            {
                m_rteCamera.CommandBuffer.DrawMeshInstanced(m_quad, 0, material, 0, m_matrices, index);
            }
        }

    }
}
