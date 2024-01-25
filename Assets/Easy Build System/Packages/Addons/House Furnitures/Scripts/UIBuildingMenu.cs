/// <summary>
/// Project : Easy Build System
/// Class : UIBuildingMenu.cs
/// Namespace : EasyBuildSystem.Packages.Addons.BuildingMenu
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;
using UnityEngine.UI;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Packages.Addons.BuildingMenu
{
    public class UIBuildingMenu : MonoBehaviour
    {
        #region Fields

        [Serializable]
        public class BuildingMenuCategory
        {
            [SerializeField] string m_Name;
            public string Name { get { return m_Name; } }

            [Serializable]
            public class Item
            {
                public BuildingPart BuildingPart;
                public int BuyPrice;
                public int SellPrice;
            }

            [SerializeField] Item[] m_Items;
            public Item[] Items { get { return m_Items; } }
        }

        [SerializeField] BuildingMenuCategory[] m_Categories;

        [SerializeField] int m_Credit = 2500;

        [SerializeField] Text m_CreditText;

        [SerializeField] Button m_UICategoryButton;
        [SerializeField] Transform m_UICategoryContainer;

        [SerializeField] UIBuildingMenuButton m_UIBuildingSlot;
        [SerializeField] Transform m_UIBuildingContainer;

        public int m_CategoryIndex;
        bool m_IsOpened;

        #endregion

        #region Unity Methods

        void Start()
        {
            for (int i = 0; i < m_Categories.Length; i++)
            {
                int index = 0;
                Button instancedCategoryButton = Instantiate(m_UICategoryButton, m_UICategoryContainer);
                instancedCategoryButton.gameObject.SetActive(true);
                instancedCategoryButton.GetComponentInChildren<Text>().text = m_Categories[i].Name;
                instancedCategoryButton.onClick.AddListener(() => { ChangeCategory(index); });
                index = i;
            }

            ChangeCategory(0);

            BuildingManager.Instance.OnPlacingBuildingPartEvent.AddListener((BuildingPart buildingPart) => 
            {
                for (int i = 0; i < m_Categories.Length; i++)
                {
                    for (int x = 0; x < m_Categories[i].Items.Length; x++)
                    {
                        if (m_Categories[i].Items[x].BuildingPart.GetGeneralSettings.Identifier ==
                        buildingPart.GetGeneralSettings.Identifier)
                        {
                            if (m_Credit < m_Categories[i].Items[x].BuyPrice)
                            {
                                Debug.Log("Not enough credit.");
                                BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                                return;
                            }

                            m_Credit -= m_Categories[i].Items[x].BuyPrice;

                            if (m_Credit < m_Categories[i].Items[x].BuyPrice)
                            {
                                BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                                return;
                            }
                        }
                    }
                }
            });

            BuildingManager.Instance.OnDestroyingBuildingPartEvent.AddListener((BuildingPart buildingPart) =>
            {
                if (buildingPart.State == BuildingPart.StateType.PREVIEW)
                {
                    return;
                }

                for (int i = 0; i < m_Categories.Length; i++)
                {
                    for (int x = 0; x < m_Categories[i].Items.Length; x++)
                    {
                        if (m_Categories[i].Items[x].BuildingPart.GetGeneralSettings.Identifier ==
                            buildingPart.GetGeneralSettings.Identifier)
                        {
                            m_Credit += m_Categories[i].Items[x].SellPrice;
                        }
                    }
                }
            });
        }

        void Update()
        {
            m_CreditText.text = "Credit: " + m_Credit + "$";
        }

        #endregion

        #region Internal Methods

        public void ChangeCategory(int index)
        {
            m_CategoryIndex = index;

            for (int x = 0; x < m_UIBuildingContainer.childCount; x++)
            {
                if (m_UIBuildingContainer.GetChild(x).gameObject.activeSelf)
                {
                    Destroy(m_UIBuildingContainer.GetChild(x).gameObject);
                }
            }

            for (int i = 0; i < m_Categories[m_CategoryIndex].Items.Length; i++)
            {
                UIBuildingMenuButton instancedSlot = Instantiate(m_UIBuildingSlot, m_UIBuildingContainer);
                instancedSlot.gameObject.SetActive(true);
                instancedSlot.SetSlot(m_Categories[m_CategoryIndex].Items[i]);

                UIBuildingMenu.BuildingMenuCategory.Item item = m_Categories[m_CategoryIndex].Items[i];

                instancedSlot.UIBuildingButton.onClick.AddListener(() =>
                {
                    if (m_Credit < item.BuyPrice)
                    {
                        Debug.Log("Not enough credit.");
                        BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                        return;
                    }

                    BuildingPlacer.Instance.SelectBuildingPart(item.BuildingPart);
                    BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                });
            }
        }

        #endregion
    }
}