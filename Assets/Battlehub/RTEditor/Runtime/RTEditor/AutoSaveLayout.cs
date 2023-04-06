using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-91)]
    public class AutoSaveLayout : LayoutExtension
    {
        protected override void Awake()
        {
            PersistentLayout = true;
            base.Awake();
        }
    }

}
