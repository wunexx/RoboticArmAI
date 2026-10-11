using UnityEngine;

public class Prop : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            GoalSpawner.Instance.Spawn();
        }
    }
}
