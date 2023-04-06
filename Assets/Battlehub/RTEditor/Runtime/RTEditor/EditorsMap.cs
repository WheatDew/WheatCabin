using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlehub.RTCommon;
using UnityEngine.UI;
using Battlehub.Utils;
using UnityEngine.Events;
using TMPro;

namespace Battlehub.RTEditor
{
    public interface IEditorsMapCreator
    {
        void Create(IEditorsMap map);
    }


    [DefaultExecutionOrder(-100)]
    public partial class EditorsMap : MonoBehaviour, IEditorsMap
    {
        private class EditorDescriptor
        {
            public int Index;
            public bool Enabled;
            public bool IsPropertyEditor;

            public EditorDescriptor(int index, bool enabled, bool isPropertyEditor)
            {
                Index = index;
                Enabled = enabled;
                IsPropertyEditor = isPropertyEditor;
            }
        }

        private class MaterialEditorDescriptor
        {
            public GameObject Editor;
            public bool Enabled;

            public MaterialEditorDescriptor(GameObject editor, bool enabled)
            {
                Editor = editor;
                Enabled = enabled;
            }
        }

        private GameObject m_defaultMaterialEditor;
        private Dictionary<Shader, MaterialEditorDescriptor> m_materialMap = new Dictionary<Shader, MaterialEditorDescriptor>();
        private Dictionary<Type, EditorDescriptor> m_map = new Dictionary<Type, EditorDescriptor>();
        private GameObject[] m_editors = new GameObject[0];
        private bool m_isLoaded = false;
        private Dictionary<Type, IComponentDescriptor> m_componentDescriptors;
        private ComponentEditor m_emptyComponentEditor;

        public Dictionary<Type, IComponentDescriptor> ComponentDescriptors
        {
            get { return m_componentDescriptors; }
        }

        public ComponentEditor VoidComponentEditor
        {
            get;
            set;
        }

        private void Awake()
        {
            IOC.RegisterFallback<IEditorsMap>(this);
            
            var type = typeof(IComponentDescriptor);
//#if !UNITY_WSA || UNITY_EDITOR
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
//#else
//            var types = type.GetTypeInfo().Assembly.GetTypes().
//                Where(p => type.IsAssignableFrom(p) && p.GetTypeInfo().IsClass && !p.IsAbstract);
//#endif

            m_componentDescriptors = new Dictionary<Type, IComponentDescriptor>();
            foreach (Type t in types)
            {
                IComponentDescriptor descriptor = (IComponentDescriptor)Activator.CreateInstance(t);
                if (descriptor == null)
                {
                    Debug.LogWarningFormat("Unable to instantiate descriptor of type " + t.FullName);
                    continue;
                }
                if (descriptor.ComponentType == null)
                {
                    Debug.LogWarningFormat("ComponentType is null. Descriptor Type {0}", t.FullName);
                    continue;
                }
                if (m_componentDescriptors.ContainsKey(descriptor.ComponentType))
                {
                    IComponentDescriptor alreadyAddedDescriptor = m_componentDescriptors[descriptor.ComponentType];
                    if(IsBulitIn(alreadyAddedDescriptor.GetType()))
                    {
                        //Overwrite built-in component descriptor
                        m_componentDescriptors[descriptor.ComponentType] = descriptor;
                    }
                    else if(!IsBulitIn(descriptor.GetType()))
                    {
                        Debug.LogWarningFormat("Duplicate descriptor for {0} found. Type name {1}. Using {2} instead", descriptor.ComponentType.FullName, descriptor.GetType().FullName, m_componentDescriptors[descriptor.ComponentType].GetType().FullName);
                    }
                }
                else
                {
                    m_componentDescriptors.Add(descriptor.ComponentType, descriptor);
                }
            }

            LoadMap();
        }

        private bool IsBulitIn(Type type)
        {
            return type.GetCustomAttribute<BuiltInDescriptorAttribute>(false) != null;
        }

