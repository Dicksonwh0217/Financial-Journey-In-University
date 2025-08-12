using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    [SerializeField] float speed = 2f;
    [SerializeField] float runSpeed = 5f;
    Vector2 motionVector;
    public Vector2 lastMotionVector;
    Animator animator;
    public bool moving;
    bool running;
    bool manualAnimationControl = false; // Flag to prevent Update from controlling animations

    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            running = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            running = false;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        motionVector = new Vector2(horizontal, vertical);

        // Only update animator if not under manual control
        if (!manualAnimationControl)
        {
            animator.SetFloat("horizontal", horizontal);
            animator.SetFloat("vertical", vertical);

            moving = horizontal != 0 || vertical != 0;
            animator.SetBool("moving", moving);

            if (horizontal != 0 || vertical != 0)
            {
                lastMotionVector = new Vector2(horizontal, vertical).normalized;
                animator.SetFloat("lastHorizontal", horizontal);
                animator.SetFloat("lastVertical", vertical);
            }
        }
    }

    void FixedUpdate()
    {
        // Only apply physics-based movement if not under manual control
        if (!manualAnimationControl)
        {
            Move();
        }
    }

    private void Move()
    {
        rigidbody2d.linearVelocity = motionVector * (running == true ? runSpeed : speed);
    }

    private void OnDisable()
    {
        rigidbody2d.linearVelocity = Vector2.zero;
    }

    public void WalkDown()
    {
        StartCoroutine(SmoothWalkDown());
    }

    private IEnumerator SmoothWalkDown()
    {
        // Take manual control of animations and physics
        manualAnimationControl = true;
        rigidbody2d.linearVelocity = Vector2.zero;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, -1.5f, 0);
        float duration = 0.8f;

        // Set animation to show downward movement
        animator.SetFloat("lastHorizontal", 0);
        animator.SetFloat("lastVertical", -1);
        animator.SetBool("moving", true);

        // Update last motion vector for consistency
        lastMotionVector = Vector2.down;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }

        // Ensure we reach the exact end position
        transform.position = endPos;

        // Stop the movement animation and return control to Update
        animator.SetBool("moving", false);
        manualAnimationControl = false;
    }
}