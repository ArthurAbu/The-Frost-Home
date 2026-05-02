using UnityEngine;

// Заставляет объект со снегом следовать за игроком.
public class SnowFollower : MonoBehaviour
{
    [SerializeField] private Transform target; // Игрок, за которым следуют частицы
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, 0f); // Смещение относительно цели

    void LateUpdate()
    {
        // Нужно чтобы перемещение происходило после движения игрока
        if (target == null) return;
        transform.position = target.position + offset;
    }
}