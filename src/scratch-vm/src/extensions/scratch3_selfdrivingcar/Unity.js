const Scratch3LooksBlocks = require('../../blocks/scratch3_looks');

class Unity{

    /**
    * @param {canvas} canvas - Reference to the WebGL2 canvas
    * @param {Runtime} runtime - Reference to the runtime (see also scratch-vm/src/engine/runtime.js)
    */
    constructor (canvas, runtime) {

        this._resetState();
        this._resetParams();

        this.canvas = canvas;
	this.canvas.id = "unity-canvas";
        this.runtime = runtime;
        this.parent = this.canvas.parentElement;
	this.instance = null;
	this.edidintTarget = this.runtime.getEditingTarget();
	this.sayTimer = null;
        window.myscratchunity = this;

/*
	console.log( this.canvas );
	console.log( this.canvas.id );
	console.log( this.canvas.style );
	console.log( this.canvas.style.width );
	console.log( this.canvas.style.height );
        console.log( window.devicePixelRatio );
*/

        this._say('loading');

        var buildUrl = "Build";
        var loaderUrl = buildUrl + "/selfdrivingcar.loader.js";
        var config = {
            dataUrl: buildUrl + "/selfdrivingcar.data",
            frameworkUrl: buildUrl + "/selfdrivingcar.framework.js",
            codeUrl: buildUrl + "/selfdrivingcar.wasm",
            streamingAssetsUrl: "StreamingAssets",
            companyName: "DefaultCompany",
            productName: "selfdrivingcar",
            productVersion: "0.1",
        };

        var script = document.createElement( "script" );
        script.src = loaderUrl;
        script.onload = () => {
            createUnityInstance( this.canvas, config )
            .then( (unityInstance) => {
                this.instance = unityInstance;
                this.runtime.startAnimationFrame();
            } )
            .catch( (message) => { 
                console.log( message ); 
            } );
        };
        document.body.appendChild(script);
    }

    _resetState (){
        this.stateTitle = false;
        this.stateRoute = false;
        this.stateGame = false;
    }

    _resetParams (){

	this.spawnSignals = true;
	this.spawnCars = true;
	this.viewMode = 1;
	this.timeWaiting = -1;

        this.mapWidth = 0;
        this.mapHeight = 0;
        this.stratX = 0;
        this.startY = 0;
        this.goalX = 0;
        this.goalY = 0;
        this.X = 0;
        this.Y = 0;
        this.preX = 0;
        this.preY = 0;
        this.reward = 0;

        this.speed = 0;
        this.targetSpeed = 0;

        this.navigatorDistance = 0;
	this.navigatorDirection = 0;

        this.signalDistance = 0;
        this.signalState = 0;

	this.leftSensor = 0;
	this.forwardLeftSensor = 0;
	this.forwardRightSensor = 0;
	this.rightSensor = 0;
    }

    Say(type){
	switch(type){
	    case 0:
	    this._say('clear');
	    break;

	    case 1:
	    this._say('welcome');
	    break;

	    case 2:
	    this._say('run');
	    break;

	    case 3:
	    this._say('chat');
	    break;

	    case 4:
	    this._say('signal');
	    break;

	    case 5:
	    this._say('gameover');
	    break;

	    case 6:
	    this._say('goal');
	    break;
	}
    }

