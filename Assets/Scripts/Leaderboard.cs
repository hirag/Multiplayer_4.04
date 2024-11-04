using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class Leaderboard : MonoBehaviour
{
    public InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;
    private InputAction playerPauseAction;
    private InputAction uiPauseAction;
    private InputAction moveCursorAction;
    private InputAction pushAction;

    public GameObject playersHolder;
    public GameObject startButton;
    public GameObject exitButton;
    public float refreshRate = 1f;
    public GameObject[] slots;
    public TextMeshProUGUI[] scoreTexts;
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] KDTexts;

    private bool isPlayerVisible = false;
    private Vector2 cursorPosition;
    public Image cursor;

    private void Awake()
    {
        playerActionMap = inputActionAsset.FindActionMap("Player");
        uiActionMap = inputActionAsset.FindActionMap("UI");

        playerPauseAction = playerActionMap.FindAction("Pause");
        uiPauseAction = uiActionMap.FindAction("Pause");
        moveCursorAction = uiActionMap.FindAction("Navigate");
        pushAction = uiActionMap.FindAction("Push");
    }

    private void OnEnable()
    {
        playerPauseAction.Enable();
        playerPauseAction.performed += TogglePlayerVisibility;

        uiPauseAction.Enable();
        uiPauseAction.performed += TogglePlayerVisibility;

        moveCursorAction.Enable();
        pushAction.Enable();
        pushAction.performed += OnClick;
    }

    private void OnDisable()
    {
        playerPauseAction.performed -= TogglePlayerVisibility;
        playerPauseAction.Disable();

        uiPauseAction.performed -= TogglePlayerVisibility;
        uiPauseAction.Disable();

        moveCursorAction.Disable();
        pushAction.performed -= OnClick;
        pushAction.Disable();
    }

    private void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
        cursorPosition = cursor.rectTransform.anchoredPosition;
        cursor.gameObject.SetActive(false); // Start with the cursor hidden
    }

    public void Refresh()
    {
        if (slots == null || nameTexts == null || scoreTexts == null || KDTexts == null)
            return;

        foreach (var slot in slots)
            slot.SetActive(false);

        var sortedPlayerList = PhotonNetwork.PlayerList.OrderByDescending(player => player.GetScore()).ToList();

        for (int i = 0; i < sortedPlayerList.Count && i < slots.Length; i++)
        {
            var player = sortedPlayerList[i];
            slots[i].SetActive(true);
            nameTexts[i].text = string.IsNullOrEmpty(player.NickName) ? "unnamed" : player.NickName;
            scoreTexts[i].text = player.GetScore().ToString();

            if (player.CustomProperties.TryGetValue("kills", out object kills) && player.CustomProperties.TryGetValue("deaths", out object deaths))
                KDTexts[i].text = $"{kills}/{deaths}";
            else
                KDTexts[i].text = "0/0";
        }
    }

    private void Update()
    {
        if (uiActionMap.enabled)
        {
            Vector2 moveInput = moveCursorAction.ReadValue<Vector2>();
            cursorPosition += moveInput * Time.deltaTime * 100f; // Adjust speed as needed
            cursorPosition = new Vector2(
                Mathf.Clamp(cursorPosition.x, -Screen.width, Screen.width),
                Mathf.Clamp(cursorPosition.y, -Screen.height, Screen.height)
            );

            cursor.rectTransform.anchoredPosition = cursorPosition;
        }
    }

    private void TogglePlayerVisibility(InputAction.CallbackContext context)
    {
        if (PhotonNetwork.LocalPlayer.IsLocal)  // これによりローカルプレイヤーのみがUIを制御
        {
            if (playerActionMap.enabled)
            {
                // Switch to UI map
                isPlayerVisible = true;
                playersHolder.SetActive(isPlayerVisible);
                startButton.SetActive(isPlayerVisible);
                exitButton.SetActive(isPlayerVisible);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                cursor.gameObject.SetActive(true); // Show the cursor
                playerActionMap.Disable();
                uiActionMap.Enable();
            }
            else if (uiActionMap.enabled)
            {
                // Switch to Player map
                isPlayerVisible = false;
                playersHolder.SetActive(isPlayerVisible);
                startButton.SetActive(isPlayerVisible);
                exitButton.SetActive(isPlayerVisible);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursor.gameObject.SetActive(false); // Hide the cursor
                uiActionMap.Disable();
                playerActionMap.Enable();
            }
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (PhotonNetwork.LocalPlayer.IsLocal)  // これによりローカルプレイヤーのみがクリックイベントを処理
        {
            // Determine the cursor's position and simulate a click
            Vector2 cursorScreenPosition = cursor.rectTransform.anchoredPosition;

            // Convert the cursor's anchored position to screen position
            Vector2 screenPoint = new Vector2(cursorScreenPosition.x + Screen.width / 2, cursorScreenPosition.y + Screen.height / 2);

            // Check if the cursor is over the start button
            if (RectTransformUtility.RectangleContainsScreenPoint(startButton.GetComponent<RectTransform>(), screenPoint))
            {
                startButton.GetComponent<Button>().onClick.Invoke();
                isPlayerVisible = false;
                playersHolder.SetActive(isPlayerVisible);
                startButton.SetActive(isPlayerVisible);
                exitButton.SetActive(isPlayerVisible);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursor.gameObject.SetActive(false); // Hide the cursor
                uiActionMap.Disable();
                playerActionMap.Enable();
            }

            // Check if the cursor is over the exit button
            if (RectTransformUtility.RectangleContainsScreenPoint(exitButton.GetComponent<RectTransform>(), screenPoint))
            {
                exitButton.GetComponent<Button>().onClick.Invoke();
                isPlayerVisible = false;
                playersHolder.SetActive(isPlayerVisible);
                startButton.SetActive(isPlayerVisible);
                exitButton.SetActive(isPlayerVisible);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursor.gameObject.SetActive(false); // Hide the cursor
                uiActionMap.Disable();
                playerActionMap.Enable();
            }

            // Add similar checks for other buttons as needed
        }
    }
}
