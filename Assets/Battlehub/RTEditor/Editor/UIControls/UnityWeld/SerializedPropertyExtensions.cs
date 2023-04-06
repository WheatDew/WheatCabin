using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Battlehub.UIControls.Binding
{
    public static class SerializedPropertyExtensions
    {
        public static T GetSerializedValue<T>(this SerializedProperty property)
        {
            object @object = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                @object = @object.GetType()
                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(@object);
            }

            if (@object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                string propertyPath = property.propertyPath;
                int b0Index = propertyPath.IndexOf('[') + 1;
                int b1Index = propertyPath.IndexOf(']');

                int propertyIndex = int.Parse(propertyPath.Substring(b0Index, b1Index - b0Index));
                return ((IList<T>)@object)[propertyIndex];
            }
            else return (T)@object;
        }
    }
}
