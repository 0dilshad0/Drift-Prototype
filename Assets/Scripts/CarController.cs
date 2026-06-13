using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Drift Mode")]
    public DriftType driftType;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider rearLeft;
    [SerializeField] private WheelCollider rearRight;

    [Header("Wheel Meshes")]
    [SerializeField] private Transform frontLeftMesh;
    [SerializeField] private Transform frontRightMesh;
    [SerializeField] private Transform rearLeftMesh;
    [SerializeField] private Transform rearRightMesh;

    [Header("Drive")]
    [SerializeField] private float motorForce;
    [SerializeField] private float maxSteerAngle = 35f;

    [Header("Grip")]
    [SerializeField] private float FbaseStiffness;
    [SerializeField] private float RbaseStiffness;
    [SerializeField] private float FdriftStiffness;
    [SerializeField] private float RdriftStiffness;

    [Header("Drift Control")]
    [SerializeField] private float maxDriftSpeed;
    [SerializeField] private float driftSpeedLimitForce = 2500f;

    [Header("Braking")]
    [SerializeField] private float handbrakeForce = 3500f;
    [SerializeField] private float engineBrakeForce = 900f;
    [SerializeField] private float FhandbrakeStiffness = 0.7f;
    [SerializeField] private float RhandbrakeStiffness = 0.25f;

    private Rigidbody rb;
    private CarInput input;

    private float FcurrentStiffness;
    private float RcurrentStiffness;
    private float currentBrake;

    public float sideSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<CarInput>();

        rb.mass = 1200f;
        rb.linearDamping = 0.02f;
        rb.angularDamping = 3.5f;
        rb.centerOfMass = new Vector3(0, -0.6f, 0);

        ApplyDriftPreset();

        FcurrentStiffness = FbaseStiffness;
        RcurrentStiffness = RbaseStiffness;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            driftType = DriftType.American;
            ApplyDriftPreset();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            driftType = DriftType.Japanese;
            ApplyDriftPreset();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            driftType = DriftType.Arab;
            ApplyDriftPreset();
        }
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleDrift();
        HandleBraking();
        UpdateWheels();
    }

    void HandleMotor()
    {
        float torque = motorForce * input.throttle;

        frontLeft.motorTorque = torque;
        frontRight.motorTorque = torque;
    }

    void HandleSteering()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f;
        float steerLimit = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.6f, speed / 100f);

        float angle = steerLimit * input.steer;

        frontLeft.steerAngle = angle;
        frontRight.steerAngle = angle;
    }

    void HandleDrift()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        sideSpeed = Mathf.Abs(localVel.x);

        float Ftarget = FbaseStiffness;
        float Rtarget = RbaseStiffness;

        if (sideSpeed > 2f)
        {
            Ftarget = FdriftStiffness;
            Rtarget = RdriftStiffness;
        }

        FcurrentStiffness = Mathf.Lerp(FcurrentStiffness, Ftarget, Time.fixedDeltaTime * 5f);
        RcurrentStiffness = Mathf.Lerp(RcurrentStiffness, Rtarget, Time.fixedDeltaTime * 5f);

        ApplyGrip(FcurrentStiffness, RcurrentStiffness);

        float speed = rb.linearVelocity.magnitude * 3.6f;

        if (sideSpeed > 2f && speed > maxDriftSpeed)
        {
            Vector3 forwardVel = Vector3.Project(rb.linearVelocity, transform.forward);
            rb.AddForce(-forwardVel.normalized * driftSpeedLimitForce);
        }
    }

    void HandleBraking()
    {
        currentBrake = 0f;

        // Engine brake
        if (Mathf.Abs(input.throttle) < 0.1f)
        {
            float speedFactor = rb.linearVelocity.magnitude;
            currentBrake = engineBrakeForce * speedFactor;
        }

        // Handbrake override
        if (input.handbrake)
        {
            currentBrake = handbrakeForce;
            ApplyGrip(FhandbrakeStiffness, RhandbrakeStiffness);
        }

        frontLeft.brakeTorque = currentBrake;
        frontRight.brakeTorque = currentBrake;
        rearLeft.brakeTorque = currentBrake;
        rearRight.brakeTorque = currentBrake;
    }


    void ApplyGrip(float Fgrip, float Rgrip)
    {
        WheelFrictionCurve fl = frontLeft.sidewaysFriction;
        WheelFrictionCurve fr = frontRight.sidewaysFriction;
        WheelFrictionCurve rl = rearLeft.sidewaysFriction;
        WheelFrictionCurve rr = rearRight.sidewaysFriction;

        fl.stiffness = Fgrip;
        fr.stiffness = Fgrip;
        rl.stiffness = Rgrip;
        rr.stiffness = Rgrip;

        frontLeft.sidewaysFriction = fl;
        frontRight.sidewaysFriction = fr;
        rearLeft.sidewaysFriction = rl;
        rearRight.sidewaysFriction = rr;
    }
    void UpdateWheels()
    {
        UpdateWheel(frontLeft, frontLeftMesh);
        UpdateWheel(frontRight, frontRightMesh);
        UpdateWheel(rearLeft, rearLeftMesh);
        UpdateWheel(rearRight, rearRightMesh);
    }

    void UpdateWheel(WheelCollider col, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);

        mesh.position = pos;
        mesh.rotation = rot;
    }

    void ApplyDriftPreset()
    {
        switch (driftType)
        {
            case DriftType.American:
                motorForce = 2600f;
                FbaseStiffness = 1.2f;
                RbaseStiffness = 1.0f;
                FdriftStiffness = 1.1f;
                RdriftStiffness = 0.4f;
                maxDriftSpeed = 90f;
                break;

            case DriftType.Japanese:
                motorForce = 2000f;
                maxSteerAngle = 45;
                FbaseStiffness = 1.3f;
                RbaseStiffness = 1.1f;
                FdriftStiffness = 1.2f;
                RdriftStiffness = 0.5f;
                maxDriftSpeed = 70f;
                break;

            case DriftType.Arab:
                motorForce = 3000f;
                FbaseStiffness = 1.5f;
                RbaseStiffness = 0.2f;
                FdriftStiffness = 1.1f;
                RdriftStiffness = 0.1f;
                maxDriftSpeed = 120f;
                break;
        }
    }
}