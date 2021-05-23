using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using System.Collections;

public class LobbySteamworks : MonoBehaviour {
    protected Callback<LobbyCreated_t> lobby_Created;
    protected Callback<LobbyMatchList_t> lobby_list;
    protected Callback<LobbyEnter_t> lobby_Enter;
    protected Callback<LobbyDataUpdate_t> lobby_info;
    ulong currentID;
    List<CSteamID> lobbyIDS;
	// Use this for initialization
	void Start () {
        lobbyIDS = new List<CSteamID>();
        lobby_Created = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobby_list = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        lobby_Enter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobby_info = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyInfo);
	}
    void Update()
    {
        
    }
    // Update is called once per frame
    void OnGUI()
    {
        SteamAPI.RunCallbacks();

        if (GUI.Button(new Rect(120,20,140,20),"Create Lobby"))
        {
            print("Create lobby");
            SteamAPICall_t host = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic,4);
        }
        if(GUI.Button(new Rect(120,20,280,20),"lobbies list"))
        {
            print("list lobbies");
            SteamAPICall_t getList = SteamMatchmaking.RequestLobbyList();
        }
        if (GUI.Button(new Rect(120, 20, 420, 20), "change first lobby"))
        {
            print("change first lobby");
            SteamAPICall_t joinLobby = SteamMatchmaking.JoinLobby(SteamMatchmaking.GetLobbyByIndex(0));
        }
        if(GUI.Button(new Rect(120,20,560,20),"Num Players"))
        {
            int numPlayers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)currentID);
            print("Number of players"+numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                print("\t Player(" + i + ")==" + SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)currentID, i)));
            }
        }
    }
    void OnLobbyCreated(LobbyCreated_t result)
    {
        if(result.m_eResult==EResult.k_EResultOK)
        {

        }
        string personalname = SteamFriends.GetPersonaName();
        SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "name", personalname);
    }
    void OnGetLobbiesList(LobbyMatchList_t result)
    {
        for (int i = 0; i < result.m_nLobbiesMatching; ++i)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDS.Add(lobbyId);
            SteamMatchmaking.RequestLobbyData(lobbyId);
        }
    }
    void OnLobbyEntered(LobbyEnter_t result)
    {
        currentID = result.m_ulSteamIDLobby;
        if(result.m_EChatRoomEnterResponse==1)
        {
            print("Lobby joined");
        }
    }
    void OnGetLobbyInfo(LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIDS.Count; i++)
        {
            if (lobbyIDS[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                print("lobby " + i + "::" + SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDS[i].m_SteamID, "name"));
                return;
            }
        }
    }
}
