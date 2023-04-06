using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.Spline3
{
    public class BaseSplineState
    {
        public Vector3[] ControlPoints;
        public ControlPointSettings[] Settings;
        public bool IsSelectable;
        public bool IsLooping;    

        public BaseSplineState(Vector3[] controlPoints, ControlPointSettings[] settings, bool isSelectable, bool isLooping)
        {
            ControlPoints = controlPoints;
            IsSelectable = isSelectable;
            IsLooping = isLooping;
            Settings = settings.Select(s => new ControlPointSettings(s)).ToArray();
        }
    }

    public class ControlPointValue
    {
        public float FloatValue;
        public int IntValue;
        public Vector4 VectorValue;

        public ControlPointValue()
        {
        }

        public ControlPointValue(ControlPointValue value)
        {
            FloatValue = value.FloatValue;
            IntValue = value.IntValue;
            VectorValue = value.VectorValue;
        }

        public ControlPointValue(float value)
        {
            FloatValue = value;
        }

        public ControlPointValue(int value)
        {
            IntValue = value;
        }

        public ControlPointValue(Vector4 value)
        {
            VectorValue = value;
        }
    }

    public class ControlPointSettings
    {
        public static readonly ControlPointValue m_defaultValue = new ControlPointValue();
        public Dictionary<string, ControlPointValue> Settings;

        public ControlPointSettings()
        {

        }

        public ControlPointSettings(ControlPointSettings settings)
        {
            if(settings.Settings != null)
            {
                Settings = settings.Settings.ToDictionary(kvp => kvp.Key, kvp => new ControlPointValue(kvp.Value));
            }   
        }

        public bool ContainsKey(string key)
        {
            return Settings != null && Settings.ContainsKey(key);
        }

        public void Remove(string key)
        {
            if(Settings != null)
            {
                Settings.Remove(key);
            }
        }

        private ControlPointValue Get(string key)
        {
            ControlPointValue val;
            if (Settings == null || !Settings.TryGetValue(key, out val))
            {
                return m_defaultValue;
            }
            return val;
        }

        private bool TryGet(string key, out ControlPointValue val)
        {
            if (Settings == null || !Settings.TryGetValue(key, out val))
            {
                val = m_defaultValue;
                return false;
            }
            return true;
        }

        private void Set(string key, ControlPointValue val)
        {
            if(Settings == null)
            {
                Settings = new Dictionary<string, ControlPointValue>();
            }

            Settings[key] = val;
        }

        public float GetFloat(string key)
        {
            return Get(key).FloatValue;
        }

        public bool TryGetFloat(string key, out float value)
        {
            ControlPointValue val;
            if(TryGet(key, out val))
            {
                value = val.FloatValue;
                return true;
            }
            value = default;
            return false;
        }

        public void SetFloat(string key, float value)
        {
            Set(key, new ControlPointValue(value));
        }

        public bool GetBool(string key)
        {
            return Get(key).IntValue == 1;
        }

        public bool TryGetBool(string key, out bool value)
        {
            ControlPointValue val;
            if (TryGet(key, out val))
            {
                value = val.IntValue == 1;
                return true;
            }
            value = default;
            return false;
        }

        public void SetBool(string key, bool value)
        {
            Set(key, new ControlPointValue(value ? 1 : 0));
        }

        public int GetInt(string key)
        {
            return Get(key).IntValue;
        }

        public bool TryGetInt(string key, out int value)
        {
            ControlPointValue val;
            if (TryGet(key, out val))
            {
                value = val.IntValue;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetEnum<T>(string key, out T value) where T : Enum
        {
            ControlPointValue val;
            if (TryGet(key, out val))
            {
                value = (T)Enum.ToObject(typeof(T), val.IntValue);
                return true;
            }
            value = default;
            return false;
        }

        public void SetInt(string key, int value)
        {
            Set(key, new ControlPointValue(value));
        }

        public Vector4 GetVector(string key)
        {
            return Get(key).VectorValue;
        }

        public bool TryGetVector(string key, out Vector4 value)
        {
            ControlPointValue val;
            if (TryGet(key, out val))
            {
                value = val.VectorValue;
                return true;
            }
            value = default;
            return false;
        }

        public void SetVector(string key, Vector4 value)
        {
            Set(key, new ControlPointValue(value));
        }
    }

    public abstract class BaseSpline : MonoBehaviour
    {
        public abstract Vector3[] LocalControlPoints
        {
            get;
            set;
        }

        public abstract ControlPointSettings[] Settings
        {
            get;
            set;
        }

        public abstract int ControlPointCount
        {
            get;
        }

        public abstract int SegmentsCount
        {
            get;
        }

        public abstract bool IsLooping
        {
            get;
            set;
        }

        public abstract bool IsSelectable
        {
            get;
            set;
        }

        public abstract bool ShowTerminalPoints
        {
            get;
            set;
        }

        public virtual int Insert(int segmentIndex, float offset = 0.5f) { throw new NotImplementedException(); }
        public abstract void Append(float distance = 0);
        public abstract void Prepend(float distance = 0);
        public abstract void Remove(int segmentIndex);

        public abstract Vector3 GetPosition(float t);
        public abstract Vector3 GetPosition(int segmentIndex, float t);
        public abstract Vector3 GetLocalPosition(float t);
        public abstract Vector3 GetLocalPosition(int segmentIndex, float t);
        public abstract Vector3 GetTangent(float t);
        public abstract Vector3 GetTangent(int segmentIndex, float t);
        public abstract Vector3 GetLocalTangent(float t);
        public abstract Vector3 GetLocalTangent(int segmentIndex, float t);
        public abstract Vector3 GetDirection(float t);
        public abstract Vector3 GetDirection(int segmentIndex, float t);
        public abstract Vector3 GetLocalDirection(float t);
        public abstract Vector3 GetLocalDirection(int segmentIndex, float t);
        
        public abstract void SetControlPoint(int index, Vector3 position);
        public abstract void SetLocalControlPoint(int index, Vector3 position);
        public abstract void SetSettings(int index, ControlPointSettings settings);
        public virtual void Refresh(bool positionsOnly)
        {
            m_renderer.Refresh(positionsOnly);
        }

        public abstract float GetT(int segmentIndex);
        public abstract int GetSegmentIndex(ref float t);

        public abstract Vector3 GetControlPoint(int index);
        public abstract Vector3 GetLocalControlPoint(int index);
        public abstract ControlPointSettings GetSettings(int index, bool useSegmentIndex = false);

        public abstract BaseSplineState GetState();
        public abstract void SetState(BaseSplineState state);

        protected SplineRenderer m_renderer;

        protected virtual void Awake()
        {
            m_renderer = gameObject.GetComponent<SplineRenderer>();
            if(m_renderer == null)
            {
                m_renderer = gameObject.AddComponent<SplineRenderer>();
            }
            m_renderer.enabled = false;
        }

        protected virtual void OnDestroy()
        {
            Destroy(m_renderer);
        }
    }

    [DefaultExecutionOrder(-1)]
    public class CatmullRomSpline : BaseSpline
    {
        [SerializeField]
        private Vector3[] m_controlPoints = null;
        private ControlPointSettings[] m_settings;
        [SerializeField]
        private bool m_showTerminalPoints = false;
        [SerializeField]
        private bool m_isLooping = true;
        [SerializeField]
        private bool m_isSelectable = true;
        
        public override bool ShowTerminalPoints
        {
            get { return m_showTerminalPoints; }
            set
            {
                m_showTerminalPoints = value;
                if (m_renderer != null)
                {
                    m_renderer.Refresh(false);
                }
            }
        }

        public override bool IsSelectable
        {
            get { return m_isSelectable; }
            set { m_isSelectable = value; }
        }

        public override bool IsLooping
        {
            get { return m_isLooping; }
            set
            {
                m_isLooping = value;
                if(m_renderer != null)
                {
                    m_renderer.Refresh(false);
                }
            }
        }

        public override Vector3[] LocalControlPoints
        {
            get { return m_controlPoints; }
            set { m_controlPoints = value; }
        }

        public override ControlPointSettings[] Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }

        public override int ControlPointCount
        {
            get { return m_controlPoints != null ? m_controlPoints.Length : 0; }
        }

        public override int SegmentsCount
        {
            get
            {
                if(m_controlPoints == null || m_controlPoints.Length == 0)
                {
                    return 0;
                }

                if(m_isLooping)
                {
                    return m_controlPoints.Length;
                }
                return Mathf.Max(0, m_controlPoints.Length - 3);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if(m_controlPoints == null || m_controlPoints.Length == 0)
            {
                m_controlPoints = new[] { Vector3.back * 3f, Vector3.back, Vector3.forward, Vector3.forward * 3f };
                m_settings = new ControlPointSettings[]
                {
                    new ControlPointSettings(), new ControlPointSettings(), new ControlPointSettings(), new ControlPointSettings()
                };
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override int Insert(int segmentIndex, float offset = 0.5f)
        {
            offset = Mathf.Clamp01(offset);
            List<Vector3> controlPoints = m_controlPoints.ToList();
            List<ControlPointSettings> settings = m_settings.ToList();

            if(IsLooping)
            {
                offset = (segmentIndex + offset) / SegmentsCount;
            }
            else
            {
                if (segmentIndex == ControlPointCount - 2)
                {
                    segmentIndex--;
                }
                offset = (segmentIndex + offset - 1) / SegmentsCount;
            }
            int ctrlPointIndex = segmentIndex + 1;

            controlPoints.Insert(ctrlPointIndex, GetLocalPosition(offset));

            //ControlPointSettings copy = new ControlPointSettings(Settings[segmentIndex]);
            ControlPointSettings defaultSettings = new ControlPointSettings();
            settings.Insert(ctrlPointIndex, defaultSettings);
            m_controlPoints = controlPoints.ToArray();
            m_settings = settings.ToArray();
            m_renderer.Refresh(false);

            return ctrlPointIndex;
        }

        public override void Append(float distance = 0)
        {
            Vector3 position;
            Vector3 tangent;
            ControlPointSettings settings;
            int controlPointsCount = ControlPointCount;
            
            if (controlPointsCount > 1)
            {
                position = m_controlPoints[m_controlPoints.Length - 1];
                tangent = position - m_controlPoints[m_controlPoints.Length - 2];
                settings = new ControlPointSettings(m_settings[m_settings.Length - 1]);
            }
            else if(controlPointsCount == 1)
            {
                position = Vector3.forward;
                tangent = Vector3.forward;
                settings = new ControlPointSettings(m_settings[m_settings.Length - 1]);
            }
            else
            {
                position = Vector3.zero;
                tangent = Vector3.forward;
                settings = new ControlPointSettings();
            }

            Array.Resize(ref m_controlPoints, m_controlPoints.Length + 1);
            Array.Resize(ref m_settings, m_settings.Length + 1);
            m_controlPoints[m_controlPoints.Length - 1] = position + tangent.normalized * distance;
            m_settings[m_settings.Length - 1] = settings;

            m_renderer.Refresh(false);
        }

        public override void Prepend(float distance = 0)
        {
            Vector3 position;
            Vector3 tangent;
            ControlPointSettings settings;
            int controlPointsCount = ControlPointCount;
            if (controlPointsCount > 1)
            {
                position = m_controlPoints[0];
                tangent = position - m_controlPoints[1];
                settings = new ControlPointSettings(m_settings[0]);
            }
            else if (controlPointsCount == 1)
            {
                position = Vector3.back;
                tangent = Vector3.back;
                settings = new ControlPointSettings(m_settings[0]);
            }
            else
            {
                position = Vector3.zero;
                tangent = Vector3.back;
                settings = new ControlPointSettings();
            }

            Array.Resize(ref m_controlPoints, m_controlPoints.Length + 1);
            Array.Resize(ref m_settings, m_settings.Length + 1);
            for(int i = m_controlPoints.Length - 1; i > 0; --i)
            {
                m_controlPoints[i] = m_controlPoints[i - 1];
                m_settings[i] = m_settings[i - 1];
            }
            m_controlPoints[0] = position + tangent.normalized * distance;
            m_settings[0] = settings;

            m_renderer.Refresh(false);
        }

        public override void Remove(int index)
        {
            List<Vector3> controlPoints = m_controlPoints.ToList();
            List<ControlPointSettings> settings = m_settings.ToList();
            controlPoints.RemoveAt(index);
            settings.RemoveAt(index);
            m_controlPoints = controlPoints.ToArray();
            m_settings = settings.ToArray();
            m_renderer.Refresh(false);
        }

        public override Vector3 GetPosition(float t)
        {
            int segmentIndex = ClampIndex(GetSegmentIndex(ref t) + (m_isLooping ? 0 : 1));
            return GetCatmullRomPosition(segmentIndex, t);
        }

        public override Vector3 GetPosition(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomPosition(segmentIndex, t);
        }

        public override Vector3 GetLocalPosition(float t)
        {
            int segmentIndex = ClampIndex(GetSegmentIndex(ref t) + (m_isLooping ? 0 : 1));
            return GetCatmullRomLocalPosition(segmentIndex, t);
        }

        public override Vector3 GetLocalPosition(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomLocalPosition(segmentIndex, t);
        }

        public override Vector3 GetTangent(float t)
        {
            int segmentIndex = ClampIndex(GetSegmentIndex(ref t) + (m_isLooping ? 0 : 1));
            return GetCatmullRomTangent(segmentIndex, t);
        }

        public override Vector3 GetTangent(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomTangent(segmentIndex, t);
        }

        public override Vector3 GetLocalTangent(float t)
        {
            int segmentIndex = ClampIndex(GetSegmentIndex(ref t) + (m_isLooping ? 0 : 1));
            return GetCatmullRomLocalTangent(segmentIndex, t);
        }

        public override Vector3 GetLocalTangent(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomLocalTangent(segmentIndex, t);
        }

        public override Vector3 GetDirection(float t)
        {
            return GetTangent(t).normalized;
        }

        public override Vector3 GetDirection(int segmentIndex, float t)
        {
            return GetTangent(segmentIndex, t).normalized;
        }

        public override Vector3 GetLocalDirection(float t)
        {
            return GetLocalTangent(t).normalized;
        }

        public override Vector3 GetLocalDirection(int segmentIndex, float t)
        {
            return GetLocalTangent(segmentIndex, t).normalized;
        }

        public override void SetControlPoint(int index, Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            if (m_controlPoints[index] != position)
            {
                m_controlPoints[index] = position;
                TryUpdateTerminalControlPoints(index);

                m_renderer.Refresh(true);
            }
        }

        public override void SetLocalControlPoint(int index, Vector3 localPosition)
        {
            if (m_controlPoints[index] != localPosition)
            {
                m_controlPoints[index] = localPosition;
                TryUpdateTerminalControlPoints(index);

                m_renderer.Refresh(true);
            }
        }

        private void TryUpdateTerminalControlPoints(int index)
        {
            if (!m_showTerminalPoints && !m_isLooping)
            {
                if (index == 1 || index == 2)
                {
                    m_controlPoints[0] = m_controlPoints[1] - (m_controlPoints[2] - m_controlPoints[1]);
                }

                int l = m_controlPoints.Length;
                if (index == l - 2 || index == l - 3)
                {
                    m_controlPoints[l - 1] = m_controlPoints[l - 2] - (m_controlPoints[l - 3] - m_controlPoints[l - 2]);
                }
            }
        }

        public override void SetSettings(int index, ControlPointSettings settings)
        {
            m_settings[index] = settings;
        }

        public override Vector3 GetControlPoint(int index)
        {
            return transform.TransformPoint(m_controlPoints[index]);
        }

        public override Vector3 GetLocalControlPoint(int index)
        {
            return m_controlPoints[index];
        }

        public override ControlPointSettings GetSettings(int index, bool useSegmentIndex = false)
        {
            if(useSegmentIndex)
            {
                if (!m_isLooping)
                {
                    index++;
                }
                index = ClampIndex(index);
            }
            return m_settings[index];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            if(m_controlPoints == null)
            {
                return;
            }

            for (int i = 0; i < m_controlPoints.Length; i++)
            {
                if ((i == 0 || i == m_controlPoints.Length - 2 || i == m_controlPoints.Length - 1) && !m_isLooping)
                {
                    continue;
                }

                DisplayCatmullRomSpline(i);
            }
        }

        private void DisplayCatmullRomSpline(int pos)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(pos - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[pos]); 
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(pos + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(pos + 2)]);            
            Vector3 lastPos = p1;

            float resolution = 0.2f;

            int loops = Mathf.FloorToInt(1f / resolution);

            for (int i = 1; i <= loops; i++)
            {
                float t = i * resolution;

                Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                Gizmos.DrawLine(lastPos, newPos);

                lastPos = newPos;
            }
        }

        private int ClampIndex(int pos)
        {
            if (pos < 0)
            {
                pos = m_controlPoints.Length - 1;
            }

            if (pos > m_controlPoints.Length)
            {
                pos = 1;
            }
            else if (pos > m_controlPoints.Length - 1)
            {
                pos = 0;
            }

            return pos;
        }

        public override float GetT(int segmentIndex)
        {
            float segmentSize;
            if (m_isLooping)
            {
                segmentSize = 1.0f / m_controlPoints.Length;
            }
            else
            {
                segmentSize = 1.0f / (m_controlPoints.Length - 3);
            }

            return segmentSize * segmentIndex;
        }

        public override int GetSegmentIndex(ref float t)
        {
            t = Mathf.Clamp01(t);
            float segmentSize;
            int segmentIndex = GetSegmentIndex(t, out segmentSize);

            t = t % segmentSize;
            t = t / segmentSize;
            return segmentIndex;
        }

        private int GetSegmentIndex(float t, out float segmentSize)
        {
            int segmentIndex;
            if (m_isLooping)
            {
                segmentSize = 1.0f / m_controlPoints.Length;
                segmentIndex = ClampIndex(Mathf.FloorToInt(t / segmentSize));
            }
            else
            {
                segmentSize = 1.0f / (m_controlPoints.Length - 3);
                //segmentIndex = ClampIndex(Mathf.FloorToInt(t / segmentSize) + 1);
                segmentIndex = ClampIndex(Mathf.FloorToInt(t / segmentSize));
            }
            return segmentIndex;
        }



        private Vector3 GetCatmullRomLocalPosition(int index, float t)
        {
            Vector3 p0 = m_controlPoints[ClampIndex(index - 1)];
            Vector3 p1 = m_controlPoints[index];
            Vector3 p2 = m_controlPoints[ClampIndex(index + 1)];
            Vector3 p3 = m_controlPoints[ClampIndex(index + 2)];

            return GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomPosition(int index, float t)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(index - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[index]);
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 2)]);

            return GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomLocalTangent(int index, float t)
        {
            Vector3 p0 = m_controlPoints[ClampIndex(index - 1)];
            Vector3 p1 = m_controlPoints[index];
            Vector3 p2 = m_controlPoints[ClampIndex(index + 1)];
            Vector3 p3 = m_controlPoints[ClampIndex(index + 2)];

            return GetCatmullRomTangent(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomTangent(int index, float t)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(index - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[index]);
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 2)]);

            return GetCatmullRomTangent(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomPosition(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return .5f * (
                (-a + 3f * b - 3f * c + d) * (t * t * t)
                + (2f * a - 5f * b + 4f * c - d) * (t * t)
                + (-a + c) * t
                + 2f * b);
        }

        public static Vector3 GetCatmullRomTangent(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return 1.5f * (-a + 3f * b - 3f * c + d) * (t * t)
                    + (2f * a - 5f * b + 4f * c - d) * t
                    + .5f * c - .5f * a;
        }

        //private Vector3 GetCatmullRomPosition(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        //{
        //    return b + (c - b) * t;
        //}

        //public static Vector3 GetCatmullRomTangent(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        //{
        //    if(t == 1)
        //    {
        //        return d - b;
        //    }
        //    else if(t == 0)
        //    {
        //        return c - a;
        //    }

        //    return c - b;
        //}

        public override BaseSplineState GetState()
        {
            return new BaseSplineState(m_controlPoints.ToArray(), m_settings, IsSelectable, IsLooping);
        }

        public override void SetState(BaseSplineState state)
        {
            m_controlPoints = state.ControlPoints.ToArray();
            m_settings = state.Settings.Select(s => new ControlPointSettings(s)).ToArray();
            m_isSelectable = state.IsSelectable;
            m_renderer.Refresh();
        }
    }
}
