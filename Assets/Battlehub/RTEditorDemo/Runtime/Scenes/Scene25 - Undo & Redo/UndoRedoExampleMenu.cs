using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene25
{
    [RequireComponent(typeof(UndoRedoExample))]
    [MenuDefinition]
    public class UndoRedoExampleMenu : MonoBehaviour
    {
        private UndoRedoExample m_example;

        private void Start()
        {
            m_example = GetComponent<UndoRedoExample>();
        }

        [MenuCommand("Example/Undo", validate: true)]
        public bool CanUndo()
        {
            return m_example.CanUndo;
        }

        [MenuCommand("Example/Undo")]
        public void Undo()
        {
            m_example.Undo();
        }

        [MenuCommand("Example/Redo", validate: true)]
        public bool CanRedo()
        {
            return m_example.CanRedo;
        }

        [MenuCommand("Example/Redo")]
        public void Redo()
        {
            m_example.Redo();
        }

        [MenuCommand("Example/Create Object")]
        public void CreateObject()
        {
            m_example.CreateObject();
        }

        [MenuCommand("Example/Destroy Selected Objects", validate:true)]
        public bool CanDestroySelectedObjects()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Example/Destroy Selected Objects")]
        public void DestroySelectedObjects()
        {
            m_example.DestroySelectedObjects();
        }

        [MenuCommand("Example/Add Component", validate: true)]
        public bool CanAddComponent()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Example/Add Component")]
        public void AddComponent()
        {
            m_example.AddComponent();
        }

        [MenuCommand("Example/Destroy Components", validate: true)]
        public bool CanDestroyComponents()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Example/Destroy Components")]
        public void DestroyComponents()
        {
            m_example.DestroyComponents();
        }

        [MenuCommand("Example/Create Generic Record (Set Object Name)", validate: true)]
        public bool CanCreateGenericRecord()
        {
            return IOC.Resolve<IRTE>().Selection.Length > 0;
        }

        [MenuCommand("Example/Create Generic Record (Set Object Name)")]
        public void CreateGenericRecord()
        {
            m_example.CreateGenericRecord();
        }
    }
}