        private void Start()
        {
            GameObject voidComponentEditor = new GameObject("VoidComponentEditor");
            voidComponentEditor.transform.SetParent(transform, false);
            VoidComponentEditor = voidComponentEditor.AddComponent<VoidComponentEditor>();
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IEditorsMap>(this);
        }

        public PropertyDescriptor[] GetPropertyDescriptors(Type componentType, ComponentEditor componentEditor = null, object converter = null)
        {
            ComponentEditor editor = componentEditor != null ? componentEditor : VoidComponentEditor;

            IComponentDescriptor componentDescriptor;
            if (!ComponentDescriptors.TryGetValue(componentType, out componentDescriptor))
            {
                componentDescriptor = null;
            }

            if (componentDescriptor != null)
            {
                if (converter == null)
                {
                    converter = componentDescriptor.CreateConverter(editor);
                }

                PropertyDescriptor[] properties = componentDescriptor.GetProperties(editor, converter);
                return properties;
            }
            else
            {
                if (componentType.IsScript())
                {
                    FieldInfo[] serializableFields = componentType.GetSerializableFields(false);
                    return serializableFields.Select(f => new PropertyDescriptor(f.Name, editor.Components, f, f)).ToArray();
                }
                else
                {
                    PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead && p.CanWrite).ToArray();
                    return properties.Select(p => new PropertyDescriptor(p.Name, editor.Components, p, p)).ToArray();
                }
            }
        }

        private class DefaultEditorsMapCreator : IEditorsMapCreator
        {
            void IEditorsMapCreator.Create(IEditorsMap map)
            {
                map.AddMapping(typeof(GameObject), 0, true, false);
                map.AddMapping(typeof(object), 1, true, true);
                map.AddMapping(typeof(UnityEngine.Object), 2, true, true);
                map.AddMapping(typeof(bool), 3, true, true);
                map.AddMapping(typeof(Enum), 4, true, true);
                map.AddMapping(typeof(List<>), 5, true, true);
                map.AddMapping(typeof(Array), 6, true, true);
                map.AddMapping(typeof(string), 7, true, true);
                map.AddMapping(typeof(int), 8, true, true);
                map.AddMapping(typeof(float), 9, true, true);
                map.AddMapping(typeof(Range), 10, true, true);
                map.AddMapping(typeof(Vector2), 11, true, true);
                map.AddMapping(typeof(Vector3), 12, true, true);
                map.AddMapping(typeof(Vector4), 13, true, true);
                map.AddMapping(typeof(Quaternion), 14, true, true);
                map.AddMapping(typeof(Color), 15, true, true);
                map.AddMapping(typeof(Bounds), 16, true, true);
                map.AddMapping(typeof(RangeInt), 17, true, true);
                map.AddMapping(typeof(RangeOptions), 18, true, true);
                map.AddMapping(typeof(HeaderText), 19, true, true);
                map.AddMapping(typeof(MethodInfo), 20, true, true);
                map.AddMapping(typeof(Component), 21, true, false);
                map.AddMapping(typeof(BoxCollider), 22, true, false);
                map.AddMapping(typeof(Camera), 21, true, false);
                map.AddMapping(typeof(CapsuleCollider), 22, true, false);
                map.AddMapping(typeof(FixedJoint), 21, true, false);
                map.AddMapping(typeof(HingeJoint), 21, true, false);
                map.AddMapping(typeof(Light), 21, true, false);
                map.AddMapping(typeof(MeshCollider), 21, true, false);
                map.AddMapping(typeof(MeshFilter), 21, true, false);
                map.AddMapping(typeof(MeshRenderer), 21, true, false);
                map.AddMapping(typeof(MonoBehaviour), 21, false, false);
                map.AddMapping(typeof(Rigidbody), 21, true, false);
                map.AddMapping(typeof(SkinnedMeshRenderer), 21, true, false);
                map.AddMapping(typeof(Skybox), 21, true, false);
                map.AddMapping(typeof(SphereCollider), 22, true, false);
                map.AddMapping(typeof(SpringJoint), 21, true, false);
                map.AddMapping(typeof(Transform), 23, true, false);
                map.AddMapping(typeof(RuntimeAnimation), 21, true, false);
                map.AddMapping(typeof(AudioSource), 21, true, false);
                map.AddMapping(typeof(AudioListener), 21, true, false);
                map.AddMapping(typeof(LayersInfo), 24, true, false);
                map.AddMapping(typeof(RangeFlags), 25, true, true);
                map.AddMapping(typeof(LayerMask), 26, true, true);
                map.AddMapping(typeof(RectTransform), 27, true, false);
                map.AddMapping(typeof(BoolFloat), 30, true, true);
                map.AddMapping(typeof(LayoutElement), 21, true, false);
                map.AddMapping(typeof(HorizontalLayoutGroup), 21, true, false);
                map.AddMapping(typeof(VerticalLayoutGroup), 21, true, false);
                map.AddMapping(typeof(GridLayoutGroup), 21, true, false);
                map.AddMapping(typeof(PersistentCall), 28, true, true);
                map.AddMapping(typeof(UnityEventBase), 29, true, true);
                map.AddMapping(typeof(Text), 21, true, false);
                map.AddMapping(typeof(Image), 21, true, false);
                map.AddMapping(typeof(Button), 21, true, false);
                map.AddMapping(typeof(Toggle), 21, true, false);
                map.AddMapping(typeof(ToggleGroup), 21, true, false);
                map.AddMapping(typeof(Canvas), 21, true, false);
                map.AddMapping(typeof(TextMeshProUGUI), 21, true, false);
            }
        }

