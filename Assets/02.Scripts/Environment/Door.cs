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
        //if (IsOpen)
        //{
        //    animate = OpenDoor;
        //    AnimateDoor(90);
        //}
        //else
        //{
        //    animate = CloseDoor;
        //    AnimateDoor(0);
        //}

        LeftDoor.SetBool("IsOpen", isOpen);
        RightDoor.SetBool("IsOpen", isOpen);

    }


    /// <summary>
    /// Door 애니메이션을 위한 메서드
    /// </summary>
    //void AnimateDoor(float endValue)
    //{ 
    //    animate(endValue);
    //}

    //float openValue = 90;
    //float closeValue = 0;
    //void OpenDoor(float endValue)
    //{
    //    float rotation = LeftDoor.transform.localEulerAngles.y;
    //    if (endValue <= Math.Abs(rotation)) 
    //    {
    //        rotation += 10 * Speed * Time.deltaTime;
    //    }
    //    else
    //    {
    //        rotation = endValue;
    //    }

    //    LeftDoor.transform.localEulerAngles = new Vector3(0, rotation, 0);
    //    RightDoor.transform.localEulerAngles = new Vector3(0, -rotation, 0);
    //}

    //void CloseDoor(float endValue) 
    //{
    //    float rotation = LeftDoor.transform.localEulerAngles.y;
    //    if (endValue >= Math.Abs(rotation))
    //    {
    //        rotation -= 10 * Speed * Time.deltaTime;
    //    }
    //    else
    //    {
    //        rotation = endValue;
    //    }

    //    LeftDoor.transform.localEulerAngles = new Vector3(0, rotation, 0);
    //    RightDoor.transform.localEulerAngles = new Vector3(0, -rotation, 0);
    //}
}
