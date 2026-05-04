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

    void Start()
    {
        spawnTrigger = true;
        NewTetromino();
    }
    void Update()
    {

    }

    public void TogglespawnTrigger()
    {
        spawnTrigger = false;
    }

    // 다음 블록을 뽑는 메서드
    public void NewTetromino()
    {
        if(!spawnTrigger)
            return;

        if (bag.Count == 0)
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
    }

    public void HoldBlock(GameObject activeBlock)
    {
        if (!canHold) return;


        //1. 화면에서 떨어지고 있던 현재 블록 삭제.
        Destroy(activeBlock);

        //2. 홀드 구역에 띄울 가짜(더미) 블록 생성.
        if (holdDummy != null) Destroy(holdDummy); //기존 HOLD칸의 더미 블록 삭제.

        holdDummy = Instantiate(Tetrominoes[currentBlockIndex], holdPos.position, Quaternion.identity);
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

    // 주머니를 채우고 섞는 메서드
    private void FillAndShuffleBag()
    {
        // 1. 주머니에 0번부터 6번까지 총 7개의 인덱스를 채워 넣음
        for (int i = 0; i < 7; i++)
        {
            bag.Add(i);
        }


        // 2. Fisher - Yates 알고리즘으로 리스트를 무작위로 섞음
        for (int i = 0; i < bag.Count; i++)
        {
            int randomIndex = Random.Range(i, bag.Count);
            int temp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }
    }
}
