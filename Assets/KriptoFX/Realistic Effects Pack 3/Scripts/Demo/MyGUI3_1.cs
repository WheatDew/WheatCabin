using System;
using UnityEngine;
using System.Collections;

public class MyGUI3_1 : MonoBehaviour
{
    public enum GuiStat
    {
        Ball,
        BallRotate,
        BallRotatex4,
        Bottom,
        Middle,
        MiddleWithoutRobot,
        Top,
        TopTarget
    }

    public Texture HUETexture;
    public int CurrentPrefabNomber = 0;
    public float UpdateInterval = 0.5F;
    public Light DirLight;
    public GameObject Target;
    public GameObject TargetForRay;
    public GameObject TopPosition;
    public GameObject MiddlePosition;
    public Vector3 defaultRobotPos;
    public GameObject BottomPosition;
    public GameObject Plane1;
    public GameObject Plane2;
    public Material[] PlaneMaterials;
    public GuiStat[] GuiStats;
    public GameObject[] Prefabs;

    private float oldLightIntensity;
    private Color oldAmbientColor;
    private GameObject currentGo;
    private bool isDay, isDefaultPlaneTexture;
    private int current;
    private EffectSettings effectSettings;
    private bool isReadyEffect;
    private Quaternion defaultRobotRotation;
    private float colorHUE;
    private GUIStyle guiStyleHeader = new GUIStyle();
    private float dpiScale;

    private void Start()
    {
        if (Screen.dpi < 1)
            dpiScale = 1;
        if (Screen.dpi < 200)
            dpiScale = 1;
        else
            dpiScale = Screen.dpi / 200f;

        oldAmbientColor = RenderSettings.ambientLight;
        oldLightIntensity = DirLight.intensity;

        guiStyleHeader.fontSize = (int) (15f * dpiScale);
        guiStyleHeader.normal.textColor = new Color(1, 1, 1);
        current = CurrentPrefabNomber;
        InstanceCurrent(GuiStats[CurrentPrefabNomber]);
    }

    private void InstanceEffect(Vector3 pos)
    {
        currentGo = Instantiate(Prefabs[current], pos, Prefabs[current].transform.rotation) as GameObject;
        effectSettings = currentGo.GetComponent<EffectSettings>();
        effectSettings.Target = GetTargetObject(GuiStats[current]);
        effectSettings.EffectDeactivated += effectSettings_EffectDeactivated;
        if (GuiStats[current]==GuiStat.Middle) {
            currentGo.transform.parent = GetTargetObject(GuiStat.Middle).transform;
            currentGo.transform.position = GetInstancePosition(GuiStat.Middle);
        }
        else
            currentGo.transform.parent = transform;
        effectSettings.CollisionEnter += (n, e) => {
            if (e.Hit.transform!=null)
                Debug.Log(e.Hit.transform.name);
        };
    }


    private GameObject GetTargetObject(GuiStat stat)
    {
        switch (stat) {
        case GuiStat.Ball: {
            return Target;
        }
        case GuiStat.BallRotate: {
            return Target;
        }
        case GuiStat.Bottom: {
            return BottomPosition;
        }
        case GuiStat.Top: {
            return TopPosition;
        }
        case GuiStat.TopTarget: {
            return BottomPosition;
        }
        case GuiStat.Middle: {
            MiddlePosition.transform.localPosition = defaultRobotPos;
            MiddlePosition.transform.localRotation = Quaternion.Euler(0, 180, 0);
            return MiddlePosition;
        }
        case GuiStat.MiddleWithoutRobot: {
            return MiddlePosition.transform.parent.gameObject;
        }
        }
        return gameObject;
    }

