using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
	// 默认池子大小
	const int DEFAULT_POOL_SIZE = 3;
	
	class Pool {

		int nextId=1;
		

		Stack<GameObject> inactive;
		
		GameObject prefab;
		
		public Pool(GameObject prefab, int initialQty) {
			this.prefab = prefab;
			
			inactive = new Stack<GameObject>(initialQty);
		}
		
		// 出栈
		public GameObject Spawn(Vector3 pos, Quaternion rot) {
			GameObject obj;
			if(inactive.Count==0) {
	
				obj = (GameObject)GameObject.Instantiate(prefab, pos, rot);
				obj.name = prefab.name + " ("+(nextId++)+")";
				obj.AddComponent<PoolMember>().myPool = this;
			}else {
				obj = inactive.Pop();
				if(obj == null) {
					return Spawn(pos, rot);
				}
			}
			
			obj.transform.position = pos;
			obj.transform.rotation = rot;
			obj.SetActive(true);
			return obj;
			
		}
		
		// 入栈
		public void Despawn(GameObject obj) {
			obj.SetActive(false);
			inactive.Push(obj);
		}
		
	}
	
	
	class PoolMember : MonoBehaviour {
		public Pool myPool;
	}
	
	//所有的池子
	static Dictionary< GameObject, Pool > pools;
	
	// 初始化所有的池子
	static void Init (GameObject prefab=null, int qty = DEFAULT_POOL_SIZE) {
		if(pools == null) {
			pools = new Dictionary<GameObject, Pool>();
		}
		if(prefab!=null && pools.ContainsKey(prefab) == false) {
			pools[prefab] = new Pool(prefab, qty);
		}
	}
	
	// 预加载对象池子
	static public void Preload(GameObject prefab, int qty = 1) {
		Init(prefab, qty);
		
		GameObject[] obs = new GameObject[qty];
		for (int i = 0; i < qty; i++) {
			obs[i] = Spawn (prefab, Vector3.zero, Quaternion.identity);
		}
		
		for (int i = 0; i < qty; i++) {
			Despawn( obs[i] );
		}
	}
	
	static public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) {
		Init(prefab);
		
		return pools[prefab].Spawn(pos, rot);
	}
	

	static public void Despawn(GameObject obj) {
		PoolMember pm = obj.GetComponent<PoolMember>();
		if(pm == null) {
			GameObject.Destroy(obj);
		}
		else {
			pm.myPool.Despawn(obj);
		}
	}
	
}
