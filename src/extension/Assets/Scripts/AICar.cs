using UnityEngine;
using System;

public class AICar : MonoBehaviour, InterfaceCar
{
    const int _speedMax = 40;  // km/h
    const float _timerStopTime = 5f;

    GameManager _gameManager = null;
    MapManager _mapManager = null;
    CarManager _carManager = null;
    Car _car;
    int _status = 0;
    bool _hit = false;

    void InterfaceCar.CreateRoute()
    {
        var dir = _car.Forward;
        var tmpx = _car.MapX;
        var tmpy = _car.MapY;
        Car.Direction[] next = new Car.Direction[4];

        _car.ClearRoute();

        while( true ){
            var dirNext = dir;
            var n = 0;
            foreach( Car.Direction d in Enum.GetValues(typeof(Car.Direction)) ){
                if( dir != (Car.Direction)( ((int)d+2)%4 ) && _mapManager.DistanceEndOfStreet(tmpx,tmpy,d) >= 1 ) next[n++] = d;
            }
            dirNext = next[UnityEngine.Random.Range(0,n)];

            _car.PushRoute(dirNext);
            if( dir != dirNext ) break;

            if( dir == Car.Direction.Up ) tmpy++;
            if( dir == Car.Direction.Left ) tmpx--;
            if( dir == Car.Direction.Down ) tmpy--;
            if( dir == Car.Direction.Right ) tmpx++;
        }
    }

    int _getSignalState()
    {
        int id = 0;
        var distance = _car.SensorSignal.distance;
        var state = _car.SensorSignal.state;

        var near = 20;
        var before = 10;
        if( near < distance ) id = 0;
        else if( before < distance ){
            if( state == Car.SensorSignalState.Green ) id = 0;
            if( state == Car.SensorSignalState.Yellow ) id = 1;
            if( state == Car.SensorSignalState.Red ) id = 1;
        }
        else if( 0 < distance ){
            if( state == Car.SensorSignalState.Green ) id = 0;
            if( state == Car.SensorSignalState.Yellow ) id = 2;
            if( state == Car.SensorSignalState.Red ) id = 2;
        }
        else id = 3;

        return id;
    }

    int _getNaviState()
    {
        int id = 0;
        var direction = _car.Navigator.direction;
        var distance = _car.Navigator.distance;

        var near = 20;
        if( near < distance ) id = 0;
        else if( 0 < distance ){
            if( direction == Car.NavigatorDirection.Left ) id = 1;
            else id = 2;
        }
        else{
            if( direction == Car.NavigatorDirection.Left ) id = 3;
            else id = 4;
        }

        return id;
    }

    int _getSensorCarLState()
    {
        int id = 0;
        var distance = _car.SensorCar[(int)Car.SensorCarPosition.Left].distance;

        var near = 15;
        var before = 10;
        if( near < distance ) id = 0;
        else if( before < distance ) id = 1;
        else id = 2;

        return id;
    }

    int _getSensorCarFLState()
    {
        int id = 0;
        var distance = _car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].distance;

        var near = 15;
        var before = 10;
        if( near < distance ) id = 0;
        else if( before < distance ) id = 1;
        else id = 2;

