using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
public Transform player;
    public float rotationSpeed = 5f;
    public float damping = 3f; // 감속 속도

    private Vector2 lastMousePosition;
    private bool isDragging = false;
    private float currentVelocity = 0f; // 현재 회전 속도
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))// 마우스 클릭 감지
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            
            currentVelocity = delta.x * rotationSpeed * Time.deltaTime; // 좌우 회전
        }

        if (!isDragging) // 서서히 멈추게
        {
            currentVelocity = Mathf.Lerp(currentVelocity, 0, Time.deltaTime * damping);
        }

        transform.RotateAround(player.position, Vector3.up, currentVelocity); // 회전 적용
    }
}