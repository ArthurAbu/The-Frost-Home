using UnityEngine;

// Управление персонажем: ходьба, бег, прыжок, гравитация.

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Скоростm")] 
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpSpeed = 7f;
    public float gravity = -9.81f;

    [Header("Проверка земли")] 
    public float groundDistance = 0.1f;
    public LayerMask groundMask;

    [Header("Задержка между прыжками")]
    public float jumpCooldownDuration = 0.3f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float lastJumpTime = -999f; // Чтобы первый прыжок был возможен

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        ApplyGravity();
    }

    // проверяет стоит ли персонаж на земле
    private void CheckGrounded()
    {
        // Вычисляем центр нижней полусферы капсулы CharacterController. Капсула состоит из цилиндра и двух полусфер сверху и снизу.
        Vector3 bottomSphereCenter = transform.position
            + controller.center
            + Vector3.down * (controller.height / 2f - controller.radius);

        // SphereCast: бросаем сферу вниз от нижней точки капсулы. Если она касается земли то персонаж стоит на земле.
        isGrounded = Physics.SphereCast(
            bottomSphereCenter,        // Откуда начинаем
            controller.radius * 0.95f, // Радиус сферы
            Vector3.down,              // Направление броска вниз
            out RaycastHit _,          
            groundDistance,            // На какое расстояние проверяем
            groundMask,                // Проверяем только слои из groundMask
            QueryTriggerInteraction.Ignore // Игнорируем триггеры
        );
    }

    // Обработка движения персонажа
    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Прыжок только если на земле
        if (isGrounded && Input.GetButtonDown("Jump") && CanJump())
        {
            velocity.y = jumpSpeed;
            lastJumpTime = Time.time;
        }
    }

    // Задержка между прыжками
    private bool CanJump()
    {
        return Time.time - lastJumpTime >= jumpCooldownDuration;
    }

    // Гравитация
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}