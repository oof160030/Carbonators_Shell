using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/New Projectile")]
public class SO_Projectile : ScriptableObject
{
    [Tooltip("ID number of this projectile, referenced by animator. Character specific.")]
    public int P_IDNum;
    [Tooltip("Projectile Gameobject to spawn. Has projectile script attached to it.")]
    public GameObject GO_Projectile;
    [Tooltip("Hitbox or hitboxes associated with projectile.")]
    public SO_Hitbox[] P_Hitboxes;
    [Tooltip("Position to spawn projectile. Relative to character position when facing right.")]
    public Vector2 P_SpawnPosition;

    [Tooltip("Direction for the spawned projectile to move, when facing right. Normalized.")]
    public Vector2 P_MoveDirection;
    [Tooltip("Speed of the spawned projectile.")]
    public float P_MoveSpeed;
    [Tooltip("If true, projectile can accelerate once created.")]
    public bool P_CanAccelerate;
    [Tooltip("Direction of projectile's acceleration, when facing right. Normalized.")]
    public Vector2 P_AccDirection;
    [Tooltip("Rate of change of projectile's speed per second.")]
    public float P_AccSpeed;
    [Tooltip("Duration the projectile lasts, in frames.")]
    public int P_Lifetime;
}
