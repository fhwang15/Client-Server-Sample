using UnityEngine;
using System.Collections.Generic;

public class StationZone : MonoBehaviour
{
    [Header("Station 설정")]
    // 몇 명이 들어와야 활성화되는지
    [SerializeField] private int _requiredPlayerCount = 1;
    // 이 Zone과 연결된 Selectable Object
    [SerializeField] private StationObject _stationObject;

    // 현재 Zone 안에 있는 플레이어 목록
    private HashSet<GameObject> _playersInZone = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playersInZone.Add(other.gameObject);
        UpdateStationState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playersInZone.Remove(other.gameObject);
        UpdateStationState();
    }

    private void UpdateStationState()
    {
        // 딱 정해진 수만큼만 있을 때만 활성화
        bool isReady = _playersInZone.Count == _requiredPlayerCount;
        _stationObject.SetActivatable(isReady);
    }

    // Lock-on 상태일 때 플레이어가 Zone에서 못 나가도록 위치를 고정시켜줌
    // StationObject에서 호출함
    public HashSet<GameObject> GetPlayersInZone()
    {
        return _playersInZone;
    }

    public int GetRequiredCount()
    {
        return _requiredPlayerCount;
    }

}