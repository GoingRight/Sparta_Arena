using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    public GameObject sword;
    private Collider attackCollider;

    private void Awake()
    {
        if (sword != null)
        {
            attackCollider = sword.GetComponentInChildren<Collider>();
        } else
        {
            Debug.LogError("SwordHandler: sword is null");
        }
    }

    public void EnableCollider()
    {
        attackCollider.enabled = true; // 콜리더 활성화
    }

    public void DisableCollider()
    {
        attackCollider.enabled = false; // 콜리더 비활성화
    }

}
