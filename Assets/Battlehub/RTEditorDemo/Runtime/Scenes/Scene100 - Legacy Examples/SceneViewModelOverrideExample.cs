using Battlehub.RTEditor.ViewModels;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Demo
{
    [Binding]
    public class SceneViewModelOverrideExample : SceneViewModel
    {
        public override void OnExternalObjectEnter()
        {
            base.OnExternalObjectEnter();
            Debug.Log("OnExternalObjectEnter " + ExternalDragObjects.First());
        }

        public override void OnExternalObjectLeave()
        {
            base.OnExternalObjectLeave();
            Debug.Log("OnExternalObjectLeave " + ExternalDragObjects.First());
        }

        public override void OnActivated()
        {
            base.OnActivated();
            Debug.Log("OnActivated");
        }

        public override void OnDeactivated()
        {
            base.OnDeactivated();
            Debug.Log("OnDeactivated");
        }
    }

}
