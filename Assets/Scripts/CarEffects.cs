using UnityEngine;

public class CarEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem smokeLeft;
    [SerializeField] ParticleSystem smokeRight;

    [SerializeField] TrailRenderer trailLeft;
    [SerializeField] TrailRenderer trailRight;

    public float driftThreshold = 2.5f;

    private CarController car;

    void Start()
    {
        car = GetComponent<CarController>();

        smokeLeft.Stop();
        smokeRight.Stop();

        trailLeft.emitting = false;
        trailRight.emitting = false;
    }

    void Update()
    {
        bool drifting = car.sideSpeed > driftThreshold;

        if (drifting)
        {
            if (!smokeLeft.isPlaying) smokeLeft.Play();
            if (!smokeRight.isPlaying) smokeRight.Play();
        }
        else
        {
            smokeLeft.Stop();
            smokeRight.Stop();
        }

        trailLeft.emitting = drifting;
        trailRight.emitting = drifting;
    }
}