using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class SignalManager : MonoBehaviour
{
    [SerializeField] GameObject _prefabSignal; 
    [SerializeField] Material[] _materialSignal;

    public enum SignalState
    {
        GreenV,
        YellowV,
        RedV,

        GreenH,
        YellowH,
        RedH,
    }

    enum _SignalTime
    {
        Green = 8,
        Yellow = 2,
        Red = 4,
    }

    struct _SignalData
    {
        public GameObject objectSignal;
        public int[] distanceSignal;
    }

    public bool Ready {get; private set;} = false;
    public SignalState State{get;private set;} = SignalState.GreenV;
    public GameObject ObjSignal(int x, int y) => _mapSignal[x,y].objectSignal;
    public float TimeWaitingV{get;private set;}  = 0f;
    public float TimeWaitingH{get;private set;}  = 0f;

    GameManager _gameManager = null;
    MapManager _mapManager = null;

    _SignalData[,] _mapSignal = null;
    bool _init = false;
    bool _enabled = true;
    float _signalTime = 0f;
    float _timer = 0f;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern bool JsGetSpawnSignals();
    #else
    public bool SpawnSignals = true;
    bool JsGetSpawnSignals(){ return SpawnSignals;}
    #endif


    public void Init()
    {        
        // Ready is set to true in Update()
        Ready = false;
        _init = true;

        _enabled = JsGetSpawnSignals();

        State = SignalState.GreenV;
        _signalTime = (int)_SignalTime.Green;
        _timer = 0f;
        TimeWaitingV = 0f;
        TimeWaitingH = (int)_SignalTime.Green + (int) _SignalTime.Yellow + (int)_SignalTime.Red;

        var w = _mapManager.Width;
        var h = _mapManager.Height;
        _mapSignal = new _SignalData[w,h];
        for( var y=0; y<h; ++y){
            for( var x=0; x<w; ++x ){
                _mapSignal[x,y].objectSignal = null;
                _mapSignal[x,y].distanceSignal = new int[]{-1,-1,-1,-1};
            }
        }

        if(_enabled )_SpawnSignals();
    }

    public void PrepareRun()
    {
        // Ready is set to true in Update()
        Ready = false;
        _init = true;
    }

    public int DistanceNextSignal( int x, int y, Car.Direction dir )
    {
        if( !_enabled) return (int)MapManager.Const.Infty;
        // cash
        if( _mapSignal[x,y].distanceSignal[(int)dir] == -1 ){
            var distanceSignal = 0;
            var tmpX = x;
            var tmpY = y;
            while( _mapManager.OnRoad(tmpX,tmpY) ){
                if( _mapSignal[tmpX,tmpY].objectSignal is not null && ( tmpX != x || tmpY != y ) ) break;
                ++distanceSignal;
                if( dir == Car.Direction.Up ) tmpY++;
                if( dir == Car.Direction.Left ) tmpX--;
                if( dir == Car.Direction.Down ) tmpY--;
                if( dir == Car.Direction.Right ) tmpX++;
            }
            if( ! _mapManager.OnRoad(tmpX,tmpY) ) distanceSignal = (int)MapManager.Const.Infty;
            _mapSignal[x,y].distanceSignal[(int)dir] = distanceSignal;
        }

        return _mapSignal[x,y].distanceSignal[(int)dir];       
    }

    void _SpawnSignals()
    {
         Action<int,int,MapManager.Type> instSingnal = (x,y,type) =>{
            var screenX = x*(int)MapManager.Const.SizeBlock + (int)MapManager.Const.SizeBlock/2;
            var screenZ = y*(int)MapManager.Const.SizeBlock + (int)MapManager.Const.SizeBlock/2;
            _mapSignal[x,y].objectSignal = Instantiate( _prefabSignal, new Vector3(screenX, 0, screenZ), Quaternion.identity );
            var i = -1;
            switch (type){
                case MapManager.Type.ULR:
                    i = 0;
                    break;
                case MapManager.Type.ULD:
                    i = 2;
                    break;
                case MapManager.Type.URD:
                    i = 1;
                    break;
                case MapManager.Type.LRD:
                    i = 3;
                break;
            }
            if( i != -1 ){
                var obj = _mapSignal[x,y].objectSignal.transform.GetChild(i).gameObject;
                Destroy(obj);
            }
        };

        var w = _mapManager.Width;
        var h = _mapManager.Height;
        for( var y=0; y<h; ++y){
            for( var x=0; x<w; ++x ){
                var type = _mapManager.GetType(x,y);
                switch(type){
                    case MapManager.Type.ULRD:
                    case MapManager.Type.ULR:
                    case MapManager.Type.ULD:
                    case MapManager.Type.URD:
                    case MapManager.Type.LRD:
                        instSingnal(x,y,type);
                    break;
                }
            }
        }
    }

    void _EmitSignal( SignalState state, bool enable )
    {
        if( enable )  _materialSignal[(int)state].EnableKeyword("_EMISSION");
        else _materialSignal[(int)state].DisableKeyword("_EMISSION");
    }

    void _UpdateSignal()
    {
        if( !_enabled ) return;

        _timer += Time.deltaTime;
        TimeWaitingV = Mathf.Max(0f, TimeWaitingV - Time.deltaTime);
        TimeWaitingH = Mathf.Max(0f, TimeWaitingH - Time.deltaTime);

        if( _timer >= _signalTime ){
            var lng =  Enum.GetValues(typeof(SignalState)).Length;
            State = (SignalState)( ((int)State+1)%lng );
            if( State == SignalState.GreenH || State == SignalState.GreenV ) _signalTime = (int)_SignalTime.Green;
            if( State == SignalState.YellowH || State == SignalState.YellowV ) _signalTime = (int)_SignalTime.Yellow;
            if( State == SignalState.RedV || State == SignalState.RedH ) _signalTime = (int)_SignalTime.Red;

            _timer = 0;
            if(State == SignalState.RedV) TimeWaitingV = (int)_SignalTime.Red + (int)_SignalTime.Green + (int) _SignalTime.Yellow + (int)_SignalTime.Red;
            if(State == SignalState.RedH) TimeWaitingH = (int)_SignalTime.Red + (int)_SignalTime.Green + (int) _SignalTime.Yellow + (int)_SignalTime.Red;

            if(State == SignalState.GreenV ) _EmitSignal( SignalState.GreenV, true );
            else _EmitSignal( SignalState.GreenV, false );

            if(State == SignalState.YellowV ) _EmitSignal( SignalState.YellowV, true );
            else _EmitSignal( SignalState.YellowV, false );

            if(State == SignalState.GreenV || State == SignalState.YellowV ) _EmitSignal( SignalState.RedV, false );
            else _EmitSignal( SignalState.RedV, true );

            if(State == SignalState.GreenH ) _EmitSignal( SignalState.GreenH, true );
            else _EmitSignal( SignalState.GreenH, false );

            if(State == SignalState.YellowH ) _EmitSignal( SignalState.YellowH, true );
            else _EmitSignal( SignalState.YellowH, false );

            if(State == SignalState.GreenH || State == SignalState.YellowH ) _EmitSignal( SignalState.RedH, false );
            else _EmitSignal( SignalState.RedH, true );
        }
    }

    public void MyUpdate()
    {
        if( _gameManager.State == GameManager.StateGame.Run ){
            _UpdateSignal();
        }
        else if( _init ){
            Ready = true;
            _init = false;

            _EmitSignal( SignalState.GreenV, true );
            _EmitSignal( SignalState.YellowV, false );
            _EmitSignal( SignalState.RedV, false );
            _EmitSignal( SignalState.GreenH, false );
            _EmitSignal( SignalState.YellowH, false );
            _EmitSignal( SignalState.RedH, true );
        }
    }

    void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _mapManager = GetComponent<MapManager>();
    }

}