        return id;
    }

    int _getSensorFRState(){
        int id = 0;
        var distance = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].distance;
        var near = 35;
        var before = 30;
        if( near < distance ) id = 0;
        else if( before < distance ) id = 1;
        else id = 2;

        return id;
    }

    int _getSensorRState()
    {
        int id = 0;
        var distance = _car.SensorCar[(int)Car.SensorCarPosition.Right].distance;

        var near = 15;
        var before = 10;
        if( near < distance ) id = 0;
        else if( before < distance ) id = 1;
        else id = 2;

        return id;
    }

    void InterfaceCar.ChangeSpeed()
    {
        _status = 0;
        var minSpeed = 40;

        var idSignal = _getSignalState();
        var idNavi = _getNaviState();
        var idCarL = _getSensorCarLState();
        var idCarFL = _getSensorCarFLState();
        var idCarFR = _getSensorFRState();
        var idCarR = _getSensorRState();

        var maxSpeed = (int)MapManager.Const.Infty;
        if( idSignal == 0 ){ // go
            maxSpeed = 40;
        } 
        else if( idSignal == 1 ){ // warn
            maxSpeed = 20;
        }
        else if( idSignal == 2 ){ // stop
            maxSpeed = 0;

            _status = 1;
        }
        else if( idSignal == 3 ){ // under signal
            maxSpeed = 40;
        }
        minSpeed = Mathf.Min( minSpeed, maxSpeed );

        maxSpeed = (int)MapManager.Const.Infty;
        if( idNavi == 0 ){ // straight
            maxSpeed = 40;
        }
        else if( idNavi == 1 ){  // near left turn
            maxSpeed = 20;
        }
        else if( idNavi == 2 ){  // near right turn
            maxSpeed = 20;
        }
        else if( idNavi == 3 ){ // left turn
            maxSpeed = 20;
        }
        else if( idNavi == 4 ){ // right turn
            maxSpeed = 20;
        }
        minSpeed = Mathf.Min( minSpeed, maxSpeed );

        // go straight
        maxSpeed = (int)MapManager.Const.Infty;
        if( idCarFL == 0 ) maxSpeed = 40;
        else if( idCarFL == 1 ) maxSpeed = 20;
        else if( idCarFL == 2 ) {
            maxSpeed = 0;
            
            _status = 2;
            var no = _car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].car.No;
            var d = _car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].distance;
            var sp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].car.Speed/1000);
            var tsp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft].car.TargetSpeed/1000);
        }
        minSpeed = Mathf.Min( minSpeed, maxSpeed );

        // turn left
        maxSpeed = (int)MapManager.Const.Infty;
        if( idNavi == 1 || idNavi == 3 ){

            if( idCarL == 0 ) maxSpeed = 40;
            if( idCarL == 1 ) maxSpeed = 20;
            if( idCarL == 2 ){
                maxSpeed = 0;

                _status = 3;
                var no = _car.SensorCar[(int)Car.SensorCarPosition.Left].car.No;
                var d = _car.SensorCar[(int)Car.SensorCarPosition.Left].distance;
                var sp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.Left].car.Speed/1000);
                var tsp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.Left].car.TargetSpeed/1000);
            } 
        }
        minSpeed = Mathf.Min( minSpeed, maxSpeed );

        // turn right
        maxSpeed = (int)MapManager.Const.Infty;
        if(  idNavi == 2 ){
            if( idCarFR == 0 && idCarR == 0 ) maxSpeed = 30;
            else if( idCarFR == 1 ) maxSpeed = 20;
            else if( idCarR == 1 ) maxSpeed = 20;
        }    
        else if( idNavi == 4 ){
            if     ( idCarFR == 0 && idCarR == 0 ) maxSpeed = 30;
            else if( idCarFR == 0 && idCarR == 1 ) maxSpeed = 20;
            else if( idCarFR == 1 && idCarR == 0 ) maxSpeed = 20;
            else if( idCarFR == 1 && idCarR == 1 ) maxSpeed = 20;
            else if( idCarFR == 2  ){
                maxSpeed = 0;

                _status = 4;
                var no = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].car.No;
                var d = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].distance;
                var sp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].car.Speed/1000);
                var tsp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].car.TargetSpeed/1000);
                var posx = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].car.PositionX[1];
                var posy = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight].car.PositionY[1];
            }
            else if( idCarR == 2 ){
                maxSpeed = 0;

                _status = 5;
                var no = _car.SensorCar[(int)Car.SensorCarPosition.Right].car.No;
                var d = _car.SensorCar[(int)Car.SensorCarPosition.Right].distance;
                var sp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.Right].car.Speed/1000);
                var tsp = (int)(_car.SensorCar[(int)Car.SensorCarPosition.Right].car.TargetSpeed/1000);
                var posx = _car.SensorCar[(int)Car.SensorCarPosition.Right].car.PositionX[1];
                var posy = _car.SensorCar[(int)Car.SensorCarPosition.Right].car.PositionY[1];
            } 
        }
        minSpeed = Mathf.Min( minSpeed, maxSpeed );

        _car.TargetSpeed = minSpeed*1000;
    }

    void InterfaceCar.Move()
    {
        if( !_hit) _car.Move();
    }

    void InterfaceCar.Hit(Car car)
    {
        Debug.Log( $"AICar.OnCollisionEnter: No: {_car.No}, status:{_status}, X:{_car.MapX}, Y:{_car.MapY}, fw:{_car.Forward}, speed:{_car.Speed}, target:{_car.TargetSpeed}, posX:{_car.PositionX[0]}, posY:{_car.PositionY[0]}");
        _car.ShowNavigatorData();
        var ld = _car.SensorCar[(int)Car.SensorCarPosition.Left];
        if( ld.car is not null ) {
            Debug.Log($"LeftDown: No: {ld.car.No}, X:{ld.car.MapX}, Y:{ld.car.MapY}, fw:{ld.car.Forward}, speed:{ld.car.Speed}, target:{ld.car.TargetSpeed}, posX:{ld.car.PositionX[0]}, posY:{ld.car.PositionY[0]}, d:{ld.distance}");
            ld.car.ShowNavigatorData();
        }

        ld = _car.SensorCar[(int)Car.SensorCarPosition.ForwardLeft];
        if( ld.car is not null ) {
            Debug.Log($"ForwardLeft: No: {ld.car.No}, X:{ld.car.MapX}, Y:{ld.car.MapY}, fw:{ld.car.Forward}, speed:{ld.car.Speed}, target:{ld.car.TargetSpeed}, posX:{ld.car.PositionX[0]}, posY:{ld.car.PositionY[0]}, d:{ld.distance}");
            ld.car.ShowNavigatorData();
        }

        ld = _car.SensorCar[(int)Car.SensorCarPosition.ForwardRight];
        if( ld.car is not null ) {
            Debug.Log($"ForwardRight: No: {ld.car.No}, X:{ld.car.MapX}, Y:{ld.car.MapY}, fw:{ld.car.Forward}, speed:{ld.car.Speed}, target:{ld.car.TargetSpeed}, posX:{ld.car.PositionX[0]}, posY:{ld.car.PositionY[0]}, d:{ld.distance}");
            ld.car.ShowNavigatorData();
        }

        ld = _car.SensorCar[(int)Car.SensorCarPosition.Right];
        if( ld.car is not null ) {
            Debug.Log($"RightUp: No: {ld.car.No}, X:{ld.car.MapX}, Y:{ld.car.MapY}, fw:{ld.car.Forward}, speed:{ld.car.Speed}, target:{ld.car.TargetSpeed}, posX:{ld.car.PositionX[0]}, posY:{ld.car.PositionY[0]}, d:{ld.distance}");
            ld.car.ShowNavigatorData();
        }

        //_hit = true;
    }

    void Awake()
    {
        var m = GameObject.Find("Manager");
        _gameManager = m.GetComponent<GameManager>();
        _mapManager = m.GetComponent<MapManager>();
        _carManager =  m.GetComponent<CarManager>();
        _car = GetComponent<Car>();
    }
}
