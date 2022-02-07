using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArkanoidVisual : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject Paddle;
    public GameObject Ball;

    public Transform SpritesParent;
    SpriteRenderer[] m_blocksSR;
    Transform[] m_blocksT;
    Animation[] m_blocksA;

    public Color[] BlockColors;

    public LineRenderer BallPath;

    Camera m_mainCamera;
    Vector3 m_mouseDownPosition;

    private void Awake()
    {
        m_blocksSR = new SpriteRenderer[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];
        m_blocksT = new Transform[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];
        m_blocksA = new Animation[GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT];

        for (int i = 0; i < GameManager.MAX_WIDTH; i++)
        {
            for (int j = 0; j < GameManager.MAX_HEIGHT; j++)
            {
                int index = i + j * GameManager.MAX_WIDTH;
                GameObject block = Instantiate(BlockPrefab, SpritesParent);
                block.transform.localScale = Vector3.one;
                m_blocksSR[index] = block.GetComponentInChildren<SpriteRenderer>();
                m_blocksA[index] = block.GetComponentInChildren<Animation>();
                m_blocksT[index] = block.transform;
            }
        }

        BallPath.enabled = false;
    }

    public void Init(GameData gameData, Camera mainCamera)
    {
        m_mainCamera = mainCamera;
        for (int i = 0; i < GameManager.MAX_WIDTH; i++)
        {
            for (int j = 0; j < GameManager.MAX_HEIGHT; j++)
            {
                int index = i + j * GameManager.MAX_WIDTH;
                m_blocksT[index].transform.localPosition = gameData.BlockPositions[index];
                m_blocksSR[index].color = BlockColors[gameData.Blocks[index]];
                m_blocksA[index].Play("Block Idle");
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
                m_blocksA[i].Play("Block Remove");

        for (int i = 0; i < BallPath.positionCount; i++)
            BallPath.SetPosition(i, gameData.BallPosition);
        Vector2 ballPos;
        if (ArkanoidLogic.BallWillHitPaddle(gameData, out ballPos))
        {
            BallPath.SetPosition(1, gameData.PaddlePosition);
            Vector2 direction = ArkanoidLogic.GetPaddleCollisionDirection(gameData, ballPos);
            BallPath.SetPosition(2, ballPos + direction * 5.0f);
        }
        else
        {
            BallPath.SetPosition(1, gameData.BallPosition + gameData.BallDirection * 10.0f);
            BallPath.SetPosition(2, gameData.BallPosition + gameData.BallDirection * 10.0f);
        }

        BallPath.enabled = (gameData.BallDirection.y < 0.0f);
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
