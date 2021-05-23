#define USE_GS_AUTH_API
#define DISABLED
using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;
using SteamNetworking;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using SteamNetworking.Messages;

public class GameServer : MonoBehaviour {

    private Dictionary<int, NetworkObject> networkObjects = new Dictionary<int, NetworkObject>();
    private HashSet<ulong> clientsReadyForInitialization = new HashSet<ulong>();
    private bool allClientsInitialized = false;
    const string SNAKES_SERVER_VERSION = "1.0.0.0";
    const ushort AUTHENTICATION_PORT = 8766;
    const ushort SERVER_PORT = 27015;
    const ushort MASTER_SERVER_UPDATER_PORT = 27016;
    protected Callback<SteamServersConnected_t> SteamServersConnected;
#if DISABLED
    protected Callback<SteamServerConnectFailure_t> SteamServersConnectedFailure;
    protected Callback<SteamServersDisconnected_t> SteamServerDisconnected;
    protected Callback<GSPolicyResponse_t> CallbackPolicyResponse;
    protected Callback<ValidateAuthTicketResponse_t> CallbackGSAuthTicketResponse;
    protected Callback<P2PSessionRequest_t> CallbackP2PSessionRequest;
    protected Callback<P2PSessionConnectFail_t> CallbackP2PSessionConnectFail;
#endif
    bool Initialized = false;
    bool _connectedToSteam = false;
    #region SteamNetwork
    public static GameServer Instance = null;

    [Header("Server Parameters")]
    public float hz = 16;
    [Tooltip("Will not send objects if they didn't change their transform. Enabling can cause teleportation for objects that start moving after being static.")]
    [SerializeField]
    private bool onlySendChanges = true;
    [Space(10)]
    public UnityEvent onServerInitialized;

    // Use this for initialization
    void Start () {
        SteamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
#if DISABLED
        SteamServersConnectedFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServersConnectFailure);
        SteamServerDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
        CallbackPolicyResponse = Callback<GSPolicyResponse_t>.CreateGameServer(OnPolicyResponse);
        CallbackGSAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(OnValidateAuthTicketResponse);
        CallbackP2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(OnP2PSessionRequest);
        CallbackP2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.CreateGameServer(OnP2PSessionConnectFail);
#endif
        Initialized = false;
        _connectedToSteam = false;
#if USE_GS_AUTH_API
        EServerMode eMode = EServerMode.eServerModeAuthenticationAndSecure;
#endif
        uint unFlags = 27016;
        AppId_t nGameAppId = new AppId_t();
        Initialized = SteamGameServer.InitGameServer(0, AUTHENTICATION_PORT, SERVER_PORT, unFlags,nGameAppId, SNAKES_SERVER_VERSION);
        if(Initialized==false)
        {
            return;
        }
        SteamGameServer.SetModDir("snakes");
        SteamGameServer.SetProduct("Steamworks Example");
        SteamGameServer.SetGameDescription("Steamworks Example");
        SteamGameServer.LogOnAnonymous();
        SteamGameServer.EnableHeartbeats(true);
        print("Started");




