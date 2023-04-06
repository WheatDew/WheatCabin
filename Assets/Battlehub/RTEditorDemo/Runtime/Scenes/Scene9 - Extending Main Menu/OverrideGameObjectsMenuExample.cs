using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This script overrides Game Object menu with prefabs from GameObjectsAsset
    /// Right mouse button click -> Create -> Runtime Editor -> Game Objects Asset
    /// </summary>
    public class OverrideGameObjectsMenuExample : EditorExtension
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
