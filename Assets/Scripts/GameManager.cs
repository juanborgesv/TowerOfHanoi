using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager gameManager;

    [SerializeField]
    private GameObject[] _discs;

    private int _playmodeDiscs = 8;

    private bool _isAutoModeOn = false;

    private Dictionary<char, Stack> _towers = new Dictionary<char, Stack>()
    {
        {'A', new Stack() },
        {'B', new Stack() },
        {'C', new Stack() }
    };

    private int _currentMoves;
    private int _movesRemaining;

    private Queue<char[]> _autoMoves = new Queue<char[]>();

    #region Discs Controller Variables

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Transform[] _moveTargets;

    [SerializeField]
    private float _moveSpeed;

    private bool _canPlay;
    private bool _isMoving;

    private bool _canSaveMoveFromKey;
    private char _moveFromKey;
    private bool _canSaveMoveToKey;
    private char _moveToKey;

    // Moving disc components
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private Transform _transform;

    private int[] _moveTargetsIndex = new int[2];
    private int _index = 0;

    #endregion


    #endregion

    #region Initializations
    private void Awake()
    {
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();

        _currentMoves = 0;
        _movesRemaining = 0;
    }
    #endregion

    private void Update()
    {
        if (_canPlay)
        {
            if (!_isMoving)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        AudioManager.audioManager.PlayTowerTouched();

                        if (_canSaveMoveFromKey)
                        {
                            Debug.Log("canSaveMoveFromKey");
                            if (IsTowerSelectedValid(hit, true, _moveFromKey))
                            {
                                Debug.Log("Tower selected is valid.");
                                _moveFromKey = GetTowerKey(hit.collider.tag);
                            }
                        }
                        else if (_canSaveMoveToKey)
                        {
                            if (IsTowerSelectedValid(hit, false, _moveFromKey))
                            {
                                _moveToKey = GetTowerKey(hit.collider.tag);

                                if (IsMoveSelectedValid(_moveFromKey, _moveToKey))
                                {
                                    // Allow the disc to move, and don't get user input
                                    // while it is still moving.
                                    _isMoving = true;

                                    // Set up the moving disc components
                                    _gameObject = MoveTopDiscInStacks(_moveFromKey, _moveToKey);
                                    _transform = _gameObject.GetComponent<Transform>();
                                    _rigidbody = _gameObject.GetComponent<Rigidbody>();

                                    _rigidbody.useGravity = false;
                                    _rigidbody.isKinematic = true;

                                    SetMoveTargetsIndex();

                                    //TODO: increase currentMoves and decrease remainingMoves

                                    if (HasLevelFinished())
                                    {
                                        //uIManager.ShowLevelCompletePanel();
                                        _canPlay = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MoveDisc();
            }
        }
    }

    public void SetUpData()
	{
        Debug.Log("Playmode Discs: " + _playmodeDiscs);

		FillStartingTower();
        Debug.Log("Tower A Count: " + _towers['A'].Count);

		GameSolver(_playmodeDiscs, 'A', 'C', 'B');
        Debug.Log("AutoMoves Count: " + _autoMoves.Count);

		_movesRemaining = (int)CalculateTotalMoves();
        Debug.Log("Moves needed: " + _movesRemaining);
        
	}

    private void FillStartingTower()
	{
		for(int i = 0; i < _discs.Length; i++)
        {
			if (i < _playmodeDiscs)
				_towers['A'].Push(_discs[i]);
			else
				_discs[i].SetActive(false);
		}
	}

    private void GameSolver(int amountOfDiscs, char towerA, char towerC, char towerB)
    {
        if (amountOfDiscs == 1)
        {
            char[] moves = { towerA, towerC };
            _autoMoves.Enqueue(moves);

            return;
        }

        GameSolver(amountOfDiscs - 1, towerA, towerB, towerC);

        char[] moves1 = { towerA, towerC };
        _autoMoves.Enqueue(moves1);

        GameSolver(amountOfDiscs - 1, towerB, towerC, towerA);
    }

    private float CalculateTotalMoves()
    {
        return Mathf.Pow(2f, (float)_playmodeDiscs) - 1f;
    }

    public int GetPlaymodeDiscs()
    {
        return _playmodeDiscs;
    }

    public void SetPlaymodeDiscs(int i)
	{
		_playmodeDiscs = i;
	}

    public bool GetIsAutoModeOn()
    {
        return _isAutoModeOn;
    }

    public int GetCollectionCount(char towerKey)
    {
        return _towers[towerKey].Count;
    }

    #region Disc Moves Verifications

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
            int moveFromRank = moveFromTopDisc.GetComponent<DiscRank>().rank;

            GameObject moveToTopDisc = (GameObject)_towers[moveToKey].Peek();
            int moveToRank = moveToTopDisc.GetComponent<DiscRank>().rank;

            if (moveFromRank < moveToRank)
                return true;
            else
                return false;
        }

        return true;
    }

    public GameObject MoveTopDiscInStacks(char moveFromKey, char moveToKey)
    {
        // Move the disc GameObject from one stack to another
        GameObject disc = (GameObject)_towers[moveFromKey].Pop();
        _towers[moveToKey].Push(disc);

        return disc;
    }

    public bool HasLevelFinished()
    {
        if (_towers['A'].Count == 0 && _towers['B'].Count == 0)
            return true;

        return false;
    }
    #endregion

    #region DiscsController methods
    // Allow player raycast from camera when the click to play button is pressed.
    public void LetPlayerPlay()
    {
        _canPlay = true;
    }

    // The disc is going to move to two position; first above its own tower, then above
    // the MoveTo tower.
    private void SetMoveTargetsIndex()
    {
        if (_moveFromKey == 'A')
            _moveTargetsIndex[0] = 0;
        else if (_moveFromKey == 'B')
            _moveTargetsIndex[0] = 1;
        else
            _moveTargetsIndex[0] = 2;

        if (_moveToKey == 'A')
            _moveTargetsIndex[1] = 0;
        else if (_moveFromKey == 'B')
            _moveTargetsIndex[1] = 1;
        else
            _moveTargetsIndex[1] = 2;
    }

    private void MoveDisc()
    {
        _transform.position = Vector3.MoveTowards(
            _transform.position,
            _moveTargets[_moveTargetsIndex[_index]].position,
            Time.deltaTime * _moveSpeed);

        if (Vector3.Distance(_moveTargets[_moveTargetsIndex[_index]].position, _transform.position) <= 0)
        {
            if (_index == 1)
            {
                _isMoving = false;
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = true;

                _canSaveMoveFromKey = true;
                _canSaveMoveToKey = true;
            }
            else
            {
                _index++;
            }
        }
    }
    #endregion
}
