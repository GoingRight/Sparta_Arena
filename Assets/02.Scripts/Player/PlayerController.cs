using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    private Player player;
    public float speed;
    private Vector2 curMoveInput;
    private Rigidbody _rigidbody;
    private bool isSprint;

    [Header("Look")]
    private Vector2 curLookInput;
    [SerializeField] private Transform camContainer;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float maxXLook;
    [SerializeField] private float minXLook;
    [SerializeField] private bool CursurLockState;
    private float curCamX;
    private bool isZoom;
    Camera cam;

    private void Awake()
    {
        // Move
        _rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<Player>();

        // Look
        lookSpeed = 0.3f;
        Cursor.lockState = (CursurLockState) ? CursorLockMode.Locked : CursorLockMode.None;
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        Look();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase== InputActionPhase.Performed)
            curMoveInput = context.ReadValue<Vector2>();
        else if (context.phase== InputActionPhase.Canceled)
            curMoveInput = Vector2.zero;
    }

    private void Move()
    {
        Vector3 vec = transform.forward * curMoveInput.y + transform.right * curMoveInput.x;
        speed = (isSprint) ? player.stat.Speed + 2f : player.stat.Speed;
        vec *= speed;
        vec.y = _rigidbody.velocity.y;

        _rigidbody.velocity = vec;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        curLookInput = context.ReadValue<Vector2>();
    }

    private void Look()
    {
        curCamX += curLookInput.y * lookSpeed;
        curCamX = Mathf.Clamp(curCamX, minXLook, maxXLook);

        camContainer.localEulerAngles = new Vector3(-curCamX, 0, 0);
        transform.eulerAngles += new Vector3(0, curLookInput.x * lookSpeed, 0);

        if (isZoom)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 30f, 10f * Time.deltaTime);
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, 10f * Time.deltaTime);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            isSprint = true;
        else
            isSprint = false;
    }
}
