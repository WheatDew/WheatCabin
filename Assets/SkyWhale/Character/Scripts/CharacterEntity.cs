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
        if (data.Map.ContainsKey(DataKey.Weapon))
        {
            BattleSystem.s.SetWeapon(data);
        }

        animator = GetComponent<Animator>();
        gameObject.layer = 7;
        gameObject.tag = "Character";
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
            //enemy.Damage(100);
        }
        hitObjects.Clear();
        hitBox.meshRenderer.enabled = false;
        hitBox.boxCollider.enabled = false;

    }

    public void Damage(float value)
    {
        data.Map[DataKey.HealthPoint].List[0].Float -= value;
    }
}
