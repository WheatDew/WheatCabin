using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(IList<string>), typeof(List<TMP_Dropdown.OptionData>))]
    public class IListOfStringToListOfTMP_DropdownOptionData : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if (valueIn == null)
            {
                return null;
            }

            IEnumerable<string> enumerable = (IEnumerable<string>)valueIn;
            return enumerable.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
        }
    }
}
