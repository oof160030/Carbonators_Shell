using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Input : MonoBehaviour
{
    private Fighter_Mov F_Mov;
    private Fighter_Attack F_Atk;
    private SpriteRenderer SR;
    public int PortNumber; //Determines if player 1 or 2 is controlling this character
    private Animator AR;

    //Keycodes
    public KeyCode UP, DOWN, LEFT, RIGHT, A, B, C;
    //Joystick variables
    public int StickPos, priorStickPos;
    private bool pos_changed;
    private float pos_duration;
    //Button variables
    private bool APressed, BPressed, CPressed;
    private float ADuration, BDuration, CDuration;

    //Positioning Variables
    private bool facingLeft = true;
    private bool onRight = true;
    public bool CanBackUp = true;

    // Reads player input frame by frame. References the fighter's moves and physics through seperate scripts.
    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        AR = GetComponent<Animator>();
        
        F_Mov = GetComponent<Fighter_Mov>();
        F_Atk = GetComponent<Fighter_Attack>(); F_Atk.Initialize(PortNumber, this,  AR);

        priorStickPos = 5; StickPos = 5;
    }

    void Update()
    {
        //Get current stick directions
        UpdateStick();
        UpdateButtons();

        //Call movement function
        int x = 0; int y = 0; bool j;
        if (StickPos % 3 == 0) x = 1; else if (StickPos % 3 == 1) x = -1;
        if (StickPos <= 3) y = -1; else if (StickPos >= 7) y = 1;

        j = (pos_changed && y > 0 && priorStickPos < 7);

        //update character facing
        if (facingLeft != onRight && F_Mov.grounded)
            facingLeft = onRight;
        SR.flipX = facingLeft;

        //Manually move the player, if they are able to be controlled
        F_Mov.Movement_Update(x, y, j);

        //Check which command inputs, if any, the player has generated
        F_Atk.CheckMoveList(APressed && ADuration==0);

        //Update player movement based on distance from other fighter
        if (!CanBackUp)
            F_Mov.WallMovement(onRight);

    }

    //Update the fighter's input parameters
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
    public void UpdateButtons()
    {
        //Increase time buttons have been in the current state, up to 3 seconds
        ADuration = Mathf.Clamp(ADuration+Time.deltaTime,0,3);

        //Check if a button has just been pressed or released
        if(Input.GetKeyDown(A))
        {
            APressed = true;
            ADuration = 0;
        }
        else if(Input.GetKeyUp(A))
        {
            APressed = false;
            ADuration = 0;
        }
    }

    //Tell the fighter which side they are on
    public void SetRightBool(bool isOnRight)
    {
        onRight = isOnRight;
    }

    public bool IsOnRight()
    {
        return onRight;
    }

    public bool IsFacingRight()
    {
        return !facingLeft;
    }

    public bool IsGrounded()
    {
        return F_Mov.grounded;
    }

}
