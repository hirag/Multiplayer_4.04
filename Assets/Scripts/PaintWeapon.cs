using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PaintWeapon : MonoBehaviour
{
    public int damage;
    public Camera camera;
    public float fireRate;
    private float nextFire;

    [Header("VFX")]
    public GameObject hitVFX;

    [Header("Ammo")]
    public int mag = 5;
    public int ammo = 30;
    public int magAmmo = 30;

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;
    public Image ammoCircle;

    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;

    [Header("Recoil Settings")]
    [Range(0f, 2f)]
    public float recoverPercent = 0.7f;

    [Space]
    public float recoilUp = 1f;
    public float recoilBack = 0f;

    private Vector3 originalPosition;
    private Vector3 recoilVelocity = Vector3.zero;

    private float recoilLength;
    private float recoverLength;

    private bool recoiling;
    public bool recovering;

    // Input System用の変数
    public InputActionAsset inputActionAsset;  // インスペクターから設定する
    private InputAction fireAction;
    private InputAction reloadAction;

    private void Awake()
    {
        // Input Actionの設定
        var playerActions = inputActionAsset.FindActionMap("Player");
        fireAction = playerActions.FindAction("Fire");
        reloadAction = playerActions.FindAction("Reload");
    }

    private void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        SetAmmo();

        originalPosition = transform.localPosition;

        recoilLength = 0;
        recoverLength = 1 / fireRate * recoverPercent;
    }

    private void OnEnable()
    {
        fireAction.performed += OnFirePerformed;
        fireAction.canceled += OnFireCanceled;
        reloadAction.performed += OnReload;
    }

    private void OnDisable()
    {
        fireAction.performed -= OnFirePerformed;
        fireAction.canceled -= OnFireCanceled;
        reloadAction.performed -= OnReload;
    }

    private void Update()
    {
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        if (recoiling)
        {
            Recoil();
        }

        if (recovering)
        {
            Recovering();
        }

        // Fireアクションが押されている間、発射処理を行う
        if (fireAction.ReadValue<float>() > 0 && nextFire <= 0 && ammo > 0 && !animation.isPlaying)
        {
            nextFire = 1 / fireRate;
            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            SetAmmo();
            Fire();
        }
    }

    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        // このメソッドは必要ありませんが、アクションのperformedイベントをバインドする必要があります
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        // 連射を止めるために追加の処理を行うことができますが、この例では不要です
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (mag > 0)
        {
            Reload();
        }
    }

    private void Reload()
    {
        animation.Play(reload.name);
        if (mag > 0)
        {
            mag--;
            ammo = magAmmo;
            SetAmmo();
        }

        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
    }

    void Fire()
    {
        recoiling = true;
        recovering = false;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        RaycastHit hit;

        PhotonNetwork.LocalPlayer.AddScore(damage);

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            if (hit.transform.gameObject.TryGetComponent(out Health health))
            {
                PhotonNetwork.LocalPlayer.AddScore(damage);

                if (damage >= health.health)
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.SetHashes();

                    PhotonNetwork.LocalPlayer.AddScore(100);
                }

                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
            }
        }
    }

    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x, originalPosition.y + recoilUp, originalPosition.z - recoilBack);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLength);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = true;
        }
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLength);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = false;
        }
    }

    void SetAmmo()
    {
        ammoCircle.fillAmount = (float) ammo / magAmmo;
    }
}
