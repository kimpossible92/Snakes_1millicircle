using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Steamworks;
using System.Collections.Generic;

public class SteamData : MonoBehaviour {
    [Header("UI Components")]
    public Text avatarName;
    public Image avatarIcon;
    [Header("Stat String Data")]
    public string[] statStrings;
    public string LeaderboardName;
    public List<LeaderboardEntry> leaderboardEntries;

    protected CallResult<LeaderboardFindResult_t> FindResult;
    protected CallResult<LeaderboardScoresDownloaded_t> DownloadResult;
    protected CallResult<LeaderboardScoreUploaded_t> UploadResult;
    SteamLeaderboard_t leaderboard;
    bool hasLeaderBoard = false;
    void Awake()
    {
        hasLeaderBoard = false;
    }

    int starC;
	// Use this for initialization
	void Start () {
        
        if (!SteamManager.Initialized)
        {
            return;
        }
        avatarName.text = SteamFriends.GetPersonaName();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnEnable()
    {
        if(!SteamManager.Initialized)
        {
            print("<Steamworks> Leader board could not be found");
        }
        else
        {
            FindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
            UploadResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardUploadResult);
        }
    }
    void OnLeaderboardFindResult(LeaderboardFindResult_t cb, bool IOFailure)
    {
        if(cb.m_bLeaderboardFound==0||IOFailure)
        {
            print("<Steamworks> Leader board could not be found");
            hasLeaderBoard = false;
        }
        else
        {
            leaderboard = cb.m_hSteamLeaderboard;
            DownloadGlobalScores(1, 100);
        }
    }
    void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t cb, bool IOFailure)
    {
        if (cb.m_bSuccess == 0 || IOFailure)
        {
            print("<Steamworks> Error Uploading Scores");
        }
        else
        {
            hasLeaderBoard = false;
        }
    }
    public bool DownloadGlobalScores(int start, int end)
    {
        if (leaderboard == null) return false;
        SteamAPICall_t steamCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, start, end);
        DownloadResult.Set(steamCall);
        return true;
    }
    public void UploadScore(int score)
    {
        if(leaderboard==null)
        {
            hasLeaderBoard = false;
        }
        else
        {
            SteamAPICall_t steamCall = SteamUserStats.UploadLeaderboardScore(leaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, score, null, 0);
            UploadResult.Set(steamCall);
        }
    }
    void OnLeaderboardDownloadResult(LeaderboardScoresDownloaded_t cb, bool IOFailure)
    {
        if (IOFailure)
        {
            print("<Steamworks> Error Downloading leaderboard scores");
        }
        else
        {

            print("<Steamworks> Downloaded " + cb.m_cEntryCount + " scores");
            for (int i = 0; i < cb.m_cEntryCount; i++)
            {
                LeaderboardEntry_t entry;
                bool ret = SteamUserStats.GetDownloadedLeaderboardEntry(cb.m_hSteamLeaderboardEntries, i, out entry, null, 0);
                leaderboardEntries.Add(new LeaderboardEntry
                {
                    SteamId = entry.m_steamIDUser.m_SteamID,
                    GlobalRank = entry.m_nGlobalRank,
                    Score = entry.m_nScore
                });
            }
        }
    }
}
[System.Serializable]
public class LeaderboardEntry
{
    public ulong SteamId { get; set; }
    public int GlobalRank { get; set; }
    public int Score { get; set; }
}