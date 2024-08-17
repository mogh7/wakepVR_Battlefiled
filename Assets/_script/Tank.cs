using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Tank : MonoBehaviour
{
    private bool isUseTank = false; // Tracks whether the tank is currently being used

    [SerializeField] private float moveSpeed; // Speed at which the tank moves
    [SerializeField] private float rotateSpeed; // Speed at which the tank rotates
    [SerializeField] private GameObject XrOrigin; // Reference to the XR Origin object
    [SerializeField] private Transform XRTargetPos; // Target position for the XR Origin when using the tank
    [SerializeField] private Transform outTargetPos; // Target position for the XR Origin when exiting the tank
    [SerializeField] private GameObject locomotionSys; // Locomotion system that will be disabled when in the tank
    [SerializeField] private GameObject downTank; // The lower part of the tank that handles movement
    [SerializeField] private GameObject upTank; // The upper part of the tank that handles rotation

    [SerializeField] private UnityEvent onUseTank, onNotUseTank; // Events triggered when entering or exiting the tank
    [SerializeField] private InputAction MoveTankAction, ShootTankAction, ExitTankAction; // Input actions for controlling the tank

    private Vector2 d_Rotation; // Rotation data for the tank
    private Rigidbody _rigidbody; // Reference to the Rigidbody component for physics-based movement
    private AudioSource audioSource; // Reference to the AudioSource component for sound effects

    private void Awake()
    {
        // Initialize Rigidbody and AudioSource components in the Awake method
        _rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Enable input actions when the script is enabled
        MoveTankAction.Enable();
        ShootTankAction.Enable();
        ExitTankAction.Enable();
    }

    private void OnDisable()
    {
        // Disable input actions when the script is disabled
        MoveTankAction.Disable();
        ShootTankAction.Disable();
        ExitTankAction.Disable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // If the tank is in use, allow movement and rotation
        if (isUseTank)
        {
            XrOrigin.transform.position = XRTargetPos.position; // Keep the XR Origin at the tank's position

            var move = MoveTankAction.ReadValue<Vector2>(); // Get movement input

            Move(move); // Call the Move method to handle tank movement

            ExitTankAction.performed += ctx => onNotUseTankToggle(); // Check for the exit action
            ShootTankAction.performed += ctx => FireTank(); // Check for the shoot action
        }
    }

    private void FireTank()
    {
        // Method to handle firing the tank's weapon
        print("FireFrom tank");
    }

    private void Move(Vector2 direction)
    {
        // Method to handle tank movement
        if (direction.sqrMagnitude < 0.01)
        {
            // If there's no significant movement input, adjust the pitch of the engine sound based on velocity
            audioSource.pitch = Mathf.Clamp(_rigidbody.velocity.sqrMagnitude, 1, 1.3f);
            return;
        }

        var rotateScale = rotateSpeed * Time.deltaTime; // Scale rotation by time
        d_Rotation.y += direction.x * rotateScale; // Apply rotation input to the tank
        downTank.transform.localEulerAngles = d_Rotation; // Apply the rotation to the lower part of the tank

        var move = Quaternion.Euler(0, downTank.transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y); // Calculate movement direction
        _rigidbody.AddForce(move * moveSpeed, ForceMode.Force); // Apply force to move the tank
        audioSource.pitch = Mathf.Clamp(_rigidbody.velocity.sqrMagnitude, 1, 1.3f); // Adjust the pitch of the engine sound
    }

    public void onUseTankToggle()
    {
        // Method to handle entering the tank
        isUseTank = true;
        XrOrigin.transform.position = XRTargetPos.position; // Position the XR Origin at the tank
        XrOrigin.transform.rotation = XRTargetPos.rotation; // Align rotation

        locomotionSys.SetActive(false); // Disable the locomotion system
        onUseTank.Invoke(); // Trigger the event for entering the tank
    }

    public void onNotUseTankToggle()
    {
        // Method to handle exiting the tank
        isUseTank = false;
        XrOrigin.transform.position = outTargetPos.position; // Move the XR Origin to the exit position
        XrOrigin.transform.rotation = outTargetPos.rotation; // Align rotation
        locomotionSys.SetActive(true); // Enable the locomotion system

        onNotUseTank.Invoke(); // Trigger the event for exiting the tank
    }
}
