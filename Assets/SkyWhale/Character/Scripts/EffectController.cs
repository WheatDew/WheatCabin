using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public RFX1_EffectAnimatorProperty Effect1;
    public GameObject Target;

    [HideInInspector] public float HUE = -1;
    [HideInInspector] public float Speed = -1;


    [System.Serializable]
    public class RFX1_EffectAnimatorProperty
    {
        public GameObject Prefab;
        public Transform BonePosition;
        public Transform BoneRotation;
        public float DestroyTime = 10;
        [HideInInspector] public GameObject CurrentInstance;
    }

    void InstantiateEffect(RFX1_EffectAnimatorProperty effect)
    {
        if (effect.Prefab == null) return;
        effect.CurrentInstance = Instantiate(effect.Prefab, effect.BonePosition.position, effect.BoneRotation.rotation);

        //if (Target != null)
        //{
        //    var target = effect.CurrentInstance.GetComponent<RFX1_Target>();
        //    if (target != null) target.Target = Target;
        //}
        //if (effect.DestroyTime > 0.001f) Destroy(effect.CurrentInstance, effect.DestroyTime);
    }

    public void ActivateEffect1()
    {
        InstantiateEffect(Effect1);
    }

}
