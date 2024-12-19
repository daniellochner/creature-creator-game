using UnityEngine;

public class FoodProxy : ProxyBehaviour
{
    public GameObject model;
    public Diet diet;
    public Vector2 minMaxHunger;
    public Vector2 minMaxHeal;

    public override bool IsValid()
    {
        if (model == null)
        {
            Debug.LogError("Model must be provided.");
            return false;
        }

        if (minMaxHunger.x < 0 || minMaxHunger.y > 1)
        {
            Debug.LogError("Hunger value must be in the range [0, 1].");
            return false;
        }

        if (minMaxHeal.x < 0 || minMaxHeal.y > 1)
        {
            Debug.LogError("Heal value must be in the range [0, 1].");
            return false;
        }

        return base.IsValid();
    }

    public enum Diet
    {
        Omnivore,
        Carnivore,
        Herbivore
    }
}
