using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.IO;
using System;

public class Car_Agent_s8 : Agent
{
    public Transform[] distSensor = new Transform[18];
    RaycastHit hit;
    public GameObject robot, goal;
    float rayLength = 4.0f;
    string filePath;
    StreamWriter writer;

    void Start()
    {
        filePath = "trajectory.csv";
        writer = new StreamWriter(filePath);
        writer.WriteLine("time, x, y");
    }

    private void OnApplicationQuit()
    {
        writer.Close();
    }

    void Update()
    {
        bool ReachGoal = false;

        for (int i = 0; i < 18; i++)
        {
            if (Physics.Raycast(distSensor[i].position, distSensor[i].forward, out hit, rayLength))
            {
                if (hit.collider.tag == "goal" && ((i >= 0 && i <= 2) || (i >= 16 && i <= 17)) && hit.distance <= 2.0f) // if reach goal with front end
                {
                    ReachGoal = true;
                    print("Goal!");
                }
            }
        }
        if(ReachGoal == false)
        {
            RequestDecision();
        }
    }


    private int DetermineStage()
    {
        int stage = 0;
        Vector3 targetDir = goal.transform.position - robot.transform.position;
        float facingAngle = Vector3.SignedAngle(robot.transform.forward, targetDir, Vector3.up);

        if (Mathf.Abs(facingAngle) <= 40)
        {
            if (Physics.Raycast(robot.transform.position, targetDir, out hit)) //cast ray along target direction
            {
                if (hit.collider.tag == "goal") //hit goal
                {
                    Debug.DrawRay(robot.transform.position, targetDir, Color.white);  //debug drawing to show targetDir
                    stage = 1;
                }
                else //there is obstacle in between
                {
                    stage = 2;
                }
            }
            else
            {
                Debug.DrawRay(robot.transform.position, targetDir, Color.red);
                print("Wrong! No object hit alogn target dir.");
            }
        }
        else // facing angle >40
        {
            stage = 3;
        }
        return stage;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (DetermineStage() == 1)
        {   // s = (1, 0, 0, theta, d1~dn)
            sensor.AddObservation(1);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            Vector3 targetDir = goal.transform.position - robot.transform.position;
            float facingAngle = Vector3.SignedAngle(robot.transform.forward, targetDir, Vector3.up);
            sensor.AddObservation(facingAngle); // theta 
        }
        else if (DetermineStage() == 2)
        {   // s = (0, 1, 0, 0, d1~dn)
            sensor.AddObservation(0);
            sensor.AddObservation(1);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else if (DetermineStage() == 3)
        {   // s = (0, 0, 1, 0, d1~dn)
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(1);
            sensor.AddObservation(0);
        }
        else
        {
            print("Error in determining stages");
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(1);
            sensor.AddObservation(0);
        }

        for (int i = 0; i < 18; i++) //add distance detected by the distance sensors
        {
            if (Physics.Raycast(distSensor[i].position, distSensor[i].forward, out hit, rayLength))
            {
                sensor.AddObservation(hit.distance / rayLength); //Normalize to 0~1
            }
            else
            {
                sensor.AddObservation(1);
            }
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        robot.transform.Translate(0, 0, vectorAction[0]*0.2f);
        robot.transform.Rotate(0, vectorAction[1]*10.0f, 0);

        //record time and (x, y) position
        string t = System.DateTime.Now.ToLongTimeString();
        float x = robot.transform.position.x;
        float z = robot.transform.position.z;
        string s = t + ", " + x.ToString() + ", " + z.ToString();
        writer.WriteLine(s);
    }

}