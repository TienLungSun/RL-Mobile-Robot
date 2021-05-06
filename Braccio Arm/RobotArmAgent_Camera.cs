using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RobotArmAgent_Camera : Agent
{
    public GameObject goal;
    public Transform UpperPivot, LowerPivot, WristPivot;
    Quaternion UpperPivotRotation, LowerPivotRotation, WristPivotRotation;

    void Start()
    {
        UpperPivotRotation = UpperPivot.rotation;
        LowerPivotRotation = LowerPivot.rotation;
        WristPivotRotation = WristPivot.rotation;
    }

    public override void OnEpisodeBegin()
    {
        GlobalVarToCheckCollision.collisionHappens = false;

        //Upper arm rotate along x (0-90) and z (-90~90) in Inspector window
        //(0-90) in Inspector window ==> localEulerAngle 0~90
        //(-90~90) in Inspector window ==> localEulerAngle 270~360, 0-90
        UpperPivot.rotation = UpperPivotRotation;
        if(Random.Range(0, 1) > 0.5f)
        {
            UpperPivot.Rotate(Random.Range(0, 90.0f), 0, Random.Range(270.0f, 360.0f));
        }
        else
        {
            UpperPivot.Rotate(Random.Range(0, 90.0f), 0, Random.Range(0, 90.0f));
        }
        
        // lower arm rotate along x (0-90) and z (-90~90)
        LowerPivot.rotation = LowerPivotRotation;
        if (Random.Range(0, 1) > 0.5f)
        {
            LowerPivot.Rotate(Random.Range(0, 90.0f), 0, Random.Range(270.0f, 360.0f));
        }
        else
        {
            LowerPivot.Rotate(Random.Range(0, 90.0f), 0, Random.Range(0, 90.0f));
        }

        // wrist roate x (0-90), y(-180~180) and z (-90~90)
        WristPivot.rotation = WristPivotRotation;
        if (Random.Range(0, 1) > 0.5f)
        {
            WristPivot.Rotate(Random.Range(0, 90.0f), Random.Range(0f, 360f), Random.Range(270.0f, 360.0f));
        }
        else
        {
            WristPivot.Rotate(Random.Range(0, 90.0f), Random.Range(0f, 360f), Random.Range(0, 90.0f));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        AddReward(-0.005f);

        // upper arm rotate along x (0-90) and z (-90~90) 
        // lower arm rotate along x (0-90) and z (-90~90)
        // wrist roate x (0-90), y(-180~180) and z (-90~90)
        UpperPivot.Rotate(vectorAction[0], 0, vectorAction[1]);
        LowerPivot.Rotate(vectorAction[2], 0, vectorAction[3]);
        WristPivot.Rotate(vectorAction[4], vectorAction[5], vectorAction[6]);

        //if collision happens, terminate this training session
        if(GlobalVarToCheckCollision.collisionHappens == true)
        {
            AddReward(-10.0f);
            //EndEpisode();
        }

        // check out of boundary, localEurlerAngel: 0~360, rotation in Inspection Window: 0~180~-180~-0
        // upper arm rotate along x (0-90) ==> localEulerAngle out of boundary 91~360
        if (UpperPivot.localEulerAngles.x > 90  && UpperPivot.localEulerAngles.x < 360)
        {
            AddReward(-5.0f);
            UpperPivot.Rotate(-vectorAction[0], 0, 0); //rotate back
        }
        
        if (UpperPivot.localEulerAngles.z > 90 && UpperPivot.localEulerAngles.z < 270) 
        { // upper arm rotate along z (-90~90)==> localEulerAngle out of boundary ==> 91~270
            AddReward(-5.0f);
            UpperPivot.Rotate(0, 0, -vectorAction[1]); //rotate back
        }
          
        if (LowerPivot.localEulerAngles.x > 90 && LowerPivot.localEulerAngles.x < 360)
        {  // lower arm rotate along x (0-90)
            AddReward(-5.0f);
            LowerPivot.Rotate(-vectorAction[2], 0, 0); //rotate back
        }

        if (LowerPivot.localEulerAngles.z > 90 && LowerPivot.localEulerAngles.z < 270)
        { // lower arm rotate along z (-90~90) 
            AddReward(-5.0f);
            LowerPivot.Rotate(0, 0, -vectorAction[3]); //rotate back
        }
  
        if (WristPivot.localEulerAngles.x > 90 && WristPivot.localEulerAngles.x < 360)
        {  // wrist rotate along x (0-90)
            AddReward(-5.0f);
            WristPivot.Rotate(-vectorAction[4], 0, 0); //rotate back
        }

        if (WristPivot.localEulerAngles.z > 90 && WristPivot.localEulerAngles.z < 270)
        { // wirst rotate along z (-90~90) 
            AddReward(-5.0f);
            WristPivot.Rotate(0, 0, -vectorAction[6]); //rotate back
        }

        // check reach goal
        float distToGoal = Vector3.Distance(WristPivot.position, goal.transform.position);
        if(distToGoal <= 0.5f)
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
        actionsOut[4] = 0.0f;
        actionsOut[5] = 0.0f;
        actionsOut[6] = 0.0f;
    }
}