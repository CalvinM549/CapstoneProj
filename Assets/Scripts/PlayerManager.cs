using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Can handle skins, persistent data between scenes


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class PlayerRunInfo
{
    public int currentHealth;
    public int currentMaxHealth;

    public IPlayerTool equippedTool;

    public List<UpgradeBase> upgrades;

}