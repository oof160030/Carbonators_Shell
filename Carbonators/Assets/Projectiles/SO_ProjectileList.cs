using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Projectile_List", menuName = "ScriptableObjects/New Projectile List")]
public class SO_ProjectileList : ScriptableObject
{
    // Stores a character's various projectile attacks
    public int CharacterID; //Character the projectiles are associated with
    public SO_Projectile[] projectile; //Character's projectiles
}
