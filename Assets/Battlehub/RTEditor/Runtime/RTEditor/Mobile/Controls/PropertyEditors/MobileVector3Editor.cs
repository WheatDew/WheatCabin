using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileVector3Editor : FourFloatEditor<Vector3>, IVector3Editor
    {
        public bool? IsInteractable
        {
            get
            {
                if (IsXInteractable == IsYInteractable && IsYInteractable == IsZInteractable)
                {
                    return IsXInteractable;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    IsXInteractable = value.Value;
                    IsYInteractable = value.Value;
                    IsZInteractable = value.Value;
                }
            }
        }

        public bool IsXInteractable
        {
            get { return m_xInput.interactable; }
            set
            {
                m_xInput.interactable = value;
                if (m_dragFields.Length > 0)
                {
                    m_dragFields[0].enabled = value;
                }
            }
        }
        public bool IsYInteractable
        {
            get { return m_yInput.interactable; }
            set
            {
                m_yInput.interactable = value;
                if (m_dragFields.Length > 1)
                {
                    m_dragFields[1].enabled = value;
                }
            }
        }
        public bool IsZInteractable
        {
            get { return m_zInput.interactable; }
            set
            {
                m_zInput.interactable = value;
                if (m_dragFields.Length > 2)
                {
                    m_dragFields[2].enabled = value;
                }
            }
        }

        protected override float GetW(Vector3 v)
        {
            return float.NaN;
        }

        protected override float GetX(Vector3 v)
        {
            return v.x;
        }

        protected override float GetY(Vector3 v)
        {
            return v.y;
        }

        protected override float GetZ(Vector3 v)
        {
            return v.z;
        }

        protected override Vector3 SetW(Vector3 v, float w)
        {
            return v;
        }

        protected override Vector3 SetX(Vector3 v, float x)
        {
            v.x = x;
            return v;
        }

        protected override Vector3 SetY(Vector3 v, float y)
        {
            v.y = y;
            return v;
        }

        protected override Vector3 SetZ(Vector3 v, float z)
        {
            v.z = z;
            return v;
        }
    }
}

