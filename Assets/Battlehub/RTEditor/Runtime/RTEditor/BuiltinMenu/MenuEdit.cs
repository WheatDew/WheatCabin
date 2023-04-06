using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [MenuDefinition(order: -80)]
    public class MenuEdit : MonoBehaviour
    {
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuEdit/Undo", validate:true)]
        public bool CanUndo()
        {
            return Editor.Undo.CanUndo;
        }

        [MenuCommand("MenuEdit/Undo", priority: 10)]
        public void Undo()
        {
            Editor.Undo.Undo();
        }

        [MenuCommand("MenuEdit/Redo", validate: true)]
        public bool CanRedo()
        {
            return Editor.Undo.CanRedo;
        }

        [MenuCommand("MenuEdit/Redo", priority: 20)]
        public void Redo()
        {
            Editor.Undo.Redo();
        }

        [MenuCommand("MenuEdit/Duplicate", validate: true)]
        public bool CanDuplicate()
        {
            GameObject activeGameObject = Editor.Selection.activeGameObject;
            if (activeGameObject == null)
            {
                return false;
            }
            ExposeToEditor exposeToEditor = activeGameObject.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null && !exposeToEditor.CanDuplicate)
            {
                return false;
            }

            return true;
        }

        [MenuCommand("MenuEdit/Duplicate", priority: 30)]
        public void Duplicate()
        {
            Editor.Duplicate(Editor.Selection.gameObjects);
        }

        [MenuCommand("MenuEdit/Delete", validate: true)]
        public bool CanDelete()
        {
            GameObject activeGameObject = Editor.Selection.activeGameObject;
            if (activeGameObject == null)
            {
                return false;
            }
            ExposeToEditor exposeToEditor = activeGameObject.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null && !exposeToEditor.CanDelete)
            {
                return false;
            }
            return true;
        }

        [MenuCommand("MenuEdit/Delete", priority: 40)]
        public void Delete()
        {
            Editor.Delete(Editor.Selection.gameObjects);
        }

        [MenuCommand("MenuEdit/Play", validate: true)]
        public bool CanPlay()
        {
            return !Editor.IsPlaying;
        }

        [MenuCommand("MenuEdit/Play", priority: 50)]
        public void Play()
        {
            Editor.IsPlaying = true;
        }

        [MenuCommand("MenuEdit/Stop", validate: true)]
        public bool CanStop()
        {
            return Editor.IsPlaying;
        }

        [MenuCommand("MenuEdit/Stop", priority: 60)]
        public void Stop()
        {
            Editor.IsPlaying = false;
        }

        [MenuCommand("MenuEdit/Settings", validate:true)]
        public bool CanShowSettings()
        {
            return true;
        }

        [MenuCommand("MenuEdit/Settings", "RTE_Settings", priority: 70)]
        public void ShowSettings()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateDialogWindow(RuntimeWindowType.Settings.ToString(), "ID_RTEditor_WM_Header_Settings",
                (sender, args) => { }, (sender, args) => { }, 250, 125, -1, -1, true);
        }
    }
}
