using MiniTeam.Tetris;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TetrisBlock : MonoBehaviour
{
    public enum BlockType { Normal, I_enable, I_disable, O }
    public BlockType type = BlockType.Normal;

    public Vector3 rotationPoint;
    private float previousTime;
    public float fallTime = 0.8f;
    public static int height = 20;
    public static int width = 10;
    private static Transform[,] grid = new Transform[width, height];

    [Header("조작감 세팅 (DAS & ARR)")]
    public float das = 0.17f;
    public float arr = 0.05f;
    private float horizontalTimer = 0f;

    [Header("고정 제어 (Lock Delay)")]
    public float lockDelay = 0.5f;
    private float lockTimer = 0f;
    private int moveCount = 0;
    private const int maxMoveCount = 15; // 바닥에서 최대 15번의 조작만 허용

    public int rotationState = 0;

    // ---  [SRS 하드코딩 데이터] ---
    // [JLSTZ = Normal 블록] 시계 방향 회전 시 벽차기 오프셋 (Test 2 ~ 5)
    private readonly Vector2[,] normalKickData = new Vector2[,] {
        { new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -2), new Vector2(-1, -2) }, // State 0 -> 1
        { new Vector2(1, 0),  new Vector2(1, -1), new Vector2(0, 2),  new Vector2(1, 2) },   // State 1 -> 2
        { new Vector2(1, 0),  new Vector2(1, 1),  new Vector2(0, -2), new Vector2(1, -2) },  // State 2 -> 3
        { new Vector2(-1, 0), new Vector2(-1, -1),new Vector2(0, 2),  new Vector2(-1, 2) }   // State 3 -> 0
    };

    // [I 블록] 긴 막대기 전용 시계 방향 벽차기 오프셋
    private readonly Vector2[,] iKickData = new Vector2[,] {
        { new Vector2(-2, 0), new Vector2(1, 0),  new Vector2(-2, -1), new Vector2(1, 2) },
        { new Vector2(-1, 0), new Vector2(2, 0),  new Vector2(-1, 2),  new Vector2(2, -1) },
        { new Vector2(2, 0),  new Vector2(-1, 0), new Vector2(2, 1),   new Vector2(-1, -2) },
        { new Vector2(1, 0),  new Vector2(-2, 0), new Vector2(1, -2),  new Vector2(-2, 1) }
    };

    private void Start()
    {
        previousTime = Time.time;

        if (!ValidMove())
        {
            Debug.Log(" GAME OVER!");
            SpawnTetromino.Instance.TogglespawnTrigger();
            TetrisGameController.Instance.OnGameFail();
            return;
            this.enabled = false;
        }
    }

    void Update()
    {
        // 0순위 : 컷신 중 조작 차단
        if (TetrisGameController.Instance != null && TetrisGameController.Instance.isCutscenePlaying)
        {
            return;
        }

        // 1순위 : HOLD
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SpawnTetromino.Instance.HoldBlock(this.gameObject);
            return;
        }

        // 2순위 : 하드 드롭 (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
            return;
        }

        // 5순위: 하강 로직
        transform.position += new Vector3(0, -1, 0);
        bool canDrop = ValidMove();
        transform.position += new Vector3(0, 1, 0);

        // [무한 회전 방지 핵심 로직] 바닥에 닿은 상태에서만 조작 횟수를 소모함
        bool isAtBottom = !canDrop;

        // 3순위 : 회전 (I_enable 블록은 2상태로 제한)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (type != BlockType.O)
            {
                // ⭐️ I_enable 블록만 상태를 0과 1로 제한
                bool isTwoStateBlock = (type == BlockType.I_enable);
                int maxStates = isTwoStateBlock ? 2 : 4;
              
                float angle = (isTwoStateBlock && rotationState == 1) ? 90f : -90f;

                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), angle);
                bool rotationSuccess = true;

                if (!ValidMove())
                {
                    if (!PerformWallKick(rotationState))
                    {
                        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -angle);
                        rotationSuccess = false;
                    }
                }

                if (rotationSuccess)
                {
                    rotationState = (rotationState + 1) % maxStates;
                    
                    if (isAtBottom)
                    {
                        if (moveCount < maxMoveCount)
                        {
                            lockTimer = 0f;
                            moveCount++;
                        }
                    }
                    else
                    {
                        lockTimer = 0f; // 공중에선 무제한 초기화 (어차피 떨어지니까)
                    }
                }
            }
        }

        // 4순위: 좌우 이동 
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (MoveHorizontal(-1))
            {
                if (isAtBottom)
                {
                    if (moveCount < maxMoveCount)
                    {
                        lockTimer = 0f;
                        moveCount++;
                    }
                }
                else
                {
                    lockTimer = 0f;
                }
            }
            horizontalTimer = Time.time + das;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (MoveHorizontal(1))
            {
                if (isAtBottom)
                {
                    if (moveCount < maxMoveCount)
                    {
                        lockTimer = 0f;
                        moveCount++;
                    }
                }
                else
                {
                    lockTimer = 0f;
                }
            }
            horizontalTimer = Time.time + das;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Time.time > horizontalTimer)
            {
                if (MoveHorizontal(-1))
                {
                    if (isAtBottom)
                    {
                        if (moveCount < maxMoveCount)
                        {
                            lockTimer = 0f;
                            moveCount++;
                        }
                    }
                    else
                    {
                        lockTimer = 0f;
                    }
                }
                horizontalTimer = Time.time + arr;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Time.time > horizontalTimer)
            {
                if (MoveHorizontal(1))
                {
                    if (isAtBottom)
                    {
                        if (moveCount < maxMoveCount)
                        {
                            lockTimer = 0f;
                            moveCount++;
                        }
                    }
                    else
                    {
                        lockTimer = 0f;
                    }
                }
                horizontalTimer = Time.time + arr;
            }
        }

        // 5순위: 하강 처리 (이미 위에서 canDrop 계산함)
        if (isAtBottom)
        {
            lockTimer += Time.deltaTime;

            if (lockTimer >= lockDelay)
            {
                LockBlock();
                return;
            }
        }
        else
        {
            if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10f : fallTime))
            {
                transform.position += new Vector3(0, -1, 0);
                previousTime = Time.time;
                
                // 아래로 한 칸이라도 내려가면 조작 횟수 및 락 딜레이 초기화
                lockTimer = 0f;
                moveCount = 0;
            }
        }

        // 6순위: 고스트 위치 업데이트
        UpdateGhost();
    }

    private void HardDrop()
    {
        while (ValidMove())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        transform.position -= new Vector3(0, -1, 0);
        LockBlock();
    }

    private void LockBlock()
    {
        AddToGrid();
        int cleared = CheckForLines();
        LineClearEventManager.Instance?.ProcessLineClear(cleared);

        if (type != BlockType.I_enable)
        {
            transform.DetachChildren();
            Destroy(gameObject);
        }
        else
        {
            this.enabled = false;
        }

        SpawnTetromino.Instance.NewTetromino();
    }

    bool PerformWallKick(int currentState)
    {
        Vector2[,] kickTable = (type == BlockType.I_enable) ? iKickData : normalKickData;

        for (int testIndex = 0; testIndex < 4; testIndex++)
        {
            Vector2 translation = kickTable[currentState, testIndex];
            transform.position += new Vector3(translation.x, translation.y, 0);

            if (ValidMove())
            {
                return true;
            }

            transform.position -= new Vector3(translation.x, translation.y, 0);
        }
        return false;
    }

    private int CheckForLines()
    {
        int clearedLines = 0;

        for (int i = height - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
                clearedLines++;
                i++; // 줄이 내려왔으므로 현재 인덱스를 다시 검사
            }
        }
        return clearedLines;
    }

    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            if (grid[j, i] == null)
            {
                return false;
            }
        }
        return true;
    }

    private void DeleteLine(int i)
    {
        bool hasTriggeredEffect = false;

        for (int j = width - 1; j >= 0; j--)
        {
            Transform cell = grid[j, i];
            if (cell != null)
            {
                Transform parentTransform = cell.parent;

                if (parentTransform != null)
                {
                    if (parentTransform.TryGetComponent(out TetrisBlock parentBlock))
                    {
                        if (parentBlock.type == BlockType.I_enable)
                        {
                            // 1. 가로/세로 판별
                            bool isHorizontal = (parentBlock.rotationState == 0);

                            // 2. 최하단 파편(bottomCell) 찾기
                            Transform bottomCell = cell;
                            float minY = cell.position.y;
                            foreach (Transform sibling in parentTransform)
                            {
                                if (sibling.position.y < minY)
                                {
                                    minY = sibling.position.y;
                                    bottomCell = sibling;
                                }
                            }

                            // 3. 살아남을 파편(형제들) 색상을 회색으로 변경
                            foreach (Transform sibling in parentTransform)
                            {
                                if (sibling != cell)
                                {
                                    if (sibling.TryGetComponent(out SpriteRenderer sr))
                                    {
                                        sr.color = Color.gray;
                                    }
                                }
                            }

                            // 4. 타마마 임팩트 발동 (할 일 먼저 완벽하게 끝내기)
                            if (!hasTriggeredEffect)
                            {
                                float distanceToWall;
                                Vector3 spawnPosition = bottomCell.position;
                                Quaternion beamRotation;

                                if (isHorizontal)
                                {
                                    Debug.Log($"[{j}번째 열] ⬇️ [세로] 방향 I_enable 블록 파편 발견! 타마마 임팩트 발동!");
                                    distanceToWall = j;
                                    beamRotation = Quaternion.identity;
                                }
                                else
                                {
                                    Debug.Log($"[{j}번째 열] ➡️ [가로] 방향 I_enable 파편 폭발!");
                                    distanceToWall = 25f;
                                    beamRotation = Quaternion.Euler(0, 0, -90f);
                                }

                                if (TetrisGameController.Instance != null)
                                {
                                    Debug.Log("TetrisBlock에서 OnTamamaImpactTriggered 호출 (데이터 전달)");
                                    TetrisGameController.Instance.OnTamamaImpactTriggered(isHorizontal, spawnPosition, beamRotation, distanceToWall);
                                }

                                hasTriggeredEffect = true;
                            }

                            // ⭐️ 5. 모든 연출 생성과 데이터 참조가 끝난 '맨 마지막'에 부모 해체 및 삭제!
                            parentTransform.DetachChildren();
                            Destroy(parentTransform.gameObject);
                        }
                    }
                }

                // 6. 줄이 지워지는 해당 칸 파괴
                Destroy(cell.gameObject);
                grid[j, i] = null;
            }
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x - 0.2f);
            int roundedY = Mathf.RoundToInt(children.transform.position.y - 0.2f);

            if (roundedY >= height)
            {
                Debug.LogWarning("블록이 천장을 뚫었습니다. GAME OVER");
                SpawnTetromino.Instance.TogglespawnTrigger();
                TetrisGameController.Instance.OnGameFail();
                return;
            }

            grid[roundedX, roundedY] = children;
        }
    }

    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x - 0.2f);
            int roundedY = Mathf.RoundToInt(children.transform.position.y - 0.2f);

            if (roundedX < 0 || roundedX >= width || roundedY < 0)
            {
                return false;
            }

            if ((roundedY < height && grid[roundedX, roundedY] != null))
            {
                return false;
            }
        }
        return true;
    }

    bool ValidMoveFor(GameObject obj)
    {
        foreach (Transform children in obj.transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x - 0.2f);
            int roundedY = Mathf.RoundToInt(children.transform.position.y - 0.2f);

            if (roundedX < 0 || roundedX >= width || roundedY < 0)
            {
                return false;
            }

            if ((roundedY < height && grid[roundedX, roundedY] != null))
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateGhost()
    {
        if (SpawnTetromino.Instance == null || SpawnTetromino.Instance.currentGhost == null) return;
        
        GameObject ghost = SpawnTetromino.Instance.currentGhost;

        // 고스트 회전을 진짜 블록과 동일하게 맞춤
        ghost.transform.rotation = transform.rotation;
        
        // 고스트를 우선 진짜 블록 위치로 가져옴
        ghost.transform.position = transform.position;

        // 바닥/기존 블록에 닿을 때까지 가상으로 한 칸씩 내려봄
        while (ValidMoveFor(ghost))
        {
            ghost.transform.position += new Vector3(0, -1, 0);
        }
        
        // 반복문이 끝났다는 것은 바닥을 뚫었다는 뜻이므로, 마지막 유효한 위치(한 칸 위)로 되돌림
        ghost.transform.position += new Vector3(0, 1, 0);
    }

    private bool MoveHorizontal(int direction)
    {
        transform.position += new Vector3(direction, 0, 0);

        if (!ValidMove())
        {
            transform.position -= new Vector3(direction, 0, 0);
            return false;
        }
        
        return true;
    }

    public static void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
        }
    }
}