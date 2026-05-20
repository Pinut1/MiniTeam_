using MiniTeam.Tetris;
using System;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public enum BlockType { Normal, I_enable,I_disable, O }
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

    public float lockDelay = 0.5f;
    private float lockTimer = 0f;

    private int rotationState = 0;
    public GameObject tamamaBeamPrefab;

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

        //spawn
        if (!ValidMove())
        {
            Debug.Log(" GAME OVER!");

            SpawnTetromino.Instance.TogglespawnTrigger();

            this.enabled = false;

            //TODO : 

        }

    }


    /// <summary>
    /// Processes player input and timed gravity for the active tetromino.
    /// </summary>
    /// <remarks>
    /// - Advances the piece downward on a timed interval (accelerated while DownArrow is held). 
    /// - Moves the piece left/right on LeftArrow/RightArrow presses.
    /// - Rotates the piece on UpArrow (no rotation for O blocks); if rotation collides, attempts wall-kick adjustments.
    /// - When the piece can no longer descend, locks it into the playfield, clears completed lines, disables this component, and requests a new tetromino spawn.
    /// - Sends a hold request to the spawn manager when LeftShift is pressed.
    /// </remarks>
    void Update()
    {
        // 입력 1순위 : HOLD (가장 먼저 검사)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SpawnTetromino.Instance.HoldBlock(this.gameObject);
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (type != BlockType.O)
            {
                // 일단 돌려봄
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
                bool rotationSuccess = true;

                // 벽에 걸렸다면 벽차기 시도
                if (!ValidMove())
                {
                    if (!PerformWallKick(rotationState))
                    {
                        // 벽차기마저 실패하면 원상복구
                        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
                        rotationSuccess = false;
                    }
                }

                // 회전에 최종적으로 성공했을 때만 상태값 증가
                if (rotationSuccess)
                {
                    rotationState = (rotationState + 1) % 4;
                    lockTimer = 0f;
                }
            }
        }

        // ⭐️ 3순위: 좌우 이동 (독립된 if문. 좌/우끼리만 동시 입력 안 되게 else if로 묶음)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveHorizontal(-1);
            horizontalTimer = Time.time + das;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveHorizontal(1);
            horizontalTimer = Time.time + das;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Time.time > horizontalTimer)
            {
                MoveHorizontal(-1);
                horizontalTimer = Time.time + arr;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Time.time > horizontalTimer)
            {
                MoveHorizontal(1);
                horizontalTimer = Time.time + arr;
            }
        }

        // ⭐️ 4순위: 하강 로직 (독립된 if문)
        // 가상으로 아래로 한 칸 움직여서 바닥(또는 다른 블록)에 닿는지 검사
        transform.position += new Vector3(0, -1, 0);
        bool canDrop = ValidMove();
        transform.position += new Vector3(0, 1, 0);     // 제자리로 복구

        if (!canDrop)
        {
            // 바닥에 닿은 상태면 유예 시간(Lock 타이머) 시작
            lockTimer += Time.deltaTime;

            if (lockTimer >= lockDelay)
            {
                LockBlock();
                return;
            }
        }
        else
        {
            // 바닥에서 떨어져서 다시 공중에 떴다면 타이머 완벽 리셋
            lockTimer = 0f;

            if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10f : fallTime))
            {
                transform.position += new Vector3(0, -1, 0);
                previousTime = Time.time;
            }
        }
        //if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10f : fallTime))
        //{
        //    transform.position += new Vector3(0, -1, 0);

        //    // 바닥이나 다른 블록에 닿았을 때
        //    if (!ValidMove())
        //    {
        //        transform.position += new Vector3(0, 1, 0); // 닿기 직전으로 원상복구

        //        AddToGrid();
        //        int cleared = CheckForLines();
        //        LineClearEventManager.Instance?.ProcessLineClear(cleared); // Null-safe 호출!

        //        // 메모리 누수 방지 (자식들은 놔두고 부모 껍데기만 깔끔하게 파괴)
        //        // I_enable 특수 블록은 나중에 폭발 검사를 받아야 하니 껍데기를 살려둠
               
        //        if (type != BlockType.I_enable)
        //        {
        //            transform.DetachChildren();
        //            Destroy(gameObject);
        //        }
        //        else
        //        {
                    
        //            this.enabled = false;
        //        }

        //        SpawnTetromino.Instance.NewTetromino();
        //        return;
        //    }

        //    // 무사히 한 칸 떨어졌다면 타이머 리셋
        //    previousTime = Time.time;
        //}

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

    /// <summary>
    /// Attempts positional adjustments (wall kicks) using the appropriate kick table to find a valid placement after a rotation.
    /// </summary>
    /// <param name="currentState">Current rotation state index (0–3) used to select the kick offsets.</param>
    /// <returns>`true` if applying any kick offset produces a valid placement, `false` otherwise.</returns>
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

    /// <summary>
    /// Scans the board for completed horizontal lines, removes each full line, and drops the rows above down to fill the gap.
    /// </summary>
    /// <remarks>
    /// The method iterates rows from the top of the playfield to the bottom. After deleting a line and shifting rows down, it rechecks the same row index to detect consecutive cleared lines that moved into this row.
    /// </remarks>
    private int CheckForLines()
    {
        int clearedLines = 0;

        for (int i = height-1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
                clearedLines++;
                i++;
            }
        }
        return clearedLines;
    }


    /// <summary>
    /// Determines whether the specified row is completely filled with blocks.
    /// </summary>
    /// <param name="i">The zero-based row index to check (0 = bottom row).</param>
    /// <returns>`true` if every column in row <paramref name="i"/> contains a block, `false` otherwise.</returns>
    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            if (grid[j,i] == null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Removes all occupied cells in the specified row, clears their grid entries, and applies special handling for fragments of `I_enable` blocks.
    /// </summary>
    /// <param name="i">Index of the row to delete (0-based, 0 is the bottom row).</param>
    private void DeleteLine(int i)
    {
        // 타마마 임팩트를 1번만 체크하기 위한 Trigger
        bool hasTriggeredEffect = false;

        for (int j = width - 1; j >= 0; j--)
        {
            Transform cell = grid[j, i];
            if (cell != null) // 안전장치
            {
                Transform parentTransform = cell.parent;

                if (parentTransform != null)
                {
                    if (parentTransform.TryGetComponent(out TetrisBlock parentBlock))
                    {
                        if (parentBlock.type == BlockType.I_enable)
                        {
                            // 블록이 가로인지 세로인지 판별
                            // 자식(파편) 2개를 잡아, X축으로 떨어져 있는지 Y축으로 떨어져 있는지 검사.
                            bool isHorizontal = true;
                            if (parentTransform.childCount >= 2)
                            {
                                Transform child1 = parentTransform.GetChild(0);
                                Transform child2 = parentTransform.GetChild(1);

                                isHorizontal = Mathf.Abs(child1.position.x - child2.position.x)
                                                > Mathf.Abs(child1.position.y - child2.position.y);
                            }

                            // 1. 살아남을 파편(형제들) 색상을 회색으로 변경
                            foreach (Transform sibling in parentTransform)
                            {
                                if (sibling != cell)
                                {
                                    if (sibling.TryGetComponent(out SpriteRenderer sr))
                                    {
                                        sr.color = Color.gray; // 살아남은 파편들은 회색으로 굳어버림!
                                    }
                                }
                            }

                            parentTransform.DetachChildren();
                            Destroy(parentTransform.gameObject);

                            // 2. 타마마 임팩트 발동
                            if (!hasTriggeredEffect)
                            {
                                if (isHorizontal)
                                {
                                    Debug.Log($"[{j}번째 열] ➡️ [가로] 방향 I_enable 파편 폭발! (가로 빔 발사!)");
                                    // TODO 가로 전용 타마마 임팩트
                                    
                                }
                                else
                                {
                                    
                                    // grid의 오른쪽부터 스캔하기 때문에, 무조건 가장 오른쪽의 블록에서 발동.
                                    Debug.Log($"[{j}번째 열] I_enable 블록 파편 발견! 타마마 임팩트 발동!");
                                    //TODO 타마마 임팩트
                                    float distanceToWall = j;


                                    // 1. 빔 프리팹 생성 (cell.position, 즉 블록 파편의 위치에서 생성)
                                    Vector3 spawnPosition = cell.position + new Vector3(-1.8f, 0, 0);
                                    GameObject beamObj = Instantiate(tamamaBeamPrefab, spawnPosition, Quaternion.identity);

                                    // 2. 생성된 빔에게 거리 전달하여 스케일/위치 맞추기
                                    if (beamObj.TryGetComponent(out TamamaBeam beamScript))
                                    {
                                        beamScript.Setup(distanceToWall);
                                    }

                                    if (TetrisGameController.Instance != null)
                                    {
                                        TetrisGameController.Instance.OnTamamaImpactTriggered();
                                    }
                                }
                                hasTriggeredEffect = true;
                            }
                        }

                    }
                }

                // 3. 줄이 지워지는 해당 칸 파괴
                Destroy(cell.gameObject);
                grid[j, i] = null;
            }
        }
    }
    /// <summary>
    /// Moves every occupied cell at or above the specified row down by one row in the grid.
    /// </summary>
    /// <param name="i">The starting row index (inclusive); all occupied cells in row <c>i</c> and above are shifted down one row. This updates both the static grid references and each moved transform's world position.</param>
    private void RowDown(int i)
    {
        for (int y = i ; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j,y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    /// <summary>
    /// Stores each child square of this tetromino into the static playfield grid at its rounded world coordinates.
    /// </summary>
    /// <remarks>
    /// Rounds each child's world X/Y to integer grid coordinates before placement. If a child's rounded Y is equal to or above the grid height, the method triggers the spawn/game-over handler and skips placing that cell.
    /// </remarks>
    /// <seealso cref="SpawnTetromino.Instance.TogglespawnTrigger"/>
    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x - 0.2f);
            int roundedY = Mathf.RoundToInt(children.transform.position.y - 0.2f);

            if (roundedY >= height)
            {
                Debug.LogWarning("블록이 천장을 뚫었습니다. GAME OVER");
                // TODO
                // 게임 오버시 처리
                SpawnTetromino.Instance.TogglespawnTrigger();
                TetrisGameController.Instance.OnGameFail();
                return;
            }

            grid[roundedX, roundedY] = children;

        }
    }

    /// <summary>
    /// Determines whether all child blocks of this tetromino are inside the playfield bounds and not colliding with occupied grid cells.
    /// </summary>
    /// <returns>`true` if every child block's rounded X is between 0 and width - 1, rounded Y is greater than or equal to 0, and no occupied grid cell exists at the rounded coordinates (overlap check is performed only when the rounded Y is less than height); `false` otherwise.</returns>
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
    private void MoveHorizontal(int direction)
    {
        transform.position += new Vector3(direction, 0, 0);

        if (!ValidMove())
        {
            transform.position -= new Vector3(direction, 0, 0);
        }
        else
        {
            lockTimer = 0f;
        }
    }
}
