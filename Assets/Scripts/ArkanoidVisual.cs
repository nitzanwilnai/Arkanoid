using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArkanoidVisual : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject Paddle;
    public GameObject Ball;

    public Transform SpritesParent;
    SpriteRenderer[] blocksSR;
    Transform[] blocksT;
    Animation[] blocksA;

    public Color[] BlockColors;

    Camera m_mainCamera;
    Vector3 m_mouseDownPosition;

    private void Awake()
    {
        blocksSR = new SpriteRenderer[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];
        blocksT = new Transform[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];
        blocksA = new Animation[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];

        for (int i = 0; i < GameManager.MAX_WIDTH; i++)
        {
            for (int j = 0; j < GameManager.MAX_HEIGHT; j++)
            {
                int index = i + j * GameManager.MAX_WIDTH;
                GameObject block = Instantiate(BlockPrefab, SpritesParent);
                block.transform.localScale = Vector3.one * 0.5f;
                blocksSR[index] = block.GetComponentInChildren<SpriteRenderer>();
                blocksA[index] = block.GetComponentInChildren<Animation>();
                blocksT[index] = block.transform;
            }
        }
    }

    public void Init(GameData gameData, Camera mainCamera)
    {
        m_mainCamera = mainCamera;
        for (int i = 0; i < GameManager.MAX_WIDTH; i++)
        {
            for (int j = 0; j < GameManager.MAX_HEIGHT; j++)
            {
                int index = i + j * GameManager.MAX_WIDTH;
                blocksT[index].transform.localPosition = gameData.BlockPositions[index];
                blocksSR[index].color = BlockColors[gameData.Blocks[index]];
                blocksA[index].Play("Block Idle");
            }
        }

        Paddle.transform.localPosition = gameData.PaddlePosition;
        Ball.transform.localPosition = gameData.BallPosition;
    }

    // Update is called once per frame
    public void SyncVisuals(GameData gameData)
    {
        Paddle.transform.localPosition = gameData.PaddlePosition;
        Ball.transform.localPosition = gameData.BallPosition;

        int numBlocks = gameData.BlockState.Length;
        for(int i = 0; i < numBlocks; i++)
            if (gameData.BlockState[i] == BlockStateEnum.DYING)
                blocksA[i].Play("Block Remove");
        //blocksT[i].gameObject.SetActive(false);
    }

    public void HandleInput(ref GameData gameData)
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = m_mainCamera.ScreenToWorldPoint(mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            m_mouseDownPosition = worldPosition;
            ArkanoidLogic.MovePaddle(ref gameData, worldPosition.x);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 offset = worldPosition - m_mouseDownPosition;
            ArkanoidLogic.MovePaddle(ref gameData, worldPosition.x);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ArkanoidLogic.MovePaddle(ref gameData, worldPosition.x);
        }
    }
}
