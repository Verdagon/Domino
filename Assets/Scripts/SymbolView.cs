using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Domino;
using UnityEngine;
using Virtence.VText;
using static Domino.Instantiator;

namespace Domino {
  public enum OutlineMode {
    NoOutline = 0,
    WithOutline = 1,
    WithBackOutline = 2
  }

  public class SymbolDescription {
    public readonly SymbolId symbolId;
    public readonly IVector4Animation frontColor;
    public readonly float rotationDegrees;
    public readonly float scale;
    public readonly OutlineMode isOutlined;
    public readonly IVector4Animation outlineColor;

    public SymbolDescription(
        SymbolId symbolId,
        IVector4Animation frontColor,
        float rotationDegrees,
        float scale,
        OutlineMode isOutlined)
      : this(symbolId, frontColor, rotationDegrees, scale, isOutlined, Vector4Animation.BLACK) { }

    public SymbolDescription(
        SymbolId symbolId,
        IVector4Animation frontColor,
        float rotationDegrees,
        float scale,
        OutlineMode isOutlined,
        IVector4Animation outlineColor) {
      this.symbolId = symbolId;
      this.frontColor = frontColor;
      this.rotationDegrees = rotationDegrees;
      this.scale = scale;
      this.isOutlined = isOutlined;
      this.outlineColor = outlineColor;

      Asserts.Assert(outlineColor != null);
    }

    public SymbolDescription WithFrontColor(IVector4Animation newFrontColor) {
      return new SymbolDescription(
        symbolId,
        newFrontColor,
        rotationDegrees,
        scale,
        isOutlined);
    }
  }

  public class ExtrudedSymbolDescription {
    public readonly RenderPriority renderPriority;
    public readonly SymbolDescription symbol;
    public readonly bool extruded;
    public readonly IVector4Animation sidesColor;

    public ExtrudedSymbolDescription(
        RenderPriority renderPriority,
        SymbolDescription symbol,
        bool extruded,
        IVector4Animation sidesColor) {
      this.renderPriority = renderPriority;
      this.symbol = symbol;
      this.extruded = extruded;
      this.sidesColor = sidesColor;
    }

    public ExtrudedSymbolDescription WithSymbol(SymbolDescription newSymbol) {
      return new ExtrudedSymbolDescription(renderPriority, newSymbol, extruded, sidesColor);
    }
    public ExtrudedSymbolDescription WithSidesColor(IVector4Animation newSidesColor) {
      return new ExtrudedSymbolDescription(renderPriority, symbol, extruded, newSidesColor);
    }
  }

  public class SymbolView : MonoBehaviour {
    public bool instanceAlive { get; private set; }

    private IClock clock;
    private ITimer timer;

    // The main object that lives in world space. It has no rotation or scale,
    // just a translation to the center of the tile the unit is in.
    // public GameObject gameObject; (provided by unity)

    // Object with a transform for the mesh, for example for rotating it.
    // Lives inside this.gameObject.
    // Specified by unity.
    public VText frontObject;
    public VText outlineObject;

    Instantiator instantiator;

    bool isOutlined;
    RenderPriority renderPriority;
    SymbolId symbolId;
    bool isExtruded;
    IVector4Animation frontColor;
    IVector4Animation sidesColor;
    float rotationDegrees;
    float scale;
    IVector4Animation outlineColor;

    public void Init(
        IClock clock,
        Instantiator instantiator,
        // If true, z=0 will be the front of the symbol.
        // If false, z=0 will be the back of the symbol (only really makes sense for extruded symbols).
        bool originFront,
        ExtrudedSymbolDescription symbolDescription) {
      this.clock = clock;
      this.instantiator = instantiator;

      InnerSetSymbolId(symbolDescription.symbol.symbolId, false);
      InnerSetOutline(symbolDescription.symbol.isOutlined != OutlineMode.NoOutline, symbolDescription.symbol.outlineColor, false);
      InnerSetExtruded(symbolDescription.extruded, false);
      Rebuild();

      this.renderPriority = symbolDescription.renderPriority;
      frontObject.transform.SetParent(gameObject.transform, false);
      outlineObject.transform.SetParent(gameObject.transform, false);
      
      if (!originFront) {
        frontObject.transform.localPosition = new Vector3(0, 0, 1);
        outlineObject.transform.localPosition = new Vector3(0, 0, 1);
      }

      SetFrontColor(symbolDescription.symbol.frontColor);
      SetSidesColor(symbolDescription.sidesColor);
      InnerSetScale(symbolDescription.symbol.scale);
      instanceAlive = true;
    }
    
    public void Destruct() {
      CheckInstanceAlive();
      Destroy(gameObject);
      instanceAlive = false;
    }

    public void CheckInstanceAlive() {
      if (!instanceAlive) {
        throw new System.Exception("SymbolView component not initialized!");
      }
    }

    private void InnerSetSymbolId(SymbolId newSymbolId, bool rebuild) {
      frontObject.MeshParameter.FontName = symbolId.fontName + ".simplified.ttf";
      frontObject.SetText(char.ConvertFromUtf32(symbolId.unicode));
      if (isOutlined) {
        outlineObject.MeshParameter.FontName = symbolId.fontName + ".expanded.ttf";
        outlineObject.SetText(char.ConvertFromUtf32(symbolId.unicode));
      }
      symbolId = newSymbolId;
      if (rebuild) {
        Rebuild();
      }
    }

