using System;
using System.Collections;
using System.Collections.Generic;
using Geomancer;
using Geomancer.Model;
using UnityEngine;

namespace Domino {
  public class UnitDescription {
    public readonly DominoDescription dominoDescription;
    public readonly ExtrudedSymbolDescription faceSymbolDescription;
    public readonly List<(ulong, ExtrudedSymbolDescription)> detailSymbolDescriptionById;
    public readonly float hpRatio;
    public readonly float mpRatio;
  
    public UnitDescription(
        DominoDescription dominoDescription,
        ExtrudedSymbolDescription faceSymbolDescription,
        List<(ulong, ExtrudedSymbolDescription)> detailSymbolDescriptionById,
        float hpRatio,
        float mpRatio) {
      this.dominoDescription = dominoDescription;
      this.faceSymbolDescription = faceSymbolDescription;
      this.detailSymbolDescriptionById = detailSymbolDescriptionById;
      this.hpRatio = hpRatio;
      this.mpRatio = mpRatio;
    }
  
  
    public override bool Equals(object obj) {
      if (!(obj is UnitDescription))
        return false;
      UnitDescription that = obj as UnitDescription;
      if (!dominoDescription.Equals(that.dominoDescription))
        return false;
      if (!faceSymbolDescription.Equals(that.faceSymbolDescription))
        return false;
      if (detailSymbolDescriptionById.Count != that.detailSymbolDescriptionById.Count)
        return false;
      for (int i = 0; i < detailSymbolDescriptionById.Count; i++) {
        if (detailSymbolDescriptionById[i].Item1 != that.detailSymbolDescriptionById[i].Item1)
          return false;
        if (!detailSymbolDescriptionById[i].Item2.Equals(that.detailSymbolDescriptionById[i].Item2))
          return false;
      }
      if (hpRatio != that.hpRatio)
        return false;
      if (mpRatio != that.mpRatio)
        return false;
      return true;
    }
    public override int GetHashCode() {
      int hashCode = 0;
      hashCode += 17 * dominoDescription.GetHashCode();
      hashCode += 33 * faceSymbolDescription.GetHashCode();
      hashCode += 53 * detailSymbolDescriptionById.Count;
      foreach (var detailSymbolDescription in detailSymbolDescriptionById) {
        hashCode += 67 * (int)detailSymbolDescription.Item1 + 79 * detailSymbolDescription.Item2.GetHashCode();
      }
      hashCode += 87 * (int)(hpRatio * 100);
      hashCode += 103 * (int)(mpRatio * 100);
      return hashCode;
    }
  }

  public class UnitView : MonoBehaviour {
    private const int OUTLINE_THICKNESS = 3;
    
    private static readonly long HOP_DURATION_MS = 300;

    ILoader loader;
    IClock clock;
    ITimer timer;
    private Vector3 lookatOffsetToCamera;

    private bool initialized = false;
    private bool instanceAlive = false;

    UnitDescription description;

    // The main object that lives in world space. It has no rotation or scale,
    // just a translation to the center of the tile the unit is in.
    // public GameObject gameObject; (provided by unity)

    // Object for slightly translating the unit.
    // Used for e.g. lunging animations. Usually Vec3(0,0,0
    // Lives inside this.gameObject.
    private GameObject offsetter;

    // Inside the offsetter, this will lean the unit back to be more straight-on to the player.
    private GameObject leaner;
    
    // Inside the leaner, this offsets things up, in case the domino goes below the ground, like tall dominos.
    // This is so the things inside will be roughly around the top of the domino.
    private GameObject lifter;

    // Inside the lifter, contains the details bar and anything that's aligned to
    // the domino (details bar, symbol, everything else).
    private GameObject body;

    // The domino. Can either be the large or small one.
    // Lives inside this.body.
    private SymbolView dominoSymbolView;

    // The symbol on the domino's field.
    // Lives inside this.body.
    private SymbolView faceSymbolView;

    // An invisible object aligned with the top of the unit, which will contain
    // any detail symbols.
    // Lives inside this.body.
    // Nullable. Only non-null when there's details.
    private SymbolBarView symbolBarView;

