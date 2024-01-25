/// <summary>
/// Project : Easy Build System
/// Class : Demo_SleepPhysicsOff.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace EasyBuildSystem.Examples.Bases.Scripts
{
    public class Demo_PhysicsSleepingOff : MonoBehaviour
    {
        void Start()
        {
            BuildingManager.Instance.OnPlacingBuildingPartEvent.AddListener((BuildingPart buildingPart) => 
            {
                if (buildingPart.TryGetPhysicsCondition != null)
                {
                    buildingPart.TryGetPhysicsCondition.IsSleeping = false;
                }
            });

            foreach (BuildingPart buildingPart in BuildingManager.Instance.RegisteredBuildingParts)
            {
                if (buildingPart.TryGetPhysicsCondition != null)
                {
                    buildingPart.TryGetPhysicsCondition.IsSleeping = false;
                }
            }
        }
    }
}