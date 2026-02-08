using Cysharp.Threading.Tasks;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock(_lock)
            {
                if(_instance == null)
                {
                    _instance = (T)FindFirstObjectByType(typeof(T)); // 해당 객체를 가진 게임 오브젝트를 찾음
                    if(_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString();

                        DontDestroyOnLoad(singleton);
                    }
                }
            }
            return _instance;
        }
    }

    protected async virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async virtual UniTask InitializeAsync() {}
}
