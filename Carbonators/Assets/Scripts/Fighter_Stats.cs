using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Stats : MonoBehaviour
{
    //Tracks the various stats of the fighter, including health, shield, and super meter.
    //Displays this data to the UI
    [SerializeField]
    private float Health;
    private float Shield, Meter;
    private Fighter_Input F_Input;

    public void Init(Fighter_Input In)
    {
        F_Input = In;
        Health = 100;
    }

    //Change health stats
    public void Take_Damage(float damage)
    {
        Health -= damage;

        //Update UI in this step
        F_Input.Get_MGR().updateHealthUI(F_Input.PortNumber, Health);
    }
}
