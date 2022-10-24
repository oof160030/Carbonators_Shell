using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Mov : MonoBehaviour
{
    // ==Fighter_Mov: Controls and records fighter's movements by setting gravity and controlling jumps and movement==

    //The fighter_input script the fighter owns, along with the animator, rigidbody2d, and groundcheck script
    private Fighter_Input parent_FInput; 
    private Animator AR;
    private Rigidbody2D RB2;
    private GroundCheck GC; //Tells fighter when they start and stop touching the ground

    //Movement attributes - the character's move and jump speed, gravity (weight), and their grounded state.
    public float WalkSpeed, JumpSpeed;
    public float gravityAcc;
    public bool grounded;
    private Vector3 saveTraj; //The fighter's prior movement trajectory before being frozen (typically by freeze method)
    public int JumpX = 0; //Stores the x input provided when the jump was input (Set by Parent_FInput)
    
    //Bounce stats - sets whether the fighter is currently bouncing, and the number of bounces suffered in a combo
    private int BounceCount, BounceMax;
    private bool GBouncing, WBouncing;

    //Animation movement - If animator is moving fighter, the direction + speed of that animation, and whether to apply gravity
    public bool ANIM_ACTIVE;
    public Vector3 ANIM_XY;
    public float ANIM_MAG;
    public bool ANIM_UseGRAV;

    //Initialization - Sets up initial references to F_Input script, animator, rigidbody2d, and groundcheck script.
    public void Initialize(Fighter_Input Finput, Animator Anim)
    {
        parent_FInput = Finput;
        AR = Anim;
        RB2 = GetComponent<Rigidbody2D>();
        GC = GetComponentInChildren<GroundCheck>(); GC.Init(this);
        
        //Animator should use gravity by default
        ANIM_UseGRAV = true;

        //Not currently bouncing, and max number of bounces per combo is one currently
        BounceMax = 1; BounceCount = 0;
    }

    #region //[2] MOVEMENT VARIABLE UPDATES - Grounded state, Wall Contact state
    /// <summary> Sets the player's grounded state, and sends the appropriate signal to the animator. Checks for ground-bounce as well. Called by groundcheck (GC) script. </summary>
    /// <param name="isGrounded">Whether the fighter is currently touching the ground.</param>
    public void Set_Grounded(bool isGrounded)
    {
        //If fighter is about to land and should ground bounce, bounce off the ground instead of landing
        if(GBouncing && BounceCount < BounceMax && isGrounded)
        {
            BounceCount++; GBouncing = false;
            SetMovement(Vector3.Reflect(RB2.velocity, Vector3.up)); //Reverse vertical momentum
            ANIMATOR_SetJumpVariables(); //Set jump variables for ground check object
            AR.SetTrigger("Hurt_Ground_Launch"); //Set animator to launch upwards off ground
            parent_FInput.Set_GravityOn(true); //Turn the gravity back on
        }
        else //Otherwise, just set the grounded state as normal
        {
            grounded = isGrounded;
            AR.SetBool("Grounded", grounded);
            if (isGrounded)
            {
                //Reset bounces - and make fighter vulnerable in case landing from an invincible wall bounce
                BounceCount = 0;
                parent_FInput.Set_Vulnerability_F_HitDet(true);
            }
        }
    }

    /// <summary> Sets the player's wall-contact state, and checks for wall-bounces.</summary>
    /// <param name="leftWall">Whether the wall being touched is the left wall.</param>
    public void Touching_Wall(bool leftWall)
    {
        //On contact with wall, check if in wall bounce state
        if(WBouncing && parent_FInput.hitStopTime == 0)
        {
            //Iterate bounce count
            BounceCount++; WBouncing = false; 
            //If all bounces used, make fighter invulnerable
            if (BounceCount > BounceMax)
                parent_FInput.Set_Vulnerability_F_HitDet(false);

            Vector3 Dir = new Vector3(10, 25); //Set fighter launch direction (the same for all wall-bounces)
            AR.SetTrigger("Hurt_Air"); //Sets damage animation trigger

            //If hitting the left wall, apply bounce. If hitting the right wall, reverse trajectory first.
            SetMovement(leftWall ? Dir : Vector3.Reflect(Dir, Vector3.left));

            parent_FInput.Set_GravityOn(true); //Turn the gravity back on
        }
    }
    #endregion

    #region//[4] MOVEMENT UPDATE METHODS - Through Input and Gravity
    /// <summary> Updates the fighter's movement based on input. Applies when walking on the ground or falling. </summary>
    /// <param name="xIn">X input the fighter received.</param>
    /// <param name="yIn">Y input the fighter received.</param>
    /// <param name="jump">Whether the fighter has received a jump input.</param>
    public void Standard_Movement_Update(int xIn, int yIn, bool jump)
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

    /// <summary> Updates fighter vertical movement under normal circumstances (Acceleration due to gravity) </summary>
    public void Gravity_Update()
    {
        //Apply only if gravity is currently active
        if(ANIM_UseGRAV)
        {
            //Accelerate gravity based on time passed
            float Y = RB2.velocity.y - (gravityAcc * Time.deltaTime);

            //Update vertical trajectory, retain horizontal speed.
            Vector3 traj = new Vector3(RB2.velocity.x, Y);
            SetMovement(traj);
        }
    }
    
    /// <summary> Recieves a vertical and horizontal speed to move, and sets rigidbody to move in that direction </summary>
    /// <param name="trajectory">The direction and speed the fighter should move in.</param>
    private void SetMovement(Vector3 trajectory)
    {
        RB2.velocity = trajectory;
    }

    /// <summary> Sets player velocity by one axis without changing the others (WIP). </summary>
    private void SetMovementByAxis(bool setX, bool setY, float X, float Y)
    {
        Vector3 traj = new Vector3((setX ? X : RB2.velocity.x), (setY ? Y : RB2.velocity.y));
        SetMovement(traj);
    }
    #endregion

    #region //[6] ANIMATOR MOVEMENT METHODS
    /// <summary> Activates the player's jump, setting player speed and ground state. Called by the player's animator. </summary>
    public void Anim_ActivateJump()
    {
        //Create temporary directional variables
        float X, Y;

        //Horizontal movement for the jump set by x input when jump triggered.
        X = WalkSpeed * JumpX;
        //Vertical movement based on jump speed
        Y = JumpSpeed;

        //Set grounded states for ground check (prevents rejumps)
        ANIMATOR_SetJumpVariables();

        //Set trajectory of fighter and apply to rigidbody
        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    //Switches gravity on or off from the animator
    public void ANIMATOR_MovGravityOn() { ANIM_UseGRAV = true; }
    public void ANIMATOR_MovGravityOff() { ANIM_UseGRAV = false; }
    
    /// <summary> Reset character animator variables to create a blank slate for animator to work with later. </summary>
    public void ANIMATOR_ResetAnimMovement()
    {
        ANIM_ACTIVE = false;
        ANIM_XY = Vector3.zero;
        ANIM_MAG = 0;
        ANIM_UseGRAV = true;
    }
    
    /// <summary> For animated moves that act like "jumps" - changes grounded state and prevents rejumps. </summary>
    public void ANIMATOR_SetJumpVariables()
    {
        //Set grounded states for ground check (prevents rejumps)
        grounded = false; GC.groundOverlap = false;
    }
    
    /// <summary> Set character velocity based on animator variables instead of player input. </summary>
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

    /// <summary> Launches the fighter in the direction received. Vertical launch si ignored unless the move is a luancher. </summary>
    /// <param name="trajectory">Trajectory the hitbox is launching the fighter, assuming the attacker is facing right.</param>
    /// <param name="Hitbox_facingRight">When true, the attacker was facing right when the hitbox spawned.</param>
    /// <param name="HB">The scriptable object of the hitbox that hit this fighter.</param>
    public void Damage_Launch(Vector3 trajectory, bool Hitbox_facingRight, SO_Hitbox HB)
    {
        if (grounded && HB.HB_LaunchProperty == HitboxLaunch.AIR_ONLY) //If target (this) is grounded and hitbox is not a launcher
            trajectory.y = 0;
        if (Hitbox_facingRight)
            RB2.velocity = trajectory;
        else //Flip trajectory horizontally if hitbox faces left
            RB2.velocity = Vector3.Reflect(trajectory, Vector3.left);
    }

    #region//[3] MOVEMENT RESTRICTION - Freeze all or some movement, Wall limitation
    /// <summary> Freeze velocity along specific axis of movement </summary>
    public void StopAxisMovement(bool FreezeX, bool FreezeY)
    {
        RB2.velocity = new Vector3(FreezeX ? 0 : RB2.velocity.x, FreezeY ? 0 : RB2.velocity.y);
    }
    /// <summary> Override horizontal velocity if up against the wall. Called by FighterInput when CanBackup is false. </summary>
    public void WallMovement(bool onRight)
    {
        if (onRight ^ RB2.velocity.x < 0)
            RB2.velocity = new Vector3(0, RB2.velocity.y);
    }

    /// <summary> Freezes fighter movement while retaining momentum, or restores said momentum</summary>
    /// <param name="freeze">When true, fighter's movement is frozen. Original velocity restored when false.</param>
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
            //Debug.Log("Restored Velocity");
        }
    }
    #endregion

    #region //[3] Set Methods
    public void Set_JumpX(int X) { JumpX = X; }
    public void Set_IsGBouncing(bool B) { GBouncing = B; }
    public void Set_IsWBouncing(bool B) { WBouncing = B; }
    #endregion

    #region // [2] Get Methods
    public float Get_SpeedX() { return RB2.velocity.x; }
    public float Get_SpeedY() { return RB2.velocity.y; }
    #endregion
}
