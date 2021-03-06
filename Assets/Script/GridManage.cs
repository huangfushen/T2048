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
    
    // 每个瓦片间的间距
    private static float borderOffset = 0.05f;
    // 边界值
    private static float horizontalSpacingOffset = -1.65f;
    private static float verticalSpacingOffset = 1.65f;
    // 边框间距 
    private static float borderSpacing = 0.1f;
    // 半个瓦片宽度
    private static float halfTileWidth = 0.55f;
    // 瓦片间的中心点位置
    private static float spaceBetweenTiles = 1.1f;
    
    // 分数
    private int points;
    private List<GameObject> tiles;
    // 重置按钮
    private Rect resetButton;
    // 游戏结束按钮
    private Rect gameOverButton;

    // 最大值
    public int maxValue = 2048;
    
    public GameObject gameOverPanel;
    public GameObject noTile;
    // 分数文本
    public Text scoreText;
    // 瓦片预制体
    public GameObject[] tilePrefabs;

    // 枚举（状态）
    public enum State {
        Loaded, 
        WaitingForInput, 
        CheckingMatches,
        GameOver
    }
    // 状态
    private State state;
    
    void Awake()
    {
        // 瓦片列表
        tiles = new List<GameObject>();
        // 初始化状态
        state = State.Loaded;
    }
    
    void LateUpdate()
    {
        // 根据状态判断执行操作
        switch (state)
        {
            case State.GameOver:
                gameOverPanel.SetActive(true);
                break;
            case State.Loaded:
                state = State.WaitingForInput;
                GenerateRandomTile();
                GenerateRandomTile();
                break;
            case State.WaitingForInput:
                InputKeyBox();
                break;
            case State.CheckingMatches:
                // 生成随机图块
                GenerateRandomTile();
                // 判断是否可以移动（合成）
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
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (MoveTilesLeft()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (MoveTilesRight()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (MoveTilesUp()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (MoveTilesDown()) {
                state = State.CheckingMatches;
            }
        } else if (Input.GetKeyDown(KeyCode.Q)) {
            Reset();
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    /**
     * 向左移动
     */
    private bool MoveTilesLeft()
    {
        bool hasMoved = false;
        for (int x = 1; x < cols; x++) {
            for (int y = 0; y < rows; y++) {
                GameObject obj = GetObjectAtGridPosition(x, y);
        
                if (obj == noTile) {
                    continue;
                }
        
                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.x -= halfTileWidth;
                //射线
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -Vector2.right, Mathf.Infinity);
                if (hit.collider != null) {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj) {
                        if (hitObject.tag == "Tile") {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile)) {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            } else {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.x += spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.x, newPosition.x)) {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        } else if (hitObject.tag == "Border") {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.x = hit.point.x + halfTileWidth + borderOffset;
                            if (!Mathf.Approximately(obj.transform.position.x, newPosition.x)) {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        } 
                    }
                }
            }
        }
    
        return hasMoved;
    }
    /**
     * 向右移动
     */
    private bool MoveTilesRight()
    {
    
        bool hasMoved = false;
        for (int x = cols - 1; x >= 0; x--) {
            for (int y = 0; y < rows; y++) {
                GameObject obj = GetObjectAtGridPosition(x, y);
                if (obj == noTile) {
                    continue;
                }
                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.x += halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.right, Mathf.Infinity);
                if (hit.collider != null) {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj) {
                        if (hitObject.tag == "Tile") {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile)) {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            } else {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.x -= spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.x, newPosition.x)) {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        } else if (hitObject.tag == "Border") {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.x = hit.point.x - halfTileWidth - borderOffset;
                            if (!Mathf.Approximately(obj.transform.position.x, newPosition.x)) {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        } 
                    }
                }
            }
        }
    
        return hasMoved;
    }
    /**
     * 向上移动
     */
    private bool MoveTilesUp()
    {
        bool hasMoved = false;
        for (int y = 1; y < rows; y++) {
            for (int x = 0; x < cols; x++) {
                GameObject obj = GetObjectAtGridPosition(x, y);
        
                if (obj == noTile) {
                    continue;
                }
        
                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.y += halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.up, Mathf.Infinity);
                if (hit.collider != null) {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj) {
                        if (hitObject.tag == "Tile") {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile)) {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            } else {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.y -= spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.y, newPosition.y)) {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        } else if (hitObject.tag == "Border") {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.y = hit.point.y - halfTileWidth - borderOffset;
                            if (!Mathf.Approximately(obj.transform.position.y, newPosition.y)) {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        } 
                    }
                }
            }
        }
  
        return hasMoved;
    }
    /**
     * 向下移动
     */
    private bool MoveTilesDown()
    {
        bool hasMoved = false;
        for (int y = rows - 1; y >= 0; y--) {
            for (int x = 0; x < cols; x++) {
                GameObject obj = GetObjectAtGridPosition(x, y);
        
                if (obj == noTile) {
                    continue;
                }
        
                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.y -= halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -Vector2.up, Mathf.Infinity);
                if (hit.collider != null) {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj) {
                        if (hitObject.tag == "Tile") {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile)) {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            } else {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.y += spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.y, newPosition.y)) {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        } else if (hitObject.tag == "Border") {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.y = hit.point.y + halfTileWidth + borderOffset;
                            if (!Mathf.Approximately(obj.transform.position.y, newPosition.y)) {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        } 
                    }
                }
            }
        }
    
        return hasMoved;
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
        if (tiles.Count >= rows * cols)
        {
            // return ;
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
     * 校验是否能移动
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
    
    private static Vector2 WorldToGridPoint(float x, float y) {
        return new Vector2((x - horizontalSpacingOffset) / (1 + borderSpacing),
            (y - verticalSpacingOffset) / -(1 + borderSpacing));
    }
    
    /**
     * 判断是否可以合并
     */
    private bool CanUpgrade(Tile thisTile, Tile thatTile) {
        return (thisTile.value != maxValue && thisTile.power == thatTile.power && !thisTile.upgradedThisTurn && !thatTile.upgradedThisTurn);
    }
 
  
    /**
     * 合并
     */
    private void UpgradeTile(GameObject toDestroy, Tile destroyTile, GameObject toUpgrade, Tile upgradeTile) {
        Vector3 toUpgradePosition = toUpgrade.transform.position;
        // 合并移除两个
        tiles.Remove(toDestroy);
        tiles.Remove(toUpgrade);
        SimplePool.Despawn(toDestroy);
        SimplePool.Despawn(toUpgrade);

        // 新创建一个
        GameObject newTile = SimplePool.Spawn(tilePrefabs[upgradeTile.power], toUpgradePosition, transform.rotation);
        tiles.Add(newTile);
        Tile tile = newTile.GetComponent<Tile>();
        tile.upgradedThisTurn = true;

        points += upgradeTile.value * 2;
        scoreText.text = points.ToString();

        TileAnimationHandler tileAnim = newTile.GetComponent<TileAnimationHandler>();
        tileAnim.AnimateUpgrade();
    }

}
