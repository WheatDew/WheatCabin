using UnityEngine;

using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using System;

namespace Battlehub.RTBuilder
{
  
    #pragma warning disable CS0612
    public interface IPolyShapeTool : IPolyShapeEditor
    #pragma warning restore CS0612
    {        
    }

    [DefaultExecutionOrder(-89)]
    public class ProBuilderPolyShapeTool : ProBuilderComplexShapeTool<PBPolyShape>, IPolyShapeTool
    {
        public override string Name
        {
            get { return Localization.GetString("ID_RTBuilder_View_PolyShape", "Poly Shape"); }
        }

        protected override string CreateShapeCommandText
        {
            get { return Localization.GetString("ID_RTBuilder_View_NewPolyShape", "New Poly Shape"); }
        }

        protected override string EditShapeCommandText
        {
            get { return Localization.GetString("ID_RTBuilder_View_EditPolyShape", "Edit Poly Shape"); }
        }

        protected override PBComplexShape CreateShape(ExposeToEditor exposeToEditor)
        {
            return exposeToEditor.gameObject.AddComponent<PBPolyShape>();
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<IPolyShapeTool>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<IPolyShapeTool>(this);
        }
    }

    #region Obsolete 

    [Obsolete] //04.08.2021
    public interface IPolyShapeEditor
    {
        LayerMask LayerMask
        {
            get;
            set;
        }
    }

    [Obsolete, DefaultExecutionOrder(-89)] //04.08.2021
    public class ProBuilderPolyShapeEditor : ProBuilderPolyShapeTool
    {
    }

    #endregion

}