    // An invisible object aligned with the top of the unit, which will contain
    // any detail symbols.
    // Lives inside this.body.
    // Nullable. Only non-null when there's details.
    private MeterView hpMeterView;

    // An invisible object aligned with the top of the unit, which will contain
    // any detail symbols.
    // Lives inside this.body.
    // Nullable. Only non-null when there's details.
    private MeterView mpMeterView;

    // These help calculate where the unit should end up.
    private Pattern pattern;
    private float elevationStepHeight;
    public Location location { get; private set; }
    private int elevation;

    // We have timers active to destroy these when theyre done, but we might
    // also destroy them if we need to Destruct fast.
    private List<KeyValuePair<SymbolView, int>> transientRunesAndTimerIds;

    public static UnitView Create(
        ILoader loader,
        IClock clock,
        ITimer timer,
        Pattern pattern,
        float elevationStepHeight,
        Location location,
        int elevation,
        UnitDescription unitDescription,
        Vector3 lookatOffsetToCamera) {
      var obj = loader.NewEmptyGameObject();
      var unitView = obj.AddComponent<UnitView>();
      unitView.Init(loader, clock, timer, pattern, elevationStepHeight, location, elevation, unitDescription, lookatOffsetToCamera);
      return unitView;
    }
    
    void Init(
        ILoader loader,
        IClock clock,
        ITimer timer,
        Pattern pattern,
        float elevationStepHeight,
        Location location,
        int elevation,
        UnitDescription unitDescription,
        Vector3 lookatOffsetToCamera) {
      this.clock = clock;
      this.loader = loader;
      this.timer = timer;
      this.pattern = pattern;
      this.elevationStepHeight = elevationStepHeight;
      this.location = location;
      Asserts.Assert(location != null);
      this.elevation = elevation;
      this.description = unitDescription;
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      transientRunesAndTimerIds = new List<KeyValuePair<SymbolView, int>>();

      // gameObject.transform.position = basePosition;

      offsetter = loader.NewEmptyGameObject();
      offsetter.transform.SetParent(gameObject.transform, false);

      leaner = loader.NewEmptyGameObject();
      leaner.transform.SetParent(offsetter.transform, false);
      float leanDegrees = 50;
      leaner.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(leanDegrees, 0, 0));
      leaner.gameObject.transform.localPosition = new Vector3(0, 0, -.2f);

      lifter = loader.NewEmptyGameObject();
      lifter.transform.SetParent(leaner.transform, false);

      body = loader.NewEmptyGameObject();
      body.transform.SetParent(lifter.transform, false);

      dominoSymbolView =
          SymbolView.Create(
              clock,
              loader,
              false,
              new ExtrudedSymbolDescription(
                  RenderPriority.DOMINO,
                  new SymbolDescription(
                      new SymbolId("AthSymbols", 0x007B),
                      new MultiplyVector4Animation(
                          Vector4Animation.RED,
                          .6f),
                      0,
                      1,
                      OutlineMode.CenteredOutline,
                      Vector4Animation.RED),
                  0,
                  Vector4Animation.BLUE));
      dominoSymbolView.gameObject.transform.SetParent(body.transform, false);

      float minY = dominoSymbolView.GetMinY();
      // // Right now the domino is half in the ground, let's bring it up a bit.
      float lift = -minY;
      lifter.gameObject.transform.localPosition = new Vector3(0, lift, 0);
      
      dominoSymbolView.gameObject.transform.localPosition = new Vector3(-.5f, 0, 0);
      
      
      
      RefreshPosition();
      RefreshRotation();

      faceSymbolView = SymbolView.Create(clock, loader, false, unitDescription.faceSymbolDescription);
      faceSymbolView.gameObject.transform.SetParent(body.transform, false);
      faceSymbolView.gameObject.transform.localScale = new Vector3(.8f, .8f, .8f);
      faceSymbolView.gameObject.transform.localPosition = new Vector3(-.4f, 0, -.001f);

