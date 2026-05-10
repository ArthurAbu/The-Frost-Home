using UnityEngine;

// Логика крафта проверяет наличие ингредиентов и выполняет крафт
public class CraftingSystem : MonoBehaviour
{
    [Header("База рецептов")]
    [SerializeField] private RecipeData[] allRecipes;

    [Header("Ссылки")]
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private CraftingUI craftingUI;
    [SerializeField] private DayManager dayManager;

    // Проверяет доступен ли рецепт
    public bool CanCraft(RecipeData recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            // Считает суммарное количество предмета во всех слотах
            if (inventory.GetItemCount(ingredient.item) < ingredient.amount)
                return false; // если не хватает хотя бы одного ингредиента
        }
        return true;
    }

    // Считает сколько раз максимально можно скрафтить рецепт
    public int GetMaxCraftCount(RecipeData recipe)
    {
        int maxCount = int.MaxValue;

        foreach (var ingredient in recipe.ingredients)
        {
            int have = inventory.GetItemCount(ingredient.item);
            // Сколько раз можно скрафтить по этому ингредиенту
            int possible = have / ingredient.amount;
            if (possible < maxCount)
                maxCount = possible;
        }

        return maxCount == int.MaxValue ? 0 : maxCount;
    }

    // Выполняет крафт указанное количество раз
    public bool TryCraft(RecipeData recipe, int times = 1)
    {
        // Проверяем что можем скрафтить столько раз
        if (GetMaxCraftCount(recipe) < times) return false;

        // Забираем ингредиенты
        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.amount * times);
        }

        // Добавляем результат в инвентарь
        inventory.AddItem(recipe.resultItem, recipe.resultCount * times);

        // Уведомляем DayManager о крафте предмета (засчитывает задание CraftItem)
        if (dayManager != null)
        {
            dayManager.OnItemCrafted(recipe.resultItem);
        }

        // Звук успешного крафта
        AudioManager.Instance?.PlayCraft();

        // Обновляем список рецептов
        craftingUI.RefreshRecipes();

        return true;
    }

    // Возвращает все рецепты
    public RecipeData[] GetAllRecipes() => allRecipes;
}