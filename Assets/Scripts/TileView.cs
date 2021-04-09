using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Geomancer;
using Geomancer.Model;
using UnityEngine;
using Virtence.VText;

namespace Domino {
  
  public class TileDescription {
    public readonly float elevationStepHeight;
    public readonly float tileRotationDegrees;
    public readonly int depth; // basically elevation
    public readonly IVector4Animation topColor;
    public readonly IVector4Animation sideColor;
    public readonly ExtrudedSymbolDescription maybeOverlaySymbolDescription;
    public readonly ExtrudedSymbolDescription maybeFeatureSymbolDescription;
    public readonly List<(ulong, ExtrudedSymbolDescription)> itemSymbolDescriptionByItemId;

    public TileDescription(
        float elevationStepHeight,
        // float elevationStepHeight,
        float tileRotationDegrees,
        int depth,
        IVector4Animation topColor,
        IVector4Animation sideColor,
        ExtrudedSymbolDescription maybeOverlaySymbolDescription,
        ExtrudedSymbolDescription maybeFeatureSymbolDescription,
        List<(ulong, ExtrudedSymbolDescription)> itemSymbolDescriptionByItemId) {
      this.elevationStepHeight = elevationStepHeight;
      this.tileRotationDegrees = tileRotationDegrees;
      this.depth = depth;
      this.topColor = topColor;
      this.sideColor = sideColor;
      this.maybeOverlaySymbolDescription = maybeOverlaySymbolDescription;
      this.maybeFeatureSymbolDescription = maybeFeatureSymbolDescription;
      this.itemSymbolDescriptionByItemId = itemSymbolDescriptionByItemId;
    }

    // public TileDescription WithTileSymbolDescription(ExtrudedSymbolDescription newTileSymbolDescription) {
    //   return new TileDescription(
    //     elevationStepHeight,
    //     tileRotationDegrees,
    //     depth,
    //     newTopColor,
    //     newTopColor,
    //     maybeOverlaySymbolDescription,
    //     maybeFeatureSymbolDescription,
    //     itemSymbolDescriptionByItemId);
    // }

    public override bool Equals(object obj) {
      if (!(obj is TileDescription))
        return false;
      TileDescription that = obj as TileDescription;
      if (elevationStepHeight != that.elevationStepHeight)
        return false;
      if (tileRotationDegrees != that.tileRotationDegrees)
        return false;
      if (depth != that.depth)
        return false;
      if (!topColor.Equals(that.topColor))
        return false;
      if (!sideColor.Equals(that.sideColor))
        return false;
      if ((maybeOverlaySymbolDescription != null) != (that.maybeOverlaySymbolDescription != null))
        return false;
      if (maybeOverlaySymbolDescription != null && !maybeOverlaySymbolDescription.Equals(that.maybeOverlaySymbolDescription))
        return false;
      if ((maybeFeatureSymbolDescription != null) != (that.maybeFeatureSymbolDescription != null))
        return false;
      if (maybeFeatureSymbolDescription != null && !maybeFeatureSymbolDescription.Equals(that.maybeFeatureSymbolDescription))
        return false;
      if (itemSymbolDescriptionByItemId.Count != that.itemSymbolDescriptionByItemId.Count)
        return false;
      for (int i = 0; i < itemSymbolDescriptionByItemId.Count; i++) {
        if (itemSymbolDescriptionByItemId[i].Item1 != that.itemSymbolDescriptionByItemId[i].Item1)
          return false;
        if (!itemSymbolDescriptionByItemId[i].Item2.Equals(that.itemSymbolDescriptionByItemId[i].Item2))
          return false;
      }
      return true;
    }
    public override int GetHashCode() {
      int hashCode = 0;
      hashCode += 27 * elevationStepHeight.GetHashCode();
      hashCode += 31 * tileRotationDegrees.GetHashCode();
      hashCode += 37 * depth.GetHashCode();
      hashCode += 41 * topColor.GetHashCode();
      hashCode += 43 * sideColor.GetHashCode();
      if (maybeOverlaySymbolDescription != null)
        hashCode += 47 * maybeOverlaySymbolDescription.GetHashCode();
      if (maybeFeatureSymbolDescription != null)
        hashCode += 53 * maybeFeatureSymbolDescription.GetHashCode();
      hashCode += 67 * itemSymbolDescriptionByItemId.Count;
      foreach (var entry in itemSymbolDescriptionByItemId) {
        hashCode += 87 * entry.Item1.GetHashCode() + 93 * entry.Item2.GetHashCode();
      }
      return hashCode;
    }
  }

