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

    public float lockDelay = 0.5f;
    private float lockTimer = 0f;

    public int rotationState = 0;

    [Header("타마마 임팩트 이펙트")]
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

        if (!ValidMove())
        {
            Debug.Log(" GAME OVER!");
            SpawnTetromino.Instance.TogglespawnTrigger();
            this.enabled = false;
        }
    }

    void Update()
    {
        // 1순위 : HOLD
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SpawnTetromino.Instance.HoldBlock(this.gameObject);
            return;
        }

        // 2순위 : 회전 (I_enable 블록은 2상태로 제한)
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
                    lockTimer = 0f;
                }
            }
        }

        // 3순위: 좌우 이동 
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

        // 4순위: 하강 로직
        transform.position += new Vector3(0, -1, 0);
        bool canDrop = ValidMove();
        transform.position += new Vector3(0, 1, 0);

        if (!canDrop)
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
            lockTimer = 0f;

            if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10f : fallTime))
            {
                transform.position += new Vector3(0, -1, 0);
                previousTime = Time.time;
            }
        }
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

                            // 옛날 버전 확인 후 바로 지우자 05222342
                            //if (parentTransform.childCount >= 2)
                            //{
                            //    Transform child1 = parentTransform.GetChild(0);
                            //    Transform child2 = parentTransform.GetChild(1);

                            //    isHorizontal = Mathf.Abs(child1.position.x - child2.position.x) > Mathf.Abs(child1.position.y - child2.position.y);
                            //}

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
                                    distanceToWall = 25f; // 
                                    beamRotation = Quaternion.Euler(0, 0, -90f);
                                }

                                // 빔 프리팹 생성 및 세팅
                                if (tamamaBeamPrefab != null)
                                {
                                    GameObject beamObj = Instantiate(tamamaBeamPrefab, spawnPosition, beamRotation);
                                    if (beamObj.TryGetComponent(out TamamaBeam beamScript))
                                    {
                                        beamScript.Setup(distanceToWall);
                                    }
                                }

                                if (TetrisGameController.Instance != null)
                                {
                                    Debug.Log("TetrisBlock에서 OnTamamaImpactTriggered 호출");
                                    TetrisGameController.Instance.OnTamamaImpactTriggered(isHorizontal);
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