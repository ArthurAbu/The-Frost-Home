using UnityEngine;

// Когда игрок смотрит на кровать и нажимает E  пытается перейти к следующему дню.
public class BedInteraction : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;

    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    // Включает/выключает подсветку при наведении на кровать
    public void ToggleHighlight(bool value)
    {
        if (outline != null) outline.enabled = value;
    }

    // Нажатие E - попытка лечь спать
    public void TryInteract()
    {
        if (dayManager == null)
        {
            Debug.LogError("В BedInteraction не назначен DayManager!");
            return;
        }

        dayManager.TrySleep();
    }
}