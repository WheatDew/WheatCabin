/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Bindings;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// An Impact Action group is an array of impact action which has a custom inspector.
    /// </summary>
    [Serializable]
    public class ImpactActionConditionGroup
    {
        [Tooltip("The list of impact actions.")]
        [SerializeReference] protected ImpactActionCondition[] m_ImpactActionConditions;

        private ActionModule m_ActionModule;

        public ImpactActionCondition[] ImpactActionConditions
        {
            get => m_ImpactActionConditions;
            set => m_ImpactActionConditions = value;
        }
        
        public int Count { get => m_ImpactActionConditions == null ? 0 : m_ImpactActionConditions.Length; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImpactActionConditionGroup()
        {
            m_ImpactActionConditions = new ImpactActionCondition[0];
        }
        
        /// <summary>
        /// Overload constructor.
        /// </summary>
        /// <param name="action">The first action.</param>
        public ImpactActionConditionGroup(ImpactActionCondition action)
        {
            m_ImpactActionConditions = new[] { action };
        }

        /// <summary>
        /// Overload constructor.
        /// </summary>
        /// <param name="actions">The starting actions.</param>
        public ImpactActionConditionGroup(ImpactActionCondition[] actions)
        {
            m_ImpactActionConditions = actions;
        }

        /// <summary>
        /// A static constructor for the default damage impacts use to quickly setup a component.
        /// </summary>
        /// <param name="useContextData">Use the context data?</param>
        /// <returns>The new impact action group setup for damage.</returns>
        public static ImpactActionConditionGroup DefaultConditionGroup(bool useContextData)
        {
            return new ImpactActionConditionGroup(new ImpactActionCondition[]
            {
                new CheckTargetImpactConditionBehaviour(),
            });
        }
        
        /// <summary>
        /// Initialize the impact actions.
        /// </summary>
        /// <param name="actionModule">The character item action module.</param>
        public void Initialize(ActionModule actionModule)
        {
            m_ActionModule = actionModule;

            if (m_ImpactActionConditions == null) { return; }

            for (int i = 0; i < m_ImpactActionConditions.Length; i++) {
                if(m_ImpactActionConditions[i] == null){ continue; }
                
                if (m_ActionModule != null) {
                    m_ImpactActionConditions[i].Initialize(m_ActionModule.Character, m_ActionModule.CharacterItemAction);
                } else {
                    m_ImpactActionConditions[i].Initialize(null, null);
                }
            }
        }
        
        /// <summary>
        /// Initialize the impact actions.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactActionCondition belongs to.</param>
        public void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            m_ActionModule = null;
            
            if (m_ImpactActionConditions == null) { return; }

            for (int i = 0; i < m_ImpactActionConditions.Length; i++) {
                if(m_ImpactActionConditions[i] == null){ continue; }
                
                m_ImpactActionConditions[i].Initialize(character, characterItemAction);
            }
        }
        
        /// <summary>
        /// Can the impact proceed from the context.
        /// </summary>
        /// <param name="impactCallbackContext">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        public bool CanImpact(ImpactCallbackContext impactCallbackContext)
        {
            if (impactCallbackContext == null) {
                Debug.LogError("impactCallbackData should not be null", m_ActionModule?.CharacterItemAction);
                return false;
            }

            if (m_ImpactActionConditions == null) { return false; }
            
            return CanImpactInternal(impactCallbackContext);
        }

        /// <summary>
        /// Can the impact proceed from the context.
        /// </summary>
        /// <param name="impactCallbackContext">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected virtual bool CanImpactInternal(ImpactCallbackContext impactCallbackContext)
        {
            for (int i = 0; i < m_ImpactActionConditions.Length; i++) {
                if (m_ImpactActionConditions[i] == null) { continue; }

                if (m_ImpactActionConditions[i].CanImpact(impactCallbackContext) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Destroy all impact action objects.
        /// </summary>
        public void OnDestroy()
        {
            if (m_ImpactActionConditions == null) { return; }

            for (int i = 0; i < m_ImpactActionConditions.Length; i++) {
                if(m_ImpactActionConditions[i] == null){ continue; }
                m_ImpactActionConditions[i].OnDestroy();
            }
        }
    }

    /// <summary>
    /// The impact action is the base class for generic actions in the context of an impact usually caused by an item.
    /// </summary>
    [Serializable]
    public abstract class ImpactActionCondition : BoundStateObject, IImpactCondition
    {
        [Tooltip("Is the effect enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        
        public bool Enabled { get => m_Enabled; set => m_Enabled = value; }
        
        protected override GameObject BoundGameObject => m_CharacterItemAction?.gameObject ?? m_StateBoundGameObject;
        protected CharacterItemAction m_CharacterItemAction;

        /// <summary>
        /// Initializes the ImpactActionCondition.
        /// </summary>
        /// <param name="character">The character GameObject.</param>
        /// <param name="characterItemAction">The Item Action that the ImpactActionCondition belongs to.</param>
        public virtual void Initialize(GameObject character, CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            base.Initialize(character);
            InitializeInternal();
        }
        
        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected virtual void InitializeInternal()
        {
            // To be overriden.
        }

        /// <summary>
        /// Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        public virtual bool CanImpact(ImpactCallbackContext ctx)
        {
            // if the condition is disabled, ignore it.
            if (m_Enabled == false) { return true; }
            
            return CanImpactInternal(ctx);
        }


        /// <summary>
        /// Internal, Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected abstract bool CanImpactInternal(ImpactCallbackContext ctx);

        /// <summary>
        /// The condition has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        /// To string writes the type name.
        /// </summary>
        /// <returns>The string name.</returns>
        public override string ToString()
        {
            return GetType()?.Name ?? "(null)";
        }
    }

    /// <summary>
    /// Check the target gameObject to see if it has an Impact condition behavior.
    /// </summary>
    [Serializable]
    public class CheckTargetImpactConditionBehaviour : ImpactActionCondition
    {
        protected override bool CanImpactInternal(ImpactCallbackContext ctx)
        {
            var conditionBehaviour = ctx.ImpactCollisionData?.ImpactGameObject?.GetCachedComponent<ImpactConditionBehaviourBase>();
            
            // No condition behaviour, pass
            if (conditionBehaviour == null) {
                return true;
            }

            return conditionBehaviour.CanImpact(ctx);
        }
    }
    
    /// <summary>
    /// Checks the ObjectIdentifier on the target gameobject.
    /// </summary>
    [Serializable]
    public class ObjectIdentifierImpactCondition : ImpactActionCondition
    {
        [SerializeField] protected IDObject<Transform> m_TargetID;
        [SerializeField] protected bool m_SearchInChildren = true;
        
        /// <summary>
        /// Internal, Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected override bool CanImpactInternal(ImpactCallbackContext ctx)
        {
            if (m_TargetID.TryGetObject(ctx.ImpactCollisionData.ImpactGameObject, m_SearchInChildren, out var obj, true)) {
                return true;
            }

            return false;
        }
    }
    
    /// <summary>
    /// Checks the ObjectIdentifier on the target gameobject.
    /// </summary>
    [Serializable]
    public class CheckWeaponSourceCategory : ImpactActionCondition
    {
        [Tooltip("Allow impact if the source does not have a character item action?")]
        [SerializeField] protected bool m_AllowNonWeaponImpact;
        [Tooltip("if false, then the impact will pass if the weapon inherits the category, if true it will fail.")]
        [SerializeField] protected bool m_ExcludeCategory;
        [Tooltip("The item categories to check.")]
        [SerializeField] protected Category[] m_ItemCategories;
        
        /// <summary>
        /// Internal, Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected override bool CanImpactInternal(ImpactCallbackContext ctx)
        {
            var characterItem = ctx.CharacterItemAction?.CharacterItem;

            if (characterItem == null) {
                return m_AllowNonWeaponImpact;
            }

            for (int i = 0; i < m_ItemCategories.Length; i++) {
                if (m_ItemCategories[i].InherentlyContains(characterItem.ItemIdentifier) == true) {
                    return !m_ExcludeCategory;
                }
            }

            return m_ExcludeCategory;
        }
    }
    
    /// <summary>
    /// Checks the ObjectIdentifier on the target gameobject.
    /// </summary>
    [Serializable]
    public class CheckWeaponSourceDefinition : ImpactActionCondition
    {
        [Tooltip("Allow impact if the source does not have a character item action?")]
        [SerializeField] protected bool m_AllowNonWeaponImpact;
        [Tooltip("if false, then the impact will pass if the weapon inherits the definition, if true it will fail.")]
        [SerializeField] protected bool m_ExcludeCategory;
        [Tooltip("The item definitions to check.")]
        [SerializeField] protected ItemDefinitionBase[] m_ItemDefinitions;
        
        /// <summary>
        /// Internal, Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected override bool CanImpactInternal(ImpactCallbackContext ctx)
        {
            var characterItem = ctx.CharacterItemAction?.CharacterItem;

            if (characterItem == null) {
                return m_AllowNonWeaponImpact;
            }

            for (int i = 0; i < m_ItemDefinitions.Length; i++) {
                if (m_ItemDefinitions[i].InherentlyContains(characterItem.ItemIdentifier) == true) {
                    return !m_ExcludeCategory;
                }
            }

            return m_ExcludeCategory;
        }
    }

    /// <summary>
    /// Checks the projectile on the source gameobject.
    /// </summary>
    [Serializable]
    public class ProjectileImpactActionCondition : ImpactActionCondition
    {
        [Tooltip("Allow impact if the source does not have a character item action?")]
        [SerializeField] protected bool m_AllowNonProjectileImpact;
        [Tooltip("Allow impact if the source does not have a character item action?")]
        [SerializeField] protected bool m_AllowProjectileImpact;
        
        /// <summary>
        /// Internal, Can the impact proceed from the context.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        /// <returns>True if the impact should proceed.</returns>
        protected override bool CanImpactInternal(ImpactCallbackContext ctx)
        {
            if (TryGetProjectile(ctx, out var projectile)) {

                return CanProjectileImpact(ctx, projectile);
            }

            return m_AllowNonProjectileImpact;
        }
        
        /// <summary>
        /// Can the projectile impact?
        /// </summary>
        /// <param name="ctx">The impact context.</param>
        /// <param name="projectile">The projectile.</param>
        /// <returns>True if the projectile can impact</returns>
        protected virtual bool CanProjectileImpact(ImpactCallbackContext ctx, Projectile projectile)
        {
            return m_AllowProjectileImpact;
        }

        /// <summary>
        /// Try to get the project from the context if it exists.
        /// </summary>
        /// <param name="ctx">The impact context.</param>
        /// <param name="projectile">The projectile that was found.</param>
        /// <returns>True if a projectile is found.</returns>
        protected virtual bool TryGetProjectile(ImpactCallbackContext ctx, out Projectile projectile)
        {
            projectile =  ctx.ImpactCollisionData?.SourceGameObject?.GetCachedComponent<Projectile>();

            return projectile != null;
        }
    }
}