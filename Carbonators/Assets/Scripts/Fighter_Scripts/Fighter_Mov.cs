using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Mov : MonoBehaviour
{
    // Controls and records fighter's movements (sets gravity, controls jumps and movement)
    private Fighter_Input parent_FInput;
    private Animator AR;
    private Rigidbody2D RB2;
    private GroundCheck GC;
    public float WalkSpeed, JumpSpeed;
    public float gravityAcc;
    public bool grounded;
    private Vector3 saveTraj;
    public int JumpX = 0; //Stores the x input provided when the jump was input (Set by Parent_FInput)
    private int BounceCount, BounceMax;
    private bool GBouncing, WBouncing;

    public bool ANIM_ACTIVE;
    public Vector3 ANIM_XY; //The character movement direction requested by the animator
    public float ANIM_MAG; //The character movement speed requested by the animator
    public bool ANIM_UseGRAV; //Determines whether animated move should use gravity

    public void Initialize(Fighter_Input Finput, Animator Anim)
    {
        parent_FInput = Finput;
        AR = Anim;
        RB2 = GetComponent<Rigidbody2D>();
        GC = GetComponentInChildren<GroundCheck>(); GC.Init(this);
        ANIM_UseGRAV = true;

        BounceMax = 1;
        BounceCount = 0;
    }

    //Sets the player's grounded state, and sends the appropriate signal to the animator. Called by groundcheck (GC) script
    public void Set_Grounded(bool isGrounded)
    {
        //If fighter is in grounde bounce-knockback state, and has not bounced yet, bounce off the ground
        if(GBouncing && BounceCount < BounceMax)
        {
            BounceCount++; GBouncing = false;
            SetMovement(Vector3.Reflect(RB2.velocity, Vector3.up)); //Reverse vertical momentum
            ANIMATOR_SetJumpVariables(); //Set jump variables for ground check object
            AR.SetTrigger("Hurt_Ground_Launch"); //Set animator to launch upwards off ground
            parent_FInput.Set_GravityOn(true); //Turn the gravity back on
        }
        else
        {
            //Hit the ground if the player cannot ground bounce
            grounded = isGrounded;
            AR.SetBool("Grounded", grounded);
            BounceCount = 0;
        }
    }

    //Fighter horizontal movement under normal circumstances (Walking or falling)
    public void Standard_Movement(int xIn, int yIn, bool jump)
    {
        //Create temporary directional variables
        float X, Y;

        //Set horizontal movement based on input
        if (grounded && yIn > -1) X = WalkSpeed * xIn; //If standing on the ground, move based on imput
        else if (grounded) X = 0; //If crouching, don't move horizontally
        else X = RB2.velocity.x; //If in the air, retain horizontal momentum

        Y = RB2.velocity.y; //Retain current vertical speed (to be changed by gravity function)

        //Set movement trajectory based on horiz and vert speed, and apply to rigidbody
        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    //Fighter vertical movement under normal circumstances (Acceleration due to gravity)
    public void Gravity_Update()
    {
        if(ANIM_UseGRAV)
        {
            //Accelerate gravity based on time passed
            float Y = RB2.velocity.y - (gravityAcc * Time.deltaTime);

            //Update vertical trajectory, retain horizontal speed.
            Vector3 traj = new Vector3(RB2.velocity.x, Y);
            SetMovement(traj);
        }
    }

    //Recieves a vertical and horizontal speed to move, and sets rigidbody to move in that direction
    private void SetMovement(Vector3 trajectory)
    {
        RB2.velocity = trajectory;
    }

    // Set player velocity by one axis without changing the others (WIP)
    private void SetMovementByAxis(bool setX, bool setY, float X, float Y)
    {
        Vector3 traj = new Vector3((setX ? X : RB2.velocity.x), (setY ? Y : RB2.velocity.y));
        SetMovement(traj);
    }

    #region //[4] ANIMATOR MOVEMENT METHODS
    //Activates the player's jump, setting player speed and ground state. Called by the player's animator.
    public void Anim_ActivateJump()
    {
        //Create temporary directional variables
        float X, Y;

        //Horizontal movement for the jump set by x input when jump triggered.
        X = WalkSpeed * JumpX;
        //Vertical movement based on jump speed
        Y = JumpSpeed;

        //Set grounded states for ground check (prevents rejumps)
        grounded = false; GC.overlap = false;

        //Set trajectory of fighter and apply to rigidbody
        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    public void ANIMATOR_MovGravityOn()
    {
        ANIM_UseGRAV = true;
    }

    public void ANIMATOR_MovGravityOff()
    {
        ANIM_UseGRAV = false;
    }

    //Reset character animator variable
    public void ANIMATOR_ResetAnimMovement()
    {
        ANIM_ACTIVE = false;
        ANIM_XY = Vector3.zero;
        ANIM_MAG = 0;
        ANIM_UseGRAV = true;
    }

    //For animated moves that act like "jumps" - changes grounded state and prevents rejumps.
    public void ANIMATOR_SetJumpVariables()
    {
        //Set grounded states for ground check (prevents rejumps)
        grounded = false; GC.overlap = false;
    }

    //Set character velocity based on animator variable
    public void SetMovementByAnimator()
    {
        //Only change velocity if animator is in use
        if(ANIM_ACTIVE)
        {
            if (parent_FInput.Get_FacingRight())
                RB2.velocity = ANIM_XY.normalized * ANIM_MAG;
            else
                RB2.velocity = Vector3.Reflect(ANIM_XY.normalized, Vector3.left) * ANIM_MAG;
        }
    }
    #endregion

    //Freeze velocity along specific axis of movement
    public void StopAxisMovement(bool FreezeX, bool FreezeY)
    {
        RB2.velocity = new Vector3(FreezeX ? 0 : RB2.velocity.x, FreezeY ? 0 : RB2.velocity.y);
    }

    /// <summary> Launches the fighter in the direction received. Vertical launch si ignored unless the move is a luancher. </summary>
    /// <param name="trajectory"></param> <param name="Hitbox_facingRight"></param>
    public void Damage_Launch(Vector3 trajectory, bool Hitbox_facingRight, SO_Hitbox HB)
    {
        if (grounded && HB.HB_LaunchProperty == HitboxLaunch.AIR_ONLY) //If target (this) is grounded and hitbox is not a launcher
            trajectory.y = 0;
        if (Hitbox_facingRight)
            RB2.velocity = trajectory;
        else //Flip trajectory horizontally if hitbox faces left
            RB2.velocity = Vector3.Reflect(trajectory, Vector3.left);
    }

    //Override horizontal velocity if up against the wall. Called by FighterInput when CanBackup is false.
    public void WallMovement(bool onRight)
    {
        if (onRight ^ RB2.velocity.x < 0)
            RB2.velocity = new Vector3(0, RB2.velocity.y);
    }

    //Freezes fighter movement while retaining momentum, or restores said momentum
    public void FreezeMovement(bool freeze)
    {
        if(freeze)
        {
            saveTraj = RB2.velocity;
            RB2.velocity = Vector3.zero;
        }
        else
        {
            RB2.velocity = saveTraj;
        }
    }

    public void Set_JumpX(int X) { JumpX = X; }
    public void Set_IsGBouncing(bool B) { GBouncing = B; }
    public void Set_IsWBouncing(bool B) { WBouncing = B; }

    public float Get_SpeedX() { return RB2.velocity.x; }
    public float Get_SpeedY() { return RB2.velocity.y; }
}
