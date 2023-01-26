using UnityEngine;

public class SwordCollision : MonoBehaviour
{

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer")) return;

        Debug.Log(other.name);
    }
}