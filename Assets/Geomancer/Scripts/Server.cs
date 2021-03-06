using System;
using System.Collections.Generic;
using System.IO;
using Domino;
using Geomancer.Model;

namespace Geomancer {
  class MemberStuff {
    public Dictionary<char, string> memberByKeyCode = new Dictionary<char, string>() {
      ['b'] = "Fire",
      ['g'] = "Grass",
      ['m'] = "Mud",
      ['d'] = "Dirt",
      ['r'] = "Rocks",
      ['o'] = "Obsidian",
      ['s'] = "DarkRocks",
      ['x'] = "Marker",
      ['c'] = "Cave",
      ['f'] = "Floor",
      ['t'] = "Tree",
      ['l'] = "Magma",
      ['h'] = "HealthPotion",
      ['p'] = "ManaPotion",
      ['w'] = "CaveWall",
      ['z'] = "ObsidianFloor",
      ['v'] = "Avelisk",
      ['z'] = "Zeddy",
      ['#'] = "Wall",
      ['`'] = "Water",
    };
  }
  
  public class EditorServer {
    GameToDominoConnection domino;

    private const int elevationStepHeight = 200;
    private int screenGW = 0;
    private int screenGH = 0;
    private Terrain terrain;
    private LookPanelView lookPanelView;
    private Dictionary<char, string> memberByKeyCode;
    TerrainController terrainPresenter;
    private SortedSet<Location> selectedLocations = new SortedSet<Location>();
    private ListView membersView;
    private Location maybeHoveredLocation;
    private Location maybeLookedLocation;
    private MemberToViewMapper vivimap;

    public EditorServer(GameToDominoConnection domino) {
      this.domino = domino;
    }

    public void Start(int screenGW, int screenGH) {
      this.screenGW = screenGW;
      this.screenGH = screenGH;
      
      var pattern = PentagonPattern9.makePentagon9Pattern();

      terrain = new Geomancer.Model.Terrain(pattern, 200, new SortedDictionary<Location, TerrainTile>());

      using (var fileStream = new FileStream("level.lev", FileMode.OpenOrCreate)) {
        using (var reader = new StreamReader(fileStream)) {
          while (true) {
            string line = reader.ReadLine();
            if (line == null) {
              break;
            }
            if (line == "") {
              continue;
            }
            string[] parts = line.Split(' ');
            int groupX = int.Parse(parts[0]);
            int groupY = int.Parse(parts[1]);
            int indexInGroup = int.Parse(parts[2]);
            int elevation = int.Parse(parts[3]);

            var location = new Location(groupX, groupY, indexInGroup);
            var tile = new TerrainTile(location, elevation, new List<string>());
            terrain.tiles.Add(location, tile);

            for (int i = 4; i < parts.Length; i++) {
              tile.members.Add(parts[i]);
            }
            // if (!tile.members.Contains("Tree")) {
            //   tile.members.Add("Tree");
            // }
          }
        }
      }
      
      if (terrain.tiles.Count == 0) {
        var tile = new TerrainTile(new Location(0, 0, 0), 1, new List<string>());
        terrain.tiles.Add(new Location(0, 0, 0), tile);
      }

      Location startLocation = new Location(0, 0, 0);
      if (!terrain.tiles.ContainsKey(startLocation)) {
        foreach (var locationAndTile in terrain.tiles) {
          startLocation = locationAndTile.Key;
          break;
        }
      }
      
      var cameraLookAtPosition = terrain.GetTileCenter(startLocation);
      
      domino.SetupGame(cameraLookAtPosition, elevationStepHeight * ModelExtensions.ModelToUnityMultiplier, pattern);

      membersView = new ListView(domino, domino.MakePanel(0, 0, 40, 16), 40, 16);
      lookPanelView = new LookPanelView(domino, screenGW, -1, 2);

      vivimap = MemberToViewMapper.LoadMap("vivimap.txt");
      terrainPresenter = new TerrainController(domino, vivimap, terrain);
    }

