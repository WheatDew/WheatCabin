using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(IEnumerable), typeof(IEnumerable<ExposeToEditor>))]
    public class IEnumerableToIEnumerableOfExposeToEditorAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if(valueIn == null)
            {
                return null;
            }

            IEnumerable enumerable = (IEnumerable)valueIn;
            return enumerable.Cast<ExposeToEditor>();
        }
    }
}
