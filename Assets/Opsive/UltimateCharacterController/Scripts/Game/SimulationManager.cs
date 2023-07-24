/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Manages when the character and cameras should move.
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        private static SimulationManager s_Instance;
        private static SimulationManager Instance
        {
            get {
                if (!s_Initialized) {
                    s_Instance = new GameObject("SimulationManager").AddComponent<SimulationManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        private float m_FixedTime;

        /// <summary>
        /// A small storage class used for storing the fixed and smooth location. This component will also move the interpolate the objects during the Update loop.
        /// </summary>
        private class SmoothedTransform
        {
            protected Transform m_Transform;
            protected Rigidbody m_Rigidbody;

            private Vector3 m_FixedPosition;
            private Quaternion m_FixedRotation;
            private Vector3 m_SmoothPosition;
            private Quaternion m_SmoothRotation;

            public Transform Transform { get { return m_Transform; } }

            /// <summary>
            /// Initializes the object.
            /// </summary>
            /// <param name="transform">The transform that is being managed by the SimulationManager.</param>
            public void Initialize(Transform transform)
            {
                m_Transform = transform;
                m_Rigidbody = transform.GetComponent<Rigidbody>();

                m_FixedPosition = m_SmoothPosition = m_Transform.position;
                m_FixedRotation = m_SmoothRotation = m_Transform.rotation;
            }

            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position.</param>
            public virtual void SmoothMove(float interpAmount)
            {
                m_Transform.SetPositionAndRotation(Vector3.Lerp(m_SmoothPosition, m_FixedPosition, interpAmount), Quaternion.Slerp(m_SmoothRotation, m_FixedRotation, interpAmount));
                if (m_Rigidbody != null) {
                    m_Rigidbody.position = m_Transform.position;
                    m_Rigidbody.rotation = m_Transform.rotation;
                }
            }

            /// <summary>
            /// Restores the location back to the fixed location. This will be performed immediately before the object is moved within FixedUpdate.
            /// </summary>
            public virtual void RestoreFixedLocation()
            {
                m_SmoothPosition = m_FixedPosition;
                m_SmoothRotation = m_FixedRotation;
                m_Transform.SetPositionAndRotation(m_SmoothPosition, m_SmoothRotation);
                if (m_Rigidbody != null) {
                    m_Rigidbody.position = m_SmoothPosition;
                    m_Rigidbody.rotation = m_SmoothRotation;
                }
            }

            /// <summary>
            /// Assigns the fixed location. This will be performed immediately after the object is moved within FixedUpdate.
            /// </summary>
            public virtual void AssignFixedLocation()
            {
                m_FixedPosition = m_Transform.position;
                m_FixedRotation = m_Transform.rotation;
            }

            /// <summary>
            /// Immediately set the object's position.
            /// </summary>
            /// <param name="position">The position of the object.</param>
            public virtual void SetPosition(Vector3 position)
            {
                m_Transform.position = m_FixedPosition = m_SmoothPosition = position;
                if (m_Rigidbody != null) {
                    m_Rigidbody.position = position;
                }
            }

            /// <summary>
            /// Immediately set the object's rotation.
            /// </summary>
            /// <param name="rotation">The rotation of the object.</param>
            public virtual void SetRotation(Quaternion rotation)
            {
                m_Transform.rotation = m_FixedRotation = m_SmoothRotation = rotation;
                if (m_Rigidbody != null) {
                    m_Rigidbody.rotation = rotation;
                }
            }
        }

        /// <summary>
        /// Smoothly moves the character.
        /// </summary>
        private class SmoothedCharacter : SmoothedTransform
        {
            private CharacterLocomotion m_Locomotion;
            private ICharacterHandler m_Handler;

            public CharacterLocomotion Locomotion { get { return m_Locomotion; } }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="locomotion">The registered character.</param>
            /// <param name="handler">The camera handler component.</param>
            public SmoothedCharacter(CharacterLocomotion locomotion, ICharacterHandler handler)
            {
                Initialize(locomotion.transform);

                m_Locomotion = locomotion;
                m_Handler = handler;
            }

            /// <summary>
            /// Moves the character.
            /// </summary>
            /// <param name="preMove">Should the character be premoved?</param>
            public void Move(bool preMove)
            {
                if (preMove) {
                    RestoreFixedLocation();
                    m_Locomotion.PreMove();
                    return;
                }

                float horizontalMovement = 0, forwardMovement = 0, deltaYaw = 0;
                if (m_Handler != null) {
                    m_Handler.GetPositionInput(out horizontalMovement, out forwardMovement);
                    m_Handler.GetRotationInput(horizontalMovement, forwardMovement, out deltaYaw);
                }

                m_Locomotion.Move(horizontalMovement, forwardMovement, deltaYaw);
                AssignFixedLocation();
            }
        }

        /// <summary>
        /// Smoothly moves the camera.
        /// </summary>
        private class SmoothedCamera : SmoothedTransform
        {
            private CameraController m_Controller;
            private CameraControllerHandler m_Handler;
            private UnityEngine.Camera m_Camera;

            private float m_FixedFieldOfView;
            private float m_SmoothFieldOfView;

            public CameraController Controller { get { return m_Controller; } }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="controller">The registered camera.</param>
            /// <param name="handler">The camera handler component.</param>
            public SmoothedCamera(CameraController controller, CameraControllerHandler handler)
            {
                Initialize(controller.transform);

                m_Controller = controller;
                m_Handler = handler;
                m_Camera = m_Controller.GetComponent<UnityEngine.Camera>();

                m_SmoothFieldOfView = m_FixedFieldOfView = m_Camera.fieldOfView;
            }

            /// <summary>
            /// Rotates the camera.
            /// </summary>
            public void Rotate()
            {
                RestoreFixedLocation();

                float horizontalMovement = 0, forwardMovement = 0;
                if (m_Handler != null) {
                    m_Handler.GetMoveInput(out horizontalMovement, out forwardMovement);
                }

                m_Controller.Rotate(horizontalMovement, forwardMovement);
            }

            /// <summary>
            /// Moves the camera.
            /// </summary>
            public void Move()
            {
                m_Controller.Move();

                AssignFixedLocation();
            }

            /// <summary>
            /// The object is moved within FixedUpdate while the camera is moved within Update. This would normally cause jitters but a separate smooth variable
            /// ensures the object stays in synchronize with the Update loop.
            /// </summary>
            /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position.</param>
            public override void SmoothMove(float interpAmount)
            {
                base.SmoothMove(interpAmount);

                m_Camera.fieldOfView = Mathf.Lerp(m_SmoothFieldOfView, m_FixedFieldOfView, interpAmount);
            }

            /// <summary>
            /// Restores the location back to the fixed location. This will be performed immediately before the object is moved within FixedUpdate.
            /// </summary>
            public override void RestoreFixedLocation()
            {
                base.RestoreFixedLocation();

                m_SmoothFieldOfView = m_FixedFieldOfView;
                m_Camera.fieldOfView = m_SmoothFieldOfView;
            }

            /// <summary>
            /// Assigns the fixed location. This will be performed immediately after the object is moved within FixedUpdate.
            /// </summary>
            public override void AssignFixedLocation()
            {
                base.AssignFixedLocation();

                m_FixedFieldOfView = m_Camera.fieldOfView;
            }
        }

        /// <summary>
        /// Smoothly moves the kinematic object.
        /// </summary>
        private class SmoothedKinematicObject : SmoothedTransform
        {
            private IKinematicObject m_KinematicObject;

            public IKinematicObject IKinematicObject { get { return m_KinematicObject; } }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="kinematicObject">The registered kinematic object.</param>
            public SmoothedKinematicObject(IKinematicObject kinematicObject)
            {
                m_KinematicObject = kinematicObject;

                Initialize(m_KinematicObject.Transform);
            }
            
            /// <summary>
            /// Moves the kinematic object.
            /// </summary>
            public void Move()
            {
                RestoreFixedLocation();
                m_KinematicObject.Move();
                AssignFixedLocation();
            }
        }

        private ResizableArray<SmoothedCharacter> m_Characters = new ResizableArray<SmoothedCharacter>();
        private ResizableArray<SmoothedCamera> m_Cameras = new ResizableArray<SmoothedCamera>();
        private ResizableArray<SmoothedKinematicObject> m_KinematicObjects = new ResizableArray<SmoothedKinematicObject>();

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
        /// Registers the character with the SimulationManager.
        /// </summary>
        /// <param name="characterLocomotion">The motor of the character that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        public static int RegisterCharacter(CharacterLocomotion characterLocomotion)
        {
            return Instance.RegisterCharacterInternal(characterLocomotion);
        }

        /// <summary>
        /// Internal method which registers the character with the SimulationManager.
        /// </summary>
        /// <param name="characterLocomotion">The motor of the character that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        private int RegisterCharacterInternal(CharacterLocomotion characterLocomotion)
        {
            m_Characters.Add(new SmoothedCharacter(characterLocomotion, characterLocomotion.GetComponent<ICharacterHandler>()));
            return m_Characters.Count - 1;
        }

        /// <summary>
        /// Registers the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        public static int RegisterCamera(CameraController cameraController)
        {
            return Instance.RegisterCameraInternal(cameraController);
        }

        /// <summary>
        /// Internal method which registers the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        private int RegisterCameraInternal(CameraController cameraController)
        {
            m_Cameras.Add(new SmoothedCamera(cameraController, cameraController.GetComponent<CameraControllerHandler>()));
            return m_Cameras.Count - 1;
        }

        /// <summary>
        /// Registers the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        public static int RegisterKinematicObject(IKinematicObject kinematicObject)
        {
            return Instance.RegisterKinematicObjectInternal(kinematicObject);
        }

        /// <summary>
        /// Internal method which registers the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be registered.</param>
        /// <returns>The index of the object within the internal array.</returns>
        private int RegisterKinematicObjectInternal(IKinematicObject kinematicObject)
        {
            m_KinematicObjects.Add(new SmoothedKinematicObject(kinematicObject));
            return m_KinematicObjects.Count - 1;
        }

        /// <summary>
        /// Immediately sets the character's rotation.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be set.</param>
        /// <param name="rotation">The rotation of the object.</param>
        public static void SetCharacterRotation(int simulationIndex, Quaternion rotation)
        {
            Instance.SetCharacterRotationInternal(simulationIndex, rotation);
        }

        /// <summary>
        /// Internal method which immediately sets the character's rotation.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be set.</param>
        /// <param name="rotation">The rotation of the object.</param>
        private void SetCharacterRotationInternal(int simulationIndex, Quaternion rotation)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Characters.Count) {
                return;
            }

            m_Characters[simulationIndex].SetRotation(rotation);
        }

        /// <summary>
        /// Immediately sets the camera's rotation.
        /// </summary>
        /// <param name="simulationIndex">The index of the camera that should be set.</param>
        /// <param name="rotation">The rotation of the object.</param>
        public static void SetCameraRotation(int simulationIndex, Quaternion rotation)
        {
            Instance.SetCameraRotationInternal(simulationIndex, rotation);
        }

        /// <summary>
        /// Internal method which immediately sets the character's rotation.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be set.</param>
        /// <param name="rotation">The rotation of the object.</param>
        private void SetCameraRotationInternal(int simulationIndex, Quaternion rotation)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Cameras.Count) {
                return;
            }

            m_Cameras[simulationIndex].SetRotation(rotation);
        }

        /// <summary>
        /// Immediately sets the character's position.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be set.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCharacterPosition(int simulationIndex, Vector3 position)
        {
            Instance.SetCharacterPositionInternal(simulationIndex, position);
        }

        /// <summary>
        /// Internal method which immediately sets the character's position.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be set.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCharacterPositionInternal(int simulationIndex, Vector3 position)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Characters.Count) {
                return;
            }

            m_Characters[simulationIndex].SetPosition(position);
        }

        /// <summary>
        /// Immediately sets the camera's position.
        /// </summary>
        /// <param name="simulationIndex">The index of the camera that should be set.</param>
        /// <param name="position">The position of the object.</param>
        public static void SetCameraPosition(int simulationIndex, Vector3 position)
        {
            Instance.SetCameraPositionInternal(simulationIndex, position);
        }

        /// <summary>
        /// Internal method which immediately sets the camera's position.
        /// </summary>
        /// <param name="simulationIndex">The index of the camera that should be set.</param>
        /// <param name="position">The position of the object.</param>
        private void SetCameraPositionInternal(int simulationIndex, Vector3 position)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Cameras.Count) {
                return;
            }

            m_Cameras[simulationIndex].SetPosition(position);
        }

        /// <summary>
        /// Unregisters the character with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be unregistered.</param>
        public static void UnregisterCharacter(int simulationIndex)
        {
            Instance.UnregisterCharacterInternal(simulationIndex);
        }

        /// <summary>
        /// Internal method which unregisters the character with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the character that should be unregistered.</param>
        private void UnregisterCharacterInternal(int simulationIndex)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Characters.Count) {
                return;
            }

            m_Characters[simulationIndex].Locomotion.SimulationIndex = -1;
            m_Characters.RemoveAt(simulationIndex);

            // Ensure the index stays correct for all of the subsequent items.
            for (int i = simulationIndex; i < m_Characters.Count; ++i) {
                m_Characters[i].Locomotion.SimulationIndex = i;
            }
        }

        /// <summary>
        /// Unregisters the camera with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the camera that should be unregistered.</param>
        public static void UnregisterCamera(int simulationIndex)
        {
            Instance.UnregisterCameraInternal(simulationIndex);
        }

        /// <summary>
        /// Internal method which unregisters the camera with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the camera that should be unregistered.</param>
        private void UnregisterCameraInternal(int simulationIndex)
        {
            if (simulationIndex < 0 || simulationIndex >= m_Cameras.Count) {
                return;
            }

            m_Cameras[simulationIndex].Controller.SimulationIndex = -1;
            m_Cameras.RemoveAt(simulationIndex);

            // Ensure the index stays correct for all of the subsequent items.
            for (int i = simulationIndex; i < m_Cameras.Count; ++i) {
                m_Cameras[i].Controller.SimulationIndex = i;
            }
        }

        /// <summary>
        /// Unregisters the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the kinematic object that should be unregistered.</param>
        public static void UnregisterKinematicObject(int simulationIndex)
        {
            Instance.UnregisterKinematicObjectInternal(simulationIndex);
        }

        /// <summary>
        /// Internal method which unregisters the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="simulationIndex">The index of the kinematic object that should be unregistered.</param>
        private void UnregisterKinematicObjectInternal(int simulationIndex)
        {
            if (simulationIndex < 0 || simulationIndex >= m_KinematicObjects.Count) {
                return;
            }

            m_KinematicObjects[simulationIndex].IKinematicObject.SimulationIndex = -1;
            m_KinematicObjects.RemoveAt(simulationIndex);

            // Ensure the index stays correct for all of the subsequent items.
            for (int i = simulationIndex; i < m_KinematicObjects.Count; ++i) {
                m_KinematicObjects[i].IKinematicObject.SimulationIndex = i;
            }
        }

        /// <summary>
        /// Executes during the FixedUpdate loop.
        /// </summary>
        private void FixedUpdate()
        {
            MoveKinematicObjects(-1);
            MoveCharacters(true, -1);
            RotateCameras();
            MoveCharacters(false, -1);
            MoveCameras(-1);

            m_FixedTime = Time.time;
        }

        /// <summary>
        /// Executes during the Update loop.
        /// </summary>
        private void Update()
        {
            var interpAmount = (Time.time - m_FixedTime) / Time.fixedDeltaTime;
            if (float.IsNaN(interpAmount)) {
                interpAmount = 0;
            }

            MoveKinematicObjects(interpAmount);
            MoveCameras(interpAmount);
            MoveCharacters(false, interpAmount);
        }

        /// <summary>
        /// Moves the kinematic objects.
        /// </summary>
        /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position. Specify -1 to update during the FixedUpdate.</param>
        public void MoveKinematicObjects(float interpAmount)
        {
            for (int i = 0; i < m_KinematicObjects.Count; ++i) {
                if (interpAmount == -1) {
                    m_KinematicObjects[i].Move();
                } else {
                    m_KinematicObjects[i].SmoothMove(interpAmount);
                }
            }
        }

        /// <summary>
        /// Premoves and moves the characters.
        /// </summary>
        public void FullMoveCharacters()
        {
            for (int i = 0; i < m_Characters.Count; ++i) {
                m_Characters[i].Move(true);
                m_Characters[i].Move(false);
            }
        }

        /// <summary>
        /// Moves the characters.
        /// </summary>
        /// <param name="preMove">Should the characters be premoved?</param>
        /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position. Specify -1 to update during the FixedUpdate.</param>
        public void MoveCharacters(bool preMove, float interpAmount)
        {
            for (int i = 0; i < m_Characters.Count; ++i) {
                if (interpAmount == -1) {
                    m_Characters[i].Move(preMove);
                } else {
                    if (!m_Characters[i].Locomotion.Interpolate) {
                        continue;
                    }
                    m_Characters[i].SmoothMove(interpAmount);
                }
            }
        }

        /// <summary>
        /// Rotates the cameras.
        /// </summary>
        public void RotateCameras()
        {
            for (int i = 0; i < m_Cameras.Count; ++i) {
                m_Cameras[i].Rotate();
            }
        }

        /// <summary>
        /// Moves the cameras.
        /// </summary>
        /// <param name="interpAmount">The amount to interpolate between the smooth and fixed position. Specify -1 to update during the FixedUpdate.</param>
        public void MoveCameras(float interpAmount)
        {
            for (int i = 0; i < m_Cameras.Count; ++i) {
                if (interpAmount == -1) {
                    // Characters not interpolated will update within LateUpdate.
                    if (m_Cameras[i].Controller.CharacterLocomotion != null && !m_Cameras[i].Controller.CharacterLocomotion.Interpolate) {
                        continue;
                    }
                    m_Cameras[i].Move();
                } else {
                    if (m_Cameras[i].Controller.CharacterLocomotion != null && !m_Cameras[i].Controller.CharacterLocomotion.Interpolate) {
                        m_Cameras[i].Move();
                        continue;
                    }
                    m_Cameras[i].SmoothMove(interpAmount);
                }
            }
        }

        /// <summary>
        /// Sets the update order so the the first character executes before the second character.
        /// </summary>
        /// <param name="firstLocomotion">A reference to the character that should be executed first.</param>
        /// <param name="secondLocomotion">A reference to the character that should be executed second.</param>
        public static void SetUpdateOrder(CharacterLocomotion firstLocomotion, CharacterLocomotion secondLocomotion)
        {
            // Schedule the reorder so the elements aren't swapped as they are executing.
            Shared.Game.Scheduler.ScheduleFixed(Time.fixedDeltaTime / 2, Instance.SetUpdateOrderInternal, firstLocomotion, secondLocomotion);
        }

        /// <summary>
        /// Internal method which sets the update order so the the first character executes before the second character.
        /// </summary>
        /// <param name="firstLocomotion">A reference to the character that should be executed first.</param>
        /// <param name="secondLocomotion">A reference to the character that should be executed second.</param>
        public void SetUpdateOrderInternal(CharacterLocomotion firstLocomotion, CharacterLocomotion secondLocomotion)
        {
            var firstIndex = -1;
            var secondIndex = -1;
            for (int i = 0; i < m_Characters.Count; ++i) {
                if (m_Characters[i].Locomotion == firstLocomotion) {
                    firstIndex = i;
                } else if (m_Characters[i].Locomotion == secondLocomotion) {
                    secondIndex = i;
                }

                if (firstIndex != -1 && secondIndex != -1) {
                    break;
                }
            }

            if (firstIndex == -1 || secondIndex == -1) {
                Debug.LogError($"Error: The character {(firstIndex == -1 ? firstLocomotion.name : secondLocomotion)} is not registered with the SimulationManager so the update order cannot be changed.");
                return;
            }

            // No changes need to be made.
            if (firstIndex < secondIndex) {
                return;
            }

            // Swap the elements.
            var tempElement = m_Characters[firstIndex];
            m_Characters[firstIndex] = m_Characters[secondIndex];
            m_Characters[secondIndex] = tempElement;
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
        public static void DomainReset()
        {
            s_Instance = null;
            s_Initialized = false;
        }
    }
}