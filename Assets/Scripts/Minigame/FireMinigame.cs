using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FireMinigame : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform _trackArea;      // TrackArea
    [SerializeField] private RectTransform _fireBar;        // 초록 바
    [SerializeField] private RectTransform _flameTarget;    // 불꽃 타겟
    [SerializeField] private Image _progressFill;           // 게이지 채워지는 이미지
    [SerializeField] private TextMeshProUGUI _resultText;   // 결과 텍스트

    [Header("게임 설정")]
    [SerializeField] private float _fireBarHeight = 120f;   // 초록 바 높이
    [SerializeField] private float _riseSpeed = 400f;       // 누를 때 올라가는 속도
    [SerializeField] private float _fallSpeed = 280f;       // 뗄 때 내려오는 속도
    [SerializeField] private float _targetMoveSpeed = 180f; // 타겟 이동 속도
    [SerializeField] private float _progressSpeed = 0.4f;   // 게이지 차는 속도
    [SerializeField] private float _decaySpeed = 0.25f;     // 게이지 줄어드는 속도

    [SerializeField] private ParticleSystem _fireParticle;
    [SerializeField] private float _maxEmissionRate = 50f;


    // 트랙 영역 높이 (런타임에 계산)
    private float _trackHeight;
    // 초록 바 중심 Y 위치
    private float _barY;
    // 타겟 현재 Y 위치
    private float _targetY;
    // 타겟 이동 방향 (+1 위, -1 아래)
    private float _targetDirection = 1f;
    // 현재 게이지 (0~1)
    private float _progress = 0f;
    // 게임 진행 중인지
    private bool _isPlaying = false;
    // 성공 여부
    private bool _isFinished = false;

    private void OnEnable()
    {
        StartMinigame();
    }

    public void StartMinigame()
    {
        _isPlaying = true;
        _isFinished = false;
        _progress = 0f;

        // 트랙 높이 계산 (중심 기준 절반)
        _trackHeight = _trackArea.rect.height;

        // 시작 위치: 바는 아래, 타겟은 중간
        _barY = -_trackHeight / 2f + _fireBarHeight / 2f;
        _targetY = 0f;
        _targetDirection = 1f;

        if (_resultText != null)
            _resultText.text = "";

        UpdateVisuals();
    }

    private void Update()
    {
        if (!_isPlaying || _isFinished) return;

        HandleBarMovement();
        HandleTargetMovement();
        HandleProgress();
        UpdateVisuals();
        CheckSuccess();


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetAndClose();
            return;
        }
    }

    private void ResetAndClose()
    {
        // 진행 상태 리셋
        _isPlaying = false;
        _progress = 0f;              // Fire만

        // StationObject한테 Unlock 신호
        GetComponentInParent<StationObject>()?.Unlock();
    }
    private void HandleBarMovement()
    {
        // 스페이스바 또는 마우스 왼쪽 버튼 누르면 올라감
        bool isPressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (isPressed)
            _barY += _riseSpeed * Time.deltaTime;
        else
            _barY -= _fallSpeed * Time.deltaTime;

        // 트랙 범위를 벗어나지 않도록 제한
        float minY = -_trackHeight / 2f + _fireBarHeight / 2f;
        float maxY = _trackHeight / 2f - _fireBarHeight / 2f;
        _barY = Mathf.Clamp(_barY, minY, maxY);
    }

    private void HandleTargetMovement()
    {
        _targetY += _targetMoveSpeed * _targetDirection * Time.deltaTime;

        // 위아래 끝에 닿으면 방향 전환
        float limit = _trackHeight / 2f - 20f;
        if (_targetY >= limit)
        {
            _targetY = limit;
            _targetDirection = -1f;
        }
        else if (_targetY <= -limit)
        {
            _targetY = -limit;
            _targetDirection = 1f;
        }
    }

    private void HandleProgress()
    {
        // 타겟이 초록 바 안에 있는지 체크
        float barTop = _barY + _fireBarHeight / 2f;
        float barBottom = _barY - _fireBarHeight / 2f;
        bool isInBar = _targetY <= barTop && _targetY >= barBottom;

        if (isInBar)
            _progress += _progressSpeed * Time.deltaTime;
        else
            _progress -= _decaySpeed * Time.deltaTime;

        _progress = Mathf.Clamp01(_progress);
    }

    private void UpdateVisuals()
    {
        _fireBar.anchoredPosition = new Vector2(0f, _barY);
        _fireBar.sizeDelta = new Vector2(_fireBar.sizeDelta.x, _fireBarHeight);
        _flameTarget.anchoredPosition = new Vector2(0f, _targetY);
        if (_progressFill != null)
            _progressFill.fillAmount = _progress;

        // 파티클 업데이트 추가
        if (_fireParticle != null)
        {
            var emission = _fireParticle.emission;
            emission.rateOverTime = _progress * _maxEmissionRate;
            var main = _fireParticle.main;
            main.startSizeMultiplier = Mathf.Lerp(0.1f, 0.5f, _progress);

            // progress가 아주 낮으면 파티클 아예 끔
            if (_progress < 0.05f && _fireParticle.isPlaying)
                _fireParticle.Stop();
            else if (_progress >= 0.05f && !_fireParticle.isPlaying)
                _fireParticle.Play();

        }
    }

    private void CheckSuccess()
    {
        if (_progress >= 1f)
        {
            _isFinished = true;
            _isPlaying = false;

            if (_resultText != null)
                _resultText.text = "Success!";

            GetComponentInParent<StationObject>()?.OnMinigameComplete();

            StartCoroutine(CloseAfterDelay(2f));
        }
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // UI 끄기
    }
}