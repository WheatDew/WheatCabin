using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine.AI;

namespace Battlehub.RTNavigation
{
    [BuiltInDescriptor]
    public class NavMeshSurfaceComponentDescriptor : ComponentDescriptorBase<NavMeshSurface, NavMeshSurfaceGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            bool overrideVoxelSize = editor.NotNullComponents.OfType<NavMeshSurface>().All(nms => nms.overrideVoxelSize);
            bool overrideTileSize = editor.NotNullComponents.OfType<NavMeshSurface>().All(nms => nms.overrideTileSize);

            MemberInfo agentTypeInfo = Strong.MemberInfo((NavMeshSurface x) => x.agentTypeID);
            MemberInfo collectObjectsInfo = Strong.MemberInfo((NavMeshSurface x) => x.collectObjects);
            MemberInfo includeLayersInfo = Strong.MemberInfo((NavMeshSurface x) => x.layerMask);
            MemberInfo useGeometryInfo = Strong.MemberInfo((NavMeshSurface x) => x.useGeometry);
            MemberInfo defaultAreaInfo = Strong.MemberInfo((NavMeshSurface x) => x.defaultArea);
            MemberInfo overrideVoxelSizeInfo = Strong.MemberInfo((NavMeshSurface x) => x.overrideVoxelSize);
            MemberInfo voxelSizeInfo = Strong.MemberInfo((NavMeshSurface x) => x.voxelSize);
            MemberInfo overrideTileSizeInfo = Strong.MemberInfo((NavMeshSurface x) => x.overrideTileSize);
            MemberInfo tileSizeInfo = Strong.MemberInfo((NavMeshSurface x) => x.tileSize);
            MemberInfo buildHightMesh = Strong.MemberInfo((NavMeshSurface x) => x.buildHeightMesh);

            MethodInfo bakeMethodInfo = Strong.MethodInfo((NavMeshSurface x) => x.BuildNavMesh());
            MethodInfo clearMethodInfo = Strong.MethodInfo((NavMeshSurface x) => x.RemoveData());

            int settingsCount = NavMesh.GetSettingsCount();
            RangeOptions.Option[] agentTypes = new RangeOptions.Option[settingsCount];
            for(int i = 0; i < settingsCount; ++i)
            {
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                var name = NavMesh.GetSettingsNameFromID(id);
                agentTypes[i] = new RangeOptions.Option(name, id);
            }
            
            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_AgentType"), editor.Components, agentTypeInfo)
            {
                PropertyMetadata = new RangeOptions(agentTypes)
            });
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_CollectObjects"), editor.Components, collectObjectsInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_IncludeLayers"), editor.Components, includeLayersInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_UseGeometry"), editor.Components, useGeometryInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_DefaultArea"), editor.Components, defaultAreaInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_OverrideVoxelSize"), editor.Components, overrideVoxelSizeInfo)
            {
                ValueChangedCallback = () => editor.BuildEditor()
            });
            if (overrideVoxelSize)
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_VoxelSize"), editor.Components, voxelSizeInfo)); 
            }
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_OverrideTileSize"), editor.Components, overrideTileSizeInfo)
            {
                ValueChangedCallback = () => editor.BuildEditor()
            });

            if (overrideTileSize)
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_TileSize"), editor.Components, tileSizeInfo));
            }
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_BuildHeightMesh"), editor.Components, buildHightMesh));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_Bake"), editor.Components, bakeMethodInfo) 
            { 
                ValueChangedCallback = () => editor.BuildEditor() 
            });
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTNavigation_NavMeshAgentComponentDescriptor_Clear"), editor.Components, clearMethodInfo)
            {
                ValueChangedCallback = () => editor.BuildEditor()
            });

            return descriptors.ToArray();
        }
    }
}