    private void effectSettings_EffectDeactivated(object sender, EventArgs e)
    {
        if (GuiStats[current]!=GuiStat.Middle)
            currentGo.transform.position = GetInstancePosition(GuiStats[current]);
        isReadyEffect = true;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10 * dpiScale, 15 * dpiScale, 105 * dpiScale, 30 * dpiScale), "Previous Effect"))
            ChangeCurrent(-1);
        if (GUI.Button(new Rect(130 * dpiScale, 15 * dpiScale, 105 * dpiScale, 30 * dpiScale), "Next Effect"))
            ChangeCurrent(+1);
        if (Prefabs[current]!=null)
            GUI.Label(new Rect(300 * dpiScale, 15 * dpiScale, 100 * dpiScale, 20 * dpiScale), "Prefab name is \"" + Prefabs[current].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
        if (GUI.Button(new Rect(10 * dpiScale, 60 * dpiScale, 225 * dpiScale, 30 * dpiScale), "Day/Night")) {
            DirLight.intensity = !isDay ? 0.00f : oldLightIntensity;
            DirLight.transform.rotation = isDay ? Quaternion.Euler(400, 30, 90) : Quaternion.Euler(350, 30, 90);
            RenderSettings.ambientLight = !isDay ? new Color(0.1f, 0.1f, 0.1f) : oldAmbientColor;
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
#else
            RenderSettings.ambientIntensity = isDay ? 0.5f : 0.1f;
            RenderSettings.reflectionIntensity = isDay ? 1 : 0.1f;
#endif
            isDay = !isDay;
        }

        GUI.DrawTexture(new Rect(12 * dpiScale, 110 * dpiScale, 220 * dpiScale, 15 * dpiScale), HUETexture, ScaleMode.StretchToFill, false, 0);
        float oldColorHUE = colorHUE;
        colorHUE = GUI.HorizontalSlider(new Rect(12 * dpiScale, 135 * dpiScale, 220 * dpiScale, 15 * dpiScale), colorHUE, 0, 360);
        if (Mathf.Abs(oldColorHUE - colorHUE) > 0.001)
            ChangeColor();
        GUI.Label(new Rect(240 * dpiScale, 105 * dpiScale, 30 * dpiScale, 30 * dpiScale), "Effect color", guiStyleHeader);
    }

    private void Update()
    {
        if (isReadyEffect) {
            isReadyEffect = false;
            currentGo.SetActive(true);
        }
        if (GuiStats[current]==GuiStat.BallRotate)
            currentGo.transform.localRotation = Quaternion.Euler(0, Mathf.PingPong(Time.time * 5, 60) - 50, 0);
        if (GuiStats[current]==GuiStat.BallRotatex4)
            currentGo.transform.localRotation = Quaternion.Euler(0, Mathf.PingPong(Time.time * 30, 100) - 70, 0);
    }

    private void InstanceCurrent(GuiStat stat)
    {
        switch (stat) {
        case GuiStat.Ball: {
            MiddlePosition.SetActive(false);
            InstanceEffect(transform.position);
            break;
        }
        case GuiStat.BallRotate: {
            MiddlePosition.SetActive(false);
            InstanceEffect(transform.position);
            break;
        }
        case GuiStat.BallRotatex4: {
            MiddlePosition.SetActive(false);
            InstanceEffect(transform.position);
            break;
        }
        case GuiStat.Bottom: {
            MiddlePosition.SetActive(false);
            InstanceEffect(BottomPosition.transform.position);
            break;
        }
        case GuiStat.Top: {
            MiddlePosition.SetActive(false);
            InstanceEffect(TopPosition.transform.position);
            break;
        }
        case GuiStat.TopTarget: {
            MiddlePosition.SetActive(false);
            InstanceEffect(TopPosition.transform.position);
            break;
        }
        case GuiStat.Middle: {
            MiddlePosition.SetActive(true);
            InstanceEffect(MiddlePosition.transform.parent.transform.position);
            break;
        }
        case GuiStat.MiddleWithoutRobot: {
            MiddlePosition.SetActive(false);
            InstanceEffect(MiddlePosition.transform.position);
            break;
        }
        }
    }

    private Vector3 GetInstancePosition(GuiStat stat)
    {
        switch (stat) {
        case GuiStat.Ball: {
            return transform.position;
        }
        case GuiStat.BallRotate: {
            return transform.position;
        }
        case GuiStat.BallRotatex4: {
            return transform.position;
        }
        case GuiStat.Bottom: {
            return BottomPosition.transform.position;
        }
        case GuiStat.Top: {
            return TopPosition.transform.position;
        }
        case GuiStat.TopTarget: {
            return TopPosition.transform.position;
        }
        case GuiStat.MiddleWithoutRobot: {
            return MiddlePosition.transform.parent.transform.position;
        }
        case GuiStat.Middle: {
            return MiddlePosition.transform.parent.transform.position;
        }
        }
        return transform.position;
    }

    private void ChangeCurrent(int delta)
    {
        Destroy(currentGo);
        CancelInvoke("InstanceDefaulBall");
        current += delta;
        if (current > Prefabs.Length - 1)
            current = 0;
        else if (current < 0)
            current = Prefabs.Length - 1;

        if (effectSettings!=null)
            effectSettings.EffectDeactivated -= effectSettings_EffectDeactivated;
        MiddlePosition.SetActive(GuiStats[current]==GuiStat.Middle);
        InstanceEffect(GetInstancePosition(GuiStats[current]));
        if (TargetForRay!=null)
            if (current==14 || current==22)
                TargetForRay.SetActive(true);
            else
                TargetForRay.SetActive(false);
    }

    private Color Hue(float H)
    {
        Color col = new Color(1, 0, 0);
        if (H >= 0 && H < 1)
            col = new Color(1, 0, H);
        if (H >= 1 && H < 2)
            col = new Color(2 - H, 0, 1);
        if (H >= 2 && H < 3)
            col = new Color(0, H - 2, 1);
        if (H >= 3 && H < 4)
            col = new Color(0, 1, 4 - H);
        if (H >= 4 && H < 5)
            col = new Color(H - 4, 1, 0);
        if (H >= 5 && H < 6)
            col = new Color(1, 6 - H, 0);
        return col;
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

        if (max > min) {
            if (g==max)
                ret.h = (b - r) / dif * 60f + 120f;
            else if (b==max)
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
        if (hsbColor.s!=0) {
            float max = hsbColor.b;
            float dif = hsbColor.b * hsbColor.s;
            float min = hsbColor.b - dif;

            float h = hsbColor.h * 360f;

            if (h < 60f) {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            }
            else if (h < 120f) {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            }
            else if (h < 180f) {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            }
            else if (h < 240f) {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            }
            else if (h < 300f) {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            }
            else if (h <= 360f) {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            }
            else {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
    }

    private void ChangeColor()
    {
        var color = Hue(colorHUE / 255f);
        var rend = currentGo.GetComponentsInChildren<Renderer>();
        foreach (var r in rend) {
            var mat = r.material;
            if (mat==null)
                continue;
            if (mat.HasProperty("_TintColor")) {
                var oldColor = mat.GetColor("_TintColor");
                var hsv = ColorToHSV(oldColor);
                hsv.h = colorHUE/360f;
                color = HSVToColor(hsv);
                color.a = oldColor.a;

                mat.SetColor("_TintColor", color);
            }
            if (mat.HasProperty("_CoreColor")) {
                var oldColor = mat.GetColor("_CoreColor");
                var hsv = ColorToHSV(oldColor);
                hsv.h = colorHUE / 360f;
                color = HSVToColor(hsv);
                color.a = oldColor.a;
                mat.SetColor("_CoreColor", color);
            }
        }
        var projectors = currentGo.GetComponentsInChildren<Projector>();
        foreach (var proj in projectors) {
            var mat = proj.material;
            if (mat==null || !mat.HasProperty("_TintColor"))
                continue;
            var oldColor = mat.GetColor("_TintColor");
            var hsv = ColorToHSV(oldColor);
            hsv.h = colorHUE / 360f;
            color = HSVToColor(hsv);
            color.a = oldColor.a;
            proj.material.SetColor("_TintColor", color);
        }
        var light = currentGo.GetComponentInChildren<Light>();
        if (light!=null)
            light.color = color;
    }
}