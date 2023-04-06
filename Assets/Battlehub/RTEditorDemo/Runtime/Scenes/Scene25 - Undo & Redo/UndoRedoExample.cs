using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene25
{
    public class UndoRedoExample : MonoBehaviour
    {
        private IRuntimeEditor m_editor;

        private void Start()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_editor.Undo.BeforeUndo += OnBeforeUndo;
            m_editor.Undo.UndoCompleted += OnUndoCompleted;
            m_editor.Undo.BeforeRedo += OnBeforeRedo;
            m_editor.Undo.RedoCompleted += OnRedoCompleted;
        }

        private void OnDestroy()
        {
            m_editor = null;
        }

        public bool CanUndo
        {
            get { return m_editor.Undo.CanUndo; }
        }

        public bool CanRedo
        {
            get { return m_editor.Undo.CanRedo; }
        }

        public void Undo()
        {
            m_editor.Undo.Undo();
        }

        public void Redo()
        {
            m_editor.Undo.Redo();
        }

        public void CreateObject()
        {
            ExposeToEditor go = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<ExposeToEditor>();

            m_editor.RegisterCreatedObjects(new[] { go.gameObject }, select: true);
        }

        public void DestroySelectedObjects()
        {
            ExposeToEditor[] selectedGameObjects = m_editor.Selection
                .OfType<GameObject>()
                .Select(go => go.GetComponent<ExposeToEditor>())
                .ToArray();

            m_editor.Undo.BeginRecord();

            m_editor.Selection.objects = null;
            m_editor.Undo.DestroyObjects(selectedGameObjects);

            m_editor.Undo.EndRecord();
        }

        public void AddComponent()
        {
            ExposeToEditor[] selectedGameObjects = m_editor.Selection
                .OfType<GameObject>()
                .Select(go => go.GetComponent<ExposeToEditor>())
                .ToArray();

            m_editor.Undo.BeginRecord();

            foreach (ExposeToEditor go in selectedGameObjects)
            {
                m_editor.Undo.AddComponentWithRequirements(go, typeof(BoxCollider));
            }

            m_editor.Undo.EndRecord();
        }

        public void DestroyComponents()
        {
            BoxCollider[] boxColliders = m_editor.Selection
                .OfType<GameObject>()
                .Select(go => go.GetComponent<BoxCollider>())
                .Where(collder => collder != null)
                .ToArray();

            m_editor.Undo.BeginRecord();

            foreach (BoxCollider boxCollider in boxColliders)
            {
                m_editor.Undo.DestroyComponent(boxCollider, null);
            }

            m_editor.Undo.EndRecord();
        }

        public void CreateGenericRecord()
        {
            ExposeToEditor[] selectedGameObjects = m_editor.Selection
                .OfType<GameObject>()
                .Select(go => go.GetComponent<ExposeToEditor>())
                .ToArray();

            m_editor.Undo.BeginRecord();

            foreach (ExposeToEditor go in selectedGameObjects)
            {
                string newName = System.Guid.NewGuid().ToString();
                string oldName = go.name;

                m_editor.Undo.CreateRecord(go, newName, oldName,
                    state =>
                    {
                        ExposeToEditor target = (ExposeToEditor)state.Target;
                        target.SetName(state.NewState.ToString());
                        return true; 
                    },
                    state => 
                    {
                        ExposeToEditor target = (ExposeToEditor)state.Target;
                        target.SetName(state.OldState.ToString());
                        return true;
                    });

                go.SetName(newName);
            }

            m_editor.Undo.EndRecord();
        }

        private void OnBeforeUndo()
        {
            Debug.Log("On Before Undo");
        }

        private void OnUndoCompleted()
        {
            Debug.Log("On Undo Completed");
        }

        private void OnBeforeRedo()
        {
            Debug.Log("On Before Redo");
        }

        private void OnRedoCompleted()
        {
            Debug.Log("On Redo Completed");
        }
    }

}