    _say (type){
	console.log('say: '+type);
	tout = 0;
	text = '';
	switch(type){
	    case 'clear':
	    text = '';
	    break;

	    case 'loading':
	    text = 'LOADING...';
	    break;

	    case 'welcome':
	    text = 'Welcome !';
	    tout = 3000;
	    break;

	    case 'run':
            switch( Math.floor( Math.random()*3 ) ){
                case 0: text = "Let's GO !"; break;
                case 1: text = "Start !"; break;
                case 2: text = "Good Luck !"; break;
            }
	    tout = 3000;
	    break;

            case 'chat':
            switch( Math.floor( Math.random()*13 ) ){
                case 0: text = "Go for it !"; break;
                case 1: text = "You'll be all right !"; break;
                case 2: text = "You can make it !"; break;
                case 3: text = "Hang in there !"; break;
                case 4: text = "Keep it up !"; break;
                case 5: text = "You can do it !"; break;
                case 6: text = "I'm believing in you !"; break;
                case 7: text = "Do your best !"; break;
                case 8: text = "You'll be fine !"; break;
                case 9: text = "Don't give up !"; break;
                case 10: text = "Take it easy !"; break;
                case 11: text = "I'm on your side !"; break;
                case 12: text = "Keep going !"; break;
	    }
	    tout = 3000;
	    break;

            case 'signal':
            if( this.timeWaiting > 0 ){
		if( this.timeWaiting == 4 ) text = "It's soon !";
		else if( this.timeWaiting < 4 ) text = this.timeWaiting.toString() + " !";
		else text = this.timeWaiting.toString();
            }
            else if( this.timeWaiting == 0 ){
               text = "Go !";
            }
	    tout = 3000;
	    break;

            case 'gameover':
            switch( Math.floor( Math.random()*3 ) ){
                case 0: text = "Ouch !"; break;
                case 1: text = "Tomorrow is another day !"; break;
                case 2: text = "Cheer up !"; break;
            }
	    tout = 5000;
	    break;

	    case 'goal':
            switch( Math.floor( Math.random()*6 ) ){
                case 0: text = "Goooooal !"; break;
                case 1: text = "Excellent !"; break;
                case 2: text = "You made it !"; break;
                case 3: text = "Good Job !"; break;
                case 4: text = "Great !"; break;
                case 5: text = "Coooool !"; break;
            }

	    tout = 5000;
	    break;
	}

	if( this.edidintTarget != this.runtime.getEditingTarget() ){
            this.runtime.emit(Scratch3LooksBlocks.SAY_OR_THINK, this.edidintTarget, 'say', '');
	    this.edidintTarget = this.runtime.getEditingTarget();
	}

	if( this.sayTimer != null ){
	    clearTimeout(this.sayTimer);
            this.sayTimer = null;
	}

        this.runtime.emit(Scratch3LooksBlocks.SAY_OR_THINK, this.edidintTarget, 'say', text);
	if(tout>0){
            this.sayTimer = setTimeout(() => {
                this._say('clear');
            }, tout);
	}
    }

    Start ( signal, car ){
	console.log("Start");
	console.log( "signal = "+signal );
	console.log( "car = "+car );
	console.log(this.stateTitle);
	console.log(this.stateRoute);
	console.log(this.stateGame);
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

        this._say('clear');
        this._resetState();
        this._resetParams ();
	if( signal == 'on' ) this.spawnSignals = true;
	else this.spawnSignals = false;
	if( car == 'on' ) this.spawnCars = true;
	else this.spawnCars = false;
        this.instance.SendMessage("Manager", "CommandStart");
        return true;
    }

    Reset (){
	console.log("Reset");
	console.log(this.stateTitle);
	console.log(this.stateRoute);
	console.log(this.stateGame);
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute || this.stateGame ){
            this._say('clear');
            this._resetState();
            this._resetParams();
            this.instance.SendMessage("Manager", "CommandReset");
        }
	return true;
    }

    ShowRoute (){
	console.log("ShowRoute");
	console.log(this.stateTitle);
	console.log(this.stateRoute);
	console.log(this.stateGame);
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute || this.stateGame ){
            this.instance.SendMessage("Manager", "CommandShowRoute");
        }
        return true;
    }

    ReturnToStart (){
	console.log("ReturnToStart");
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this.instance.SendMessage("Manager", "CommandReturnToStart");
        }
        return true;
    }

    Up (){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this.instance.SendMessage("Manager", "CommandPushRoute", "0");
            console.log( "reward: "+this.reward+", nx: "+this.routeX+", ny: "+this.routeY );
        }
        return true;
    }

    Left (){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this.instance.SendMessage("Manager", "CommandPushRoute", "1");
            console.log( "reward: "+this.reward+", nx: "+this.routeX+", ny: "+this.routeY );
        }
        return true;
    }

    Down (){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this.instance.SendMessage("Manager", "CommandPushRoute", "2");
            console.log( "reward: "+this.reward+", nx: "+this.routeX+", ny: "+this.routeY );
        }
        return true;
    }

    Right (){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this.instance.SendMessage("Manager", "CommandPushRoute", "3");
            console.log( "reward: "+this.reward+", nx: "+this.routeX+", ny: "+this.routeY );
        }
        return true;
    }

    Run (){
	console.log("Run");
	console.log(this.stateTitle);
	console.log(this.stateRoute);
	console.log(this.stateGame);
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute ){
            this._resetState();
            this.instance.SendMessage("Manager", "CommandRun");
        }
        return true;
    }

    View ( viewMode ){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateGame && viewMode != this.viewMode ){
	    this.viewMode = viewMode;
            this.instance.SendMessage("Manager", "CommandToggleView");
        }
        return true;
    }

    SetTargetSpeed (speed){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateGame ){
            if( speed < 0 ) speed = 0;
	    this.targetSpeed = speed*1000;
            console.log( "speed: "+ speed );
        }
        return true;
    }

    About (){
	if( this.stateTitle == false && this.stateRoute == false && this.stateGame == false ) return false;

	if( this.stateRoute || this.stateGame ){
            this.instance.SendMessage("Manager", "CommandAbout");
        }
        return true;
    }
};

module.exports = Unity;
