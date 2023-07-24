/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.StateSystem
{
    using Opsive.Shared.Game;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Handles the activation and deactivation of states.
    /// </summary>
    public class StateManager : MonoBehaviour
    {
        [Tooltip("Should the global OnStateChange event be sent when the state changes active status?")]
        [SerializeField] protected bool m_SendStateChangeEvent;

        public bool SendStateChangeEvent { get { return m_SendStateChangeEvent; } set { m_SendStateChangeEvent = value; } }

        private static StateManager s_Instance;
        private static StateManager Instance {
            get {
                if (!s_Initialized) {
                    s_Instance = new GameObject("StateManager").AddComponent<StateManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        private Dictionary<object, Dictionary<string, State>> m_ObjectNameStateMap = new Dictionary<object, Dictionary<string, State>>();
        private Dictionary<GameObject, Dictionary<string, List<State>>> m_GameObjectNameStateList = new Dictionary<GameObject, Dictionary<string, List<State>>>();
        private Dictionary<GameObject, List<GameObject>> m_LinkedGameObjectList = new Dictionary<GameObject, List<GameObject>>();
        private Dictionary<State, State[]> m_StateArrayMap = new Dictionary<State, State[]>();
        private Dictionary<GameObject, HashSet<string>> m_ActiveCharacterStates = new Dictionary<GameObject, HashSet<string>>();
        private Dictionary<GameObject, Dictionary<string, ScheduledEventBase>> m_DisableStateTimerMap;

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Initializes the states belonging to the owner on the GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        public static void Initialize(GameObject gameObject, IStateOwner owner, State[] states)
        {
            Instance.InitializeInternal(gameObject, owner, states);
        }

        /// <summary>
        /// Internal method which initializes the states belonging to the owner on the GameObject.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        private void InitializeInternal(GameObject stateGameObject, IStateOwner owner, State[] states)
        {
            // The last state will always be reserved for the default state.
            if (states[states.Length - 1] == null) {
                states[states.Length - 1] = new State("Default", true);
            }
            states[states.Length - 1].Preset = DefaultPreset.CreateDefaultPreset();

            if (!m_ObjectNameStateMap.TryGetValue(owner, out var nameStateMap)) {
                nameStateMap = new Dictionary<string, State>();
                m_ObjectNameStateMap.Add(owner, nameStateMap);
            }

            // Populate the maps for quick lookup based on owner and GameObject.
            GameObject characterGameObject = null;
            var characterLocomotion = stateGameObject.GetCachedParentComponent<Character.ICharacter>();
            if (characterLocomotion != null) {
                characterGameObject = characterLocomotion.GameObject;
            }
            if (characterGameObject == null) { // The state may be beneath the camera.
                var camera = stateGameObject.GetCachedParentComponent<Shared.Camera.ICamera>();
                if (camera != null) {
                    characterGameObject = camera.Character;
                }
            }
            for (int i = 0; i < states.Length; ++i) {
                if (states[i].Preset == null) {
                    Debug.LogError(string.Format("Error: The state {0} on {1} does not have a preset. Ensure each non-default state contains a preset.", states[i].Name, owner), owner as Object);
                }
                nameStateMap.Add(states[i].Name, states[i]);

                if (!m_GameObjectNameStateList.TryGetValue(stateGameObject, out var nameStateList)) {
                    nameStateList = new Dictionary<string, List<State>>();
                    m_GameObjectNameStateList.Add(stateGameObject, nameStateList);
                }

                // Child GameObjects should listen for states set on the parent. This for example allows an item to react to a state change even if that state change
                // is set on the character. The character GameObject does not need to be made aware of the Default state.
                if (i != states.Length - 1) {
                    if (characterGameObject != null && stateGameObject != characterGameObject) {
                        if (!m_GameObjectNameStateList.TryGetValue(characterGameObject, out var characterNameStateList)) {
                            characterNameStateList = new Dictionary<string, List<State>>();
                            m_GameObjectNameStateList.Add(characterGameObject, characterNameStateList);
                        }

                        if (!characterNameStateList.TryGetValue(states[i].Name, out var characterStateList)) {
                            characterStateList = new List<State>();
                            characterNameStateList.Add(states[i].Name, characterStateList);
                        }

                        characterStateList.Add(states[i]);
                    }
                }

                if (!nameStateList.TryGetValue(states[i].Name, out var stateList)) {
                    stateList = new List<State>();
                    nameStateList.Add(states[i].Name, stateList);
                }

                stateList.Add(states[i]);
                m_StateArrayMap.Add(states[i], states);
            }

            // Initialize the state after the map has been created.
            for (int i = 0; i < states.Length; ++i) {
                states[i].Initialize(owner, nameStateMap);
            }

            // The default state is always last.
            states[states.Length - 1].Active = true;

            // Remember the active character states so if a GameObject is initialized after a state has already been activated that newly initialized GameObject
            // can start the correct states. As an example an item could be picked up after the character is already aiming. That item should go directly
            // into the aim state instead of requiring the character to aim again.
            if (characterGameObject != null) {
                if (characterGameObject == stateGameObject) {
                    // If the current GameObject is the character then the active states should be tracked.
                    if (!m_ActiveCharacterStates.ContainsKey(stateGameObject)) {
                        m_ActiveCharacterStates.Add(stateGameObject, new HashSet<string>());
                    }
                } else {
                    // If the current GameObject is not the character then the active character states should be applied to the child object.
                    if (m_ActiveCharacterStates.TryGetValue(characterGameObject, out var activeStates)) {
                        if (activeStates.Count > 0) {
                            foreach (var stateName in activeStates) {
                                SetState(stateGameObject, stateName, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Links the original GameObject to the linked GameObject. When GameObjects are linked the state will be updated for each GameObject even when only the
        /// original GameObject is set.
        /// </summary>
        /// <param name="original">The original GameObject to link.</param>
        /// <param name="linkedGameObject">The GameObject that should be linked to the original GameObject.</param>
        /// <param name="link">Should the GameObjects be linked. If fales the GameObjects will be unlinked.</param>
        public static void LinkGameObjects(GameObject original, GameObject linkedGameObject, bool link)
        {
            Instance.LinkGameObjectsInternal(original, linkedGameObject, link);
        }

        /// <summary>
        /// Internal method which links the original GameObject to the linked GameObject. When GameObjects are linked the state will be updated for each 
        /// GameObject even when only the original GameObject is set.
        /// </summary>
        /// <param name="original">The original GameObject to link.</param>
        /// <param name="linkedGameObject">The GameObject that should be linked to the original GameObject.</param>
        /// <param name="link">Should the GameObjects be linked. If fales the GameObjects will be unlinked.</param>
        private void LinkGameObjectsInternal(GameObject original, GameObject linkedGameObject, bool link)
        {
            if (original == linkedGameObject) {
                return;
            }

            if (!m_LinkedGameObjectList.TryGetValue(original, out var linkedGameObjectList) && link) {
                linkedGameObjectList = new List<GameObject>();
                m_LinkedGameObjectList.Add(original, linkedGameObjectList);
            }

            if (linkedGameObjectList != null) {
                if (link) {
                    linkedGameObjectList.Add(linkedGameObject);

                    // If the current GameObject is not the character then the active character states should be applied to the child object.
                    if (m_ActiveCharacterStates.TryGetValue(original, out var activeStates)) {
                        if (activeStates.Count > 0) {
                            foreach (var stateName in activeStates) {
                                SetState(linkedGameObject, stateName, true);
                            }
                        }
                    }
                } else {
                    linkedGameObjectList.Remove(linkedGameObject);
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state.
        /// </summary>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        public static void SetState(object owner, State[] states, string stateName, bool active)
        {
            Instance.SetStateInternal(owner, states, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates the specified state.
        /// </summary>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetStateInternal(object owner, State[] states, string stateName, bool active)
        {
            // Lookup the state by owner.
            if (!m_ObjectNameStateMap.TryGetValue(owner, out var nameStateMap)) {
                Debug.LogWarning($"Warning: Unable to find the name state map on object {owner}.");
                return;
            }

            // Lookup the state by name.
            if (!nameStateMap.TryGetValue(stateName, out var state)) {
                Debug.LogWarning($"Warning: Unable to find the state with name {stateName}.");
                return;
            }

            // The state has been found, activate or deactivate the states.
            if (state.Active != active) {
                ActivateStateInternal(state, active, states);
            }
        }

        /// <summary>
        /// Activates or deactivates all of the states on the specified GameObject with the specified name.
        /// </summary>
        /// <param name="gameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        public static void SetState(GameObject gameObject, string stateName, bool active)
        {
            Instance.SetStateInternal(gameObject, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates all of the states on the specified GameObject with the specified name.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetStateInternal(GameObject stateGameObject, string stateName, bool active)
        {
            // Remember the active character status.
            var character = stateGameObject.GetCachedComponent<Character.ICharacter>();
            if (character != null) {
                if (m_ActiveCharacterStates.TryGetValue(stateGameObject, out var activeStates)) {
                    // If the state name appears within the set then the state is active.
                    if (active) {
                        activeStates.Add(stateName);
                    } else {
                        activeStates.Remove(stateName);
                    }
                }
            }

            // Lookup the states by GameObject.
            if (!m_GameObjectNameStateList.TryGetValue(stateGameObject, out var nameStateList)) {
                SetLinkStateInternal(stateGameObject, stateName, active);
                return;
            }

            // Lookup the states by name.
            if (!nameStateList.TryGetValue(stateName, out var stateList)) {
                SetLinkStateInternal(stateGameObject, stateName, active);
                return;
            }

            // An event can be sent when the active status changes. This is useful for multiplayer in that it allows the networking implementation
            // to send the state changes across the network.
            if (m_SendStateChangeEvent) {
                Shared.Events.EventHandler.ExecuteEvent("OnStateChange", stateGameObject, stateName, active);
            }

            // The states have been found, activate or deactivate the states.
            for (int i = 0; i < stateList.Count; ++i) {
                if (stateList[i].Active != active) {
                    // The state array must exist to be able to apply the changes.
                    if (!m_StateArrayMap.TryGetValue(stateList[i], out var states)) {
                        Debug.LogWarning($"Warning: Unable to find the state array with state name {stateName}.");
                        return;
                    }

                    // Notify the owner that the states will change.
                    stateList[i].Owner.StateWillChange();

                    ActivateStateInternal(stateList[i], active, states);

                    // Notify the owner that the state has changed.
                    stateList[i].Owner.StateChange();
                }
            }

            SetLinkStateInternal(stateGameObject, stateName, active);
        }

        /// <summary>
        /// Internal method which activates or deactivates all of the states on the GameObjects linked from the GameObject with the specified name.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to enable or disable all of the states on.</param>
        /// <param name="stateName">The name of the state to change the active status of.</param>
        /// <param name="active">Should the state be activated?</param>
        private void SetLinkStateInternal(GameObject stateGameObject, string stateName, bool active)
        {
            if (m_LinkedGameObjectList.TryGetValue(stateGameObject, out var linkedGameObjects)) {
                for (int i = 0; i < linkedGameObjects.Count; ++i) {
                    SetStateInternal(linkedGameObjects[i], stateName, active);
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the specified state. In most cases SetState should be used instead of ActivateState.
        /// </summary>
        /// <param name="state">The state to activate or deactivate.</param>
        /// <param name="active">Should the state be activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        public static void ActivateState(State state, bool active, State[] states)
        {
            Instance.ActivateStateInternal(state, active, states);
        }

        /// <summary>
        /// Internal method which activates or deactivates the specified state. In most cases SetState should be used instead of ActivateState.
        /// </summary>
        /// <param name="state">The state to activate or deactivate.</param>
        /// <param name="active">Should the state be activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        private void ActivateStateInternal(State state, bool active, State[] states)
        {
            if (!Application.isPlaying) {
                return;
            }

            // Return early if there no work needs to be done.
            if (state.Active == active) {
                return;
            }

            // Set the active state.
            state.Active = active;

            // Apply the changes.
            CombineStates(state, active, states);
        }

        /// <summary>
        /// Loops through the states and applies the value. The states are looped in the order specified within the inspector from top to bottom.
        /// </summary>
        /// <param name="state">The state that was activated or deactivated.</param>
        /// <param name="active">Was the activated?</param>
        /// <param name="states">The array of states that the state belongs to.</param>
        private void CombineStates(State state, bool active, State[] states)
        {
            if (active) {
                // Apply the default value of the blocked states before looping through all of the states. This will ensure the default value
                // is set for that property if no other states set the property value.
                for (int i = states.Length - 2; i > -1; --i) {
                    if (states[i].Active && states[i].IsBlocked()) {
                        states[states.Length - 1].ApplyValues(states[i].Preset.Delegates);
                    }
                }
            } else {
                // Restore the default values if the state is no longer active.
                states[states.Length - 1].ApplyValues(state.Preset.Delegates);
            }

            // Loop backwards so the higher priority states are applied first. Do not apply the default state because it was applied above.
            for (int i = states.Length - 2; i > -1; --i) {
                // Don't apply the state if the state isn't active.
                if (!states[i].Active) {
                    continue;
                }

                // Do not apply the state if it is currently blocked by another state.
                if (states[i].IsBlocked()) {
                    continue;
                }

                states[i].ApplyValues();
            }
        }

        /// <summary>
        /// Activates the state and then deactivates the state after the specified amount of time.
        /// </summary>
        /// <param name="gameObject">The Gameobject to set the state on.</param>
        /// <param name="stateName">The name of the state to activate and then deactivate.</param>
        /// <param name="time">The amount of time that should elapse before the state is disabled.</param>
        public static void DeactivateStateTimer(GameObject gameObject, string stateName, float time)
        {
            Instance.DeactivateStateTimerInternal(gameObject, stateName, time);
        }

        /// <summary>
        /// Internal method which activates the state and then deactivates the state after the specified amount of time.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to set the state on.</param>
        /// <param name="stateName">The name of the state to activate and then deactivate.</param>
        /// <param name="time">The amount of time that should elapse before the state is disabled.</param>
        private void DeactivateStateTimerInternal(GameObject stateGameObject, string stateName, float time)
        {
            if (m_DisableStateTimerMap == null) {
                m_DisableStateTimerMap = new Dictionary<GameObject, Dictionary<string, ScheduledEventBase>>();
            }

            if (m_DisableStateTimerMap.TryGetValue(stateGameObject, out var stateNameEventMap)) {
                if (stateNameEventMap.TryGetValue(stateName, out var disableEvent)) {
                    // The state name exists. This means that the timer is currently active and should first been cancelled.
                    Scheduler.Cancel(disableEvent);
                    disableEvent = Scheduler.Schedule(time, DeactivateState, stateGameObject, stateName);
                } else {
                    // The state name hasn't been added yet. Add it to the map.
                    disableEvent = Scheduler.Schedule(time, DeactivateState, stateGameObject, stateName);
                    stateNameEventMap.Add(stateName, disableEvent);
                }
            } else {
                // Neither the GameObject nor the state has been activated. Create the maps.
                stateNameEventMap = new Dictionary<string, ScheduledEventBase>();
                var disableEvent = Scheduler.Schedule(time, DeactivateState, stateGameObject, stateName);
                stateNameEventMap.Add(stateName, disableEvent);
                m_DisableStateTimerMap.Add(stateGameObject, stateNameEventMap);
            }
        }

        /// <summary>
        /// Deactives the specified state and removes it form the timer map.
        /// </summary>
        /// <param name="stateGameObject">The GameObject to set the state on.</param>
        /// <param name="stateName">The name of the state to set.</param>
        private void DeactivateState(GameObject stateGameObject, string stateName)
        {
            SetState(stateGameObject, stateName, false);

            if (m_DisableStateTimerMap.TryGetValue(stateGameObject, out var stateNameEventMap)) {
                stateNameEventMap.Remove(stateName);
            }
        }

        /// <summary>
        /// Adds a state to the state array.
        /// </summary>
        /// <param name="ownerGameObject">The GameObject that state belongs to.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="existingStates">A list of all of the states which the owner contains.</param>
        /// <param name="newState">The new state.</param>
        /// <param name="index">The index that the state should be added to.</param>
        public static void AddState(GameObject ownerGameObject, IStateOwner owner, State[] existingStates, State newState, int index)
        {
            Instance.AddStateInternal(ownerGameObject, owner, existingStates, newState, index);
        }

        /// <summary>
        /// Internal method which adds a state to the state array.
        /// </summary>
        /// <param name="ownerGameObject">The GameObject that state belongs to.</param>
        /// <param name="owner">The object that state belongs to.</param>
        /// <param name="states">A list of all of the states which the owner contains.</param>
        /// <param name="state">The new state.</param>
        /// <param name="index">The index that the state should be added to.</param>
        private void AddStateInternal(GameObject ownerGameObject, IStateOwner owner, State[] states, State state, int index)
        {
            // There are many reasons why a new state cannot be added.
            if (!Application.isPlaying || m_ObjectNameStateMap == null) {
                return;
            }
            if (states == null || state == null) {
                Debug.LogError("Error: Unable to add an uninitialized state.");
                return;
            }
            if (index < 0 || index >= states.Length) {
                Debug.LogError($"Error: Unable to add the state {state.Name} to index {index}. The index is out of range.");
                return;
            }
            if (state.Default) {
                Debug.LogError("Error: Unable to add a default state after initialization.");
                return;
            }
            if (string.IsNullOrEmpty(state.Name)) {
                Debug.LogError("Error: The state name cannot be empty.");
                return;
            }
            if (state.Preset == null) {
                Debug.LogError("Error: The state must have a preset.");
                return;
            }
            for (int i = 0; i < states.Length; ++i) {
                if (states[i].Name == state.Name) {
                    Debug.LogError($"Error: A state with the name {state.Name} already exists.");
                    return;
                }
            }
            if (!m_ObjectNameStateMap.TryGetValue(owner, out var nameStateMap) ||
                !m_GameObjectNameStateList.TryGetValue(ownerGameObject, out var nameStateList)) {
                Debug.LogError($"Error: The object {owner} has not been registered with the StateManager.");
                return;
            }

            // The state is valid. Add the state to the states array.
            System.Array.Resize(ref states, states.Length + 1);
            for (int i = states.Length - 1; i > index; --i) {
                states[i] = states[i - 1];
            }
            states[index] = state;
            if (owner is StateBehavior) {
                (owner as StateBehavior).States = states;
            } else if (owner is StateObject) {
                (owner as StateObject).States = states;
            }

            // Update the internal mappings.
            nameStateMap.Add(state.Name, state);
            if (!nameStateList.TryGetValue(state.Name, out var stateList)) {
                stateList = new List<State>();
                nameStateList.Add(state.Name, stateList);
            }
            stateList.Insert(index, state);
            for (int i = 0; i < states.Length; ++i) {
                if (i == index) {
                    continue;
                }
                m_StateArrayMap[states[i]] = states;
            }
            m_StateArrayMap.Add(state, states);
            m_GameObjectNameStateList[ownerGameObject] = nameStateList;

            // All of the lists have been updated. Initialize the state.
            state.Initialize(owner, nameStateMap);
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_Initialized = false;
            s_Instance = null;
        }
    }
}