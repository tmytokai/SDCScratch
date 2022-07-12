using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class MapManager : MonoBehaviour
{
    [SerializeField] GameObject[] _prefabRoadParts;
    [SerializeField] GameObject[] _prefabBuildings;
    [SerializeField] GameObject _prefabStart;
    [SerializeField] GameObject _prefabGoal;

    readonly Color[] _colorRoute = new Color[]{
        Color.white,
        Color.clear,
        Color.yellow,
        Color.green,
        Color.blue,
        Color.red,
        Color.red,
    };

    public enum Const
    {
        SizeBlock = 30,   // meters
        SizeBuilding = 20, // meters
        WidthRoad = 4,  // meters
        WidthBand = 1,  // meters
        WidthCrossWalk = 4, // meters 

        SizeBlockImage = 16,  // pixels
        SizeRouteMap = 340, // pixels

        WidthCanvas = 480,
        HeightCanvas = 360,

        Infty = 10000,
    }

    enum _Reward
    {
        OK = -1,
        Failed = -100,
        Goal = 100,
        Init = 0,
    }

    public enum Type
    {
        ULRD,

        ULR,
        ULD,
        URD,
        LRD,

        UL,
        UR,
        UD,
        LR,
        LD,
        RD,

        Ground
    }

   enum _RoadParts
   {
        Cross,
        Curve,
        Straight,
        T,
        Ground,
    }

    enum _RouteColor
    {
        Road,
        Ground,
        Route,
        Cursor,
        Start,
        Goal,
        Error,
    }

    struct _MapData
    {
        public int raw;
        public Type type;
        public int[] distanceEndOfStreet;
    }

    public bool Ready {get; private set;} = false;
    public int Width{get;private set;} = 0;
    public int Height{get;private set;} = 0;

    public int StartX{get;private set;} = 0;
    public int StartY{get;private set;} = 0;
    public int GoalX{get;private set;} = 0;
    public int GoalY{get;private set;}  = 0;

    public Type GetType( int x, int y) => _mapData[x,y].type;
    public bool OnRoad(int x, int y ) => (_mapData[x,y].raw == 0);

    public List<Car.Direction> Route {get;private set;} = null;

    public bool isRouteMapShown => _CanvasRoute.activeSelf;

    public Texture2D Image {get;private set;} = null;

    GameManager _gameManager = null;
    bool _init = false;
    
    GameObject _CanvasRoute;
    GameObject _UiRouteMapBody;
    CanvasRenderer _UiRouteMapBodyRenderer;
    RectTransform _UiRouteMapBodyRectTransform;
    RectTransform _UiRouteMapStartRectTransform;
    RectTransform _UiRouteMapGoalRectTransform;

    _MapData[,] _mapData = null;
    
    int[,] _mapRoute;
    Mesh _meshRoute = null;
    bool _renderRoute = false;
    int _idxRouteLoop = 0;

    int _routeX = 0;
    int _routeY = 0;
    bool _readyRoute = false;
    bool _retryRoute = false;

    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] static extern void JsSetMapWH(int mapWidth, int mapHeight);
    [DllImport("__Internal")] static extern void JsSetStartXY(int x, int y);
    [DllImport("__Internal")] static extern void JsSetGoalXY(int x, int y);
    [DllImport("__Internal")] static extern void JsSetXY(int x, int y);
    [DllImport("__Internal")] static extern void JsSetReward(int reward);
    #else
    void JsSetMapWH(int mapWidth, int mapHeight){}
    void JsSetStartXY(int x, int y){}
    void JsSetGoalXY(int x, int y){}
    void JsSetXY(int x, int y){}
    void JsSetReward(int reward){}
    #endif

    public void Init()
    {
        var rawData = new int[,]{
            {1,1,1,1,1,1,1,1,1},
            {1,0,0,0,1,0,0,0,1},
            {1,0,1,0,1,0,1,0,1},
            {1,0,0,0,0,0,1,0,1},
            {1,0,1,0,1,1,1,0,1},
            {1,0,0,0,0,0,1,0,1},
            {1,0,1,0,1,0,1,0,1},
            {1,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1},
        };
        var startX = 1;
        var startY = 1;
        var goalX = 6;
        var goalY = 7;

        // Ready is set to true in Update()
        Ready = false;
        _init = true;

        _CreateMap(rawData,startX,startY,goalX,goalY);
        _CreateMeshRoute();        
        _mapRoute = new int[Width,Height];
        Route = new List<Car.Direction>();
        _ClearRoute();
    }

    public void PrepareRun()
    {
        // Ready is set to true in Update()
        Ready = false;
        _init = true;

        _CreateImage();
        JsSetXY(StartX,StartY);
    }

    public int DistanceEndOfStreet( int x, int y, Car.Direction dir )
    {
        // cash
        if( _mapData[x,y].distanceEndOfStreet[(int)dir] == -1 ){
            var distanceCurve = 0;
            var tmpX = x;
            var tmpY = y;
            while( OnRoad(tmpX,tmpY) && tmpX >= 0 && tmpX < Width && tmpY >=0 && tmpY < Height ){
                ++distanceCurve;
                if(dir==Car.Direction.Up) ++tmpY;
                if(dir==Car.Direction.Left) --tmpX;
                if(dir==Car.Direction.Down) --tmpY;
                if(dir==Car.Direction.Right) ++tmpX;  
            }
            if( OnRoad(tmpX,tmpY) ) distanceCurve = (int)Const.Infty;
            else distanceCurve--;
            _mapData[x,y].distanceEndOfStreet[(int)dir] = distanceCurve;
        }

        return _mapData[x,y].distanceEndOfStreet[(int)dir];
    }

    public int EndOfStreetX(int x, int y, Car.Direction direction)
    {
        var ret = x;
        var distance = DistanceEndOfStreet(x,y,direction);
        switch(direction){
            case Car.Direction.Left:
                ret = x-distance;
            break;
            case Car.Direction.Right:
                ret = x+distance;
            break;
        }
        return ret;
    }

    public int EndOfStreetY(int x, int y, Car.Direction direction)
    {
        var ret = y;
        var distance = DistanceEndOfStreet(x,y,direction);
        switch(direction){
            case Car.Direction.Up:
                ret = y+distance;
            break;
            case Car.Direction.Down:
                ret = y-distance;
            break;
        }
        return ret;
    }

    void _CreateMap( int[,] rawData, int startX, int startY, int goalX, int goalY )
    {
        Width = rawData.GetLength(1);
        Height = rawData.GetLength(0);
        _mapData = new _MapData[Width,Height];

        Action<float,float,float> createBuilding = (x,z,rot) =>{
            var idx = UnityEngine.Random.Range( 0, _prefabBuildings.Length );
            var v3 = new Vector3(x,0,z);
            Instantiate( _prefabBuildings[idx], v3, Quaternion.Euler(0,rot,0) );
        };

        StartX = startX;
        StartY = startY;
        GoalX = goalX;
        GoalY = goalY;

        JsSetMapWH(Width,Height);
        JsSetStartXY(StartX,StartY);
        JsSetGoalXY(GoalX,GoalY);

        var sb = (int)Const.SizeBlock;
        Instantiate( _prefabStart, new Vector3( StartX*sb+sb/2,0.005f,StartY*sb+sb/2), Quaternion.Euler(90,0,0) );
        Instantiate( _prefabGoal, new Vector3( GoalX*sb+sb/2,0.005f,GoalY*sb+sb/2), Quaternion.Euler(90,0,0) );

        var mapRaw = new int[Width,Height];
        for( var y=0; y<Height; ++y){
            for( var x=0; x<Width; ++x ){
                var my = Height-y-1;
                _mapData[x,my] = new _MapData();
                _mapData[x,my].distanceEndOfStreet = new int[]{-1,-1,-1,-1};
                var rd = rawData[y,x];
                mapRaw[x,my] =rd;
            }
        }

        for( var y=0; y<Height; ++y){
            for( var x=0; x<Width; ++x ){
                var raw = mapRaw[x,y];
                _mapData[x,y].raw = raw;
                if( raw == 0 ){
                    var sum = mapRaw[x-1,y] +mapRaw[x+1,y] + mapRaw[x,y-1] + mapRaw[x,y+1];
                    if( sum == 0 ){
                        _mapData[x,y].type = Type.ULRD;
                    }
                    if( sum == 1){
                        if( mapRaw[x,y-1] == 1 ) _mapData[x,y].type = Type.ULR;
                        else if( mapRaw[x+1,y] == 1 ) _mapData[x,y].type = Type.ULD;
                        else if( mapRaw[x-1,y] == 1 ) _mapData[x,y].type = Type.URD;
                        else _mapData[x,y].type = Type.LRD;
                    }
                    if( sum == 2){
                        if( mapRaw[x,y+1] == 0 && mapRaw[x-1,y] == 0 ) _mapData[x,y].type = Type.UL;
                        else if( mapRaw[x,y+1] == 0 && mapRaw[x+1,y] == 0 ) _mapData[x,y].type = Type.UR;
                        else if( mapRaw[x,y-1] == 0 && mapRaw[x,y+1] == 0 ) _mapData[x,y].type = Type.UD;
                        else if( mapRaw[x-1,y] == 0 && mapRaw[x+1,y] == 0 ) _mapData[x,y] .type= Type.LR;
                        else if( mapRaw[x-1,y] == 0 && mapRaw[x,y-1] == 0) _mapData[x,y].type = Type.LD;
                        else _mapData[x,y].type = Type.RD;
                    }
                }
                else if( raw == 1 ){
                    _mapData[x,y].type = Type.Ground;
                }
            }
        }

        for( var y=0; y<Height; ++y){
            for( var x=0; x<Width; ++x ){
                var type = _RoadParts.Ground;
                var rot = 0f;
                var sbld = (int)Const.SizeBuilding;
                var screenX = x*(int)Const.SizeBlock + (int)Const.SizeBlock/2;
                var screenZ = y*(int)Const.SizeBlock + (int)Const.SizeBlock/2;
                switch (_mapData[x, y].type){
                    case Type.ULRD:
                        type = _RoadParts.Cross;
                        createBuilding(screenX - sbld, screenZ + sbld, 90);
                        createBuilding(screenX + sbld, screenZ + sbld, 90);
                        createBuilding(screenX - sbld, screenZ - sbld, -90);
                        createBuilding(screenX + sbld, screenZ - sbld, -90);
                        break;

                    case Type.ULR:
                        type = _RoadParts.T;
                        rot = -90f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ - sbld, -90);
                        createBuilding(screenX - sbld, screenZ + sbld, 90);
                        createBuilding(screenX + sbld, screenZ + sbld, 90);
                        break;

                    case Type.ULD:
                        type = _RoadParts.T;
                        rot = 180f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + sbld, screenZ + i * sbld, 180);
                        createBuilding(screenX - sbld, screenZ + sbld, 0);
                        createBuilding(screenX - sbld, screenZ - sbld, 0);
                        break;

                    case Type.URD:
                        type = _RoadParts.T;
                        rot = 0f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX - sbld, screenZ + i * sbld, 0);
                        createBuilding(screenX + sbld, screenZ + sbld, 180);
                        createBuilding(screenX + sbld, screenZ - sbld, 180);
                        break;

                    case Type.LRD:
                        type = _RoadParts.T;
                        rot = 90f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ + sbld, 90);
                        createBuilding(screenX - sbld, screenZ - sbld, -90);
                        createBuilding(screenX + sbld, screenZ - sbld, -90);
                        break;

                    case Type.UL:
                        type = _RoadParts.Curve;
                        rot = 180f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ - sbld, -90);
                        for (var i = 0; i < 2; ++i) createBuilding(screenX + sbld, screenZ + i * sbld, 180);
                        createBuilding(screenX - sbld, screenZ + sbld, 0);
                        break;

                    case Type.UR:
                        type = _RoadParts.Curve;
                        rot = -90f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ - sbld, -90);
                        for (var i = 0; i < 2; ++i) createBuilding(screenX - sbld, screenZ + i * sbld, 0);
                        createBuilding(screenX + sbld, screenZ + sbld, 180);
                        break;

                    case Type.UD:
                        type = _RoadParts.Straight;
                        rot = 0f;
                        if (x % 2 == 1 && y % 2 == 1){
                            for (var i = -1; i < 2; ++i) createBuilding(screenX - sbld, screenZ + i * sbld, 0);
                            for (var i = -1; i < 2; ++i) createBuilding(screenX + sbld, screenZ + i * sbld, 180);
                        }
                        break;

                    case Type.LR:
                        type = _RoadParts.Straight;
                        rot = 90f;
                        if (x % 2 == 1 && y % 2 == 1){
                            for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ + sbld, 90);
                            for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ - sbld, -90);
                        }
                        break;

                    case Type.LD:
                        type = _RoadParts.Curve;
                        rot = 90f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ + sbld, 90);
                        for (var i = -1; i < 1; ++i) createBuilding(screenX + sbld, screenZ + i * sbld, 180);
                        createBuilding(screenX - sbld, screenZ - sbld, 0);
                        break;

                    case Type.RD:
                        type = _RoadParts.Curve;
                        rot = 0f;
                        for (var i = -1; i < 2; ++i) createBuilding(screenX + i * sbld, screenZ + sbld, 90);
                        for (var i = -1; i < 1; ++i) createBuilding(screenX - sbld, screenZ + i * sbld, 0);
                        createBuilding(screenX + sbld, screenZ - sbld, 180);
                        break;
                }
                Instantiate(_prefabRoadParts[(int)type], new Vector3(screenX, 0, screenZ), Quaternion.Euler(0, rot, 0));
            }
        }
    }

    void _CreateMeshRoute()
    {
        Action<Vector3[],int,int,int,int> setVertices = (vertices,idx,x,y,sb) => {
            var i = idx*4;
            var vx = x*sb-sb*Width/2;
            var vy = y*sb-sb*Height/2;
            vertices[i++].Set(vx,vy,0);
            vertices[i++].Set(vx,vy+sb,0);
            vertices[i++].Set(vx+sb,vy+sb,0);
            vertices[i++].Set(vx+sb,vy,0);
        };

        Action<int[],int> setTriangle = (triangles,idx) => {
            var i = idx*6;
            var vi = idx*4;
            triangles[i++] = vi+0;
            triangles[i++] = vi+1;
            triangles[i++] = vi+2;
            triangles[i++] = vi+0;
            triangles[i++] = vi+2;
            triangles[i++] = vi+3;
        };

        var vertices = new Vector3[4*Width*Height];
        var triangles = new int[6*Width*Height];
        var colors = new Color[4*Width*Height];

        var sb = (int)Const.SizeRouteMap/Mathf.Max(Width,Height);
        var idx = 0;
        for(var y=0; y < Height; ++y ){
            for(var x=0; x < Width; ++x ){
                setVertices(vertices,idx,x,y,sb);
                setTriangle(triangles,idx);
                ++idx;
            }
        }

        _meshRoute = new Mesh();
        _meshRoute.vertices = vertices;
        _meshRoute.colors = colors;
        _meshRoute.triangles = triangles;

        var sw = Width*sb;
        var sh = Height*sb;
        var sx = StartX*sb+sb/2-sw/2;
        var sy = StartY*sb+sb/2- sh/2;
        _UiRouteMapStartRectTransform.localPosition = new Vector3(sx,sy,0);
        sx = GoalX*sb+sb/2-sw/2;
        sy = GoalY*sb+sb/2- sh/2;
        _UiRouteMapGoalRectTransform.localPosition = new Vector3(sx,sy,0);       
    }

    void _RenderRouteMap()
    {
        if( ! _CanvasRoute.activeSelf ) return;

        Action<Color[],int,_RouteColor> setColor = (colors,idx,type) => {
            var i = idx*4;
            var col = _colorRoute[(int)type];
            colors[i++] = col;
            colors[i++] = col;
            colors[i++] = col;
            colors[i++] = col;
        };

        var colors = _meshRoute.colors;
        var idx = 0;
        for(var y=0; y < Height; ++y ){
            for(var x=0; x < Width; ++x ){
                var type = _RouteColor.Road;
                if( x == _routeX && y == _routeY ){
                    if( _retryRoute ) type = _RouteColor.Error;
                    else if( _readyRoute ) type = _RouteColor.Goal;
                    else type = _RouteColor.Cursor;
                }
                else if( x == StartX && y ==StartY ) type = _RouteColor.Start;
                else if( _mapRoute[x,y] != -1 ) type = _RouteColor.Route;
                else if( OnRoad(x,y) ) type = _RouteColor.Road;
                else type = _RouteColor.Ground;
                setColor(colors,idx,type);
                ++idx;
            }
        }
        _meshRoute.colors = colors;

        _UiRouteMapBodyRenderer.materialCount = 1;
        _UiRouteMapBodyRenderer.SetMaterial(Canvas.GetDefaultCanvasMaterial(),0);
        _UiRouteMapBodyRenderer.SetColor(new Color32(255,255,255,255));
        _UiRouteMapBodyRenderer.SetMesh( _meshRoute );

        _renderRoute = false;
    }

    void _ClearRoute()
    {
        for(var i=0;i<Width;++i) for( var j=0;j<Height;j++) _mapRoute[i,j] = -1;
        Route.Clear();
        _routeX = StartX;
        _routeY = StartY;
        _readyRoute = false;
        _retryRoute = false;
        _idxRouteLoop = -1;
        JsSetReward( (int)_Reward.Init );
        JsSetXY( _routeX, _routeY );
        JsSetXY( _routeX, _routeY ); // to reset myscratchunity.preX/preY
    }

    public void MakeRouteLoop()
    {
        if(_idxRouteLoop >= 0 ){
            Route.RemoveRange(0,_idxRouteLoop);
            _idxRouteLoop = -2;
        }
        else if(_idxRouteLoop == -1 ) Route.Clear();
    }

    public void ReturnToStart()
    {
        _ClearRoute();
        _renderRoute = true;
    }

    public void PushRoute(Car.Direction dir )
    {        
        var reward = _Reward.OK;

        if( _readyRoute ||  _retryRoute ) _ClearRoute();

        _mapRoute[_routeX,_routeY] = Route.Count;
        Route.Add(dir);

        if(dir==Car.Direction.Up) ++_routeY;
        if(dir==Car.Direction.Left) --_routeX;
        if(dir==Car.Direction.Down) --_routeY;
        if(dir==Car.Direction.Right) ++_routeX;

        if( _routeX == GoalX && _routeY == GoalY ){
            reward = _Reward.Goal;
            _readyRoute = true;

            _mapRoute[_routeX,_routeY] = Route.Count;
            Route.Add(dir);
        }
        else if( ! OnRoad(_routeX,_routeY) ){
            reward = _Reward.Failed;
            _retryRoute = true;
            Route.RemoveAt(Route.Count-1);
        }
        else if( _mapRoute[_routeX,_routeY] != -1 ){
            reward = _Reward.Failed;
            _retryRoute = true;

            _idxRouteLoop = _mapRoute[_routeX,_routeY];
            while( Route[_idxRouteLoop] == dir ){
                Route.Add(Route[_idxRouteLoop]);
                ++_idxRouteLoop;
            }
            Route.Add(Route[_idxRouteLoop]);
            ++_idxRouteLoop;
        }

        JsSetReward( (int)reward );
        JsSetXY( _routeX, _routeY );
        _renderRoute = true;
    }

    public void ShowRoute()
    {
        _CanvasRoute.SetActive( ! _CanvasRoute.activeSelf );
        if( ! _CanvasRoute.activeSelf ) _UiRouteMapBodyRenderer.Clear();
        else _renderRoute = true;
    }


    void _CreateImage()
    {
        var sb = (int)Const.SizeBlockImage;
        if( Image == null ){
            Image =  new Texture2D( Width*sb,Height*sb,TextureFormat.ARGB32,false);
        }

        var cols = Image.GetPixels32(0);
        for(var y=0; y < Height; ++y ){
            for(var x=0; x < Width; ++x ){
                var type = _RouteColor.Road;
                if( x == GoalX && y == GoalY ) type = _RouteColor.Goal;
                else if( x == StartX && y == StartY ) type = _RouteColor.Start;
                else if( _mapRoute[x,y] != -1 ) type = _RouteColor.Route;
                else if( OnRoad(x,y) ) type = _RouteColor.Road;
                else type = _RouteColor.Ground;
                var col = _colorRoute[(int)type];
                for( var i = 0; i < sb; ++i ) {
                    for( var j = 0; j < sb; ++j ) {
                        var idx = y*Width*sb*sb+x*sb+i*Width*sb+j;
                        cols[idx] =col;
                    }
                }
            }
        }
        Image.SetPixels32( cols, 0 );
        Image.Apply( false );
    }

    public void MyUpdate()
    {
        if( _init ){
            Ready = true;
            _init = false;
        }

        if(_renderRoute) _RenderRouteMap();
    }

    void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        _CanvasRoute = GameObject.Find("CanvasRoute");
        var rmap = _CanvasRoute.transform.Find("RouteMap").gameObject;
        _UiRouteMapBody = rmap.transform.GetChild(0).gameObject;
        _UiRouteMapBodyRenderer = _UiRouteMapBody.GetComponent<CanvasRenderer>();
        _UiRouteMapBodyRectTransform = _UiRouteMapBody.GetComponent<RectTransform>();
        _UiRouteMapBodyRectTransform.sizeDelta = new Vector2( (int)MapManager.Const.SizeRouteMap, (int)MapManager.Const.SizeRouteMap );
        _UiRouteMapStartRectTransform = rmap.transform.GetChild(1).GetComponent<RectTransform>();
        _UiRouteMapGoalRectTransform = rmap.transform.GetChild(2).GetComponent<RectTransform>();

        _CanvasRoute.SetActive(false);
    }

    private void OnDestroy()
    {
        // clean up texture to avoid memory leak
        if( Image != null ){
            Destroy(Image);
            Image = null;
        }
    }
}
