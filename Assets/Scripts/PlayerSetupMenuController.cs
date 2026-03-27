using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 新增：建立一個類別來設定「角色預製體」與「Ready畫面角色圖」的對應關係
[System.Serializable]
public class CharacterImageMapping
{
    public GameObject characterPrefab;
    public Sprite readyImage; // 該角色在 Ready 畫面時要顯示的圖片
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

    // 新增：Ready 面板上用來顯示角色圖片的 UI Image 元件
    [SerializeField]
    private Image readyCharacterDisplay;

    // 新增：在 Inspector 中設定角色與圖片的對應清單
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

    public void SetCharacter(GameObject characterPrefab)
    {
        if (!inputEnable) { return; }

        // 記錄玩家選擇的角色
        PlayerConfigurationManager.Instance.SetPlayerCharacterPrefab(playerIndex, characterPrefab);

        // 新增：尋找對應的圖片並顯示在 Ready 面板上
        if (readyCharacterDisplay != null)
        {
            foreach (var mapping in characterImages)
            {
                if (mapping.characterPrefab == characterPrefab)
                {
                    readyCharacterDisplay.sprite = mapping.readyImage;
                    break; // 找到對應圖片就跳出迴圈
                }
            }
        }

        readyPanel.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
    }

    public void ReadyPlayer()
    {
        if (!inputEnable) { return; }
        PlayerConfigurationManager.Instance.ReadyPlayer(playerIndex);
        readyButton.gameObject.SetActive(false);
    }
}