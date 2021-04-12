using System;
using System.Collections;
using System.Collections.Generic;
using Domino;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using UnityEngine.Networking;


public class Root : MonoBehaviour {
  private Camera camera;
  private Canvas canvas;
  private Requester requester;
  private OverlayPaneler overlayPaneler;
  private CameraController cameraController;
  private SlowableTimerClock clock;
  private Loader loader;
  private DominoToGameConnection server;
  private Pattern pattern;
  private float elevationStepHeight;
  private TerrainPresenter terrainPresenter;
  private UnitsPresenter unitsPresenter;
  private PanelPresenter panelPresenter;

  // public so we can see it in the unity editor
  public bool finishedStartMethod = false;

  void Start() {
    loader = Loader.Create(gameObject);
    loader.onReady += AfterLoaded;
  }

  void AfterLoaded() {
    camera = GetComponentInChildren<Camera>();
    canvas = GetComponentInChildren<Canvas>();

    requester = gameObject.AddComponent<Requester>();

    server = new DominoToGameConnection(requester, "http://127.0.0.1:8080", HandleMessage);

    clock = new SlowableTimerClock(1.0f);

    overlayPaneler = new OverlayPaneler(canvas.gameObject, loader, clock);
    panelPresenter = new PanelPresenter(clock, clock, loader, overlayPaneler, server);

    cameraController =
        new CameraController(
            clock,
            camera,
            new Vector3(0, 0, 0),
            new Vector3(0, -10, 5));

    server.Start(overlayPaneler.screenGW, overlayPaneler.screenGH);
    // start here

    finishedStartMethod = true;
  }

  private void HandleMessage(IDominoMessage message) {

    if (message is SetupGameMessage setupGame) {
      Debug.Log(
          $"SetupGameMessage cameraPosition {setupGame.cameraPosition.ToUnity()} lookatOffsetToCamera {setupGame.lookatOffsetToCamera.ToUnity()} elevationStepHeight {setupGame.elevationStepHeight} pattern {setupGame.pattern}");
      cameraController.StartRotatingCameraTo(setupGame.lookatOffsetToCamera.ToUnity(), 1000);
      elevationStepHeight = setupGame.elevationStepHeight * ModelExtensions.ModelToUnityMultiplier;
      pattern = setupGame.pattern;
      terrainPresenter = new TerrainPresenter(server, clock, clock, loader, pattern, elevationStepHeight);
      unitsPresenter =
          new UnitsPresenter(
              server,
              clock,
              clock,
              loader,
              pattern,
              cameraController.targetLookatOffsetToCamera,
              elevationStepHeight,
              loc => terrainPresenter.GetElevation(loc));
      // setupGame.cameraPosition;
    } else if (message is SetElevationMessage setElevation) {
      terrainPresenter.SetElevation(setElevation.tileViewId, setElevation.elevation);
      unitsPresenter.RefreshElevation(terrainPresenter.GetLocation(setElevation.tileViewId));
    } else if (message is CreateTileMessage ||
        message is SetSurfaceColorMessage ||
        message is SetCliffColorMessage ||
        message is DestroyTileMessage ||
        message is SetOverlayMessage ||
        message is SetFeatureMessage ||
        message is AddItemMessage ||
        message is RemoveItemMessage ||
        message is ClearItemsMessage) {
      terrainPresenter.HandleMessage(message);
    } else if (message is CreateUnitMessage ||
        message is DestroyUnitMessage) {
      unitsPresenter.HandleMessage(message);

      // var location = createUnit.initialUnit.location;
      // var position = pattern.GetTileCenter(location).ToVec3().ToUnity();
      // position.y += createUnit.initialUnit.elevation * elevationStepHeight;
      // UnitView.Create(
      //     loader, clock, clock, position,
      //     new UnitDescription(
      //         new DominoDescription(true, Vector4Animation.RED),
      //         new ExtrudedSymbolDescription(
      //             RenderPriority.DOMINO,
      //             new SymbolDescription(
      //                 new SymbolId("AthSymbols", 0x006A),
      //                 Vector4Animation.BLUE,
      //                 0,
      //                 0,
      //                 OutlineMode.WithOutline),
      //             true,
      //             Vector4Animation.PINK),
      //         new List<(ulong, ExtrudedSymbolDescription)>(),
      //         1,
      //         1),
      //     new Vector3(0, -1, 0));
    } else if (message is MakePanelMessage ||
        message is ScheduleCloseMessage ||
        message is AddRectangleMessage ||
        // message is AddStringMessage ||
        message is AddSymbolMessage ||
        message is RemoveViewMessage ||
        message is SetFadeInMessage ||
        message is SetFadeOutMessage ||
        message is AddButtonMessage) {
      panelPresenter.HandleMessage(message);
    } else {
      Debug.LogWarning("Ignoring: " + message.GetType().Name);
    }
  }

