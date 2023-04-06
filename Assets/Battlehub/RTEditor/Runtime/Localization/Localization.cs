using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.Utils;

namespace Battlehub
{
    public static class ILocalizationExt
    {
        public static string GetString(this ILocalization localization, string key, string fallback = null)
        {
            if(localization == null)
            {
                if(fallback == null)
                {
                    return key;
                }
                return fallback;
            }

            string str = localization.GetString(key);
            if(str == null)
            {
                if(fallback == null)
                {
                    return key;
                }
                return fallback;
            }
            return str;
        }
    }

    public interface ILocalization
    {
        event PropertyChangedEventHandler<string> LocaleChanged;
        string Locale
        {
            get;
            set;
        }

        void LoadStringResources(string path);
        string GetString(string key);
    }

    [Serializable]
    public class StringResource
    {
        [XmlAttribute("id")]
        public string Id;
        [XmlAttribute("value")]
        public string Value;
    }

    [Serializable]
    public class StringResources
    {
        public StringResource[] Resources;
    }

    [DefaultExecutionOrder(-100)]
    public class Localization : MonoBehaviour, ILocalization
    {
        private Dictionary<string, string> m_idToString;

        [SerializeField]
        private List<string> m_stringResources = null;
        
        [SerializeField]
        private string m_locale = "en-US";

        public event PropertyChangedEventHandler<string> LocaleChanged;
        public string Locale
        {
            get { return m_locale; }
            set
            {
                if(m_locale != value)
                {
                    string oldValue = m_locale;
                    m_locale = value;
                    LoadResources();
                    LocaleChanged?.Invoke(this, oldValue, value);
                }
            }
        }

        private void Awake()
        {
            LoadResources();
            IOC.RegisterFallback<ILocalization>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<ILocalization>(this);
        }

        private void LoadResources()
        {
            m_idToString = new Dictionary<string, string>();

            string prefix = "." + m_locale;
            foreach (string res in m_stringResources)
            {
                TextAsset textAsset = Resources.Load<TextAsset>(res + prefix);
                if (textAsset == null)
                {
                    Debug.LogFormat("String resource file {0} was not found", res + prefix);
                    continue;
                }

                LoadStringResources(textAsset);
            }
        }

        public void LoadStringResources(string path)
        {
            string prefix = "." + m_locale;
            TextAsset textAsset = Resources.Load<TextAsset>(path + prefix);
            if (textAsset == null)
            {
                Debug.LogFormat("String resource file {0} was not found", path + prefix);
                return;
            }

            LoadStringResources(textAsset);
        }

        private void LoadStringResources(TextAsset textAsset)
        {
            try
            {
                StringResources stringResources = XmlUtility.FromXml<StringResources>(textAsset.text);
                foreach (StringResource stringResource in stringResources.Resources)
                {
                    if (m_idToString.ContainsKey(stringResource.Id))
                    {
                        string exisiting = m_idToString[stringResource.Id];
                        Debug.LogWarning("Duplicate resource found " + stringResource.Id + " " + exisiting + ". Duplicate: " + stringResource.Value);
                        continue;
                    }

                    m_idToString.Add(stringResource.Id, stringResource.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public string GetString(string key)
        {
            if(key == null)
            {
                return null;
            }

            key = key.Trim();
            string str;
            if(m_idToString.TryGetValue(key, out str))
            {
                return str;
            }

            return null;
        }
    }

}
