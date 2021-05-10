using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.IO;
using System;

public class RobotArmAgent_s4_RecordData : Agent
{
    public GameObject goal;
    public Transform BasePivot, UpperPivot, LowerPivot, WristPivot, End;
    Vector3 goalOriginalPos;
    Quaternion BasePivotRoation, UpperPivotRotation, LowerPivotRotation, WristPivotRotation;
    int TotalTests, NoTest;
    string filePath;
    StreamWriter writer;
    public int trainingEnv = 2; // 2 (state only contains relative pos.) or 3 (state include arm rotation angles)

    void Start()
    {   
        goalOriginalPos = goal.transform.position;
        BasePivotRoation = BasePivot.rotation;
        UpperPivotRotation = UpperPivot.rotation;
        LowerPivotRotation = LowerPivot.rotation;
        WristPivotRotation = WristPivot.rotation;

        TotalTests = 2; // test the NN model performance for N times
        NoTest = 1;
        filePath = "trajectory.csv";
        writer = new StreamWriter(filePath);
        writer.WriteLine("time, x, y, z, reward");
    }

    private void OnApplicationQuit()
    {
        writer.Close();
    }

    public override void OnEpisodeBegin()
    {
        GlobalVarToCheckCollision.collisionHappens = false;
        goal.transform.position = goalOriginalPos; //Back to original position
        BasePivot.rotation = BasePivotRoation;
        UpperPivot.rotation = UpperPivotRotation;
        LowerPivot.rotation = LowerPivotRotation;
        WristPivot.rotation = WristPivotRotation;
    }

    Boolean ReachGoal()
    {
        float distToGoal = Vector3.Distance(End.position, goal.transform.position);
        if (distToGoal <= 0.3f)
            return true;
        else
            return false;
    }

    void Update()
    {
        if (NoTest <= TotalTests)
        {
            if (ReachGoal() == false)
            {
                RequestDecision();
            }
            else //reach goal
            {
                string s = "Finish No " + NoTest.ToString();
                writer.WriteLine(s);
                NoTest = NoTest + 1;
                EndEpisode(); // Finish this test and start next test
            }
        }
        // else NoTest already larger than TotalTests, do nothing, wait for user to close the game
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (trainingEnv == 2)
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
        float reward = 0.0f;

        BasePivot.Rotate(0, vectorAction[0] * speed, 0);
        UpperPivot.Rotate(vectorAction[1] * speed, 0, 0);
        LowerPivot.Rotate(vectorAction[2] * speed, 0, 0);
        WristPivot.Rotate(vectorAction[3] * speed, 0, 0);

        reward = reward - 0.005f;

        //record time, (x, y) position and reward
        string t = System.DateTime.Now.ToLongTimeString();
        float x = End.transform.position.x;
        float y = End.transform.position.y;
        float z = End.transform.position.z;

        //if collision happens or angle rotation is our of range, terminate this training session
        if (GlobalVarToCheckCollision.collisionHappens || !Rotation_in_range())
        {
            reward = reward - 5.0f;
        }

        // check reach goal
        if(ReachGoal())
        {
            reward = reward + 20.0f;
        }

        //write to file
        string s = t + ", " + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ", " + reward.ToString();
        writer.WriteLine(s);
    }
}