      if (unitDescription.detailSymbolDescriptionById.Count != 0) {
        symbolBarView = MakeSymbolBarView(clock, loader, unitDescription.detailSymbolDescriptionById, unitDescription.dominoDescription.large);
        symbolBarView.gameObject.transform.SetParent(body.transform, false);
      }
      
      // bool showHpMeter = unitDescription.hpRatio < .999f;
      // if (showHpMeter) {
      //   hpMeterView = loader.CreateMeterView(clock, unitDescription.hpRatio, Vector4Animation.GREEN, Vector4Animation.RED);
      //   hpMeterView.gameObject.transform.FromMatrix(MakeMeterViewTransform(0));
      //   hpMeterView.transform.SetParent(body.transform, false);
      // }
      //
      // bool showMpMeter = unitDescription.mpRatio < .999f;
      // if (showMpMeter) {
      //   int position = (showHpMeter ? 1 : 0);
      //   mpMeterView = loader.CreateMeterView(clock, unitDescription.mpRatio, Vector4Animation.BLUE, Vector4Animation.WHITE);
      //   mpMeterView.gameObject.transform.FromMatrix(MakeMeterViewTransform(position));
      //   mpMeterView.transform.SetParent(body.transform, false);
      // }

      initialized = true;
      instanceAlive = true;
    }

    private void RefreshPosition() {
      gameObject.transform.localPosition = CalculatePosition(elevationStepHeight, pattern, location, elevation);
    }
    
    private static Vector3 CalculatePosition(float elevationStepHeight, Pattern pattern, Location location, int elevation) {
      var positionVec2 = pattern.GetTileCenter(location);
      var positionVec3 = new Vec3(positionVec2.x, positionVec2.y, 0);
      var unityPos = positionVec3.ToUnity();
      unityPos.y += elevation * elevationStepHeight;
      return unityPos;
    }

    // public void SetUnitViewActive(bool enabled) {
    //   gameObject.SetActive(false);
    // }

    // public void SetDescription(UnitDescription newUnitDescription) {
    //   this.description = newUnitDescription;
    //   dominoView.SetDescription(newUnitDescription.dominoDescription);
    //   Debug.LogWarning("impl set desc unitview");
    //   // faceSymbolView.SetDescription(newUnitDescription.faceSymbolDescription);
    //
    //   // if (symbolBarView == null) {
    //   //   if (newUnitDescription.detailSymbolDescriptionById.Count == 0) {
    //   //     // Dont do anything, its already gone
    //   //   } else {
    //   //     symbolBarView = MakeSymbolBarView(clock, loader, newUnitDescription.detailSymbolDescriptionById, newUnitDescription.dominoDescription.large);
    //   //     symbolBarView.gameObject.transform.SetParent(body.transform, false);
    //   //   }
    //   // } else {
    //   //   if (newUnitDescription.detailSymbolDescriptionById.Count == 0) {
    //   //     symbolBarView.Destruct();
    //   //     symbolBarView = null;
    //   //   } else {
    //   //     symbolBarView.SetDescriptions(newUnitDescription.detailSymbolDescriptionById);
    //   //   }
    //   // }
    //
    //   // bool shouldShowHpMeter = newUnitDescription.hpRatio < .999f;
    //   // bool didShowHpMeter = (hpMeterView != null);
    //   // bool showingHpMeterChanged = (shouldShowHpMeter != didShowHpMeter);
    //   // if (shouldShowHpMeter) {
    //   //   if (hpMeterView == null) {
    //   //     hpMeterView = loader.CreateMeterView(clock, newUnitDescription.hpRatio, Vector4Animation.GREEN, Vector4Animation.RED);
    //   //     hpMeterView.gameObject.transform.FromMatrix(MakeMeterViewTransform(0));
    //   //     hpMeterView.transform.SetParent(body.transform, false);
    //   //   } else {
    //   //     hpMeterView.ratio = newUnitDescription.hpRatio;
    //   //   }
    //   // } else {
    //   //   if (hpMeterView == null) {
    //   //     // Do nothing
    //   //   } else {
    //   //     hpMeterView.Destruct();
    //   //     hpMeterView = null;
    //   //   }
    //   // }
    //
    //   // bool shouldShowMpMeter = newUnitDescription.mpRatio < .999f;
    //   // int mpMeterPosition = (shouldShowHpMeter ? 1 : 0);
    //   // if (shouldShowMpMeter) {
    //   //   if (mpMeterView == null) {
    //   //     mpMeterView = loader.CreateMeterView(clock, newUnitDescription.mpRatio, Vector4Animation.BLUE, Vector4Animation.WHITE);
    //   //     mpMeterView.gameObject.transform.FromMatrix(MakeMeterViewTransform(mpMeterPosition));
    //   //     mpMeterView.transform.SetParent(body.transform, false);
    //   //   } else {
    //   //     mpMeterView.ratio = newUnitDescription.mpRatio;
    //   //   }
    //   // } else {
    //   //   if (mpMeterView == null) {
    //   //     // Do nothing
    //   //   } else {
    //   //     mpMeterView.Destruct();
    //   //     mpMeterView = null;
    //   //   }
    //   // }
    //   // if (shouldShowMpMeter && showingHpMeterChanged) {
    //   //   mpMeterView.gameObject.transform.FromMatrix(MakeMeterViewTransform(mpMeterPosition));
    //   // }
    // }

