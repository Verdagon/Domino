using System.Collections.Generic;
using UnityEngine;
using Virtence.VText;

namespace Domino {
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
    Mesh GetSymbolOutlineMesh(SymbolId symbolId, int outlineThicknessPercent);
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

  public class FontCacheItem {
    public readonly VTextGlyphBuilder glyphBuilder;
    public readonly Dictionary<int, Dictionary<int, Mesh>> unicodeToThickness1000ToOutlineMesh;

    public FontCacheItem(VTextGlyphBuilder glyphBuilder) {
      this.glyphBuilder = glyphBuilder;
      this.unicodeToThickness1000ToOutlineMesh = new Dictionary<int, Dictionary<int, Mesh>>();
    }
  }
  
  public class Loader : MonoBehaviour, ILoader {
    public delegate void IReady();

    public event IReady onReady;

    private Dictionary<FontParameters, FontCacheItem> fontCache;

    public static Loader Create(GameObject gameObject) {
      var loader = gameObject.AddComponent<Loader>();
      loader.Init();
      return loader;
    }

    private void Init() {
      white = Instantiate(Resources.Load("White")) as Material;
      white.color = Color.white;
      white.enableInstancing = true;

      glowWhite = Instantiate(Resources.Load("White")) as Material;
      glowWhite.color = Color.white;
      glowWhite.enableInstancing = true;
      glowWhite.EnableKeyword("_EMISSION");
      glowWhite.SetColor("_EmissionColor", new Vector4(1, 1, 1, 1));

      black = Instantiate(Resources.Load("White")) as Material;
      black.color = Color.black;
      white.enableInstancing = true;

      fontCache = new Dictionary<FontParameters, FontCacheItem>();
      var fontNamesToLoad = new[] {"AthSymbols"};
      foreach (var fontName in fontNamesToLoad) {
        foreach (var expanded in new[] {false, true}) {
          var fontFilename = fontName + (expanded ? "Expanded.ttf" : ".ttf");
          VTextFontHash.FetchFont(
              fontFilename, (font) => {
                font.GlyphMeshAttributesHash = new Dictionary<char, MeshAttributes>();
                foreach (var extruded in new[] {false, true}) {
                  var param = new VTextMeshParameter {Depth = extruded ? 1 : 0, FontName = fontFilename};
                  var glyphBuilder = new VTextGlyphBuilder(param, font);
                  fontCache.Add(new FontParameters(fontName, expanded, extruded), new FontCacheItem(glyphBuilder));
                  if (fontCache.Count == fontNamesToLoad.Length * 4) {
                    onReady?.Invoke();
                  }
                }
              });
        }
      }
    }

    public Mesh GetSymbolOutlineMesh(SymbolId symbolId, int outlineThicknessPercent) {
      char c = char.ConvertFromUtf32(symbolId.unicode)[0];
      var fontParam = new FontParameters(symbolId.fontName, false, false);
      if (fontCache.TryGetValue(fontParam, out var fontCacheItem)) {
        
        if (!fontCacheItem.unicodeToThickness1000ToOutlineMesh.ContainsKey(symbolId.unicode)) {
          fontCacheItem.unicodeToThickness1000ToOutlineMesh.Add(symbolId.unicode, new Dictionary<int, Mesh>());
        }
        var thickness1000ToOutlineMesh = fontCacheItem.unicodeToThickness1000ToOutlineMesh[symbolId.unicode];
        if (!thickness1000ToOutlineMesh.ContainsKey(outlineThicknessPercent)) {
          var contours = fontCacheItem.glyphBuilder.GetContours((char) symbolId.unicode);
          var outlinesMeshBuilder = new MeshBuilder();
          foreach (var contour in contours) {
            OutlineMesh.BuildFrontFaceOutlines(
                outlinesMeshBuilder, new Vector3(0, 0, 1), contour.VertexList, outlineThicknessPercent / 100f);
          }
          var outlinesMesh = outlinesMeshBuilder.Build();
          thickness1000ToOutlineMesh.Add(outlineThicknessPercent, outlinesMesh);
        }
        return thickness1000ToOutlineMesh[outlineThicknessPercent];
      } else {
        Asserts.Assert(false, "Font not loaded: " + symbolId.fontName);
        return null;
      }
    }

    public Mesh getSymbolMesh(MeshParameters parameters) {
      string s = char.ConvertFromUtf32(parameters.symbolId.unicode);
      char c = s[0];
      var fontParam = new FontParameters(parameters.symbolId.fontName, parameters.expanded, parameters.extruded);
      if (fontCache.TryGetValue(fontParam, out var fontCacheItem)) {
        var mesh = fontCacheItem.glyphBuilder.GetMesh(c, 1.0f);
        // Combine the submeshes into one submesh
        mesh.SetTriangles(mesh.triangles, 0);
        mesh.subMeshCount = 1;
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
      var loaded = Resources.Load("Fonts/" + name);
      if (loaded == null) {
        Debug.LogError("Couldn't load " + name);
        return null;
      }
      return loaded as Font;
    }
  }
}