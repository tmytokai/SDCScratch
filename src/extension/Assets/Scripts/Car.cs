using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

interface InterfaceCar
{
    void CreateRoute();
    void ChangeSpeed();
    void Move();
    void Hit(Car car);
}

public class Car : MonoBehaviour
{
    const float _radiusSmallTurn = 5f;
    const float _radiusNormalTurn = 5f;
    const float _radiusLargeTurn = 5f;
    const float _radiusHugeTurn = 8f;

    const float _targetSlideSmallTurnBefore = 1f;
    const float _targetSlideNormalTurnBefore = 0f;
    const float _targetSlideLargeTurnBefore = 0f;
    const float _targetSlideHugeTurnBefore = 1f;

    const float _targetSlideSmallTurnAfter = 0f;
    const float _targetSlideNormalTurnAfter = 0f;
    const float _targetSlideLargeTurnAfter = 0f;
    const float _targetSlideHugeTurnAfter = 1f;

    const float _targetForwardSmallTurn  = (int)MapManager.Const.SizeBlock/2f - (int)MapManager.Const.WidthRoad/2f - _targetSlideSmallTurnAfter  -  _radiusSmallTurn;
    const float _targetForwardNormalTurn = (int)MapManager.Const.SizeBlock/2f - (int)MapManager.Const.WidthRoad/2f - _targetSlideNormalTurnAfter -  _radiusNormalTurn;
    const float _targetForwardLargeTurn  = (int)MapManager.Const.SizeBlock/2f + (int)MapManager.Const.WidthRoad/2f - _targetSlideLargeTurnAfter  -  _radiusLargeTurn;
    const float _targetForwardHugeTurn   = (int)MapManager.Const.SizeBlock/2f + (int)MapManager.Const.WidthRoad/2f - _targetSlideHugeTurnAfter   -  _radiusHugeTurn;

    const float _dForward2dSlide = 0.2f;
    const float _dSpeedUp = 10000f;
    const float _dSpeedDown = 30000f;
    const float _dSpeedDownHard = 50000f;

    // if Speed is over _speedLimitCurve when turning, radius lengthens for (Speed - _speedLimitCurve )*_metersPerSpeedCurve meters.
    const float _speedLimitCurve = 30000;
    const float _metersPerSpeedCurve = 0.3f/1000f;

    const float _intervalRescanSensorCar = 0.2f; // sec
    
    public enum Direction
    {
        Up,
        Left,
        Down,  
        Right,      
    }

    public enum _Mode
    {
        Straight,
        TurnSmallLeft,
        TurnNormalLeft,
        TurnLargeRight,
        TurnHugeRight,
        Stop,
    }

    enum _TranslateMode
    {
        Forward,
        Rotate,
    }

    public enum TransformPosition
    {
        Front,
        Center,
        Rear,
    }

    public enum NavigatorDirection
    {
        Left,
        Forward,
        Right,
    }

    public struct NavigatorData
    {
        public float distance;
        public NavigatorDirection direction;
        public int turnX;
        public int turnY;
    }

    public enum SensorCarPosition
    {
        Left,
        ForwardLeft,
        ForwardRight,
        Right,
    }

    public struct SensorCarData
    {
        public float distance;
        public Car car;
        public Transform transform;
    }

