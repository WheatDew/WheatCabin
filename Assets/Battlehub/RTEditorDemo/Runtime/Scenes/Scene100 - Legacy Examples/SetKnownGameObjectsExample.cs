using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class SetKnownGameObjectsExample : EditorExtension
    {
        [SerializeField]
        private GameObjectsAsset m_knownGameObjects = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            settings.KnownGameObjects = m_knownGameObjects;
        }
    }
}
