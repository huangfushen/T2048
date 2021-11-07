using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimationHandler : MonoBehaviour
{
    // 动画速度
    public float scaleSpeed;
    // 增长大小
    public float growSize;
    
    private Transform _transform;
    // 增长向量
    private Vector3 _growVector;
    
    /**
     * 动画入口
     */
    public void AnimateEntry() {
        // 协程
        StartCoroutine("AnimationEntry");
    }
    
   /**
    * 升级动画
    */
    public void AnimateUpgrade() {
        StartCoroutine("AnimationUpgrade");
    }
    
    
    private IEnumerator AnimationEntry() {
        while (_transform == null) {
            yield return null;
        }

        _transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        while (_transform.localScale.x < 1f) {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);
            yield return null;
        }
    }
    
    private IEnumerator AnimationUpgrade() {
        while (_transform == null) {
            yield return null;
        }

        while (_transform.localScale.x < 1f + growSize) {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one + _growVector, scaleSpeed * Time.deltaTime);
            yield return null;
        }

        while (_transform.localScale.x > 1f) {
            _transform.localScale = Vector3.MoveTowards(_transform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);
            yield return null;
        }
    }
    void Start()
    {
        _transform = transform;
        _growVector = new Vector3(growSize, growSize, 0f);
    }
    
}
