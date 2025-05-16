using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using Monou;

public enum ServerEnviroment { dev, stg, prd }

public class Main: MonoBehaviour
{

    public static Main inst;
    void Awake(){
        if(Main.inst == null){ Main.inst = this; /*DontDestroyOnLoad(gameObject);*/ }
        else Destroy(gameObject);
    }

#if UNITY_WEBGL && !UNITY_EDITOR 
    [DllImport("__Internal")]
    private static extern void OnAuthorizationError(string token);
    [DllImport("__Internal")]
    private static extern void OnExit();
#else
    private void OnExit(){}
#endif

    public ServerEnviroment env;
    public string token;
    public string GRAPHQL_API;
    public string MONOU_API;
    public GameObject prefab;
    public GameObject prefabPreset;
    public GameObject avatarMimic;
    private bool prefabSetted = false;
    private Dictionary<string, string> dataStorage;
    private GameObject userGO;
    private MonouAvatar monouAvatar;
    private string userId;
    private int userMoney = 0;
    private GameObject loadding;
    private GameObject rewardPopup;
    private string uidFriend = "";
    private UserMoneyData monouData;


    // Start is called before the first frame update
    void Start()
    {
        loadding = GameObject.Find("Loadding");
        rewardPopup = GameObject.Find("RewardPopup"); rewardPopup.SetActive(false);
        //LoadCongiguration();
        //StartCoroutine(_loadMonouData());
        IniMonouAvatar();
    }

