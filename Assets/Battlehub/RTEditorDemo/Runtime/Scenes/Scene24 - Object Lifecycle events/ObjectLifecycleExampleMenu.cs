using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene24
{

    [MenuDefinition]
    public class ObjectLifecycleExampleMenu : MonoBehaviour
    {
        [MenuCommand("Edit/Undo", validate:true)]
        public bool CanUndo()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            return editor.Undo.CanUndo;
        }

        [MenuCommand("Edit/Undo")]
        public void Undo()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.Undo();
        }

        [MenuCommand("Edit/Redo", validate:true)]
        public bool CanRedo()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            return editor.Undo.CanRedo;
        }

        [MenuCommand("Edit/Redo")]
        public void Redo()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.Redo();
        }

        [MenuCommand("Selection/Activate", validate:true)]
        public bool CanActivate()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Selection/Activate")]
        public void Activate() 
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();
            foreach (GameObject go in editor.Selection.OfType<GameObject>())
            {
                editor.Undo.CreateRecord(
                    state => { go.SetActive(false); return true; },
                    state => { go.SetActive(true); return true; });
                go.SetActive(true);   
            }
            editor.Undo.EndRecord();
        }

        [MenuCommand("Selection/Deactivate", validate: true)]
        public bool CanDeactivate()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Selection/Deactivate")]
        public void Deactivate()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();
            foreach (GameObject go in editor.Selection.OfType<GameObject>())
            {
                editor.Undo.CreateRecord(
                    state => { go.SetActive(true); return true; },
                    state => { go.SetActive(false); return true; });
                go.SetActive(false);
            }
            editor.Undo.EndRecord();
        }

        [MenuCommand("Selection/Destroy", validate: true)]
        public bool CanDestroy()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Selection/Destroy")]
        public void Destroy()
        {
            IRTE editor = IOC.Resolve<IRTE>();

            var objectsToDestroy = editor.Selection.OfType<GameObject>().Select(obj => obj.GetComponent<ExposeToEditor>()).ToArray();
            if(objectsToDestroy != null)
            {
                editor.Undo.BeginRecord();
                editor.Selection.objects = null;
                editor.Undo.DestroyObjects(objectsToDestroy);
                editor.Undo.EndRecord();
            }
        }

        [MenuCommand("Selection/Add Box Collider", validate: true)]
        public bool CanAddBoxCollider()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Selection/Add Box Collider")]
        public void AddBoxCollider()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();
            foreach (GameObject go in editor.Selection.OfType<GameObject>())
            {
                editor.Undo.AddComponentWithRequirements(go.GetComponent<ExposeToEditor>(), typeof(BoxCollider));
            }
            editor.Undo.EndRecord();
        }

        [MenuCommand("Selection/Destroy Box Collider", validate: true)]
        public bool CanDestroyBoxCollider()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Selection/Destroy Box Collider")]
        public void DestroyBoxCollider()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();
            foreach (GameObject go in editor.Selection.OfType<GameObject>())
            {
                editor.Undo.DestroyComponent(go.GetComponent<BoxCollider>(), null);
            }
            editor.Undo.EndRecord();
        }

        [MenuCommand("Selection/Set Random Name", validate: true)]
        public bool CanSetRandomName()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }


        [MenuCommand("Selection/Set Random Name")]
        public void SetRandomName()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.BeginRecord();
            foreach (GameObject go in editor.Selection.OfType<GameObject>())
            {
                ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();

                string newName = System.Guid.NewGuid().ToString();
                string oldName = go.name;

                editor.Undo.CreateRecord(exposeToEditor, newName, oldName,
                    state => { ((ExposeToEditor)state.Target).SetName(state.NewState.ToString()); return true; },
                    state => { ((ExposeToEditor)state.Target).SetName(state.OldState.ToString()); return true;  });

                exposeToEditor.SetName(newName);
            }
            editor.Undo.EndRecord();
        }


        [MenuCommand("MenuFile", hide: true)]
        public void HideMenuFile() { }

        [MenuCommand("MenuEdit", hide: true)]
        public void HideMenuEdit() { }

        [MenuCommand("MenuWindow", hide: true)]
        public void HideMenuWindow() { }

        [MenuCommand("MenuHelp", hide: true)]
        public void HideHelp() { }

    }
}

