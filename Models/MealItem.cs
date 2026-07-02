namespace DiabetesHelper.Models;

public class MealItem
{
    public int Id { get; set; }

    public int MealId { get; set; }

    public string Name { get; set; } = string.Empty;

    public double CarbsGrams { get; set; }

    public int? GlycemicIndex { get; set; }
}
