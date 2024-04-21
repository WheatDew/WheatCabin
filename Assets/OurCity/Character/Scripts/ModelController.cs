using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    public Transform rightHand, fireEffect;

    public void SetWeapon(bool value)
    {
        rightHand.gameObject.SetActive(value);
    }

    public void SetGunFire(bool value)
    {
        fireEffect.gameObject.SetActive(value);
    }

    public void SetWeaponDisplay()
    {
        rightHand.gameObject.SetActive(true);
    }

    public void SetWeaponHidden()
    {
        rightHand.gameObject.SetActive(false);
    }

    public void SetGunFireDisplay()
    {
        fireEffect.gameObject.SetActive(true);
    }
    public void SetGunFireHidden()
    {
        fireEffect.gameObject.SetActive(false);
    }
}
