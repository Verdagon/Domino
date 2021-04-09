using System.Collections;
using System.Collections.Generic;
using Domino;
using UnityEngine;

// Every frame, this will evaluate an animation, and call the given callback.
// This is useful because it's a MonoBehaviour and therefore tied to the lifetime
// of an object.
public class Vec3Animator : MonoBehaviour {
  public delegate void IOnValue(Vector3 value);
  
  private IClock clock;
  private IOnValue onValue;
  private IVector3Animation animation;

  public static Vec3Animator MakeOrGetFrom(IClock clock, GameObject gameObject, Vector3 initialValue, IOnValue onValue) {
    var animator = gameObject.GetComponent<Vec3Animator>() as Vec3Animator;
    if (animator == null) {
      animator = gameObject.AddComponent<Vec3Animator>() as Vec3Animator;
      animator.Init(clock, initialValue, onValue);
    }
    return animator;
  }

  public void Init(IClock clock, Vector3 initialValue, IOnValue onValue) {
    this.onValue = onValue;
    this.clock = clock;
    this.animation = new ConstantVector3Animation(initialValue);
  }

  public IVector3Animation Get() {
    return animation;
  }

  public void Set(IVector3Animation newAnimation, RenderPriority newRenderPriority) {
    Asserts.Assert(newAnimation != null);
    animation = newAnimation;

    Update();
  }

  public void Start() { }

  public void Update() {
    Asserts.Assert(animation != null, "No animation??");
    Asserts.Assert(clock != null, "No animation??");
    Asserts.Assert(onValue != null, "No onValue??");
    animation = animation.Simplify(clock.GetTimeMs());
    var value = animation.Get(clock.GetTimeMs());
    onValue(value);

    if (animation is ConstantVector3Animation || animation is IdentityVector3Animation) {
      Destroy(this);
    }
  }
}
