using UnityEngine;
using System.Collections.Generic;

// Отвечает за отображение списка рецептов и кнопки «Крафт»
public class CraftingUI : MonoBehaviour
{
    [Header("Ссылки на системы")]
    [SerializeField] private CraftingSystem craftingSystem;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private InventoryUI inventoryUI;   

    [Header("UI-объекты")]
    [SerializeField] private GameObject craftingPanel;   // Вся панель крафта
    [SerializeField] private Transform recipeListParent; // Контейнер для карточек рецептов
    [SerializeField] private GameObject recipeCardPrefab;// Префаб одной карточки рецепта

    // Храним созданные карточки чтобы удалять их при обновлении
    private List<GameObject> spawnedCards = new();

    private bool isPanelOpen = false;

    void Start()
    {
        craftingPanel.SetActive(false);
    }

    void Update()
    {
        // Открытие/закрытие на O
        if (Input.GetKeyDown(KeyCode.O))
        {
            isPanelOpen = !isPanelOpen;
            craftingPanel.SetActive(isPanelOpen);

            if (isPanelOpen)
            {
                // При открытии обновляем список и разблокируем курсор
                RefreshRecipes();
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                // При закрытии блокируем курсор обратно
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        // Пока панель открыта принудительно держим курсор
        if (isPanelOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // Перестраивает список рецептов с нуля
    public void RefreshRecipes()
    {
        // Удаляем старые карточки
        foreach (var card in spawnedCards)
            Destroy(card);
        spawnedCards.Clear();

        // Создаём карточку для каждого рецепта
        foreach (var recipe in craftingSystem.GetAllRecipes())
        {
            // Проверяем доступен ли рецепт
            bool canCraft = craftingSystem.CanCraft(recipe);
            // Считаем максимум перед созданием карточки
            int  maxCount = craftingSystem.GetMaxCraftCount(recipe);

            // Создаём карточку из префаба
            GameObject card = Instantiate(recipeCardPrefab, recipeListParent);
            spawnedCards.Add(card);

            // Заполняем данные карточки через скрипт
            RecipeCardUI cardUI = card.GetComponent<RecipeCardUI>();
            cardUI.Setup(recipe, canCraft, maxCount, this);
        }
    }

    // Вызывается карточкой при нажатии кнопки «Крафт»
    public void OnCraftButtonPressed(RecipeData recipe, int times)
    {
        if (craftingSystem.TryCraft(recipe, times))
        {
            // После крафта обновляем инвентарь
            inventoryUI.RefreshUI();
        }
    }

    // Закрыть панель крафта
    public void ClosePanel()
    {
        isPanelOpen = false;
        craftingPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
}