  public class TileView : MonoBehaviour {
    private bool initialized = false;
    public bool alive {  get { return initialized;  } }

    public ulong tileViewId { get; private set; }
    public Location location { get; private set; }
    public int elevation { get; private set; }
    
    private IClock clock;
    private ITimer timer;

    private Pattern pattern;
    private float elevationStepHeight;
    private IVector4Animation surfaceColor;
    private IVector4Animation cliffColor;
    private ExtrudedSymbolDescription maybeFeature;
    private ExtrudedSymbolDescription maybeOverlay;
    private SymbolId tileSymbolId;
    private float tileRotationDegrees;

    private SymbolView overlaySymbolView;
    private SymbolView featureSymbolView;
    private List<(ulong, SymbolView)> itemSymbolViewByItemId = new List<(ulong, SymbolView)>();

    // private Task<Mesh> meshTask;
    
    // private float tileScale;
    // private OutlineMode tileOutlineMode;
    // private IVector4Animation tileOutlineColor;

    // We have timers active to destroy these when theyre done, but we might
    // also destroy them if we need to Destruct fast.
    private List<KeyValuePair<SymbolView, int>> transientPrismSymbolsAndTimerIds;

    public List<GameObject> groundGameObjects;
    public List<GameObject> outlineGameObjects;

    private Mesh groundMesh;
    private Mesh outlinesMesh;
    private ILoader loader;
    
    public static TileView Create(
        ulong tileViewId,
        Location location,
        Pattern pattern,
        ILoader loader,
        Mesh groundMesh,
        Mesh outlinesMesh,
        IClock clock,
        ITimer timer,
        int elevation,
        TileDescription initialDescription) {
      var gameObject = loader.NewEmptyGameObject();
      var tileView = gameObject.AddComponent<TileView>();
      
      tileView.Init(tileViewId, location, pattern, loader, clock, timer, elevation, initialDescription, groundMesh, outlinesMesh);
      return tileView;
      // return (facesObject, outlinesObject);
    }

    public void Init(
        ulong tileViewId,
        Location location,
        Pattern pattern,
        ILoader loader,
        IClock clock,
        ITimer timer,
        int elevation,
        TileDescription initialDescription,
        Mesh groundObject,
        Mesh outlinesObject) {
      this.tileViewId = tileViewId;
      this.location = location;
      this.pattern = pattern;
      this.loader = loader;
      this.groundGameObjects = new List<GameObject>();
      this.outlineGameObjects = new List<GameObject>();
      this.groundMesh = groundObject;
      this.outlinesMesh = outlinesObject;
      this.elevation = elevation;
      this.clock = clock;
      this.timer = timer;

      transientPrismSymbolsAndTimerIds = new List<KeyValuePair<SymbolView, int>>();

      initialized = true;

      // tileSymbolId = initialDescription.tileSymbolDescription.symbol.symbolId;
      tileRotationDegrees = initialDescription.tileRotationDegrees;//tileSymbolDescription.symbol.rotationDegrees;
      elevationStepHeight = initialDescription.elevationStepHeight;
      // tileScale = initialDescription.tileSymbolDescription.symbol.scale;
      // tileOutlineMode = initialDescription.tileSymbolDescription.symbol.isOutlined;
      // tileOutlineColor = initialDescription.tileSymbolDescription.symbol.outlineColor;
      SetSurfaceColor(initialDescription.topColor);
      SetCliffColor(initialDescription.sideColor);
      // This is when the tile views are actually made
      SetDepth(initialDescription.depth);
      SetOverlay(initialDescription.maybeOverlaySymbolDescription);
      SetFeature(initialDescription.maybeFeatureSymbolDescription);
      foreach (var (itemId, itemDescription) in initialDescription.itemSymbolDescriptionByItemId) {
        AddItem(itemId, itemDescription);
      }

      RefreshPosition();
    }

