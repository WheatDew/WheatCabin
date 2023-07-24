/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a starting window with useful links.
    /// </summary>
    [OrderedEditorItem("Welcome", 0)]
    public class WelcomeScreenManager : Manager
    {
        private const string c_DocumentationTextureGUID = "4ed35e016803fb7408050e41a75f1d93";
        private const string c_VideosTextureGUID = "0697e4652b4c4a040a26a775a24847d1";
        private const string c_IntegrationsTextureGUID = "753cf93668e9fc9449c32306d3136a19";
        private const string c_ForumTextureGUID = "231c482ec921d5d469d0039bc18dbfda";
        private const string c_DiscordTextureGUID = "c234ca342a2cb274c94da04b6db74730";
        private const string c_ReviewTextureGUID = "8af2dbe9f9221bf4882aee4d8375f7d1";
        private const string c_ShowcaseTextureGUID = "7723bcc8e25ee3443a9954c35ac939ec";

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var centeredContent = new VisualElement();
            centeredContent.style.alignSelf = Align.Center;
            centeredContent.style.flexGrow = 1;

            var welcomeLabel = new Label();
            welcomeLabel.text = string.Format("Thank you for purchasing the {0}.\nThe resources below will help you get the most out of the controller.",
                                            UltimateCharacterController.Utility.AssetInfo.Name);
            welcomeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            centeredContent.Add(welcomeLabel);

            // Documentation.
            var imageElement = new Image();
            imageElement.AddToClassList(ManagerStyles.LinkCursor);
            imageElement.style.marginTop = 10f;
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(c_DocumentationTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL("https://opsive.com/support/documentation/ultimate-character-controller/");
            });
            centeredContent.Add(imageElement);

            // Videos and Integrations.
            AddHorizontalImages(centeredContent, c_VideosTextureGUID, GetVideosURL(),
                                c_IntegrationsTextureGUID, IntegrationsManager.GetIntegrationLink());
            // Forum and Discord.
            AddHorizontalImages(centeredContent, c_ForumTextureGUID, "https://opsive.com/forum/",
                                c_DiscordTextureGUID, "https://discord.gg/QX6VFgc");
            // Review and Showcase.
            AddHorizontalImages(centeredContent, c_ReviewTextureGUID, GetAssetURL(),
                                c_ShowcaseTextureGUID, "https://opsive.com/showcase/");
            m_ManagerContentContainer.Add(centeredContent);

            // Version number at the bottom.
            var version = new Label();
            version.text = string.Format("Ultimate Character Controller version {0}.", UltimateCharacterController.Utility.AssetInfo.Version);
            version.style.paddingLeft = 2;
            version.style.paddingBottom = 2;
            m_ManagerContentContainer.Add(version);
        }

        /// <summary>
        /// Adds two images stacked beside each other.
        /// </summary>
        /// <param name="parent">The VisualElement that the content should be added to.</param>
        /// <param name="leftTextureGUID">The GUID for the left image.</param>
        /// <param name="leftURL">The URL for the left image.</param>
        /// <param name="rightTextureGUID">The GUID for the right image.</param>
        /// <param name="rightURL">The URL for the right image.</param>
        private void AddHorizontalImages(VisualElement parent, string leftTextureGUID, string leftURL, string rightTextureGUID, string rightURL)
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.style.flexDirection = FlexDirection.Row;
            horizontalLayout.style.flexGrow = 0;
            horizontalLayout.style.alignItems = Align.Center;
            horizontalLayout.style.alignSelf = Align.Center;
            horizontalLayout.style.marginTop = 2;
            var imageElement = new Image();
            imageElement.AddToClassList(ManagerStyles.LinkCursor);
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(leftTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL(leftURL);
            });
            horizontalLayout.Add(imageElement);

            imageElement = new Image();
            imageElement.style.marginLeft = 2;
            imageElement.AddToClassList(ManagerStyles.LinkCursor);
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(rightTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL(rightURL);
            });
            horizontalLayout.Add(imageElement);

            parent.Add(horizontalLayout);
        }

        /// <summary>
        /// Returns the URL for the videos page.
        /// </summary>
        /// <returns>The URL for the videos page.</returns>
        private string GetVideosURL()
        {
            switch (UltimateCharacterController.Utility.AssetInfo.Name) {
                case "Ultimate Character Controller":
                    return "https://opsive.com/videos/?pid=923";
                case "Ultimate First Person Shooter":
                    return "https://opsive.com/videos/?pid=185";
                case "Third Person Controller":
                    return "https://opsive.com/videos/?pid=926";
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the URL for the asset page.
        /// </summary>
        /// <returns>The URL for the asset page.</returns>
        private string GetAssetURL()
        {
            switch (UltimateCharacterController.Utility.AssetInfo.Name) {
                case "Ultimate Character Controller":
                    return "https://assetstore.unity.com/packages/slug/233710";
                case "Ultimate First Person Shooter":
                    return "https://assetstore.unity.com/packages/slug/233711";
                case "Third Person Controller":
                    return "https://assetstore.unity.com/packages/slug/233712";
            }
            return string.Empty;
        }
    }
}