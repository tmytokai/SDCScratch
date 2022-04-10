mergeInto(LibraryManager.library, {

JsSetStateTitle: function(){
        myscratchunity.stateTitle = true;
},

JsSetStateRoute: function(){
        myscratchunity.stateRoute = true;
},

JsSetStateGame: function(){
        myscratchunity.stateGame = true;
},

JsSay: function(type){
        myscratchunity.Say(type);
},

JsGetSpawnSignals: function(){
        return myscratchunity.spawnSignals;
},

JsGetSpawnCars: function(){
        return myscratchunity.spawnCars;
},

JsGetViewMode: function(){
        return myscratchunity.viewMode;
},

JsSetTimeWaiting: function(timeWaiting){
        myscratchunity.timeWaiting = timeWaiting;
},

JsSetMapWH: function(mapWidth,mapHeight){
        myscratchunity.mapWidth = mapWidth;
        myscratchunity.mapHeight = mapHeight;
},

JsSetStartXY: function(x,y){
        myscratchunity.startX = x;
        myscratchunity.startY = y;
},

JsSetGoalXY: function(x,y){
        myscratchunity.goalX = x;
        myscratchunity.goalY = y;
},

JsSetXY: function(x,y){
        myscratchunity.X = x;
        myscratchunity.Y = y;
},

JsSetReward: function(reward){
        myscratchunity.reward = reward;
},

JsSetSpeed: function(speed){
        myscratchunity.speed = speed;
},

JsGetTargetSpeed: function(){
        return myscratchunity.targetSpeed;
},

JsSetNavigatorDistance: function(distance){
        myscratchunity.navigatorDistance = distance;
},

JsSetNavigatorDirection: function(direction){
        myscratchunity.navigatorDirection = direction;
},

JsSetSignalDistance: function(distance){
        myscratchunity.signalDistance = distance;
},

JsSetSignalState: function(state){
        myscratchunity.signalState = state;
},

JsSetLeftSensor: function(distance){
        myscratchunity.leftSensor = distance;
},

JsSetForwardLeftSensor: function(distance){
        myscratchunity.forwardLeftSensor = distance;
},

JsSetForwardRightSensor: function(distance){
        myscratchunity.forwardRightSensor = distance;
},

JsSetRightSensor: function(distance){
        myscratchunity.rightSensor = distance;
},

});