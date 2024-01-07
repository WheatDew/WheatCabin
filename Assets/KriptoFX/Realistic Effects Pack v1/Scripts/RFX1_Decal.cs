using UnityEngine;
using UnityEngine.Rendering;

#if KRIPTO_FX_LWRP_RENDERING
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif

[ExecuteInEditMode]
public class RFX1_Decal : MonoBehaviour
{
    public bool UseWorldSpaceRotation = false;
    public bool UseRandomRotationAndScale = true;
    public float randomScalePercent = 20;
    public bool IsScreenSpace = true;

    // Material mat;
    ParticleSystem ps;
    ParticleSystem.MainModule psMain;
    private MaterialPropertyBlock props;
    MeshRenderer rend;
    private Vector3 startScale;
    private Vector3 worldRotation = new Vector3(0, 0, 0);

    void Awake()
    {
        startScale = transform.localScale;
    }


    private void OnEnable()
    {
        //if (Application.isPlaying) mat = GetComponent<Renderer>().material;
        //else mat = GetComponent<Renderer>().sharedMaterial;

        var meshRend = GetComponent<MeshRenderer>();
        if (meshRend == null) return;

        ps = GetComponent<ParticleSystem>();
        if (ps != null) psMain = ps.main;

        //if (Camera.main.depthTextureMode != DepthTextureMode.Depth) Camera.main.depthTextureMode = DepthTextureMode.Depth;

        GetComponent<MeshRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;

#if KRIPTO_FX_LWRP_RENDERING
        var addCamData = Camera.main.GetComponent<LWRPAdditionalCameraData>();
        if (addCamData != null) IsScreenSpace = addCamData.requiresDepthTexture;
#endif

        if (Camera.main.orthographic) IsScreenSpace = false;

        if (!IsScreenSpace)
        {
            var sharedMaterial = GetComponent<Renderer>().sharedMaterial;
            sharedMaterial.EnableKeyword("USE_QUAD_DECAL");
            sharedMaterial.SetInt("_ZTest1", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            if (Application.isPlaying)
            {
                var pos = transform.localPosition;
                pos.z += 0.1f;
                transform.localPosition = pos;
                var scale = transform.localScale;
                scale.y = 0.001f;
                transform.localScale = scale;
            }
        }
        else
        {
            var sharedMaterial = GetComponent<Renderer>().sharedMaterial;
            sharedMaterial.DisableKeyword("USE_QUAD_DECAL");
            sharedMaterial.SetInt("_ZTest1", (int)UnityEngine.Rendering.CompareFunction.Greater);
        }

        if (Application.isPlaying && UseRandomRotationAndScale && !UseWorldSpaceRotation)
        {
            transform.localRotation = Quaternion.Euler(Random.Range(0, 360), 90, 90);
            var randomScaleRange = Random.Range(startScale.x - startScale.x * randomScalePercent * 0.01f,
                startScale.x + startScale.x * randomScalePercent * 0.01f);
            transform.localScale = new Vector3(randomScaleRange, IsScreenSpace ? startScale.y : 0.001f, randomScaleRange);
        }
    }

    void LateUpdate()
    {
        Matrix4x4 invTransformMatrix = transform.worldToLocalMatrix;
        // mat.SetMatrix("_InverseTransformMatrix", invTransformMatrix);
        if (props == null) props = new MaterialPropertyBlock();
        if (rend == null) rend = GetComponent<MeshRenderer>();
        rend.GetPropertyBlock(props);
       
        props.SetMatrix("_InverseTransformMatrix", invTransformMatrix);
        rend.SetPropertyBlock(props);
        
        if (ps != null) psMain.scalingMode = ParticleSystemScalingMode.Hierarchy;

        if (UseWorldSpaceRotation)
        {
            transform.rotation = Quaternion.Euler(worldRotation);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(this.transform.TransformPoint(Vector3.zero), this.transform.rotation, this.transform.lossyScale);
        Gizmos.color = new Color(1, 1, 1, 1);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
