using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public float spawnInterval = 5f;
    public float spawnRadius = 1f;  // アイテムを探す半径
    public Vector3 spawnAreaMin;    // スポーンエリアの最小座標
    public Vector3 spawnAreaMax;    // スポーンエリアの最大座標
    public LayerMask ignoreLayerMask;  // 無視するレイヤーのマスク

    

    void Start()
    {
        StartCoroutine(WaitAndSpawnItems());
    }

    private IEnumerator WaitAndSpawnItems()
    {
        // クライアントが部屋に参加するまで待つ
        while (!PhotonNetwork.InRoom)
        {
            yield return null; // 次のフレームまで待つ
        }

        // 部屋に参加している場合のみアイテムをスポーン
        StartCoroutine(SpawnItems());
    }

    private IEnumerator SpawnItems()
    {
        while (true)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 potentialPosition = Vector3.zero;
        bool validPosition = false;

        while (!validPosition)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);

            potentialPosition = new Vector3(x, y, z);

            // その位置に他のオブジェクトがないことを確認
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, spawnRadius, ~ignoreLayerMask);
            validPosition = colliders.Length == 0;
        }

        return potentialPosition;
    }
}
