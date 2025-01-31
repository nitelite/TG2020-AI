﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Server server = null;
    public SettingsHolder settings;

    enum Windows : int  { Menu, Credits, Rules , Lobby, Settings}
    //Canvas
    [SerializeField] private GameObject canvas_menu = null;
    [SerializeField] private GameObject canvas_credits = null;
    [SerializeField] private GameObject canvas_rules = null;
    [SerializeField] private GameObject canvas_lobby = null;
    [SerializeField] private GameObject canvas_lobby_playerlist = null;
    [SerializeField] private GameObject canvas_settings = null;

    // Menu buttons
    [SerializeField] private Button menu_credits = null;
    [SerializeField] private Button menu_exit = null;
    [SerializeField] private Button menu_rules = null;
    [SerializeField] private Button menu_start = null;
    [SerializeField] private Button menu_settings = null;

    // Credits buttons
    [SerializeField] private Button credits_back = null;

    // Rules buttons
    [SerializeField] private Button rules_back = null;

    // Lobby buttons
    [SerializeField] private Button lobby_back = null;
    [SerializeField] private Button lobby_start = null;

    // Settings buttons
    [SerializeField] private Button settings_back = null;


    //Lobby assets
    public GameObject lobby_area_players = null;
    private GameObject playerListUI_component = null;

    // Start is called before the first frame update
    void Start()
    {
        // Settings assignment
        settings = GameObject.Find("SettingsHolder").GetComponent<SettingsHolder>();

        // Menu buttons
        menu_start.onClick.AddListener(() => SwapTo(Windows.Lobby));
        menu_settings.onClick.AddListener(() => SwapTo(Windows.Settings));
        menu_credits.onClick.AddListener(() => SwapTo(Windows.Credits));
        menu_rules.onClick.AddListener(() => SwapTo(Windows.Rules));
        menu_exit.onClick.AddListener(ExitGame);

        // Credits buttons
        credits_back.onClick.AddListener(() => SwapTo(Windows.Menu));

        // Rules buttons
        rules_back.onClick.AddListener(() => SwapTo(Windows.Menu));

        // Lobby buttons
        lobby_back.onClick.AddListener(() => SwapTo(Windows.Menu));
        lobby_start.onClick.AddListener(server.StartServer);

        // Settings buttons
        settings_back.onClick.AddListener(() => SwapTo(Windows.Menu));

        lobby_area_players = canvas_lobby.transform.GetChild(0).gameObject; // What on earth is this hack?
        playerListUI_component = Resources.Load<GameObject>("UI/PlayerListItem"); // Used for the lobby menu

        InvokeRepeating("UpdateLobbyPlayers", 0f, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {

    }


    void UpdateLobbyPlayers()
    {
        // Using playerListUI from Start()
        Debug.Log("Update player lobby");
        List<CarController> list = server.GetPlayers();

        // Nuke all the children before building them again
        foreach (Transform child in canvas_lobby_playerlist.transform)
        {
            Destroy(child.gameObject);
        }


        foreach (CarController car in list)
        {
            GameObject gameObject = Instantiate(playerListUI_component, canvas_lobby_playerlist.transform);
            Button kick_button = gameObject.GetComponentInChildren<Button>();
            Text player_name = gameObject.transform.Find("PlayerName").gameObject.GetComponent<Text>();

            kick_button.onClick.AddListener(car.KickPlayer);
            player_name.text = car.UserName;
        }
    }

    void SwapTo(Windows state)
    {
        canvas_menu.SetActive(false);
        canvas_rules.SetActive(false);
        canvas_lobby.SetActive(false);
        canvas_credits.SetActive(false);
        canvas_settings.SetActive(false);

        switch (state)
        {
            case Windows.Menu:
                canvas_menu.SetActive(true);
                break;
            case Windows.Credits:
                canvas_credits.SetActive(true);
                break;
            case Windows.Rules:
                canvas_rules.SetActive(true);
                break;
            case Windows.Lobby:
                canvas_lobby.SetActive(true);
                break;
            case Windows.Settings:
                settings.refresh();
                canvas_settings.SetActive(true);
                break;
            default:
                Debug.LogError("Trying to set invalid state");
                break;
        }
    }

    void ExitGame()
    {
        print("Exit game");
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
