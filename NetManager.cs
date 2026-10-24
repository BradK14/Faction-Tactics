using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    [SerializeField] private ServerManager serverManagerPrefab;
    [SerializeField] private MenuHandler menuHandler;
    ServerManager serverManager;
    private ISession session;

    private void Start()
    {
        _ = InitializeUnityServicesAsync();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private async Task InitializeUnityServicesAsync()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            menuHandler.SessionPrepBackButton();
        }
    }

    public async Task StartSessionAsHost(string name, string mapName, string timer)
    {
        var options = new SessionOptions
        {
            MaxPlayers = 2,
            Name = name,
            SessionProperties =
            {
                ["Map"] = new SessionProperty(mapName),
                ["Timer"] = new SessionProperty(timer)
            }
        }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
        session = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");

        // Create a server manager to handle world based networked values
        serverManager = Instantiate(serverManagerPrefab);
        serverManager.GetComponent<NetworkObject>().Spawn();
    }

    public async Task<QuerySessionsResults> GetSessionListAsync(QuerySessionsOptions qso)
    {
        return await MultiplayerService.Instance.QuerySessionsAsync(qso);
    }

    public async Task JoinSession(string sid)
    {
        session = await MultiplayerService.Instance.JoinSessionByIdAsync(sid);
    }

    public async Task LeaveSession()
    {
        await session.LeaveAsync();
        session = null;
    }
}
