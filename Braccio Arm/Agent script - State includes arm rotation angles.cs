using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RobotArmAgent_s3 : Agent
{
    public Transform goal;
    public Transform BasePivot, UpperPivot, LowerPivot, WristPivot, End;
    Quaternion BasePivotRoation, UpperPivotRotation, LowerPivotRotation, WristPivotRotation, GoalRotation;

    void Start()
    {
        BasePivotRoation = BasePivot.rotation;
        UpperPivotRotation = UpperPivot.rotation;
        LowerPivotRotation = LowerPivot.rotation;
        WristPivotRotation = WristPivot.rotation;
        GoalRotation = goal.rotation;
    }

    public override void OnEpisodeBegin()
    {
        GlobalVarToCheckCollision.collisionHappens = false;
        goal.transform.localPosition = new Vector3(Random.Range(-1.2f, 1.2f), -1.487f, Random.Range(0.5f, 1.3f));  //Back to original position
        goal.rotation = GoalRotation;

        BasePivot.rotation = BasePivotRoation;
        UpperPivot.rotation = UpperPivotRotation;
        LowerPivot.rotation = LowerPivotRotation;
        WristPivot.rotation = WristPivotRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(End.position - goal.transform.position);

        float BaseRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(BasePivot).y;
        float UArmRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(UpperPivot).x;
        float LArmRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(LowerPivot).x;
        float WRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(WristPivot).x;

        sensor.AddObservation(BaseRotationAngle);
        sensor.AddObservation(UArmRotationAngle);
        sensor.AddObservation(LArmRotationAngle);
        sensor.AddObservation(WRotationAngle);
    }

    bool Rotation_in_range()  // check if arm's rotation is within range
    {
        float BaseRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(BasePivot).y;
        float UArmRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(UpperPivot).x;
        float LArmRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(LowerPivot).x;
        float WRotationAngle = UnityEditor.TransformUtils.GetInspectorRotation(WristPivot).x;
        if ((BaseRotationAngle >= -90 && UArmRotationAngle <= 90) &&
            (UArmRotationAngle >= 0 && UArmRotationAngle <= 90) &&
            (LArmRotationAngle >= 0 && LArmRotationAngle <= 90) &&
            (WRotationAngle >= 0 && WRotationAngle <= 90))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float speed = 1.0f;
        AddReward(-0.005f);

        BasePivot.Rotate(0, vectorAction[0] * speed, 0);
        UpperPivot.Rotate(vectorAction[1] * speed, 0, 0);
        LowerPivot.Rotate(vectorAction[2] * speed, 0, 0);
        WristPivot.Rotate(vectorAction[3] * speed, 0, 0);

        //if collision happens or angle rotation is our of range, terminate this training session
        if (GlobalVarToCheckCollision.collisionHappens || !Rotation_in_range() || End.transform.position.y <= 0.07f)
        {
            AddReward(-5.0f);
            EndEpisode();
        }

        // check reach goal
        float distToGoal = Vector3.Distance(End.position, goal.transform.position);
        //print("dist = " + distToGoal.ToString("0.00") + "\n");

        if (distToGoal <= 0.3f)
        {
            AddReward(20.0f);
            //print("Goal !\n");
            EndEpisode();
        }
    }
}