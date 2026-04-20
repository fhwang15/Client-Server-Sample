using UnityEngine;

[CreateAssetMenu(fileName = "StationTask", menuName = "Potion/StationTask")]
public class StationTaskSO : ScriptableObject
{
    [Header("스테이션 정보")]
    public string taskName;
    public string description;
    public int requiredPlayerCount;

    [Header("런타임 (런타임에서 바뀜)")]
    public bool isCompleted = false;

    [Header("Stirring 패턴 (Stirring 스테이션만 채우면 돼!)")]
    public StirStep[] stirSteps;
}