        //SteamNetworking
        // Pause everything until all clients are initialized
        Time.timeScale = 0;

        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviour] += OnMessageNetworkBehaviour;
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] += OnMessageNetworkBehaviourInitialized;
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.Initialization] += OnMessageInitialization;
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] += OnMessagePingPong;
    }
    IEnumerator ServerUpdate()
    {
        Time.timeScale = 1;

        while (true)
        {
            yield return new WaitForSecondsRealtime(1.0f / hz);

            SendAllNetworkObjects(onlySendChanges, SendType.Unreliable);
        }
    }

    private int GetHierarchyDepthOfTransform(Transform transform, int depth)
    {
        if (transform.parent != null)
        {
            return GetHierarchyDepthOfTransform(transform.parent, 1 + depth);
        }

        return depth;
    }

    private void SendAllNetworkObjects(bool onlySendChangedTransforms, SendType sendType)
    {
        // Save all network object messages that need to be sended into one pool
        // Sort the pool by the depth of the transform in the hierarchy
        // This makes sure that parents are instantiated before their children
        List<KeyValuePair<int, NetworkObject>> networkObjectsToSend = new List<KeyValuePair<int, NetworkObject>>();

        foreach (NetworkObject networkObject in networkObjects.Values)
        {
            if (!onlySendChangedTransforms || networkObject.HasChanged())
            {
                KeyValuePair<int, NetworkObject> networkObjectToAdd = new KeyValuePair<int, NetworkObject>(GetHierarchyDepthOfTransform(networkObject.transform, 0), networkObject);
                networkObjectsToSend.Add(networkObjectToAdd);
            }
        }

        // Sort by the depth of the transform
        networkObjectsToSend.Sort
        (
            delegate (KeyValuePair<int, NetworkObject> a, KeyValuePair<int, NetworkObject> b)
            {
                return a.Key - b.Key;
            }
        );

        // Standard is UDP packet size, 1200 bytes
        int maximumTransmissionLength = 1200;

        if (sendType == SendType.Reliable)
        {
            // TCP packet, up to 1MB
            maximumTransmissionLength = 1000000;
        }

        // Create and send network object list messages until the pool is empty
        while (networkObjectsToSend.Count > 0)
        {
            // Make sure that the message is small enough to fit into the UDP packet (1200 bytes)
            MessageNetworkObjectList messageNetworkObjectList = new MessageNetworkObjectList();

            while (true)
            {
                if (networkObjectsToSend.Count > 0)
                {
                    // Add next message
                    MessageNetworkObject message = new MessageNetworkObject(networkObjectsToSend[0].Value);
                    messageNetworkObjectList.messages.AddLast(message.ToBytes());

                    // Check if length is still small enough
                    if (messageNetworkObjectList.GetLength() <= maximumTransmissionLength)
                    {
                        // Small enough, keep message and remove from objects to send
                        networkObjectsToSend.RemoveAt(0);
                    }
                    else
                    {
                        // Too big, remove message and create a new list to send the rest
                        messageNetworkObjectList.messages.RemoveLast();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Send the message to all clients
            NetworkManager.Instance.SendToAllClients(messageNetworkObjectList.ToBytes(), NetworkMessageType.NetworkObjectList, sendType);
        }
    }

    public GameObject InstantiateInScene(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
    {
        // Switch scenes, instantiate object and then switch the scene back
        Scene previouslyActiveScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(gameObject.scene);
        GameObject result = Instantiate(original, position, rotation, parent);
        SceneManager.SetActiveScene(previouslyActiveScene);
        return result;
    }

    public void RegisterAndSendMessageNetworkObject(NetworkObject networkObject)
    {
        if (allClientsInitialized)
        {
            // Make sure that objects are spawned on the server (with UDP it could happen that they don't spawn)
            MessageNetworkObject message = new MessageNetworkObject(networkObject);
            NetworkManager.Instance.SendToAllClients(message.ToBytes(), NetworkMessageType.NetworkObject, SendType.Reliable);
        }

        networkObjects.Add(networkObject.networkID, networkObject);
    }

    public NetworkObject GetNetworkObject(int networkID)
    {
        return networkObjects[networkID];
    }

    public void RemoveNetworkObject(NetworkObject networkObject)
    {
        networkObjects.Remove(networkObject.networkID);
    }

    public void SendMessageDestroyNetworkObject(NetworkObject networkObject)
    {
        byte[] data = BitConverter.GetBytes(networkObject.networkID);
        NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.DestroyNetworkObject, SendType.Reliable);
    }

    void OnMessageInitialization(byte[] data, ulong steamID)
    {
        // Only start the server loop if all the clients have loaded the scene and sent the message
        bool allClientsReady = true;
        clientsReadyForInitialization.Add(steamID);

        ulong[] lobbyMemberIDs = NetworkManager.Instance.GetLobbyMemberIDs();

        foreach (ulong id in lobbyMemberIDs)
        {
            if (!clientsReadyForInitialization.Contains(id))
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            allClientsInitialized = true;

            // Make sure that all the objects on the server are spawned for all clients
            SendAllNetworkObjects(false, SendType.Reliable);
            NetworkManager.Instance.serverMessageEvents[NetworkMessageType.Initialization] -= OnMessageInitialization;

            // Answer to all the clients that the initialization finished
            // This works because the messages are reliable and in order (meaning all the objects on the client must have spawned when this message arrives)
            NetworkManager.Instance.SendToAllClients(data, NetworkMessageType.Initialization, SendType.Reliable);

            // Start the server loop and invoke all subscribed actions
            StartCoroutine(ServerUpdate());
            onServerInitialized.Invoke();
        }
    }

    void OnMessagePingPong(byte[] data, ulong steamID)
    {
        // Send the time back but also append the current server hz and server time
        ArrayList tmp = new ArrayList(data);
        tmp.AddRange(BitConverter.GetBytes(hz));
        tmp.AddRange(BitConverter.GetBytes(Time.unscaledTime));
        NetworkManager.Instance.SendToClient(steamID, (byte[])tmp.ToArray(typeof(byte)), NetworkMessageType.PingPong, SendType.Unreliable);
    }

    void OnMessageNetworkBehaviour(byte[] data, ulong steamID)
    {
        MessageNetworkBehaviour message = MessageNetworkBehaviour.FromBytes(data, 0);
        networkObjects[message.networkID].HandleNetworkBehaviourMessage(message.index, message.data, steamID);
    }

    void OnMessageNetworkBehaviourInitialized(byte[] data, ulong steamID)
    {
        MessageNetworkBehaviourInitialized message = ByteSerializer.FromBytes<MessageNetworkBehaviourInitialized>(data);
        networkObjects[message.networkID].HandleNetworkBehaviourInitializedMessage(message.index, steamID);
    }

    void OnDestroy()
    {
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviour] -= OnMessageNetworkBehaviour;
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.NetworkBehaviourInitialized] -= OnMessageNetworkBehaviourInitialized;
        NetworkManager.Instance.serverMessageEvents[NetworkMessageType.PingPong] -= OnMessagePingPong;
    }
    #endregion
    // Update is called once per frame
    void Update () {
        if(Initialized==false)
        {
            return;
        }
        if (_connectedToSteam)
        {
            SendUpdatedServerSteam();
        }
	}
    private void OnSteamServersConnected(SteamServersConnected_t LogonSuccess)
    {
        _connectedToSteam = true;
        SteamGameServer.SetMaxPlayerCount(4);
        SteamGameServer.SetPasswordProtected(false);
        SteamGameServer.SetServerName("Snakes");
        SteamGameServer.SetBotPlayerCount(0);
        SteamGameServer.SetMapName("inGame");
    }
    void OnSteamServersConnectFailure(SteamServerConnectFailure_t connectedFailured)
    {
        _connectedToSteam = false;
    }
    void OnSteamServersDisconnected(SteamServersDisconnected_t serverDisconnected)
    {
        _connectedToSteam = false;
    }
    void OnPolicyResponse(GSPolicyResponse_t PolicyResponses)
    {
        if (SteamGameServer.BSecure())
        {
            print("VAC is Secure");
        }
        else
        {
            print("not VAC is Secure");
        }
        print("SteamID:" + SteamGameServer.GetSteamID().ToString());
    }
    void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t pResponse)
    {
        print(pResponse.m_SteamID);
        if(pResponse.m_eAuthSessionResponse==EAuthSessionResponse.k_EAuthSessionResponseOK)
        {

        }
        else
        {

        }
    }
    void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        SteamGameServerNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
    }
    void OnP2PSessionConnectFail(P2PSessionConnectFail_t Callbackfailed)
    {
        print("OnP2PSessionConnectFail:" + Callbackfailed.m_steamIDRemote);
    }
    void SendUpdatedServerSteam()
    {
        SteamGameServer.SetMaxPlayerCount(4);
        SteamGameServer.SetPasswordProtected(false);
        SteamGameServer.SetServerName("Snakes");
        SteamGameServer.SetBotPlayerCount(0);
        SteamGameServer.SetMapName("inGame");
#if USE_GS_AUTH_API
        for (uint i = 0; i < 4; ++i)
        {

        }
#endif
    }
}
