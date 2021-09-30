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
        gameData.BallDirection.Normalize();
    }

    public static void Tick(float dt, ref GameData gameData)
    {
        if (gameData.GameState == GameStateEnum.IN_GAME)
        {
            int numBlocks = gameData.BlockState.Length;
            for (int i = 0; i < numBlocks; i++)
                if (gameData.BlockState[i] == BlockStateEnum.DYING)
                    gameData.BlockState[i] = BlockStateEnum.DEAD;

            Vector2 ballPos = gameData.BallPosition;
            ballPos.y += gameData.BallDirection.y * dt * GameManager.BALL_VELOCITY;
            if (ballPos.y > GameManager.BOARD_LIMIT_Y)
            {
                gameData.BallDirection.y = -gameData.BallDirection.y;
                ballPos.y = gameData.BallPosition.y;
            }
            else if (ballPos.y < gameData.PaddlePosition.y + GameManager.PADDLE_SIZE_Y && (ballPos.x > gameData.PaddlePosition.x - GameManager.PADDLE_SIZE_X && ballPos.x < gameData.PaddlePosition.x + GameManager.PADDLE_SIZE_X))
            {
                gameData.BallDirection.y = -gameData.BallDirection.y;
                gameData.BallDirection.x = (ballPos.x - gameData.PaddlePosition.x) / GameManager.PADDLE_SIZE_X * 2.0f;
                gameData.BallDirection.Normalize();
                ballPos.y = gameData.BallPosition.y;
            }
            else if (ballPos.y < gameData.PaddlePosition.y)
            {
                gameData.GameState = GameStateEnum.BALL_LOST;
            }
            else if (ballPos.y < GameManager.FLOOR_LIMIT_Y)
            {
                gameData.GameState = GameStateEnum.RESTART;
            }
            else if (ApplyBallBlocksCollision(ref gameData))
            {
                gameData.BallDirection.y = -gameData.BallDirection.y;
                ballPos.y = gameData.BallPosition.y;
            }

            ballPos.x += gameData.BallDirection.x * dt * GameManager.BALL_VELOCITY;
            if (ballPos.x < -GameManager.BOARD_LIMIT_X || ballPos.x > GameManager.BOARD_LIMIT_X)
            {
                gameData.BallDirection.x = -gameData.BallDirection.x;
                ballPos.x = gameData.BallPosition.x;
            }
            else if (ApplyBallBlocksCollision(ref gameData))
            {
                gameData.BallDirection.x = -gameData.BallDirection.x;
                ballPos.x = gameData.BallPosition.x;
            }

            gameData.BallPosition = ballPos;
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

    public static bool ApplyBallBlocksCollision(ref GameData gameData)
    {
        int numBlocks = gameData.BlockPositions.Length;
        bool returnValue = false;
        for(int i = 0; i < numBlocks; i++)
            if (gameData.BlockState[i] != BlockStateEnum.DEAD && BallInBlock(gameData.BallPosition, gameData.BlockPositions[i]))
            {
                gameData.Score += 100;
                gameData.BlockState[i] = BlockStateEnum.DYING;
                returnValue = true;
            }
        return returnValue;
    }

    public static bool BallInBlock(Vector2 ballPos, Vector2 blockPos)
    {
        return ((ballPos.x >= blockPos.x - GameManager.BLOCK_SIZE_X && ballPos.x <= blockPos.x + GameManager.BLOCK_SIZE_X) &&
            (ballPos.y >= blockPos.y - GameManager.BLOCK_SIZE_Y && ballPos.y <= blockPos.y + GameManager.BLOCK_SIZE_Y));
    }

    public static void MovePaddle(ref GameData gameData, float x)
    {
        gameData.PaddlePosition.x = x;
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
