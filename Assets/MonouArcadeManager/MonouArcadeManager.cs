using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using SimpleJSON;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Monou
{
    public class MonouArcadeManager : MonoBehaviour
    {



#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetURL();
        [DllImport("__Internal")]
        private static extern bool WebGL_IsMobile();
        [DllImport("__Internal")]
        private static extern bool ShowAdds();
#endif
        static bool IsMobile(){
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGL_IsMobile();
#else
            return (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);
#endif
        }

        private const string TANGANANICANANA = "lHPqoraAUINGvLyQWmObcjFSzhkYiCwRnXMuBpZKtxdgETDeVJsf";
        private const string TANGANANICA = "bqkcgomgha";
        private const string TANGANANA = "mvsdftbwrt";

        [Header("Configuración de Torneo")]
        [Tooltip("URL del api de plataforma Monou.gg. Se auto sobreesribe en producción")]
        public string api;
        [Tooltip("Id del jugador de plataforma Monou.gg. Se auto sobreesribe en producción")]
        public string userId;
        [Tooltip("Slug del torneo generado en plataforma Monou.gg. Se auto sobreesribe en producción")]
        public string slug;

        [Header("Configuración la interfaz")]
        [Tooltip("Url del link de los términos y condiciones de Monou.gg")]
        public string termsUrl;
        [Tooltip("Cantidad de registros mostrados del ranking")]
        public int maxRankingRows = 5;

        // public Texture splashTexture;
        // public Texture bgTexture;
        // public Color textColor;
        // public Texture bgButton;
        // public Color textButtonColor;
        [Header("Configuración de Juego")]
        [Tooltip("Nombre de juego. Obligatorio")]
        public string game;
        [Tooltip("Prefab del juego. Solo la parte jugable. Sin menu comenzar o volver a jugar, etc.")]
        public GameObject gamePrefab;

        private int timeZoneOffset = 6;

        private UIDocument ui;
        private VisualElement root;
        private ScrollView content;
        private VisualElement splashMonou;
        private VisualElement splash;
        private VisualElement notReady;
        private Label notReady_timerText;
        private Button notReady_tutorialButton;
        private Button notReady_demoButton;
        private Button notReady_registerButton;
        private Label notReady_registredText;
        private VisualElement alReady;
        private Label alReady_timerText;
        private Button alReady_playButton;
        private Button alReady_registerButton;
        private Label alReady_registredText;
        private VisualElement alReady_ranking;
        private Label alReady_ranking_empty;
        private VisualElement finished;
        private Button finished_termsButton;
        private VisualElement finished_ranking;
        private Label finished_ranking_empty;
        private VisualElement gameoverDemo;
        private Label gameoverDemo_scoreText;
        private Button gameoverDemo_playAgainButton;
        private Button gameoverDemo_registerButton;
        private Label gameoverDemo_registredText;
        private Button gameoverDemo_moreGamesButton;
        private Label gameoverDemo_timerText;
        private VisualElement gameover;
        private Label gameover_scoreText;
        private Label gameover_placeText;
        private Button gameover_playAgainButton;
        private Label gameover_timerText;
        private VisualElement gameover_ranking;
        private Label gameover_ranking_empty;

        private Button closeButton;
        private VisualElement tutorialViewer;
        private VisualElement tutorialContent;
        private Button prevButton;
        private Button nextButton;
        private Button finishButton;
        private VisualElement modal;
        private Label modalTitle;
        private Button modalConfirm;
        private Button modalCancel;
        private Label demoHint;

        private string tournametId = "";
        private DateTime timestandStart;
        private DateTime timestandFinish;
        private int score = 0;
        private string teamId = "";
        private bool isDemo = false;
        private GameObject gameInstance;
        private string logId;
        private bool junglePass = false;
        private string frontApi = "";
        private string table = "tetrix_monou_stg";

        private string[] HEADTITLES = new string[3]{"Pos", "Name", "Points"};

        public static MonouArcadeManager inst;
        void Awake(){
            if(MonouArcadeManager.inst == null){ MonouArcadeManager.inst = this; /*DontDestroyOnLoad(gameObject);*/ }
            else Destroy(gameObject);
        }


        void Start()
        {
            ui = GetComponent<UIDocument>();
            root = ui.rootVisualElement;
            content = root.Query<ScrollView>("content").First();
            splashMonou = root.Query<VisualElement>("splashMonou").First();
            splash = root.Query<VisualElement>("splash").First();
            notReady = root.Query<VisualElement>("notReady").First();
            notReady_timerText = root.Query<Label>("notReady_timerText").First();
            notReady_tutorialButton = root.Query<Button>("notReady_tutorialButton").First();
            notReady_tutorialButton.RegisterCallback<MouseUpEvent>(ev => ShowTutorial());
            notReady_demoButton = root.Query<Button>("notReady_demoButton").First();
            notReady_demoButton.RegisterCallback<MouseUpEvent>(ev => PlayDemo());
            notReady_registerButton = root.Query<Button>("notReady_registerButton").First();
            notReady_registerButton.RegisterCallback<MouseUpEvent>(ev => Register(notReady_registerButton));
            notReady_registredText = root.Query<Label>("notReady_registredText").First();
            alReady = root.Query<VisualElement>("alReady").First();
            alReady_timerText = root.Query<Label>("alReady_timerText").First();
            alReady_playButton = root.Query<Button>("alReady_playButton").First();
            alReady_playButton.RegisterCallback<MouseUpEvent>(ev => Play());
            alReady_registerButton = root.Query<Button>("alReady_registerButton").First();
            alReady_registerButton.RegisterCallback<MouseUpEvent>(ev => Register(alReady_registerButton));
            alReady_registredText = root.Query<Label>("notReady_registredText").First();
            alReady_ranking = root.Query<VisualElement>("alReady_ranking").First();
            alReady_ranking_empty = root.Query<Label>("alReady_ranking_empty").First();
            finished = root.Query<VisualElement>("finished").First();
            finished_termsButton = root.Query<Button>("finished_termsButton").First();
            finished_termsButton.RegisterCallback<MouseUpEvent>(ev => ShowTerms());
            finished_ranking = root.Query<VisualElement>("finished_ranking").First();
            finished_ranking_empty = root.Query<Label>("finished_ranking_empty").First();
            gameoverDemo = root.Query<VisualElement>("gameoverDemo").First();
            gameoverDemo_scoreText = root.Query<Label>("gameoverDemo_scoreText").First();
            gameoverDemo_playAgainButton = root.Query<Button>("gameoverDemo_playAgainButton").First();
            gameoverDemo_playAgainButton.RegisterCallback<MouseUpEvent>(ev => PlayDemo());
            gameoverDemo_registerButton = root.Query<Button>("gameoverDemo_registerButton").First();
            gameoverDemo_registerButton.RegisterCallback<MouseUpEvent>(ev => Register(gameoverDemo_registerButton));
            gameoverDemo_registredText = root.Query<Label>("gameoverDemo_registredText").First();
            gameoverDemo_moreGamesButton = root.Query<Button>("gameoverDemo_moreGamesButton").First();
            gameoverDemo_moreGamesButton.RegisterCallback<MouseUpEvent>(ev => { Debug.Log("MoreGames"); });
            gameoverDemo_timerText = root.Query<Label>("gameoverDemo_timerText").First();
            gameover = root.Query<VisualElement>("gameover").First();
            gameover_scoreText = root.Query<Label>("gameover_scoreText").First();
            gameover_placeText = root.Query<Label>("gameover_placeText").First();
            gameover_playAgainButton = root.Query<Button>("gameover_playAgainButton").First();
            gameover_playAgainButton.RegisterCallback<MouseUpEvent>(ev => Play());
            gameover_timerText = root.Query<Label>("gameover_timerText").First();
            gameover_ranking = root.Query<VisualElement>("gameover_ranking").First();
            gameover_ranking_empty = root.Query<Label>("gameover_ranking_empty").First();
            closeButton = root.Query<Button>("closeButton").First();
            closeButton.RegisterCallback<MouseUpEvent>(ev => CloseGame());
            tutorialViewer = root.Query<VisualElement>("tutorialViewer").First();
            tutorialContent = root.Query<VisualElement>("tutorialContent").First();
            prevButton = root.Query<Button>("prevButton").First();
            prevButton.RegisterCallback<MouseUpEvent>(ev => ChangeTutorial(-1));
            nextButton = root.Query<Button>("nextButton").First();
            nextButton.RegisterCallback<MouseUpEvent>(ev => ChangeTutorial(1));
            finishButton = root.Query<Button>("finishButton").First();
            finishButton.RegisterCallback<MouseUpEvent>(ev => CloseTutorial());
            modal = root.Query<VisualElement>("modal").First();
            modalTitle = root.Query<Label>("modalTitle").First();
            modalConfirm = root.Query<Button>("modalConfirm").First();
            modalConfirm.RegisterCallback<MouseUpEvent>(ev => { hideModal(); if(modalSuccess!=null) modalSuccess.Invoke(); });
            modalCancel = root.Query<Button>("modalCancel").First();
            modalCancel.RegisterCallback<MouseUpEvent>(ev => hideModal());
            demoHint = root.Query<Label>("demoHint").First();

            TimeZoneInfo localZone = TimeZoneInfo.Local;
            timeZoneOffset = (int)localZone.BaseUtcOffset.TotalHours;

            content.schedule.Execute(() => {
                int toStart = (int)(timestandStart - DateTime.UtcNow).TotalSeconds + timeZoneOffset *60*60;
                string timeleft = _GetTimer(toStart);
                notReady_timerText.text = timeleft;
                gameoverDemo_timerText.text = timeleft;

                int toFinish = (int)(timestandFinish - DateTime.UtcNow).TotalSeconds + timeZoneOffset *60*60;
                string timetoend = _GetTimer(toFinish);
                alReady_timerText.text = timetoend;
                gameover_timerText.text = timetoend;
            }).Every(1000);
            ForceUpdate(content);

            // if(splashTexture != null) splash.style.backgroundImage = new StyleBackground((Texture2D)splashTexture);
            // if(bgTexture != null) content.style.backgroundImage = new StyleBackground((Texture2D)bgTexture);
            // if(bgButton != null){
            //     notReady_tutorialButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     notReady_demoButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     notReady_registerButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     alReady_playButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     finished_termsButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     gameoverDemo_registerButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     gameoverDemo_playAgainButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     gameoverDemo_moreGamesButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            //     gameover_playAgainButton.style.backgroundImage = new StyleBackground((Texture2D)bgButton);
            // }

            // content.style.color = textColor;
            // notReady_tutorialButton.style.color = textButtonColor;
            // notReady_demoButton.style.color = textButtonColor;
            // notReady_registerButton.style.color = textButtonColor;
            // alReady_playButton.style.color = textButtonColor;
            // finished_termsButton.style.color = textButtonColor;
            // gameoverDemo_registerButton.style.color = textButtonColor;
            // gameoverDemo_playAgainButton.style.color = textButtonColor;
            // gameoverDemo_moreGamesButton.style.color = textButtonColor;
            // gameover_playAgainButton.style.color = textButtonColor;

            ShowSplash();
            ExtractDataFromWebGL();
            CheckStatus();
            HideButtonsForWebGL();
        }
        void Update(){
            /*if(gameInstance == null && tournametId != ""){
                if(
                    (gameoverDemo.style.display == DisplayStyle.Flex ||
                    notReady.style.display == DisplayStyle.Flex) &&
                    timestandStart<=0
                ) ShowAlReady();
                if(
                    (alReady.style.display == DisplayStyle.Flex ||
                    gameover.style.display == DisplayStyle.Flex) && 
                    timestandFinish<=0
                ) ShowFinished();
            }*/
        }

        private string advanceInterval = "";
        private int tangananicaOffset = 0;
        private void ResetAdvanceLog(){
            tangananicaOffset = (int)((Time.time *100)%48);
            advanceInterval = "" + TANGANANICANANA[tangananicaOffset];
        }
        public void Advance(int deltaScore){ //de 16 bits, no mayor a 32768, no menor a -32767
            //Debug.Log("deltaScore"+deltaScore);
            // trabaja el log de puntaje avanzado, componiendo el número en una string desordenada y agregando al azar números
            var cc = new List<int>();
            cc.Add(deltaScore & 0xf);
            cc.Add((deltaScore>>4) & 0xf);
            cc.Add((deltaScore>>8) & 0xf);
            cc.Add((deltaScore>>12) & 0xf);
            int _time = (int)(Time.time * 100);
            foreach(int i in cc) if(i>0){
                advanceInterval += TANGANANICANANA[(i + tangananicaOffset)%TANGANANICANANA.Length];
            }else advanceInterval += ((_time%advanceInterval.Length)%10).ToString();
            tangananicaOffset += (int)TANGANANICANANA[tangananicaOffset%TANGANANICANANA.Length]; 
        }
        public void Success(int theScore){
            //demoHint.style.display = DisplayStyle.None;
            if(gameInstance != null) Destroy(gameInstance);
            content.style.display = DisplayStyle.Flex;
#if !UNITY_WEBGL || UNITY_EDITOR
            closeButton.style.display = DisplayStyle.Flex;
#endif
            score = theScore;
            if(isDemo){
                ShowGameoverDemo();
            }else{
                SaveScore(()=>ShowGameover());
            }
            FinishLog();
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowAdds();
#endif
        }
        public void Goto(string target){
#if UNITY_EDITOR
            Debug.Log("Goto Target");
#elif UNITY_WEBGL

#else
            //Main.ints.Goto(target);
#endif
        }


        private void HideAll(){
            notReady.style.display = DisplayStyle.None;
            alReady.style.display = DisplayStyle.None;
            finished.style.display = DisplayStyle.None;
            gameoverDemo.style.display = DisplayStyle.None;
            gameover.style.display = DisplayStyle.None;
        }
        private void ShowSplash(){
            HideAll();
            splashMonou.style.display = DisplayStyle.Flex;
            SetTimeout(()=>{
                splash.style.display = DisplayStyle.Flex;
                splashMonou.style.display = DisplayStyle.None;
                SetTimeout(()=>{
                    splash.style.display = DisplayStyle.None;
                },1);
            },1);
        }
        private void ShowNotReady(){
            HideAll();
            notReady.style.display = DisplayStyle.Flex;

        }
        private void ShowAlReady(){
            HideAll();
            alReady.style.display = DisplayStyle.Flex;
            GetRanking( alReady_ranking, alReady_ranking_empty );

        }
        private void ShowGameoverDemo(){
            HideAll();
            gameoverDemo.style.display = DisplayStyle.Flex;
            gameoverDemo_scoreText.text = score.ToString();

        }
        private void ShowGameover(){
            HideAll();
            gameover.style.display = DisplayStyle.Flex;
            gameover_scoreText.text = score.ToString();
            GetRanking( gameover_ranking, gameover_ranking_empty );

        }
        private void ShowFinished(){
            HideAll();
            finished.style.display = DisplayStyle.Flex;
            GetRanking( finished_ranking, finished_ranking_empty );

        }

        private void UpdatePrizes(JSONNode prizes){
            var list = alReady.Query<VisualElement>(className: "reward").ToList();
            foreach (var item in list) item.style.display = DisplayStyle.None;
            List<string> rewardsText = new List<string>();
            int x = 0;
            foreach (JSONNode prize in prizes){
                    if(prize["place"] == 1){x = 0;}
                else if(prize["place"] == 2){x = 1;}
                else if(prize["place"] == 3){x = 2;}
                else continue;
                list[x].style.display = DisplayStyle.Flex;
                list[x].Query<Label>(className: "reward_description").First().text = prize["prize_name"];
            }
        }

        List<VisualElement> tutorialList;
        int tutorialIndex = 0;
        private void ShowTutorial(){
            tutorialViewer.style.display = DisplayStyle.Flex;
            tutorialList = tutorialContent.Query<VisualElement>("tutorial").ToList();
            foreach(var item in tutorialList) item.style.display = DisplayStyle.None;
            tutorialList[0].style.display = DisplayStyle.Flex;
            prevButton.style.display = DisplayStyle.None;
            nextButton.style.display = tutorialList.Count>1? DisplayStyle.Flex: DisplayStyle.None;
            finishButton.style.display = tutorialList.Count<2? DisplayStyle.Flex: DisplayStyle.None;
            tutorialIndex = 0;
        }
        private void ChangeTutorial(int d){
            int nextTutorialIndex = tutorialIndex + d;
            if(nextTutorialIndex<0) nextTutorialIndex=0;
            if(nextTutorialIndex>tutorialList.Count-1) nextTutorialIndex=tutorialList.Count-1;
            prevButton.style.display = nextTutorialIndex>0? DisplayStyle.Flex: DisplayStyle.None;
            nextButton.style.display = nextTutorialIndex<tutorialList.Count-1? DisplayStyle.Flex: DisplayStyle.None;
            finishButton.style.display = nextTutorialIndex>=tutorialList.Count-1? DisplayStyle.Flex: DisplayStyle.None;
            foreach(var item in tutorialList) item.style.display = DisplayStyle.None;
            tutorialList[nextTutorialIndex].style.display = DisplayStyle.Flex;
            tutorialIndex = nextTutorialIndex;
        }
        private void CloseTutorial(){
            tutorialViewer.style.display = DisplayStyle.None;
        }
        private void PlayDemo(){
            ShowModal("Entiendo que ésta es una partida de práctica, que no representa ningún tipo de premio", "¡A Jugar!", "", ()=>{
                //demoHint.style.display = DisplayStyle.Flex;
                isDemo= true; StartGame();
            });
        }
        private void Play(){ isDemo = false; StartGame(); }
        private void Register(Button btn){
            btn.SetEnabled(false);
            Get(api + "list-teams/5747", success=>{
                JSONNode td = JSON.Parse(success);
                teamId = td["message"][0]["id"];
                ArcadeRegisterPostData data = new ArcadeRegisterPostData(userId, teamId, tournametId);
                Post(api + "inscription-tournament-decision-fast/", JsonUtility.ToJson(data), success =>{
                    notReady_registredText.style.display = DisplayStyle.Flex;
                    //alReady_registredText.style.display = DisplayStyle.Flex;
                    alReady_playButton.style.display = DisplayStyle.Flex;
                    gameoverDemo_registredText.style.display = DisplayStyle.Flex;
                    notReady_registerButton.style.display = DisplayStyle.None;
                    alReady_registerButton.style.display = DisplayStyle.None;
                    gameoverDemo_registerButton.style.display = DisplayStyle.None;
                    HideButtonsForWebGL();
                }, err=>{
#if UNITY_WEBGL && !UNITY_EDITOR
                    if(junglePass){
                        ShowModal(
                            "Ups! Para ingresar a este torneo, necesitas renovar tu Jungle Pass.",
                            "Adquirir JunglePass",
                            "Cancelar",
                            ()=>Application.OpenURL(frontApi + "JunglePass?returnUrl=/torneo/"+slug+"/informacion")
                        );
                    }else
                        ShowModal(
                            "Ups! Parece que no tienes Mounedas suficientes para participar en este Torneo.",
                            "Obten Mounedas",
                            "Cancelar",
                            ()=>Application.OpenURL(frontApi + "tokens?returnUrl=/torneo/"+slug+"/informacion")
                        );                        
#else
                    if(junglePass){
                        ShowModal(
                            "Ups! Para ingresar a este torneo, necesitas renovar tu Jungle Pass.",
                            "Adquirir JunglePass",
                            "Cancelar",
                            ()=>Goto("MonouJunglePass")
                        );
                    }else
                        ShowModal(
                            "Ups! Parece que no tienes Mounedas suficientes para participar en este Torneo.",
                            "Obten Mounedas",
                            "Cancelar",
                            ()=>Goto("MonouMounedas")
                        );
#endif
                    btn.SetEnabled(true);
                });
            },err=>{});
        }
        private void ShowTerms(){ Application.OpenURL(termsUrl); }

        private void StartGame(){
            ResetAdvanceLog();
            HideAll();
            content.style.display = DisplayStyle.None;
            closeButton.style.display = DisplayStyle.None;
            gameInstance = Instantiate(gamePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            StartLog();
        }

        private void CloseGame(){
            ShowModal("¿Estas seguro que quieres salir?", "Salir", "Cancelar", ()=>Goto("MonouArcade")); 
        }

        private void ForceUpdate(ScrollView view){
            view.schedule.Execute(() =>
            {
                var fakeOldRect = Rect.zero;
                var fakeNewRect = view.layout;
             
                using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
                evt.target = view.contentContainer;
                view.contentContainer.SendEvent(evt);
                //Debug.Log("success ScrollView: "+fakeOldRect.ToString()+"; "+fakeNewRect.ToString());
            }).Every(1000);
        }

        private Action modalSuccess;
        private void ShowModal(string title, string yesButton, string noButton, Action success){
            modalTitle.text = title;
            modalConfirm.style.display = yesButton.Length>0? DisplayStyle.Flex: DisplayStyle.None;
            modalConfirm.text = yesButton;
            modalCancel.style.display = noButton.Length>0? DisplayStyle.Flex: DisplayStyle.None;
            modalCancel.text = noButton;
            modalSuccess = success;
            modal.style.display = DisplayStyle.Flex;
        }
        private void hideModal(){
            modal.style.display = DisplayStyle.None;
        }

        private void CheckStatus(){
            Get(api + "tournamentBySlug/" + slug, success=>{
                JSONNode td = JSON.Parse(success);
                junglePass = td["data"][0][0][0]["tournament"]["type_visibility"] == "Suscrip";
                string m_statusTornament = td["data"][0][0][0]["tournament"]["tournament_status"];
                string m_start = td["data"][0][0][0]["tournament"]["date_start"]+" "+td["data"][0][0][0]["tournament"]["time_start"];
                string m_finish = td["data"][0][0][0]["tournament"]["date_end"]+" "+td["data"][0][0][0]["tournament"]["time_end"];
                tournametId = td["data"][0][0][0]["tournament"]["id"];
                timestandStart = DateTime.ParseExact(m_start, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                timestandFinish = DateTime.ParseExact(m_finish, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                // Debug.Log(">>>t "+tournametId);
                // Debug.Log(">>>t "+m_statusTornament);
                // Debug.Log(">>>s "+m_start);
                // Debug.Log(">>>f "+timestandStart);
                switch (m_statusTornament){
                    case "0": ShowNotReady(); break; // no ha iniciado
                    case "1": ShowAlReady(); break; // en curso
                    case "2": ShowFinished(); break; // termino
                    case "3": ShowNotReady(); break; // en pausa
                    case "4": ShowNotReady(); break; // esta en preparacion
                }
                UpdatePrizes( td["data"][0][0][0]["tournament"]["prizes"] );
                CheckRegistered();
            }, err=>{ Debug.Log("err ArcadeGame CheckStatus"); });
        }

        private void CheckRegistered(){
            Debug.Log("CheckRegistered>>>");
            Get(api + "tournament/" + tournametId +"/teams/?current_page=1&per_page=1000", success=>{
                bool isRegistered = false;
                JSONNode node = JSON.Parse(success); Debug.Log(success);
                for(int i = 0; i < node["data"][0][0][0]["teams"].Count; i++)
                    if(String.Equals(node["data"][0][0][0]["teams"][i]["creador"]["id"], userId)){
                        isRegistered = true;
                        teamId = node["data"][0][0][0]["teams"][i]["id"];
                        break;
                    }
                Debug.Log(isRegistered);
                if(isRegistered){
                    notReady_registredText.style.display = DisplayStyle.Flex;
                    //alReady_registredText.style.display = DisplayStyle.Flex;
                    alReady_playButton.style.display = DisplayStyle.Flex;
                    gameoverDemo_registredText.style.display = DisplayStyle.Flex;
                    notReady_registerButton.style.display = DisplayStyle.None;
                    alReady_registerButton.style.display = DisplayStyle.None;
                    gameoverDemo_registerButton.style.display = DisplayStyle.None;
                }else{
                    notReady_registredText.style.display = DisplayStyle.None;
                    alReady_registredText.style.display = DisplayStyle.None;
                    alReady_playButton.style.display = DisplayStyle.None;
                    gameoverDemo_registredText.style.display = DisplayStyle.None;
                    notReady_registerButton.style.display = DisplayStyle.Flex;
                    alReady_registerButton.style.display = DisplayStyle.Flex;
                    gameoverDemo_registerButton.style.display = DisplayStyle.Flex;
                }
                HideButtonsForWebGL();
            }, err=>{ Debug.Log("err ArcadeGame CheckRegistered"); });
        }

        private void GetRanking(VisualElement rank, Label emptyText){
            Get(api + "tournament/royale/positions/" + tournametId + "/?orderby=kills", success=>{
                JSONNode data = JSON.Parse(success);
                var cols = rank.Query<VisualElement>(className: "column").ToList();
                Debug.Log(cols.Count);
                for(int i=0; i<cols.Count; i++){
                    cols[i].Clear();
                    Label head = new Label();
                    head.AddToClassList("columnhead");
                    head.text = HEADTITLES[i];
                    cols[i].Add(head);
                }
                int counter=0;
                foreach (JSONNode player in data["data"]){
                    Label pos = new Label(); pos.text = player["place"]; cols[0].Add(pos); pos.AddToClassList("pos"); pos.AddToClassList("row"+counter.ToString());pos.AddToClassList("row");
                    Label name = new Label(); name.text = player["name"]; cols[1].Add(name); name.AddToClassList("name"); name.AddToClassList("row"+counter.ToString());pos.AddToClassList("row");
                    Label points = new Label(); points.text = player["kills"]; cols[2].Add(points); points.AddToClassList("points"); points.AddToClassList("row"+counter.ToString());pos.AddToClassList("row");
                    counter++; if(counter>=maxRankingRows) break;
                }
                emptyText.style.display = counter>0? DisplayStyle.None: DisplayStyle.Flex;
            }, err=>{ Debug.Log("err ArcadeGame GetRanking"); });
        }

        private void SaveScore(Action onSuccess){
            ArcadeScore data = new ArcadeScore();
            data.place = 99;
            data.team_id = teamId;
            data.round = 1;
            data.tournament_id = tournametId;
            data.kills = score;
            data.deaths = 0;
            data.assistence = 0;
            data.confirm = Md5Sum(TANGANANICA+userId.ToString()+score.ToString()+TANGANANA);
            data.log = advanceInterval;
            Post(api + "tournament/match/decision/arcade/", JsonUtility.ToJson(data), success=>{
                Debug.Log("Arcade Save Score "+score.ToString());
                onSuccess();
            }, err=>{ Debug.Log("err ArcadeGame SaveData"); });
        }

        private static string Md5Sum(string strToEncrypt){
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);
            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            return hashString.PadLeft(32, '0');
        }

        private void StartLog(){
            var data = new GameArcadeLog();
            data.type = "start";
            data.table = table;
            data.slug = slug;
            data.user_id = userId;
            data.tipo = isDemo? "practica": "juego";
            Post("https://umff6h7j5duoakqcxvhyypvsja0pguqz.lambda-url.us-east-2.on.aws/", JsonUtility.ToJson(data),
            success=>{ logId = success; }, err=>{ Debug.Log("err ArcadeGame Log start"); });
        }
        private void FinishLog(){
            var data = new GameArcadeLog();
            data.type = "finish";
            data.table = game;
            data.table = table;
            data.id = logId;
            data.user_id = userId;
            data.points = score;
            data.confirm = Md5Sum(TANGANANICA+userId.ToString()+score.ToString()+TANGANANA);
            data.data = advanceInterval;
            Post("https://umff6h7j5duoakqcxvhyypvsja0pguqz.lambda-url.us-east-2.on.aws/", JsonUtility.ToJson(data),
            success=>{  }, err=>{ Debug.Log("err ArcadeGame Log finish"); });
        }

        private int SECONDS_IN_ONE_MINUTE = 60;
        private int SECONDS_IN_ONE_HOUR = 60 * 60;
        private int SECONDS_IN_ONE_DAY = 24 * 60 * 60;
        private string _GetTimer(int diff){
            if(diff<=0) return "00d 00h 00m";
            int days = diff/SECONDS_IN_ONE_DAY; diff -= days*SECONDS_IN_ONE_DAY;
            int hours = diff/SECONDS_IN_ONE_HOUR; diff -= hours*SECONDS_IN_ONE_HOUR;
            int minutes = diff/SECONDS_IN_ONE_MINUTE; diff -= minutes*SECONDS_IN_ONE_MINUTE;
            string result =
                (days<10? "0": "")+days.ToString()+"d "+
                (hours<10? "0": "")+hours.ToString()+"h "+
                (minutes<10? "0": "")+minutes.ToString()+"m";
                //(diff<10? "0": "")+diff.ToString();
            return result;
        }

        private void ExtractDataFromWebGL(){
#if UNITY_WEBGL && !UNITY_EDITOR
            string url = GetURL(); //https://tetris.monou.gg/?userId=78&tournamentId=tetris-test-5&ambiente=https://dev-torneos-fe.monou.gg/
            userId = SearchParameters(url, "userId");
            slug = SearchParameters(url, "tournamentId");
            api = SearchParameters(url, "ambiente");
            switch (api){
                case "https://dev-torneos-fe.monou.gg/":
                    api = "https://pwpawoqa3p63hwi9un57qb2wz.monou.gg/api/";
                    table = "tetrix_monou_stg";
                    break;
                case "https://stg-torneos-fe.monou.gg/":
                    api = "https://e6e6j0v1xah51y9eec0p2f12h.monou.gg/api/";
                    table = "tetrix_monou_stg";
                    break;
                case "https://rel-torneos-fe.monou.gg/":
                    api = "https://keyu65uwekgf21rjs23fgjkds.monou.gg/api/";
                    table = "tetrix_monou_stg";
                    break;
                case "https://monou.gg/":
                    api = "https://dgu2evhs9qmnap4nqu9dhmcw1.monou.gg/api/";
                    table = "tetrix_monou"; //<-- prod
                    break;
            }
#endif
        }
        private void HideButtonsForWebGL(){
#if UNITY_WEBGL && !UNITY_EDITOR
            //hide register button
            alReady_registerButton.style.display = DisplayStyle.None;
            alReady_registredText.style.display = DisplayStyle.None;
            notReady_registerButton.style.display = DisplayStyle.None;
            notReady_registredText.style.display = DisplayStyle.None;
            gameoverDemo_registerButton.style.display = DisplayStyle.None;
            gameoverDemo_registredText.style.display = DisplayStyle.None;
            closeButton.style.display = DisplayStyle.None;
            //hide exit button
#endif
        }
        private string SearchParameters(string url, string key){
            string[] prev = url.Split(key + "=");
            if(prev.Length<2) return "";
            string[] next = prev[1].Split("&");
            return next[0];
        }


        // IEnumerator SetElmBackground(VisualElement elm, string MediaUrl) {
        //     UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        //     yield return request.SendWebRequest();
        //     if(request.result == UnityWebRequest.Result.ConnectionError) Debug.Log(request.error);
        //     else{
        //         Texture2D bg = (Texture2D)((DownloadHandlerTexture) request.downloadHandler).texture;
        //         elm.style.backgroundImage = new StyleBackground(bg);
        //     }
        // }
        void SetTimeout(Action fn, float s){ StartCoroutine(_SetTimeout(fn, s)); }
        IEnumerator _SetTimeout(Action fn, float s){
            yield return new WaitForSeconds(s);
            fn.Invoke();
        }

        // =========================== WEBREQUEST =============================
        public void Post(string url, string json, System.Action<string> success, System.Action<string> error){
            StartCoroutine(LoadUrl(url, json, success, error));
        }
        IEnumerator LoadUrl(string url, string json, System.Action<string> success, System.Action<string> error){
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(url, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //www.SetRequestHeader("Authorization", token);
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
                //_SaveCache(url, www.downloadHandler.text);
            }
        }
        public void Get(string url, System.Action<string> success, System.Action<string> error){
            StartCoroutine(GetUrl(url, success, error));
            Debug.Log(url);
        }
        IEnumerator GetUrl(string url, System.Action<string> success, System.Action<string> error){
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.Send();
            if (www.isNetworkError) {
                error.Invoke(www.error);
                //Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
            }else {
                Debug.Log(www.downloadHandler.text);
                success.Invoke(www.downloadHandler.text);
                //_SaveCache(url, www.downloadHandler.text);
            }
        }

    }

    public class ArcadeRank : ScriptableObject{
        public List<ArcadeRankElement> ranking;
    }
    [Serializable]
    public class ArcadeRankElement {
        public string place;
        public string name;
        public string points;
        public ArcadeRankElement(string p, string n, string s){ place = p; name = n; points = s; }
    }

    [Serializable]
    public class ArcadeScore {
        public string team_id;
        public string tournament_id;
        public int place;
        public int round;
        public int kills;
        public int deaths;
        public int assistence;
        public string confirm;
        public string log;
    }


    [Serializable]
    public class ArcadeRegisterPostData {
        public string player_id;
        public string team_id;
        public string tournament_id;
        public string type_payment;// "Mounedas"
        public ArcadeRegisterPostData(string userId, string teamId, string tournamentId){
            player_id = userId;
            team_id = teamId;
            tournament_id = tournamentId;
            type_payment = "Mounedas";
        }
    }

    [Serializable]
    public class GameArcadeLog {
        public string type;
        public string table;
        public string id;
        public string game;
        public string slug;
        public string user_id;
        public int points;
        public string tipo;
        public string data;
        public string confirm;
    }

#if UNITY_EDITOR
    // *********************** Bundles *************************
    public class GenerateAssetBundles : MonoBehaviour
    {
       [MenuItem("Assets/Create AssetBundle-Monou")]
        static void BuildBundles(){

            List<BundleBa> file2Bundle = new List<BundleBa>();

            GenerateAssetBundles.Scrach("Assets", "AssetBundle-Monou", file2Bundle);

            
            int idx = -1;
            string bundleName = "";
            List<string> fileNames = new List<string>();
            AssetBundleBuild[] buildMap = new AssetBundleBuild[file2Bundle.Count];
            foreach(BundleBa f2b in file2Bundle){
                if(bundleName != f2b.group){
                    bundleName = f2b.group;
                    if(idx>=0){
                        string[] data = new string[fileNames.Count];
                        int c=0; foreach(string n in fileNames){
                            data[c++] = n;
                            print(">>>>" + n);
                        }
                        buildMap[idx].assetNames = data;
                    }
                    idx++;
                    fileNames = new List<string>();
                    buildMap[idx].assetBundleName = "app_" + f2b.group;
                    print(">>group_" + f2b.group);
                }
                fileNames.Add(f2b.file);
            }
            if(idx>=0){
                string[] data = new string[fileNames.Count];
                int c=0; foreach(string n in fileNames){
                    data[c++] = n;
                    print(">>>>" + n);
                }
                buildMap[idx].assetNames = data;
            }

            AssetBundleBuild[] theFinalBuildMap = new AssetBundleBuild[idx+1];
            for(int i=0; i<=idx; i++){
                theFinalBuildMap[i].assetBundleName = buildMap[i].assetBundleName;
                theFinalBuildMap[i].assetNames = buildMap[i].assetNames;
            }

            //print("build WEBGL");
            //BuildPipeline.BuildAssetBundles("AssetBundles/WebGL", theFinalBuildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
            print("build Android");
            BuildPipeline.BuildAssetBundles("AssetBundles/Android", theFinalBuildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
            print("build Windows");
            BuildPipeline.BuildAssetBundles("AssetBundles/Windows", theFinalBuildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
            print("build ios");
            BuildPipeline.BuildAssetBundles("AssetBundles/Ios", theFinalBuildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
            print("build osx");
            BuildPipeline.BuildAssetBundles("AssetBundles/Osx", theFinalBuildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);

        }

        static void Scrach(string path, string bundleKey, List<BundleBa> file2Bundle){
            if(path == "Assets/MonouAvatarWorld") return;
            // toma los archivos en la carpeta
            DirectoryInfo info = new DirectoryInfo(path);
            var directoryFileInfo = info.GetFiles();
            foreach(FileInfo file in directoryFileInfo){
                string[] nn = file.Name.Split(".");
                string ext = nn[nn.Length-1];
                if(ext == "cs") continue;
                if(ext == "unity") continue;
                file2Bundle.Add(new BundleBa(path + "/" + file.Name, bundleKey));
                Debug.Log("Ext: "+ext+": "+file.Name);
            }
            var directoryDirInfo = info.GetDirectories();
            foreach (DirectoryInfo dri in directoryDirInfo){
                 Debug.Log("<< "+dri.Name);
                Scrach(path + "/" +dri.Name, bundleKey, file2Bundle);
            }
        }
    }

    public class BundleBa {
        public string file;
        public string group;
        public BundleBa(string f, string g){
            file = f;
            group = g;
        }
    }
#endif

}