using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Input : MonoBehaviour
{
    private Fighter_Mov F_Mov;
    private SpriteRenderer SR;

    //Keycodes
    public KeyCode UP, DOWN, LEFT, RIGHT, A, B, C;
    public int StickPos, priorStickPos;
    private bool pos_changed;
    private float pos_duration;
    private bool facingLeft = true;
    private bool onRight = true;

    // Reads player input frame by frame. References the fighter's moves and physics through seperate scripts.
    void Start()
    {
        F_Mov = GetComponent<Fighter_Mov>();
        SR = GetComponent<SpriteRenderer>();
        priorStickPos = 5; StickPos = 5;
    }

    void Update()
    {
        //Get current stick directions
        UpdateStick();

        //Call movement function
        int x = 0; int y = 0; bool j;
        if (StickPos % 3 == 0) x = 1; else if (StickPos % 3 == 1) x = -1;
        if (StickPos <= 3) y = -1; else if (StickPos >= 7) y = 1;

        j = (pos_changed && y > 0 && priorStickPos < 7);

        //update character facing
        if (facingLeft != onRight && F_Mov.grounded)
            facingLeft = onRight;
        SR.flipX = facingLeft;

        F_Mov.Movement_Update(x, y, j);

    }

    public void UpdateStick()
    {
        //Read player inputs
        pos_changed = false;
        int pos = 5;
        if (Input.GetKey(LEFT)) pos--; if (Input.GetKey(RIGHT)) pos++;
        if (Input.GetKey(UP)) pos += 3; if (Input.GetKey(DOWN)) pos -= 3;

        //Update stick position save if position changed
        if(pos != StickPos)
        {
            priorStickPos = StickPos;
            StickPos = pos;
            pos_duration = 0;
            pos_changed = true;
        }
        else
        {
            pos_duration = Mathf.Clamp(pos_duration + Time.deltaTime,0,3);
        }

    }

    //Tell the fighter which side they are on
    public void SetRightBool(bool isOnRight)
    {
        onRight = isOnRight;
    }

}
