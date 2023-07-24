/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.UIElements.Managers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// An editor window which allows for a single location to update all of the conflicted references on the character.
    /// </summary>
    public class ReferenceResolverWindow : EditorWindow
    {
        /// <summary>
        /// Represents an object that needs to be updated.
        /// </summary>
        public class ConflictingObjects
        {
            [Tooltip("The owning object that is in a conflicted state.")]
            public object Object;
            [Tooltip("The field that is in a conflicted state.")]
            public FieldInfo Field;
            [Tooltip("The class path to the field.")]
            public string Path;
            [Tooltip("Event that is invoked when the value is set.")]
            public System.Action<object> OnChangeEvent;

            /// <summary>
            /// Returns the value of the object.
            /// </summary>
            /// <returns>The value of the object.</returns>
            public object GetValue() { return Field.GetValue(Object); }

            /// <summary>
            /// Sets a new value.
            /// </summary>
            /// <param name="value">The new object value.</param>
            public void SetValue(object value)
            { 
                Field.SetValue(Object, value);
                OnChangeEvent(Object);
            }
        }

        private List<ConflictingObjects> m_ConflictingObjects;
        private Transform m_TemplateTransform;
        private Action m_OnClose;
        private static Dictionary<Transform, HumanBodyBones> s_TemplateBoneByTransform;

        private bool m_Close;

        /// <summary>
        /// The window should be closed so it displays the accurate fields.
        /// </summary>
        public void OnDisable()
        {
            m_Close = true;
        }

        /// <summary>
        /// Closes the window after the project has been compiled.
        /// </summary>
        public void OnEnable()
        {
            if (m_Close) {
                EditorApplication.update += CloseWindow;
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void CloseWindow()
        {
            EditorApplication.update -= CloseWindow;

            if (m_OnClose != null) {
                m_OnClose();
            }

            Close();
        }

        /// <summary>
        /// Initializes the window.
        /// </summary>
        public void Initialize(List<ConflictingObjects> conflictingObjects, GameObject templateObject, string objectType, Action onClose = null)
        {
            m_ConflictingObjects = conflictingObjects;
            m_TemplateTransform = templateObject.transform;
            m_OnClose = onClose;

            rootVisualElement.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0"));//shared uss
            rootVisualElement.styleSheets.Add(ManagerStyles.StyleSheet);

            BuildVisualElements(objectType);
        }

        /// <summary>
        /// Builds the Visual Elements for the window.
        /// </summary>
        private void BuildVisualElements(string objectType)
        {
            rootVisualElement.Clear();

            if (m_ConflictingObjects == null || m_ConflictingObjects.Count == 0) {
                rootVisualElement.Add(new HelpBox("No fields need resolving.", HelpBoxMessageType.Info));
                return;
            }


            var scrollView = new ScrollView();
            rootVisualElement.Add(scrollView);
            
            var instructionsLabel = new Label($"The Reference Resolver tried to update all of the field references to the new {objectType}. " +
                                            "The fields below are not able to be automatically updated. Please manually update these fields.");
            instructionsLabel.style.whiteSpace = WhiteSpace.Normal;
            scrollView.Add(instructionsLabel);

            for (int i = 0; i < m_ConflictingObjects.Count; ++i) {
                // Apply a special formatting to display the class path.
                var pathSplit = m_ConflictingObjects[i].Path.Split('.');
                var headerText = m_ConflictingObjects[i].Path.Substring(0, m_ConflictingObjects[i].Path.Length - pathSplit[pathSplit.Length - 1].Length - 1);
                headerText = headerText.Replace("m_", "").Replace(" ", "").Replace("[]", "").Replace("+", ".");

                var headerLabel = new Label(headerText);
                headerLabel.tooltip = headerText;
                headerLabel.AddToClassList("header-text");
                headerLabel.style.marginTop = 4;
                scrollView.Add(headerLabel);

                var helpBox = new HelpBox($"The destination is a child of the template {objectType}.", HelpBoxMessageType.Warning);
                if (typeof(IList).IsAssignableFrom(m_ConflictingObjects[i].Field.FieldType)) {
                    var listValue = (IList)m_ConflictingObjects[i].GetValue();
                    var index = i;
                    for (int j = 0; j < listValue.Count; ++j) {
                        var elementValue = (UnityEngine.Object)listValue[j];
                        if (!IsValueTemplateChild(elementValue)) {
                            continue;
                        }

                        var elementIndex = j;
                        var destinationObjectField = new ObjectField(ObjectNames.NicifyVariableName(pathSplit[pathSplit.Length - 1]) + $" (Element {elementIndex})");
                        destinationObjectField.objectType = m_ConflictingObjects[i].Field.FieldType.GetElementType();
                        destinationObjectField.value = elementValue;
                        destinationObjectField.RegisterValueChangedCallback(c =>
                        {
                            listValue[elementIndex] = c.newValue;
                            m_ConflictingObjects[index].SetValue(listValue);
                            helpBox.style.display = IsValueTemplateChild(destinationObjectField.value) ? DisplayStyle.Flex : DisplayStyle.None;
                        });
                        destinationObjectField.AddToClassList("flex-grow");
                        scrollView.Add(destinationObjectField);

                        helpBox.style.display = IsValueTemplateChild(destinationObjectField.value) ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                } else {
                    var destinationObjectField = new ObjectField(ObjectNames.NicifyVariableName(pathSplit[pathSplit.Length - 1]));
                    destinationObjectField.objectType = m_ConflictingObjects[i].Field.FieldType;
                    destinationObjectField.value = (UnityEngine.Object)m_ConflictingObjects[i].GetValue();
                    var index = i;
                    destinationObjectField.RegisterValueChangedCallback(c =>
                    {
                        m_ConflictingObjects[index].SetValue(c.newValue);
                        helpBox.style.display = IsValueTemplateChild(destinationObjectField.value) ? DisplayStyle.Flex : DisplayStyle.None;
                    });
                    destinationObjectField.AddToClassList("flex-grow");
                    scrollView.Add(destinationObjectField);

                    helpBox.style.display = IsValueTemplateChild(destinationObjectField.value) ? DisplayStyle.Flex : DisplayStyle.None;
                }

                scrollView.Add(helpBox);
            }

            var closeButton = new Button();
            closeButton.style.marginTop = 10;
            closeButton.text = "Close";
            closeButton.clicked += () =>
            {
                CloseWindow();
            };
            rootVisualElement.Add(closeButton);
        }

        /// <summary>
        /// Is the value a child of the template object?
        /// </summary>
        /// <param name="value">The value of the object.</param>
        /// <returns>True if the value is a child of the template character.</returns>
        private bool IsValueTemplateChild(object value)
        {
            if (value == null || value.Equals(null)) {
                return false;
            }
            if (value is Component) {
                value = (value as Component).transform;
            } else if (value is GameObject) {
                value = (value as GameObject).transform;
            } else {
                return false;
            }

            return (value as Transform).IsChildOf(m_TemplateTransform) || (value as Transform) == m_TemplateTransform;
        }

        /// <summary>
        /// Traverses through all of the fields and resolves the field value the new character.
        /// </summary>
        public static void ResolveFields<T>(GameObject templateParent, GameObject targetParent, Type animatorIdentifier, object templateObject, T targetObject, List<ReferenceResolverWindow.ConflictingObjects> conflictingObjects)
        {
            if (s_TemplateBoneByTransform == null) {
                s_TemplateBoneByTransform = new Dictionary<Transform, HumanBodyBones>();
            } else {
                s_TemplateBoneByTransform.Clear();
            }

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var fields = templateObject.GetType().GetFields(bindingFlags);
            for (int i = 0; i < fields.Length; ++i) {
                if (!fields[i].IsPublic && fields[i].GetCustomAttribute<SerializeField>() == null && fields[i].GetCustomAttribute<SerializeReference>() == null) {
                    continue;
                }
                if (fields[i].GetCustomAttribute<Shared.Utility.IgnoreReferences>() != null) {
                    continue;
                }

                var index = i;
                Action<object> onChangeEvent = (object changedValue) =>
                {
                    if (!fields[index].FieldType.IsAssignableFrom(changedValue.GetType())) {
                        return;
                    }
                    fields[index].SetValue(targetObject, changedValue);
                    if (targetObject is UnityEngine.Object) {
                        Utility.EditorUtility.SetDirty(targetObject as UnityEngine.Object);
                    }
                };

                bool resolved;
                var value = ResolveFields(templateParent, targetParent, animatorIdentifier, fields[i].GetValue(templateObject), fields[i].GetValue(targetObject), fields[i].FieldType, targetObject.GetType().Name + "." + fields[i].Name, conflictingObjects, onChangeEvent, out resolved);
                if (!resolved) {
                    conflictingObjects.Add(new ReferenceResolverWindow.ConflictingObjects {
                        Object = targetObject,
                        Field = fields[i],
                        Path = targetObject.GetType().Name + "." + fields[i].Name,
                        OnChangeEvent = onChangeEvent
                    });
                }

                fields[i].SetValue(targetObject, value);
            }
        }

        /// <summary>
        /// Returns the resolved value on the new object.
        /// </summary>
        private static object ResolveFields(GameObject templateParent, GameObject targetParent, Type animatorIdentifier, object templateValue, object targetValue, Type fieldType, string path, List<ReferenceResolverWindow.ConflictingObjects> conflictingObjects, Action<object> onChangeEvent, out bool resolved)
        {
            // Value is being retrieved from reflection. Due to the unmanaged/managed Unity Object conversion != and the Equals method must be used to truly check for null.
            if (templateValue == null || templateValue.Equals(null) || typeof(ScriptableObject).IsAssignableFrom(fieldType) || 
                                typeof(Sprite).IsAssignableFrom(fieldType) || typeof(Material).IsAssignableFrom(fieldType) || typeof(AudioClip).IsAssignableFrom(fieldType)) {
                resolved = true;
                return templateValue;
            }

            // If the object is a prefab that is not part of the template then the object is resolved.
            if (typeof(GameObject).IsAssignableFrom(fieldType) && EditorUtility.IsPersistent((UnityEngine.Object)templateValue)) {
                if (!((GameObject)templateValue).transform.IsChildOf(templateParent.transform)) {
                    resolved = true;
                    return templateValue;
                }
            }

            // The default parameter assignment type may be different.
            if (templateValue.GetType().IsClass && targetValue != null && templateValue.GetType() != targetValue.GetType()) {
                targetValue = Activator.CreateInstance(templateValue.GetType());
            }

            // If the transform is a child of the parent then that value should persist.
            Transform targetTransform = null;
            if (targetValue != null && !targetValue.Equals(null)) {
                if (targetValue is Component) {
                    targetTransform = (targetValue as Component).transform;
                } else if (targetValue is GameObject) {
                    targetTransform = (targetValue as GameObject).transform;
                }
            }
            if (targetTransform != null) {
                if (targetTransform.IsChildOf(targetParent.transform)) {
                    resolved = true;
                    return targetValue;
                }
            }

            // Ensure each array element is resolved.
            if (typeof(IList).IsAssignableFrom(fieldType)) {
                Type elementType;
                if (fieldType.IsArray) {
                    elementType = fieldType.GetElementType();
                } else {
                    var baseFieldType = fieldType;
                    while (!baseFieldType.IsGenericType) {
                        baseFieldType = baseFieldType.BaseType;
                    }
                    elementType = baseFieldType.GetGenericArguments()[0];
                }
                var templateList = (IList)templateValue;
                var targetList = (IList)targetValue;
                if (templateList != null && (targetList == null || templateList.Count != targetList.Count)) {
                    // Deep copy the list so the values will not reference the same object.
                    if (fieldType.IsArray) {
                        targetList = Array.CreateInstance(elementType, templateList.Count);
                    } else if (fieldType.IsGenericType) {
                        targetList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), new object[] { templateList }) as IList;
                    } else {
                        targetList = Activator.CreateInstance(fieldType, new object[] { templateList }) as IList;
                    }
                    var originalTargetList = (IList)targetValue;
                    if (originalTargetList != null) {
                        for (int i = 0; i < originalTargetList.Count; ++i) {
                            if (i >= targetList.Count) {
                                break;
                            }

                            targetList[i] = originalTargetList[i];
                        }
                    }
                }

                // Ensure the list types match.
                if (templateList != null && targetList != null && templateList.Count == targetList.Count) {
                    for (int i = 0; i < targetList.Count; ++i) {
                        if (templateList[i] == null || templateList[i].Equals(null)) {
                            continue;
                        }
                        if (templateList[i] is ScriptableObject || templateList[i] is Sprite || templateList[i] is Material || templateList[i] is AudioClip) {
                            continue;
                        }
                        if (targetList[i] == null || templateList[i].GetType() != targetList[i].GetType()) {
                            if (templateList[i].GetType().IsPrimitive || templateList[i].GetType().IsValueType || templateList[i] is string) {
                                targetList[i] = templateList[i];
                            } else {
                                targetList[i] = Activator.CreateInstance(templateList[i].GetType(), true);
                            }
                        }
                    }
                }

                // Resolve the element.
                resolved = true;
                if (targetList != null) {
                    for (int i = 0; i < targetList.Count; ++i) {
                        var index = i;
                        targetList[i] = ResolveFields(templateParent, targetParent, animatorIdentifier, templateList[i], targetList[i], templateList[i] != null ? templateList[i].GetType() : elementType, $"{path}.Element{i}", conflictingObjects, (object changedValue) =>
                        {
                            targetList[index] = changedValue;
                            onChangeEvent(targetList);
                        }, out resolved);
                    }
                }
                return targetList;
            }

            // The strings should always be the template value.
            if (fieldType == typeof(string)) {
                resolved = true;
                return new string((string)templateValue);
            }

            // Nested objects must also be resolved.
            if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive)) { // Classes and structs.
                var targetObj = fieldType.IsPrimitive ? templateValue : targetValue;
                if (targetObj == null && !typeof(UnityEngine.Object).IsAssignableFrom(fieldType) && fieldType != typeof(string) && !(fieldType.IsValueType && !fieldType.IsPrimitive) && !fieldType.IsAbstract) {
                    targetObj = Activator.CreateInstance(fieldType, true);
                }

                var fields = fieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < fields.Length; ++i) {
                    if (!fields[i].IsPublic && fields[i].GetCustomAttribute<SerializeField>() == null && fields[i].GetCustomAttribute<SerializeReference>() == null) {
                        continue;
                    }
                    if (fields[i].GetCustomAttribute<Shared.Utility.IgnoreReferences>() != null) {
                        continue;
                    }

                    bool objResolved;
                    var objValue = fields[i].GetValue(templateValue);
                    var index = i;
                    var resolvedValue = ResolveFields(templateParent, targetParent, animatorIdentifier, objValue, fields[i].GetValue(targetObj), objValue != null ? objValue.GetType() : fields[i].FieldType, $"{path}.{fields[i].Name}", conflictingObjects,
                            (object changedValue) =>
                            {
                                fields[index].SetValue(targetObj, changedValue);
                                onChangeEvent(targetObj);
                            }, out objResolved);
                    fields[i].SetValue(targetObj, resolvedValue);
                    if (!objResolved) {
                        conflictingObjects.Add(new ReferenceResolverWindow.ConflictingObjects {
                            Object = targetObj,
                            Field = fields[i],
                            Path = $"{path}.{fields[i].Name}",
                            OnChangeEvent = onChangeEvent
                        });
                    }
                }
                if (targetObj != null && !targetObj.Equals(null)) {
                    templateValue = targetObj;
                }
            }

            if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) {
                resolved = true;
                return templateValue;
            }

            // If the value is a humanoid transform referencing an animator bone then replace that bone with the new character bone.
            resolved = false;
            Transform templateTransform = null;
            if (templateValue != null) {
                if (templateValue is Component) {
                    templateTransform = (templateValue as Component).transform;
                } else if (templateValue is GameObject) {
                    templateTransform = (templateValue as GameObject).transform;
                }
            }
            if (templateTransform != null) {
                if (templateTransform.IsChildOf(templateParent.transform)) {
                    var humanoidBone = GetHumanoidBoneMapping(templateParent, targetParent, animatorIdentifier, templateTransform);
                    if (humanoidBone != null) {
                        if (templateValue is Component) {
                            targetValue = humanoidBone.GetComponent(templateValue.GetType());
                            // Add the new component if the object is a Collider.
                            if ((targetValue == null || targetValue.Equals(null)) && templateValue is Collider) {
                                targetValue = humanoidBone.gameObject.AddComponent(templateValue.GetType());
                                EditorUtility.CopySerialized(templateValue as Component, targetValue as Component);
                            }
                        } else { // GameObject.
                            targetValue = humanoidBone.gameObject;
                        }
                        resolved = true;
                        return targetValue;
                    }
                }
            }

            // Special case for the player input proxy.
            if (templateValue is Shared.Input.PlayerInput) {
                templateValue = targetParent.GetComponentInChildren(templateValue.GetType());
                resolved = templateValue != null;
            }

            return templateValue;
        }

        /// <summary>
        /// Returns the target transform based on the template transform bone.
        /// </summary>
        private static Transform GetHumanoidBoneMapping(GameObject templateParent, GameObject targetParent, Type animatorIdentifier, Transform transform)
        {
            // Build the mapping based off of the template target if it hasn't already been built.
            var templateAnimatorIdentifier = templateParent.GetComponentInChildren(animatorIdentifier);
            if (s_TemplateBoneByTransform.Count == 0) {
                if (templateAnimatorIdentifier != null) {
                    var templateAnimator = templateAnimatorIdentifier.GetComponent<Animator>();
                    if (templateAnimator != null) {
                        for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i) {
                            var templateBone = templateAnimator.GetBoneTransform((HumanBodyBones)i);
                            if (templateBone != null) {
                                s_TemplateBoneByTransform.Add(templateBone, (HumanBodyBones)i);
                            }
                        }
                    }
                }
            }

            // Try to convert the template transform to the target transform based on the humanoid bone.
            var targetAnimatorIdentifier = targetParent.GetComponentInChildren(animatorIdentifier);
            if (s_TemplateBoneByTransform.TryGetValue(transform, out var bone)) {
                if (targetAnimatorIdentifier != null) {
                    var targetAnimator = targetAnimatorIdentifier.GetComponent<Animator>();
                    if (targetAnimator != null) {
                        return targetAnimator.GetBoneTransform(bone);
                    }
                }
            }


            // The target object may be the animator GameObject itself.
            if (templateAnimatorIdentifier != null && targetAnimatorIdentifier != null) {
                var templateAnimator = templateAnimatorIdentifier.GetComponent<Animator>();
                var targetAnimator = targetAnimatorIdentifier.GetComponent<Animator>();
                if (templateAnimator != null && targetAnimator != null) {
                    if (templateAnimator.gameObject == transform.gameObject) {
                        return targetAnimator.transform;
                    }
                }
            }
            return null;
        }
    }
}