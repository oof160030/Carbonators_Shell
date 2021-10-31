using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_AnimControl : MonoBehaviour
{
    //Resets animator triggers if left for too long
    public int Block_AnimTimer, Unblock_AnimTimer, Light_AnimTimer;
    private Animator AR;

    public void Init(Animator fighterAR)
    {
        AR = fighterAR;
    }

    // Update is called once per frame
    void Update()
    {
        if(Light_AnimTimer > 0)
        {
            Light_AnimTimer--;
            if (Light_AnimTimer == 0)
                AR.ResetTrigger("A_Pressed");
        }
        if (Block_AnimTimer > 0)
        {
            Block_AnimTimer--;
            if (Block_AnimTimer == 0)
                AR.ResetTrigger("Block");
        }
        if (Unblock_AnimTimer > 0)
        {
            Unblock_AnimTimer--;
            if (Unblock_AnimTimer == 0)
                AR.ResetTrigger("Unblock");
        }
    }
}
