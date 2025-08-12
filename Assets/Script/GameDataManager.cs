using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance
    {
        get; private set;
    }

    private PlayerData playerData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance= this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerData(PlayerData data)
    {
        playerData = data;
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }
}
