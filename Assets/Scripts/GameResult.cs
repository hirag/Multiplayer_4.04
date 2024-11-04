using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Photon.Pun.UtilityScripts;

public class GameResult : MonoBehaviourPunCallbacks
{
    public int winningScore = 2000;
    public GameObject winCanvas;
    public GameObject loseCanvas;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI loseText;
    public float delayBeforeShowingResult = 2f;

    private bool gameEnded = false;

    private void Update()
    {
        if (gameEnded) return;

        var playerList = PhotonNetwork.PlayerList;
        foreach (var player in playerList)
        {
            int playerScore = player.GetScore(); // GetScoreを使用

            if (playerScore >= winningScore)
            {
                EndGame(player);
                break;
            }
        }
    }

    private void EndGame(Player winningPlayer)
    {
        gameEnded = true;
        Invoke(nameof(ShowResults), delayBeforeShowingResult);
    }

    private void ShowResults()
    {
        var localPlayer = PhotonNetwork.LocalPlayer;
        if (localPlayer.GetScore() >= winningScore)
        {
            winCanvas.SetActive(true);
            winText.text = "You Win!";
        }
        else
        {
            loseCanvas.SetActive(true);
            loseText.text = "You Lose!";
        }
    }

    public override void OnLeftRoom()
    {
        winCanvas.SetActive(false);
        loseCanvas.SetActive(false);
    }
}
