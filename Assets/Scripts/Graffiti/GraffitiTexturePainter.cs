/*
NOTA 1: LA SUPERFICIE EN LA QUE PINTAR DEBE:
- Tener la capa "Paintable"
- Tener un MeshCollider (OJO: MeshCollider, ningun otro collider sirve)
NOTA 2: En la funcion "HitTestUVPosition()" se debe multiplicar uvWorldPosition x/y por la proporción de la RenderTexture.
Ejemplo: Si la RenderTexture tiene una proporcion de 5x1 (5 veces mas ancha que alta) se debe hacer : uvWorldPosition.x = (pixelUV.x - canvasCam.orthographicSize) * 5;
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraffitiTexturePainter : MonoBehaviour
{
    public string handR;
    public Transform hand;
    //public GameObject colorPicker;
    public GameObject cuadWall, cuadReset;
    public const float MAX_DIST = 1f;

    public GameObject brushContainer; //The cursor that overlaps the model and our container for the brushes painted
    public Camera canvasCam;  //The camera that looks at the model, and the camera that looks at the canvas.
    public Sprite cursorPaint; // Cursor for the differen functions 
    public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    public Material baseMaterial; // The material of our base texture (Were we will save the painted texture)

    //Painter_BrushMode mode; //Our painter mode (Paint brushes or decals)
    float brushSize = 1.0f; //The size of our brush
    Color brushColor; //The selected color
    int brushCounter = 0, MAX_BRUSH_COUNT = 200; //To avoid having millions of brushes
    bool saving = false; //Flag to check if we are saving the texture

    public LayerMask paintMask;
    public GameObject brushPrefab, cursorPrefab;
    SpriteRenderer cursorSprite;
    public ParticleSystem sprayParticle;
    ParticleSystem.MainModule sprayParticleModule;
    bool flag, flagSoundSpray1, flagSoundSpray2;

    public Animator handSpray;
    public AudioSource spraySound;

    private void Start()
    {
        //PoolManager.instance.CreatePool(brushPrefab, MAX_BRUSH_COUNT);

        cursorPrefab.SetActive(false);
        //cursorSprite = GameObject.Find("BrushCursor").GetComponent<SpriteRenderer>();
        sprayParticleModule = hand.GetComponentInChildren<ParticleSystem>().main;
    }

    void Update()
    {
        if (GameManager.GM.pointingCP)
        {
            if (cursorPrefab.activeSelf)
                cursorPrefab.SetActive(false);
            if (sprayParticle.isPlaying)
            {
                sprayParticle.Stop();
                spraySound.Stop();
                flagSoundSpray1 = false;
            }
            return;
        }

        //if (colorPicker.activeSelf)
        brushColor = GraffitiManager.Instance.color; //Color.blue; // ColorSelector.GetColor();  //Updates our painted color with the selected color


        if (Input.GetButton("triggerButton_" + handR))
        {
            flag = true;
            DoAction();
        }
        else
        {
            sprayParticle.Stop();
            spraySound.Stop();
            flagSoundSpray1 = false;

            if (handSpray.GetCurrentAnimatorStateInfo(0).IsName("Push"))
                handSpray.Play("Release");

            if (flag && brushCounter >= 150)
            {
                //SaveTexture();
                flag = false;
            }
        }

        //RESET DRAWING
        if (Input.GetButton("secondaryButton_" + handR))
        {

            ResetTexture();
        }

        ShowCursor();
        sprayParticleModule.startColor = new ParticleSystem.MinMaxGradient(new Color(brushColor.r, brushColor.g, brushColor.b, 0.25f));
        //cursorSprite.color = brushColor;

        if (flagSoundSpray1 && flagSoundSpray2)
        {
            spraySound.Play();
            flagSoundSpray2 = false;

            handSpray.Play("Push");
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

    }


    //The main action, instantiates a brush or decal entity at the clicked position on the UV map
    void DoAction()
    {
        if (saving)
            return;

        if (!flagSoundSpray1)
        {
            flagSoundSpray1 = true; // Para empezar a reproducir el audio de spray
            flagSoundSpray2 = true; // Para empezar a reproducir el audio de spray
        }
        sprayParticle.Play();

        Vector3 uvWorldPosition = Vector3.zero;
        if (HitTestUVPosition(ref uvWorldPosition))
        {
            GameObject brushObj;

            brushColor.a = 1f / Mathf.Exp(brushSize * 1200); //brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.

            brushObj = (GameObject)Instantiate(brushPrefab);//(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
            //brushObj = PoolManager.instance.ReuseObject(brushPrefab, Vector3.zero, Quaternion.identity);

            brushObj.GetComponent<SpriteRenderer>().color = brushColor; //Set the brush color


            brushObj.transform.parent = brushContainer.transform; //Add the brush to our container to be wiped later
            brushObj.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
            brushObj.transform.localScale = Vector3.one * brushSize;//The size of the brush
        }
        brushCounter++; //Add to the max brushes
        if (brushCounter >= MAX_BRUSH_COUNT)
        { //If we reach the max brushes available, flatten the texture and clear the brushes
            //brushCursor.SetActive(false);
            saving = true;

            SaveTexture();

        }
    }

    //Returns the position on the texuremap according to a hit in the mesh collider
    bool HitTestUVPosition(ref Vector3 uvWorldPosition)
    {
        RaycastHit hit;

        Ray cursorRay = new Ray(hand.position, hand.forward);
        if (Physics.Raycast(cursorRay, out hit, MAX_DIST, paintMask))
        {
            brushSize = (hit.distance / 2) * (0.02f); //* (hit.distance * 1.2f));

            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return false;
            Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
            uvWorldPosition.x = (pixelUV.x - canvasCam.orthographicSize) * 5;//To center the UV on X
            uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y
            uvWorldPosition.z = 0.0f;

            return true;
        }
        else
        {
            return false;
        }

    }

    void ShowCursor()
    {
        RaycastHit hit;

        Ray cursorRay = new Ray(hand.position, hand.forward);
        if (Physics.Raycast(cursorRay, out hit, MAX_DIST / 2, paintMask))
        {
            //if (!cursorPrefab)
            //{
            //    Instantiate(cursorPrefab);
            //}

            float cursorSize = (hit.distance / 2) * (0.02f); //* (hit.distance * 1.2f));

            /*
            cursorPrefab.GetComponent<SpriteRenderer>().color = new Color(
                cursorPrefab.GetComponent<SpriteRenderer>().color.r,
                cursorPrefab.GetComponent<SpriteRenderer>().color.b,
                cursorPrefab.GetComponent<SpriteRenderer>().color.g,
                1f / Mathf.Exp(brushSize * 1200));
            */


            cursorPrefab.transform.position = hit.point + -cursorPrefab.transform.forward * 0.001f; ;
            cursorPrefab.transform.localScale = Vector3.one * cursorSize * 3f; // This number is the multiplier of the scale of the surface that we are painting on

            if (!cursorPrefab.activeSelf)
                cursorPrefab.SetActive(true);
        }
        else
        {
            if (cursorPrefab.activeSelf)
                cursorPrefab.SetActive(false);
        }

    }

    //Sets the base material with a our canvas texture, then removes all our brushes
    void SaveTexture()
    {
        brushCounter = 0;
        System.DateTime date = System.DateTime.Now;
        RenderTexture.active = canvasTexture;
        Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);

        //***
        tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);

        //StartCoroutine(Wait1(2, tex));
        //***

        if (cuadWall)
            Destroy(cuadWall);

        //***
        tex.Apply();
        RenderTexture.active = null;
        baseMaterial.mainTexture = tex; //Put the painted texture as the base
        foreach (Transform child in brushContainer.transform)
        {
            //Clear brushes
            Destroy(child.gameObject);
        }

        StartCoroutine(Wait3(0.2f));

        //StartCoroutine(Wait2(2, tex));
        //***

        //StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
        Invoke("Cc", 0.1f);
    }

    //Sets the base material with a our canvas texture, then removes all our brushes
    void ResetTexture()
    {
        cuadReset.SetActive(true);

        brushCounter = 0;
        System.DateTime date = System.DateTime.Now;
        RenderTexture.active = canvasTexture;
        Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);

        tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);

        if (cuadWall)
            Destroy(cuadWall);

        tex.Apply();
        RenderTexture.active = null;
        baseMaterial.mainTexture = tex; //Put the painted texture as the base

        StartCoroutine(Wait3(0.2f));
    }

    //Show again the user cursor (To avoid saving it to the texture)
    void Cc()
    {
        saving = false;
    }

    IEnumerator Wait1(float time, Texture2D tex)
    {
        yield return new WaitForSeconds(time);
        tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
        if (cuadWall)
            Destroy(cuadWall);
    }

    IEnumerator Wait2(float time, Texture2D tex)
    {
        yield return new WaitForSeconds(time);
        tex.Apply();
        RenderTexture.active = null;
        baseMaterial.mainTexture = tex; //Put the painted texture as the base
        foreach (Transform child in brushContainer.transform)
        {
            //Clear brushes
            Destroy(child.gameObject);
        }
    }

    IEnumerator Wait3(float time)
    {
        foreach (Transform child in brushContainer.transform)
        {
            //Clear brushes
            Destroy(child.gameObject);
        }
        yield return new WaitForSeconds(time);

        if (cuadReset)
            cuadReset.SetActive(false);
    }





    /*
        ////////////////// PUBLIC METHODS //////////////////

        public void SetBrushMode(Painter_BrushMode brushMode)
        { //Sets if we are painting or placing decals
            mode = brushMode;
            //brushCursor.GetComponent<SpriteRenderer>().sprite = cursorPaint;
        }
        public void SetBrushSize(float newBrushSize)
        { //Sets the size of the cursor brush or decal
            brushSize = newBrushSize;
            //brushCursor.transform.localScale = Vector3.one * brushSize;
        }
    */



}

