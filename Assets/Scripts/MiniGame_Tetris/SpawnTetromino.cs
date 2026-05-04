using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTetromino : MonoBehaviour
{
    public GameObject[] Tetrominoes;
    private List<int> bag = new List<int>();
    public static SpawnTetromino Instance;

    private bool spawnTrigger;

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

        Instantiate(Tetrominoes[index], transform.position, Quaternion.identity);
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