    private void IniMonouAvatar(){
        monouAvatar = MonouAvatar.inst;
        monouAvatar.GraphqlAPI = GRAPHQL_API;
        monouAvatar.OnAuthError += OnAuthError;
        monouAvatar.OnInitialDataLoaded += OnInitialDataLoaded;
        monouAvatar.OnUserDataLoaded += OnUserDataLoaded;
        monouAvatar.OnUpdated += OnAvatarUpdated;
        monouAvatar.setToken(token);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetToken(string _token){ token=_token; Debug.Log("SetToken "+ _token); }
    public void SetApp(string _appName){
        var myResource = Resources.Load(_appName);
        Debug.Log("SetApp "+ _appName+ ": "+(myResource!=null));
        if(myResource!=null) prefab= myResource as GameObject;
        prefabSetted = true;
    }
    public string GetToken(){ return token; }
    public string GetGraphqlAPI(){ return GRAPHQL_API; }
    public string GetMonouAPI(){ return MONOU_API; }
    public string GetUserId(){ return userId; }
    public string[] GetPath(){ return new string[] {""}; }
    public string LocalStorage(string key){ return PlayerPrefs.HasKey(key)? PlayerPrefs.GetString(key): ""; }
    public void LocalStorage(string key, string val){ PlayerPrefs.SetString(key,val); }
    public GameObject GetPlayer(){ return userGO; }
    public void SetPlayer(GameObject go){ userGO = go; }
    public void SetLoadding(bool status){ loadding.SetActive(status); }
    public void HideLoadding(){ loadding.SetActive(false); }
    public void GoTo(string path){ loadding.SetActive(true); Debug.Log("MAIN GOTO >> "+path); }
    public void GoTo(MonoBehaviour caller, string path){
        if (Application.platform == RuntimePlatform.WebGLPlayer){
            if(path=="back"){
                GameObject a = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                a.name = a.name.Substring(0,a.name.Length-7);
                Destroy(caller.gameObject);
            }else OnExit();
            return;
        }
        Destroy(caller.gameObject);
    }
    public bool IsDev(){ return env == ServerEnviroment.dev; }
    public bool IsStg(){ return env == ServerEnviroment.stg; }
    public bool IsPrd(){ return env == ServerEnviroment.prd; }


    public void LoadCongiguration(){
        string path = Application.persistentDataPath + "Delemente.txt";
        string data;
        try{
            data = File.ReadAllText(path);
            dataStorage = JsonUtility.FromJson<Dictionary<string,string>>(data);
        } catch( System.Exception ex ) {
            Debug.LogException(ex);
            dataStorage = new Dictionary<string, string>();
        }
    }
    public void SaveConfiguration(){
        string path = Application.persistentDataPath + "Delemente.txt";
        string json = JsonUtility.ToJson(dataStorage);
        File.WriteAllText(path, json);
    }

    public void setAPI(string _url ){ GRAPHQL_API = _url; }
    public void setMonoutAPI(string _url ){ MONOU_API = _url; }
    public void showUser(string _uid){ uidFriend = _uid; }

#if UNITY_WEBGL && !UNITY_EDITOR 
        // cuando tiene erro de autenticación
        private void OnAuthError(string token){
            if(prefabSetted){
                GameObject a = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                a.name = a.name.Substring(0,a.name.Length-7);
                Debug.Log("instancn "+ a.name);
            }
            OnAuthorizationError(token); // llamada webgl
        }
#endif
#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnAuthError(string token){
            if(prefabSetted){
                GameObject a = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                a.name = a.name.Substring(0,a.name.Length-7);
                Debug.Log("instancn "+ a.name);
            }
        }
#endif
        private void OnInitialDataLoaded(string mainUserId){
            userMoney = monouAvatar.GetMainUserMoney();
            if(uidFriend != ""){ mainUserId = uidFriend; }
            userId = mainUserId;
            bool isOk = monouAvatar.LoadAndCreateUserPreloaded(mainUserId,0, out userGO);
            //userGO = monouAvatar.LoadAndCreateUser(mainUserId);
        }
        private void OnUserDataLoaded(string uid , bool requirePreset){
            GameObject a;
            if(uid!=userId) return;
            Debug.Log("VIEwer: "+prefabSetted);
            if(avatarMimic != null){ SetLoadding(false); }
            else if(prefabSetted || !requirePreset || uidFriend != ""){
                if(prefab!=null){
                    a = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    a.name = a.name.Substring(0,a.name.Length-7);
                    if(!prefabSetted) StartCoroutine( CheckForAcquiredBundles() );
                }
            }else
                if(prefabPreset!=null){
                    a = Instantiate(prefabPreset, new Vector3(0, 0, 0), Quaternion.identity);
                    a.name = a.name.Substring(0,a.name.Length-7);
                    if(!prefabSetted) StartCoroutine( CheckForAcquiredBundles() );
                }
        }
        public void OnAvatarUpdated(GameObject go, bool isCreate, string uId){
            if(isCreate && uId==userId && userId=="guest") StartCoroutine(_loadGuestApparence());
            StartCoroutine(_updateShaders(go));
            // copia el animador y esconde el render del objecto a imitar
            if(uId==userId && avatarMimic!=null && !mimicAnimatorAlready){
                avatarMimicAni = avatarMimic.GetComponentInChildren<Animator>();
                userGOAni = userGO.GetComponent<Animator>();
                if(avatarMimicAni != null){
                    mimicAnimatorAlready = true;
                    userGOAni.runtimeAnimatorController = avatarMimicAni.runtimeAnimatorController;
                    userGO.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>().raceAnimationControllers.defaultAnimationController = avatarMimicAni.runtimeAnimatorController;
                    var meshRenderer = avatarMimicAni.GetComponentInChildren<MeshRenderer>(); if(meshRenderer!=null) meshRenderer.enabled=false;
                    var skinnedMeshRenderer = avatarMimicAni.GetComponentInChildren<SkinnedMeshRenderer>(); if(skinnedMeshRenderer!=null) skinnedMeshRenderer.enabled=false;
                }
                StartCoroutine(UpdateMimic());
            }else StartCoroutine(UpdateMimic());
            if(uId==userId && !isCreate) _isLoaded = true;
        }
        IEnumerator _loadGuestApparence(){
            yield return new WaitForSeconds(0F);
            if(PlayerPrefs.HasKey("MonouAvatarEditor")){
                var avatar = monouAvatar.GetAvatarByUser("guest");
                string wd=PlayerPrefs.GetString("MonouAvatarEditor");
                if(wd.Length>=4){
                string[] data = wd.Substring(2, wd.Length-4).Split("},{");
                    foreach(var d in data) {
                       var wardrobeItem = JsonUtility.FromJson<GQLMainWardrobeItem>("{"+d+"}");
                        monouAvatar.addItemFromId(wardrobeItem.wardrobeAssetAssetId, avatar);
                        yield return new WaitForSeconds(1F);
                    }
                }
            }
        }
        IEnumerator _updateShaders(GameObject go){
            Debug.Log("UpdateShader");
            yield return new WaitForSeconds(0.01F);
            go.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            /*Material[] mm= go.GetComponentInChildren<SkinnedMeshRenderer>().materials;
            for(int i=0; i<mm.Length; i++){
                string shaderName = mm[i].shader.ToString();
                int found = shaderName.IndexOf(" (UnityEngine.Shader)");
                if(found<0) continue;
                Shader s = Shader.Find(shaderName.Substring(0, found));
                if(s!=null){
                    Debug.Log(s);
                    mm[i].shader = s;
                }
            }*/
        }
    void OnApplicationQuit(){
        //SaveConfiguration();
    }

