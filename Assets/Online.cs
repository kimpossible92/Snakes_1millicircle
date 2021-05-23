using UnityEngine;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System;

public class Online : MonoBehaviour {
    protected Callback<P2PSessionRequest_t> _newConnection;
    public List<CSteamID> lobby_members;
    public List<string> lobby_names;

    // Use this for initialization
    void Start () {
        lobby_names = new List<string>();
        lobby_members = new List<CSteamID>();
        _newConnection = Callback<P2PSessionRequest_t>.Create(OnNetworkConnection);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void getNetworkData()
    {
        uint msgSize;
        //while(SteamNetworking.IsP2PPacketAvailable(out msgSize))
        //{
        //    byte[] packet = new byte[msgSize];
        //    CSteamID steamIDRemote;
        //    uint bytesRead = 0;
        //    if(SteamNetworking.ReadP2PPacket(packet, msgSize, out bytesRead, out steamIDRemote))
        //    {
        //        int TYPE = packet[0];
        //        string msg = System.Text.Encoding.UTF8.GetString(SubArray(packet, 1, packet.Length - 1));
        //        switch(TYPE)
        //        {
        //            case 1:
        //                print(lobby_names[getPlayerIndex(steamIDRemote)]+"say:"+msg);
        //                break;
        //            default: print("Bad packet"); break;
        //        }
        //    }
        //}
    }
    void OnNetworkConnection(P2PSessionRequest_t result)
    {
        foreach(CSteamID id in lobby_members)
        {
            if(id==result.m_steamIDRemote)
            {
                //SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
                return;
            }
        }
    }
    int getPlayerIndex(CSteamID input)
    {
        for(int i = 0;i<lobby_members.Count;i++)
        {
            if (lobby_members[i] == input)
                return i;
        }
        return -1;
    }
    public T[] SubArray<T>(T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
}
