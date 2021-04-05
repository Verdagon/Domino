using System;
using System.Collections.Generic;
using Domino;
using Geomancer;
using Geomancer.Model;
using UnityEngine;


public class Root : MonoBehaviour {
  private Camera camera;
  private Canvas canvas;
  private OverlayPaneler overlayPaneler;
  private CameraController cameraController;
  private SlowableTimerClock clock;
  private Loader loader;
  private DominoToGameConnection server;
  private TerrainPresenter terrainPresenter;
  private PanelPresenter panelPresenter;

  // public so we can see it in the unity editor
  public bool finishedStartMethod = false;

  void Start() {
    loader = Loader.Create(gameObject);
    loader.onReady += AfterLoaded;
  }

  void AfterLoaded() {
    server = new DominoToGameConnection();

    camera = GetComponentInChildren<Camera>();
    canvas = GetComponentInChildren<Canvas>();

    clock = new SlowableTimerClock(1.0f);

    overlayPaneler = new OverlayPaneler(canvas.gameObject, loader, clock);
    panelPresenter = new PanelPresenter(clock, clock, loader, overlayPaneler, server);

    cameraController =
        new CameraController(
            clock,
            camera,
            new Vector3(0, 0, 0), //terrain.GetTileCenter(startLocation).ToUnity(),
            new Vector3(0, -10, 5));

    server.Start(overlayPaneler.screenGW, overlayPaneler.screenGH);
    // start here

    finishedStartMethod = true;
  }

  public void Update() {
    if (!finishedStartMethod) {
      // There was probably an error in the logs that said why we're not loaded
      return;
    }

    var messages = server.TakeMessages();
    foreach (var message in messages) {
      if (message is SetupGameMessage setupGame) {
        terrainPresenter =
            new TerrainPresenter(
                server, clock, clock, loader, setupGame.pattern, setupGame.elevationStepHeight);
        // setupGame.cameraPosition;
      } else if (message is CreateTileMessage ||
          message is SetSurfaceColorMessage ||
          message is SetCliffColorMessage ||
          message is DestroyTileMessage) {
        terrainPresenter.HandleMessage(message);
      } else if (message is MakePanelMessage ||
          message is ScheduleCloseMessage ||
          message is AddRectangleMessage ||
          // message is AddStringMessage ||
          message is AddSymbolMessage ||
          message is RemoveViewMessage ||
          message is SetFadeInMessage ||
          message is SetFadeOutMessage) {
        panelPresenter.HandleMessage(message);
      } else {
        Debug.LogWarning("Ignoring: " + message.GetType().Name);
      }
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

    bool ctrlDown = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
    bool leftAltDown = Input.GetKeyDown(KeyCode.LeftAlt);
    bool rightAltDown = Input.GetKeyDown(KeyCode.RightAlt);
    bool leftShiftDown = Input.GetKeyDown(KeyCode.LeftShift);
    bool rightShiftDown = Input.GetKeyDown(KeyCode.RightShift);

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
