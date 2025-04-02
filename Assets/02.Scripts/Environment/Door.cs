using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator LeftDoor;
    public Animator RightDoor;

    public bool isOpen;
    public bool IsOpen
    {
        get { return isOpen; }
        set { isOpen = value; }
    }

    private void Update()
    {
        LeftDoor.SetBool("IsOpen", isOpen);
        RightDoor.SetBool("IsOpen", isOpen);
    }
}
