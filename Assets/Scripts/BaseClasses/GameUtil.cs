using UnityEngine;

public static class GameUtil
{
    public static string GetRoomTypeName(this RoomType type)
    {
        return type switch
        {
            RoomType.Start => "",
            RoomType.Combat => "Standard",
            RoomType.Elite => "Elite",
            RoomType.Story => "Archive",
            RoomType.Vault => "Vault",
            RoomType.Shop => "Shop",
            RoomType.Rest => "Repair",
            RoomType.End => "Boss",
            _ => ""
        };
    }
}
