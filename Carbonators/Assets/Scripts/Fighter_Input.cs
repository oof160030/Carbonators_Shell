using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FighterState {NEUTRAL, ATTACK, BLOCK, HITSTUN, HITSTOP, TUMBLE, GROUNDED, STUN, DEAD, ANIMATING, LOCKED};
public class Fighter_Input : MonoBehaviour
{
    //The master script for each fighter, responsible for calling all other scripts
    public GameMGR MGR;

    //Fighter State - Referenced to keep track of what the fighter is currently doing and influence if inputs are received.
    public FighterState F_State;
    private float stunTime = 0; //Duration used for various stun effects
    public float hitStopTime = 0; //Duration used for attack impacts

    //Child components
    private Fighter_Mov F_Mov; //Controls fighter movement
    private Fighter_Attack F_Atk; //Calls fighter attacks and interprets inputs
    private Fighter_HitDetection F_HitDet;
    private Fighter_Stats F_Stats;
    private Fighter_AnimControl F_AnimCtrl; //Resets animator triggers and calls special animations (?)

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
    private int Stick_X, Stick_Y; //Stores the absolute horizontal and vertical axis direction
    //Button variables
    private bool APressed, BPressed, CPressed; //Detects whether the button is currently being pressed or not
    private float ADuration, BDuration, CDuration; //Tracks how long the button has been pressed / released

    //Positioning Variables
    private bool facingLeft = true; //Stores if the fighter is currently facing left
    private bool onRight = true; //Stores whether the fighter is currently on the right of the opponent
    public bool CanBackUp = true; //Stores whether the fighter is too close to the wall behind them
    public bool gravityOn = true; //Determines if gravity is currently active

    // Reads player input frame by frame. References the fighter's moves and physics through seperate scripts.
    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        AR = GetComponent<Animator>();

        F_AnimCtrl = GetComponent<Fighter_AnimControl>(); F_AnimCtrl.Init(AR);
        F_Mov = GetComponent<Fighter_Mov>(); F_Mov.Initialize(this, AR);
        F_Atk = GetComponent<Fighter_Attack>(); F_Atk.Initialize(PortNumber, this,  AR, F_AnimCtrl);
        F_HitDet = GetComponentInChildren<Fighter_HitDetection>(); F_HitDet.Init(PortNumber, this);
        F_Stats = GetComponent<Fighter_Stats>(); F_Stats.Init(this);
        

