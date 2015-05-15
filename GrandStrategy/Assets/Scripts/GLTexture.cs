using UnityEngine;
using System.Collections;


public class GLTexture : MonoBehaviour
{
    Texture2D texture;
    Material material;


    void Start()
    {
        // create material for GL rendering //
        material = new Material(Shader.Find("GUI/Text Shader"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.shader.hideFlags = HideFlags.HideAndDontSave;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int timer = System.Environment.TickCount;

            Destroy(texture);
            texture = RenderGLToTexture(512, 512, material);
            texture = RenderGLToTexture(512, 512, material);


            Debug.Log("Avarage generation is: " + (System.Environment.TickCount - timer) + "ms");

        }
    }


    static Texture2D RenderGLToTexture(int width, int height, Material material)
    {


        // get a temporary RenderTexture //
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);

        // set the RenderTexture as global target (that means GL too)
        RenderTexture.active = renderTexture;

        // clear GL //
        GL.Clear(false, true, Color.black);

        // render GL immediately to the active render texture //
        RenderGLStuff(width, height, material);

        // read the active RenderTexture into a new Texture2D //
        Texture2D newTexture = new Texture2D(width, height);
        newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // apply pixels and compress //
        bool applyMipsmaps = false;
        newTexture.Apply(applyMipsmaps);
        bool highQuality = true;
        newTexture.Compress(highQuality);

        // clean up after the party //
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);


        // return the goods //
        return newTexture;
    }


    static void RenderGLStuff(int width, int height, Material material)
    {
        material.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, width, height, 0);
        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(new Color(1, 0, 0, 1));
        for (int i = 0; i < 243; i++) GL.Vertex3(Random.value * width, Random.value * height, 0);
        GL.End();
        GL.PopMatrix();
    }


    void OnGUI()
    {
        GUILayout.Label("Press <SPACE> to render GL commands to a Texture2D");
        if (texture == null) return;
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - texture.width * 0.5f, Screen.height * 0.5f - texture.height * 0.5f, texture.width, texture.height), texture);
    }

}
