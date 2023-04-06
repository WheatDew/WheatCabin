using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    internal static class GameObjectEditorUtils 
    {
        public class GameObjectWrapper
        {
            private GameObject m_gameObject;

            public bool IsActive
            {
                get
                {
                    if (!m_gameObject)
                    {
                        return false;
                    }

                    return m_gameObject.activeSelf;
                }
                set { m_gameObject.SetActive(value); }
            }

            public GameObjectWrapper(GameObject gameObject)
            {
                m_gameObject = gameObject;
            }
        }

        /// <summary>
        /// Get object name
        /// </summary>
        /// <param name="objects">objects</param>
        /// <returns>The name of the first object, if all objects have the same name. Otherwise returns null</returns>
        internal static string GetObjectName(UnityObject[] objects)
        {
            string name = objects[0].name;
            for (int i = 1; i < objects.Length; ++i)
            {
                UnityObject go = objects[i];
                if (go == null)
                {
                    continue;
                }

                if (go.name != name)
                {
                    return null;
                }
            }
            return name;
        }

        /// <summary>
        /// Check if component valid to be represented by component editor
        /// </summary>
        /// <param name="component">component</param>
        /// <param name="ignoreComponents">list of components which must be ignored</param>
        /// <returns>true if component is valid</returns>
        internal static bool IsComponentValid(Component component, HashSet<Component> ignoreComponents)
        {
            if (component == null)
            {
                return false;
            }

            return !ignoreComponents.Contains(component) && (component.hideFlags & HideFlags.HideInInspector) == 0;
        }


        /// <summary>
        /// Find intersection of components of game objects
        /// </summary>
        /// <param name="gameObjects">game objects</param>
        /// <returns>list of groups of components with the same type</returns>
        internal static List<List<Component>> GetComponentGroups(GameObject[] gameObjects)
        {
            List<List<Component>> groups = new List<List<Component>>();
            List<List<Component>> allComponents = new List<List<Component>>();
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];

                HashSet<Component> ignoreComponents = IgnoreComponents(go);
                allComponents.Add(go.GetComponents<Component>().Where(component => IsComponentValid(component, ignoreComponents)).ToList());
            }

            List<Component> primaryList = allComponents[0];
            for (int i = 0; i < primaryList.Count; ++i)
            {
                Component primary = primaryList[i];
                Type primaryType = primary.GetType();

                List<Component> group = new List<Component>();
                group.Add(primary);

                for (int j = 1; j < allComponents.Count; ++j)
                {
                    List<Component> secondaryList = allComponents[j];
                    if (secondaryList.Count == 0)
                    {
                        //one of the lists is exhausted -> break outer loop
                        i = primaryList.Count;
                        group = null;
                        break;
                    }

                    //find component of type
                    for (int k = 0; k < secondaryList.Count; k++)
                    {
                        Component secondary = secondaryList[k];
                        if (primaryType == secondary.GetType())
                        {
                            group.Add(secondary);
                            secondaryList.RemoveAt(k);
                            break;
                        }
                    }

                    if (group.Count != j + 1)
                    {
                        //not all game objects have a component with the same type
                        group = null;
                        break;
                    }
                }

                if (group != null)
                {
                    //all game objects have a component with the same type
                    groups.Add(group);
                }
            }

            return groups;
        }

        internal static HashSet<Component> IgnoreComponents(GameObject go)
        {
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            HashSet<Component> ignoreComponents = new HashSet<Component>();
            if (exposeToEditor != null)
            {
                if (exposeToEditor.Colliders != null)
                {
                    for (int i = 0; i < exposeToEditor.Colliders.Length; ++i)
                    {
                        Collider collider = exposeToEditor.Colliders[i];
                        if (!ignoreComponents.Contains(collider))
                        {
                            ignoreComponents.Add(collider);
                        }
                    }
                }

                ignoreComponents.Add(exposeToEditor);
            }

            return ignoreComponents;
        }

        internal static ComponentEditor CreateComponentEditor(Transform componentsPanel, IList<Component> components)
        {
            if (components.Count == 0)
            {
                return null;
            }

            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            Component component = components[0];
            Type componentType = component.GetType();
            if (editorsMap.IsObjectEditorEnabled(componentType))
            {
                GameObject editorPrefab = editorsMap.GetObjectEditor(componentType);
                if (editorPrefab != null)
                {
                    ComponentEditor componentEditorPrefab = editorPrefab.GetComponent<ComponentEditor>();
                    if (componentEditorPrefab != null)
                    {
                        ComponentEditor editor = UnityObject.Instantiate(componentEditorPrefab);
                        editor.name = componentEditorPrefab.name;
                        editor.EndEditCallback = () =>
                        {
                            UpdatePreviews(components);
                        };
                        editor.transform.SetParent(componentsPanel, false);
                        editor.Components = components.ToArray();
                        return editor;
                    }
                    else
                    {
                        Debug.LogErrorFormat("editor prefab {0} does not have ComponentEditor script", editorPrefab.name);
                        return null;
                    }
                }
            }

            return null;
        }

        private static void UpdatePreviews(IList<Component> components)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            for (int i = 0; i < components.Count; ++i)
            {
                Component component = components[i];
                if (component != null && component.gameObject != null)
                {
                    editor.UpdatePreviewAsync(component.gameObject);
                }
            }
            editor.IsDirty = true;
        }

        internal static void EndEditName(string name, IList<GameObject> gameObjects)
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();

            var nameProperty = Strong.MemberInfo((ExposeToEditor x) => x.Name);
            var goNameProperty = Strong.MemberInfo((GameObject x) => x.name);

            for (int i = 0; i < gameObjects.Count; ++i)
            {
                GameObject go = gameObjects[i];
                if (go == null)
                {
                    continue;
                }

                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null)
                {
                    editor.Undo.BeginRecordValue(exposeToEditor, nameProperty);
                    exposeToEditor.SetName(name, true);
                    editor.Undo.EndRecordValue(exposeToEditor, nameProperty);
                }
                else
                {
                    editor.Undo.BeginRecordValue(go, goNameProperty);
                    go.name = name;
                    editor.Undo.EndRecordValue(go, goNameProperty);
                }
            }

            editor.Undo.EndRecord();
        }
    }
}
