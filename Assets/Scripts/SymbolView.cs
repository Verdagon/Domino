using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Domino;
using UnityEngine;
using Virtence.VText;
using static Domino.Instantiator;
using Random = System.Random;

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
    // public GameObject faceObject;
    // public GameObject sidesObject;
    private GameObject faceObject;
    private GameObject outlineObject;

    private ILoader loader;

    IVector4Animation frontColor;
    IVector4Animation sidesColor;
    IVector4Animation outlineColor;
    float rotationDegrees;
    float scale;
    private ExtrudedSymbolDescription symbolDescription;

    // private static void MaybeSetMesh(GameObject gameObject, Mesh mesh) {
    //   // Check if its been destroyed
    //   if (gameObject != null) {
    //     gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    //   }
    // }

    public static SymbolView Create(
        IClock clock,
        ILoader loader,
        ExtrudedSymbolDescription symbolDescription) {
      var symbolViewObject = loader.NewEmptyGameObject();
      var symbolView = symbolViewObject.AddComponent<SymbolView>();
      symbolView.Init(clock, loader, symbolDescription);
      return symbolView;
    }
    
    private void Init(
        IClock clock,
        ILoader loader,
        // // If true, z=0 will be the front of the symbol.
        // // If false, z=0 will be the back of the symbol (only really makes sense for extruded symbols).
        // bool originFront,
        ExtrudedSymbolDescription symbolDescription) {
      this.clock = clock;
      this.loader = loader;
      this.symbolDescription = symbolDescription;
      
      var frontExtruded = symbolDescription.extruded && !(symbolDescription.symbol.isOutlined != OutlineMode.NoOutline);
      faceObject = loader.NewQuad();
      faceObject.GetComponent<MeshFilter>().sharedMesh =
          loader.getSymbolMesh(new MeshParameters(symbolDescription.symbol.symbolId, false, frontExtruded));
      faceObject.transform.SetParent(gameObject.transform, false);
      faceObject.transform.localScale = new Vector3(1, 1, frontExtruded ? 1 : 0);
      
      if (symbolDescription.symbol.isOutlined != OutlineMode.NoOutline) {
        var outlineExtruded = symbolDescription.extruded && (symbolDescription.symbol.isOutlined != OutlineMode.NoOutline);
        outlineObject = loader.NewQuad();
        var mesh = loader.getSymbolMesh(new MeshParameters(symbolDescription.symbol.symbolId, true, outlineExtruded));
        outlineObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        outlineObject.GetComponent<MeshRenderer>().sharedMaterial = loader.black; 
        outlineObject.transform.SetParent(gameObject.transform, false);
        outlineObject.transform.localPosition = new Vector3(0, 0, 0.001f);
        outlineObject.transform.localScale = new Vector3(1, 1, 1); //outlineExtruded ? 1 : 0);
      }

      // this.renderPriority = symbolDescription.renderPriority;
      
      // if (!originFront) {
      //   frontObject.transform.localPosition = new Vector3(0, 0, 1);
      //   outlineObject.transform.localPosition = new Vector3(0, 0, 1);
      // }

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


    public void SetFrontColor(IVector4Animation newColor) {
      var animator = Vec4Animator.MakeOrGetFrom(
          clock, faceObject.gameObject, new Vector4(0, 0, 0, 1), (vec4) => {
            foreach (var meshRenderer in faceObject.GetComponentsInChildren<MeshRenderer>()) {
              var props = new MaterialPropertyBlock();
              props.SetColor("_Color", new Color(vec4.x, vec4.y, vec4.z, vec4.w));
              meshRenderer.SetPropertyBlock(props);
            }
          });
      animator.Set(newColor, symbolDescription.renderPriority);
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
      faceObject.transform.localScale = new Vector3(newScale, newScale, newScale);
      if (outlineObject != null) {
        outlineObject.transform.localScale = new Vector3(newScale, newScale, newScale);
      }
      scale = newScale;
    }

    public void Start() {
      CheckInstanceAlive();
    }

    public void FadeInThenOut(long inDurationMs, long outDurationMs) {
      Asserts.Assert(false);
      // var frontAnimator = ColorAnimator.MakeOrGetFrom(clock, frontObject.gameObject);
      // frontAnimator.Set(
      //     FadeAnimator.FadeInThenOut(frontAnimator.Get(), clock.GetTimeMs(), inDurationMs, outDurationMs),
      //     renderPriority);
      // var frontOutlineAnimator = ColorAnimator.MakeOrGetFrom(clock, outlineObject.gameObject);
      // frontOutlineAnimator.Set(
      //     FadeAnimator.FadeInThenOut(frontOutlineAnimator.Get(), clock.GetTimeMs(), inDurationMs, outDurationMs),
      //     renderPriority);
    }

    public void Fade(long durationMs) {
      Asserts.Assert(false);
      // var frontAnimator = ColorAnimator.MakeOrGetFrom(clock, frontObject.gameObject);
      // frontAnimator.Set(
      //   FadeAnimator.Fade(frontAnimator.Get(), clock.GetTimeMs(), durationMs),
      //   renderPriority);
      // var frontOutlineAnimator = ColorAnimator.MakeOrGetFrom(clock, outlineObject.gameObject);
      // frontOutlineAnimator.Set(
      //   FadeAnimator.Fade(frontOutlineAnimator.Get(), clock.GetTimeMs(), durationMs),
      //   renderPriority);
    }
  }
}
