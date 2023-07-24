/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Specifies a magic object (particle, generic GameObject) that can be spawned over the network.
    /// </summary>
    public interface IMagicObjectAction
    {
        /// <summary>
        /// The GameObject that was spawned.
        /// </summary>
        GameObject SpawnedGameObject { set; }

        /// <summary>
        /// The ID of the cast.
        /// </summary>
        uint CastID { set; }
    }

    /// <summary>
    /// A Magic Cast Effect Module.
    /// </summary>
    [Serializable]
    public abstract class MagicCastEffectModule : MagicActionModule, IModuleOnChangePerspectives
    {
        /// <summary>
        /// Specifies the current status of the effect.
        /// </summary>
        public enum EffectState
        {
            None,       // The effect has not started.
            Pending,    // The effect will start.
            Processing, // The effect has started.
            Complete    // The effect has completed.
        }
        
        [Tooltip("The delay to start the cast after the item has been used (-1 means no repeats, 0 is continuous loop).")]
        [SerializeField] protected float m_Delay;
        [Tooltip("The delay to repeat the cast when it is already complete (-1 means same as delay).")]
        [SerializeField] protected float m_InitialDelay = -1;

        public float Delay { get { return m_Delay; } set { m_Delay = value; } }
        public float InitialDelay { get { return m_InitialDelay; } set { m_InitialDelay = value; } }

        protected float m_LastCastTime;
        protected float m_LastCompletedTime;
        protected uint m_CastID;
        protected int m_CastCount;
        protected EffectState m_CurrentState;

        [Shared.Utility.NonSerialized] public uint CastID { get => m_CastID; set => m_CastID = value; }

        public int CastCount => m_CastCount;
        public EffectState CurrentState => m_CurrentState;

        /// <summary>
        /// Returns true if the cast has completed.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>True if the cast is complete.</returns>
        public virtual bool IsCastComplete(MagicUseDataStream useDataStream)
        {
            return m_CurrentState == EffectState.Complete;
        }

        /// <summary>
        /// Is the specified position a valid target position?
        /// </summary>
        /// <param name="useDataStream">The use data stream has the cast data.</param>
        /// <param name="position">The position that may be a valid target position.</param>
        /// <param name="normal">The normal of the position.</param>
        /// <returns>True if the specified position is a valid target position.</returns>
        public virtual bool IsValidTargetPosition(MagicUseDataStream useDataStream, Vector3 position, Vector3 normal)
        {
            return true;
        }

        /// <summary>
        /// Start casting.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public virtual void StartCast(MagicUseDataStream useDataStream)
        {
            m_CastID = useDataStream.CastData.CastID;
            m_CastCount = 0;
            m_LastCastTime = -1;
            m_LastCompletedTime = -1;
            m_CurrentState = EffectState.Pending;
        }
        
        /// <summary>
        /// The the cast pending to be used due to some delay or other condition?
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public virtual bool IsInitialPendingDoCast(MagicUseDataStream useDataStream)
        {
            // The cast is not pending if it hasn't started.
            if (m_CurrentState != EffectState.Pending) { return false;}

            // The cast can be prevented/interupted, causing it to no longer being pending. 
            var canCast = CanDoCast(useDataStream);
            if (canCast == false) { return false; }

            var delay = m_InitialDelay < 0 ? m_Delay : m_InitialDelay;
            if (delay <= 0) {
                return false;
            }
            
            // The delay can make the cast pending.
            if (useDataStream.CastData.StartCastTime + delay > Time.time) {
                return true;
            }

            // It is no longer pending
            return false;
        }
        
        /// <summary>
        /// The the cast pending to be used due to some delay or other condition?
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public virtual bool IsRepeatPendingDoCast(MagicUseDataStream useDataStream)
        {
            // The delay can make the cast pending.
            if (m_LastCastTime + m_Delay  > Time.time 
                || m_LastCompletedTime + m_Delay > Time.time) {
                return true;
            }

            // It is no longer pending
            return false;
        }

        /// <summary>
        /// Can the action be casted?
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>True if the action can be casted.</returns>
        public virtual bool CanDoCast(MagicUseDataStream useDataStream)
        {
            // Do not cast if the cast effect hasn't started.
            if (m_CurrentState == EffectState.None) { return false; }

            // If complete and there is no repeat delay, then it cannot be casted any longer.
            var isComplete = IsCastComplete(useDataStream);
            if (isComplete && m_Delay < 0) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return true if the effect was cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>Return true if the effect was cast.</returns>
        public virtual void OnCastUpdate(MagicUseDataStream useDataStream)
        {
            TryCast(useDataStream);
        }

        /// <summary>
        /// Try casting the effect if the conditions allow it.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public virtual void TryCast(MagicUseDataStream useDataStream)
        {
            if (!CanDoCast(useDataStream)) { return; }
            
            // The initial pending for the first cast.
            if (IsInitialPendingDoCast(useDataStream)) { return; }
            
            // The delay can make the cast pending.
            if (IsRepeatPendingDoCast(useDataStream)) { return; }

            DoCast(useDataStream);
        }

        /// <summary>
        /// Do the cast and cast complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public virtual void DoCast(MagicUseDataStream useDataStream)
        {
            DoCastInternal(useDataStream);
            m_CastCount++;
            m_LastCastTime = Time.time;
            m_CurrentState = EffectState.Processing;
            CastComplete(useDataStream);
        }

        /// <summary>
        /// Do the casting.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        protected virtual void DoCastInternal(MagicUseDataStream useDataStream)
        {
           // To be overriden.
        }

        /// <summary>
        /// The cast was complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        protected void CastComplete(MagicUseDataStream useDataStream)
        {
            m_CurrentState = EffectState.Complete;
            m_LastCompletedTime = Time.time;
            CastCompleteInternal(useDataStream);
        }
        
        /// <summary>
        /// The cast was complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public abstract void CastCompleteInternal(MagicUseDataStream useDataStream);

        /// <summary>
        /// The cast will be stopped. Start any cleanup.
        /// </summary>
        public virtual void CastWillStop() { }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public virtual void StopCast()
        {
            m_CastCount = 0;
            m_CastID = 0;
            m_LastCastTime = -1;
            m_LastCompletedTime = -1;
            m_CurrentState = EffectState.None;
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public virtual void OnChangePerspectives(bool firstPersonPerspective) { }
    }

    /// <summary>
    /// A cast effect that can target multiple targets at once.
    /// </summary>
    [Serializable]
    public abstract class MagicMultiTargetCastEffectModule : MagicCastEffectModule
    {
        /// <summary>
        /// The data stored for each cast associated to a target.
        /// </summary>
        public struct TargetCache
        {
            private bool m_CastComplete;
            private float m_LastCastTime;
            private float m_LastCastCompleteTime;
            private int m_CastCount;
            
            public bool CastComplete => m_CastComplete;
            public float LastCastTime => m_LastCastTime;
            public float LastCastCompleteTime => m_LastCastCompleteTime;
            public int CastCount => m_CastCount;
            public static TargetCache None => new TargetCache(false, -1, -1, 0);

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="castComplete">Is the cast complete?</param>
            /// <param name="lastCastTime">The last time a spell was Casted.</param>
            /// <param name="lastCastCompleteTime">The last time a spell has completed.</param>
            /// <param name="castCount">The number of casts.</param>
            public TargetCache(bool castComplete, float lastCastTime, float lastCastCompleteTime, int castCount)
            {
                m_CastComplete = castComplete;
                m_LastCastTime = lastCastTime;
                m_LastCastCompleteTime = lastCastCompleteTime;
                m_CastCount = castCount;
            }
            
            /// <summary>
            /// Copy Constructor. 
            /// </summary>
            /// <param name="lastCastTime">The last time a spell was Casted.</param>
            /// <param name="other">The other target cache to copy.</param>
            /// <returns>The new target cache.</returns>
            public static TargetCache Cast(float lastCastTime, TargetCache other)
            {
                return new TargetCache(
                    other.CastComplete,
                    lastCastTime,
                    other.LastCastCompleteTime,
                    other.CastCount + 1
                );
            }
            
            /// <summary>
            /// Copy Constructor. 
            /// </summary>
            /// <param name="lastCastCompleteTime">The last time a spell has completed.</param>
            /// <param name="other">The other target cache to copy.</param>
            /// <returns>The new target cache.</returns>
            public static TargetCache Complete(float lastCastCompleteTime, TargetCache other)
            {
                return new TargetCache(
                    true,
                    other.LastCastTime,
                    lastCastCompleteTime,
                    other.CastCount
                );
            }
        }
        
        [Tooltip("Allowing multiple targets will allow the casting to be used multiple times until the action has been casted on all targets.")]
        [SerializeField] protected bool m_AllowMultiTarget = true;

        public bool AllowMultiTarget { get => m_AllowMultiTarget; set => m_AllowMultiTarget = value; }

        protected List<TargetCache> m_TargetCaches;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_TargetCaches = new List<TargetCache>();
            m_TargetCaches.Add(TargetCache.None);
        }

        /// <summary>
        /// Get a list slice for the target data caches.
        /// </summary>
        /// <param name="useDataStream">The use data stream defines the number of targets.</param>
        /// <returns>A list slice of target caches.</returns>
        public ListSlice<TargetCache> GetTargetCaches(MagicUseDataStream useDataStream)
        {
            var targetCount =  Mathf.Max(1, m_AllowMultiTarget ? useDataStream.CastData.Targets.Count : 1);
            m_TargetCaches.EnsureSize(targetCount);

            return (m_TargetCaches, 0, targetCount);
        }

        /// <summary>
        /// Start casting.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void StartCast(MagicUseDataStream useDataStream)
        {
            base.StartCast(useDataStream);

            var targetCaches = GetTargetCaches(useDataStream);

            // Set all target completes to false.
            for (int i = 0; i < targetCaches.Count; i++) {
                m_TargetCaches[i] = TargetCache.None;
            }
        }

        /// <summary>
        /// The the cast pending to be used due to some delay or other condition?
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override bool IsRepeatPendingDoCast(MagicUseDataStream useDataStream)
        {
            var targetIndex = m_AllowMultiTarget ? useDataStream.CastData.TargetIndex : 0;
            GetTargetCaches(useDataStream);

            var lastCastTime = m_TargetCaches[targetIndex].LastCastTime;
            var lastCastCompleteTime = m_TargetCaches[targetIndex].LastCastCompleteTime;
            
            // The delay can make the cast pending.
            if (lastCastTime + m_Delay  > Time.time 
                || lastCastCompleteTime + m_Delay > Time.time) {
                return true;
            }

            // It is no longer pending
            return false;
        }
        
        /// <summary>
        /// Do the cast and cast complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void DoCast(MagicUseDataStream useDataStream)
        {
            var targetIndex = m_AllowMultiTarget ? useDataStream.CastData.TargetIndex : 0;
            GetTargetCaches(useDataStream);

            if (targetIndex <= -1) {
                //Check all when target index is -1
                for (int i = 0; i < m_TargetCaches.Count; i++) {
                    m_TargetCaches[i] = TargetCache.Cast(Time.time, m_TargetCaches[i]);
                }
            } else {
                m_TargetCaches[targetIndex] =  TargetCache.Cast(Time.time, m_TargetCaches[targetIndex]);
            }
            
            base.DoCast(useDataStream);
        }

        /// <summary>
        /// Returns true if the cast has completed.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>True if the cast is complete.</returns>
        public override bool IsCastComplete(MagicUseDataStream useDataStream)
        {
            var targetIndex = m_AllowMultiTarget ? useDataStream.CastData.TargetIndex : 0;
            var targetCaches = GetTargetCaches(useDataStream);

            if (targetIndex == -1) {

                //Check all when target index is -1
                for (int i = 0; i < targetCaches.Count; i++) {
                    if (m_TargetCaches[i].CastComplete == false) {
                        return false;
                    }
                }

                return true;
            }

            if (targetIndex >= targetCaches.Count) {
                Debug.LogWarning("Target Index out of range");
                return true;
            }
            
            return m_TargetCaches[targetIndex].CastComplete;
        }

        /// <summary>
        /// The cast was complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void CastCompleteInternal(MagicUseDataStream useDataStream)
        {
            var targetIndex = m_AllowMultiTarget ? useDataStream.CastData.TargetIndex: 0;
            GetTargetCaches(useDataStream);

            if (targetIndex <= -1) {
                //Check all when target index is -1
                for (int i = 0; i < m_TargetCaches.Count; i++) {
                    m_TargetCaches[i] = TargetCache.Complete(Time.time, m_TargetCaches[i]);
                }
            } else {
                m_TargetCaches[targetIndex] = TargetCache.Complete(Time.time, m_TargetCaches[targetIndex]);
            }
        }
    }

    /// <summary>
    /// A module which allows Magic CastEffects to be nested and invoked in sequence or in paralell. 
    /// </summary>
    [Serializable]
    public class MagicCastEffectNester : MagicCastEffectModule
    {
        [Tooltip("Invoke the modules in sequence or in parallel?")]
        [SerializeField] protected bool m_Sequential;
        [Tooltip("The number of times the nested effects will be repeated before it is considered complete.")]
        [SerializeField] protected int m_RepeatCount = 0;
        [ActionModuleGroup(Actions.MagicAction.c_CastEffectIconGuid)]
        [Tooltip("The nested cast effect modules.")]
        [SerializeField] protected ActionModuleGroup<MagicCastEffectModule> m_Modules = new ActionModuleGroup<MagicCastEffectModule>();
        
        public bool Sequential { get => m_Sequential; set => m_Sequential = value; }
        public ActionModuleGroup<MagicCastEffectModule> Modules { get => m_Modules; set => m_Modules = value; }

        protected int m_SequentialIndex = 0;
        protected int m_RepeatCounter;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_Modules.Initialize(itemAction);
        }

        /// <summary>
        /// Start casting.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void StartCast(MagicUseDataStream useDataStream)
        {
            base.StartCast(useDataStream);

            m_SequentialIndex = 0;
            m_RepeatCounter = 0;
        }

        /// <summary>
        /// Return true if the effect was cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>Return true if the effect was cast.</returns>
        public override void OnCastUpdate(MagicUseDataStream useDataStream)
        {
            base.OnCastUpdate(useDataStream);

            // Not processing yet, return
            if (m_CurrentState != EffectState.Processing) {
                return;
            }
            
            var allModules = m_Modules.Modules;
            if (m_Sequential) {
                
                // Sequential
                for (int i = m_SequentialIndex; i < allModules.Count; i++) {
                    // Ignore if not enabled. But keep the check in case states are used to change which modules are active.
                    var module = allModules[i];
                    if(module.Enabled == false){ continue; }

                    if (module.CurrentState == EffectState.None) {
                        // Start the module if it hasn't been started yet.
                        module.StartCast(useDataStream);
                        module.TryCast(useDataStream);
                    }

                    if (module.IsCastComplete(useDataStream)) {
                        // When complete, go to the next index
                        m_SequentialIndex++;
                        continue;
                    } 
                    
                    // It is pending or processing, that means it started already.
                    break;
                }
                
            } else {
                // Parallel
                for (int i = m_SequentialIndex; i < allModules.Count; i++) {
                    var module = allModules[i];
                    // Ignore if not enabled. But keep the check in case states are used to change which modules are active.
                    if(module.Enabled == false){ continue; }
                    if (module.CurrentState != EffectState.None) { continue;}
                    module.StartCast(useDataStream);
                    module.TryCast(useDataStream);
                }
            }
            
            // Update the cast effects.
            var enabledModules = m_Modules.EnabledModules;
            for (int i = 0; i < enabledModules.Count; i++) {
                var module = enabledModules[i];
                module.OnCastUpdate(useDataStream);
            }

            // If all modules are complete set as complete.
            var allComplete = true;
            for (int i = 0; i < enabledModules.Count; i++) {
                if (enabledModules[i].IsCastComplete(useDataStream) == false) {
                    allComplete = false;
                }
            }
            if (allComplete) {
                if (m_RepeatCounter >= m_RepeatCount) {
                    CastComplete(useDataStream);
                } else {
                    DoRepeat();
                }
            }
        }

        /// <summary>
        /// Try casting a module.
        /// </summary>
        /// <param name="useDataStream">The use data stream has the cast data.</param>
        /// <param name="module">The module to cast.</param>
        public virtual void TryCast(MagicUseDataStream useDataStream, MagicCastEffectModule module)
        {
            module.TryCast(useDataStream);
        }

        /// <summary>
        /// Cast all the nested modules again.
        /// </summary>
        private void DoRepeat()
        {
            m_RepeatCounter++;
            m_SequentialIndex = 0;

            var enabledModules = m_Modules.EnabledModules;
            // Stopping the cast of the nested modules will allow them to Repeat.
            for (int i = 0; i < enabledModules.Count; i++) {
                enabledModules[i].CastWillStop();
                enabledModules[i].StopCast();
            }
        }

        /// <summary>
        /// Is the specified position a valid target position?
        /// </summary>
        /// <param name="useDataStream">The use data stream has the cast data.</param>
        /// <param name="position">The position that may be a valid target position.</param>
        /// <param name="normal">The normal of the position.</param>
        /// <returns>True if the specified position is a valid target position.</returns>
        public override bool IsValidTargetPosition(MagicUseDataStream useDataStream, Vector3 position, Vector3 normal)
        {
            var valid = base.IsValidTargetPosition(useDataStream, position, normal);
            if (valid == false) { return false; }
            
            var enabledModules = m_Modules.EnabledModules;
            // Stopping the cast of the nested modules will allow them to Repeat.
            for (int i = 0; i < enabledModules.Count; i++) {
                if (enabledModules[i].IsValidTargetPosition(useDataStream, position, normal) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Do the cast and cast complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void DoCast(MagicUseDataStream useDataStream)
        {
            DoCastInternal(useDataStream);
            m_CastCount++;
            m_CurrentState = EffectState.Processing;
            
            //Don't cast complete yet.
        }

        /// <summary>
        /// Returns true if the cast has completed.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>True if the cast is complete.</returns>
        public override bool IsCastComplete(MagicUseDataStream useDataStream)
        {
            return CurrentState == EffectState.Complete;
        }

        /// <summary>
        /// The cast was complete.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        public override void CastCompleteInternal(MagicUseDataStream useDataStream)
        {
            // Do nothing.
        }

        /// <summary>
        /// The cast will be stopped. Start any cleanup.
        /// </summary>
        public override void CastWillStop()
        {
            base.CastWillStop();
            var enabledModules = m_Modules.EnabledModules;
            // Stopping the cast of the nested modules will allow them to Repeat.
            for (int i = 0; i < enabledModules.Count; i++) { enabledModules[i].CastWillStop(); }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            base.StopCast();
            var enabledModules = m_Modules.EnabledModules;
            // Stopping the cast of the nested modules will allow them to Repeat.
            for (int i = 0; i < enabledModules.Count; i++) { enabledModules[i].StopCast(); }
        }
    }

    /// <summary>
    /// Invoke some item effects during a cast.
    /// </summary>
    [Serializable]
    public class CastItemEffects : MagicMultiTargetCastEffectModule
    {
        [Tooltip("The item effects to invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;
        [Tooltip("Prevent casting unless all the effects can be invoked.")]
        [SerializeField] protected bool m_BlockUntilEffectsCanBeUsed;
        
        public ItemEffectGroup EffectGroup { get => m_EffectGroup; set => m_EffectGroup = value; }
        public bool BlockUntilEffectsCanBeUsed { get => m_BlockUntilEffectsCanBeUsed; set => m_BlockUntilEffectsCanBeUsed = value; }

        /// <summary>
        /// Can the action be casted?
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        /// <returns>True if the action can be casted.</returns>
        public override bool CanDoCast(MagicUseDataStream useDataStream)
        {
            var result = base.CanDoCast(useDataStream);
            if (result == false) {
                return false;
            }

            if (m_BlockUntilEffectsCanBeUsed) {
                return m_EffectGroup.CanInvokeEffects();
            }

            return true;
        }

        /// <summary>
        /// Do the casting.
        /// </summary>
        /// <param name="useDataStream">The use data stream.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            base.DoCastInternal(useDataStream);
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EffectGroup.OnDestroy();
        }
    }
}