using System.Collections;
using System.Collections.Generic;
using Domino;
using UnityEngine;

// Every frame, this will evaluate an animation, and call the given callback.
// This is useful because it's a MonoBehaviour and therefore tied to the lifetime
// of an object.
public class Vec4Animator : MonoBehaviour {
  public delegate void IOnValue(Vector4 value);
  
  private IClock clock;
  private IOnValue onValue;
  private IVector4Animation animation;

  public static Vec4Animator MakeOrGetFrom(IClock clock, GameObject gameObject, Vector4 initialValue, IOnValue onValue) {
    var animator = gameObject.GetComponent<Vec4Animator>() as Vec4Animator;
    if (animator == null) {
      animator = gameObject.AddComponent<Vec4Animator>() as Vec4Animator;
      animator.Init(clock, initialValue, onValue);
    }
    return animator;
  }

  public void Init(IClock clock, Vector4 initialValue, IOnValue onValue) {
    this.onValue = onValue;
    this.clock = clock;
    this.animation = new ConstantVector4Animation(initialValue);
  }

  public IVector4Animation Get() {
    return animation;
  }

  public void Set(IVector4Animation newAnimation, RenderPriority newRenderPriority) {
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

    if (animation is ConstantVector4Animation || animation is IdentityVector4Animation) {
      Destroy(this);
    }
  }
}
