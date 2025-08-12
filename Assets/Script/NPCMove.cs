using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCMove : MonoBehaviour
{
    Rigidbody2D rb2d;
    public Transform moveTo;
    [SerializeField] float speed = 3f;

    Animator[] animators;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if(moveTo == null)
        {
            return;
        }

        if(Vector3.Distance(transform.position, moveTo.position) < 0.8f)
        {
            StopMoving();
            return;
        }

        Vector3 direction = (moveTo.position - transform.position).normalized;
        foreach (Animator animator in animators)
        {
            animator.SetFloat("horizontal", direction.x);
            animator.SetFloat("vertical", direction.y);
        }

        direction *= speed;
        rb2d.linearVelocity = direction;
    }

    private void StopMoving()
    {
        moveTo = null;
        rb2d.linearVelocity = Vector3.zero;
    }
}
