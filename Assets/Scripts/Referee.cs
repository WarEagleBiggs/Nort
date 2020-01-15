using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

enum GameState {
    CountDownState = 0,
    PlayingState,
    PausedState,
    GameOverState
}


public class Referee : MonoBehaviour
{
    private GameState m_GameState = GameState.CountDownState;

    public Player m_PlayerA;
    public Player m_PlayerB;
    public GameObject m_ReplayButton;
    public TMPro.TMP_Text m_ScoreTm;
    public int m_ScoreGoal = 5;

    private float m_InitialTime;
    public const float c_CountDownSec = 2.0f;


    private void HideRestartButton()
    {
        if (m_ReplayButton != null) {
            // initially ensure replay button is not active 
            m_ReplayButton.SetActive(false);
        }
    }
    

    private void Start()
    {
        HideRestartButton();
        m_InitialTime = Time.time;

        // initial score text
        m_ScoreTm.text = "0-0";
    }

    // Update is called once per frame
    void Update()
    {
        MonitorGameState();

        UpdateScore();

    }

    private void UpdateScore()
    {
        string scoreStr = "";

        scoreStr += m_PlayerA.m_Score.ToString();
        scoreStr += "-";
        scoreStr += m_PlayerB.m_Score.ToString();

        m_ScoreTm.text = scoreStr;

        if (m_PlayerA.m_Score >= m_ScoreGoal || m_PlayerB.m_Score >= m_ScoreGoal) {
            SceneController.GotoMenu();

        }
        //int numGames = m_PlayerA.m_Score + m_PlayerB.m_Score;

        //if (numGames >= 10) {
        //    SceneController.GotoMenu();
        //}

    }

    private void MonitorGameState()
    {
        switch (m_GameState) {
            case GameState.CountDownState:
                if (Time.time > (m_InitialTime + c_CountDownSec)) {
                    // transition to playing state
                    m_GameState = GameState.PlayingState;
                    // start players
                    StartGameplay();
                }
                break;
            case GameState.PlayingState:

                if (m_PlayerA.IsLiving == false) {
                    m_PlayerB.m_Score += 1;
                    m_GameState = GameState.GameOverState;
                }

                if (m_PlayerB.IsLiving == false) {
                    m_PlayerA.m_Score += 1;
                    m_GameState = GameState.GameOverState;
                }

                break;

            case GameState.GameOverState:
                // ensure players are stopped
                StopGameplay();


                if (m_ReplayButton != null) {
                    // show replay button
                    m_ReplayButton.SetActive(true);
                }

                break;
            case GameState.PausedState:
                // TODO
                break;
        }
    }

        private void StartGameplay()
    {
        // --- tell players to start ---

        m_PlayerA.m_IsPlaying = true;
        m_PlayerB.m_IsPlaying = true;

    }

    private void StopGameplay()
    {
        // --- tell players to stop ---

        m_PlayerA.m_IsPlaying = false;
        m_PlayerB.m_IsPlaying = false;
    }

    public void OnRestart()
    {
        m_GameState = GameState.CountDownState;
        m_InitialTime = Time.time;
        
        HideRestartButton();

        m_PlayerA.Restart();
        m_PlayerB.Restart();
        
    }

}
