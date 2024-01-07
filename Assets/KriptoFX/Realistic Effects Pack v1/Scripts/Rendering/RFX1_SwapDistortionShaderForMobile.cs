#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;


public class RFX1_SwapDistortionShaderForMobile
{

    //public int callbackOrder { get { return 0; } }
    //public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    //{
    //    var mats = FindAllMaterials();
    //    var pcShader =  Shader.Find("KriptoFX/RFX1/Distortion");
    //    var mobileShader = Shader.Find("KriptoFX/RFX1/DistortionMobile");
    //    if (pcShader == null || mobileShader == null) return;

    //    if (newTarget == BuildTarget.Android || newTarget == BuildTarget.iOS || newTarget == BuildTarget.Switch)
    //    {
    //        foreach (var mat in mats)
    //        {
    //            if (mat.shader == pcShader) mat.shader = mobileShader;
    //        }
    //    }
    //    else
    //    {
    //        foreach (var mat in mats)
    //        {
    //            if (mat.shader == mobileShader) mat.shader = pcShader;
    //        }
    //    }
    //}

    //List<Material> FindAllMaterials()
    //{
    //    var guids = AssetDatabase.FindAssets("t:Material");

    //    var materials = new List<Material>();
    //    foreach (var g in guids)
    //    {
    //        Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
    //        materials.Add(m);
    //    }
    //    return materials;
    //}

}
#endif