using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private Animator animator; // Reference to the Animator component for controlling animations
    private NavMeshAgent agent; // Reference to the NavMeshAgent component for AI navigation
    private Transform playerTrans; // Reference to the player's transform

    public UnityEvent OnDying; // Event triggered when the enemy is dying

    void Start()
    {
        // Initialize components at the start
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Update is called once per frame
        if (playerTrans && agent.enabled)
        {
            // If the player is found and the NavMeshAgent is enabled, move towards the player
            agent.SetDestination(playerTrans.position);

            // Check if the enemy is within attacking distance of the player
            if (Vector3.Distance(transform.position, playerTrans.position) < agent.stoppingDistance)
            {
                // Face the player and trigger the attack animation
                transform.LookAt(new Vector3(playerTrans.position.x, 0, playerTrans.position.z));
                animator.SetBool("gunPlay", true);
            }
            else
            {
                // Stop the attack animation if the player is out of range
                animator.SetBool("gunPlay", false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect when the enemy collides with a trigger collider
        if (other.gameObject.TryGetComponent<XROrigin>(out XROrigin xROrigin))
        {
            // If the collided object is the player, set the player transform and trigger an animation
            playerTrans = xROrigin.Camera.gameObject.transform;
            animator.SetTrigger("playerFound");
        }
    }
}
