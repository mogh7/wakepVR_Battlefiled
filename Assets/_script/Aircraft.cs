using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Aircraft : MonoBehaviour
{
    // Serialized fields allow customization in the Unity Editor
    [SerializeField] private GameObject xRRig; // Reference to the XR Rig object
    [SerializeField] private Transform xRRigAirPos; // Position of XR Rig when the aircraft is in use (flying)
    [SerializeField] private Transform xRRigOutAirPos; // Position of XR Rig when the aircraft is not in use
    [SerializeField] private GameObject locomotionSys; // Reference to the locomotion system (movement)
    [SerializeField] private GameObject bodyObj; // Reference to the aircraft's body object

    [Header("Material")]
    [SerializeField] private Material originalAirMat; // Original material of the aircraft
    [SerializeField] private Material useAirMat; // Material to apply when aircraft is in use (flying)

    [Header("Aircraft Properties")]
    [SerializeField] private float flySpeed; // Speed of the aircraft while flying
    [SerializeField] private float rotationSpeed; // Speed at which the aircraft rotates
    [SerializeField] private float moveSpeed = 4; // Speed of the aircraft's movement
    [SerializeField] private float MaxFly = 30; // Maximum altitude the aircraft can reach

    [Header("Events")]
    [SerializeField] private UnityEvent onUseAir, onNotUseAir; // Events triggered when aircraft is used or not used

    [Header("Input Actions")]
    [SerializeField] private InputAction flyAction, moveAction, exitAction, shootAction; // Input actions for flying, moving, exiting, and shooting

    private bool isUseAir = false; // Boolean to check if the aircraft is currently in use
    private float distToGround; // Distance from the aircraft to the ground
    private Rigidbody rb; // Rigidbody component for physics-based movement
    private Vector3 fly_d; // Vector3 to store flying direction

    private Animator animator; // Animator component to handle animations
    private AudioSource audioSource; // AudioSource component to handle audio
    private Vector2 d_Rotation; // Vector2 to store rotation values

    private void OnEnable()
    {
        // Enable input actions when the script is enabled
        flyAction.Enable();
        moveAction.Enable();
        exitAction.Enable();
        shootAction.Enable();
    }

    private void OnDisable()
    {
        // Disable input actions when the script is disabled
        flyAction.Disable();
        moveAction.Disable();
        exitAction.Disable();
        shootAction.Disable();
    }

    private void Awake()
    {
        // Initialize components at the start
        distToGround = GetComponent<Collider>().bounds.extents.y;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        // Main logic for flying the aircraft
        if (isUseAir)
        {
            // Listen for the exit action to stop flying
            exitAction.performed += ctx => NotUseAir();

            // Set XR Rig position and rotation to flying position
            xRRig.transform.position = xRRigAirPos.position;
            xRRig.transform.rotation = xRRigAirPos.rotation;

            // Read input for flying and moving
            var fly = flyAction.ReadValue<Vector2>();
            var move = moveAction.ReadValue<Vector2>();

            // Apply flying and moving logic
            Fly(fly);
            Move(move);
        }
    }

    private void Move(Vector2 move_d)
    {
        // Handle movement input

        if (move_d.sqrMagnitude < .01)
        {
            return; // Ignore small movements
        }

        if (IsGround())
            return; // Prevent movement if on the ground

        var scaledRotateSpeed = rotationSpeed * Time.deltaTime;
        d_Rotation.y += move_d.x * scaledRotateSpeed; // Apply rotation based on input
        transform.localEulerAngles = d_Rotation;

        var move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(move_d.x, 0, move_d.y);
        rb.AddForce(move * moveSpeed, ForceMode.Force); // Apply movement force
    }

    private void Fly(Vector2 fly)
    {
        // Handle flying input

        if (fly.sqrMagnitude < .01)
            return; // Ignore small flying input

        if (fly.y < 0 && IsGround())
            return; // Prevent descending if on the ground

        if (fly.y > 0 && transform.position.y >= MaxFly)
            return; // Prevent ascending beyond max altitude

        fly_d = new Vector3(0, fly.y, 0);
        rb.AddForce(fly_d * flySpeed, ForceMode.Force); // Apply flying force
    }

    public void UseAir()
    {
        // Activate aircraft use (flying mode)
        rb.useGravity = false;
        animator.SetBool("useAir", true);

        // Change aircraft material to indicate flying mode
        if (bodyObj.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            meshRenderer.material = useAirMat;
        }

        // Disable character controller and locomotion system during flight
        xRRig.GetComponent<CharacterController>().enabled = false;
        locomotionSys.SetActive(false);
        onUseAir.Invoke(); // Trigger the on-use event

        // Set XR Rig position and rotation to flying position
        xRRig.transform.position = xRRigAirPos.position;
        xRRig.transform.rotation = xRRigAirPos.rotation;
        isUseAir = true; // Set flag to indicate the aircraft is in use
    }

    public void NotUseAir()
    {
        // Deactivate aircraft use (exit flying mode)
        if (!IsGround())
            return; // Prevent exiting flying mode if not on the ground

        rb.useGravity = true;
        animator.SetBool("useAir", false);

        // Revert aircraft material to original
        if (bodyObj.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            meshRenderer.material = originalAirMat;
        }

        // Enable character controller and locomotion system
        xRRig.GetComponent<CharacterController>().enabled = true;
        locomotionSys.SetActive(true);
        onNotUseAir.Invoke(); // Trigger the on-not-use event

        // Set XR Rig position and rotation to ground position
        xRRig.transform.position = xRRigOutAirPos.position;
        xRRig.transform.rotation = xRRigAirPos.rotation;
        xRRig.transform.localRotation = new Quaternion(0, 1, 0, 1) * xRRig.transform.rotation;

        isUseAir = false; // Set flag to indicate the aircraft is not in use
    }

    private bool IsGround()
    {
        // Check if the aircraft is on the ground
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.5f);
    }
}

