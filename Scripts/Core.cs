/*
ilya.s@fmlht.com
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core : MonoBehaviour
{
    public static Core a;
    
    public Text statusText;
    public Text textFromSave;
    public Text textFromLeaderboard;
    public Text loginButtonText;

    public GameObject loginPlate;

    private int m_currentScore = 0;
    private List<int> scoresSaved;

    void Awake() {
        a = this;
    }

    void Start()
    {
        Input.simulateMouseWithTouches = true;
        loginPlate.SetActive(true);
        scoresSaved = new List<int>();

        CoreGPGS.Init(true, false);
        CoreGPGS.LoginSilent(AfterSignIn);
    }

    public void AfterSignIn() {
        loginButtonText.text = "logout";
        loginPlate.SetActive(false);
        CoreGPGS.ReadSavedGame("save_file_2",
            result => {
                StringToList(result);
                textFromSave.text = "Saved top 10: " + ListToString();
            });
        GetMyLeaderboard();
    }

#region BUTTONS
    public void ButtonA() {
        CoreGPGS.ShowUI(CoreGPGS.UI.Achievements);
    }

    public void ButtonHS() {
        CoreGPGS.ShowUI(CoreGPGS.UI.Leaderboard);
    }

    public void ButtonLogin() {
        if (!CoreGPGS.IsAuthentificated) {
            CoreGPGS.LoginNonSilent(AfterSignIn);
        } else {
            CoreGPGS.Logout(()=>{
                loginButtonText.text = "login";
            });
        }
    }

    public void ButtonHighscore() {
        if (m_currentScore == 100)
            CoreGPGS.GetAchievement(GPGSIds.achievement_100_batons);
        AddScoreToList(m_currentScore);
        CoreGPGS.WriteSavedGame("save_file_2", ListToString());
        CoreGPGS.UpdateLeaderboard(GPGSIds.leaderboard_top_baton_test, m_currentScore,
        ok => {
            GetMyLeaderboard();
            m_currentScore = 0;
            statusText.text = "0";
        });
    }

    public void OnBatonClicked() {
        m_currentScore++;
        statusText.text = m_currentScore.ToString();
    }
#endregion
    
#region UTILS
    void StringToList(string data) {
        scoresSaved = new List<int>();
        for (int i = 0; i < 10; i++)
            scoresSaved.Add(0);
        var _dataSplit = data.Split(","[0]);
        for (int i = 0; i < _dataSplit.Length; i++) {
            scoresSaved[i] = Convert.ToInt32(_dataSplit[i]);
        }
    }

    string ListToString() {
        return String.Join(",", scoresSaved);
    }

    void AddScoreToList(int val) {
        scoresSaved.Add(m_currentScore);
        scoresSaved.Sort();
        scoresSaved.Reverse();
        scoresSaved = scoresSaved.GetRange(0, 10);
    }

    public void GetMyLeaderboard() {
        CoreGPGS.GetLeaderboard(GPGSIds.leaderboard_top_baton_test,
            score => {
                textFromLeaderboard.text = "Leaderboard: " + score.ToString();
            },
            () => {
                textFromLeaderboard.text = "No leaderboard score";
            }
        );
    }
#endregion
}