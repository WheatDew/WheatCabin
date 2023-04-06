using System;
using UnityEngine;

namespace Battlehub.RTCommon
{
    public enum InputAxis
    {
        X,
        Y,
        Z,
        Horizontal,
        Vertical,
    }

    public interface ITouchInput
    {
        bool IsTouchSupported
        {
            get;
        }

        int TouchCount
        {
            get;
        }

        Touch GetTouch(int index);
        
    }

    public interface IInput
    {
        bool IsAnyKeyDown();
        bool IsAnyKey();

        float GetAxis(InputAxis axis);
        bool GetKeyDown(KeyCode key);
        bool GetKeyUp(KeyCode key);
        bool GetKey(KeyCode key);

        bool GetPointerDown(int button);
        bool GetPointerUp(int button);
        bool GetPointer(int button);

        Vector3 GetPointerXY(int pointer);
    }
    
    public class DisabledInput : IInput, ITouchInput
    {
        public float GetAxis(InputAxis axis)
        {
            return 0;
        }

        public bool GetKey(KeyCode key)
        {
            return false;
        }

        public bool GetKeyDown(KeyCode key)
        {
            return false;
        }

        public bool GetKeyUp(KeyCode key)
        {
            return false;
        }

        public bool GetPointer(int button)
        {
            return false;
        }

        public bool GetPointerDown(int button)
        {
            return false;
        }

        public bool GetPointerUp(int button)
        {
            return false;
        }

        public Vector3 GetPointerXY(int pointer)
        {
            if (pointer == 0)
            {
                return Input.mousePosition;
            }
            else
            {
                Touch touch = Input.GetTouch(pointer);
                return touch.position;
            }
        }


        public bool IsAnyKeyDown()
        {
            return false;
        }

        public bool IsAnyKey()
        {
            return false;
        }

        public bool IsTouchSupported
        {
            get { return Input.touchSupported; }
        }

        public int TouchCount
        {
            get { return 0; }
        }

        public Touch GetTouch(int index)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public class InputLow : IInput, ITouchInput
    {
        public virtual bool IsAnyKeyDown()
        {
            return Input.anyKeyDown;
        }

        public virtual bool IsAnyKey()
        {
            return Input.anyKey;
        }

        public virtual bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public virtual bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public virtual bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public virtual float GetAxis(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.X:
                    return Input.GetAxis("Mouse X");
                case InputAxis.Y:
                    return Input.GetAxis("Mouse Y");
                case InputAxis.Z:
                    return Input.GetAxis("Mouse ScrollWheel");
                case InputAxis.Horizontal:
                    return Input.GetAxis("Horizontal");
                case InputAxis.Vertical:
                    return Input.GetAxis("Vertical");
                default:
                    return 0;
            }
        }

        public virtual Vector3 GetPointerXY(int pointer)
        {
            if (pointer == 0)
            {
                return Input.mousePosition;
            }
            else 
            {
                if(Input.touchCount > pointer)
                {
                    Touch touch = Input.GetTouch(pointer);
                    return touch.position;
                }
                return Vector3.zero;
            }
        }

        public virtual bool GetPointerDown(int index)
        {
            return Input.GetMouseButtonDown(index);
        }

        public virtual bool GetPointerUp(int index)
        {
            return Input.GetMouseButtonUp(index);
        }

        public virtual bool GetPointer(int index)
        {
            return Input.GetMouseButton(index);
        }

        public bool IsTouchSupported
        {
            get { return Input.touchSupported; }
        }

        public int TouchCount
        {
            get { return Input.touchCount; }
        }

        public Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }
    }
}
