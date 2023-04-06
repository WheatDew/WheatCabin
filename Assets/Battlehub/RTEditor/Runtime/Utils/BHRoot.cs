using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub
{
    public class KnownAssemblies
    {
        private static string[] m_assemblyNames = new[] { "Assembly-CSharp" };
        public static string[] Names
        {
            get { return m_assemblyNames; }
        }

        public static void Add(string assemblyName)
        {
            if(Array.IndexOf(m_assemblyNames, assemblyName) < 0)
            {
                int len = m_assemblyNames.Length;
                Array.Resize(ref m_assemblyNames, len + 1);
                m_assemblyNames[len] = assemblyName;
            }
        }
    }

    internal class BHRoot : BHRoot<BHRoot> 
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RegisterAssembly()
        {
            KnownAssemblies.Add("Battlehub.RTEditor");
        }
    }

    public class BHRoot<T> : ScriptableObject where T : BHRoot<T>
    {
#if UNITY_EDITOR
        private static string m_packagePath;
        public static string PackagePath
        {
            get
            {
                if(string.IsNullOrEmpty(m_packagePath))
                {
                    T script = CreateInstance<T>();
                    
                    MonoScript monoScript = MonoScript.FromScriptableObject(script);
                    m_packagePath = AssetDatabase.GetAssetPath(monoScript);
                    m_packagePath = Path.GetDirectoryName(m_packagePath);
                    m_packagePath = Path.GetDirectoryName(m_packagePath);
                    m_packagePath = Path.GetDirectoryName(m_packagePath);

                    DestroyImmediate(script);
                }

                return m_packagePath;
            }
        }

        public static string PackageRuntimeContentPath
        {
            get
            {
                string packagePath = PackagePath;
                return Path.Combine(Path.Combine(packagePath, "Content"), "Runtime");
            }
        }

        public static string PackageEditorContentPath
        {
            get 
            {
                string packagePath = PackagePath;
                return Path.Combine(Path.Combine(packagePath, "Content"), "Editor");
            }
        }

        public static string AssetsPath
        {
            get
            {
                return "Assets/Battlehub";
            }
        }
#endif
    }
}
