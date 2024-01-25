/// <summary>
/// Project : Easy Build System
/// Class : InstantiateOnDestroy.cs
/// Namespace : EasyBuildSystem.Packages.Addons.WoodenModularBuilding
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Part;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Packages.Addons.WoodenModularBuilding
{
    public class InstantiateOnDestroy : MonoBehaviour
    {
        [SerializeField] GameObject m_BuildingDebris;
        [SerializeField] float m_BuildingDebrisLifetime = 15f;

        BuildingPart m_BuildingPart;
        public BuildingPart GetBuildingPart
        {
            get
            {
                if (m_BuildingPart == null)
                {
                    m_BuildingPart = GetComponentInParent<BuildingPart>();
                }

                return m_BuildingPart;
            }
        }

        void Start()
        {
            if (GetBuildingPart != null)
            {
                if (GetBuildingPart.TryGetPhysicsCondition != null)
                {
                    GetBuildingPart.TryGetPhysicsCondition.FallingTime = 0f;

                    GetBuildingPart.TryGetPhysicsCondition.OnFallingBuildingPartEvent.AddListener(() =>
                    {
                        Destroy(Instantiate(m_BuildingDebris,
                            transform.position, transform.rotation), m_BuildingDebrisLifetime);
                    });
                }
            }
        }

        void OnDestroy()
        {
            if (!this.gameObject.scene.isLoaded) return;

            if (GetBuildingPart == null)
            {
                return;
            }

            if (GetBuildingPart.State == BuildingPart.StateType.PREVIEW)
            {
                return;
            }

            GameObject instancedFracture = Instantiate(m_BuildingDebris,
                        transform.position, transform.rotation);

            instancedFracture.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"));

            Destroy(instancedFracture, m_BuildingDebrisLifetime);
        }
    }
}