namespace Battlehub.RTEditor
{
    public class SceneViewInput : BaseViewInput<SceneView>
    {
        protected override void StartOverride()
        {
            base.StartOverride();
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            //if (SelectAllAction())
            //{
            //    View.SelectAll();
            //}

            if (DuplicateAction())
            {
                Editor.Duplicate(Editor.Selection.gameObjects);
            }

            if (DeleteAction())
            {
                Editor.Delete(Editor.Selection.gameObjects);
            }
        }
    }
}

