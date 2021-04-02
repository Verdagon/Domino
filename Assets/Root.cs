using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AthPlayer;
using Domino;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using Virtence.VText;

public interface ILoader {
  Material white { get; }
  Material black { get; }
  Material glowWhite { get; }
  Mesh getSymbolMesh(MeshParameters symbolId);
  Font LoadFont(string name);
  GameObject NewEmptyUIObject();
  GameObject NewEmptyGameObject();
  GameObject NewEmptyGameObject(Vector3 position, Quaternion rotation);
  GameObject NewQuad();
}

public struct FontParameters {
  public readonly string fontName;
  public readonly bool expanded;
  public readonly bool extruded;

  public FontParameters(
      string fontName,
      bool expanded,
      bool extruded) {
    this.fontName = fontName;
    this.expanded = expanded;
    this.extruded = extruded;
  }

  public override int GetHashCode() {
    return fontName.GetHashCode() + (expanded ? 47 : 0) + (extruded ? 73 : 0);
  }
}

public struct MeshParameters {
  public readonly SymbolId symbolId;
  public readonly bool expanded;
  public readonly bool extruded;

  public MeshParameters(
      SymbolId symbolId,
      bool expanded,
      bool extruded) {
    this.symbolId = symbolId;
    this.expanded = expanded;
    this.extruded = extruded;
  }

  public override int GetHashCode() {
    return symbolId.GetHashCode() + (expanded ? 47 : 0) + (extruded ? 31 : 0);
  }
}

public class Root : MonoBehaviour, ILoader {
  private Camera camera;
  private Canvas canvas;

  private OverlayPaneler overlayPaneler;

  private CameraController cameraController;

  private SlowableTimerClock clock;

  private LookPanelView lookPanelView;
  private Location maybeLookedLocation;
    
  private TerrainPresenter terrainPresenter;

  private Dictionary<KeyCode, string> memberByKeyCode;

  private SortedSet<Location> selectedLocations = new SortedSet<Location>();

  private ListView membersView;

  // public so we can see it in the unity editor
  public bool finishedStartMethod = false;

  private Dictionary<FontParameters, VTextGlyphBuilder> fontParamToGlyphBuilder;
  // A cache for what we calculate from the glyph builders
  private Dictionary<MeshParameters, Mesh> meshParamToMesh;

  public Mesh getSymbolMesh(MeshParameters parameters) {
    // GameObject vtextGameObject = Instantiate(Resources.Load("VText")) as GameObject;
    // Asserts.Assert(vtextGameObject != null, "Couldn't instantiate VText!");
    // VText vtext = vtextGameObject.GetComponent<VText>();
    // vtext.MeshParameter.FontName = parameters.symbolId.fontName + (parameters.expanded ? "Expanded.ttf" : ".ttf");
    // vtext.SetText(char.ConvertFromUtf32(parameters.symbolId.unicode));
    // vtext.RenderParameter.Materials = new[] {black, black, black};
    // vtext.MeshParameter.Depth = parameters.extruded ? 1 : 0;
    // vtext.Rebuild();

    if (meshParamToMesh.TryGetValue(parameters, out var m)) {
      return m;
    }
    
    string s = char.ConvertFromUtf32(parameters.symbolId.unicode);
    char c = s[0];
    var fontParam = new FontParameters(parameters.symbolId.fontName, parameters.expanded, parameters.extruded);
    if (fontParamToGlyphBuilder.TryGetValue(fontParam, out var glyphBuilder)) {
      var mesh = glyphBuilder.GetMesh(c, 1.0f);
      // Combine the submeshes into one submesh
      mesh.SetTriangles(mesh.triangles, 0);
      mesh.subMeshCount = 1;
      meshParamToMesh.Add(parameters, mesh);
      return mesh;
    } else {
      Asserts.Assert(false, "Font not loaded: " + parameters.symbolId.fontName);
      return null;
    }
  }

