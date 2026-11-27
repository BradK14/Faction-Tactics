using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    // Variables
    [SerializeField] private GameObject loadOverlay;
    private ISession session;

    // Functions
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        // Only the server handles scene events
        if (!IsServer) return;

        // Wait for all clients to arrive before spawning client specific objects with the camera being last
        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            // Spawn player objects, units, structures, then camera
            Debug.Log("All clients here");

            LoadScene();
        }
    }

    // Server only function to organize and communicate starting information for all clients
    private async void LoadScene()
    {
        // Get session info
        session = MultiplayerService.Instance.Sessions.Values.FirstOrDefault();
        if (session != null)
        {
            Debug.Log($"Session Name: {session.Name}");
            Debug.Log($"Session ID: {session.Id}");
            Debug.Log($"Join Code: {session.Code}");
            Debug.Log($"Players: {session.PlayerCount} / {session.MaxPlayers}");

            if (session.Properties.TryGetValue("Map", out SessionProperty m))
            {
                Debug.Log($"Map: {m.Value}");
            }

            if (session.Properties.TryGetValue("Timer", out SessionProperty t))
            {
                Debug.Log($"Timer: {t.Value}");
            }
        }

        // Map
        // Delay to test
        await Awaitable.WaitForSecondsAsync(5.0f);

        // Drop the load overlay
        RemoveLoadOverlayRpc();
    }

    // RPCs
    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveLoadOverlayRpc()
    {
        loadOverlay.SetActive(false);
    }
}
