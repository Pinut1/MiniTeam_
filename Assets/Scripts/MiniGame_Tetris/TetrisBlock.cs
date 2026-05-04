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


    void Update()
    {

        //블록 하강
        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime))
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove())
            {
                transform.position += new Vector3(0, 1, 0);
             
                AddToGrid();
                CheckForLines();

                this.enabled = false;

                SpawnTetromino.Instance.NewTetromino();
            }
            previousTime = Time.time;
        }


        //블록 좌우
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!ValidMove())
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }

        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position += new Vector3(1, 0, 0);
            if (!ValidMove())
            {
                transform.position += new Vector3(-1, 0, 0);
            }
        }

        //블록 회전
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (type == BlockType.O) return;
            
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0,0,1), -90);

            // 회전하는데 벽에 걸린 경우
            if (!ValidMove())
            {

                if(!PerformWallKick(rotationState))
                {
                    transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
                    return;
                }

            }

            rotationState = (rotationState + 1) % 4;
        }
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

    private void CheckForLines()
    {
        for (int i = height-1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);

                i++;
            }
        }
    }


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
            }
            Destroy(cell.gameObject);
            grid[j, i] = null;
        }
    }
    private void RowDown(int i)
    {
        for (int y = i; y < height; y++)
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

    //배경을 벗어나지 않도록 체크하는 메서드
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

}
