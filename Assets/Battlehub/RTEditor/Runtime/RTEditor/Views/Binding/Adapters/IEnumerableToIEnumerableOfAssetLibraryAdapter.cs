using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityWeld.Binding;
using static Battlehub.RTEditor.ViewModels.AssetLibrarySelectViewModel;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(IEnumerable), typeof(IEnumerable<AssetLibrary>))]
    public class IEnumerableToIEnumerableOfAssetLibraryAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            if (valueIn == null)
            {
                return null;
            }

            IEnumerable enumerable = (IEnumerable)valueIn;
            return enumerable.Cast<AssetLibrary>();
        }
    }
}
