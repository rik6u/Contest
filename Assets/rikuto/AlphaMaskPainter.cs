using UnityEngine;

public class AlphaMaskPainter : MonoBehaviour
{
    public RenderTexture alphaRT;
    public Material paintMat;
    public Camera cam;
    public Transform mirror;
    public Material recoverMat;

    [Header("ブラシ設定")]
    public float brushSize = 0.05f;

    [Header("回復設定")]
    [Range(0f, 1f)]
    public float recoverSpeed = 0.05f;

    private RenderTexture tempRT;
    private Vector2? lastUV = null;

    void Start()
    {
        RenderTexture.active = alphaRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        tempRT = new RenderTexture(alphaRT.width, alphaRT.height, 0, alphaRT.format);
        tempRT.Create();

        Debug.Log($"[Start] alphaRT = {alphaRT.width}x{alphaRT.height}, format = {alphaRT.format}");
        Debug.Log($"[Start] tempRT  = {tempRT.width}x{tempRT.height}, format = {tempRT.format}");
        Debug.Log($"[Start] 中心ピクセル初期値 = {GetCenterValue(alphaRT)}");
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            DrawAtMouse();
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastUV = null;
            Debug.Log("[Input] マウスを離したので lastUV をリセット");
        }

        Recover();
    }

    void DrawAtMouse()
    {
        Vector3 mouse = Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = 0;

        Vector3 local = mirror.InverseTransformPoint(world);

        float uvX = local.x + 0.5f;
        float uvY = local.y + 0.5f;

        Vector2 currentUV = new Vector2(uvX, uvY);

        //Debug.Log($"[DrawAtMouse] mouse={mouse} world={world} local={local} uv={currentUV}");

        if (lastUV != null)
        {
            float dist = Vector2.Distance(lastUV.Value, currentUV);
            int steps = Mathf.CeilToInt(dist / brushSize);

            //Debug.Log($"[DrawAtMouse] 補間あり dist={dist} steps={steps}");

            for (int i = 0; i <= steps; i++)
            {
                Vector2 uv = Vector2.Lerp(
                    lastUV.Value,
                    currentUV,
                    i / (float)steps
                );

                DrawQuad(uv);
            }
        }
        else
        {
           // Debug.Log("[DrawAtMouse] 最初の1点だけ描画");
            DrawQuad(currentUV);
        }

        lastUV = currentUV;
    }

    void DrawQuad(Vector2 uv)
    {
        float before = GetCenterValue(alphaRT);

        RenderTexture.active = alphaRT;

        GL.PushMatrix();
        GL.LoadOrtho();

        paintMat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.Vertex3(uv.x - brushSize, uv.y - brushSize, 0);
        GL.Vertex3(uv.x + brushSize, uv.y - brushSize, 0);
        GL.Vertex3(uv.x + brushSize, uv.y + brushSize, 0);
        GL.Vertex3(uv.x - brushSize, uv.y + brushSize, 0);

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = null;

        float after = GetCenterValue(alphaRT);

        //Debug.Log($"[DrawQuad] uv={uv} brushSize={brushSize} 中心ピクセル before={before} after={after}");
    }

    void Recover()
    {
        float strength = recoverSpeed * Time.deltaTime;

        float before = GetCenterValue(alphaRT);

       // Debug.Log($"[Recover] recoverSpeed={recoverSpeed} deltaTime={Time.deltaTime} strength={strength}");

        if (strength <= 0f)
        {
            Debug.Log("[Recover] strength が 0 以下なので処理をスキップ");
            return;
        }

        // alphaRT -> tempRT にコピー
        Graphics.Blit(alphaRT, tempRT);

        float tempValue = GetCenterValue(tempRT);
        Debug.Log($"[Recover] tempRT の中心ピクセル = {tempValue}");

        // Shader に値を渡す
        recoverMat.SetTexture("_MainTex", tempRT);
        recoverMat.SetFloat("_Strength", strength);

        // tempRT -> alphaRT に書き戻し
        Graphics.Blit(tempRT, alphaRT, recoverMat);

        float after = GetCenterValue(alphaRT);

        Debug.Log($"[Recover] 中心ピクセル before={before} after={after} 差分={before - after}");
    }

    /// <summary>
    /// RenderTexture の中心ピクセルの R 値を取得する
    /// </summary>
    float GetCenterValue(RenderTexture rt)
    {
        RenderTexture current = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        float value = tex.GetPixel(rt.width / 2, rt.height / 2).r;

        Destroy(tex);
        RenderTexture.active = current;

        return value;
    }

    void OnDestroy()
    {
        if (tempRT != null)
        {
            tempRT.Release();
        }
    }
}