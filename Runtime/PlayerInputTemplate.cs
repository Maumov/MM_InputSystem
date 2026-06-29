using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private InputActions actions;

    public Vector2 Move
    {
        get; private set;
    }
    public bool JumpPressed
    {
        get; private set;
    }

    void Awake()
    {
        actions = new InputActions();
    }

    void OnEnable()
    {
        actions.Enable();

        //actions.Player.Jump.performed += OnJump;
    }

    void OnDisable()
    {

        //actions.Player.Jump.performed -= OnJump;

        actions.Disable();
    }

    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private void OnJump( InputAction.CallbackContext ctx )
    {
        jumpBufferCounter = jumpBufferTime;
    }

    public void ConsumeJump()
    {
        JumpPressed = false;
        jumpBufferCounter = 0;
    }

    void Update()
    {
        ProcessMoveInput();

        jumpBufferCounter -= Time.deltaTime;
        JumpPressed = jumpBufferCounter > 0;
    }

    [Header( "Move Axis" )]
    public float currentX;
    public float currentY;
    Vector2 rawMoveInput;
    public float smoothTime = 0.1f;  // Smooth time for the input values (how fast the input changes)

    public float clampMin = -1f;     // Minimum input value
    public float clampMax = 1f;      // Maximum input value
    public float epsilon = 0.05f;    // Threshold to handle "stickiness" at boundaries

    private float currentMoveInputX = 0f; // Current smoothed input value for X (horizontal)
    private float currentMoveInputY = 0f; // Current smoothed input value for Y (vertical)

    // Velocity variables for SmoothDamp
    private float velocityX = 0f;
    private float velocityY = 0f;
    void ProcessMoveInput()
    {
        // Get the raw movement input (raw input from keyboard or controller)
        rawMoveInput = actions.Gameplay.Move.ReadValue<Vector2>();
        
        //Check if input is in the contrary direction to snap faster to the correct direction.
        if ( (rawMoveInput.x > 0 && currentMoveInputX < 0 ) || ( rawMoveInput.x < 0 && currentMoveInputX > 0 ) )
        {
            currentMoveInputX = rawMoveInput.x;
        }
        if ( (rawMoveInput.y > 0 && currentMoveInputY < 0) || ( rawMoveInput.y < 0 && currentMoveInputY > 0 )  )
        {
            currentMoveInputY = rawMoveInput.y;
        }
        
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

        Move = new Vector2( currentMoveInputX, currentMoveInputY );

        // Normalize the direction to avoid diagonal movement being faster
        if ( Move.magnitude > 1 )
            Move.Normalize();

        currentX = currentMoveInputX;
        currentY = currentMoveInputY;

    }


}
