using UnityEngine;
using System.Collections.Generic;

public class StationObject : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _activatableMaterial;  // 활성화 가능 상태
    [SerializeField] private Material _lockedMaterial;       // Lock-on 상태

    [Header("연결된 Zone")]
    [SerializeField] private StationZone _zone;

    private bool _isActivatable = false;   // 클릭 가능한 상태인가?
    private bool _isLockedOn = false;      // 현재 Lock-on 상태인가?

    // Lock-on된 플레이어들의 위치를 고정시키기 위해 저장
    private Dictionary<GameObject, Vector3> _lockedPositions = new Dictionary<GameObject, Vector3>();

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
        _waitingForClick = false;
        SetMaterial(_lockedMaterial);
        // 커서는 이미 풀려있으니 그대로 유지

        var players = _zone.GetPlayersInZone();
        _lockedPositions.Clear();
        foreach (var player in players)
            _lockedPositions[player] = player.transform.position;

        Debug.Log("Station Lock-on!");
    }

    private void Unlock()
    {
        _isLockedOn = false;
        _lockedPositions.Clear();
        SetMaterial(_isActivatable ? _activatableMaterial : _defaultMaterial);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Station Unlocked!");
    }

    private void SetMaterial(Material mat)
    {
        if (_renderer != null && mat != null)
            _renderer.material = mat;
    }
}