    private void Rebuild() {
      frontObject.Rebuild();
      if (isOutlined) {
        outlineObject.Rebuild();
      }
    }

    private void InnerSetExtruded(bool isExtruded, bool rebuild) {
      this.isExtruded = isExtruded;
      RefreshDepth(false);
      if (rebuild) {
        Rebuild();
      }
    }

    private void RefreshDepth(bool rebuild) {
      frontObject.MeshParameter.Depth = (!isOutlined && isExtruded) ? 1 : 0;
      outlineObject.MeshParameter.Depth = (isOutlined && isExtruded) ? 1 : 0;
      if (rebuild) {
        Rebuild();
      }
    }

    private void InnerSetOutline(bool isOutlined, IVector4Animation newOutlineColor, bool rebuild) {
      this.isOutlined = isOutlined;
      outlineObject.enabled = isOutlined;
      RefreshDepth(false);
      if (rebuild) {
        Rebuild();
      }
      ColorAnimator.MakeOrGetFrom(clock, outlineObject.gameObject).Set(newOutlineColor, renderPriority);
      outlineColor = newOutlineColor;
    }

    public void SetFrontColor(IVector4Animation newColor) {
      ColorAnimator.MakeOrGetFrom(clock, frontObject.gameObject).Set(newColor, renderPriority);
      
      frontColor = newColor;
    }

    public void SetSidesColor(IVector4Animation newColor) {
      // ColorAnimator.MakeOrGetFrom(clock, sidesObject).Set(newColor, renderPriority);
      sidesColor = newColor;
      Debug.LogWarning("impl side color");
    }

    private void InnerSetRotationDegrees(float newRotationDegrees) {
      transform.rotation = Quaternion.AngleAxis(rotationDegrees, Vector3.forward);
      rotationDegrees = newRotationDegrees;
    }

    private void InnerSetScale(float newScale) {
      frontObject.transform.localScale = new Vector3(newScale, newScale, newScale);
      outlineObject.transform.localScale = new Vector3(newScale, newScale, newScale);
      if (newScale > 9) {
        Debug.Log("yeah over 9");
      }
      scale = newScale;
    }

    private static Vector3[] GetMinAndMax(Mesh mesh) {
      Vector3 min = mesh.vertices[0];
      Vector3 max = mesh.vertices[0];
      foreach (var vertex in mesh.vertices) {
        min.x = Math.Min(min.x, vertex.x);
        min.y = Math.Min(min.y, vertex.y);
        min.z = Math.Min(min.z, vertex.z);
        max.x = Math.Max(max.x, vertex.x);
        max.y = Math.Max(max.y, vertex.y);
        max.z = Math.Max(max.z, vertex.z);
      }
      return new Vector3[] { min, max };
    }

    private static Matrix4x4 CalculateSymbolTransform(bool front) {
      MatrixBuilder transform = new MatrixBuilder(Matrix4x4.identity);

      //if (!front) {
      //  transform.Translate(new Vector3(0, 0, 1));
      //}

      // When we generated the .obj models, they had varying X and Y coordinates, and the
      // Z was flat. We imagined them as lying on the ground.

      // Our models were made in right-hand space. Unity tries to "help" by multiplying
      // all X coordinates by -1. This flipped all our models horizontally.

      // In the end, this means that they're standing upright, with their X flipped.

      // We undo the horizontal flip here.
      //transform.Scale(new Vector3(-1, 1, 1));

      // They're still standing upright, remember.

      // Our models have their normals pointing +Z, and have the sides going -Z, because
      // we assume the camera is high in the +Z.
      // In Unity, the camera is low in the -Z, so rotate to face it.
      //transform.Rotate(Quaternion.AngleAxis(180, Vector3.up));

      // One would think that we'd have to flip things horizontally since we're looking
      // at it from the other side, but we don't since Unity flips all .obj vertices'
      // X coordinates by -1 when it imports.


      //// Somehow, the above shifted the Z by a lot. We shift it back.
      //transform.Translate(new Vector3(0, 0, 1));

      // Now, it's centered inside 0,0 1,1.
      return transform.matrix;
    }

    public void Start() {
      CheckInstanceAlive();
    }

    public void FadeInThenOut(long inDurationMs, long outDurationMs) {
      var frontAnimator = ColorAnimator.MakeOrGetFrom(clock, frontObject.gameObject);
      frontAnimator.Set(
          FadeAnimator.FadeInThenOut(frontAnimator.Get(), clock.GetTimeMs(), inDurationMs, outDurationMs),
          renderPriority);
      var frontOutlineAnimator = ColorAnimator.MakeOrGetFrom(clock, outlineObject.gameObject);
      frontOutlineAnimator.Set(
          FadeAnimator.FadeInThenOut(frontOutlineAnimator.Get(), clock.GetTimeMs(), inDurationMs, outDurationMs),
          renderPriority);
    }

    public void Fade(long durationMs) {
      var frontAnimator = ColorAnimator.MakeOrGetFrom(clock, frontObject.gameObject);
      frontAnimator.Set(
        FadeAnimator.Fade(frontAnimator.Get(), clock.GetTimeMs(), durationMs),
        renderPriority);
      var frontOutlineAnimator = ColorAnimator.MakeOrGetFrom(clock, outlineObject.gameObject);
      frontOutlineAnimator.Set(
        FadeAnimator.Fade(frontOutlineAnimator.Get(), clock.GetTimeMs(), durationMs),
        renderPriority);
    }
  }
}
