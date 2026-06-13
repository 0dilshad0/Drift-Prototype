using UnityEngine;

public class CarInput : MonoBehaviour
{
    public float steer;
    public float throttle;
    public bool handbrake;

    void Update()
    {
        steer = Input.GetAxis("Horizontal");
        throttle = Input.GetAxis("Vertical");
        handbrake = Input.GetKey(KeyCode.Space);
    }
}