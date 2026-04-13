using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StationObject : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _activatableMaterial;  
    [SerializeField] private Material _lockedMaterial;       

    [Header("연결된 Zone")]
    [SerializeField] private StationZone _zone;

    private bool _isActivatable = false;   
    private bool _isLockedOn = false;

    [Header("미니게임 연결")]
    [SerializeField] private StationTaskSO _task; 

    [Header("UI")]
    [SerializeField] private GameObject _miniGameUI;   
    [SerializeField] private TextMeshProUGUI _statusText; 

    private Dictionary<GameObject, Vector3> _lockedPositions = new Dictionary<GameObject, Vector3>();
    private int _lockedOnCount = 0;

    private Renderer _renderer;

    private bool _waitingForClick = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        SetMaterial(_defaultMaterial);
    }

    // StationZone이 플레이어 수 변할 때마다 호출해줌
    public void SetActivatable(bool activatable)
    {
        // Lock-on 중엔 상태 변경 안 됨
        if (_isLockedOn) return;

        _isActivatable = activatable;
        SetMaterial(_isActivatable ? _activatableMaterial : _defaultMaterial);
    }

    private void Update()
    {
        // Lock-on 상태일 때 플레이어 위치 고정
        if (_isLockedOn)
        {
            foreach (var kvp in _lockedPositions)
                kvp.Key.transform.position = kvp.Value;
        }

        // 활성화 가능 상태 → 커서 먼저 풀기
        if (_isActivatable && !_isLockedOn && !_waitingForClick)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _waitingForClick = true; // 다음 프레임부터 클릭 감지
        }

        // 비활성화 상태로 돌아오면 커서 다시 잠금
        if (!_isActivatable && !_isLockedOn && _waitingForClick)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _waitingForClick = false;
        }

        // 커서 풀린 상태에서 왼클릭 → Raycast
        if (_waitingForClick && !_isLockedOn && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == this.transform)
                    LockOn();
            }
        }

        // Lock-on 상태 + 우클릭 → 해제
        if (_isLockedOn && Input.GetMouseButtonDown(1))
            Unlock();
    }

    private void LockOn()
    {
        _isLockedOn = true;
        _lockedOnCount++;
        SetMaterial(_lockedMaterial);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var players = _zone.GetPlayersInZone();
        _lockedPositions.Clear();
        foreach (var player in players)
            _lockedPositions[player] = player.transform.position;

        if (_lockedOnCount >= _zone.GetRequiredCount())
            StartMiniGame();

        Debug.Log($"Lock-on! ({_lockedOnCount}/{_zone.GetRequiredCount()})");
    }

    private void Unlock()
    {
        _isLockedOn = false;
        _lockedOnCount = Mathf.Max(0, _lockedOnCount - 1);
        _lockedPositions.Clear();
        SetMaterial(_isActivatable ? _activatableMaterial : _defaultMaterial);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 미니게임 중단
        StopMiniGame();

        Debug.Log("Station Unlocked!");
    }

    private void StartMiniGame()
    {
        if (_miniGameUI != null)
            _miniGameUI.SetActive(true);

        if (_statusText != null)
            _statusText.text = _task != null
                ? $"미니게임 시작!\n{_task.taskName}\n{_task.description}"
                : "미니게임 시작!";

        Debug.Log("미니게임 시작!");
        // 나중에 여기에 실제 미니게임 로직 연결
    }

    private void StopMiniGame()
    {
        if (_miniGameUI != null)
            _miniGameUI.SetActive(false);

        if (_statusText != null)
            _statusText.text = "";

        Debug.Log("미니게임 중단!");
    }

    private void SetMaterial(Material mat)
    {
        if (_renderer != null && mat != null)
            _renderer.material = mat;
    }
}