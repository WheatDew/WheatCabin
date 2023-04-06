using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;

namespace Battlehub.RTBuilder
{
    public class ProBuilderBezierShapeTool : ProBuilderComplexShapeTool<PBBezierShape>
    {
        public override string Name
        {
            get { return Localization.GetString("ID_RTBuilder_View_BezierShape", "Bezier Shape"); }
        }

        protected override string CreateShapeCommandText
        {
            get { return Localization.GetString("ID_RTBuilder_View_NewBezierShape", "New Bezier Shape"); }
        }

        protected override string EditShapeCommandText
        {
            get { return Localization.GetString("ID_RTBuilder_View_EditBezierShape", "Edit Bezier Shape"); }
        }

        protected override PBComplexShape CreateShape(ExposeToEditor exposeToEditor)
        {
            return exposeToEditor.gameObject.AddComponent<PBBezierShape>();
        }

        protected override void Awake()
        {
            base.Awake();
            //IOC.RegisterFallback<IPolyShapeTool>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //IOC.UnregisterFallback<IPolyShapeTool>(this);
        }
    }

}
