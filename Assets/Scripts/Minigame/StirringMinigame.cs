using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum StirDirection { Left, Right }

[System.Serializable]
public class StirStep
{
    public StirDirection direction;
    public float rotations = 1f;
}

public class StirringMinigame : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField] private StationTaskSO _task;

    [Header("UI 연결")]
    [SerializeField] private RectTransform _spoonIndicator; // 스푼 (원 위를 도는 점)
    [SerializeField] private RectTransform _circleCenter;   // 원의 중심
    [SerializeField] private TextMeshProUGUI _instructionText; // "왼쪽 2바퀴!" 안내
    [SerializeField] private TextMeshProUGUI _progressText;    // "1.5 / 2 바퀴"
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Image[] _stepIndicators; // 스텝마다 체크 표시용

    [SerializeField] private float _wrongDirectionDelay = 2f;
    private float _wrongDirectionTimer = 0f;
    private bool _isWrongDirection = false;

    [Header("설정")]
    [SerializeField] private float _circleRadius = 100f;    // 스푼이 도는 원 반지름
    [SerializeField] private float _rotationThreshold = 30f; // 방향 감지 민감도 (각도)

    // 현재 몇 번째 스텝인지
    private int _currentStepIndex = 0;
    // 현재 스텝에서 누적된 각도
    private float _accumulatedAngle = 0f;
    // 마우스의 이전 프레임 각도
    private float _prevMouseAngle = 0f;
    // 게임 진행 중인지
    private bool _isPlaying = false;
    // 현재 스텝의 목표 각도 (바퀴 수 * 360)
    private float _targetAngle = 0f;

    private void OnEnable()
    {
        StartMinigame();
    }

    public void StartMinigame()
    {
        if (_task == null || _task.stirSteps.Length == 0)
        {
            Debug.LogError("StirringRecipe가 없어!");
            return;
        }

        _isPlaying = true;
        _currentStepIndex = 0;
        _accumulatedAngle = 0f;

        // 마우스 현재 각도 초기화
        _prevMouseAngle = GetMouseAngle();

        UpdateStepUI();
    }

    private void Update()
    {
        if (!_isPlaying) return;

        HandleStirring();
        UpdateSpoonVisual();

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
        _accumulatedAngle = 0f;      // Stirring만
        _currentStepIndex = 0;       // Stirring만

        // StationObject한테 Unlock 신호
        GetComponentInParent<StationObject>()?.Unlock();
    }

    private void HandleStirring()
    {
        float currentAngle = GetMouseAngle();
        float delta = Mathf.DeltaAngle(_prevMouseAngle, currentAngle);
        _prevMouseAngle = currentAngle;

        if (Mathf.Abs(delta) < 0.5f) return;

        StirStep currentStep = _task.stirSteps[_currentStepIndex];

        // 방향 체크
        // delta < 0 = 시계방향 = 오른쪽
        // delta > 0 = 반시계방향 = 왼쪽
        bool isMovingLeft = delta > 0;
        bool isCorrectDirection =
            (currentStep.direction == StirDirection.Left && isMovingLeft) ||
            (currentStep.direction == StirDirection.Right && !isMovingLeft);

        if (isCorrectDirection)
        {
            _accumulatedAngle += Mathf.Abs(delta);
            _isWrongDirection = false;      // 맞는 방향으로 돌아오면 타이머 리셋
            _wrongDirectionTimer = 0f;

            if (_accumulatedAngle >= currentStep.rotations * 360f)
                CompleteStep();
        }
        else
        {
            // 즉시 실패 대신 타이머 시작
            if (!_isWrongDirection)
            {
                _isWrongDirection = true;
                _wrongDirectionTimer = 0f;
            }

            _wrongDirectionTimer += Time.deltaTime;

            if (_wrongDirectionTimer >= _wrongDirectionDelay)
            {
                _isWrongDirection = false;
                _wrongDirectionTimer = 0f;
                WrongDirection();
            }
        }
        UpdateProgressUI();
    }

    private void CompleteStep()
    {
        // 스텝 완료 표시
        if (_stepIndicators != null && _currentStepIndex < _stepIndicators.Length)
            _stepIndicators[_currentStepIndex].color = Color.green;

        _currentStepIndex++;
        _accumulatedAngle = 0f;

        // 모든 스텝 완료!
        if (_currentStepIndex >= _task.stirSteps.Length)
        {
            Success();
            return;
        }

        UpdateStepUI();
        Debug.Log($"Step {_currentStepIndex} Complete!");
    }

    private void WrongDirection()
    {
        Debug.Log("틀렸어! 처음부터 다시!");

        // 스텝 인디케이터 전부 빨갛게
        if (_stepIndicators != null)
            foreach (var indicator in _stepIndicators)
                indicator.color = Color.red;

        // 잠깐 후 리셋
        _currentStepIndex = 0;
        _accumulatedAngle = 0f;

        // 인디케이터 다시 회색으로
        if (_stepIndicators != null)
            foreach (var indicator in _stepIndicators)
                indicator.color = Color.gray;

        UpdateStepUI();
    }

    private void Success()
    {
        _isPlaying = false;
        if (_resultText != null)
            _resultText.text = "Stirring Complete!";


        GetComponentInParent<StationObject>()?.OnMinigameComplete();

        StartCoroutine(CloseAfterDelay(2f));


    }

    private float GetMouseAngle()
    {
        // 원 중심의 스크린 좌표
        Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(
            null, _circleCenter.position);

        Vector2 dir = (Vector2)Input.mousePosition - centerScreen;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    // 스푼을 마우스 각도에 맞춰 원 위에 배치
    private void UpdateSpoonVisual()
    {
        if (_spoonIndicator == null || _circleCenter == null) return;

        float angle = GetMouseAngle() * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(
            Mathf.Cos(angle) * _circleRadius,
            Mathf.Sin(angle) * _circleRadius
        );
        _spoonIndicator.anchoredPosition =
            _circleCenter.anchoredPosition + offset;
    }

    private void UpdateStepUI()
    {
        if (_task == null) return;
        StirStep step = _task.stirSteps[_currentStepIndex];

        string dirText = step.direction == StirDirection.Left ? "<= Left" : " Right =>";
        if (_instructionText != null)
            _instructionText.text = $"{dirText} {step.rotations}";
    }

    private void UpdateProgressUI()
    {
        if (_task == null) return;
        StirStep step = _task.stirSteps[_currentStepIndex];
        float current = _accumulatedAngle / 360f;
        float target = step.rotations;

        if (_progressText != null)
            _progressText.text = $"{current:F1} / {target} turn";
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // UI 끄기
    }

}