    /*IEnumerator _loadMonouData(){
        SetLoadding(true);
        UnityWebRequest www = UnityWebRequest.PostWwwForm(MONOU_API, "");
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Token "+token);
        www.SetRequestHeader("Content-Type", "application/json");
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(0);
        //www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); OnAuthError(token); }
        else {
            SetLoadding(false);
            Debug.Log(www.downloadHandler.text);
            monouData = JsonUtility.FromJson<UserMoneyData>(www.downloadHandler.text);
            IniMonouAvatar();
        }
    }*/
    public void SetMoney(int current){ userMoney = current; }
    public int GetMoney(){ return userMoney; }



        IEnumerator CheckForAcquiredBundles( ) {
            SetLoadding(true);
            queryGQL q = new queryGQL("query MyQuery { checkJunglePass { assets options } checkFreeBundle { assets options } }");
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GRAPHQL_API, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
            else Debug.Log(www.downloadHandler.text);

            q = new queryGQL("query MyQuery { checkForNewAcquiredBundle { id bundle { name subject } assets options } }");
            json = JsonUtility.ToJson(q);
            Debug.Log(json);
            www = UnityWebRequest.PostWwwForm(GRAPHQL_API, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
            else {
                SetLoadding(false); 
                Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<AcquiredBundles>(www.downloadHandler.text);
                if(n.data.checkForNewAcquiredBundle.id!=null && n.data.checkForNewAcquiredBundle.id!=""){
                    rewardPopup.SetActive(true);
                    rewardPopup.GetComponent<MonouRewardPopup>().SetData(
                        n.data.checkForNewAcquiredBundle.bundle.subject,
                        n.data.checkForNewAcquiredBundle.assets,
                        n.data.checkForNewAcquiredBundle.options
                    );
                }
            }
        }

        public bool _isLoaded = false;
        public bool IsLoaded(){ return _isLoaded = true; }

        private bool mimicAnimatorAlready;
        private Animator avatarMimicAni;
        private Animator userGOAni;
        public void SetAvatarMimic(GameObject go){
            avatarMimic = go;
            mimicAnimatorAlready = true;
            userGOAni = userGO.GetComponent<Animator>();
            avatarMimicAni = avatarMimic.GetComponent<Animator>();
            if(avatarMimicAni == null ) avatarMimicAni = avatarMimic.GetComponentInChildren<Animator>();
            if(avatarMimicAni != null ) userGOAni.runtimeAnimatorController = avatarMimicAni.runtimeAnimatorController;
            userGO.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>().raceAnimationControllers.defaultAnimationController = avatarMimicAni.runtimeAnimatorController;
            var meshRenderers = avatarMimicAni.GetComponentsInChildren<MeshRenderer>();
            for(int i=0; i<meshRenderers.Length; i++) if(meshRenderers[i]!=null) meshRenderers[i].enabled=false;
            var skinnedMeshRenderer = avatarMimicAni.GetComponentsInChildren<SkinnedMeshRenderer>();
            for(int i=0; i<skinnedMeshRenderer.Length; i++) if(skinnedMeshRenderer[i]!=null) skinnedMeshRenderer[i].enabled=false;
        }
        IEnumerator UpdateMimic(){
            do{
                yield return new WaitForSeconds(0.00F);
                if(avatarMimic == null || userGO == null) continue;
                // imita la posición
                userGO.transform.position = Vector3.Lerp(userGO.transform.position, avatarMimic.transform.position, 0.5f);
                userGO.transform.eulerAngles = avatarMimic.transform.eulerAngles;
                userGO.transform.localScale = avatarMimic.transform.localScale;
                // imita animaciones
                if(mimicAnimatorAlready)
                for(int i=0; i<avatarMimicAni.parameters.Length; i++)
                    if(avatarMimicAni.parameters[i].type == AnimatorControllerParameterType.Int){ userGOAni.SetInteger( avatarMimicAni.parameters[i].name, avatarMimicAni.GetInteger(avatarMimicAni.parameters[i].name) ); }else
                    if(avatarMimicAni.parameters[i].type == AnimatorControllerParameterType.Float){ userGOAni.SetFloat( avatarMimicAni.parameters[i].name, avatarMimicAni.GetFloat(avatarMimicAni.parameters[i].name) ); }else
                    if(avatarMimicAni.parameters[i].type == AnimatorControllerParameterType.Bool){ userGOAni.SetBool( avatarMimicAni.parameters[i].name, avatarMimicAni.GetBool(avatarMimicAni.parameters[i].name) ); }
            }while(true);
        }

        // =========================== WEBREQUEST =============================
    public bool HasInternetConnection(){
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    public void Post(string url, string json, string token, System.Action<string> success, System.Action<string> error){
        if(!HasInternetConnection()){
            string data = _LoadCache(url);
            if(data==""){ error.Invoke("empty"); }else success.Invoke(data);
        }else StartCoroutine( LoadUrl(url, json, token, success, error) );
    }

    public IEnumerator LoadUrl(string url, string json, string token, System.Action<string> success, System.Action<string> error){
        Debug.Log(json);
        UnityWebRequest www = UnityWebRequest.PostWwwForm(url, json);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", token);
        www.SetRequestHeader("Content-Type", "application/json");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) {
            error.Invoke(www.error);
            //Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }else {
            Debug.Log(www.downloadHandler.text);
            success.Invoke(www.downloadHandler.text);
            _SaveCache(url, www.downloadHandler.text);
        }
    }
    private string _GetPath(string url){
        Hash128 hash  = new Hash128(); hash.Append(url);
        return Application.persistentDataPath + "XOStudio"+hash.ToString()+".txt";
    }
    private string _LoadCache(string url){
        try{ return File.ReadAllText(_GetPath(url)); }
        catch( System.Exception ex ){ return ""; }
    }
    private void _SaveCache(string url, string data){ File.WriteAllText(_GetPath(url), data); }
}

    [System.Serializable]
    public class UserMoneyData {
        public int monedas;
        public string email;
        public string user_id;
        public string username;
        public bool has_subscription;
        public UserMoneySuscription[] suscripcion;
    }
        [System.Serializable]
        public class UserMoneySuscription {
            public string id;
            public string from_date;
            public string to_date;
            public UserMoneyTypeSuscription type_subscription;
        }
            [System.Serializable]
            public class UserMoneyTypeSuscription {
                public string id;
                public string name;
                public string package_name;
            }

    [System.Serializable]
    public class AcquiredBundles { public AcquiredBundlesData data; }
        [System.Serializable]
        public class AcquiredBundlesData { public AcquiredBundlesNew checkForNewAcquiredBundle; }
            [System.Serializable]
            public class AcquiredBundlesNew {
                public string id;
                public AcquiredBundlesNewBundle bundle;
                public string[] assets;
                public string[] options;
            }
                [System.Serializable]
                public class AcquiredBundlesNewBundle {
                    public string name;
                    public string subject;
                }

    [System.Serializable]
    public class GQLMainWardrobeItem {
        public string[] options;
        public string wardrobeAssetAssetId;
    }