    public void SetHoveredLocation(ulong tileViewId, Location newMaybeHoveredLocation) {
      HandleTerrainTileHovered(newMaybeHoveredLocation);
      terrainPresenter.SetHoveredLocation(newMaybeHoveredLocation);
    }
    
    //
    // public void LocationMouseOut(ulong tileViewId, Location location) {
    //   HandleTerrainTileHovered(null);
    // }
    
    public void LocationMouseDown(ulong tileViewId, Location location) {
      
    }

    public void KeyDown(int c, bool leftShiftDown, bool rightShiftDown, bool ctrlDown, bool leftAltDown, bool rightAltDown) {
      switch (c) {
        case '\u005c':
          SetSelection(new SortedSet<Location>());
          break;
        case '/':
          var allLocations = new SortedSet<Location>();
          foreach (var locationAndTile in terrainPresenter.terrain.tiles) {
            allLocations.Add(locationAndTile.Key);
          }
          SetSelection(allLocations);
          break;
        case '=':
        case '+':
        case -2:
          foreach (var loc in selectedLocations) {
            terrainPresenter.GetTilePresenter(loc).SetElevation(terrainPresenter.terrain.tiles[loc].elevation + 1);
          }
          Save();
          UpdateLookPanelView();
          break;
        case '-':
        case '_':
        case -1:
          foreach (var loc in selectedLocations) {
            terrainPresenter.GetTilePresenter(loc).SetElevation(
                Math.Max(1, terrainPresenter.terrain.tiles[loc].elevation - 1));
          }
          Save();
          UpdateLookPanelView();
          break;
        case '\u007F':
          foreach (var loc in new SortedSet<Location>(selectedLocations)) {
            selectedLocations.Remove(loc);
            var tile = terrainPresenter.terrain.tiles[loc];
            terrainPresenter.terrain.tiles.Remove(loc);
            tile.Destruct();
          }
          Save();
          UpdateLookPanelView();
          break;
      }

      foreach (var keyCodeAndMember in memberByKeyCode) {
        if (c == keyCodeAndMember.Key) {
          bool addKeyDown = rightAltDown;
          bool removeKeyDown = leftAltDown;
          ChangeMember(keyCodeAndMember.Value, addKeyDown, removeKeyDown);
          Save();
        }
        UpdateLookPanelView();
      }
    }

    public void HandlePhantomTileClicked(Location location) {
      var terrainTile = new TerrainTile(location, 1, new List<string>());
      terrainPresenter.AddTile(terrainTile);
      Save();

      var newSelection = new SortedSet<Location>(selectedLocations);
      newSelection.Add(location);
      SetSelection(newSelection);
    }

    public void HandleTerrainTileClicked(Location location) {
      var newSelection = new SortedSet<Location>(selectedLocations);
      if (newSelection.Contains(location)) {
        newSelection.Remove(location);
      } else {
        newSelection.Add(location);
      }
      SetSelection(newSelection);
    }

    public void HandleTerrainTileHovered(Location location) {
      UpdateLookPanelView();
    }

    private void Save() {
      using (var fileStream = new FileStream("level.athlev", FileMode.Create)) {
        using (var writer = new StreamWriter(fileStream)) {
          Save(writer);
        }
      }

      var timestamp = (int) new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
      using (var fileStream = new FileStream("level" + timestamp + ".athlev", FileMode.Create)) {
        using (var writer = new StreamWriter(fileStream)) {
          Save(writer);
        }
      }
    }

    private void Save(StreamWriter writer) {
      foreach (var locAndTile in terrainPresenter.terrain.tiles) {
        var loc = locAndTile.Key;
        var tile = locAndTile.Value;
        string line = loc.groupX + " " + loc.groupY + " " + loc.indexInGroup + " " + tile.elevation;
        foreach (var member in tile.members) {
          line += " " + member;
        }
        writer.WriteLine(line);
      }
      writer.Close();
    }

