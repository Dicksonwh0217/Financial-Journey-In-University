using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInitializer : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(InitializeCharacter), 0.1f);
    }

    private void InitializeCharacter()
    {
        if (GameDataManager.Instance != null)
        {
            PlayerData data = GameDataManager.Instance.GetPlayerData();

            if (data != null && data.characterBody != null)
            {
                BodyPartsManager chosenoneBodyParts = GetComponent<BodyPartsManager>();

                if (chosenoneBodyParts != null)
                {
                    chosenoneBodyParts.SetCharacterBody(data.characterBody);
                }
            }
        }
    }
}
