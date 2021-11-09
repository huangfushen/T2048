using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
  public int value;
  public int power;
  //一次滑动只能合并一次
  public bool upgradedThisTurn;
}
