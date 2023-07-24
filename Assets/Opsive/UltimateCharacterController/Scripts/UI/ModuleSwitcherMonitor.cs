/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using System;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using UnityEngine;
    using UnityEngine.UI;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An Item Action Module Group Monitor is used to show the Modules that are active.
    /// </summary>
    public class ModuleSwitcherMonitor : MonoBehaviour
    {
        public Action<ModuleSwitcherMonitor,bool> OnSelect;

        [Tooltip("The module switcher ID that UI represents.")]
        [SerializeField] protected int m_ModuleSwitcherID;
        [Tooltip("A game object that is active while this monitor is selected.")]
        [SerializeField] protected GameObject m_EnabledWhileSelected;
        [Tooltip("The text displaying the switcher index name.")]
        [SerializeField] protected Text m_SwitcherIndexName;
        [Tooltip("The image displaying the switcher index icon.")]
        [SerializeField] protected Image m_SwitcherIndexIcon;

        private IModuleSwitcher m_ModuleSwitcher;
        protected bool m_IsSelected;
        public bool IsSelected => m_IsSelected;

        /// <summary>
        /// Can this monitor display the specified switcher.
        /// </summary>
        /// <param name="moduleSwitcher">The module switcher to check if it can be displayed by this monitor.</param>
        /// <returns>True if the specified module switcher can be displayed by this switcher.</returns>
        public virtual bool DoesSwitcherMatch(IModuleSwitcher moduleSwitcher)
        {
            return m_ModuleSwitcherID == moduleSwitcher.ID;
        }

        /// <summary>
        /// Set the switcher to bind to this monitor.
        /// </summary>
        /// <param name="moduleSwitcher">The module switcher to display.</param>
        public virtual void SetSwitcher(IModuleSwitcher moduleSwitcher)
        {
            if (m_ModuleSwitcher != null) {
                EventHandler.UnregisterEvent<int>(m_ModuleSwitcher.gameObject, "ModuleSwitcher_OnSwitch_Index", HandleOnSwitch);
            }
            
            m_ModuleSwitcher = moduleSwitcher;

            if (m_ModuleSwitcher != null) {
                EventHandler.RegisterEvent<int>(m_ModuleSwitcher.gameObject, "ModuleSwitcher_OnSwitch_Index", HandleOnSwitch);
            }
            
            gameObject.SetActive(moduleSwitcher != null);
           
            Refresh();
        }

        /// <summary>
        /// Handle the switcher changing index.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        private void HandleOnSwitch(int newIndex)
        {
            Refresh();
        }
        
        /// <summary>
        /// Switch to a specific index.
        /// </summary>
        /// <param name="index">The index to switch to.</param>
        public virtual void SwitchTo(int index)
        {
            m_ModuleSwitcher.SwitchTo(index);
            Refresh();
        }

        /// <summary>
        /// Switch to the previous index.
        /// </summary>
        public virtual void DoSwitchPrevious()
        {
            m_ModuleSwitcher.SwitchToPrevious();
            Refresh();
        }

        /// <summary>
        /// Switch to the next index.
        /// </summary>
        public virtual void DoSwitchNext()
        {
            m_ModuleSwitcher.SwitchToNext();
            Refresh();
        }

        /// <summary>
        /// Show the Select or Deselect state.
        /// </summary>
        /// <param name="select">Should this switcher be selected.</param>
        public virtual void Select(bool select)
        {
            m_IsSelected = select;
            m_EnabledWhileSelected.SetActive(m_IsSelected);
            Refresh();
            OnSelect?.Invoke(this, m_IsSelected);
        }

        /// <summary>
        /// Refresh the displayed UI.
        /// </summary>
        protected virtual void Refresh()
        {
            m_SwitcherIndexName.text = m_ModuleSwitcher?.GetIndexName() ?? "(null)";
            if (m_SwitcherIndexIcon != null) {
                m_SwitcherIndexIcon.sprite = m_ModuleSwitcher?.GetIndexIcon();
            }
        }
    }
}