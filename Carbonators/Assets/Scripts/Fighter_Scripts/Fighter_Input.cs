using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FighterState {NEUTRAL, ATTACK, BLOCK, HITSTUN, HITSTOP, TUMBLE, GROUNDED, STUN, DEAD, ANIMATING, LOCKED};
public class Fighter_Input : MonoBehaviour
{
    //==FIGHTER_INPUT: The master script for each fighter, responsible for calling all other scripts==

    private GameMGR MGR;
    private Fighter_Input Opponent; //Script for the opposing fighter

    //Fighter State - Referenced to keep track of what the fighter is currently doing and influence if inputs are received.
    public FighterState F_State;
    private float stunTime = 0; //Duration used for various stun effects, in seconds
    public float hitStopTime = 0; //Duration used for attack impacts, in frames

    //Child components
    private Fighter_Mov F_Mov; //Controls fighter movement
    private Fighter_Attack F_Atk; //Calls fighter attacks and interprets inputs
    private Fighter_HitDetection F_HitDet; //Connected to the fighters hurtboxes
    private Fighter_Stats F_Stats; //Tracks the stats of the fighter (such as Health and Shield)
    private Fighter_AnimControl F_AnimCtrl; //Resets animator triggers and calls special animations (?)

    //Gameobject components
    private SpriteRenderer SR; //Displays sprites, used here to flip sprites when looking left
    public int PortNumber; //Determines if player 1 or 2 is controlling this character
    private Animator AR; //The gameobject's animator object
    public Transform Hurtbox_TF; //The fighter's hurtbox transform, used to mirror the fighter's hurtbox when facing left

    //Keycodes
    public KeyCode UP, DOWN, LEFT, RIGHT, A, B, C; //Inputs can be changed by changing these Keycode variables
    //Joystick variables
    public int stickPos, priorStickPos; //Stores current and prior stick position (1-9)
    private bool pos_changed; //Triggers the frame when the stick changes position
    private float pos_duration; //Stores the time the current stick position has been held, up to 3 seconds
    private int Stick_X, Stick_Y; //Stores the horizontal and vertical axes (-1, 0, 1) seperately
    private bool jump;
    
    //Button variables
    private bool APressed, BPressed, CPressed; //Detects whether the button is currently being pressed or not
    private float ADuration, BDuration, CDuration; //Tracks how long the button has been pressed/released. When duration=0, has just changed

    //Positioning Variables
    private bool facingLeft = true; //Stores if the fighter is currently facing left
    private bool onRight = true; //Stores whether the fighter is currently further right than the opponent
    private int touchingWall = 0;
    public bool CanBackUp = true; //When false, the fighter is too close to the wall behind them to back up. Will be used to determine pushback.
    public bool gravityOn = true; //Determines if gravity is currently affecting the fighter

    // Reads player input frame by frame. References the fighter's moves and physics through seperate scripts.
    public void Init(GameMGR manager, bool playerOne, Fighter_Input enemy)
    {
        //Reconnect to manager
        MGR = manager;

        //Connect to opponent
        Opponent = enemy;
        
        if(playerOne)
        {
            //Set player position and port number
            transform.position = new Vector3(-4.4f, -4f, 0);
            Set_OnRight(false);
            PortNumber = 1;

            //Set keycodes
            UP = KeyCode.UpArrow;
            DOWN = KeyCode.DownArrow;
            LEFT = KeyCode.LeftArrow;
            RIGHT = KeyCode.RightArrow;
            A = KeyCode.Z;
            B = KeyCode.X;
            C = KeyCode.Space;
        }
        else
        {
            transform.position = new Vector3(4.4f, -4f, 0);
            Set_OnRight(true);
            PortNumber = 2;

            //Set keycodes
            UP = KeyCode.R;
            DOWN = KeyCode.F;
            LEFT = KeyCode.D;
            RIGHT = KeyCode.G;
            A = KeyCode.A;
            B = KeyCode.S;
            C = KeyCode.Q;
        }

        //Gain access to and initialize components
        SR = GetComponent<SpriteRenderer>();
        AR = GetComponent<Animator>();

        F_AnimCtrl = GetComponent<Fighter_AnimControl>(); F_AnimCtrl.Init(AR);
        F_Mov = GetComponent<Fighter_Mov>(); F_Mov.Initialize(this, AR);
        F_Atk = GetComponent<Fighter_Attack>(); F_Atk.Init(PortNumber, this, F_AnimCtrl);
        F_HitDet = GetComponentInChildren<Fighter_HitDetection>(); F_HitDet.Init(PortNumber, this);
        F_Stats = GetComponent<Fighter_Stats>(); F_Stats.Init(this);

        priorStickPos = 5; stickPos = 5;
    }

