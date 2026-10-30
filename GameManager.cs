using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
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
        }
    }
}
