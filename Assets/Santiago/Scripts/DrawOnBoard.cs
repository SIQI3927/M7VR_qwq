using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DrawOnBoard : NetworkBehaviour
{
    public NetworkVariable<int> n_brushSize = new NetworkVariable<int>();
    public NetworkVariable<Color> n_color = new NetworkVariable<Color>();
    private Texture2D backgroundTexture; // Guarda la textura en el servidor
    private List<Vector2Int> pixelBuffer = new List<Vector2Int>();

    public bool isSending = false;
    public float interval = 0.5f;

    public Color paintColor = Color.white; // Color del "pincel"
    public int brushSizeValue = 5; // Tamaño del pincel

    public Vector2? previousDot = null;
    private Texture2D texture; // Textura de la pizarra

    [SerializeField]
    private MeshRenderer boardMaterial; // Renderer de la pizarra

    [SerializeField]
    private Slider brushSize;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            InitTexture();
        }
        else
        {
            RequestFullTextureServerRpc();
        }
        n_brushSize.OnValueChanged += (oldValue, newValue) =>
        {
            brushSizeValue = newValue;
            brushSize.value = brushSizeValue;
        };

        n_color.OnValueChanged += (oldValue, newValue) =>
        {
            paintColor = newValue;
        };
    }

    [ServerRpc(RequireOwnership =false)]
    public void RequestNewBrushSizeServerRpc(int newValue)
    {
        n_brushSize.Value = newValue;
    }

    [ServerRpc(RequireOwnership =false)]
    public void RequestNewColorBrushServerRpc(Color newColor)
    {
        n_color.Value = newColor;
    }

    private void InitTexture()
    {
        //originalTexture = boardMaterial.material.mainTexture as Texture2D;
        texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        backgroundTexture = new Texture2D(texture.width, texture.height);
        for (int i = 0; i < backgroundTexture.width; i++)
        {
            for (int j = 0; j < backgroundTexture.height; j++)
            {
                backgroundTexture.SetPixel(i, j, Color.white); // Inicializar en blanco    
            }
        }
        texture.SetPixels32(backgroundTexture.GetPixels32());
        texture.Apply();
        boardMaterial.material.mainTexture = texture;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFullTextureServerRpc(ServerRpcParams rpcParams = default)
    {
        SendFullTextureClientRpc(texture.GetPixels32(), rpcParams.Receive.SenderClientId);
    }
    void Start()
    {
        InitTexture();
        NewBrushSize();
        EraseDrawServerRpc();
    }

    [ClientRpc]
    private void SendFullTextureClientRpc(Color32[] colors, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            texture.SetPixels32(colors);
            texture.Apply();
        }
    }

    public void NewBrushSize()
    {
        if (IsClient)
        {
            RequestNewBrushSizeServerRpc((int)brushSize.value);
        }
    }

    public void PaintOnTexture(Vector2 uv)
    {
        if (texture == null) return;

        if (previousDot != null)
        {
            Vector2 start = previousDot.Value;
            Vector2 end = uv;

            float distance = Vector2.Distance(start, end);
            int steps = Mathf.CeilToInt(distance * texture.width);

            for (int step = 0; step <= steps; step++)
            {
                float t = (float)step / steps;
                Vector2 InterpolatedUV = Vector2.Lerp(start, end, t);

                DrawPoint(InterpolatedUV,paintColor, brushSizeValue);
            }
        }
        //DrawPoint(uv, paintColor, brushSizeValue);
        previousDot = uv;
    }

    private IEnumerator SendPixelsCoroutine()
    {
        isSending = true;
        while (pixelBuffer.Count > 0)
        {
            yield return new WaitForSeconds(0.8f);

            if (pixelBuffer.Count > 0)
            {
                SendPixelsToServerRpc(pixelBuffer.ToArray(), paintColor);
                pixelBuffer.Clear();
            }
        }
        isSending = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPixelsToServerRpc(Vector2Int[] pixels, Color color)
    {
        ApplyPixelsToClientsClientRpc(pixels, color);
    }

    [ClientRpc]
    private void ApplyPixelsToClientsClientRpc(Vector2Int[] pixels, Color color)
    {
        foreach (var pixel in pixels)
        {
            //// Convertir las coordenadas UV a píxeles
            int x = pixel.x;
            int y = pixel.y;
            int px = 0;
            int py = 0;

            // Pintar en la textura dentro del área del pincel
            for (int i = -brushSizeValue; i <= brushSizeValue; i++)
            {
                for (int j = -brushSizeValue; j <= brushSizeValue; j++)
                {
                    float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(x + i, y + j));
                    //float distanceToCenter = Mathf.Sqrt(i*i + j*j);
                    px = Mathf.Clamp(x + i, 0, texture.width - 1);
                    py = Mathf.Clamp(y + j, 0, texture.height - 1);
                    if (distanceToCenter < brushSizeValue)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
        texture.Apply();
    }

    public void DrawPoint(Vector2 uv, Color color, int brushSize)
    {
        //// Convertir las coordenadas UV a píxeles
        int x = (int)(uv.x * texture.width);
        int y = (int)(uv.y * texture.height);

        pixelBuffer.Add(new Vector2Int(x, y));
        // Iniciar envío si no está en curso

        StartCoroutine(SendPixelsCoroutine());
    }

    [ServerRpc(RequireOwnership = false)]
    public void EraseDrawServerRpc()
    {
        EraseBoardClientRpc();
    }

    [ClientRpc]
    public void EraseBoardClientRpc()
    {
        if (IsClient)
        {
            texture.SetPixels32(backgroundTexture.GetPixels32());
            texture.Apply();
        }
    }
    public void PaintColorBlack()
    {
        if (IsClient)
            RequestNewColorBrushServerRpc(Color.black);
    }

    public void PaintColorRed()
    {
        if (IsClient)
            RequestNewColorBrushServerRpc(Color.red);
    }

    public void PaintColorBlue()
    {
        if (IsClient)
            RequestNewColorBrushServerRpc(Color.blue);
    }

    public void PaintColorGreen()
    {
        if (IsClient)
            RequestNewColorBrushServerRpc(Color.green);
    }

    public void PaintColorWhite()
    {
        if (IsClient)
            RequestNewColorBrushServerRpc(Color.white);
    }
}
