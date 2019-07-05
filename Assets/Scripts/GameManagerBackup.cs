using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerBackup : MonoBehaviour
{
    #region Variables
    //public static GameManager gameManager;

    [SerializeField]
    private Camera _camera;

    // 0-Move to A, 1-Move to B, 2-Move to C
    [SerializeField]
    Transform[] _moveTargets;

    [SerializeField]
    private GameObject[] _discs;

    [SerializeField]
    private static int _playmodeDiscs = 8;

    [SerializeField]
    private float _discMoveSpeed = 100f;

    [SerializeField]
    private GameObject levelCompletePanel;

    [SerializeField]
    private static bool _isAutoModeOn = false;

    private Dictionary<char, Stack> _towers = new Dictionary<char, Stack>()
    {
        {'A', new Stack() },
        {'B', new Stack() },
        {'C', new Stack() }
    };

    private bool _canSelectOrigin = true;
    private bool _canSelectDestination = true;

    // Indicates which tower is going to move from, and where it is going.
    private char _moveFrom; // N: null
    private char _moveTo; // N: null

    private bool _isDiscMoving = false;

    // Moving disc components
    private Rigidbody _rigidbody;
    private Transform _transform;

    private int[] _moveTargetsIndex;
    private int _waypointIndex;

    private int _currentMoves;
    private int _movesRemaining;

    private Queue<char[]> _autoMoves = new Queue<char[]>();

	private bool _isGameReady;
    #endregion

    #region Initializations
    private void Awake()
    {
        //if (gameManager == null)
            //gameManager = GetComponent<GameManager>();

        _isGameReady = false;

        _currentMoves = 0;
        _movesRemaining = 0;
        
        _waypointIndex = 0;
        _moveTargetsIndex = new int[2];
    }
    #endregion

    private void Start()
    {
        if (_discs.Length > 0)
        {
            for (int i = 0; i < _discs.Length; i++)
            {
                if (i < _playmodeDiscs)
                    _towers['A'].Push(_discs[i]);
                else
                    _discs[i].SetActive(false);
            }
        }
        else
            Debug.LogError("Discs not specified.");

        Debug.Log("Awake end. Initial tower count: " + _towers['A'].Count);

        _movesRemaining = (int)CalculateTotalMoves();

        GameSolver(_playmodeDiscs, 'A', 'C', 'B');

        foreach (var move in _autoMoves)
        {
            Debug.Log("From " + move[0] + " to " + move[1]);
        }
    }

    private void Update()
    {
        if (_isGameReady)
		{
            if (!_isDiscMoving)
            {
                if (_isAutoModeOn)
                {
                    if (_autoMoves.Count > 0)
                    {
                        Debug.Log("Moves count: " + _autoMoves.Count);
                        char[] moves = _autoMoves.Dequeue();
                        _moveFrom = moves[0];
                        _moveTo = moves[1];

                        MoveTopuDisc();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        // If the ray hits something...
                        if(Physics.Raycast(ray, out hit))
                        {
                            if (DidHitATower(hit))
                            {
                                AudioManager.audioManager.PlayTowerTouched();

                                if (_canSelectOrigin)
                                {
                                    // Validate if the origin tower is empty.
                                    CheckMoveTower(hit, ref _moveFrom, true);
                                }
                                else if (_canSelectDestination)
                                {
                                    CheckMoveTower(hit, ref _moveTo, false);

                                    if (_moveFrom == _moveTo)
                                    {
                                        Debug.Log("INVALID MOVE, ORIGIN == DESTINATION");
                                        ResetSelections();
                                    }
                                    else
                                        MoveTopuDisc();
                                }
                                else
                                {
                                    Debug.Log("Can't move yet.");
                                }
                            }
                            else if(hit.collider.CompareTag("Background"))
                            {
                                ResetSelections();
                            }
                        }
                    }
                }
            }
            else
            {
                Movement();
            }
		}
    }

    private bool DidHitATower(RaycastHit hit)
    {
        Debug.Log("DidHitATower init");
        if (hit.collider.CompareTag("TowerA") ||
            hit.collider.CompareTag("TowerB") ||
            hit.collider.CompareTag("TowerC"))
            return true;

        return false;
    }

    // If tower selected is the origin tower and empty, then don't save the tower key
    // and log an error message. Else, store its key.
    private void CheckMoveTower(RaycastHit hit, ref char tower, bool isOrigin)
    {
        Debug.Log("CheckMoveTower init");

        if (hit.collider.CompareTag("TowerA"))
        {
            if (isOrigin && IsTowerEmpty('A'))
                return;

            tower = 'A';
        }
        else if (hit.collider.CompareTag("TowerB"))
        {
            if (isOrigin && IsTowerEmpty('B'))
                return;

            tower = 'B';
        }
        else if (hit.collider.CompareTag("TowerC"))
        {
            if (isOrigin && IsTowerEmpty('C'))
                return;

            tower = 'C';
        }

        if (isOrigin)
            _canSelectOrigin = false;
        else
            _canSelectDestination = false;
    }

    // 
    private bool IsMovePossible()
    {
        Debug.Log("IsMovePossible init");

        GameObject topOrigin = (GameObject)_towers[_moveFrom].Peek();
        int originRank = topOrigin.GetComponent<DiscRank>().rank;

        // If tower destination does not have discs, return true;
        if (_towers[_moveTo].Count > 0)
        {
            GameObject topDestination = (GameObject)_towers[_moveTo].Peek();
            int destinationRank = topDestination.GetComponent<DiscRank>().rank;

            if (originRank < destinationRank)
                return true;

            return false;
        }

        return true;
    }

    private bool IsTowerEmpty(char towerKey)
    {
        Debug.Log("IsTowerEmpty init.");
        if (_towers[towerKey].Count == 0)
        {
            Debug.Log("Tower " + towerKey + " is empty. Invalid move.");
            return true;
        }
        return false;
    }

    private void ResetSelections()
    {
        _canSelectOrigin = true;
        _canSelectDestination = true;
    }

    private void Movement()
    {
        // Move towards waypoint.
        _transform.position = Vector3.MoveTowards(_transform.position,
            _moveTargets[_moveTargetsIndex[_waypointIndex]].position, _discMoveSpeed * Time.deltaTime);

        if (Vector3.Distance(_moveTargets[_moveTargetsIndex[_waypointIndex]].position, _transform.position) <= 0)
        {
            
            _waypointIndex++;

            if (_waypointIndex >= 2)
            {
                _isDiscMoving = false;
                _rigidbody.useGravity = true;
                _rigidbody.isKinematic = false;
                _waypointIndex = 0;

                ResetSelections();
                //_time = Time.time + _waitTime;
                return;
            }
        }
    }

    private void SetTargets()
    {
        if (_moveFrom == 'A')
            _moveTargetsIndex[0] = 0;
        else if (_moveFrom == 'B')
            _moveTargetsIndex[0] = 1;
        else if (_moveFrom == 'C')
            _moveTargetsIndex[0] = 2;

        if (_moveTo == 'A')
            _moveTargetsIndex[1] = 0;
        else if (_moveTo == 'B')
            _moveTargetsIndex[1] = 1;
        else if (_moveTo == 'C')
            _moveTargetsIndex[1] = 2;
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

    public void UpdateDiscsData()
	{
		if (_discs.Length > 0)
		{
			for (int i = 0; i < _discs.Length; i++)
			{
				if (i < _playmodeDiscs)
				{
					_towers['A'].Push(_discs[i]);
					_discs[i].SetActive(true);
				}
				else
					_discs[i].SetActive(false);
			}
		}
		else
			Debug.LogError("Discs not specified.");

		Debug.Log("Awake end. Initial tower count: " + _towers['A'].Count);

		_movesRemaining = (int)CalculateTotalMoves();

		GameSolver(_playmodeDiscs, 'A', 'C', 'B');
        
	}

    public void SaveGameConfiguration(int playmodeDiscs, bool isAutoModeOn)
    {
        _playmodeDiscs = playmodeDiscs;
        _isAutoModeOn = isAutoModeOn;
    }

    public int GetPlaymodeDiscs()
    {
        return _playmodeDiscs;
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

    private void MoveTopuDisc()
    {
        Debug.Log("MoveTopDisc init");

        if (IsMovePossible())
        {
            Debug.Log("Moving top disc from tower" + _moveFrom + " to tower " + _moveTo);

            _isDiscMoving = true;

            GameObject movingDisc = (GameObject)_towers[_moveFrom].Pop();
            _towers[_moveTo].Push(movingDisc);

            _transform = movingDisc.GetComponent<Transform>();

            _rigidbody = movingDisc.GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;

            SetTargets();

            _currentMoves++;
            //_currentMovesText.text = _currentMoves.ToString();
            _movesRemaining--;
            //movesRemainingText.text = "Remaining: " + _movesRemaining.ToString();

            if (_towers['A'].Count == 0 && _towers['B'].Count == 0)
                levelCompletePanel.SetActive(true);

        }
        else
        {
            ResetSelections();
            Debug.Log("Invalid move.");
        }
    }
    #endregion

    //  public void StartGame()
    //  {
    //_isGameReady = true;

    //      if (_autoPlay == false)
    //          _remainingIndicator.SetActive(false);
    //      else
    //          _remainingIndicator.SetActive(true);
    //  }
}
