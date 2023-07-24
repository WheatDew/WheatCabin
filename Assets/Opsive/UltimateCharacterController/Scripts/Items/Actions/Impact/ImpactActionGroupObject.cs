/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for ImpactAction scriptable Objects.
    /// </summary>
    public abstract class ImpactActionObject : ScriptableObject
    {
        /// <summary>
        /// Method which performs the impact action.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactAction belongs to.</param>
        /// <param name="ctx">Context about the hit.</param>
        /// <param name="forceImpact">Force the impact even if the action is not multi hit.</param>
        public abstract void DoImpact(GameObject character, CharacterItemAction characterItemAction,ImpactCallbackContext ctx, bool forceImpact);
    }
    
    /// <summary>
    /// A scriptable object that contains a impact action group.
    /// </summary>
    [CreateAssetMenu(fileName = "ImpactActionGroupObject", menuName = "Opsive/Impact/Impact Action Group Object", order = 1)]
    public class ImpactActionGroupObject : ImpactActionObject
    {
        [Tooltip("The Impact Action Group.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActionGroup = ImpactActionGroup.DefaultDamageGroup(true);

        /// <summary>
        /// Method which performs the impact action.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactAction belongs to.</param>
        /// <param name="ctx">Context about the hit.</param>
        /// <param name="forceImpact">Force the impact even if the action is not multi hit.</param>
        public override void DoImpact(GameObject character, CharacterItemAction characterItemAction, ImpactCallbackContext ctx, bool forceImpact)
        {
            m_ImpactActionGroup.Initialize(character, characterItemAction);
            m_ImpactActionGroup.OnImpact(ctx, forceImpact);
        }
    }
    
    /// <summary>
    /// This action will print the impact context in the console. 
    /// </summary>
    [Serializable]
    public class UseImpactActionObject : ImpactAction
    {
        [Tooltip("The scriptable Object containing the impact action objects.")]
        [SerializeField] private ImpactActionObject m_ImpactActionObject;

        /// <summary>
        /// Invoke the impact action.
        /// </summary>
        /// <param name="ctx">The impact callback data.</param>
        /// <param name="forceImpact">Force the impact?</param>
        public override void TryInvokeOnImpact(ImpactCallbackContext ctx, bool forceImpact)
        {
            if (m_ImpactActionObject == null) {
                return;
            }
            m_ImpactActionObject.DoImpact(m_StateBoundGameObject, m_CharacterItemAction, ctx, forceImpact);
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            //DO nothing.
        }

        /// <summary>
        /// To string writes the type name.
        /// </summary>
        /// <returns>The string name.</returns>
        public override string ToString()
        {
            if (m_ImpactActionObject == null) {
                return base.ToString();
            } else {
                return "Object: " + m_ImpactActionObject.name;
            }
        }
    }
}