/* NEW SAVING TEXTURE (SE SUPONE QUE ES MORE PERFORMANT)

    void SaveTexture()
    {
        brushCounter = 0;

        Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);

        RenderTexture.active = canvasTexture;

        Texture2D image = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);

        image.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);

        if (cuadWall)
            Destroy(cuadWall);

        image.Apply();

        RenderTexture.active = null;

        Graphics.CopyTexture(image, tex);

        baseMaterial.mainTexture = tex; //Put the painted texture as the base
        foreach (Transform child in brushContainer.transform)
        {
            //Clear brushes
            Destroy(child.gameObject);
        }
        //StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
        Invoke("Cc", 0.1f);
    }

*/

/* CAMBIAR COLOR CURSOR

    cursorColor = GraffitiManager.Instance.color; //Color.blue; // ColorSelector.GetColor();  //Updates our painted color with the selected color
                                                      //cursorColor = brushColor;
                                                      //cursorColor.a = 1f / Mathf.Exp(brushSize * 1200); //brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.

        cursorColor.a = 1f / Mathf.Exp(brushSize * 1000); //brushSize * 2.0f; // Brushes have alpha to have a merging effect when painted over.
        cursorPrefab.GetComponent<SpriteRenderer>().color = cursorColor; //Set the brush color
*/

