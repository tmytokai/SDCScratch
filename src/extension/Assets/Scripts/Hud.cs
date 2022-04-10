using UnityEngine;
using UnityEngine.UI;
using System;

public class Hud : MonoBehaviour
{
    GameManager _gameManager;
    MapManager _mapManager = null;
    SignalManager _signalManager;
    Car _car;
    Transform _transform;
    Camera _cameraMain;

    GameObject _UiTime;
    Text _UiTimeText;

    GameObject _UiSpeed;
    Text _UiSpeedText;

    GameObject[] _UiCar;
    RectTransform[] _UiCarImageRectTransform;

    GameObject _UiSignal;
    RectTransform _UiSignalRectTransform;
    Image _UiSignalImage;
    RectTransform _UiSignalImageRectTransform;

    GameObject _UiMap;
    GameObject _UiMapBody;
    RectTransform _UiMapBodyRectTransform;
    RawImage _UiMapBodyImage;
    RectTransform _UiMapBodyImageRectTransform;
    RectTransform _UiMapBodyNRectTransform;

    GameObject _UiPointMyCar;

    GameObject _UiNavigator;
    GameObject _UiNavigatorLeft;
    GameObject _UiNavigatorRight;
    Text _UiNavigatorDistanceText;

    float _time = 0f;

    public void PrepareRun()
    {
        _time = 0f;

        _UiTime.SetActive(true);
        _UiSpeed.SetActive(true);
        _UiMap.SetActive(true);
        _UiPointMyCar.SetActive(true);
    }

    void _DrawSensorCar(Car.SensorCarPosition spos)
    {
        var carDistance =_car.SensorCar[(int)spos].distance;
        if( carDistance < (int)MapManager.Const.Infty && _gameManager.ViewMode <= 2 ){

                var zoom = 1.2f;

                var wpC = _car.SensorCar[(int)spos].car.Coll.bounds.center;
                var spC = _cameraMain.WorldToScreenPoint(wpC);

                var spMaxX = -100000f;
                var spMinX =  100000f;
                var spMaxY = -100000f;
                var spMinY =  100000f;

                var wpExt = _car.SensorCar[(int)spos].car.Coll.bounds.extents;
                float[] extX = {wpExt.x,-wpExt.x};
                float[] extY = {wpExt.y,-wpExt.y};
                float[] extZ = {wpExt.z,-wpExt.z};
                var wpTmp = Vector3.zero;
                for(var i=0;i<2;++i){
                    for(var j=0;j<2;++j){
                        for(var k=0;k<2;++k){
                            wpTmp.x = wpC.x + extX[i];
                            wpTmp.y = wpC.y + extY[j];
                            wpTmp.z = wpC.z + extZ[k];
                            var spTmp = _cameraMain.WorldToScreenPoint(wpTmp);
                            if(spTmp.x > spMaxX ) spMaxX = spTmp.x;
                            if(spTmp.x < spMinX ) spMinX = spTmp.x;
                            if(spTmp.y > spMaxY ) spMaxY = spTmp.y;
                            if(spTmp.y < spMinY ) spMinY = spTmp.y;
                        }
                    }
                }
                spC.x = (spMaxX + spMinX)/2;
                spC.y = (spMaxY + spMinY)/2;
                var size = (spMaxY - spMinY)*zoom*(int)GameManager.Const.HeightCanvas/_cameraMain.pixelHeight;
                _UiCarImageRectTransform[(int)spos].position = spC;

                if( spC.z > 0 
                &&_UiCarImageRectTransform[(int)spos].anchoredPosition3D.x >= -(int)GameManager.Const.WidthCanvas/2 
                && _UiCarImageRectTransform[(int)spos].anchoredPosition3D.x <= (int)GameManager.Const.WidthCanvas/2
                && _UiCarImageRectTransform[(int)spos].anchoredPosition3D.y >= -(int)GameManager.Const.HeightCanvas/2
                && _UiCarImageRectTransform[(int)spos].anchoredPosition3D.y <= (int)GameManager.Const.HeightCanvas/2
                ){
                     _UiCar[(int)spos].SetActive(true);
                    _UiCarImageRectTransform[(int)spos].sizeDelta = new Vector2(size, size);
                }
                else{
                    _UiCar[(int)spos].SetActive(false);
                }
            }
            else{
                _UiCar[(int)spos].SetActive(false);
            }
    }

