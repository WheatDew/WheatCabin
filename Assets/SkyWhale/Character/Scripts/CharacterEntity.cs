using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntity : Entity
{
    public static string LifeBarKey = "LifeBar";
    public static string WeaponKey = "Weapon";

    public Transform weaponPoint;
    public Weapon weapon;
    public Animator animator;

    public override void Init()
    {
        if (data.ContainsKey(LifeBarKey))
        {
            SLifeBar.s.CreateLifeBar(data);
        }
        if (data.ContainsKey(WeaponKey))
        {
            BattleSystem.s.SetWeapon(data);
        }

        animator = GetComponent<Animator>();
        gameObject.layer = 7;
    }
}
