using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IEditorsMap
    {
        Dictionary<Type, IComponentDescriptor> ComponentDescriptors
        {
            get;
        }

        PropertyDescriptor[] GetPropertyDescriptors(Type componentType, ComponentEditor componentEditor = null, object converter = null);

        void RegisterEditor(ComponentEditor editor);
        void RegisterEditor(PropertyEditor editor);
        bool HasMapping(Type type);

        void AddMapping(Type type, int editorIndex, bool enabled, bool isPropertyEditor);
        void AddMapping(Type type, Type editorType, bool enabled, bool isPropertyEditor);
        void AddMapping(Type type, GameObject editor, bool enabled, bool isPropertyEditor);
        void RemoveMapping(Type type);
        

        bool IsObjectEditorEnabled(Type type);
        bool IsPropertyEditorEnabled(Type type, bool strict = false);
        bool IsMaterialEditorEnabled(Shader shader);
        GameObject GetObjectEditor(Type type, bool strict = false);
        GameObject GetPropertyEditor(Type type, bool strict = false);
        GameObject GetMaterialEditor(Shader shader, bool strict = false);
        Type[] GetEditableTypes();
    }
}

