using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum MenuStateEnum { START_MENU, IN_GAME, GAME_OVER, VICTORY }; 

public enum GameStateEnum { IN_GAME, BALL_LOST, RESTART };

public enum BlockStateEnum {  ALIVE, DYING, DEAD };

public struct GameData
{
    public sbyte[] Blocks;
    public Vector2[] BlockPositions;
    public BlockStateEnum[] BlockState;

    public Vector2 BallPosition;
    public Vector2 BallDirection;
    public Vector2 PaddlePosition;

    public int NumBalls;
    public int Score;

    public GameStateEnum GameState;
}

public class GameManager : MonoBehaviour
{
    public static int MAX_WIDTH = 13;
    public static int MAX_HEIGHT = 5;
    public static float XOFFSET = 0.64f;
    public static float YOFFSET = 0.32f;

    public static float BLOCK_START_Y = 2.75f;
    public static float BALL_START_Y = -1.75f;
    public static float PADDLE_START_Y = -2.25f;

    public static float BALL_VELOCITY = 7.5f;

    public static float BOARD_LIMIT_X = 4.0f;
    public static float BOARD_LIMIT_Y = 5.6f;
    public static float FLOOR_LIMIT_Y = -5.0f;

    public static float BLOCK_SIZE_X = 0.48f;
    public static float BLOCK_SIZE_Y = 0.31f;
    public static float PADDLE_SIZE_X = 0.8f;
    public static float PADDLE_SIZE_Y = 0.32f;

    [Header("UI")]
    public GameObject UIMainMenu;
    public GameObject UIInGameMenu;
    public GameObject UIGameOverMenu;
    public GameObject UIVictorMenu;
    public GameObject SpritesParent;
    public Text ScoreText;
    public Text BallsText;

    MenuStateEnum menuState;

    public ArkanoidVisual ArkanoidVisual;
    public Camera MainCamera;

    GameData gameData;

    private void Awake()
    {
        gameData = new GameData();
        gameData.Blocks = new sbyte[MAX_WIDTH * MAX_HEIGHT];
        gameData.BlockPositions = new Vector2[MAX_WIDTH * MAX_HEIGHT];
        gameData.BlockState = new BlockStateEnum[MAX_WIDTH * MAX_HEIGHT];
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMenuState(MenuStateEnum.START_MENU);
        //StartGame();
    }

    public void StartGame()
    {
        ArkanoidLogic.Init(ref gameData, 0);
        ArkanoidVisual.Init(gameData, MainCamera);
        SetMenuState(MenuStateEnum.IN_GAME);
    }

    public void SetMenuState(MenuStateEnum newMenuState)
    {
        menuState = newMenuState;
        UIMainMenu.SetActive(menuState == MenuStateEnum.START_MENU);
        UIInGameMenu.SetActive(menuState == MenuStateEnum.IN_GAME);
        UIGameOverMenu.SetActive(menuState == MenuStateEnum.GAME_OVER);
        UIVictorMenu.SetActive(menuState == MenuStateEnum.VICTORY);
        SpritesParent.SetActive(menuState == MenuStateEnum.IN_GAME);
    }

    // Update is called once per frame
    void Update()
    {
        if(menuState == MenuStateEnum.IN_GAME)
        {
            BallsText.text = "Balls left: " + gameData.NumBalls;
            ScoreText.text = "Score: " + gameData.Score;

            ArkanoidVisual.HandleInput(ref gameData);
            ArkanoidLogic.Tick(Time.deltaTime, ref gameData);
            ArkanoidVisual.SyncVisuals(gameData);

            if (gameData.NumBalls <= 0)
                SetMenuState(MenuStateEnum.GAME_OVER);
            else if (ArkanoidLogic.CheckWinCondition(gameData))
                SetMenuState(MenuStateEnum.VICTORY);
        }

    }
}
