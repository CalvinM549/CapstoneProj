using UnityEngine;

[CreateAssetMenu(menuName = "Databases/GameDatabase")]
public class GameDatabase : ScriptableObject
{    
    public ToolDatabase tools;
    public WeaponDatabase weapons;

    public RoomDatabase rooms;
    public EnemyDatabase enemies;
    public ArchiveDatabase archives;
}
