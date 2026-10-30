using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    [SerializeField] private NetManager networkManager;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject multiplayerMenu;
    [SerializeField] private GameObject createSessionMenu;
    [SerializeField] private GameObject joinSessionMenu;
    [SerializeField] private GameObject sessionPrepMenu;
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Toggle timerToggle;
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private List<TMP_Text> playerTexts;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private TMP_Text timerInfoText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button sessionPrepBackButton;
    [SerializeField] private List<Button> sessionButtons;
    private QuerySessionsResults querySessionResults;
    private ServerManager serverManager;

    // Start function
    private void Start()
    {
        titleMenu.SetActive(true);
    }

    // Get/Set functions
    public void SetServerManager(ServerManager sm)
    {
        serverManager = sm;
        readyButton.interactable = true;
    }

    public void AddPlayerText(string id)
    {
        for (int i = 0; i < playerTexts.Count; i++)
        {
            if (playerTexts[i].text == "")
            {
                playerTexts[i].text = id;
                break;
            }
        }
    }

    public void RemovePlayerText(ulong id)
    {
        for (int i = 0; i < playerTexts.Count; i++)
        {
            if (playerTexts[i].text == $"{id}")
            {
                playerTexts[i].text = "";
                break;
            }
        }
    }

    private void ResetPlayerText()
    {
        foreach (TMP_Text t in playerTexts)
        {
            t.color = new Color(1, 1, 1);
            t.text = "";
        }
    }

    // Button functions
    public void SinglePlayerButton()
    {
        // No Single Player Mode planned yet
    }

    public void MultiplayerButton()
    {
        titleMenu.SetActive(false);
        multiplayerMenu.SetActive(true);
    }

    public void CreateSessionButton()
    {
        multiplayerMenu.SetActive(false);
        createSessionMenu.SetActive(true);
    }

    public void JoinSessionButton()
    {
        multiplayerMenu.SetActive(false);
        joinSessionMenu.SetActive(true);
        _ = CreateSessionListAsync();
    }

    public void CreateButton()
    {
        createSessionMenu.SetActive(false);

        _ = CreateSessionAsync();
    }

    public void JoinButton()
    {
        joinSessionMenu.SetActive(false);

        _ = JoinSessionAsync();
    }
    
    public void ReadyButton()
    {
        if (readyButton.GetComponentInChildren<TMP_Text>().text == "Ready")
        {
            serverManager.SetReady(true);
        }
        else
        {
            serverManager.SetReady(false);
        }
    }

    public void RefreshButton()
    {
        _ = CreateSessionListAsync();
    }

    public void MultiplayerBackButton()
    {
        multiplayerMenu.SetActive(false);
        titleMenu.SetActive(true);
    }

    public void CreateSessionBackButton()
    {
        createSessionMenu.SetActive(false);

        // Reset session information for the create session menu
        sessionNameInput.text = "";
        mapDropdown.value = 0;
        timerToggle.isOn = true;

        multiplayerMenu.SetActive(true);
    }

    public void JoinSessionBackButton()
    {
        joinSessionMenu.SetActive(false);
        multiplayerMenu.SetActive(true);
    }

    public void SessionPrepBackButton()
    {
        sessionPrepMenu.SetActive(false);

        // Reset session information for the create session menu
        sessionNameInput.text = "";
        mapDropdown.value = 0;
        timerToggle.isOn = true;

        serverManager.RemovePlayer();
        _ = LeaveSessionAsync();
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    // Async network call functions
    private async Task CreateSessionListAsync()
    {
        // Reset Session buttons
        for (int i = 0; i < sessionButtons.Count; i++)
        {
            sessionButtons[i].interactable = false;
            sessionButtons[i].gameObject.SetActive(false);
        }

        // Gather up to date list of sessions
        QuerySessionsOptions qso = new QuerySessionsOptions();
        querySessionResults = await networkManager.GetSessionListAsync(qso);

        // Display current sessions
        for (int i = 0; i < querySessionResults.Sessions.Count && i < sessionButtons.Count; i++)
        {
            sessionButtons[i].GetComponentInChildren<TMP_Text>().text = $"{querySessionResults.Sessions[i].Name}\n{querySessionResults.Sessions[i].Properties["Map"].Value}.  {querySessionResults.Sessions[i].Properties["Timer"].Value}";
            sessionButtons[i].interactable = true;
            sessionButtons[i].gameObject.SetActive(true);
        }
    }

    private async Task CreateSessionAsync()
    {
        // Start the session
        string timerText;
        if (timerToggle.isOn) { timerText = "Timer: Yes"; } else { timerText = "Timer: No"; }
        await networkManager.StartSessionAsHost(sessionNameInput.text, mapDropdown.options[mapDropdown.value].text, timerText);

        // Set up details for the session prep menu
        sessionNameText.text = sessionNameInput.text;
        mapNameText.text = $"Map: {mapDropdown.options[mapDropdown.value].text}";
        timerInfoText.text = timerText;

        // Display the session prep menu
        sessionPrepMenu.SetActive(true);
    }

    private async Task JoinSessionAsync()
    {
        // Join the session
        await networkManager.JoinSession(querySessionResults.Sessions[0].Id);  // TODO: use the appropriately selected session instead of the first

        // Set up details for the session prep menu
        sessionNameText.text = querySessionResults.Sessions[0].Name;
        mapNameText.text = $"Map: {querySessionResults.Sessions[0].Properties["Map"].Value}";
        timerInfoText.text = querySessionResults.Sessions[0].Properties["Timer"].Value;

        // Display the session prep menu
        sessionPrepMenu.SetActive(true);
    }

    private async Task LeaveSessionAsync()
    {
        // Disconnect from the session
        await networkManager.LeaveSession();

        // Reset session prep menu info
        serverManager = null;
        ResetPlayerText();
        readyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
        readyButton.gameObject.GetComponent<Image>().color = new Color(0f, 0.9f, 0f);
        readyButton.interactable = false;

        // Return to the multiplayer menu
        multiplayerMenu.SetActive(true);
    }

    // Other functions
    public void SetSessionPrepButtonValues(bool r)
    {
        if (r)
        {
            sessionPrepBackButton.interactable = false;
            readyButton.GetComponentInChildren<TMP_Text>().text = "Cancel";
            readyButton.gameObject.GetComponent<Image>().color = new Color(0.9f, 0f, 0f);
        }
        else
        {
            sessionPrepBackButton.interactable = true;
            readyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            readyButton.gameObject.GetComponent<Image>().color = new Color(0f, 0.9f, 0f);
        }
    }

    public void SetPlayerTextReadyState(bool r, FixedString32Bytes id)
    {
        for (int i = 0; i < playerTexts.Count; i++)
        {
            if (playerTexts[i].text == id)
            {
                if (r)
                {
                    playerTexts[i].color = new Color(0f, 0.9f, 0f);
                }
                else
                {
                    playerTexts[i].color = new Color(1f, 1f, 1f);
                }

                break;
            }
        }
    }

    public void SyncPlayerText(Dictionary<FixedString32Bytes, bool> players)
    {
        Dictionary<FixedString32Bytes, bool> newPlayers = new Dictionary<FixedString32Bytes, bool>(players);

        foreach (TMP_Text text in playerTexts)
        {
            if (newPlayers.ContainsKey(text.text))
            {
                newPlayers.Remove(text.text);
            }
            else
            {
                text.text = "";
            }
        }

        int x = 0;
        foreach (KeyValuePair<FixedString32Bytes, bool> key in newPlayers)
        {
            while (playerTexts[x].text != "") { x++; }
            playerTexts[x].text += key.Key;
            SetPlayerTextReadyState(key.Value, key.Key);
        }
    }
}
