using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager audioManager;

    [Header("AudioSource & SFX")]
    [SerializeField]
    private AudioSource _audioSource;


    [SerializeField]
    private AudioClip _uiTouchSFX;
    [SerializeField]
    private AudioClip _towerTouchedSFX;
    [SerializeField]
    private AudioClip _gameStartSFX;

    [Header("Audio UI")]
    [SerializeField]
    private Image _audioImage;
    [SerializeField]
    private Sprite _soundsOnIcon;
    [SerializeField]
    private Sprite _soundsOffIcon;

    private void Awake()
    {
        if (audioManager == null)
            audioManager = GetComponent<AudioManager>();
    }

    public void PlayGameStart() => _audioSource.PlayOneShot(_gameStartSFX);

    public void PlayTowerTouched() => _audioSource.PlayOneShot(_towerTouchedSFX);

    public void PlayUiTouch() => _audioSource.PlayOneShot(_uiTouchSFX);

    public void ChangeAudioListenerState()
    {
        AudioListener.pause = !AudioListener.pause;
        UpdateAudioIcon(true);
    }

    public void UpdateAudioIcon(bool isButtonTouched)
    {
        if (AudioListener.pause == true)
            _audioImage.overrideSprite = _soundsOffIcon;
        else if (AudioListener.pause == false)
        {
            _audioImage.overrideSprite = _soundsOnIcon;

            if (isButtonTouched)
                PlayUiTouch();
        }
    }
}
