using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterImageMapping
{
    public GameObject characterPrefab;
    public Sprite readyImage;
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

    // Ready 畫面上的角色圖片 UI
    [SerializeField]
    private Image readyCharacterDisplay;

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

        if (readyCharacterDisplay != null)
        {
            // 找到對應圖片並換上去，但先將 GameObject 關閉，不讓玩家看到
            foreach (var mapping in characterImages)
            {
                if (mapping.characterPrefab == characterPrefab)
                {
                    readyCharacterDisplay.sprite = mapping.readyImage;
                    break;
                }
            }
            // 關鍵：選角色時先隱藏圖片
            readyCharacterDisplay.gameObject.SetActive(false);
        }

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

        // 關鍵：按下按鈕後才顯示角色圖片
        if (readyCharacterDisplay != null)
        {
            readyCharacterDisplay.gameObject.SetActive(true);
        }

        PlayerConfigurationManager.Instance.ReadyPlayer(playerIndex);

        // 隱藏 Ready 按鈕
        readyButton.gameObject.SetActive(false);
    }
}