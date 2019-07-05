using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscsController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private UIManager uIManager;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LayerMask _whatIsATower;

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

                    if (Physics.Raycast(ray, out hit, _whatIsATower))
                    {
                        AudioManager.audioManager.PlayTowerTouched();

                        if (_canSaveMoveFromKey)
                        {
                            if (gameManager.IsTowerSelectedValid(hit, true, _moveFromKey))
                                _moveFromKey = gameManager.GetTowerKey(hit.collider.tag);
                        }
                        else if (_canSaveMoveToKey)
                        {
                            if (gameManager.IsTowerSelectedValid(hit, false, _moveFromKey))
                            {
                                _moveToKey = gameManager.GetTowerKey(hit.collider.tag);

                                if (gameManager.IsMoveSelectedValid(_moveFromKey, _moveToKey))
                                {
                                    // Allow the disc to move, and don't get user input
                                    // while it is still moving.
                                    _isMoving = true;

                                    // Set up the moving disc components
                                    _gameObject = gameManager.MoveTopDiscInStacks(_moveFromKey, _moveToKey);
                                    _transform = _gameObject.GetComponent<Transform>();
                                    _rigidbody = _gameObject.GetComponent<Rigidbody>();

                                    _rigidbody.useGravity = false;
                                    _rigidbody.isKinematic = true;

                                    SetMoveTargetsIndex();

                                    //TODO: increase currentMoves and decrease remainingMoves

                                    if (gameManager.HasLevelFinished())
                                    {
                                        uIManager.ShowLevelCompletePanel();
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
}
