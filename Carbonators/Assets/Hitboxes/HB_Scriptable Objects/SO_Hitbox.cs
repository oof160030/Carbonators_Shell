using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HitBox", menuName = "ScriptableObjects/New Hitbox")]
public class SO_Hitbox : ScriptableObject
{
    //All of the information associated with a specific hitbox
    [Tooltip("1st ID number of this hitbox, referenced by animator. Refers to which move it is associated with.")]
    public int Move_IDNum;
    [Tooltip("2nd ID number of this hitbox, referenced by animator. For moves with multiple hitboxes, refers to which hitbox this one is.")]
    public int Box_IDNum;
    [Tooltip("Hitbox dimensions")]
    public float sizeX, sizeY;
    [Tooltip("Position the hitbox instantiates in, relative to the fighter. Assumes right facing.")]
    public Vector3 HB_position;
    [Tooltip("Damage the hitbox deals to its target")]
    public int HB_damage;
    [Tooltip("The number of frames the hitbox persists for if not interacted with.")]
    public int HB_lifespan;
    [Tooltip("If a move connects with multiple hitboxes, the one with the highest priority takes effect.")]
    public int HB_priority;
    [Tooltip("The direction the hitbox will send the opponent on contact. Assumes right facing.")]
    public Vector3 HB_Knockback;
    [Tooltip("The number of frames the attacker's and defender's animations pause for on hit.")]
    public int hitStop;
    [Tooltip("The duration, in frames, of the defender's hit animation.")]
    public int hitStun;
    [Tooltip("The duration, in frames, of the defender's block animation.")]
    public int blockStun;
    [Tooltip("Determines whether hitbox deals more barrier damage when blocked high or low, both, or neither.")]
    public HitboxHeight HB_HighLow;
    [Tooltip("Determines how the hitbox launches the player - only in the air, launches with gravity, or launches without gravity.")]
    public HitboxLaunch HB_LaunchProperty;
    [Tooltip("A list of all hitbox effects applied: if move is a projectile; if move partially or fully ignores barrier; if move hits only grounded or aerial targets.")]
    public HitboxProperties[] HB_Properties;
}
