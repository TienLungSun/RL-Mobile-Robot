using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Car_Agent_s7 : Agent
{
    public Transform[] distSensor = new Transform[18];
    RaycastHit hit;
    public GameObject robot, goal, block;
    float rayLength = 4.0f;
    Vector3 CarOriginalPos, GoalOriginalPos, BlockOriginalPos;
    Quaternion BlockOriginalRotation;

    void Start()
    {
        CarOriginalPos = robot.transform.position;
        GoalOriginalPos = goal.transform.position;
        BlockOriginalPos = block.transform.position;
        BlockOriginalRotation = block.transform.rotation; 
    }

    public override void OnEpisodeBegin()
    {
        robot.transform.position = CarOriginalPos; //Back to original position
        //robot.transform.Translate(Random.Range(-1.0f, 1.0f), 0, Random.Range(-0.5f, 0.5f));
        robot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        goal.transform.position = GoalOriginalPos; 
        //goal.transform.Translate(Random.Range(-1.0f, 1.0f), 0, Random.Range(-0.5f, 0.5f));
        goal.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        block.transform.position = BlockOriginalPos;
        //block.transform.Translate(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        block.transform.rotation = BlockOriginalRotation;
        //block.transform.Rotate(0, Random.Range(-5.0f, 5.0f), 0);
    }

    // Stage 1: Goal is in +-40 degrees (in real robot this means the robot can 'see' the goal) and there are no blocks in this area 
    // Stage 2: Goal is in +-40 degrees but there are blocks in the facing direction
    // Stage 3: Goal is not in +-40 degrees (in real robot this means the robot cannot 'see' the goal) 
    private int DetermineStage()
    {
        int stage=0;
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
        if (DetermineStage()==1) 
        {   // s = (1, 0, 0, theta, d1~dn)
            sensor.AddObservation(1); 
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            Vector3 targetDir = goal.transform.position - robot.transform.position;
            float facingAngle = Vector3.SignedAngle(robot.transform.forward, targetDir, Vector3.up);
            sensor.AddObservation(facingAngle); // theta 
        }
        else if(DetermineStage() == 2)
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
        int oldStage = DetermineStage();
        robot.transform.Translate(0, 0, vectorAction[0]*0.4f);
        robot.transform.Rotate(0, vectorAction[1]*10.0f, 0);
        int newStage = DetermineStage();
        AddReward(-0.005f * newStage); //punish more steps no. and steps at larger state no.
        AddReward(-0.005f * (oldStage-newStage)); //punish stage change if newStage < oldStage

        //Part II: rewards based on distance sensors, e.g. Lidar
        for (int i = 0; i < 18; i++)
        {
            //Debug.DrawRay(distSensor[i].position, distSensor[i].forward* rayLength, Color.white);
            if (Physics.Raycast(distSensor[i].position, distSensor[i].forward, out hit, rayLength))
            {
                if (hit.collider.tag == "goal" && ((i >= 0 && i <= 2) || (i >= 16 && i <= 17)) && hit.distance <= 2.0f) // if reach goal with front end
                {
                    //print("Goal!");
                    AddReward(100.0f);
                    EndEpisode();
                }
                else if (hit.distance < 1.0f)  //too close to obstacle
                {
                    Debug.DrawRay(distSensor[i].position, distSensor[i].forward * rayLength, Color.red);
                    AddReward(-0.5f);
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