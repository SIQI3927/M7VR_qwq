using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameManager : NetworkBehaviour
{
    public enum GameState { None, PreGame, OnGame, PostGame };

    public MiniGame miniGame;

    [SerializeField]
    private Transform gameArena;

    [SerializeField]
    private Transform postGameArena;

    private TeleportationProvider teleportationProvider;

    private NetworkVariable<GameState> networkGameState = new(GameState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkList<ulong> networkGameQueue;

    private NetworkVariable<int> networkTimeToPrepare = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private int minNumberOfPLayers = 2;
    [SerializeField]
    private int maxNumberOfPlayers = 4;

    [SerializeField]
    private int timeToPrepare = 10;

    [SerializeField]
    private int timeToFinish = 5;


    private void Awake()
    {
        networkGameQueue = new NetworkList<ulong>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            networkTimeToPrepare.OnValueChanged += (oldValue, newValue) =>
            {
                TimeClientRpc(newValue);
            };

            networkGameQueue.OnListChanged += (changeEvent) =>
            {
                if (networkGameState.Value == GameState.None && networkGameQueue.Count >= minNumberOfPLayers)
                {
                    networkGameState.Value = GameState.PreGame;
                    _ = InitGame();
                }
            };

            networkGameState.OnValueChanged += (oldValue, newValue) =>
            {
            };
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        teleportationProvider = FindFirstObjectByType<TeleportationProvider>();
    }

    [ClientRpc]
    public void TimeClientRpc(int currentTime)
    {
        print(currentTime);
        print($"Numero de jugadores en la queue # {networkGameQueue.Count}");
    }
    [ServerRpc(RequireOwnership = false)]
    public void JoinClientToGameServerRpc(ulong clientId)
    {
        if (networkGameQueue.Count < maxNumberOfPlayers && !networkGameQueue.Contains(clientId) && NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            networkGameQueue.Add(clientId);
        }
    }
    public void JoinGame()
    {
        JoinClientToGameServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public async Task InitGame()
    {
        if (networkGameState.Value == GameState.PreGame)
        {
            await PreGame();
            TeleportToGame();
        }
        if (networkGameState.Value == GameState.OnGame)
        {
            await OnGame();
        }
    }
    public async Task PreGame()
    {
        while (networkTimeToPrepare.Value < timeToPrepare)
        {
            await Task.Delay(1000);
            networkTimeToPrepare.Value += 1;
        }
        networkGameState.Value = GameState.OnGame;
    }

    public void TeleportToGame()
    {
        print("!El juego ha comenzado");
        TeleportPlayersServerRpc(gameArena.position, gameArena.rotation);
    }

    public async Task OnGame()
    {
        await miniGame.OnGame();
    }

    public void PostGame()
    {
        print("!El juego ha terminado");
        TeleportPlayersServerRpc(postGameArena.position, postGameArena.rotation);
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayersServerRpc(Vector3 targetPosition, Quaternion targetRotation)
    {
        foreach (var player in networkGameQueue)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(player, out var client))
            {
                print($"Teleportando al jugador {player}");
                TeleportPlayerClientRpc(player, targetPosition, targetRotation);
            }
        }
    }


    [ClientRpc]
    public void TeleportPlayerClientRpc(ulong playerId, Vector3 targetPosition, Quaternion targetRotation)
    {
        Vector3 spawnPosition = targetPosition+ new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)); // Evitar superposiciones
        Quaternion spawnRotation = targetRotation;
        TeleportToArea(spawnPosition, spawnRotation);
        print($"Jugador {playerId} teletransportado");
    }
    void TeleportToArea(Vector3 position, Quaternion rotation)
    {
        TeleportRequest teleportRequest = new TeleportRequest
        {
            destinationPosition = position,
            destinationRotation = rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        teleportationProvider.QueueTeleportRequest(teleportRequest);
    }
}