    void Update()
    {
        //Hitstop - If fighter is in hitstop, countdown hitstop duration
        if(hitStopTime > 0)
        {
            //Reduce hitstop duration by frames, but don't reduce past 0
            hitStopTime = Mathf.Clamp(hitStopTime - Time.deltaTime * 60f,0,100); 
            //Once hitstop equals zero, unfreeze movement and animations
            if (hitStopTime == 0)
            {
                F_Mov.FreezeMovement(false);
                F_AnimCtrl.SetAnimSpeed(1);
            }
        }
        
        //If not in hitstop, reduce duration of hitstun and blockstun
        if(hitStopTime == 0 && (F_State == FighterState.HITSTUN || F_State == FighterState.BLOCK))
        {
            //Reduce stuntime. If zero or less, change state to neutral and reset stunTime
            stunTime -= Time.deltaTime;
            if(stunTime <= 0)
            {
                stunTime = 0;
                Change_State(FighterState.NEUTRAL);
            }
        }

        //Get current stick directions and button inputs, then convert stick input to jump input and axis directions 
        UpdateStick(); //Sets stick position based on phone numpad code (1-9)
        UpdateButtons(); //Sets press state of A, B, and C buttons, as well as how long the button has(n't) been pressed.
        UpdateJoystickAxes(); //Sets the X and Y axes of each stick (-1, 0, 1)

        //Update the direction the character is facing
        UpdateFacing();

        //Update animator stick inputs based on axis
        AR.SetInteger("Horiz_Input", Relative_X());
        AR.SetInteger("Vert_Input", Stick_Y);

        //Standard Movement - Move the fighter if fighter is in neutral state and not in hitstop
        if (F_State == FighterState.NEUTRAL && hitStopTime == 0)
        {
            //If grounded and receiving a jump input, set jump animator trigger
            if (jump && Get_IsGrounded())
            {
                F_AnimCtrl.SetTrigger("Jump");
                F_Mov.Set_JumpX(Stick_X);
            }

            //Manually move the player, if they are able to be controlled
            F_Mov.Standard_Movement_Update(Stick_X, Stick_Y, jump);

            //Activate animator triggers for each button (Should this still only be called when not in hitstop?)
            F_Atk.CheckMoveList(APressed, ADuration, BPressed, BDuration, CPressed, CDuration);
        }
        //If in attack state and not in hitstop, allow animation keyframes to move the fighter
        else if(F_State == FighterState.ATTACK && hitStopTime == 0)
        {
            F_Mov.SetMovementByAnimator();
        }

        //Update vertical speed based on gravity UNLESS fighter is in hitstop or animating with keyframes (in which case gravity is off).
        if(hitStopTime == 0 && gravityOn)
            F_Mov.Gravity_Update();

        //Limit player's horizontal movement if the current fighter cannot back up
        if (!CanBackUp)
        {
            F_Mov.WallMovement(onRight);
            F_Mov.Touching_Wall(!onRight);
        }

        //Update animator movement variables that are based on speed
        AR.SetFloat("XSpeed", F_Mov.Get_SpeedX());
        AR.SetFloat("YSpeed", F_Mov.Get_SpeedY());
    }

