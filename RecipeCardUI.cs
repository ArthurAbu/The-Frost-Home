using UnityEngine;
using UnityEngine.UI;

// Скрипт одной карточки рецепта в списке крафта
public class RecipeCardUI : MonoBehaviour
{
    [Header("UI-элементы карточки")]
    [SerializeField] private Text recipeNameText;    // Название рецепта
    [SerializeField] private Text ingredientsText;   // Список ингредиентов
    [SerializeField] private Text resultText;        // Что получится
    [SerializeField] private Image resultIcon;       // Иконка результата
    [SerializeField] private Image cardBackground;   // Фон карточки

    [Header("Кнопки крафта")]
    [SerializeField] private Button craftOneButton;  // Кнопка «× 1»
    [SerializeField] private Button craftMaxButton;  // Кнопка «Макс»
    [SerializeField] private Text craftMaxLabel;     // Текст на кнопке «Макс» — показывает число

    [Header("Цвета фона карточек")]
    [SerializeField] private Color availableColor   = new Color(0.2f, 0.5f, 0.2f, 0.9f); // Зелёный — доступен
    [SerializeField] private Color unavailableColor = new Color(0.3f, 0.3f, 0.3f, 0.6f); // Серый — нет ингредиентов

    private RecipeData recipe;
    private CraftingUI craftingUI;
    private int maxCount; // Сколько раз можно скрафтить максимум

    // Инициализация карточки
    public void Setup(RecipeData newRecipe, bool canCraft, int craftableCount, CraftingUI ui)
    {
        recipe = newRecipe;
        craftingUI = ui;
        maxCount = craftableCount;

        // Заполняем название
        if (recipeNameText != null)
            recipeNameText.text = recipe.recipeName;

        // Заполняем список ингредиентов
        if (ingredientsText != null)
        {
            string ingredientsList = "";
            foreach (var ingredient in recipe.ingredients)
            {
                ingredientsList += $"• {ingredient.item.itemName} x{ingredient.amount}\n";
            }
            ingredientsText.text = ingredientsList.TrimEnd('\n');
        }

        // Заполняем результат
        if (resultText != null)
            resultText.text = $"→ {recipe.resultItem.itemName} x{recipe.resultCount}";

        // Иконка результата
        if (resultIcon != null && recipe.resultItem.icon != null)
            resultIcon.sprite = recipe.resultItem.icon;

        // Цвет карточки зависит от доступности
        if (cardBackground != null)
            cardBackground.color = canCraft ? availableColor : unavailableColor;

        // Кнопка x1 
        if (craftOneButton != null)
        {
            craftOneButton.interactable = canCraft;
            craftOneButton.onClick.RemoveAllListeners();
            craftOneButton.onClick.AddListener(OnCraftOneClicked);
        }

        // Кнопка max
        if (craftMaxButton != null)
        {
            craftMaxButton.interactable = canCraft;
            craftMaxButton.onClick.RemoveAllListeners();
            craftMaxButton.onClick.AddListener(OnCraftMaxClicked);
        }

        // Подпись на кнопке max показывает максимальное возможное количество
        if (craftMaxLabel != null)
            craftMaxLabel.text = canCraft ? $"{maxCount}" : "0";
    }

    private void OnCraftOneClicked()
    {
        craftingUI.OnCraftButtonPressed(recipe, 1);
    }

    private void OnCraftMaxClicked()
    {
        craftingUI.OnCraftButtonPressed(recipe, maxCount);
    }
}