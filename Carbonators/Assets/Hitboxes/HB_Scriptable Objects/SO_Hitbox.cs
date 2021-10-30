using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HitBox", menuName = "ScriptableObjects/New Hitbox")]
public class SO_Hitbox : ScriptableObject
{
    //All of the information associated with a specific hitbox
    [Tooltip("ID number of this hitbox, referenced by animator. Character specific.")]
    public int IDNum;
    [Tooltip("Hitbox dimensions")]
    public float sizeX, sizeY;
    [Tooltip("Position the hitbox instantiates in, relative to the fighter. Assumes right facing.")]
    public Vector3 HB_position;
    [Tooltip("Damage the hitbox deals to its target")]
    public int HB_damage;
    [Tooltip("The time in seconds the hitbox persists for if not interacted with.")]
    public float HB_lifespan;
    [Tooltip("The direction the hitbox will send the opponent on contact. Assumes right facing.")]
    public Vector3 HB_Knockback;
    [Tooltip("The number of frames the attacker's and defender's animations pause for on hit.")]
    public int hitStop;
    [Tooltip("The duration, in frames, of the defender's hit animation.")]
    public int hitStun;
    [Tooltip("The duration, in frames, of the defender's block animation.")]
    public int blockStun;
}
