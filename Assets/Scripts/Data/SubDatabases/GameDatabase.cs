using UnityEngine;

[CreateAssetMenu(menuName = "Databases/GameDatabase")]
public class GameDatabase : ScriptableObject
{
    public RoomDatabase rooms;

    public ToolDatabase tools;
    public WeaponDatabase weapons;
}
