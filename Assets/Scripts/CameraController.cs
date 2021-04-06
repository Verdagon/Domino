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
    private Vector3 cameraEndLookAtPosition;
    public Vector3 lookatOffsetToCamera { get; private set; }

    private readonly static float cameraSpeedPerSecond = 8.0f;

    public CameraController(IClock clock, Camera cameraObject, Vector3 initialLookAtPosition, Vector3 initiallookatOffsetToCamera) {
      this.clock = clock;
      this.cameraObject = cameraObject;

      cameraEndLookAtPosition = initialLookAtPosition;
      lookatOffsetToCamera = initiallookatOffsetToCamera;

      GetOrCreateCameraAnimator().lookAtAnimation = new ConstantVector3Animation(cameraEndLookAtPosition);
      GetOrCreateCameraAnimator().lookatOffsetToCameraAnimation = new ConstantVector3Animation(lookatOffsetToCamera);
    }

    public Vector3 GetCurrentLookatOffsetToCamera() {
      var animator = cameraObject.GetComponent<CameraAnimator>();
      if (animator == null) {
        return lookatOffsetToCamera;
      } else {
        return animator.lookatOffsetToCameraAnimation.Get(clock.GetTimeMs());
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

    public void StartRotatingCameraTo(Vector3 new_offsetToLookAt, long durationMs) {
      var animator = GetOrCreateCameraAnimator();
      if (durationMs == 0) {
        animator.lookAtAnimation =
          new ConstantVector3Animation(new_offsetToLookAt);
      } else {
        var currentlookatOffsetToCamera = lookatOffsetToCamera;
        var offsetToLookAtDifference = new_offsetToLookAt - currentlookatOffsetToCamera;
        animator.lookatOffsetToCameraAnimation =
            new AddVector3Animation(
                animator.lookatOffsetToCameraAnimation,
                new ClampVector3Animation(
                    clock.GetTimeMs(), clock.GetTimeMs() + durationMs,
                    new AddVector3Animation(
                        new ConstantVector3Animation(offsetToLookAtDifference),
                        new LinearVector3Animation(
                            clock.GetTimeMs(), clock.GetTimeMs() + durationMs, -offsetToLookAtDifference, new Vector3(0, 0, 0)))));
      }
      lookatOffsetToCamera = new_offsetToLookAt;
    }

    public void StartMovingCameraTo(Vector3 newCameraEndLookAtPosition, long durationMs) {
      var animator = GetOrCreateCameraAnimator();
      if (durationMs == 0) {
        animator.lookAtAnimation =
          new ConstantVector3Animation(newCameraEndLookAtPosition);
      } else {
        var currentCameraEndLookAtPosition = cameraEndLookAtPosition;
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
      cameraEndLookAtPosition = newCameraEndLookAtPosition;
    }
    
    public void MoveIn(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition +
                cameraObject.transform.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveOut(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition -
                cameraObject.transform.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveUp(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition +
                Vector3.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveDown(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition -
              Vector3.forward * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveLeft(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition -
              Vector3.right * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }

    public void MoveRight(float deltaTime) {
      var newEndLookAtPosition =
          cameraEndLookAtPosition +
              Vector3.right * deltaTime * cameraSpeedPerSecond;
      StartMovingCameraTo(newEndLookAtPosition, 50);
    }
  }
}
