using System;
using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public class TerrainPresenter {
    private DominoToGameConnection server;
    private Pattern pattern;
    private float elevationStepHeight;
    IClock clock;
    ITimer timer;
    ILoader loader;
    private TileShapeMeshCache tileShapeMeshCache;
    Dictionary<ulong, NetworkTilePresenter> idToTilePresenters = new Dictionary<ulong, NetworkTilePresenter>();
    Dictionary<Location, NetworkTilePresenter> locToTilePresenters = new Dictionary<Location, NetworkTilePresenter>();

    TileView maybeMouseHoveredLocation = null;
    // private SortedSet<TileView> highlightedLocations = new SortedSet<Location>();

    public TerrainPresenter(
        DominoToGameConnection server,
        IClock clock,
        ITimer timer,
        ILoader loader,
        Pattern pattern,
        float elevationStepHeight) {
      this.pattern = pattern;
      this.tileShapeMeshCache = new TileShapeMeshCache(pattern);
      this.server = server;
      this.clock = clock;
      this.timer = timer;
      this.loader = loader;
      this.elevationStepHeight = elevationStepHeight;
      this.idToTilePresenters = new Dictionary<ulong, NetworkTilePresenter>();
      this.locToTilePresenters = new Dictionary<Location, NetworkTilePresenter>();

      // foreach (var locationAndTile in terrain.tiles) {
      //   addTerrainTile(locationAndTile.Key, locationAndTile.Value);
      // }

      // RefreshPhantomTiles();
    }

    // public void AddTile(TerrainTilePresenter presenter) {
    //   tilePresenters.Add(presenter.location, presenter);
    // }

    // public Location GetMaybeMouseHighlightLocation() { return maybeMouseHighlightedLocation; }

    public void DestroyTerrainPresenter() {
      foreach (var entry in idToTilePresenters) {
        entry.Value.Destroy();
      }
      idToTilePresenters.Clear();
      locToTilePresenters.Clear();
    }

    public int GetElevation(Location loc) {
      return locToTilePresenters[loc].elevation;
    }
    public Location GetLocation(ulong tileViewId) {
      return idToTilePresenters[tileViewId].location;
    }

    public void SetElevation(ulong tileViewId, int elevation) {
      idToTilePresenters[tileViewId].SetElevation(elevation);
    }

    public void HandleMessage(IDominoMessage message) {
      if (message is CreateTileMessage createTile) {
        var tilePresenter =
            new NetworkTilePresenter(
                loader, clock, timer, tileShapeMeshCache, elevationStepHeight, pattern, createTile.id,
                createTile.initialTile);
        idToTilePresenters.Add(createTile.id, tilePresenter);
        locToTilePresenters.Add(createTile.initialTile.location, tilePresenter);
      } else if (message is DestroyTileMessage destroyTile) {
        var loc = idToTilePresenters[destroyTile.tileViewId].location;
        idToTilePresenters[destroyTile.tileViewId].Destroy();
        idToTilePresenters.Remove(destroyTile.tileViewId);
        locToTilePresenters.Remove(loc);
      } else if (message is SetSurfaceColorMessage setSurfaceColor) {
        idToTilePresenters[setSurfaceColor.tileViewId].HandleMessage(message);
      } else if (message is SetElevationMessage setElevation) {
        idToTilePresenters[setElevation.tileViewId].HandleMessage(message);
      } else if (message is SetCliffColorMessage setCliffColor) {
        idToTilePresenters[setCliffColor.tileViewId].HandleMessage(message);
      } else {
        Asserts.Assert(false, message.GetType().Name);
      }
    }

    public void UpdateMouse(UnityEngine.Ray ray) {
      var tileView = TileViewUnderMouse(ray);
      if (tileView != maybeMouseHoveredLocation) {
        maybeMouseHoveredLocation = tileView;
        if (maybeMouseHoveredLocation != null) {
          server.SetHoveredLocation(maybeMouseHoveredLocation.tileViewId, maybeMouseHoveredLocation.location);
        } else {
          server.SetHoveredLocation(0, null);
        }
      }

      if (Input.GetMouseButtonDown(0)) {
        if (maybeMouseHoveredLocation != null) {
          server.LocationMouseDown(maybeMouseHoveredLocation.tileViewId, maybeMouseHoveredLocation.location);
        }
      }
    }

    private TileView TileViewUnderMouse(UnityEngine.Ray ray) {
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit)) {
        if (hit.collider != null) {
          var gameObject = hit.collider.gameObject;
          
          var tileView = gameObject.GetComponentInParent<TileView>();
          if (tileView) {
            return tileView;
          }
        }
      }
      return null;
    }
    
    // public void AddTile(TerrainTile tile) {
    //   if (phantomTilePresenters.TryGetValue(tile.location, out var presenter)) {
    //     presenter.DestroyPhantomTilePresenter();
    //     phantomTilePresenters.Remove(tile.location);
    //   }
    //   terrain.tiles.Add(tile.location, tile);
    //   addTerrainTile(tile.location, terrain.tiles[tile.location]);
    //   RefreshPhantomTiles();
    // }
    // public void RemoveTile(TerrainTile tile) {
    //   tilePresenters.Remove(tile.location);
    //   var newHighlightedLocations = new SortedSet<Location>(highlightedLocations);
    //   newHighlightedLocations.Remove(tile.location);
    //   SetHighlightedLocations(newHighlightedLocations);
    //   RefreshPhantomTiles();
    // }
    // public TerrainTilePresenter GetTilePresenter(Location location) {
    //   if (tilePresenters.TryGetValue(location, out var presenter)) {
    //     return presenter;
    //   }
    //   return null;
    // }
    //
    // private void RefreshPhantomTiles() {
    //   var phantomTileLocations =
    //     terrain.pattern.GetAdjacentLocations(new SortedSet<Location>(terrain.tiles.Keys), false, true);
    //   var previousPhantomTileLocations = phantomTilePresenters.Keys;
    //
    //   var addedPhantomTileLocations = new SortedSet<Location>(phantomTileLocations);
    //   SetUtils.RemoveAll(addedPhantomTileLocations, previousPhantomTileLocations);
    //
    //   var removedPhantomTileLocations = new SortedSet<Location>(previousPhantomTileLocations);
    //   SetUtils.RemoveAll(removedPhantomTileLocations, phantomTileLocations);
    //
    //   foreach (var removedPhantomTileLocation in removedPhantomTileLocations) {
    //     removePhantomTile(removedPhantomTileLocation);
    //   }
    //
    //   foreach (var addedPhantomTileLocation in addedPhantomTileLocations) {
    //     addPhantomTile(addedPhantomTileLocation);
    //   }
    // }

    // private void removePhantomTile(Location removedPhantomTileLocation) {
    //   phantomTilePresenters[removedPhantomTileLocation].DestroyPhantomTilePresenter();
    //   phantomTilePresenters.Remove(removedPhantomTileLocation);
    // }
    //
    // private void addTerrainTile(Location location, TerrainTile tile) {
    //   var presenter = new TerrainTilePresenter(clock, timer, vivimap, terrain, location, tile, loader, tileShapeMeshCache);
    //   tilePresenters.Add(location, presenter);
    // }
    //
    // private void addPhantomTile(Location location) {
    //   var presenter = new PhantomTilePresenter(clock, timer, terrain.pattern, location, loader, tileShapeMeshCache, terrain.elevationStepHeight);
    //   phantomTilePresenters.Add(location, presenter);
    // }

    // public void SetHighlightedLocations(SortedSet<Location> locations) {
    //   var (addedLocations, removedLocations) = Geomancer.Model.SetUtils.Diff(highlightedLocations, locations);
    //   highlightedLocations = locations;
    //   foreach (var addedLocation in addedLocations) {
    //     UpdateLocationHighlighted(addedLocation);
    //   }
    //   foreach (var removedLocation in removedLocations) {
    //     UpdateLocationHighlighted(removedLocation);
    //   }
    // }
  }
}
