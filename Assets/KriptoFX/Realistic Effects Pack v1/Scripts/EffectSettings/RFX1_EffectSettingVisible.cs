using System.Collections.Generic;
using UnityEngine;

public class RFX1_EffectSettingVisible : MonoBehaviour
{
    public bool IsActive = true;
    public float FadeOutTime = 3;


    private bool previousActiveStatus;
    bool needUpdate;
    bool needLastUpdate;

    Dictionary<string, float> startAlphaColors;

    string[] colorProperties =
    {
        "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor",
        "_MainColor", "_CoreColor", "_FresnelColor"
    };

    void OnEnable()
    {
        alpha = 1;
        prevAlpha = 1;
        IsActive = true;
    }


    float alpha;
    float prevAlpha;

    private void Update()
    {
        if(!IsActive && startAlphaColors == null)
        {
            InitStartAlphaColors();
        }

        if (IsActive && alpha < 1) alpha += Time.deltaTime / FadeOutTime;
        if (!IsActive && alpha > 0) alpha -= Time.deltaTime / FadeOutTime;

        if (alpha > 0 && alpha < 1)
        {
            needUpdate = true;
        }
        else
        {
            needUpdate = false;
            alpha = Mathf.Clamp01(alpha);
            if (Mathf.Abs(prevAlpha - alpha) >= Mathf.Epsilon) UpdateVisibleStatus();
        }
        prevAlpha = alpha;

        if (needUpdate) UpdateVisibleStatus();
    }

    void InitStartAlphaColors()
    {
        startAlphaColors = new Dictionary<string, float>();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            var mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                GetStartAlphaByProperties(rend.GetHashCode().ToString(), i, mats[i]);
            }
        }

        var lights = GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            startAlphaColors.Add(lights[i].GetHashCode().ToString() + i, lights[i].intensity);
        }

        var projectors = GetComponentsInChildren<Projector>();
        foreach (var proj in projectors)
        {
            var mat = proj.material;

            GetStartAlphaByProperties(proj.GetHashCode().ToString(), 0, mat);
        }

        var audios = GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < audios.Length; i++)
        {
            startAlphaColors.Add(audios[i].GetHashCode().ToString() + i, audios[i].volume);
        }
    }

    void UpdateVisibleStatus()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            var mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                UpdateAlphaByProperties(rend.GetHashCode().ToString(), i, mats[i], alpha);
            }
        }

        var lightCurves = GetComponentsInChildren<RFX1_LightCurves>();
        foreach(var lightCurve in lightCurves)
        {
            lightCurve.enabled = IsActive;
        }

        var lights = GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            var startAlpha = startAlphaColors[lights[i].GetHashCode().ToString() + i];
            lights[i].intensity = alpha * startAlpha;
        }

        var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            if (!IsActive && !ps.isStopped) ps.Stop();
            if (IsActive && ps.isStopped) ps.Play();
        }


        var projectors = GetComponentsInChildren<Projector>();
        foreach (var proj in projectors)
        {
            var mat = proj.material;

            UpdateAlphaByProperties(proj.GetHashCode().ToString(), 0, mat, alpha);
        }

        var audios = GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < audios.Length; i++)
        {
            var startAlpha = startAlphaColors[audios[i].GetHashCode().ToString() + i];
            audios[i].volume = alpha * startAlpha;
        }

        
    }

    void UpdateAlphaByProperties(string rendName, int materialNumber, Material mat, float alpha)
    {
        foreach (var prop in colorProperties)
        {
            if (mat.HasProperty(prop))
            {
                var startAlpha = startAlphaColors[rendName + materialNumber + prop.ToString()];
                var color = mat.GetColor(prop);
                color.a = alpha * startAlpha;
                mat.SetColor(prop, color);
            }
        }
    }

    void GetStartAlphaByProperties(string rendName, int materialNumber, Material mat)
    {
        foreach (var prop in colorProperties)
        {
            if (mat.HasProperty(prop))
            {
                startAlphaColors.Add(rendName + materialNumber + prop.ToString(), mat.GetColor(prop).a);
            }
        }
    }

}
