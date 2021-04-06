using System;
using System.Collections;
using System.Collections.Generic;
using Domino;
using UnityEngine;

namespace Domino {
  public enum DominoShape {
    SMALL_SQUARE = 1,
    TALL_DOMINO = 2,
  }
  
  public class DominoDescription {
    public readonly bool large;
    public readonly IVector4Animation color;
    public DominoDescription(
        bool large,
        IVector4Animation color) {
      this.large = large;
      this.color = color;
    }

    public override bool Equals(object obj) {
      if (!(obj is DominoDescription)) {
        return false;
      }
      DominoDescription that = obj as DominoDescription;
      return large == that.large && color.Equals(that.color);
    }
    public override int GetHashCode() {
      return (large ? 137 : 0) + 1343 * color.GetHashCode();
    }
  }

  public class DominoView : MonoBehaviour {
    public static readonly int DOMINO_RENDER_QUEUE = 3002;

    private bool initialized = false;

    private IClock clock;

    // The main object that lives in world space. It has no rotation or scale,
    // just a translation to the center of the tile the unit is in.
    // public GameObject gameObject; (provided by unity)

    // Object with a transform for the mesh, for example for rotating it.
    // Lives inside this.gameObject.
    private GameObject innerObject;

    ILoader loader;

    bool large;
    IVector4Animation color;

    public static DominoView Create(
        IClock clock,
        ILoader loader,
        DominoDescription dominoDescription) {
      var obj = loader.NewEmptyGameObject();
      var dominoView = obj.AddComponent<DominoView>();
      dominoView.Init(clock, loader, dominoDescription);
      return dominoView;
    }
    
    void Init(
        IClock clock,
        ILoader loader,
        DominoDescription dominoDescription) {
      this.clock = clock;
      this.loader = loader;

      InnerSetLarge(dominoDescription.large);
      InnerSetColor(dominoDescription.color);
      innerObject.transform.SetParent(gameObject.transform, false);

      initialized = true;
    }

    // public void SetDescription(DominoDescription newDominoDescription) {
    //   large = newDominoDescription.large;
    //   color = newDominoDescription.color;
    // }

    private void InnerSetLarge(bool newLarge) {
      Destroy(innerObject);
      innerObject = loader.NewQuad();
      innerObject.GetComponent<MeshCollider>().enabled = false;
      // ColorChanger.AddTo(gameObject, new[] {innerObject}, new GameObject[0] { });
      // if (newLarge) {
      //   innerObject = loader.CreateLargeDomino();
      // } else {
      //   innerObject = loader.CreateSmallDomino();
      // }
      large = newLarge;
      if (newLarge) {
        innerObject.transform.localScale = new Vector3(1, GetHeight(), 1);
      } else {
        // innerObject = loader.CreateSmallDomino();
      }
      RefreshRotation();
      //InnerSetColor(color);
    }

    public float GetHeight() {
      return large ? 1.3f : 1;
    }

    public void RefreshRotation() {
      float dominoHeight = GetHeight();
      
      float leanDegrees = 50;
      gameObject.transform.localRotation = Quaternion.Euler(new Vector3(leanDegrees, 0, 0));
      float leanRadians = (float)(leanDegrees * Math.PI / 180);

      // Right now the domino is half in the ground, let's bring it up a bit.
      float lift = (dominoHeight / 2) * (float) Math.Cos(leanRadians);
      // The domino's center is above the center of the tile, let's push it back a tiny bit.
      float stepBack = (dominoHeight / 2) * (float) Math.Sin(leanRadians) / 2;
      gameObject.transform.localPosition = new Vector3(0, lift, stepBack);
    }

    private void InnerSetColor(IVector4Animation newColor) {
      // ColorAnimator.MakeOrGetFrom(clock, gameObject).Set(newColor, RenderPriority.DOMINO);
      color = newColor;
    }

    public void Start() {
      if (!initialized) {
        throw new System.Exception("SymbolView component not initialized!");
      }
    }

    public void Fade(long durationMs) {
      // var animator = ColorAnimator.MakeOrGetFrom(clock, innerObject);
      // animator.Set(
      //   FadeAnimator.Fade(animator.Get(), clock.GetTimeMs(), durationMs),
      //   RenderPriority.DOMINO);
    }
  }
}