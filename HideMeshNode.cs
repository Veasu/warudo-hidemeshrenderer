
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
namespace veasu.hiderenderer
{
  [NodeType(Id = "com.veasu.hiderenderer", Title = "Hide Renderer From Asset", Category = "Hide Renderer")]
  public class HideMeshRendererNode : Warudo.Core.Graphs.Node
  {
    [DataInput]
    [Label("Root Object")]
    Warudo.Plugins.Core.Assets.GameObjectAsset rootAsset;

    [DataInput]
    [HiddenIf(nameof(hideHideAll))]
    [Label("Hide All")]
    public bool hideAllMeshRenderers = true;

    [DataInput]
    [HiddenIf(nameof(hideGameObject))]
    [AutoComplete(nameof(AutoCompleteChildObjectList))]
    [Label("Mesh Renderer")]
    public string childObjects;

    private bool hideGameObject() {
      return renderDict == null || renderDict.Count == 0 || hideAllMeshRenderers;
    }

    private bool hideHideAll() {
      return renderDict == null || renderDict.Count == 0;
    }
    private async UniTask<AutoCompleteList> AutoCompleteChildObjectList() => AutoCompleteList.Single(renderDict.Select<KeyValuePair<string,Renderer>, AutoCompleteEntry>((Func<KeyValuePair<string,Renderer>, AutoCompleteEntry>)(it => new AutoCompleteEntry()
    {
      label = it.Key,
      value = it.Key
    })));

    private Dictionary<string, Renderer> renderDict;

    protected override void OnCreate() {
      Watch<Warudo.Plugins.Core.Assets.GameObjectAsset>(nameof(rootAsset), (from, to) => {
        if (from != null) {
          foreach (Renderer objectRenderer in from.GameObject.GetComponentsInChildren<Renderer>()) {
            objectRenderer.enabled = true;
          }
          from.Broadcast();
        }
        if (to != null && to.Active) {
          var tempDict = new Dictionary<string, Renderer>();
          foreach (Renderer objectRenderer in to.GameObject.GetComponentsInChildren<Renderer>())
          {
            if (objectRenderer != null) {
              if (!tempDict.ContainsKey(objectRenderer.name)) {
                tempDict.Add(objectRenderer.name, objectRenderer);
              }               
              if (hideAllMeshRenderers) objectRenderer.enabled = false;
            }
          }
          this.renderDict = tempDict;
        } else {
          this.renderDict = new Dictionary<string, Renderer>();
        }
        childObjects = null;
        BroadcastDataInput(nameof(childObjects));
      });

      Watch<string>(nameof(childObjects), (from, to) => {
        if (to != null) {
          renderDict[to].enabled = false;
        }
        if (from != null){
          renderDict[from].enabled = true;
        }
      });

      Watch<bool>(nameof(hideAllMeshRenderers), (from, to) => {
        foreach (Renderer renderObj in renderDict.Values) {
          renderObj.enabled = !to;
        }
      });
    }
  }
}
