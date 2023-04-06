using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class AnimationTimelineView : MonoBehaviour
    {
        public event Action ClipBeginModify;
        public event Action ClipModified;

#pragma warning disable 0414
        [SerializeField]
        private GameObject m_timeline = null;

        [SerializeField]
        private float m_defaultClipLength = 1; //seconds

        public int CurrentSample
        {
            get
            {
                if(IsDopesheet)
                {
                    if(m_dopesheet == null)
                    {
                        return 0;
                    }

                    return m_dopesheet.CurrentSample;
                }

                if(m_curves == null)
                {
                    return 0;
                }

                return m_curves.CurrentSample;
            }
        }

        [SerializeField]
        private TimelineControl m_dopesheet = null;
        private ITimelineControl Dopesheet
        {
            get { return m_dopesheet; }
        }


        [SerializeField]
        private TimelineControl m_curves = null;
#pragma warning restore 0219
        public bool IsDopesheet
        {
            get;
            set;
        }

        private RuntimeAnimation m_target;

        public RuntimeAnimation Target
        {
            get { return m_target; }
            set
            {
                if(m_target != value)
                {
                    m_target = value;
                    SetCurrentSample();
                }
            }
        }

        private void SetCurrentSample()
        {
            if (m_target != null && m_target.Clips != null && m_target.ClipIndex > -1 && m_target.ClipIndex < m_target.Clips.Count && m_target.Clips[m_target.ClipIndex] != null && m_target.State != null)
            {
                int frameNumber = Mathf.CeilToInt(m_target.State.time * m_target.Clips[m_target.ClipIndex].Clip.frameRate);
                m_dopesheet.SetSample(frameNumber);
            }
            else
            {
                m_dopesheet.SetSample(0);
            }
        }

        private RuntimeAnimationClip m_clip;
        public RuntimeAnimationClip Clip
        {
            get { return m_clip; }
            set
            {
                
                m_clip = value;
                if (m_clip != null)
                {                    
                    AnimationClip clip = m_clip.Clip;
                    const int frameRate = 60;
                    clip.frameRate = frameRate;

                    int samplesCount;
                    if (clip.empty)
                    {
                        samplesCount = Mathf.CeilToInt(m_defaultClipLength * clip.frameRate);
                    }
                    else
                    {
                        samplesCount = Mathf.CeilToInt(clip.length * clip.frameRate);
                    }
                    
                    Dopesheet.Clip = new Dopesheet.DsAnimationClip(samplesCount, frameRate);
                    Dopesheet.VisibleRowsCount = 1;

                    List<RuntimeAnimationProperty> addedProperties = new List<RuntimeAnimationProperty>();
                    List<int> addedIndexes = new List<int>();
                    if (m_clip.Properties.Count > 0)
                    {
                        addedProperties.Add(new RuntimeAnimationProperty { ComponentTypeName = RuntimeAnimationProperty.k_SpecialEmptySpace });
                        addedIndexes.Add(0);
                    }
                    int index = 1;
                    foreach (RuntimeAnimationProperty property in m_clip.Properties)
                    {
                        addedProperties.Add(property);
                        addedIndexes.Add(index);
                        index++;

                        if (property.Children != null)
                        {
                            for (int i = 0; i < property.Children.Count; i++)
                            {
                                addedProperties.Add(property.Children[i]);
                                addedIndexes.Add(index);
                                index++;
                            }
                        }
                    }

                    //Dopesheet.BeginRefresh();
                    AddRows(addedIndexes.ToArray(), addedProperties.ToArray(), false);
                    Dopesheet.Refresh(true, true, false);
                }
                else
                {
                    Dopesheet.Clip = new Dopesheet.DsAnimationClip();
                    Dopesheet.VisibleRowsCount = 1;
                }

                if (m_timeline != null)
                {
                    m_timeline.SetActive(m_clip != null);
                }

                SetCurrentSample();
            }
        }

        private void Start()
        {
            Dopesheet.VisibleRowsCount = 1;
            Dopesheet.ClipBeginModify += OnClipBeginModify;
            Dopesheet.ClipModified += OnClipModified;
            Dopesheet.SampleChanged += SyncTargetWithDopesheet;
            if (m_timeline != null)
            {
                m_timeline.SetActive(m_clip != null);
            }
        }

        private void OnDestroy()
        {
            if((Dopesheet as Component) != null)
            {
                Dopesheet.ClipBeginModify -= OnClipBeginModify;
                Dopesheet.ClipModified -= OnClipModified;
                Dopesheet.SampleChanged -= SyncTargetWithDopesheet;
            }
        }

        private void Update()
        {
            if(Target != null && Target.IsPlaying)
            {
                Dopesheet.SetNormalizedTime(Target.NormalizedTime % 1, false);
                if(Target.NormalizedTime > 1)
                {
                    Target.NormalizedTime %= 1;
                }
            }
        }

        public void BeginSetKeyframeValues(bool refresh)
        {
            Dopesheet.BeginSetKeyframeValues(refresh);
        }

        public void EndSetKeyframeValues(bool refresh)
        {
            Dopesheet.EndSetKeyframeValues(refresh);
        }

        public void SetKeyframeValue(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.SetKeyframeValue(Convert.ToSingle(property.Value), row);
        }

        public void AddRows(int[] rows, RuntimeAnimationProperty[] properties, bool isNew = true)
        {
            //if(isNew)
            //{
            //    OnClipBeginModify();
            //}

            int parentIndex = 0;
            for(int i = 0; i < properties.Length; ++i)
            {
                RuntimeAnimationProperty property = properties[i];

                if(property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace)
                {
                    Dopesheet.AddRow(true, isNew, -1, 0, null);
                }
                else
                {
                    if(property.Parent == null)
                    {
                        if (property.Curve != null)
                        {
                            Dopesheet.AddRow(true, isNew, 0, property.FloatValue, property.Curve);
                        }
                        else
                        {
                            parentIndex = rows[i];
                            Dopesheet.AddRow(true, isNew, 0, 0, null);
                        }
                    }
                    else
                    {
                        Dopesheet.AddRow(false, isNew, parentIndex, property.FloatValue, property.Curve);
                    }
                }
            }

            if(isNew)
            {
                OnClipModified();
            }
            else
            {
                float clipLength = Clip.Clip.length;
                Dopesheet.BeginSetKeyframeValues(false);
                for (int i = 0; i < properties.Length; ++i)
                {
                    RuntimeAnimationProperty property = properties[i];
                    if (property.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace)
                    {
                        continue;
                    }

                    AnimationCurve curve = property.Curve;
                    if (curve != null)
                    {
                        Keyframe[] keys = curve.keys;
                        for (int k = 0; k < keys.Length; ++k)
                        {
                            Keyframe kf = keys[k];

                            int sample = Mathf.RoundToInt(kf.time * Dopesheet.SamplesCount / clipLength);
                            Dopesheet.SetKeyframeValue(kf.value, rows[i], sample);
                        }
                    }
                }
                Dopesheet.EndSetKeyframeValues(false);
            }
        }

        public void RemoveRows(int[] rows, RuntimeAnimationProperty[] properties)
        {
            //Dopesheet.BeginRefresh();

            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveKeyframes(rows[i]);
            }

            for (int i = properties.Length - 1; i >= 0; --i)
            {
                Dopesheet.RemoveRow(rows[i]);
            }

            Dopesheet.Refresh(true, true, true);
        }
              
        public void ExpandRow(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.Expand(row, property.Children.Count);
        }

        public void CollapseRow(int row, RuntimeAnimationProperty property)
        {
            Dopesheet.Collapse(row, property.Children.Count);
        }

        public void SetSample(int sampleNumber)
        {
            Dopesheet.SetSample(sampleNumber);
        }

        public void NextSample()
        {
            Dopesheet.NextSample();
        }

        public void PrevSample()
        {
            Dopesheet.PrevSample();
        }

        public void LastSample()
        {
            Dopesheet.LastSample();
        }

        public void FirstSample()
        {
            Dopesheet.FirstSample();
        }

        private void OnClipBeginModify()
        {
            if(ClipBeginModify != null)
            {
                ClipBeginModify();
            }
        }

        private void OnClipModified()
        {
            if(ClipModified != null)
            {
                ClipModified();
            }
        }

        public void SyncTargetWithDopesheet()
        {
            if(Target != null)
            {
                Target.NormalizedTime = Dopesheet.NormalizedTime;
            }
        }
    }

}

