using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using AthPlayer;

namespace Domino {
  public class CameraController {
    private IClock clock;
    private Camera cameraObject;
    // Where it's supposed to be, after all the animations are done.
    private Vector3 targetCameraEndLookAtPosition;
    public Vector3 targetPolarLookatOffsetToCamera { get; private set; }

    public Vector3 targetLookatOffsetToCamera => PolarToCartesian(targetPolarLookatOffsetToCamera);

    private readonly static float cameraSpeedPerSecond = 8.0f;

    public CameraController(IClock clock, Camera cameraObject, Vector3 initialLookAtPosition, Vector3 initialLookatOffsetToCamera) {
      this.clock = clock;
      this.cameraObject = cameraObject;

      targetCameraEndLookAtPosition = initialLookAtPosition;
      targetPolarLookatOffsetToCamera = CartesianToPolar(initialLookatOffsetToCamera);

      GetOrCreateCameraAnimator().lookAtAnimation = new ConstantVector3Animation(targetCameraEndLookAtPosition);
      GetOrCreateCameraAnimator().polarLookatOffsetToCameraAnimation = new ConstantVector3Animation(targetPolarLookatOffsetToCamera);
    }

    public static Vector3 CartesianToPolar(Vector3 cartesian) {
      // Notice the flip here from z to y, because unity is left-handed
      double x = cartesian.x, y = cartesian.z, z = cartesian.y;
      double r = Math.Sqrt(x * x + y * y + z * z);
      double lng = Math.Acos(x / Math.Sqrt(x * x + y * y)) * (y < 0 ? -1 : 1);
      double lat = Math.Acos(z / r);
      return new Vector3((float)r, (float)lat, (float)lng);
    }

    public static Vector3 PolarToCartesian(Vector3 cartesian) {
      // Notice the flip here from z to y, because unity is left-handed
      double r = cartesian.x, lat = cartesian.y, lng = cartesian.z;
      double x = r * Math.Sin(lat) * Math.Cos(lng);
      double y = r * Math.Sin(lat) * Math.Sin(lng);
      double z = r * Math.Cos(lat);
      // Notice the flip here from z to y, because unity is left-handed
      return new Vector3((float)x, (float)z, (float)y);
    }

    public Vector3 GetCurrentLookatOffsetToCamera() {
      var animator = cameraObject.GetComponent<CameraAnimator>();
      if (animator == null) {
        return PolarToCartesian(targetPolarLookatOffsetToCamera);
      } else {
        return PolarToCartesian(animator.polarLookatOffsetToCameraAnimation.Get(clock.GetTimeMs()));
      }
    }
    
    private CameraAnimator GetOrCreateCameraAnimator() {
      // var animator =
      //     Vec3Animator.MakeOrGetFrom(
      //         clock,
      //         cameraObject.gameObject,
      //         lookatOffsetToCamera,
      //         (vec) => {
      //         });
      
      var animator = cameraObject.GetComponent<CameraAnimator>();
      if (animator == null) {
        animator = cameraObject.gameObject.AddComponent<CameraAnimator>();
        animator.Init(
          clock,
          cameraObject.gameObject,
          new IdentityVector3Animation(),
          new IdentityVector3Animation());
      }
      Asserts.Assert(animator != null);
      return animator;
    }

    public void StartRotatingCameraTo(Vector3 newOffsetToLookAt, long durationMs) {
      Vector3 newPolarOffsetToLookAt = CartesianToPolar(newOffsetToLookAt);
      var animator = GetOrCreateCameraAnimator();
      if (durationMs == 0) {
        animator.polarLookatOffsetToCameraAnimation = new ConstantVector3Animation(newPolarOffsetToLookAt);
      } else {
        var currentPolarLookatOffsetToCamera = targetPolarLookatOffsetToCamera;
        
        var polarOffsetToLookAtDifference = newPolarOffsetToLookAt - currentPolarLookatOffsetToCamera;
        animator.polarLookatOffsetToCameraAnimation =
            new AddVector3Animation(
                animator.polarLookatOffsetToCameraAnimation,
                new ClampVector3Animation(
                    clock.GetTimeMs(), clock.GetTimeMs() + durationMs,
                    new AddVector3Animation(
                        new ConstantVector3Animation(polarOffsetToLookAtDifference),
                        new LinearVector3Animation(
                            clock.GetTimeMs(), clock.GetTimeMs() + durationMs, -polarOffsetToLookAtDifference, new Vector3(0, 0, 0)))));
      }
      targetPolarLookatOffsetToCamera = newPolarOffsetToLookAt;
    }

    public void StartMovingCameraTo(Vector3 newCameraEndLookAtPosition, long durationMs) {
      var animator = GetOrCreateCameraAnimator();
      if (durationMs == 0) {
        animator.lookAtAnimation =
          new ConstantVector3Animation(newCameraEndLookAtPosition);
      } else {
        var currentCameraEndLookAtPosition = targetCameraEndLookAtPosition;
        var cameraDifference = newCameraEndLookAtPosition - currentCameraEndLookAtPosition;
        animator.lookAtAnimation =
            new AddVector3Animation(
                animator.lookAtAnimation,
                new ClampVector3Animation(
                    clock.GetTimeMs(), clock.GetTimeMs() + durationMs,
                    new AddVector3Animation(
                        new ConstantVector3Animation(cameraDifference),
                        new LinearVector3Animation(
                            clock.GetTimeMs(), clock.GetTimeMs() + durationMs, -cameraDifference, new Vector3(0, 0, 0)))));
      }
      targetCameraEndLookAtPosition = newCameraEndLookAtPosition;
    }
    
    public void MoveIn(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition +
                cameraObject.transform.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveOut(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition -
                cameraObject.transform.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveUp(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition +
                Vector3.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveDown(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition -
              Vector3.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveLeft(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition -
              Vector3.right * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveRight(float deltaTime) {
      var newEndLookAtPosition =
          targetCameraEndLookAtPosition +
              Vector3.right * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }
  }
}
