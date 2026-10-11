using System.Collections.Generic;
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

public class RoboticArmAgent : MonoBehaviour
{
    [SerializeField] List<JointInfo> joints;

    static readonly Key[] positiveKeys = { Key.Q, Key.S, Key.D, Key.R, Key.T, Key.H };
    static readonly Key[] negativeKeys = { Key.A, Key.W, Key.E, Key.F, Key.G, Key.Y };

    void Start()
    {
    }

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        int count = Mathf.Min(joints.Count, positiveKeys.Length);
        for (int i = 0; i < count; i++)
        {
            float input = 0f;
            if (kb[positiveKeys[i]].isPressed) input += 1f;
            if (kb[negativeKeys[i]].isPressed) input -= 1f;

            RotateJoint(joints[i], input);
        }
    }

    void RotateJoint(JointInfo j, float input)
    {
        float actual = j.body.jointPosition[0] * Mathf.Rad2Deg;

        j.target += input * j.speed * Time.fixedDeltaTime;
        j.target = Mathf.Clamp(j.target, actual - 20f, actual + 20f);

        ArticulationDrive drive = j.body.xDrive;
        drive.target = j.target;
        j.body.xDrive = drive;
    }
}
