﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Car_Agent_s6 : Agent
{
    public Transform[] distSensor = new Transform[18];
    RaycastHit hit;
    public GameObject robot, goal;
    float rayLength = 4.0f;
    Vector3 CarOriginalPos, GoalOriginalPos;

    void Start()
    {
        CarOriginalPos = new Vector3(robot.transform.position.x, robot.transform.position.y, robot.transform.position.z);
        GoalOriginalPos = new Vector3(goal.transform.position.x, goal.transform.position.y, goal.transform.position.z);
    }

    public override void OnEpisodeBegin()
    {
        robot.transform.position = CarOriginalPos; //回到原點
        robot.transform.Translate(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        robot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        goal.transform.position = GoalOriginalPos; //回到原點
        goal.transform.Translate(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        goal.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Get facing angle
        Vector3 targetDir = goal.transform.position - robot.transform.position;
        float facingAngle = Vector3.SignedAngle(targetDir, robot.transform.forward, Vector3.up); //-180~180
        sensor.AddObservation(facingAngle / 180.0f); //Normalize to -1~1

        for (int i = 0; i < 18; i++)
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
        AddReward(-0.005f);
        robot.transform.Translate(0, 0, vectorAction[0]*0.2f);
        robot.transform.Rotate(0, vectorAction[1]*10.0f, 0);

        //scan the environment
        for (int i = 0; i < 18; i++)
        {
            if (Physics.Raycast(distSensor[i].position, distSensor[i].forward, out hit, rayLength))
            {
                if (hit.distance < 2.0f) //the robot is too close to an object
                {
                    if (hit.collider.tag == "goal" && ((i >= 0 && i <= 3) || (i >= 15 && i <= 17))) // if hit goal with front end
                    {
                        //print("Goal!\n");
                        AddReward(10.0f);
                        EndEpisode();
                    }
                    else
                    {
                        //print("Hit objects!\n");
                        AddReward(-5.0f);
                    }
                }
            }
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }
}