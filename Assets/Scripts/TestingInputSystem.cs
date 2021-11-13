using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static scr_Models;

public class TestingInputSystem : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInputActions playerInputActions;
    public Vector2 inputView;
    public Vector2 inputMovement;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")] 
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.MouseLook.performed += e => inputView = e.ReadValue<Vector2>();
        playerInputActions.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {

    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
    }

    private void CalculateView()
    {

        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -inputView.x : inputView.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? inputView.y : -inputView.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {
        var verticalSpeed = playerSettings.WalkingForwardSpeed * inputMovement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed * inputMovement.x * Time.deltaTime;

        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin && jumpingForce.y < 0.1f)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }

        if (playerGravity < -1 && characterController.isGrounded)
        {
            playerGravity = -1;
        }

        if (jumpingForce.y > 0.1f)
        {
            playerGravity = 0;
        }

        newMovementSpeed.y += playerGravity;

        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
        if (jumpingForce.y < 0.01f) jumpingForce.y = 0;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!characterController.isGrounded)
        {
            return;
        }
        Debug.Log("jump " + context.phase);

        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
    }
}
