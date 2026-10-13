using UnityEngine;
using System.Collections.Generic;

public class EnvironmentGoalSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject propPrefab;
    [SerializeField] GameObject goalPrefab;

    [Header("Spawn Radius")]
    [SerializeField] float minRadius = 1;
    [SerializeField] float maxRadius = 3;

    [SerializeField] float minPropDistance = 0.5f;

    [Header("Goal Height")]
    [SerializeField] float minGoalHeight = 1;
    [SerializeField] float maxGoalHeight = 5;

    [Header("Other")]
    [SerializeField] int maxSpawnAttempts = 50;
    [SerializeField] LayerMask groundLayer;

    GameObject prop;
    GameObject goal;

#if UNITY_EDITOR
    [SerializeField] bool drawGizmos = true;
#endif

    private void Awake()
    {
        prop = Instantiate(propPrefab, transform);
        goal = Instantiate(goalPrefab, transform);
    }

    public void Spawn()
    {
        PlaceObject(prop, GetRandomPosition() + Vector3.up * 0.1f);

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            PlaceObject(goal, GetRandomPosition(true));
            if (Vector3.Distance(prop.transform.position, goal.transform.position) >= minPropDistance)
                break;
        }
    }

    void PlaceObject(GameObject obj, Vector3 position)
    {
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.position = position;

        if(obj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    Vector3 GetRandomPosition(bool spawnInAir = false)
    {
        float angle = Random.Range(0, Mathf.PI * 2);
        float r = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));

        Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);

        Vector3 origin = pos + Vector3.up * 50f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            pos.y = spawnInAir ? hit.point.y + Random.Range(minGoalHeight, maxGoalHeight) : hit.point.y;
        }

        return pos;
    }

    public bool IsPropOutOfReach()
    {
        Vector3 d = prop.transform.position - transform.position;
        d.y = 0;
        return d.magnitude > maxRadius + 0.5f || prop.transform.position.y < transform.position.y - 1f;
    }

    public Transform Prop => prop.transform;
    public Transform Goal => goal.transform;

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
