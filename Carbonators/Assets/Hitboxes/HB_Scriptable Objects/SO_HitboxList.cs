using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hitbox_List", menuName = "ScriptableObjects/New Hitbox List")]
public class SO_HitboxList : ScriptableObject
{
    public int CharacterID; //Character the hitboxes are associated with
    public SO_Hitbox[] hitbox; //The character's hitboxes
}
