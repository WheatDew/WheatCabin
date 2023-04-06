using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public interface IProBuilderCustomTool
    {
        string Name
        {
            get;
        }

        void OnBeforeCommandsUpdate();

        void GetCommonCommands(List<ToolCmd> commands);

        void GetObjectCommands(List<ToolCmd> commands);

        void GetFaceCommands(List<ToolCmd> commands);

        void GetEdgeCommands(List<ToolCmd> commands);

        void GetVertexCommands(List<ToolCmd> commands);
    }


    public abstract class ProBuilderCustomTool : MonoBehaviour, IProBuilderCustomTool
    {
        public abstract string Name
        {
            get;
        }

        protected virtual void Awake()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        public virtual void OnBeforeCommandsUpdate()
        {

        }

        public virtual void GetCommonCommands(List<ToolCmd> commands)
        {
            
        }
        public virtual void GetFaceCommands(List<ToolCmd> commands)
        {

        }

        public virtual void GetEdgeCommands(List<ToolCmd> commands)
        {
            
        }

        public virtual void GetObjectCommands(List<ToolCmd> commands)
        {
            
        }

        public virtual void GetVertexCommands(List<ToolCmd> commands)
        {
            
        }

        protected ToolCmd GetCustomToolsCmd(List<ToolCmd> commands)
        {
            ILocalization localization = IOC.Resolve<ILocalization>();
            ToolCmd toolsCmd = commands.Where(cmd => cmd.Text == localization.GetString("ID_RTBuilder_View_Tools", "Tools")).FirstOrDefault();
            if(toolsCmd == null)
            {
                toolsCmd = new ToolCmd(localization.GetString("ID_RTBuilder_View_Tools", "Tools"), () => { });
                toolsCmd.Children = new List<ToolCmd>();

                commands.Add(toolsCmd);
            }

            return toolsCmd;
        }
    }
}