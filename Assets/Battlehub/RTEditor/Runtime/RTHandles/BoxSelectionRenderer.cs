using Battlehub.RTCommon;
using System;
namespace Battlehub.RTHandles
{
    /*[Obsolete]*/
    public sealed class BoxSelectionRenderer  : SelectionPicker
    {
        public BoxSelectionRenderer(RuntimeWindow window, Action<FilteringArgs> filterCallback = null) : base(window, filterCallback) { }
    }
}

