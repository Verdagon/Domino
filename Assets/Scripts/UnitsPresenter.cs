using System;
using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class UnitsPresenter {
    public delegate int IGetElevation(Location loc);
    
    private DominoToGameConnection server;
    private Pattern pattern;
    private float elevationStepHeight;
    IClock clock;
    ITimer timer;
    ILoader loader;
    private Vector3 lookatOffsetToCamera;
    // private TileShapeMeshCache tileShapeMeshCache;
    Dictionary<ulong, NetworkUnitPresenter> idToUnitPresenter = new Dictionary<ulong, NetworkUnitPresenter>();
    Dictionary<Location, HashSet<ulong>> locToUnitIds = new Dictionary<Location, HashSet<ulong>>();
    private IGetElevation getElevation;

    public UnitsPresenter(
        DominoToGameConnection server,
        IClock clock,
        ITimer timer,
        ILoader loader,
        Pattern pattern,
        Vector3 lookatOffsetToCamera,
        float elevationStepHeight,
        IGetElevation getElevation) {
      this.pattern = pattern;
      // this.tileShapeMeshCache = new TileShapeMeshCache(pattern);
      this.server = server;
      this.clock = clock;
      this.timer = timer;
      this.loader = loader;
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      this.elevationStepHeight = elevationStepHeight;
      this.getElevation = getElevation;
      this.idToUnitPresenter = new Dictionary<ulong, NetworkUnitPresenter>();
    }

    public void DestroyUnitsPresenter() {
      foreach (var entry in idToUnitPresenter) {
        entry.Value.Destroy();
      }
    }

    public void HandleMessage(IDominoMessage message) {
      if (message is CreateUnitMessage createUnit) {
        idToUnitPresenter.Add(
            createUnit.id,
            new NetworkUnitPresenter(
                loader,
                clock,
                timer,
                elevationStepHeight,
                pattern,
                createUnit.id,
                lookatOffsetToCamera,
                createUnit.initialUnit,
                loc => getElevation(loc)));
        if (!locToUnitIds.ContainsKey(createUnit.initialUnit.location)) {
          locToUnitIds.Add(createUnit.initialUnit.location, new HashSet<ulong>());
        }
        locToUnitIds[createUnit.initialUnit.location].Add(createUnit.id);
      } else if (message is DestroyUnitMessage destroyUnit) {
        var loc = idToUnitPresenter[destroyUnit.unitViewId].location;
        locToUnitIds[loc].Remove(destroyUnit.unitViewId);
        idToUnitPresenter[destroyUnit.unitViewId].Destroy();
        idToUnitPresenter.Remove(destroyUnit.unitViewId);
      // } else if (message is SetSurfaceColorMessage setSurfaceColor) {
      //   unitPresenters[setSurfaceColor.tileViewId].HandleMessage(message);
      // } else if (message is SetCliffColorMessage setCliffColor) {
      //   unitPresenters[setCliffColor.tileViewId].HandleMessage(message);
      } else {
        Asserts.Assert(false, message.GetType().Name);
      }
    }

    public void RefreshElevation(Location location) {
      if (locToUnitIds.TryGetValue(location, out var unitIds)) {
        foreach (var id in unitIds) {
          idToUnitPresenter[id].RefreshElevation();
        }
      }
    }

    public void SetCameraDirection(Vector3 lookatOffsetToCamera) {
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      foreach (var idAndUnitPresenter in idToUnitPresenter) {
        idAndUnitPresenter.Value.SetCameraDirection(lookatOffsetToCamera);{{}}
      }
    }
  }
}
