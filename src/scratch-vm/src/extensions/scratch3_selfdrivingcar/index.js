const ArgumentType = require('../../extension-support/argument-type');
const BlockType = require('../../extension-support/block-type');
const Cast = require('../../util/cast');
const log = require('../../util/log');
const formatMessage = require('format-message');
const Unity = require('./Unity');

class Scratch3SelfDrivingCar {

    constructor (runtime) {
        this.promiseReset = null;
        this.cancelReset = false;

        this.runtime = runtime;
        this.myscratchunity = new Unity( this.runtime.renderer.canvas, this.runtime );
        this.runtime.on('PROJECT_START', this._startPushed.bind(this));
        this.runtime.on('PROJECT_STOP_ALL', this._stopPushed.bind(this));
        this.runtime.on('PROJECT_LOADED', this._loaded.bind(this));
        this.runtime.on('RUNTIME_DISPOSED', this._disposed.bind(this));
    }

    getInfo () {

        return {
            id: 'selfdrivingcar',
            name: formatMessage({
                id: 'selfdrivingcar.categoryName',
                default: 'Self Driving Car',
            }),
            color1: '#FF8C1A', // foreground
            color2: '#DB6E00', // background
            blocks: [

                {
                    opcode: 'Start',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Start',
                        default: 'Start : Signal[SIGNAL] Car[CAR]',
                    }),
                    arguments: {
                        SIGNAL: {
                            type: ArgumentType.STRING,
			    menu: 'onOff',
                            defaultValue: 'on'
                        },
                        CAR: {
                            type: ArgumentType.STRING,
			    menu: 'onOff',
                            defaultValue: 'on'
                        },
                    },
                },

                {
                    opcode: 'Reset',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Reset',
                        default: 'Reset',
                    }),
                },

                {
                    opcode: 'ShowRoute',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.ShowRoute',
                        default: 'Show Route Map',
                    }),
                },

                {
                    opcode: 'ReturnToStart',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.ReturnToStart',
                        default: 'Return To Start',
                    }),
                },

                {
                    opcode: 'Up',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Up',
                        default: 'Up',
                    }),
                },

                {
                    opcode: 'Left',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Left',
                        default: 'Left',
                    }),
                },

                {
                    opcode: 'Down',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Down',
                        default: 'Down',
                    }),
                },

                {
                    opcode: 'Right',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Right',
                        default: 'Right',
                    }),
                },

                {
                    opcode: 'Run',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Run',
                        default: 'Run',
                    }),
                },

                {
                    opcode: 'View',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.View',
                        default: 'View [VALUE]',
                    }),
                    arguments: {
                        VALUE: {
                            type: ArgumentType.STRING,
			    menu: 'view',
                            defaultValue: '1'
                        }
                    },
                },

                {
                    opcode: 'SetTargetSpeed',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.SetTargetSpeed',
                        default: 'Set Speed to [VALUE]',
                    }),
                    arguments: {
                        VALUE: {
                            type: ArgumentType.NUMBER,
                            defaultValue: 30
                        }
                    },
                },

                {
                    opcode: 'Brake',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.Brake',
                        default: 'Brake',
                    }),
                },

                {
                    opcode: 'About',
                    blockType: BlockType.COMMAND,
                    text: formatMessage({
                        id: 'selfdrivingcar.About',
                        default: 'About',
                    }),
                },

                {
                    opcode: 'MapWidth',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.mapWidth',
                        default: 'Map Width',
                    }),
                },

                {
                    opcode: 'MapHeight',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.mapHeight',
                        default: 'Map Height',
                    }),
                },

                {
                    opcode: 'StartX',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.startX',
                        default: 'Start X',
                    }),
                },

                {
                    opcode: 'StartY',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.startY',
                        default: 'Start Y',
                    }),
                },

                {
                    opcode: 'GoalX',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.goalX',
                        default: 'Goal X',
                    }),
                },

                {
                    opcode: 'GoalY',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.goalY',
                        default: 'Goal Y',
                    }),
                },

                {
                    opcode: 'X',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.X',
                        default: 'X',
                    }),
                },

                {
                    opcode: 'Y',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.Y',
                        default: 'Y',
                    }),
                },

                {
                    opcode: 'PreX',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.PreX',
                        default: 'PreX',
                    }),
                },

                {
                    opcode: 'PreY',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.PreY',
                        default: 'PreY',
                    }),
                },

                {
                    opcode: 'Reward',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.Reward',
                        default: 'Reward',
                    }),
                },

                {
                    opcode: 'Speed',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.Speed',
                        default: 'Speed',
                    }),
                },

                {
                    opcode: 'NavigatorDistance',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.NavigatorDistance',
                        default: 'Navigator Distance',
                    }),
                },

                {
                    opcode: 'NavigatorDirection',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.NavigatorDirection',
                        default: 'Navigator Direction',
                    }),
                },

                {
                    opcode: 'SignalDistance',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.SignalDistance',
                        default: 'Signal Distance',
                    }),
                },

                {
                    opcode: 'SignalState',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.SignalState',
                        default: 'Signal State',
                    }),
                },

                {
                    opcode: 'LeftSensor',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.LeftSensor',
                        default: 'Left Sensor',
                    }),
                },

                {
                    opcode: 'ForwardLeftSensor',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.ForwardLeftSensor',
                        default: 'Forward Left Sensor',
                    }),
                },

                {
                    opcode: 'ForwardRightSensor',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.ForwardRightSensor',
                        default: 'Forward Right Sensor',
                    }),
                },

                {
                    opcode: 'RightSensor',
                    blockType: BlockType.REPORTER,
                    text: formatMessage({
                        id: 'selfdrivingcar.RightSensor',
                        default: 'Right Sensor',
                    }),
                },

            ],
            menus: {
                onOff: ['on','off'],
                view: ['1','2','3','4']
            }
        };
    }

    _startPushed (){
        console.log("started");
	// cancel reset because PROJECT_STOP_ALL event is called before PROJECT_START event is called.
	// see also Runtime.greenFlag ()
        this.cancelReset = true;
    }

    _stopPushed (){
        console.log("stopped");
        this.cancelReset = false;
        this.promiseReset = new Promise( resolve => {
            setTimeout(() => {
                if( this.cancelReset == false ) this.myscratchunity.Reset();
                resolve();
             }, 1);
	});
    }

    _loaded (){
        console.log("loaded");
    }

    _disposed (){
        console.log("disposed");
	this.myscratchunity.Reset();
    }

    Start (args, util){
        if( ! this.myscratchunity.Start( args.SIGNAL, args.CAR ) ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Reset (args, util){
	if( ! this.myscratchunity.Reset() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    ShowRoute (args, util){
        if( ! this.myscratchunity.ShowRoute() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    ReturnToStart (args, util){
        if( ! this.myscratchunity.ReturnToStart() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Up (args, util){
        if( ! this.myscratchunity.Up() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Left (args, util){
        if( ! this.myscratchunity.Left() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Down (args, util){
        if( ! this.myscratchunity.Down() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Right (args, util){
        if( ! this.myscratchunity.Right() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Run (args, util){
        if( ! this.myscratchunity.Run() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    View (args, util){
        if( ! this.myscratchunity.View(Cast.toNumber(args.VALUE)) ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    SetTargetSpeed (args, util){
        if( ! this.myscratchunity.SetTargetSpeed(Cast.toNumber(args.VALUE)) ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    Brake (args, util){
        if( ! this.myscratchunity.SetTargetSpeed(0) ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    About (args, util){
        if( ! this.myscratchunity.About() ){
            console.log("waiting...");
            util.yieldTick();
        }
    }

    MapWidth () {
        return this.myscratchunity.mapWidth;
    }

    MapHeight () {
        return this.myscratchunity.mapHeight;
    }

    StartX () {
        return this.myscratchunity.startX;
    }

    StartY () {
        return this.myscratchunity.startY;
    }

    GoalX () {
        return this.myscratchunity.goalX;
    }

    GoalY () {
        return this.myscratchunity.goalY;
    }

    X () {
        return this.myscratchunity.X;
    }

    Y () {
        return this.myscratchunity.Y;
    }

    PreX () {
        return this.myscratchunity.preX;
    }

    PreY () {
        return this.myscratchunity.preY;
    }

    Reward () {
        return this.myscratchunity.reward;
    }

    Speed () {
        return this.myscratchunity.speed;
    }

    NavigatorDistance () {
        return this.myscratchunity.navigatorDistance;
    }

    NavigatorDirection () {
	return this.myscratchunity.navigatorDirection;
    }

    SignalDistance () {
        return this.myscratchunity.signalDistance
    }

    SignalState () {
        return this.myscratchunity.signalState;
    }

    LeftSensor (){
        return this.myscratchunity.leftSensor;
    }
	
    ForwardLeftSensor (){
        return this.myscratchunity.forwardLeftSensor;
    }

    ForwardRightSensor (){
        return this.myscratchunity.forwardRightSensor;
    }

    RightSensor (){
        return this.myscratchunity.rightSensor;
    }
}

module.exports = Scratch3SelfDrivingCar;
