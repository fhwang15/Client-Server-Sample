using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskEntryUI : MonoBehaviour
{
    [SerializeField] private Image _checkIcon;
    [SerializeField] private TextMeshProUGUI _taskNameText;

    // 완료 색 / 미완료 색
    private readonly Color _completedColor = new Color(0.3f, 0.8f, 0.3f);
    private readonly Color _pendingColor = new Color(0.6f, 0.6f, 0.6f);
    private readonly Color _lockedColor = new Color(0.3f, 0.3f, 0.3f); // 아직 순서 안 된 것

    public void Setup(string taskName, bool isCompleted, bool isLocked)
    {
        if (_taskNameText != null)
            _taskNameText.text = taskName;

        if (_checkIcon != null)
        {
            if (isCompleted)
                _checkIcon.color = _completedColor;
            else if (isLocked)
                _checkIcon.color = _lockedColor;
            else
                _checkIcon.color = _pendingColor;
        }
    }
}