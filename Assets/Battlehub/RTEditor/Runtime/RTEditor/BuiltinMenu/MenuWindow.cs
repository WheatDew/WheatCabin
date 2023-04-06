using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [MenuDefinition(order: -60)]
    public class MenuWindow : MonoBehaviour
    {
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuWindow/Layouts", priority: 10)]
        public void Layouts()
        {
        }

        [MenuCommand("MenuWindow/General", priority:20)]
        public void General()
        {
        }

        [MenuCommand("MenuWindow/Layouts/Default", priority:10)]
        public void DefaultLayout()
        {
            Editor.ResetToDefaultLayout();
        }

        [MenuCommand("MenuWindow/General/Scene", validate:true)]
        public bool CanCreateScene()
        {
            return RenderPipelineInfo.Type == RPType.Standard || RenderPipelineInfo.UseForegroundLayerForUI;
        }

        [MenuCommand("MenuWindow/General/Scene", "RTE_View_Scene", priority:10)]
        public void Scene()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Scene.ToString());
        }

        [MenuCommand("MenuWindow/General/Game", validate: true)]
        public bool CanCreateGameView()
        {
            return RenderPipelineInfo.Type == RPType.Standard || RenderPipelineInfo.UseForegroundLayerForUI;
        }

        [MenuCommand("MenuWindow/General/Game", "RTE_View_GameView", priority: 20)]
        public void Game()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Game.ToString());
        }

        [MenuCommand("MenuWindow/General/Inspector", "RTE_View_Inspector", priority: 30)]
        public void Inspector()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Inspector.ToString());
        }

        [MenuCommand("MenuWindow/General/Hierarchy", "RTE_View_Hierarchy", priority: 40)]
        public void Hierarchy()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Hierarchy.ToString());
        }

        [MenuCommand("MenuWindow/General/Project", "RTE_View_Project", priority: 50)]
        public void Project()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Project.ToString());
        }

        [MenuCommand("MenuWindow/General/Console", "RTE_View_Console", priority: 60)]
        public void Console()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Console.ToString());
        }

        [MenuCommand("MenuWindow/General/Animation", "RTE_View_Animation", priority: 70)]
        public void Animation()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.Animation.ToString());
        }

    }
}