    #region //[2] Trigger functions - Activate when touching or moving away from the wall
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If backed against the wall for the first time, tell the fighter they are touching the wall. Relative to direction faced.
        if ((collision.gameObject.CompareTag("LeftWall") && !facingLeft) || (collision.gameObject.CompareTag("RightWall") && facingLeft))
            touchingWall = (facingLeft ? 1 : -1);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //When moving away from a wall, reset touching wall trait
        if (collision.gameObject.CompareTag("LeftWall") || collision.gameObject.CompareTag("RightWall"))
            touchingWall = 0;
    }
    #endregion

    #region //[2] Manage character facing parameters, and updating their relative position
    //Update character facing
    /// <summary> Updates player facing and sprite/hurtbox flip based on their relative position and ground state. </summary>
    private void UpdateFacing()
    {
        //Set facing direction when direction changes if on the ground and in an actionable state
        if (facingLeft != onRight && F_Mov.grounded && F_State == FighterState.NEUTRAL)
            facingLeft = onRight;
        
        //Update sprite renderer and hurtbox based on results
        SR.flipX = facingLeft;
        Hurtbox_TF.localScale = new Vector3((facingLeft ? -1 : 1), 1, 1);
    }

    /// <summary> Updates the fighters On_Right variables, and whether the fighter can back up </summary>
    public void UpdateRelativePositionValues()
    {
        //Update relative position parameters based on opponent's position
        if (transform.position.x > Opponent.transform.position.x && !onRight)
            Set_OnRight(true);
        else if (transform.position.x < Opponent.transform.position.x && onRight)
            Set_OnRight(false);

        //Update whether the fighter can backup
        //Fighter can't back up if too far from opponent
        if (Mathf.Abs(transform.position.x - Opponent.transform.position.x) > 15 || touchingWall != 0)
            CanBackUp = false;
        else
            CanBackUp = true;
    }
    #endregion

    #region //[4] INPUT UPDATES - Methods that track or evaluate player input
    /// <summary> Tracks joystick position and duration, sets numerical position </summary>
    public void UpdateStick()
    {
        //Read player inputs, to set the numerical value of the input (1-9)
        pos_changed = false;
        int new_Pos = 5;
        if (Input.GetKey(LEFT)) new_Pos--; if (Input.GetKey(RIGHT)) new_Pos++;
        if (Input.GetKey(UP)) new_Pos += 3; if (Input.GetKey(DOWN)) new_Pos -= 3;

        //If the stick's position has changed, store the new stick position and reset stick position.
        if(new_Pos != stickPos)
        {
            priorStickPos = stickPos;
            stickPos = new_Pos;
            pos_duration = 0;
            pos_changed = true; //Tels the game the stick's position changed that frame.
        }
        else //If stick position not changed, count upt the dutation the stick has been held (max of 3)
        {
            pos_duration = Mathf.Clamp(pos_duration + Time.deltaTime,0,3);
        }
    }

    /// <summary> Check when face buttons are pressed / released, tracks input duration </summary>
    public void UpdateButtons()
    {
        //Increase time buttons have been in the current state, up to 3 seconds
        ADuration = Mathf.Clamp(ADuration + Time.deltaTime, 0, 3);
        BDuration = Mathf.Clamp(BDuration + Time.deltaTime, 0, 3);
        CDuration = Mathf.Clamp(CDuration + Time.deltaTime, 0, 3);

        //Check if each button has just been pressed or released - if yes, reset duration and change state
        if (Input.GetKeyDown(A)) { APressed = true; ADuration = 0; }
        else if(Input.GetKeyUp(A)) { APressed = false; ADuration = 0; }

        if (Input.GetKeyDown(B)) { BPressed = true; BDuration = 0; }
        else if (Input.GetKeyUp(B)) { BPressed = false; BDuration = 0; }

        if (Input.GetKeyDown(C)) { CPressed = true; CDuration = 0; }
        else if (Input.GetKeyUp(C)) { CPressed = false; CDuration = 0; }
    }
    
    /// <summary> Sets Joystick X and Y axes, and generates jump inputs </summary>
    private void UpdateJoystickAxes()
    {
        //For the X and Y input, set whether it is positive, negative, or neutral.
        Stick_X = 0; Stick_Y = 0;
        if (stickPos % 3 == 0) Stick_X = 1; else if (stickPos % 3 == 1) Stick_X = -1;
        if (stickPos <= 3) Stick_Y = -1; else if (stickPos >= 7) Stick_Y = 1;

        //If stick Y axis switched from down / neutral to up, generate jump input.
        jump = (pos_changed && Stick_Y > 0 && priorStickPos < 7);
    }
    
    /// <summary> Determines if fighter is holding forward or back relative to direction they're facing </summary>
    /// <returns> X Axis integer (-1, 0, 1) </returns>
    private int Relative_X()
    {
        //If fighter is holding back relative to direction they are facing, return -1. If holding forward, return 1.
        return facingLeft ? -Stick_X : Stick_X;
    }
    #endregion

    #region //[2] Manage state changes based on moves and taking damage
    /// <summary> Validates requests to change the fighter's state - activates some animation triggers  </summary>
    /// <param name="newState"></param>
    private void Change_State(FighterState newState)
    {
        //Check the current state, then choose action based on new requested state
        switch(F_State)
        {
            case FighterState.NEUTRAL:
                //Neutral >> Hitstun
                if (newState == FighterState.HITSTUN)
                {
                    F_State = FighterState.HITSTUN;
                }
                //Neutral >> Tumble
                else if (newState == FighterState.TUMBLE)
                {
                    F_State = FighterState.TUMBLE;
                }
                //Neutral >> Block; sets "Block" animation trigger
                else if (newState == FighterState.BLOCK)
                {
                    F_State = FighterState.BLOCK;
                    //Set block trigger
                    F_AnimCtrl.SetTrigger("Block");
                }
                //Neutral >> Attack; also stop horizontal movement if used on the ground
                else if (newState == FighterState.ATTACK)
                {
                    F_State = FighterState.ATTACK;
                    if (Get_IsGrounded())
                        F_Mov.StopAxisMovement(true, false);
                }
                break;

            case FighterState.ATTACK: //When switching away from attack state, disable personal hitbox if acitve
                //Attack >> Neutral
                if (newState == FighterState.NEUTRAL)
                {
                    F_State = FighterState.NEUTRAL;
                    F_Atk.DisableHitbox();
                }
                //Attack >> Hitstun
                else if (newState == FighterState.HITSTUN)
                {
                    F_State = FighterState.HITSTUN;
                    F_Atk.DisableHitbox();
                }
                //Attack >> Tumble
                else if (newState == FighterState.TUMBLE)
                {
                    F_State = FighterState.TUMBLE;
                    F_Atk.DisableHitbox();
                }
                break;

            case FighterState.HITSTUN:
                //Hitstun >> Neutral; sets "Hurt_Recover" animation trigger
                if (newState == FighterState.NEUTRAL)
                {
                    F_State = FighterState.NEUTRAL;
                    //Set recovery trigger
                    F_AnimCtrl.SetTrigger("Hurt_Recover");
                }
                //Hitstun >> Tumble
                else if (newState == FighterState.TUMBLE)
                {
                    F_State = FighterState.TUMBLE;
                }
                break;

            case FighterState.TUMBLE:
                //Tumble >> Grounded
                if (newState == FighterState.GROUNDED)
                {
                    F_State = FighterState.GROUNDED;
                }
                break;

            case FighterState.BLOCK:
                //Block >> Neutral; sets "Unblock" animation trigger
                if (newState == FighterState.NEUTRAL)
                {
                    F_State = FighterState.NEUTRAL;
                    //Set unblock trigger
                    F_AnimCtrl.SetTrigger("Unblock");
                }
                break;

            case FighterState.GROUNDED:
                //Grounded >> Neutral
                if(newState == FighterState.NEUTRAL)
                {
                    F_State = FighterState.NEUTRAL;
                }
                break;
        }
    }
    /// <summary> Allows child Fighter Attack Script to request state change, since Change_State is private </summary>
    /// <param name="setToAttack"></param>
    public void SetAttackState(bool setToAttack)
    {
        //Allow switching between neutral state and attack state
        if(setToAttack && F_State == FighterState.NEUTRAL)
            Change_State(FighterState.ATTACK);
        else if (!setToAttack && F_State == FighterState.ATTACK)
            Change_State(FighterState.NEUTRAL);
    }
    #endregion

    #region //[2] Calls that manage damage states and pushback
    /// <summary> Called by hurtbox to apply damage to the fighter. Also checks for blocking and initiates pushback </summary>
    /// <param name="HB_Data"></param> <param name="hitbox_facing_right"></param> <param name="Attacker"></param>
    public void Damaged(SO_Hitbox HB_Data, bool hitbox_facing_right, Fighter_Input Attacker)
    {
        //Determine whether the attack was blocked or not
        bool blocked_attack = false;
        if(Relative_X() < 0)
            blocked_attack = true;

        //If the attack was NOT blocked, deal damage and knockback;
        if(!blocked_attack)
        {
            //Deduct health
            F_Stats.Take_Damage(HB_Data.HB_damage);

            //Reset any movement effects applied by animator
            F_Mov.ANIMATOR_ResetAnimMovement();

            //If hit on the ground, change fighter state to hitstun, and set duration of the stun
            if(Get_IsGrounded())
            {
                if(HB_Data.HB_LaunchProperty == HitboxLaunch.AIR_ONLY)
                {
                    Change_State(FighterState.HITSTUN);
                    stunTime = HB_Data.hitStun / 60.0f;
                }
                else
                    Change_State(FighterState.TUMBLE);
            }
            //But if hit in the air, change to tumble state instead. Lasts until the fighter lands
            else if(!Get_IsGrounded())
                Change_State(FighterState.TUMBLE);

            //if the hitbox is a no_grav hitbox, turn off gravity
            if (HB_Data.HB_LaunchProperty == HitboxLaunch.NO_GRAV)
                Set_GravityOn(false);

            //Tell Fighter_Mov whether the character is in a ground or wall bounce state
            if (HB_Data.HB_BounceProperty == HitboxBounceType.GROUND)
            {
                F_Mov.Set_IsGBouncing(true);
                //F_Mov.ANIMATOR_SetJumpVariables(); //Deactivates ground states in movement script, in case the fighter is already grounded.
            }
            else if (HB_Data.HB_BounceProperty == HitboxBounceType.WALL)
                F_Mov.Set_IsWBouncing(true);

            //Trigger the correct damage animation based on groundedness and crouch status:
            /* If ground or wall bounced, start gravity-less launch effect
             * If hit on the air by a non-launching attack, start aerial knockback animation
             * If on the ground AND hit by a launching attack, use a special grounded-launch animation
             * If on the ground and crouching, use crouch-damage animation
             * In all other cases, use normal hitstun animation */

            if (HB_Data.HB_BounceProperty != HitboxBounceType.NONE)
                AR.SetTrigger("Hurt_Launch_NoGrav");
            else if (!Get_IsGrounded())
                AR.SetTrigger("Hurt_Air");
            else if (Get_IsGrounded() && HB_Data.HB_LaunchProperty != HitboxLaunch.AIR_ONLY) //Use special air launch animation when grounded and launched
                AR.SetTrigger("Hurt_Ground_Launch");
            else if (Stick_Y == -1)
                AR.SetTrigger("Hurt_Crouching");
            else
                AR.SetTrigger("Hurt_Grounded");

            //Call F_Mov script to launch the fighter
            F_Mov.Damage_Launch(HB_Data.HB_Knockback, hitbox_facing_right, HB_Data);
            
            //Bug check - ground bounce doesn't work when on the ground already. Maybe here begin special ground bounce?

            //If against the wall, push back the attacking fighter
            if(!CanBackUp && HB_Data.HB_BounceProperty != HitboxBounceType.WALL)
                Attacker.Block_Pushback(HB_Data, hitbox_facing_right);

            //Set the fighter's hitstop duration based on the attack received
            Set_HitStopTime(HB_Data.hitStop);
        }
        
        //If the attack WAS blocked, play block animation and push the fighter
        if(blocked_attack)
        {
            //Change fighter state, and set time to return to normal state
            Change_State(FighterState.BLOCK);
            stunTime = HB_Data.blockStun / 60.0f;
            Set_HitStopTime(HB_Data.hitStop);

            //Push self away, or push the attacker instead if against the wall
            if(CanBackUp)
                F_Mov.Damage_Launch(HB_Data.HB_Knockback, hitbox_facing_right, HB_Data);
            else
                Attacker.Block_Pushback(HB_Data, hitbox_facing_right);
        }
    }

    /// <summary> Pushes the fighter backwards when they are grounded and hitting an opponent against the wall. </summary>
    /// <param name="HB_Data"></param> <param name="hb_facing_right"></param>
    public void Block_Pushback(SO_Hitbox HB_Data, bool hb_facing_right)
    {
        //May check if grounded before defender calls this method
        if (Get_IsGrounded()) 
        {
            //Always pushes back along the x only - distance is based on magnitude of attack's normal knockback
            F_Mov.Damage_Launch(Vector3.right*HB_Data.HB_Knockback.magnitude, !hb_facing_right, HB_Data);
        }
            
    }
    #endregion

    #region//[2] Set Functions, used to set variables without exposing them
    /// <summary> Sets whether the fighter is further to the right than their opponent </summary>
    /// <param name="isOnRight"></param>
    public void Set_OnRight(bool isOnRight)
    {
        onRight = isOnRight;
    }
    /// <summary> Sets the duration of time the player is in hitstop for, and activates the hitstop.
    /// Called by the Damage script when taking damage, and by the hitbox when dealing damage </summary>
    /// <param name="stopTime"></param>
    public void Set_HitStopTime(float stopTime)
    {
        if(stopTime > 0)
        {
            hitStopTime = stopTime;
            F_Mov.FreezeMovement(true);
            F_AnimCtrl.SetAnimSpeed(0);
        }
    }

    /// <summary> Toggles whether the fighter has its gravity active or not. </summary>
    /// <param name="G"></param>
    public void Set_GravityOn(bool G)
    {
        gravityOn = G;
    }

    /// <summary> Sets whether the fighter's hurtboxes are currently active. Calls method on Fighter_HitDetection. </summary>
    /// <param name="isVulnerable"></param>
    public void Set_Vulnerability_F_HitDet(bool isVulnerable)
    {
        F_HitDet.SetVulnStatus(isVulnerable);
    }
    #endregion

    #region//[6] Get Functions, used to control access to variables
    public GameMGR Get_MGR()
    {
        return MGR;
    }
    public bool Get_OnRight()
    {
        return onRight;
    }
    public bool Get_FacingRight()
    {
        return !facingLeft;
    }
    public bool Get_IsGrounded()
    {
        return F_Mov.grounded;
    }
    public FighterState Get_FighterState()
    {
        return F_State;
    }
    public Fighter_Attack Get_FAtk()
    {
        return F_Atk;
    }
    #endregion
}
