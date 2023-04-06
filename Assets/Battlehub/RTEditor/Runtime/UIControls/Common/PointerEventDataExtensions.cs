using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls.Common
{
    public static class PointerEventDataExtensions 
    {
        public static bool IsDefaultPointerId(this PointerEventData data)
        {
#if INPUTSYSTEM_1_0_1_OR_NEWER
            ExtendedPointerEventData extendedData = data as ExtendedPointerEventData;
            if(extendedData != null)
            {
                return extendedData.button == PointerEventData.InputButton.Left;
            }
#endif

            return data.pointerId == 0 || data.pointerId == -1;
        }
    }
}