    private static Matrix4x4 MakeMeterViewTransform(int position) {
      MatrixBuilder hpMeterTransform = new MatrixBuilder(Matrix4x4.identity);
      hpMeterTransform.Scale(new Vector3(1, 1, .1f));
      hpMeterTransform.Translate(new Vector3(-.5f, 0, 0));
      hpMeterTransform.Rotate(Quaternion.AngleAxis(270, Vector3.right));
      hpMeterTransform.Translate(new Vector3(0, 0, -.1f));
      hpMeterTransform.Translate(new Vector3(0, .1f * position, 0));
      return hpMeterTransform.matrix;
    }

    private static SymbolBarView MakeSymbolBarView(
      IClock clock,
        ILoader loader,
        List<(ulong, ExtrudedSymbolDescription)> symbolsIdsAndDescriptions,
        bool large) {
      SymbolBarView symbolBarView =
          SymbolBarView.Create(clock, loader, symbolsIdsAndDescriptions);
      symbolBarView.transform.localPosition = new Vector3(0, 1f, -.1f);
      return symbolBarView;
    }

    public void Destruct() {
      instanceAlive = false;
      foreach (var transientRuneAndTimerId in transientRunesAndTimerIds) {
        var rune = transientRuneAndTimerId.Key;
        var timerId = transientRuneAndTimerId.Value;
        timer.CancelTimer(timerId);
        rune.Destruct();
      }
      Destroy(this.gameObject);
    }

    public void Start() {
      if (!initialized) {
        throw new Exception("UnitView component not initialized!");
      }
    }

    public void TeleportTo(Location location, int elevation) {
      this.location = location;
      this.elevation = elevation;
      // Vector3 oldBasePosition = basePosition;
      // basePosition = newBasePosition;
      RefreshPosition();
    }

    public long HopTo(Location newLocation, int newElevation) {
      Vector3 oldBasePosition = CalculatePosition(elevationStepHeight, pattern, location, elevation);
      this.location = newLocation;
      this.elevation = newElevation;
      Vector3 newBasePosition = CalculatePosition(elevationStepHeight, pattern, newLocation, newElevation);
      RefreshPosition();

      StartHopAnimation(HOP_DURATION_MS, newBasePosition - oldBasePosition, 0.5f);
      return clock.GetTimeMs() + HOP_DURATION_MS;
    }

    private MovementAnimator GetOrCreateMovementAnimator() {
      var animator = offsetter.GetComponent<MovementAnimator>();
      if (animator == null) {
        animator = offsetter.AddComponent<MovementAnimator>() as MovementAnimator;
        animator.Init(clock);
      }
      return animator;
    }

