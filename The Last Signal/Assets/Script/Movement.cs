using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownMovement : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float smoothTime = 0.1f;

    private Vector2 moveInput;
    private float speed, speedRef, animRef;

    // Input del sistema de Input System
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        Move();
    }

    public void Move()
    {
        if (!playerRoot || !controller) return;

        // En top-down, el movimiento es directo en el plano XZ
        Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // Velocidad
        float targetSpeed = moveSpeed * moveInput.magnitude;
        speed = Mathf.SmoothDamp(speed, targetSpeed, ref speedRef, smoothTime);

        // Rotación hacia la dirección del movimiento
        if (dir.sqrMagnitude > 0.01f)
        {
            playerRoot.rotation = Quaternion.Slerp(playerRoot.rotation,
                Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
        }

        // Movimiento del controller
        controller.Move(dir * speed * Time.deltaTime);

        // Animación
        if (animator)
        {
            float targetAnim = moveInput.magnitude;
            float smoothed = Mathf.SmoothDamp(animator.GetFloat("Speed"), targetAnim, ref animRef, smoothTime);
            animator.SetFloat("Speed", smoothed);
        }
    }

    public void ResetMove()
    {
        moveInput = Vector2.zero;
        speed = 0f;
        if (animator) animator.SetFloat("Speed", 0f);
    }
}