    void _DrawSensorSignal()
    {
        var signalDistance = (int)_car.SensorSignal.distance;
        if( signalDistance < (int)MapManager.Const.Infty && _gameManager.ViewMode <= 2){

            var dx = (int)MapManager.Const.WidthRoad/2;
            var dz = 7.5f;
            var y = 4.75f;
            var height = 2f;

            var wpC = _car.SensorSignal.transform.position;
            wpC.y = y;
            switch(_car.Forward){
                case Car.Direction.Up:
                    wpC.x -= dx;
                    wpC.z += dz;
                break;
                case Car.Direction.Left:
                    wpC.x -= dz;
                    wpC.z -= dx;
                break;            
                case Car.Direction.Down:
                    wpC.x += dx;
                    wpC.z -= dz;
                break;
                case Car.Direction.Right:
                    wpC.x += dz;
                    wpC.z += dx;
                break;
            }

            var wpUp = wpC;
            var wpDown = wpC;
            wpUp.y = y + height/2;
            wpDown.y = y - height/2;

            var spC = _cameraMain.WorldToScreenPoint(wpC);
            var spUp = _cameraMain.WorldToScreenPoint(wpUp);
            var spDown = _cameraMain.WorldToScreenPoint(wpDown);
            var size = (spUp.y - spDown.y)*(int)GameManager.Const.HeightCanvas/_cameraMain.pixelHeight;

            _UiSignalRectTransform.position = spC;

            if(  spC.z > 0 
            && _UiSignalRectTransform.anchoredPosition3D.x >= -(int)GameManager.Const.WidthCanvas/2 
            && _UiSignalRectTransform.anchoredPosition3D.x <= (int)GameManager.Const.WidthCanvas/2
            && _UiSignalRectTransform.anchoredPosition3D.y >= -(int)GameManager.Const.HeightCanvas/2
            && _UiSignalRectTransform.anchoredPosition3D.y <= (int)GameManager.Const.HeightCanvas/2
            ){
                _UiSignal.SetActive(true);           
                _UiSignalImageRectTransform.sizeDelta = new Vector2(size, size);
    
                switch(_car.SensorSignal.state){
                    case Car.SensorSignalState.Green:
                        _UiSignalImage.color = Color.green;
                    break;
                   case Car.SensorSignalState.Yellow:
                    _UiSignalImage.color = Color.yellow;
                    break;
                    default:
                        _UiSignalImage.color = Color.red;
                    break;
                }
            }
            else{
                _UiSignal.SetActive(false);
            }
        }
        else{
            _UiSignal.SetActive(false);
        } 
    }

    void DrawNavigator()
    {
        if( _UiMapBodyImage.texture == null ){
            var sb = (int)MapManager.Const.SizeBlockImage;
            _UiMapBodyImageRectTransform.sizeDelta = new Vector2( _mapManager.Width*sb, _mapManager.Height*sb );
            _UiMapBodyImage.texture = _mapManager.Image;
        }
        _UiMapBodyRectTransform.eulerAngles = new Vector3( 0, 0, _transform.root.eulerAngles.y );
        var r = (float)MapManager.Const.SizeBlockImage/(int)MapManager.Const.SizeBlock;
        _UiMapBodyImageRectTransform.localPosition = new Vector2(-_transform.position.x*r, -_transform.position.z*r);
        _UiMapBodyNRectTransform.eulerAngles = Vector3.zero;

        if( _car.Navigator.distance < (int)MapManager.Const.Infty ){
            _UiNavigator.SetActive(true);
            if( _car.Navigator.distance > 0 ) _UiNavigatorDistanceText.text = $"{_car.Navigator.distance:000}m";
            else _UiNavigatorDistanceText.text = "";

            if( _car.Navigator.direction == Car.NavigatorDirection.Left ){
                _UiNavigatorLeft.SetActive(true);
                _UiNavigatorRight.SetActive(false); 
            }
            else if( _car.Navigator.direction == Car.NavigatorDirection.Right ){
                _UiNavigatorLeft.SetActive(false);
                _UiNavigatorRight.SetActive(true); 
            }
        }
        else{
            _UiNavigator.SetActive(false);
        }
    }

