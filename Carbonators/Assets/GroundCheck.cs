using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    // Checks if the fighter is touching the ground, is referenced by fighter_input. Descriptions updated 10/3
    public bool overlap; //If the hitbox is currently overlapping with the ground
    private float groundStay; //How long the hitbox has been touching the ground

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Resets the groundcheck if the hitbox is overlapping the ground and not currently set to touch the ground
        if (collision.CompareTag("Ground") && !overlap)
        {
            groundStay += Time.deltaTime;
            if (groundStay > 3)
            {
                overlap = true;
                groundStay = 0;
            }
        }
    }

    //Set overlap bool based on when the collider enters or exits the ground
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            overlap = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            overlap = false;
    }
}
