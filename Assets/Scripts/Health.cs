using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public bool isLocalPlayer;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public Image healthCircle;

    [Header("VFX")]
    public GameObject magicDisappearEffectPrefab; // 魔法消滅エフェクトのプレハブ

    private void Start()
    {
        SetHP();
    }

    private void Update()
    {
        SetHP();
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        health -= damage;
        SetHP();

        if (health <= 0)
        {
            Die();
        }
    }

    private void SetHP()
    {
        healthText.text = health.ToString();
        healthCircle.fillAmount = (float)health / maxHealth;
    }

    public void Die()
    {
        // 魔法消滅エフェクトを全クライアントで生成
        PhotonNetwork.Instantiate(magicDisappearEffectPrefab.name, transform.position, Quaternion.identity);

        // オブジェクトの削除
        Destroy(gameObject);

        if (isLocalPlayer)
        {
            RoomManager.instance.SpawnPlayer();
            RoomManager.instance.deaths++;
            RoomManager.instance.SetHashes();
        }
    }
}
