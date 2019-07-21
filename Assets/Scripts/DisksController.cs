using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisksController : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Transform[] _moveTargets;
    
    [SerializeField]
    private float _moveSpeed;

    private bool _isMoving = false;

    private bool _canSaveMoveFromKey = true;
    private char _moveFromKey;
    private bool _canSaveMoveToKey = true;
    private char _moveToKey;

    // Disk components
    private GameObject _gameObject;
    private Rigidbody _rigidbody;
    private Transform _transform;

    private int[] _moveTargetsIndex = new int[2];
    private int _index = 0;

    private float _waitTime = 0;

    private bool _hasReachedSelectionPos = false;

    // Components of a selected disk.
    private Transform _selectionTransform;
    private Vector3 _selectionPosition;
    private Vector3 _deltaPosition = new Vector3(0f, 0.1f, 0f);
    [SerializeField]
    private float _risingSpeed = 1f;
    [SerializeField]
    private float _motionSpeed = 5f;

    private void Update()
    {
        if (GameManager.gameManager.CanPlay && !_isMoving)
        {
            if (GameManager.gameManager.IsAutoModeOn)
            {
                if (AutoSolver.GetAutoMovesCount() > 0 && Time.time >= _waitTime)
                {
                    Debug.Log("Auto Moves > 0 and Time > wait time");
                    char[] moves = AutoSolver.DequeueAutoMoves();
                    _moveFromKey = moves[0];
                    _moveToKey = moves[1];

                    _isMoving = true;

                    _hasReachedSelectionPos = false;

                    GameManager.gameManager.CurrentMoves++;
                    GameManager.gameManager.RemainingMoves--;

                    //TODO: Change UpdateCurrentMoves and UpdateRemainingMoves with Events.
                    // Update UI
                    UIManager.uIManager.UpdateCurrentMoves(GameManager.gameManager.CurrentMoves);
                    UIManager.uIManager.UpdateRemainingMoves(GameManager.gameManager.RemainingMoves);

                    // Set up the moving disk components
                    _gameObject = GameManager.gameManager.MoveTopDisk(_moveFromKey, _moveToKey);
                    _transform = _gameObject.GetComponent<Transform>();
                    _rigidbody = _gameObject.GetComponent<Rigidbody>();

                    _rigidbody.useGravity = false;
                    _rigidbody.isKinematic = true;

                    SetMoveTargetsIndex();

                    _waitTime = Time.time + 1.1f;

                    if (GameManager.gameManager.HasLevelFinished())
                    {
                        UIManager.uIManager.ShowLevelCompletePanel();
                        UIManager.uIManager.HidePlaymodeUI();
                        GameManager.gameManager.CanPlay = false;
                    }
                }
            }
            else
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
                            if (GameManager.gameManager.IsTowerSelectedValid(hit, true, _moveFromKey))
                            {
                                _moveFromKey = GameManager.gameManager.GetTowerKey(hit.collider.tag);

                                GameObject disk = GameManager.gameManager.PeekTower(_moveFromKey);
                                _selectionTransform = disk.GetComponent<Transform>();
                                _rigidbody = disk.GetComponent<Rigidbody>();

                                _rigidbody.isKinematic = true;
                                _rigidbody.useGravity = false;

                                _selectionPosition = new Vector3(_selectionTransform.position.x, _selectionTransform.position.y + 0.3f, _selectionTransform.position.z);

                                _canSaveMoveFromKey = false;
                            }
                        }
                        else if (_canSaveMoveToKey)
                        {
                            if (GameManager.gameManager.IsTowerSelectedValid(hit, false, _moveFromKey))
                            {
                                _moveToKey = GameManager.gameManager.GetTowerKey(hit.collider.tag);

                                _canSaveMoveToKey = false;

                                if (GameManager.gameManager.IsMoveSelectedValid(_moveFromKey, _moveToKey))
                                {
                                    // Allow the disc to move, and don't get user input
                                    // while it is still moving.
                                    _isMoving = true;

                                    _hasReachedSelectionPos = false;

                                    GameManager.gameManager.CurrentMoves++;
                                    GameManager.gameManager.RemainingMoves--;
                                    UIManager.uIManager.UpdateCurrentMoves(GameManager.gameManager.CurrentMoves);
                                    UIManager.uIManager.UpdateRemainingMoves(GameManager.gameManager.RemainingMoves);

                                    // Set up the moving disc components
                                    _gameObject = GameManager.gameManager.MoveTopDisk(_moveFromKey, _moveToKey);
                                    _transform = _gameObject.GetComponent<Transform>();
                                    _rigidbody = _gameObject.GetComponent<Rigidbody>();

                                    _rigidbody.useGravity = false;
                                    _rigidbody.isKinematic = true;


                                    SetMoveTargetsIndex();

                                    if (GameManager.gameManager.HasLevelFinished())
                                    {
                                        UIManager.uIManager.ShowLevelCompletePanel();
                                        UIManager.uIManager.HidePlaymodeUI();
                                        GameManager.gameManager.CanPlay = false;
                                    }
                                }
                                else
                                {
                                    UIManager.uIManager.ShowInvalidMoveText();

                                    _canSaveMoveFromKey = true;
                                    _canSaveMoveToKey = true;

                                    _rigidbody.isKinematic = false;
                                    _rigidbody.useGravity = true;

                                    _hasReachedSelectionPos = false;
                                }
                            }
                            else
                            {
                                UIManager.uIManager.ShowInvalidMoveText();

                                _canSaveMoveFromKey = true;
                                _canSaveMoveToKey = true;

                                _rigidbody.isKinematic = false;
                                _rigidbody.useGravity = true;

                                _hasReachedSelectionPos = false;
                            }
                        }
                    }
                    else
                    {
                        _canSaveMoveFromKey = true;
                        _canSaveMoveToKey = true;

                        if (_rigidbody != null)
                        {
                            _rigidbody.isKinematic = false;
                            _rigidbody.useGravity = true;
                        }

                        _hasReachedSelectionPos = false;
                    }
                }
                else
                {
                    // If a disk has been selected but no tower destination
                    // has been specified, move it around.
                    if (_canSaveMoveFromKey == false && _canSaveMoveToKey == true)
                    {
                        if (!_hasReachedSelectionPos)
                        {
                            _selectionTransform.position = Vector3.MoveTowards(_selectionTransform.position, _selectionPosition, Time.deltaTime * _risingSpeed);

                            if (Vector3.Distance(_selectionTransform.position, _selectionPosition) <= 0.01)
                                _hasReachedSelectionPos = true;
                        }
                        else
                        {
                            _selectionTransform.position = Vector3.Lerp(_selectionPosition, _selectionPosition + _deltaPosition, Mathf.PingPong(Time.timeSinceLevelLoad * _motionSpeed, 1f));
                        }
                    }
                }
            }
        }
        else
        {
            if (_isMoving)
                MoveDisc();
        }
    }

    // TODO: Change comment, does not define what is happening above
    // The disk is going to move to two position; first above its own tower, then above
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
        else if (_moveToKey == 'B')
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
                _index = 0;
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
}