  public Material white { get; private set; }
  public Material black { get; private set; }
  public Material glowWhite { get; private set; }
  public GameObject NewEmptyUIObject() {
    return Instantiate(Resources.Load("EmptyUIObject")) as GameObject;  
  }
  public GameObject NewEmptyGameObject() {
    return Instantiate(Resources.Load("EmptyGameObject")) as GameObject;  
  }
  public GameObject NewEmptyGameObject(Vector3 position, Quaternion rotation) {
    return Instantiate(Resources.Load("EmptyGameObject"), position, rotation) as GameObject;  
  }
  public GameObject NewQuad() {
    return Instantiate(Resources.Load("Quad")) as GameObject;  
  }
  public Font LoadFont(string name) {
    // var folderPath = Application.streamingAssetsPath + "/Fonts";  //Get path of folder
    // // filePaths = Directory.GetFiles(folderPath, "*.png"); // Get all files of type .png in this folder
    // var filePath = folderPath + "/" + name + ".ttf";
    // byte[] pngBytes = System.IO.File.ReadAllBytes(filePath);
    // var font = new Font();
    // font.
    var loaded = Resources.Load("Fonts/" + name);
    if (loaded == null) {
      Debug.LogError("Couldn't load " + name);
      return null;
    }
    return loaded as Font;
  }
  
  void Start() {
    meshParamToMesh = new Dictionary<MeshParameters, Mesh>();
    fontParamToGlyphBuilder = new Dictionary<FontParameters, VTextGlyphBuilder>();
    var fontNamesToLoad = new [] { "AthSymbols" };
    foreach (var fontName in fontNamesToLoad) {
      foreach (var expanded in new[]{ false, true }) {
        var fontFilename = fontName + (expanded ? "Expanded.ttf" : ".ttf");
        VTextFontHash.FetchFont(fontFilename, (font) => {
          font.GlyphMeshAttributesHash = new Dictionary<char, MeshAttributes>();
          foreach (var extruded in new[]{ false, true }) {
            var param = new VTextMeshParameter();
            param.Depth = extruded ? 1 : 0;
            param.FontName = fontFilename;
            var glyphBuilder = new VTextGlyphBuilder(param, font);
            fontParamToGlyphBuilder.Add(new FontParameters(fontName, expanded, extruded), glyphBuilder);
            if (fontParamToGlyphBuilder.Count == fontNamesToLoad.Length * 4) {
              doThings();
            }
          }
        });
      }
    }
  }
  
