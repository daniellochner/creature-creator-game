using System;
using UnityEngine;

public class BattleProxy : ProxyBehaviour
{
    [Range(1, 100)] public float width = 25f;
    [Range(1, 100)] public float length = 25f;
    public Round[] rounds = new Round[3];

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, 0, length));
    }

    private void OnValidate()
    {
        for (int i = 0; i < rounds.Length; i++)
        {
            Round round = rounds[i];
            if (round != null)
            {
                round.name = $"Round {i + 1}";
            }
        }
    }

    public override bool IsValid()
    {
        foreach (Round round in rounds)
        {
            if (round.spawners.Length == 0)
            {
                Debug.LogError($"{round.name} of battle '{name}' must have at least one spawner.", gameObject);
                return false;
            }
        }

        return base.IsValid();
    }

    [Serializable]
    public class Round
    {
        [HideInInspector] public string name;
        public NPCSpawnerProxy[] spawners;
    }
}
