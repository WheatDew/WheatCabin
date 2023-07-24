/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The CrosshairsMonitor will update the UI for the crosshair.
    /// </summary>
    public class CrosshairsMonitor : CharacterMonitor
    {
#if UNITY_EDITOR
        [Tooltip("Draw a debug line to see the direction that the crosshairs is looking (editor only).")]
        [SerializeField] protected bool m_DebugDrawLookRay;
#endif
        [Tooltip("Should the crosshairs automatically set the target on the on the AssistAim ability?")]
        [SerializeField] protected bool m_AutoAssignAssistAimTarget = true;
        [Tooltip("The radius of the crosshair's collision sphere to detect if it is targetting an enemy.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The maximum number of colliders that the crosshairs can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 40;
        [Tooltip("Specifies if the crosshairscan detect triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_TriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("Should the crosshairs move with the crosshairs?")]
        [SerializeField] protected bool m_MoveWithCursor;
        [Tooltip("The amount of spread to add based on the character's movement speed.")]
        [Range(0, 360)] [SerializeField] protected float m_MovementSpread;
        [Tooltip("The maximum squared magnitude of the character. The spread will be at the max value when the character is moving at this magnitude.")]
        [SerializeField] protected float m_MaxSpreadVelocitySqrMagnitude = 50;
        [Tooltip("The speed at which the spread value should change values.")]
        [SerializeField] protected float m_MovementSpreadSpeed = 6;
        [Tooltip("The crosshairs used when the item doesn't specify a crosshairs.")]
        [SerializeField] protected Sprite m_DefaultSprite;
        [Tooltip("The default color of the crosshairs.")]
        [SerializeField] protected Color m_DefaultColor = Color.white;
        [Tooltip("The color of the crosshairs when a target is in sight.")]
        [SerializeField] protected Color m_TargetColor = Color.red;
        [Tooltip("A reference to the image used for the center crosshairs.")]
        [SerializeField] protected Image m_CenterCrosshairsImage;
        [Tooltip("A reference to the image used for the left crosshairs.")]
        [SerializeField] protected Image m_LeftCrosshairsImage;
        [Tooltip("A reference to the image used for the top crosshairs.")]
        [SerializeField] protected Image m_TopCrosshairsImage;
        [Tooltip("A reference to the image used for the right crosshairs.")]
        [SerializeField] protected Image m_RightCrosshairsImage;
        [Tooltip("A reference to the image used for the bottom crosshairs.")]
        [SerializeField] protected Image m_BottomCrosshairsImage;
        [Tooltip("Should the crosshairs be disabled when the character dies?")]
        [SerializeField] protected bool m_DisableOnDeath = true;

        public bool AutoAssignAssistAimTarget { get { return m_AutoAssignAssistAimTarget; } set { m_AutoAssignAssistAimTarget = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public QueryTriggerInteraction TriggerInteraction { get { return m_TriggerInteraction; } set { m_TriggerInteraction = value; } }
        public bool MoveWithCursor { get { return m_MoveWithCursor; } set { 
                if (m_MoveWithCursor == value) { return; }
                m_MoveWithCursor = value;
                if (m_MoveWithCursor) {
                    m_OriginalPosition = m_RectTransform.position;
                } else {
                    m_RectTransform.position = m_OriginalPosition;
                }
            } 
        }
        public float MovementSpread { get { return m_MovementSpread; } set { m_MovementSpread = value; } }
        public float MaxSpreadVelocitySqrMagnitude { get { return m_MaxSpreadVelocitySqrMagnitude; } set { m_MaxSpreadVelocitySqrMagnitude = value; } }
        public float MovementSpreadSpeed { get { return m_MovementSpreadSpeed; } set { m_MovementSpreadSpeed = value; } }
        public Color DefaultColor { get { return m_DefaultColor; } set { m_DefaultColor = value; } }
        public Color TargetColor { get { return m_TargetColor; } set { m_TargetColor = value; } }
        public bool DisableOnDeath { get { return m_DisableOnDeath; } set { m_DisableOnDeath = value; } }

        [System.NonSerialized] private GameObject m_GameObject;
        private UnityEngine.Camera m_Camera;
        private CameraController m_CameraController;
        private Transform m_CharacterTransform;
        private CharacterLayerManager m_CharacterLayerManager;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Shared.Input.IPlayerInput m_PlayerInput;
        private Character.Abilities.AssistAim m_AssistAim;

        private RectTransform m_RectTransform;
        private RectTransform m_CenterRectTransform;
        private RectTransform m_LeftRectTransform;
        private RectTransform m_TopRectTransform;
        private RectTransform m_RightRectTransform;
        private RectTransform m_BottomRectTransform;

        private RaycastHit[] m_RaycastHits;
        private UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();
        private CharacterItem m_EquippedCharacterItem;
        private float m_CurrentCrosshairsSpread;
        private float m_TargetCrosshairsSpread;
        private float m_CrosshairsSpreadVelocity;
        private bool m_Aiming;
        private bool m_EnableImage;
        private int m_EquippedItemCount;
        private float m_SmoothedMovementVelocity;
        private Vector3 m_OriginalPosition;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_RectTransform = GetComponent<RectTransform>();
            if (m_CenterCrosshairsImage == null) {
                m_CenterCrosshairsImage = GetComponent<Image>();
            }
            m_CenterCrosshairsImage.sprite = m_DefaultSprite;
            m_CenterRectTransform = m_CenterCrosshairsImage.GetComponent<RectTransform>();
            if ((m_CenterCrosshairsImage.enabled = (m_DefaultSprite != null)) == true) {
                UnityEngineUtility.SizeSprite(m_CenterCrosshairsImage.sprite, m_CenterRectTransform);
            }

            if (m_LeftCrosshairsImage != null) m_LeftRectTransform = m_LeftCrosshairsImage.GetComponent<RectTransform>();
            if (m_TopCrosshairsImage != null) m_TopRectTransform = m_TopCrosshairsImage.GetComponent<RectTransform>();
            if (m_RightCrosshairsImage != null) m_RightRectTransform = m_RightCrosshairsImage.GetComponent<RectTransform>();
            if (m_BottomCrosshairsImage != null) m_BottomRectTransform = m_BottomCrosshairsImage.GetComponent<RectTransform>();

            m_RaycastHits = new RaycastHit[m_MaxCollisionCount];

            // Setup the crosshairs defaults.
            ResetMonitor();
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
                EventHandler.UnregisterEvent<CharacterItem, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
                EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAddCrosshairsSpread", OnAddCrosshairsSpread);
                EventHandler.UnregisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                ResetMonitor();
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }

            m_Camera = Shared.Camera.CameraUtility.FindCamera(m_Character);
            m_CameraController = m_Camera.gameObject.GetCachedComponent<CameraController>();
            m_CameraController.SetCrosshairs(transform);

            m_CharacterTransform = m_Character.transform;
            m_CharacterLayerManager = m_Character.GetCachedComponent<CharacterLayerManager>();
            m_CharacterLocomotion = m_Character.GetCachedComponent<UltimateCharacterLocomotion>();
            m_PlayerInput = m_Character.GetCachedComponent<Shared.Input.IPlayerInput>();
            m_AssistAim = m_CharacterLocomotion.GetAbility<Character.Abilities.AssistAim>();
            m_EnableImage = false;
            gameObject.SetActive(CanShowUI());

            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<CharacterItem, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAddCrosshairsSpread", OnAddCrosshairsSpread);
            EventHandler.RegisterEvent<bool, bool>(m_Character, "OnAimAbilityStart", OnAim);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);

            // An item may already be equipped.
            var inventory = m_Character.GetCachedComponent<UltimateCharacterController.Inventory.InventoryBase>();
            if (inventory != null) {
                for (int i = 0; i < inventory.SlotCount; ++i) {
                    var item = inventory.GetActiveCharacterItem(i);
                    if (item != null) {
                        OnEquipItem(item, i);
                    }
                }
            }
        }

        /// <summary>
        /// Determine any targets that are within the crosshairs raycast.
        /// </summary>
        private void Update()
        {
            var crosshairsColor = m_DefaultColor;
            var crosshairsRay = m_Camera.ScreenPointToRay(m_CenterRectTransform.position);
            Transform target = null;
            // Prevent the ray between the character and the camera from causing a false collision.
            if (!m_CharacterLocomotion.FirstPersonPerspective) {
                var direction = m_CharacterTransform.InverseTransformPoint(crosshairsRay.origin);
                direction.y = 0;
                crosshairsRay.origin = crosshairsRay.GetPoint(direction.magnitude);
            }
#if UNITY_EDITOR
            // Visualize the direction of the look direction.
            if (m_DebugDrawLookRay) {
                Debug.DrawRay(crosshairsRay.origin, crosshairsRay.direction * m_CameraController.LookDirectionDistance, Color.white);
            }
#endif
            var hitCount = Physics.SphereCastNonAlloc(crosshairsRay, m_CollisionRadius, m_RaycastHits, m_CameraController.LookDirectionDistance, m_CharacterLayerManager.IgnoreInvisibleLayers, m_TriggerInteraction);
#if UNITY_EDITOR
            if (hitCount == m_MaxCollisionCount) {
                Debug.LogWarning("Warning: The crosshairs detected the maximum number of objects. Consider increasing the Max Collision Count on the Crosshairs Monitor.");
            }
#endif
            if (hitCount > 0) {
                for (int i = 0; i < hitCount; ++i) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_RaycastHits, hitCount, i, m_RaycastHitComparer);
                    var closestRaycastHitTransform = closestRaycastHit.transform;
                    // The crosshairs cannot hit the character that is attached to the camera.
                    if (closestRaycastHitTransform.IsChildOf(m_CharacterTransform)) {
                        continue;
                    }

                    if (MathUtility.InLayerMask(closestRaycastHitTransform.gameObject.layer, m_CharacterLayerManager.EnemyLayers)) {
                        target = closestRaycastHitTransform;
                        crosshairsColor = m_TargetColor;
                    }
                    break;
                }
            }
            
            if (m_AutoAssignAssistAimTarget && m_AssistAim != null) {
                m_AssistAim.Target = target;
            }

            // The crosshairs can move with the cursor position.
            if (m_MoveWithCursor && Cursor.visible) {
                m_RectTransform.position = new Vector3(m_PlayerInput.GetMousePosition().x, m_PlayerInput.GetMousePosition().y, 0);
            }

            // The spread of the crosshairs can change based on how quickly the character is moving.
            if (m_MovementSpread > 0) {
                var velocity = Mathf.Clamp01(m_CharacterLocomotion.LocalVelocity.sqrMagnitude / m_MaxSpreadVelocitySqrMagnitude);
                m_SmoothedMovementVelocity = Mathf.SmoothStep(m_SmoothedMovementVelocity, velocity, m_MovementSpreadSpeed * Time.deltaTime);
            }
            if (m_EquippedCharacterItem != null) {
                var movementSpread = (((m_MovementSpread / 360) + m_SmoothedMovementVelocity) / 2) * m_EquippedCharacterItem.MaxQuadrantSpread;
                m_CurrentCrosshairsSpread = Mathf.SmoothDamp(m_CurrentCrosshairsSpread,
                                                Mathf.Clamp(m_TargetCrosshairsSpread + movementSpread, 0, m_EquippedCharacterItem.MaxQuadrantSpread), 
                                                ref m_CrosshairsSpreadVelocity, m_EquippedCharacterItem.QuadrantSpreadDamping);
            }
            m_CenterCrosshairsImage.color = crosshairsColor;
            if (m_LeftCrosshairsImage != null && m_LeftCrosshairsImage.enabled) {
                m_LeftCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_LeftRectTransform, -m_EquippedCharacterItem.QuadrantOffset - m_CurrentCrosshairsSpread, 0);
            }
            if (m_TopCrosshairsImage != null && m_TopCrosshairsImage.enabled) {
                m_TopCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_TopRectTransform, 0, m_EquippedCharacterItem.QuadrantOffset + m_CurrentCrosshairsSpread);
            }
            if (m_RightCrosshairsImage != null && m_RightCrosshairsImage.enabled) {
                m_RightCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_RightRectTransform, m_EquippedCharacterItem.QuadrantOffset + m_CurrentCrosshairsSpread, 0);
            }
            if (m_BottomCrosshairsImage != null && m_BottomCrosshairsImage.enabled) {
                m_BottomCrosshairsImage.color = crosshairsColor;
                PositionSprite(m_BottomRectTransform, 0, -m_EquippedCharacterItem.QuadrantOffset - m_CurrentCrosshairsSpread);
            }

            var enableImage = !m_Aiming || (m_EquippedCharacterItem != null && m_EquippedCharacterItem.ShowCrosshairsOnAim);
            if (enableImage != m_EnableImage) {
                m_EnableImage = enableImage;
                EnableCrosshairsImage(enableImage);
            }
        }

        /// <summary>
        /// Returns the spread based on the movement values.
        /// </summary>
        /// <returns>The spread based on the movement values (0-1).</returns>
        public float GetMovementSpread()
        {
            var modifier = Mathf.Clamp01(m_CharacterLocomotion.LocalVelocity.sqrMagnitude / m_MaxSpreadVelocitySqrMagnitude);
            return (m_MovementSpread / 360) * modifier;
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="characterItem">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(CharacterItem characterItem, int slotID)
        {
            if (!characterItem.DominantItem) {
                return;
            }

            m_CurrentCrosshairsSpread = m_TargetCrosshairsSpread = 0;
            m_EquippedCharacterItem = characterItem;
            m_CenterCrosshairsImage.sprite = m_EquippedCharacterItem.CenterCrosshairs != null ? m_EquippedCharacterItem.CenterCrosshairs : m_DefaultSprite;
            if (m_CenterCrosshairsImage.sprite != null) {
                m_CenterCrosshairsImage.enabled = !m_Aiming || m_EquippedCharacterItem.ShowCrosshairsOnAim;
                UnityEngineUtility.SizeSprite(m_CenterCrosshairsImage.sprite, m_CenterRectTransform);
            } else {
                m_CenterCrosshairsImage.enabled = false;
            }
            if (m_LeftCrosshairsImage != null) {
                m_LeftCrosshairsImage.sprite = m_EquippedCharacterItem.LeftCrosshairs;
                if (m_LeftCrosshairsImage.sprite != null) {
                    m_LeftCrosshairsImage.enabled = !m_Aiming || m_EquippedCharacterItem.ShowCrosshairsOnAim;
                    PositionSprite(m_LeftRectTransform, -m_EquippedCharacterItem.QuadrantOffset, 0);
                    UnityEngineUtility.SizeSprite(m_LeftCrosshairsImage.sprite, m_LeftRectTransform);
                } else {
                    m_LeftCrosshairsImage.enabled = false;
                }
            }
            if (m_TopCrosshairsImage != null) {
                m_TopCrosshairsImage.sprite = m_EquippedCharacterItem.TopCrosshairs;
                if (m_TopCrosshairsImage.sprite != null) {
                    m_TopCrosshairsImage.enabled = !m_Aiming || m_EquippedCharacterItem.ShowCrosshairsOnAim;
                    PositionSprite(m_TopRectTransform, 0, m_EquippedCharacterItem.QuadrantOffset);
                    UnityEngineUtility.SizeSprite(m_TopCrosshairsImage.sprite, m_TopRectTransform);
                } else {
                    m_TopCrosshairsImage.enabled = false;
                }
            }
            if (m_RightCrosshairsImage != null) {
                m_RightCrosshairsImage.sprite = m_EquippedCharacterItem.RightCrosshairs;
                if (m_RightCrosshairsImage.sprite != null) {
                    m_RightCrosshairsImage.enabled = !m_Aiming || m_EquippedCharacterItem.ShowCrosshairsOnAim;
                    PositionSprite(m_RightRectTransform, m_EquippedCharacterItem.QuadrantOffset, 0);
                    UnityEngineUtility.SizeSprite(m_RightCrosshairsImage.sprite, m_RightRectTransform);
                } else {
                    m_RightCrosshairsImage.enabled = false;
                }
            }
            if (m_BottomCrosshairsImage != null) {
                m_BottomCrosshairsImage.sprite = m_EquippedCharacterItem.BottomCrosshairs;
                if (m_BottomCrosshairsImage.sprite != null) {
                    m_BottomCrosshairsImage.enabled = !m_Aiming || m_EquippedCharacterItem.ShowCrosshairsOnAim;
                    PositionSprite(m_BottomRectTransform, 0, -m_EquippedCharacterItem.QuadrantOffset);
                    UnityEngineUtility.SizeSprite(m_BottomCrosshairsImage.sprite, m_BottomRectTransform);
                } else {
                    m_BottomCrosshairsImage.enabled = false;
                }
            }
            m_EquippedItemCount++;
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="characterItem">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        private void OnUpdateDominantItem(CharacterItem characterItem, bool dominantItem)
        {
            if (characterItem.DominantItem && characterItem.IsActive()) {
                OnEquipItem(characterItem, -1);
            } else if (m_EquippedCharacterItem == characterItem) {
                m_EquippedItemCount--;
                if (m_EquippedItemCount == 0) {
                    ResetMonitor();
                }
            }
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            if (characterItem != m_EquippedCharacterItem) {
                return;
            }

            m_EquippedItemCount--;
            if (m_EquippedItemCount == 0) {
                ResetMonitor();
            }
        }

        /// <summary>
        /// Resets the monitor back to the default state.
        /// </summary>
        private void ResetMonitor()
        {
            m_EquippedCharacterItem = null;
            m_EquippedItemCount = 0;
            m_CenterCrosshairsImage.sprite = m_DefaultSprite;
            m_CenterCrosshairsImage.enabled = m_DefaultSprite != null;
            if (m_LeftCrosshairsImage != null) {
                m_LeftCrosshairsImage.sprite = null;
                m_LeftCrosshairsImage.enabled = false;
            }
            if (m_TopCrosshairsImage != null) {
                m_TopCrosshairsImage.sprite = null;
                m_TopCrosshairsImage.enabled = false;
            }
            if (m_RightCrosshairsImage != null) {
                m_RightCrosshairsImage.sprite = null;
                m_RightCrosshairsImage.enabled = false;
            }
            if (m_BottomCrosshairsImage != null) {
                m_BottomCrosshairsImage.sprite = null;
                m_BottomCrosshairsImage.enabled = false;
            }
        }

        /// <summary>
        /// Positions the sprite according to the specified x and y position.
        /// </summary>
        /// <param name="spriteRectTransform">The transform to position.</param>
        /// <param name="xPosition">The x position of the sprite.</param>
        /// <param name="yPosition">The y position of the sprite.</param>
        private void PositionSprite(RectTransform spriteRectTransform, float xPosition, float yPosition)
        {
            var position = spriteRectTransform.localPosition;
            position.x = xPosition;
            position.y = yPosition;
            spriteRectTransform.localPosition = position;
        }

        /// <summary>
        /// Adds a force to the quadrant recoil spring.
        /// </summary>
        /// <param name="start">Is the spread just starting?</param>
        /// <param name="fromRecoil">Is the spread being added from a recoil?</param>
        private void OnAddCrosshairsSpread(bool start, bool fromRecoil)
        {
            if (m_EquippedCharacterItem == null) {
                return;
            }

            if (start) {
                m_CurrentCrosshairsSpread = fromRecoil ? m_EquippedCharacterItem.MaxQuadrantSpread : 0;
                m_TargetCrosshairsSpread = fromRecoil ? 0 : m_EquippedCharacterItem.MaxQuadrantSpread;
            } else {
                m_TargetCrosshairsSpread = 0;
            }
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }

            m_Aiming = aim;
        }

        /// <summary>
        /// Enables or disables the crosshairs image.
        /// </summary>
        /// <param name="enable">Should the crosshairs be enabled?</param>
        private void EnableCrosshairsImage(bool enable)
        {
            m_CenterCrosshairsImage.enabled = enable && m_CenterCrosshairsImage.sprite != null;
            if (m_LeftCrosshairsImage != null) m_LeftCrosshairsImage.enabled = enable && m_LeftCrosshairsImage.sprite != null;
            if (m_TopCrosshairsImage != null) m_TopCrosshairsImage.enabled = enable && m_TopCrosshairsImage.sprite != null;
            if (m_RightCrosshairsImage != null) m_RightCrosshairsImage.enabled = enable && m_RightCrosshairsImage.sprite != null;
            if (m_BottomCrosshairsImage != null) m_BottomCrosshairsImage.enabled = enable && m_BottomCrosshairsImage.sprite != null;
            if (!enable) { m_TargetCrosshairsSpread = m_CurrentCrosshairsSpread = 0; }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (m_DisableOnDeath) {
                m_GameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            if (m_DisableOnDeath && base.CanShowUI()) {
                m_GameObject.SetActive(true);
            }

            // Force the crosshairs to update so the color will be correct.
            Update();
        }

        /// <summary>
        /// Shows or hides the UI.
        /// </summary>
        /// <param name="show">Should the UI be shown?</param>
        protected override void ShowUI(bool show)
        {
            base.ShowUI(show);

            if (!CanShowUI() || m_GameObject == null) {
                m_TargetCrosshairsSpread = m_CurrentCrosshairsSpread = 0;
            } else {
                Update();
            }
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && (!m_DisableOnDeath || (m_CharacterLocomotion != null && m_CharacterLocomotion.Alive));
        }
    }
}