        public void LoadMap()
        {
            if (m_isLoaded)
            {
                return;
            }
            m_isLoaded = true;

            IEditorsMapCreator editorsMapCreator = IOC.Resolve<IEditorsMapCreator>();
            if(editorsMapCreator == null)
            {
                editorsMapCreator = new DefaultEditorsMapCreator();
            }
            editorsMapCreator.Create(this);

            EditorsMapStorage editorsMap = Resources.Load<EditorsMapStorage>(EditorsMapStorage.EditorsMapPrefabName);
            if (editorsMap == null)
            {
                editorsMap = Resources.Load<EditorsMapStorage>(EditorsMapStorage.EditorsMapTemplateName);
            }
            if (editorsMap != null)
            {
                m_editors = editorsMap.Editors;

                for (int i = 0; i < editorsMap.MaterialEditors.Length; ++i)
                {
                    GameObject materialEditor = editorsMap.MaterialEditors[i];
                    Shader shader = editorsMap.Shaders[i];
                    bool enabled = editorsMap.IsMaterialEditorEnabled[i];
                    if (!m_materialMap.ContainsKey(shader))
                    {
                        m_materialMap.Add(shader, new MaterialEditorDescriptor(materialEditor, enabled));
                    }
                    m_defaultMaterialEditor = editorsMap.DefaultMaterialEditor;
                }
            }
            else
            {
                Debug.LogError("Editors map is null");
            }
        }

        public void RegisterEditor(ComponentEditor editor)
        {
            Array.Resize(ref m_editors, m_editors.Length + 1);
            m_editors[m_editors.Length - 1] = editor.gameObject;
        }

        public void RegisterEditor(PropertyEditor editor)
        {
            Array.Resize(ref m_editors, m_editors.Length + 1);
            m_editors[m_editors.Length - 1] = editor.gameObject;
        }

        public bool HasMapping(Type type)
        {
            return m_map.ContainsKey(type);
        }

        public void AddMapping(Type type, int editorIndex, bool enabled, bool isPropertyEditor)
        {
            m_map.Add(type, new EditorDescriptor(editorIndex, enabled, isPropertyEditor));
        }

        public void AddMapping(Type type, Type editorType, bool enabled, bool isPropertyEditor)
        {
            GameObject editor = m_editors.Where(ed => ed.GetComponents<Component>().Any(c => c.GetType() == editorType)).FirstOrDefault();
            if (editor == null)
            {
                throw new ArgumentException("editorType");
            }

            AddMapping(type, editor, enabled, isPropertyEditor);
        }

