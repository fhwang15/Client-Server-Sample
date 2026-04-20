using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookMinigame : MonoBehaviour
{
    [Header("레시피 연결")]
    [SerializeField] private PotionRecipeSO _recipe;

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TaskEntryUI[] _taskEntries; // Task 수만큼!
    [SerializeField] private Button _finishButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private TextMeshProUGUI _resultText;

    private void OnEnable()
    {
        RefreshUI();

        // 버튼 이벤트 연결
        if (_finishButton != null)
            _finishButton.onClick.AddListener(OnFinish);
        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnReset);
    }

    private void OnDisable()
    {
        // 메모리 누수 방지
        if (_finishButton != null)
            _finishButton.onClick.RemoveListener(OnFinish);
        if (_resetButton != null)
            _resetButton.onClick.RemoveListener(OnReset);
    }

    private void Update()
    {
        // 매 프레임 체크 (다른 Station에서 완료될 수 있으니까!)
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_recipe == null) return;

        // 타이틀
        if (_titleText != null)
            _titleText.text = _recipe.potionName;

        // 각 Task 상태 업데이트
        for (int i = 0; i < _taskEntries.Length; i++)
        {
            if (i >= _recipe.tasks.Length) break;

            StationTaskSO task = _recipe.tasks[i];
            bool isCompleted = task.isCompleted;
            // 이전 Task가 완료되지 않으면 잠김
            bool isLocked = i > 0 && !_recipe.tasks[i - 1].isCompleted;

            _taskEntries[i].Setup(task.taskName, isCompleted, isLocked);
        }

        // 전부 완료됐는지 체크
        bool allCompleted = _recipe.CurrentTaskIndex >= _recipe.tasks.Length;

        // Finish 버튼 활성화 여부
        if (_finishButton != null)
            _finishButton.interactable = allCompleted;

        // 결과 텍스트
        if (_resultText != null)
            _resultText.text = allCompleted ? "포션 완성 준비됐어!" : "";
    }

    private void OnFinish()
    {
        if (_resultText != null)
            _resultText.text = "포션 완성!";

        Debug.Log("포션 완성!");
        // 나중에 여기서 게임 클리어 로직 연결!
    }

    private void OnReset()
    {
        _recipe.Reset();
        if (_resultText != null)
            _resultText.text = "";

        RefreshUI();
        Debug.Log("레시피 리셋!");
    }
}