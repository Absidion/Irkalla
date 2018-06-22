using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Author: Josue
//Last edited: James 03/08/2018

public class PlayerHUDManager : MonoBehaviour
{

    #region Public Members
    public static PlayerHUDManager instance = null;    //singleton instance of the HUDManager

    public Canvas DeathCanvas;                          //The canvas that appears when the player dies
    public Canvas GameOverCanvas;                       //The canvas that appears when both players die
    public Canvas PlayerUICanvas;                       //The ui that is tied to the player's hud
    #endregion

    #region Private Memebers
    [SerializeField]
    private Text SpectatingText;                //the text component of the death canvas that contains the value of who you're spectating
    [SerializeField]
    private Image PickedUpItemImage;            //image which displays the item sprite and tooltip when you have picked up an item
    [SerializeField]
    private Image PickedUpItemSprite;           //sprite of the picked up item
    [SerializeField]
    private Text PickedUpItemText;              //tooltip of the picked up item
    [SerializeField]
    private Image ShopItemImage;                //image which displays the item sprite and cost of item when in shop
    [SerializeField]
    private Text ShopItemName;                  //text which displays the name of the shop item
    [SerializeField]
    private Text ShopItemPrice;                 //text which displays the cost of the shop item
    [SerializeField]
    private Image InteractToggle;               //when in range of something that can be interacted with, displays the interact button
    [SerializeField]
    private ClockTimerSprite ItemClockSprite;   //contains the current active item image and cooldown radial
    [SerializeField]
    private ClockTimerSprite Ability1Sprite;    //contains ability 1 image and cooldown radial
    [SerializeField]
    private ClockTimerSprite Ability2Sprite;    //contains ability 2 image and cooldown radial
    [SerializeField]
    private Text AmmoCount;                     //text of player's ammo count if they use it
    [SerializeField]
    private Image PlayerAmmoClock;
    [SerializeField]
    private Image CurrencyBox;                  //parent image which contains currency value inside
    [SerializeField]
    private Text CurrencyField;                 //currency field which appears when you gain currency or when you are in the shop
    [SerializeField]
    private Image LocalPlayerHealthGlobe;       //local player's health which is displayed in a filled globe
    [SerializeField]
    private Image LocalPlayerArmorGlobe;        //local player's armor which is displayed to the right of the health globe
    [SerializeField]
    private Text LocalPlayerHealthNumber;       //local player's health displayed in text alongside the image
    [SerializeField]
    private Text LocalPlayerArmorNumber;        //local player's health displayed in text alongside the image
    [SerializeField]
    private GameObject OtherPlayerHealthBar;    //other players health bar which is shown in a small bar
    [SerializeField]
    private Image BossHealthBar;                //health bar of the boss which displays when in a boss fight
    [SerializeField]
    private Text BossName;                      //name of the boss displayed under the health bar
    [SerializeField]
    private Text WaitingForPlayer;              //the waiting for other player text, appears before moving to another level
    [SerializeField]
    private Text DownedText;                    //text shows when player is downed
    [SerializeField]
    private ClockTimerSprite ReviveTimer;       //timer that increments while player is being revived
    [SerializeField]
    private Text AllyName;                      //The name of the other player.
    [SerializeField]
    private Text AllyHealthAmount;              //Amount of health the other player has.
    [SerializeField]
    private Image DownedNotification;           //When the other player is Downed, this activates.
    [SerializeField]
    private Image DeadNotification;             //When the other player is dead, this activates.
    [SerializeField]
    private Image HitMarker;                    //When the player hits an enemy, this activates briefly. 
    [SerializeField]
    private Image DirectionalMarker;            //When the player gets hit by an enemy, this activates briefly to display the direction

    private Player m_LocalPlayerRef;                    //reference to the local player
    private Health m_LocalPlayerHealthRef;              //reference to local player's health
    private Player m_OtherPlayerRef;                    //reference to the client player
    private Health m_BossHealthRef;                     //reference to the health of the current boss
    private bool m_CanInit = false;                     //until all the players are loaded, hud can't be initialized
    private bool m_HasAmmo = false;                     //does this character need ammo displayed?
    private bool m_InitDone = false;                    //when the initialization is done, this is true
    private bool m_AlreadyDeadCheck = false;            //This check is so we don't keep trying to turn on the death notification.
    private float m_BossStartingHP = 0;                 //health of the boss at the start of the battle
    private WeaponType m_LocalPlayerWeaponType;         //The weapon type that the local player is currently using 

