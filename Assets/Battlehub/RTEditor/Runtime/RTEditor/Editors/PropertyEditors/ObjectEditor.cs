using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public interface IObjectEditorLoader
    {
        void Load(object obj, Type memberInfoType, Action<UnityObject> callback);
        Type GetObjectType(object obj, Type memberInfoType);
    }


    public class ObjectEditor : PropertyEditor<UnityObject>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject DragHighlight = null;
        [SerializeField]
        private TMP_InputField Input = null;
        [SerializeField]
        private Button BtnSelect = null;
        [SerializeField]
        private TMP_Dropdown TypeSelector = null;

        private IEditorsMap m_editorsMap;
        private IObjectEditorLoader m_loader;
        private ILocalization m_localization;
        private Type[] m_allowedTypes = new Type[0];

        protected override void SetInputField(UnityObject value)
        {
            if (HasMixedValues())
            {
                Input.text = null;
            }
            else
            {
                HideTypeSelector();
                m_allowedTypes = new Type[0];

                Type memberInfoType = GetMemberType();
                bool isDestroyed = value == null;
                if (!isDestroyed)
                {
                    GameObject go = null;
                    if (value is GameObject)
                    {
                        go = (GameObject)value;
                    }
                    else if (value is Component)
                    {
                        go = ((Component)value).gameObject;
                    }

                    if (go != null)
                    {
                        ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                        if (exposeToEditor != null && exposeToEditor.MarkAsDestroyed)
                        {
                            isDestroyed = true;
                        }
                        else
                        {
                            memberInfoType = value.GetType();
                            m_allowedTypes = GetAllowedTypes();
                            UpdateTypeSelectorState();
                        }
                    }
                }

                string memberInfoTypeName = m_localization.GetString("ID_RTEditor_PE_TypeName_" + memberInfoType.Name, memberInfoType.Name);
                if (isDestroyed)
                {
                    Input.text = string.Format(m_localization.GetString("ID_RTEditor_PE_ObjectEditor_None", "None") + " ({0})", memberInfoTypeName);
                }
                else
                {
                    Input.text = string.Format("{1} ({0})", memberInfoTypeName, value.name);
                }
            }  
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            UnityEventHelper.AddListener(BtnSelect, btn => btn.onClick, OnSelect);
            UnityEventHelper.AddListener(TypeSelector, dropdown => dropdown.onValueChanged, OnTypeSelected);

            m_editorsMap = IOC.Resolve<IEditorsMap>();
            m_localization = IOC.Resolve<ILocalization>();
            m_loader = IOC.Resolve<IObjectEditorLoader>();
            if(m_loader == null)
            {
                m_loader = Editor.Root.gameObject.AddComponent<ObjectEditorLoader>();
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            UnityEventHelper.RemoveListener(BtnSelect, btn => btn.onClick, OnSelect);
            UnityEventHelper.RemoveListener(TypeSelector, dropdown => dropdown.onValueChanged, OnTypeSelected);

            if (Editor != null)
            {
                Editor.DragDrop.Drop -= OnDrop;
            }
        }
        
        private void OnSelect()
        {
            ISelectObjectDialog objectSelector = null;

            IWindowManager wm = IOC.Resolve<IWindowManager>();

            Type memberInfoType = GetMemberType();
            string memberInfoTypeName = m_localization.GetString("ID_RTEditor_PE_TypeName_" + memberInfoType.Name, memberInfoType.Name);
            string select = m_localization.GetString("ID_RTEditor_PE_ObjectEditor_Select", "Select") + " ";
            if (wm.IsWindowRegistered("Select" + memberInfoType.Name))
            {
                Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow("Select" + memberInfoType.Name, select + memberInfoTypeName,
                      (sender, args) =>
                      {
                          if (objectSelector.IsNoneSelected)
                          {
                              SetObject(null);
                          }
                          else
                          {
                              SetObject(objectSelector.SelectedObject);
                          }
                      });
            }
            else
            {
                Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(RuntimeWindowType.SelectObject.ToString(), select + memberInfoTypeName,
                    (sender, args) =>
                    {
                        if (objectSelector.IsNoneSelected)
                        {
                            SetObject(null);
                        }
                        else
                        {
                            SetObject(objectSelector.SelectedObject);
                        }
                    });
            }
            
            objectSelector = IOC.Resolve<ISelectObjectDialog>();
            objectSelector.ObjectType = GetMemberType();
        }

        private void OnTypeSelected(int index)
        {
            Type type = m_allowedTypes[index];
            if(type == typeof(GameObject))
            {
                UnityObject obj = GetValue();
                if(obj is Component)
                {
                    Component component = (Component)obj;
                    SetObject(component.gameObject);
                }
            }
            else 
            {
                UnityObject obj = GetValue();
                GameObject go;
                if(obj is GameObject)
                {
                    go = (GameObject)obj;
                }
                else
                {
                    Component component = (Component)obj;
                    go = component.gameObject;
                }

                SetObject(go.GetComponent(type));
            }
        }

        public void SetObject(UnityObject obj)
        {
            BeginEdit();
            SetValue(obj);
            EndEdit();
            SetInputField(obj);
        }


        private void OnDrop(PointerEventData pointerEventData)
        {
            object dragObject = Editor.DragDrop.DragObjects[0];

            Type memberInfoType = GetMemberType();

            Editor.IsBusy = true;
            m_loader.Load(dragObject, memberInfoType, loadedObject =>
            {
                Editor.IsBusy = false;
                SetObject(loadedObject);
                HideDragHighlight();
            });
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(!Editor.DragDrop.InProgress)
            {
                return;
            }
            object dragObject = Editor.DragDrop.DragObjects[0];

            Type memberInfoType = GetMemberType();

            Type type = m_loader.GetObjectType(dragObject, memberInfoType);
           
            if (type != null && memberInfoType.IsAssignableFrom(type))
            {
                Editor.DragDrop.Drop -= OnDrop;
                Editor.DragDrop.Drop += OnDrop;
                ShowDragHighlight();
                Editor.DragDrop.SetCursor(Utils.KnownCursor.DropAllowed);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Editor.DragDrop.Drop -= OnDrop;
            if(Editor.DragDrop.InProgress)
            {
                Editor.DragDrop.SetCursor(Utils.KnownCursor.DropNotAllowed);
                HideDragHighlight();
            }
        }

        private void ShowDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(true);
            }
        }

        private void HideDragHighlight()
        {
            if(DragHighlight != null)
            {
                DragHighlight.SetActive(false);
            }
        }

        private Type GetMemberType()
        {
            Type memberInfoType = MemberInfoType;
            if (Accessor is CustomTypeFieldAccessor)
            {
                CustomTypeFieldAccessor accessor = (CustomTypeFieldAccessor)Accessor;
                memberInfoType = accessor.Type;
            }
            
            return memberInfoType;
        }

        private void UpdateTypeSelectorState()
        {
            if(TypeSelector == null)
            {
                return;
            }

            if (m_allowedTypes.Length > 1)
            {
                TypeSelector.options = m_allowedTypes.Select(t => new TMP_Dropdown.OptionData(m_localization.GetString("ID_RTEditor_PE_TypeName_" + t.Name, t.Name))).ToList();
                TypeSelector.SetValueWithoutNotify(Array.IndexOf(m_allowedTypes, GetValue().GetType()));
                TypeSelector.gameObject.SetActive(true);
            }
            else
            {
                HideTypeSelector();
            }
        }

        private void HideTypeSelector()
        {
            TypeSelector.gameObject.SetActive(false);
            TypeSelector.ClearOptions();
        }

        private Type[] GetAllowedTypes()
        {
            Type memberType = GetMemberType();
            UnityObject obj = GetValue();
            HashSet<Type> types = new HashSet<Type>();
            GameObject go = null;
            if(obj is Component)
            {
                Component component = (Component)obj;
                go = component.gameObject;
            }
            else if(obj is GameObject)
            {
                go = (GameObject)obj;
            }
            else
            {
                types.Add(memberType);
            }

            if (go != null)
            {
                if (memberType != typeof(GameObject))
                {
                    Component[] components;
                    if (memberType.IsSubclassOf(typeof(Component)))
                    {
                        components = go.GetComponents(memberType);
                    }
                    else
                    {
                        types.Add(typeof(GameObject));
                        components = go.GetComponents<Component>(); 
                    }

                    for (int i = 0; i < components.Length; ++i)
                    {
                        Component component = components[i];
                        Type componentType = component.GetType();
                        if (m_editorsMap.IsObjectEditorEnabled(componentType) && componentType.IsSubclassOf(memberType))
                        {
                            types.Add(componentType);
                        }
                    }
                }
                else
                {
                    types.Add(typeof(GameObject));
                }
            }

            return types.ToArray();
        }
    }
}
