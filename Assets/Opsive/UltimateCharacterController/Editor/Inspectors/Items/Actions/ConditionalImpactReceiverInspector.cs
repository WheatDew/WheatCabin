/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using UnityEditor;

    [CustomEditor(typeof(ConditionalImpactReceiver), true)]
    public class ConditionalImpactReceiverInspector : UIElementsInspector
    {
        protected ConditionalImpactReceiver m_ConditionalImpactReceiver;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ConditionalImpactReceiver = target as ConditionalImpactReceiver;
            base.InitializeInspector();
        }
    }
}