    public void DestroyTile() {
      initialized = false;

      foreach (var transientRuneAndTimerId in transientPrismSymbolsAndTimerIds) {
        var rune = transientRuneAndTimerId.Key;
        var timerId = transientRuneAndTimerId.Value;
        timer.CancelTimer(timerId);
        rune.Destruct();
      }

      Destroy(this.gameObject);
    }

    private void SetTileOrPrismTransform(SymbolView tileSymbolView, float elevationStepHeight, float rotationDegrees, int elevation, int height) {
      // No idea why we need the -90 or the - before the rotation. It has to do with
      // unity's infuriating mishandling of .obj file imports.
      tileSymbolView.gameObject.transform.localRotation =
          Quaternion.Euler(new Vector3(-90, -rotationDegrees, 0));
      tileSymbolView.gameObject.transform.localScale =
          new Vector3(1, -1, elevationStepHeight * height);
      tileSymbolView.gameObject.transform.localPosition =
          new Vector3(0, elevationStepHeight * elevation);
    }

    public void AddItem(ulong id, ExtrudedSymbolDescription symbolDescription) {
      foreach (var x in itemSymbolViewByItemId) {
        if (x.Item1 == id) {
          Asserts.Assert(false, "Item ID " + id + " already exists!");
        }
      }
      var itemSymbolView = SymbolView.Create(clock, loader, true, symbolDescription);
      itemSymbolViewByItemId.Add((id, itemSymbolView));
      UpdateItemPositions();
    }

    public void ClearItems() {
      foreach (var x in itemSymbolViewByItemId) {
        x.Item2.Destruct();
      }
      itemSymbolViewByItemId.Clear();
    }

    public void RemoveItem(ulong id) {
      for (int i = 0; i < itemSymbolViewByItemId.Count; i++) {
        if (itemSymbolViewByItemId[i].Item1 == id) {
          itemSymbolViewByItemId[i].Item2.Destruct();
          itemSymbolViewByItemId.RemoveAt(i);
          UpdateItemPositions();
          return;
        }
      }
      Asserts.Assert(false, "Item ID " + id + " doesnt exist!");
    }

    private void UpdateItemPositions() {
      float[] radiansForIndex = {
          0 * (float)Math.PI / 180,
          120 * (float)Math.PI / 180,
          240 * (float)Math.PI / 180,
          60 * (float)Math.PI / 180,
          180 * (float)Math.PI / 180,
          300 * (float)Math.PI / 180,
          30 * (float)Math.PI / 180,
          150 * (float)Math.PI / 180,
          270 * (float)Math.PI / 180,
          90 * (float)Math.PI / 180,
          210 * (float)Math.PI / 180,
          330 * (float)Math.PI / 180,
          // one can calculate the angles past this, but its probably noise at this point
      };
      
      for (int itemIndex = 0; itemIndex < itemSymbolViewByItemId.Count; itemIndex++) {
        var itemId = itemSymbolViewByItemId[itemIndex].Item1;
        var itemSymbolView = itemSymbolViewByItemId[itemIndex].Item2;

        float inscribeCircleRadius = 0.75f; // chosen cuz it looks about right
                                            // https://math.stackexchange.com/questions/666491/three-circles-within-a-larger-circle
        float itemRadius = (-3 + 2 * 1.732f) * inscribeCircleRadius;

        float itemCenterXFromTileCenter = 0;
        float itemCenterYFromTileCenter = 0;

        if (itemSymbolViewByItemId.Count == 1) {
          itemCenterXFromTileCenter = 0;
          itemCenterYFromTileCenter = 0;
        } else if (itemSymbolViewByItemId.Count == 2) {
          if (itemIndex == 0) {
            itemCenterXFromTileCenter = -itemRadius / 2;
            itemCenterYFromTileCenter = 0;
          } else {
            itemCenterXFromTileCenter = itemRadius / 2;
            itemCenterYFromTileCenter = 0;
          }
        } else {
          // 0.866 is cos(30)
          // I don't know why we need that / 2 there.
          float itemCenterDistanceToTileCenter = itemRadius / 0.866f / 2;
          itemCenterXFromTileCenter = itemCenterDistanceToTileCenter * (float)Math.Cos(radiansForIndex[itemIndex % radiansForIndex.Length]);
          itemCenterYFromTileCenter = itemCenterDistanceToTileCenter * (float)Math.Sin(radiansForIndex[itemIndex % radiansForIndex.Length]);
          // TODO: adjust upward if the unit is on the tile
          itemCenterYFromTileCenter += 0;
        }

        itemSymbolView.gameObject.transform.localPosition =
            new Vector3(
                itemCenterXFromTileCenter,
                .05f,
                itemCenterYFromTileCenter);
        itemSymbolView.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 0f, 0));
        itemSymbolView.gameObject.transform.localScale =
          new Vector3(
              itemRadius,
              itemRadius,
              .1f);

