using UnityEngine;

public class KeybindsInterfaceTemplate : MonoBehaviour
{
    public float currentX;
    public float currentY;

    public float smoothTime = 0.1f;  // Smooth time for the input values (how fast the input changes)

    public float clampMin = -1f;     // Minimum input value
    public float clampMax = 1f;      // Maximum input value
    public float epsilon = 0.05f;    // Threshold to handle "stickiness" at boundaries


    private float currentMoveInputX = 0f; // Current smoothed input value for X (horizontal)
    private float currentMoveInputY = 0f; // Current smoothed input value for Y (vertical)
    private Vector3 moveDirection;
    private InputActions controls; //Input Asset

    // Velocity variables for SmoothDamp
    private float velocityX = 0f;
    private float velocityY = 0f;

    private void Awake()
    {
        // Initialize the controls
        controls = new InputActions();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Gameplay.Disable();
    }

    private void Update()
    {
        // Get the raw movement input (raw input from keyboard or controller)
        Vector2 rawMoveInput = controls.Gameplay.Move.ReadValue<Vector2>();

        // Smoothly transition the input values to the raw input with SmoothDamp
        currentMoveInputX = Mathf.SmoothDamp( currentMoveInputX, rawMoveInput.x, ref velocityX, smoothTime );
        currentMoveInputY = Mathf.SmoothDamp( currentMoveInputY, rawMoveInput.y, ref velocityY, smoothTime );

        // Clamp the smoothed input values to stay within the range of -1 and 1
        currentMoveInputX = Mathf.Clamp( currentMoveInputX, clampMin, clampMax );
        currentMoveInputY = Mathf.Clamp( currentMoveInputY, clampMin, clampMax );

        // Fix "stickiness" when close to 0 or max/min value by using epsilon
        if ( Mathf.Abs( currentMoveInputX ) < epsilon )
        {
            currentMoveInputX = 0f;  // Snap to 0 if close enough
        }
        // Snap to min or max if close enough
        else if ( Mathf.Abs( currentMoveInputX - clampMax ) < epsilon )
        {
            currentMoveInputX = clampMax;  // Snap to max if close enough
        }
        else if ( Mathf.Abs( currentMoveInputX - clampMin ) < epsilon )
        {
            currentMoveInputX = clampMin;  // Snap to min if close enough
        }

        if ( Mathf.Abs( currentMoveInputY ) < epsilon )
        {
            currentMoveInputY = 0f;  // Snap to 0 if close enough
        }
        else if ( Mathf.Abs( currentMoveInputY - clampMax ) < epsilon )
        {
            currentMoveInputY = clampMax;  // Snap to max if close enough
        }
        else if ( Mathf.Abs( currentMoveInputY - clampMin ) < epsilon )
        {
            currentMoveInputY = clampMin;  // Snap to min if close enough
        }
        // Create the move direction from the smoothed input values
        moveDirection = new Vector3( currentMoveInputX, 0, currentMoveInputY );

        // Normalize the direction to avoid diagonal movement being faster
        if ( moveDirection.magnitude > 1 )
            moveDirection.Normalize();

        currentX = currentMoveInputX;
        currentY = currentMoveInputY;
    }
}
