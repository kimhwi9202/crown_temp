using UnityEngine;

namespace xLIB
{
    /*
    * 툴에서 오브젝트를 생성하지 않으며, 스크립터 상에서 필요에 의해 생성 
    * 씬이 바뀔때 존재하게 할건지 선택할수 있다. 
    */
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;
        public static T I
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    GameObject container = new GameObject("Singleton_" + typeof(T).ToString());
                    _instance = container.AddComponent(typeof(T)) as T;
                    //Debug.Log(typeof(T).ToString() + " > ISingleton::GetInstance ID = " + _instance.GetInstanceID());
                    DontDestroyOnLoad(_instance.gameObject); // 씬전환시에 클래스 보존을 기본으로 했다.. 삭제하고 싶으면 Destroy() 를 사용.
                }
                return _instance;
            }
        }

        public void CreateInstance()
        {
            DontDestroyOnLoad(I.gameObject);
        }
        private void OnApplicationQuit()
        {
            _instance = null;
        }
        public static void Destroy()
        {
            if(_instance) DestroyImmediate(_instance.gameObject);
            _instance = null;
        }
    }

    public interface IMain
    {
        void Initialize();
    }
    /*
    * 1. 게임의 기본 Main 함수 정의 오브젝트를 직접 생성한다음 상속 받아라
    * 2. 여러 Scene 이 존재할건데.. 모든 신에 생성한 오브젝트를 복사해서 추가해라
    */
    public class MainSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [HideInInspector]   public eTutorial _Tutorial = eTutorial.off;
        [HideInInspector]   public eNetworkMode _NetworkMode = eNetworkMode.Local;
        [HideInInspector]   public eConfigLoadMode _ConfigLoadMode = eConfigLoadMode.Local;
        [HideInInspector]   public eBundleLoadMode _BundleLoadMode = eBundleLoadMode.Local;
        [HideInInspector]   public ePlatform _Platform = ePlatform.None;
        [HideInInspector]   public eLocalization _Localization = eLocalization.korean;
        [HideInInspector]   public string _ConfigURL;

        public delegate void delegateChangePlatform();
        public static delegateChangePlatform eventChangePlatform = null;

        public delegate void delegateChangeLocalization();
        public static delegateChangeLocalization eventChangeLocalization = null;

        private bool _Init = false;
        private static T _instance = null;
        public static T I
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    GameObject container = new GameObject(typeof(T).ToString());
                    _instance = container.AddComponent(typeof(T)) as T;
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
        private void OnApplicationQuit()
        {
            _instance = null;
        }

        // 씬전환시 Awake()는 한번만 실행되도록 필요한코드다.
        // 상속받은 메인용 싱글톤에서는 Awake 를 선언하지말고 virAwake() 이용해야 한다.
        void Awake()
        {
            I.gameObject.tag = "Player";

            var obj  = GameObject.FindObjectOfType<T>();
            if (obj != this)
            {
                Destroy(gameObject);  // 새로운 씬 로딩에 Main 이 있다면 삭제.
                return;
            }

            DontDestroyOnLoad(I.gameObject);

            CONFIG.CurrentConfigURL = _ConfigURL;
            CONFIG.CurrentPlatform = _Platform;
            CONFIG.CurrentNetworkMode = _NetworkMode;
            CONFIG.CurrentConfigLoadMode = _ConfigLoadMode;
            CONFIG.CurrentBundleLoadMode = _BundleLoadMode;
            CONFIG.CurrentLocalization = _Localization;
            CONFIG.CurrentTutorial = _Tutorial;
            CONFIG.Initialize();

            virAwake();

            I.StartCoroutine(CONFIG.WWWLoadConfigFile((ok,msg)=> {
                _Init = ok;
                Debug.Log(msg);
                if(_Init == true) virStart();
                else
                {
                    Application.Quit();
                }
            }));
        }

        virtual protected void virAwake() { }
        virtual protected void virStart() { }
    }
}