        itemSymbolView.gameObject.transform.SetParent(transform, false);
      }
    }
    
    private static Vector3 CalculatePosition(float elevationStepHeight, Pattern pattern, Location location, int elevation) {
      var positionVec2 = pattern.GetTileCenter(location);
      var positionVec3 = new Vec3(positionVec2.x, positionVec2.y, 0);
      var unityPos = positionVec3.ToUnity();
      unityPos.y += elevation * elevationStepHeight;
      return unityPos;
    }

    public void SetSurfaceColor(IVector4Animation frontColor) {
      this.surfaceColor = frontColor;
      RefreshSurfaceColor();
    }

    private void RefreshSurfaceColor() {
      var animator = Vec4Animator.MakeOrGetFrom(
          clock, gameObject, new Vector4(0, 0, 0, 1), (vec4) => {
            foreach (var groundGameObject in groundGameObjects) {
              var props = new MaterialPropertyBlock();
              props.SetColor("_Color", new Color(vec4.x, vec4.y, vec4.z, vec4.w));
              groundGameObject.GetComponent<MeshRenderer>().SetPropertyBlock(props);
            }
          });
      animator.Set(surfaceColor, RenderPriority.TILE);
    }

    public void SetCliffColor(IVector4Animation sideColor) {
      this.cliffColor = sideColor;
      // foreach (var tsv in tileSymbolViews) {
      //   tsv.SetSidesColor(sideColor);
      // }
    }

    public void SetElevation(int elevation) {
      this.elevation = elevation;
      SetDepth(elevation);
      RefreshPosition();
    }

    private void RefreshPosition() {
      gameObject.transform.localPosition =
          CalculatePosition(elevationStepHeight, pattern, location, elevation);
    }

    private static void MaybeSetMesh(GameObject gameObject, Mesh mesh) {
      // Check if its been destroyed
      if (gameObject != null) {
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
      }
    }
    
    public void SetFeature(ExtrudedSymbolDescription maybeFeature) {
      if (this.maybeFeature != null) {
        featureSymbolView.Destruct();
      }
      this.maybeFeature = maybeFeature;
      if (this.maybeFeature != null) {
        float lift = 0.04f;
        float scale = 0.7f;
        float forward = 0.2f;
        var symbolView = SymbolView.Create(clock, loader, false, maybeFeature);
        symbolView.gameObject.transform.SetParent(gameObject.transform, false);
        symbolView.gameObject.transform.localPosition = new Vector3(-scale/2, lift, -forward);
        symbolView.gameObject.transform.localRotation = Quaternion.AngleAxis(40, Vector3.right);
        symbolView.gameObject.transform.localScale = new Vector3(scale, scale, 0.05f);
        featureSymbolView = symbolView;
      }
    }

    public void SetOverlay(ExtrudedSymbolDescription maybeOverlay) {
      if (this.maybeOverlay != null) {
        overlaySymbolView.Destruct();
      }
      this.maybeOverlay = maybeOverlay;
      if (this.maybeOverlay != null) {
        float lift = 0.01f;
        float scale = 0.707f;
        var overlayView = SymbolView.Create(clock, loader, false, maybeOverlay);
        overlayView.gameObject.transform.SetParent(transform, false);
        overlayView.gameObject.transform.localPosition = new Vector3(-scale/2, lift, -scale/2);
        overlayView.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        overlayView.gameObject.transform.localScale = new Vector3(scale, scale, 1);
        overlaySymbolView = overlayView;
      }
    }

    public void SetDepth(int depth) {
      while (groundGameObjects.Count > depth) {
        var groundGameObject = groundGameObjects[groundGameObjects.Count - 1];
        groundGameObjects.RemoveAt(groundGameObjects.Count - 1);
        Destroy(groundGameObject);
        var outlinesGameObject = outlineGameObjects[outlineGameObjects.Count - 1];
        outlineGameObjects.RemoveAt(outlineGameObjects.Count - 1);
        Destroy(outlinesGameObject);
      }
      while (groundGameObjects.Count < depth) {
        var newIndex = groundGameObjects.Count;
        
        var rotation = Quaternion.AngleAxis(-tileRotationDegrees, Vector3.up);
        var translate = new Vector3(0, -newIndex * elevationStepHeight, 0);

        var groundObject = loader.NewQuad();
        groundObject.GetComponent<MeshRenderer>().sharedMaterial = loader.white;
        groundObject.GetComponent<MeshFilter>().sharedMesh = groundMesh;
        groundObject.GetComponent<MeshCollider>().sharedMesh = groundMesh;
        groundObject.transform.SetParent(gameObject.transform, false);
        groundObject.transform.localPosition = translate;
        groundObject.transform.localRotation = rotation;
        groundGameObjects.Add(groundObject);
        
        var outlinesObject = loader.NewQuad();
        outlinesObject.GetComponent<MeshRenderer>().sharedMaterial = loader.black;
        outlinesObject.GetComponent<MeshFilter>().sharedMesh = outlinesMesh;
        outlinesObject.GetComponent<MeshCollider>().sharedMesh = groundMesh;
        outlinesObject.transform.SetParent(gameObject.transform, false);
        outlinesObject.transform.localPosition = translate;
        outlinesObject.transform.localRotation = rotation;
        outlineGameObjects.Add(outlinesObject);
      }
      RefreshSurfaceColor();
    }


    public void ShowRune(ExtrudedSymbolDescription runeSymbolDescription) {
      Asserts.Assert(false);
      // var symbolView = instantiator.CreateSymbolView(clock, true, runeSymbolDescription);
      // symbolView.transform.localRotation = Quaternion.Euler(new Vector3(-50, 180, 0));
      // symbolView.transform.localScale = new Vector3(1, 1, .1f);
      // symbolView.transform.localPosition = new Vector3(0, 0.5f, -.2f);
      // symbolView.transform.SetParent(gameObject.transform, false);
      // symbolView.FadeInThenOut(100, 400);
      // timer.ScheduleTimer(1000, () => {
      //   if (alive) {
      //     symbolView.Destruct();
      //   }
      // });
    }

    public void FadeInThenOut(long inDurationMs, long outDurationMs) {
      Asserts.Assert(false);
      // List<SymbolView> allSymbolViews = new List<SymbolView>();
      // allSymbolViews.AddRange(tileSymbolViews);
      // allSymbolViews.Add(overlaySymbolView);
      // allSymbolViews.Add(featureSymbolView);
      // foreach (var thing in itemSymbolViewByItemId) {
      //   allSymbolViews.Add(thing.Item2);
      // }
      // foreach (var symbol in allSymbolViews) {
      //   symbol.FadeInThenOut(inDurationMs, outDurationMs);
      // }
    }

    public long ShowPrism(
      ExtrudedSymbolDescription prismDescription,
      ExtrudedSymbolDescription prismOverlayDescription) {

      Asserts.Assert(false);
      // var prismGameObject = instantiator.CreateEmptyGameObject();
      // prismGameObject.transform.SetParent(gameObject.transform, false);
      // // We want to rotate the overall prism object because we want the
      // // overlay symbol to be aligned with the camera but want the polygon
      // // symbol to be aligned with the terrain tile.
      // // However, we do animate the scale of this object.
      // var scaleAnimator = ScaleAnimator.MakeOrGetFrom(clock, prismGameObject);
      // var yScaleAnimation =
      //   new AddFloatAnimation(
      //     new ConstantFloatAnimation(.95f),
      //     new MultiplyFloatAnimation(
      //       new ConstantFloatAnimation(.05f),
      //       FloatAnimations.InThenOut(clock.GetTimeMs(), 100, 400, 1, 1, 0)));
      // var scaleAnimation = new Vector3Animation(new ConstantFloatAnimation(.9f), yScaleAnimation, new ConstantFloatAnimation(.9f));
      // scaleAnimator.Set(scaleAnimation);
      //
      // var polygonView =
      //   instantiator.CreateSymbolView(
      //     clock,
      //     false,
      //     prismDescription);
      // SetTileOrPrismTransform(polygonView, elevationStepHeight, tileRotationDegrees, 0, 3);
      // polygonView.transform.SetParent(prismGameObject.transform, false);
      // polygonView.FadeInThenOut(100, 400);
      // ScheduleSymbolViewDestruction(polygonView);
      //
      // var overlayView =
      //   instantiator.CreateSymbolView(
      //     clock,
      //     false,
      //     prismOverlayDescription);
      //
      // float overlayThickness = .35f * elevationStepHeight;
      // // No idea why we need the -90. It has to do with
      // // unity's infuriating mishandling of .obj file imports.
      // overlayView.gameObject.transform.localRotation =
      //     Quaternion.Euler(new Vector3(-90, 0, 0));
      // overlayView.gameObject.transform.localScale =
      //     new Vector3(1 * .8f, -1 * .8f, overlayThickness);
      // overlayView.gameObject.transform.localPosition =
      //     new Vector3(0, elevationStepHeight * 3f + overlayThickness);
      //
      // overlayView.transform.SetParent(prismGameObject.transform, false);
      // overlayView.FadeInThenOut(100, 400);
      // ScheduleSymbolViewDestruction(overlayView);

      return clock.GetTimeMs() + 500;
    }

    private void ScheduleSymbolViewDestruction(SymbolView symbolView) {
      int prismOverlayTimerId =
        timer.ScheduleTimer(1000, () => {
          for (int i = 0; i < transientPrismSymbolsAndTimerIds.Count; i++) {
            if (transientPrismSymbolsAndTimerIds[i].Key == symbolView) {
              transientPrismSymbolsAndTimerIds.RemoveAt(i);
              symbolView.Destruct();
              return;
            }
          }
          Asserts.Assert(false, "Couldnt find!");
        });
      transientPrismSymbolsAndTimerIds.Add(new KeyValuePair<SymbolView, int>(symbolView, prismOverlayTimerId));
    }

    // public void Start() {
    //   if (!initialized) {
    //     throw new Exception("TileView component not initialized!");
    //   }
    // }

    // public void Update() {
    //   if (meshTask != null) {
    //     if (meshTask.IsCompleted) {
    //       var blark = loader.NewQuad();
    //       float lift = 0;
    //       float scale = 0.7f;
    //       Debug.Log("Assigning shared mesh! " + meshTask.Result.triangles.Length);
    //       blark.GetComponent<MeshFilter>().sharedMesh = meshTask.Result;
    //       Asserts.Assert(blark.GetComponent<MeshFilter>().sharedMesh != null);
    //       blark.transform.SetParent(gameObject.transform, false);
    //       blark.transform.localPosition = new Vector3(-.5f * scale, lift, -0.001f);
    //       blark.transform.localRotation = Quaternion.AngleAxis(40, Vector3.right);
    //       blark.transform.localScale = new Vector3(scale, scale, 1);
    //       meshTask = null;
    //     }
    //   }
    // }
  }
}