    private float m_OldHealth = 0;                      //most updated health value before any changes have been made
    private float m_OldMaxHealth = 0;                   //most updated max health value before any changes have been made
    private float m_OldArmor = 0;                       //most updated armor value before any changes have been made
    private int m_OldAmmoCount = 0;                     //most updated ammo value before any changes have been made
    private int m_OldCurrency = 0;                      //most updated currency before any changes have been made
    private float m_OldBossHealth = 0;                  //most updated boss health value before any changes have been made

    private float m_NotificationTimer = float.MinValue; //timer which increments once the item notification has popped up and increments until the window fades away
    private float m_CurrencyTimer = float.MinValue;     //timer which increments when you pick up currency and increments until currency window fades away

    public Player OtherPlayer { get { return m_OtherPlayerRef; } }
    public Player LocalPlayer { get { return m_LocalPlayerRef; } }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        //set up singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            gameObject.SetActive(false);
            return;
        }

        WaitingForPlayer.enabled = false;

        GameManager.Instance.DontDestroyNormalObject(gameObject);
    }

    private void Update()
    {
        //keep checking until all players have joined before initializing the UI
        if (!m_CanInit)
        {
            Player[] players = FindObjectsOfType<Player>();

            if (players.Length == PhotonNetwork.playerList.Length)
            {
                m_CanInit = true;
            }
        }
        else if (m_CanInit && !m_InitDone)
        {
            Init();
            m_InitDone = true; //once the init is done, we can start checking for any updates
        }

        if (m_InitDone)
        {
            UpdateAbilityCoolDown();
            UpdateCooldownTimers();
            CheckIfInBossRoomOrNot();
            UpdateNotificationFade();
            CheckIfInShopRoomOrNot();
            CheckIfJournalIsOpen();
            UpdateReviveTimer();
        }
    }

    private void LateUpdate()
    {
        if (m_InitDone)
        {
            UpdateHealth();
            UpdateBossHealth();
            CheckIfBossIsDead();
            UpdateAmmo();
            UpdateCurrency();
        }
    }

    #endregion

    #region Public Methods
    //when the player is in range to interact with an item, this gets called
    public void ToggleInteractImage(bool flag)
    {
        InteractToggle.gameObject.SetActive(flag);
    }

    //toggles the currency box when in shop or when currency is picked up
    public void ToggleCurrency(bool flag, bool pickUp = false)
    {
        CurrencyBox.gameObject.SetActive(flag);

        if (flag == true)
        {
            CurrencyField.text = m_LocalPlayerRef.GetCurrency().ToString();

            if (pickUp == true)
            {
                m_CurrencyTimer = 0.0f; //start the currency window timer
            }
        }
    }

    public void ToggleLevelTransition(bool flag)
    {
        WaitingForPlayer.enabled = flag;
    }

    //when an item is bought or picked up from the treasure room, this spawns the notification which fades after a short whil
    public void ActivateItemNotification(bool isActiveItem)
    {
        //if it's an active item we search the specific active item variable. otherwise we search the list of passives
        if (isActiveItem)
        {
            //set sprite and text and then set gameobject to active
            PickedUpItemSprite.sprite = Resources.Load<Sprite>(m_LocalPlayerRef.Journal.ActiveItem.SpriteName);
            PickedUpItemText.text = m_LocalPlayerRef.Journal.ActiveItem.Tooltip;

            //set window to active
            PickedUpItemImage.gameObject.SetActive(true);

            ItemClockSprite.SetSprite(m_LocalPlayerRef.Journal.ActiveItem.SpriteName);  //set the item sprite
            ItemClockSprite.gameObject.SetActive(true);                                 //activate item sprite

            m_NotificationTimer = 0.0f; //start the timer for fade
        }
        else
        {
            //set sprite and text and then set gameobject to active
            Journal playerJournal = m_LocalPlayerRef.Journal;
            TheNegative.Items.AbstractPassive item = playerJournal.PassiveItems[playerJournal.PassiveItems.Count - 1];
            PickedUpItemSprite.sprite = Resources.Load<Sprite>(item.SpriteName);
            PickedUpItemText.text = item.Tooltip;

            //set window to active
            PickedUpItemImage.gameObject.SetActive(true);

            m_NotificationTimer = 0.0f; //start the timer for fade
        }
    }

    //enables/disables the shop windows and shows the relevant item name and it's cost
    public void ToggleShopWindow(bool flag, string itemName = "", string itemPrice = "")
    {
        ShopItemImage.gameObject.SetActive(flag);

        if (flag == true)
        {
            ShopItemName.text = itemName;
            ShopItemPrice.text = itemPrice;
        }
    }

    //toggles the boss health bar to display when the boss battle has started
    public void ToggleBossHealthBar(bool flag)
    {
        BossHealthBar.gameObject.SetActive(flag);

        if (flag == true)
        {
            //get reference to the health of the boss in the room and set the starting hp
            m_BossHealthRef = m_LocalPlayerRef.MyIslandRoom.EnemiesInRoom[0].gameObject.GetComponent<Health>();
            m_BossStartingHP = m_BossHealthRef.HP;
            m_OldBossHealth = m_BossStartingHP;
        }
        else
        {
            m_BossHealthRef = null;
            m_BossStartingHP = 0.0f;
            m_OldBossHealth = 0.0f;
            BossName.enabled = false;
        }
    }

    //toggles the window that notifies that you're down once you reach 0 health
    public void ToggleDownedWindow(bool flag) //Used to tell local player they are down and need to be revived.
    {
        DownedText.gameObject.SetActive(flag);
    }

    public void ToggleDownedNotication() //Used to notify other player that are down and need help
    {
        if (!OtherPlayerHealthBar.activeSelf)
            return;

        if (DownedNotification.enabled)
            DownedNotification.enabled = false;
        else
            DownedNotification.enabled = true;

        DeadNotification.enabled = false;
    }

    public void ToggleDeadNotication()
    {
        if (!OtherPlayerHealthBar.activeSelf)
            return;

        if (DeadNotification.enabled)
            DeadNotification.enabled = false;
        else
        {
            DeadNotification.enabled = true;
            AllyHealthAmount.text = "Dead";
            OtherPlayerHealthBar.GetComponent<Image>().fillAmount = 0;
        }


        DownedNotification.enabled = false;
    }

    public void ToggleNotificationsOff()
    {
        if (!OtherPlayerHealthBar.activeSelf)
            return;

        DeadNotification.enabled = false;
        DownedNotification.enabled = false;
    }

    //toggles the radial timer that displays how long the revive has left until completion
    public void ToggleReviveTimer(bool flag)
    {
        ReviveTimer.gameObject.SetActive(flag);
    }

    //reset the game into this scene fresh
    public void ResetGame()
    {
        InGameMenuNavigator.IsGamePaused = false;
        GameManager.Instance.ReloadCurrentLevel();
    }

    //will move the players back to the main menu scene and to the lobby
    public void QuitToLobby()
    {
        InGameMenuNavigator.IsGamePaused = false;
        GameManager.Instance.QuitToLobby();
    }

    //will make both players and quit the game
    public void QuitToMainMenu()
    {
        InGameMenuNavigator.IsGamePaused = false;

        GameObject launch = GameObject.Find("Managers");
        launch.GetPhotonView().RPC("SyncScenes", PhotonTargets.All, false);
        //if the current photon player is the master client then we need to pass owner ship to the other player
        if (PhotonNetwork.isMasterClient && m_OtherPlayerRef != null)
            PhotonNetwork.SetMasterClient(m_OtherPlayerRef.photonView.owner);      

        GameManager.Instance.QuitToMain();
    }

    public void StopPlayerVelocity()
    {
        if (m_LocalPlayerRef != null)
        {
            m_LocalPlayerRef.rigidbody.velocity = Vector3.zero;
        }
    }

    public IEnumerator HitMarkerActive()
    {
        HitMarker.enabled = true;
        Debug.Log("HitMarkerActive");
        yield return new WaitForSeconds(0.2f);
        HitMarker.enabled = false;
    }

    public IEnumerator HitDirectionActive(float angle)
    {
        DirectionalMarker.enabled = true;
        //Vector3 eular = new Vector3(0, 0, angle);
        Quaternion target = Quaternion.Euler(0, 0, angle);
        DirectionalMarker.rectTransform.rotation = target;
        yield return new WaitForSeconds(1f);
        DirectionalMarker.enabled = false;
    }

    #endregion

    #region Private Methods
    private void Init()
    {
        Player[] players = FindObjectsOfType<Player>(); //get an array of every player in the game

        if (players.Length == 2)
        {
            //loop through every player. if they are owned by us, set them as the local player ref. otherwise they are the other player
            for (int i = 0; i < players.Length; i++)
            {
                Player player = players[i].GetComponent<Player>();

                if (player.photonView.isMine)
                {
                    SetupLocalPlayer(player);
                }
                else
                {
                    m_OtherPlayerRef = player;
                    if (m_OtherPlayerRef != null)
                    {
                        OtherPlayerHealthBar.SetActive(true);
                        AllyName.text = m_OtherPlayerRef.gameObject.name;
                        AllyHealthAmount.text = m_OtherPlayerRef.Health.HP.ToString();
                    }
                }
            }
        }
        else
        {
            SetupLocalPlayer(players[0].GetComponent<Player>());
        }

        AmmoCount.text = m_LocalPlayerRef.AmmoAmount.ToString();
        m_OldAmmoCount = m_LocalPlayerRef.AmmoAmount;
        AmmoCount.gameObject.SetActive(true);

        //set up the sprite images for both abilities
        Ability1Sprite.SetSprite(m_LocalPlayerRef.RangedAbilityName);
        Ability2Sprite.SetSprite(m_LocalPlayerRef.MeleeAbilityName);

        if (m_OtherPlayerRef != null)
            SpectatingText.text += m_OtherPlayerRef.name;

        SetUpDeathCanvases();
        m_LocalPlayerWeaponType = m_LocalPlayerRef.WeaponType;
    }

    private void SetupLocalPlayer(Player player)
    {
        //set the local player reference and their health amount
        m_LocalPlayerRef = player;
        m_LocalPlayerHealthRef = player.GetComponent<Health>();
        LocalPlayerHealthNumber.text = m_LocalPlayerHealthRef.HP.ToString();

        //set up the old variable values to their starting values
        m_OldHealth = m_LocalPlayerHealthRef.HP;
        m_OldMaxHealth = m_LocalPlayerHealthRef.MaxHp;
        m_OldArmor = m_LocalPlayerHealthRef.ArmorHP;
        m_OldCurrency = m_LocalPlayerRef.GetCurrency();
    }

    //Updates abilitiy and item cooldowns when they have been used
    private void UpdateCooldownTimers()
    {

        //if the ranged ability has been activated
        if (m_LocalPlayerRef.RangedAbilityTimer != float.MinValue || m_LocalPlayerRef.MeleeAbilityTimer != float.MinValue)
        {
            //set the fill amount to a normalized value between 0 and 1
            if (m_LocalPlayerWeaponType == WeaponType.RANGED)
            {
                float normalizedValue = m_LocalPlayerRef.RangedAbilityTimer / m_LocalPlayerRef.RangedAbilityCooldown;
                Ability1Sprite.SetFillAmount(normalizedValue);
            }
            if (m_LocalPlayerWeaponType == WeaponType.MELEE)
            {
                float normalizedValue = m_LocalPlayerRef.MeleeAbilityTimer / m_LocalPlayerRef.MeleeAbilityCooldown;
                Ability1Sprite.SetFillAmount(normalizedValue);
            }
        }

        else
        {
            Ability1Sprite.SetFillAmount(0);
        }

        //if ability 2 has been activated
        if (m_LocalPlayerRef.MeleeAbilityTimer != float.MinValue)
        {
            //set the fill amount to a normalized value between 0 and 1
            float normalizedValue = m_LocalPlayerRef.MeleeAbilityTimer / m_LocalPlayerRef.MeleeAbilityCooldown;
            Ability2Sprite.SetFillAmount(normalizedValue);
        }
        else
        {
            Ability2Sprite.SetFillAmount(0);
        }

        //if the player has an active item
        if (ItemClockSprite.gameObject.activeInHierarchy)
        {
            TheNegative.Items.AbstractItem activeItem = m_LocalPlayerRef.Journal.ActiveItem;

            if (activeItem != null)
            {
                //if the item is room based
                if (activeItem.ItemType == TheNegative.Items.ItemType.ActiveRoom)
                {
                    TheNegative.Items.AbstractActiveRoomBased roomItem = activeItem as TheNegative.Items.AbstractActiveRoomBased;

                    //normalize value and then set fill amount
                    float cooldownFrom0 = roomItem.RoomCount - roomItem.RoomCooldown; //this is so cooldown counts up instead of down for UI
                    float normalizedValue = cooldownFrom0 / roomItem.RoomCount;
                    ItemClockSprite.SetFillAmount(normalizedValue, true);
                }
                else if (activeItem.ItemType == TheNegative.Items.ItemType.ActiveTimer) //otherwise if the item is timer based
                {
                    TheNegative.Items.AbstractActiveTimerBased timerItem = activeItem as TheNegative.Items.AbstractActiveTimerBased;

                    //normalize value and then set fill amount
                    TheNegative.Items.AbstractActiveTimerBased secondCheckItem = m_LocalPlayerRef.Journal.ActiveItem as TheNegative.Items.AbstractActiveTimerBased;
                    float normalizedValue = secondCheckItem.Timer / secondCheckItem.CooldownTime;

                    if (normalizedValue >= 1)
                    {
                        ItemClockSprite.SetFillAmount(0);
                    }
                    else
                    {
                        ItemClockSprite.SetFillAmount(normalizedValue);
                    }
                }
            }
        }
    }

    //Check what weapon type the player is using and swap the active ability 
    private void UpdateAbilityCoolDown()
    {
        //If the weaponType is not the same a before switch whick cooldown is baing used
        if (m_LocalPlayerRef.WeaponType != m_LocalPlayerWeaponType)
        {
            m_LocalPlayerWeaponType = m_LocalPlayerRef.WeaponType;

            if (m_LocalPlayerWeaponType == WeaponType.MELEE)
            {
                Ability1Sprite.SetSprite(m_LocalPlayerRef.MeleeAbilityName);
            }
            if (m_LocalPlayerWeaponType == WeaponType.RANGED)
            {
                Ability1Sprite.SetSprite(m_LocalPlayerRef.RangedAbilityName);
            }

        }
    }


    //Check if health or armor values have changed and updates UI elements accordingly
    private void UpdateHealth()
    {
        //if health or max health value has changed
        if (m_OldHealth != m_LocalPlayerHealthRef.HP || m_OldMaxHealth != m_LocalPlayerHealthRef.MaxHp)
        {
            //normalize the health amount and set that as the globe fill amount
            float normalizedValue = (float)m_LocalPlayerHealthRef.HP / m_LocalPlayerHealthRef.MaxHp;
            LocalPlayerHealthGlobe.fillAmount = normalizedValue;

            //set the health number and reset the old health value
            LocalPlayerHealthNumber.text = m_LocalPlayerHealthRef.HP.ToString();
            m_OldHealth = m_LocalPlayerHealthRef.HP;
            m_OldMaxHealth = m_LocalPlayerHealthRef.MaxHp;
        }

        //if armor value has changed
        if (m_OldArmor != m_LocalPlayerHealthRef.ArmorHP)
        {
            //normalize the armor value and set that as the globe fill amount
            float normalizedValue = (float)m_LocalPlayerHealthRef.ArmorHP / m_LocalPlayerHealthRef.MaxArmorHP;
            LocalPlayerArmorGlobe.fillAmount = normalizedValue;
            m_OldArmor = m_LocalPlayerHealthRef.ArmorHP; //reset the old armor value
            LocalPlayerArmorNumber.text = m_LocalPlayerHealthRef.ArmorHP.ToString();
        }

        if (m_LocalPlayerHealthRef.ArmorHP == 0)
        {
            LocalPlayerArmorGlobe.enabled = false;
            LocalPlayerArmorNumber.enabled = false;
        }
        else
        {
            LocalPlayerArmorGlobe.enabled = true;
            LocalPlayerArmorNumber.enabled = true;
        }

        if (m_OtherPlayerRef == null)
        {
            if (OtherPlayerHealthBar.activeSelf)
            {
                OtherPlayerHealthBar.SetActive(false);
                AllyHealthAmount.text = "";
                AllyName.text = "";
            }
        }

        if (m_OtherPlayerRef == null)
            return;

        UpdateAllyHealthBar();
    }

    private void UpdateAllyHealthBar()
    {
        float normalizedValue = (float)m_OtherPlayerRef.Health.HP / m_OtherPlayerRef.Health.MaxHp;

        OtherPlayerHealthBar.GetComponent<Image>().fillAmount = normalizedValue;
        AllyHealthAmount.text = m_OtherPlayerRef.Health.HP.ToString();

    }

    //updates the timer which fades out the notification when time is up
    private void UpdateNotificationFade()
    {
        if (m_NotificationTimer != float.MinValue)
        {
            m_NotificationTimer += Time.deltaTime;

            if (m_NotificationTimer >= 2.0f)
            {
                //TODO: add fade
                PickedUpItemImage.gameObject.SetActive(false);
            }
        }

        if (m_CurrencyTimer != float.MinValue)
        {
            m_CurrencyTimer += Time.deltaTime;

            if (m_CurrencyTimer >= 2.0f)
            {
                //TODO: add fade
                CurrencyBox.gameObject.SetActive(false);
            }
        }
    }

    //if the player has ammo, update it when it changes
    private void UpdateAmmo()
    {
        if (m_LocalPlayerRef.AmmoAmount != m_OldAmmoCount)
        {
            AmmoCount.text = m_LocalPlayerRef.AmmoAmount.ToString();
            m_OldAmmoCount = m_LocalPlayerRef.AmmoAmount;
            //Change the fill of the AmmoClock
            float NormalizedAmmoCount = (float)m_LocalPlayerRef.AmmoAmount / (float)m_LocalPlayerRef.MaxAmmo;
            PlayerAmmoClock.fillAmount = NormalizedAmmoCount;
        }
    }

    //Checks if player has entered or exited the shop room to enable/disable currency text
    private void CheckIfInShopRoomOrNot()
    {
        if (m_LocalPlayerRef.MyIslandRoom != null)
        {
            if (m_LocalPlayerRef.MyIslandRoom.TypeOfRoom == RoomType.Shop && CurrencyField.gameObject.activeInHierarchy != true)
            {
                ToggleCurrency(true);
            }
            else if (m_LocalPlayerRef.MyIslandRoom.TypeOfRoom != RoomType.Shop && CurrencyField.gameObject.activeInHierarchy == true)
            {
                //this check in place so it doesn't get set to false when the player has picked up currency
                if (m_CurrencyTimer == float.MinValue)
                    ToggleCurrency(false);
            }
        }
    }

    //if the player enters the boss room, activate the boss health bar
    private void CheckIfInBossRoomOrNot()
    {
        if (m_LocalPlayerRef.MyIslandRoom != null)
        {
            if (m_LocalPlayerRef.MyIslandRoom.TypeOfRoom == RoomType.Boss && BossHealthBar.gameObject.activeInHierarchy != true)
            {
                ToggleBossHealthBar(true);
            }
        }
    }

    private void CheckIfJournalIsOpen()
    {
        if (GameObject.Find("JournalCanvas").GetComponent<Canvas>().enabled)
        {
            ToggleCurrency(true);
        }
    }

    //if the boss is dead, deactivate the health bar
    private void CheckIfBossIsDead()
    {
        if (m_BossHealthRef != null)
        {
            if (BossHealthBar.gameObject.activeInHierarchy)
            {
                if (m_BossHealthRef.IsDead)
                {
                    ToggleBossHealthBar(false);
                }
            }
        }
    }

    //if currency has changed, updated it
    private void UpdateCurrency()
    {
        if (m_OldCurrency != m_LocalPlayerRef.GetCurrency())
        {
            m_OldCurrency = m_LocalPlayerRef.GetCurrency();
            CurrencyField.text = m_OldCurrency.ToString();
        }
    }

    //when in boss fight, the boss health updates
    private void UpdateBossHealth()
    {
        if (BossHealthBar == null || m_BossHealthRef == null)
            return;

        if (BossHealthBar.gameObject.activeInHierarchy)
        {
            if (m_OldBossHealth != m_BossHealthRef.HP)
            {
                //normalize the health value and set that to the bar fill amount
                float normalizedValue = m_BossHealthRef.HP / m_BossStartingHP;
                BossHealthBar.fillAmount = normalizedValue;
                m_OldBossHealth = m_BossHealthRef.HP; //update the old health
            }
        }
    }

    private void SetUpDeathCanvases()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            Button[] buttons = GameOverCanvas.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.GetComponentInChildren<Text>().text.ToLower() != "quit")
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateReviveTimer()
    {
        if (ReviveTimer.gameObject.activeInHierarchy)
        {
            if (m_LocalPlayerRef.IsReviving)
            {
                float normalizedValue = m_LocalPlayerRef.ReviveTimer / 5.0f;
                ReviveTimer.SetFillAmount(normalizedValue);
            }
            else
            {
                float normalizedValue = m_OtherPlayerRef.ReviveTimer / 5.0f;
                ReviveTimer.SetFillAmount(normalizedValue);
            }
        }
    }
    #endregion  
}
