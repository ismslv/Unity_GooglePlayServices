/*
ilya.s@fmlht.com
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.SocialPlatforms;

public static class CoreGPGS
{
    public static void Init(bool saveGame, bool debug = false) {
        var config = new PlayGamesClientConfiguration.Builder();
        if (saveGame)
            config.EnableSavedGames();
        if (debug)
            PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.InitializeInstance(config.Build());
        PlayGamesPlatform.Activate();
    }

#region DATA
    public static bool IsAuthentificated {
        get {
            return PlayGamesPlatform.Instance.localUser.authenticated;
        }
    }

    public static string MyID {
        get {
            return Social.localUser.id;
        }
    }

    public static string MyName {
        get {
            return Social.localUser.userName;
        }
    }

    public static Texture2D MyImage {
        get {
            return Social.localUser.image;
        }
    }
#endregion

#region LOGIN
    public static void Login(bool silent = true, Action callbackSuccess = null, Action callbackFailure = null) {
        PlayGamesPlatform.Instance.Authenticate(ok => {
            if (ok && callbackSuccess != null) {
                callbackSuccess.Invoke();
            } else if (!ok && callbackFailure != null) {
                callbackFailure.Invoke();
            }
        }, silent);
    }

    public static void LoginNonSilent(Action callbackSuccess = null, Action callbackFailure = null) {
        Login(false, callbackSuccess, callbackFailure);
    }

    public static void LoginSilent(Action callbackSuccess = null, Action callbackFailure = null) {
        Login(true, callbackSuccess, callbackFailure);
    }

    public static void Logout(Action callback = null) {
        PlayGamesPlatform.Instance.SignOut();
        if (callback != null)
            callback.Invoke();
    }
#endregion

#region UI
    public enum UI {Achievements, Leaderboard};

    public static void ShowUI(UI type) {
        switch (type)
        {
            case UI.Achievements:
                PlayGamesPlatform.Instance.ShowAchievementsUI();
                break;
            case UI.Leaderboard:
                PlayGamesPlatform.Instance.ShowLeaderboardUI();
                break;
        }
    }
#endregion

#region SAVEGAME
    private static void ReadSavedGameHelper(string filename, 
                             Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(
            filename, 
            DataSource.ReadCacheOrNetwork, 
            ConflictResolutionStrategy.UseLongestPlaytime, 
            callback);
    }
    
    private static void WriteSavedGameHelper(ISavedGameMetadata game, byte[] savedData, 
                               Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
            .WithUpdatedPlayedTime(TimeSpan.FromMinutes(game.TotalTimePlayed.Minutes + 1))
            .WithUpdatedDescription("Saved at: " + System.DateTime.Now);
        
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, callback);
    }

    public static void ReadSavedGame(string fileName, Action<string> callbackSuccess, Action callbackFailure = null) {
        Action<SavedGameRequestStatus, byte[]> readBinaryCallback = 
        (SavedGameRequestStatus status, byte[] data) => {
            if (status == SavedGameRequestStatus.Success) {
                try {
                    string result = System.Text.Encoding.UTF8.GetString(data);
                    callbackSuccess.Invoke(result);
                } catch (Exception e) {
                    throw e;
                }
            } else {
                if (callbackFailure != null)
                        callbackFailure.Invoke();
            }
        };

        Action<SavedGameRequestStatus, ISavedGameMetadata> readCallback = 
        (SavedGameRequestStatus status, ISavedGameMetadata game) => {
            if (status == SavedGameRequestStatus.Success) {
                PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, 
                                                    readBinaryCallback);
            }
        };

        ReadSavedGameHelper(fileName, readCallback);
    }

    public static void WriteSavedGame(string fileName, string data, Action callbackSuccess = null, Action callbackFailure = null) {
        Action<SavedGameRequestStatus, ISavedGameMetadata> writeCallback = 
        (SavedGameRequestStatus status, ISavedGameMetadata game) => {
            if (status == SavedGameRequestStatus.Success) {
                if (callbackSuccess != null)
                    callbackSuccess.Invoke();
            } else {
                if (callbackFailure != null)
                    callbackFailure.Invoke();
            }
        };

        Action<SavedGameRequestStatus, ISavedGameMetadata> readCallback = 
        (SavedGameRequestStatus status, ISavedGameMetadata game) => {
            if (status == SavedGameRequestStatus.Success) {
                byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                WriteSavedGameHelper(game, dataBytes, writeCallback);
            }
        };

        ReadSavedGameHelper(fileName, readCallback);
    }
#endregion

#region ACHIEVEMENTS
    public static void GetAchievement(string id, Action<bool> callback = null) {
        PlayGamesPlatform.Instance.ReportProgress(id, 100.0f, callback);
    }

    public static void GetAchievement(string id, double value, Action<bool> callback = null) {
        PlayGamesPlatform.Instance.ReportProgress(id, value, callback);
    }
#endregion

#region LEADERBOARDS
    public static void UpdateLeaderboard(string id, int value, Action<bool> callback = null) {
        PlayGamesPlatform.Instance.ReportScore(value, id, callback);
    }

    public static void GetLeaderboard(string id, string userid, Action<long> callbackSuccess = null, Action callbackFailure = null) {
        ILeaderboard lb = PlayGamesPlatform.Instance.CreateLeaderboard();
        lb.id = id;
        lb.SetUserFilter(new string[] {userid});
        lb.LoadScores(ok =>
            {
                if (ok) {
                    if (callbackSuccess != null)
                        callbackSuccess.Invoke(lb.localUserScore.value);
                }
                else {
                    if (callbackFailure != null)
                        callbackFailure.Invoke();
                }
            });
    }

    public static void GetLeaderboard(string id, Action<long> callbackSuccess = null, Action callbackFailure = null) {
        GetLeaderboard(id, MyID, callbackSuccess, callbackFailure);
    }    
    #endregion
}
