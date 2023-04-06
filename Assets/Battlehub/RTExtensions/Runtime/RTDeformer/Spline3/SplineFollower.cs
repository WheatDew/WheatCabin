using UnityEngine;

namespace Battlehub.Spline3
{
    public class SplineFollower : MonoBehaviour
    {
        [SerializeField]
        protected BaseSpline m_spline = null;
        public BaseSpline Spline
        {
            get { return m_spline; }
            set { m_spline = value; }
        }

        [SerializeField]
        protected float m_speed = 1.0f;
        public virtual float Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        protected virtual float CurrentSpeed
        {
            get { return m_speed; }
        }

        [SerializeField]
        protected float m_smoothRotation = 1.0f;
        public float SmoothRotation
        {
            get { return m_smoothRotation; }
            set { m_smoothRotation = value; }
        }

        [SerializeField]
        protected bool m_loop = true;
        public bool Loop
        {
            get { return m_loop; }
            set { m_loop = value; }
        }

        [SerializeField]
        protected bool m_playOnStart = true;
        public bool PlayOnStart
        {
            get { return m_playOnStart; }
            set { m_playOnStart = value; }
        }

        protected bool m_isPlaying = false;
        public virtual bool IsPlaying
        {
            get { return m_isPlaying; }
            set
            {
                if(m_isPlaying != value)
                {
                    m_isPlaying = value;
                    enabled = value;
                    OnStarted();
                }
            }
        }

        protected float m_segT;
        public virtual float SegT
        {
            get { return m_segT; }
        }

        protected float m_t;
        public virtual float T
        {
            get { return m_t; }
            set
            {
                m_t = value;
                m_segT = m_t;
                m_segment = m_spline.GetSegmentIndex(ref m_segT);
            }
        }

        protected int m_segment;
        protected int SegmentIndex
        {
            get { return m_segment; }
            set
            {
                m_segment = value;
                m_t = m_spline.GetT(m_segment);
                m_segT = 0;
            }
        }

        protected virtual void Start()
        {
            Vector3 tangent = m_spline.GetTangent(m_t);
            transform.position = m_spline.GetPosition(0);
            transform.rotation = Quaternion.LookRotation(tangent);

            if(PlayOnStart)
            {
                IsPlaying = true;
            }
            else
            {
                enabled = false;
            }
        }

        protected virtual void Update()
        {
            if(!IsPlaying)
            {
                return;
            }

            float prevT = m_t;
            int prevSegment = m_segment;

            Vector3 tangent = m_spline.GetTangent(m_t);
            float v = tangent.magnitude;
            v *= m_spline.SegmentsCount;
            m_t += (Time.deltaTime * CurrentSpeed) / v;
            m_segT = m_t;
            m_segment = m_spline.GetSegmentIndex(ref m_segT);

            if (m_t >= 1)
            {
                OnCompleted();

                if (m_loop)
                {
                    m_t %= 1;
                }
                else
                {
                    m_t = 1;
                    IsPlaying = false;
                }

                m_segT = m_t;
                m_segment = m_spline.GetSegmentIndex(ref m_segT);
                prevSegment = m_segment;

                if(IsPlaying)
                {
                    OnStarted();
                }
            }

            UpdateFollower(prevT, prevSegment);
        }

        protected virtual void OnStarted()
        {
            m_segT = m_t;
            m_segment = m_spline.GetSegmentIndex(ref m_segT);

            if (!m_spline.IsLooping)
            {
                transform.position = m_spline.GetPosition(m_t);
                transform.rotation = Quaternion.LookRotation(m_spline.GetTangent(m_t));
            }
        }

        protected virtual void OnCompleted()
        {
            
        }

        protected virtual void UpdateFollower(float prevT, int prevSegment)
        {
            transform.position = m_spline.GetPosition(m_t);

            transform.rotation = SmoothRotation <= 0 ?
                Quaternion.LookRotation(m_spline.GetTangent(m_t)) :
                Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_spline.GetTangent(m_t)), Time.deltaTime * CurrentSpeed / SmoothRotation);
        }
    }

}
