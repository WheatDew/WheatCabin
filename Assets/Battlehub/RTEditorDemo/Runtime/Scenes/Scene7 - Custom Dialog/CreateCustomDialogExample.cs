using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene6
{
    /// <summary>
    /// This script creates a custom floating window after the completion of the initial layout
    /// </summary>
    public class CreateCustomDialogExample : LayoutExtension
    {
        protected override void OnAfterBuildLayout(IWindowManager wm)
        {       
            base.OnAfterBuildLayout(wm);
            wm.CreateDialogWindow("Custom Example Dialog", "Custom Dialog", 
                (sender, args) => { Debug.Log("On OK"); },
                (sender, args) => { Debug.Log("On Cancel"); });
        }
    }
}