    // position of LiDAR
    //
    //          This.Forward  (Not North !)
    //
    //            r o a d
    // Y         |   |   |     
    //           |   |   |
    // 3         |   |   |
    //           |   |   |
    //----------------------------
    // 2         |   |   |         r
    //---------------------------- o
    // 1         |   |   |         a
    //---------------------------- d
    //           |   |   |
    // 0         |   |   |
    //      0    | 1 | 2 |    3     X
    //
    static int[,,] _position2LidarPositionX ={
        // this.Forward = UP
        {
            // car.PositionX = 0
            {
                0, // car.PoPositionY = 0
                0, // car.PoPositionY = 1
                0, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                1, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                1, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                2, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                2, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                3, // car.PoPositionY = 0
                3, // car.PoPositionY = 1
                3, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
        },
        // this.Forward = Left
        {
            // car.PositionX = 0
            { 
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
        },
        // this.Forward = Down
        {
            // car.PositionX = 0
            {
                3, // car.PoPositionY = 0
                3, // car.PoPositionY = 1
                3, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                2, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                2, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                1, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                1, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                0, // car.PoPositionY = 0
                0, // car.PoPositionY = 1
                0, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
        },
        // this.Forward = Right
        {
            // car.PositionX = 0
            { 
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
        },
    };
    static int[,,] _position2LidarPositionY ={
        // this.Forward = UP
        {
            // car.PositionX = 0
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                0, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
        },
        // this.Forward = Left
        {
            // car.PositionX = 0
            { 
                3, // car.PoPositionY = 0
                3, // car.PoPositionY = 1
                3, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                2, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                2, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                1, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                1, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                0, // car.PoPositionY = 0
                0, // car.PoPositionY = 1
                0, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
        },
        // this.Forward = Down
        {
            // car.PositionX = 0
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                3, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
        },
        // this.Forward = Right
        {
            // car.PositionX = 0
            { 
                0, // car.PoPositionY = 0
                0, // car.PoPositionY = 1
                0, // car.PoPositionY = 2
                0, // car.PoPositionY = 3
            },
            // car.PositionX = 1
            {
                1, // car.PoPositionY = 0
                1, // car.PoPositionY = 1
                1, // car.PoPositionY = 2
                1, // car.PoPositionY = 3
            },
            // car.PositionX = 2
            {
                2, // car.PoPositionY = 0
                2, // car.PoPositionY = 1
                2, // car.PoPositionY = 2
                2, // car.PoPositionY = 3
            },
            // car.PositionX = 3
            {
                3, // car.PoPositionY = 0
                3, // car.PoPositionY = 1
                3, // car.PoPositionY = 2
                3, // car.PoPositionY = 3
            },
        },
    };

    public enum SensorSignalState
    {
        Green,
        Yellow,
        Red,
    }

    public struct SensorSignalData
    {
        public float distance;
        public Transform transform;
        public SensorSignalState state;
    }

    public int No = 0;

    public bool Ready {get; private set;} = false;

    // position on the map
    //
    // X = (int)(transform.position.x/MapManager.Length.SizeBlock)
    // Y = (int)(transform.position.z/MapManager.Length.SizeBlock)
    //
    public int MapX {get;private set;} = 0;
    public int MapY {get;private set;} = 0;

    // position in a block
    //
    //             North
    //
    //            r o a d
    // Y         |   |   |     
    //           |   |   |
    // 3         |   |   |
    //           |   |   |
    //----------------------------
    // 2         |   |   |         r
    //---------------------------- o
    // 1         |   |   |         a
    //---------------------------- d
    //           |   |   |
    // 0         |   |   |
    //      0    | 1 | 2 |    3    X
    //
    public int[] PositionX {get;private set;} = null; // {front, center, center}
    public int[] PositionY {get;private set;} = null;  // {front, center, center}
 
    public Car.Direction Forward {get;private set;} = Car.Direction.Up;
    public Car.Direction Left {get;private set;} = Car.Direction.Left;
    public Car.Direction Right {get;private set;} = Car.Direction.Right;
    public Car.Direction Back {get;private set;} = Car.Direction.Down;
    public float Speed {get;private set;} = 0f;   // meters/hour
    public float TargetSpeed {get;set;} = 0f; // meters/hour
    public bool TurningRight => ( (_mode == _Mode.TurnLargeRight || _mode == _Mode.TurnHugeRight) );
    public bool TurningLeft => ( (_mode == _Mode.TurnSmallLeft || _mode == _Mode.TurnNormalLeft) );

    public Car[] CarNext {get; set;} = null;
    
    public NavigatorData Navigator { get{ return _navigatorData;} private set{}}
    public SensorCarData[] SensorCar{ get{ return _sensorCar;} private set{}}
    public SensorSignalData SensorSignal { get{ return _sensorSignal;} private set{}}

    public Collider Coll {get;private set;} = null;

    GameManager _gameManager = null;
    SignalManager _signalManager = null;
    MapManager _mapManager = null;
    CarManager _carManager = null;
    InterfaceCar _interface = null;
    Hud _hud = null;
    GameObject _body = null;
    GameObject _shadow = null;
    Transform[] _transform = null; 
    int[] _prePositionX = null;
    int[] _prePositionY= null;

    _Mode _mode = _Mode.Stop;
    int _state = 0;
    float _targetForward = 0f;
    float _targetSlide = 0f;
    float _targetAngle = 0f;
    float _dForward2dAngle = 0f;
    Vector3 _centerRotate;
    int _skipMove = 0;
    bool _invisible = false;
    List<int> _listTriggerEnter = null;

    public NavigatorData _navigatorData;
    List<Direction> _route;
    int _idxRoute = 0;
 
    SensorCarData[] _sensorCar = null;
    float _timerRescanSensorCar = 0f;

    SensorSignalData _sensorSignal;

    public void Init()
    {
        // Ready is set to true in Initializing()
        Ready = false;

        _UpdatePosition();

        if( _route.Count == 0 ){
            // set the first direction randomly
            Car.Direction[] firstDir = new Car.Direction[4];
            var n = 0;
            foreach( Car.Direction dir in Enum.GetValues(typeof(Car.Direction)) ){
                if( _mapManager.DistanceEndOfStreet(MapX,MapY,dir) >= 1 ) firstDir[n++] = dir;
            }
            _InitPosition(firstDir[UnityEngine.Random.Range(0,n)]);
            _interface.CreateRoute();
        }

        if( _route.Count > 0 ) _InitPosition(_route[0]);

        _mode = _Mode.Stop;
    }

    public void PrepareRun()
    {
        // Ready is set to true in Initializing()
        Ready = false;

        if(_hud is not null) _hud.PrepareRun();

        _ScanSensorCar();
        _ScanSensorSignal();
    }

    void _AdjustTransform()
    {
        var x = transform.position.x;
        var z = transform.position.z;
        var sb = (float)MapManager.Const.SizeBlock;
        var wr = (float)MapManager.Const.WidthRoad;
        switch (Forward){
            case Car.Direction.Up:
                transform.eulerAngles = new Vector3(0, 0, 0);
                _targetSlide = (MapX * sb + sb/2 - wr/2f) - x;
                break;
            case Car.Direction.Left:
                transform.eulerAngles = new Vector3(0, -90, 0);
                _targetSlide = (MapY*sb + sb/2 - wr/2f) - z;
                break;
            case Car.Direction.Down:
                transform.eulerAngles = new Vector3(0, 180, 0);
                _targetSlide = -( (MapX * sb + sb/2 + wr/2f) - x);
                break;
            case Car.Direction.Right:
                transform.eulerAngles = new Vector3(0, 90, 0);
                _targetSlide = -( (MapY*sb + sb/2 + wr/2f) - z);
            break;
        }
    }

    bool _Translate( _TranslateMode tmode )
    {
       // if( Invisible ) return true;

        var ret = true;
        var dForward = Speed/3600f*Time.deltaTime;
        var preX = MapX;
        var preY = MapY;

        if( tmode == _TranslateMode.Forward ){
            if( _targetForward != 0 ){
                if( _targetForward - dForward < 0 ){
                    dForward = _targetForward;
                    _targetForward = 0;
                    ret = false;
                }
                else _targetForward -= dForward;
            }
            transform.Translate(0,0,dForward);
        }
        else if( tmode == _TranslateMode.Rotate ){
            var sgn = Mathf.Sign(_targetAngle);
            var dAngle = _dForward2dAngle*dForward;
            if( sgn*(_targetAngle - dAngle) < 0 ){
                dAngle = _targetAngle;
                _targetAngle = 0f;
                ret = false;
            }
            else _targetAngle -= dAngle;
            transform.RotateAround( _centerRotate, Vector3.up, dAngle );
        }

        // slide
        if( _targetSlide != 0 ){
            var sgn = Mathf.Sign(_targetSlide);
            var dSlide = _dForward2dSlide*dForward*sgn;
            if( sgn*(_targetSlide - dSlide) < 0 ){
                dSlide = _targetSlide;
                _targetSlide = 0f;
            }
            else _targetSlide -= dSlide;
            transform.Translate(dSlide,0,0);
        }

        _UpdatePosition();
        _UpdateCarNext();
        _UpdateNavigator(dForward);
        _UpdateSensorCar(preX,preY);
        _UpdateSensorSignal(dForward);

        return ret;
    }

    bool _Straight()
    {
        var preX = MapX;
        var preY = MapY;

        _Translate(_TranslateMode.Forward);

        if( _sensorSignal.distance == 0 || _sensorSignal.distance == (int)MapManager.Const.Infty ){
            var tpos = TransformPosition.Rear;
            if(     (PositionX[(int)tpos] == 0 && _prePositionX[(int)tpos] != 0 )
                 || (PositionX[(int)tpos] == 3 && _prePositionX[(int)tpos] != 3 )
                 || (PositionY[(int)tpos] == 0 && _prePositionY[(int)tpos] != 0 )
                 || (PositionY[(int)tpos] == 3 && _prePositionY[(int)tpos] != 3 ) 
            ){
                _ScanSensorSignal();
            }
        }

        if( preX != MapX || preY != MapY ){
            _mode = _Mode.Stop;
            _state = 0;
            return false;
        }

        return true;
    }

    bool _Turn()
    {
        var preX = MapX;
        var preY = MapY;
        if( _state == 0 ){
            if( _mode == _Mode.TurnSmallLeft ){
                _targetForward = _targetForwardSmallTurn;
                _targetSlide = -_targetSlideSmallTurnBefore;
            }
            if( _mode == _Mode.TurnNormalLeft ){
                _targetForward = _targetForwardNormalTurn;
                _targetSlide = -_targetSlideNormalTurnBefore;
            }
            if( _mode == _Mode.TurnLargeRight ){
                _targetForward = _targetForwardLargeTurn;
                _targetSlide = _targetSlideLargeTurnBefore;
            }
            if( _mode == _Mode.TurnHugeRight ){
                _targetForward = _targetForwardHugeTurn;
                _targetSlide = _targetSlideHugeTurnBefore;
            }
            _state = 1;
        }
        if( _state == 1 ){
            if( ! _Translate(_TranslateMode.Forward)){
                var radius = 0f;
                if( _mode == _Mode.TurnSmallLeft ){
                    radius =  -_radiusSmallTurn;
                    _targetAngle = -60f;
                    //Destination = Left;
                }
                if( _mode == _Mode.TurnNormalLeft ){
                    radius =  -_radiusNormalTurn;
                    _targetAngle = -60f;
                    //Destination = Left;
                }
                if( _mode == _Mode.TurnLargeRight ){
                    radius = _radiusLargeTurn;
                    _targetAngle = 60f;
                    //Destination = Right;
                }
                if( _mode == _Mode.TurnHugeRight ){
                    radius = _radiusHugeTurn;
                    _targetAngle = 60f;
                    //Destination = Right;
                }
                if( Speed > _speedLimitCurve ) radius += Mathf.Sign(radius)*(Speed - _speedLimitCurve)*_metersPerSpeedCurve;
                _dForward2dAngle = Mathf.Rad2Deg/radius;
                _centerRotate = transform.position + radius * transform.right;
                _state = 2;
            }
        }
        if( _state == 2 ){

            // always scan the other car while rotating
            //_timerRescanSensorCar = 0f;

            if( ! _Translate(_TranslateMode.Rotate) ){
                if( _mode == _Mode.TurnSmallLeft ){
                    _targetAngle = -30f;
                    _targetSlide += _targetSlideSmallTurnAfter;
                }
                if( _mode == _Mode.TurnNormalLeft ){
                    _targetAngle = -30f;
                    _targetSlide += _targetSlideNormalTurnAfter;
                }
                if( _mode == _Mode.TurnLargeRight ){
                    _targetAngle = 30f;
                    _targetSlide += -_targetSlideLargeTurnAfter;
                }
                if( _mode == _Mode.TurnHugeRight ){
                    _targetAngle = 30f;
                    _targetSlide += -_targetSlideHugeTurnAfter;
                }
                _state = 3;
            }
        }
        if( _state == 3 ){

            // always scan the other car while rotating
            _timerRescanSensorCar = 0f;

            if( ! _Translate(_TranslateMode.Rotate) ){

                if( _mode == _Mode.TurnSmallLeft || _mode == _Mode.TurnNormalLeft ) _UpdateDirection(Left);
                if( _mode == _Mode.TurnLargeRight || _mode == _Mode.TurnHugeRight ) _UpdateDirection(Right);
                _AdjustTransform();

                _ClearNavigator();

                _timerRescanSensorCar = 0f;
                _RequestScanSensorCar(Forward,MapX,MapY);
                _RequestScanSensorCar(Back,MapX,MapY);
                _RequestScanSensorCar(Left,MapX,MapY);
                _RequestScanSensorCar(Right,MapX,MapY);

                _ScanSensorSignal();

                _state = 4;
            }
        }
        if( _state == 4 ){
            _Translate(_TranslateMode.Forward);
        }

        if( preX != MapX || preY != MapY ){
            _mode = _Mode.Stop;
            _state = 0;
            return false;
        }

        return true;
    }

    public bool Move()
    {
        if( _mode == _Mode.Stop ){
            if( ! _PopRoute() ) return false;
        }

        // skip some frames and rescan cars when restarting the car turning right
        if( _skipMove == 0  && Speed == 0 && TargetSpeed > 0 && _navigatorData.direction == NavigatorDirection.Right ){
            _skipMove = 3;
        }
        if( _skipMove > 0 ){
            --_skipMove;
            _timerRescanSensorCar = 0;
            if( _skipMove > 0 ) TargetSpeed = 0;
        }

        if( Speed < TargetSpeed ) Speed = Mathf.Min( TargetSpeed, Speed + _dSpeedUp*Time.deltaTime );
        else if( Speed > TargetSpeed ){
            if( TurningLeft || TurningRight ) Speed = Mathf.Max( TargetSpeed, Speed - _dSpeedDownHard*Time.deltaTime );
            else  Speed = Mathf.Max( TargetSpeed, Speed - _dSpeedDown*Time.deltaTime );
        }

        var ret = false;
        switch(_mode){
            case _Mode.Straight:
                ret = _Straight();
            break;

            case _Mode.TurnSmallLeft:
            case _Mode.TurnNormalLeft:
            case _Mode.TurnLargeRight:
            case _Mode.TurnHugeRight:
                ret = _Turn();
            break;
        }
        if( !ret ){
            ret = _PopRoute();
            if( !ret ){
                _interface.CreateRoute();
                ret = _PopRoute();
            }
        }

        return ret;
    }

    public void ClearRoute()
    {
        _route.Clear();
        _idxRoute = 0;
        _mode = _Mode.Stop;
    }

    public void PushRoute( Direction dir )
    {
        _route.Add(dir);
    }

    bool _PopRoute()
    {
        bool ret = false;
        if( _route.Count == 0 || _idxRoute == _route.Count ){
            _mode = _Mode.Stop;
            ret = false;
        }
        else{
            var dir = _route[_idxRoute];
            if( Forward == dir ){
                _mode = _Mode.Straight;
                _idxRoute++;
                ret = true;
            }
            else if( dir == Left ){
                if( _signalManager.ObjSignal(MapX,MapY) is not null ) _mode = _Mode.TurnSmallLeft;
                else _mode = _Mode.TurnNormalLeft;
                _idxRoute++;
                ret = true;
            }
            else if( dir == Right ){
                if( _signalManager.ObjSignal(MapX,MapY) is not null ) _mode = _Mode.TurnHugeRight;
                else _mode = _Mode.TurnLargeRight;
                _idxRoute++;
                ret = true;
            }
            // U turn is prohibited
            else{
                _idxRoute = _route.Count;
                _mode = _Mode.Stop;
                ret = false;
            }
        }

        _ScanNavigator();
        return ret;
    }

    public void ShowNavigatorData()
    {
        Debug.Log( $"{_navigatorData.distance}, {_navigatorData.direction}, {_navigatorData.turnX}, {_navigatorData.turnY}");
    }

    void _ClearNavigator()
    {
        _navigatorData.direction = NavigatorDirection.Forward;
        _navigatorData.distance = (int)MapManager.Const.Infty;
        _navigatorData.turnX = (int)MapManager.Const.Infty;
        _navigatorData.turnY = (int)MapManager.Const.Infty;
    }

    void _ScanNavigator()
    {
        _ClearNavigator();

        if( _mode == _Mode.Stop ) return;

        var sb = (int)MapManager.Const.SizeBlock;
        var offset = 0f;
        var tpos = TransformPosition.Front;
        if (Forward == Direction.Up) offset = _transform[(int)tpos].position.z - MapY*sb;
        else if (Forward == Direction.Left) offset = (MapX+1)*sb - _transform[(int)tpos].transform.position.x;
        else if (Forward == Direction.Down) offset = (MapY+1)*sb - _transform[(int)tpos].transform.position.z;
        else offset = _transform[(int)tpos].transform.position.x - MapX*sb;

        var distance = 0;
        var tmpX = MapX;
        var tmpY = MapY;
        for (var i = _idxRoute-1; i < _route.Count; ++i){
            var rt = _route[i];
            if (rt == Forward) distance++;
            else{
                if( rt == Left ) _navigatorData.direction = NavigatorDirection.Left;
                else if( rt == Right ) _navigatorData.direction = NavigatorDirection.Right;
                _navigatorData.distance = distance*sb + sb/3 - offset;
                _navigatorData.turnX = tmpX;
                _navigatorData.turnY = tmpY;
                break;
            }
            if( rt == Direction.Up ) tmpY++;
            if( rt == Direction.Left ) tmpX--;
            if( rt == Direction.Down) tmpY--;
            if( rt == Direction.Right) tmpX++;
        }

        if( _navigatorData.direction == NavigatorDirection.Forward ) _ClearNavigator();
    }

    void _UpdateNavigator(float dForward )
    {
        if( _navigatorData.distance < (int)MapManager.Const.Infty ) {
            _navigatorData.distance = Mathf.Max( 0f, _navigatorData.distance - dForward );
        }
    }

    void _ClearSensorCar(Car.SensorCarPosition spos)
    {
        _sensorCar[(int)spos].distance = (int)MapManager.Const.Infty;
        _sensorCar[(int)spos].car = null;
        _sensorCar[(int)spos].transform = null;
    }

    void _ScanSensorCar()
    {
         Func<int,int,int,int,int,int,bool> onStreet = (x,y,startX,startY,endX,endY) =>{
                if( startX > endX ){
                    var tmp = endX;
                    endX = startX;
                    startX = tmp;
                }
                if( startY > endY ){
                    var tmp = endY;
                    endY = startY;
                    startY = tmp;
                }
                if( x < startX || endX < x ) return false;
                if( y < startY || endY < y ) return false;
                return true;
         };

        Action <SensorCarPosition,TransformPosition,Car,float> setSensor = (spos,tpos,car,distance) =>{
            if( distance < _sensorCar[(int)spos].distance ){
                _sensorCar[(int)spos].car = car;
                _sensorCar[(int)spos].transform = car._transform[(int)tpos];
                _sensorCar[(int)spos].distance = distance;
            }
        };

        foreach( Car.SensorCarPosition spos in Enum.GetValues(typeof(Car.SensorCarPosition)) ) _ClearSensorCar(spos);

        var endX = _mapManager.EndOfStreetX(MapX,MapY,Forward);
        var endY = _mapManager.EndOfStreetY(MapX,MapY,Forward);
        var frontPos = _transform[(int)TransformPosition.Front].position;
        var rearPos = _transform[(int)TransformPosition.Rear].position;

        bool stopFowardLeftScan = false;
        bool stopForwardRightScan = false;
        if( _navigatorData.direction == NavigatorDirection.Forward ||_navigatorData.direction == NavigatorDirection.Left ) stopForwardRightScan = true;

        // scan the upper side
        var car = CarNext[(int)Forward];
        while(car is not null){

            if( _navigatorData.direction != NavigatorDirection.Forward 
            && ( ( Forward == Direction.Up   && car.MapY > _navigatorData.turnY )
              || ( Forward == Direction.Left && car.MapX < _navigatorData.turnX )
              || ( Forward == Direction.Down && car.MapY < _navigatorData.turnY )
              || ( Forward == Direction.Right && car.MapX > _navigatorData.turnX ) )
            ){
                stopFowardLeftScan = true;
            }

            if( onStreet(car.MapX,car.MapY,MapX,MapY,endX,endY) 
            && ( ! car._invisible || (car._invisible && car.Forward == Forward ) )
            ){

                var carFrontPos = car._transform[(int)TransformPosition.Front].position;
                var carRearPos  = car._transform[(int)TransformPosition.Rear].position;

                var behind = ( Vector3.Dot(frontPos,carFrontPos ) <= 0 || Vector3.Dot(frontPos,carRearPos) <= 0 );
                if( !behind ){

                    var dFront = carFrontPos - frontPos;
                    var dRear = carRearPos - frontPos;
                    var distanceFront = dFront.magnitude;
                    var distanceRear = dRear.magnitude;
                    var dxFront = Mathf.Abs(dFront.x);
                    var dyFront = Mathf.Abs(dFront.z);
                    var dxRear = Mathf.Abs(dRear.x);
                    var dyRear = Mathf.Abs(dRear.z);

                    var distance = Mathf.Min(distanceFront, distanceRear);                    
                    var dx = 0f;
                    var dy = 0f;

                    if(dxFront < dxRear){
                        if( Forward == Direction.Up || Forward == Direction.Down ) dx = dxFront;
                        else dy = dxFront;
                    }
                    else{
                        if( Forward == Direction.Up || Forward == Direction.Down ) dx = dxRear;
                        else dy = dxRear;
                    }
                    if(dyFront < dyRear){
                        if( Forward == Direction.Up || Forward == Direction.Down ) dy = dyFront;
                        else dx = dyFront;
                    }
                    else{
                        if( Forward == Direction.Up || Forward == Direction.Down ) dy = dyRear;
                        else dx = dyRear;
                    }

                    var crossCarFront = Vector3.Cross(_transform[(int)TransformPosition.Front].forward, dFront ).y;
                    var crossCarRear = Vector3.Cross( _transform[(int)TransformPosition.Front].forward, dRear ).y;

                    foreach( TransformPosition tpos in Enum.GetValues(typeof(TransformPosition)) ){

                        var carLidarPosX = _position2LidarPositionX[(int)Forward, car.PositionX[(int)tpos],car.PositionY[(int)tpos]];
                        var carLidarPosY = _position2LidarPositionY[(int)Forward, car.PositionX[(int)tpos],car.PositionY[(int)tpos]];

                        // car is on the turning point
                        if( car.MapX == _navigatorData.turnX && car.MapY == _navigatorData.turnY ){ 

                            if( _navigatorData.direction == NavigatorDirection.Left ){

                                if( (carLidarPosX == 1 && carLidarPosY == 0) 
                                 || (carLidarPosX == 1 && carLidarPosY == 1) 
                                ){
                                    if(! stopFowardLeftScan) setSensor(SensorCarPosition.ForwardLeft,tpos,car,distance);
                                }
                            }

                            // NavigatorDirection.Right
                            else{  
                                // ignore the upper-side car turning right
                                if( car.Forward == Back && car.TurningRight )
                                {}
                                // ignore the left-side car turning left
                                else if( car.Forward == Right && car.TurningLeft )
                                {}
                                else if( (carLidarPosX == 1 && carLidarPosY == 0)
                                      || (carLidarPosX == 1 && carLidarPosY == 1)
                                      || (carLidarPosX == 1 && carLidarPosY == 2)
                                ){
                                    if(! stopFowardLeftScan) setSensor(SensorCarPosition.ForwardLeft,tpos,car,distance);
                                }
                                else if( (carLidarPosX == 2 && carLidarPosY == 1)
                                      || (carLidarPosX == 2 && carLidarPosY == 2)
                                ){
                                    if(! stopForwardRightScan) setSensor(SensorCarPosition.ForwardRight,tpos,car,distance);
                                }
                                else if( (carLidarPosX == 2 && carLidarPosY == 3)
                                ){
                                    if(! stopForwardRightScan){
                                        
                                        // ignore the car stopping at a yellow/red signal
                                        if( car.Speed == 0 && car.TargetSpeed == 0 && car._sensorSignal.state != SensorSignalState.Green ){
                                            stopForwardRightScan = true;
                                        }
                                        else setSensor(SensorCarPosition.ForwardRight,tpos,car,distance);
                                    }
                                }
                            }
                        }

                        // car is NOT on the turning point
                        else{ 

                           // left lane
                            if( carLidarPosX == 1 ){ 
                                if(! stopFowardLeftScan) setSensor(SensorCarPosition.ForwardLeft,tpos,car,distance);
                            }

                            // right lane
                            else if( carLidarPosX == 2 ){ 
                                
                                if( _navigatorData.direction == NavigatorDirection.Right
                                    && (   ( Forward == Direction.Up    && car.MapY > _navigatorData.turnY )
                                        || ( Forward == Direction.Left  && car.MapX < _navigatorData.turnX )
                                        || ( Forward == Direction.Down  && car.MapY < _navigatorData.turnY )
                                        || ( Forward == Direction.Right && car.MapX > _navigatorData.turnX ) )
                                ){
                                    // ignore the upper-side car turning right
                                    if( car.Forward == Back && car.TurningRight )
                                    {}    
                                    // ignore the car stopping at a yellow/red signal
                                    if( car.Speed == 0 && car.TargetSpeed == 0 && car._sensorSignal.state != SensorSignalState.Green ){
                                        stopForwardRightScan = true;
                                    }                                        
                                    else if(! stopForwardRightScan) setSensor(SensorCarPosition.ForwardRight,tpos,car,distance);
                                }
                            }
                        }

                    } // foreach
                    if( _sensorCar[(int)SensorCarPosition.ForwardLeft].distance < (int)MapManager.Const.Infty ) stopFowardLeftScan = true;
                    if( _sensorCar[(int)SensorCarPosition.ForwardRight].distance < (int)MapManager.Const.Infty ) stopForwardRightScan = true;
                } // behind
            } // onStreet

            if( stopFowardLeftScan && stopForwardRightScan ) break;
            car = car.CarNext[(int)Forward];
        } // while

        // scan the left side
        if( _navigatorData.direction == NavigatorDirection.Left &&  _sensorCar[(int)SensorCarPosition.ForwardLeft].distance == (int)MapManager.Const.Infty ){
            var spos = SensorCarPosition.Left;
            var sdir = Left;
            var dx = 0;
            var dy = 0;
            if( Forward == Direction.Up ) dx = -1;
            else if( Forward == Direction.Left ) dy = -1;
            else if( Forward == Direction.Down ) dx = 1;
            else dy = 1;

            car = CarNext[(int)sdir];
            while(car is not null){
                var carX = car._transform[(int)TransformPosition.Center].position.x;
                var carY = car._transform[(int)TransformPosition.Center].position.z;
                if( ( car.MapX == _navigatorData.turnX && car.MapY == _navigatorData.turnY ) || (car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy) ){
                    var distance = Vector3.Distance( _transform[(int)TransformPosition.Center].position, car._transform[(int)TransformPosition.Center].position);
                    if( distance > 0 ) foreach( TransformPosition tpos in Enum.GetValues(typeof(TransformPosition)) ){
                        var lidarPosX = _position2LidarPositionX[(int)sdir,car.PositionX[(int)tpos],car.PositionY[(int)tpos]];
                        var lidarPosY = _position2LidarPositionY[(int)sdir,car.PositionX[(int)tpos],car.PositionY[(int)tpos]];
                        if( car.MapX == _navigatorData.turnX && car.MapY == _navigatorData.turnY && lidarPosX == 1 && lidarPosY == 3){
                            setSensor(spos,tpos,car,distance);
                        }
                        else if( car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy && lidarPosX == 1 && lidarPosY == 0) {
                            setSensor(spos,tpos,car,distance);
                        }
                        else if( car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy && lidarPosX == 1 && lidarPosY == 1){
                            setSensor(spos,tpos,car,distance);
                        }
                    }
                }
                if( _sensorCar[(int)spos].distance < (int)MapManager.Const.Infty ) break;
                car = car.CarNext[(int)sdir];
            }
        }

        // scan the right side
        if( _navigatorData.direction == NavigatorDirection.Right ){
            var spos = SensorCarPosition.Right;
            var sdir = Right;
            var dx = 0;
            var dy = 0;
            if( Forward == Direction.Up ) dx = 1;
            else if( Forward == Direction.Left ) dy = 1;
            else if( Forward == Direction.Down ) dx = -1;
            else dy = -1;
            
            car = CarNext[(int)sdir];
            while(car is not null){
                var carX = car._transform[(int)TransformPosition.Center].position.x;
                var carY = car._transform[(int)TransformPosition.Center].position.z;
                if( ( car.MapX == _navigatorData.turnX && car.MapY == _navigatorData.turnY ) || (car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy) ){
                    var distance = Vector3.Distance( _transform[(int)TransformPosition.Center].position, car._transform[(int)TransformPosition.Center].position);
                    if( distance > 0 ) foreach( TransformPosition tpos in Enum.GetValues(typeof(TransformPosition)) ){
                        var lidarPosX = _position2LidarPositionX[(int)sdir,car.PositionX[(int)tpos],car.PositionY[(int)tpos]];
                        var lidarPosY = _position2LidarPositionY[(int)sdir,car.PositionX[(int)tpos],car.PositionY[(int)tpos]];
                        if( car.MapX == _navigatorData.turnX && car.MapY == _navigatorData.turnY && lidarPosX == 1 && lidarPosY == 3){
                            setSensor(spos,tpos,car,distance);
                        }
                        else if( car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy && lidarPosX == 1 && lidarPosY == 0) {
                            setSensor(spos,tpos,car,distance);
                        }
                        else if( car.MapX == _navigatorData.turnX + dx && car.MapY == _navigatorData.turnY + dy && lidarPosX == 1 && lidarPosY == 1){
                            setSensor(spos,tpos,car,distance);
                        }
                    }
                }
                if( _sensorCar[(int)spos].distance < (int)MapManager.Const.Infty ) break;
                car = car.CarNext[(int)sdir];
            }
        }

        if( _sensorCar[(int)SensorCarPosition.Right].car is not null && _sensorCar[(int)SensorCarPosition.Right].car.Equals( _sensorCar[(int)SensorCarPosition.ForwardRight].car) ){
            _ClearSensorCar(SensorCarPosition.Right);
        }

        if( _sensorCar[(int)SensorCarPosition.ForwardLeft].car is not null && _sensorCar[(int)SensorCarPosition.ForwardLeft].car.Equals( _sensorCar[(int)SensorCarPosition.ForwardRight].car) ){
            _ClearSensorCar(SensorCarPosition.ForwardRight);
        }

        _UpdateSensorCarDistance();
    }

    void _RequestScanSensorCar( Direction dir, int x, int y )
    {         
        Func<int,int,int,int,int,int,bool> onStreet = (x,y,startX,startY,endX,endY) =>{
                if( startX > endX ){
                    var tmp = endX;
                    endX = startX;
                    startX = tmp;
                }
                if( startY > endY ){
                    var tmp = endY;
                    endY = startY;
                    startY = tmp;
                }
                if( x < startX || endX < x ) return false;
                if( y < startY || endY < y ) return false;
                return true;
         };

        var oppositeDir = (Direction)(((int)dir+2)%4);
        var endStreetX = _mapManager.EndOfStreetX(x,y,dir);
        var endStreetY = _mapManager.EndOfStreetY(x,y,dir);
             
        var car = CarNext[(int)dir];
        while(car is not null){
            if( car.Forward == oppositeDir && onStreet(car.MapX,car.MapY,x,y,endStreetX,endStreetY) ) car._timerRescanSensorCar = 0f;
            car = car.CarNext[(int)dir];
        }
    }

    void _UpdateSensorCar( int preX, int preY)
    {
        foreach( TransformPosition tpos in Enum.GetValues(typeof(TransformPosition)) ){
            if( PositionY[(int)tpos] != _prePositionY[(int)tpos] ){
                _timerRescanSensorCar = 0f;
                _RequestScanSensorCar(Direction.Left,MapX,MapY);
                _RequestScanSensorCar(Direction.Right,MapX,MapY);
            }
            if( PositionX[(int)tpos] != _prePositionX[(int)tpos] ){
                _timerRescanSensorCar = 0f;
                _RequestScanSensorCar(Direction.Up,MapX,MapY);
                _RequestScanSensorCar(Direction.Down,MapX,MapY);
            }
        }
        if( MapY != preY ){
            _timerRescanSensorCar = 0f;
           _RequestScanSensorCar(Direction.Left,preX,preY);
           _RequestScanSensorCar(Direction.Right,preX,preY);
        }
        if( MapX != preX ){
            _timerRescanSensorCar = 0f;
           _RequestScanSensorCar(Direction.Up,preX,preY);
           _RequestScanSensorCar(Direction.Down,preX,preY);
        }        
        var car = _sensorCar[(int)SensorCarPosition.ForwardRight].car;
        if( car is not null ){
            bool rescan = false;
            var tpos = (int)TransformPosition.Front;
            switch(Forward){
                case Direction.Up:
                    if( _transform[tpos].position.z > car._transform[tpos].position.z ) rescan = true;
                break;
                case Direction.Left:
                    if( _transform[tpos].position.x < car._transform[tpos].position.x ) rescan = true;
                break;
                case Direction.Down:
                    if( _transform[tpos].position.z < car._transform[tpos].position.z ) rescan = true;
                break;
                case Direction.Right:
                    if( _transform[tpos].position.x > car._transform[tpos].position.x ) rescan = true;
                break;
            }
            if(rescan){
                _timerRescanSensorCar = 0f;
                car._timerRescanSensorCar = 0f;
            }
        }
    }

    void _UpdateSensorCarDistance()
    {
        foreach( Car.SensorCarPosition dir in Enum.GetValues(typeof(Car.SensorCarPosition)) ){
            var car = _sensorCar[(int)dir].car;
            if( car is not null ){
                _sensorCar[(int)dir].distance =  (int)Vector3.Distance( _transform[(int)TransformPosition.Front].position, car._transform[(int)TransformPosition.Center].position );
            }
            else _sensorCar[(int)dir].distance = (int)MapManager.Const.Infty;
        }
    }

    void _ClearSensorSignal()
    {
        _sensorSignal.transform = null;
        _sensorSignal.distance = (int)MapManager.Const.Infty;
        _sensorSignal.state = SensorSignalState.Green;
    }

    void _ScanSensorSignal()
    {
        _ClearSensorSignal();

        var distance = _signalManager.DistanceNextSignal(MapX,MapY,Forward);
        if( distance == (int)MapManager.Const.Infty ) return;

        var sb = (int)MapManager.Const.SizeBlock;
        var offset = 0f;
        var tpos = TransformPosition.Front;
        if (Forward == Direction.Up){
            _sensorSignal.transform = _signalManager.ObjSignal(MapX,MapY+distance).GetComponent<Transform>();
            offset = _transform[(int)tpos].position.z - MapY*sb;
        }
        else if (Forward == Direction.Left){
            _sensorSignal.transform = _signalManager.ObjSignal(MapX-distance,MapY).GetComponent<Transform>();
            offset = (MapX+1)*sb - _transform[(int)tpos].transform.position.x;
        }
        else if (Forward == Direction.Down){
            _sensorSignal.transform = _signalManager.ObjSignal(MapX,MapY-distance).GetComponent<Transform>();
            offset = (MapY+1)*sb - _transform[(int)tpos].transform.position.z;
        }
        else{
            _sensorSignal.transform = _signalManager.ObjSignal(MapX+distance,MapY).GetComponent<Transform>();
            offset = _transform[(int)tpos].transform.position.x - MapX*sb;
        }
        _sensorSignal.distance = distance*sb + sb/3 - offset;
    }

    void _UpdateSensorSignal(float dForward )
    {
        if( _sensorSignal.distance < (int)MapManager.Const.Infty ){

            _sensorSignal.distance = Mathf.Max(0f,_sensorSignal.distance - dForward);

            if( Forward == Direction.Up || Forward == Direction.Down ){
                switch(_signalManager.State){
                    case SignalManager.SignalState.GreenV:
                        _sensorSignal.state = SensorSignalState.Green;
                    break;
                    case SignalManager.SignalState.YellowV:
                        _sensorSignal.state = SensorSignalState.Yellow;
                    break;
                   default:
                        _sensorSignal.state = SensorSignalState.Red;
                 break;
                }
            }
            else{
               switch(_signalManager.State){
                   case SignalManager.SignalState.GreenH:
                        _sensorSignal.state = SensorSignalState.Green;
                    break;
                    case SignalManager.SignalState.YellowH:
                        _sensorSignal.state = SensorSignalState.Yellow;
                    break;
                    default:
                        _sensorSignal.state = SensorSignalState.Red;
                    break;
                }
            }
        }
    }

    void _InitPosition( Direction dir )
    {
        _UpdatePosition();
        _UpdateDirection(dir);
        _AdjustTransform();
        if( _targetSlide != 0 ){
            var pos = transform.position;
            switch(Forward){
                case Direction.Up:
                    pos.x += _targetSlide;
                break;
                case Direction.Left:
                    pos.z += _targetSlide;
                break;
                case Direction.Down:
                    pos.x -= _targetSlide;
                break;
                case Direction.Right:
                    pos.z -= _targetSlide;
                break;
            }
            transform.position = pos;
            _targetSlide = 0;
        }
        _UpdatePosition();
        _navigatorData.distance = (int)MapManager.Const.Infty;
        _sensorSignal.distance = (int)MapManager.Const.Infty;
    }

    void _UpdatePosition()
    {
        var x = transform.position.x;
        var y = transform.position.z;
        var sb = (int)MapManager.Const.SizeBlock;
        var wr = (int)MapManager.Const.WidthRoad;
        
        MapX = (int)(x/sb);
        MapY = (int)(y/sb);

        foreach( TransformPosition pos in Enum.GetValues(typeof(TransformPosition)) ){
            _prePositionX[(int)pos] = PositionX[(int)pos];
            _prePositionY[(int)pos] = PositionY[(int)pos];
        }

        var c = MapX* sb + sb / 2;
        foreach( TransformPosition pos in Enum.GetValues(typeof(TransformPosition)) ){
            var tmpX = _transform[(int)pos].position.x;
            if( tmpX < c-wr ) PositionX[(int)pos] = 0;
            else if( tmpX < c ) PositionX[(int)pos] = 1;
            else if( tmpX < c+wr ) PositionX[(int)pos] = 2;
            else PositionX[(int)pos] = 3;
        }
        c = MapY*sb+sb/2;
        foreach( TransformPosition pos in Enum.GetValues(typeof(TransformPosition)) ){
            var tmpY = _transform[(int)pos].position.z;
            if( tmpY < c-wr ) PositionY[(int)pos] = 0;
            else if( tmpY < c ) PositionY[(int)pos] = 1;
            else if( tmpY < c+wr ) PositionY[(int)pos] = 2;
            else PositionY[(int)pos] = 3;
        }
    }

    void _UpdateDirection( Direction dir )
    {
        Forward = dir;
        Left = (Direction) ( ( (int)Forward +1 )%4 );
        Right = (Direction) ( ( (int)Forward +3 )%4 );
        Back = (Direction) ( ( (int)Forward +2 )%4 );
    }

    void _UpdateCarNext()
    {
        var dir = Car.Direction.Up;
        for( var i=0; i<2; ++i){
            var oppositeDir = (Car.Direction)(((int)dir+2)%4);

            var x = transform.position.x;
            var z = transform.position.z;
            Car before = CarNext[(int)oppositeDir];
            var after = CarNext[(int)dir];
            while (after is not null){
                if (dir == Car.Direction.Up  && after.transform.position.z > z) break;
                if (dir == Car.Direction.Right && after.transform.position.x > x) break;

                CarNext[(int)dir] = after.CarNext[(int)dir];
                if( CarNext[(int)dir] is not null ) CarNext[(int)dir].CarNext[(int)oppositeDir] = this;
                else _carManager.HeadCar[(int)oppositeDir] = this;
                after.CarNext[(int)dir] = this;
                after.CarNext[(int)oppositeDir] = before;
                CarNext[(int)oppositeDir] = after;
                if( before is not null ) before.CarNext[(int)dir] = after;
                else _carManager.HeadCar[(int)dir] = after;

                before = CarNext[(int)oppositeDir];
                after = CarNext[(int)dir];
            }
            while (before is not null){
                if (dir == Car.Direction.Up  && before.transform.position.z < z) break;
                if (dir == Car.Direction.Right && before.transform.position.x < x) break;

                CarNext[(int)oppositeDir] = before.CarNext[(int)oppositeDir];
                if( CarNext[(int)oppositeDir] is not null ) CarNext[(int)oppositeDir].CarNext[(int)dir] = this;
                else _carManager.HeadCar[(int)dir] = this;
                before.CarNext[(int)oppositeDir] = this;
                before.CarNext[(int)dir] = after;
                CarNext[(int)dir] = before;
                if( after is not null ) after.CarNext[(int)oppositeDir] = before;
                else _carManager.HeadCar[(int)oppositeDir] = before;

                before = CarNext[(int)oppositeDir];
                after = CarNext[(int)dir];
            }
            dir = Car.Direction.Right;
        }
    }

    public void Initializing()
    {
         Ready = true;
    }

    public void SetInvisible()
    {
        if( ! _invisible && _signalManager.ObjSignal(MapX,MapY) is not null 
        && Speed == 0
        &&  (   ( (Forward == Direction.Up   || Forward == Direction.Down)  && _signalManager.State == SignalManager.SignalState.GreenH)
             || ( (Forward == Direction.Left || Forward == Direction.Right) && _signalManager.State == SignalManager.SignalState.GreenV)
            )
        ){
            var check = false;
            if( _sensorSignal.distance == 0 ) check = true;
            else foreach( TransformPosition pos in Enum.GetValues(typeof(TransformPosition)) ){
                if( (PositionX[(int)pos] == 1 && PositionY[(int)pos] == 1)
                 || (PositionX[(int)pos] == 1 && PositionY[(int)pos] == 2)
                 || (PositionX[(int)pos] == 2 && PositionY[(int)pos] == 1)
                 || (PositionX[(int)pos] == 2 && PositionY[(int)pos] == 2) 
                ){
                    check = true;
                    break;
                }
            }
            if(check){
                _invisible = true;
            }
        }
    }

    public void UnsetInvisible()
    {
        if( _invisible 
        && _listTriggerEnter.Count == 0 && Speed > 0 && Speed >= TargetSpeed/2 
        ){
            var check = false;
            foreach( TransformPosition pos in Enum.GetValues(typeof(TransformPosition)) ){
                if( (PositionX[(int)pos] == 1 && PositionY[(int)pos] == 1)
                 || (PositionX[(int)pos] == 1 && PositionY[(int)pos] == 2)
                 || (PositionX[(int)pos] == 2 && PositionY[(int)pos] == 1)
                 || (PositionX[(int)pos] == 2 && PositionY[(int)pos] == 2) 
                ){
                    check = true;
                    break;
                }
            }
            if(!check){
                _invisible = false;
                _body.SetActive(true);
                _shadow.SetActive(true);
            }
        }
    }
    
    public void MyUpdate()
    {
        _interface.ChangeSpeed();
        _interface.Move();
    }

    public void MyLateUpdate()
    {
        if( Speed == 0 && TargetSpeed == 0 ) _timerRescanSensorCar -= Time.deltaTime;
        if( _timerRescanSensorCar <= 0f ){
            _ScanSensorCar();
            _timerRescanSensorCar = _intervalRescanSensorCar;
        }
        else _UpdateSensorCarDistance();

        if( _hud is not null ) _hud.MyUpdate();
    }

    public void ShowBody()
    {
        if( _invisible ){
            _body.SetActive( !_body.activeSelf );
            _shadow.SetActive( !_shadow.activeSelf );
        }
    }

    public void HideBody()
    {
        if( _invisible ){
            _body.SetActive( !_body.activeSelf );
            _shadow.SetActive( !_shadow.activeSelf );
        }
    }

    void _addListTriggerEnter( Car car )
    {
        if( ! _listTriggerEnter.Contains(car.No) ){
            _listTriggerEnter.Add(car.No);
        }
    }

    void _removeListTriggerEnter( Car car )
    {                
        if( _listTriggerEnter.Contains(car.No) ){
            _listTriggerEnter.Remove(car.No);
        }
    }

    void Awake()
    {
        var m = GameObject.Find("Manager");
        _gameManager = m.GetComponent<GameManager>();
        _signalManager = m.GetComponent<SignalManager>();
        _mapManager = m.GetComponent<MapManager>();
        _carManager =  m.GetComponent<CarManager>();
        Coll = GetComponent<Collider>();
        PositionX = new int[3];
        PositionY = new int[3];
        _prePositionX = new int[3];
        _prePositionY = new int[3];
        _body = transform.GetChild(0).gameObject;
        _shadow = transform.GetChild(3).gameObject;
        _transform = new Transform[3];
        _transform[(int)TransformPosition.Front] = transform.GetChild(1);
        _transform[(int)TransformPosition.Center] = transform;
        _transform[(int)TransformPosition.Rear] = transform.GetChild(2);
        _interface = GetComponent<MyCar>();
        if( _interface is null ) _interface = GetComponent<AICar>();
        _hud = GetComponent<Hud>();
        CarNext = new Car[4];
        _sensorCar = new SensorCarData[7];
        foreach( Car.SensorCarPosition spos in Enum.GetValues(typeof(Car.SensorCarPosition)) ) _ClearSensorCar(spos);
        _route = new List<Direction>();
        _listTriggerEnter = new List<int>();

        ClearRoute();
        _UpdatePosition();
    }

    void OnTriggerEnter( Collider collider )
    {
        var car = collider.GetComponentInParent<Car>();
        if( car is not null ){
            _addListTriggerEnter(car);
            car._addListTriggerEnter(this);

            if( !_invisible &&  !car._invisible ){
                _interface.Hit(car);
            }
        }
    }

    void OnTriggerExit( Collider collider )
    {
        var car = collider.GetComponentInParent<Car>();
        if( car is not null ){
            _removeListTriggerEnter(car);
            car._removeListTriggerEnter(this);
        }
    }
}
