using System.Collections;
using System.Collections.Generic;
using Domino;
using UnityEngine;

public class ColorChanger : MonoBehaviour {
  private GameObject[] lightColoredParts;
  private GameObject[] darkColoredParts;
  
  // private MaterialCache lightMaterialCache;
  // private MaterialCache darkMaterialCache;
  
  private Color currentColor;
  private RenderPriority renderPriority;
  
  
  private MaterialPropertyBlock lightProps;
  private MaterialPropertyBlock darkProps;
  
  
  public static ColorChanger AddTo(
      GameObject gameObject,
      GameObject[] lightColoredParts,
      GameObject[] darkColoredParts) {
    var changer = gameObject.AddComponent<ColorChanger>();
    changer.Init(lightColoredParts, darkColoredParts);
    return changer;
  }
  
  private void Init(
      GameObject[] lightColoredParts,
      GameObject[] darkColoredParts) {
    this.lightColoredParts = lightColoredParts;
    this.darkColoredParts = darkColoredParts;
    this.lightProps = new MaterialPropertyBlock();
    this.darkProps = new MaterialPropertyBlock();
    // lightMaterialCache = new MaterialCache(loader);
    // darkMaterialCache = new MaterialCache(loader);
  }
  
  public (Color, RenderPriority) Get() {
    return (currentColor, renderPriority);
  }
  
  public void Set(Color newCurrentColor, RenderPriority newRenderPriority) {
    if (newCurrentColor == currentColor && newRenderPriority == renderPriority) {
      return;
    }
    
    renderPriority = newRenderPriority;
    currentColor = newCurrentColor;
    
    lightProps.SetColor("_Color", currentColor);
    
    //       cachedTransparentMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + (int)renderPriority;
  
    // var lightMaterial = lightMaterialCache.GetMaterial(newCurrentColor, renderPriority);
    foreach (var part in lightColoredParts) {
      var meshRenderer = part.GetComponent<MeshRenderer>();
      meshRenderer.SetPropertyBlock(lightProps);
      // meshRenderer.material = lightMaterial;
      meshRenderer.enabled = newCurrentColor.a > .001f;
    }
  
    if (darkColoredParts.Length > 0) {
      Color darkColor = new Color(currentColor.r * .8f, currentColor.g * .8f, currentColor.b * .8f, currentColor.a);
      darkProps.SetColor("_Color", darkColor);
      // var darkMaterial = darkMaterialCache.GetMaterial(darkColor, renderPriority);
      foreach (var part in darkColoredParts) {
        var meshRenderer = part.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(darkProps);
        // meshRenderer.material = darkMaterial;
        meshRenderer.enabled = newCurrentColor.a > .001f;
      }
    }
  }
  
  // class MaterialCache {
  //   private ILoader loader;
  //   private Material cachedGlowMaterial = null;
  //   private Material cachedOpaqueMaterial = null;
  //   private Material cachedTransparentMaterial = null;
  //
  //   public MaterialCache(ILoader loader) {
  //     this.loader = loader;
  //   }
  //
  //   public Material GetMaterial(Color color, RenderPriority renderPriority) {
  //     if (color.a < .99f) {
  //       if (ReferenceEquals(cachedTransparentMaterial, null)) {
  //         cachedTransparentMaterial = Instantiate(loader.white);//Instantiate(changer.transparentMaterial);
  //       }
  //       cachedTransparentMaterial.color = color;
  //       cachedTransparentMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + (int)renderPriority;
  //       return cachedTransparentMaterial;
  //     } else if (color.a > 1.01f) {
  //       if (ReferenceEquals(cachedTransparentMaterial, null)) {
  //         cachedGlowMaterial = loader.white;
  //       }
  //       cachedGlowMaterial.color = color;
  //       cachedGlowMaterial.SetColor("_EmissionColor", color);
  //       return cachedGlowMaterial;
  //     } else {
  //       if (ReferenceEquals(cachedOpaqueMaterial, null)) {
  //         cachedOpaqueMaterial = Instantiate(loader.white);
  //       }
  //       cachedOpaqueMaterial.color = color;
  //       return cachedOpaqueMaterial;
  //     }      
  //   }
  // }
}
