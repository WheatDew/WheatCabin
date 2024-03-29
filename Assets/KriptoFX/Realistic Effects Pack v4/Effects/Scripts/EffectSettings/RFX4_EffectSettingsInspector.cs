#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(RFX4_EffectSettings))]
public class RFX4_EffectSettingsInspector : Editor
{

    public override void OnInspectorGUI()
    {


        Undo.RecordObjects(targets, "Changed Update Particles Effects");

       
        var script = (RFX4_EffectSettings)target;
       

       
        var isMobilePlatfrom = IsMobilePlatform();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Main Parameters", EditorStyles.boldLabel);

        script.ParticlesBudget = EditorGUILayout.Slider("Particles Budget", script.ParticlesBudget, 0.1f, 1);
        script.UseLightShadows = EditorGUILayout.Toggle("Use Light Shadows", script.UseLightShadows);
        script.UseFastFlatDecalsForMobiles = EditorGUILayout.Toggle("Use Fast Flat Decals for Mobiles", script.UseFastFlatDecalsForMobiles);
        script.UseCustomColor = EditorGUILayout.Toggle("Use Custom Color", script.UseCustomColor);
        if (script.UseCustomColor) script.EffectColor = EditorGUILayout.ColorField("Effect Color", script.EffectColor);

        script.IsVisible = EditorGUILayout.Toggle("Is Visible", script.IsVisible);
        script.FadeoutTime = EditorGUILayout.FloatField("Fadeout Time", script.FadeoutTime);

        EditorGUILayout.EndVertical();

        if (script.GetComponentInChildren<RFX4_PhysicsMotion>() != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Projectile parameters", EditorStyles.boldLabel);
            script.UseCollisionDetection = EditorGUILayout.Toggle("Use Collision Detection", script.UseCollisionDetection);
            script.LimitMaxDistance = EditorGUILayout.Toggle("Use Max Distance Limit",       script.LimitMaxDistance);
            if (script.LimitMaxDistance) script.MaxDistnace = EditorGUILayout.FloatField("Max Distance", script.MaxDistnace);
            script.Mass = EditorGUILayout.FloatField("Mass",       script.Mass);
            script.Speed = EditorGUILayout.FloatField("Speed",     script.Speed);
            script.AirDrag = EditorGUILayout.FloatField("AirDrag", script.AirDrag);
            script.UseGravity = EditorGUILayout.Toggle("Use Gravity", script.UseGravity);
            EditorGUILayout.EndVertical();
        }

        var allSelectedScripts = targets;
        foreach (var selectedScript in allSelectedScripts)
        {
            var s = (RFX4_EffectSettings)selectedScript;
            if (s == script) continue;

            s.ParticlesBudget = script.ParticlesBudget;
            s.UseLightShadows = script.UseLightShadows;
            s.UseFastFlatDecalsForMobiles = script.UseFastFlatDecalsForMobiles;
            s.UseCustomColor = script.UseCustomColor;
            if (s.UseCustomColor) s.EffectColor = script.EffectColor;

            s.IsVisible = script.IsVisible;
            s.FadeoutTime = script.FadeoutTime;

            if (s.GetComponentInChildren<RFX4_PhysicsMotion>() != null)
            {
                s.UseCollisionDetection = script.UseCollisionDetection;
                s.LimitMaxDistance = script.LimitMaxDistance;
                if (s.LimitMaxDistance) s.MaxDistnace = script.MaxDistnace;
                s.Mass = script.Mass;
                s.Speed = script.Speed;
                s.AirDrag = script.AirDrag;
                s.UseGravity = script.UseGravity;
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }


    bool IsMobilePlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
            || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS
            || EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer) return true;

        return false;
    }
}
#endif