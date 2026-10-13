using UnityEngine;

public class ClawContact : MonoBehaviour
{
    [HideInInspector] public bool touching;
    private void OnCollisionEnter(Collision c) { if (c.gameObject.CompareTag("Prop")) touching = true; }
    private void OnCollisionExit(Collision c) { if (c.gameObject.CompareTag("Prop")) touching = false; }

}
