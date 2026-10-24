using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    private NetworkVariable<Dictionary<FixedString32Bytes, bool>> playersReady = new NetworkVariable<Dictionary<FixedString32Bytes, bool>>();  // TODO: Making this variable public changes its behaviour for some reason
    private MenuHandler menuHandler;
    private string playerName; // TEMPORARY SIMPLE SOLUTION TO NAMING A PLAYER

    public override void OnNetworkSpawn()
    {
        // TODO: Something seems to go wrong in this function
        menuHandler = FindAnyObjectByType<MenuHandler>();
        menuHandler.SetServerManager(this);

        if (playersReady.Value == null && IsServer)
        {
            playersReady.Value = new Dictionary<FixedString32Bytes, bool>();
        }
        //playersReady.OnValueChanged += OnPlayersReadyVariableChanged;  TODO: Decide to get rid of this or implement it
        playerName = $"Player {NetworkManager.Singleton.LocalClientId + 1}";
        AddPlayerReadyValueRpc(NetworkManager.Singleton.LocalClientId);

        SyncPlayerTextRpc();
    }

    /*
    private void OnPlayersReadyVariableChanged(Dictionary<FixedString32Bytes, bool> prev, Dictionary<FixedString32Bytes, bool> newValue)
    {
        Debug.Log($"Player Count from variable change function: {prev.Count} -> {newValue.Count}");
    }
    */

    public void SetReady(bool r)
    {
        SetReadyRpc(r, NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void AddPlayerReadyValueRpc(ulong id)
    {
        AddPlayerReadyValueImmediatelyRpc(id);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddPlayerReadyValueImmediatelyRpc(ulong id)
    {
        playersReady.Value[$"Player {id + 1}"] = false;
    }

    [Rpc(SendTo.Server)]
    private void SetReadyRpc(bool r, ulong id)
    {
        SetReadyInUIRpc(r, id);

        playersReady.Value[$"Player {id + 1}"] = r;

        // TODO: Check if lobby is full and all players are ready then start the game (load the next scene)
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetReadyInUIRpc(bool r, ulong id)
    {
        if (NetworkManager.Singleton.LocalClientId == id)
        {
            menuHandler.SetSessionPrepButtonValues(r);
        }

        menuHandler.SetPlayerTextReadyState(r, $"Player {id + 1}");
    }


    [Rpc(SendTo.Server)]
    private void SyncPlayerTextRpc()
    {
        UpdatePlayerTextRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePlayerTextRpc()
    {
        menuHandler.SyncPlayerText(playersReady.Value);
    }

    public void RemovePlayer()
    {
        RemovePlayerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RemovePlayerRpc(ulong id)
    {
        playersReady.Value.Remove($"Player {id + 1}");
        UpdatePlayerTextRpc();
    }
}
