using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static T mInstance = null;

	public static T I
    {
        get
        {
			if (mInstance == null)
            {
            	mInstance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (mInstance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    mInstance = go.AddComponent<T>();
                }
            }
            return mInstance;
        }
    }

    /*
     * û���κ�ʵ�ֵĺ��������ڱ�֤MonoSingleton��ʹ��ǰ�Ѵ���
     */
    public void Startup()
    {

    }

    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this as T;
        }

        DontDestroyOnLoad(gameObject);
        Init();
    }
 
    protected virtual void Init()
    {

    }

    public void DestroySelf()
    {
        Dispose();
        MonoSingleton<T>.mInstance = null;
        UnityEngine.Object.Destroy(gameObject);
    }

    public virtual void Dispose()
    {

    }

}