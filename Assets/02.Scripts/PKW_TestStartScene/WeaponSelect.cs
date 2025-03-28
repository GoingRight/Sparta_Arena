using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelect : MonoBehaviour
{
    public GameObject swordPrefab;
    public GameObject gunPrefab;
    private GameObject equippedWeapon;
    private string currentWeaponType = "";

    public Transform jointItemR;

    public void SelectWeapon(string weaponType)
    {
        if (equippedWeapon != null)
        {
            Destroy(equippedWeapon);
        }

        if (weaponType == "Sword")
        {
            equippedWeapon = Instantiate(swordPrefab, jointItemR);
        }
        else if (weaponType == "Gun")
        {
            equippedWeapon = Instantiate(gunPrefab, jointItemR);
        }

        if (currentWeaponType == weaponType)
        {
            Destroy(equippedWeapon);
            equippedWeapon = null;
            currentWeaponType = "";
            return;
        }

        currentWeaponType = weaponType;
    }
}
