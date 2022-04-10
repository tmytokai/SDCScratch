using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum Const
    {
        WidthCanvas = 480,
        HeightCanvas = 360,

        Infty = 10000,
    }

    const int _timeHideMessage = 2;

    public enum StateGame
    {
        Init,
        InitMapManager,
        InitSignalManager,
        InitCarManager,

        Route,

        PrepareRun,
        Run,

        GameOver,
    }

    public enum ReasonGameover
    {
        Clear,
        Hit,
        RedSignal,
        OutOfGas,
        OutOfRoad,
    }

    public enum SayType
    {
        Clear,
        Welcome,
        Run,
        Chat,
        Signal,
        GameOver,
        Goal,
    }

    public StateGame State{get;private set;} = StateGame.Init;

    public int ViewMode{get;private set;} = 1;

    MapManager _mapManager = null;
    CarManager _carManager = null;
    SignalManager _signalManager = null;

    GameObject _cameraMain;
    GameObject _carmeraBird;
    GameObject _CanvasHud;
    GameObject _UiMessage;
    Text _UiMessageText;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern void JsSetStateRoute();
    [DllImport("__Internal")] static extern void JsSetStateGame();
    [DllImport("__Internal")] static extern int JsGetViewMode();
    [DllImport("__Internal")] static extern void JsSay(int type);
    #else
    void JsSetStateRoute(){}
    void JsSetStateGame(){}
    int JsGetViewMode(){ return ViewMode; }
    void JsSay(int type){}
    #endif

    public void GameOver( ReasonGameover reason )
    {
        _UiMessage.SetActive(true);
        if( reason == ReasonGameover.Clear ){
            _UiMessageText.text = "Goal";
            JsSay((int)SayType.Goal);
        }
        else if( reason == ReasonGameover.Hit ){
            _UiMessageText.text = "Hit";
            JsSay((int)SayType.GameOver);
        }
        else if( reason == ReasonGameover.OutOfGas ){
            _UiMessageText.text = "Out Of Gas";
            JsSay((int)SayType.GameOver);
        }
        else if( reason == ReasonGameover.RedSignal ){
            _UiMessageText.text = "Red Signal";
            JsSay((int)SayType.GameOver);
        }
        else _UiMessageText.text = "Game Over";
        State = StateGame.GameOver;
        Time.timeScale = 0;
    }
   
    public void MyStart()
    {
        if( State == StateGame.Route || State == StateGame.Run || State == StateGame.GameOver ){
            SceneManager.LoadScene("Game");
        }
    }

    public void Reset()
    {
        if( State == StateGame.Route || State == StateGame.Run || State == StateGame.GameOver ){
            SceneManager.LoadScene("Title");
        }
    }

    public void ShowRoute()
    {
        if( State == StateGame.Route || State == StateGame.Run ){
            _mapManager.ShowRoute();
            if( State == StateGame.Route ){
                _CanvasHud.SetActive( !_CanvasHud.activeSelf );
                _cameraMain.SetActive( !_cameraMain.activeSelf );
                _carmeraBird.SetActive( !_cameraMain.activeSelf );
            }
        }
    }
    public void PushRoute( string value )
    {
        if( State == StateGame.Route ){
            var dir = (Car.Direction)int.Parse(value);
            _mapManager.PushRoute(dir);
        }
    }

    public void Run()
    {
        if( State == StateGame.Route ){
                                
            if( _mapManager.isRouteMapShown ) ShowRoute();

            State = StateGame.PrepareRun;
            _mapManager.PrepareRun();
            _carManager.PrepareRun();
            _signalManager.PrepareRun();
        }
    }


    public void ToggleView()
    {
        if(State == StateGame.Run ){
            ViewMode = JsGetViewMode();
            if( ViewMode == 1 ){
                _cameraMain.SetActive( true );
                _carmeraBird.SetActive( false );
                _CanvasHud.SetActive( true );
                _cameraMain.transform.localPosition = new Vector3(0,3.5f,-6.5f);
                _cameraMain.transform.localEulerAngles = new Vector3(15f,0,0);
            }
            if( ViewMode == 2 ){
                _cameraMain.SetActive( true );
                _carmeraBird.SetActive( false );
                _CanvasHud.SetActive( true );
                _cameraMain.transform.localPosition = new Vector3(0,1.5f,-0.5f);
                _cameraMain.transform.localEulerAngles = Vector3.zero;
            }
            if( ViewMode == 3 ){
                _cameraMain.SetActive( true );
                _carmeraBird.SetActive( false );
                _CanvasHud.SetActive( true );
                _cameraMain.transform.localPosition = new Vector3(0,3.5f,10f);
                _cameraMain.transform.localEulerAngles = new Vector3(15f,180f,0);
            }
            if( ViewMode == 4 ){
                _cameraMain.SetActive( false );
                _carmeraBird.SetActive( true );
                _CanvasHud.SetActive( false );
            }
        }
    }

    public void About()
    {
        if( State == StateGame.Route || State == StateGame.Run || State == StateGame.GameOver ){
            var about = GameObject.Find("CanvasHud").transform.Find("About").gameObject;
            about.SetActive( ! about.activeSelf );
        }
    }

    IEnumerator _HideMessage()
    {
        yield return new WaitForSeconds(_timeHideMessage);
        _UiMessage.SetActive(false);
    }

    void Awake()
    {
        _mapManager = GetComponent<MapManager>();
        _carManager = GetComponent<CarManager>();
        _signalManager = GetComponent<SignalManager>();

        _cameraMain = Camera.main.gameObject;
        _carmeraBird = GameObject.Find("CameraBird");

        _CanvasHud = GameObject.Find("CanvasHud");
        _UiMessage = _CanvasHud.transform.Find("Message").gameObject;
        _UiMessageText = _UiMessage.GetComponent<Text>();

        #if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
        #endif

        Time.timeScale = 1;
    }

    void Update()
    {
        _mapManager.MyUpdate();
        _signalManager.MyUpdate();
        _carManager.MyUpdate();

        switch( State ){
            case StateGame.Init:
                State = StateGame.InitMapManager;
                _mapManager.Init();
                break;
            case StateGame.InitMapManager:
                if( _mapManager.Ready ){
                    State = StateGame.InitSignalManager;
                    _signalManager.Init();
                }
                break;
            case StateGame.InitSignalManager:
                if( _signalManager.Ready ){
                    State = StateGame.InitCarManager;
                    _carManager.Init();
                }
                break;
            case StateGame.InitCarManager:
                if( _carManager.Ready ){
                    _UiMessageText.text = "GET READY?";
                    _UiMessage.SetActive(true);
                    State = StateGame.Route;
                    JsSetStateRoute();
                }
                break;
            case StateGame.PrepareRun:
                if( _mapManager.Ready && _carManager.Ready && _signalManager.Ready ){
                    _UiMessageText.text = "GO!";
                    _UiMessage.SetActive(true);
                    StartCoroutine("_HideMessage");
                    State = StateGame.Run;
                    JsSetStateGame();
                    JsSay((int)SayType.Run);
                }
            break;
        }

        #if UNITY_EDITOR
        if( Input.GetKeyDown(KeyCode.Escape)){
            Reset();
        }
        if( Input.GetKeyDown(KeyCode.S)){
            MyStart();
        }
        if( Input.GetKeyDown(KeyCode.R)){
            Run();
        }
        if( Input.GetKeyDown(KeyCode.M)){
            ShowRoute();
        }
        if( Input.GetKeyDown(KeyCode.A)){
            About();
        }
        if( Input.GetKeyDown(KeyCode.UpArrow)){
            PushRoute("0");
        }
        if( Input.GetKeyDown(KeyCode.LeftArrow)){
            PushRoute("1");
        }
        if( Input.GetKeyDown(KeyCode.DownArrow)){
            PushRoute("2");
        }
        if( Input.GetKeyDown(KeyCode.RightArrow)){
            PushRoute("3");
        }
        if( Input.GetKeyDown(KeyCode.Alpha1)){
            ViewMode = 1;
            ToggleView();
        }
        if( Input.GetKeyDown(KeyCode.Alpha2)){
            ViewMode = 2;
            ToggleView();
        }
        if( Input.GetKeyDown(KeyCode.Alpha3)){
            ViewMode = 3;
            ToggleView();
        }
        if( Input.GetKeyDown(KeyCode.Alpha4)){
            ViewMode = 4;
            ToggleView();
        }
        #endif
    }

}
