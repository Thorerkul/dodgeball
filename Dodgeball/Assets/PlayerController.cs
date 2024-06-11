using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public int playerID = -1;

    public bool isDummy = true;

    public GameObject referenceball;
    Rigidbody rb;
    public float deadZone = 0.2f;

    public bool isOnRedTeam = true;

    public Collider redZone;
    public Collider blueZone;

    bool canMove = true;

    public float accelerationSpeed = 50;
    public float topSpeed = 5;
    public float stopSpeed = 0.3f;

    public float dodgeCooldown = 1f; // Cooldown time for dodging in seconds
    public float dodgeSpeed = 10;

    public bool isDodging = false;
    public float lastDodgeTime = -Mathf.Infinity;

    public bool hasBall;
    public bool canPickupBall;
    public bool canThrow;
    public bool isInPickupRange;
    public GameObject pickupableBall;

    public int health = 5;
    public int maxHealth = 5;

    private Vector3 inputVector;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (isDummy)
        {
            canMove = false;
            canPickupBall = false;
            canThrow = false;
        }

        referenceball = GameObject.FindWithTag("ReferenceBall");
        redZone = GameObject.FindWithTag("RedZone").GetComponent<Collider>();
        blueZone = GameObject.FindWithTag("BlueZone").GetComponent<Collider>();
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();
        inputVector = new Vector3(input.x, 0, input.y);
    }

    public void OnThrow(InputAction.CallbackContext value)
    {
        
        if (hasBall && canThrow && value.phase == InputActionPhase.Performed)
        {
            Throw(inputVector);
        }
    }

    public void OnDodge(InputAction.CallbackContext value)
    {
        // Check for dodge input
        if (Time.time > lastDodgeTime + dodgeCooldown && value.phase == InputActionPhase.Performed)
        {
            StartCoroutine(Dodge(inputVector));
        }
    }

    public void OnPickup(InputAction.CallbackContext value)
    {
        if (canPickupBall && isInPickupRange)
        {
            hasBall = true;
            canPickupBall = false;
            Destroy(pickupableBall.transform.parent.gameObject);
        }
    }

    void Throw(Vector3 inputVector)
    {
        hasBall = false;
        canPickupBall = true;
        GameObject newball = Instantiate(referenceball);
        if (isOnRedTeam)
        {
            newball.transform.position = transform.position + Vector3.forward * 3;
            newball.GetComponent<Rigidbody>().velocity = (Vector3.forward * 50 + (inputVector * 50) + rb.velocity) / 2;
            newball.GetComponent<ballScript>().isThrownByRed = true;
        } else
        {
            newball.transform.position = transform.position + Vector3.back * 3;
            newball.GetComponent<Rigidbody>().velocity = (Vector3.back * 50 + (inputVector * 50) + rb.velocity) / 2;
            newball.GetComponent<ballScript>().isThrownByRed = false;
        }

        newball.GetComponent<ballScript>().isThrown = true;
        newball.GetComponent<ballScript>().knockBack = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }

        if (isDummy)
        {
            // Gradually reduce velocity to zero when no input is detected
            Vector3 velocityChange = -rb.velocity;
            velocityChange = Vector3.ClampMagnitude(velocityChange, stopSpeed * Time.deltaTime);
            rb.velocity += new Vector3(velocityChange.x, 0, velocityChange.z);
        }

        if (canMove && !isDodging)
        {
            if (inputVector.magnitude > deadZone)
            {
                // Calculate the target velocity
                Vector3 targetVelocity = inputVector * topSpeed;

                // Calculate the difference between the current velocity and the target velocity
                Vector3 velocityChange = targetVelocity - rb.velocity;

                // Limit the velocity change to the acceleration speed
                velocityChange = Vector3.ClampMagnitude(velocityChange, accelerationSpeed * Time.deltaTime);

                // Apply the velocity change to the Rigidbody
                rb.velocity += velocityChange;

                // Optionally, if you want to keep the rigidbody's y velocity (for gravity or jumping)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);

                // Rotate to face the direction of movement
                Quaternion targetRotation = Quaternion.LookRotation(inputVector);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);
            }
            else
            {
                // Gradually reduce velocity to zero when no input is detected
                Vector3 velocityChange = -rb.velocity;
                velocityChange = Vector3.ClampMagnitude(velocityChange, stopSpeed * Time.deltaTime);
                rb.velocity += new Vector3(velocityChange.x, 0, velocityChange.z);
            }
        }
    }

    private IEnumerator Dodge(Vector3 direction)
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        // Calculate dodge velocity
        Vector3 dodgeVelocity = direction.normalized * dodgeSpeed;

        // Apply dodge velocity
        rb.velocity = dodgeVelocity;

        // Wait for a short duration to simulate the dodge effect (e.g., 0.2 seconds)
        yield return new WaitForSeconds(0.2f);

        isDodging = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOnRedTeam && other.gameObject.layer == 10)
        {
            if (other == blueZone)
            {
                rb.transform.position = new Vector3(0, rb.transform.position.y, -12.5f);
            }
        } else if (!isOnRedTeam && other.gameObject.layer == 10)
        {
            if (other == redZone)
            {
                rb.transform.position = new Vector3(0, rb.transform.position.y,  12.5f);
            }
        }

        if (other.gameObject.layer == 8)
        {
            isInPickupRange = true;
            pickupableBall = other.gameObject;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            ballScript ball = collision.gameObject.GetComponent<ballScript>();
            if (ball.isThrownByRed != isOnRedTeam && ball.isThrown)
            {
                health -= 1;
                ball.isThrown = false;

                Vector3 forceDirection = new Vector3(transform.position.x - ball.transform.position.x, 1, transform.position.z - ball.transform.position.z);

                forceDirection.Normalize();

                rb.AddForce(forceDirection * ball.knockBack, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            isInPickupRange = false;
        }
    }
}