    private void StartHopAnimation(long durationMs, Vector3 offset, float height) {
      // We technically just moved the unit to the new position, but we need to compensate
      // for it here to make it look like it's still back there and slowly transitioning.
      var anim = GetOrCreateMovementAnimator();
      anim.transformAnimation =
          new ComposeMatrix4x4Animation(
              anim.transformAnimation,
              new ComposeMatrix4x4Animation(
                  new ConstantMatrix4x4Animation(Matrix4x4.Translate(-offset)),
                  new HopAnimation(
                      clock.GetTimeMs(), clock.GetTimeMs() + durationMs, offset, height)));
    }

    public long Lunge(Vector3 offset) {
      StartLungeAnimation(150, offset);
      return clock.GetTimeMs() + 150;
    }

    private void StartLungeAnimation(long durationMs, Vector3 offset) {
      var anim = GetOrCreateMovementAnimator();
      anim.transformAnimation =
          new ComposeMatrix4x4Animation(
              anim.transformAnimation,
              new LungeAnimation(clock.GetTimeMs(), clock.GetTimeMs() + durationMs, offset));
    }

    public long ShowRune(ExtrudedSymbolDescription runeSymbolDescription) {
      // var symbolView = loader.CreateSymbolView(clock, true, runeSymbolDescription);
      // symbolView.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
      // symbolView.transform.localScale = new Vector3(1, 1, .1f);
      // if (description.dominoDescription.large) {
      //   symbolView.transform.localPosition = new Vector3(0, 1f, -.2f);
      // } else {
      //   symbolView.transform.localPosition = new Vector3(0, 0.5f, -.2f);
      // }
      // symbolView.transform.SetParent(body.transform, false);
      // symbolView.FadeInThenOut(100, 400);
      // int timerId =
      //   timer.ScheduleTimer(1000, () => {
      //     for (int i = 0; i < transientRunesAndTimerIds.Count; i++) {
      //       if (transientRunesAndTimerIds[i].Key == symbolView) {
      //         transientRunesAndTimerIds.RemoveAt(i);
      //         symbolView.Destruct();
      //         return;
      //       }
      //     }
      //     Asserts.Assert(false, "Couldnt find!");
      //   });
      // transientRunesAndTimerIds.Add(new KeyValuePair<SymbolView, int>(symbolView, timerId));
      return clock.GetTimeMs() + 500;
    }


    public long Die() {
      StartFadeAnimation(500);
      return clock.GetTimeMs() + 500;
    }

    private void StartFadeAnimation(long durationMs) {
      // dominoView.Fade(durationMs);
      Asserts.Assert(false);

      faceSymbolView.Fade(durationMs);

      if (hpMeterView != null) {
        hpMeterView.Fade(durationMs);
      }

      var anim = GetOrCreateMovementAnimator();
      anim.transformAnimation =
          new ComposeMatrix4x4Animation(
              anim.transformAnimation,
              new ClampMatrix4x4Animation(clock.GetTimeMs(), clock.GetTimeMs() + durationMs,
                  new LinearMatrix4x4Animation(clock.GetTimeMs(), clock.GetTimeMs() + durationMs,
                      Matrix4x4.identity,
                      Matrix4x4.Translate(new Vector3(0, -.05f, 0)))));
    }

    public void SetCameraDirection(Vector3 lookatOffsetToCamera) {
      this.lookatOffsetToCamera = lookatOffsetToCamera;
      RefreshRotation();
      // unitView.SetCameraDirection(lookatOffsetToCamera);
    }

    void RefreshRotation() {
      var horizontalCameraDirection = lookatOffsetToCamera;//new Vector3(lookatOffsetToCamera.x, 0, lookatOffsetToCamera.z).normalized;
      horizontalCameraDirection.y = 0;
      horizontalCameraDirection = horizontalCameraDirection.normalized;
      var angles = Quaternion.LookRotation(horizontalCameraDirection, Vector3.up).eulerAngles;
      offsetter.transform.localRotation = Quaternion.Euler(angles);
      
      body.transform.localPosition = new Vector3(.1f, 0, 0);
      body.transform.localScale = new Vector3(.8f, .8f, .8f);
    }
  }
}
