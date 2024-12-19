using UnityEngine;

public class FoodSpawnerProxy : ProxyBehaviour
{
    public FoodProxy[] food;
    public Vector2 spawnCooldown = new Vector2(120, 180);

    public override bool IsValid()
    {
        if (food.Length == 0)
        {
            Debug.LogError("Must have at least one food proxy.");
            return false;
        }

        if (spawnCooldown.x < 10)
        {
            Debug.LogError("Spawn cooldown must be in the range [10, Infinity).", gameObject);
            return false;
        }

        return base.IsValid();
    }
}
