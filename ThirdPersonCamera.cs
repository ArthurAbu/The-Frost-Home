using UnityEngine;

// Камера от третьего лица. Следует за игроком и вращается мышью.

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Цель")]
    public Transform target;

    [Header("Настройка камеры")]
    public float distance = 5f;
    public float height = 2f;
    public float sensitivity = 2f;
    public float smoothSpeed = 10f;

    [Header("Поворот тела")]
    public bool rotateBodyWithCamera = true;

    [Header("Ограничение вертикального угла")]
    public float minVerticalAngle = 0f; // не заглядывать под себя
    public float maxVerticalAngle = 60f;  // не загляядывать вверх слишком высоко

    private float rotationX;
    private float rotationY;

    void Start()
    {
        rotationY = target.eulerAngles.y;
    }

    // Камера обновляется после движения персонажа
    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity;

        rotationY += mouseX;
        rotationX += mouseY;

        // Ограничиние вертикального угол
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Поворот тела персонажа за камерой
        if (rotateBodyWithCamera)
        {
            target.rotation = Quaternion.Euler(0f, rotationY, 0f);
        }

        Vector3 desiredPosition = CalculateDesiredPosition();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * height);
    }

    // Вычисляем позицию камеры относительно персонажа
    private Vector3 CalculateDesiredPosition()
    {
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + rotation * direction + Vector3.up * height;
        return desiredPosition;
    }
}