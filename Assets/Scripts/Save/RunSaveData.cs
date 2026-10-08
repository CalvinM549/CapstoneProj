using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RunSaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public string ownerProfileId;
    public string saveTimestamp;

    // Map
    public int seed;
    public int chapter;
    public MapSaveData map;
    public string currentNodeId;
    public List<string> pathTaken = new();

    //Run
    public int currency;
    public float runDuration;
    public int enemiesKilled;
    public int damageTaken;

    public RewardSaveData rewards = new();

    // Player
    public PlayerSaveData player = new();
}

#region Map

[Serializable]
public class MapSaveData
{
    public string entryId;
    public List<SavedNode> nodes = new();

    public static MapSaveData ToSave(SectorMap map)
    {
        var data = new MapSaveData { entryId = map.entryNode.id };

        foreach (var node in map.nodes)
        {
            var saved = new SavedNode()
            {
                id = node.id,
                depth = node.depth,
                rowIndex = node.rowIndex,
                typeId = node.type.Id,
                roomId = node.room.Id,
                sectorCode = node.sectorCode,
            };

            foreach(var offer in node.offers)
                saved.offers.Add(new SavedOffer 
                { 
                    category = offer.category.ToString(), 
                    count = offer.count
                });

            foreach (var exit in node.exits)
                saved.exits.Add(exit.id);

            data.nodes.Add(saved);
        }

        return data;
    }

    public SectorMap ToMap(RoomDatabase roomDb, RoomTypeDatabase typesDb, int seed, int chapter)
    {
        if(nodes == null || nodes.Count == 0) return null;

        var map = new SectorMap()
        {
            seed = seed,
            chapter = chapter
        };

        // Generate Nodes
        foreach (var savedNode in nodes)
        {
            var type = typesDb.Get(savedNode.typeId);
            var room = roomDb.Get(savedNode.roomId);

            if (type == null || room == null)
            {
                Debug.LogError($"[MapSaveData] Node {savedNode.id} is missing type or room id");
                return null;
            }

            var offers = new ResolvedOffer[savedNode.offers.Count];
            for (int i = 0; i < offers.Length; i++)
            {
                if (!Enum.TryParse(savedNode.offers[i].category, out RewardCategory category))
                {
                    Debug.LogError($"[MapSave] Node {savedNode.id}: unknown reward category '{savedNode.offers[i].category}'");
                    return null;
                }

                offers[i] = new ResolvedOffer 
                { 
                    category = category,
                    count = savedNode.offers[i].count 
                };
            }

            var node = new MapNode()
            {
                id = savedNode.id,
                depth = savedNode.depth,
                rowIndex = savedNode.rowIndex,
                coordinates = new Vector2Int(savedNode.depth, savedNode.rowIndex),
                type = type,
                room = room,
                sectorCode = savedNode.sectorCode,
                offers = offers
            };

            if (!map.nodesByID.TryAdd(node.id, node))
            {
                Debug.LogError($"[MapSave] Duplicate node id {node.id}");
                return null;
            }
            map.nodes.Add(node);
        }

        //Connect Nodes
        foreach (var savedNode in nodes)
        {
            var node = map.nodesByID[savedNode.id];

            foreach (var exitId in savedNode.exits)
            {
                if (!map.nodesByID.TryGetValue(exitId, out var target))
                {
                    Debug.LogError($"[MapSave] Node {savedNode.id} exits to unknown node {exitId}");
                    return null;
                }
                node.exits.Add(target);
            }
        }

        // Check entry
        if (!map.nodesByID.TryGetValue(entryId ?? "", out map.entryNode))
        {
            Debug.LogError("[MapSave] Entry node missing");
            return null;
        }

        map.rowCount = map.nodes.Max(n => n.depth) + 1;
        map.totalNodes = map.nodes.Count;
        return map;
    }
}

[Serializable]
public class SavedOffer
{
    public string category;
    public int count;
}

[Serializable]
public class SavedNode
{
    public string id;
    public int depth;
    public int rowIndex;
    public string typeId;
    public string roomId;
    public string sectorCode;
    public List<SavedOffer> offers = new();
    public List<string> exits = new();
}

#endregion

#region Rewards


[Serializable]
public class RewardSaveData
{
    public List<string> ownedWeaponIds = new();
    public List<string> ownedToolIds = new();

    public List<string> seenItemIds = new();
}

#endregion

#region Player

[Serializable]
public class PlayerSaveData
{
    public int activeSegmentIndex = 0;
    public List<SegmentSave> segments = new();

    public float currentMomentum = 0f;

    public string equippedTool = "";
    public string equippedWeapon = "";

    public PlayerUpgradeSave upgradeSave;
}

[Serializable]
public class SegmentSave
{
    public float currentHealth;
    public float maxHealth;
    public bool isActive;
    public bool isDestroyed;
}

[Serializable]
public class PlayerUpgradeSave
{
    // Each slot stores the ID of the equipped Perk
    public string majorFrame = "";
    public string majorWeapons = "";
    public string majorPropulsion = "";

    public List<string> subUpgradeIDs;

    public List<string> auxUpgradeIDs;
}

#endregion