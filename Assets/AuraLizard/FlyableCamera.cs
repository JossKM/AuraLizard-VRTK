/*
 * Based on unlicensed code
 * Source: https://gist.github.com/6ilberM/4fbee7680c0cc5de1a0c47bda0530d63
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Flyable Camera Controller")]
public class FlyableCamera : MonoBehaviour
{
    public float lookSpeedH = 5f;
    public float lookSpeedV = 5f;
    public float zoomSpeed = 2f;
    public float moveSpeed = 5f;
    public float sprintMultiplier = 5f;

    private float yaw;
    private float pitch;
    private float mouseWheelLast = 0.0f;

    [SerializeField]
    Camera camera;

    [SerializeField]
    App app;

    [SerializeField]
    private LayerMask interactibleLayerMask;

    [SerializeField]
    private bool b_shouldMove = true;

    private void Awake()
    { EnhancedTouchSupport.Enable(); }

    private void Start()
    {
        camera = GetComponent<Camera>();
        // x - right    pitch
        // y - up       yaw
        // z - forward  roll
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        mouseWheelLast = Mouse.current.scroll.y.ReadValue();
    }

    private void FlyMovement()
    {
       // if (Keyboard.current.spaceKey.wasReleasedThisFrame) { b_shouldMove = !b_shouldMove; }

        if (b_shouldMove)
        {
            if (Touch.activeTouches.Count > 0)
            {
                float touchToMouseScale = 0.25f;
                // look around with first touch
                //Touch t0 = Input.GetTouch(0);
                var t0 = Touch.activeTouches[0];
                yaw += lookSpeedH * touchToMouseScale * t0.delta.x;
                pitch -= lookSpeedV * touchToMouseScale * t0.delta.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0f);

                // and if have extra touch, also fly forward
                if (Touch.activeTouches.Count > 1)
                {
                    Touch t1 = Touch.activeTouches[1];
                    Vector3 offset = new Vector3(t1.delta.x, 0, t1.delta.y);
                    transform.Translate(offset * Time.deltaTime * touchToMouseScale, Space.Self);
                }
            }
            else
            {
                //Look around with Mouse
                if (Mouse.current.rightButton.isPressed)
                {
                    yaw   += camera.fieldOfView * lookSpeedH * Time.deltaTime * Mouse.current.delta.x.ReadValue();
                    pitch -= camera.fieldOfView * lookSpeedV * Time.deltaTime * Mouse.current.delta.y.ReadValue();
                    pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);

                    transform.eulerAngles = new Vector3(pitch, yaw, 0f);

                    Vector3 offset = Vector3.zero;
                    float offsetDelta = Time.deltaTime * moveSpeed * camera.fieldOfView;
                    if (Keyboard.current.leftShiftKey.isPressed) offsetDelta *= sprintMultiplier;
                    if (Keyboard.current.sKey.isPressed) offset.z -= offsetDelta;
                    if (Keyboard.current.wKey.isPressed) offset.z += offsetDelta;
                    if (Keyboard.current.aKey.isPressed) offset.x -= offsetDelta;
                    if (Keyboard.current.dKey.isPressed) offset.x += offsetDelta;
                    if (Keyboard.current.qKey.isPressed) offset.y -= offsetDelta;
                    if (Keyboard.current.eKey.isPressed) offset.y += offsetDelta;

                    transform.Translate(offset, Space.Self);
                }

                //drag camera around with Middle Mouse
                if (Mouse.current.middleButton.isPressed)
                {
                    transform.Translate(-Mouse.current.delta.x.ReadValue() * Time.deltaTime * moveSpeed * camera.fieldOfView,
                        -Mouse.current.delta.y.ReadValue() * Time.deltaTime * moveSpeed * camera.fieldOfView, 0);
                }


                if(Mouse.current.scroll.IsActuated())
                {
                    float mouseWheelNew = Mouse.current.scroll.y.ReadValue();
                    float deltaWheel = mouseWheelNew - mouseWheelLast;
                    camera.fieldOfView = Mathf.Clamp(camera.fieldOfView - (deltaWheel * Time.deltaTime * zoomSpeed), 5.0f, 120.0f);

                    //Zoom in and out with Mouse Wheel
                    //transform.Translate(0, 0, deltaWheel * Time.deltaTime * zoomSpeed, Space.Self);
                    //mouseWheelLast = mouseWheelNew;
                }
            }
        }
    }

    void Update()
    {
        if (!enabled) return;

        FlyMovement();


        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, interactibleLayerMask))
        {
            Node node = hitInfo.collider.gameObject.GetComponentInParent<Node>();

            if(node == null)
            {
                Debug.LogError("node raycast hit something else...?");
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && !app.isEditModeOn)
            {
                app.SelectNode(node);
                node.Ping(1.0f, 0.0f);
            }
            else if (Mouse.current.leftButton.isPressed && app.isEditModeOn) // Drag
            {
                Plane dragPlane = new Plane(-camera.transform.forward, node.transform.position);
                if(dragPlane.Raycast(ray, out float enter)); // project mouse position onto the plane of view
                {
                    node.transform.position = ray.GetPoint(enter); // Set node to that position
                    node.eventOnPositionChanged.Invoke();
                    app.HoverNode(node);
                    app.SelectNode(node);
                }
                
            } else
            {
                app.HoverNode(node);
            }


        } else // Not over a node
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                app.SelectNode(null);
            } else
            {
                app.HoverNode(null);
            }
        }
    }

}