// TODO:
// - alle Informationen in einen globale Variable
// - Prüfung, ob ein _result einen Error enthält -> commands
// - gCurrentFeatures auf "nur Bildkoordinaten ändern"
// - Updated 10/01/2020
'use strict'
console.log('Loading AS200')

var tools = {}
var cogUtils = {}
var cogMath = {}
if (process['platform'] === 'win32') {
  // tools = require('InSight.Tools')
  cogUtils = require('./CogUtils.js')
  cogMath = require('./CogMath.js')
} else {
  tools = process.binding('InSight.Tools')
  cogUtils = require('CogUtils.js')
  cogMath = require('CogMath.js')
}

const version = Object.freeze(new cogUtils.Version(2, 0, 9))
console.log('AS200 Version: ' + version.toString())

// Logger
const myLog = cogUtils.log
const myLogger = new Logger()

const MAX_LOGGING_ENTRIES = 60

// System

const MAX_LOOPS = 3

const MIN_POSES = 11
const MIN_POSES_ROTATED = 2
const MIN_ANGLE_RANGE = 1
const MIN_DIST_PIXEL = 50
const START_MOVE_DISTANCE_MM = 2
// Camera ID      0000 0000 xxxx xxxx 0 - 255
// Part 1         0000 0000 0000 xxxx 0 - 15
// Part 2         0000 0000 xxxx 0000 0 - 15
// Gripper 1      0000 xxxx 0000 0000

// Moving Camera  0000 1000 0000 0000

const MASK_CAM_ID = 0xff
const MASK_PART_ID_1 = 0x0f
const MASK_PART_ID_2 = 0xf0
const MASK_GRIPPER_ID_1 = 0xf00

const MASK_ID_1 = 0x0f
const MASK_ID_2 = 0xf0
const SHIFT_ID_1 = 0
const SHIFT_ID_2 = 4

const MASK_MOVING_CAMERA = 0x800

const SHIFT_PART_ID_1 = 0
const SHIFT_PART_ID_2 = 4
const SHIFT_GRIPPER_ID_1 = 8
const SHIFT_MOVING_CAMERA = 11

const MAX_CALIBRATIONS = 2

const MAX_CAMERAS = 2
const MAX_FEATURES_PER_CAMERA = 2
const MAX_GRIPPERS = 4

// For the recipe-tables
const RECIPE_MAX_STEPS = 12
const RECIPE_MAX_FEATURES = 12
const RECIPE_MAX_PARTS = 12
const RECIPE_STEPTABLE = 'Recipe.StepTable'
const RECIPE_FEATURETABLE = 'Recipe.FeatureTable'
const RECIPE_PARTTABLE = 'Recipe.PartTable'

// Communication
const INTERNAL_PORT = 7891

const CAMERACONFIG_FILENAME = 'CameraConfig.cxd'
const SENSORCONFIG_FILENAME = 'SensorConfig.cxd'

var timeTracker = (function () {
  var startTime = process.hrtime()
  return {
    restart: function () {
      startTime = process.hrtime()
    },
    getElapsedTime: function () {
      let dif = process.hrtime(startTime)
      return ((dif[0] + dif[1] / 1000000000).toFixed(6))
    }
  }
})()

var tracer = (function () {
  var messages = []
  return {
    clear: function () {
      messages = []
    },
    addMessage: function (message) {
      messages.push(message)
      if (messages.length > 150) {
        messages.splice(0, messages.length - 150)
      }
    },
    print: function () {
      messages.forEach(function (message) {
        myLog(message)
      })
    }
  }
})()

var g_Inspections = {}

var g_CustomCalibrationSettings = null
var g_HECalibrationSettings = null

var g_HRes = 1280
var g_VRes = 960
var g_Border = [[0, 0, 0, 1280], [0, 1280, 960, 1280], [960, 1280, 960, 0], [960, 0, 0, 0]]

var g_Graphics = { 'ShowCalibrationPoints_1': 0, 'ShowCalibrationPoints_2': 0, 'ShowCrossHair': null }
var g_Settings = {
  'UseShuttledCameras': false,
  'Sensor': null,
  'Cams': null,
  'JobServer': null,
  'Communication': null,
  'AutoCalibration': null,
  'LogImageFtp': null,
  'CustomCalibration': null
}

var g_CurrentFeatures = null
var g_FeaturesInfos = null
var g_RuntimeFeatures = null
var g_NumberFound = null
var g_AdditionalError = null
var g_TrainedFeatures = null
var g_StoredTrainedFeatures = null
var g_TrainedRobotPoses = null
var g_StoredTrainedRobotPoses = null
var g_GripCorrections = null
var g_FrameCorrections = null

var g_Parts = null

var g_AutoCalibRuntime = {}
var g_StepsByID = null
var g_LooukupTables = null

var g_Calibrations = null

var g_IsEnableLimit = null
var g_LimitX = null
var g_LimitY = null
var g_LimitTheta = null

//* ***************************************************************************/
var RobotPose = (function () {
  function RobotPose (x, y, z, thetaZ, thetaY, thetaX, valid) {
    this.valid = valid
    this.x = x
    this.y = y
    this.z = z
    this.thetaZ = thetaZ
    this.thetaY = thetaY
    this.thetaX = thetaX
  }
  return RobotPose
}())
RobotPose.prototype.getAsString = function () {
  let res = InSightFunctions.fnStringf('%.5f,%.5f,%.5f,%.5f,%.5f,%.5f', this.x, this.y, this.z, this.thetaZ, this.thetaY, this.thetaX)
  return res
}

var FeatureInfo = (function () {
  function FeatureInfo (cameraIsMoving, partIsMoving, shuttlePose, partID) {
    this.cameraIsMoving = cameraIsMoving
    this.partIsMoving = partIsMoving
    this.shuttlePose = shuttlePose
    this.partID = partID
  }
  return FeatureInfo
}())

var CustomCalibrationSettings = (function () {
  function CustomCalibrationSettings (swapHandedness, featureOffsetX, featureOffsetY, calibrationID) {
    this.swapHandedness = swapHandedness
    this.featureOffsetX = featureOffsetX
    this.featureOffsetY = featureOffsetY
    this.calibrationID = calibrationID
  }
  return CustomCalibrationSettings
}())
var HECalibrationSettings = (function () {
  function HECalibrationSettings () {
    this.calibrationID = -1
  }
  return HECalibrationSettings
}())
function Result () {
  this.state = 0 // contains the state of the operation/result => OK/nOK
  this.isValid = 0 // the result structure was updated, the data are ready to use
  this.isNeeded = 0
  this.data = []
}

//* ***************************************************************************/

var Feature = (function () {
  function Feature (x, y, thetaInDegrees, valid) {
    this.valid = valid
    this.x = x
    this.y = y
    this.thetaInDegrees = thetaInDegrees
  }
  return Feature
}())

Feature.prototype.reset = function () {
  this.x = 0
  this.y = 0
  this.thetaInDegrees = 0
  this.valid = 0
}
var CogCalibFixComputationModeConstants;
(function (CogCalibFixComputationModeConstants) {
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['Linear'] = 1] = 'Linear'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['PerspectiveAndRadialWarp'] = 2] = 'PerspectiveAndRadialWarp'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['LinescanWarp'] = 3] = 'LinescanWarp'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['Linescan2DWarp'] = 4] = 'Linescan2DWarp'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['SineTanLawProjectionWarp'] = 5] = 'SineTanLawProjectionWarp'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['ThreeParamRadialWarp'] = 6] = 'ThreeParamRadialWarp'
  CogCalibFixComputationModeConstants[CogCalibFixComputationModeConstants['NoDistortionWarp'] = 7] = 'NoDistortionWarp'
})(CogCalibFixComputationModeConstants || (CogCalibFixComputationModeConstants = {}))

// ReSharper disable once InconsistentNaming
var CogNPointToNPointDOFConstants;
(function (CogNPointToNPointDOFConstants) {
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['None'] = 0] = 'None'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['TranslationX'] = 1] = 'TranslationX'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['TranslationY'] = 2] = 'TranslationY'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['Translation'] = 3] = 'Translation'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['RotationAndTranslation'] = 4] = 'RotationAndTranslation'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['ScalingRotationAndTranslation'] = 5] = 'ScalingRotationAndTranslation'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['ScalingAspectRotationAndTranslation'] = 6] = 'ScalingAspectRotationAndTranslation'
  CogNPointToNPointDOFConstants[CogNPointToNPointDOFConstants['ScalingAspectRotationSkewAndTranslation'] = 7] = 'ScalingAspectRotationSkewAndTranslation'
})(CogNPointToNPointDOFConstants || (CogNPointToNPointDOFConstants = {}))

var CogNPointToNPoint = (function () {
  function CogNPointToNPoint () {
    this.groupAPoints = []
    this.groupBPoints = []
    this.computationMode = CogCalibFixComputationModeConstants.Linear
    this.dofsToCompute = CogNPointToNPointDOFConstants.RotationAndTranslation
    this.S_ = {
      n: 0,
      sum_u: 0,
      sum_v: 0,
      sum_u2: 0,
      sum_v2: 0,
      sum_uv: 0,
      sum_x: 0,
      sum_y: 0,
      sum_x2: 0,
      sum_y2: 0,
      sum_xy: 0,
      sum_ux: 0,
      sum_uy: 0,
      sum_vx: 0,
      sum_vy: 0
    }
  }
  Object.defineProperty(CogNPointToNPoint.prototype, 'NumPoints', {
    // Constructor
    // Public properties
    // ReSharper disable once InconsistentNaming
    get: function () {
      return this.groupAPoints.length
    },
    enumerable: true,
    configurable: true
  })
  Object.defineProperty(CogNPointToNPoint.prototype, 'ComputationMode', {
    // ReSharper disable once InconsistentNaming
    get: function () {
      return this.computationMode
    },
    // ReSharper disable once InconsistentNaming
    set: function (val) {
      if (val !== CogCalibFixComputationModeConstants.Linear) { throw new Error('Only Linear mode is supported') }
      this.computationMode = val
    },
    enumerable: true,
    configurable: true
  })
  Object.defineProperty(CogNPointToNPoint.prototype, 'DOFsToCompute', {
    // ReSharper disable once InconsistentNaming
    get: function () {
      return this.dofsToCompute
    },
    // ReSharper disable once InconsistentNaming
    set: function (val) {
      if (val !== CogNPointToNPointDOFConstants.RotationAndTranslation) { throw new Error('Only RotationAndTranslation mode is supported') }
      this.dofsToCompute = val
    },
    enumerable: true,
    configurable: true
  })

  // Public Methods
  // ReSharper disable once InconsistentNaming
  CogNPointToNPoint.prototype.AddPointPair = function (groupAPoint, groupBPoint) {
    if (groupAPoint.valid && groupBPoint.valid) {
      this.groupAPoints.push(groupAPoint)
      this.groupBPoints.push(groupBPoint)
      var u = groupAPoint.x
      var v = groupAPoint.y
      var x = groupBPoint.x
      var y = groupBPoint.y
      this.S_.sum_u += u
      this.S_.sum_v += v
      this.S_.sum_u2 += u * u
      this.S_.sum_v2 += v * v
      this.S_.sum_uv += u * v
      this.S_.sum_x += x
      this.S_.sum_y += y
      this.S_.sum_x2 += x * x
      this.S_.sum_y2 += y * y
      this.S_.sum_xy += x * y
      this.S_.sum_ux += u * x
      this.S_.sum_vx += v * x
      this.S_.sum_uy += u * y
      this.S_.sum_vy += v * y
      this.S_.n++
    }
  }
  CogNPointToNPoint.prototype.ComputeGroupAFromGroupBTransform = function () {
    var eps = 1e-15
    var n = this.S_.n
    if (n < 2) { throw new Error('Unstable Error!') }
    var c1 = n * (this.S_.sum_ux + this.S_.sum_vy) - this.S_.sum_u * this.S_.sum_x - this.S_.sum_v * this.S_.sum_y
    var c2 = n * (this.S_.sum_vx - this.S_.sum_uy) + this.S_.sum_u * this.S_.sum_y - this.S_.sum_v * this.S_.sum_x
    var angle = Math.atan2(c2, c1)
    if (Math.abs(c1) < eps && Math.abs(c2) < eps) { throw new Error('Unstable Error!') }
    var s = Math.sin(angle)
    var c = Math.cos(angle)
    var x = (this.S_.sum_u - c * this.S_.sum_x + s * this.S_.sum_y) / n
    var y = (this.S_.sum_v - s * this.S_.sum_x - c * this.S_.sum_y) / n
    var groupAFromGroupBTransform = new cogMath.cc2XformLinear()
    groupAFromGroupBTransform.setXform([[c, -s, x], [s, c, y]])
    var rmsError = 0
    for (var i = 0; i < this.groupBPoints.length; i++) {
      var mappedPoint = groupAFromGroupBTransform.mapPoint([this.groupBPoints[i].x, this.groupBPoints[i].y])
      var xError = this.groupAPoints[i].x - mappedPoint[0]
      var yError = this.groupAPoints[i].y - mappedPoint[1]
      rmsError += xError * xError + yError * yError
    }

    rmsError = Math.sqrt(rmsError / this.groupBPoints.length)
    return { groupAFromGroupBTransform: groupAFromGroupBTransform, rmsError: rmsError }
  }
  return CogNPointToNPoint
}())
function ComputeDesiredStagePosition (trainFeatures, runFeatures, heCalibResultJSON, unCorrectedStagePoseX, unCorrectedStagePoseY, unCorrectedStagePoseThetaDeg) {
  if (runFeatures.length !== trainFeatures.length) { throw new Error('The number of train features is not equal to the number of run features') }
  var heCalibResult = new cogMath.ccMultiViewPlanarMotionCalibResult()
  // heCalibResult.initFromObject(JSON.parse(heCalibResultJSON));
  heCalibResult.initFromObject(heCalibResultJSON)
  // Convert stage position to corrected Home2D
  var home2DFromStage2DUnCorrected = new cogMath.cc2Rigid()
  home2DFromStage2DUnCorrected.setXform(unCorrectedStagePoseThetaDeg, [unCorrectedStagePoseX, unCorrectedStagePoseY])
  var home2DFromStage2D = heCalibResult
    .convertUncorrectedHome2DFromStage2DToHome2DFromStage2D(home2DFromStage2DUnCorrected)
  var stage2DFromHome2D = home2DFromStage2D.inverse()
  var xform = {}
  if (trainFeatures.length >= 2) {
    // Map run time features to position when stage is at home
    var remappedRunFeatures = []
    for (var _i = 0, runFeatures_1 = runFeatures; _i < runFeatures_1.length; _i++) {
      var feature = runFeatures_1[_i]
      var mappedFeature = stage2DFromHome2D.mapPoint([feature.x, feature.y])
      remappedRunFeatures.push(new Feature(mappedFeature[0], mappedFeature[1], 0, feature.valid))
    }
    // Compute trainFromRun transform. This is the desire position of the stage
    var nPointToNPoint = new CogNPointToNPoint()
    for (var i = 0; i < trainFeatures.length; i++) {
      nPointToNPoint.AddPointPair(trainFeatures[i], remappedRunFeatures[i])
    }
    // var rmsError = void 0
    xform = nPointToNPoint.ComputeGroupAFromGroupBTransform()
  } else if (trainFeatures.length === 1) {
    var home2DRunFromPart2D = new cogMath.cc2Rigid()
    home2DRunFromPart2D.setXform(runFeatures[0].thetaInDegrees, [runFeatures[0].x, runFeatures[0].y])
    var stage2DRunFromPart2D = stage2DFromHome2D.compose(home2DRunFromPart2D)
    var trainFromPart = new cogMath.cc2XformLinear()
    trainFromPart.setXformScaleRotation(trainFeatures[0].thetaInDegrees, trainFeatures[0].thetaInDegrees, 1.0, 1.0, [trainFeatures[0].x, trainFeatures[0].y])
    var trainFromRun = trainFromPart.compose(stage2DRunFromPart2D.inverse())
    var trainFromRunLinear = new cogMath.cc2XformLinear()
    trainFromRunLinear.setXformScaleRotation(trainFromRun.rotationInDegrees(), trainFromRun.rotationInDegrees(), 1.0, 1.0, trainFromRun.trans())
    xform = { groupAFromGroupBTransform: trainFromRunLinear, rmsError: 0.0 }
  } else { throw new Error('At least two valid point pairs should be added') }

  // Convert to uncorrected Home2D
  var desiredHome2DFromStage2DUnCorrected = new cogMath.cc2Rigid()
  desiredHome2DFromStage2DUnCorrected.setXform(xform['groupAFromGroupBTransform'].rotationInDegrees(), xform['groupAFromGroupBTransform'].trans())
  var desiredHome2DFromStage2DCorrected = heCalibResult
    .convertUncorrectedHome2DFromStage2DToHome2DFromStage2D(desiredHome2DFromStage2DUnCorrected)
  // Generate results object
  var result = {}
  result['desiredHome2DFromStage2DCorrected'] = desiredHome2DFromStage2DCorrected
  result['rmsError'] = xform['rmsError']
  return result
}

//* ***************************************************************************/
//* ***************************************************************************/
// Definitions
//* ***************************************************************************/
// TODO:
// FIXME: // NOT RUNNING
//* ***************************************************************************/
// States
var States;
(function (States) {
  States[States['WAITING_FOR_NEW_COMMAND'] = 1] = 'WAITING_FOR_NEW_COMMAND'
  States[States['WAITING_FOR_IMAGE_ACQUIRED'] = 2] = 'WAITING_FOR_IMAGE_ACQUIRED'
  States[States['WAITING_FOR_TOOLS_DONE'] = 3] = 'WAITING_FOR_TOOLS_DONE'
  States[States['WAITING_FOR_SLAVE_RESULT'] = 4] = 'WAITING_FOR_SLAVE_RESULT'
})(States || (States = {}))
//* ***************************************************************************/
// Error codes
var ECodes;
(function (ECodes) {
  ECodes[ECodes['E_NO_ERROR'] = 99999] = 'E_NO_ERROR'
  ECodes[ECodes['E_UNSPECIFIED'] = 0] = 'E_UNSPECIFIED'

  ECodes[ECodes['E_TIMEOUT'] = -1000] = 'E_TIMEOUT'
  ECodes[ECodes['E_UNKNOWN_COMMAND'] = -1001] = 'E_UNKNOWN_COMMAND'
  ECodes[ECodes['E_INDEX_OUT_OF_RANGE'] = -1002] = 'E_INDEX_OUT_OF_RANGE'
  ECodes[ECodes['E_TOO_FEW_ARGUMENTS'] = -1003] = 'E_TOO_FEW_ARGUMENTS'
  ECodes[ECodes['E_INVALID_ARGUMENT'] = -1005] = 'E_INVALID_ARGUMENT'
  ECodes[ECodes['E_COMMAND_NOT_ALLOWED'] = -1006] = 'E_COMMAND_NOT_ALLOWED'
  ECodes[ECodes['E_COMBINATION_NOT_ALLOWED'] = -1007] = 'E_COMBINATION_NOT_ALLOWED'
  ECodes[ECodes['E_BUSSY'] = -1008] = 'E_BUSSY'
  ECodes[ECodes['E_NOT_FULLY_IMPLEMENTED'] = -1009] = 'E_NOT_FULLY_IMPLEMENTED'
  ECodes[ECodes['E_NOT_SUPPORTED'] = -1010] = 'E_NOT_SUPPORTED'
  ECodes[ECodes['E_RESULSTRING_TO_LONG'] = -1011] = 'E_RESULSTRING_TO_LONG'

  ECodes[ECodes['E_DIFFERENT_JOB_NAMES'] = -1101] = 'E_DIFFERENT_JOB_NAMES'
  ECodes[ECodes['E_DIFFERENT_VERSIONS'] = -1102] = 'E_DIFFERENT_VERSIONS'

  ECodes[ECodes['E_NOT_CALIBRATED'] = -2001] = 'E_NOT_CALIBRATED'
  ECodes[ECodes['E_CALIBRATION_FAILED'] = -2002] = 'E_CALIBRATION_FAILED'
  ECodes[ECodes['E_INVALID_CALIBRATION_DATA'] = -2003] = 'E_INVALID_CALIBRATION_DATA'
  ECodes[ECodes['E_NOT_GIVEN_CALIBRATION_POSE'] = -2004] = 'E_NOT_GIVEN_CALIBRATION_POSE'
  ECodes[ECodes['E_NO_START_COMMAND'] = -2005] = 'E_NO_START_COMMAND'

  ECodes[ECodes['E_FEATURE_NOT_TRAINED'] = -3001] = 'E_FEATURE_NOT_TRAINED'
  ECodes[ECodes['E_FEATURE_NOT_FOUND'] = -3002] = 'E_FEATURE_NOT_FOUND'
  ECodes[ECodes['E_FEATURE_NOT_MAPPED'] = -3003] = 'E_FEATURE_NOT_MAPPED'
  ECodes[ECodes['E_TARGET_POSE_NOT_TRAINED'] = -3004] = 'E_TARGET_POSE_NOT_TRAINED'
  ECodes[ECodes['E_ROBOT_POSE_NOT_TRAINED'] = -3005] = 'E_ROBOT_POSE_NOT_TRAINED'

  ECodes[ECodes['E_INVALID_PART_ID'] = -4001] = 'E_INVALID_PART_ID'
  ECodes[ECodes['E_PART_NOT_ALL_FEATURES_LOCATED'] = -4002] = 'E_PART_NOT_ALL_FEATURES_LOCATED'
  ECodes[ECodes['E_PART_NO_VALID_GRIP_CORRECTION'] = -4003] = 'E_PART_NO_VALID_GRIP_CORRECTION'
  ECodes[ECodes['E_PART_NO_VALID_FRAME_CORRECTION'] = -4004] = 'E_PART_NO_VALID_FRAME_CORRECTION'

  ECodes[ECodes['E_ALIGN_OVER_LIMITATION'] = -5001] = 'E_ALIGN_OVER_LIMITATION'
  ECodes[ECodes['E_ALIGN_FAIL_PATMAX_CONDITION'] = -5002] = 'E_ALIGN_FAIL_PATMAX_CONDITION'
  ECodes[ECodes['E_ALIGN_FAIL_HISTOGRAM_CONDITION'] = -5003] = 'E_ALIGN_FAIL_HISTOGRAM_CONDITION'

  ECodes[ECodes['E_INTERNAL_ERROR'] = -9999] = 'E_INTERNAL_ERROR'
})(ECodes || (ECodes = {}))

var CoordinateSystem;
(function (CoordinateSystem) {
  CoordinateSystem[CoordinateSystem['HOME2D'] = 1] = 'HOME2D'
  CoordinateSystem[CoordinateSystem['CAM2D'] = 2] = 'CAM2D'
  CoordinateSystem[CoordinateSystem['RAW2D'] = 3] = 'RAW2D'
})(CoordinateSystem || (CoordinateSystem = {}))

var ResultMode;
(function (ResultMode) {
  ResultMode[ResultMode['ABS'] = 1] = 'ABS'
  ResultMode[ResultMode['OFF'] = 2] = 'OFF'
  ResultMode[ResultMode['FRAME'] = 3] = 'FRAME'
  ResultMode[ResultMode['PICKED'] = 4] = 'PICKED'
  ResultMode[ResultMode['GC'] = 5] = 'GC'
  ResultMode[ResultMode['GCP'] = 6] = 'GCP'
})(ResultMode || (ResultMode = {}))
//* ***************************************************************************/
// End of Definitions
//* ***************************************************************************/
//* ***************************************************************************/

//* ***************************************************************************/
// AS200
//* ***************************************************************************/
function AS200 () {
  // ScriptBase.call(this);
  timeTracker.restart()
  myLog('AS200 Script: -> Init!')

  this.counter = 0

  this.firstRunDone = 0
  this.firstTimeOnlineDone = 0
  this.autoExposureState = 0

  this.currentFeatureMask = 1
  this.triggerMode = 32
  // Contains Informations about the camera (IP, Name, Master, JobName, SLMP, Index,...)
  this.myDetails = new MyDetails()

  // Contains the Informations from the SensorConfig file (only Master)
  // this.sensorConfigFile = null; // The whole file
  this.mySensor = null // Sensor information from the SensorConfig file
  this.myRecipes = null // all recipes stored in the SensorConfig file

  this.mySensorConfiguration = null// new SensorConfiguration();
  // this.sensorConfigFile.loadFromFile();

  this.myRecipeTables = null// new RecipeTables();

  // CameraConfig file (Master and Slave)
  // this.cameraConfigFile = null;

  this.myCalibrations = 0
  this.commands = null
  this.slmpMap = null
  this.currentCommand = null
  this.currentState = States.WAITING_FOR_NEW_COMMAND
  this.myParts = null
  // this.sendToSlavesLookUp = { "useIndex": {}, "usePartID": {}, "useStepID": {} };

  this.mySharedObjects = SharedObjects.getInstance()
  this.mySharedObjects.addSharedObject('AS200', this)

  for (let i in masterCommands) {
    if (typeof masterCommands[i] === 'function') {
      inheritPseudoClass(CommandBase, masterCommands[i])
    }
  }

  for (let i in slaveCommands) {
    if (typeof slaveCommands[i] === 'function') {
      inheritPseudoClass(SlaveBase, slaveCommands[i])
    }
  }
  myLog('AS200 init done ' + timeTracker.getElapsedTime())
};

AS200.prototype.run = function (image, online) {
  tracer.addMessage('-> AS200 Script: Run ' + timeTracker.getElapsedTime())
  if (this.firstRunDone === 0) {
    this.firstRun()
  } else if ((online === 1) && (this.firstRunDone === 1) && (this.firstTimeOnlineDone === 0)) {
    this.firstTimeOnline()
  } else if (online === 0) {
    this.currentState = States.WAITING_FOR_NEW_COMMAND
    this.currentCommand = null
  }

  tracer.addMessage('<- AS200 Script: Run ' + timeTracker.getElapsedTime())
  return this.currentState
}

AS200.prototype.save = function () {
  myLog('-> AS200 Save data ')
  myLog('<- AS200 Save data')
  return {
    'g_StoredTrainedFeatures': JSON.stringify(g_TrainedFeatures),
    'g_StoredTrainedRobotPoses': JSON.stringify(g_TrainedRobotPoses)
  }
}

AS200.prototype.load = function (saved) {
  myLog('-> AS200 Load stored data')
  console.log('saved = ' + JSON.stringify(saved))
  g_StoredTrainedFeatures = JSON.parse(saved.g_StoredTrainedFeatures)
  g_StoredTrainedRobotPoses = JSON.parse(saved.g_StoredTrainedRobotPoses)
  myLog('<- AS200 Load stored data')
}

AS200.prototype.drawGraphics = function (gr) {
  let colors = [0x00FF00, 0x00FFFF]
  tracer.addMessage('-> Draw ' + timeTracker.getElapsedTime())

  if (g_Graphics.ShowCalibrationPoints_1 > 0) {
    let targetX = this.myCalibrations.calibrations[1].calibrationData.targetX
    let targetY = this.myCalibrations.calibrations[1].calibrationData.targetY

    for (var i = 0; i < targetX.length; i++) {
      gr.plotPoint(targetX[i], targetY[i], (i + 1).toString(), 0x00FF00)
    }
  }

  if (g_Graphics.ShowCalibrationPoints_2 > 0) {
    let targetX = this.myCalibrations.calibrations[2].calibrationData.targetX
    let targetY = this.myCalibrations.calibrations[2].calibrationData.targetY

    for (let i = 0; i < targetX.length; i++) {
      gr.plotPoint(targetX[i], targetY[i], (i + 1).toString(), 0x00FF00)
    }
  }
  if (g_Graphics.ShowCrossHair != null) {
    let count = g_Graphics.ShowCrossHair.length
	//GSSVN: Re-draw golden pose mark and color
    for (let i = 0; i < count; i++) {
      var lines = CrossHair(g_Graphics.ShowCrossHair[i][0], g_Graphics.ShowCrossHair[i][1], g_Graphics.ShowCrossHair[i][2])
      //gr.plotLine(lines[0][0], lines[0][1], lines[0][2], lines[0][3], '', colors[i], 1, 0, 0)
      //gr.plotLine(lines[1][0], lines[1][1], lines[1][2], lines[1][3], '', colors[i], 1, 0, 0)
      gr.plotCross(g_Graphics.ShowCrossHair[i][0], g_Graphics.ShowCrossHair[i][1], g_Graphics.ShowCrossHair[i][2], 200,200 , '', colors[i], 1)
      gr.plotCircle(g_Graphics.ShowCrossHair[i][0], g_Graphics.ShowCrossHair[i][1],20,'',colors[i], 1)
    }
  }
  tracer.addMessage('<- Draw ' + timeTracker.getElapsedTime())
}
AS200.prototype.initVeryFirstTime = function () {
  let firstTime = InSightFunctions.fnGetCellValue('Internal.VeryFirstInitializationDone')
  if (firstTime == 0) {    
    InSightFunctions.fnSetCellValue('Internal.VeryFirstInitializationDone', 1)
  }
}
AS200.prototype.firstRun = function () {
  myLog('Script Base: -> First Run')

  if (this.checkJobVersion() === false) {
    tracer.addMessage('Jobversion to old!')
    throw new Error('Jobversion to old!')
  }

  this.initVeryFirstTime()

  this.triggerMode = InSightFunctions.fnGetCellValue('TriggerMode')
  this.runStateMachine = this.runSlaveStateMachine
  this.autoExposureState = InSightFunctions.fnGetCellValue('AutoImageAdjustment.Enable')

  InSightFunctions.fnSetCellValue('Internal.AS200.Version', version.toString())
  InSightFunctions.fnSetCellValue('Internal.CogMath.Version', cogMath.version.toString())
  InSightFunctions.fnSetCellValue('Internal.CogUtils.Version', cogUtils.version.toString())

  InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 0)
  InSightFunctions.fnSetCellValue('Internal.RecipeLoaded', 0)
  InSightFunctions.fnSetCellValue('Internal.LoadSlaves', 0)
  InSightFunctions.fnSetCellValue('Init.Hostname', '127.0.0.1')
  InSightFunctions.fnSetCellValue('Communication.EnableTimer', 0)

  g_HRes = InSightFunctions.fnGetSystemConfig('HRESOLUTION')
  g_VRes = InSightFunctions.fnGetSystemConfig('VRESOLUTION')
  g_Border = [[0, 0, 0, g_HRes],
    [0, g_HRes, g_VRes, g_HRes],
    [g_VRes, g_HRes, g_VRes, 0],
    [g_VRes, 0, 0, 0]]

  myLogger.clearLog()

  g_CurrentFeatures = {}
  for (var f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
    g_CurrentFeatures[f] = new Feature(0, 0, 0, 0)
  }
  // this.commands = masterCommands;
  this.loadSensorConfig()

  LoadCameraConfigFile()

  this.fillSLMP_Map()
  this.firstRunDone = 1
  myLog('Script Base: <- First Run')
}
AS200.prototype.firstTimeOnline = function () {
  myLog('-> First time onlinne')

  InSightFunctions.fnSetCellValue('Internal.LoadSlaves', 1)

  InSightFunctions.fnUpdateGui(1)

  this.firstTimeOnlineDone = 1
  myLog('<- First time onlinne')
}

AS200.prototype.fillSLMP_Map = function () {
  myLog('-> Fill SLMP-Map ' + timeTracker.getElapsedTime())
  this.slmpMap = {}
  for (var i in this.commands) {
    if (typeof this.commands[i] === 'function') {
      var o = new (this.commands[i])(1, '1,1,1,1,0,0,0,0,0,0,0')

      if (o.hasOwnProperty('_slmpCode')) {
        var code = o._slmpCode
        this.slmpMap[code] = i
      }
    }
  }
  myLog(this.slmpMap)
  myLog('<- Fill SLMP-Map ' + timeTracker.getElapsedTime())
}
AS200.prototype.initTCPDevices = function () {
  myLog('-> initTCPDevices')
  try {
    if (this.myDetails.iAmMaster == 1) {
      for (var i = 1; i <= 2; i++) {
        let port = INTERNAL_PORT
        let ip = ''
        let enable = 0
        if (this.mySensor.cams.hasOwnProperty('Cam_' + i.toString()) == true) {
          let cam = this.mySensor.cams['Cam_' + i.toString()]
          ip = cam.IPAddress
          if (cam.Master == true) {
            port = this.mySensor.plcPort
            ip = ''
          }
          enable = 1
        }

        let t1 = 'Communication.TCPDevice.' + i.toString() + '.Port'
        InSightFunctions.fnSetCellValue(t1, port)

        let t2 = 'Communication.TCPDevice.' + i.toString() + '.HostIP'
        InSightFunctions.fnSetCellValue(t2, ip)

        let t3 = 'Communication.TCPDevice.' + i.toString() + '.Enabled'
        InSightFunctions.fnSetCellValue(t3, enable)
      }
    } else {
      InSightFunctions.fnSetCellValue('Communication.TCPDevice.1.Port', INTERNAL_PORT)
      InSightFunctions.fnSetCellValue('Communication.TCPDevice.1.HostIP', '')
      InSightFunctions.fnSetCellValue('Communication.TCPDevice.1.Enabled', 1)
      InSightFunctions.fnSetCellValue('Communication.TCPDevice.2.Enabled', 0)
    }
  } catch (e) {
    myLog(e)
  } finally {
    myLog('<- initTCPDevices')
  }
}
AS200.prototype.checkJobVersion = function () {
  let result = false

  let jobMajor = InSightFunctions.fnGetCellValue('Version.Major')

  if (jobMajor >= 1) {
    result = true
  }

  return result
}
AS200.prototype.clearLog = function () {
  myLogger.clearLog()
}
AS200.prototype.setRecipeTablesToDefault = function () {
  this.myRecipeTables.setToDefault()
}
AS200.prototype.onDataReceived = function (channel, plc, master, slave, timeout, device, data) {
  let ret = '---'
  if (device == 1) {
    if (plc == 1) {
      this.onDataFromPLCReceived(channel, data)
      ret = data
    } else if (master == 1) {
      this.onDataFromMasterReceived(channel, data)
      ret = data
    } else if (slave == 1) {
      this.onDataFromSlaveReceived(channel, timeout, data)
      ret = data
    } else {
      myLog('No valid TCP configuration!')
    }
  } else if ((timeout == 1) && (slave == 1)) {
    this.onDataFromSlaveReceived(channel, timeout, data)
    ret = 'Timeout'
    myLog('Timeout!')
  }
  return ret
}
AS200.prototype.onDataFromPLCOverSLMPReceived = function (data) {
  let splitted = data.split(',')
  //GSSVN: Check XTT Type, using if send XTT with XI command
  InSightFunctions.fnSetCellValue('Align.Tray.XTTType', parseInt(0))
  if (this.slmpMap.hasOwnProperty(splitted[0])) {
    // var toRep = splitted[0] + ",";
    // var repWith = this.SLMP_Map[splitted[0]] + ",";
    // receivedData = slmpData.replace(toRep, repWith);
    let index = data.indexOf(',')
    data = this.slmpMap[splitted[0]] + data.substr(index, data.length)
    InSightFunctions.fnSetCellValue('Align.Tray.CmdID', parseInt(splitted[0]))
  }
  //GSSVN: Add SLMP code for reset busy state
  if(parseInt(splitted[0]) == 9999)
  {
    data = 'RESET';
  }
  
  this.onDataFromPLCReceived(0, data)
}
AS200.prototype.onDataFromPLCReceived = function (channel, data) {
  timeTracker.restart()
  tracer.clear()
  data = data.toUpperCase()

  tracer.addMessage('-> Data from PLC received on channel ' + channel + ' -> ' + data)
  myLogger.addLogMessage(0, '-> ' + data)

  //Read Limitation information
  g_IsEnableLimit = InSightFunctions.fnGetCellValue('Align.Limit.Enable')
  g_LimitX = InSightFunctions.fnGetCellValue('Align.Limit.ErrFree.X')
  g_LimitY = InSightFunctions.fnGetCellValue('Align.Limit.ErrFree.Y')
  g_LimitTheta = InSightFunctions.fnGetCellValue('Align.Limit.ErrFree.Theta')
  //myLogger.addLogMessage(0, '[Limit] Enable: ' + g_IsEnableLimit + ' X: ' + g_LimitX + ' Y: ' + g_LimitY+ ' T: ' + g_LimitTheta)

  if (this.currentState === States.WAITING_FOR_NEW_COMMAND) {
    this.currentCommand = getCommandObject(this.commands, this.myDetails.myIndex, data)

    if (typeof this.currentCommand !== 'object') {
      // Unknow command / wrong number of arguments / ....
      let ret = InSightFunctions.fnStringf('%s,%d', data.split(',')[0], this.currentCommand)
      this.sendToPlc(ret)
      this.currentCommand = null
    } else {
      // valid command
      InSightFunctions.fnSetCellValue('Internal.Command', this.currentCommand._splittedCmd[0])
      //GSSVN: If command is XTT, write pocket position to cell
      // XTT,FeatureID,Mode,Pocket1,Pocket2,X,Y,Z,A,B,C
      if(this.currentCommand._splittedCmd[0]=='XTT')
      {
        //myLogger.addLogMessage(0, "Pocket1: " + this.currentCommand._splittedCmd[3])
        //myLogger.addLogMessage(0, "Pocket2: " + this.currentCommand._splittedCmd[4])
 		InSightFunctions.fnSetCellValue('Align.Tray.EnableAllToolDone', 0)

        InSightFunctions.fnSetCellValue('Align.Tray.Pocket1', parseInt(this.currentCommand._splittedCmd[3]))
        InSightFunctions.fnSetCellValue('Align.Tray.Pocket2', parseInt(this.currentCommand._splittedCmd[4]))
        //InSightFunctions.fnSetCellValue('Align.Tray.EnableAllToolDone', 1)
         
      }
      this.runStateMachine()
    }
  } else {
    // State machine is not waiting for a new command -> bussy
    let ret = ''
    if (data == 'RESET') {
      this.currentState = States.WAITING_FOR_NEW_COMMAND
      this.currentCommand = null
      ret = 'RESET,1'
    } else {
      ret = InSightFunctions.fnStringf('%s,%d', data.split(',')[0], ECodes.E_BUSSY)
    }
    this.sendToPlc(ret)
  }
  tracer.addMessage('<- Data from PLC received ' + timeTracker.getElapsedTime())
}

AS200.prototype.onDataFromMasterReceived = function (channel, data) {
  timeTracker.restart()
  tracer.clear()
  data = data.toUpperCase()

  tracer.addMessage('Data from Master received on channel ' + channel + ' -> ' + data)
  if (this.currentState === States.WAITING_FOR_NEW_COMMAND) {
    this.currentCommand = getCommandObject(this.commands, 1, data)
    if (typeof this.currentCommand !== 'object') {
      // Unknow command / wrong number of arguments / ....
      let ret = InSightFunctions.fnStringf('%s,%d', data.split(',')[0], this.currentCommand)
      this.sendToMaster(ret)
      this.currentCommand = null
    } else {
      // valid command
      InSightFunctions.fnSetCellValue('Internal.Command', this.currentCommand._splittedCmd[0])
      this.runStateMachine()
    }
  } else {
    // State machine is not waiting for a new command -> bussy
    let ret = InSightFunctions.fnStringf('%s,%d', data.split(',')[0], ECodes.E_BUSSY)
    this.sendToMaster(ret)
  }
}
AS200.prototype.onDataFromSlaveReceived = function (channel, timeout, data) {
  tracer.addMessage('-> Data from Slave received on channel ' + channel + ' -> ' + data)

  let result = new Result()
  result.isNeeded = 1
  result.isValid = 1

  if (timeout != 1) {
    tracer.addMessage('data.length ' + data.length)
    if (data.length > 2) {
      result = JSON.parse(data)
    } else {
      result.state = ECodes.E_UNSPECIFIED
    }
  } else {
    result.state = ECodes.E_TIMEOUT
  }

  this.currentCommand._results[channel] = result

  InSightFunctions.fnSetCellValue('Communication.EnableTimer', 0)
  InSightFunctions.fnUpdateGui(1)
  if (this.currentState == States.WAITING_FOR_SLAVE_RESULT) {
    this.runStateMachine()
  }

  tracer.addMessage('<- Data from Slave received ' + timeTracker.getElapsedTime())
}
AS200.prototype.onResetToFactorySettings = function () {
  myLog('-> Reset to factory settings')

  for (let f in g_TrainedFeatures) {
    g_TrainedFeatures[f].splice(MAX_GRIPPERS)
    for (let g = 0; g < MAX_GRIPPERS; g++) {
      g_TrainedFeatures[f][g] = new Feature(0, 0, 0, 0)
    }
  }

  for (let f in g_TrainedRobotPoses) {
    // g_TrainedRobotPoses[f] = []
    g_TrainedRobotPoses[f].splice(MAX_GRIPPERS)
    for (let g = 0; g < MAX_GRIPPERS; g++) {
      g_TrainedRobotPoses[f][g] = new RobotPose(0, 0, 0, 0, 0, 0, 0)
    }
  }

  myLog(g_TrainedFeatures)
  myLog(g_TrainedRobotPoses)
  myLog('<- Reset to factory settings')
}

AS200.prototype.onFeatureTypeChanged = function (channel) {
  myLog('-> Feature Type changed')
  let features = g_LooukupTables.features
  for (let f in features) {
    if ((features[f]['CameraID'] == this._myIndex) && (features[f]['CamFeatureID'] == channel)) {
      for (let g = 0; g < MAX_GRIPPERS; g++) {
        g_TrainedFeatures[f][g].valid = 0
        g_TrainedFeatures[f][g].x = 0
        g_TrainedFeatures[f][g].y = 0
        g_TrainedFeatures[f][g].angleInDegrees = 0
      }
    }
  }
  myLog('<- Feature Type changed')
}
AS200.prototype.onReloadSensorConfig = function () {
  myLog('-> Reload SensorConfig file')
  this.loadSensorConfig()
  myLog('<- Reload SensorConfig file')
}
AS200.prototype.onReloadCameraConfig = function () {
  myLog('-> Reload CameraConfig')
  LoadCameraConfigFile()
  myLog('<- Reload CameraConfig')
}

AS200.prototype.sendToPlc = function (resultStr) {
  tracer.addMessage('-> Send to PLC ' + timeTracker.getElapsedTime())
  let splitted = resultStr.split(',')
  myLogger.addLogMessage(5, '<- ' + resultStr)

  InSightFunctions.fnSetCellValue('Communication.TCPDevice.SendingToPLC', resultStr)
  InSightFunctions.fnSetEvent(82)
  InSightFunctions.fnSetEvent(86)
  //GSSVN: Check if Send XTT with XI command, select XTT type follow Tray direction 
  InSightFunctions.fnSetCellValue('Align.Tray.EnableAllToolDone', 0)
  tracer.addMessage('<- Send to PLC ' + timeTracker.getElapsedTime())

  let isSendXTT = InSightFunctions.fnGetCellValue('Align.Tray.IsSendXTT')
  //Decide XTT direction by XI return result
  let trayDir = parseInt(splitted[2])
  let XTTDir = 1
  /*
  if(trayDir == 0)
  {
    XTTDir = 1
  }
  else if(trayDir == 1)
  {
    XTTDir = 2
  }
  */
  if((isSendXTT == 1) && (splitted[0] == 'XI'))
  {
    InSightFunctions.fnSetCellValue('Align.Tray.XTTType', parseInt(XTTDir))
  }
}

AS200.prototype.sendToMaster = function (resultStr) {
  tracer.addMessage('-> Send to Master ' + timeTracker.getElapsedTime())
  myLogger.addLogMessage(0, '-> ' + resultStr)
  InSightFunctions.fnSetCellValue('Communication.TCPDevice.SendingToMaster', resultStr)
  InSightFunctions.fnSetEvent(81)
  InSightFunctions.fnSetEvent(86)
  tracer.addMessage('<- Send toMaster ' + timeTracker.getElapsedTime())
}

AS200.prototype.sendToSlaves = function (cmdString) {
  tracer.addMessage('-> Send command to slave ' + timeTracker.getElapsedTime())

  let slaveCommand = this.currentCommand.getSlaveCommandString()
  // myLogger.addLogMessage(0, "-> " + slaveCommand)
  InSightFunctions.fnSetCellValue('Communication.TCPDevice.SendingToSlave', slaveCommand)
  InSightFunctions.fnSetCellValue('Communication.EnableTimer', 1)
  InSightFunctions.fnSetEvent(81)

  tracer.addMessage('<- Send command to slave ' + timeTracker.getElapsedTime())
}

AS200.prototype.setAutoExposureState = function (state) {
  this.autoExposureState = state
}
AS200.prototype.setManualFeatureMask = function (feature_1, feature_2, inspection) {
  this.currentFeatureMask = feature_1 + ((!!feature_2) << 1) + (inspection << 8)
}

AS200.prototype.imageAcquired = function (feature_1, feature_2, inspection) {
  tracer.addMessage('-> Image acquired ' + timeTracker.getElapsedTime())

  let mask = 1
  g_Graphics.ShowCalibrationPoints_1 = false
  g_Graphics.ShowCalibrationPoints_2 = false

  g_Graphics.ShowCrossHair = null

  if (this.currentState == States.WAITING_FOR_IMAGE_ACQUIRED) {
    InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 0)

    if (this.autoExposureState == false) {
      this.runStateMachine()
      //this.currentFeatureMask = this.currentCommand._featureMask
      this.currentFeatureMask = this.currentCommand._enabledFeatures
      mask = this.currentFeatureMask
    } else {
      mask = 0
    }
  } else {
    this.setManualFeatureMask(feature_1, feature_2, inspection)
    mask = this.currentFeatureMask
  }
  tracer.addMessage('<- Image acquired ' + timeTracker.getElapsedTime())
  InSightFunctions.fnSetCellValue('Align.Tray.EnableAllToolDone', 1)
  return mask
}

AS200.prototype.addInspection = function (index) {
  let result = false
  if (!g_Inspections.hasOwnProperty(index)) {
    g_Inspections[index] = new Inspection(index)
    result = true
  }
  return result
}

AS200.prototype.removeInspection = function (index) {
  let result = false
  if (g_Inspections.hasOwnProperty(index)) {
    delete g_Inspections[index]
    result = true
  }
  return result
}

AS200.prototype.registerNewComputeTotalResult = function (index, userFunction) {
  if (!g_Inspections.hasOwnProperty(index)) {
    g_Inspections[index] = new Inspection(index)
  }

  g_Inspections[index].userComputeTotalResult = userFunction
}
AS200.prototype.onInspectionSettingsChanged = function (index, isCam1Used, isCam2Used, isPartMoving, shuttlingPos) {

}

AS200.prototype.onInspectionAcqSettingsChanged = function (index, exposure, mode, light1, light2, light3, light4) {
  myLog('-> onInspectionAcqSettingsChanged ' + JSON.stringify(arguments))

  // if (g_InspectionAcqSetting.hasOwnProperty(index)) {
  if (!g_Inspections.hasOwnProperty(index)) {
    this.addInspection(index)
  }

  g_Inspections[index].acqSettings.exposure = exposure
  g_Inspections[index].acqSettings.mode = mode
  g_Inspections[index].acqSettings.light1 = light1
  g_Inspections[index].acqSettings.light2 = light2
  g_Inspections[index].acqSettings.light3 = light3
  g_Inspections[index].acqSettings.light4 = light4

  setInspectionAcqSettings(index, true)
  /* } else {
    g_InspectionAcqSetting[index] = new InspectionAcqSettings(exposure, mode, light1, light2, light3, light4)
  } */
  myLog('<- onInspectionAcqSettingsChanged ')
}

AS200.prototype.onInspectionSelectionChanged = function (index, enabled) {
  myLog('-> onInspectionSelectionChanged ' + JSON.stringify(arguments))
  setInspectionAcqSettings(index, enabled)
  myLog('<- onInspectionSelectionChanged')
}
AS200.prototype.onInspectionToolsDone = function (index, state, data) {
  tracer.addMessage('-> Inspection Tools done ' + timeTracker.getElapsedTime() + ' <State= ' + this.currentState + '>')
  if (this.currentCommand != null) {
    this.currentCommand._results[this.currentCommand._myIndex].isValid = 1
    this.currentCommand._results[this.currentCommand._myIndex].state = state
    this.currentCommand._results[this.currentCommand._myIndex].data.push(data)

    this.runStateMachine()
  }
  tracer.addMessage('<- Inspection Tools done ' + timeTracker.getElapsedTime())
}
AS200.prototype.inspectionGetTransformed = function(shuttlingPoseIndex, partIsMoving, featureX,featureY,featureAngle, featureValid, robX, robY, robTheta) {
  let feature = new Feature(featureX, featureY, featureAngle, featureValid)
  let robot = new RobotPose(robX, robY, 0, robTheta, 0, 0, 1)
  let isCameraMoving=false
  if (g_Calibrations[shuttlingPoseIndex].calibration !== null) {
    isCameraMoving = g_Calibrations[shuttlingPoseIndex].calibration.isCameraMoving_
  }

  let res = getTransformed(g_Calibrations, shuttlingPoseIndex, isCameraMoving, partIsMoving, feature, robot)

  return res
}

AS200.prototype.toolsDone = function (enable_1, trained_1, valid_1, x_1, y_1, angle_1, enable_2, trained_2, valid_2, x_2, y_2, angle_2,number_found,errorCode) {
  //tracer.addMessage('-> Tools done ' + timeTracker.getElapsedTime() + ' <State= ' + this.currentState + '>')
  //myLogger.addLogMessage(0,'-> Tools done ' + timeTracker.getElapsedTime() + ' <1= ' + x_1 + ',' + y_1 + ',' + angle_1 + ',' + enable_1 + ',2= ' + x_2 + ',' + y_2 + ',' + angle_2+ ',' + enable_2 + ',><'+number_found+'>')
  g_NumberFound = 0
  g_AdditionalError = 0
  if (enable_1 > 0) {
    g_CurrentFeatures[1] = new Feature(x_1, y_1, angle_1, valid_1)
    if (valid_1 <= 0) {
      if (trained_1 == 0) {
        g_CurrentFeatures[1].valid = ECodes.E_FEATURE_NOT_TRAINED
      } else {
        g_CurrentFeatures[1].valid = ECodes.E_FEATURE_NOT_FOUND
      }
    }
  } else {
    g_CurrentFeatures[1] = new Feature(0, 0, 0, 0)
  }

  if (enable_2 > 0) {
    g_CurrentFeatures[2] = new Feature(x_2, y_2, angle_2, valid_2)
    if (valid_2 <= 0) {
      if (trained_2 == 0) {
        g_CurrentFeatures[2].valid = ECodes.E_FEATURE_NOT_TRAINED
      } else {
        g_CurrentFeatures[2].valid = ECodes.E_FEATURE_NOT_FOUND
      }
    }
  } else {
    g_CurrentFeatures[2] = new Feature(0, 0, 0, 0)
  }
  //GSSVN: Read pattern/object found number, send along with command XTF, XTR, XTA, XAF, XAR, XAA
  g_NumberFound = number_found
  g_AdditionalError = errorCode
  this.runStateMachine()
  tracer.addMessage('<- Tools done ' + timeTracker.getElapsedTime())
}
/*
AS200.prototype.getEnabledFeatures = function () {
  let ret = 0

  if (this.currentCommand != null) {
    ret = this.currentCommand.getEnabledFeatures()
  }
  return ret
}
*/
AS200.prototype.writeCalibrationInfosToSheet = function () {
  if (this.myCalibrations.calibrations['1'] != null) {
    let calib = this.myCalibrations.calibrations['1']
    InSightFunctions.fnSetCellValue('HECalibration.Valid', calib.runstatus)

    if (calib.runstatus == 1) {
      let trans = new cogMath.cc2XformLinear()
      if (calib['calibration']['isCameraMoving_'] == true) {
        trans.setXform(calib.results.Transforms.Stage2DFromImage2D.xform)
      } else {
        trans.setXform(calib.results.Transforms.Home2DFromImage2D.xform)
      }
      let xScale = trans.xScale()
      let yScale = trans.yScale()

      let diagnostics = calib.results.Diagnostics
      InSightFunctions.fnSetCellValue('HECalibration.MaxImage2D', diagnostics['OverallResidualsImage2D']['Max'])
      InSightFunctions.fnSetCellValue('HECalibration.RMSImage2D', diagnostics['OverallResidualsImage2D']['Rms'])
      InSightFunctions.fnSetCellValue('HECalibration.MaxHome2D', diagnostics['OverallResidualsHome2D']['Max'])
      InSightFunctions.fnSetCellValue('HECalibration.RMSHome2D', diagnostics['OverallResidualsHome2D']['Rms'])
      InSightFunctions.fnSetCellValue('HECalibration.PixelSizeX', xScale)
      InSightFunctions.fnSetCellValue('HECalibration.PixelSizeY', yScale)
    }
  } else {
    InSightFunctions.fnSetCellValue('HECalibration.Valid', 0)
    InSightFunctions.fnSetCellValue('HECalibration.MaxImage2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.RMSImage2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.MaxHome2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.RMSHome2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.PixelSizeX', 0)
    InSightFunctions.fnSetCellValue('HECalibration.PixelSizeY', 0)
  }
}

AS200.prototype.loadSensorConfig = function () {
  myLog('-> Load SensorConfig')
  // Try to load the SensorConfiguration
  this.mySensor = new Sensor()
  let sensorConfigFile = cogUtils.loadFile(SENSORCONFIG_FILENAME)

  if (Object.keys(sensorConfigFile).length > 0) {
    this.mySensorConfiguration = new SensorConfiguration()
    this.mySensorConfiguration.init(sensorConfigFile)

    // this.mySensor = new Sensor(this.mySensorConfiguration);
    this.mySensor.initFromSensorConfig(this.mySensorConfiguration)
    // this.mySensor.writeToSheet();

    this.myRecipes = new Recipes(this.mySensorConfiguration)

    this.myDetails = new MyDetails()
    this.myDetails.myIndex = this.mySensor.findIndexByIp(this.myDetails.myIP)
    this.myDetails.iAmMaster = 1

    let usedCameras = this.myRecipes.recipes[(this.myDetails.jobName).toLowerCase()]['UsedCameras']
    this.myRecipeTables = new RecipeTables(this.myDetails.myIndex, usedCameras)
    this.myRecipeTables.readFromSheet()
    g_LooukupTables = this.myRecipeTables
    this.commands = masterCommands

    this.runStateMachine = this.runMasterStateMachine
  } else {
    this.myDetails = new MyDetails()
    this.myDetails.myIndex = 0
    this.myDetails.iAmMaster = 0
    this.runStateMachine = this.runSlaveStateMachine
    g_TrainedFeatures = {}
    for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
      if ((g_StoredTrainedFeatures != null) && (g_StoredTrainedFeatures.hasOwnProperty(f))) {
        g_TrainedFeatures[f] = g_StoredTrainedFeatures[f]
      } else {
        g_TrainedFeatures[f] = []
        for (let g = 0; g < MAX_GRIPPERS; g++) {
          g_TrainedFeatures[f][g] = new Feature(0, 0, 0, 0)
        }
      }
    }

    this.commands = slaveCommands
  }
  this.commands.myIndex = this.myDetails.myIndex

  this.myCalibrations = new Calibrations()
  this.myCalibrations.loadCalibrationsFromFile()

  g_Calibrations = this.myCalibrations.calibrations

  InSightFunctions.fnSetCellValue('HECalibration.Selector', 0)
  this.mySensor.writeToSheet()
  this.myDetails.writeToSheet()
  this.initTCPDevices()
  myLog('<- Load SensorConfig')
}

//* ***************************************************************************/

AS200.prototype.getCamerIDFromFeatureID = function (featureID) {
  return this.myRecipeTables.features[featureID]['CameraID']
}
AS200.prototype.getCamerFeatureIDFromFeatureID = function (featureID) {
  return this.myRecipeTables.features[featureID]['CamFeatureID']
}

AS200.prototype.writeLogToSheet = function () {
  myLogger.writeLogToSheet()
}

AS200.prototype.onPrintCalibrations = function () {
  myLog('-> g_Calibrations')
  myLog(g_Calibrations)
  myLog('<- g_Calibrations')
}
AS200.prototype.onPrintRuntimeFeatures = function () {
  myLog('-> g_RuntimeFeatures')
  myLog(g_RuntimeFeatures)
  myLog('<- g_RuntimeFeatures')
}
AS200.prototype.onPrintTrainedFeatures = function () {
  myLog('-> g_TrainedFeatures')
  myLog(g_TrainedFeatures)
  myLog('<- g_TrainedFeatures')
}
AS200.prototype.onPrintTrainedRobotPoses = function () {
  myLog('-> g_TrainedRobotPoses')
  myLog(g_TrainedRobotPoses)
  myLog('<- g_TrainedRobotPoses')
}

AS200.prototype.onPrintCurrentFeatures = function () {
  myLog('-> g_CurrentFeatures')
  myLog(g_CurrentFeatures)
  myLog('<- g_CurrentFeatures')
}
AS200.prototype.onPrintParts = function () {
  myLog('-> g_Parts')
  myLog(g_Parts)
  myLog('<- g_Parts')
}
AS200.prototype.onPrintFeaturesInfos = function () {
  myLog('-> g_FeaturesInfos')
  myLog(g_FeaturesInfos)
  myLog('<- g_FeaturesInfos')
}
AS200.prototype.onPrintStepsByID = function () {
  myLog('-> g_StepsByID')
  myLog(g_StepsByID)
  myLog('<- g_StepsByID')
}
AS200.prototype.onPrintRecipeTables = function () {
  myLog('-> g_StepsByID')
  myLog(this.myRecipeTables)
  myLog('<- g_StepsByID')
}
AS200.prototype.onPrintTrace = function () {
  myLog('-> Trace')
  tracer.print()
  myLog('<- Trace')
}

AS200.prototype.runMasterStateMachine = function () {
  tracer.addMessage('-> State machine ' + timeTracker.getElapsedTime())
  tracer.addMessage('-> State = ' + this.currentState)
  //myLogger.addLogMessage(0,'Current State: ' + this.currentState)

  switch (this.currentState) {
    case States.WAITING_FOR_NEW_COMMAND:
      if (this.currentCommand != null) {
        if ((this.currentCommand._sendToSlave > 0) && (this.currentCommand._onlyForMaster == 0)) {
          this.sendToSlaves('cmdString')
        }

        let index = this.currentCommand.getIndex()

        if (this.currentCommand.getUseAsStepID() == 1) {
          for (let i = 1; i <= MAX_CAMERAS; i++) {
            this.currentCommand._results[i].isNeeded = g_LooukupTables.stepLookup[index]['Cam_' + i].Enabled && (!this.currentCommand._onlyForMaster)
            this.currentCommand._results[i].isValid = 0
          }
        } else if (this.currentCommand.getUseAsPartID() == 1) {
          for (let i = 1; i <= MAX_CAMERAS; i++) {
            this.currentCommand._results[i].isNeeded = g_LooukupTables.partLookup[this.currentCommand._partID1]['Cam_' + i].Enabled && (!this.currentCommand._onlyForMaster)
            this.currentCommand._results[i].isValid = 0
          }
        } /* else if (this.currentCommand.getUseAsInspectionID() == 1) {
          for (let i = 1; i <= MAX_CAMERAS; i++) {
            this.currentCommand._results[i].isNeeded = (this.myDetails.myIndex == i ? 1 : 0)
            this.currentCommand._results[i].isValid = 0
          }
        } */else {
          if ((g_LooukupTables.cameraLookup[index]['SendToSlave'] == 1) || (index == this.myDetails.myIndex) || (index == 0)) {
            for (let i = 1; i <= MAX_CAMERAS; i++) {
              if (((index == 0) && (g_LooukupTables.cameraLookup[i]['SendToSlave'] == 1)) || ((index == 0) && (i == this.myDetails.myIndex)) || (index == i)) {
                this.currentCommand._results[i].isNeeded = 1
              }
              this.currentCommand._results[i].isValid = 0
            }
          } else {
            this.currentCommand._results.error = ECodes.E_INDEX_OUT_OF_RANGE
          }
        }

        if ((this.currentCommand._results[this.myDetails.myIndex].isNeeded == 1) || (this.currentCommand._onlyForMaster > 0)) {
          //myLogger.addLogMessage(0, "Run execute")
          this.currentState = this.currentCommand.execute(this)
        } else {
          this.currentState = States.WAITING_FOR_SLAVE_RESULT
        }

        if (this.currentCommand._results.error == ECodes.E_NO_ERROR) {
          if (this.currentState == States.WAITING_FOR_SLAVE_RESULT) {
            let neededCnt = 0
            let validCnt = 0

            for (let i = 1; i <= MAX_CAMERAS; i++) {
              if (this.currentCommand._results[i].isNeeded > 0) {
                neededCnt++
                if (this.currentCommand._results[i].isValid > 0) {
                  validCnt++
                }
              }
            }
            if (neededCnt == validCnt) {
              let resStrPlc = this.currentCommand.computeTotalResult()
              this.sendToPlc(resStrPlc)

              this.currentCommand = null
              this.currentState = States.WAITING_FOR_NEW_COMMAND
            }
          }
        } else {
          let plcString = this.currentCommand._splittedCmd[0] + ',' + this.currentCommand._results.error
          this.sendToPlc(plcString)
          this.currentCommand = null
          this.currentState = States.WAITING_FOR_NEW_COMMAND
        }
      } else {
        tracer.addMessage('WAITING_FOR_NEW_COMMAND <no valid command>!')
      }
      break

    case States.WAITING_FOR_IMAGE_ACQUIRED:
      if (this.currentCommand != null) {
        this.currentState = this.currentCommand.imgAcquired(this)

        if (this.currentCommand._results.error != ECodes.E_NO_ERROR) {
          let plcString = this.currentCommand._splittedCmd[0] + ',' + this.currentCommand._results.error
          this.sendToPlc(plcString)

          this.currentCommand = null
          this.currentState = States.WAITING_FOR_NEW_COMMAND
        }
      }
      break

    case States.WAITING_FOR_TOOLS_DONE:
      if (this.currentCommand != null) {
      
        this.currentState = this.currentCommand.toolsDone(this)

        if (this.currentCommand._results.error == ECodes.E_NO_ERROR) {
          if (this.currentState == States.WAITING_FOR_SLAVE_RESULT) {
            let neededCnt = 0
            let validCnt = 0

            for (var i = 1; i <= MAX_CAMERAS; i++) {
              if (this.currentCommand._results[i].isNeeded > 0) {
                neededCnt++
                if (this.currentCommand._results[i].isValid > 0) {
                  validCnt++
                }
              }
            }

            if (neededCnt == validCnt) {
              let resStrPlc = this.currentCommand.computeTotalResult()
              tracer.addMessage(resStrPlc)
              this.sendToPlc(resStrPlc)
              this.currentCommand = null
              this.currentState = States.WAITING_FOR_NEW_COMMAND
            }
          }
        } else {
          let plcString = this.currentCommand._splittedCmd[0] + ',' + this.currentCommand._results.error
          this.sendToPlc(plcString)
          this.currentCommand = null
          this.currentState = States.WAITING_FOR_NEW_COMMAND
        }
      }
      break

    case States.WAITING_FOR_SLAVE_RESULT:
      if (this.currentCommand != null) {
        //myLogger.addLogMessage(0, "Waiting for slave")
        let resStrPlc = this.currentCommand.computeTotalResult()
        tracer.addMessage(resStrPlc)

        this.sendToPlc(resStrPlc)

        this.currentCommand = null
        this.currentState = States.WAITING_FOR_NEW_COMMAND
      }
      break
    default:
  }
  /*
  if (this.currentState == States.WAITING_FOR_NEW_COMMAND) {
    // Start Looger
    InSightFunctions.fnSetEvent(86)
    // myLogger.writeLogToSheet();
  }
*/
  tracer.addMessage('<- State = ' + this.currentState)
  tracer.addMessage('<- State machine ' + timeTracker.getElapsedTime())
}

AS200.prototype.runSlaveStateMachine = function () {
  tracer.addMessage('-> Slave State machine ' + timeTracker.getElapsedTime())
  tracer.addMessage('-> State = ' + this.currentState)

  switch (this.currentState) {
    case States.WAITING_FOR_NEW_COMMAND:
      if (this.currentCommand != null) {
        this.currentState = this.currentCommand.execute(this)

        if (this.currentCommand._results.error == ECodes.E_NO_ERROR) {
          if (this.currentState == States.WAITING_FOR_NEW_COMMAND) {
            // let slaveString = this.currentCommand._splittedCmd[0] + "," + JSON.stringify(this.currentCommand._results["1"]);
            let slaveString = JSON.stringify(this.currentCommand._results['1'])
            this.sendToMaster(slaveString)
            this.currentCommand = null
            this.currentState = States.WAITING_FOR_NEW_COMMAND
          }
        } else {
          // let slaveString = this.currentCommand._splittedCmd[0] + "," + this.currentCommand._results.error;
          let slaveString = JSON.stringify(this.currentCommand._results['1'])
          this.sendToMaster(slaveString)
          this.currentCommand = null
          this.currentState = States.WAITING_FOR_NEW_COMMAND
        }
      }
      break

    case States.WAITING_FOR_IMAGE_ACQUIRED:
      if (this.currentCommand != null) {
        this.currentState = this.currentCommand.imgAcquired(this)
      }
      break

    case States.WAITING_FOR_TOOLS_DONE:
      if (this.currentCommand != null) {
        this.currentState = this.currentCommand.toolsDone(this)
        let resString = JSON.stringify(this.currentCommand._results['1'])

        this.sendToMaster(resString)
        this.currentCommand = null
        this.currentState = States.WAITING_FOR_NEW_COMMAND
      }
      break
    default:
  }

  if (this.currentState == States.WAITING_FOR_NEW_COMMAND) {
    // Start Looger
    InSightFunctions.fnSetEvent(86)
    // myLogger.writeLogToSheet();
  }

  tracer.addMessage('<- Slave State = ' + this.currentState)

  tracer.addMessage('<- Slave State machine ' + timeTracker.getElapsedTime())
}

/*
AS200.prototype.draw = function (gr) {
    tracer.addMessage("AS200 Script: -> Draw");
    //gr.plotString("Total = " + this.total.toString(), 100, 200, 0xFF0000);
    //gr.plotShape(this.pattern[0], "asdf", 0xc0ffee*Math.random());
}
*/

function InspectionAcqSettings (exposure, mode, light1, light2, light3, light4) {
  this.exposure = exposure
  this.mode = mode
  this.light1 = light1
  this.light2 = light2
  this.light3 = light3
  this.light4 = light4
}

function Inspection (index) {
  this.acqSettings = new InspectionAcqSettings(0.1, 0, 50, 0, 0, 0)
  this.userComputeTotalResult = null
}


//* ***************************************************************************/
// Commands MASTER
//* ***************************************************************************/
function CmdInfos () {
  this.featureMask = 0 // As Bit Array
  this.exposureSet = 1
  this.shuttlingPose = 1
  this.isCameraMoving = 0
  this.isPartMoving = 0
}

function CommandBase (myIndex, cmdString) {
  this._myIndex = myIndex
  this._cmdString = cmdString
  this._splittedCmd = cmdString.split(',')

  this._hasIndex = 0
  this._isCameraMoving = 0
  this._useAsStepID = 0
  this._useAsPartID = 0
  this._numArguments = -1
  this._slmpCode = -1
  this._onlyForMaster = 0
  this._validCMD = 0

  this._index = -1
  this._sendToSlave = 0
  this._alignMode = 1
  this._enabledFeatures = 0
  this._featureMask = 0
  this._exposureSet = 1
  this._shuttlingPose = 1
  this._isPartMoving = 0
  this._logImageType = ''
  this._partID1 = 0 // should be the same as _index
  this._partID2 = 0
  this._gripperID1 = 0
  this._computeBothParts = 0

  this._slaveCmdInfos = new CmdInfos()

  this._robotPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

  this._results = {}
  this._results.error = ECodes.E_NO_ERROR
  // this._results.overAllState = 0

  for (let i = 1; i <= MAX_CAMERAS; i++) {
    this._results[i] = new Result()
  }
};

CommandBase.prototype.isValid = function () {
  return this._validCMD
}
CommandBase.prototype.hasIndex = function () {
  return this._hasIndex
}
CommandBase.prototype.getIndex = function () {
  return this._index
}
CommandBase.prototype.getSLMPCode = function () {
  return this._slmpCode
}
CommandBase.prototype.getNumArgs = function () {
  return this._numArguments
}
CommandBase.prototype.getUseAsStepID = function () {
  return this._useAsStepID
}
CommandBase.prototype.getUseAsPartID = function () {
  return this._useAsPartID
}
CommandBase.prototype.getUseAsInspectionID = function () {
  return this._useAsInspectionID
}
CommandBase.prototype.isFeatureEnabled = function (featureID) {
  //return !!(this._featureMask & (1 << (featureID - 1)))
  return !!(this._enabledFeatures & (1 << (featureID - 1)))
}
/*
CommandBase.prototype.getEnabledFeatures = function () {
  return this._enabledFeatures
} */

CommandBase.prototype.execute = function (t) {
  tracer.addMessage('-> Execute ' + timeTracker.getElapsedTime())

  // TODO: Check the _index

  if (this._useAsStepID > 0) {
    if (g_LooukupTables.stepLookup[this._index]['Cam_' + this._myIndex]['Enabled'] == 0) {
      tracer.addMessage('------------------------------------ Error ')
    }
  }

  if (this._useAsPartID > 0) {
    if (g_LooukupTables.partLookup[this._partID1]['Cam_' + this._myIndex]['Enabled'] == 0) {
      tracer.addMessage('------------------------------------ Error ')
    }
  }
  this._enabledFeatures = 0

  InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', this._exposureSet - 1)

  if (this._logImageType.length > 2) {
    InSightFunctions.fnSetCellValue(this._logImageType, 1)
  }

  if (t.triggerMode == 32) {
    InSightFunctions.fnSetEvent(32)
  }

  tracer.addMessage('<- Execute ' + timeTracker.getElapsedTime())
  return States.WAITING_FOR_IMAGE_ACQUIRED
}
CommandBase.prototype.imgAcquired = function () {
  tracer.addMessage('-> Image acquired (CMD)' + timeTracker.getElapsedTime())

  this._enabledFeatures = this._featureMask
  /* for (let i = 1; i <= MAX_FEATURES_PER_CAMERA; i++) {
        if (this.isFeatureEnabled(i) == true) {
            InSightFunctions.fnSetCellValue("Target." + i + ".Enable", 1);
        }
    } */
  tracer.addMessage('<- Image acquired (CMD)' + timeTracker.getElapsedTime())
 
  return States.WAITING_FOR_TOOLS_DONE
}
CommandBase.prototype.toolsDone = function (t) {
  tracer.addMessage('-> Tools done ' + timeTracker.getElapsedTime())
  if (this._logImageType.length > 2) {
    InSightFunctions.fnSetCellValue(this._logImageType, 0)
  }

  this._results[this._myIndex]['isValid'] = 1
  for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
    let transformed = new Feature(0, 0, 0, 0)
    if (this.isFeatureEnabled(f) == true) {
      if (g_CurrentFeatures[f].valid > 0) {
        transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
        this._results[this._myIndex].state = transformed.valid
      } else {
        this._results[this._myIndex].state = g_CurrentFeatures[f].valid
      }
      this._results[this._myIndex].data.push(transformed)
    }
  }
  tracer.addMessage('<- Tools done ' + timeTracker.getElapsedTime())

  return States.WAITING_FOR_SLAVE_RESULT
}
CommandBase.prototype.computeTotalResult = function () {
  tracer.addMessage('-> Compute result ' + timeTracker.getElapsedTime())
  let plcString = ''
  tracer.addMessage(this._results)
  this.checkResultsState()

  let retState = ECodes.E_UNSPECIFIED
  if (this._results.error == ECodes.E_NO_ERROR) {
    retState = 1
  } else {
    retState = this._results.error
  }

  plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)

  tracer.addMessage('<- Compute result ' + timeTracker.getElapsedTime())
  return plcString
}
CommandBase.prototype.getSlaveCommandString = function () {
  tracer.addMessage('-> Get slave command string (Base)')

  let slaveCommand = InSightFunctions.fnStringf('%s,%d,%d,%d,%d,%d',
    this._splittedCmd[0],
    this._slaveCmdInfos.featureMask,
    this._slaveCmdInfos.exposureSet,
    this._slaveCmdInfos.shuttlingPose,
    this._slaveCmdInfos.isCameraMoving,
    this._slaveCmdInfos.isPartMoving
  )

  let commandData = this.getCommandData()
  if (commandData.length > 0) {
    slaveCommand = slaveCommand + ',' + commandData
  }
  tracer.addMessage('<- Get slave command string (Base)')
  return slaveCommand
}
CommandBase.prototype.getCommandData = function () {
  let end = this._splittedCmd.length
  let start = this._hasIndex + 1
  let data = this.copyCommandData(start, end)
  return data
}
CommandBase.prototype.copyCommandData = function (start, end) {
  let data = ''
  for (let i = start; i < end; i++) {
    data = data + this._splittedCmd[i] + ','
  }
  data = data.slice(0, data.length - 1)
  return data
}

CommandBase.prototype.checkLengthAndSetIndex = function (minIndex, maxIndex) {
  // Check length and extract the index
  let resState = false
  if (this._splittedCmd.length >= (this._numArguments + this._hasIndex + 1)) {
    if (this._hasIndex) {
      if (!isNaN(this._splittedCmd[1])) {
        let index = parseInt(this._splittedCmd[1])
        this._index = index & MASK_CAM_ID
        // this._isCameraMoving = (index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA

        this._partID1 = (index & MASK_PART_ID_1) >> SHIFT_PART_ID_1
        this._partID2 = (index & MASK_PART_ID_2) >> SHIFT_PART_ID_2
        this._gripperID1 = (index & MASK_GRIPPER_ID_1) >> SHIFT_GRIPPER_ID_1

        // if ((this._index >= minIndex) && (this._index <= maxIndex)) {
        resState = true
        // } else {
        //  this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
        // }
      } else {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
      }
    } else {
      resState = true
    }
  } else {
    this._validCMD = ECodes.E_TOO_FEW_ARGUMENTS // Wrong number of arguments
  }
  return resState
}
CommandBase.prototype.checkArgumentsNumbers = function (startIndex, endIndex) {
  for (let i = startIndex; i <= endIndex; i++) {
    if (isNaN(this._splittedCmd[i])) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return false
    }
  }
  return true
}
CommandBase.prototype.copyRobotPose = function (start) {
  this._robotPose.x = parseFloat(this._splittedCmd[start])
  this._robotPose.y = parseFloat(this._splittedCmd[start + 1])
  this._robotPose.z = parseFloat(this._splittedCmd[start + 2])
  this._robotPose.thetaZ = parseFloat(this._splittedCmd[start + 3])
  this._robotPose.thetaY = parseFloat(this._splittedCmd[start + 4])
  this._robotPose.thetaX = parseFloat(this._splittedCmd[start + 5])
  this._robotPose.valid = 1
}

CommandBase.prototype.fillCmdInfoWithPartID = function () {
  // let info = g_LooukupTables.partLookup[this._index]
  if (!g_LooukupTables.partLookup.hasOwnProperty(this._partID1)) {
    this._validCMD = ECodes.E_INVALID_ARGUMENT
    return false
  }

  let index = parseInt(this._splittedCmd[1])
  let isCameraMoving = (index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA

  let info = g_LooukupTables.partLookup[this._partID1]
  let info2 = info

  if((this._partID2 > 0) && (this._computeBothParts == 1)){
    info2 = g_LooukupTables.partLookup[this._partID2]
  }

  for (let c = 1; c <= MAX_CAMERAS; c++) {
    if (c == this._myIndex) {
      this._featureMask = info['Cam_' + c]['FeatureMask']
      this._exposureSet = info['Cam_' + c]['ExpSetting']
      this._shuttlingPose = info['Cam_' + c]['ShuttlePose']
      this._isPartMoving = info['Cam_' + c]['IsMoving']
      if(this._computeBothParts){
        this._featureMask = this._featureMask | (info2['Cam_' + c]['FeatureMask'] << SHIFT_ID_2)
        this._shuttlingPose = this._shuttlingPose | (info2['Cam_' + c]['ShuttlePose'] << SHIFT_ID_2)
        this._isPartMoving = this._isPartMoving | (info2['Cam_' + c]['IsMoving'] << SHIFT_ID_2)
      }
    } else {
      this._slaveCmdInfos.featureMask = info['Cam_' + c]['FeatureMask']
      this._slaveCmdInfos.exposureSet = info['Cam_' + c]['ExpSetting']
      this._slaveCmdInfos.shuttlingPose = info['Cam_' + c]['ShuttlePose']
      this._slaveCmdInfos.isPartMoving = info['Cam_' + c]['IsMoving']
      if(this._computeBothParts){
        this._slaveCmdInfos.featureMask = this._slaveCmdInfos.featureMask | (info2['Cam_' + c]['FeatureMask'] << SHIFT_ID_2)
        this._slaveCmdInfos.shuttlingPose= this._slaveCmdInfos.shuttlingPose | (info2['Cam_' + c]['ShuttlePose'] << SHIFT_ID_2)
        this._slaveCmdInfos.isPartMoving = this._slaveCmdInfos.isPartMoving | (info2['Cam_' + c]['IsMoving'] << SHIFT_ID_2)
      }
      
      if (c === this._partID1) {
        this._slaveCmdInfos.isCameraMoving = isCameraMoving
      } else {
        this._slaveCmdInfos.isCameraMoving = 0
      }
    }
  }

  this._sendToSlave = info['SendToSlave']
  return true
}
CommandBase.prototype.fillCmdInfoWithStepID = function () {
  if (!g_LooukupTables.stepLookup.hasOwnProperty(this._index)) {
    this._validCMD = ECodes.E_INVALID_ARGUMENT
    return false
  }

  let index = parseInt(this._splittedCmd[1])
  let isCameraMoving = (index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA

  let info = g_LooukupTables.stepLookup[this._index]

  for (let c = 1; c <= MAX_CAMERAS; c++) {
    if (c == this._myIndex) {
      this._featureMask = info['Cam_' + c]['FeatureMask']
      this._exposureSet = info['Cam_' + c]['ExpSetting']
      this._shuttlingPose = info['Cam_' + c]['ShuttlePose']
      this._isPartMoving = info['Cam_' + c]['IsMoving']
    } else {
      this._slaveCmdInfos.featureMask = info['Cam_' + c]['FeatureMask']
      this._slaveCmdInfos.exposureSet = info['Cam_' + c]['ExpSetting']
      this._slaveCmdInfos.shuttlingPose = info['Cam_' + c]['ShuttlePose']
      this._slaveCmdInfos.isPartMoving = info['Cam_' + c]['IsMoving']
      if (c === this._partID1) {
        this._slaveCmdInfos.isCameraMoving = isCameraMoving
      } else {
        this._slaveCmdInfos.isCameraMoving = 0
      }
    }
  }

  this._sendToSlave = info['SendToSlave']
  return true
}
CommandBase.prototype.fillCmdInfoWithCameraID = function (featureMask, expSetting, shuttlingPose, isPartMoving) {
  if (!g_LooukupTables.cameraLookup.hasOwnProperty(this._index)) {
    this._validCMD = ECodes.E_INVALID_ARGUMENT
    return false
  }

  let index = parseInt(this._splittedCmd[1])
  let isCameraMoving = (index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA

  let info = g_LooukupTables.cameraLookup[this._index]

  for (let c = 1; c <= MAX_CAMERAS; c++) {
    if (c == this._myIndex) {
      this._featureMask = featureMask
      this._exposureSet = expSetting
      this._shuttlingPose = shuttlingPose
      this._isPartMoving = isPartMoving
    } else {
      this._slaveCmdInfos.featureMask = featureMask
      this._slaveCmdInfos.exposureSet = expSetting
      this._slaveCmdInfos.shuttlingPose = shuttlingPose
      this._slaveCmdInfos.isPartMoving = isPartMoving
      if (c === this._partID1) {
        this._slaveCmdInfos.isCameraMoving = isCameraMoving
      } else {
        this._slaveCmdInfos.isCameraMoving = 0
      }
    }
  }
  this._sendToSlave = info['SendToSlave']

  return true
}
CommandBase.prototype.getAllResultData = function () {
  let resultStr = ''
  for (var c = 1; c <= MAX_CAMERAS; c++) {
    if (this._results[c].isNeeded > 0) {
      if (this._results[c].isValid > 0) {
        if (this._results[c].state > 0) {
          if (this._results[c].hasOwnProperty('data')) {
            let dat = this._results[c].data
            for (let a = 0; a < dat.length; a++) {
              for (let d in dat[a]) {
                resultStr = resultStr + ',' + dat[a][d]
              }
            }
          }
        }
      }
    }
  }
  return resultStr
}
CommandBase.prototype.checkResultsState = function () {
  for (var c = 1; c <= MAX_CAMERAS; c++) {
    if (this._results[c].isNeeded > 0) {
      if (this._results[c].isValid > 0) {
        if (this._results[c].state <= 0) {
          this._results.error = this._results[c].state
        }
      }
    }
  }
}

/*
TM,<cam>,<hw trigger>
HEB,<cam>
HE,<cam>,<featureID>,<X>,<Y>,<Z>,<A>,<B>,<C>
HEE,<cam>
ACB,<cam>,<featureID>,<X>,<Y>,<Z>,<A>,<B>,<C>
AC,<cam>,<featureID>,<X>,<Y>,<Z>,<A>,<B>,<C>
CCB,<cam>,<SwapHandedness>,<FeatureOffsetX>,<FeatureOffsetY>
CC,<cam>,<featureID>,<X>,<Y>,<Z>,<A>,<B>,<C>
CCE,<cam>
TA,<partID>
XA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XAS,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
SGP,<stepID>,<coords>,<X>,<Y>,<Theta>
GGP,<stepID>,<coord>
GCP,<stepID>,<coord>,<X>,<Y>,<Z>,<A>,<B>,<C>
TT,<partID>,<X>,<Y>,<Z>,<A>,<B>,<C>
TTR,<partID>,<X>,<Y>,<Z>,<A>,<B>,<C>
XT,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XTS,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
LF,< stepID >,<ProductID>,<X>,<Y>,<Z>,<A>,<B>,<C>
TP,<partID>,<AlignMode>
TPR,<partID>,<AlignMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
GP,<partID>,<AlignMode>,<ResultMode>, [current motion pose]
XAF,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XAR,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XAA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XTT,<partID>,<resultMode>,<Pocket1>,<Pocket2>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XTF,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XTR,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
XTA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
*/

var masterCommands = {
  //  GS,<cam>
  //  GS,<Status>
  // SLMP: 1010
  GS: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 1010
    this._numArguments = 0
    this._hasIndex = 1

    if (this._splittedCmd.length < 2) {
      this._splittedCmd.push('0')
    }

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }
    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }

    this._validCMD = 1
    this.execute = function (t) {
      tracer.addMessage('-> GS: Execute ' + timeTracker.getElapsedTime())
      let major = InSightFunctions.fnGetCellValue('Version.Major')
      let minor = InSightFunctions.fnGetCellValue('Version.Minor')
      let subMinor = InSightFunctions.fnGetCellValue('Version.SubMinor')
      let jobName = InSightFunctions.fnGetCellValue('Internal.Recipe.JobName')

      this._results[this._myIndex].data.push({ 'Major': major, 'Minor': minor, 'SubMinor': subMinor, 'JobName': jobName })
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1
      tracer.addMessage('<- GS: Execute ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_SLAVE_RESULT
    }
    this.computeTotalResult = function () {
      tracer.addMessage('-> GS: Compute result ' + timeTracker.getElapsedTime())
      // TODO: //Prüfung ob alle Versionen gleich sind
      let plcString = ''
      let retState = 1
      let major = -1
      let minor = -1
      let subMinor = -1
      let jobName = ''

      this.checkResultsState()

      if (this._results.error == ECodes.E_NO_ERROR) {
        for (let c = 1; c <= MAX_CAMERAS; c++) {
          if (this._results[c].isNeeded > 0) {
            if (major == -1) {
              major = this._results[c].data[0].Major
              minor = this._results[c].data[0].Minor
              subMinor = this._results[c].data[0].SubMinor
            } else {
              let data = this._results[c].data[0]
              if ((major != data.Major) || (minor != data.Minor) || (subMinor != data.SubMinor)) {
                retState = ECodes.E_DIFFERENT_VERSIONS
              }
            }

            if (jobName.length == 0) {
              jobName = (this._results[c].data[0].JobName).toLowerCase()
            } else {
              if (jobName != (this._results[c].data[0].JobName).toLowerCase()) {
                retState = ECodes.E_DIFFERENT_JOB_NAMES
              }
            }
          }
        }
      } else {
        retState = this._results.error
      }

      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
      tracer.addMessage('-> GS: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  //  GV,<cam>
  //  GV,<Status>,<Major>,<Minor>,<SubMinor>
  // SLMP: 1011
  GV: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 1011
    this._numArguments = 0
    this._hasIndex = 1

    if (this._splittedCmd.length < 2) {
      this._splittedCmd.push('0')
    }

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> GV: Execute')
      let major = InSightFunctions.fnGetCellValue('Version.Major')
      let minor = InSightFunctions.fnGetCellValue('Version.Minor')
      let subMinor = InSightFunctions.fnGetCellValue('Version.SubMinor')

      this._results[this._myIndex].data.push({ 'Major': major, 'Minor': minor, 'SubMinor': subMinor })
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1
      tracer.addMessage('<- GV: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> GV: Compute result')
      // TODO: //Prüfung ob alle Versionen gleich sind
      let plcString = ''
      let resStr = ''
      let retState = 1
      let major = -1
      let minor = -1
      let subMinor = -1

      this.checkResultsState()

      if (this._results.error == ECodes.E_NO_ERROR) {
        for (let c = 1; c <= MAX_CAMERAS; c++) {
          if (this._results[c].isNeeded > 0) {
            if (major == -1) {
              major = this._results[c].data[0].Major
              minor = this._results[c].data[0].Minor
              subMinor = this._results[c].data[0].SubMinor
            } else {
              let data = this._results[c].data[0]
              if ((major != data.Major) || (minor != data.Minor) || (subMinor != data.SubMinor)) {
                retState = ECodes.E_DIFFERENT_VERSIONS
              }
            }
          }
        }
      } else {
        retState = this._results.error
      }

      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%d,%d,%d', major, minor, subMinor)
      }
      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)
      tracer.addMessage('-> GV: Compute result')
      return plcString
    }
  },

  // TM,<cam>,<hw trigger> (0=off/1=on)
  // TM,<Status>
  // SLMP: 1050
  TM: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 1050
    this._numArguments = 1
    this._hasIndex = 1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    var triggerMode = parseInt(this._splittedCmd[2])
    if (!(triggerMode == 1 || triggerMode == 0)) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> TM: Execute')
      if (triggerMode == 0) {
        triggerMode = 32
      } else {
        triggerMode = 0
      }
      t.triggerMode = triggerMode

      InSightFunctions.fnSetCellValue('TriggerMode', triggerMode)
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      tracer.addMessage('<- TM: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },

  // HEB,<cam>
  // HEB,<Status>
  // SLMP: 2020
  HEB: function (myIndex, cmdString) { // Camera ID 0/1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2020
    this._numArguments = 0
    this._hasIndex = 1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }
    let index = parseInt(this._splittedCmd[1])
    this._isCameraMoving = ((index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA) || g_Settings.IsRobotMounted

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }
    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> HEB: Execute')
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      // g_HEB_Received = 1;

      g_HECalibrationSettings = new HECalibrationSettings()

      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      tracer.addMessage('<- HEB: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  // HE,<cam>,<targetID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // HE,<Status>
  // SLMP: 2021
  HE: function (myIndex, cmdString) { // Camera ID 0/1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2021
    this._numArguments = 7
    this._hasIndex = 1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    var featureID = parseInt(this._splittedCmd[2])
    if (featureID == 0) {
      featureID = 1
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if (featureID > MAX_FEATURES_PER_CAMERA) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0) !== true) {
      return
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> Execute ' + timeTracker.getElapsedTime())

      if (this._useAsStepID > 0) {
        if (g_LooukupTables.stepLookup[this._index]['Cam_' + this._myIndex]['Enabled'] == 0) {
          tracer.addMessage('------------------------------------ Error ')
        }
      }

      if (this._useAsPartID > 0) {
        if (g_LooukupTables.partLookup[this._partID1]['Cam_' + this._myIndex]['Enabled'] == 0) {
          tracer.addMessage('------------------------------------ Error ')
        }
      }
      this._enabledFeatures = 0

      InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', this._exposureSet - 1)

      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 1)
      }
      if (g_HECalibrationSettings != null) {
        if (g_HECalibrationSettings.calibrationID == -1) {
          g_HECalibrationSettings.calibrationID = featureID
          g_Calibrations[featureID] = new Calibration(featureID)
          g_Calibrations[featureID]['calibrationData']['isMoving'] = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
          InSightFunctions.fnSetCellValue('HECalibration.' + featureID + '.IsCameraMoving', g_Calibrations[featureID]['calibrationData']['isMoving'])
        } else if (g_HECalibrationSettings.calibrationID != featureID) {
          this._validCMD.ECodes.E_NOT_SUPPORTED
        }
      } else {
        this._validCMD = ECodes.E_NO_START_COMMAND
      }

      InSightFunctions.fnSetEvent(32)

      tracer.addMessage('<- Execute ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_IMAGE_ACQUIRED
    }

    this.toolsDone = function (t) {
      tracer.addMessage('-> HE: Tools done')
      g_Graphics['ShowCalibrationPoints_' + featureID] = 1
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')
      let targetTrained = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained')
      this._results[this._myIndex].isValid = 1

      if (targetTrained > 0) {
        if (targetValid > 0) {
          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          g_Calibrations[featureID].calibrationData.targetX.push(targetX)
          g_Calibrations[featureID].calibrationData.targetY.push(targetY)
          g_Calibrations[featureID].calibrationData.targetTheta.push(targetAngle)
          g_Calibrations[featureID].calibrationData.targetValid.push(targetValid)
          g_Calibrations[featureID].calibrationData.robotX.push(this._robotPose.x)
          g_Calibrations[featureID].calibrationData.robotY.push(this._robotPose.y)
          g_Calibrations[featureID].calibrationData.robotTheta.push(this._robotPose.thetaZ)

          g_Calibrations[featureID].calibrationData.count = g_Calibrations[featureID].calibrationData.targetX.length
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
      }

      tracer.addMessage('<- HE: Tools done')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  // HEE,<cam>
  // HEE,<Status>
  // SLMP: 2022
  HEE: function (myIndex, cmdString) { // Camera ID 0/1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2022
    this._numArguments = 0
    this._hasIndex = 1
    var selectedCalibration = -1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> HEE: Execute')
      if (g_HECalibrationSettings == null) {
        this._validCMD = ECodes.E_NO_START_COMMAND
        return
      }
      selectedCalibration = g_HECalibrationSettings.calibrationID

      g_Graphics['ShowCalibrationPoints_' + selectedCalibration] = 1
      this._results[this._myIndex].isValid = 1
      let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
      var valid = t.myCalibrations.CheckData(g_Calibrations[selectedCalibration].calibrationData, isMoving)
      if (valid > 0) {
        g_Calibrations[selectedCalibration].computeCalibration()
        if (g_Calibrations[selectedCalibration].runstatus > 0) {
          g_Calibrations[selectedCalibration].saveCalibrationDataToFile()
          InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
      }
      g_HECalibrationSettings = null
      tracer.addMessage('<- HEE: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },

  // ACB,<cam>,<targetID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // ACB,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 2030
  ACB: function (myIndex, cmdString) { // Camera ID 1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2030
    this._numArguments = 7
    this._hasIndex = 1

    var featureID = 0

    if (this.checkLengthAndSetIndex(1, MAX_CAMERAS) != true) {
      return
    }

    this.checkArgumentsNumbers(1, this._numArguments + this._hasIndex)

    featureID = parseInt(this._splittedCmd[2])
    if (featureID == 0) {
      featureID = 1
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }
    if (featureID > MAX_FEATURES_PER_CAMERA) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    this.copyRobotPose(3)
    let index = parseInt(this._splittedCmd[1])
    this._isCameraMoving = ((index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA) || g_Settings.IsRobotMounted

    // this.fillCmdInfoWithCameraID((0xff & (1 << featureID - 1)), featureID, featureID, 0)
    if (this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0) !== true) {
      return
    }

    this._validCMD = 1

    this.toolsDone = function (t) {
      tracer.addMessage('-> ACB: Tools done')

      InSightFunctions.fnSetCellValue('HECalibration.Selector', featureID - 1)
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      InSightFunctions.fnSetCellValue('HECalibration.' + featureID + '.IsCameraMoving', this._isCameraMoving)

      g_AutoCalibRuntime = {}
      g_AutoCalibRuntime = g_Settings.AutoCalibration
      g_AutoCalibRuntime['innerDist'] = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Trainregion.InnerDist')
      g_AutoCalibRuntime['loopCnt'] = 0
      g_AutoCalibRuntime['stepCount'] = 0
      g_AutoCalibRuntime['direction'] = 1
      g_AutoCalibRuntime['minDist_Pixel'] = 55

      g_AutoCalibRuntime['rotAngle'] = (g_AutoCalibRuntime['AngleMax'] - g_AutoCalibRuntime['AngleMin']) / 10
      g_AutoCalibRuntime['advCalibPoints'] = []

      g_AutoCalibRuntime['lastMoveDistance_X'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['lastMoveDistance_Y'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['angleCompX'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['angleCompY'] = START_MOVE_DISTANCE_MM

      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')

      if (InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained') > 0) {
        if (targetValid > 0) {
          let firstPos = { 'X': this._robotPose.x, 'Y': this._robotPose.y, 'Z': this._robotPose.z, 'A': this._robotPose.thetaZ, 'B': this._robotPose.thetaY, 'C': this._robotPose.thetaX }
          g_AutoCalibRuntime['FirstPos'] = firstPos
          g_AutoCalibRuntime['PreCalibration'] = { 'Calibrated': 0 }
          g_AutoCalibRuntime['PreCalibPoses'] = []
          g_AutoCalibRuntime['CalibPoints'] = []
          g_AutoCalibRuntime['RobCalibPoses'] = []
          g_AutoCalibRuntime['NextRobotPose'] = {}
          g_AutoCalibRuntime['Compensation'] = {}
          g_AutoCalibRuntime['LastNextPos'] = { 'X': 0, 'Y': 0, 'Z': 0, 'A': 0, 'B': 0, 'C': 0 }

          g_Graphics['ShowCalibrationPoints_' + featureID] = 1
          t.myCalibrations.calibrations[featureID] = new Calibration(featureID)
          t.myCalibrations.calibrations[featureID]['calibrationData']['isMoving'] = this._isCameraMoving

          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          let nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)

          if (nextRobotPose.Valid) {
            nextRobotPose.NextX = Math.round(nextRobotPose.NextX * 10000) / 10000
            nextRobotPose.NextY = Math.round(nextRobotPose.NextY * 10000) / 10000
            nextRobotPose.NextAngle = Math.round(nextRobotPose.NextAngle * 10000) / 10000
          }
          this._results[t.myDetails.myIndex].isValid = 1
          this._results[t.myDetails.myIndex].state = nextRobotPose.Valid
          this._results[t.myDetails.myIndex].data = nextRobotPose
        } else {
          this._results[t.myDetails.myIndex].isValid = 1
          this._results[t.myDetails.myIndex].state = ECodes.E_FEATURE_NOT_FOUND
        }
      } else {
        this._results[t.myDetails.myIndex].isValid = 1
        this._results[t.myDetails.myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
      }
      tracer.addMessage('<- ACB: Tools done')
      return States.WAITING_FOR_SLAVE_RESULT
    }
    this.computeTotalResult = function () {
      tracer.addMessage('-> ACB: Compute result')
      let plcString = ''
      this.checkResultsState()

      if (this._results.error == ECodes.E_NO_ERROR) {
        let res = this._results[this._index]
        plcString = InSightFunctions.fnStringf('%s,%d,%.3f,%.3f,%.3f,%.3f,%.3f,%.3f', this._splittedCmd[0], res.state, res.data.NextX, res.data.NextY, this._robotPose.z, res.data.NextAngle, this._robotPose.thetaY, this._robotPose.thetaX)
      } else {
        plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], this._results.error)
      }
      tracer.addMessage('<- ACB: Compute result')
      return plcString
    }
  },
  // AC,<cam>,<targetID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // AC,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 2031
  AC: function (myIndex, cmdString) { // Camera ID 1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2031
    this._numArguments = 7
    this._hasIndex = 1
    var featureID = 0

    if (this.checkLengthAndSetIndex(1, MAX_CAMERAS) != true) {
      return
    }
    this.checkArgumentsNumbers(1, this._numArguments + this._hasIndex)

    featureID = parseInt(this._splittedCmd[2])
    if (featureID == 0) {
      featureID = 1
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }
    if (featureID > MAX_FEATURES_PER_CAMERA) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    this.copyRobotPose(3)

    // this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0)
    if (this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0) !== true) {
      return
    }
    this._validCMD = 1

    this.toolsDone = function (t) {
      tracer.addMessage('-> AC Tools done')

      var nextRobotPose = { 'Valid': 0 }
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')

      if (InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained') > 0) {
        if (targetValid > 0) {
          let targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          let targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          let targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          if (g_AutoCalibRuntime['PreCalibration'].Calibrated == 0) {
            nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)
          } else {
            g_Graphics['ShowCalibrationPoints_' + featureID] = 1
            let calib = t.myCalibrations.calibrations[featureID]

            calib.calibrationData.targetX.push(targetX)
            calib.calibrationData.targetY.push(targetY)
            calib.calibrationData.targetTheta.push(targetAngle)
            calib.calibrationData.targetValid.push(targetValid)
            calib.calibrationData.robotX.push(this._robotPose.x)
            calib.calibrationData.robotY.push(this._robotPose.y)
            calib.calibrationData.robotTheta.push(this._robotPose.thetaZ)

            calib.calibrationData.count = calib.calibrationData.targetX.length

            nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)

            if (nextRobotPose.Valid == 1) {
              let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
              var valid = t.myCalibrations.CheckData(calib.calibrationData, isMoving)
              if (valid > 0) {
                calib.computeCalibration()
                if (calib.runstatus > 0) {
                  calib.saveCalibrationDataToFile()
                  InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
                } else {

                }
              } else {
                console.log('Invalid calibration data! Calibration not saved!')
                // TODO: t.initCalibData();
                // this.calibData["LogMessage"]=logMessage;
              }
            }
          }

          if (nextRobotPose.Valid) {
            nextRobotPose.NextX = Math.round(nextRobotPose.NextX * 10000) / 10000
            nextRobotPose.NextY = Math.round(nextRobotPose.NextY * 10000) / 10000
            nextRobotPose.NextAngle = Math.round(nextRobotPose.NextAngle * 10000) / 10000
          }

          this._results[t.myDetails.myIndex].isValid = 1
          this._results[t.myDetails.myIndex].state = nextRobotPose.Valid
          this._results[t.myDetails.myIndex].data = nextRobotPose
        } else {
          this._results[t.myDetails.myIndex].isValid = 1
          this._results[t.myDetails.myIndex].state = ECodes.E_FEATURE_NOT_FOUND
          this._results[t.myDetails.myIndex].data = {}
        }
      } else {
        this._results[t.myDetails.myIndex].isValid = 1
        this._results[t.myDetails.myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
        this._results[t.myDetails.myIndex].data = {}
      }
      tracer.addMessage('<- AC Tools done')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> AC compute total result')
      let plcString = ''
      let errCnt = 0
      let errCode
      for (var i = 1; i <= MAX_CAMERAS; i++) {
        if (this._results[i].isNeeded > 0) {
          if (this._results[i].isValid > 0) {
            if (this._results[i].state <= 0) {
              errCnt++
              errCode = this._results[i].state
            }
          }
        }
      }
      let res = this._results[this._index]
      if (errCnt > 0) {
        plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], errCode)
      } else {
        plcString = InSightFunctions.fnStringf('%s,%d,%.3f,%.3f,%.3f,%.3f,%.3f,%.3f', this._splittedCmd[0], res.state, res.data.NextX, res.data.NextY, this._robotPose.z, res.data.NextAngle, this._robotPose.thetaY, this._robotPose.thetaX)
      }
      tracer.addMessage('<- AC compute total result')
      return plcString
    }
  },

  // CCB,<cam>,<SwapHandedness>,<FeatureOffsetX>,<FeatureOffsetY> (0=don't swap / 1=swap)
  // CCB,<Status>
  // SLMP: 2040
  CCB: function (myIndex, cmdString) { // Camera ID 0/1/2
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2040
    this._numArguments = 3
    this._hasIndex = 1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }
    var swapHandedness = parseInt(this._splittedCmd[2])
    var featureOffsetX = parseFloat(this._splittedCmd[3])
    var featureOffsetY = parseFloat(this._splittedCmd[4])

    let index = parseInt(this._splittedCmd[1])
    this._isCameraMoving = ((index & MASK_MOVING_CAMERA) >> SHIFT_MOVING_CAMERA) || g_Settings.IsRobotMounted

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    // this.fillCmdInfoWithCameraID(0, 0, 0, 0)
    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> CCB: Execute')
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 0)
      g_CustomCalibrationSettings = new CustomCalibrationSettings(swapHandedness, featureOffsetX, featureOffsetY, -1)

      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      tracer.addMessage('<- CCB: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },

  // CC,<cam>,<targetID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // CC,<Status>
  // SLMP: 2041
  CC: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2041
    this._numArguments = 7
    this._hasIndex = 1

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    if ((this._index == this._myIndex) && (g_CustomCalibrationSettings == null)) {
      this._validCMD = ECodes.E_NO_START_COMMAND
      return
    }

    var featureID = parseInt(this._splittedCmd[2])
    if (featureID == 0) {
      featureID = 1
    }

    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }
    if (featureID > MAX_FEATURES_PER_CAMERA) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    if (this._index == this._myIndex) {
      this.copyRobotPose(3)

      if (g_CustomCalibrationSettings.calibrationID < 1) {
        g_CustomCalibrationSettings.calibrationID = featureID
        g_Calibrations[featureID] = new Calibration(featureID)
        g_Calibrations[featureID]['calibrationData']['isMoving'] = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
        InSightFunctions.fnSetCellValue('HECalibration.' + featureID + '.IsCameraMoving', g_Calibrations[featureID]['calibrationData']['isMoving'])
      }
    }

    // this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0)
    if (this.fillCmdInfoWithCameraID(0xff & (1 << (featureID - 1)), featureID, featureID, 0) !== true) {
      return
    }
    this._validCMD = 1

    this.toolsDone = function (t) {
      tracer.addMessage('-> CC: Tools done')
      g_Graphics['ShowCalibrationPoints_' + featureID] = 1
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')
      let targetTrained = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained')
      this._results[this._myIndex].isValid = 1

      if (targetTrained > 0) {
        if (targetValid > 0) {
          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          g_Calibrations[featureID].calibrationData.targetX.push(targetX)
          g_Calibrations[featureID].calibrationData.targetY.push(targetY)
          g_Calibrations[featureID].calibrationData.targetTheta.push(targetAngle)
          g_Calibrations[featureID].calibrationData.targetValid.push(targetValid)
          g_Calibrations[featureID].calibrationData.robotX.push(this._robotPose.x)
          g_Calibrations[featureID].calibrationData.robotY.push(this._robotPose.y)
          g_Calibrations[featureID].calibrationData.robotTheta.push(this._robotPose.thetaZ)

          g_Calibrations[featureID].calibrationData.count = g_Calibrations[featureID].calibrationData.targetX.length
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
      }

      tracer.addMessage('<- CC: Tools done')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  // CCE,<cam>
  // CCE,<Status>
  // SLMP: 2042
  CCE: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 2042
    this._numArguments = 0
    this._hasIndex = 1
    var selectedCalibration = 0

    if (this.checkLengthAndSetIndex(0, MAX_CAMERAS) != true) {
      return
    }

    if ((this._index == this._myIndex) && (g_CustomCalibrationSettings == null)) {
      this._validCMD = ECodes.E_NO_START_COMMAND
      return
    }
    if (this._index > MAX_CAMERAS) {
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    // this.fillCmdInfoWithCameraID(0, 0, 0, 0)
    if (this.fillCmdInfoWithCameraID(0, 0, 0, 0) !== true) {
      return
    }

    if (this._index == this._myIndex) {
      selectedCalibration = g_CustomCalibrationSettings.calibrationID
    }

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> CCE: Execute')
      g_Graphics['ShowCalibrationPoints_' + selectedCalibration] = 1
      this._results[this._myIndex].isValid = 1
      let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
      var ccData = t.myCalibrations.doCustomCalibration(g_Calibrations[selectedCalibration].calibrationData,
        g_CustomCalibrationSettings.swapHandedness, g_CustomCalibrationSettings.featureOffsetX, g_CustomCalibrationSettings.featureOffsetY)

      if (ccData != 0) {
        var valid = t.myCalibrations.CheckData(g_Calibrations[selectedCalibration].calibrationData, isMoving)
        if (valid > 0) {
          g_Calibrations[selectedCalibration].computeCalibration()
          if (g_Calibrations[selectedCalibration].runstatus > 0) {
            g_Calibrations[selectedCalibration].saveCalibrationDataToFile()
            InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
            this._results[this._myIndex].state = 1
          } else {
            this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
          }
        } else {
          this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
      }

      g_CustomCalibrationSettings = null
      tracer.addMessage('<- CCE: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },

  // TA,<partID>
  // TA,<Status>
  // SLMP: 3010
  TA: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3010
    this._numArguments = 0
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsTrainImage'

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    if (this._splittedCmd.length >= 8) {
      if (this.checkArgumentsNumbers(2, this._splittedCmd.length - 1) == false) {
        return
      }
      this.copyRobotPose(2)
    }

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    this._validCMD = 1

    this.computeTotalResult = function () {
      tracer.addMessage('-> TA Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let errCnt = 0
      let errCode = ECodes.E_NO_ERROR

      for (var i = 1; i <= MAX_CAMERAS; i++) {
        if (this._results[i].isNeeded > 0) {
          if (this._results[i].isValid > 0) {
            if (this._results[i].state <= 0) {
              errCnt++
              errCode = this._results[i].state
            }
          }
        }
      }

      let state = ECodes.E_INTERNAL_ERROR
      if (errCnt == 0) {
        state = 1 // No error
        let partInfo = g_LooukupTables.partLookup[this._partID1]
        let message = ''

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            message += targetID + ', gribber ' + this._gripperID1 + ', '
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees

              g_TrainedFeatures[targetID][this._gripperID1].valid = g_RuntimeFeatures[targetID].valid
              g_TrainedFeatures[targetID][this._gripperID1].x = g_RuntimeFeatures[targetID].x
              g_TrainedFeatures[targetID][this._gripperID1].y = g_RuntimeFeatures[targetID].y
              g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees = g_RuntimeFeatures[targetID].thetaInDegrees

              resIndex++
            }
          }
        }
        //myLogger.addLogMessage(0, message)
        InSightFunctions.fnSetEvent(83)
      } else {
        state = errCode
      }

      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)

      tracer.addMessage('<- TA Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode> (1=ABS, 2=OFF) 
  // XA,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 3011
  XA: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3011
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'
    let logMsg = ['', '', '']

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len-1; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }
	//GSSVN: Fix result mode from PLC, "2" for Offset mode
    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)

    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    this._validCMD = 1

    //GSSVN: Extract barcode and send to save image on FTP server
    if (this._splittedCmd.length > 9) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }

    this.toolsDone = function (t) {
      tracer.addMessage('-> XA: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            //myLogger.addLogMessage(0, 'xformData: ' + transformed.x + '\t' + transformed.y )
            this._results[this._myIndex].state = transformed.valid
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push(transformed)
        }
      }

      g_Graphics.ShowCrossHair = []

      let camInfo = g_LooukupTables.partLookup[this._partID1]['Cam_' + this._myIndex]

      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let targetID = camInfo['Feature_' + f]
        if (targetID > 0) {
          let imgPos = []
          let trained = g_TrainedFeatures[targetID][this._gripperID1].valid
          if (trained > 0) {
            imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[targetID][this._gripperID1].x, g_TrainedFeatures[targetID][this._gripperID1].y, g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees, 0, 0, 0)
            g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
          }
        }
      }
      tracer.addMessage('<- XA: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XA: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      if (this._results.error == ECodes.E_NO_ERROR) {
        let partInfo = g_LooukupTables.partLookup[this._partID1]
        let message = ''
        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              message += 'targetID = ' + targetID + ', feature = ' + f + ' '
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              logMsg[targetID] = InSightFunctions.fnStringf('%d, %d, %07.2f, %07.2f, %07.2f', c, resIndex, g_RuntimeFeatures[targetID].x, g_RuntimeFeatures[targetID].y, g_RuntimeFeatures[targetID].thetaInDegrees)
              resIndex++
            }
          }
        }
        message += this._index + ' ' + this._myIndex
        //myLogger.addLogMessage(0, message)
        newRobPose = ComputeAlignMode_1(this._index, this._gripperID1, resMode, this._robotPose)

        retState = newRobPose.valid
      } else {
        retState = this._results.error
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      let resStr = ''
      if (retState > 0) {
      	//GSSVN: Format output string, fix output length
        resStr = InSightFunctions.fnStringf(', %07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)
      //myLogger.addLogMessage(0, logMsg[1])
      //myLogger.addLogMessage(0, logMsg[2])

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(newRobPose.x)>g_LimitX)||(Math.abs(newRobPose.y)>g_LimitY)||(Math.abs(newRobPose.thetaZ)>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XA: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  //Command: XAA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
  //XAA,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<XFwd1>,<YFwd1>,<TFwd1>,<XFwd2>,<YFwd2>,<TFwd2>,<XRev1>,<YRev1>,<TRev1>,<XRev2>,<YRev2>,<TRev2><Distance>
  XAA: function(myIndex, cmdString){
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3015
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 0
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }
    //TODO
    //myLogger.addLogMessage(0, 'Call XAA command')
    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)

    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
     return
    }
    this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    this._validCMD = 1

    //Extract barcode
    if (this._splittedCmd.length > 9) {
    let barcode = this._splittedCmd[9]
    InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
    //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }

    this.toolsDone = function (t) {
     //myLogger.addLogMessage(0, '-> XAA: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }
      
      this._results[this._myIndex]['isValid'] = 1
      let errorState = 0
      let isFeatureValid = true;
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        //if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            //myLogger.addLogMessage(0, 'xformData: ' + transformed.x + ' ' + transformed.y )
            this._results[this._myIndex].state = transformed.valid
            if (transformed.valid < 0)
            {
              errorState = transformed.valid
              //Prevent Error state if any Feature is not found
              //isFeatureValid = false
            }
          } else {
            errorState = g_CurrentFeatures[f].valid
            transformed.valid = 0
            //isFeatureValid = false
          }
          
          this._results[this._myIndex].data.push(transformed)

          //myLogger.addLogMessage(0, 'DataLenght ' + this._results[this._myIndex].data.length )
        //}
      }
      
      

      //myLogger.addLogMessage(0, '-> XAA: Tools done ' + isFeatureValid)
      if (!isFeatureValid)
        this._results[this._myIndex].state = errorState

      g_Graphics.ShowCrossHair = []

      for (let partID = 1; partID <= 2; partID++)
      {
        let camInfo = g_LooukupTables.partLookup[partID]['Cam_' + this._myIndex]
        for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
          let targetID = camInfo['Feature_' + f]
          if (targetID > 0) {
            let imgPos = []
            let trained = g_TrainedFeatures[targetID][this._gripperID1].valid
            if (trained > 0) {
              imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[targetID][this._gripperID1].x, g_TrainedFeatures[targetID][this._gripperID1].y, g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees, 0, 0, 0)
              g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
            }
          }
        }
      }
      //myLogger.addLogMessage(0, '<- XAA: Tools done ' + timeTracker.getElapsedTime())
      
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      //myLogger.addLogMessage(0, '-> XAA: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let newRobPoseFwd = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseFwd = [0, 0, 0, 0, 0, 0]
      let newRobPoseRev = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseRev = [0, 0, 0, 0, 0, 0]
      let logMsg = ['', '', '']
      let foundPoseNo = 0;

      this.checkResultsState()
      let retStateFwd = ECodes.E_UNSPECIFIED
      let retStateRev = ECodes.E_UNSPECIFIED
      let retState = ECodes.E_UNSPECIFIED
      let message = ''
      let featureValid = ''
      let isDistanceValid = true
      //if (this._results.error == ECodes.E_NO_ERROR) {
        let resIndex = 0
        for (let partID = 1; partID <= 2; partID++)
        {          
          message += 'partID = ' + partID + ';'
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              message += 'targetID = ' + targetID + ', feature valid = ' + this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              logMsg[targetID] = InSightFunctions.fnStringf('%d, %d, %.3f, %.3f, %.3f', c, resIndex, g_RuntimeFeatures[targetID].x, g_RuntimeFeatures[targetID].y, g_RuntimeFeatures[targetID].thetaInDegrees)

              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
              resIndex++
            }
          }
          newRobPoseFwd = ComputeAlignMode_1(partID, this._gripperID1, resMode, this._robotPose)
          combinedPoseFwd[3*(partID - 1) + 0] = newRobPoseFwd.x
          combinedPoseFwd[3*(partID - 1) + 1] = newRobPoseFwd.y
          combinedPoseFwd[3*(partID - 1) + 2] = newRobPoseFwd.thetaZ

          retStateFwd = newRobPoseFwd.valid
        }
        resIndex = 0
        for (let partID = 2; partID >= 1; partID--)
        {          
          message += 'partID = ' + partID + ';'
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              message += 'targetID = ' + targetID + ', feature valid = ' + this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              logMsg[targetID] = InSightFunctions.fnStringf('%d, %d, %.3f, %.3f, %.3f', c, resIndex, g_RuntimeFeatures[targetID].x, g_RuntimeFeatures[targetID].y, g_RuntimeFeatures[targetID].thetaInDegrees)
              resIndex++
            }
          }
          newRobPoseRev = ComputeAlignMode_1(partID, this._gripperID1, resMode, this._robotPose)
          combinedPoseRev[3*(partID - 1) + 0] = newRobPoseRev.x
          combinedPoseRev[3*(partID - 1) + 1] = newRobPoseRev.y
          combinedPoseRev[3*(partID - 1) + 2] = newRobPoseRev.thetaZ

          retStateRev = newRobPoseRev.valid
        }
        retState = retStateFwd & retStateRev
      //} else {
        //retState = this._results.error
      //}
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', featureValid, combinedPoseFwd[0], combinedPoseFwd[1], combinedPoseFwd[2], combinedPoseFwd[3], combinedPoseFwd[4], combinedPoseFwd[5],combinedPoseRev[3], combinedPoseRev[4], combinedPoseRev[5], combinedPoseRev[0], combinedPoseRev[1], combinedPoseRev[2], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)
      //myLogger.addLogMessage(0, message)
      //myLogger.addLogMessage(0, logMsg[1])
      //myLogger.addLogMessage(0, logMsg[2])

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPoseFwd[0])>g_LimitX)||(Math.abs(combinedPoseFwd[3])>g_LimitX)||(Math.abs(combinedPoseFwd[1])>g_LimitY)||(Math.abs(combinedPoseFwd[4])>g_LimitY)||(Math.abs(combinedPoseFwd[2])>g_LimitTheta)||(Math.abs(combinedPoseFwd[5])>g_LimitTheta)||(Math.abs(combinedPoseRev[0])>g_LimitX)||(Math.abs(combinedPoseRev[3])>g_LimitX)||(Math.abs(combinedPoseRev[1])>g_LimitY)||(Math.abs(combinedPoseRev[4])>g_LimitY)||(Math.abs(combinedPoseRev[2])>g_LimitTheta)||(Math.abs(combinedPoseRev[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }
      
      tracer.addMessage('<- XAA: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  //Command: XAF,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
  //XAF,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<X1>,<Y1>,<T1>,<X2>,<Y2>,<T2>,<Distance>
  XAF: function(myIndex, cmdString){
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3013
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 0
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }
    //TODO
    //myLogger.addLogMessage(0, 'Call XAF command')
    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }
    
    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)

    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }
    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
     return
    }
    this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    this._validCMD = 1

    //Extract barcode
    if (this._splittedCmd.length > 9) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }
      else{
        let barcode = 'Nan'
        InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
        //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }

    this.toolsDone = function (t) {
      //myLogger.addLogMessage(0, '-> XAF: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }
      
      this._results[this._myIndex]['isValid'] = 1
      let errorState = 0
      let isFeatureValid = true;
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        //if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            //myLogger.addLogMessage(0, 'xformData: ' + transformed.x + ' ' + transformed.y )
            this._results[this._myIndex].state = transformed.valid
            if (transformed.valid < 0)
            {
              errorState = transformed.valid
              //Prevent Error state if any Feature is not found
              //isFeatureValid = false
            }
          } else {
            errorState = g_CurrentFeatures[f].valid
            transformed.valid = 0
            //isFeatureValid = false
          }
          
          this._results[this._myIndex].data.push(transformed)

          //myLogger.addLogMessage(0, 'DataLenght ' + this._results[this._myIndex].data.length )
        //}
      }

      //myLogger.addLogMessage(0, '-> XAF: Tools done ' + isFeatureValid)
      if (!isFeatureValid)
        this._results[this._myIndex].state = errorState

      g_Graphics.ShowCrossHair = []

      for (let partID = 1; partID <= 2; partID++)
      {
        let camInfo = g_LooukupTables.partLookup[partID]['Cam_' + this._myIndex]
        for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
          let targetID = camInfo['Feature_' + f]
          if (targetID > 0) {
            let imgPos = []
            let trained = g_TrainedFeatures[targetID][this._gripperID1].valid
            if (trained > 0) {
              imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[targetID][this._gripperID1].x, g_TrainedFeatures[targetID][this._gripperID1].y, g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees, 0, 0, 0)
              g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
            }
          }
        }
      }
      //myLogger.addLogMessage(0, '<- XAF: Tools done ' + timeTracker.getElapsedTime())
      
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      //myLogger.addLogMessage(0, '-> XAF: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPose = [0, 0, 0, 0, 0, 0]
      let logMsg = ['', '', '']
      let foundPoseNo = 0;

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      let message = ''
      let featureValid = ''
      let isDistanceValid = true;
      if (this._results.error == ECodes.E_NO_ERROR) {
        let resIndex = 0
        for (let partID = 1; partID <= 2; partID++)
        {          
          message += 'partID = ' + partID + ';'
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              message += 'targetID = ' + targetID + ', feature valid = ' + this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              logMsg[targetID] = InSightFunctions.fnStringf('%d, %d, %.3f, %.3f, %.3f', c, resIndex, g_RuntimeFeatures[targetID].x, g_RuntimeFeatures[targetID].y, g_RuntimeFeatures[targetID].thetaInDegrees)

              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
              resIndex++
            }
          }
          newRobPose = ComputeAlignMode_1(partID, this._gripperID1, resMode, this._robotPose)
          combinedPose[3*(partID - 1) + 0] = newRobPose.x
          combinedPose[3*(partID - 1) + 1] = newRobPose.y
          combinedPose[3*(partID - 1) + 2] = newRobPose.thetaZ

          retState = newRobPose.valid
        }
      } else {
        retState = this._results.error
      }
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      //myLogger.addLogMessage(0, 'err: ' + g_AdditionalError + ' Code: ' + retState)
      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f', featureValid, combinedPose[0], combinedPose[1], combinedPose[2], combinedPose[3], combinedPose[4], combinedPose[5], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)
      //myLogger.addLogMessage(0, message)
      //myLogger.addLogMessage(0, logMsg[1])
      //myLogger.addLogMessage(0, logMsg[2])

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPose[0])>g_LimitX)||(Math.abs(combinedPose[3])>g_LimitX)||(Math.abs(combinedPose[1])>g_LimitY)||(Math.abs(combinedPose[4])>g_LimitY)||(Math.abs(combinedPose[2])>g_LimitTheta)||(Math.abs(combinedPose[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XAF: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  //XAR,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C><Barcode>
  //XAR,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<X1>,<Y1>,<T1>,<X2>,<Y2>,<T2>
  XAR: function(myIndex, cmdString){
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3014
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 0
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }
    //TODO
    //myLogger.addLogMessage(0, 'Call XAR command')
    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }
    
    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)

    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }
    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
     return
    }
    this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    this._validCMD = 1
        //Extract barcode
    if (this._splittedCmd.length > 9) {
    let barcode = this._splittedCmd[9]
    InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
    //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }

    this.toolsDone = function (t) {
      //myLogger.addLogMessage(0, '-> XAR: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }
      
      this._results[this._myIndex]['isValid'] = 1
      let errorState = 0
      let isFeatureValid = true;
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        //if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            //myLogger.addLogMessage(0, 'xformData: ' + transformed.x + ' ' + transformed.y )
            this._results[this._myIndex].state = transformed.valid
            if (transformed.valid < 0)
            {
              errorState = transformed.valid
              //Prevent Error state if any Feature is not found
              //isFeatureValid = false
            }
          } else {
            errorState = g_CurrentFeatures[f].valid
            transformed.valid = 0
            //isFeatureValid = false
          }
          
          this._results[this._myIndex].data.push(transformed)

          //myLogger.addLogMessage(0, 'DataLenght ' + this._results[this._myIndex].data.length )
        //}
      }
      //myLogger.addLogMessage(0, '-> XAR: Tools done ' + isFeatureValid)
      if (!isFeatureValid)
        this._results[this._myIndex].state = errorState

      g_Graphics.ShowCrossHair = []

      for (let partID = 1; partID <= 2; partID++)
      {
        let camInfo = g_LooukupTables.partLookup[partID]['Cam_' + this._myIndex]
        for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
          let targetID = camInfo['Feature_' + f]
          if (targetID > 0) {
            let imgPos = []
            let trained = g_TrainedFeatures[targetID][this._gripperID1].valid
            if (trained > 0) {
              imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[targetID][this._gripperID1].x, g_TrainedFeatures[targetID][this._gripperID1].y, g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees, 0, 0, 0)
              g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
            }
          }
        }
      }
      //myLogger.addLogMessage(0, '<- XAR: Tools done ' + timeTracker.getElapsedTime())
      
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      //myLogger.addLogMessage(0, '-> XAR: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPose = [0, 0, 0, 0, 0, 0]
      let logMsg = ['', '', '']
      let foundPoseNo = 0;

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      let message = ''
      let featureValid = ''
      let isDistanceValid = true;
      if (this._results.error == ECodes.E_NO_ERROR) {
        let resIndex = 0
        for (let partID = 2; partID >= 1; partID--)
        {          
          message += 'partID = ' + partID + ';'
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              message += 'targetID = ' + targetID + ', feature valid = ' + this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              logMsg[targetID] = InSightFunctions.fnStringf('%d, %d, %.3f, %.3f, %.3f', c, resIndex, g_RuntimeFeatures[targetID].x, g_RuntimeFeatures[targetID].y, g_RuntimeFeatures[targetID].thetaInDegrees)

              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
              resIndex++
            }
          }
          newRobPose = ComputeAlignMode_1(partID, this._gripperID1, resMode, this._robotPose)
          combinedPose[3*(partID - 1) + 0] = newRobPose.x
          combinedPose[3*(partID - 1) + 1] = newRobPose.y
          combinedPose[3*(partID - 1) + 2] = newRobPose.thetaZ

          retState = newRobPose.valid
        }
      } else {
        retState = this._results.error
      }
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f', featureValid, combinedPose[3], combinedPose[4], combinedPose[5], combinedPose[0], combinedPose[1], combinedPose[2], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)
      //myLogger.addLogMessage(0, message)
      //myLogger.addLogMessage(0, logMsg[1])
      //myLogger.addLogMessage(0, logMsg[2])

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPose[0])>g_LimitX)||(Math.abs(combinedPose[3])>g_LimitX)||(Math.abs(combinedPose[1])>g_LimitY)||(Math.abs(combinedPose[4])>g_LimitY)||(Math.abs(combinedPose[2])>g_LimitTheta)||(Math.abs(combinedPose[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XAR: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XAS,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // XAS,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Score>
  // SLMP: 3012
  XAS: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3012
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    resMode = parseInt(this._splittedCmd[2])
    let resModeString = this._splittedCmd[2]

    if (!((resMode == ResultMode.ABS) || (resMode == ResultMode.OFF) || (resModeString === ResultMode[ResultMode.ABS]) || (resModeString === ResultMode[ResultMode.OFF]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    this._validCMD = 1

    this.toolsDone = function (t) {
      tracer.addMessage('-> XAS: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        let score = -1

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
            score = InSightFunctions.fnGetCellValue('Target.' + f + '.Pattern_Score')
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push([transformed, score])
        }
      }

      g_Graphics.ShowCrossHair = []

      let camInfo = g_LooukupTables.partLookup[this._partID1]['Cam_' + this._myIndex]

      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let targetID = camInfo['Feature_' + f]
        if (targetID > 0) {
          let imgPos = []
          let trained = g_TrainedFeatures[targetID][this._gripperID1].valid
          if (trained > 0) {
            imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[targetID][this._gripperID1].x, g_TrainedFeatures[targetID][this._gripperID1].y, g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees, 0, 0, 0)
            g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
          }
        }
      }

      tracer.addMessage('<- XA: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XA: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let scores = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      if (this._results.error == ECodes.E_NO_ERROR) {
        let partInfo = g_LooukupTables.partLookup[this._partID1]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex][0].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex][0].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex][0].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex][0].thetaInDegrees
              scores = scores + InSightFunctions.fnStringf(',%.3f', this._results[c].data[resIndex][1])
              resIndex++
            }
          }
        }
        newRobPose = ComputeAlignMode_1(this._index, this._gripperID1, resMode, this._robotPose)
        retState = newRobPose.valid
      } else {
        retState = this._results.error
      }

      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%.3f,%.3f,%.3f,%.3f,%.3f,%.3f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)
        resStr = resStr + scores
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      tracer.addMessage('<- XA: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XI,<cam>,<inspection>,<Barcode>
  // XI,<Status>
  // SLMP: 6010
  XI: function (myIndex, cmdString) {
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 6010
    this._numArguments = 0
    this._hasIndex = 1
    this._useAsPartID = 0
    // this._useAsInspectionID = 1
    this._logImageType = 'LogImage.IsProductionImage'
    this._sendToSlave = 0

    this._index = parseInt(this._splittedCmd[1])
    var inspectionID = parseInt(this._splittedCmd[2])
	
	  //myLogger.addLogMessage(0, InSightFunctions.fnStringf('CommandLength %d', this._splittedCmd.length))
	
    if (this.fillCmdInfoWithCameraID(inspectionID, 0, 0, 0) !== true) {
      return
    }
    this._isCameraMoving = 0
    this._validCMD = 1
    if(inspectionID==2)
    {
    //XI,1,2,<ntf1>,<ntf2>,<barcode>
    //Run inspection 2
    if (this._splittedCmd.length >= 5) {
      let ntf1 = parseInt(this._splittedCmd[3])
      let ntf2 = parseInt(this._splittedCmd[4])
      //myLogger.addLogMessage(0,'NTF receive: ' + ntf1 + ' ' + ntf2)
      InSightFunctions.fnSetCellValue('Inspection.2.PatternToFind.1', ntf1)
      InSightFunctions.fnSetCellValue('Inspection.2.PatternToFind.2', ntf2)
      //myLogger.addLogMessage(0,'NTF add done: ' + ntf1 + ' ' + ntf2)
    }
	  //myLogger.addLogMessage(0, InSightFunctions.fnStringf('CommandLength %d', this._splittedCmd.length))
    if (this._splittedCmd.length >= 6) {
      let barcode = this._splittedCmd[5]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
  }
  else if (inspectionID==1)
  {
    if (this._splittedCmd.length >= 4) {
      let barcode = this._splittedCmd[3]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
  }
  else if (inspectionID==5)
  {
    if (this._splittedCmd.length >= 8) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
    }
  }
    this.execute = function (t) {
      tracer.addMessage('-> XI: Execute')
      this._enabledFeatures = 0

      if (g_Inspections.hasOwnProperty(inspectionID)) {
        setInspectionAcqSettings(inspectionID, 1)
        InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', 2)

        if (this._logImageType.length > 2) {
          InSightFunctions.fnSetCellValue(this._logImageType, 1)
        }

        if (t.triggerMode == 32) {
          InSightFunctions.fnSetEvent(32)
        }
      } else {
        this._results.error = ECodes.E_INVALID_ARGUMENT
      }

      tracer.addMessage('<- XI: Execute ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_IMAGE_ACQUIRED
    }

    this.imgAcquired = function () {
      tracer.addMessage('-> XI: Image acquired' + timeTracker.getElapsedTime())
      this._featureMask = inspectionID << 8
      this._enabledFeatures = this._featureMask
      tracer.addMessage('<- XI: Image acquired' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_TOOLS_DONE
    }

    this.toolsDone = function (t) {
      tracer.addMessage('-> XI: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }
      tracer.addMessage('<- XI: Tools done ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XI: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let resultStr = ''

      tracer.addMessage(this._results)

      this.checkResultsState()

      let retState = ECodes.E_UNSPECIFIED
      if (this._results.error == ECodes.E_NO_ERROR) {
        retState = 1
        resultStr = this.getAllResultData()
      } else {
        retState = this._results.error
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resultStr)

      tracer.addMessage('<- XI: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // SGP,<stepID>,<coords>,<X>,<Y>,<Theta> (1=HOME2D, 2=CAM2D, 3=RAW2D)
  // SGP,<Status>
  // SLMP: 3020
  SGP: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3020
    this._numArguments = 4
    this._hasIndex = 1
    this._useAsStepID = 1
    // this._onlyForMaster = 1;

    var mode = -1
    var newGoldenPose = new Feature(0, 0, 0, 0)
    var featureID = -1

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    if (g_StepsByID[this._index].FeatureIDs.length > 1) {
      this._validCMD = ECodes.E_COMBINATION_NOT_ALLOWED
      return
    } else {
      featureID = g_StepsByID[this._index].FeatureIDs[0]
      // calibration = g_Calibrations[g_FeaturesInfos[featureID].shuttlePose];
    }

    if ((this._splittedCmd[2] == CoordinateSystem.HOME2D) || (this._splittedCmd[2] == CoordinateSystem.CAM2D) || (this._splittedCmd[2] == CoordinateSystem.RAW2D) ||
      (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.HOME2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.CAM2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.RAW2D])) {
      mode = this._splittedCmd[2]
      newGoldenPose.x = parseFloat(this._splittedCmd[3])
      newGoldenPose.y = parseFloat(this._splittedCmd[4])
      newGoldenPose.thetaInDegrees = parseFloat(this._splittedCmd[5])
      newGoldenPose.valid = 1
    } else {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    if (this.fillCmdInfoWithStepID() !== true) {
      return
    }

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    // this._onlyForMaster = 0
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> SGP: Execute')

      this._results[this._myIndex].state = ECodes.E_UNSPECIFIED
      this._results[this._myIndex].isValid = 1

      if (false) {
        this._results[this._myIndex].state = ECodes.E_COMBINATION_NOT_ALLOWED
      } else {
        if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {
          this._results[this._myIndex].state = 1
        } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
          let newGPInWorld = getWorldFromCam(g_Calibrations, this._shuttlingPose, newGoldenPose.x, newGoldenPose.y, newGoldenPose.thetaInDegrees, 0, 0, 0)
          newGoldenPose.valid = newGPInWorld.valid
          if (newGoldenPose.valid > 0) {
            newGoldenPose.x = newGPInWorld.x
            newGoldenPose.y = newGPInWorld.y
            newGoldenPose.thetaInDegrees = newGPInWorld.thetaInDegrees
          }

          this._results[this._myIndex].state = newGoldenPose.valid
        } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
          let newGPInWorld = getWorldFromImage(g_Calibrations, this._shuttlingPose, newGoldenPose.x, newGoldenPose.y, newGoldenPose.thetaInDegrees, 0, 0, 0)
          newGoldenPose.valid = newGPInWorld.valid
          if (newGoldenPose.valid > 0) {
            newGoldenPose.x = newGPInWorld.x
            newGoldenPose.y = newGPInWorld.y
            newGoldenPose.thetaInDegrees = newGPInWorld.thetaInDegrees
          }
          this._results[this._myIndex].state = newGoldenPose.valid
        }
      }

      this._results[this._myIndex].data.push(newGoldenPose)
      /* if (newGoldenPose.valid > 0) {
        g_TrainedFeatures[featureID][this._gripperID1].x = newGoldenPose.x
        g_TrainedFeatures[featureID][this._gripperID1].y = newGoldenPose.y
        g_TrainedFeatures[featureID][this._gripperID1].thetaInDegrees = newGoldenPose.thetaInDegrees
        g_TrainedFeatures[featureID][this._gripperID1].valid = newGoldenPose.valid
      } */

      tracer.addMessage('<- SGP: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> SGP: Compute result')
      let plcString = ''
      this.checkResultsState()

      let retState = ECodes.E_UNSPECIFIED
      if (this._results.error == ECodes.E_NO_ERROR) {
        let partInfo = g_LooukupTables.partLookup[this._partID1]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees

              g_TrainedFeatures[targetID][this._gripperID1].valid = g_RuntimeFeatures[targetID].valid
              g_TrainedFeatures[targetID][this._gripperID1].x = g_RuntimeFeatures[targetID].x
              g_TrainedFeatures[targetID][this._gripperID1].y = g_RuntimeFeatures[targetID].y
              g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees = g_RuntimeFeatures[targetID].thetaInDegrees

              resIndex++
            }
          }
        }
        InSightFunctions.fnSetEvent(83)
        retState = 1
      } else {
        retState = this._results.error
      }

      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
      tracer.addMessage('<- SGP: Compute result')
      return plcString
    }
  },
  // GGP,<stepID>,<coord>
  // GGP,<Status>,<X>,<Y>,<Theta>
  // SLMP: 3021
  GGP: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3021
    this._numArguments = 1
    this._hasIndex = 1
    this._useAsStepID = 1
    this._onlyForMaster = 0
    var mode = -1

    var featureID = -1

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    if (g_StepsByID[this._index].FeatureIDs.length > 1) {
      this._validCMD = ECodes.E_COMBINATION_NOT_ALLOWED
      return
    } else {
      featureID = g_StepsByID[this._index].FeatureIDs[0]
      // calibration = g_Calibrations[g_FeaturesInfos[featureID].shuttlePose];
    }

    if ((this._splittedCmd[2] == CoordinateSystem.HOME2D) || (this._splittedCmd[2] == CoordinateSystem.CAM2D) || (this._splittedCmd[2] == CoordinateSystem.RAW2D) ||
      (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.HOME2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.CAM2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.RAW2D])) {
      mode = this._splittedCmd[2]
    } else {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    if (this.fillCmdInfoWithStepID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    // this._sendToSlave = 0

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> GGP: Execute')

      this._results[this._myIndex].state = ECodes.E_UNSPECIFIED
      this._results[this._myIndex].isValid = 1

      if (false) {
        this._results[this._myIndex].state = ECodes.E_COMBINATION_NOT_ALLOWED
      } else {
        if (g_TrainedFeatures[featureID][this._gripperID1].valid > 0) {
          let goldenPose = cloneObj(g_TrainedFeatures[featureID][this._gripperID1])
          if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {
            // this._results[this._myIndex].state = 1;

          } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
            let goldenPoseInCam2D = getCamFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees)
            goldenPose.valid = goldenPoseInCam2D.valid
            if (goldenPose.valid > 0) {
              goldenPose.x = goldenPoseInCam2D.x
              goldenPose.y = goldenPoseInCam2D.y
              goldenPose.thetaInDegrees = goldenPoseInCam2D.thetaInDegrees
            }
          } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
            let goldenPoseInRaw2D = getImageFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees, 0, 0, 0)
            goldenPose.valid = goldenPoseInRaw2D.valid
            if (goldenPose.valid > 0) {
              goldenPose.x = goldenPoseInRaw2D.x
              goldenPose.y = goldenPoseInRaw2D.y
              goldenPose.thetaInDegrees = goldenPoseInRaw2D.thetaInDegrees
            }
          }
          this._results[this._myIndex].state = goldenPose.valid
          this._results[this._myIndex].data.push(goldenPose)
        } else {
          this._results[this._myIndex].state = ECodes.E_TARGET_POSE_NOT_TRAINED
        }

        tracer.addMessage('<- GGP: Execute')
        return States.WAITING_FOR_SLAVE_RESULT
      }
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> GGP: Compute result')
      let plcString = ''
      let resultString = ''
      this.checkResultsState()

      let retState = this._results.error
      if (retState === ECodes.E_NO_ERROR) {
        for (let r in this._results) {
          if ((typeof this._results[r] === 'object') && (this._results[r].isNeeded == true)) {
            let data = this._results[r].data[0]
            resultString += InSightFunctions.fnStringf(',%.3f,%.3f,%.3f', data.x, data.y, data.thetaInDegrees)
            retState = this._results[r].isValid
          }
        }
      }
      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resultString)

      tracer.addMessage('<- GGP: Compute result')
      return plcString
    }
    /*
    this.computeTotalResult = function () {
      tracer.addMessage('-> GGP: Compute result')
      let plcString = ''
      let goldenPose = cloneObj(g_TrainedFeatures[featureID][this._gripperID1])

      if (this._results[this._myIndex].isValid > 0) {
        if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {
        // this._results[this._myIndex].state = 1;

        } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
          let goldenPoseInCam2D = getCamFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees)
          goldenPose.valid = goldenPoseInCam2D.valid
          if (goldenPose.valid > 0) {
            goldenPose.x = goldenPoseInCam2D.x
            goldenPose.y = goldenPoseInCam2D.y
            goldenPose.thetaInDegrees = goldenPoseInCam2D.thetaInDegrees
          }
        } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
          let goldenPoseInRaw2D = getImageFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees, 0, 0, 0)
          goldenPose.valid = goldenPoseInRaw2D.valid
          if (goldenPose.valid > 0) {
            goldenPose.x = goldenPoseInRaw2D.x
            goldenPose.y = goldenPoseInRaw2D.y
            goldenPose.thetaInDegrees = goldenPoseInRaw2D.thetaInDegrees
          }
        }
      }

      let resStr = ''
      if (goldenPose.valid > 0) {
        resStr = InSightFunctions.fnStringf(',%.3f,%.3f,%.3f', goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], goldenPose.valid, resStr)

      tracer.addMessage('<- GGP: Compute result')
      return plcString
    }
    */
  },
  // GCP,<stepID>,<coord>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // GCP,<Status>,<X>,<Y>,<Theta>
  // SLMP: 3022

  GCP: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 3022
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsStepID = 1

    var mode = -1
    // var featureID = -1;
    // var calibration = null;

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    if (g_StepsByID[this._index].FeatureIDs.length > 1) {
      this._validCMD = ECodes.E_COMBINATION_NOT_ALLOWED
      return
    }

    // else {
    //    featureID = g_StepsByID[this._index].FeatureIDs[0];
    // calibration = g_Calibrations[g_FeaturesInfos[featureID].shuttlePose];
    // }

    if ((this._splittedCmd[2] == CoordinateSystem.HOME2D) || (this._splittedCmd[2] == CoordinateSystem.CAM2D) || (this._splittedCmd[2] == CoordinateSystem.RAW2D) ||
      (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.HOME2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.CAM2D]) || (this._splittedCmd[2] == CoordinateSystem[CoordinateSystem.RAW2D])) {
      mode = this._splittedCmd[2]
    } else {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    this.copyRobotPose(3)

    if (this.fillCmdInfoWithStepID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.computeTotalResult = function () {
      tracer.addMessage('-> GCP: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      let featureID = g_LooukupTables.stepLookup[this._index].FeatureIDs[0]
      let pose = cloneObj(g_RuntimeFeatures[featureID])

      if (this._results.error == ECodes.E_NO_ERROR) {
        retState = 1 // No error

        let stepInfo = g_LooukupTables.stepLookup[this._index]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0

          let camInfo = stepInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              resIndex++
            }
          }
        }
        pose = cloneObj(g_RuntimeFeatures[featureID])

        if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {

        } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
          let goldenPoseInCam2D = getCamFromWorld(g_Calibrations, this._shuttlingPose, pose.x, pose.y, pose.thetaInDegrees)
          pose.valid = goldenPoseInCam2D.valid
          if (pose.valid > 0) {
            pose.x = goldenPoseInCam2D.x
            pose.y = goldenPoseInCam2D.y
            pose.thetaInDegrees = goldenPoseInCam2D.thetaInDegrees
          }
        } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
          let goldenPoseInRaw2D = getImageFromWorld(g_Calibrations, this._shuttlingPose, pose.x, pose.y, pose.thetaInDegrees, 0, 0, 0)
          pose.valid = goldenPoseInRaw2D.valid
          if (pose.valid > 0) {
            pose.x = goldenPoseInRaw2D.x
            pose.y = goldenPoseInRaw2D.y
            pose.thetaInDegrees = goldenPoseInRaw2D.thetaInDegrees
          }
        }

        retState = pose.valid
      } else {
        retState = this._results.error
      }

      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%.3f,%.3f,%.3f', pose.x, pose.y, pose.thetaInDegrees)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      tracer.addMessage('<- GCP: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },

  // TT,<partID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // TT,<Status>
  // SLMP: 4010
  TT: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4010
    this._numArguments = 6
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsTrainImage'

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 1; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    this.copyRobotPose(2)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.computeTotalResult = function () {
      tracer.addMessage('-> TT Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED

      if (this._results.error == ECodes.E_NO_ERROR) {
        let partInfo = g_LooukupTables.partLookup[this._partID1]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees

              g_TrainedFeatures[targetID][this._gripperID1].valid = g_RuntimeFeatures[targetID].valid
              g_TrainedFeatures[targetID][this._gripperID1].x = g_RuntimeFeatures[targetID].x
              g_TrainedFeatures[targetID][this._gripperID1].y = g_RuntimeFeatures[targetID].y
              g_TrainedFeatures[targetID][this._gripperID1].thetaInDegrees = g_RuntimeFeatures[targetID].thetaInDegrees

              resIndex++
            }
          }
        }
        retState = 1
        InSightFunctions.fnSetEvent(83)
      } else {
        retState = this._results.error
      }

      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)

      tracer.addMessage('<- TT Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // TTR,<partID>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // TTR,<Status>
  // SLMP: 4011
  TTR: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4011
    this._numArguments = 6
    this._hasIndex = 1
    this._useAsPartID = 1
    this._onlyForMaster = 1

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 1; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    this._robotPose.x = parseFloat(this._splittedCmd[2])
    this._robotPose.y = parseFloat(this._splittedCmd[3])
    this._robotPose.z = parseFloat(this._splittedCmd[4])
    this._robotPose.thetaZ = parseFloat(this._splittedCmd[5])
    this._robotPose.thetaY = parseFloat(this._splittedCmd[6])
    this._robotPose.thetaX = parseFloat(this._splittedCmd[7])
    this._robotPose.valid = 1

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> TTR: Execute')

      g_Parts[this._index].trainedRobotPose[this._gripperID1].x = this._robotPose.x
      g_Parts[this._index].trainedRobotPose[this._gripperID1].y = this._robotPose.y
      g_Parts[this._index].trainedRobotPose[this._gripperID1].z = this._robotPose.z
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaZ = this._robotPose.thetaZ
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaY = this._robotPose.thetaY
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaX = this._robotPose.thetaX
      g_Parts[this._index].trainedRobotPose[this._gripperID1].valid = 1

      myLog(g_Parts[this._index])

      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      tracer.addMessage('<- TTR: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> TTR: Compute result')
      let plcString = ''
      let state = 1
      if (this._results[this._myIndex].isValid > 0) {
        state = this._results[this._myIndex].state
        InSightFunctions.fnSetEvent(83)
      }
      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)
      tracer.addMessage('<- TTR: Compute result')
      return plcString
    }
  },
  // XT,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
  // XT,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 4012
  XT: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4012
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }   
    if(!g_Parts.hasOwnProperty(this._partID1)){
      this._validCMD = ECodes.E_INDEX_OUT_OF_RANGE
      return
    }

    if(this._partID2 > 0) {
      this._computeBothParts = 1
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

   resMode = this._splittedCmd[2]
   let resModeInt = parseInt(resMode)
   if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resModeInt === ResultMode.FRAME) || (resModeInt === ResultMode.PICKED) || (resModeInt === ResultMode.GC) || (resModeInt === ResultMode.GCP) ||
     (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
     this._validCMD = ECodes.E_INVALID_ARGUMENT
     return
   }
   if(!isNaN(resModeInt))
   {
     if(resModeInt === 1)
     {
      resMode = 'ABS'
     }
     else if(resModeInt === 2)
     {
      resMode = 'OFF'
     }
   }
   //GSSVN: Extract barcode and send to save image on FTP server
   if (this._splittedCmd.length > 9) {
    let barcode = this._splittedCmd[9]
    InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
    //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
   }
   else{
    let barcode = 'Nan'
    InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
    //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
     }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    if (g_Calibrations[(this._shuttlingPose&MASK_ID_1)].calibration !== null) {
      this._isCameraMoving = g_Calibrations[(this._shuttlingPose&MASK_ID_1)].calibration.isCameraMoving_
    }    

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
	    this._computeBothParts = 0
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }
    this._validCMD = 1

    this.imgAcquired = function () {
      tracer.addMessage('-> XT: Image acquired' + timeTracker.getElapsedTime())
      let features=(this._featureMask & MASK_ID_1)
      
      if(this._computeBothParts == 1){
        
        features = features | ((this._featureMask & MASK_ID_2) >> SHIFT_ID_2)    
      }      
       
      this._enabledFeatures = features
      tracer.addMessage('<- XT: Image acquired' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_TOOLS_DONE
    }

    this.toolsDone = function (t) {
      tracer.addMessage('-> XT: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }

      let transformed = new Feature(0, 0, 0, 0)
      let transformed2 = new Feature(0, 0, 0, 0)

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {        
        if (this.isFeatureEnabled(f) == true) {

          if (g_CurrentFeatures[f].valid > 0) {

            if((this._featureMask & MASK_ID_1) & f){
              transformed = new Feature(0, 0, 0, 0)
              let shuttlingPose = this._shuttlingPose & MASK_ID_1
              let isCameraMoving = this._isCameraMoving & MASK_ID_1
              let isPartMoving = this._isPartMoving & MASK_ID_1

              transformed = getTransformed(g_Calibrations, shuttlingPose, isCameraMoving, isPartMoving, g_CurrentFeatures[f], this._robotPose)
              this._results[this._myIndex].data.push(transformed)
              if(this._results[this._myIndex].state >= 0 ) {
                this._results[this._myIndex].state = transformed.valid
              }
            }

            if(this._computeBothParts == 1) {
              if(((this._featureMask & MASK_ID_2)>>SHIFT_ID_2 ) & f  ){
                transformed2 = new Feature(0, 0, 0, 0)
                let shuttlingPose = (this._shuttlingPose & MASK_ID_2) >> SHIFT_ID_2
                let isCameraMoving = (this._isCameraMoving & MASK_ID_2) >> SHIFT_ID_2
                let isPartMoving = (this._isPartMoving & MASK_ID_2) >> SHIFT_ID_2

                transformed2 = getTransformed(g_Calibrations, shuttlingPose, isCameraMoving, isPartMoving, g_CurrentFeatures[f], this._robotPose)
                this._results[this._myIndex].data.push(transformed2)
                if(this._results[this._myIndex].state >= 0 ) {
                  this._results[this._myIndex].state = transformed2.valid
                }
              }
            }
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }        
        }
      }
      tracer.addMessage('<- XT: Tools done ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XT: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let newRobPose2 = new RobotPose(0, 0, 0, 0, 0, 0, 0)

      this.checkResultsState()

      let retState = ECodes.E_UNSPECIFIED

      if (this._results.error == ECodes.E_NO_ERROR) {
        g_Graphics.ShowCrossHair = []

        let partInfo = g_LooukupTables.partLookup[this._partID1]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              resIndex++
            }

            if(this._computeBothParts == 1){
              let partInfo2 = g_LooukupTables.partLookup[this._partID2]
              let camInfo2 = partInfo2['Cam_' + c]
              let targetID2 = camInfo2['Feature_' + f]
              if (targetID2 > 0) {
                g_RuntimeFeatures[targetID2].valid = this._results[c].data[resIndex].valid
                g_RuntimeFeatures[targetID2].x = this._results[c].data[resIndex].x
                g_RuntimeFeatures[targetID2].y = this._results[c].data[resIndex].y
                g_RuntimeFeatures[targetID2].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
                resIndex++
              }
            }
          }
        }
        newRobPose = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
        retState = newRobPose.valid

        if(this._computeBothParts == 1){
          newRobPose2 = ComputeAlignMode_2(this._partID2, this._partID2, this._gripperID1, resMode, this._robotPose)
          if(retState > 0){
            retState = newRobPose2.valid
          }
        }
      } else {
        retState = this._results.error
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      let resStr = ''
      if (retState > 0) {
        //resStr = InSightFunctions.fnStringf(',%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)
        resStr = InSightFunctions.fnStringf(',%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)        
        if(this._computeBothParts == 1){
          resStr = resStr + InSightFunctions.fnStringf(',%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', newRobPose2.x, newRobPose2.y, newRobPose2.z, newRobPose2.thetaZ, newRobPose2.thetaY, newRobPose2.thetaX)        
        }
      }
      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(newRobPose.x)>g_LimitX)||(Math.abs(newRobPose2.x)>g_LimitX)||(Math.abs(newRobPose.y)>g_LimitY)||(Math.abs(newRobPose2.y)>g_LimitY)||(Math.abs(newRobPose.thetaZ)>g_LimitTheta)||(Math.abs(newRobPose2.thetaZ)>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XT: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
    // XTT,<partID>,<resultMode>,<PocketNumber1>,<PocketNumber2>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Barcode>
  // XTT,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 4017
  XTT: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4017
    this._numArguments = 9
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    //myLogger.addLogMessage(0, "Trace 0")

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }
    

      resMode = this._splittedCmd[2]
      let resModeInt = parseInt(resMode)
      if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resModeInt === ResultMode.FRAME) || (resModeInt === ResultMode.PICKED) || (resModeInt === ResultMode.GC) || (resModeInt === ResultMode.GCP) ||
        (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
      if(!isNaN(resModeInt))
      {
        if(resModeInt === 1)
        {
          resMode = 'ABS'
        }
        else if(resModeInt === 2)
        {
          resMode = 'OFF'
        }
      }
        //Extract barcode
        
        if (this._splittedCmd.length > 10) {
          let barcode = this._splittedCmd[11]
          InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
          //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
          }
        else{
          let barcode = 'Nan'
          InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
          //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
          }
          
    this.copyRobotPose(5)
    
   //myLogger.addLogMessage(0, "Trace 2")

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    //myLogger.addLogMessage(0, "Trace 3")

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }
   
    //myLogger.addLogMessage(0, "Trace 4")
    this._validCMD = 1

    this.computeTotalResult = function () {
      tracer.addMessage('-> XTT: Compute result ' + timeTracker.getElapsedTime())
      
      let plcString = ''
      let featureValid = ''
      let newRobPoseFwd = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseFwd = [0, 0, 0, 0, 0, 0]
      let newRobPoseRev = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseRev = [0, 0, 0, 0, 0, 0]

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED
      let retStateFwd = ECodes.E_UNSPECIFIED
      let retStateRev = ECodes.E_UNSPECIFIED

      let message = ''
      let featureValid = ''
      let isDistanceValid = true
        g_Graphics.ShowCrossHair = []
        let resIndex = 0
        for (let partID = 2; partID >=1; partID--)
        {   
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
            }
          }
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPoseRev = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          combinedPoseRev[3*(partID - 1) + 0] = newRobPoseRev.x
          combinedPoseRev[3*(partID - 1) + 1] = newRobPoseRev.y
          combinedPoseRev[3*(partID - 1) + 2] = newRobPoseRev.thetaZ
          retStateRev = newRobPoseRev.valid
          resIndex++
      }

      resIndex = 0
      for (let partID = 1; partID <= 2; partID++)
      {     
          //myLogger.addLogMessage(0, 'XTF: Run partID ' + partID)
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
            }
          }
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPoseFwd = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          //myLogger.addLogMessage(0, 'XTF: Done ComputeAlign ' + partID)
          combinedPoseFwd[3*(partID - 1) + 0] = newRobPoseFwd.x
          combinedPoseFwd[3*(partID - 1) + 1] = newRobPoseFwd.y
          combinedPoseFwd[3*(partID - 1) + 2] = newRobPoseFwd.thetaZ
          //myLogger.addLogMessage(0, 'XTF: Done partID ' + partID)
          retStateFwd = newRobPoseFwd.valid
          resIndex++
      }
      //} else {
      //    retState = this._results.error
      //  }

      retState = retStateFwd & retStateRev
      let distance = 0.0
      //featureValid = g_NumberFound + ',0,'
      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', featureValid, combinedPoseFwd[0], combinedPoseFwd[1], combinedPoseFwd[2], combinedPoseFwd[3], combinedPoseFwd[4], combinedPoseFwd[5],combinedPoseRev[3], combinedPoseRev[4], combinedPoseRev[5], combinedPoseRev[0], combinedPoseRev[1], combinedPoseRev[2], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPoseFwd[0])>g_LimitX)||(Math.abs(combinedPoseFwd[3])>g_LimitX)||(Math.abs(combinedPoseFwd[1])>g_LimitY)||(Math.abs(combinedPoseFwd[4])>g_LimitY)||(Math.abs(combinedPoseFwd[2])>g_LimitTheta)||(Math.abs(combinedPoseFwd[5])>g_LimitTheta)||(Math.abs(combinedPoseRev[0])>g_LimitX)||(Math.abs(combinedPoseRev[3])>g_LimitX)||(Math.abs(combinedPoseRev[1])>g_LimitY)||(Math.abs(combinedPoseRev[4])>g_LimitY)||(Math.abs(combinedPoseRev[2])>g_LimitTheta)||(Math.abs(combinedPoseRev[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XTA: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  
  // XTF,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C><Barcode>
  // XTF,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<X1>,<Y1>,<T1>,<X2>,<Y2>,<T2>,<Distance>
  // SLMP: 7010
  XTF: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4014
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len-1; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

   resMode = this._splittedCmd[2]
   let resModeInt = parseInt(resMode)
   if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resModeInt === ResultMode.FRAME) || (resModeInt === ResultMode.PICKED) || (resModeInt === ResultMode.GC) || (resModeInt === ResultMode.GCP) ||
     (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
     this._validCMD = ECodes.E_INVALID_ARGUMENT
     return
   }
   if(!isNaN(resModeInt))
   {
     if(resModeInt === 1)
     {
      resMode = 'ABS'
     }
     else if(resModeInt === 2)
     {
      resMode = 'OFF'
     }
   }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    this._validCMD = 1

    //Extract barcode
    if (this._splittedCmd.length > 9) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XT: Compute result ' + timeTracker.getElapsedTime())
      //myLogger.addLogMessage(0, '-> XTF: compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let featureValid = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPose = [0, 0, 0, 0, 0, 0]

     

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED
      let isDistanceValid = true;
      //if (this._results.error == ECodes.E_NO_ERROR) {
        //myLogger.addLogMessage(0, 'XTF: No error, calculating...')
        g_Graphics.ShowCrossHair = []
        let resIndex = 0
        for (let partID = 1; partID <= 2; partID++)
        {   
          //myLogger.addLogMessage(0, 'XTF: Run partID ' + partID)
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            //myLogger.addLogMessage(0, 'XTF Run partID, result data length = ' + this._results[c].data.length + ' TargetID = '+ targetID + ' ResIndex = ' + resIndex)
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              //myLogger.addLogMessage(0, 'XTF: RuntimeFeature {IsValid: ' + g_RuntimeFeatures[targetID].valid + ' X: '+ g_RuntimeFeatures[targetID].x+ ' Y: '+ g_RuntimeFeatures[targetID].y + ' Theta: '+ g_RuntimeFeatures[targetID].thetaInDegrees)
              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
            }
          }
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' PartID1 = ' + this._partID1 +' PartID2 = ' + this._partID2)
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' ResMode = ' + resMode +' robot = ' + this._robotPose.x + ' ' + this._robotPose.y + ' ' + this._robotPose.thetaZ)
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPose = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          //myLogger.addLogMessage(0, 'XTF: Done ComputeAlign ' + partID)
          combinedPose[3*(partID - 1) + 0] = newRobPose.x
          combinedPose[3*(partID - 1) + 1] = newRobPose.y
          combinedPose[3*(partID - 1) + 2] = newRobPose.thetaZ
          //myLogger.addLogMessage(0, 'XTF: Done partID ' + partID)
          retState = newRobPose.valid
          resIndex++
      }
      //} else {
       //   retState = this._results.error
       // }
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f', featureValid, combinedPose[0], combinedPose[1], combinedPose[2], combinedPose[3], combinedPose[4], combinedPose[5], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPose[0])>g_LimitX)||(Math.abs(combinedPose[3])>g_LimitX)||(Math.abs(combinedPose[1])>g_LimitY)||(Math.abs(combinedPose[4])>g_LimitY)||(Math.abs(combinedPose[2])>g_LimitTheta)||(Math.abs(combinedPose[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }
      tracer.addMessage('<- XTF: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XTR,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C><Barcode>
  // XTR,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<X1>,<Y1>,<T1>,<X2>,<Y2>,<T2>,<Distance>
  // SLMP: 7011
  XTR: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4015
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len-1; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)
    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resModeInt === ResultMode.FRAME) || (resModeInt === ResultMode.PICKED) || (resModeInt === ResultMode.GC) || (resModeInt === ResultMode.GCP) ||
      (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    this._validCMD = 1

    //Extract barcode
    if (this._splittedCmd.length > 9) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XTR: Compute result ' + timeTracker.getElapsedTime())
      //myLogger.addLogMessage(0, '-> XTF: compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let featureValid = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPose = [0, 0, 0, 0, 0, 0]


      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED
      let isDistanceValid = true;
      //if (this._results.error == ECodes.E_NO_ERROR) {
        //myLogger.addLogMessage(0, 'XTF: No error, calculating...')
        g_Graphics.ShowCrossHair = []
        let resIndex = 0
        for (let partID = 2; partID >=1; partID--)
        {   
          //myLogger.addLogMessage(0, 'XTF: Run partID ' + partID)
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            //myLogger.addLogMessage(0, 'XTF Run partID, result data length = ' + this._results[c].data.length + ' TargetID = '+ targetID + ' ResIndex = ' + resIndex)
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              //myLogger.addLogMessage(0, 'XTF: RuntimeFeature {IsValid: ' + g_RuntimeFeatures[targetID].valid + ' X: '+ g_RuntimeFeatures[targetID].x+ ' Y: '+ g_RuntimeFeatures[targetID].y + ' Theta: '+ g_RuntimeFeatures[targetID].thetaInDegrees)
              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
            }
          }
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' PartID1 = ' + this._partID1 +' PartID2 = ' + this._partID2)
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' ResMode = ' + resMode +' robot = ' + this._robotPose.x + ' ' + this._robotPose.y + ' ' + this._robotPose.thetaZ)
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPose = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          //myLogger.addLogMessage(0, 'XTF: Done ComputeAlign ' + partID)
          combinedPose[3*(partID - 1) + 0] = newRobPose.x
          combinedPose[3*(partID - 1) + 1] = newRobPose.y
          combinedPose[3*(partID - 1) + 2] = newRobPose.thetaZ
          //myLogger.addLogMessage(0, 'XTF: Done partID ' + partID)
          retState = newRobPose.valid
          resIndex++
      }
      //} else {
      //    retState = this._results.error
      //  }
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }


      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f, %07.2f', featureValid, combinedPose[3], combinedPose[4], combinedPose[5], combinedPose[0], combinedPose[1], combinedPose[2], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPose[0])>g_LimitX)||(Math.abs(combinedPose[3])>g_LimitX)||(Math.abs(combinedPose[1])>g_LimitY)||(Math.abs(combinedPose[4])>g_LimitY)||(Math.abs(combinedPose[2])>g_LimitTheta)||(Math.abs(combinedPose[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XTR: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XTA,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C><Barcode>
  // XTA,1,<Object1 Present/Absent>,<Object2 Present/Absent>,<XFwd1>,<YFwd1>,<TFwd1>,<XFwd2>,<YFwd2>,<TFwd2>,<XRev1>,<YRev1>,<TRev1>,<XRev2>,<YRev2>,<TRev2><Distance>
  // SLMP: 7011
  XTA: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4016
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len-1; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    resMode = this._splittedCmd[2]
    let resModeInt = parseInt(resMode)
    if (!((resModeInt === ResultMode.ABS) || (resModeInt === ResultMode.OFF) || (resModeInt === ResultMode.FRAME) || (resModeInt === ResultMode.PICKED) || (resModeInt === ResultMode.GC) || (resModeInt === ResultMode.GCP) ||
      (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }
    if(!isNaN(resModeInt))
    {
      if(resModeInt === 1)
      {
       resMode = 'ABS'
      }
      else if(resModeInt === 2)
      {
       resMode = 'OFF'
      }
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    this._validCMD = 1

    //Extract barcode
    if (this._splittedCmd.length > 9) {
      let barcode = this._splittedCmd[9]
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }
    else{
      let barcode = 'Nan'
      InSightFunctions.fnSetCellValue('LogImage.Barcode', barcode)
      //myLogger.addLogMessage(0,'Barcode receive: ' + barcode)
      }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XTA: Compute result ' + timeTracker.getElapsedTime())
      //myLogger.addLogMessage(0, '-> XTF: compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let featureValid = ''
      let newRobPoseFwd = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseFwd = [0, 0, 0, 0, 0, 0]
      let newRobPoseRev = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      let combinedPoseRev = [0, 0, 0, 0, 0, 0]

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED
      let retStateFwd = ECodes.E_UNSPECIFIED
      let retStateRev = ECodes.E_UNSPECIFIED

      let message = ''
      let featureValid = ''
      let isDistanceValid = true
      //if (this._results.error == ECodes.E_NO_ERROR) {
        //myLogger.addLogMessage(0, 'XTF: No error, calculating...')
        g_Graphics.ShowCrossHair = []
        let resIndex = 0
        for (let partID = 2; partID >=1; partID--)
        {   
          //myLogger.addLogMessage(0, 'XTF: Run partID ' + partID)
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            //myLogger.addLogMessage(0, 'XTF Run partID, result data length = ' + this._results[c].data.length + ' TargetID = '+ targetID + ' ResIndex = ' + resIndex)
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              //myLogger.addLogMessage(0, 'XTF: RuntimeFeature {IsValid: ' + g_RuntimeFeatures[targetID].valid + ' X: '+ g_RuntimeFeatures[targetID].x+ ' Y: '+ g_RuntimeFeatures[targetID].y + ' Theta: '+ g_RuntimeFeatures[targetID].thetaInDegrees)
              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
            }
          }
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' PartID1 = ' + this._partID1 +' PartID2 = ' + this._partID2)
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' ResMode = ' + resMode +' robot = ' + this._robotPose.x + ' ' + this._robotPose.y + ' ' + this._robotPose.thetaZ)
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPoseRev = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          //myLogger.addLogMessage(0, 'XTF: Done ComputeAlign ' + partID)
          combinedPoseRev[3*(partID - 1) + 0] = newRobPoseRev.x
          combinedPoseRev[3*(partID - 1) + 1] = newRobPoseRev.y
          combinedPoseRev[3*(partID - 1) + 2] = newRobPoseRev.thetaZ
          //myLogger.addLogMessage(0, 'XTF: Done partID ' + partID)
          retStateRev = newRobPoseRev.valid
          resIndex++
      }

      resIndex = 0
      for (let partID = 1; partID <= 2; partID++)
      {     
          //myLogger.addLogMessage(0, 'XTF: Run partID ' + partID)
          let partInfo = g_LooukupTables.partLookup[partID]
          for (let c = 1; c <= MAX_CAMERAS; c++) {
            let camInfo = partInfo['Cam_' + c]
            let targetID = camInfo['Feature_' + partID]
            //myLogger.addLogMessage(0, 'XTF Run partID, result data length = ' + this._results[c].data.length + ' TargetID = '+ targetID + ' ResIndex = ' + resIndex)
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              //myLogger.addLogMessage(0, 'XTF: RuntimeFeature {IsValid: ' + g_RuntimeFeatures[targetID].valid + ' X: '+ g_RuntimeFeatures[targetID].x+ ' Y: '+ g_RuntimeFeatures[targetID].y + ' Theta: '+ g_RuntimeFeatures[targetID].thetaInDegrees)
              /*
              if (this._results[c].data[resIndex].valid)
                featureValid += '1,'
              else{
                featureValid += '0,'
                isDistanceValid = false
              }
              */
            }
          }
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' PartID1 = ' + this._partID1 +' PartID2 = ' + this._partID2)
          //myLogger.addLogMessage(0, 'XTF: Run ' + ' ResMode = ' + resMode +' robot = ' + this._robotPose.x + ' ' + this._robotPose.y + ' ' + this._robotPose.thetaZ)
          if(partID == 1)
          {
            this._partID1 = 1 
          }
          else{
            this._partID1 = 2
          }
          newRobPoseFwd = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          //myLogger.addLogMessage(0, 'XTF: Done ComputeAlign ' + partID)
          combinedPoseFwd[3*(partID - 1) + 0] = newRobPoseFwd.x
          combinedPoseFwd[3*(partID - 1) + 1] = newRobPoseFwd.y
          combinedPoseFwd[3*(partID - 1) + 2] = newRobPoseFwd.thetaZ
          //myLogger.addLogMessage(0, 'XTF: Done partID ' + partID)
          retStateFwd = newRobPoseFwd.valid
          resIndex++
      }
      //} else {
      //    retState = this._results.error
      //  }

      retState = retStateFwd & retStateRev
      let distance = 0.0
      featureValid = g_NumberFound + ',0,'
      if(g_NumberFound==0)
      {
        retState = ECodes.E_FEATURE_NOT_FOUND
      }

      if(g_AdditionalError != 1)
      {
        if(g_AdditionalError == -1)
        {
          retState = ECodes.E_ALIGN_FAIL_PATMAX_CONDITION;
        }
        else if(g_AdditionalError == -2)
        {
            retState = ECodes.E_ALIGN_FAIL_HISTOGRAM_CONDITION;
        }
      }

      if (isDistanceValid)
        distance = Math.sqrt(Math.pow(g_RuntimeFeatures[1].x-g_RuntimeFeatures[2].x,2) + Math.pow(g_RuntimeFeatures[1].y-g_RuntimeFeatures[2].y,2))
      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%s%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f,%07.2f', featureValid, combinedPoseFwd[0], combinedPoseFwd[1], combinedPoseFwd[2], combinedPoseFwd[3], combinedPoseFwd[4], combinedPoseFwd[5],combinedPoseRev[3], combinedPoseRev[4], combinedPoseRev[5], combinedPoseRev[0], combinedPoseRev[1], combinedPoseRev[2], distance)
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      //GSS: Check Limitation
      if(g_IsEnableLimit)
      {
        if((Math.abs(combinedPoseFwd[0])>g_LimitX)||(Math.abs(combinedPoseFwd[3])>g_LimitX)||(Math.abs(combinedPoseFwd[1])>g_LimitY)||(Math.abs(combinedPoseFwd[4])>g_LimitY)||(Math.abs(combinedPoseFwd[2])>g_LimitTheta)||(Math.abs(combinedPoseFwd[5])>g_LimitTheta)||(Math.abs(combinedPoseRev[0])>g_LimitX)||(Math.abs(combinedPoseRev[3])>g_LimitX)||(Math.abs(combinedPoseRev[1])>g_LimitY)||(Math.abs(combinedPoseRev[4])>g_LimitY)||(Math.abs(combinedPoseRev[2])>g_LimitTheta)||(Math.abs(combinedPoseRev[5])>g_LimitTheta))
        {
          retState = ECodes.E_ALIGN_OVER_LIMITATION
          plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], retState)
        }
      }

      tracer.addMessage('<- XTA: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },
  // XTS,<partID>,<resultMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // XTS,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>,<Score>
  // SLMP: 4013
  XTS: function (myIndex, cmdString) { // Step ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 4013
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    var resMode = ResultMode.ABS

    if (this.checkLengthAndSetIndex(0, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 3; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    resMode = parseInt(this._splittedCmd[2])
    let resModeString = this._splittedCmd[2]

    if (!((resMode == ResultMode.ABS) || (resMode == ResultMode.OFF) || (resMode == ResultMode.FRAME) || (resMode == ResultMode.PICKED) ||
      (resModeString === ResultMode[ResultMode.ABS]) || (resModeString === ResultMode[ResultMode.OFF]) || (resModeString === ResultMode[ResultMode.FRAME]) || (resModeString === ResultMode[ResultMode.PICKED]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    this.copyRobotPose(3)

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.toolsDone = function (t) {
      tracer.addMessage('-> XTS: Tools done ' + timeTracker.getElapsedTime())
      if (this._logImageType.length > 2) {
        InSightFunctions.fnSetCellValue(this._logImageType, 0)
      }

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        let score = -1

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
            score = InSightFunctions.fnGetCellValue('Target.' + f + '.Pattern_Score')
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push([transformed, score])
        }
      }
      tracer.addMessage('<- XTS: Tools done ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> XTS: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''
      let scores = ''
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

      this.checkResultsState()

      let retState = ECodes.E_UNSPECIFIED

      if (this._results.error == ECodes.E_NO_ERROR) {
        g_Graphics.ShowCrossHair = []

        let partInfo = g_LooukupTables.partLookup[this._partID1]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex][0].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex][0].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex][0].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex][0].thetaInDegrees
              scores = scores + InSightFunctions.fnStringf(',%.3f', this._results[c].data[resIndex][1])
              resIndex++
            }
          }
        }
        newRobPose = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
        retState = newRobPose.valid
      } else {
        retState = this._results.error
      }

      let resStr = ''
      if (retState > 0) {
        resStr = InSightFunctions.fnStringf(',%.3f,%.3f,%.3f,%.3f,%.3f,%.3f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)
        resStr = resStr + scores
      }

      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], retState, resStr)

      tracer.addMessage('<- XTS: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }
  },

  // CP
  // CP, <Status>
  // SLMP: 5010
  CP: function (myIndex, cmdString) { // Camera ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 5010
    this._numArguments = 0
    this._hasIndex = 0
    this._onlyForMaster = 1

    this._validCMD = 1

    this._index = this._myIndex

    this.execute = function (t) {
      tracer.addMessage('-> CP: Execute')

      for (let i in g_RuntimeFeatures) {
        g_RuntimeFeatures[i].reset()
      }

      this._results[this._myIndex].state = 1
      this._results[this._myIndex].isValid = 1
      tracer.addMessage('<- CP: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> CP: Compute result')
      let plcString = ''
      let state = 1
      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)
      tracer.addMessage('<- CP: Compute result')
      return plcString
    }
  },
  // LF, <StepID>, <ProductID>,[<X>,<Y>,<Z>,<A>,<B>,<C>]
  // LF, <Status>, <Token>, <ProductID>
  // SLMP: 5011
  LF: function (myIndex, cmdString) { // Step ID
    tracer.addMessage('-> LF init ' + timeTracker.getElapsedTime())
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 5011
    this._numArguments = 1
    this._hasIndex = 1
    this._useAsStepID = 1
    this._logImageType = 'LogImage.IsProductionImage'

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_STEPS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 1; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    var productID = parseFloat(this._splittedCmd[2])
    if (this._splittedCmd.length >= 9) {
      this._robotPose.x = parseFloat(this._splittedCmd[3])
      this._robotPose.y = parseFloat(this._splittedCmd[4])
      this._robotPose.z = parseFloat(this._splittedCmd[5])
      this._robotPose.thetaZ = parseFloat(this._splittedCmd[6])
      this._robotPose.thetaY = parseFloat(this._splittedCmd[7])
      this._robotPose.thetaX = parseFloat(this._splittedCmd[8])
      this._robotPose.valid = 1
    }
    if (this.fillCmdInfoWithStepID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.computeTotalResult = function () {
      tracer.addMessage('-> LF: Compute result ' + timeTracker.getElapsedTime())
      let plcString = ''

      this.checkResultsState()
      let retState = ECodes.E_UNSPECIFIED
      if (this._results.error == ECodes.E_NO_ERROR) {
        let partInfo = g_LooukupTables.stepLookup[this._index]

        for (let c = 1; c <= MAX_CAMERAS; c++) {
          let resIndex = 0
          let camInfo = partInfo['Cam_' + c]
          for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
            let targetID = camInfo['Feature_' + f]
            if (targetID > 0) {
              g_RuntimeFeatures[targetID].valid = this._results[c].data[resIndex].valid
              g_RuntimeFeatures[targetID].x = this._results[c].data[resIndex].x
              g_RuntimeFeatures[targetID].y = this._results[c].data[resIndex].y
              g_RuntimeFeatures[targetID].thetaInDegrees = this._results[c].data[resIndex].thetaInDegrees
              resIndex++
            }
          }
        }
        retState = 1
      } else {
        retState = this._results.error
      }

      plcString = InSightFunctions.fnStringf('%s,%d,0,%d', this._splittedCmd[0], retState, productID)

      tracer.addMessage('<- LF: Compute result ' + timeTracker.getElapsedTime())
      return plcString
    }

    tracer.addMessage('<- LF init ' + timeTracker.getElapsedTime())
  },
  // TP,<partID>,<AlignMode>
  // TP,<Status>
  // SLMP: 5012
  TP: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 5012
    this._numArguments = 1
    this._hasIndex = 1
    this._useAsPartID = 1
    this._onlyForMaster = 1

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_PARTS) != true) {
      return
    }
    let len = this._numArguments + this._hasIndex + 1
    for (let i = 2; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    this._alignMode = parseInt(this._splittedCmd[2])

    if (g_Parts.hasOwnProperty(this._index) == false) {
      this._validCMD = ECodes.E_INVALID_PART_ID
      return
    }

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> TP: Execute')
      let validCnt = 0
      for (var f = 0; f < g_Parts[this._index]['runtimeFeatures'].length; f++) {
        if (g_Parts[this._index]['runtimeFeatures'][f].valid > 0) {
          g_Parts[this._index]['trainedFeatures'][f][this._gripperID1].x = g_Parts[this._index]['runtimeFeatures'][f].x
          g_Parts[this._index]['trainedFeatures'][f][this._gripperID1].y = g_Parts[this._index]['runtimeFeatures'][f].y
          g_Parts[this._index]['trainedFeatures'][f][this._gripperID1].thetaInDegrees = g_Parts[this._index]['runtimeFeatures'][f].thetaInDegrees
          g_Parts[this._index]['trainedFeatures'][f][this._gripperID1].valid = g_Parts[this._index]['runtimeFeatures'][f].valid

          validCnt++
        }
      }

      for (let c = 1; c <= MAX_CAMERAS; c++) {
        if (c != this._myIndex) {
          this._results[c].isNeeded = 0
        }
      }

      this._results[this._myIndex].isValid = 1

      if (validCnt == g_Parts[this._index]['runtimeFeatures'].length) {
        this._results[this._myIndex].state = 1
      } else {
        this._results[this._myIndex].state = ECodes.E_PART_NOT_ALL_FEATURES_LOCATED
      }
      tracer.addMessage('<- TP: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> TP: Compute result')
      let plcString = ''
      let state = -1
      tracer.addMessage(this._results)
      if (this._results[this._myIndex].isValid > 0) {
        state = this._results[this._myIndex].state
        if (state > 0) {
          InSightFunctions.fnSetEvent(83)
        }
      }

      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)
      tracer.addMessage('<- TP: Compute result')
      return plcString
    }
  },
  // TPR,<partID>,<AlignMode>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // TPR,<Status>
  // SLMP: 5013
  TPR: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 5013
    this._numArguments = 7
    this._hasIndex = 1
    this._useAsPartID = 1
    this._onlyForMaster = 1

    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_PARTS) != true) {
      return
    }

    let len = this._numArguments + this._hasIndex + 1
    for (let i = 2; i < len; i++) {
      if (isNaN(this._splittedCmd[i])) {
        this._validCMD = ECodes.E_INVALID_ARGUMENT
        return
      }
    }

    // TODO: Check the alignMode and partID
    this._alignMode = parseInt(this._splittedCmd[2])

    this._robotPose.x = parseFloat(this._splittedCmd[3])
    this._robotPose.y = parseFloat(this._splittedCmd[4])
    this._robotPose.z = parseFloat(this._splittedCmd[5])
    this._robotPose.thetaZ = parseFloat(this._splittedCmd[6])
    this._robotPose.thetaY = parseFloat(this._splittedCmd[7])
    this._robotPose.thetaX = parseFloat(this._splittedCmd[8])
    this._robotPose.valid = 1

    if (g_Parts.hasOwnProperty(this._index) == false) {
      this._validCMD = ECodes.E_INVALID_PART_ID
      return
    }

    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    for (let c = 1; c <= MAX_CAMERAS; c++) {
      if (c == this._myIndex) {
        this._results[c].isNeeded = 1
      } else {
        this._results[c].isNeeded = 0
      }
    }
    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> TPR: Execute')

      g_Parts[this._index].trainedRobotPose[this._gripperID1].x = this._robotPose.x
      g_Parts[this._index].trainedRobotPose[this._gripperID1].y = this._robotPose.y
      g_Parts[this._index].trainedRobotPose[this._gripperID1].z = this._robotPose.z
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaZ = this._robotPose.thetaZ
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaY = this._robotPose.thetaY
      g_Parts[this._index].trainedRobotPose[this._gripperID1].thetaX = this._robotPose.thetaX
      g_Parts[this._index].trainedRobotPose[this._gripperID1].valid = this._robotPose.valid

      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      for (let c = 1; c <= MAX_CAMERAS; c++) {
        if (c != this._myIndex) {
          this._results[c].isNeeded = 0
        }
      }

      tracer.addMessage('<- TPR: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> TPR: Compute result')
      let plcString = ''
      let state = 1
      if (this._results[this._myIndex].isValid > 0) {
        state = this._results[this._myIndex].state
        InSightFunctions.fnSetEvent(83)
      }
      plcString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)
      tracer.addMessage('<- TPR: Compute result')
      return plcString
    }
  },
  // GP,<partID>,<AlignMode>,<ResultMode>, [current motion pose]
  // GP,<Status>,<X>,<Y>,<Z>,<A>,<B>,<C>
  // SLMP: 5014
  GP: function (myIndex, cmdString) { // Part ID
    CommandBase.call(this, myIndex, cmdString)
    this._slmpCode = 5014
    this._numArguments = 1
    this._hasIndex = 1
    this._useAsPartID = 1
    this._onlyForMaster = 1

    var resMode = ResultMode.ABS

    // this.checkLengthAndSetIndex(1, RECIPE_MAX_PARTS);
    if (this.checkLengthAndSetIndex(1, RECIPE_MAX_PARTS) != true) {
      return
    }

    this._alignMode = parseInt(this._splittedCmd[2])

    resMode = this._splittedCmd[3]
    if (!((resMode === ResultMode.ABS) || (resMode === ResultMode.OFF) || (resMode === ResultMode.FRAME) || (resMode === ResultMode.PICKED) || (resMode === ResultMode.GC) || (resMode === ResultMode.GCP) ||
      (resMode === ResultMode[ResultMode.ABS]) || (resMode === ResultMode[ResultMode.OFF]) || (resMode === ResultMode[ResultMode.FRAME]) || (resMode === ResultMode[ResultMode.PICKED]) || (resMode === ResultMode[ResultMode.GC]) || (resMode === ResultMode[ResultMode.GCP]))) {
      this._validCMD = ECodes.E_INVALID_ARGUMENT
      return
    }

    if (this._splittedCmd.length >= 10) {
      for (let i = 4; i < 10; i++) {
        if (isNaN(this._splittedCmd[i])) {
          this._validCMD = ECodes.E_INVALID_ARGUMENT
          return
        }
      }

      this._robotPose.x = parseFloat(this._splittedCmd[4])
      this._robotPose.y = parseFloat(this._splittedCmd[5])
      this._robotPose.z = parseFloat(this._splittedCmd[6])
      this._robotPose.thetaZ = parseFloat(this._splittedCmd[7])
      this._robotPose.thetaY = parseFloat(this._splittedCmd[8])
      this._robotPose.thetaX = parseFloat(this._splittedCmd[9])
      this._robotPose.valid = 1
    }

    if (g_Parts.hasOwnProperty(this._partID1) == false) {
      this._validCMD = ECodes.E_INVALID_PART_ID
      return
    }
    if (this.fillCmdInfoWithPartID() !== true) {
      return
    }
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
      myLogger.addLogMessage(0, 'Part ID 1: ' + this._partID1.toString())
      myLogger.addLogMessage(0, 'Part ID 2: ' + this._partID2.toString())
      myLogger.addLogMessage(0, 'Gripper ID: ' + this._gripperID1.toString())
    }

    this._validCMD = 1

    this.execute = function (t) {
      tracer.addMessage('-> GP: Execute')
      let validCnt = 0
      for (var f = 0; f < g_Parts[this._partID1]['runtimeFeatures'].length; f++) {
        if (g_Parts[this._partID1]['runtimeFeatures'][f].valid > 0) {
          validCnt++
        }
      }

      for (let c = 1; c <= MAX_CAMERAS; c++) {
        if (c != this._myIndex) {
          this._results[c].isNeeded = 0
        }
      }
      this._results[this._myIndex].isValid = 1
      if (validCnt == g_Parts[this._partID1]['runtimeFeatures'].length) {
        this._results[this._myIndex].state = 1
      } else {
        this._results[this._myIndex].state = ECodes.E_PART_NOT_ALL_FEATURES_LOCATED
      }

      tracer.addMessage('<- GP: Execute')
      return States.WAITING_FOR_SLAVE_RESULT
    }

    this.computeTotalResult = function () {
      tracer.addMessage('-> GP: Compute result')
      let plcString = ''
      // let state = ECodes.E_INTERNAL_ERROR;
      let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
      if (this._results[this._myIndex].isValid > 0) {
        // newRobPose.valid = this._results[this._myIndex].state;
        if (this._alignMode == 1) {
          if (this._robotPose.valid > 0) {
            newRobPose = ComputeAlignMode_1(this._partID1, this._gripperID1, resMode, this._robotPose)
          }
        } else if (this._alignMode == 2) {
          if (this._robotPose.valid > 0) {
            newRobPose = ComputeAlignMode_2(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          }
        } else if (this._alignMode == 3) {
          if (this._robotPose.valid > 0) {
            newRobPose = ComputeAlignMode_3(this._partID1, this._partID2, this._gripperID1, resMode, this._robotPose)
          }
        } else {
          newRobPose.valid = ECodes.E_NOT_SUPPORTED
        }
      }
      let resStr = ''
      if (newRobPose.valid > 0) {
        resStr = InSightFunctions.fnStringf(',%.3f,%.3f,%.3f,%.3f,%.3f,%.3f', newRobPose.x, newRobPose.y, newRobPose.z, newRobPose.thetaZ, newRobPose.thetaY, newRobPose.thetaX)
      }
      plcString = InSightFunctions.fnStringf('%s,%d%s', this._splittedCmd[0], newRobPose.valid, resStr)
      tracer.addMessage('<- GP: Compute result')
      return plcString
    }
  }

}

//* ***************************************************************************/
//* ***************************************************************************/
function SlaveBase (myIndex, cmdString) {
  this._myIndex = myIndex

  this._cmdString = cmdString
  this._splittedCmd = cmdString.split(',')

  this._enabledFeatures = 0
  this._featureMask = parseInt(this._splittedCmd[1])
  this._exposureSet = parseInt(this._splittedCmd[2])
  this._shuttlingPose = parseInt(this._splittedCmd[3])
  this._isCameraMoving = parseInt(this._splittedCmd[4]) || g_Settings.IsRobotMounted
  // InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)

  this._isPartMoving = parseInt(this._splittedCmd[5])

  this._numArguments = -1

  this._index = -1

  this._validCMD = 1
  this._gripperID1 = 0

  this._robotPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)
  this._results = {}
  this._results.error = ECodes.E_NO_ERROR
  // this._results.overAllState = 0

  this._results[this._myIndex] = new Result()
  this._results[this._myIndex].isNeeded = 1
};
SlaveBase.prototype.isValid = function () {
  return this._validCMD
}
/*
SlaveBase.prototype.getEnabledFeatures = function () {
  return this._enabledFeatures
} */

SlaveBase.prototype.execute = function (t) {
  tracer.addMessage('-> Execute ' + timeTracker.getElapsedTime())

  this._enabledFeatures = 0
  // InSightFunctions.fnSetCellValue("Target.1.Enable", 0);
  // InSightFunctions.fnSetCellValue("Target.2.Enable", 0);
  InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', this._exposureSet - 1)

  if (t.triggerMode == 32) {
    InSightFunctions.fnSetEvent(32)
  }

  tracer.addMessage('<- Execute ' + timeTracker.getElapsedTime())
  return States.WAITING_FOR_IMAGE_ACQUIRED
}

SlaveBase.prototype.imgAcquired = function () {
  tracer.addMessage('-> Image acquired ' + timeTracker.getElapsedTime())

  this._enabledFeatures = this._featureMask
  /* for (let i = 1; i <= MAX_FEATURES_PER_CAMERA; i++) {
         if (this.isFeatureEnabled(i) == true) {
             InSightFunctions.fnSetCellValue("Target." + i + ".Enable", 1);
         }
     } */
  tracer.addMessage('<- Image acquired ' + timeTracker.getElapsedTime())

  return States.WAITING_FOR_TOOLS_DONE
}
SlaveBase.prototype.toolsDone = function (t) {
  tracer.addMessage('-> Tools done ' + timeTracker.getElapsedTime())

  this._results[this._myIndex]['isValid'] = 1
  for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
    let transformed = new Feature(0, 0, 0, 0)

    if (this.isFeatureEnabled(f) == true) {
      if (g_CurrentFeatures[f].valid > 0) {
        transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
        this._results[this._myIndex].state = transformed.valid
      } else {
        this._results[this._myIndex].state = g_CurrentFeatures[f].valid
      }
      this._results[this._myIndex].data.push(transformed)
    }
  }

  tracer.addMessage('<- Tools done ' + timeTracker.getElapsedTime())

  return States.WAITING_FOR_NEW_COMMAND
}
SlaveBase.prototype.computeTotalResult = function () {
  tracer.addMessage('-> Compute result ' + timeTracker.getElapsedTime())
  let resString = ''
  let errCnt = 0
  let errCode

  if (this._results['1'].isValid > 0) {
    if (this._results['1'].state <= 0) {
      errCnt++
      errCode = this._results['1'].state
    }
  }

  let state = ECodes.E_INTERNAL_ERROR
  if (errCnt == 0) {
    state = 1 // No error
  } else {
    state = errCode
  }

  resString = InSightFunctions.fnStringf('%s,%d', this._splittedCmd[0], state)

  tracer.addMessage('<-  Compute result ' + timeTracker.getElapsedTime())
  return resString
}

SlaveBase.prototype.isFeatureEnabled = function (featureID) {
  return !!(this._featureMask & (1 << (featureID - 1)))
}
SlaveBase.prototype.copyRobotPose = function (start) {
  this._robotPose.x = parseFloat(this._splittedCmd[start])
  this._robotPose.y = parseFloat(this._splittedCmd[start + 1])
  this._robotPose.z = parseFloat(this._splittedCmd[start + 2])
  this._robotPose.thetaZ = parseFloat(this._splittedCmd[start + 3])
  this._robotPose.thetaY = parseFloat(this._splittedCmd[start + 4])
  this._robotPose.thetaX = parseFloat(this._splittedCmd[start + 5])
  this._robotPose.valid = 1
}
// inheritPseudoClass(SlaveBase, SlaveCommands);

var slaveCommands = {
  myIndex: 1,
  GS: function (myIndex, cmdString) {
    tracer.addMessage('-> GS init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    tracer.addMessage('<- GS init ' + +timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> GS: Execute')
      let major = InSightFunctions.fnGetCellValue('Version.Major')
      let minor = InSightFunctions.fnGetCellValue('Version.Minor')
      let subMinor = InSightFunctions.fnGetCellValue('Version.SubMinor')
      let jobName = InSightFunctions.fnGetCellValue('Internal.Recipe.JobName')

      this._results[this._myIndex].data.push({ 'Major': major, 'Minor': minor, 'SubMinor': subMinor, 'JobName': jobName })
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1
      tracer.addMessage('<- GS: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  GV: function (myIndex, cmdString) {
    tracer.addMessage('-> GV init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)

    tracer.addMessage('<- GV init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> GV: Execute')
      let major = InSightFunctions.fnGetCellValue('Version.Major')
      let minor = InSightFunctions.fnGetCellValue('Version.Minor')
      let subMinor = InSightFunctions.fnGetCellValue('Version.SubMinor')
      this._results[this._myIndex].data.push({ 'Major': major, 'Minor': minor, 'SubMinor': subMinor })
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1
      tracer.addMessage('<- GV: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },

  TM: function (myIndex, cmdString) {
    tracer.addMessage('-> TM init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    var triggerMode = parseInt(this._splittedCmd[6])
    tracer.addMessage('<- TM init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> TM: Execute')
      tracer.addMessage('Triggermode ' + triggerMode)
      this._results[this._myIndex].isValid = 1
      if ((triggerMode == 0) || (triggerMode == 1)) {
        if (triggerMode == 0) {
          triggerMode = 32
        } else {
          triggerMode = 0
        }

        InSightFunctions.fnSetCellValue('TriggerMode', triggerMode)
        this._results[this._myIndex].state = 1
      } else {
        this._results[this._myIndex].state = ECodes.E_INVALID_ARGUMENT
      }

      tracer.addMessage('<- TM: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },

  HEB: function (myIndex, cmdString) {
    tracer.addMessage('-> HEB init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    tracer.addMessage('<- HEB init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> HEB: Execute')
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      // g_HEB_Received = 1;
      g_HECalibrationSettings = new HECalibrationSettings()
      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1
      tracer.addMessage('<- HEB: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  HE: function (myIndex, cmdString) {
    tracer.addMessage('-> HE init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    var featureID = this._featureMask// TODO:
    if (featureID == 0) {
      featureID = 1
    }
    // g_SelectedCalibration = featureID;
    if (g_HECalibrationSettings != null) {
      if (g_HECalibrationSettings.calibrationID == -1) {
        g_HECalibrationSettings.calibrationID = featureID
        g_Calibrations[featureID] = new Calibration(featureID)
        g_Calibrations[featureID]['calibrationData']['isMoving'] = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
        InSightFunctions.fnSetCellValue('HECalibration.' + featureID + '.IsCameraMoving', g_Calibrations[featureID]['calibrationData']['isMoving'])
      } else if (g_HECalibrationSettings.calibrationID != featureID) {
        this._validCMD.ECodes.E_NOT_SUPPORTED
      }
    } else {
      this._validCMD = ECodes.E_NO_START_COMMAND
    }

    tracer.addMessage('<- HE init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> HE: Tools done')
      g_Graphics['ShowCalibrationPoints_' + featureID] = 1
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')
      let targetTrained = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained')
      this._results[this._myIndex].isValid = 1

      if (targetTrained > 0) {
        if (targetValid > 0) {
          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          g_Calibrations[featureID].calibrationData.targetX.push(targetX)
          g_Calibrations[featureID].calibrationData.targetY.push(targetY)
          g_Calibrations[featureID].calibrationData.targetTheta.push(targetAngle)
          g_Calibrations[featureID].calibrationData.targetValid.push(targetValid)
          g_Calibrations[featureID].calibrationData.robotX.push(this._robotPose.x)
          g_Calibrations[featureID].calibrationData.robotY.push(this._robotPose.y)
          g_Calibrations[featureID].calibrationData.robotTheta.push(this._robotPose.thetaZ)

          g_Calibrations[featureID].calibrationData.count = g_Calibrations[featureID].calibrationData.targetX.length
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
      }

      tracer.addMessage('<- HE: Tools done')
      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  HEE: function (myIndex, cmdString) {
    tracer.addMessage('-> HEE init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)

    if (g_HECalibrationSettings == null) {
      this._validCMD = ECodes.E_NO_START_COMMAND
      return
    }
    var selectedCalibration = g_HECalibrationSettings.calibrationID

    tracer.addMessage('<- HEE init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> HEE: Execute')
      g_Graphics['ShowCalibrationPoints_' + selectedCalibration] = 1
      this._results[this._myIndex].isValid = 1
      let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
      var valid = t.myCalibrations.CheckData(g_Calibrations[selectedCalibration].calibrationData, isMoving)
      if (valid > 0) {
        g_Calibrations[selectedCalibration].computeCalibration()
        if (g_Calibrations[selectedCalibration].runstatus > 0) {
          g_Calibrations[selectedCalibration].saveCalibrationDataToFile()
          InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
      }
      g_HECalibrationSettings = null
      tracer.addMessage('<- HEE: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  ACB: function (myIndex, cmdString) {
    tracer.addMessage('-> ACB init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    var featureID = this._featureMask// TODO:

    tracer.addMessage('<- ACB init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> ACB: Tools done')
      InSightFunctions.fnSetCellValue('HECalibration.Selector', featureID - 1)
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      InSightFunctions.fnSetCellValue('HECalibration.' + featureID + '.IsCameraMoving', this._isCameraMoving)

      g_AutoCalibRuntime = {}
      g_AutoCalibRuntime = g_Settings.AutoCalibration
      g_AutoCalibRuntime['innerDist'] = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Trainregion.InnerDist')
      g_AutoCalibRuntime['loopCnt'] = 0
      g_AutoCalibRuntime['stepCount'] = 0
      g_AutoCalibRuntime['direction'] = 1
      g_AutoCalibRuntime['minDist_Pixel'] = 55

      g_AutoCalibRuntime['rotAngle'] = (g_AutoCalibRuntime['AngleMax'] - g_AutoCalibRuntime['AngleMin']) / 10
      g_AutoCalibRuntime['advCalibPoints'] = []

      g_AutoCalibRuntime['lastMoveDistance_X'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['lastMoveDistance_Y'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['angleCompX'] = START_MOVE_DISTANCE_MM
      g_AutoCalibRuntime['angleCompY'] = START_MOVE_DISTANCE_MM
      /** */

      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')
      this._results['1'].isValid = 1

      if (InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained') > 0) {
        if (targetValid > 0) {
          let firstPos = { 'X': this._robotPose.x, 'Y': this._robotPose.y, 'Z': this._robotPose.z, 'A': this._robotPose.thetaZ, 'B': this._robotPose.thetaY, 'C': this._robotPose.thetaX }
          g_AutoCalibRuntime['FirstPos'] = firstPos
          g_AutoCalibRuntime['PreCalibration'] = { 'Calibrated': 0 }
          g_AutoCalibRuntime['PreCalibPoses'] = []
          g_AutoCalibRuntime['CalibPoints'] = []
          g_AutoCalibRuntime['RobCalibPoses'] = []
          g_AutoCalibRuntime['NextRobotPose'] = {}
          g_AutoCalibRuntime['Compensation'] = {}
          g_AutoCalibRuntime['LastNextPos'] = { 'X': 0, 'Y': 0, 'Z': 0, 'A': 0, 'B': 0, 'C': 0 }

          g_Graphics['ShowCalibrationPoints_' + featureID] = 1

          t.myCalibrations.calibrations[featureID] = new Calibration(featureID)

          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          let nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)

          if (nextRobotPose.Valid) {
            nextRobotPose.NextX = Math.round(nextRobotPose.NextX * 10000) / 10000
            nextRobotPose.NextY = Math.round(nextRobotPose.NextY * 10000) / 10000
            nextRobotPose.NextAngle = Math.round(nextRobotPose.NextAngle * 10000) / 10000
          }
          this._results[this._myIndex].state = nextRobotPose.Valid
          this._results[this._myIndex].data = nextRobotPose
          // this._results[this._myIndex].data = InSightFunctions.fnStringf("%.6f,%.6f,%.6f,%.6f,%.6f,%.6f",nextRobotPose.NextX,nextRobotPose.NextY,this._robotPose.z,nextRobotPose.NextAngle,this._robotPose.thetaY,this._robotPose.thetaX);
        } else {
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
          this._results[this._myIndex].data = []
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
        this._results[this._myIndex].data = []
      }

      tracer.addMessage('<- ACB: Tools done')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  AC: function (myIndex, cmdString) {
    tracer.addMessage('-> AC init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    var featureID = this._featureMask// TODO:

    tracer.addMessage('<- AC init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> AC Tools done')

      var nextRobotPose = { 'Valid': 0 }
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')

      if (InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained') > 0) {
        if (targetValid > 0) {
          let targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          let targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          let targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          if (g_AutoCalibRuntime['PreCalibration'].Calibrated == 0) {
            nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)
          } else {
            g_Graphics['ShowCalibrationPoints_' + featureID] = 1
            let calib = t.myCalibrations.calibrations[featureID]

            calib.calibrationData.targetX.push(targetX)
            calib.calibrationData.targetY.push(targetY)
            calib.calibrationData.targetTheta.push(targetAngle)
            calib.calibrationData.targetValid.push(targetValid)
            calib.calibrationData.robotX.push(this._robotPose.x)
            calib.calibrationData.robotY.push(this._robotPose.y)
            calib.calibrationData.robotTheta.push(this._robotPose.thetaZ)

            calib.calibrationData.count = calib.calibrationData.targetX.length

            nextRobotPose = t.myCalibrations.doAutoCalibration(this._robotPose.x, this._robotPose.y, this._robotPose.z, this._robotPose.thetaZ, this._robotPose.thetaY, this._robotPose.thetaX, targetX, targetY, targetAngle, targetValid)

            if (nextRobotPose.Valid == 1) {
              let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
              var valid = t.myCalibrations.CheckData(calib.calibrationData, isMoving)
              if (valid > 0) {
                calib.computeCalibration()
                if (calib.runstatus > 0) {
                  calib.saveCalibrationDataToFile()
                  InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
                } else {

                }
              } else {
                console.log('Invalid calibration data! Calibration not saved!')
                // TODO: t.initCalibData();
                // this.calibData["LogMessage"]=logMessage;
              }
            }
          }

          if (nextRobotPose.Valid) {
            nextRobotPose.NextX = Math.round(nextRobotPose.NextX * 10000) / 10000
            nextRobotPose.NextY = Math.round(nextRobotPose.NextY * 10000) / 10000
            nextRobotPose.NextAngle = Math.round(nextRobotPose.NextAngle * 10000) / 10000
          }

          this._results[this._myIndex].isValid = 1
          this._results[this._myIndex].state = nextRobotPose.Valid
          this._results[this._myIndex].data = nextRobotPose
        } else {
          this._results[this._myIndex].isValid = 1
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
          this._results[this._myIndex].data = []
        }
      } else {
        this._results[this._myIndex].isValid = 1
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
        this._results[this._myIndex].data = []
      }
      tracer.addMessage('<- AC Tools done')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  CCB: function (myIndex, cmdString) {
    tracer.addMessage('-> CCB init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)

    var swapHandedness = parseInt(this._splittedCmd[6])
    var featureOffsetX = parseFloat(this._splittedCmd[7])
    var featureOffsetY = parseFloat(this._splittedCmd[8])

    tracer.addMessage('<- CCB init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> CCB: Execute')
      InSightFunctions.fnSetCellValue('HECalibration.IsCameraMoving', this._isCameraMoving)
      InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 0)
      g_CustomCalibrationSettings = new CustomCalibrationSettings(swapHandedness, featureOffsetX, featureOffsetY, -1)

      this._results[this._myIndex].isValid = 1
      this._results[this._myIndex].state = 1

      tracer.addMessage('<- CCB: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  CC: function (myIndex, cmdString) {
    tracer.addMessage('-> CC init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)

    if (g_CustomCalibrationSettings == null) {
      this._validCMD = ECodes.E_NO_START_COMMAND
      return
    }

    this.copyRobotPose(7)
    var featureID = this._featureMask// TODO:
    if (featureID == 0) {
      featureID = 1
    }

    if (g_CustomCalibrationSettings.calibrationID < 1) {
      g_CustomCalibrationSettings.calibrationID = featureID
      g_Calibrations[featureID] = new Calibration(featureID)
    }

    tracer.addMessage('<- CC init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> CC: Tools done')
      g_Graphics['ShowCalibrationPoints_' + featureID] = 1
      let targetValid = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Valid')
      let targetTrained = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern.Trained')
      this._results[this._myIndex].isValid = 1

      if (targetTrained > 0) {
        if (targetValid > 0) {
          var targetX = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_X')
          var targetY = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Y')
          var targetAngle = InSightFunctions.fnGetCellValue('Target.' + featureID + '.Pattern_Angle')

          g_Calibrations[featureID].calibrationData.targetX.push(targetX)
          g_Calibrations[featureID].calibrationData.targetY.push(targetY)
          g_Calibrations[featureID].calibrationData.targetTheta.push(targetAngle)
          g_Calibrations[featureID].calibrationData.targetValid.push(targetValid)
          g_Calibrations[featureID].calibrationData.robotX.push(this._robotPose.x)
          g_Calibrations[featureID].calibrationData.robotY.push(this._robotPose.y)
          g_Calibrations[featureID].calibrationData.robotTheta.push(this._robotPose.thetaZ)

          g_Calibrations[featureID].calibrationData.count = g_Calibrations[featureID].calibrationData.targetX.length
          this._results[this._myIndex].state = 1
        } else {
          this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_FOUND
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_FEATURE_NOT_TRAINED
      }

      tracer.addMessage('<- CC: Tools done')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  CCE: function (myIndex, cmdString) {
    tracer.addMessage('-> CCE init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)

    if (g_CustomCalibrationSettings == null) {
      this._validCMD = ECodes.E_NO_START_COMMAND
      return
    }

    var selectedCalibration = g_CustomCalibrationSettings.calibrationID
    tracer.addMessage('selectedCalibration ' + selectedCalibration)

    tracer.addMessage('<- CCE init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> CCE: Execute')
      g_Graphics['ShowCalibrationPoints_' + selectedCalibration] = 1
      this._results[this._myIndex].isValid = 1
      let isMoving = InSightFunctions.fnGetCellValue('HECalibration.IsCameraMoving')
      var ccData = t.myCalibrations.doCustomCalibration(g_Calibrations[selectedCalibration].calibrationData,
        g_CustomCalibrationSettings.swapHandedness, g_CustomCalibrationSettings.featureOffsetX, g_CustomCalibrationSettings.featureOffsetY)

      if (ccData != 0) {
        var valid = t.myCalibrations.CheckData(g_Calibrations[selectedCalibration].calibrationData, isMoving)

        if (valid > 0) {
          g_Calibrations[selectedCalibration].computeCalibration()
          if (g_Calibrations[selectedCalibration].runstatus > 0) {
            g_Calibrations[selectedCalibration].saveCalibrationDataToFile()
            InSightFunctions.fnSetCellValue('HECalibration.NewCalibrationDone', 1)
            this._results[this._myIndex].state = 1
          } else {
            this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
          }
        } else {
          this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
        }
      } else {
        this._results[this._myIndex].state = ECodes.E_CALIBRATION_FAILED
      }

      g_CustomCalibrationSettings = null
      tracer.addMessage('<- CCE: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },

  TA: function (myIndex, cmdString) {
    tracer.addMessage('-> TA init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    tracer.addMessage('<- TA init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> TA: Tools done ' + timeTracker.getElapsedTime())

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          g_TrainedFeatures[f][this._gripperID1] = new Feature(0, 0, 0, 0)
          g_TrainedFeatures[f][this._gripperID1].valid = transformed.valid
          g_TrainedFeatures[f][this._gripperID1].x = transformed.x
          g_TrainedFeatures[f][this._gripperID1].y = transformed.y
          g_TrainedFeatures[f][this._gripperID1].thetaInDegrees = transformed.thetaInDegrees

          this._results[this._myIndex].data.push(transformed)
        }
      }

      InSightFunctions.fnSetEvent(83)

      tracer.addMessage('<- TA: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  XA: function (myIndex, cmdString) {
    tracer.addMessage('-> XA init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- XA init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> XA: Tools done ' + timeTracker.getElapsedTime())

      g_Graphics.ShowCrossHair = []

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push(transformed)

          if (g_TrainedFeatures[f][this._gripperID1].valid > 0) {
            let imgPos = []
            imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[f][this._gripperID1].x, g_TrainedFeatures[f][this._gripperID1].y, g_TrainedFeatures[f][this._gripperID1].thetaInDegrees, 0, 0, 0)
            g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
          }
        }
      }

      tracer.addMessage('<- XA: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  XAS: function (myIndex, cmdString) {
    tracer.addMessage('-> XAS init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    tracer.addMessage('<- XAS init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> XAS: Tools done ' + timeTracker.getElapsedTime())

      g_Graphics.ShowCrossHair = []

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        let score = -1

        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
            score = InSightFunctions.fnGetCellValue('Target.' + f + '.Pattern_Score')
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push([transformed, score])

          if (g_TrainedFeatures[f][this._gripperID1].valid > 0) {
            let imgPos = []
            imgPos = getImageFromWorld(g_Calibrations, this._shuttlingPose, g_TrainedFeatures[f][this._gripperID1].x, g_TrainedFeatures[f][this._gripperID1].y, g_TrainedFeatures[f][this._gripperID1].thetaInDegrees, 0, 0, 0)
            g_Graphics.ShowCrossHair.push([imgPos.x, imgPos.y, imgPos.thetaInDegrees, imgPos.valid])
          }
        }
      }

      tracer.addMessage('<- XAS: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_SLAVE_RESULT
    }
  },
  XI: function (myIndex, cmdString) {
    tracer.addMessage('-> XI init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    var inspectionID = parseInt(this._splittedCmd[6])
    tracer.addMessage('<- XI init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> XI: Execute')
      this._enabledFeatures = 0
      if (g_Inspections.hasOwnProperty(inspectionID)) {
        setInspectionAcqSettings(inspectionID, 1)
        InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', 2)

        if (t.triggerMode == 32) {
          InSightFunctions.fnSetEvent(32)
        }
      } else {
        this._results[this._myIndex].isValid = 1
        this._results[this._myIndex].state = ECodes.E_INVALID_ARGUMENT
        this._results[this._myIndex].data.push({})
        this._results.error = ECodes.E_INVALID_ARGUMENT
      }

      tracer.addMessage('<- XI: Execute')
      return States.WAITING_FOR_IMAGE_ACQUIRED
    }

    this.imgAcquired = function () {
      tracer.addMessage('-> XI: Image acquired ' + timeTracker.getElapsedTime())
      this._featureMask = inspectionID << 8
      this._enabledFeatures = this._featureMask

      tracer.addMessage('<- XI: Image acquired ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_TOOLS_DONE
    }

    this.toolsDone = function (t) {
      tracer.addMessage('-> XI: Tools done ' + timeTracker.getElapsedTime())

      tracer.addMessage('<- XI: Tools done ' + timeTracker.getElapsedTime())
      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  SGP: function (myIndex, cmdString) {
    tracer.addMessage('-> SGP init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    var mode = this._splittedCmd[6]
    var newGoldenPose = new Feature(0, 0, 0, 0)
    let featureID = this._featureMask
    // let calibration = g_Calibrations[this._shuttlingPose];

    newGoldenPose.x = parseFloat(this._splittedCmd[7])
    newGoldenPose.y = parseFloat(this._splittedCmd[8])
    newGoldenPose.thetaInDegrees = parseFloat(this._splittedCmd[9])

    newGoldenPose.valid = 1

    tracer.addMessage('<- SGP init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> SGP: Execute')

      this._results[this._myIndex].state = ECodes.E_UNSPECIFIED
      this._results[this._myIndex].isValid = 1

      if (false) {
        this._results[this._myIndex].state = ECodes.E_COMBINATION_NOT_ALLOWED
      } else {
        if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {
          this._results[this._myIndex].state = 1
        } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
          let newGPInWorld = getWorldFromCam(g_Calibrations, this._shuttlingPose, newGoldenPose.x, newGoldenPose.y, newGoldenPose.thetaInDegrees, 0, 0, 0)
          newGoldenPose.valid = newGPInWorld.valid
          if (newGoldenPose.valid > 0) {
            newGoldenPose.x = newGPInWorld.x
            newGoldenPose.y = newGPInWorld.y
            newGoldenPose.thetaInDegrees = newGPInWorld.thetaInDegrees
          }

          this._results[this._myIndex].state = newGoldenPose.valid
        } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
          let newGPInWorld = getWorldFromImage(g_Calibrations, this._shuttlingPose, newGoldenPose.x, newGoldenPose.y, newGoldenPose.thetaInDegrees, 0, 0, 0)
          newGoldenPose.valid = newGPInWorld.valid
          if (newGoldenPose.valid > 0) {
            newGoldenPose.x = newGPInWorld.x
            newGoldenPose.y = newGPInWorld.y
            newGoldenPose.thetaInDegrees = newGPInWorld.thetaInDegrees
          }
          this._results[this._myIndex].state = newGoldenPose.valid
        }
      }

      if (newGoldenPose.valid > 0) {
        if (!g_TrainedFeatures.hasOwnProperty(featureID)) {
          g_TrainedFeatures[featureID] = []
          for (let g = 0; g < MAX_GRIPPERS; g++) {
            g_TrainedFeatures[featureID][g] = new Feature(0, 0, 0, 0)
          }
        }

        g_TrainedFeatures[featureID][this._gripperID1].x = newGoldenPose.x
        g_TrainedFeatures[featureID][this._gripperID1].y = newGoldenPose.y
        g_TrainedFeatures[featureID][this._gripperID1].thetaInDegrees = newGoldenPose.thetaInDegrees
        g_TrainedFeatures[featureID][this._gripperID1].valid = newGoldenPose.valid

        this._results[this._myIndex].data.push(newGoldenPose)

        InSightFunctions.fnSetEvent(83)
      }

      tracer.addMessage('<- SGP: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },

  GGP: function (myIndex, cmdString) {
    tracer.addMessage('-> GGP init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    tracer.addMessage('<- GGP init ' + timeTracker.getElapsedTime())

    this.execute = function (t) {
      tracer.addMessage('-> GGP: Execute')
      var mode = this._splittedCmd[6]
      let featureID = this._featureMask
      this._results[this._myIndex].state = ECodes.E_UNSPECIFIED
      this._results[this._myIndex].isValid = 1

      if (g_TrainedFeatures[featureID][this._gripperID1].valid > 0) {
        let goldenPose = cloneObj(g_TrainedFeatures[featureID][this._gripperID1])
        if ((mode == CoordinateSystem.HOME2D) || (mode == CoordinateSystem[CoordinateSystem.HOME2D])) {
          // this._results[this._myIndex].state = 1;

        } else if ((mode == CoordinateSystem.CAM2D) || (mode == CoordinateSystem[CoordinateSystem.CAM2D])) {
          let goldenPoseInCam2D = getCamFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees)
          goldenPose.valid = goldenPoseInCam2D.valid
          if (goldenPose.valid > 0) {
            goldenPose.x = goldenPoseInCam2D.x
            goldenPose.y = goldenPoseInCam2D.y
            goldenPose.thetaInDegrees = goldenPoseInCam2D.thetaInDegrees
          }
        } else if ((mode == CoordinateSystem.RAW2D) || (mode == CoordinateSystem[CoordinateSystem.RAW2D])) {
          let goldenPoseInRaw2D = getImageFromWorld(g_Calibrations, this._shuttlingPose, goldenPose.x, goldenPose.y, goldenPose.thetaInDegrees, 0, 0, 0)
          goldenPose.valid = goldenPoseInRaw2D.valid
          if (goldenPose.valid > 0) {
            goldenPose.x = goldenPoseInRaw2D.x
            goldenPose.y = goldenPoseInRaw2D.y
            goldenPose.thetaInDegrees = goldenPoseInRaw2D.thetaInDegrees
          }
        }

        this._results[this._myIndex].state = goldenPose.valid
        this._results[this._myIndex].data.push(goldenPose)
      } else {
        this._results[this._myIndex].state = ECodes.E_TARGET_POSE_NOT_TRAINED
      }
      tracer.addMessage('<- GGP: Execute')
      return States.WAITING_FOR_NEW_COMMAND
    }
  },

  GCP: function (myIndex, cmdString) {
    tracer.addMessage('-> GCP init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- GCP init ' + timeTracker.getElapsedTime())
  },
  TT: function (myIndex, cmdString) {
    tracer.addMessage('-> TT init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(6)

    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }

    tracer.addMessage('<- TT init ' + timeTracker.getElapsedTime())
  },
  TTR: function (myIndex, cmdString) { SlaveBase.call(this, myIndex, cmdString) },
  XT: function (myIndex, cmdString) {
    tracer.addMessage('-> XT init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- XT init ' + timeTracker.getElapsedTime())
  },
  XTT: function (myIndex, cmdString) {
    tracer.addMessage('-> XTT init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- XTT init ' + timeTracker.getElapsedTime())
  },
  XTS: function (myIndex, cmdString) {
    tracer.addMessage('-> XTS init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- XTS init ' + timeTracker.getElapsedTime())

    this.toolsDone = function (t) {
      tracer.addMessage('-> XTS: Tools done ' + timeTracker.getElapsedTime())

      this._results[this._myIndex]['isValid'] = 1
      for (let f = 1; f <= MAX_FEATURES_PER_CAMERA; f++) {
        let transformed = new Feature(0, 0, 0, 0)
        let score = -1
        if (this.isFeatureEnabled(f) == true) {
          if (g_CurrentFeatures[f].valid > 0) {
            transformed = getTransformed(g_Calibrations, this._shuttlingPose, this._isCameraMoving, this._isPartMoving, g_CurrentFeatures[f], this._robotPose)
            this._results[this._myIndex].state = transformed.valid
            score = InSightFunctions.fnGetCellValue('Target.' + f + '.Pattern_Score')
          } else {
            this._results[this._myIndex].state = g_CurrentFeatures[f].valid
          }
          this._results[this._myIndex].data.push([transformed, score])
        }
      }

      tracer.addMessage('<- XTS: Tools done ' + timeTracker.getElapsedTime())

      return States.WAITING_FOR_NEW_COMMAND
    }
  },
  CP: function (myIndex, cmdString) { SlaveBase.call(this, myIndex, cmdString) },
  LF: function (myIndex, cmdString) {
    tracer.addMessage('-> LF init ' + timeTracker.getElapsedTime())
    SlaveBase.call(this, myIndex, cmdString)
    this.copyRobotPose(7)
    if (g_Calibrations[this._shuttlingPose].calibration !== null) {
      this._isCameraMoving = g_Calibrations[this._shuttlingPose].calibration.isCameraMoving_
    }
    tracer.addMessage('<- LF init ' + timeTracker.getElapsedTime())
  },
  TP: function (myIndex, cmdString) { SlaveBase.call(this, myIndex, cmdString) },
  TPR: function (myIndex, cmdString) { SlaveBase.call(this, myIndex, cmdString) },
  GP: function (myIndex, cmdString) { SlaveBase.call(this, myIndex, cmdString) }
}
//* ***************************************************************************/
//* ***************************************************************************/
//* ***************************************************************************/

//* ***************************************************************************/
// InSight functions
//* ***************************************************************************/
var InSightFunctions = {

  fnGetCellValue: new tools.GetCellValue(),
  fnSetCellValue: new tools.SetCellValue(),
  fnGetCellName: new tools.GetCellName(),
  fnSetEvent: new tools.SetEvent(),
  fnUpdateGui: new tools.UpdateGui(),
  fnStringf: new tools.Stringf(),
  fnLineToLine: new tools.LineToLine(),

  fnGetSystemConfig: new tools.GetSystemConfig(),
  fnCalibrateAdvanced: new tools.CalibrateAdvanced(),
  fnTransPixelToWorld: new tools.TransPixelToWorld(),
  fnTransWorldToPixel: new tools.TransWorldToPixel(),
  fnPoint: new tools.Point(),
  fnPlotPoint: new tools.PlotPoint(),
  fnPointToPoint: new tools.PointToPoint()
}

//* ***************************************************************************/
// My Details
//* ***************************************************************************/
function MyDetails () {
  this.myIP = InSightFunctions.fnGetSystemConfig('hostip')
  this.myName = InSightFunctions.fnGetSystemConfig('hostname')
  this.myMAC = InSightFunctions.fnStringf('%M')
  this.myIndex = 0
  this.slmp = (InSightFunctions.fnGetSystemConfig('ServicesEnabled') == 16) ? 1 : 0
  this.iAmMaster = false

  // this.fullJobName = (InSightFunctions.fnGetSystemConfig('jobname')).toLowerCase()
  this.fullJobName = InSightFunctions.fnGetSystemConfig('jobname')

  this.jobName = this.getJobName(this.fullJobName)
};

MyDetails.prototype.writeToSheet = function () {
  InSightFunctions.fnSetCellValue('Internal.Sensor.MyName', this.myName)
  InSightFunctions.fnSetCellValue('Internal.Sensor.MyIP', this.myIP)
  InSightFunctions.fnSetCellValue('Internal.Sensor.MyMAC', this.myMAC)

  InSightFunctions.fnSetCellValue('Internal.Sensor.IamMaster', this.iAmMaster)
  InSightFunctions.fnSetCellValue('Internal.Sensor.MyIndex', this.myIndex)
  InSightFunctions.fnSetCellValue('Internal.Sensor.SLMP', this.slmp)

  InSightFunctions.fnSetCellValue('Internal.Recipe.FullJobName', this.fullJobName)
  InSightFunctions.fnSetCellValue('Internal.Recipe.JobName', this.jobName)
}

MyDetails.prototype.getJobName = function (fullName) {
  var splittedName = []
  splittedName = fullName.split('\\')

  if (splittedName[splittedName.length - 1].indexOf('/') !== -1) {
    splittedName = splittedName[splittedName.length - 1].split('/')
  }

  var name = splittedName[splittedName.length - 1].replace('.job', '')
  return name
}

function Sensor () {
  this.version = -1
  this.name = ''
  this.description = ''
  this.plcPort = 7890
  this.useShuttledCameras = 0
  this.shuttledCameraCalibrations = {}
  this.cams = {}
  this.jobserver = {}
  this.logImageFtp = {}
  this.messageLengthHeader = false
};
Sensor.prototype.initFromSensorConfig = function (sensorConfig) {
  this.version = sensorConfig.sensor.Version
  this.name = sensorConfig.sensor.Name
  this.description = sensorConfig.sensor.Description
  this.plcPort = sensorConfig.sensor.PLCPort
  this.useShuttledCameras = sensorConfig.sensor.UseShuttledCameras
  this.shuttledCameraCalibrations = sensorConfig.sensor.ShuttledCameraCalibrations
  this.cams = sensorConfig.sensor.Cams
  this.jobserver = sensorConfig.sensor.Jobserver
  this.logImageFtp = sensorConfig.sensor.LogImageFtp
  this.messageLengthHeader = !!sensorConfig.sensor.MessageLengthHeader
}

Sensor.prototype.writeToSheet = function () {
  InSightFunctions.fnSetCellValue('Internal.Sensor.Name', this.name)
  InSightFunctions.fnSetCellValue('Internal.Sensor.Description', this.description)

  InSightFunctions.fnSetCellValue('Internal.Sensor.PLCPort', this.plcPort)
  InSightFunctions.fnSetCellValue('Communication.MessageLengthHeader', this.messageLengthHeader)
  InSightFunctions.fnSetCellValue('Internal.Sensor.UseShuttledCameras', this.useShuttledCameras)

  for (var i = 1; i <= MAX_CAMERAS; i++) {
    let tagName = ''
    let ip = ''
    let mac = '---'
    let master = 0

    if (this.cams.hasOwnProperty('Cam_' + i.toString()) === true) {
      var cam = this.cams['Cam_' + i.toString()]
      ip = cam.IPAddress

      mac = cam.MACAddress
      master = cam.Master
    }
    tagName = 'Internal.Sensor.IP_' + i.toString()
    InSightFunctions.fnSetCellValue(tagName, ip)
    tagName = 'Internal.Sensor.MAC_' + i.toString()
    InSightFunctions.fnSetCellValue(tagName, mac)
    tagName = 'Internal.Sensor.Master_' + i.toString()
    InSightFunctions.fnSetCellValue(tagName, master)
  }
}

Sensor.prototype.findIndexByIp = function (ip) {
  let ret = -1
  for (var i in this.cams) {
    if (this.cams[i]['IPAddress'] == ip) {
      ret = parseInt(i.replace('Cam_', ''))
    }
  }
  return ret
}

function Recipes (sensorConfiguration) {
  this.recipes = {}
  let recipeKeys = Object.keys(sensorConfiguration.recipes)
  for (let index in recipeKeys) {
    this.recipes[recipeKeys[index].toLowerCase()] = sensorConfiguration.recipes[recipeKeys[index]]
  }
};
// Recipes.prototype.init = function (sensorConfiguration) {
//  this.recipes = sensorConfiguration.recipes
// }

//* ***************************************************************************/
// SensorConfiguration
//* ***************************************************************************/

function SensorConfiguration () {
  this.sensor = null
  this.recipes = null
  this.configuration = null
};

SensorConfiguration.prototype.loadFromFile = function () {
  let fileObj = cogUtils.loadFile(SENSORCONFIG_FILENAME)
  let ret = false

  if (fileObj != 'undefined') {
    this.init(fileObj)
    ret = true
  }
  return ret
}

SensorConfiguration.prototype.init = function (sensorConfiguration) {
  this.configuration = sensorConfiguration
  this.sensor = sensorConfiguration.Sensor
  this.recipes = sensorConfiguration.Recipes
}

//* ***************************************************************************/
// CameraConfiguration
//* ***************************************************************************/

function LoadCameraConfigFile () {
  let cc = cogUtils.loadFile(CAMERACONFIG_FILENAME)

  if (cc != 'undefined') {
    g_Settings.LogImageFtp = cc['LogImageFtp']
    g_Settings.AutoCalibration = cc['AutoCalibration']
    g_Settings.CustomCalibration = cc['CustomCalibration']
    g_Settings.SensorName = cc['SensorName']
    if (cc.hasOwnProperty('IsRobotMounted')) {
      g_Settings.IsRobotMounted = cc['IsRobotMounted']
    } else {
      g_Settings.IsRobotMounted = false
    }
  }

  g_Settings.AutoCalibration.stepSizeX = InSightFunctions.fnGetCellValue('Advanced.Calibration.StepSizeX')
  g_Settings.AutoCalibration.stepSizeY = InSightFunctions.fnGetCellValue('Advanced.Calibration.StepSizeY')

  g_Settings.AutoCalibration.calibRegionX = InSightFunctions.fnGetCellValue('Advanced.Calibration.AutoCalibRegion.X')
  g_Settings.AutoCalibration.calibRegionY = InSightFunctions.fnGetCellValue('Advanced.Calibration.AutoCalibRegion.Y')
  g_Settings.AutoCalibration.calibRegionHight = InSightFunctions.fnGetCellValue('Advanced.Calibration.AutoCalibRegion.Hight')
  g_Settings.AutoCalibration.calibRegionWidth = InSightFunctions.fnGetCellValue('Advanced.Calibration.AutoCalibRegion.Width')
  g_Settings.AutoCalibration.calibRegionAngle = InSightFunctions.fnGetCellValue('Advanced.Calibration.AutoCalibRegion.Angle')

  WriteCameraConfigToSheet()
};
function WriteCameraConfigToSheet () {
  if (g_Settings.LogImageFtp != null) {
    InSightFunctions.fnSetCellValue('LogImage.Enabled', g_Settings.LogImageFtp.Enabled)
    InSightFunctions.fnSetCellValue('LogImage.LogTrainImages', g_Settings.LogImageFtp.LogTrainImages)
    InSightFunctions.fnSetCellValue('LogImage.LogProductionImages', g_Settings.LogImageFtp.LogProductionImages)
    InSightFunctions.fnSetCellValue('LogImage.LogFailImages', g_Settings.LogImageFtp.LogFailImages)

    InSightFunctions.fnSetCellValue('LogImage.Password', g_Settings.LogImageFtp.Password)
    InSightFunctions.fnSetCellValue('LogImage.UserName', g_Settings.LogImageFtp.UserName)
    InSightFunctions.fnSetCellValue('LogImage.Hostname', g_Settings.LogImageFtp.Hostname)
    InSightFunctions.fnSetCellValue('LogImage.SensorName', g_Settings.SensorName)
  }

  if (g_Settings.AutoCalibration != null) {
    InSightFunctions.fnSetCellValue('Advanced.Calibration.NumStepsX', g_Settings.AutoCalibration.NumStepsX)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.NumStepsY', g_Settings.AutoCalibration.NumStepsY)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.AutoStepSize', g_Settings.AutoCalibration.AutoStepSize)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.MovingRangeMinX', g_Settings.AutoCalibration.MovingRangeMinX)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.MovingRangeMaxX', g_Settings.AutoCalibration.MovingRangeMaxX)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.MovingRangeMinY', g_Settings.AutoCalibration.MovingRangeMinY)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.MovingRangeMaxY', g_Settings.AutoCalibration.MovingRangeMaxY)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.NumStepsRotation', g_Settings.AutoCalibration.NumStepsRotation)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.AngleMin', g_Settings.AutoCalibration.AngleMin)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.AngleMax', g_Settings.AutoCalibration.AngleMax)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.CantileverCompensation', g_Settings.AutoCalibration.CantileverCompensation)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.StepSizeX', g_Settings.AutoCalibration.stepSizeX)
    InSightFunctions.fnSetCellValue('Advanced.Calibration.StepSizeY', g_Settings.AutoCalibration.stepSizeY)
  }

  if (g_Settings.CustomCalibration != null) {
    InSightFunctions.fnSetCellValue('CustomCalibration.Enabled', g_Settings.CustomCalibration.Enabled)
  }
};

//* ***************************************************************************/
// Commands Helper
//* ***************************************************************************/
// Helper
function Object_create (o) {
  var F = function () { }
  F.prototype = o
  return new F()
};

function inheritPseudoClass (Super, Sub) {
  Sub.prototype = Object_create(Super.prototype)
  Sub.prototype.constructor = Sub
};

function getCommandObject (commands, myIndex, cmdStr) {
  var cmdObj = ECodes.E_UNKNOWN_COMMAND // 1 Valid / -1 Wrong number of arguments / -2 Wrong argument type / -3 Index out of range / -4 Unknow command
  var splittedCmdStr = cmdStr.toUpperCase().split(',')
  if (typeof commands[splittedCmdStr[0]] === 'function') {
    cmdObj = new (commands[splittedCmdStr[0]])(myIndex, cmdStr)
    let valid = cmdObj.isValid()
    if (valid != 1) {
      cmdObj = valid
    }
  }
  //GSSVN: fix featureMask to enable toolsDone function
  if ( splittedCmdStr[0] === 'XAF' || splittedCmdStr[0] === 'XAR' || splittedCmdStr[0] === 'XAA' ||splittedCmdStr[0] === 'XTF' || splittedCmdStr[0] === 'XTR' || splittedCmdStr[0] === 'XTA') {
    cmdObj._featureMask = 3
  }

  if(splittedCmdStr[0] === 'XTT'){
    cmdObj._featureMask = 7
  }
  return cmdObj
};

//* ***************************************************************************/
// Recipe Tables
//* ***************************************************************************/

const DefaultRecipeTables = {
  'Default': {
    'Recipe.PartTable.1': { 'PartID': 1, 'Description': 'Part Cam 1', 'Moving': false, 'FeatureIDs': [1] },
    'Recipe.PartTable.2': { 'PartID': 2, 'Description': 'Part Cam 2', 'Moving': false, 'FeatureIDs': [3] },
    'Recipe.PartTable.3': { 'PartID': 3, 'Description': 'Part Cam 1 + Cam 2', 'Moving': false, 'FeatureIDs': [2, 4] },
    'Recipe.PartTable.4': '',

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': { 'FeatureID': 2, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 3 },
    'Recipe.FeatureTable.3': { 'FeatureID': 3, 'Description': '', 'CameraID': 2, 'CamFeatureID': 1, 'PartID': 2 },
    'Recipe.FeatureTable.4': { 'FeatureID': 4, 'Description': '', 'CameraID': 2, 'CamFeatureID': 1, 'PartID': 3 },

    'Recipe.StepTable.1': { 'StepID': 0, 'Description': 'All cameras', 'ShuttlingPoses': [1, 1], 'FeatureIDs': [2, 4], 'ExpSettings': [1, 1] },
    'Recipe.StepTable.2': { 'StepID': 1, 'Description': 'Camera 1', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.3': { 'StepID': 2, 'Description': 'Camera 2', 'ShuttlingPoses': [1], 'FeatureIDs': [3], 'ExpSettings': [1] },
    'Recipe.StepTable.4': ''
  },
  'Single_Stationary_Camera_1_Feature': {
    'Recipe.PartTable.1': { 'PartID': 0, 'Description': 'Part 1', 'Moving': false, 'FeatureIDs': [1] },
    'Recipe.PartTable.2': { 'PartID': 1, 'Description': 'Part 1', 'Moving': false, 'FeatureIDs': [1] },
    'Recipe.PartTable.3': '',
    'Recipe.PartTable.4': '',

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': '',
    'Recipe.FeatureTable.3': '',
    'Recipe.FeatureTable.4': '',

    'Recipe.StepTable.1': { 'StepID': 1, 'Description': 'All cameras', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.2': '',
    'Recipe.StepTable.3': '',
    'Recipe.StepTable.4': ''
  },
  'Single_Stationary_Camera_2_Features': {
    'Recipe.PartTable.1': { 'PartID': 0, 'Description': 'Part 1', 'Moving': false, 'FeatureIDs': [1, 2] },
    'Recipe.PartTable.2': { 'PartID': 1, 'Description': 'Part 1', 'Moving': false, 'FeatureIDs': [1, 2] },
    'Recipe.PartTable.3': '',
    'Recipe.PartTable.4': '',

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': { 'FeatureID': 2, 'Description': '', 'CameraID': 1, 'CamFeatureID': 2, 'PartID': 1 },
    'Recipe.FeatureTable.3': '',
    'Recipe.FeatureTable.4': '',

    'Recipe.StepTable.1': { 'StepID': 1, 'Description': 'Feature 1', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.2': { 'StepID': 2, 'Description': 'Feature 2', 'ShuttlingPoses': [1], 'FeatureIDs': [2], 'ExpSettings': [2] },
    'Recipe.StepTable.3': { 'StepID': 3, 'Description': 'All features', 'ShuttlingPoses': [1], 'FeatureIDs': [1, 2], 'ExpSettings': [1] },
    'Recipe.StepTable.4': ''
  },
  'Two_Stationary_Cameras_1_Feature_Per_Camera': {
    'Recipe.PartTable.1': { 'PartID': 0, 'Description': 'Cam 1+2 Part', 'Moving': false, 'FeatureIDs': [3, 4] },
    'Recipe.PartTable.2': { 'PartID': 1, 'Description': 'Cam 1 Part', 'Moving': false, 'FeatureIDs': [1] },
    'Recipe.PartTable.3': { 'PartID': 2, 'Description': 'Cam 2 Part', 'Moving': false, 'FeatureIDs': [2] },
    'Recipe.PartTable.4': { 'PartID': 3, 'Description': 'Cam 1 + 2 Part', 'Moving': false, 'FeatureIDs': [3, 4] },

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': { 'FeatureID': 2, 'Description': '', 'CameraID': 2, 'CamFeatureID': 1, 'PartID': 2 },
    'Recipe.FeatureTable.3': { 'FeatureID': 3, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 3 },
    'Recipe.FeatureTable.4': { 'FeatureID': 4, 'Description': '', 'CameraID': 2, 'CamFeatureID': 1, 'PartID': 3 },

    'Recipe.StepTable.1': { 'StepID': 1, 'Description': 'Camera 1', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.2': { 'StepID': 2, 'Description': 'Camera 2', 'ShuttlingPoses': [1], 'FeatureIDs': [2], 'ExpSettings': [1] },
    'Recipe.StepTable.3': { 'StepID': 3, 'Description': 'All cameras', 'ShuttlingPoses': [1], 'FeatureIDs': [3, 4], 'ExpSettings': [1, 1] },
    'Recipe.StepTable.4': ''
  },
  'Single_Shuttling_Camera_1_Feature_Per_Pos': {
    'Recipe.PartTable.1': { 'PartID': 1, 'Description': 'Part 1', 'Moving': false, 'FeatureIDs': [1, 2] },
    'Recipe.PartTable.2': '',
    'Recipe.PartTable.3': '',
    'Recipe.PartTable.4': '',

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': { 'FeatureID': 2, 'Description': '', 'CameraID': 1, 'CamFeatureID': 2, 'PartID': 1 },
    'Recipe.FeatureTable.3': '',
    'Recipe.FeatureTable.4': '',

    'Recipe.StepTable.1': { 'StepID': 1, 'Description': 'Pos. 1', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.2': { 'StepID': 2, 'Description': 'Pos. 2', 'ShuttlingPoses': [2], 'FeatureIDs': [2], 'ExpSettings': [2] },
    'Recipe.StepTable.3': '',
    'Recipe.StepTable.4': ''
  },
  'Single_Stationary_Camera_1_Shuttling_Part_1_Feature_Per_Pos': {
    'Recipe.PartTable.1': { 'PartID': 1, 'Description': 'Part 1', 'Moving': true, 'FeatureIDs': [1, 2] },
    'Recipe.PartTable.2': '',
    'Recipe.PartTable.3': '',
    'Recipe.PartTable.4': '',

    'Recipe.FeatureTable.1': { 'FeatureID': 1, 'Description': '', 'CameraID': 1, 'CamFeatureID': 1, 'PartID': 1 },
    'Recipe.FeatureTable.2': { 'FeatureID': 2, 'Description': '', 'CameraID': 1, 'CamFeatureID': 2, 'PartID': 1 },
    'Recipe.FeatureTable.3': '',
    'Recipe.FeatureTable.4': '',

    'Recipe.StepTable.1': { 'StepID': 1, 'Description': 'Pos. 1', 'ShuttlingPoses': [1], 'FeatureIDs': [1], 'ExpSettings': [1] },
    'Recipe.StepTable.2': { 'StepID': 2, 'Description': 'Pos. 2', 'ShuttlingPoses': [1], 'FeatureIDs': [2], 'ExpSettings': [2] },
    'Recipe.StepTable.3': '',
    'Recipe.StepTable.4': ''
  }
}
function RecipeTables (myIndex, usedCameras) {
  this.myIndex = myIndex
  this.usedCameras = usedCameras
  this.parts = {}
  this.steps = {}
  this.features = {}
  this.stepLookup = {}
  this.partLookup = {}
  this.cameraLookup = {}
};

RecipeTables.prototype.readFromSheet = function () {
  tracer.addMessage('Reading the Recipe Tables!')
  this.parts = {}
  this.steps = {}
  this.features = {}

  g_FeaturesInfos = {}
  g_RuntimeFeatures = {}
  g_TrainedFeatures = {}
  g_TrainedRobotPoses = {}
  g_GripCorrections = {}
  g_FrameCorrections = {}

  // Read the feature-table
  for (let i = 1; i <= RECIPE_MAX_FEATURES; i++) {
    let obj = {}
    let val = ''
    let path = RECIPE_FEATURETABLE + '.' + i.toString()
    if (checkTagNameAvailable(path) === true) {
      val = InSightFunctions.fnGetCellValue(path)
      obj = checkAndGetObjectFromJsonString(val)

      if (obj != null) {
        let index = obj['FeatureID']
        this.features[index] = obj

        g_FeaturesInfos[index] = new FeatureInfo(false, false, 1, 1)
        g_RuntimeFeatures[index] = new Feature(i, 0, 0, 0)

        if ((g_StoredTrainedFeatures != null) && (g_StoredTrainedFeatures.hasOwnProperty(index) == true)) {
          g_TrainedFeatures[index] = g_StoredTrainedFeatures[index]
        } else {
          g_TrainedFeatures[index] = []
          for (let g = 0; g < MAX_GRIPPERS; g++) {
            g_TrainedFeatures[index][g] = new Feature(0, 0, 0, 0)
          }
        }
      }
    }
  }
  g_Parts = {}

  for (let i = 1; i <= RECIPE_MAX_PARTS; i++) {
    let obj = {}
    let val = ''
    let path = RECIPE_PARTTABLE + '.' + i.toString()
    if (checkTagNameAvailable(path) === true) {
      val = InSightFunctions.fnGetCellValue(path)

      obj = checkAndGetObjectFromJsonString(val)

      if (obj != null) {
        let index = obj['PartID']
        this.parts[index] = obj

        g_Parts[index] = new Part()

        if ((g_StoredTrainedRobotPoses != null) && (g_StoredTrainedRobotPoses.hasOwnProperty(index) == true)) {
          g_TrainedRobotPoses[index] = g_StoredTrainedRobotPoses[index]
        } else {
          g_TrainedRobotPoses[index] = []
          for (let g = 0; g < MAX_GRIPPERS; g++) {
            g_TrainedRobotPoses[index][g] = new RobotPose(0, 0, 0, 0, 0, 0, 0)
          }
        }
        g_GripCorrections[index] = []
        for (let g = 0; g < MAX_GRIPPERS; g++) {
          g_GripCorrections[index][g] = new RobotPose(0, 0, 0, 0, 0, 0, 0)
        }

        g_FrameCorrections[index] = new RobotPose(0, 0, 0, 0, 0, 0, 0)

        g_Parts[index]['trainedRobotPose'] = g_TrainedRobotPoses[index]
        g_Parts[index]['gripCorrection'] = g_GripCorrections[index]
        g_Parts[index]['frameCorrection'] = g_FrameCorrections[index]

        for (var f = 0; f < obj['FeatureIDs'].length; f++) {
          g_Parts[index].runtimeFeatures.push(g_RuntimeFeatures[obj['FeatureIDs'][f]])
          g_Parts[index].trainedFeatures.push(g_TrainedFeatures[obj['FeatureIDs'][f]])
          g_Parts[index].featuresInfos.push(g_FeaturesInfos[obj['FeatureIDs'][f]])
          g_FeaturesInfos[obj['FeatureIDs'][f]]['partID'] = index
        }
      }
    }
  }

  for (var i = 1; i <= RECIPE_MAX_STEPS; i++) {
    var obj = {}
    var path = RECIPE_STEPTABLE + '.' + i.toString()
    if (checkTagNameAvailable(path) === true) {
      obj = checkAndGetObjectFromJsonString(InSightFunctions.fnGetCellValue(path))

      if (obj != null) {
        let index = obj['StepID']
        this.steps[index] = obj
      }
    }
  }

  // let myIndex = InSightFunctions.fnGetCellValue('Internal.Sensor.MyIndex')

  this.createStepLookup()
  this.createPartLookup()
  this.createCameraLookup()
}

RecipeTables.prototype.createStepLookup = function () {
  tracer.addMessage('Create Step-Look-Up-Table')
  let lookUp = {}

  for (let i in this.steps) {
    var step = this.steps[i]

    let stepID = step['StepID']
    let featureIDs = step['FeatureIDs']
    let shuttlingPoses = step['ShuttlingPoses']
    let expSettingsIDs = step['ExpSettings']

    step = new StepLookupInfo()
    for (let i = 0; i < featureIDs.length; i++) {
      let feature = this.features[featureIDs[i]]
      let partID = feature['PartID']
      let part = this.parts[partID]
      let isMoving = part['Moving']
      let cameraID = feature['CameraID']
      let camFeatureID = feature['CamFeatureID']
      step.FeatureIDs.push(featureIDs[i])
      step.PartIDs.push(partID)

      step['Cam_' + cameraID.toString()]['IsMoving'] = isMoving
      step['Cam_' + cameraID.toString()]['Enabled'] = 1
      step['Cam_' + cameraID.toString()]['Feature_' + camFeatureID.toString()] = featureIDs[i]
      step['Cam_' + cameraID]['FeatureMask'] = step['Cam_' + cameraID]['FeatureMask'] | ((!!step['Cam_' + cameraID]['Feature_' + camFeatureID]) << (camFeatureID - 1))

      if (shuttlingPoses.length == featureIDs.length) {
        step['Cam_' + cameraID.toString()]['ShuttlePose'] = shuttlingPoses[i]
      } else {
        step['Cam_' + cameraID.toString()]['ShuttlePose'] = shuttlingPoses[0]
      }

      if (expSettingsIDs.length == featureIDs.length) {
        step['Cam_' + cameraID.toString()]['ExpSetting'] = expSettingsIDs[i]
      } else {
        step['Cam_' + cameraID.toString()]['ExpSetting'] = expSettingsIDs[0]
      }

      g_FeaturesInfos[featureIDs[i]].partIsMoving = isMoving
      g_FeaturesInfos[featureIDs[i]].shuttlePose = step['Cam_' + cameraID.toString()]['ShuttlePose']

      if ((cameraID != this.myIndex) && (this.usedCameras.indexOf(cameraID) >= 0)) {
        step.SendToSlave = 1
      }
    };
    lookUp[stepID] = step
  }

  this.stepLookup = lookUp
  g_StepsByID = lookUp
  // return lookUp;
}

RecipeTables.prototype.createPartLookup = function () {
  tracer.addMessage('Create Part-Look-Up-Table')
  let partLookup = {}
  for (let p in this.parts) {
    var part = new PartLookupInfo()
    // let id = this.parts[p]['PartID']
    part.FeatureIDs = this.parts[p]['FeatureIDs']

    part.PartIsMoving = this.parts[p]['Moving']

    for (let f = 0; f < part.FeatureIDs.length; f++) {
      let feature = this.features[part.FeatureIDs[f]]
      let cameraID = feature['CameraID']
      if ((cameraID != this.myIndex) && (this.usedCameras.indexOf(cameraID) >= 0)) {
        part.SendToSlave = 1
      }

      let camFeatureID = feature['CamFeatureID']
      part['Cam_' + cameraID]['Feature_' + camFeatureID] = part.FeatureIDs[f]
      part['Cam_' + cameraID]['Enabled'] = 1
      part['Cam_' + cameraID]['FeatureMask'] = part['Cam_' + cameraID]['FeatureMask'] | ((!!part['Cam_' + cameraID]['Feature_' + camFeatureID]) << (camFeatureID - 1))

      for (let s in this.steps) {
        let featureIDs = this.steps[s]['FeatureIDs']
        let shuttlingPoses = this.steps[s]['ShuttlingPoses']
        let expSettings = this.steps[s]['ExpSettings']

        for (let i = 0; i < featureIDs.length; i++) {
          if (featureIDs[i] == part['FeatureIDs'][f]) {
            if (shuttlingPoses.length == featureIDs.length) {
              part['Cam_' + cameraID.toString()]['ShuttlePose'] = shuttlingPoses[i]
            } else {
              part['Cam_' + cameraID.toString()]['ShuttlePose'] = shuttlingPoses[0]
            }

            if (expSettings.length == featureIDs.length) {
              part['Cam_' + cameraID.toString()]['ExpSetting'] = expSettings[i]
            } else {
              part['Cam_' + cameraID.toString()]['ExpSetting'] = expSettings[0]
            }
          }
        }
      }
    }
    for (let c = 1; c <= MAX_CAMERAS; c++) {
      part['Cam_' + c]['IsMoving'] = part.PartIsMoving
    }
    partLookup[p] = part
  }
  this.partLookup = partLookup
}
RecipeTables.prototype.createCameraLookup = function () {
  let camLookup = {}

  for (var c = 0; c <= MAX_CAMERAS; c++) {
    camLookup[c] = new CameraLookupInfo()
    camLookup[c]['SendToSlave'] = 0

    if ((c != this.myIndex) && (this.usedCameras.indexOf(c) >= 0)) {
      camLookup[c]['SendToSlave'] = 1
    }
  }
  if ((this.usedCameras.length > 1) || (this.usedCameras[0] != this.myIndex)) {
    camLookup[0]['SendToSlave'] = 1
  }

  this.cameraLookup = camLookup
}
RecipeTables.prototype.setToDefault = function () {
  var recipe = DefaultRecipeTables.Two_Stationary_Cameras_1_Feature_Per_Camera
  for (var k in recipe) {
    let val = ''
    if (typeof recipe[k] === 'object') {
      val = JSON.stringify(recipe[k])
    }
    InSightFunctions.fnSetCellValue(k, val)
  }
  this.readFromSheet()
}

function Camera () {
  this.Enabled = 0
  this.Feature_1 = 0
  this.Feature_2 = 0
  this.FeatureMask = 0
  this.ShuttlePose = 1
  this.ExpSetting = 1
  this.IsMoving = -560
};

function StepLookupInfo () {
  this.SendToSlave = 0
  this.FeatureIDs = []
  this.PartIDs = []
  this.Cam_1 = new Camera()
  this.Cam_2 = new Camera()
};
function PartLookupInfo () {
  this.SendToSlave = 0
  this.PartIsMoving = 0
  this.FeatureIDs = []

  this.Cam_1 = new Camera()
  this.Cam_2 = new Camera()
};
function CameraLookupInfo () {
  this.SendToSlave = 0
};
//* ***************************************************************************/
// Calibration
//* ***************************************************************************/

function Calibration (index) {
  this.calibrationData = new CalibrationData()
  this.calibration = null
  this.results = null
  this.runstatus = 0
  this.index = index
};
Calibration.prototype.computeCalibration = function () {
  if (this.calibrationData.count >= 5) {
    let so = SharedObjects.getInstance()

    let calibrationMath = so.getSharedObject('CalibrathionMath')

    let calib = calibrationMath.computeCalibration(this.calibrationData, InSightFunctions.fnGetCellValue('Internal.HRes'), InSightFunctions.fnGetCellValue('Internal.VRes'))
    if (calib.Runstatus == 1) {
      this.calibration = calib.Calibration
      this.results = calib.Results
      this.runstatus = calib.Runstatus
      // this.writeCalibrationInfoToSheet()
    }
  }
  this.writeCalibrationInfoToSheet()
}
Calibration.prototype.writeCalibrationInfoToSheet = function () {
  InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.IsCameraMoving', this.calibrationData.isMoving)
  if (this.calibration != null) {
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.Valid', this.runstatus)

    if (this.runstatus == 1) {
      let trans = new cogMath.cc2XformLinear()
      if (this.calibration['isCameraMoving_'] == true) {
        trans.setXform(this.results.Transforms.Stage2DFromImage2D.xform)
      } else {
        trans.setXform(this.results.Transforms.Home2DFromImage2D.xform)
      }
      let xScale = trans.xScale()
      let yScale = trans.yScale()
      // let angle = trans.rotationInDegrees()

      let diagnostics = this.results.Diagnostics
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.MaxImage2D', diagnostics['OverallResidualsImage2D']['Max'])
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.RMSImage2D', diagnostics['OverallResidualsImage2D']['Rms'])
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.MaxHome2D', diagnostics['OverallResidualsHome2D']['Max'])
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.RMSHome2D', diagnostics['OverallResidualsHome2D']['Rms'])
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.PixelSizeX', xScale)
      InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.PixelSizeY', yScale)
    }
  } else {
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.Valid', 0)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.MaxImage2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.RMSImage2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.MaxHome2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.RMSHome2D', -1)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.PixelSizeX', 0)
    InSightFunctions.fnSetCellValue('HECalibration.' + this.index + '.PixelSizeY', 0)
  }
}
Calibration.prototype.loadCalibrationDataFromFile = function () {
  tracer.addMessage('-> Load calibration data from file')

  let heCalibFileName = InSightFunctions.fnGetCellValue('HECalibration.FileName')
  if (heCalibFileName.length == 0) {
    heCalibFileName = InSightFunctions.fnGetCellValue('HECalibration.DefaultFileName')
  }

  if (this.index > 1) {
    heCalibFileName = heCalibFileName + '_SP' + this.index.toString()
  }
  let loadResult = this.calibrationData.loadFromFile(heCalibFileName + '.json')
  tracer.addMessage('<- Load calibration data from file')
  return loadResult
}
Calibration.prototype.saveCalibrationDataToFile = function () {
  tracer.addMessage('-> Save calibration data to file')

  let heCalibFileName = InSightFunctions.fnGetCellValue('HECalibration.FileName')
  if (heCalibFileName.length == 0) {
    heCalibFileName = InSightFunctions.fnGetCellValue('HECalibration.DefaultFileName')
  }
  if (this.index > 1) {
    heCalibFileName = heCalibFileName + '_SP' + this.index.toString()
  }
  this.calibrationData.saveToFile(heCalibFileName + '.json')
  tracer.addMessage('<- Save calibration data to file')
}

function CalibrationData () {
  this.targetX = []
  this.targetY = []
  this.targetTheta = []
  this.targetValid = []
  this.robotX = []
  this.robotY = []
  this.robotTheta = []
  this.count = 0
  this.isMoving = 0
};
CalibrationData.prototype.init = function (data) {
  this.targetX = data['TargetX']
  this.targetY = data['TargetY']
  this.targetTheta = data['TargetTheta']
  this.targetValid = data['TargetValid']
  this.robotX = data['RobotX']
  this.robotY = data['RobotY']
  this.robotTheta = data['RobotTheta']
  this.count = data['Count']
  if (data.hasOwnProperty('isMoving')) {
    this.isMoving = data['isMoving']
  }
}
CalibrationData.prototype.loadFromFile = function (filename) {
  let fileObj = cogUtils.loadFile(filename)
  let result = 0

  if (fileObj != 'undefined') {
    this.reset()
    this.init(fileObj)
    result = 1
  }
  return result
}
CalibrationData.prototype.saveToFile = function (filename) {
  let data = {}

  data['TargetX'] = this.targetX
  data['TargetY'] = this.targetY
  data['TargetTheta'] = this.targetTheta
  data['TargetValid'] = this.targetValid
  data['RobotX'] = this.robotX
  data['RobotY'] = this.robotY
  data['RobotTheta'] = this.robotTheta
  data['Count'] = this.count
  data['isMoving'] = Number(this.isMoving)
  cogUtils.saveFile(filename, data)
}
CalibrationData.prototype.reset = function () {
  this.targetX = []
  this.targetY = []
  this.targetTheta = []
  this.targetValid = []
  this.robotX = []
  this.robotY = []
  this.robotTheta = []
  this.count = 0
}

function Calibrations () {
  this.calibrations = { '1': new Calibration(1), '2': new Calibration(2) }
};
Calibrations.prototype.loadCalibrationsFromFile = function () {
  tracer.addMessage('loadCalibrationsFromFile')
  for (var i = 1; i <= MAX_CALIBRATIONS; i++) {
    if (this.calibrations[i] != null) {
      let exists = this.calibrations[i].loadCalibrationDataFromFile()
      if (exists <= 0) {
        this.calibrations[i] = null
      }
    }
  }
  this.computeCalibrations()
}
Calibrations.prototype.computeCalibrations = function () {
  for (var i in this.calibrations) {
    if (this.calibrations[i] != null) {
      this.calibrations[i].computeCalibration()
    }
  }
}
Calibrations.prototype.doAutoCalibration = function (robX, robY, robZ, robA, robB, robC, targetX, targetY, targetAngle, targetValid) {
  var acData = g_AutoCalibRuntime
  var nextRobotPose = { 'Valid': 0 }
  var inRange = 1

  if (acData.PreCalibPoses.length > 0) {
    inRange = inRange && Math.abs(acData.LastNextPos.X - acData.LastNextPos.X).between(0, 0.1, true)
    inRange = inRange && Math.abs(acData.LastNextPos.Y - acData.LastNextPos.Y).between(0, 0.1, true)
    inRange = inRange && Math.abs(acData.LastNextPos.Z - acData.LastNextPos.Z).between(0, 0.1, true)
    inRange = inRange && Math.abs(acData.LastNextPos.A - acData.LastNextPos.A).checkAngleDelta(0, 0.1, true)
    inRange = inRange && Math.abs(acData.LastNextPos.B - acData.LastNextPos.B).checkAngleDelta(0, 0.1, true)
    inRange = inRange && Math.abs(acData.LastNextPos.C - acData.LastNextPos.C).checkAngleDelta(0, 0.1, true)
  }

  if (inRange == 1) {
    if (acData.PreCalibration.Calibrated == 0) {
      if (acData.PreCalibPoses.length < 4) {
        switch (acData.PreCalibPoses.length) {
          case 0: nextRobotPose = this.firstPosition(acData, targetX, targetY, targetAngle, targetValid, robX, robY, robA); break
          case 1: nextRobotPose = this.secondPosition(acData, targetX, targetY, targetAngle, targetValid, robX, robY, robA); break
          case 2: nextRobotPose = this.thirdPosition(acData, targetX, targetY, targetAngle, targetValid, robX, robY, robA); break
          case 3: nextRobotPose = this.fourthPosition(acData, targetX, targetY, targetAngle, targetValid, robX, robY, robA); break
          default: console.log('Error in ACxxx Command'); break
        }
      }
      if (nextRobotPose != 0) {
        if ((acData.PreCalibPoses.length == 4) || ((acData.PreCalibPoses.length == 3) && (!acData.CantileverCompensation))) {
          // this.fcnCalibrateAdvanced = new Tools.CalibrateAdvanced();
          // this.fcnTransPixelToWorld = new Tools.TransPixelToWorld();

          console.log('Calc Pre-Calibration')
          var calibration = InSightFunctions.fnCalibrateAdvanced(acData.PreCalibPoses[0][0], acData.PreCalibPoses[0][1], acData.PreCalibPoses[0][3], acData.PreCalibPoses[0][4],
            acData.PreCalibPoses[1][0], acData.PreCalibPoses[1][1], acData.PreCalibPoses[1][3], acData.PreCalibPoses[1][4],
            acData.PreCalibPoses[2][0], acData.PreCalibPoses[2][1], acData.PreCalibPoses[2][3], acData.PreCalibPoses[2][4])

          //                      tracer.addMessage("PreCalibPoses "+acData.PreCalibPoses.length);
          if (acData.PreCalibPoses.length == 4) {
            var targetWorld_0 = InSightFunctions.fnTransPixelToWorld(calibration, acData.PreCalibPoses[0][0], acData.PreCalibPoses[0][1])
            var targetWorld_Rot = InSightFunctions.fnTransPixelToWorld(calibration, acData.PreCalibPoses[3][0], acData.PreCalibPoses[3][1])

            acData.angleCompX = (targetWorld_0.x - targetWorld_Rot.x) / acData.rotAngle
            acData.angleCompY = (targetWorld_0.y - targetWorld_Rot.y) / acData.rotAngle
          }

          acData.PreCalibration.Calibrated = 1
          acData.PreCalibration.Calibration = calibration
          // acData.NextRobotPose={"Valid":0};
        }

        // acData.Compensation.CompensationX=acData.angleCompX;
        // acData.Compensation.CompensationY=acData.angleCompY;
      }
    }
    if (acData.PreCalibration.Calibrated == 1) {
      var stepsX = acData.NumStepsX
      var stepsY = acData.NumStepsY
      var stepsRot = acData.NumStepsRotation
      var startX = acData.MovingRangeMinX
      var startY = acData.MovingRangeMinY
      var stepSizeX = (acData.MovingRangeMaxX - acData.MovingRangeMinX) / (acData.NumStepsX - 1)
      var stepSizeY = (acData.MovingRangeMaxY - acData.MovingRangeMinY) / (acData.NumStepsY - 1)
      var innerDist = acData.innerDist

      if (acData.CalibPoints.length == 0) {
        var innerDistXPix = innerDist
        var innerDistYPix = innerDist
        var centerX = 0
        var centerY = 0
        var robAngle = acData.PreCalibPoses[0][5]
        var compX = acData.angleCompX
        var compY = acData.angleCompY

        if (acData.AutoStepSize) {
          var xWidth = (acData.calibRegionHight - innerDistXPix) / (stepsX - 1)
          var yWidth = (acData.calibRegionWidth - innerDistYPix) / (stepsY - 1)
          var offsetX = (innerDistXPix) / 2
          var offsetY = (innerDistYPix) / 2
          var angle = acData.calibRegionAngle

          for (var x = 0; x < stepsX; x++) {
            for (var y = 0; y < stepsY; y++) {
              let point = InSightFunctions.fnPoint(acData.calibRegionX, acData.calibRegionY, angle, offsetX + (x * xWidth), offsetY + (y * yWidth), 2)

              centerX += point.x
              centerY += point.y

              var ppoint = InSightFunctions.fnPlotPoint(point.x, point.y, 'a', 5, 1)
              acData.CalibPoints.push(ppoint)

              let robPose = InSightFunctions.fnTransPixelToWorld(acData.PreCalibration.Calibration, point.x, point.y, 0)
              acData.RobCalibPoses.push([robPose.x, robPose.y, robAngle])
            }
          }
          centerX = centerX / (stepsX * stepsY)
          centerY = centerY / (stepsX * stepsY)

          let point = InSightFunctions.fnPoint(0, 0, 0, centerX, centerY, 2)
          acData.CalibPoints.push(point)

          // stepsRot, angleMin, angleMax
          var angleRange = acData.AngleMax - acData.AngleMin
          var angleStepSize = angleRange / (stepsRot - 1)
          //                tracer.addMessage("-> "+angleRange +" / "+angleStepSize+" / "+stepsRot+" / "+centerX+" / "+centerY+" / "+stepsX+" / "+ stepsY)

          for (var r = 0; r < stepsRot; r++) {
            var rot = acData.AngleMin + (r * angleStepSize)

            var cX = rot * compX
            var cY = rot * compY

            var robotR = robAngle + rot
            if (robotR > 180.0) {
              robotR = (robotR - 360)
            } else
            if (robotR < -180.0) {
              robotR = (robotR + 360)
            }

            let robPose = InSightFunctions.fnTransPixelToWorld(acData.PreCalibration.Calibration, centerX, centerY, 0)
            acData.RobCalibPoses.push([robPose.x + cX, robPose.y + cY, robotR])
          }
        } else {
          var xWidth = stepSizeX
          var yWidth = stepSizeY
          var offsetX = 0
          var offsetY = 0
          var angle = 0

          var robStartX = acData.PreCalibPoses[0][3] + startX
          var robStartY = acData.PreCalibPoses[0][4] + startY

          for (var x = 0; x < stepsX; x++) {
            for (var y = 0; y < stepsY; y++) {
              /*
                            var point = this.point(calibRegionX,calibRegionY,angle,offsetX+(x*xWidth),offsetY+(y*yWidth),2);
                            //console.log(JSON.stringify(point));
                            centerX+=point.x;
                            centerY+=point.y;

                            var ppoint =this.ppoint(point.x,point.y,"a",5,1);
                            //console.log(JSON.stringify(ppoint));
                            //this.calibPoints.push(point);
                            this.calibPoints.push(ppoint);

                            var robPose=this.fnTransPixelToWorld(calibration,point.x,point.y,0);
                             */

              acData.RobCalibPoses.push([robStartX + x * stepSizeX, robStartY + y * stepSizeY, robAngle])
            }
          }
          for (var i = 0; i < (stepsX * stepsY); i++) {
            centerX += acData.RobCalibPoses[i][0]
            centerY += acData.RobCalibPoses[i][1]
          }

          centerX = centerX / (stepsX * stepsY)
          centerY = centerY / (stepsX * stepsY)

          /*
                    var point = this.point(0,0,0,centerX,centerY,2);
                    this.calibPoints.push(point);
                    */
          // stepsRot, angleMin, angleMax
          var angleRange = acData.AngleMax - acData.AngleMin
          var angleStepSize = angleRange / (stepsRot - 1)

          for (var r = 0; r < stepsRot; r++) {
            let rot = acData.AngleMin + (r * angleStepSize)
            let cX = rot * compX
            let cY = rot * compY

            let robotR = robAngle + rot
            if (robotR > 180.0) {
              robotR = (robotR - 360)
            } else
            if (robotR < -180.0) {
              robotR = (robotR + 360)
            }
            // var robPose=this.fnTransPixelToWorld(calibration,centerX+cX,centerY+cY,0);
            acData.RobCalibPoses.push([centerX + cX, centerY + cY, robotR])
          }
        }
        // console.log(JSON.stringify(this.robCalibPoses));
      }

      if (acData.stepCount < (stepsX * stepsY + stepsRot)) {
        nextRobotPose['NextX'] = acData.RobCalibPoses[acData.stepCount][0]
        nextRobotPose['NextY'] = acData.RobCalibPoses[acData.stepCount][1]
        nextRobotPose['NextAngle'] = acData.RobCalibPoses[acData.stepCount][2]
        nextRobotPose['Step'] = acData.stepCount
        nextRobotPose['Valid'] = 2
        acData.stepCount++
      } else
      if (acData.stepCount == (stepsX * stepsY + stepsRot)) {
        nextRobotPose['NextX'] = acData.PreCalibPoses[0][3]
        nextRobotPose['NextY'] = acData.PreCalibPoses[0][4]
        nextRobotPose['NextAngle'] = acData.PreCalibPoses[0][5]
        nextRobotPose['Step'] = acData.stepCount
        nextRobotPose['Valid'] = 2
        acData.stepCount++
      } else {
        nextRobotPose['NextX'] = acData.PreCalibPoses[0][3]
        nextRobotPose['NextY'] = acData.PreCalibPoses[0][4]
        nextRobotPose['NextAngle'] = acData.PreCalibPoses[0][5]
        nextRobotPose['Valid'] = 1
      }
    }
  } else {
    nextRobotPose['Valid'] = ECodes.E_NOT_GIVEN_CALIBRATION_POSE
  }

  if (nextRobotPose['Valid'] > 0) {
    acData.LastNextPos.X = nextRobotPose['NextX']
    acData.LastNextPos.Y = nextRobotPose['NextY']
    acData.LastNextPos.Z = robZ
    acData.LastNextPos.A = nextRobotPose['NextAngle']
    acData.LastNextPos.B = robB
    acData.LastNextPos.C = robC
  }
  return nextRobotPose
}
Calibrations.prototype.CheckData = function (data, isMoving) {
  if (data.count < MIN_POSES) {
    console.log('To few positions! ' + MIN_POSES + ' positions needed!')
    return ECodes.E_INVALID_CALIBRATION_DATA
  }

  if (data.hasOwnProperty('LastNextPos')) {
    delete data['LastNextPos']
  }

  var cntValid = 0
  var cntRotated = 0

  var minRobotAngle = data.robotTheta[0]
  var maxRobotAngle = data.robotTheta[0]

  for (var i = 0; i < data.count; i++) {
    if (data.targetValid[i] == 1) {
      cntValid++

      if (data.robotTheta[i] < minRobotAngle) {
        minRobotAngle = data.robotTheta[i]
        cntRotated++
      } else
      if (data.robotTheta[i] > maxRobotAngle) {
        maxRobotAngle = data.robotTheta[i]
        cntRotated++
      }
    }
  }

  var onStraight = true
  for (var t = 2; t < data.count; t++) {
    onStraight = this.CheckPointOnStraightLine(data.robotX[0], data.robotY[0], data.robotX[1], data.robotY[1], data.robotX[t], data.robotY[t])
    // console.log(onStraight);
    if (onStraight == false) { break }
  }

  if (onStraight == true) {
    // this.calibData["LogMessage"]="All positions on a straight!";
    console.log('All positions on a straight!')
    return 0
  }

  if ((cntValid) < MIN_POSES) {
    // this.calibData["LogMessage"]="To few valid positions! "+(this.MIN_POSES)+" valid positions needed!";
    console.log('To few valid positions! ' + (MIN_POSES) + ' valid positions needed!')
    return 0
  }

  if ((cntRotated + 1) < MIN_POSES_ROTATED) {
    // this.calibData["LogMessage"]="To few valid rotated positions! "+(this.MIN_POSES_ROTATED)+"rotated positions needed!";
    console.log('To few valid rotated positions! ' + (MIN_POSES_ROTATED) + 'rotated positions needed!')
    return 0
  }

  var angleRange = Math.abs(maxRobotAngle - minRobotAngle)
  if (angleRange < MIN_ANGLE_RANGE) {
    // this.calibData["LogMessage"]="The angle range is to small! "+(this.MIN_ANGLE_RANGE)+"° needed!";

    console.log('The angle range is to small! ' + (MIN_ANGLE_RANGE) + '° needed!')
    return 0
  }

  if (isMoving == 1) {
    data.isMoving = 1
  }

  console.log('Angle range: ' + minRobotAngle + ' / ' + maxRobotAngle + ' / ' + angleRange)
  console.log('Valid: ' + cntValid + ' / Rotated: ' + (cntRotated))

  return 1
}
Calibrations.prototype.CheckPointOnStraightLine = function (pX0, pY0, pX1, pY1, pX, pY) {
  var ret = false

  var dirX = pX1 - pX0
  var dirY = pY1 - pY0

  var rX = pX - pX0
  var rY = pY - pY0

  if ((dirX == 0) && (dirY == 0)) {
    ret = true
  } else {
    if ((dirX == 0) && (rX == 0)) {
      ret = true
    } else
    if ((dirY == 0) && (rY == 0)) {
      ret = true
    } else
    if (((pX - pX0) / dirX) == ((pY - pY0) / dirY)) {
      ret = true
    } else {
      ret = false
    }
  }

  return ret
}
Calibrations.prototype.checkMinDistance = function (x1, y1, x2, y2) {
  var ret = false
  var dist = this.computeDistance(x1, y1, x2, y2)
  if (dist >= 50) /* --------------------------this.minDist_Pixel */ {
    ret = true
  }
  return ret
}
Calibrations.prototype.computeDistance = function (x1, y1, x2, y2) {
  var dist = Math.sqrt(Math.pow(x1 - x2, 2) + Math.pow(y1 - y2, 2))
  return dist
}
Calibrations.prototype.firstPosition = function (acData, targetX, targetY, targetAngle, targetValid, robX, robY, robAngle) {
  console.log('Add first Position')
  var nextRobotPose = { 'Valid': 0 }
  if (targetValid > 0) {
    // Add this point to the calibration points
    acData.PreCalibPoses.push([targetX, targetY, targetAngle, robX, robY, robAngle])

    nextRobotPose['NextX'] = robX + acData.lastMoveDistance_X
    nextRobotPose['NextY'] = robY
    nextRobotPose['NextAngle'] = robAngle
    nextRobotPose['Valid'] = 2
  }

  return nextRobotPose
}
Calibrations.prototype.secondPosition = function (acData, targetX, targetY, targetAngle, targetValid, robX, robY, robAngle) {
  console.log('Check second Position')
  var nextRobotPose = { 'Valid': 0 }
  var valid = 1

  if (targetValid > 0) {
    // Check min distance
    if (this.checkMinDistance(acData.PreCalibPoses[0][0], acData.PreCalibPoses[0][1], targetX, targetY) || !acData.AutoStepSize || (acData.loopCnt != 0)) {
      console.log('Add second Position')
      // Add this point to the calibration points
      acData.PreCalibPoses.push([targetX, targetY, targetAngle, robX, robY, robAngle])
      if (acData.loopCnt != 0) {
        acData.lastMoveDistance_Y = START_MOVE_DISTANCE_MM
      } else {
        acData.lastMoveDistance_Y = acData.lastMoveDistance_X
      }

      acData.lastMoveDistance_X = 0
    } else {
      console.log('Compute an other second Position')

      var dist = this.computeDistance(acData.PreCalibPoses[0][0], acData.PreCalibPoses[0][1], targetX, targetY)
      var mmPerPix = acData.lastMoveDistance_X / dist

      acData.lastMoveDistance_X = mmPerPix * MIN_DIST_PIXEL * 1.1/* ---------------------------------minDist_Pixel*1.1; */
      acData.lastMoveDistance_Y = 0
    }
    acData.loopCnt = 0
    acData.Direction = 1
  } else {
    acData.loopCnt++

    if ((acData.loopCnt > MAX_LOOPS) && (acData.Direction == -1)) {
      valid = 0
    } else {
      console.log('No target found, reduce step size!')

      if ((acData.loopCnt > MAX_LOOPS) && (acData.Direction == 1)) {
        acData.loopCnt = 0
        acData.Direction = -1
        console.log('Reverse direction!')
      }

      console.log('Maybe out of FOV, reduce step size!')

      if ((acData.loopCnt == 0) && (acData.Direction == -1)) {
        acData.lastMoveDistance_X = START_MOVE_DISTANCE_MM * -1
      } else {
        acData.lastMoveDistance_X = acData.lastMoveDistance_X / 2
      }
      acData.lastMoveDistance_Y = 0
    }
  }

  if (valid > 0) {
    nextRobotPose['NextX'] = acData.PreCalibPoses[0][3] + acData.lastMoveDistance_X
    nextRobotPose['NextY'] = acData.PreCalibPoses[0][4] + acData.lastMoveDistance_Y
    nextRobotPose['NextAngle'] = acData.PreCalibPoses[0][5]
    nextRobotPose['Valid'] = 2
  } else {
    console.log('Max loops done! No target found!')
    nextRobotPose['Valid'] = ECodes.E_CALIBRATION_FAILED
  }
  return nextRobotPose
}
Calibrations.prototype.thirdPosition = function (acData, targetX, targetY, targetAngle, targetValid, robX, robY, robAngle) {
  console.log('Check third Position')
  var nextRobotPose = { 'Valid': 0 }
  var rot = 0
  var valid = 1
  if (targetValid > 0) {
    // Check min distance
    if (this.checkMinDistance(acData.PreCalibPoses[0][0], acData.PreCalibPoses[0][1], targetX, targetY) || !acData.AutoStepSize || (acData.loopCnt != 0)) {
      console.log('Add third Position')
      // Add this point to the calibration points
      acData.PreCalibPoses.push([targetX, targetY, targetAngle, robX, robY, robAngle])

      // Nothing more to do
      acData.lastMoveDistance_Y = 0
      acData.lastMoveDistance_X = 0
      rot = acData.rotAngle
    } else {
      console.log('Compute an other Distance Position')
      var dist = Math.sqrt(Math.pow(acData.PreCalibPoses[0][0] - targetX, 2) + Math.pow(acData.PreCalibPoses[0][1] - targetY, 2))
      var mmPerPix = acData.lastMoveDistance_Y / dist
      acData.lastMoveDistance_Y = mmPerPix * MIN_DIST_PIXEL * 1.1 /* ------------------this.minDist_Pixel */
      acData.lastMoveDistance_X = 0
    }
    acData.loopCnt = 0
    acData.Direction = 1
  } else {
    acData.loopCnt++
    if ((acData.loopCnt > MAX_LOOPS) && (acData.Direction == -1)) {
      valid = 0
    } else {
      console.log('No target found, reduce step size!')

      if ((acData.loopCnt > MAX_LOOPS) && (acData.Direction == 1)) {
        acData.loopCnt = 0
        acData.Direction = -1
        console.log('Reverse direction!')
      }

      console.log('Maybe out of FOV, reduce step size!')

      if ((acData.loopCnt == 0) && (acData.Direction == -1)) {
        acData.lastMoveDistance_Y = START_MOVE_DISTANCE_MM * -1
      } else {
        acData.lastMoveDistance_Y = acData.lastMoveDistance_Y / 2
      }
    }
  }

  if (valid > 0) {
    nextRobotPose['NextX'] = acData.PreCalibPoses[0][3] + acData.lastMoveDistance_X
    nextRobotPose['NextY'] = acData.PreCalibPoses[0][4] + acData.lastMoveDistance_Y
    var r = acData.PreCalibPoses[0][5] + rot
    if (r > 180.0) {
      r = (r - 360)
    } else
    if (r < -180.0) {
      r = (r + 360)
    }

    nextRobotPose['NextAngle'] = r
    nextRobotPose['Valid'] = 2
  } else {
    console.log('Max loops done! No target found!')
    nextRobotPose['Valid'] = ECodes.E_CALIBRATION_FAILED
  }

  return nextRobotPose
}
Calibrations.prototype.fourthPosition = function (acData, targetX, targetY, targetAngle, targetValid, robX, robY, robAngle) {
  console.log('Check fourth Position')
  var nextRobotPose = { 'Valid': 0 }
  if (targetValid > 0) {
    // Add this point to the calibration points
    acData.PreCalibPoses.push([targetX, targetY, targetAngle, robX, robY, robAngle])
    // compute the next robot pose

    nextRobotPose['NextX'] = acData.PreCalibPoses[0][3]
    nextRobotPose['NextY'] = acData.PreCalibPoses[0][4]
    nextRobotPose['NextAngle'] = acData.PreCalibPoses[0][5]
    nextRobotPose['Valid'] = 2
  }

  return nextRobotPose
}
Calibrations.prototype.doCustomCalibration = function (data, handedness, offsetX, offsetY) {
  var retData = data

  var hand = (handedness == 0 ? 1 : -1)
  if ((data.count == 2) || (data.count == 9)) {
    if (data.count == 2) {
      retData = this.create2(data, hand)
    } else {
      retData = data
    }

    retData = this.createRotated(retData, hand, offsetX, offsetY)
  }
  return retData
}
Calibrations.prototype.create2 = function (data, handedness) {
  //  var mCrsp = {};
  //  mCrsp.targetX = [];
  //  mCrsp.targetY = [];
  //  mCrsp.targetTheta = [];
  //  mCrsp.targetValid = [];
  //  mCrsp.robotX = [];
  //  mCrsp.robotY = [];
  //  mCrsp.robotTheta = [];
  //  mCrsp.count = 0;

  // Motion vector
  var dx = data.robotX[1] - data.robotX[0]
  var dy = data.robotY[1] - data.robotY[0]
  // real motion vector
  var vm1 = new cogMath.Vector2(dx, dy)
  // orthogonal motion vector
  var vm2 = vm1.rotate(0.5 * Math.PI)

  // Vision vector
  var dCol = data.targetX[1] - data.targetX[0]
  var dRow = data.targetY[1] - data.targetY[0]
  // real vision moving vector
  var vv1 = new cogMath.Vector2(dCol, dRow)
  // orthogonal vision moving vector
  var vv2 = vv1.rotate(handedness * 0.5 * Math.PI)

  // First HECalib positions
  var posM = new cogMath.Vector2(data.robotX[0], data.robotY[0]) // 1st motion position
  var posV = new cogMath.Vector2(data.targetX[0], data.targetY[0]) // 1st vision position

  // Now move motion and vision positions in a 3 x 3 matrix
  for (var dir2 = 0; dir2 < 3; dir2++) { // direction2 loop
    var v = vm2.scale(dir2)
    var vmStart = posM.add(v)

    v = vv2.scale(dir2)
    var vvStart = posV.add(v)

    for (var dir1 = 0; dir1 < 3; dir1++) { // direction1 loop
      v = vm1.scale(dir1)
      var vm = vmStart.add(v)

      v = vv1.scale(dir1)
      var vv = vvStart.add(v)

      data.targetX.push(vv.x)
      data.targetY.push(vv.y)
      data.targetValid.push(1)
      data.targetTheta.push(0)

      data.robotX.push(vm.x)
      data.robotY.push(vm.y)
      data.robotTheta.push(0)

      data.count++
    }
  }

  data.targetX.splice(0, 2)
  data.targetY.splice(0, 2)
  data.targetValid.splice(0, 2)
  data.targetTheta.splice(0, 2)

  data.robotX.splice(0, 2)
  data.robotY.splice(0, 2)
  data.robotTheta.splice(0, 2)

  data.count = data.targetX.length

  return data
}

Calibrations.prototype.createRotated = function (data, handedness, offsetX, offsetY) {
  // Create a 3-Point calibration from matrix position (0,0), (2,0) and (2,2)
  // var retData = data;
  var calibPoints = []
  var cpt = {}
  cpt.Vx = data.targetX[0]
  cpt.Vy = data.targetY[0]
  cpt.Mx = data.robotX[0] + offsetX
  cpt.My = data.robotY[0] + offsetY
  calibPoints.push(cpt)
  cpt = {}
  cpt.Vx = data.targetX[2]
  cpt.Vy = data.targetY[2]
  cpt.Mx = data.robotX[2] + offsetX
  cpt.My = data.robotY[2] + offsetY
  calibPoints.push(cpt)
  cpt = {}
  cpt.Vx = data.targetX[8]
  cpt.Vy = data.targetY[8]
  cpt.Mx = data.robotX[8] + offsetX
  cpt.My = data.robotY[8] + offsetY
  calibPoints.push(cpt)

  var calib = InSightFunctions.fnCalibrateAdvanced(calibPoints[0].Vx, calibPoints[0].Vy, calibPoints[0].Mx, calibPoints[0].My,
    calibPoints[1].Vx, calibPoints[1].Vy, calibPoints[1].Mx, calibPoints[1].My,
    calibPoints[2].Vx, calibPoints[2].Vy, calibPoints[2].Mx, calibPoints[2].My)

  // Rotated positions
  // Vector Origin -> Feature in motion space:
  var vFeature0 = new cogMath.Vector2(offsetX, offsetY)
  var vRotPoint = new cogMath.Vector2(data.robotX[0], data.robotY[0])

  for (var rot = -15; rot < 20; rot += 30) {
    var vFeatureRot = vFeature0.rotate(handedness * Math.PI * rot / 180.0) // Rotated feature pos in world coords
    var vFeatureRotMoved = vFeatureRot.add(vRotPoint)
    var ptRot = InSightFunctions.fnTransWorldToPixel(calib, vFeatureRotMoved.x, vFeatureRotMoved.y, 0) // Rotated feature pos in pixels

    data.robotX.push(data.robotX[0])
    data.robotY.push(data.robotY[0])
    data.robotTheta.push(data.robotTheta[0] + rot)

    data.targetX.push(ptRot.row)
    data.targetY.push(ptRot.col)
    data.targetValid.push(1)
    data.targetTheta.push(rot)
    data.count++
  }
  return data
}
//* ***************************************************************************/
// Calculations
//* ***************************************************************************/
function ComputeAlignMode_1 (partID, gripperID, resMode, robPose) {
  // TODO: nur gültige Kalibrierung zulassen

  let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

  if (!g_Parts.hasOwnProperty(partID)) {
    newRobPose.valid = ECodes.E_INVALID_PART_ID
    return newRobPose
  }
  let trainPoints = []
  let runPoints = []
  let state = ECodes.E_NO_ERROR
  let heCalib = null
  heCalib = g_Calibrations[1]['calibration']

  if (g_Parts[partID]['runtimeFeatures'].length == g_Parts[partID]['trainedFeatures'].length) {
    for (var i = 0; i < g_Parts[partID]['runtimeFeatures'].length; i++) {
      let tf = cloneObj(g_Parts[partID]['trainedFeatures'][i][gripperID])
      let rf = cloneObj(g_Parts[partID]['runtimeFeatures'][i])
      if (tf.valid > 0) {
        if (g_Parts[partID]['featuresInfos'][i]['partIsMoving'] == 1) {
          let homeFromHand_RT = new cogMath.cc2Rigid()
          homeFromHand_RT.setXform(robPose.thetaZ, [robPose.x, robPose.y])
          homeFromHand_RT = cogMath.convertToUncorrected(g_Calibrations[1]['results']['Transforms'], homeFromHand_RT)

          let rob = cloneObj(g_Parts[partID]['trainedRobotPose'][gripperID])
          let homeFromHand_TT = new cogMath.cc2Rigid()
          homeFromHand_TT.setXform(rob.thetaZ, [rob.x, rob.y])
          homeFromHand_TT = cogMath.convertToUncorrected(g_Calibrations[1]['results']['Transforms'], homeFromHand_TT)

          let taXf = new cogMath.cc2Rigid()
          taXf.setXform(tf.thetaInDegrees, [tf.x, tf.y])

          let raXf = new cogMath.cc2Rigid()
          raXf.setXform(rf.thetaInDegrees, [rf.x, rf.y])

          taXf = homeFromHand_TT.compose(taXf)
          raXf = homeFromHand_RT.compose(raXf)

          tf.x = taXf.trans()[0]
          tf.y = taXf.trans()[1]
          // tf.thetaInDegrees = taXf.rotationInDegrees();
          tf.thetaInDegrees = taXf.angleInDegrees()

          rf.x = raXf.trans()[0]
          rf.y = raXf.trans()[1]
          // rf.thetaInDegrees = raXf.rotationInDegrees();
          rf.thetaInDegrees = raXf.angleInDegrees()
        }
        trainPoints.push(new Feature(tf.x, tf.y, tf.thetaInDegrees, tf.valid))
        runPoints.push(new Feature(rf.x, rf.y, rf.thetaInDegrees, rf.valid))
      } else {
        state = ECodes.E_TARGET_POSE_NOT_TRAINED
      }
    }

    if (state == ECodes.E_NO_ERROR) {
      let result = {}

      result = ComputeDesiredStagePosition(trainPoints, runPoints, heCalib, robPose.x, robPose.y, robPose.thetaZ)

      let abs = {}
      abs.X = result.desiredHome2DFromStage2DCorrected.trans()[0]
      abs.Y = result.desiredHome2DFromStage2DCorrected.trans()[1]
      abs.A = result.desiredHome2DFromStage2DCorrected.angleInDegrees()
      abs.Valid = 1

      let robTargetPos = new cogMath.cc2Rigid()
      robTargetPos.setXform(abs.A, [abs.X, abs.Y])

      let robPos = new cogMath.cc2Rigid()
      robPos.setXform(robPose.thetaZ, [robPose.x, robPose.y])

      if ((resMode == ResultMode.ABS) || (resMode === ResultMode[ResultMode.ABS])) {
        newRobPose.x = abs.X
        newRobPose.y = abs.Y
        newRobPose.z = robPose.z
        newRobPose.thetaZ = abs.A
        newRobPose.thetaY = robPose.thetaY
        newRobPose.thetaX = robPose.thetaX
        newRobPose.valid = 1
      } else if ((resMode === ResultMode.OFF) || (resMode === ResultMode[ResultMode.OFF])) {
        let offRes = new cogMath.cc2Rigid()
        offRes.setXform(robTargetPos.angleInDegrees() - robPos.angleInDegrees(), [robTargetPos.trans()[0] - robPos.trans()[0], robTargetPos.trans()[1] - robPos.trans()[1]])

        newRobPose.x = offRes.trans()[0]
        newRobPose.y = offRes.trans()[1]
        newRobPose.z = 0.0
        newRobPose.thetaZ = offRes.angleInDegrees()
        newRobPose.thetaY = 0.0
        newRobPose.thetaX = 0.0
        newRobPose.valid = 1
      } else {
        newRobPose.valid = ECodes.E_INVALID_ARGUMENT
      }
    } else {
      newRobPose.valid = state
    }
  }
  return newRobPose
};

function ComputeAlignMode_2 (partID1, partID2, gripperID, resMode, robPose) {
  let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

  if (!g_Parts.hasOwnProperty(partID1)) {
    newRobPose.valid = ECodes.E_INVALID_PART_ID
    return newRobPose
  }

  let trainedRobotPose = cloneObj(g_Parts[partID1]['trainedRobotPose'][gripperID]) // rtr
  if (trainedRobotPose.valid <= 0) {
    newRobPose.valid = ECodes.E_ROBOT_POSE_NOT_TRAINED
    return newRobPose
  }

  let runtimeFeatuers = g_Parts[partID1]['runtimeFeatures']
  let trainedFeatuers = g_Parts[partID1]['trainedFeatures']
  /*
        for (let rf = 0; rf < runtimeFeatuers.length; rf++) {
            if (runtimeFeatuers[rf].valid <= 0) {
                newRobPose.valid = ECodes.E_PART_NOT_ALL_FEATURES_LOCATED;
                return newRobPose;
            }
        }
    */
  for (let tf = 0; tf < trainedFeatuers.length; tf++) {
    if (trainedFeatuers[tf][gripperID].valid <= 0) {
      newRobPose.valid = ECodes.E_TARGET_POSE_NOT_TRAINED
      return newRobPose
    }
  }
  // let isPartMoving = g_Parts[partID1]['featuresInfos'][0]['isMoving']
  let shuttlePose = g_Parts[partID1]['featuresInfos'][0]['shuttlePose']

  if ((runtimeFeatuers.length != trainedFeatuers.length) || (runtimeFeatuers.length > 2) || (runtimeFeatuers.length < 1)) {
    newRobPose.valid = ECodes.E_UNSPECIFIED
    return newRobPose
  }
  let length = runtimeFeatuers.length
  let sumRuntime = { 'x': 0, 'y': 0, 'theta': 0 } // trt
  let sumTrained = { 'x': 0, 'y': 0, 'theta': 0 } // ttr

  for (let i = 0; i < length; i++) {
    sumRuntime.x += runtimeFeatuers[i].x
    sumRuntime.y += runtimeFeatuers[i].y
    sumRuntime.theta += runtimeFeatuers[i].thetaInDegrees

    sumTrained.x += trainedFeatuers[i][gripperID].x
    sumTrained.y += trainedFeatuers[i][gripperID].y
    sumTrained.theta += trainedFeatuers[i][gripperID].thetaInDegrees
  }

  if (length > 1) {
    sumRuntime.x /= length
    sumRuntime.y /= length
    sumRuntime.theta /= length

    sumTrained.x /= length
    sumTrained.y /= length
    sumTrained.theta /= length
    /*
    let dXr = runtimeFeatuers[0].x - runtimeFeatuers[1].x
    let dYr = runtimeFeatuers[0].y - runtimeFeatuers[1].y

    if (dXr === 0) {
      sumRuntime.theta = 90
    } else {
      sumRuntime.theta = Math.degrees(Math.atan((dYr) / (dXr)))
    }
    */
    sumRuntime.theta = InSightFunctions.fnPointToPoint(runtimeFeatuers[0].x, runtimeFeatuers[0].y, runtimeFeatuers[1].x, runtimeFeatuers[1].y, 0).getAngle()

    /*
    let dXt = trainedFeatuers[0][gripperID].x - trainedFeatuers[1][gripperID].x
    let dYt = trainedFeatuers[0][gripperID].y - trainedFeatuers[1][gripperID].y

    if (dXt === 0) {
      sumTrained.theta = 90
    } else {
      sumTrained.theta = Math.degrees(Math.atan((dYt) / (dXt)))
    }
*/
    sumTrained.theta = InSightFunctions.fnPointToPoint(trainedFeatuers[0][gripperID].x, trainedFeatuers[0][gripperID].y, trainedFeatuers[1][gripperID].x, trainedFeatuers[1][gripperID].y, 0).getAngle()
  }

  let calibXforms = g_Calibrations[shuttlePose].results.Transforms
  // let isCameraMoving = g_Calibrations[shuttlePose].calibration.isCameraMoving_

  let trainResult = {}
  let runtimeResult = {}
  // Train
  {
    let homeFromTarget = new cogMath.cc2Rigid()
    homeFromTarget.setXform(sumTrained.theta, [sumTrained.x, sumTrained.y])
    trainResult['Home2DFromTarget2D'] = homeFromTarget

    var homeFromHand = new cogMath.cc2Rigid()
    homeFromHand.setXform(trainedRobotPose.thetaZ, [trainedRobotPose.x, trainedRobotPose.y])
    homeFromHand = cogMath.convertToCorrected(calibXforms, homeFromHand)
    trainResult['Home2DFromRobot2D'] = homeFromHand

    var tmpLinear = homeFromHand.inverse().compose(homeFromTarget)
    var handFromTarget = cogMath.lin2Rigid(tmpLinear)
    trainResult['Robot2DFromTarget2D'] = handFromTarget
  }
  // Runtime
  // eslint-disable-next-line no-lone-blocks
  {
    var homeFromTarget = new cogMath.cc2Rigid()
    homeFromTarget.setXform(sumRuntime.theta, [sumRuntime.x, sumRuntime.y])
    runtimeResult['Home2DFromTarget2D'] = homeFromTarget

    // Step is trained, we are running the "XT" command
    var tmpLinear = new cogMath.cc2XformLinear()
    tmpLinear.setXform(trainResult['Robot2DFromTarget2D'].xform)
    var handFromTarget = cogMath.lin2Rigid(tmpLinear)

    // var homeFromTargetTrain = new cogMath.cc2Rigid()
    // ++tmpLinear.setXform(trainResult['Home2DFromTarget2D'].xform)
    // homeFromTargetTrain = cogMath.lin2Rigid(tmpLinear)

    var homeFromHandRT = new cogMath.cc2Rigid() // cc2XformLinear();
    homeFromHandRT = homeFromTarget.compose(handFromTarget.inverse())

    var homeFromHandRT_u = cogMath.convertToUncorrected(calibXforms, homeFromHandRT)

    var robotAbsXYA = {}
    robotAbsXYA['A'] = homeFromHandRT_u.angleInDegrees()
    robotAbsXYA['X'] = homeFromHandRT_u.trans()[0]
    robotAbsXYA['Y'] = homeFromHandRT_u.trans()[1]
    // runtimeResult["RobotRTAbs"] = robotAbsXYA;

    // The offset position
    var homeFromHandTrained = new cogMath.cc2Rigid()
    tmpLinear.setXform(trainResult['Home2DFromRobot2D'].xform)
    homeFromHandTrained = cogMath.lin2Rigid(tmpLinear)

    // Convert to uncorrected motion space
    var homeFromHandTrained_u = cogMath.convertToUncorrected(calibXforms, homeFromHandTrained)

    var trainFromRT = new cogMath.cc2Rigid()
    trainFromRT = homeFromHandRT_u.compose(homeFromHandTrained_u.inverse())

    var robotOffXYA = {}
    robotOffXYA['A'] = homeFromHandRT_u.angleInDegrees() - homeFromHandTrained_u.angleInDegrees()
    robotOffXYA['X'] = homeFromHandRT_u.trans()[0] - homeFromHandTrained_u.trans()[0]
    robotOffXYA['Y'] = homeFromHandRT_u.trans()[1] - homeFromHandTrained_u.trans()[1]

    var robotFrameXYA = {}
    robotFrameXYA['A'] = trainFromRT.angleInDegrees()
    robotFrameXYA['X'] = trainFromRT.trans()[0]
    robotFrameXYA['Y'] = trainFromRT.trans()[1]

    // Picked
    var robotPickedXYA = {}
    var homeFromRobot = new cogMath.cc2Rigid()
    homeFromRobot.setXform(robPose.thetaZ, [robPose.x, robPose.y])

    var picked = new cogMath.cc2Rigid()
    picked = homeFromHand.inverse().compose(homeFromHandRT_u)

    var homeFromHandPicked = new cogMath.cc2Rigid()
    homeFromHandPicked = homeFromRobot.compose(picked.inverse())

    robotPickedXYA['A'] = homeFromHandPicked.angleInDegrees()
    robotPickedXYA['X'] = homeFromHandPicked.trans()[0]
    robotPickedXYA['Y'] = homeFromHandPicked.trans()[1]
  }

  if ((resMode === ResultMode.ABS) || (resMode === ResultMode[ResultMode.ABS])) {
    newRobPose.x = robotAbsXYA.X
    newRobPose.y = robotAbsXYA.Y
    newRobPose.thetaZ = robotAbsXYA.A
    newRobPose.z = trainedRobotPose.z
    newRobPose.thetaY = trainedRobotPose.thetaY
    newRobPose.thetaX = trainedRobotPose.thetaX
    newRobPose.valid = 1
  } else if ((resMode === ResultMode.OFF) || (resMode === ResultMode[ResultMode.OFF])) {
    newRobPose.x = robotOffXYA.X
    newRobPose.y = robotOffXYA.Y
    newRobPose.z = 0.0
    newRobPose.thetaZ = robotOffXYA.A
    newRobPose.thetaY = 0.0
    newRobPose.thetaX = 0.0
    newRobPose.valid = 1
  } else if ((resMode === ResultMode.FRAME) || (resMode === ResultMode[ResultMode.FRAME])) {
    newRobPose.x = robotFrameXYA.X
    newRobPose.y = robotFrameXYA.Y
    newRobPose.z = 0.0
    newRobPose.thetaZ = robotFrameXYA.A
    newRobPose.thetaY = 0.0
    newRobPose.thetaX = 0.0
    newRobPose.valid = 1
    g_FrameCorrections[partID1].x = robotFrameXYA.X
    g_FrameCorrections[partID1].y = robotFrameXYA.Y
    g_FrameCorrections[partID1].thetaZ = robotFrameXYA.A
    g_FrameCorrections[partID1].valid = 1
  } else if ((resMode === ResultMode.PICKED) || (resMode === ResultMode[ResultMode.PICKED])) {
    newRobPose.x = robotPickedXYA.X
    newRobPose.y = robotPickedXYA.Y
    newRobPose.z = robPose.z
    newRobPose.thetaZ = robotPickedXYA.A
    newRobPose.thetaY = robPose.thetaY
    newRobPose.thetaX = robPose.thetaX
    newRobPose.valid = 1
  } else if ((resMode === ResultMode.GC) || (resMode === ResultMode[ResultMode.GC])) {
    let homeFromTargetTrained = new cogMath.cc2XformLinear()
    homeFromTargetTrained.setXform(trainResult['Home2DFromTarget2D'].xform)

    let homeFromTargetRuntime = new cogMath.cc2XformLinear()
    homeFromTargetRuntime.setXform(runtimeResult['Home2DFromTarget2D'].xform)

    let homeFromHandRuntime = new cogMath.cc2Rigid()
    homeFromHandRuntime.setXform(robPose.thetaZ, [robPose.x, robPose.y])

    let handFromTargetRuntime = homeFromHandRuntime.inverse().compose(homeFromTargetRuntime)

    let newHomeFromHandRuntime = homeFromTargetTrained.compose(handFromTargetRuntime.inverse())

    let gripCorrection = newHomeFromHandRuntime.inverse().compose(homeFromHandRuntime)

    newRobPose.x = gripCorrection.trans()[0]
    newRobPose.y = gripCorrection.trans()[1]
    newRobPose.z = 0
    newRobPose.thetaZ = gripCorrection.rotationInDegrees()
    newRobPose.thetaY = 0
    newRobPose.thetaX = 0
    newRobPose.valid = 1

    g_Parts[partID1].gripCorrection[gripperID] = newRobPose
  } else if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
    // let homeFromTargetTrained = new cogMath.cc2XformLinear()
    // homeFromTargetTrained.setXform(trainResult['Home2DFromTarget2D'].xform)

    let handFromTargetTrained = new cogMath.cc2XformLinear()
    handFromTargetTrained.setXform(trainResult['Robot2DFromTarget2D'].xform)

    let homeFromTargetRuntime = new cogMath.cc2XformLinear()
    homeFromTargetRuntime.setXform(runtimeResult['Home2DFromTarget2D'].xform)

    if (!g_Parts.hasOwnProperty(partID2)) {
      newRobPose.valid = ECodes.E_INVALID_PART_ID
      return newRobPose
    }

    let gc = g_Parts[partID2].gripCorrection[gripperID]
    if (gc.valid === 1) {
      let gripCorrection = new cogMath.cc2Rigid()
      gripCorrection.setXform(gc.thetaZ, [gc.x, gc.y])
      let newHomeFromHand = homeFromTargetRuntime.compose(handFromTargetTrained.inverse()).compose(gripCorrection.inverse())
      newRobPose.x = newHomeFromHand.trans()[0]
      newRobPose.y = newHomeFromHand.trans()[1]
      newRobPose.z = trainedRobotPose.z
      newRobPose.thetaZ = newHomeFromHand.rotationInDegrees()
      newRobPose.thetaY = trainedRobotPose.thetaY
      newRobPose.thetaX = trainedRobotPose.thetaX
      newRobPose.valid = 1
    } else {
      newRobPose.valid = ECodes.E_INTERNAL_ERROR
    }
  } else {
    newRobPose.valid = ECodes.E_INVALID_ARGUMENT
  }

  return newRobPose
}
AS200.prototype.ComputeRobotPoses = function(shuttlingPoseIndex, 
                                                    robotPoseXYATrained, 
                                                    featurePositionsTrained, 
                                                    featurePositionsRuntime ){
  myLog('-> Compute Robot Pose ')

  let result = {"valid":ECodes.E_INTERNAL_ERROR}
  let absPosition = {}
  let offPosition = {}

try{
    let robotPoseTrained = new RobotPose(robotPoseXYATrained.x, robotPoseXYATrained.y, 0, robotPoseXYATrained.thetaZ, 0 ,0 ,robotPoseXYATrained.valid)
    let trainResult = ComputeTrained(shuttlingPoseIndex, robotPoseTrained, featurePositionsTrained)
    
    let runtimeResult = ComputeRuntime(shuttlingPoseIndex, featurePositionsRuntime)
    absPosition = ComputeAbsolut(shuttlingPoseIndex, trainResult, runtimeResult)
    offPosition = ComputeOffset(shuttlingPoseIndex,trainResult, absPosition)
    
  
    result['abs'] = absPosition
    result['off'] = offPosition
    result['valid']=1
  } catch (e) {
    myLog('Exception ComputeSeveralRobotPoses')
    myLog(e)
    result['abs'] = {}
    result['off'] = {}
    result['valid']= ECodes.E_UNSPECIFIED
  }
  myLog('<- Compute Robot Pose ')
  return result
}
function ComputeOffset (shuttlingPoseIndex,trainResult, homeFromHandRT_u){
  myLog('-> Compute Offset ')
  let calibXforms = g_Calibrations[shuttlingPoseIndex].results.Transforms

  // The offset position
  let tmpLinear = new cogMath.cc2XformLinear()
  let homeFromHandTrained = new cogMath.cc2Rigid()
  tmpLinear.setXform(trainResult['Home2DFromRobot2D'].xform)
  homeFromHandTrained = cogMath.lin2Rigid(tmpLinear)

  // Convert to uncorrected motion space
  var homeFromHandTrained_u = cogMath.convertToUncorrected(calibXforms, homeFromHandTrained)

  var robotOffXYA = {}
  robotOffXYA['theta'] = homeFromHandRT_u['theta'] - homeFromHandTrained_u.angleInDegrees()
  robotOffXYA['x'] = homeFromHandRT_u['x'] - homeFromHandTrained_u.trans()[0]
  robotOffXYA['y'] = homeFromHandRT_u['y'] - homeFromHandTrained_u.trans()[1]
  robotOffXYA['valid']=1
  myLog('<- Compute Offset ')
  return robotOffXYA
}
function ComputeAbsolut (shuttlingPoseIndex,trainResult, runtimeResult){
  myLog('-> Compute Abs ')

  let calibXforms = g_Calibrations[shuttlingPoseIndex].results.Transforms
  // Step is trained, we are running the "XT" command
  var tmpLinear = new cogMath.cc2XformLinear()
  tmpLinear.setXform(trainResult['Robot2DFromTarget2D'].xform)
  var handFromTarget = cogMath.lin2Rigid(tmpLinear)

  var tmpLinear = new cogMath.cc2XformLinear()
  tmpLinear.setXform(runtimeResult['Home2DFromTarget2D'].xform)
  var homeFromTarget = cogMath.lin2Rigid(tmpLinear)  
  var homeFromHandRT = homeFromTarget.compose(handFromTarget.inverse())
  var homeFromHandRT_u = cogMath.convertToUncorrected(calibXforms, homeFromHandRT)
  
  var robotAbsXYA = {}
  robotAbsXYA['theta'] = homeFromHandRT_u.angleInDegrees()
  robotAbsXYA['x'] = homeFromHandRT_u.trans()[0]
  robotAbsXYA['y'] = homeFromHandRT_u.trans()[1]
  robotAbsXYA['valid'] = 1
  myLog('<- Compute Abs ')
  return robotAbsXYA
}

function ComputeRuntime(shuttlingPosIndex, featurePositonsRuntime){
  let result = ECodes.E_INTERNAL_ERROR;
  
  myLog('-> Compute Runtime ')
  let runtimeData = ComputePosMean(featurePositonsRuntime)
  let runtimeResult = {}
  var homeFromTarget = new cogMath.cc2Rigid()
  homeFromTarget.setXform(runtimeData.theta, [runtimeData.x, runtimeData.y])
  runtimeResult['Home2DFromTarget2D'] = homeFromTarget
    
  result = runtimeResult
  myLog('<- Compute Runtime ')

  return result
}

function ComputeTrained(shuttlingPosIndex, robotPoseTrained, featurePosTrained){
  let result = ECodes.E_INTERNAL_ERROR;
  
  myLog('-> Compute Trained ')
  for (let tf = 0; tf < featurePosTrained.length; tf++) {
    if (featurePosTrained[tf].valid <= 0) {
      result = ECodes.E_TARGET_POSE_NOT_TRAINED
      return result
    }
  }
  if(robotPoseTrained.valid <= 0) {
    result =ECodes.E_ROBOT_POSE_NOT_TRAINED
    return result
  }
  let trainedData = ComputePosMean(featurePosTrained)
  let calibXforms = g_Calibrations[shuttlingPosIndex].results.Transforms
  let trainResult = {}

  let homeFromTarget = new cogMath.cc2Rigid()
    homeFromTarget.setXform(trainedData.theta, [trainedData.x, trainedData.y])
    trainResult['Home2DFromTarget2D'] = homeFromTarget
    
    var homeFromHand = new cogMath.cc2Rigid()
    homeFromHand.setXform(robotPoseTrained.thetaZ, [robotPoseTrained.x, robotPoseTrained.y])
    homeFromHand = cogMath.convertToCorrected(calibXforms, homeFromHand)
    trainResult['Home2DFromRobot2D'] = homeFromHand
        
    var tmpLinear = homeFromHand.inverse().compose(homeFromTarget)
    var handFromTarget = cogMath.lin2Rigid(tmpLinear)
    trainResult['Robot2DFromTarget2D'] = handFromTarget

    result = trainResult  
    myLog('<- Compute Trained ')
  return result
}

function ComputePosMean(featurePositions){  
  let mean  = { 'x': 0, 'y': 0, 'theta': 0 } 

  let length = featurePositions.length
  for(let i=0; i<length;i++){
    mean.x += featurePositions[i].x
    mean.y += featurePositions[i].y
    mean.theta += featurePositions[i].theta
  }

  if (featurePositions.length > 1) {
    mean.x /= length
    mean.y /= length
    mean.theta /= length

    mean.theta = InSightFunctions.fnPointToPoint(featurePositions[0].x, featurePositions[0].y, featurePositions[1].x, featurePositions[1].y, 0).getAngle()
  }  
return mean
}

function ComputeAlignMode_3 (partID1, partID2, gripperID, resMode, robPose) {
  let newRobPose = new RobotPose(0, 0, 0, 0, 0, 0, 0)

  if (!g_Parts.hasOwnProperty(partID1)) {
    newRobPose.valid = ECodes.E_INVALID_PART_ID
    return newRobPose
  }

  let trainedRobotPose = cloneObj(g_Parts[partID1]['trainedRobotPose'][gripperID]) // rtr
  if (trainedRobotPose.valid <= 0) {
    newRobPose.valid = ECodes.E_ROBOT_POSE_NOT_TRAINED
    return newRobPose
  }

  let runtimeFeatuers = g_Parts[partID1]['runtimeFeatures']
  let trainedFeatuers = g_Parts[partID1]['trainedFeatures']

  for (let tf = 0; tf < trainedFeatuers.length; tf++) {
    if (trainedFeatuers[tf][gripperID].valid <= 0) {
      newRobPose.valid = ECodes.E_TARGET_POSE_NOT_TRAINED
      return newRobPose
    }
  }

  let shuttlePose = g_Parts[partID1]['featuresInfos'][0]['shuttlePose']

  if ((runtimeFeatuers.length != trainedFeatuers.length) || (runtimeFeatuers.length > 2) || (runtimeFeatuers.length < 1)) {
    newRobPose.valid = ECodes.E_UNSPECIFIED
    return newRobPose
  }
  let length = runtimeFeatuers.length
  let sumRuntime = { 'x': 0, 'y': 0, 'theta': 0 } // trt
  let sumTrained = { 'x': 0, 'y': 0, 'theta': 0 } // ttr

  for (let i = 0; i < length; i++) {
    sumRuntime.x += runtimeFeatuers[i].x
    sumRuntime.y += runtimeFeatuers[i].y
    sumRuntime.theta += runtimeFeatuers[i].thetaInDegrees

    sumTrained.x += trainedFeatuers[i][gripperID].x
    sumTrained.y += trainedFeatuers[i][gripperID].y
    sumTrained.theta += trainedFeatuers[i][gripperID].thetaInDegrees
  }

  if (length > 1) {
    sumRuntime.x /= length
    sumRuntime.y /= length
    sumRuntime.theta /= length

    sumTrained.x /= length
    sumTrained.y /= length
    sumTrained.theta /= length

    sumRuntime.theta = InSightFunctions.fnPointToPoint(runtimeFeatuers[0].x, runtimeFeatuers[0].y, runtimeFeatuers[1].x, runtimeFeatuers[1].y, 0).getAngle()
    sumTrained.theta = InSightFunctions.fnPointToPoint(trainedFeatuers[0][gripperID].x, trainedFeatuers[0][gripperID].y, trainedFeatuers[1][gripperID].x, trainedFeatuers[1][gripperID].y, 0).getAngle()
  }

  let calibXforms = g_Calibrations[shuttlePose].results.Transforms
  // let isCameraMoving = g_Calibrations[shuttlePose].calibration.isCameraMoving_

  let trainResult = {}
  let runtimeResult = {}
  // Train
  {
    let homeFromTarget = new cogMath.cc2Rigid()
    homeFromTarget.setXform(sumTrained.theta, [sumTrained.x, sumTrained.y])
    trainResult['Home2DFromTarget2D'] = homeFromTarget

    var homeFromHand = new cogMath.cc2Rigid()
    homeFromHand.setXform(trainedRobotPose.thetaZ, [trainedRobotPose.x, trainedRobotPose.y])
    homeFromHand = cogMath.convertToCorrected(calibXforms, homeFromHand)
    trainResult['Home2DFromRobot2D'] = homeFromHand

    var tmpLinear = homeFromHand.inverse().compose(homeFromTarget)
    var handFromTarget = cogMath.lin2Rigid(tmpLinear)
    trainResult['Robot2DFromTarget2D'] = handFromTarget
  }
  // Runtime
  // eslint-disable-next-line no-lone-blocks
  {
    var homeFromTarget = new cogMath.cc2Rigid()
    homeFromTarget.setXform(sumRuntime.theta, [sumRuntime.x, sumRuntime.y])
    runtimeResult['Home2DFromTarget2D'] = homeFromTarget

    // Step is trained, we are running the "XT" command
    var tmpLinear = new cogMath.cc2XformLinear()
    tmpLinear.setXform(trainResult['Robot2DFromTarget2D'].xform)
    var handFromTarget = cogMath.lin2Rigid(tmpLinear)

    var homeFromHandRT = new cogMath.cc2Rigid() // cc2XformLinear();
    homeFromHandRT = homeFromTarget.compose(handFromTarget.inverse())

    var homeFromHandRT_u = cogMath.convertToUncorrected(calibXforms, homeFromHandRT)

    var robotAbsXYA = {}
    robotAbsXYA['A'] = homeFromHandRT_u.angleInDegrees()
    robotAbsXYA['X'] = homeFromHandRT_u.trans()[0]
    robotAbsXYA['Y'] = homeFromHandRT_u.trans()[1]

    // The offset position
    var homeFromHandTrained = new cogMath.cc2Rigid()
    tmpLinear.setXform(trainResult['Home2DFromRobot2D'].xform)
    homeFromHandTrained = cogMath.lin2Rigid(tmpLinear)

    // Convert to uncorrected motion space
    var homeFromHandTrained_u = cogMath.convertToUncorrected(calibXforms, homeFromHandTrained)

    var trainFromRT = new cogMath.cc2Rigid()
    trainFromRT = homeFromHandRT_u.compose(homeFromHandTrained_u.inverse())
  }

  let homeFromHandPocket_u = new cogMath.cc2Rigid()
  homeFromHandPocket_u.setXform(robPose.thetaZ, [robPose.x, robPose.y])
  homeFromHandPocket_u = cogMath.convertToUncorrected(calibXforms, homeFromHandPocket_u)

  let homeFromPocketFrameCorrected = new cogMath.cc2Rigid()
  let fc = g_Parts[partID1].frameCorrection
  if (fc.valid === 1) {
    let tmp = new cogMath.cc2Rigid()
    tmp.setXform(fc.thetaZ, [fc.x, fc.y])
    homeFromPocketFrameCorrected = tmp.compose(homeFromHandPocket_u)
  } else {
    newRobPose.valid = ECodes.E_PART_NO_VALID_FRAME_CORRECTION
    return newRobPose
  }

  if ((resMode === ResultMode.GCP) || (resMode === ResultMode[ResultMode.GCP])) {
    let handFromTargetTrained = new cogMath.cc2XformLinear()
    handFromTargetTrained.setXform(trainResult['Robot2DFromTarget2D'].xform)

    let homeFromTargetRuntime = new cogMath.cc2XformLinear()
    homeFromTargetRuntime.setXform(runtimeResult['Home2DFromTarget2D'].xform)
    if (!g_Parts.hasOwnProperty(partID2)) {
      newRobPose.valid = ECodes.E_INVALID_PART_ID
      return newRobPose
    }
    let gc = g_Parts[partID2].gripCorrection[gripperID]
    let gripCorrection = new cogMath.cc2Rigid()
    if (gc.valid === 1) {
      gripCorrection.setXform(gc.thetaZ, [gc.x, gc.y])
    } else {
      newRobPose.valid = ECodes.E_PART_NO_VALID_GRIP_CORRECTION
      return newRobPose
    }

    let newHomeFromHand = homeFromPocketFrameCorrected.compose(gripCorrection.inverse())
    newRobPose.x = newHomeFromHand.trans()[0]
    newRobPose.y = newHomeFromHand.trans()[1]
    newRobPose.z = trainedRobotPose.z
    newRobPose.thetaZ = newHomeFromHand.angleInDegrees()
    newRobPose.thetaY = trainedRobotPose.thetaY
    newRobPose.thetaX = trainedRobotPose.thetaX
    newRobPose.valid = 1
  } else
  if ((resMode === ResultMode.ABS) || (resMode === ResultMode[ResultMode.ABS])) {
    newRobPose.x = homeFromPocketFrameCorrected.trans()[0]
    newRobPose.y = homeFromPocketFrameCorrected.trans()[1]
    newRobPose.z = trainedRobotPose.z
    newRobPose.thetaZ = homeFromPocketFrameCorrected.angleInDegrees()
    newRobPose.thetaY = trainedRobotPose.thetaY
    newRobPose.thetaX = trainedRobotPose.thetaX
    newRobPose.valid = 1
  } else {
    newRobPose.valid = ECodes.E_INVALID_ARGUMENT
  }
  return newRobPose
}

function Part () {
  this.runtimeFeatures = []
  this.trainedFeatures = []
  this.featuresInfos = []
  this.trainedRobotPose = []
  this.gripCorrection = []
  // for (let g = 0; g < MAX_GRIPPERS; g++) {
  //  this.trainedRobotPose[g] = new RobotPose(0, 0, 0, 0, 0, 0, 0)
  // }
};

//* ***************************************************************************/
// Transoformations
//* ***************************************************************************/
function getTransformed (calibrations, shuttlingPoseIndex, cameraIsMoving, partIsMoving, feature, robot) {
  // let retPos = new Pos();
  let calibration = calibrations[shuttlingPoseIndex]
  //myLogger.addLogMessage(0, 'Feature: ' + feature.x)
  let retFeaturedTransformed = new Feature()
  if (calibration.calibration != null) {
    let heXf = new cogMath.cc2XformLinear()
    let taXf = new cogMath.cc2Rigid()

    taXf.setXform(feature.thetaInDegrees, [feature.x, feature.y])

    if ((cameraIsMoving == 0) && (partIsMoving == 0)) {
      heXf.setXform(calibration['results']['Transforms']['Home2DFromImage2D']['xform'])
      retFeaturedTransformed.valid = 1
    } else if ((cameraIsMoving == 1) && (partIsMoving == 0)) {
      let stageFromImage = new cogMath.cc2XformLinear()
      let homeFromStage = new cogMath.cc2Rigid()

      homeFromStage.setXform(robot.thetaZ, [robot.x, robot.y])
      homeFromStage = cogMath.convertToCorrected(calibration['results']['Transforms'], homeFromStage)
      stageFromImage.setXform(calibration['results']['Transforms']['Stage2DFromImage2D']['xform'])

      heXf.setXform(calibration['results']['Transforms']['Stage2DFromImage2D']['xform'])
      heXf = homeFromStage.compose(stageFromImage)

      retFeaturedTransformed.valid = 1
    } else if ((cameraIsMoving == 0) && (partIsMoving == 1)) {
      let homeFromHand = new cogMath.cc2Rigid()
      homeFromHand.setXform(robot.thetaX, [robot.x, robot.y])
      homeFromHand = cogMath.convertToCorrected(calibration['results']['Transforms'], homeFromHand)

      let xf = new cogMath.cc2XformLinear()
      xf.setXform(calibration['results']['Transforms']['Home2DFromImage2D']['xform'])
      heXf = homeFromHand.inverse().compose(xf)
      retFeaturedTransformed.valid = 1
    } else {
      tracer.addMessage('Combination of moving camera and moving part is not allowed')
    }

    if (retFeaturedTransformed.valid == 1) {
      let comp = heXf.compose(taXf)
      retFeaturedTransformed.x = comp.trans()[0]
      retFeaturedTransformed.y = comp.trans()[1]
      if (comp.hasOwnProperty('rotationInDegrees') === true) {
        retFeaturedTransformed.thetaInDegrees = comp.rotationInDegrees()
      } else {
        retFeaturedTransformed.thetaInDegrees = comp.angleInDegrees()
      }
    }
  } else {
    retFeaturedTransformed.valid = ECodes.E_NOT_CALIBRATED
    if (shuttlingPoseIndex == 2) {
      InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 1)
    }
  }
  return retFeaturedTransformed
};
function getWorldFromCam (calibrations, shuttlingPoseIndex, cam_X, cam_Y, cam_Angle, rob_X, rob_Y, rob_Theta) {
  let world = new Feature(0, 0, 0, 0)

  let calibration = calibrations[shuttlingPoseIndex]

  if (calibration.runstatus == 1) {
    var homeFromTarget = new cogMath.cc2XformLinear()
    var homeFromImage = new cogMath.cc2XformLinear()

    var camFromTarget = new cogMath.cc2XformLinear()
    var tmp = new cogMath.cc2Rigid()
    tmp.setXform(cam_Angle, [cam_X, cam_Y])
    camFromTarget.setXform(tmp.xform)

    if (calibration.calibration.isCameraMoving_ == true) {
      world.valid = ECodes.E_COMMAND_NOT_ALLOWED
    } else {
      homeFromImage.setXform(calibration.results.Transforms.Home2DFromCameraCenter2D.xform)
      homeFromTarget = homeFromImage.compose(camFromTarget)
    }

    world.x = homeFromTarget.trans()[0]
    world.y = homeFromTarget.trans()[1]
    world.thetaInDegrees = homeFromTarget.rotationInDegrees()

    world.valid = 1
  } else {
    world.valid = ECodes.E_NOT_CALIBRATED
    if (shuttlingPoseIndex == 2) {
      InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 1)
    }
  }
  return world
};
function getCamFromWorld (calibrations, shuttlingPoseIndex, world_X, world_Y, world_Angle) {
  var point = new Feature(0, 0, 0, 0)

  let calibration = calibrations[shuttlingPoseIndex]

  if (calibration.runstatus == 1) {
    if (calibration.calibration['isCameraMoving_'] == 1) {
    } else {
      let homeFromCamera = calibration.results.Transforms.Home2DFromCameraCenter2D

      var taXf = new cogMath.cc2Rigid()
      taXf.setXform(world_Angle, [world_X, world_Y])

      var comp = homeFromCamera.inverse().compose(taXf)

      point.x = comp.trans()[0]
      point.y = comp.trans()[1]
      point.thetaInDegrees = comp.rotationInDegrees()
      point.valid = 1
    }
  } else {
    point.valid = ECodes.E_NOT_CALIBRATED
    if (shuttlingPoseIndex == 2) {
      InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 1)
    }
  }
  return point
};
function getWorldFromImage (calibrations, shuttlingPoseIndex, img_X, img_Y, img_Angle, rob_X, rob_Y, rob_Theta) {
  var world = new Feature(0, 0, 0, 0)

  let calibration = calibrations[shuttlingPoseIndex]
  if (calibration.runstatus == 1) {
    var homeFromTarget = new cogMath.cc2XformLinear()
    var homeFromImage = new cogMath.cc2XformLinear()

    var imageFromTarget = new cogMath.cc2XformLinear()
    var tmp = new cogMath.cc2Rigid()
    tmp.setXform(img_Angle, [img_X, img_Y])
    imageFromTarget.setXform(tmp.xform)

    if (calibration.calibration.isCameraMoving_ == true) {
      var stageFromImage = new cogMath.cc2XformLinear()
      stageFromImage.setXform(calibration.results.Transforms.Stage2DFromImage2D.xform)

      var homeFromHand = new cogMath.cc2XformLinear()
      var tmp2 = new cogMath.cc2Rigid()

      tmp2.setXform(rob_Theta, [rob_X * 1.0, rob_Y * 1.0])
      homeFromHand.setXform(tmp2.xform)

      homeFromTarget = homeFromHand.compose(stageFromImage).compose(imageFromTarget)
    } else {
      homeFromImage.setXform(calibration.results.Transforms.Home2DFromImage2D.xform)
      homeFromTarget = homeFromImage.compose(imageFromTarget)
    }

    world.x = homeFromTarget.trans()[0]
    world.y = homeFromTarget.trans()[1]
    world.thetaInDegrees = homeFromTarget.rotationInDegrees()
    world.valid = 1
  } else {
    world.valid = ECodes.E_NOT_CALIBRATED
    if (shuttlingPoseIndex == 2) {
      InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 1)
    }
  }
  return world
};
function getImageFromWorld (calibrations, shuttlingPoseIndex, world_X, world_Y, world_Angle, rob_X, rob_Y, rob_Theta) {
  var point = new Feature(0, 0, 0, 0)

  let calibration = calibrations[shuttlingPoseIndex]

  if (calibration.runstatus == 1) {
    if (calibration.calibration['isCameraMoving_'] == 1) {

    } else {
      var homeFromImage = calibration.results.Transforms.Home2DFromImage2D

      var taXf = new cogMath.cc2Rigid()
      taXf.setXform(world_Angle, [world_X, world_Y])

      var comp = homeFromImage.inverse().compose(taXf)

      point.x = comp.trans()[0]
      point.y = comp.trans()[1]
      point.thetaInDegrees = comp.rotationInDegrees()
      point.valid = 1
    }
  } else {
    point.valid = ECodes.E_NOT_CALIBRATED
    if (shuttlingPoseIndex == 2) {
      InSightFunctions.fnSetCellValue('HECalibration.2.ShowNotValid', 1)
    }
  }
  return point
}

//* ***************************************************************************/
// Logger
//* ***************************************************************************/
function Logger () {
  this.messages = []
  this.logCounter = 0
};
/*
  0 -> dim grey normal message
  1 -> green
  2 -> yellow
  3 -> orange
  4 -> red
  5 -> grey     incomming command
*/
Logger.prototype.addLogMessage = function (type, message) {
  let time = process.hrtime()

  let sec = time[0]
  let min = Math.floor(sec / 60)
  sec %= 60
  let hour = Math.floor(min / 60)
  min %= 60
  let milSec = Math.floor(time[1] / 1000000)

  let msg = InSightFunctions.fnStringf('%02d:%02d:%02d:%03d %s', hour, min, sec, milSec, message)// .toString() +": "+message;
  this.messages.splice(0, 0, [type, msg])
  //Increase logCounter here to sync log to Inspection Log
  this.logCounter++
}
Logger.prototype.clearLog = function () {
  for (var i = 1; i <= MAX_LOGGING_ENTRIES; i++) {
    InSightFunctions.fnSetCellValue('Logging.MessageType_' + i.toString(), 0)
    InSightFunctions.fnSetCellValue('Logging.Message_' + i.toString(), '')
  }
  this.messages = []
  this.logCounter = 0
  this._updateSheet()
}
Logger.prototype.writeLogToSheet = function () {
  tracer.addMessage('-> Write Log ' + timeTracker.getElapsedTime())

  let logLength = this.messages.length

  if (logLength > MAX_LOGGING_ENTRIES) {
    this.messages.splice(60, logLength - 60)
  }
  logLength = this.messages.length

  for (var i = 0; i < logLength; i++) {
    InSightFunctions.fnSetCellValue('Logging.MessageType_' + (i + 1).toString(), this.messages[i][0])
    InSightFunctions.fnSetCellValue('Logging.Message_' + (i + 1).toString(), this.messages[i][1])
  }

  this._updateSheet()

  tracer.addMessage('<- Write Log ' + timeTracker.getElapsedTime())
}
Logger.prototype._updateSheet = function () {
  tracer.addMessage('-> Update Logger on the sheet ' + timeTracker.getElapsedTime())
  //
  let logLength = this.messages.length
  InSightFunctions.fnSetCellValue('Logging.Counter', this.logCounter)
  InSightFunctions.fnSetCellValue('Logging.Count', logLength)

  InSightFunctions.fnUpdateGui(1)

  tracer.addMessage('<- Update Logger on the sheet ' + timeTracker.getElapsedTime())
}
//* ***************************************************************************/
// functions
//* ***************************************************************************/
function setInspectionAcqSettings (index, enabled) {
  if (g_Inspections.hasOwnProperty(index) && enabled) {
    let settings = g_Inspections[index].acqSettings
    // InSightFunctions.fnSetCellValue('Inspections.SelectedIndex', index)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.ExposureTime', settings.exposure)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.LightControl.Mode', settings.mode)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.LightControl.Light_1', settings.light1)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.LightControl.Light_2', settings.light2)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.LightControl.Light_3', settings.light3)
    InSightFunctions.fnSetCellValue('InspectionAcquisitionSettings.LightControl.Light_4', settings.light4)
    InSightFunctions.fnSetCellValue('AcquisitionSettings.Selector', 2)
  }
}

function checkTagNameAvailable (tagName) {
  var available = false
  try {
    let cell = InSightFunctions.fnGetCellName(tagName)
    available = true
  } catch (error) {
    tracer.addMessage('Tag <' + tagName + '> not found!')
  }
  return available
}
//* ***************************************************************************/
// Border
//* ***************************************************************************/

function CrossHair (row_0, col_0, angle) {
  tracer.addMessage('-> CrossHair')
  if (Math.abs(angle % 90) < 1e-15) {
    angle = angle + 1e-15
  }

  let row_1 = row_0 + 100
  let col_1 = col_0 + (100 * Math.tan(angle * Math.PI / 180))

  let row_2 = row_0 + 100
  let col_2 = col_0 + (100 * Math.tan((angle + 90) * Math.PI / 180))

  let line_1 = []
  let line_2 = []
  for (var i = 0; i < 4; i++) {
    var dist_1 = InSightFunctions.fnLineToLine(row_0, col_0, row_1, col_1, g_Border[i][0], g_Border[i][1], g_Border[i][2], g_Border[i][3], 0)
    if (dist_1.getDistance() == 0) {
      var row_intersect_1 = dist_1.getRow(0)
      var col_intersect_1 = dist_1.getCol(0)

      if ((row_intersect_1 >= 0) && (Math.floor(row_intersect_1) <= g_VRes) &&
        (col_intersect_1 >= 0) && (Math.floor(col_intersect_1) <= g_HRes)) {
        line_1.push([row_intersect_1, col_intersect_1])
      }
    }

    var dist_2 = InSightFunctions.fnLineToLine(row_0, col_0, row_2, col_2, g_Border[i][0], g_Border[i][1], g_Border[i][2], g_Border[i][3], 0)
    if (dist_2.getDistance() == 0) {
      var row_intersect_2 = dist_2.getRow(0)
      var col_intersect_2 = dist_2.getCol(0)

      if ((row_intersect_2 >= 0) && (Math.floor(row_intersect_2) <= g_VRes) &&
        (col_intersect_2 >= 0) && (Math.floor(col_intersect_2) <= g_HRes)) {
        line_2.push([row_intersect_2, col_intersect_2])
      }
    }
  }
  tracer.addMessage('<- CrossHair')
  return [[line_1[0][0], line_1[0][1], line_1[1][0], line_1[1][1]],
    [line_2[0][0], line_2[0][1], line_2[1][0], line_2[1][1]]]
};

//* ***************************************************************************/
// HELPERS
//* ***************************************************************************/

var SharedObjects = (function () {
  // Instance stores a reference to the Singleton
  var instance
  function init () {
    // Singleton
    // Private methods and variables
    // function privateMethod () {
    //  console.log('I am private')
    // }

    var sharedObjects = {}
    // var privateVariable = "Im also private";

    return {
      // Public methods and variables
      addSharedObject: function (name, obj) {
        if (sharedObjects.hasOwnProperty(name)) {
          tracer.addMessage('Object-list contains such a Name!')
        } else {
          sharedObjects[name] = obj
        }
        return sharedObjects[name]
      },

      getSharedObject: function (name) {
        let obj = null
        if (sharedObjects.hasOwnProperty(name)) {
          obj = sharedObjects[name]
        }
        return obj
      },

      removeSharedObject: function (name) {
        if (sharedObjects.hasOwnProperty(name)) {
          delete sharedObjects[name]
        }
      }
      // publicProperty: "I am also public",
    }
  };

  return {
    // Get the Singleton instance if one exists
    // or create one if it doesn't
    getInstance: function () {
      if (!instance) {
        instance = init()
      }

      return instance
    }
  }
})()

function cloneObj (obj) {
  var clone = {}
  for (var i in obj) {
    if (obj[i] != null && typeof (obj[i]) === 'object') { clone[i] = cloneObj(obj[i]) } else { clone[i] = obj[i] }
  }
  return clone
}
function checkAndGetObjectFromJsonString (objString) {
  let ret = null

  if (objString.length > 2) {
    if (typeof JSON.parse(objString) === 'object') {
      ret = JSON.parse(objString)
    }
  }
  return ret
};
Math.radians = function (degrees) {
  return degrees * Math.PI / 180
}

Math.degrees = function (radians) {
  return radians * 180 / Math.PI
}

Number.prototype.between = function (a, b, inclusive) {
  var min = Math.min(a, b)
  var max = Math.max(a, b)

  return inclusive ? this >= min && this <= max : this > min && this < max
}
Number.prototype.checkAngleDelta = function (maxDelta, inclusive) {
  var delta = Math.abs(this)

  if (delta > 180) { delta -= 360 }

  return Math.abs(delta).between(0, maxDelta, inclusive)
}
module.exports.SharedObjects = SharedObjects
module.exports.ECodes = ECodes
module.exports.AS200 = AS200
module.exports.version = version

console.log('Loading AS200 done')
