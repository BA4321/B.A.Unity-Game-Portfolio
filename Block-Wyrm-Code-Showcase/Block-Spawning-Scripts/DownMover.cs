using UnityEngine;

public class DownMover : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float speed      = 2f;  // units per second
    [SerializeField] private float moveTime   = 3f;  // seconds the object should keep moving

    private float elapsed = 0f;

    void Update()
    {
        // Move only while the timer is running
        if (elapsed < moveTime)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
        }
        else
        {
            enabled = false;           // disable Update() to save CPU once finished
        }
    }
}
