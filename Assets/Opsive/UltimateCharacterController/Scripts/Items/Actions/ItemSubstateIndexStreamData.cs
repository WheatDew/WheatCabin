/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The item substate Index data contains the index and the priority of the substate.
    /// </summary>
    [Serializable]
    public struct ItemSubstateIndexData {
        
        [Tooltip("The Should the index be added to the stream?.")]
        [SerializeField] private bool m_Additive;
        [Tooltip("The substate index.")]
        [SerializeField] private int m_Index;
        [Tooltip("The priority of that substate over others.")]
        [SerializeField] private int m_Priority;

        public bool Additive => m_Additive;
        public int Priority => m_Priority;

        public int Index => m_Index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The substate index.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="additive">Should the index be added to the existing index?</param>
        public ItemSubstateIndexData(int index, int priority, bool additive = false)
        {
            m_Index = index;
            m_Priority = priority;
            m_Additive = additive;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The substate index.</param>
        /// <param name="other">The other substate index to copy.</param>
        public ItemSubstateIndexData(int index, ItemSubstateIndexData other)
        {
            m_Index = index;
            m_Priority = other.Priority;
            m_Additive = other.Additive;
        }
        
        /// <summary>
        /// Easy to read string.
        /// </summary>
        /// <returns>Easy to read string.</returns>
        public override string ToString()
        {
            return $"<(Priority:{m_Priority});(Index:{m_Index});(Additive:{m_Additive})>";
        }
    }
    
    /// <summary>
    /// The item substate Index module data contains the priority and the module to which it is attached.
    /// </summary>
    [Serializable]
    public struct ItemSubstateIndexModuleData {
        [Tooltip("The substate index data.")]
        [SerializeField] private ItemSubstateIndexData m_SubstateIndexData;
        [Tooltip("The module attached to that substate index.")]
        [HideInInspector] [NonSerialized] private ActionModule m_Module;

        public int Priority => m_SubstateIndexData.Priority;

        public int SubstateIndex => m_SubstateIndexData.Index;
        public ItemSubstateIndexData SubstateIndexData => m_SubstateIndexData;
        public ActionModule Module => m_Module;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="other">The other substate index module to copy.</param>
        public ItemSubstateIndexModuleData(ActionModule module, ItemSubstateIndexModuleData other)
        {
            m_SubstateIndexData = other.SubstateIndexData;
            m_Module = module;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The substate index.</param>
        /// <param name="other">The other substate index module to copy.</param>
        public ItemSubstateIndexModuleData(int index, ItemSubstateIndexModuleData other)
        {
            m_SubstateIndexData = new ItemSubstateIndexData(index, other.SubstateIndexData);
            m_Module = other.Module;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="substateIndex">The substate index.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="module">The module.</param>
        public ItemSubstateIndexModuleData(int substateIndex, int priority, ActionModule module)
        {
            m_SubstateIndexData = new ItemSubstateIndexData(substateIndex, priority);
            m_Module = module;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="substateIndexData">The substate index data.</param>
        /// <param name="module">The module.</param>
        public ItemSubstateIndexModuleData(ActionModule module,ItemSubstateIndexData substateIndexData)
        {
            m_SubstateIndexData = substateIndexData;
            m_Module = module;
        }
        
        /// <summary>
        /// Easy to read string.
        /// </summary>
        /// <returns>Easy to read string.</returns>
        public override string ToString()
        {
            return $"[{m_Module};{m_SubstateIndexData}]";
        }
    }
    
    /// <summary>
    /// The item substate index stream data contains a list of all the item substate index module data. Very useful for debugging and combining substates together.
    /// </summary>
    public class ItemSubstateIndexStreamData
    {
        protected ItemSubstateIndexModuleData m_Data;
        protected List<ItemSubstateIndexModuleData> m_SubstateIndexModuleDataList;

        public int SubstateIndex => m_Data.SubstateIndex;
        public int Priority => m_Data.Priority;
        public ItemSubstateIndexModuleData Data => m_Data;
        public IReadOnlyList<ItemSubstateIndexModuleData> SubstateIndexModuleDataList => m_SubstateIndexModuleDataList;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ItemSubstateIndexStreamData()
        {
            m_SubstateIndexModuleDataList = new List<ItemSubstateIndexModuleData>();
            m_Data = new ItemSubstateIndexModuleData(-1, -1, null);
        }

        /// <summary>
        /// Clear the data such that the object can be reused.
        /// </summary>
        public void Clear()
        {
            m_SubstateIndexModuleDataList.Clear();
            m_Data = new ItemSubstateIndexModuleData(-1, -1, null);
        }

        /// <summary>
        /// Try adding a substate to the list if the priority permits it.
        /// </summary>
        /// <param name="module">The module the data is attached to.</param>
        /// <param name="data">The item substate index data to add.</param>
        public void TryAddSubstateData(ActionModule module, ItemSubstateIndexData data)
        {
            TryAddSubstateData(new ItemSubstateIndexModuleData(module,data));
        }
        
        /// <summary>
        /// Try adding a substate to the list if the priority permits it.
        /// </summary>
        /// <param name="data">The item substate index data to add.</param>
        public void TryAddSubstateData(ItemSubstateIndexModuleData data)
        {
            if (Priority > data.Priority) { return;}

            AddSubstateData(data);
        }

        /// <summary>
        /// Add a substate to the list.
        /// </summary>
        /// <param name="data">The item substate index module data to add.</param>
        public void AddSubstateData(ItemSubstateIndexModuleData data)
        {
            m_SubstateIndexModuleDataList.Add(data);
            
            //Add the index if it is additive.
            var index = data.SubstateIndexData.Additive
                ? SubstateIndex + data.SubstateIndex
                : data.SubstateIndex;
            
            m_Data = new ItemSubstateIndexModuleData(index, data);
        }
        
        /// <summary>
        /// Add a substate index to the list.
        /// </summary>
        /// <param name="substateIndex">The item substate index to add.</param>
        /// <param name="priority">The priority of that item substate index.</param>
        /// <param name="module">The item action module it is attached to.</param>
        public void AddSubstateData(int substateIndex, int priority, ActionModule module)
        {
            var data = new ItemSubstateIndexModuleData(substateIndex, priority, module);
            AddSubstateData(data);
        }
    }
}