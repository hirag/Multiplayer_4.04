using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public float startTime = 5f; // カウントダウン開始時間（秒）
    private float timer;
    public GameObject timerScreen;

    void Start()
    {
        timer = startTime;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            countdownText.text = Mathf.Ceil(timer).ToString(); // テキストを更新
        }
        else
        {
            if (countdownText.text != "Go")
            {
                // カウントダウン終了時の処理
                countdownText.text = "Go";
                // Coroutineを開始して1秒後にtimerScreenを非表示にする
                StartCoroutine(HideTimerScreenAfterDelay(1f));

            }
        }
    }

    private IEnumerator HideTimerScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 1秒待つ
        timerScreen.SetActive(false); // timerScreenを非表示にする
    }
}
