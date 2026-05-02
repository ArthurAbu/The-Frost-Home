using UnityEngine;

// Управляет всеми взаимодействиями игрока с миром: подбор предметов (E), рубка деревьев (ЛКМ), подброс топлива в костёр (E), взаимодействие с кроватью (E), выброс предмета (G), еда (F).

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactDistance = 12f; // Дистанция подбора
    [SerializeField] private LayerMask interactableLayer; // Слой для предметов
    [SerializeField] private LayerMask treeLayer; // Слой для деревьев
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask bedLayer;   // Слой для кровати
    private BedInteraction currentBed;             // Кровать
    private WorldItem currentItem; // Предмет, на который мы сейчас смотрим
    private ChoppableTree currentTree;

    void Update()
    {
        CheckForItems();
        CheckForTrees();
        CheckForBed();

        // Нажатие G для выбрасывания из хотбара
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropFromHotbar();
        }

        // ЛКМ для удара по дереву
        if (Input.GetMouseButtonDown(0))
        {
            if (currentTree != null) TryChopTree();
        }

        // Нажатие E
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Смотрим ли на костёр, если получилось можно подкинуть бревно в костер
            if (TryAddFuelToCampfire()) return; 

            // Смотрим ли на кровать, если получилось можно перейти на следующий день
            if (currentBed != null) { currentBed.TryInteract(); return; }

            // Иначе — обычный подбор предмета
            if (currentItem != null) PickUp();
        }

        // Нажатие F съесть предмет из активного слота хотбара
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryEatFromHotbar();
        }
    }

    private void CheckForItems()
    {
        // Луч идет ровно из центра экрана
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Игнорировать всё кроме предметов
        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            WorldItem item = hit.collider.GetComponent<WorldItem>();
            
            if (item != null)
            {
                if (currentItem != item)
                {
                    // Если перевели взгляд с одного предмета на другой
                    if (currentItem != null) currentItem.ToggleHighlight(false);
                    currentItem = item;
                    currentItem.ToggleHighlight(true);
                }
                return; // Нашли предмет — выходим из метода
            }
        }

        // Если луч не попал в предмет
        if (currentItem != null)
        {
            currentItem.ToggleHighlight(false);
            currentItem = null;
        }
    }

    // Проверяем, смотрим ли мы на дерево
    private void CheckForTrees()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // Луч летит только на слой "Tree"
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, treeLayer))
        {
            ChoppableTree tree = hit.collider.GetComponentInParent<ChoppableTree>();
            if (tree != null)
            {
                currentTree = tree;
                return;
            }
        }
        // Если не смотрим на дерево то сбрасываем ссылку
        currentTree = null;
    }

    // Попытка срубить дерево
    private void TryChopTree()
    {
        // Проверяем, есть ли у игрока инструмент в активном слоте хотбара
        int activeIndex = inventoryUI.GetActiveHotbarIndex();
        InventorySlot activeSlot = inventory.slots[activeIndex];
        if (activeSlot.item == null || activeSlot.item.type != ItemType.Tool) return;

        // Передаём себя, чтобы дерево могло вызвать SpawnItemInWorld
        currentTree.TryChop(this);
    }

    // Проверка смотрит ли игрок на кровать
    private void CheckForBed()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, bedLayer))
        {
            BedInteraction bed = hit.collider.GetComponentInParent<BedInteraction>();
            if (bed != null)
            {
                if (currentBed != bed)
                {
                    if (currentBed != null) currentBed.ToggleHighlight(false);
                    currentBed = bed;
                    currentBed.ToggleHighlight(true);
                }
                return;
            }
        }

        if (currentBed != null)
        {
            currentBed.ToggleHighlight(false);
            currentBed = null;
        }
    }

    private void PickUp()
    {
        if (inventory.AddItem(currentItem.itemData, currentItem.count))
        {
            // Перед удалением выключаем подсветку на всякий случай
            currentItem.ToggleHighlight(false);
            DayManager dm = FindObjectOfType<DayManager>();   // Уведомляем DayManager что подобрали предмет
            if (dm != null) dm.OnItemCollected(currentItem.itemData);
            Destroy(currentItem.gameObject);
            currentItem = null; 
            inventoryUI.RefreshUI();
        }
    }

    // Выкидываем предметы с хотбара
    private void DropFromHotbar()
    {
        int activeIndex = inventoryUI.GetActiveHotbarIndex();
        InventorySlot slot = inventory.slots[activeIndex];

        if (slot.item != null)
        {
            SpawnItemInWorld(slot.item, 1); // Всегда 1 предмет из хотбара
            inventory.RemoveFromSlot(activeIndex, 1);
            inventoryUI.RefreshUI();
        }
    }

    // Метод для создания предмета перед игроком
    public void SpawnItemInWorld(ItemData data, int count, Vector3? customSpawnPos = null)
    {
        // Проверяем, есть ли у предмета префаб
        if (data.dropPrefab == null)
        {
            Debug.LogError($"У предмета {data.itemName} не назначен префаб (dropPrefab)!");
            return; // Если модели нет, прерываем выбрасывание
        }

        // Вычисляем точку появления чуть впереди и выше игрока
        Vector3 spawnPos = customSpawnPos ?? 
            (playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.5f);

        // Создаем предмет из префаба
        GameObject go = Instantiate(data.dropPrefab, spawnPos, Quaternion.identity);

        // Принудительно ставим нужный слой, чтобы могли его подобрать
        go.layer = LayerMask.NameToLayer("Items"); 

        // Ищем или добавляем компонент WorldItem
        WorldItem wi = go.GetComponent<WorldItem>();
        if (wi == null) wi = go.AddComponent<WorldItem>();
        wi.itemData = data;
        wi.count = count;

        // Ищем MeshCollider на объекте или его детях
        MeshCollider meshCol = go.GetComponentInChildren<MeshCollider>();
        if (meshCol != null) meshCol.convex = true; // Включаем Convex

        // Ищем или добавляем физику
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        
        // Толкаем предмет вперед и немного вверх
        rb.AddForce(playerTransform.forward * 3f + Vector3.up * 1f, ForceMode.Impulse);
        
        // Добавляем красивое случайное вращение в полете
        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 5f, ForceMode.Impulse);
    }

    // Метод для проверки смотрит ли игрок на костёр и подбрасывает топливо
    private bool TryAddFuelToCampfire()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Луч на любой коллайдер в пределах дистанции
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // Ищем Campfire
            Campfire campfire = hit.collider.GetComponentInParent<Campfire>();
            if (campfire != null)
            {
                // Берём предмет из активного слота хотбара
                int activeIndex = inventoryUI.GetActiveHotbarIndex();
                InventorySlot activeSlot = inventory.slots[activeIndex];

                if (activeSlot.item == null)
                {
                    return true; // Возвращаем true, чтобы не выполнять PickUp
                }

                // Пытаемся подбросить
                if (campfire.AddFuel(activeSlot.item))
                {
                    inventory.RemoveFromSlot(activeIndex, 1);
                    inventoryUI.RefreshUI();
                }

                return true; // Если костёр был то взаимодействие с предметами не нужно
            }
        }

        return false; // Костра нет то обычный подбор
    }

    // Попытка съесть предмет из активного слота хотбара
    private void TryEatFromHotbar()
    {
        int activeIndex = inventoryUI.GetActiveHotbarIndex();
        InventorySlot activeSlot = inventory.slots[activeIndex];

        // Проверяем что в слоте вообще что-то есть
        if (activeSlot.item == null) return;

        // Проверяем что предмет еда
        if (activeSlot.item.type != ItemType.Food) return;

        // Ищем PlayerStats на игроке
        PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogError("PlayerStats не найден на игроке!");
            return;
        }

        // Сохраняем значения до удаления из слота
        float food = activeSlot.item.foodValue;
        float heal = activeSlot.item.healValue;

        // Применяем эффекты еды
        stats.AddFood(food);
        if (heal > 0f) stats.Heal(heal);

        // Убираем 1 единицу еды из слота
        inventory.RemoveFromSlot(activeIndex, 1);
        inventoryUI.RefreshUI();
    }
}