using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_Control : MonoBehaviour
{
    //Controls the camera's position relative to the fighters. Descriptions updated 10/3
    
    public Transform fighter1, fighter2; //References to the two fighter's positions
    Vector3 Cam_Pos = new Vector3(0,0,-10); //The last valid position of the camera.

    public void Init(Transform f1, Transform f2)
    {
        fighter1 = f1; fighter2 = f2;
    }

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
        Cam_Pos.x = Mathf.Clamp((fighter1.position.x + fighter2.position.x) / 2.0f, -5.5f, 5.5f);

        //SET CAMERA VERTICAL POSITION
        //Set y position to track highest fighter's position
        Cam_Pos.y = Mathf.Clamp(Mathf.Max(fighter1.position.y, fighter2.position.y), 0, 15);

        //Update the camera position based on calculation
        transform.position = Cam_Pos;
    }
}
