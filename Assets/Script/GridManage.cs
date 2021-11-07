using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GridManage : MonoBehaviour
{
    private static int rows = 4;
    private static int cols = 4;
    
    // 随机生成的图块值大小
    private static int lowestNewTileValue = 2;
    private static int highestNewTileValue = 4;
    
    private static float borderOffset = 0.05f;
    private static float horizontalSpacingOffset = -1.65f;
    private static float verticalSpacingOffset = 1.65f;
    // 边框间距 
    private static float borderSpacing = 0.1f;
    // 半平铺宽度
    private static float halfTileWidth = 0.55f;
    // 瓦片间的空间
    private static float spaceBetweenTiles = 1.1f;
    
    // 分数
    private int points;
    private List<GameObject> tiles;
    // 重置按钮
    private Rect resetButton;
    // 游戏结束按钮
    private Rect gameOverButton;
    // 触摸起始位置
    private Vector2 touchStartPosition = Vector2.zero;  
    // 最大值
    public int maxValue = 2048;
    
    public GameObject gameOverPanel;
    public GameObject noTile;
    // 分数文本
    public Text scoreText;
    // 瓦片预制体
    public GameObject[] tilePrefabs;
    // 背景层
    public LayerMask backgroundLayer;
    // 最小滑动距离
    public float minSwipeDistance = 10.0f;
    
    // 枚举（状态）
    private enum State {
        Loaded, 
        WaitingForInput, 
        CheckingMatches,
        GameOver
    }
    // 状态
    private State state;
    
    void Awake()
    {
        tiles = new List<GameObject>();
        state = State.Loaded;
    }
    
    void Update()
    {
        switch (state)
        {
            case State.GameOver:
                gameOverPanel.SetActive(true);
                break;
            case State.Loaded:
                state = State.WaitingForInput;
                break;
            case State.WaitingForInput:
                InputKeyBox();
                break;
            case State.CheckingMatches:
                // 生成随机图块
                GenerateRandomTile();
                if (CheckForMovesLeft()) {
                    ReadyTilesForUpgrading();
                    state = State.WaitingForInput;
                } else {
                    state = State.GameOver;
                }
                break;
        }
    }

    /**
     * 监听键盘按下按键
     */
    private void InputKeyBox()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            if (MoveTilesLeft()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.D)) {
            if (MoveTilesRight()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.W)) {
            if (MoveTilesUp()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.S)) {
            if (MoveTilesDown()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.Q)) {
            Reset();
        } else if (Input.GetKeyDown(KeyCode.R)) {
            Application.Quit();
        }
    }

    /**
     * 向左移动
     */
    private bool MoveTilesLeft()
    {
        return false;
    }
    /**
     * 向右移动
     */
    private bool MoveTilesRight()
    {
        return false;
    }
    /**
     * 向上移动
     */
    private bool MoveTilesUp()
    {
        return false;
    }
    /**
     * 向下移动
     */
    private bool MoveTilesDown()
    {
        return false;
    }

    /**
     * 重置方法
     */
    public void Reset()
    {
        gameOverPanel.SetActive(false);
        foreach (var tile in tiles) {
            SimplePool.Despawn(tile);
        }
        tiles.Clear();
        points = 0;
        scoreText.text = "0";
        state = State.Loaded;
    }
    
    /**
     * 生成随机图块
     */
    public void GenerateRandomTile()
    {
        // 校验图块个数
        if (tiles.Count >= rows * cols) {
            throw new UnityException("生成的图块过多，运行异常");
        }
        // 生成最新图块的值 90%-2 10%-4
        int value;
        float highOrLowChance = Random.Range(0f, 0.99f);
        if (highOrLowChance >= 0.9f) {
            value = highestNewTileValue;
        } else {
            value = lowestNewTileValue;
        }
        // 随机生成初始位置
        int x = Random.Range(0, cols);
        int y = Random.Range(0, rows);
        
        // 校验该位置是否为空
        bool found = false;
        while (!found) {
            if (GetObjectAtGridPosition(x, y) == noTile) {
                found = true;
                Vector2 worldPosition = GridToWorldPoint(x, y);
                GameObject obj;
                if (value == lowestNewTileValue) {
                    obj = SimplePool.Spawn(tilePrefabs[0], worldPosition, transform.rotation);
                } else {
                    obj = SimplePool.Spawn(tilePrefabs[1], worldPosition, transform.rotation);
                }
        
                tiles.Add(obj);
                TileAnimationHandler tileAnimManager = obj.GetComponent<TileAnimationHandler>();
                tileAnimManager.AnimateEntry();
            }
      
            x++;
            if (x >= cols) {
                y++;
                x = 0;
            }
      
            if (y >= rows) {
                y = 0;
            }
        }
    }

    /**
     * 校验是否能向左移动
     */
    private bool CheckForMovesLeft()
    {
        // 校验是否满格了
        if (tiles.Count < rows * cols) {
            return true;
        }
        // 校验是否还能有下一步移动
        for (int x = 0; x < cols; x++) {
            for (int y = 0; y < rows; y++) {
                Tile currentTile = GetObjectAtGridPosition(x, y).GetComponent<Tile>();
                Tile rightTile = GetObjectAtGridPosition(x + 1, y).GetComponent<Tile>();
                Tile downTile = GetObjectAtGridPosition(x, y + 1).GetComponent<Tile>();
        
                if (x != cols - 1 && currentTile.value == rightTile.value) {
                    return true;
                } else if (y != rows - 1 && currentTile.value == downTile.value) {
                    return true;
                }
            }
        }
        
        return false;
    }
    /**
     * 为瓦片升级
     */
    private void ReadyTilesForUpgrading()
    {
        foreach (var obj in tiles) {
            Tile tile = obj.GetComponent<Tile>();
            tile.upgradedThisTurn = false;
        }
    }
    /**
     * 获取物体网格位置
     */
    private GameObject GetObjectAtGridPosition(int x, int y) {
        RaycastHit2D hit = Physics2D.Raycast(GridToWorldPoint(x, y), Vector2.right, borderSpacing);
    
        if (hit && hit.collider.gameObject.GetComponent<Tile>() != null) {
            return hit.collider.gameObject;
        } else {
            return noTile;
        }
    }
    /**
     * 获取网格瓦片中心点
     */
    private static Vector2 GridToWorldPoint(int x, int y) {
        return new Vector2(x + horizontalSpacingOffset + borderSpacing * x, 
            -y + verticalSpacingOffset - borderSpacing * y);
    }

}
