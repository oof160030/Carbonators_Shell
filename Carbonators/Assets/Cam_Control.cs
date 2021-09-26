using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_Control : MonoBehaviour
{
    public Transform fighter1, fighter2;
    Vector3 Cam_Pos = new Vector3(0,0,-10);

    // Update is called once per frame
    void Update()
    {
        //Set x position to be between the two fighters if they are 10+ units apart
        float xDist = Mathf.Abs(fighter1.position.x - fighter2.position.x);
        Cam_Pos.x = Mathf.Clamp((fighter1.position.x + fighter2.position.x)/2.0f,-5.5f,5.5f);

        //If they are closer than 10 units, allow camera to stay still if between the fighters
        if (xDist < 10)
        {
            float range = (10 - xDist)/2.0f;
            Cam_Pos.x = Mathf.Clamp(transform.position.x, Cam_Pos.x - range, Cam_Pos.x + range);
        }

        //Set y position to track highest current fighter
        Cam_Pos.y = Mathf.Clamp(Mathf.Max(fighter1.position.y, fighter2.position.y),0,15);
        transform.position = Cam_Pos;
    }
}
