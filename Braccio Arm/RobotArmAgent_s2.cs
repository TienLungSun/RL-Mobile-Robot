using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RobotArmAgent_s2 : Agent
{
    public GameObject goal;
    public Transform BasePivot, UpperPivot, LowerPivot, WristPivot, End;
    Vector3 goalOriginalPos;
    Quaternion BasePivotRoation, UpperPivotRotation, LowerPivotRotation, WristPivotRotation;

    void Start()
    {   
        goalOriginalPos = goal.transform.position;
        BasePivotRoation = BasePivot.rotation;
        UpperPivotRotation = UpperPivot.rotation;
        LowerPivotRotation = LowerPivot.rotation;
        WristPivotRotation = WristPivot.rotation;
    }

    public override void OnEpisodeBegin()
    {
        GlobalVarToCheckCollision.collisionHappens = false;
        goal.transform.position = goalOriginalPos; //Back to original position
        //goal.transform.Translate(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

        BasePivot.rotation = BasePivotRoation;
        BasePivot.Rotate(0, Random.Range(-45, 45), 0);
        UpperPivot.rotation = UpperPivotRotation;
        UpperPivot.Rotate(Random.Range(-10, 10), 0, 0);
        LowerPivot.rotation = LowerPivotRotation;
        LowerPivot.Rotate(Random.Range(-10, 10), 0, 0);
        WristPivot.rotation = WristPivotRotation;
        WristPivot.Rotate(Random.Range(-10, 10),0, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(WristPivot.position - goal.transform.position);
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

        if ((BaseRotationAngle >= -90 && BaseRotationAngle <= 90) &&
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
        float factor = 10.0f;

        AddReward(-0.005f);

        BasePivot.Rotate(0, vectorAction[0]*factor, 0);
        UpperPivot.Rotate(vectorAction[1]*factor, 0, 0);
        LowerPivot.Rotate(vectorAction[2]*factor, 0, 0);
        WristPivot.Rotate(vectorAction[3]*factor, 0, 0);

        //if collision happens or angle rotation is our of range, terminate this training session
        if (GlobalVarToCheckCollision.collisionHappens || !Rotation_in_range())
        {
            AddReward(-5.0f);
            EndEpisode();
        }

        // check reach goal
        float distToGoal = Vector3.Distance(End.position, goal.transform.position);
        //print("dist = " + distToGoal.ToString("0.00") + "\n");
        if(distToGoal <= 0.3f)
        {
            AddReward(10.0f);
            print("Goal !\n");
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
        actionsOut[2] = 0.0f;
        actionsOut[3] = 0.0f;
    }
}