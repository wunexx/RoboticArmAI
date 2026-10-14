using UnityEngine;

public class ClawContact : MonoBehaviour
{
    int count;
    public bool touching => count > 0;
    private void OnCollisionEnter(Collision c) { if (c.gameObject.CompareTag("Prop")) count++; }
    private void OnCollisionExit(Collision c) { if (c.gameObject.CompareTag("Prop")) count = Mathf.Max(0, count - 1); }

    public void ResetContact() { count = 0; }
}
