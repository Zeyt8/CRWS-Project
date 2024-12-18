using System.Collections.Generic;
using UnityEngine;

public class AnimationPreview : MonoBehaviour
{
    [SerializeField] private List<Animator> _animators = new List<Animator>();
    [SerializeField] private float _animationSpeed;
    [SerializeField] private bool _moving;
    [SerializeField] private float _velocity = 1f;

    // Update is called once per frame
    void Update()
    {
        foreach (Animator animator in _animators)
        {
            animator.SetBool("Moving", _moving);
            animator.SetFloat("Animation Speed", _animationSpeed);
            animator.SetFloat("Velocity", _velocity);
        }
    }

    [ContextMenu("Attack")]
    public void Attack()
    {
        foreach (Animator animator in _animators)
        {
            animator.SetTrigger("Attack");
        }
    }

    [ContextMenu("Jump")]
    public void Jump()
    {
        foreach (Animator animator in _animators)
        {
            animator.SetTrigger("Jump");
        }
    }

    [ContextMenu("Fall")]
    public void Fall()
    {
        foreach (Animator animator in _animators)
        {
            animator.SetTrigger("Fall");
        }
    }

    [ContextMenu("Land")]
    public void Land()
    {
        foreach (Animator animator in _animators)
        {
            animator.SetTrigger("Land");
        }
    }
}
