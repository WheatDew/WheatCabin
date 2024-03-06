using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class CharacterEntity : Entity
{

    public Transform weaponPoint;
    public Weapon weapon;
    public CHitbox hitBox;
    public HashSet<int> hitObjects = new HashSet<int>();
    public Animator animator;
    public bool isHitboxDisplay = false;

    public override void Init()
    {
        if (data.Map.ContainsKey(DataKey.LifeBar))
        {
            SLifeBar.s.CreateLifeBar(data);
        }
        //if (data.Map.ContainsKey(DataKey.Weapon))
        //{
        //    BattleSystem.s.SetWeapon(data);
        //}

        animator = GetComponent<Animator>();
        gameObject.layer = 7;
        gameObject.tag = "Character";
        RangeCalculateSystem.s.Add(this);
    }

    public void SetHitbox(CHitbox prefab,Vector3 position,Vector3 scale)
    {
        if (hitBox != null)
            Destroy(hitBox.gameObject);
        hitBox = Instantiate(prefab);
        hitBox.name = "HitBox";
        hitBox.tags.Add("Character");
        hitBox.transform.parent = transform;
        hitBox.transform.localPosition = position;
        hitBox.transform.localScale = scale;
        hitBox.onTriggerEnter.AddListener(delegate(Collider other)
        {
            
            if (other.gameObject != gameObject)
                hitObjects.Add(other.transform.GetInstanceID());
        });
    }

    public void DisplayHitbox()
    {
        isHitboxDisplay = true;
    }

    public void HiddenHitbox()
    {
        isHitboxDisplay = false;
    }

    public void EnableHitbox(Vector3 position, Vector3 scale)
    {
        hitObjects.Clear();
        if (isHitboxDisplay)
            hitBox.meshRenderer.enabled = true;
        hitBox.boxCollider.enabled = true;
        hitBox.transform.localPosition = position;
        hitBox.transform.localScale = scale;

    }

    public void DisableHitbox(float damageValue)
    {
        Debug.Log(hitObjects.Count);
        foreach(var item in hitObjects)
        {
            
            CharacterEntity enemy = PropertyMap.s.GetEntity<CharacterEntity>(item);
            Debug.LogFormat("{0} {1}", gameObject.name, enemy.gameObject.name);
            enemy.Damage(damageValue,this);
        }
        hitObjects.Clear();
        hitBox.meshRenderer.enabled = false;
        hitBox.boxCollider.enabled = false;

    }

    public void Damage(float value,CharacterEntity origin)
    {
        INya health = data.Map[DataKey.HealthPoint].List[0];
        health.Float -= value;
        if (health.Float > 0)
            HitAnimation(origin);
        else
        {
            health.Float = 0;
            animator.SetTrigger("Die");
            transform.tag = "Die";
        }
    }

    public void HitAnimation(CharacterEntity origin)
    {
        // 获取 Enemy 和 Origin 的位置信息
        Vector3 selfPosition = transform.position;
        Vector3 originPosition = origin.transform.position;



        // 计算 Origin 相对于 Enemy 的位置差
        Vector3 offset = originPosition - selfPosition;

        // 判断 Origin 位于哪个区域
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
        {
            if (offset.x > 0)
            {
                // Origin 位于右面
                animator.SetTrigger("HitRight");
            }
            else
            {
                // Origin 位于左面
                animator.SetTrigger("HitLeft");
            }
        }
        else
        {
            if (offset.z > 0)
            {
                // Origin 位于前面
                animator.SetTrigger("HitForward");
            }
            else
            {
                // Origin 位于后面
                animator.SetTrigger("HitBack");
            }
        }
    }


    //public RFX1_EffectAnimatorProperty Effect1;
    //public GameObject Target;

    //[HideInInspector] public float HUE = -1;
    //[HideInInspector] public float Speed = -1;

    //[System.Serializable]
    //public class RFX1_EffectAnimatorProperty
    //{
    //    public GameObject Prefab;
    //    public Transform BonePosition;
    //    public Transform BoneRotation;
    //    public float DestroyTime = 10;
    //    [HideInInspector] public GameObject CurrentInstance;
    //}

    //public void ActivateEffect1()
    //{
    //    InstantiateEffect(Effect1);
    //}

    //void InstantiateEffect(RFX1_EffectAnimatorProperty effect)
    //{
    //    if (effect.Prefab == null) return;
    //    effect.CurrentInstance = Instantiate(effect.Prefab, effect.BonePosition.position, effect.BoneRotation.rotation);

    //    //if (Target != null)
    //    //{
    //    //    var target = effect.CurrentInstance.GetComponent<RFX1_Target>();
    //    //    if (target != null) target.Target = Target;
    //    //}
    //    //if (effect.DestroyTime > 0.001f) Destroy(effect.CurrentInstance, effect.DestroyTime);
    //}
}
