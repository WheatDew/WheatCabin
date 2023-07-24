/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic
{
    using System;

    /// <summary>
    /// The base module for magic actions that can start and stop.
    /// </summary>
    [Serializable]
    public abstract class MagicStartStopModule : MagicActionModule, IModuleOnChangePerspectives
    {
        protected bool m_BeginAction;

        /// <summary>
        /// Initialize to check if this is a begin or end action.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            m_BeginAction = MagicAction.BeginModuleGroup == m_ModuleGroup;
        }

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public virtual void Start(MagicUseDataStream useDataStream) { }

        /// <summary>
        /// Updates the action.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public virtual void Update(MagicUseDataStream useDataStream) { }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public virtual void Stop(MagicUseDataStream useDataStream) { }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public virtual void OnChangePerspectives(bool firstPersonPerspective) { }
    }
}