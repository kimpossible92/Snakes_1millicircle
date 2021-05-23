using Facepunch.Steamworks;
using SteamNetworking.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Facepunch.Steamworks.Networking;

namespace SteamNetworking
{
    
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance = null;

        // The app id should be 480 for testing purposes
        public uint appId = 480;
        public bool debugClientMessages = false;
        public bool debugServerMessages = false;

        // Dynamically let other classes subscribe to these events
        public Dictionary<NetworkMessageType, System.Action<byte[], ulong>> clientMessageEvents;
        public Dictionary<NetworkMessageType, System.Action<byte[], ulong>> serverMessageEvents;

        private Client client = null;
        private int serverMessagesOffset = 0;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
                //Debug.LogWarning(nameof(NetworkManager) + " cannot have multiple instances! Duplicate destroyed.");
                return;
            }
        }
            // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (IsInitialized())
            {
                UnityEngine.Profiling.Profiler.BeginSample("Steam Update");
                client.Update();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
        public ulong[] GetLobbyMemberIDs()
        {
            return client.Lobby.GetMemberIDs();
        }
        private void SendToClient(ulong steamID, byte[] data, int channel, SendType sendType)
        {
            if (client != null && client.IsValid)
            {
                // Send the message to the client on the channel of this message type
                if (!client.Networking.SendP2PPacket(steamID, data, data.Length, sendType.GetNetworkingSendType(), channel))
                {
                    Debug.Log("Could not send peer to peer packet to user " + steamID);
                }
                else if (debugClientMessages)
                {
                    Debug.Log("Sending message to " + steamID + ":\n" + System.Text.Encoding.UTF8.GetString(data));
                }
            }
        }

        public void SendToClient(ulong steamID, byte[] data, NetworkMessageType networkMessageType, SendType sendType)
        {
            SendToClient(steamID, data, (int)networkMessageType, sendType);
        }

        public void SendToAllClients(byte[] data, NetworkMessageType networkMessageType, SendType sendType)
        {
            if (client != null && client.IsValid)
            {
                ulong[] lobbyMemberIDs = client.Lobby.GetMemberIDs();

                foreach (ulong steamID in lobbyMemberIDs)
                {
                    SendToClient(steamID, data, (int)networkMessageType, sendType);
                }
            }
        }

        public void SendToServer(byte[] data, NetworkMessageType networkMessageType, SendType sendType)
        {
            // Messages for the server are sent on a different channel than messages for a client
            // This way the client knows if the incoming message is for him as a client or him as a server
            SendToClient(client.Lobby.Owner, data, serverMessagesOffset + (int)networkMessageType, sendType);
        }
        

        public bool IsInitialized()
        {
            return client != null && client.IsValid;
        }
        bool OnIncomingConnection(ulong steamID)
        {
            Debug.Log("Incoming peer to peer connection from user " + steamID);
            return true;
        }

        void OnConnectionFailed(ulong steamID, Networking.SessionError sessionError)
        {
            DialogBox.Show("Connection failed with user " + steamID + ", " + sessionError, false, false, null, null);
        }

        // This is where all the messages are received and delegated to the respective events
        void OnP2PData(ulong steamID, byte[] data, int dataLength, int channel)
        {
            byte[] trimmedData = new byte[dataLength];
            System.Array.Copy(data, trimmedData, dataLength);

            if (channel < serverMessagesOffset)
            {
                // The message is for the client
                NetworkMessageType messageType = (NetworkMessageType)channel;
                clientMessageEvents[messageType].Invoke(trimmedData, steamID);
            }
            else
            {
                // The message is for the server (which is running on this client)
                NetworkMessageType messageType = (NetworkMessageType)(channel - serverMessagesOffset);
                serverMessageEvents[messageType].Invoke(trimmedData, steamID);
            }
        }

        void OnDestroy()
        {
            if (IsInitialized())
            {
                client.Networking.OnIncomingConnection -= OnIncomingConnection;
                client.Networking.OnConnectionFailed -= OnConnectionFailed;
                client.Networking.OnP2PData -= OnP2PData;
                client.Dispose();
                client = null;
            }
        }

    }
}