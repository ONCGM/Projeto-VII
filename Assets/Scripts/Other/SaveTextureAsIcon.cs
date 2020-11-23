using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Other {
    /// <summary>
    /// Saves a camera render texture as an image.
    /// </summary>
    public class SaveTextureAsIcon : MonoBehaviour {
        #if UNITY_EDITOR
        [SerializeField] private string texName = "";
        [SerializeField] private int width = 512;
        [SerializeField] private int height = 512;
        //[SerializeField] private int distanceToCaptureObjectFrom = 5;
        //[SerializeField] private List<GameObject> prefabsToCapture = new List<GameObject>();
        private readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
        private int loopCount;

        // Triggers the coroutine to save the things.
        // private IEnumerator Start() {
        //     waitFrame = new WaitForEndOfFrame();
        //     foreach(var prefab in prefabsToCapture) {
        //         var gO = Instantiate(prefab, transform);
        //         gO.transform.localPosition = Vector3.forward * distanceToCaptureObjectFrom;
        //         transform.LookAt(gO.transform);
        //         StartCoroutine(nameof(SaveTexture));
        //         Destroy(gO);
        //         loopCount++;
        //         yield return waitFrame;
        //     }
        // }

        // Triggers Capture from editor.
        [ContextMenu("Capture")]
        private void StartCapture() {
            StartCoroutine(nameof(SaveTexture));
            loopCount++;
        }
        
        /// <summary>
        /// Saves a camera render texture to png.
        /// </summary>
        private IEnumerator SaveTexture() {
            yield return waitFrame;
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.alphaIsTransparency = true;
            texture.anisoLevel = 2;
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
            texture.Apply();
            byte[] texBytes = texture.EncodeToPNG();
            Object.Destroy(texture);
            File.WriteAllBytes((Application.dataPath + "/Art/Sprites/Icon/" + texName + loopCount + ".png"), texBytes);
            print((Application.dataPath + "/Art/Sprites/Icon/" + texName + loopCount + ".png"));
        }
        #endif
    }
}