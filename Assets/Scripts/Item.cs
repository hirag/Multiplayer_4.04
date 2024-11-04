using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Item : MonoBehaviour
{
    public bool enableAimAssist = true;
    public float assistDuration = 10f;
    public float destroyDelay = 10f; // インスペクターから設定可能

    private Renderer itemRenderer;
    private Collider itemCollider;
    private PhotonView photonView;

    public bool itemObtained = false;

    [Header("Music")]
    public AudioClip diamondSound;
    AudioSource audioSource;

    void Awake()
    {
        itemRenderer = GetComponent<Renderer>();
        itemCollider = GetComponent<Collider>();
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        // 設定された時間後にオブジェクトを破壊
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(DestroyAfterDelay(destroyDelay));
    }

    void OnTriggerEnter(Collider other)
    {
        // トリガーを起こしたオブジェクトがローカルプレイヤーであるかどうかを確認
        PhotonView otherPhotonView = other.GetComponent<PhotonView>();
        if (otherPhotonView != null && otherPhotonView.IsMine)
        {
            audioSource.PlayOneShot(diamondSound);
            // アイテムを見えなくし、コライダーを無効にする
            DisableItem();

            // 子オブジェクトのMain Cameraにアクセス
            Transform cameraTransform = other.transform.Find("Main Camera");
            if (cameraTransform != null)
            {
                MouseLook cameraController = cameraTransform.GetComponent<MouseLook>();
                if (cameraController != null)
                {
                    // エイムアシストを有効にするコルーチンを開始
                    StartCoroutine(EnableAimAssistForDuration(cameraController));
                }
            }
        }
    }

    private void DisableItem()
    {
        itemRenderer.enabled = false;
        itemCollider.enabled = false;
        itemObtained = true;
    }

    private IEnumerator EnableAimAssistForDuration(MouseLook cameraController)
    {
        cameraController.ToggleAimAssist(enableAimAssist);
        yield return new WaitForSeconds(assistDuration);
        cameraController.ToggleAimAssist(!enableAimAssist);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        float elapsedTime = 0f;

        while (elapsedTime < delay)
        {
            if (itemObtained)
            {
                yield break; // コルーチンを終了してオブジェクトを破壊しない
            }

            elapsedTime += Time.deltaTime;
            yield return null; // 次のフレームまで待機
        }

        Destroy(gameObject); // 指定時間後にオブジェクトを破壊
    }
}
