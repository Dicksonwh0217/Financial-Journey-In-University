using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string nameEssentialScene;
    [SerializeField] string nameNewGameStartScene;

    [SerializeField] PlayerData playerData;

    public Gender selectedGender;
    public TMPro.TMP_InputField nameInputField;
    [SerializeField] private BodyPartsManager bodyPartsManager;

    AsyncOperation operation;

    private void Start()
    {
        SetGenderAny();
        UpdateName();
        

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SetPlayerData(playerData);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Quiting the game...");
        Application.Quit();
    }

    public void StartNewGame()
    {
        CaptureCharacterCustomization();

        SceneManager.LoadScene(nameNewGameStartScene, LoadSceneMode.Single);
        SceneManager.LoadScene(nameEssentialScene, LoadSceneMode.Additive);
    }

    private void CaptureCharacterCustomization()
    {
        if (bodyPartsManager != null)
        {
            SO_CharacterBody currentBody = bodyPartsManager.GetCurrentCharacterBody();

            if (currentBody != null)
            {
                playerData.characterBody = currentBody;

                if (GameDataManager.Instance != null)
                {
                    GameDataManager.Instance.SetPlayerData(playerData);
                }

                Debug.Log("Character Customization Captured!");
            }
        }
    }

    public void SetGenderAny()
    {
        selectedGender = Gender.Any;
        playerData.playerGender = selectedGender;
    }


    public void UpdateName()
    {
        playerData.characterName = nameInputField.text;
    }

    public void SetSavingSlot(int num)
    {
        playerData.saveSlotId = num;
    }
}