    private void SetSelection(SortedSet<Location> locations) {
      selectedLocations = locations;
      terrainPresenter.SetHighlightedLocations(selectedLocations);

      SortedSet<string> commonMembers = null;
      foreach (var loc in selectedLocations) {
        if (commonMembers == null) {
          commonMembers = new SortedSet<string>();
          foreach (var member in terrainPresenter.terrain.tiles[loc].members) {
            commonMembers.Add(member);
          }
        } else {
          var members = new SortedSet<string>();
          foreach (var member in terrainPresenter.terrain.tiles[loc].members) {
            members.Add(member);
          }
          foreach (var member in new SortedSet<string>(commonMembers)) {
            if (!members.Contains(member)) {
              commonMembers.Remove(member);
            }
          }
        }
      }

      var entries = new List<ListView.Entry>();
      if (commonMembers != null) {
        foreach (var member in commonMembers) {
          entries.Add(new ListView.Entry(new SymbolId("AthSymbols", 0x0072), member));
        }
      }
      membersView.ShowEntries(entries);
    }
    
    private void UpdateLookPanelView() {
      var location = terrainPresenter.GetMaybeMouseHighlightLocation();
      if (location != maybeLookedLocation) {
        maybeLookedLocation = location;
        if (location == null) {
          lookPanelView.SetStuff(false, "", "", new List<KeyValuePair<SymbolDescription, string>>());
        } else {
          var message = "(" + location.groupX + ", " + location.groupY + ", " + location.indexInGroup + ")";

          var symbolsAndDescriptions = new List<KeyValuePair<SymbolDescription, string>>();
          if (terrainPresenter.terrain.tiles.ContainsKey(location)) {
            message += " elevation " + terrainPresenter.terrain.tiles[location].elevation;
            foreach (var member in terrainPresenter.terrain.tiles[location].members) {
              var symbol =
                  new SymbolDescription(
                      new SymbolId("AthSymbols", 0x0072),
                      Vector4Animation.Color(1f, 1f, 1f, 0), 180, 1, OutlineMode.WithOutline, Vector4Animation.Color(1, 1, 1));
              symbolsAndDescriptions.Add(new KeyValuePair<SymbolDescription, string>(symbol, member));
            }
          }

          lookPanelView.SetStuff(true, message, "", symbolsAndDescriptions);
        }
      }
    }
    
    private void ChangeMember(string member, bool addKeyDown, bool removeKeyDown) {
      if (addKeyDown && removeKeyDown) {
        return;
      } else if (addKeyDown) {
        // Add one to each tile
        foreach (var location in selectedLocations) {
          terrainPresenter.GetTilePresenter(location).AddMember(member);
        }
      } else if (removeKeyDown) {
        foreach (var location in selectedLocations) {
          if (LocationHasMember(location, member)) {
            terrainPresenter.GetTilePresenter(location).RemoveMember(member);
          }
        }
      } else {
        // Toggle; ensure it's there if its not
        if (!AllLocationsHaveMember(selectedLocations, member)) {
          foreach (var location in selectedLocations) {
            // Add it if its not already there
            if (!LocationHasMember(location, member)) {
              terrainPresenter.GetTilePresenter(location).AddMember(member);
            }
          }
        } else {
          foreach (var location in selectedLocations) {
            // Remove all of them that are present
            while (LocationHasMember(location, member)) {
              terrainPresenter.GetTilePresenter(location).RemoveMember(member);
            }
          }
        }
      }
      
    }

    private bool AllLocationsHaveMember(SortedSet<Location> locations, string member) {
      foreach (var location in locations) {
        if (!LocationHasMember(location, member)) {
          return false;
        }
      }
      return true;
    }

    private bool LocationHasMember(Location location, string member) {
      foreach (var hayMember in terrainPresenter.terrain.tiles[location].members) {
        if (member == hayMember) {
          return true;
        }
      }
      return false;
    }
  }
}
