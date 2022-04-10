using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class CarManager : MonoBehaviour
{
    const float _intervalInvisible = 0.2f; // sec

    [SerializeField] GameObject[] _prefabCars;

    public bool Ready {get; private set;} = false;
    public Car[] HeadCar = new Car[4];

    GameManager _gameManager = null;
    MapManager _mapManager = null;
    bool _initAll = false;
    bool _initMycar = false;
    List<Car> _listCar;
    Car _mycar;
    float _timerInvisible = 0;
    bool _hideBody = false;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern bool JsGetSpawnCars();
    #else
    public bool SpawnCars = true;
    bool JsGetSpawnCars(){ return SpawnCars;}
    #endif

    public void Init()
    {
        // Ready is set to true in Update()
        Ready = false;
        _initAll = true;

        _mycar = GameObject.Find("MyCar").GetComponent<Car>();
        _listCar.Add(_mycar);
        _mycar.No = _listCar.Count;
        var sb = (int)MapManager.Const.SizeBlock;
        _mycar.transform.position = new Vector3(sb*_mapManager.StartX+sb/2,0,sb*_mapManager.StartY+sb/2);

        if( JsGetSpawnCars() ) _SpawnCars();

        _listCar.ForEach( car => car.Init() );
    }

    public void PrepareRun()
    {
        // Ready is set to true in Update()
        Ready = false;
        _initMycar = true;
        
        _mycar.Init();
    }

    void _SpawnCars()
    {
        for(var y = 0; y < _mapManager.Height; ++y ){
            for(var x = 0; x < _mapManager.Width; ++x ){
                if( _mapManager.OnRoad(x,y) && !( x == _mapManager.StartX && y == _mapManager.StartY ) ){
                    var n = UnityEngine.Random.Range(0,_prefabCars.Length);
                    var sb = (int)MapManager.Const.SizeBlock;
                    var position = new Vector3( x*sb+sb/2, 0, y*sb+sb/2 );
                    var obj = Instantiate( _prefabCars[n], position, Quaternion.identity );
                    var car = obj.GetComponent<Car>();
                    _listCar.Add( car );
                    car.No = _listCar.Count;
                    obj.name = "No."+car.No.ToString();
                }
            }
        }
    }

    void _InitSortData()
    {
        var dir = Car.Direction.Up;
        for( var i=0; i<2; ++i){
            var oppositeDir = (Car.Direction)(((int)dir+2)%4);
            
            // insertion sort
            foreach( var car in _listCar ){
                var x = car.transform.position.x;
                var z = car.transform.position.z;
                Car before = null;
                var after = HeadCar[(int)dir];
                while (after != null){
                    if (dir == Car.Direction.Up  && after.transform.position.z > z) break;
                    if (dir == Car.Direction.Right && after.transform.position.x > x) break;
                    before = after;
                    after = before.CarNext[(int)dir];

                }
                if (before == null){
                    HeadCar[(int)dir] = car;
                    car.CarNext[(int)oppositeDir] = null;
                }
                else{
                    before.CarNext[(int)dir] = car;
                    car.CarNext[(int)oppositeDir] = before;
                }
                if (after == null){
                    HeadCar[(int)oppositeDir] = car;
                    car.CarNext[(int)dir] = null;
                }
                else{
                    after.CarNext[(int)oppositeDir] = car;
                    car.CarNext[(int)dir] = after;
                }
            }
            
            dir = Car.Direction.Right;
        }
    }

    public void MyUpdate()
    {
        if( _gameManager.State == GameManager.StateGame.Run ){
            _listCar.ForEach( car => car.MyUpdate() );
            _listCar.ForEach( car => car.MyLateUpdate() );
            _listCar.ForEach( car => car.SetInvisible() );
            _listCar.ForEach( car => car.UnsetInvisible() );        
            _timerInvisible -= Time.deltaTime;
            if( _timerInvisible <= 0f ){
                if( _hideBody )_listCar.ForEach( car => car.HideBody() );
                else _listCar.ForEach( car => car.ShowBody() );
                _hideBody = !_hideBody;
                _timerInvisible = _intervalInvisible;
            }
        }
        else if( _initMycar ){
            _mycar.Initializing();
            if( _mycar.Ready ){
                _InitSortData();
                _listCar.ForEach( car => car.PrepareRun() );
                _initMycar = false;
                _initAll = true;
            }
        }
        else if( _initAll ){
            _listCar.ForEach( car => car.Initializing() );
            foreach( var car in _listCar ){
                if( !car.Ready) return;
            }
            Ready = true;
            _initAll = false;
        }
    }

    void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _mapManager = GetComponent<MapManager>();
        _listCar =  new List<Car>();
    }
}
