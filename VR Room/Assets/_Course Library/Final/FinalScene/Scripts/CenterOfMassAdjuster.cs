using UnityEngine;

public class CenterOfMassAdjuster : MonoBehaviour
{
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0); // Lower is heavier

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = centerOfMassOffset;
        }
    }
}