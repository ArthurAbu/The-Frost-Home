using UnityEngine;

// Один рецепт крафта
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Crafting/Recipe", order = 1)]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public class Ingredient
    {
        public ItemData item; // Какой предмет нужен
        public int amount; // Сколько штук нужно
    }

    [Header("Ингредиенты")]
    public Ingredient[] ingredients; // Список необходимых предметов

    [Header("Результат")]
    public ItemData resultItem; // Что получится
    public int resultCount = 1; // Сколько штук получится

    [Header("Отображение в UI")]
    public string recipeName; // Название рецепта в списке
    [TextArea] public string description; // Описание
}