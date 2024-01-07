using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class RFX1_PerPlatformSettings : MonoBehaviour
{
    public bool DisableOnMobiles;
    public GameObject[] DisabledMobileObjects;
    [Range(0.1f, 1)] public float ParticleBudgetForMobiles = 1f;

    // Use this for initialization
    private bool isMobile;


    void Awake()
    {
        isMobile = IsMobilePlatform();

    }

    private void Start()
    {
        if (isMobile)
        {
            if (DisableOnMobiles)
            {
                foreach (var go in DisabledMobileObjects)
                    go.SetActive(false);
            }
            else
            {
                if (ParticleBudgetForMobiles < 0.99f) ChangeParticlesBudget(ParticleBudgetForMobiles);
            }
        }
    }

    bool IsMobilePlatform()
    {
        bool isMobile = false;
#if UNITY_EDITOR
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
            || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS
            || EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer)
            isMobile = true;
#else
        if (Application.isMobilePlatform) isMobile = true;
#endif
        return isMobile;
    }

    void ChangeParticlesBudget(float particlesMul)
    {
        var particles = GetComponent<ParticleSystem>();
        if (particles == null) return;

        var main = particles.main;
        main.maxParticles = Mathf.Max(1, (int)(main.maxParticles * particlesMul));

        var emission = particles.emission;
        if (!emission.enabled) return;

        var rateOverTime = emission.rateOverTime;
        {
            if (rateOverTime.constantMin > 1) rateOverTime.constantMin *= particlesMul;
            if (rateOverTime.constantMax > 1) rateOverTime.constantMax *= particlesMul;
            emission.rateOverTime = rateOverTime;
        }

        var rateOverDistance = emission.rateOverDistance;
        if (rateOverDistance.constantMin > 1)
        {
            if (rateOverDistance.constantMin > 1) rateOverDistance.constantMin *= particlesMul;
            if (rateOverDistance.constantMax > 1) rateOverDistance.constantMax *= particlesMul;
            emission.rateOverDistance = rateOverDistance;
        }

        ParticleSystem.Burst[] burst = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(burst);
        for (var i = 0; i < burst.Length; i++)
        {

            if (burst[i].minCount > 1) burst[i].minCount = (short)(burst[i].minCount * particlesMul);
            if (burst[i].maxCount > 1) burst[i].maxCount = (short)(burst[i].maxCount * particlesMul);
        }
        emission.SetBursts(burst);
    }
}
