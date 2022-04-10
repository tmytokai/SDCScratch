using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class CommandReceiver : MonoBehaviour
{
    GameManager _gameManager = null;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern void JsSetStateTitle();
    [DllImport("__Internal")] static extern void JsSay(int type);
    #else
    void JsSetStateTitle(){}
    void JsSay(int type){}
    #endif

    public void CommandStart()
    {
        if( _gameManager is not null ) _gameManager.MyStart();
        else{
            SceneManager.LoadScene("Game");
        }
    }

    public void CommandReset()
    {
        if( _gameManager is not null ) _gameManager.Reset();
        else JsSetStateTitle();
    }

    public void CommandShowRoute()
    {
        if( _gameManager is not null ) _gameManager.ShowRoute();
    }

    public void CommandPushRoute( string value )
    {
        if( _gameManager is not null ) _gameManager.PushRoute(value);
    }

    public void CommandRun()
    {
        if( _gameManager is not null ) _gameManager.Run();
        else JsSetStateTitle();
    }

    public void CommandToggleView()
    {
        if( _gameManager is not null ) _gameManager.ToggleView();
    }

    public void CommandAbout()
    {
        if( _gameManager is not null ) _gameManager.About();
    }

    void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        if( _gameManager is null ){
            JsSetStateTitle();
            JsSay((int)GameManager.SayType.Welcome);

            #if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
            #endif
        }
    }

    #if UNITY_EDITOR
    void Update()
    {
        if( Input.GetKeyDown(KeyCode.S)){
            CommandStart();
        }
    }
    #endif
}
