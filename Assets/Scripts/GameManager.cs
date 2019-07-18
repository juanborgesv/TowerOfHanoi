using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Fields
    //TODO: Use "instance" instead of "gameManager"
    public static GameManager gameManager;

    [SerializeField]
    private GameObject[] _discs;
    private Dictionary<char, Stack> _towers = new Dictionary<char, Stack>
    {
        {'A', new Stack() },
        {'B', new Stack() },
        {'C', new Stack() }
    };

	private float _timeInMenu = 0;
	#endregion

	#region Properties
	public int CurrentMoves { get; set; }
    public int RemainingMoves { get; set; }
    public int Disks { get; set; } = 8;
    public bool IsAutoModeOn { get; set; } = false;
    public bool CanPlay { get; set; }
    #endregion

    #region Initializations
    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GetComponent<GameManager>();
        }

        Disks = PlayerPrefs.GetInt("Disks", 1);

		if (PlayerPrefs.GetInt("Auto", 0) == 0)
            IsAutoModeOn = false;
        else
            IsAutoModeOn = true;
    }
    #endregion

    public void SetUpGame()
    {
        _timeInMenu = Time.timeSinceLevelLoad;

        FillStartingTower();

		CurrentMoves = 0;
		CanPlay = true;

		if (IsAutoModeOn)
        {
            AutoSolver.GenerateAutoMoves(Disks);
            RemainingMoves = AutoSolver.CalculateTotalMoves(Disks);
        }

        UIManager.uIManager.ActivatePlaymodeUI();
    }

    private void FillStartingTower()
    {
        for (int i = 0; i < _discs.Length; i++)
        {
            if (i < Disks)
                _towers['A'].Push(_discs[i]);
            else
                _discs[i].SetActive(false);
        }
    }

	public GameObject PeekTower(char towerKey)
	{
		return (GameObject)_towers[towerKey].Peek();
	}

	public int GetCollectionCount(char towerKey)
    {
        return _towers[towerKey].Count;
    }

    public string GetPlayTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.timeSinceLevelLoad - _timeInMenu);
        StringBuilder stringBuilder = new StringBuilder("", 8);

        if (timeSpan.Minutes < 10)
            stringBuilder.Append('0');
        stringBuilder.Append(timeSpan.Minutes.ToString());

        stringBuilder.Append(':');

        if (timeSpan.Seconds < 10)
            stringBuilder.Append('0');
        stringBuilder.Append(timeSpan.Seconds.ToString());

        return stringBuilder.ToString();
    }

    // TODO: delete (already exists) and adapt ISTowerSelectedValid
    public char GetTowerKey(string towerTag)
    {
        if (towerTag == "TowerA")
            return 'A';
        else if (towerTag == "TowerB")
            return 'B';
        else
            return 'C';
    }

    public bool IsTowerSelectedValid(RaycastHit hit, bool isMoveFrom, char moveFromKey)
    {
        char towerKey = GetTowerKey(hit.collider.tag);

        // If it is the tower the disc is going to move from,
        // then check if the tower selected has discs.
        if (isMoveFrom)
        {
            if (GetCollectionCount(towerKey) > 0)
                return true;
            else
            {
                Debug.Log("Invalid selection. Tower has no discs.");
                return false;
            }
        }
        else
        {
            // If it is the tower the disc is goint to move to,
            // then check if the tower selected is not the same
            // has the tower it is moving from.
            if (towerKey == moveFromKey)
                return false;
            else
                return true;
        }
    }

    public bool IsMoveSelectedValid(char moveFromKey, char moveToKey)
    {
        // If MoveTo tower have discs compare top disc ranks, and evaluate
        // If it is possible based on MoveFrom Top Disc can not have greater
        // rank than MoveTo Top Disc.
        if (_towers[moveToKey].Count > 0)
        {
            GameObject moveFromTopDisc = (GameObject)_towers[moveFromKey].Peek();
            int moveFromRank = moveFromTopDisc.GetComponent<DiskRank>().rank;

            GameObject moveToTopDisc = (GameObject)_towers[moveToKey].Peek();
            int moveToRank = moveToTopDisc.GetComponent<DiskRank>().rank;

            if (moveFromRank < moveToRank)
                return true;
            else
                return false;
        }

        return true;
    }

    public GameObject MoveTopDisk(char moveFromKey, char moveToKey)
    {
        // Move the disc GameObject from one stack to another
        GameObject disk = (GameObject)_towers[moveFromKey].Pop();
        _towers[moveToKey].Push(disk);

        return disk;
    }

    public bool HasLevelFinished()
    {
        if (_towers['A'].Count == 0 && _towers['B'].Count == 0)
            return true;

        return false;
    }
}
