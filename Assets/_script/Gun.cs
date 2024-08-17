using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour
{
    [SerializeField] private float fireRate; // The rate at which the gun fires (time between shots)
    [SerializeField] private TrailRenderer bulletTrailPref; // Prefab for the bullet trail effect
    [SerializeField] private Transform bulletOutPos; // The position from which the bullets are fired
    [SerializeField] private float BulletSpeed = 20; // Speed of the bullet trail
    [SerializeField] private ParticleSystem[] fireParticle; // Array of particle systems for muzzle flash effects
    [SerializeField] private InputAction ShotAction; // Input action for shooting
    private XRGrabInteractable xRGrab; // Reference to the XRGrabInteractable component
    private AudioSource audioSource; // Reference to the AudioSource component for gunshot sounds
    private bool ReadyToShot = true; // Boolean to check if the gun is ready to shoot

    private void OnEnable()
    {
        // Enable the shooting action when the script is enabled
        ShotAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the shooting action when the script is disabled
        ShotAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize components
        xRGrab = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if the gun is held by the player and the shoot action is pressed
        if (xRGrab.isSelected)
        {
            if (ShotAction.IsPressed() && ReadyToShot)
            {
                // If ready to shoot, start the shooting coroutine
                StartCoroutine(Fire());
            }
        }
    }

    IEnumerator Fire()
    {
        // Coroutine to handle shooting
        ReadyToShot = false; // Prevents shooting until the next shot is ready
        PlayShootingSystem(); // Play muzzle flash and shooting effects
        audioSource.PlayOneShot(audioSource.clip); // Play gunshot sound

        // Raycast to detect hit
        if (Physics.Raycast(bulletOutPos.position, bulletOutPos.forward, out RaycastHit hit, float.MaxValue, 1, QueryTriggerInteraction.Ignore))
        {
            // If the raycast hits something, create a bullet trail towards the hit point
            TrailRenderer trail = Instantiate(bulletTrailPref, bulletOutPos.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit.point, hit, true));
        }
        else
        {
            // If the raycast doesn't hit anything, create a bullet trail in the forward direction
            TrailRenderer trail = Instantiate(bulletTrailPref, bulletOutPos.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, bulletOutPos.position + bulletOutPos.forward * 100, hit, false));
        }

        yield return new WaitForSeconds(fireRate); // Wait for the fire rate duration before allowing the next shot

        ReadyToShot = true; // Allow shooting again
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, RaycastHit hit, bool isHitting)
    {
        // Coroutine to animate the bullet trail
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            // Move the trail towards the hit point
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= BulletSpeed * Time.deltaTime;
            yield return null;
        }

        // If the trail hits an enemy, trigger the enemy's death event
        if (isHitting && hit.collider.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.OnDying.Invoke();
        }

        // Set the final position of the trail and destroy it after its duration ends
        Trail.transform.position = HitPoint;
        Destroy(Trail.gameObject, Trail.time);
    }

    private void PlayShootingSystem()
    {
        // Play all the particle systems for muzzle flash effects
        foreach (var particle in fireParticle)
        {
            particle.Play();
        }
    }
}
