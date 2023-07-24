/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Events
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Items;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// (float) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityFloatEvent : UnityEvent<float> { }

    /// <summary>
    /// (Transform) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityTransformEvent : UnityEvent<Transform> { }

    /// <summary>
    /// (MovementType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityMovementTypeBoolEvent : UnityEvent<MovementType, bool> { }

    /// <summary>
    /// (Ability, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityAbilityBoolEvent : UnityEvent<Ability, bool> { }

    /// <summary>
    /// (ItemAbility, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityItemAbilityBoolEvent : UnityEvent<ItemAbility, bool> { }

    /// <summary>
    /// (Item) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemEvent : UnityEvent<CharacterItem> { }

    /// <summary>
    /// (Item, int) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemIntEvent : UnityEvent<CharacterItem, int> { }

    /// <summary>
    /// (IItemIdentifier, float) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityItemIdentifierFloatEvent : UnityEvent<IItemIdentifier, float> { }

    /// <summary>
    /// (Item, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemBoolBoolEvent : UnityEvent<CharacterItem, bool, bool> { }

    /// <summary>
    /// (Item, float, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemFloatBoolBoolEvent : UnityEvent<CharacterItem, float, bool, bool> { }

    /// <summary>
    /// (Item, RaycastHit, SurfaceImpact) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemRaycastHitSurfaceImpactEvent : UnityEvent<CharacterItem, RaycastHit, SurfaceSystem.SurfaceImpact> { }

    /// <summary>
    /// (IItemIdentifier, float, bool, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityItemIdentifierFloatBoolBoolEvent : UnityEvent<IItemIdentifier, float, bool, bool> { }

    /// <summary>
    /// (Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityVector3Vector3GameObjectEvent : UnityEvent<Vector3, Vector3, GameObject> { }

    /// <summary>
    /// (float, Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable] public class UnityFloatVector3Vector3GameObjectEvent : UnityEvent<float, Vector3, Vector3, GameObject> { }

    /// <summary>
    /// (ViewType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityViewTypeBoolEvent : UnityEvent<ViewType, bool> { }
}