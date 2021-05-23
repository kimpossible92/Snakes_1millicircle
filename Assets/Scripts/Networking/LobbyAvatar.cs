using System.Collections;
using UnityEngine;

namespace SteamNetworking.Avatar
{
    public class LobbyAvatar : Avatar
    {
        public bool ready = false;

        public UnityEngine.UI.Image imageReadyOutline;

        public void Refresh()
        {
            if (bool.TryParse(Facepunch.Steamworks.Client.Instance.Lobby.GetMemberData(steamID, "Ready"), out ready))
            {
                imageReadyOutline.color = ready ? Color.green : Color.red;
            }
        }
    }
}