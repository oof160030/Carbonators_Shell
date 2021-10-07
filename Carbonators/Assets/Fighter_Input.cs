﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FighterState {NEUTRAL, ATTACK, BLOCK, HITSTUN, HITSTOP, TUMBLE, GROUNDED, STUN, DEAD, ANIMATING, LOCKED};
public class Fighter_Input : MonoBehaviour
{
    //The master script for each fighter, responsible for calling all other scripts

    //Fighter State - Referenced to keep track of what the fighter is currently doing and influence if inputs are received.
    private FighterState F_State;
    private float stunTime = 0; //Duration used for various stun effects

    //Child components
    private Fighter_Mov F_Mov; //Controls fighter movement
    private Fighter_Attack F_Atk; //Calls fighter attacks and interprets inputs
    private Fighter_HitDetection F_HitDet;
    private Fighter_Stats F_Stats;

    //Gameobject components
    private SpriteRenderer SR; //Displays sprites
    public int PortNumber; //Determines if player 1 or 2 is controlling this character
    private Animator AR; //The gameobject's animator object, controls some fighter logic
    public Transform Hurtbox_TF; //The fighter's hurtbox position

    //Keycodes
    public KeyCode UP, DOWN, LEFT, RIGHT, A, B, C;
    //Joystick variables
    public int StickPos, priorStickPos; //Stores current stick position (1-9) and the prior position
    private bool pos_changed; //Triggers the frame when the stick changes position
    private float pos_duration; //Stores the time the position has been held, up to 3 seconds
    //Button variables
    private bool APressed, BPressed, CPressed; //Detects whether the button is currently being pressed or not
    private float ADuration, BDuration, CDuration; //Tracks how long the button has been pressed / released

    //Positioning Variables
    private bool facingLeft = true; //Stores if the fighter is currently facing left
    private bool onRight = true; //Stores whether the fighter is currently on the right of the opponent
    public bool CanBackUp = true; //Stores whether the fighter is too close to the wall behind them

    // Reads player input frame by frame. References the fighter's moves and physics through seperate scripts.
    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        AR = GetComponent<Animator>();
        
        F_Mov = GetComponent<Fighter_Mov>();
        F_Atk = GetComponent<Fighter_Attack>(); F_Atk.Initialize(PortNumber, this,  AR);
        F_HitDet = GetComponentInChildren<Fighter_HitDetection>(); F_HitDet.Init(PortNumber, this);
        F_Stats = GetComponent<Fighter_Stats>(); F_Stats.Init(this);

        priorStickPos = 5; StickPos = 5;
    }

    void Update()
    {
        //Reduce stun times
        if(F_State == FighterState.HITSTUN)
        {
            stunTime -= Time.deltaTime;
            if(stunTime <= 0)
            {
                stunTime = 0;
                Change_State(FighterState.NEUTRAL);
            }
        }

        //Get current stick directions
        UpdateStick();
        UpdateButtons();

        //Call movement function
        int x = 0; int y = 0; bool j;
        if (StickPos % 3 == 0) x = 1; else if (StickPos % 3 == 1) x = -1;
        if (StickPos <= 3) y = -1; else if (StickPos >= 7) y = 1;

        j = (pos_changed && y > 0 && priorStickPos < 7);

        //update character facing
        UpdateFacing();

        if(F_State == FighterState.NEUTRAL)
        {
            //Manually move the player, if they are able to be controlled
            F_Mov.Movement_Update(x, y, j);

            //Check which command inputs, if any, the player has generated
            F_Atk.CheckMoveList(APressed && ADuration == 0);
        }
        F_Mov.Gravity_Update();

        //Update player movement based on distance from other fighter
        if (!CanBackUp)
            F_Mov.WallMovement(onRight);

    }

    //Update character facing
    private void UpdateFacing()
    {
        //Set facing direction if on ground and direction has changed 
        if (facingLeft != onRight && F_Mov.grounded)
            facingLeft = onRight;
        
        //Update sprite renderer and hurtbox based on results
        SR.flipX = facingLeft;
        Hurtbox_TF.localScale = new Vector3((facingLeft ? -1 : 1), 1, 1);
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

    //Manage state changes based on moves and taking damage
    private void Change_State(FighterState newState)
    {
        //Check if the state transition is valid
        switch(F_State)
        {
            case FighterState.NEUTRAL:
                if(newState == FighterState.HITSTUN)
                {
                    //transition to Hitstun
                    F_State = FighterState.HITSTUN;
                }
                break;
            case FighterState.HITSTUN:
                {
                    //Transition to Neutral
                    F_State = FighterState.NEUTRAL;
                }
                break;
        }

    }
    public void Damaged(SO_Hitbox HB_Data, bool facing_right)
    {
        //Deduct health
        F_Stats.Take_Damage(HB_Data.HB_damage);

        //Change fighter state, and set time to return to normal state
        Change_State(FighterState.HITSTUN);
        stunTime = HB_Data.hitStun;

        //Launch the fighter (using mov script)
        F_Mov.Damage_Launch(HB_Data.HB_Knockback, facing_right);
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
