using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager uIManager;

    // Indicates how many discs are being configured by the Slider
    [Header("Configuration Panel")]
    [SerializeField]
    private TextMeshProUGUI _disksSliderValue;
    [SerializeField]
    private Slider _disksSlider;
    [SerializeField]
    private Toggle _autoToggle;

    [Header("Playmode Indicators")]
    [SerializeField]
    private GameObject _currentMovesGO;
    [SerializeField]
    private TextMeshProUGUI _currentMovesTMP;
    [SerializeField]
    private GameObject _remainingMovesGO;
    [SerializeField]
    private TextMeshProUGUI _remainingMovesTMP;

    [Space, SerializeField]
    private GameObject _levelCompletePanel;
    [SerializeField]
    private GameObject _levelCompleteTip;

    [Space, SerializeField]
    private GameObject _menuButton;

    [SerializeField]
    private Animator _invalidMoveAnimator;

    [Header("Level Complete Panel")]

    [SerializeField]
    private TextMeshProUGUI _movesText;
    [SerializeField]
    private TextMeshProUGUI _timeText;

    private void Awake()
    {
        if (uIManager == null)
            uIManager = GetComponent<UIManager>();
    }

    private void Start()
    {
        _disksSlider.value = PlayerPrefs.GetInt("Disks", 1);

        AudioManager.audioManager.UpdateAudioIcon(false);

        if (PlayerPrefs.GetInt("Auto", 0) == 0)
            _autoToggle.isOn = false;
        else
            _autoToggle.isOn = true;
    }

    #region Settings Methods
    // If slider value changes, then update its value indicator
    // and save it in PlayerPrefs
    public void UpdateDisksSliderIndicator()
    {
        _disksSliderValue.text = _disksSlider.value.ToString();

        GameManager.gameManager.Disks = (int)_disksSlider.value;

        PlayerPrefs.SetInt("Disks", (int)_disksSlider.value);
    }

    // If the Auto Toggle changes, then update AutoMode in
    // GameManager and save it in PlayerPrefs
    public void ChangeAutoModeState()
    {
        GameManager.gameManager.IsAutoModeOn = _autoToggle.isOn;

        if (GameManager.gameManager.IsAutoModeOn)
            PlayerPrefs.SetInt("Auto", 1);
        else
            PlayerPrefs.SetInt("Auto", 0);
    }
    #endregion

    #region Playmode Methods
    // Each time the player moves a disc, update the current moves text
    public void UpdateCurrentMoves(int currentMoves)
    {
        _currentMovesTMP.text = currentMoves.ToString();
    }

    // Each time the auto player moves a disc, update the remaining moves
    public void UpdateRemainingMoves(int remainingMoves)
    {
        _remainingMovesTMP.text = "Remaining: " + remainingMoves.ToString();
    }

    public void ActivatePlaymodeUI()
    {
        _currentMovesGO.SetActive(true);

        if (GameManager.gameManager.IsAutoModeOn)
            _remainingMovesGO.SetActive(true);
    }

    public void ShowLevelCompletePanel()
    {
        SetLevelCompleteStats();

       if (IsTheFirstLevelComplete())
            _levelCompleteTip.SetActive(true);
       else
            _levelCompletePanel.SetActive(true);
    }

    public void SetLevelCompleteStats()
    {
        _movesText.text = GameManager.gameManager.CurrentMoves.ToString();
        _timeText.text = GameManager.gameManager.GetPlayTime();
    }

    public void HidePlaymodeUI()
    {
        _currentMovesGO.SetActive(false);
        _menuButton.SetActive(false);

        if (GameManager.gameManager.IsAutoModeOn)
            _remainingMovesGO.SetActive(false);
    }

    public void ShowInvalidMoveText()
    {
        _invalidMoveAnimator.Play("InvalidMove");
    }
    #endregion

    private bool IsTheFirstLevelComplete()
    {
        int isTheFirstTime = PlayerPrefs.GetInt("IsTheFirstTime", 0);

        if (isTheFirstTime == 0)
        {
            PlayerPrefs.SetInt("IsTheFirstTime", 1);
            return true;
        }
        else
        {
            return false;
        }
    }
}
