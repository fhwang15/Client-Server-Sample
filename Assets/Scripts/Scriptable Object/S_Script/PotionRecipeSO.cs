using UnityEngine;

[CreateAssetMenu(fileName = "PotionRecipe", menuName = "Potion/Recipe")]
public class PotionRecipeSO : ScriptableObject
{
    [Header("포션 정보")]
    public string potionName;        
    public Sprite potionIcon;         

    [Header("순서대로 완료해야 할 Task 목록")]
    public StationTaskSO[] tasks;    

    public int CurrentTaskIndex { get; private set; } = 0;

    public StationTaskSO GetCurrentTask()
    {
        if (CurrentTaskIndex >= tasks.Length) return null;
        return tasks[CurrentTaskIndex];
    }

    // task 완료 시 호출
    public bool CompleteCurrentTask()
    {
        if (CurrentTaskIndex >= tasks.Length) return false;

        tasks[CurrentTaskIndex].isCompleted = true;
        CurrentTaskIndex++;

        // 전부 완료됐으면 true 반환
        return CurrentTaskIndex >= tasks.Length;
    }

    // 게임 시작할 때 초기화
    public void Reset()
    {
        CurrentTaskIndex = 0;
        foreach (var task in tasks)
            task.isCompleted = false;
    }
}
