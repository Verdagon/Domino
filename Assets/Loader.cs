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

  public class Loader : MonoBehaviour, ILoader {
    public delegate void IReady();

    public event IReady onReady;

    private Dictionary<FontParameters, VTextGlyphBuilder> fontParamToGlyphBuilder;

    // A cache for what we calculate from the glyph builders
    private Dictionary<MeshParameters, Mesh> meshParamToMesh;

    public static Loader 
        Create(GameObject gameObject) {
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

      meshParamToMesh = new Dictionary<MeshParameters, Mesh>();
      fontParamToGlyphBuilder = new Dictionary<FontParameters, VTextGlyphBuilder>();
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
                  fontParamToGlyphBuilder.Add(new FontParameters(fontName, expanded, extruded), glyphBuilder);
                  if (fontParamToGlyphBuilder.Count == fontNamesToLoad.Length * 4) {
                    onReady?.Invoke();
                  }
                }
              });
        }
      }
    }

    public Mesh getSymbolMesh(MeshParameters parameters) {
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
      var loaded = Resources.Load("Fonts/" + name);
      if (loaded == null) {
        Debug.LogError("Couldn't load " + name);
        return null;
      }
      return loaded as Font;
    }
  }
}