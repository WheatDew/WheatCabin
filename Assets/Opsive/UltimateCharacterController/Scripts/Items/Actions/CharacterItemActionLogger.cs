/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Types of logging.
    /// </summary>
    public enum CharacterItemActionDebugModuleType
    {
        NoLogs,     // Do not log anything in the console.
        AllEnabled, // Only log messages from enabled modules in the console.
        All,        // Log message of all modules in the console including disabled modules.
    }

    /// <summary>
    /// A class used to debug Item actions. Remove the 'ULTIMATE_CHARACTER_CONTROLLER_DEBUG' definition to stop logging and boost performance.
    /// </summary>
    [Serializable]
    public class CharacterItemActionLogger
    {
        public Action<string> OnInfoChange;
        
        [Tooltip("Debug the item action.")]
        [SerializeField] protected bool m_DebugAction;
        [Tooltip("The option on what information should be logged in the console.")]
        [SerializeField] private CharacterItemActionDebugModuleType m_CharacterItemActionDebugModuleType;

        protected Dictionary<string, string> m_InfoMessages;

        public bool IsDebugging => m_DebugAction || InspectorActive;
        public bool IsNotDebugging => !IsDebugging;
        public IReadOnlyDictionary<string, string> InfoDictionary => m_InfoMessages;
        public bool InspectorActive { get; set; }

        protected CharacterItemAction m_CharacterItemAction;

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="characterItemAction">The character item action.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public void Initialize(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            m_InfoMessages = new Dictionary<string, string>();
        }

        /// <summary>
        /// Can the module log information in the console.
        /// </summary>
        /// <param name="module">The module to check if it can log messages.</param>
        /// <returns>True of this module can log messages in the console.</returns>
        public virtual bool CanLog(ActionModule module)
        {
            if(IsNotDebugging){ return false; }
            
            if (m_CharacterItemActionDebugModuleType == CharacterItemActionDebugModuleType.NoLogs) { return false; }

            if (module != null && m_CharacterItemActionDebugModuleType == CharacterItemActionDebugModuleType.AllEnabled &&
                module.Enabled == false) { return false; }

            return true;
        }
        
        /// <summary>
        /// Log a message in the console.
        /// </summary>
        /// <param name="message">The message to log in the console.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void Log(string message)
        {
            if(IsNotDebugging){ return; }
            
            if (m_DebugAction == false) { return; }
            
            if (CanLog(null) == false) { return; }

            Debug.Log($"[{Time.frameCount}][{m_CharacterItemAction.name}] : {message}", m_CharacterItemAction);
        }

        /// <summary>
        /// Log a module message to the console.
        /// </summary>
        /// <param name="module">The module that wrote the message.</param>
        /// <param name="message">The message to write in the console.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void Log(ActionModule module, string message)
        {
            if(IsNotDebugging){ return; }
            
            if (CanLog(module) == false) { return; }

            Debug.Log($"[{Time.frameCount}][{m_CharacterItemAction.name} - {module.GetType()}] : {message}", m_CharacterItemAction);
        }
        
        /// <summary>
        /// Draw a ray.
        /// </summary>
        /// <param name="module">The module that draws the ray.</param>
        /// <param name="point">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void DrawRay(ActionModule module,Vector3 point, Vector3 direction)
        {
            if(IsNotDebugging){ return; }
            
            //if (CanLog(module) == false) { return; }

            Debug.DrawRay(point, direction);
        }

        /// <summary>
        /// Draw a ray.
        /// </summary>
        /// <param name="module">The module that draws the ray.</param>
        /// <param name="point">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="color">The color of the ray.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void DrawRay(ActionModule module, Vector3 point, Vector3 direction, Color color)
        {
            if(IsNotDebugging){ return; }
            
            //if (CanLog(module) == false) { return; }

            Debug.DrawRay(point, direction, color);
        }

        /// <summary>
        /// Draw a ray.
        /// </summary>
        /// <param name="module">The module that draws the ray.</param>
        /// <param name="point">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="color">The color of the ray.</param>
        /// <param name="duration">The duration of the ray.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void DrawRay(ActionModule module, Vector3 point, Vector3 direction, Color color, float duration)
        {
            if(IsNotDebugging){ return; }
            
            //if (CanLog(module) == false) { return; }

            Debug.DrawRay(point, direction, color, duration);
        }

        /// <summary>
        /// Draw a ray.
        /// </summary>
        /// <param name="module">The module that draws the ray.</param>
        /// <param name="point">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="color">The color of the ray.</param>
        /// <param name="duration">The duration of the ray.</param>
        /// <param name="depthTest">The depth test.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public virtual void DrawRay(ActionModule module, Vector3 point, Vector3 direction, Color color, float duration, bool depthTest)
        {
            if(IsNotDebugging){ return; }
            
            //if (CanLog(module) == false) { return; }

            Debug.DrawRay(point, direction, color, duration, depthTest);
        }

        /// <summary>
        /// Set an info to display in the inspector.
        /// </summary>
        /// <param name="infoKey">The info key.</param>
        /// <param name="message">The message for that info key.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public void SetInfo(string infoKey, string message)
        {
            if(IsNotDebugging){ return; }
            
            if (m_InfoMessages.ContainsKey(infoKey)) {
                if (m_InfoMessages[infoKey] == message) {
                    return;
                }
            }
            
            m_InfoMessages[infoKey] = message;
            OnInfoChange?.Invoke(infoKey);
        }
        
        /// <summary>
        /// Add an message to an info key (it concatenates the message).
        /// </summary>
        /// <param name="infoKey">The info key.</param>
        /// <param name="message">The message to add to the info.</param>
        [Conditional("DEBUG"), Conditional("ULTIMATE_CHARACTER_CONTROLLER_DEBUG")]
        public void AddToInfo(string infoKey, string message)
        {
            if(IsNotDebugging){ return; }

            string newMessage = message;
            
            if (m_InfoMessages.ContainsKey(infoKey)) {

                newMessage = m_InfoMessages[infoKey] + "\n" + message;
                
                if (m_InfoMessages[infoKey] == newMessage) {
                    return;
                }
            }
            
            m_InfoMessages[infoKey] = newMessage;
            OnInfoChange?.Invoke(infoKey);
        }
    }
}