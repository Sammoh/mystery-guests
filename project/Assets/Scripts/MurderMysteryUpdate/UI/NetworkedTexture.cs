using UnityEngine;
using Unity.Netcode;

public class NetworkedTexture : NetworkBehaviour
{

    // public void SendTexture(Texture2D texture)
    // {
    //     if (IsServer)
    //     {
    //         byte[] textureBytes = texture.EncodeToPNG();
    //         networkedTextureBytes.Value = textureBytes;
    //     }
    // }
    //
    // private void Update()
    // {
    //     if (IsClient)
    //     {
    //         byte[] textureBytes = networkedTextureBytes.Value;
    //         if (textureBytes != null)
    //         {
    //
    //             // Do something with the received texture
    //         }
    //     }
    // }
}