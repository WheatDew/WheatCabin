/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
/// 
namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Draws the inspector for an integrations that has been installed.
    /// </summary>
    public abstract class IntegrationInspector
    {
        protected MainManagerWindow m_MainManagerWindow;
        public MainManagerWindow MainManagerWindow { set { m_MainManagerWindow = value; } }

        /// <summary>
        /// Draws the integration inspector.
        /// </summary>
        /// <summary>
        /// <param name="container">The parent VisualElement container.</param>
        public abstract void ShowIntegration(VisualElement container);
    }

    /// <summary>
    /// Draws a list of all of the available integrations.
    /// </summary>
    [OrderedEditorItem("Integrations", 10)]
    public class IntegrationsManager : Manager
    {
        private string[] m_ToolbarStrings = { "Integration Inspectors", "Available Integrations" };
        [SerializeField] private bool m_ShowInstalledIntegrations;

        private IntegrationInspector[] m_IntegrationInspectors;
        private string[] m_IntegrationNames;

        private VisualElement m_InstalledContainer;
        private VisualElement m_AvailableContainer;

        /// <summary>
        /// Stores the information about the integration asset.
        /// </summary>
        private class AssetIntegration
        {
            private const int c_IconSize = 78;

            private int m_ID;
            private string m_Name;
            private string m_IntegrationURL;
            private Texture2D m_Icon;

            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;

            private VisualElement m_Container;

            /// <summary>
            /// Constructor for the AssetIntegration class.
            /// </summary>
            public AssetIntegration(int id, string name, string iconURL, string integrationURL)
            {
                m_ID = id;
                m_Name = name;
                m_IntegrationURL = integrationURL;

                // Start loading the icon as soon as the url is retrieved.
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();

                EditorApplication.update += WaitForIconWebRequest;
            }

            /// <summary>
            /// Retrieves the icon for the integration.
            /// </summary>
            private void WaitForIconWebRequest()
            {
                if (m_IconRequest.isDone) {
                    if (string.IsNullOrEmpty(m_IconRequest.error)) {
                        m_Icon = m_TextureDownloadHandler.texture;
                    }
                    m_IconRequest = null;
                    ShowIntegration(null);

                    EditorApplication.update -= WaitForIconWebRequest;
                }
            }

            /// <summary>
            /// Draws the integration details at the specified position.
            /// </summary>
            /// <param name="container">The parent container.</param>
            public void ShowIntegration(VisualElement container)
            {
                if (container != null) {
                    m_Container = container;
                } else {
                    m_Container.Clear();
                }

                // Draw the icon, name, and integration/Asset Store link.
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                horizontalLayout.style.marginTop = 8;
                horizontalLayout.style.marginLeft = 5;
                horizontalLayout.style.marginBottom = 8;
                horizontalLayout.style.marginRight = 5;
                m_Container.Add(horizontalLayout);

                if (m_Icon != null) {
                    var iconImage = new Image();
                    iconImage.image = m_Icon;
                    iconImage.style.flexShrink = 0;
                    iconImage.style.width = iconImage.style.height = c_IconSize;
                    horizontalLayout.Add(iconImage);
                }

                var verticalLayout = new VisualElement();
                verticalLayout.style.marginLeft = 5;
                horizontalLayout.Add(verticalLayout);

                var nameLabel = new Label(m_Name);
                nameLabel.AddToClassList("large-title");
                verticalLayout.Add(nameLabel);

                var buttonHorizontalLayout = new VisualElement();
                buttonHorizontalLayout.AddToClassList("horizontal-layout");
                buttonHorizontalLayout.style.flexGrow = 0;
                verticalLayout.Add(buttonHorizontalLayout);

                if (!string.IsNullOrEmpty(m_IntegrationURL)) {
                    var overviewButton = new Button();
                    overviewButton.text = "Integration";
                    overviewButton.style.width = 120;
                    buttonHorizontalLayout.Add(overviewButton);

                    overviewButton.clicked += () =>
                    {
                        Application.OpenURL(m_IntegrationURL);
                    };
                }

                if (m_ID > 0) {
                    var assetStoreButton = new Button();
                    assetStoreButton.text = "Asset Store";
                    assetStoreButton.style.width = 120;
                    buttonHorizontalLayout.Add(assetStoreButton);

                    assetStoreButton.clicked += () =>
                    {
                        Application.OpenURL("https://opsive.com/asset/UltimateCharacterController/AssetRedirect.php?asset=" + m_ID);
                    };
                }
            }
        }

        private UnityEngine.Networking.UnityWebRequest m_IntegrationsReqest;
        private AssetIntegration[] m_Integrations;

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            BuildInstalledIntegrations();

            m_ShowInstalledIntegrations = m_Integrations != null && m_Integrations.Length > 0;
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed integrations.
        /// </summary>
        private void BuildInstalledIntegrations()
        {
            var integrationInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var integrationIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement IntegrationInspector.
                    if (!typeof(IntegrationInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    integrationInspectors.Add(assemblyTypes[j]);
                    var index = integrationIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    integrationIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_IntegrationInspectors != null && m_IntegrationInspectors.Length == integrationInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = integrationInspectors.ToArray();
            Array.Sort(integrationIndexes.ToArray(), inspectorTypes);

            m_IntegrationInspectors = new IntegrationInspector[integrationInspectors.Count];
            m_IntegrationNames = new string[integrationInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_IntegrationInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as IntegrationInspector;
                m_IntegrationInspectors[i].MainManagerWindow = m_MainManagerWindow;

                var name = ObjectNames.NicifyVariableName(inspectorTypes[i].Name);
                if (integrationInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_IntegrationNames[i] = name;
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            m_InstalledContainer = new VisualElement();
            m_AvailableContainer = new VisualElement();

            var tabToolbar = new TabToolbar(m_ToolbarStrings, m_ShowInstalledIntegrations ? 0 : 1, (int selected) =>
            {
                m_ShowInstalledIntegrations = selected == 0;
                if (m_ShowInstalledIntegrations) {
                    m_AvailableContainer.Clear();
                    ShowInstalledIntegrations();
                } else {
                    m_InstalledContainer.Clear();
                    ShowAvailableIntegrations();
                }
            }, true);
            m_ManagerContentContainer.Add(tabToolbar);

            var scrollView = new ScrollView();
            scrollView.Add(m_InstalledContainer);
            scrollView.Add(m_AvailableContainer);
            m_ManagerContentContainer.Add(scrollView);

            if (m_ShowInstalledIntegrations) {
                ShowInstalledIntegrations();
            } else {
                ShowAvailableIntegrations();
            }
        }

        /// <summary>
        /// Draws the inspector for all installed integrations.
        /// </summary>
        private void ShowInstalledIntegrations()
        {
            m_InstalledContainer.Clear();

            if (m_IntegrationInspectors == null || m_IntegrationInspectors.Length == 0) {
                var helpBox = new HelpBox("No integrations installed use a custom inspector.\n\nSelect the \"Available Integrations\" tab to see a list of all of the available integrations.", HelpBoxMessageType.Info);
                m_InstalledContainer.Add(helpBox);
                return;
            }

            for (int i = 0; i < m_IntegrationInspectors.Length; ++i) {
                var nameLabel = new Label(m_IntegrationNames[i]);
                nameLabel.AddToClassList("large-title");
                m_InstalledContainer.Add(nameLabel);
                m_IntegrationInspectors[i].ShowIntegration(m_InstalledContainer);
            }
        }

        /// <summary>
        /// Draws all of the integrations that are currently available.
        /// </summary>
        private void ShowAvailableIntegrations()
        {
            m_AvailableContainer.Clear();

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.marginTop = 5;
            m_AvailableContainer.Add(horizontalLayout);

            // UIToolkit does not support links as of Unity 2021.3.
            var startLinkLabel = new Label("Integrations can also be found on ");
            horizontalLayout.Add(startLinkLabel);
            var linkConfigLabel = new Label(string.Format("<color={0}>this page</color>", EditorGUIUtility.isProSkin ? "#00aeff" : "#0000ee"));
            linkConfigLabel.RegisterCallback<ClickEvent>(c =>
            {
                Application.OpenURL(GetIntegrationLink());
            });
            linkConfigLabel.enableRichText = true;
            linkConfigLabel.AddToClassList("hyperlink");
            horizontalLayout.Add(linkConfigLabel);
            var endClickLabel = new Label(".");
            endClickLabel.style.marginLeft = -3;
            horizontalLayout.Add(endClickLabel);

            if (m_Integrations == null && m_IntegrationsReqest == null) {
                m_IntegrationsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateCharacterController/Version3IntegrationsList.txt");
                m_IntegrationsReqest.SendWebRequest();
                EditorApplication.update += WaitForIntegrationWebRequest;
            }

            // Draw the integrations once they are loaded.
            if (m_Integrations != null && m_Integrations.Length > 0) {
                for (int i = 0; i < m_Integrations.Length; ++i) {
                    var container = new VisualElement();
                    m_Integrations[i].ShowIntegration(container);
                    m_AvailableContainer.Add(container);
                }
            } else {
                if (m_IntegrationsReqest != null && m_IntegrationsReqest.isDone && !string.IsNullOrEmpty(m_IntegrationsReqest.error)) {
                    var helpbox = new HelpBox("Error: Unable to retrieve integrations.", HelpBoxMessageType.Error);
                    m_AvailableContainer.Add(helpbox);
                } else {
                    var helpbox = new HelpBox("Retrieveing the list of current integrations...", HelpBoxMessageType.Info);
                    m_AvailableContainer.Add(helpbox);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of available integrations.
        /// </summary>
        private void WaitForIntegrationWebRequest()
        {
            if (m_Integrations == null && m_IntegrationsReqest.isDone && string.IsNullOrEmpty(m_IntegrationsReqest.error)) {
                var splitIntegrations = m_IntegrationsReqest.downloadHandler.text.Split('\n');
                m_Integrations = new AssetIntegration[splitIntegrations.Length];
                var count = 0;
                for (int i = 0; i < splitIntegrations.Length; ++i) {
                    if (string.IsNullOrEmpty(splitIntegrations[i])) {
                        continue;
                    }

                    // The data must contain info on the integration name, id, icon, and integraiton url.
                    var integrationData = splitIntegrations[i].Split(',');
                    if (integrationData.Length < 4) {
                        continue;
                    }

                    m_Integrations[count] = new AssetIntegration(int.Parse(integrationData[0].Trim()), integrationData[1].Trim(), integrationData[2].Trim(), integrationData[3].Trim());
                    count++;
                }

                if (count != m_Integrations.Length) {
                    System.Array.Resize(ref m_Integrations, count);
                }
                m_IntegrationsReqest = null;
                if (!m_ShowInstalledIntegrations) {
                    ShowAvailableIntegrations();
                }

                EditorApplication.update -= WaitForIntegrationWebRequest;
            }
        }

        /// <summary>
        /// Returns the integration link for the current asset.
        /// </summary>
        /// <returns>The integration link for the current asset.</returns>
        public static string GetIntegrationLink()
        {
#pragma warning disable 0162
            return "https://opsive.com/downloads/";
#pragma warning restore 0162
        }
    }
}