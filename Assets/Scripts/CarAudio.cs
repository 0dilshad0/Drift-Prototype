using UnityEngine;

public class CarAudio : MonoBehaviour
{
    [SerializeField] AudioSource engineAudio;
    [SerializeField] AudioSource driftAudio;

    private CarController car;
    private CarInput input;
    private Rigidbody rb;

    void Start()
    {
        car = GetComponent<CarController>();
        input = GetComponent<CarInput>();
        rb = GetComponent<Rigidbody>();

        engineAudio.loop = true;
        engineAudio.Play();

        driftAudio.volume = 0f;
    }

    void Update()
    {
        HandleEngine();
        HandleDriftAudio();
    }

    void HandleEngine()
    {
        float speed = rb.linearVelocity.magnitude;

        engineAudio.pitch = Mathf.Lerp(0.8f, 2f, speed / 30f);
        engineAudio.volume = Mathf.Lerp(0.3f, 1f, Mathf.Abs(input.throttle));
    }

    void HandleDriftAudio()
    {
        bool drifting = car.sideSpeed > 2.5f;

        if (drifting)
        {
            driftAudio.volume = Mathf.Lerp(driftAudio.volume, 1f, Time.deltaTime * 5f);
            if (!driftAudio.isPlaying) driftAudio.Play();
        }
        else
        {
            driftAudio.volume = Mathf.Lerp(driftAudio.volume, 0f, Time.deltaTime * 5f);
            if (driftAudio.volume < 0.05f) driftAudio.Stop();
        }
    }
}