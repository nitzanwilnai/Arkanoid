using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArkanoidLogic
{
    public static int Seed;
    public static int CustomRandInt()
    {
        Seed = (214013 * Seed + 2531011);
        return (Seed >> 16) & 0x7FFF;
    }
    public static float CustomRandFloat()
    {
        Seed = (214013 * Seed + 2531011);
        return (float)((Seed >> 16) & 0x7FFF) / (32768.0f);
    }

    public static void Init(ref GameData gameData, int seed)
    {
        Seed = seed;

        gameData.GameState = GameStateEnum.IN_GAME;

        float startX = -(Mathf.Floor(GameManager.MAX_WIDTH / 2.0f) - 0.5f) * GameManager.XOFFSET;
        float startY = GameManager.BLOCK_START_Y;
        for (int i = 0; i < GameManager.MAX_WIDTH * GameManager.MAX_HEIGHT; i++)
        {
            gameData.Blocks[i] = (sbyte)(i / GameManager.MAX_WIDTH);
            int x = i % GameManager.MAX_WIDTH;
            int y = i / GameManager.MAX_WIDTH;
            gameData.BlockPositions[i] = new Vector2(startX + x * GameManager.XOFFSET, startY + y * GameManager.YOFFSET);
            gameData.BlockState[i] = BlockStateEnum.ALIVE;
        }

        gameData.PaddlePosition = new Vector2(0.0f, GameManager.PADDLE_START_Y);

        gameData.NumBalls = 3;
        gameData.Score = 0;

        ResetBall(ref gameData);
    }

    public static void ResetBall(ref GameData gameData)
    {
        gameData.BallPosition = new Vector2(0.0f, GameManager.BALL_START_Y);
        gameData.BallDirection = new Vector2(CustomRandFloat() - 0.5f, 0.5f);
        //gameData.BallDirection.Normalize();
    }

    public static void Tick(float dt, ref GameData gameData)
    {
        if (gameData.GameState == GameStateEnum.IN_GAME)
        {
            int numBlocks = gameData.BlockState.Length;
            for (int i = 0; i < numBlocks; i++)
                if (gameData.BlockState[i] == BlockStateEnum.DYING)
                    gameData.BlockState[i] = BlockStateEnum.DEAD;

            gameData.BallPosition = moveBall(dt, ref gameData);
        }
        if (gameData.GameState == GameStateEnum.BALL_LOST)
        {
            gameData.BallPosition += gameData.BallDirection * dt * GameManager.BALL_VELOCITY;
            if (gameData.BallPosition.y < GameManager.FLOOR_LIMIT_Y)
            {
                gameData.GameState = GameStateEnum.RESTART;
            }
        }
        else if (gameData.GameState == GameStateEnum.RESTART)
        {
            gameData.NumBalls--;
            if(gameData.NumBalls > 0)
                ResetBall(ref gameData);
            gameData.GameState = GameStateEnum.IN_GAME;
        }
    }

    static Vector2 moveBall(float dt, ref GameData gameData)
    {
        Vector2 currentBallPos = gameData.BallPosition;

        Vector2 newBallPos = currentBallPos;
        newBallPos.y = currentBallPos.y + gameData.BallDirection.y * dt * GameManager.BALL_VELOCITY;
        if (newBallPos.y > GameManager.BOARD_LIMIT_Y)
        {
            gameData.BallDirection.y = -gameData.BallDirection.y;
            newBallPos.y = currentBallPos.y;
        }
        else if (BallInPaddle(newBallPos, gameData.PaddlePosition))
        {
            gameData.BallDirection = GetPaddleCollisionDirection(gameData, newBallPos);
            newBallPos.y = currentBallPos.y;
        }
        else if (newBallPos.y < gameData.PaddlePosition.y)
        {
            gameData.GameState = GameStateEnum.BALL_LOST;
        }
        else if (newBallPos.y < GameManager.FLOOR_LIMIT_Y)
        {
            gameData.GameState = GameStateEnum.RESTART;
        }
        else if (BallBlocksCollision(ref gameData, newBallPos))
        {
            gameData.BallDirection.y = -gameData.BallDirection.y;
            newBallPos.y = currentBallPos.y;
        }

        newBallPos.x += gameData.BallDirection.x * dt * GameManager.BALL_VELOCITY;
        if (newBallPos.x < -GameManager.BOARD_LIMIT_X || newBallPos.x > GameManager.BOARD_LIMIT_X)
        {
            gameData.BallDirection.x = -gameData.BallDirection.x;
            newBallPos.x = currentBallPos.x;
        }
        else if (BallBlocksCollision(ref gameData, newBallPos))
        {
            gameData.BallDirection.x = -gameData.BallDirection.x;
            newBallPos.x = currentBallPos.x;
        }

        return newBallPos;
    }

    public static bool BallBlocksCollision(ref GameData gameData, Vector2 newBallPos)
    {
        int numBlocks = gameData.BlockPositions.Length;
        bool returnValue = false;
        for(int i = 0; i < numBlocks; i++)
            if (gameData.BlockState[i] != BlockStateEnum.DEAD && ballInBlock(newBallPos, gameData.BlockPositions[i]))
            {
                gameData.Score += 100;
                gameData.BlockState[i] = BlockStateEnum.DYING;
                returnValue = true;
            }
        return returnValue;
    }
    public static bool BallInPaddle(Vector2 ballPos, Vector2 paddlePosition)
    {
        return (ballPos.y < paddlePosition.y + GameManager.PADDLE_SIZE_Y && (ballPos.x > paddlePosition.x - GameManager.PADDLE_SIZE_X && ballPos.x < paddlePosition.x + GameManager.PADDLE_SIZE_X));
    }

    static bool ballInBlock(Vector2 ballPos, Vector2 blockPos)
    {
        return ((ballPos.x >= blockPos.x - GameManager.BLOCK_SIZE_X && ballPos.x <= blockPos.x + GameManager.BLOCK_SIZE_X) &&
            (ballPos.y >= blockPos.y - GameManager.BLOCK_SIZE_Y && ballPos.y <= blockPos.y + GameManager.BLOCK_SIZE_Y));
    }
    
    public static void MovePaddle(ref GameData gameData, float x)
    {
        gameData.PaddlePosition.x = x;
    }

    public static bool BallWillHitPaddle(GameData gameData, out Vector2 ballPos)
    {
        ballPos = gameData.BallPosition;
        float dt = 1.0f / 60.0f;
        if(gameData.BallDirection.y < 0.0f)
        {
            while(ballPos.y > GameManager.FLOOR_LIMIT_Y)
            {
                ballPos += gameData.BallDirection * GameManager.BALL_VELOCITY * dt;
                if (BallInPaddle(ballPos, gameData.PaddlePosition))
                    return true;
            }
        }
        return false;
    }

    public static Vector2 GetPaddleCollisionDirection(GameData gameData, Vector2 ballPos)
    {
        Vector2 newDirection;
        newDirection.y = -gameData.BallDirection.y;
        newDirection.x = (ballPos.x - gameData.PaddlePosition.x) / GameManager.PADDLE_SIZE_X * 2.0f;
        newDirection.x = Mathf.Clamp(newDirection.x, -0.5f, 0.5f);
        return newDirection;
    }

    public static bool CheckWinCondition(GameData gameData)
    {
        int numBlocks = gameData.BlockPositions.Length;
        for (int i = 0; i < numBlocks; i++)
            if (gameData.BlockState[i] != BlockStateEnum.DEAD)
                return false;
        return true;
    }
}
