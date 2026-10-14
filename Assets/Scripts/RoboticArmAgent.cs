using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class JointInfo 
{
    public string name;
    public ArticulationBody body;

    [HideInInspector] public float target;

    public float speed = 90f;
}

public class RoboticArmAgent : Agent
{
    [SerializeField] Transform baseTransform;
    [SerializeField] Transform gripPoint;
    [SerializeField] ClawContact claw1, claw2;

    [SerializeField] List<JointInfo> joints;

    EnvironmentGoalSpawner environmentGoalSpawner;
    EnvironmentUIManager environmentUIManager;

    float prevGripDist, prevGoalDist, prevPropY;

    float maxReward = 0;
    int episode;

    float floorY;

    static readonly Key[] positiveKeys = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.H };
    static readonly Key[] negativeKeys = { Key.A, Key.S, Key.D, Key.F, Key.G, Key.Y };

    public override void Initialize()
    {
        environmentGoalSpawner = transform.parent.GetComponent<EnvironmentGoalSpawner>();
        environmentUIManager = transform.parent.GetComponent<EnvironmentUIManager>();
    }
    public override void OnEpisodeBegin()
    {
        episode++;

        environmentGoalSpawner.Spawn();

        floorY = environmentGoalSpawner.Prop.position.y;
        prevPropY = floorY;

        claw1.ResetContact();
        claw2.ResetContact();

        foreach (var j in joints)
        {
            j.body.jointPosition = new ArticulationReducedSpace(0f);
            j.body.jointVelocity = new ArticulationReducedSpace(0f);

            j.target = 0f;
            ArticulationDrive drive = j.body.xDrive;
            drive.target = 0f;
            j.body.xDrive = drive;
        }

        prevGripDist = Vector3.Distance(gripPoint.position, environmentGoalSpawner.Prop.position);
        prevGoalDist = Vector3.Distance(environmentGoalSpawner.Prop.position, environmentGoalSpawner.Goal.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var j in joints)
        {
            float a = j.body.jointPosition[0];
            sensor.AddObservation(Mathf.Sin(a));
            sensor.AddObservation(Mathf.Cos(a));
        }

        foreach (var j in joints)
            sensor.AddObservation(j.body.jointVelocity[0] / 5f);

        Vector3 prop = baseTransform.InverseTransformPoint(environmentGoalSpawner.Prop.position);
        Vector3 goal = baseTransform.InverseTransformPoint(environmentGoalSpawner.Goal.position);
        Vector3 grip = baseTransform.InverseTransformPoint(gripPoint.position);

        sensor.AddObservation(prop);
        sensor.AddObservation(grip);
        sensor.AddObservation(prop - grip);
        sensor.AddObservation(goal);
        sensor.AddObservation(goal - prop);

        sensor.AddObservation(claw1.touching ? 1f : 0f);
        sensor.AddObservation(claw2.touching ? 1f : 0f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        ActionSegment<float> a = actionsOut.ContinuousActions;

        for (int i = 0; i < joints.Count; i++)
        {
            a[2 * i] = kb[positiveKeys[i]].isPressed ? 1f : 0f;
            a[2 * i + 1] = kb[negativeKeys[i]].isPressed ? 1f : 0f;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<float> a = actions.ContinuousActions;
        for (int i = 0; i < joints.Count; i++)
        {
            float input = Mathf.Clamp(a[2 * i] - a[2 * i + 1], -1f, 1f);
            RotateJoint(joints[i], input);
        }
    }

    void FixedUpdate()
    {
        if (environmentGoalSpawner == null) return;

        if (environmentGoalSpawner.IsPropOutOfReach())
        {
            FinishEpisode(-10f);
            return;
        }

        Transform prop = environmentGoalSpawner.Prop;
        Transform goal = environmentGoalSpawner.Goal;

        bool grabbed = claw1.touching && claw2.touching;
        bool lifted = prop.position.y > floorY + 0.2f;

        float gripDist = Vector3.Distance(gripPoint.position, prop.position);

        float goalDist = Vector3.Distance(prop.position, goal.position);

        if (!grabbed) AddReward((prevGripDist - gripDist) * 0.5f);
        prevGripDist = gripDist;

        if (grabbed && lifted) 
            AddReward((prevGoalDist - goalDist) * 3f);
        prevGoalDist = goalDist;

        if (grabbed)
            AddReward((prop.position.y - prevPropY) * 2f);
        prevPropY = prop.position.y;

        if (goalDist < 0.6f && grabbed)
        {
            FinishEpisode(10f);
            return;
        }

        AddReward(-1f / MaxStep);

        environmentUIManager.DisplayInfo($"Max Reward: {maxReward}\nCurrent Reward: {GetCumulativeReward()}\nSteps: {StepCount} / {MaxStep}\nEpisode: {episode}");
    }

    void FinishEpisode(float finalReward = 0f)
    {
        AddReward(finalReward);

        if (GetCumulativeReward() > maxReward)
            maxReward = GetCumulativeReward();

        EndEpisode();
    }

    void RotateJoint(JointInfo j, float input)
    {
        float actual = j.body.jointPosition[0] * Mathf.Rad2Deg;

        j.target += input * j.speed * Time.fixedDeltaTime;
        j.target = Mathf.Clamp(j.target, actual - 10f, actual + 10f);

        ArticulationDrive drive = j.body.xDrive;
        drive.target = j.target;
        j.body.xDrive = drive;
    }
}
