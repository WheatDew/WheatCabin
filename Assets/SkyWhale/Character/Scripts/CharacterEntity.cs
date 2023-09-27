using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntity : Entity
{

    public Transform weaponPoint;
    public Weapon weapon;
    public List<CHitbox> hitBoxs=new List<CHitbox>();
    public Animator animator;

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
    }
}
