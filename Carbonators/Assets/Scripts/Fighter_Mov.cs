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
    
    void Start()
    {
        RB2 = GetComponent<Rigidbody2D>();
        GC = GetComponentInChildren<GroundCheck>();
    }

    public void Initialize(Fighter_Input Finput, Animator Anim)
    {
        parent_FInput = Finput;
        AR = Anim;
    }

    private void Update()
    {
        grounded = GC.overlap;
        AR.SetBool("Grounded", grounded);
    }

    public void Anim_ActivateJump()
    {
        float X, Y;

        //(Horizontal movement for the jump already set by normal movement)
        X = RB2.velocity.x;
        //Vertical movement based on jump speed
        Y = JumpSpeed;

        //Set grounded states for ground check (prevents rejumps)
        grounded = false; GC.overlap = false;
        
        //Set trajectory of fighter and apply to rigidbody
        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    //Allows Fighter to move itself under normal circumstances
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

    public void Gravity_Update()
    {
        //Accelerate gravity based on time passed
        float Y = RB2.velocity.y - (gravityAcc * Time.deltaTime);

        //Update vertical trajectory, retain horizontal speed.
        Vector3 traj = new Vector3(RB2.velocity.x, Y);
        SetMovement(traj);
    }

    private void SetMovement(Vector3 trajectory)
    {
        //Recieves a vertical and horizontal speed to move, in the form of a vector 2 or 3
        RB2.velocity = trajectory;
    }

    /// <summary>
    /// Launches the fighter in the direction received.
    /// </summary>
    /// <param name="trajectory"></param>
    /// <param name="right"></param>
    public void Damage_Launch(Vector3 trajectory, bool right)
    {
        if (grounded) //And not a launcher
            trajectory.y = 0;
        if (right)
            RB2.velocity = trajectory;
        else
            RB2.velocity = Vector3.Reflect(trajectory, Vector3.left);
    }

    //Override velocity if up against the wall
    public void WallMovement(bool onRight)
    {
        if (onRight ^ RB2.velocity.x < 0)
            RB2.velocity = new Vector3(0, RB2.velocity.y);
    }

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
}
