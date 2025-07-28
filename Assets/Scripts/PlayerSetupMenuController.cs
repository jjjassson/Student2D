using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void SetColor(Material color)
    {
        if (!inputEnable) { return; }
        PlayerConfigurationManager.Instance.SetPlayerColor(playerIndex, color);
        readyPanel.SetActive(true);
        readyButton.Select();
        menuPanel.SetActive(false);
    }

    public void SetCharacter(GameObject characterPrefab)
    {
        if (!inputEnable) { return; }

        PlayerConfigurationManager.Instance.SetPlayerCharacterPrefab(playerIndex, characterPrefab);

        // 接下來流程一樣
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
