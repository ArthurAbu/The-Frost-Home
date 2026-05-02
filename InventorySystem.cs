using System.Collections.Generic;
using UnityEngine;

// Одна ячейка инвентаря: предмет и количество

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;
}

// Хранит все предметы игрока. 90 слотов: 0–9 хотбар, 10–89 основной инвентарь.
public class InventorySystem : MonoBehaviour
{
    [SerializeField] private int slotCount = 90; // 80 инвентаря + 10 для хотбар
    public List<InventorySlot> slots = new();

    void Awake()
    {
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot { item = null, count = 0 });
    }
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count) return;

        InventorySlot temp = new InventorySlot 
        { 
            item = slots[indexA].item, 
            count = slots[indexA].count 
        };

        slots[indexA].item = slots[indexB].item;
        slots[indexA].count = slots[indexB].count;

        slots[indexB].item = temp.item;
        slots[indexB].count = temp.count;
    }
    // Добавляет предмет в инвентарь
    public bool AddItem(ItemData item, int count)
    {
        if (item == null) return false;

        // Добавляет в существующие стаки
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.count < item.stackLimit)
            {
                int add = Mathf.Min(count, item.stackLimit - slot.count);
                slot.count += add;
                count -= add;
                if (count <= 0) return true;
            }
        }

        // Потом в пустые слоты
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                int add = Mathf.Min(count, item.stackLimit);
                slot.item = item;
                slot.count = add;
                count -= add;
                if (count <= 0) return true;
            }
        }

        return count == 0;
    }

    // Удаляет предмет из инвентаря
    public bool RemoveItem(ItemData item, int count)
    {
        int removed = 0;
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                int take = Mathf.Min(slot.count, count - removed);
                slot.count -= take;
                removed += take;
                if (slot.count <= 0) slot.item = null;
                if (removed >= count) return true;
            }
        }
        return false;
    }

    // Возвращает общее количество предметов в инвентаре
    public int GetItemCount(ItemData item)
    {
        int total = 0;
        foreach (var slot in slots)
            if (slot.item == item) total += slot.count;
        return total;
    }

    // Удаляет определенное количество предметов из конкретного слота
    public void RemoveFromSlot(int index, int amount)
    {
        if (index < 0 || index >= slots.Count) return;

        if (slots[index].item != null)
        {
            slots[index].count -= amount;
            if (slots[index].count <= 0)
            {
                slots[index].item = null;
                slots[index].count = 0;
            }
        }
    }
}