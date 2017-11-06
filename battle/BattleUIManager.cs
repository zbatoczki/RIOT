using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class BattleUIManager : MonoBehaviour {

    private BattleSystem battleSystem;
    private PlayerStateMachine player;
    private EnemyStateMachine enemy;

    //player stats variable;
    [SerializeField] private Text playerHP;
    [SerializeField] private Text playerMP;
    //[SerializeField] private Text playerBanQuantity;
    [SerializeField] private Image limitBreak;
    Transform playerLimitBar;

    //player menu variables
    [SerializeField] private GameObject playerActionMenu;
    [SerializeField] private GameObject playerMagicMenu;
    [SerializeField] private GameObject playerItemsMenu;
    [SerializeField] private GameObject playerLimitButton;

    //enemy menu variables
    [SerializeField] private GameObject enemyActionMenu;
    [SerializeField] private GameObject enemyLimitBar;
    [SerializeField] private Text enemyHP;
    [SerializeField] private Text enemyMP;

    [SerializeField] private Button playerAutoPlayBtn;
    [SerializeField] private Button enemyAutoPlayBtn;

    public enum UIWindows
    {
        PLAYERMENU,
        PLAYERITEMS,
        ENEMYMENU
    }

    private Dictionary<string, string> buttonDescriptions = new Dictionary<string, string>();
    
    [SerializeField] private GameObject descriptionBox;

    // Use this for initialization
    void Awake () {
        battleSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<BattleSystem>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>();
        enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyStateMachine>();
        playerLimitBar = GameObject.FindGameObjectWithTag("PlayerLimitBar").transform;
        UpdatePlayerLimitBar(-(playerLimitBar.localScale.x));
        UpdateEnemyLimitBar(0f);
        SetButtonDescriptions();
        HideButtonDesc();
        AllMenusOff();
    }

    public void UpdateUI()
    {
        //player update
        int currentHP = player.GetStat(Player.STATS.CURRENTHP);
        int maxHP = player.GetStat(Player.STATS.MAXHP);
        int currentMP = player.GetStat(Player.STATS.CURRENTMP);
        int maxMP = player.GetStat(Player.STATS.MAXMP);
        //int playerBanCount = player.GetStat(Player.STATS.BANQUANTITY);

        playerHP.text = "HP: " + currentHP + "/" + maxHP;
        playerMP.text = "MP: " + currentMP + "/" + maxMP;
        //playerBanQuantity.text = "Ban x" + playerBanCount;

        //enemy update
        currentHP = enemy.GetStat(Enemy.STATS.CURRENTHP);
        maxHP = enemy.GetStat(Enemy.STATS.MAXHP);
        currentMP = enemy.GetStat(Enemy.STATS.CURRENTMP);
        maxMP = enemy.GetStat(Enemy.STATS.MAXMP);

        enemyHP.text = "HP: " + currentHP + "/" + maxHP;
        enemyMP.text = "MP: " + currentMP + "/" + maxMP;

    }

    public void SetGameOverMenu(string winner)
    {
        GameObject ui = GameObject.FindGameObjectWithTag("UI");
        GameObject menu = Resources.Load("Prefabs/GameOverPanel") as GameObject;
        GameObject instance = Instantiate(menu);

        instance.transform.SetParent(ui.transform, false);
        instance.transform.position = ui.transform.position;

        instance.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "The game is over. " + winner + "\nPlay again?";

        Button[] buttons = instance.transform.GetChild(0).GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() => { battleSystem.RestartGame(); });
        buttons[1].onClick.AddListener(() => { battleSystem.ExitGame(); });
    }

    private void AllMenusOff()
    {
        PlayerMenusOff();
        //EnemyMenusOff();
    }

    //turn off all player menu options
    public void PlayerMenusOff()
    {
        playerActionMenu.SetActive(false);
        playerMagicMenu.SetActive(false);
        playerItemsMenu.SetActive(false);
    }
    
    //turn on main menu for player (Attack, Defend,...)
    public void PlayerActionMenuOn()
    {
        playerActionMenu.SetActive(true);
    }


    //open the items sub menu; disable main menu (Attack, Defend,...)
    private void OpenItemsMenu()
    {
        playerItemsMenu.SetActive(true);
    }

    //turn off restore item
    public void RestoreOff()
    {
        GameObject restore = playerItemsMenu.transform.GetChild(3).gameObject;
        restore.GetComponent<Button>().interactable = false;
    }

    //turn magic button as interactable (true) or not (false)
    public void toggleMagic(bool set)
    {
        Button magic = playerActionMenu.transform.GetChild(3).gameObject.GetComponent<Button>();
        if(set)
            magic.interactable = true;
        else
            magic.interactable = false;
    }


    /// <summary>
    /// Sets interactable for item buttons on or off
    /// </summary>
    /// <param name="button">Buttton to set</param>
    /// <param name="set">set on if true, off if false</param>
    public void toggleItems(string button, bool set)
    {
        int index = button.Equals("potion") ? 0 : 1;
        Button itemBtn = playerItemsMenu.transform.GetChild(index).gameObject.GetComponent<Button>();
        if (set)
            itemBtn.interactable = true;
        else
            itemBtn.interactable = false;
    }

    public float checkPlayerLimit()
    {
        return playerLimitBar.localScale.x;
    }

    public void UpdatePlayerLimitBar(float amount)
    {   
        if(playerLimitBar.localScale.x + amount >= 1.0f)
            playerLimitBar.localScale = new Vector3(1f, playerLimitBar.localScale.y);
        else if(amount == -1f)
            playerLimitBar.localScale = new Vector3(0, playerLimitBar.localScale.y);
        else
        {
            playerLimitBar.localScale += new Vector3(amount, 0);
        }
           
    }

    public bool LimitReady()
    {
        return playerLimitBar.localScale.x >= 1.0;
    }

    public void enableLimitButton()
    {
        playerLimitButton.SetActive(true);
    }

    public void disableLimitButton()
    {
        playerLimitButton.SetActive(false);
    }

    /* //////////////////////ENEMY////////////////////*/

    //turn on main menu for enemy (Attack, Defend,...)
    public void EnemyActionMenuOn()
    {
        enemyActionMenu.SetActive(true);
    }

    //turn off all player menu options
    public void EnemyMenusOff()
    {
        enemyActionMenu.SetActive(false);
    }

    public void UpdateEnemyLimitBar(float percentage)
    {
        enemyLimitBar.transform.localScale = new Vector3(percentage, 1, 1);
    }

    

    //open items menu
    public void ItemsBtn()
    {
        OpenItemsMenu();
    }

    public void EnableBan()
    {
        playerItemsMenu.transform.GetChild(2).GetComponent<Button>().interactable = true;
    }

    public void DisableBan()
    {
        playerItemsMenu.transform.GetChild(2).GetComponent<Button>().interactable = false;
    }


    public void ShowButtonDesc(string button)
    {
        descriptionBox.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = buttonDescriptions[button];
        descriptionBox.SetActive(true);
    }

    public void HideButtonDesc()
    {
        descriptionBox.SetActive(false);
    }

    public void SetAutoplayBtn(bool player, bool autoplay)
    {
        if (player) //if button clicked was the player button
        {
            Text playerbtnText = playerAutoPlayBtn.GetComponentInChildren<Text>();
            if (autoplay)
                playerbtnText.text = "Player Auto Play: ON";
            else
                playerbtnText.text = "Player Auto Play: OFF";

        }
        else // enemy button was pressed
        {
            Text enemybtnText = enemyAutoPlayBtn.GetComponentInChildren<Text>();
            if (autoplay)
                enemybtnText.text = "Enemy Auto Play: ON";
            else
                enemybtnText.text = "Enemy Auto Play: OFF";
        }
            
    }

    private void SetButtonDescriptions()
    {
        buttonDescriptions.Add("Attack", "A physical attack. Costs no resources.");
        buttonDescriptions.Add("Defend", "Reduces damage taken and restores some health.");
        buttonDescriptions.Add("Magic", "A strong magical attack. Costs 200 MP.");
        buttonDescriptions.Add("Items", "Open a menu of items to consume");
        buttonDescriptions.Add("Potion", "Restores 25% of missing HP.");
        buttonDescriptions.Add("Magic Potion", "Restores 25% of missing MP");
        //buttonDescriptions.Add("Ban", "Skips enemy's next turn.");
        buttonDescriptions.Add("Restore", "Fully replenishes HP and MP. You only have one, so use it wisely!");
        buttonDescriptions.Add("Democracy", "Does between 0% to 100% of enemy's max HP.");
    }

}
