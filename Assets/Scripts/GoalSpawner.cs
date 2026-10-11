using UnityEngine;
using System.Collections.Generic;

public class GoalSpawner : MonoBehaviour
{
    public static GoalSpawner Instance;

    [SerializeField] List<GameObject> itemPrefabs;
    [SerializeField] GameObject goalPrefab;

    [SerializeField] float minRadius = 1;
    [SerializeField] float maxRadius = 3;

#if UNITY_EDITOR
    [SerializeField] bool drawGizmos = true;
#endif

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public void Spawn()
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minRadius);
    }
#endif
}