        priorStickPos = 5; StickPos = 5;
    }

    void Update()
    {
        

        //Reduce hitstop duration
        if(hitStopTime > 0)
        {
            hitStopTime = Mathf.Clamp(hitStopTime - Time.deltaTime * 60f,0,100); //Reduce hitstop duration by frames
            if (hitStopTime == 0)
            {
                F_Mov.FreezeMovement(false);
                F_AnimCtrl.SetAnimSpeed(1);
            }
        }
        
        //Reduce stun times
        if((F_State == FighterState.HITSTUN || F_State == FighterState.BLOCK) && hitStopTime == 0)
        {
            stunTime -= Time.deltaTime;
            if(stunTime <= 0)
            {
                stunTime = 0;
                Change_State(FighterState.NEUTRAL);
            }
        }

        //Get current stick directions and button inputs
        UpdateStick();
        UpdateButtons();

        //Convert stick input to jump input and axis directions 
        Stick_X = 0; Stick_Y = 0; bool jump;
        if (StickPos % 3 == 0) Stick_X = 1; else if (StickPos % 3 == 1) Stick_X = -1;
        if (StickPos <= 3) Stick_Y = -1; else if (StickPos >= 7) Stick_Y = 1;

        jump = (pos_changed && Stick_Y > 0 && priorStickPos < 7);

        //update character facing
        UpdateFacing();

        //Update animator stick inputs
        AR.SetInteger("Horiz_Input", relative_X());
        AR.SetInteger("Vert_Input", Stick_Y);        

        //Standard Movement if fighter is in neutral state and not in hitstop
        if (F_State == FighterState.NEUTRAL && hitStopTime == 0)
        {
            //Set Jump animator trigger if needed
            if (jump && IsGrounded())
            {
                AR.SetTrigger("Jump");
                F_AnimCtrl.Jump_AnimTimer = 6;
            }

            //Manually move the player, if they are able to be controlled
            F_Mov.Standard_Movement(Stick_X, Stick_Y, jump);

            //Check which command inputs, if any, the player has generated
            F_Atk.CheckMoveList((APressed && ADuration == 0), (F_Mov.grounded && StickPos < 4));
        }

        //Update speed based on gravity UNLESS fighter is in hitstop or animating with keyframes.
        if(hitStopTime == 0)
            F_Mov.Gravity_Update();

        //Update player movement if the current fighter cannot back up
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

    private int relative_X()
    {
        return facingLeft ? -Stick_X : Stick_X;
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
                else if(newState == FighterState.TUMBLE)
                {
                    //transition to Tumble
                    F_State = FighterState.TUMBLE;
                }
                else if(newState == FighterState.BLOCK)
                {
                    //transition to Block state
                    F_State = FighterState.BLOCK;
                    //Set block trigger
                    AR.SetTrigger("Block");
                    F_AnimCtrl.Block_AnimTimer = 6;
                }
                else if(newState == FighterState.ATTACK)
                {
                    //transition to Attack state
                    F_State = FighterState.ATTACK;
                }
                break;
            case FighterState.ATTACK:
                if (newState == FighterState.NEUTRAL)
                {
                    //transition to Neutral 
                    F_State = FighterState.NEUTRAL;
                }
                else if (newState == FighterState.HITSTUN)
                {
                    //transition to Hitstun
                    F_State = FighterState.HITSTUN;
                }
                else if (newState == FighterState.TUMBLE)
                {
                    //transition to Tumble
                    F_State = FighterState.TUMBLE;
                }
                break;
            case FighterState.HITSTUN:
                if (newState == FighterState.NEUTRAL)
                {
                    //Transition to Neutral
                    F_State = FighterState.NEUTRAL;
                    //Set recovery trigger
                    AR.SetTrigger("Hurt_Recover");
                }
                else if (newState == FighterState.TUMBLE)
                {
                    //transition to Tumble
                    F_State = FighterState.TUMBLE;
                }
                break;
            case FighterState.TUMBLE:
                if(newState == FighterState.GROUNDED)
                {
                    //Transition to Neutral
                    F_State = FighterState.GROUNDED;
                }
                break;
            case FighterState.BLOCK:
                if(newState == FighterState.NEUTRAL)
                {
                    //transition to Neutral
                    F_State = FighterState.NEUTRAL;
                    //Set block trigger
                    AR.SetTrigger("Unblock");
                    F_AnimCtrl.Unblock_AnimTimer = 6;
                }
                break;
            case FighterState.GROUNDED:
                if(newState == FighterState.NEUTRAL)
                {
                    //transition to Neutral
                    F_State = FighterState.NEUTRAL;
                }
                break;
        }

    }

    public void SetAttackState(bool state)
    {
        //Validate attack request here
        if(state && F_State == FighterState.NEUTRAL)
            Change_State(FighterState.ATTACK);
        else if (!state && F_State == FighterState.ATTACK)
            Change_State(FighterState.NEUTRAL);
    }

    public void Damaged(SO_Hitbox HB_Data, bool facing_right)
    {
        bool blocked_attack = false;
        //Determine whether the attack was blocked or not
        if(relative_X() < 0)
        {
            blocked_attack = true;
        }

        //If the attack was NOT blocked, deal damage and knockback;
        if(!blocked_attack)
        {
            //Deduct health
            F_Stats.Take_Damage(HB_Data.HB_damage);

            //If hit on the ground, change fighter state, and set time to return to normal state
            if(IsGrounded())
            {
                Change_State(FighterState.HITSTUN);
                stunTime = HB_Data.hitStun / 60.0f;
            }
            //If hit in the air, change to tumble state until landing
            else if(!IsGrounded())
            {
                Change_State(FighterState.TUMBLE);
            }
            

            //Choose the damage animation
            if (!IsGrounded())
                AR.SetTrigger("Hurt_Air");
            else if (Stick_Y == -1)
                AR.SetTrigger("Hurt_Crouching");
            else
                AR.SetTrigger("Hurt_Grounded");

            //Launch the fighter (using mov script)
            F_Mov.Damage_Launch(HB_Data.HB_Knockback, facing_right);
            setHitStop(HB_Data.hitStop);
        }
        //If the attack WAS blocked, play block animation and push the fighter
        if(blocked_attack)
        {
            //Change fighter state, and set time to return to normal state
            Change_State(FighterState.BLOCK);
            stunTime = HB_Data.blockStun / 60.0f;
            setHitStop(HB_Data.hitStop);

            //Push self away, OR push the attacker if against the wall
        }
        
    }

    //Tell the fighter which side they are on
    public void SetRightBool(bool isOnRight)
    {
        onRight = isOnRight;
    }

    public void setHitStop(float stopTime)
    {
        if(stopTime > 0)
        {
            hitStopTime = stopTime;
            F_Mov.FreezeMovement(true);
            F_AnimCtrl.SetAnimSpeed(0);
        }
    }

    //GETTER Functions

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

    public FighterState GetState()
    {
        return F_State;
    }
}
