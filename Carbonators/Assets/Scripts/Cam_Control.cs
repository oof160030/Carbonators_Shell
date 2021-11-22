using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_Control : MonoBehaviour
{
    //Controls the camera's position relative to the fighters. Descriptions updated 10/3
    
    public Transform fighter1, fighter2; //References to the two fighter's positions
    Vector3 Cam_Pos = new Vector3(0,0,-10); //The last valid position of the camera.

    void Update()
    {
        //Update camera position every other frame
        if (Time.frameCount % 2 == 0)
            updateCameraPos();
    }

    private void updateCameraPos()
    {
        //SET CAMERA HORIZONTAL POSITION
        //Get the distance between the two fighters, and set the default horiz. camera position directly between them.
        float xDist = Mathf.Abs(fighter1.position.x - fighter2.position.x);
        Cam_Pos.x = Mathf.Clamp((fighter1.position.x + fighter2.position.x) / 2.0f, -5.5f, 5.5f);
        /*
        //When the fighters are closer than 4 units, only move if the camera is too far from the further fighter
        if (xDist < 4)
        {
            float range = (4 - xDist) / 2.0f;

            Cam_Pos.x = Mathf.Clamp(transform.position.x, Cam_Pos.x - range, Cam_Pos.x + range);
        }
        */

        //SET CAMERA VERTICAL POSITION
        //Set y position to track highest fighter's position
        Cam_Pos.y = Mathf.Clamp(Mathf.Max(fighter1.position.y, fighter2.position.y), 0, 15);

        //Update the camera position based on calculation
        transform.position = Cam_Pos;
    }
}
