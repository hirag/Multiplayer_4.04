using Photon.Pun;
using UnityEngine;

public class BoardButton : MonoBehaviourPun
{
    public bool gameStart = false;
    public GameObject countdown;
    public GameObject leaderboard;
    public GameObject startButton;
    public GameObject exitButton;
    public GameObject spawnItems;

    public void OnButtonToStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        leaderboard.SetActive(false);
        startButton.SetActive(false);
        exitButton.SetActive(false);

        if (countdown == null)
        {
            Debug.LogError("Countdown GameObject is not assigned.");
            return;
        }

        // RPCを呼び出して全クライアントでカウントダウンを表示
        photonView.RPC("ActivateCountdown", RpcTarget.All);
    }

    public void OnButtonToExit()
    {
        Application.Quit();
    }

    [PunRPC]
    void ActivateCountdown()
    {
        gameStart = true;
        spawnItems.SetActive(true);
        countdown.SetActive(true);
    }
}
