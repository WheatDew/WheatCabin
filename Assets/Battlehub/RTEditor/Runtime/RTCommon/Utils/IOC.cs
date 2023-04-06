using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTCommon
{
    public class IOCContainer
    {
        private class Item
        {
            public object Instance;
            public MulticastDelegate Function;
            public Item(object instance)
            {
                Instance = instance;
            }

            public Item(MulticastDelegate function)
            {
                Function = function;
            }

            public T Resolve<T>()
            {
                if (Instance != null)
                {
                    return (T)Instance;
                }

                return ((Func<T>)Function)();
            }
        }

        private Dictionary<Type, Dictionary<string, Item>> m_named = new Dictionary<Type, Dictionary<string, Item>>();
        private Dictionary<Type, Item> m_registered = new Dictionary<Type, Item>();
        private Dictionary<Type, Item> m_fallbacks = new Dictionary<Type, Item>();

        public bool IsRegistered<T>(string name)
        {
            if (!m_named.TryGetValue(typeof(T), out Dictionary<string, Item> nameToItem))
            {
                return false;   
            }
            return nameToItem.ContainsKey(name);
        }

        public void Register<T>(string name, Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            Dictionary<string, Item> nameToItem;
            if (!m_named.TryGetValue(typeof(T), out nameToItem))
            {
                nameToItem = new Dictionary<string, Item>();
                m_named.Add(typeof(T), nameToItem);
            }

            if (nameToItem.ContainsKey(name))
            {
                Debug.LogWarningFormat("item with name {0} already registered", name);
            }

            nameToItem[name] = new Item(func);
        }

        public void Register<T>(string name, T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("func");
            }

            Dictionary<string, Item> nameToItem;
            if (!m_named.TryGetValue(typeof(T), out nameToItem))
            {
                nameToItem = new Dictionary<string, Item>();
                m_named.Add(typeof(T), nameToItem);
            }

            if (nameToItem.ContainsKey(name))
            {
                Debug.LogWarningFormat("item with name {0} already registered", name);
            }

            nameToItem[name] = new Item(instance);
        }

        public bool IsRegistered<T>()
        {
            return m_registered.ContainsKey(typeof(T));
        }

        public void Register<T>(Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (m_registered.ContainsKey(typeof(T)))
            {
                Debug.LogWarningFormat("type {0} already registered.", typeof(T).FullName);
            }

            m_registered[typeof(T)] = new Item(func);
        }

        public void Register<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (m_registered.ContainsKey(typeof(T)))
            {
                Debug.LogWarningFormat("type {0} already registered.", typeof(T).FullName);
            }

            m_registered[typeof(T)] = new Item(instance);
        }

        public void Unregister<T>(string name, Func<T> func)
        {
            Dictionary<string, Item> nameToItem;
            if (m_named.TryGetValue(typeof(T), out nameToItem))
            {
                Item item;
                if (nameToItem.TryGetValue(name, out item))
                {
                    if (item.Function != null && item.Function.Equals(func))
                    {
                        nameToItem.Remove(name);
                        if (nameToItem.Count == 0)
                        {
                            m_named.Remove(typeof(T));
                        }
                    }
                }
            }
        }

        public void Unregister<T>(string name, T instance)
        {
            Dictionary<string, Item> nameToItem;
            if (m_named.TryGetValue(typeof(T), out nameToItem))
            {
                Item item;
                if (nameToItem.TryGetValue(name, out item))
                {
                    if (ReferenceEquals(item.Instance, instance))
                    {
                        nameToItem.Remove(name);
                        if (nameToItem.Count == 0)
                        {
                            m_named.Remove(typeof(T));
                        }
                    }
                }
            }
        }

        public void Unregister<T>(Func<T> func)
        {
            Item item;
            if (m_registered.TryGetValue(typeof(T), out item))
            {
                if (item.Function != null && item.Function.Equals(func))
                {
                    m_registered.Remove(typeof(T));
                }
            }
        }

        public void Unregister<T>(T instance)
        {
            Item item;
            if (m_registered.TryGetValue(typeof(T), out item))
            {
                if (ReferenceEquals(item.Instance, instance))
                {
                    m_registered.Remove(typeof(T));
                }
            }
        }

        public void Unregister<T>()
        {
            m_registered.Remove(typeof(T));
        }

        public bool IsFallbackRegistered<T>()
        {
            return m_fallbacks.ContainsKey(typeof(T));
        }

        public void RegisterFallback<T>(Func<T> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (m_fallbacks.ContainsKey(typeof(T)))
            {
                Debug.LogWarningFormat("fallback for type {0} already registered.", typeof(T).FullName);
            }
            m_fallbacks[typeof(T)] = new Item(func);
        }

        public void RegisterFallback<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (m_fallbacks.ContainsKey(typeof(T)))
            {
                Debug.LogWarningFormat("type {0} already registered.", typeof(T).FullName);
            }

            m_fallbacks[typeof(T)] = new Item(instance);
        }

        public void UnregisterFallback<T>(Func<T> func)
        {
            Item item;
            if (m_fallbacks.TryGetValue(typeof(T), out item))
            {
                if (item.Function != null && item.Function.Equals(func))
                {
                    m_fallbacks.Remove(typeof(T));
                }
            }
        }

        public void UnregisterFallback<T>(T instance)
        {
            Item item;
            if (m_fallbacks.TryGetValue(typeof(T), out item))
            {
                if (ReferenceEquals(item.Instance, instance))
                {
                    m_fallbacks.Remove(typeof(T));
                }
            }
        }

        public void UnregisterFallback<T>()
        {
            m_fallbacks.Remove(typeof(T));
        }

        public T Resolve<T>(string name)
        {
            Dictionary<string, Item> nameToItem;
            if (m_named.TryGetValue(typeof(T), out nameToItem))
            {
                Item item;
                if (nameToItem.TryGetValue(name, out item))
                {
                    return item.Resolve<T>();
                }
            }
            return default(T);
        }

        public T Resolve<T>()
        {
            Item item;
            if (m_registered.TryGetValue(typeof(T), out item))
            {
                return item.Resolve<T>();
            }
            else
            {
                if (m_fallbacks.TryGetValue(typeof(T), out item))
                {
                    return item.Resolve<T>();
                }
            }
            return default(T);
        }

        public void Clear()
        {
            m_registered.Clear();
            m_fallbacks.Clear();
            m_named.Clear();
        }
    }

    /// <summary>
    /// Simple IoC container implementation.
    /// </summary>
    public static class IOC
    {
        /// <summary>
        /// Checks if a named dependency of type is registered.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="name">Name of dependency.</param>
        /// <returns>True if dependency of type with name registered.</returns>
        public static bool IsRegistered<T>(string name)
        {
            return m_container.IsRegistered<T>(name);
        }

        /// <summary>
        /// Registers named dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="name">Name of dependency.</param>
        /// <param name="func">Function which constructs object of type.</param>
        public static void Register<T>(string name, Func<T> func)
        {
            m_container.Register(name, func);
        }

        /// <summary>
        /// Registers named dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="name">Name of dependency.</param>
        /// <param name="instance">Instance of type.</param>
        public static void Register<T>(string name, T instance)
        {
            m_container.Register(name, instance);
        }

        /// <summary>
        /// Unregisters named dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="name">Name of dependency.</param>
        /// <param name="func">Function which was passed to Register method.</param>
        public static void Unregister<T>(string name, Func<T> func)
        {
            m_container.Unregister(name, func);
        }

        /// <summary>
        /// Unregisters named dependency of type
        /// </summary>
        /// <typeparam name="T">Type of dependency</typeparam>
        /// <param name="name">Name of dependency</param>
        /// <param name="instance">Instance which was passed to Register method.</param>
        public static void Unregister<T>(string name, T instance)
        {
            m_container.Unregister(name, instance);
        }

        /// <summary>
        /// Checks if a dependency of type is registered.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <returns>True if dependency of type with name registered.</returns>
        public static bool IsRegistered<T>()
        {
            return m_container.IsRegistered<T>();
        }

        /// <summary>
        /// Registers dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="func">Function which constructs object of type.</param>
        public static void Register<T>(Func<T> func)
        {
            m_container.Register(func);
        }

        /// <summary>
        /// Registers dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="instance">Instance of type.</param>
        public static void Register<T>(T instance)
        {
            m_container.Register(instance);
        }

        /// <summary>
        /// Unregisters dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="func">Function which was passed to Register method.</param>
        public static void Unregister<T>(Func<T> func)
        {
            m_container.Unregister(func);
        }

        /// <summary>
        /// Unregisters dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="instance">Instance which was passed to Register method.</param>
        public static void Unregister<T>(T instance)
        {
            m_container.Unregister(instance);
        }

        /// <summary>
        /// Unregisters dependency of type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Unregister<T>()
        {
            m_container.Unregister<T>();
        }

        /// <summary>
        ///  Checks if a fallback dependency of type is registered.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        /// <returns>True if fallback dependency of type with name registered.</returns>
        public static bool IsFallbackRegistered<T>()
        {
            return m_container.IsFallbackRegistered<T>();
        }

        /// <summary>
        /// Registers fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        /// <param name="func">Function which constructs object of type.</param>
        public static void RegisterFallback<T>(Func<T> func)
        {
            m_container.RegisterFallback(func);
        }

        /// <summary>
        /// Registers fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        /// <param name="instance">Instance of type.</param>
        public static void RegisterFallback<T>(T instance)
        {
            m_container.RegisterFallback(instance);
        }

        /// <summary>
        /// Unregisters fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        /// <param name="func">Function which was passed to RegisterFallback method.</param>
        public static void UnregisterFallback<T>(Func<T> func)
        {
            m_container.UnregisterFallback(func);
        }

        /// <summary>
        /// Unregisters fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        /// <param name="func">Instance which was passed to Register method.</param>
        public static void UnregisterFallback<T>(T instance)
        {
            m_container.UnregisterFallback(instance);
        }

        /// <summary>
        /// Unregisters fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of fallback dependency.</typeparam>
        public static void UnregisterFallback<T>()
        {
            m_container.UnregisterFallback<T>();
        }

        /// <summary>
        /// Resolves named dependency or fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <param name="name">Name of dependency.</param>
        /// <returns>Object of type (or null if dependency of type is not registered)</returns>
        public static T Resolve<T>(string name)
        {
            return m_container.Resolve<T>(name);
        }

        /// <summary>
        /// Resolves dependency or fallback dependency of type.
        /// </summary>
        /// <typeparam name="T">Type of dependency.</typeparam>
        /// <returns>Object of type (or null if dependency of type is not registered)</returns>
        public static T Resolve<T>()
        {
            return m_container.Resolve<T>();
        }

        private static IOCContainer m_container;

        static IOC()
        {
            m_container = new IOCContainer();
        }

        /// <summary>
        /// Clears container and removes all registered dependencies and fallbacks
        /// </summary>
        public static void ClearAll()
        {
            m_container.Clear();
        }
    }


}

