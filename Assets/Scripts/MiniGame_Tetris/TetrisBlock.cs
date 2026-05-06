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

    private int rotationState = 0;

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

        //1 순위 입력. HOLD 입력
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //SpawnTetromino 한테 자신을 넘기며 홀드 요청
            SpawnTetromino.Instance.HoldBlock(this.gameObject);
            return;
        }


        //2 순위 입력. 블록 회전
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (type == BlockType.O) return;

            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);

            // 회전하는데 벽에 걸린 경우
            if (!ValidMove())
            {

                if (!PerformWallKick(rotationState))
                {
                    transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
                    return;
                }

            }

            rotationState = (rotationState + 1) % 4;
        }

        
        //3 순위 입력. 블록 좌우
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

        // 키를 '꾹' 누르고 있을때
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

        //4 순위 입력. 블록 하강
        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime))
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove())
            {
                transform.position += new Vector3(0, 1, 0);
             
                AddToGrid();
                int cleared = CheckForLines();
                LineClearEventManager.Instance?.ProcessLineClear(cleared);

                this.enabled = false;

                SpawnTetromino.Instance.NewTetromino();
                return;
            }
            previousTime = Time.time;
        }


        

        

       
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
        for (int j = 0; j < width; j++)
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
                            Debug.Log("I_enable 블록의 파편을 찾았습니다");
                            parentBlock.type = BlockType.I_disable;
                            //TODO 타마마 임펙트 처리

                            foreach (Transform sibling in parentTransform)
                            {
                                // 이번에 지워질 자기 자신은 어차피 곧 파괴되니 색칠할 필요 없음
                                if (sibling != cell)
                                {
                                    if (sibling.TryGetComponent(out SpriteRenderer sr))
                                    {
                                        sr.color = Color.gray; // 살아남은 파편들은 회색으로 굳어버림!
                                    }
                                }
                            }
                        }
                    }
                }
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
        for (int y = i + 1; y < height; y++)
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
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedY >= height)
            {
                Debug.LogWarning("블록이 천장을 뚫었습니다. GAME OVER");
                // TODO
                // 게임 오버시 처리
                SpawnTetromino.Instance.TogglespawnTrigger();

                continue;
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
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

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
    }
}
