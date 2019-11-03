using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeanutAgent : Agent
{
    [Header("Pig Agent Settings")]
    public float moveSpeed = 1f;
    public float rotateSpeed = 2f;
    public float nostrilWidth = .5f;
    public GameObject eye;
    private PeanutAcademy agentAcademy;
    private PeanutArea agentArea;
    private Rigidbody2D agentRigidbody;
    private RayPerception rayPerception;

    private int trufflesCollected = 0;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        agentAcademy = FindObjectOfType<PeanutAcademy>();
        agentArea = transform.parent.GetComponent<PeanutArea>();
        agentRigidbody = GetComponent<Rigidbody2D>();
        rayPerception = GetComponent<RayPerception2D>();
    }

    /// <summary>
    /// Collect all observations that the agent will use to make decisions
    /// </summary>
    public override void CollectObservations()
    {

        // Add raycast perception observations for stumps and walls
        float rayDistance = 100f;
        float[] rayAngles = { 0f,45f,90f,135f,180f };
        string[] detectableObjects = { "stump", "wall" };
        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

        // Sniff for truffles
        AddVectorObs(GetNostrilStereo());

        // Add velocity observation
        Vector3 localVelocity = transform.InverseTransformDirection(agentRigidbody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.y);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Determine the rotation action
        float rotateAmount = 0;
        if (vectorAction[1] == 1)
        {
            rotateAmount = -rotateSpeed;
        }
        else if (vectorAction[1] == 2)
        {
            rotateAmount = rotateSpeed;
        }

        // Apply the rotation
        /* float rotateVector =  rotateAmount;
         agentRigidbody.MoveRotation((agentRigidbody.rotation + rotateVector * rotateSpeed));*/
        Vector3 rotVector = (transform.right) * rotateAmount;

        agentRigidbody.AddForce(rotVector * moveSpeed, ForceMode2D.Impulse);

        // Determine move action
        float moveAmount = 0;
        if (vectorAction[0] == 1)
        {
            moveAmount = moveSpeed;
        }
        else if (vectorAction[0] == 2)
        {
            moveAmount = moveSpeed * -.5f; // move at half-speed going backwards
        }

        // Apply the movement
        Vector3 moveVector = (transform.up) * moveAmount;

        agentRigidbody.AddForce(moveVector * moveSpeed, ForceMode2D.Impulse);

        // Determine state
        if (GetCumulativeReward() <= -5f)
        {
            // Reward is too negative, give up
            Done();

            // Indicate failure with the ground material
            StartCoroutine(agentArea.SwapGroundMaterial(success: false));

            // Reset
            agentArea.ResetArea();
        }
        else if (trufflesCollected >= agentArea.GetSmellyObjects().Count)
        {
            // All truffles collected, success!
            Done();

            // Indicate success with the ground material
            StartCoroutine(agentArea.SwapGroundMaterial(success: true));

            // Reset
            agentArea.ResetArea();
        }
        else
        {
            // Encourage movement with a tiny time penalty and pdate the score text display
            AddReward(-.001f);
            agentArea.UpdateScore(GetCumulativeReward());
        }
    }

    /// <summary>
    /// Reset the agent
    /// </summary>
    public override void AgentReset()
    {
        // Reset velocity
        agentRigidbody.velocity = Vector3.zero;

        // Reset number of truffles collected
        trufflesCollected = 0;
    }

    /// <summary>
    /// Calculates the strength of smell in each nostril
    /// </summary>
    /// <returns>A Vector2 where x is the left nostril and y is the right nostril</returns>
    private Vector2 GetNostrilStereo()
    {   

        List<GameObject> smellyObjects = agentArea.GetSmellyObjects();
        if (smellyObjects == null)
            return Vector2.zero;

        float leftNostril = 0;
        Vector3 leftNostrilPosition = transform.position - nostrilWidth / 2.0f * transform.right;
        float rightNostril = 0;
        Vector3 rightNostrilPosition = transform.position + nostrilWidth / 2.0f * transform.right;

        foreach (GameObject smellyObject in smellyObjects)
        {
            if (smellyObject != null)
            {
                leftNostril += .8f - .5f * Mathf.Log10(Vector3.Distance(smellyObject.transform.position, leftNostrilPosition));
                rightNostril += .8f - .5f * Mathf.Log10(Vector3.Distance(smellyObject.transform.position, rightNostrilPosition));
            }
        }
        Vector3 eyev= transform.position;
        return new Vector2(eyev[0], eyev[1]);
    }

    /// <summary>
    /// React to a collision
    /// </summary>
    /// <param name="collision">The collision description</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("truffle"))
        {
            CollectTruffle();
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("wall"))
        {
            AddReward(-.01f);
        }
    }

    /// <summary>
    /// Collect a truffle and increment relevant counters
    /// </summary>
    private void CollectTruffle()
    {
        trufflesCollected++;

        // Reward and update the score text display
        AddReward(1f);
        agentArea.UpdateScore(GetCumulativeReward());
    }
}
