using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{
    public GameObject[] Tetrominoes;
    private List<int> bag = new List<int>();
    public static SpawnTetromino Instance;

    private bool spawnTrigger;

    // [Hold 기능용 변수 추가] 
    public int currentBlockIndex;     // 현재 화면에서 떨어지고 있는 블록의 번호
    public int heldBlockIndex = -1;   // 홀드칸에 있는 블록 번호 (-1은 비어있음을 의미)
    public bool canHold = true;       // 1턴 1홀드 제한용 스위치
    private GameObject holdDummy;     // 홀드 구역(허공)에 보여줄 가짜 블록
    public Transform holdPos;

    [Header("Next 블록 세팅")]
    public Transform[] nextAnchors;
    private GameObject[] nextDumies;

   

    /// <summary>
    /// Initializes the class-level singleton Instance; if another instance already exists, destroys this component.
    /// </summary>
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Enables piece spawning and spawns the initial tetromino when the component starts.
    /// </summary>
    void Start()
    {
        spawnTrigger = true;
        nextDumies = new GameObject[nextAnchors.Length];
        NewTetromino();

    }
    void Update()
    {

    }

    /// <summary>
    /// Disables spawning of new tetrominoes by setting the internal spawn trigger to false.
    /// </summary>
    public void TogglespawnTrigger()
    {
        spawnTrigger = false;
    }

    /// <summary>
    /// Spawns the next tetromino from the bag and prepares hold state for the new piece.
    /// </summary>
    /// <remarks>
    /// If spawning is disabled via <c>spawnTrigger</c>, the method does nothing.
    /// If the internal bag is empty it will be refilled before selecting a piece.
    /// The selected tetromino index becomes <c>currentBlockIndex</c>, <c>canHold</c> is set to <c>true</c>,
    /// and the corresponding prefab from <c>Tetrominoes</c> is instantiated at this object's position with no rotation.
    /// </remarks>
    public void NewTetromino()
    {
        if(!spawnTrigger)
            return;

        if (bag.Count <= nextAnchors.Length)
        {
            FillAndShuffleBag();
        }

        // 주머니에서 맨 앞의 블록을 꺼내고 리스트에서 삭제
        int index = bag[0];
        bag.RemoveAt(0);

        //1턴 1HOLD를 구현하기 위함. 새로 꺼낸 블록의 번호를 기억하고, 홀드 스위치를 다시 ON.
        currentBlockIndex = index;
        canHold = true;

        Instantiate(Tetrominoes[index], transform.position, Quaternion.identity);

        
        UpdateNextBlocks();
    }

    /// <summary>
    /// Performs the hold action: stores the current active tetromino in the hold slot or swaps it with the previously held tetromino, and updates the active piece accordingly.
    /// </summary>
    /// <param name="activeBlock">The currently falling tetromino GameObject to remove and place into the hold slot or swap out.</param>
    /// <remarks>
    /// Creates a non-interactive dummy representation of the held tetromino in the hold area, destroys the provided activeBlock, and either spawns a new tetromino when the hold was empty or replaces the active tetromino with the previously held one. Disables further holds until the next spawn by setting <c>canHold</c> to false.
    /// </remarks>
    public void HoldBlock(GameObject activeBlock)
    {
        if (!canHold) return;


        //1. 화면에서 떨어지고 있던 현재 블록 삭제.
        Destroy(activeBlock);

        //2. 홀드 구역에 띄울 가짜(더미) 블록 생성.
        if (holdDummy != null) Destroy(holdDummy); //기존 HOLD칸의 더미 블록 삭제.

        holdDummy = Instantiate(Tetrominoes[currentBlockIndex], holdPos.position, Quaternion.identity);

        holdDummy.transform.localScale = Vector3.one * 0.65f;

        Vector3 realCenter = GetCenter(holdDummy);
        Vector3 offset = holdPos.position - realCenter;
        holdDummy.transform.position += offset;

        holdDummy.GetComponent<TetrisBlock>().enabled = false; // 움직이지 않게 

        //3. 블록 Swap 로직. 비어있었던 경우와, 이미 홀드된 블록이 있는 경우 두가지로 나뉜다.

        //케이스 A : 처음에 홀드 칸이 비어있었던 경우
        if (heldBlockIndex == -1)
        {
            heldBlockIndex = currentBlockIndex;
            NewTetromino();

            canHold = false;
        }

        //케이스 B : 이미 홀드된 블록이 있었던 경우 (서로 교환)
        else
        {
            int temp = currentBlockIndex;
            currentBlockIndex = heldBlockIndex;
            heldBlockIndex = temp;

            Instantiate(Tetrominoes[currentBlockIndex], transform.position, Quaternion.identity);
            canHold = false;
        }

    }

    public void UpdateNextBlocks()
    {
        // 기존에 떠 있던 Next 더미 블록을 전부 파괴해서 초기화
        for (int i = 0; i < nextDumies.Length; i++)
        {
            if (nextDumies[i] != null) Destroy(nextDumies[i]);
        }


        // 보여줄 개수(Anchor)만큼 반복하며 렌더링
        for (int i = 0; i < nextDumies.Length; i++)
        {
            //주머니 부족 버그 방어용 조건문
            if (i < bag.Count)
            {
                int blockIndex = bag[i];
                GameObject dummy = Instantiate(Tetrominoes[blockIndex], nextAnchors[i].position, Quaternion.identity);

                
                dummy.GetComponent<TetrisBlock>().enabled = false;

                dummy.transform.localScale = Vector3.one * 0.65f;

                // 블록의 무게중심 보정
                Vector3 realCenter = GetCenter(dummy);
                Vector3 offset = nextAnchors[i].position - realCenter;
                dummy.transform.position += offset;

                // 다음 턴에 지울 수 있게 배열에 기억해둠
                nextDumies[i] = dummy;
            }
        }
    }
    /// <summary>
    /// Refills the internal bag with one copy of each tetromino index (0–6) and randomizes their order.
    /// </summary>
    private void FillAndShuffleBag()
    {
        List<int> newBag = new List<int>();
        for (int i = 0; i < 7; i++) newBag.Add(i);
        


        // 2. Fisher - Yates 알고리즘으로 리스트를 무작위로 섞음
        for (int i = 0; i < newBag.Count; i++)
        {
            int randomIndex = Random.Range(i, newBag.Count);
            int temp = newBag[i];
            newBag[i] = newBag[randomIndex];
            newBag[randomIndex] = temp;
        }

        bag.AddRange(newBag);
    }

    private Vector3 GetCenter(GameObject block)
    {
        Vector3 CenterPos = Vector3.zero;
        foreach (Transform item in block.transform)
        {
            CenterPos += item.position;
        }
        return CenterPos / block.transform.childCount;
    }

  
}
