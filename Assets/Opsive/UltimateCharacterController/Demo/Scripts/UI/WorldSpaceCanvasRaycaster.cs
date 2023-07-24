/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// This component needs to be set next to a Graphic Raycaster.
    /// It will allow you to interact with World Space Canvas even when the Cursor is locked.
    /// </summary>
    public class WorldSpaceCanvasRaycaster : MonoBehaviour
    {
        [Tooltip("The distance at which raycasts will be taken into account.")]
        [SerializeField] private float m_RaycastDistance = 20;

        private GraphicRaycaster m_Raycaster;
        private PointerEventData m_PointerEventData;
        private EventSystem m_EventSystem;

        private List<RaycastResult> m_RaycastResults;
        private List<Selectable> m_NewSelectables;
        private List<Selectable> m_PreviousSelectables;
        private List<Selectable> m_DownedSelectables;

        private bool m_PreviousInputDown = false;

        private GameObject m_Character;

        public float RaycastDistance { get => m_RaycastDistance; set => m_RaycastDistance = value; }
        public GameObject Character { get => m_Character; set => m_Character = value; }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        private void Start()
        {
            m_RaycastResults = new List<RaycastResult>();
            m_NewSelectables = new List<Selectable>();
            m_PreviousSelectables = new List<Selectable>();
            m_DownedSelectables = new List<Selectable>();
            m_Raycaster = GetComponent<GraphicRaycaster>();
            m_EventSystem = EventSystem.current;

            if (m_Character == null) {
                m_Character = GameObject.FindGameObjectWithTag("Player");
            }
        }

        /// <summary>
        /// Raycast the UI to pointer up, down, enter, exit and click.
        /// </summary>
        private void Update()
        {
            // Only use this component while the cursor is locked.
            if (Cursor.lockState != CursorLockMode.Locked) { return; }

            m_PointerEventData = new PointerEventData(m_EventSystem);

            // Set the Pointer Event Position to that of the mouse position, which is the center of the screen.
            m_PointerEventData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            m_PointerEventData.pointerId = 0;
            m_RaycastResults.Clear();

            m_Raycaster.Raycast(m_PointerEventData, m_RaycastResults);
            m_NewSelectables.Clear();

            foreach (RaycastResult result in m_RaycastResults) {

                // The distance cannot be set on the Raycaster raycast.
                if (result.distance > m_RaycastDistance) {
                    continue;
                }

                var selectable = result.gameObject.GetCachedParentComponent<Selectable>();
                if (selectable == null) { continue; }
                if (m_NewSelectables.Contains(selectable)) { continue; }
                m_NewSelectables.Add(selectable);

                if (m_PreviousSelectables.Contains(selectable)) { continue; }

                m_PointerEventData.pointerEnter = selectable.gameObject;
                selectable.OnPointerEnter(m_PointerEventData);
            }

            // Pointer exit selectable that are no longer in the raycast.
            for (int i = 0; i < m_PreviousSelectables.Count; i++) {
                var previousSelected = m_PreviousSelectables[i];
                if (m_NewSelectables.Contains(previousSelected)) { continue; }

                previousSelected.OnPointerExit(m_PointerEventData);

                if (m_DownedSelectables.Contains(previousSelected)) {
                    m_PointerEventData.button = PointerEventData.InputButton.Left;
                    previousSelected.OnPointerUp(m_PointerEventData);
                    m_DownedSelectables.Remove(previousSelected);
                }
            }

            var inputDown = Input.GetKey(KeyCode.Mouse0);
            var pointerDown = !m_PreviousInputDown && inputDown;
            var pointerUp = m_PreviousInputDown && !inputDown;
            if (pointerDown) {
                for (int i = 0; i < m_NewSelectables.Count; i++) {
                    var selectable = m_NewSelectables[i];
                    m_PointerEventData.button = PointerEventData.InputButton.Left;
                    selectable.OnPointerDown(m_PointerEventData);
                    m_DownedSelectables.Add(selectable);
                }
            } else if (pointerUp) {
                for (int i = 0; i < m_NewSelectables.Count; i++) {
                    var selectable = m_NewSelectables[i];

                    if (!m_DownedSelectables.Contains(selectable)) { continue; }
                    m_DownedSelectables.Remove(selectable);

                    m_PointerEventData.button = PointerEventData.InputButton.Left;
                    selectable.OnPointerUp(m_PointerEventData);

                    if (selectable is IPointerClickHandler pointerClickHandler) {
                        pointerClickHandler.OnPointerClick(m_PointerEventData);
                    }
                }
            }

            if (m_Character != null) {
                if (m_PreviousSelectables.Count > 0 && m_NewSelectables.Count == 0) {
                    // There used to be something selected but no longer.
                    StateManager.SetState(m_Character, "HoveringWorldSpaceSelectable", false);
                } else if (m_PreviousSelectables.Count == 0 && m_NewSelectables.Count > 0) {
                    // Something is now selected.
                    StateManager.SetState(m_Character, "HoveringWorldSpaceSelectable", true);
                }
            }

            // Set the variables for the next loop previous.
            m_PreviousSelectables.Clear();
            m_PreviousSelectables.AddRange(m_NewSelectables);
            m_PreviousInputDown = inputDown;
        }
    }
}