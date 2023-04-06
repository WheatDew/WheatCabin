using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileVector2Editor : FourFloatEditor<Vector2>
    {
        protected override float GetW(Vector2 v)
        {
            return float.NaN;
        }

        protected override float GetX(Vector2 v)
        {
            return v.x;
        }

        protected override float GetY(Vector2 v)
        {
            return v.y;
        }

        protected override float GetZ(Vector2 v)
        {
            return float.NaN;
        }

        protected override Vector2 SetW(Vector2 v, float w)
        {
            return v;
        }

        protected override Vector2 SetX(Vector2 v, float x)
        {
            v.x = x;
            return v;
        }

        protected override Vector2 SetY(Vector2 v, float y)
        {
            v.y = y;
            return v;
        }

        protected override Vector2 SetZ(Vector2 v, float z)
        {
            return v;
        }
    }
}

