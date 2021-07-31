using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Domino {
  public class Requester : MonoBehaviour {
    public delegate void IHandleResponse(JSONObject json);

    private bool requesting;
    private IHandleResponse handleResponse;

    public void Request(string url, JSONObject body, IHandleResponse handleResponse) {
      if (requesting) {
        Debug.LogWarning("Already requesting!");
        return;
      }
      requesting = true;
      this.handleResponse = handleResponse;
      StartCoroutine(PostRequest(url, body.ToString()));
    }

    IEnumerator PostRequest(string url, string json) {
      var uwr = new UnityWebRequest(url, "POST");
      byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
      uwr.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
      uwr.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
      uwr.SetRequestHeader("Content-Type", "application/json");

      //Send the request then wait here until it returns
      yield return uwr.SendWebRequest();

      if (uwr.isNetworkError) {
        Debug.Log("Error while sending to:\n" + url + "\nError: " + uwr.error);
        requesting = false;
        handleResponse(null);
      } else {
        Debug.Log("Received: " + uwr.downloadHandler.text);
        requesting = false;
        var node = JSONObject.Parse(uwr.downloadHandler.text);
        if (node == null) {
          Debug.LogError("Couldn't parse response JSON!");
          handleResponse(null);
        } else if (node is JSONObject obj) {
          handleResponse(obj);
        } else {
          Debug.LogError("Response JSON wasn't an object!");
          handleResponse(null);
        }
      }
    }
  }
}
