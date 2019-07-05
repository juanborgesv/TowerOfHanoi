using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    //public static UIManager uIManager;
    [SerializeField]
    GameManager gameManager;

    // Indicates how many discs are being configured by the Slider
    [Header("Configuration Panel")]
    [SerializeField]
    private TextMeshProUGUI _discsSliderIndicator;
    [SerializeField]
    private Slider _discsSlider;
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

    private void Awake()
    {
        //if (uIManager == null)
          //  uIManager = GetComponent<UIManager>();

        //Debug.Log("GetPlaymodeDiscs: " + GameManager.gameManager.GetPlaymodeDiscs());
        //_discsSlider.value = GameManager.gameManager.GetPlaymodeDiscs();
        _autoToggle.isOn = gameManager.GetIsAutoModeOn();
    }

    // When slider value changes, update its value indicator.
    public void UpdateDiscsSliderIndicator()
    {
        _discsSliderIndicator.text = _discsSlider.value.ToString();
        gameManager.SetPlaymodeDiscs((int)_discsSlider.value);
    }

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

        if (gameManager.GetIsAutoModeOn())
            _remainingMovesGO.SetActive(true);
    }

    // Save game configuration from UI elements to game manager variables
    public void SaveGameConfiguration()
    {
        //GameManager.gameManager.SaveGameConfiguration((int)_discsSlider.value, _autoToggle.isOn);
    }

    public void ShowLevelCompletePanel()
    {
        _levelCompletePanel.SetActive(true);
    }
}