  void doThings() {
    camera = GetComponentInChildren<Camera>();
    canvas = GetComponentInChildren<Canvas>();

    // vtextParametersToMesh = new Dictionary<MeshParameters, SimpleFuture<GameObject>>();

    white = Instantiate(Resources.Load("White")) as Material;
    white.color = Color.white;
    white.enableInstancing = true;

    glowWhite = Instantiate(Resources.Load("White")) as Material;
    glowWhite.color = Color.white;
    glowWhite.enableInstancing = true;
    glowWhite.EnableKeyword("_EMISSION");
    glowWhite.SetColor("_EmissionColor", new Vector4(1,1,1,1));

    black = Instantiate(Resources.Load("White")) as Material;
    black.color = Color.black;
    white.enableInstancing = true;
    

    //
    // foreach (var groundGameObject in tileObjects) {
    //   groundGameObject.isStatic = true;
    // }
    // foreach (var outlineGameObject in outlineObjects) {
    //   outlineGameObject.isStatic = true;
    // }
    // StaticBatchingUtility.Combine(tileObjects.ToArray(), Instantiate(Resources.Load("EmptyGameObject")) as GameObject);
    // StaticBatchingUtility.Combine(outlineObjects.ToArray(), Instantiate(Resources.Load("EmptyGameObject")) as GameObject);
    
    
    
      clock = new SlowableTimerClock(1.0f);

      memberByKeyCode = new Dictionary<KeyCode, string>() {
        [KeyCode.B] = "Fire",
        [KeyCode.G] = "Grass",
        [KeyCode.M] = "Mud",
        [KeyCode.D] = "Dirt",
        [KeyCode.R] = "Rocks",
        [KeyCode.O] = "Obsidian",
        [KeyCode.S] = "DarkRocks",
        [KeyCode.X] = "Marker",
        [KeyCode.C] = "Cave",
        [KeyCode.F] = "Floor",
        [KeyCode.T] = "Tree",
        [KeyCode.L] = "Magma",
        [KeyCode.H] = "HealthPotion",
        [KeyCode.P] = "ManaPotion",
        [KeyCode.W] = "CaveWall",
        [KeyCode.Z] = "ObsidianFloor",
        [KeyCode.V] = "Avelisk",
        [KeyCode.Z] = "Zeddy",
        [KeyCode.Hash] = "Wall",
        [KeyCode.BackQuote] = "Water",
      };

      overlayPaneler = new OverlayPaneler(canvas.gameObject, this, clock);
      lookPanelView = new LookPanelView(overlayPaneler, -1, 2);

      //var pattern = SquarePattern.MakeSquarePattern();
      //var pattern = HexPattern.MakeHexPattern();

      var pattern = PentagonPattern9.makePentagon9Pattern();
      var terrain = new Geomancer.Model.Terrain(pattern, 200, new SortedDictionary<Location, TerrainTile>());

      using (var fileStream = new FileStream("level.athlev", FileMode.OpenOrCreate)) {
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

      var tileShapeMeshCache = new TileShapeMeshCache(pattern);
      terrainPresenter = new TerrainPresenter(clock, clock, MemberToViewMap.MakeVivimap(), terrain, this, tileShapeMeshCache);
      terrainPresenter.PhantomTileClicked += HandlePhantomTileClicked;
      terrainPresenter.TerrainTileClicked += HandleTerrainTileClicked;
      terrainPresenter.TerrainTileHovered += HandleTerrainTileHovered;

      Location startLocation = new Location(0, 0, 0);
      if (!terrain.tiles.ContainsKey(startLocation)) {
        foreach (var locationAndTile in terrain.tiles) {
          startLocation = locationAndTile.Key;
          break;
        }
      }

      cameraController =
        new CameraController(
          clock,
          camera,
          terrain.GetTileCenter(startLocation).ToUnity(),
          new Vector3(0, -10, 5));

      membersView =
        new ListView(
          overlayPaneler.MakePanel(
            0, 0, 40, 16));

      finishedStartMethod = true;
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

    public void Update() {
      if (!finishedStartMethod) {
        // There was probably an error in the logs that said why we're not loaded
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
      if (Input.GetKeyDown(KeyCode.Escape)) {
        SetSelection(new SortedSet<Location>());
      }
      if (Input.GetKeyDown(KeyCode.Slash)) {
        var allLocations = new SortedSet<Location>();
        foreach (var locationAndTile in terrainPresenter.terrain.tiles) {
          allLocations.Add(locationAndTile.Key);
        }
        SetSelection(allLocations);
      }
      if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Mouse2)) {
      
        foreach (var loc in selectedLocations) {
          terrainPresenter.GetTilePresenter(loc).SetElevation(terrainPresenter.terrain.tiles[loc].elevation + 1);
        }
        Save();
        UpdateLookPanelView();
      }
      if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore) || Input.GetKeyDown(KeyCode.Mouse1)) {
        foreach (var loc in selectedLocations) {
          terrainPresenter.GetTilePresenter(loc).SetElevation(
              Math.Max(1, terrainPresenter.terrain.tiles[loc].elevation - 1));
        }
        Save();
        UpdateLookPanelView();
      }
      if (Input.GetKeyDown(KeyCode.Delete)) {
        foreach (var loc in new SortedSet<Location>(selectedLocations)) {
          selectedLocations.Remove(loc);
          var tile = terrainPresenter.terrain.tiles[loc];
          terrainPresenter.terrain.tiles.Remove(loc);
          tile.Destruct();
        }
        Save();
        UpdateLookPanelView();
      }
      foreach (var keyCodeAndMember in memberByKeyCode) {
        if (Input.GetKeyDown(keyCodeAndMember.Key)) {
          bool addKeyDown = Input.GetKey(KeyCode.RightAlt);
          bool removeKeyDown = Input.GetKey(KeyCode.LeftAlt);
          ChangeMember(keyCodeAndMember.Value, addKeyDown, removeKeyDown);
          Save();
        }
        UpdateLookPanelView();
      }

      UnityEngine.Ray ray = camera.ScreenPointToRay(Input.mousePosition);
      terrainPresenter.UpdateMouse(ray);
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

    private void Save() {
      using (var fileStream = new FileStream("level.athlev", FileMode.Create)) {
        using (var writer = new StreamWriter(fileStream)) {
          Save(writer);
        }
      }

      var timestamp = (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
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
}