  public void Update() {
    if (!finishedStartMethod) {
      // There was probably an error in the logs that said why we're not loaded
      return;
    }
    if (unitsPresenter == null) {
      return;
    }
    if (terrainPresenter == null) {
      return;
    }

    clock.Update();

    if (Input.GetKey(KeyCode.RightBracket)) {
      cameraController.MoveIn(Time.deltaTime);
    }
    if (Input.GetKey(KeyCode.LeftBracket)) {
      cameraController.MoveOut(Time.deltaTime);
    }
    if (Input.GetKey(KeyCode.UpArrow)) {
      cameraController.MoveUp(Time.deltaTime);
    }
    if (Input.GetKey(KeyCode.DownArrow)) {
      cameraController.MoveDown(Time.deltaTime);
    }
    if (Input.GetKey(KeyCode.RightArrow)) {
      cameraController.MoveRight(Time.deltaTime);
    }
    if (Input.GetKey(KeyCode.LeftArrow)) {
      cameraController.MoveLeft(Time.deltaTime);
    }
    
    if (Input.GetKeyDown(KeyCode.Backslash)) {
      cameraController.StartRotatingCameraTo(
          Quaternion.Euler(0, 70, 0) * cameraController.targetLookatOffsetToCamera,
          200);
      // unitsPresenter.SetCameraDirection(cameraController.lookatOffsetToCamera);
    }
    if (Input.GetKeyDown(KeyCode.Slash)) {
      cameraController.StartRotatingCameraTo(
          Quaternion.Euler(0, -70, 0) * cameraController.targetLookatOffsetToCamera,
          200);
    }
    
    unitsPresenter.SetCameraDirection(cameraController.GetCurrentLookatOffsetToCamera());

    bool ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    bool leftAltDown = Input.GetKey(KeyCode.LeftAlt);
    bool rightAltDown = Input.GetKey(KeyCode.RightAlt);
    bool leftShiftDown = Input.GetKey(KeyCode.LeftShift);
    bool rightShiftDown = Input.GetKey(KeyCode.RightShift);

    var keyCodes = new List<KeyCode> {
        KeyCode.Escape, KeyCode.Slash, KeyCode.Equals, KeyCode.Plus, KeyCode.Minus, KeyCode.Underscore, KeyCode.Delete
    };
    for (int i = 'a'; i <= 'z'; i++) {
      keyCodes.Add((KeyCode) i);
    }
    foreach (var keyCode in keyCodes) {
      if (Input.GetKeyDown(keyCode)) {
        server.KeyDown((int) keyCode, leftShiftDown, rightShiftDown, ctrlDown, leftAltDown, rightAltDown);
      }
    }
    if (Input.GetKeyDown(KeyCode.Mouse1)) {
      server.KeyDown(-1, leftShiftDown, rightShiftDown, ctrlDown, leftAltDown, rightAltDown);
    }
    if (Input.GetKeyDown(KeyCode.Mouse2)) {
      server.KeyDown(-2, leftShiftDown, rightShiftDown, ctrlDown, leftAltDown, rightAltDown);
    }

    UnityEngine.Ray ray = camera.ScreenPointToRay(Input.mousePosition);
    terrainPresenter.UpdateMouse(ray);
  }
}
