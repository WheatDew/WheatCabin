using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.Utils
{
    internal static class ReflectionUtils
    {
        public static void GetUOAssembliesAndTypes(out Assembly[] assemblies, out Type[] types)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(
                a => !a.FullName.Contains("UnityEditor") 
                  && !a.FullName.Contains("Assembly-CSharp-Editor")
                  && !a.FullName.Contains("UnityWeld")).OrderBy(a => a.FullName).ToArray();

            List<Type> allUOTypes = new List<Type>();
            List<Assembly> assembliesList = new List<Assembly>();

            for (int i = 0; i < assemblies.Length; ++i)
            {
                Assembly assembly = assemblies[i];
                if (assembly.FullName.StartsWith("RTSLTypeModel"))
                {
                    continue;
                }
                try
                {
                    Type[] uoTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(UnityEngine.Object)) && t.FullName != typeof(BHRoot).FullName && !t.IsGenericType && t.IsPublic).ToArray();
                    if (uoTypes.Length > 0)
                    {
                        assembliesList.Add(assembly);
                        allUOTypes.AddRange(uoTypes);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to process :" + assembly.FullName + Environment.NewLine + e.ToString());
                }
            }

            types = allUOTypes.OrderByDescending(t => t.FullName.Contains("UnityEngine")).ToArray();
            assemblies = new Assembly[] { null }.Union(assembliesList.OrderBy(a => a.FullName)).ToArray();
        }
    }

}
