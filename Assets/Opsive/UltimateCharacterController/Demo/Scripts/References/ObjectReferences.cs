/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.References
{
    using UnityEngine;

    /// <summary>
    /// Helper class which references the objects.
    /// </summary>
    [Utility.IgnoreTemplateCopy]
    public class ObjectReferences : MonoBehaviour
    {
        [Tooltip("A reference to the first person objects.")]
        [SerializeField] protected Object[] m_FirstPersonObjects;
        [Tooltip("A reference to the third person objects.")]
        [SerializeField] protected Object[] m_ThirdPersonObjects;
        [Tooltip("Any object that should always be removed.")]
        [SerializeField] protected Object[] m_RemoveObjects;
        [Tooltip("Objects that should use the shadow caster while in a first person only perspective.")]
        [SerializeField] protected GameObject[] m_ShadowCasterObjects;
        [Tooltip("A reference to other Object References that should be checked.")]
        [SerializeField] protected ObjectReferences[] m_NestedReferences;

        public Object[] FirstPersonObjects { get { return m_FirstPersonObjects; } set { m_FirstPersonObjects = value; } }
        public Object[] ThirdPersonObjects { get { return m_ThirdPersonObjects; } set { m_ThirdPersonObjects = value; } }
        public Object[] RemoveObjects { get { return m_RemoveObjects; } set { m_RemoveObjects = value; } }
        public GameObject[] ShadowCasterObjects { get { return m_ShadowCasterObjects; } set { m_ShadowCasterObjects = value; } }
        public ObjectReferences[] NestedReferences { get { return m_NestedReferences; } set { m_NestedReferences = value; } }
    }
}