        public void RemoveMapping(Type type)
        {
            m_map.Remove(type);
        }

        public void AddMapping(Type type, GameObject editor, bool enabled, bool isPropertyEditor)
        {
            int index = Array.IndexOf(m_editors, editor);
            if (index < 0)
            {
                Array.Resize(ref m_editors, m_editors.Length + 1);
                index = m_editors.Length - 1;
                m_editors[index] = editor;
            }

            if(!m_map.ContainsKey(type))
            {
                m_map.Add(type, new EditorDescriptor(index, enabled, isPropertyEditor));
            }
        }

        public bool IsObjectEditorEnabled(Type type)
        {
            return IsEditorEnabled(type, false, true);
        }

        public bool IsPropertyEditorEnabled(Type type, bool strict = false)
        {
            return IsEditorEnabled(type, true, strict);
        }

        private bool IsEditorEnabled(Type type, bool isPropertyEditor, bool strict)
        {
            EditorDescriptor descriptor = GetEditorDescriptor(type, isPropertyEditor, strict);
            if (descriptor != null)
            {
                return descriptor.Enabled;
            }
            return false;
        }

        public bool IsMaterialEditorEnabled(Shader shader)
        {
            MaterialEditorDescriptor descriptor = GetEditorDescriptor(shader);
            if (descriptor != null)
            {
                return descriptor.Enabled;
            }

            return false;
        }

        public GameObject GetObjectEditor(Type type, bool strict = false)
        {
            return GetEditor(type, false, strict);
        }

        public GameObject GetPropertyEditor(Type type, bool strict = false)
        {
            return GetEditor(type, true, strict);
        }

        private GameObject GetEditor(Type type, bool isPropertyEditor, bool strict = false)
        {
            EditorDescriptor descriptor = GetEditorDescriptor(type, isPropertyEditor, strict);
            if (descriptor != null && descriptor.Index < m_editors.Length)
            {
                return m_editors[descriptor.Index];
            }
            return null;
        }

        public GameObject GetMaterialEditor(Shader shader, bool strict = false)
        {
            MaterialEditorDescriptor descriptor = GetEditorDescriptor(shader);
            if (descriptor != null)
            {
                return descriptor.Editor;
            }

            if (strict)
            {
                return null;
            }

            return m_defaultMaterialEditor;
        }

        private MaterialEditorDescriptor GetEditorDescriptor(Shader shader)
        {
            MaterialEditorDescriptor descriptor;
            if (m_materialMap.TryGetValue(shader, out descriptor))
            {
                return m_materialMap[shader];
            }

            return null;
        }

        private EditorDescriptor GetEditorDescriptor(Type type, bool isPropertyEditor, bool strict)
        {
            if (type == typeof(MethodInfo))
            {
                EditorDescriptor descriptor;
                if (m_map.TryGetValue(type, out descriptor))
                {
                    return descriptor;
                }
                return null;
            }

            do
            {
                EditorDescriptor descriptor;
                if (m_map.TryGetValue(type, out descriptor))
                {
                    if (descriptor.IsPropertyEditor == isPropertyEditor)
                    {
                        return descriptor;
                    }
                }
                else
                {
                    if (type.IsGenericType)
                    {
                        if (m_map.TryGetValue(type.GetGenericTypeDefinition(), out descriptor))
                        {
                            if (descriptor.IsPropertyEditor == isPropertyEditor)
                            {
                                return descriptor;
                            }
                        }
                    }
                }

                if (strict)
                {
                    break;
                }

                type = type.BaseType();
            }
            while (type != null);
            return null;
        }

        public Type[] GetEditableTypes()
        {
            return m_map.Where(kvp => kvp.Value != null && kvp.Value.Enabled).Select(kvp => kvp.Key).ToArray();
        }
    }
}
