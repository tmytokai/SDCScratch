using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class MyCar : MonoBehaviour, InterfaceCar
{
    public bool AutoChangeSpeed = false;
    public bool AutoCreateRoute = false;

    public float RedSignalMargin = 5.0f;

    GameManager _gameManager = null;
    MapManager _mapManager = null;
    SignalManager _signalManager = null;
    Car _car;
    InterfaceCar _aicar = null;
    bool _passRedSignal = false;
    int _prePosX = 0;
    int _prePosY = 0;
    int _preTimeWaiting = -1;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern void JsSay(int type);

    [DllImport("__Internal")] static extern void JsSetTimeWaiting(int timeWaiting);
    [DllImport("__Internal")] static extern void JsSetXY(int x, int y);

    [DllImport("__Internal")] static extern void JsSetSpeed(int speed);
    [DllImport("__Internal")] static extern int JsGetTargetSpeed();


    [DllImport("__Internal")] static extern void JsSetNavigatorDistance(int distance);
    [DllImport("__Internal")] static extern void JsSetNavigatorDirection(int direction);

    [DllImport("__Internal")] static extern void JsSetSignalDistance(int distance);
    [DllImport("__Internal")] static extern void JsSetSignalState(int state);

    [DllImport("__Internal")] static extern void JsSetLeftSensor(int distance);
    [DllImport("__Internal")] static extern void JsSetForwardLeftSensor(int distance);
    [DllImport("__Internal")] static extern void JsSetForwardRightSensor(int distance);
    [DllImport("__Internal")] static extern void JsSetRightSensor(int distance);
    #else
    void JsSay(int type){}

    void JsSetTimeWaiting(int timeWaiting){}
    void JsSetXY(int x, int y){}

    void JsSetSpeed(int speed){}
    int _targetSpeed = 0;
    int JsGetTargetSpeed(){
        return _targetSpeed;
    }  

    void JsSetNavigatorDistance(int distance){}
    void JsSetNavigatorDirection(int direction){}

    void JsSetSignalDistance(int distance){}
    void JsSetSignalState(int state){}

    void JsSetLeftSensor(int distance){}
    void JsSetForwardLeftSensor(int distance){}
    void JsSetForwardRightSensor(int distance){}
    void JsSetRightSensor(int distance){}

    #endif

    void InterfaceCar.CreateRoute()
    {
        if( !AutoCreateRoute ){
            _car.ClearRoute();
            if(_mapManager.Route.Count > 0 ){
                _mapManager.Route.ForEach( direction => _car.PushRoute(direction) );
                _mapManager.MakeRouteLoop();
            }
        }
        else _aicar.CreateRoute();
    }

    void InterfaceCar.ChangeSpeed()
    {
        #if UNITY_EDITOR
        if( Input.GetKeyDown(KeyCode.Z)){
             _targetSpeed += 10000;
        }
        if( Input.GetKeyDown(KeyCode.X)){
            _targetSpeed = Mathf.Max(0,_targetSpeed-10000);
        }
        #endif
    
        if( !AutoChangeSpeed ) _car.TargetSpeed = JsGetTargetSpeed();
        else _aicar.ChangeSpeed();
    }

    void InterfaceCar.Move()
    {
        var preX = _car.MapX;
        var preY = _car.MapY;

        if( !AutoCreateRoute ){

            if( !_car.Move() ){
                _gameManager.GameOver( GameManager.ReasonGameover.OutOfGas );
            }
            
            var posX = _car.PositionX[(int)Car.TransformPosition.Front];
            var posY = _car.PositionY[(int)Car.TransformPosition.Front];
            if( _prePosX != posX || _prePosY != posY ){

                if( (posX == 1 && posY == 1 ) ||
                    (posX == 1 && posY == 2 ) ||
                    (posX == 2 && posY == 1 ) ||
                    (posX == 2 && posY == 2 )
                ){
                    if(_car.MapX == _mapManager.GoalX && _car.MapY == _mapManager.GoalY ) _gameManager.GameOver( GameManager.ReasonGameover.Clear );
                    if( _car.SensorSignal.distance == 0 && _car.SensorSignal.state == Car.SensorSignalState.Red && _passRedSignal == false ){
                        _gameManager.GameOver( GameManager.ReasonGameover.RedSignal );
                    }
                }

                if(
                    (posX == 0 && posY == 0 ) ||
                    (posX == 0 && posY == 3 ) ||
                    (posX == 3 && posY == 0 ) ||
                    (posX == 3 && posY == 3 )
                ){
                    _gameManager.GameOver( GameManager.ReasonGameover.Hit );
                }

                _prePosX = _car.PositionX[(int)Car.TransformPosition.Front];
                _prePosY = _car.PositionY[(int)Car.TransformPosition.Front];
            }

            if(_car.SensorSignal.distance > RedSignalMargin){
                _passRedSignal = false;
            }
            else if(_car.SensorSignal.state == Car.SensorSignalState.Yellow)
            {
                _passRedSignal = true;
            }
        }
        else _aicar.Move();

        if( _gameManager.State == GameManager.StateGame.Run && _car.Speed == 0 && _car.SensorSignal.state == Car.SensorSignalState.Red && _car.SensorSignal.distance > 0 && _car.SensorSignal.distance < (int)MapManager.Const.SizeBlock/3*2 ){
            var timeWaiting = (int)( 0.1f + ( ( _car.Forward == Car.Direction.Up || _car.Forward == Car.Direction.Down ) ? _signalManager.TimeWaitingV : _signalManager.TimeWaitingH) );
            if( timeWaiting >= 0 && timeWaiting != _preTimeWaiting ){
                _preTimeWaiting = timeWaiting;
                JsSetTimeWaiting(_preTimeWaiting);
                JsSay((int)GameManager.SayType.Signal);
            }
        }
        else{
            _preTimeWaiting = -1;
            JsSetTimeWaiting(_preTimeWaiting);
        }

        if( _gameManager.State == GameManager.StateGame.Run && (preX != _car.MapX || preY != _car.MapY) ){
            JsSay((int)GameManager.SayType.Chat);
        }

        JsSetXY((int)_car.MapX, (int)_car.MapY);
        JsSetSpeed((int)_car.Speed);

        JsSetNavigatorDistance((int)_car.Navigator.distance);
        JsSetNavigatorDirection((int)_car.Navigator.direction);

        JsSetSignalDistance((int)_car.SensorSignal.distance);
        JsSetSignalState((int)_car.SensorSignal.state);

        JsSetLeftSensor((int)_car.SensorCar[(int)Car.SensorCarPosition.Left].distance);
        JsSetForwardLeftSensor((int)_car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].distance);
        JsSetForwardRightSensor((int)_car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].distance);
        JsSetRightSensor((int)_car.SensorCar[(int)Car.SensorCarPosition.Right].distance);
    }

    void InterfaceCar.Hit(Car car)
    {
        _gameManager.GameOver( GameManager.ReasonGameover.Hit );       
    }

    void Awake()
    {
        var m = GameObject.Find("Manager");
        _gameManager = m.GetComponent<GameManager>();
        _mapManager = m.GetComponent<MapManager>();
        _signalManager = m.GetComponent<SignalManager>();
        _car = GetComponent<Car>();
        _aicar = GetComponent<AICar>();
    }

}
