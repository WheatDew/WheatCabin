using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class LayerMaskEditor : FlagsEditor<LayerMask>
    {
        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            List<RangeOptions.Option> options = new List<RangeOptions.Option>();
            LayersInfo layersInfo = LayersEditor.LoadedLayers;
            if(layersInfo != null)
            {
                foreach (LayersInfo.Layer layer in layersInfo.Layers)
                {
                    if (!string.IsNullOrEmpty(layer.Name))
                    {
                        options.Add(new RangeOptions.Option(layer.Name, layer.Index));
                    }
                }
            }
            
            Options = options.ToArray();

            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
        }

        protected override void ResetCurrentValue()
        {
            CurrentValue = -1;
        }

        protected override LayerMask ToValue()
        {
            return ToValueImpl();
        }

        protected override int[] FromValue(LayerMask value)
        {
            return FromValueImpl(value);
        }
    }

}
