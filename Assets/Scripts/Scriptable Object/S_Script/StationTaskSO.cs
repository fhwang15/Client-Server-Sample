using UnityEngine;

[CreateAssetMenu(fileName = "StationTask", menuName = "Potion/StationTask")]
public class StationTaskSO : ScriptableObject
{
    [Header("스테이션 정보")]
    public string taskName;           
    public string description;        
    public int requiredPlayerCount;   

    [Header("상태 (런타임에서 바뀜)")]
    public bool isCompleted = false;
}
