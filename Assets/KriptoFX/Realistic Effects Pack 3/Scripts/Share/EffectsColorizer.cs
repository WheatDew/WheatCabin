using System.Collections;
using UnityEngine;

public class EffectsColorizer : MonoBehaviour {

	public Color TintColor;
    public bool UseInstanceWhenNotEditorMode = true;

	private Color oldColor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(oldColor != TintColor) ChangeColor(gameObject, TintColor);
		oldColor = TintColor;
	}

	void ChangeColor(GameObject effect, Color color)
	{	
		var rend = effect.GetComponentsInChildren<Renderer>();
		foreach (var r in rend) {
        Material mat;

#if UNITY_EDITOR
             mat = r.sharedMaterial;
#else
            if (UseInstanceWhenNotEditorMode)
            {
                mat = r.material;
            }
            else
            {
                mat = r.sharedMaterial;
            }
#endif

		    var currentColor = ColorToHSV(TintColor); 
             if (mat == null) continue;
             if (mat.HasProperty("_TintColor"))
             {
                 var oldColor = mat.GetColor("_TintColor");
                 var hsv = ColorToHSV(oldColor);
                 hsv.h = currentColor.h / 360f;
                 color = HSVToColor(hsv);
                 mat.SetColor("_TintColor", color);
             }
             if (mat.HasProperty("_CoreColor"))
             {
                 var oldColor = mat.GetColor("_CoreColor");
                 var hsv = ColorToHSV(oldColor);
                 hsv.h = currentColor.h / 360f;
                 color = HSVToColor(hsv);
                 mat.SetColor("_CoreColor", color);
             }
             var projectors = effect.GetComponentsInChildren<Projector>();
             foreach (var proj in projectors)
             {
                 mat = proj.material;
                 if (mat == null || !mat.HasProperty("_TintColor")) continue;
                 var oldColor = mat.GetColor("_TintColor");
                 var hsv = ColorToHSV(oldColor);
                 hsv.h = currentColor.h / 360f;
                 color = HSVToColor(hsv);
                 proj.material.SetColor("_TintColor", color);
             }
		}
		var light = effect.GetComponentInChildren<Light>();
		if(light!=null) light.color = color;
	}

    public struct HSBColor
    {
        public float h;
        public float s;
        public float b;
        public float a;

        public HSBColor(float h, float s, float b, float a)
        {
            this.h = h;
            this.s = s;
            this.b = b;
            this.a = a;
        }
    }

    public HSBColor ColorToHSV(Color color)
    {
        HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

        float r = color.r;
        float g = color.g;
        float b = color.b;

        float max = Mathf.Max(r, Mathf.Max(g, b));

        if (max <= 0)
            return ret;

        float min = Mathf.Min(r, Mathf.Min(g, b));
        float dif = max - min;

        if (max > min)
        {
            if (g == max)
                ret.h = (b - r) / dif * 60f + 120f;
            else if (b == max)
                ret.h = (r - g) / dif * 60f + 240f;
            else if (b > g)
                ret.h = (g - b) / dif * 60f + 360f;
            else
                ret.h = (g - b) / dif * 60f;
            if (ret.h < 0)
                ret.h = ret.h + 360f;
        }
        else
            ret.h = 0;

        ret.h *= 1f / 360f;
        ret.s = (dif / max) * 1f;
        ret.b = max;

        return ret;
    }

    public Color HSVToColor(HSBColor hsbColor)
    {
        float r = hsbColor.b;
        float g = hsbColor.b;
        float b = hsbColor.b;
        if (hsbColor.s != 0)
        {
            float max = hsbColor.b;
            float dif = hsbColor.b * hsbColor.s;
            float min = hsbColor.b - dif;

            float h = hsbColor.h * 360f;

            if (h < 60f)
            {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f)
            {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f)
            {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f)
            {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f)
            {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f)
            {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
    }
}
