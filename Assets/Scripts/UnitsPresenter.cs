using System;
using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class UnitsPresenter {
    private DominoToGameConnection server;
    private Pattern pattern;
    private float elevationStepHeight;
    IClock clock;
    ITimer timer;
    ILoader loader;
    private Vector3 lookatOffsetToCamera;
    // private TileShapeMeshCache tileShapeMeshCache;
    Dictionary<ulong, NetworkUnitPresenter> unitPresenters = new Dictionary<ulong, NetworkUnitPresenter>();

    public UnitsPresenter(
        DominoToGameConnection server,
        IClock clock,
        ITimer timer,
        ILoader loader,
        Pattern pattern,
        Vector3 lookatOffsetToCamera,
        float elevationStepHeight) {
      this.pattern = pattern;
      // this.tileShapeMeshCache = new TileShapeMeshCache(pattern);
      this.server = server;
      this.clock = clock;
      this.timer = timer;
      this.loader = loader;
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      this.elevationStepHeight = elevationStepHeight;
      this.unitPresenters = new Dictionary<ulong, NetworkUnitPresenter>();
    }

    public void DestroyUnitsPresenter() {
      foreach (var entry in unitPresenters) {
        entry.Value.Destroy();
      }
    }

    public void HandleMessage(IDominoMessage message) {
      if (message is CreateUnitMessage createUnit) {
        unitPresenters.Add(
            createUnit.id,
            new NetworkUnitPresenter(
                loader, clock, timer, elevationStepHeight, pattern, createUnit.id, lookatOffsetToCamera, createUnit.initialUnit));
      } else if (message is DestroyUnitMessage destroyUnit) {
        unitPresenters[destroyUnit.unitViewId].Destroy();
        unitPresenters.Remove(destroyUnit.unitViewId);
      // } else if (message is SetSurfaceColorMessage setSurfaceColor) {
      //   unitPresenters[setSurfaceColor.tileViewId].HandleMessage(message);
      // } else if (message is SetCliffColorMessage setCliffColor) {
      //   unitPresenters[setCliffColor.tileViewId].HandleMessage(message);
      } else {
        Asserts.Assert(false, message.GetType().Name);
      }
    }

    public void SetCameraDirection(Vector3 lookatOffsetToCamera) {
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      foreach (var idAndUnitPresenter in unitPresenters) {
        idAndUnitPresenter.Value.SetCameraDirection(lookatOffsetToCamera);{{}}
      }
    }
  }
}
