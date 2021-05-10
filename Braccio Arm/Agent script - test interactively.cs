using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RobotArmAgent_s4_Interactively : Agent
{
    public GameObject goal;
    public Transform BasePivot, UpperPivot, LowerPivot, WristPivot, End;
    public int trainingEnv = 2; // 2 (state only contains relative pos.) or 3 (state include arm rotation angles)
    
    bool ReachGoal()
    {
        float distToGoal = Vector3.Distance(End.position, goal.transform.position);
        if (distToGoal <= 0.5f)
        {
            print("Reach goal !\n");
            return true;
        }
        else
            return false;
    }

    void Update()
    {
        if (ReachGoal() == false)
        {
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(trainingEnv == 2)
            sensor.AddObservation(End.position - goal.transform.position);
        else //trainingEnv == 3
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
        BasePivot.Rotate(0, vectorAction[0] * speed, 0);
        UpperPivot.Rotate(vectorAction[1] * speed, 0, 0);
        LowerPivot.Rotate(vectorAction[2] * speed, 0, 0);
        WristPivot.Rotate(vectorAction[3] * speed, 0, 0);

        //if collision happens or angle rotation is our of range, terminate this training session
        if (GlobalVarToCheckCollision.collisionHappens || !Rotation_in_range() || End.transform.position.y <= 0.07f)
        {
            print(GlobalVarToCheckCollision.collisionHappens + "\n");
        }
    }
}