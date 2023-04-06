using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.UIControls.MenuControl;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-50), MenuDefinition(order: -70)]
    public class MenuGameObject : MonoBehaviour
    {
        [MenuCommand("MenuGameObject")]
        public void Menu() { } //create placeholder

        [SerializeField]
        private Menu m_menu = null;

        private ISettingsComponent m_settings;

        private void Awake()
        {
            m_settings = IOC.Resolve<ISettingsComponent>();
            m_settings.KnownGameObjectsChanged += OnKnownGameObjectsChanged;

            InitMenu();
        }

        private void OnDestroy()
        {
            if(m_settings != null)
            {
                m_settings.KnownGameObjectsChanged -= OnKnownGameObjectsChanged;
            }
        }

        private void OnKnownGameObjectsChanged(object sender, GameObjectsAsset oldValue, GameObjectsAsset newValue)
        {
            InitMenu();
        }

        private void InitMenu()
        {
            List<MenuItemInfo> menuItems = new List<MenuItemInfo>();
            foreach (string path in m_settings.KnownGameObjects.MenuPath)
            {
                MenuItemInfo menuItem = new MenuItemInfo
                {
                    Path = path,
                    Command = path,
                    Action = new MenuItemEvent()
                };
                menuItem.Action.AddListener(InstantiateGameObject);
                menuItems.Add(menuItem);
            }

            m_menu.SetMenuItems(menuItems.ToArray());
        }

        private void InstantiateGameObject(string cmd)
        {
            GameObject go = m_settings.KnownGameObjects.Instantiate(cmd);
            if (go != null)
            {
                IPlacementModel placementModel = IOC.Resolve<IPlacementModel>();
                placementModel.AddGameObjectToScene(go);
            }
        }
    }

}