    public void MyUpdate()
    {
        _time += Time.deltaTime;
        _UiTimeText.text = $"{(int)(_time):0000}";
        _UiSpeedText.text = $"{(int)(_car.Speed/1000):000}";
        foreach( Car.SensorCarPosition spos in Enum.GetValues(typeof(Car.SensorCarPosition)) ) _DrawSensorCar(spos);
        _DrawSensorSignal();
        DrawNavigator();
    }
   
    void Awake()
    {        
        _cameraMain = Camera.main;

        var m = GameObject.Find("Manager");
        _gameManager = m.GetComponent<GameManager>();
        _mapManager = m.GetComponent<MapManager>();
        _signalManager = m.GetComponent<SignalManager>();
        _car = GetComponent<Car>();
        _transform = GetComponent<Transform>();

        var cv = GameObject.Find("CanvasHud").transform;

        _UiTime = cv.Find("Time").gameObject;
        _UiTimeText = _UiTime.GetComponent<Text>();

        _UiSpeed = cv.Find("Speed").gameObject;
        _UiSpeedText = _UiSpeed.transform.GetChild(0).gameObject.GetComponent<Text>();

        _UiCar = new GameObject[4];
        _UiCarImageRectTransform = new RectTransform[4];
        _UiCar[(int)Car.SensorCarPosition.Left] = cv.Find("CarLeft").gameObject;
        _UiCar[(int)Car.SensorCarPosition.ForwardLeft] = cv.Find("CarForwardLeft").gameObject;
        _UiCar[(int)Car.SensorCarPosition.ForwardRight] = cv.Find("CarForwardRight").gameObject;
        _UiCar[(int)Car.SensorCarPosition.Right] = cv.Find("CarRight").gameObject;
        for(var i=0; i<4; ++i ){
            _UiCarImageRectTransform[i] = _UiCar[i].transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        }

        _UiSignal = cv.Find("Signal").gameObject;
        _UiSignalRectTransform = _UiSignal.GetComponent<RectTransform>();
        _UiSignalImage = _UiSignal.transform.GetChild(0).gameObject.GetComponent<Image>();
        _UiSignalImageRectTransform = _UiSignal.transform.GetChild(0).gameObject.GetComponent<RectTransform>();

        _UiMap = cv.Find("Map").gameObject;
        _UiMapBody = _UiMap.transform.GetChild(1).gameObject;
        _UiMapBodyRectTransform = _UiMapBody.gameObject.GetComponent<RectTransform>();
        _UiMapBodyImage = _UiMapBody.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        _UiMapBodyImageRectTransform = _UiMapBody.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        _UiMapBodyNRectTransform = _UiMapBody.transform.GetChild(1).gameObject.GetComponent<RectTransform>();

        _UiPointMyCar = cv.Find("PointMyCar").gameObject;

        _UiNavigator = cv.Find("Navigator").gameObject;
        _UiNavigatorLeft = _UiNavigator.transform.GetChild(0).gameObject;
        _UiNavigatorRight = _UiNavigator.transform.GetChild(1).gameObject;
        _UiNavigatorDistanceText = _UiNavigator.transform.GetChild(2).gameObject.GetComponent<Text>();
    }
}
