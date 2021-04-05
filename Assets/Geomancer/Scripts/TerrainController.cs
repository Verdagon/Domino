using System;
using System.Collections.Generic;
using Geomancer.Model;
using UnityEngine;
using Domino;

namespace Geomancer {
  public delegate void OnTerrainTileHovered(Location location);
  public delegate void OnTerrainTileClicked(Location location);
  public delegate void OnPhantomTileClicked(Location location);

  public class TerrainController {
    public OnTerrainTileHovered TerrainTileHovered;
    public OnTerrainTileClicked TerrainTileClicked;
    public OnPhantomTileClicked PhantomTileClicked;
    
    private GameToDominoConnection domino;

    // IClock clock;
    // ITimer timer;
    MemberToViewMapper vivimap;

    public readonly Geomancer.Model.Terrain terrain;

    // ILoader loader;
    // private TileShapeMeshCache tileShapeMeshCache;
    Dictionary<Location, TerrainTilePresenter> tilePresenters = new Dictionary<Location, TerrainTilePresenter>();
    Dictionary<Location, PhantomTilePresenter> phantomTilePresenters = new Dictionary<Location, PhantomTilePresenter>();

    Location maybeMouseHighlightedLocation = null;
    private SortedSet<Location> selectedLocations = new SortedSet<Location>();

    public TerrainController(
        GameToDominoConnection domino,
        MemberToViewMapper vivimap,
        Geomancer.Model.Terrain terrain) {
      this.domino = domino;
      // this.clock = clock;
      // this.timer = timer;
      this.vivimap = vivimap;
      this.terrain = terrain;
      // this.loader = loader;
      // this.tileShapeMeshCache = tileShapeMeshCache;

      foreach (var locationAndTile in terrain.tiles) {
        addTerrainTile(locationAndTile.Key, locationAndTile.Value);
      }

      RefreshPhantomTiles();
    }

    public void AddTile(TerrainTilePresenter presenter) {
      tilePresenters.Add(presenter.location, presenter);
    }

    public Location GetMaybeMouseHighlightLocation() {
      return maybeMouseHighlightedLocation;
    }

    public void DestroyTerrainController() {
      foreach (var entry in tilePresenters) {
        entry.Value.DestroyTerrainTilePresenter();
      }
    }

    public void UpdateLocationHighlighted(Location location) {
      bool highlighted = location == maybeMouseHighlightedLocation;
      bool selected = selectedLocations.Contains(location);
      if (location != null) {
        if (tilePresenters.TryGetValue(location, out var newMousedTerrainTilePresenter)) {
          newMousedTerrainTilePresenter.SetHighlighted(highlighted);
          newMousedTerrainTilePresenter.SetSelected(selected);
        }
        if (phantomTilePresenters.TryGetValue(location, out var newMousedPhantomTilePresenter)) {
          // Cant select a phantom tile
          newMousedPhantomTilePresenter.SetHighlighted(highlighted);
        }
      }
    }

    public void AddTile(TerrainTile tile) {
      if (phantomTilePresenters.TryGetValue(tile.location, out var presenter)) {
        presenter.DestroyPhantomTilePresenter();
        phantomTilePresenters.Remove(tile.location);
      }
      terrain.tiles.Add(tile.location, tile);
      addTerrainTile(tile.location, terrain.tiles[tile.location]);
      RefreshPhantomTiles();
    }

    public void RemoveTile(TerrainTile tile) {
      tilePresenters.Remove(tile.location);
      var newHighlightedLocations = new SortedSet<Location>(selectedLocations);
      newHighlightedLocations.Remove(tile.location);
      SetHighlightedLocations(newHighlightedLocations);
      RefreshPhantomTiles();
    }

    public TerrainTilePresenter GetTilePresenter(Location location) {
      if (tilePresenters.TryGetValue(location, out var presenter)) {
        return presenter;
      }
      return null;
    }

    private void RefreshPhantomTiles() {
      var phantomTileLocations =
          terrain.pattern.GetAdjacentLocations(new SortedSet<Location>(terrain.tiles.Keys), false, true);
      var previousPhantomTileLocations = phantomTilePresenters.Keys;

      var addedPhantomTileLocations = new SortedSet<Location>(phantomTileLocations);
      SetUtils.RemoveAll(addedPhantomTileLocations, previousPhantomTileLocations);

      var removedPhantomTileLocations = new SortedSet<Location>(previousPhantomTileLocations);
      SetUtils.RemoveAll(removedPhantomTileLocations, phantomTileLocations);

      foreach (var removedPhantomTileLocation in removedPhantomTileLocations) {
        removePhantomTile(removedPhantomTileLocation);
      }

      foreach (var addedPhantomTileLocation in addedPhantomTileLocations) {
        addPhantomTile(addedPhantomTileLocation);
      }
    }

    private void removePhantomTile(Location removedPhantomTileLocation) {
      phantomTilePresenters[removedPhantomTileLocation].DestroyPhantomTilePresenter();
      phantomTilePresenters.Remove(removedPhantomTileLocation);
    }

    private void addTerrainTile(Location location, TerrainTile tile) {
      var presenter = new TerrainTilePresenter(domino, vivimap, terrain, location, tile);
      tilePresenters.Add(location, presenter);
    }

    private void addPhantomTile(Location location) {
      var presenter = new PhantomTilePresenter(domino, terrain.pattern, location, terrain.elevationStepHeight);
      phantomTilePresenters.Add(location, presenter);
    }

    public void SetHighlightedLocations(SortedSet<Location> locations) {
      var (addedLocations, removedLocations) = Geomancer.Model.SetUtils.Diff(selectedLocations, locations);
      selectedLocations = locations;
      foreach (var addedLocation in addedLocations) {
        UpdateLocationHighlighted(addedLocation);
      }
      foreach (var removedLocation in removedLocations) {
        UpdateLocationHighlighted(removedLocation);
      }
    }

    public void SetHoveredLocation(Location newMaybeMouseHighlightedLocation) {
      if (newMaybeMouseHighlightedLocation != maybeMouseHighlightedLocation) {
        var oldMaybeMouseHighlightedLocation = maybeMouseHighlightedLocation;
        maybeMouseHighlightedLocation = newMaybeMouseHighlightedLocation;
        UpdateLocationHighlighted(oldMaybeMouseHighlightedLocation);
        UpdateLocationHighlighted(newMaybeMouseHighlightedLocation);
      }
    }

    public void LocationMouseDown(ulong tileViewId, Location location) {
      if (location != null) {
        if (tilePresenters.TryGetValue(maybeMouseHighlightedLocation, out var newMousedTerrainTilePresenter)) {
          TerrainTileClicked?.Invoke(maybeMouseHighlightedLocation);
        }
        if (phantomTilePresenters.TryGetValue(maybeMouseHighlightedLocation, out var newMousedPhantomTilePresenter)) {
          PhantomTileClicked?.Invoke(maybeMouseHighlightedLocation);
        }
      }
    }
  }
}
