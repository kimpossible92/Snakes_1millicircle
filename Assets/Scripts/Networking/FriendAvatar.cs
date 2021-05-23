using System.Collections;
using UnityEngine;

namespace SteamNetworking.Avatar
{
    public class FriendAvatar : Avatar
    {
        public void Invite()
        {
            Facepunch.Steamworks.Client.Instance.Lobby.InviteUserToLobby(steamID);
        }
    }
}