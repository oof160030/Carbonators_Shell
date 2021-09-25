using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool overlap;
    private float groundStay;

    private void OnTriggerStay2D(Collider2D collision)
    {
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
