using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 1. 修改資料結構，支援大圖與小圖
[System.Serializable]
public class CharacterImageMapping
{
    public GameObject characterPrefab;
    public Sprite largeReadyImage; // 放大圖
    public Sprite smallReadyImage; // 小圖
}

public class PlayerSetupMenuController : MonoBehaviour
{
    private int playerIndex;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private GameObject readyPanel;
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private Button readyButton;

    // 2. 宣告兩個 UI Image 元件來分別顯示大圖與小圖
    [Header("Character Displays")]
    [SerializeField]
    private Image largeCharacterDisplay; // 負責顯示放大圖的 UI Image
    [SerializeField]
    private Image smallCharacterDisplay; // 負責顯示小圖的 UI Image

    [SerializeField]
    private List<CharacterImageMapping> characterImages;

    private float ignoreInputTime = 1.5f;
    private bool inputEnable;

    public void SetPlayerIndex(int pi)
    {
        playerIndex = pi;
        titleText.SetText("Player" + (pi + 1).ToString());
        ignoreInputTime = Time.time + ignoreInputTime;
    }

    void Update()
    {
        if (Time.time > ignoreInputTime)
        {
            inputEnable = true;
        }
    }

    // 步驟 1：玩家選擇了角色
    public void SetCharacter(GameObject characterPrefab)
    {
        if (!inputEnable) { return; }

        PlayerConfigurationManager.Instance.SetPlayerCharacterPrefab(playerIndex, characterPrefab);

        // 3. 找到對應圖片並分別設定給兩個 Image 元件
        foreach (var mapping in characterImages)
        {
            if (mapping.characterPrefab == characterPrefab)
            {
                if (largeCharacterDisplay != null)
                    largeCharacterDisplay.sprite = mapping.largeReadyImage;

                if (smallCharacterDisplay != null)
                    smallCharacterDisplay.sprite = mapping.smallReadyImage;

                break;
            }
        }

        // 選角色時先隱藏這兩張圖片
        if (largeCharacterDisplay != null)
            largeCharacterDisplay.gameObject.SetActive(false);

        if (smallCharacterDisplay != null)
            smallCharacterDisplay.gameObject.SetActive(false);

        // 顯示 Ready 畫面並讓玩家可以點擊 Ready 按鈕
        readyPanel.SetActive(true);
        readyButton.gameObject.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
    }

    // 步驟 2：玩家按下 Ready 按鈕
    public void ReadyPlayer()
    {
        if (!inputEnable) { return; }

        // 4. 按下按鈕後，將大圖與小圖同時顯示出來
        if (largeCharacterDisplay != null)
            largeCharacterDisplay.gameObject.SetActive(true);

        if (smallCharacterDisplay != null)
            smallCharacterDisplay.gameObject.SetActive(true);

        PlayerConfigurationManager.Instance.ReadyPlayer(playerIndex);

        // 隱藏 Ready 按鈕
        readyButton.gameObject.SetActive(false);
    }
}