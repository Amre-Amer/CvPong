using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pong : MonoBehaviour {
    GameObject ball;
    GameObject ballNew;
    GameObject wallRight;
    GameObject wallLeft;
    GameObject wallTop;
    GameObject wallBottom;
    GameObject paddle;
    GameObject paddleTarget;
    Vector3 vect;
    Vector3 vectPaddle;
    GameObject ballEst;
    int pixelRes = 256;
    float paddleThickness = 1;
    float paddleX = 30;
    float paddleWidth = 10;
    float paddleSpeed = 2f;
    float ballSpeed = 2;
    float ipd = 30;
    float headX = 5;
    float headZ = 25;
    float nearClipPlaneDist = 3;
    float fov = 100;
    public bool ynStep;
    float timeStart;
    GameObject ll;
    GameObject ul;
    GameObject lr;
    GameObject camBodyL;
    GameObject camBodyR;
    GameObject pixL;
    GameObject pixR;
    GameObject scrL;
    GameObject scrR;
    GameObject linL;
    GameObject linR;
    GameObject linTarget;
    RenderTexture rtL;
    RenderTexture rtR;
    Material matL;
    Material matR;
    int frameCount;
    Camera camL;
    Camera camR;

	// Use this for initialization
	void Start () {
//        test();
        initGOs();
	}

    void test() {
        Vector3 a1 = new Vector3(0, 0, 0);        
        Vector3 a2 = new Vector3(1, 0, 1);        
        Vector3 b1 = new Vector3(1, 0, 0);        
        Vector3 b2 = new Vector3(0, 0, 1);
        //
        GameObject pInt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pInt.name = "pInt";
        pInt.transform.localScale = new Vector3(2, 2, 2);
        pInt.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        pInt.GetComponent<Renderer>().material.color = Color.cyan;
        //
        GameObject linA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linA.name = "linA";
        linA.GetComponent<Renderer>().material.color = Color.blue;
        GameObject linB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linB.name = "linB";
        linB.GetComponent<Renderer>().material.color = Color.red;
        //
        pInt.transform.position = intersect2D(a1, a2, b1, b2);
        adjustLine(linA, a1, a2);
        adjustLine(linB, b1, b2);
    }

    void Update()
    {
        if (ynStep == true) {
            if (Time.realtimeSinceStartup - timeStart > 1)
            {
                timeStart = Time.realtimeSinceStartup;
                UpdateOne();
            }
        } else {
            UpdateOne();
        }
    }

	void UpdateOne () {
        moveBall();
        updateCv();
        movePaddle();
        //
        updateCam();
        frameCount++;
	}

    void updateCv()
    {
        updateLine(camBodyL, camL, rtL, scrL, pixL, linL);        
        updateLine(camBodyR, camR, rtR, scrR, pixR, linR);
        //
        updateBallEst();
        updatePaddleTarget();
    }

    void updateBallEst()
    {
        ballEst.transform.position = intersect2D(camBodyL.transform.position, pixL.transform.position, camBodyR.transform.position, pixR.transform.position);
    }

    void updatePaddleTarget()
    {
        float x = paddle.transform.position.x;
        float y = 0;
        float z = ballEst.transform.position.z;
        paddleTarget.transform.position = new Vector3(x, y, z);
        adjustLine(linTarget, ballEst.transform.position, paddleTarget.transform.position);
    }

    void updateLine(GameObject camBody, Camera cam, RenderTexture rt, GameObject scr, GameObject pix, GameObject lin) {
        int bestN = getBestPixel(rt);
        int i = bestN % pixelRes;
        int j = bestN / pixelRes;
        updatePixel(cam, scr, pix, i, j);
        adjustLine(lin, camBody.transform.position, pix.transform.position);
    }

    int getBestPixel(RenderTexture rt) {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(pixelRes, pixelRes, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, pixelRes, pixelRes), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        Color[] pixels = tex.GetPixels(0, 0, pixelRes, pixelRes);
        int bestN = -1;
        float bestDist = 1000;
        for (int n = 0; n < pixels.Length; n++)
        {
            Color c = pixels[n];
            float distColor = Vector3.Distance(new Vector3(c.r, c.g, c.b), new Vector3(Color.green.r, Color.green.g, Color.green.b));
            if (distColor < bestDist || n == 0)
            {
                bestN = n;
                bestDist = distColor;
            }
        }
        return bestN;
    }

    Vector3 intersect2D(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        float dxa = a2.x - a1.x;
        float dya = a2.z - a1.z;
        float dxb = b2.x - b1.x;
        float dyb = b2.z - b1.z;
        float ma = dya / dxa;
        float mb = dyb / dxb;
        float inta = a1.z - ma * a1.x;
        float intb = b1.z - mb * b1.x;
        float x = (intb - inta) / (ma - mb);
        float y = ma * x + inta;
        return new Vector3(x, 0, y);
    }

    void updatePixel(Camera cam, GameObject scr, GameObject pix, int i, int j)
    {
        float w = scr.transform.localScale.x;
        float h = scr.transform.localScale.y;
        float sw = w / pixelRes;
        float sh = h / pixelRes;
        pix.transform.position = scr.transform.position + scr.transform.right * (i * sw - w / 2);
        pix.transform.position += scr.transform.up * (j * sh - h / 2);
        pix.transform.position = cam.ScreenToWorldPoint(new Vector3(i, j, cam.nearClipPlane));
    }

    void adjustLine(GameObject go, Vector3 p1, Vector3 p2)
    {
        go.transform.position = (p1 + p2) / 2;
        float dist = Vector3.Distance(p1, p2);
        go.transform.localScale = new Vector3(.05f, .05f, dist * 150);
        //go.transform.position = p1;
        go.transform.LookAt(p2);
        //go.transform.position += go.transform.forward * dist / 2;
    }

    void movePaddle() {
        vectPaddle = paddleTarget.transform.position - paddle.transform.position;
        vectPaddle = Vector3.Normalize(vectPaddle) * paddleSpeed;
        paddle.transform.position += vectPaddle;            
    }

    void updateCam() {
        Camera.main.transform.position = ball.transform.position + new Vector3(10, 20, 10);
        Camera.main.transform.LookAt(ball.transform.position);
    }

    void moveBall()
    {
        advanceBallNew();
        checkWalls();
        checkPaddle();
        ball.transform.position = ballNew.transform.position;
    }

    void advanceBallNew()
    {
        ballNew.transform.position = ball.transform.position + Vector3.Normalize(vect) * ballSpeed;
    }

    void checkPaddle() {
        if (ballNew.transform.position.x <= paddle.transform.position.x && ball.transform.position.x > paddle.transform.position.x)
        {
            float dz = paddleTarget.transform.position.z - paddle.transform.position.z;
            if (Mathf.Abs(dz) <= paddleWidth / 2)
            {
                vect.x *= -1;
                advanceBallNew();
                paddle.GetComponent<Renderer>().material.color = Color.red;
            }
        } else {
            paddle.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    void checkWalls()
    {
        if (ballNew.transform.position.x < wallLeft.transform.position.x || ballNew.transform.position.x > wallRight.transform.position.x)
        {
            vect.x *= -1;
            advanceBallNew();
        }
        if (ballNew.transform.position.z < wallBottom.transform.position.z || ballNew.transform.position.z > wallTop.transform.position.z)
        {
            vect.z *= -1;
            advanceBallNew();
        }
    }

    void hideLayer(GameObject go) {
        go.layer = LayerMask.NameToLayer("picturePlane");
    }

    void initRenderTextures() {
        rtL = new RenderTexture(pixelRes, pixelRes, 24, RenderTextureFormat.ARGB32);
        rtR = new RenderTexture(pixelRes, pixelRes, 24, RenderTextureFormat.ARGB32);
    }

    void initMaterials() {
        matL = new Material(Shader.Find("Unlit/Texture"));
        matL.mainTexture = rtL;
        //
        matR = new Material(Shader.Find("Unlit/Texture"));
        matR.mainTexture = rtR;
    }

    void initGOs()
    {
        //
        initRenderTextures();
        initMaterials();
        initWalls();
        //
        vect = new Vector3(.75f, 0, .25f);
        //
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "ball";
        ball.transform.localScale = new Vector3(2, 2, 2);
        ball.transform.position = new Vector3(40, 0, 25);
        ball.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        ball.GetComponent<Renderer>().material.color = Color.green;
        //
        ballEst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballEst.name = "ballEst";
        ballEst.transform.localScale = new Vector3(2, 2, 2);
        ballEst.transform.position = new Vector3(40, 0, 25);
        ballEst.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        ballEst.GetComponent<Renderer>().material.color = Color.blue;
        hideLayer(ballEst);
        //
        ballNew = GameObject.CreatePrimitive(PrimitiveType.Sphere);                
        ballNew.name = "ballNew";
        ballNew.transform.localScale = new Vector3(1, 1, 1);
        ballNew.transform.position = new Vector3(40, 0, 25);
        ballNew.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        ballNew.GetComponent<Renderer>().material.color = Color.black;
        hideLayer(ballNew);
        //
        ll = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ll.name = "ll";
        ll.transform.localScale = new Vector3(.1f, .1f, .1f);
        ll.GetComponent<Renderer>().material.color = Color.cyan;
        hideLayer(ll);
        //
        lr = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lr.name = "lr";
        lr.transform.localScale = new Vector3(.1f, .1f, .1f);
        lr.GetComponent<Renderer>().material.color = Color.cyan;
        hideLayer(lr);
        //
        ul = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ul.name = "ul";
        ul.transform.localScale = new Vector3(.1f, .1f, .1f);
        ul.GetComponent<Renderer>().material.color = Color.cyan;
        hideLayer(ul);
        //
        camBodyL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camBodyL.name = "camBodyL";
        camBodyL.transform.position = new Vector3(headX, 0, headZ + ipd/2);
        camBodyL.transform.localScale = new Vector3(.25f, .25f, .25f);
        camBodyL.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(camBodyL);
        camL = camBodyL.AddComponent<Camera>();
        camL.targetTexture = rtL;
        camL.targetDisplay = 1;
        camL.cullingMask &= ~(1 << LayerMask.NameToLayer("picturePlane"));
        camL.clearFlags = CameraClearFlags.SolidColor;
        camL.nearClipPlane = nearClipPlaneDist;
        camL.fieldOfView = fov;
        scrL = GameObject.CreatePrimitive(PrimitiveType.Quad);
        scrL.name = "scrL";
        scrL.GetComponent<Renderer>().material = matL;

        ll.transform.position = camL.ScreenToWorldPoint(new Vector3(0, 0, camL.nearClipPlane));
        lr.transform.position = camL.ScreenToWorldPoint(new Vector3(pixelRes, 0, camL.nearClipPlane));
        ul.transform.position = camL.ScreenToWorldPoint(new Vector3(0, pixelRes, camL.nearClipPlane));
        scrL.transform.position = camBodyL.transform.position + camBodyL.transform.forward * camL.nearClipPlane;
        scrL.transform.localScale = new Vector3(Vector3.Distance(ll.transform.position, lr.transform.position), Vector3.Distance(ll.transform.position, ul.transform.position), 1);
        scrL.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(scrL);

        //
        camBodyR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camBodyR.name = "camBodyR";
        camBodyR.transform.position = new Vector3(headX, 0, headZ - ipd/2);
        camBodyR.transform.localScale = new Vector3(.25f, .25f, .25f);
        camBodyR.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(camBodyR);
        camR = camBodyR.AddComponent<Camera>();
        camR.targetTexture = rtR;
        camR.targetDisplay = 1;
        camR.cullingMask &= ~(1 << LayerMask.NameToLayer("picturePlane"));
        camR.clearFlags = CameraClearFlags.SolidColor;
        camR.nearClipPlane = nearClipPlaneDist;
        camR.fieldOfView = fov;
        scrR = GameObject.CreatePrimitive(PrimitiveType.Quad);
        scrR.name = "scrR";
        scrR.transform.eulerAngles = new Vector3(0, 90, 0);
        scrR.transform.localScale = new Vector3(5, 5, 5);
        scrR.GetComponent<Renderer>().material = matR;

        ll.transform.position = camR.ScreenToWorldPoint(new Vector3(0, 0, camR.nearClipPlane));
        lr.transform.position = camR.ScreenToWorldPoint(new Vector3(pixelRes, 0, camR.nearClipPlane));
        ul.transform.position = camR.ScreenToWorldPoint(new Vector3(0, pixelRes, camR.nearClipPlane));
        scrR.transform.position = camBodyR.transform.position + camBodyR.transform.forward * camR.nearClipPlane;
        scrR.transform.localScale = new Vector3(Vector3.Distance(ll.transform.position, lr.transform.position), Vector3.Distance(ll.transform.position, ul.transform.position), 1);
        scrR.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(scrR);

        //
        pixL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pixL.name = "pixL";
        pixL.GetComponent<Renderer>().material.color = Color.red;
        float sL = scrL.transform.localScale.x / pixelRes;
        pixL.transform.localScale = new Vector3(sL, sL, sL);
        pixL.transform.position = new Vector3(-5, 0, 25);
        hideLayer(pixL);
        //
        pixR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pixR.name = "pixR";
        pixR.GetComponent<Renderer>().material.color = Color.red;
        float sR = scrR.transform.localScale.x / pixelRes;
        pixR.transform.localScale = new Vector3(sR, sR, sR);
        pixR.transform.position = new Vector3(-5, 0, 25);
        hideLayer(pixR);
        //
        linL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linL.name = "linL";
        hideLayer(linL);
        //
        linR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linR.name = "linR";
        hideLayer(linR);
        //
        linTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linTarget.name = "linTarget";
        linTarget.GetComponent<Renderer>().material.color = Color.magenta;
        hideLayer(linTarget);
        //
        paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paddle.name = "paddle";
        paddle.transform.position = new Vector3(paddleX, 0, 25);
        paddle.transform.localScale = new Vector3(paddleWidth, 2, paddleThickness);
        paddle.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(paddle);
        //
        paddleTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        paddleTarget.name = "paddleTarget";
        paddleTarget.GetComponent<Renderer>().material.color = Color.red;
        paddleTarget.transform.position = paddle.transform.position + paddle.transform.right * 2;
        paddleTarget.transform.localScale = new Vector3(1, 1, 1);
        paddleTarget.transform.eulerAngles = new Vector3(0, 90, 0);
        hideLayer(paddleTarget);
    }

    void initWalls() {
        wallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallRight.transform.position = new Vector3(100, 0, 25);
        wallRight.transform.localScale = new Vector3(.1f, 1, 50);
        //
        wallLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallLeft.transform.position = new Vector3(0, 0, 25);
        wallLeft.transform.localScale = new Vector3(.1f, 1, 50);
        //
        wallTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallTop.transform.position = new Vector3(50, 0, 50);
        wallTop.transform.localScale = new Vector3(100, 1, .1f);
        //
        wallBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallBottom.transform.position = new Vector3(50, 0, 0);
        wallBottom.transform.localScale = new Vector3(100, 1, .1f);
    }

    ///

    //void init()
    //{
    //    ll = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    ll.name = "ll";
    //    ll.transform.localScale = new Vector3(.1f, .1f, .1f);
    //    ll.GetComponent<Renderer>().material.color = Color.cyan;
    //    //
    //    lr = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    lr.name = "lr";
    //    lr.transform.localScale = new Vector3(.1f, .1f, .1f);
    //    lr.GetComponent<Renderer>().material.color = Color.cyan;
    //    //
    //    ul = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    ul.name = "ul";
    //    ul.transform.localScale = new Vector3(.1f, .1f, .1f);
    //    ul.GetComponent<Renderer>().material.color = Color.cyan;
    //    //
    //    vect = new Vector3(0, 0, .5f);
    //    //
    //    lineLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    hideLayer(lineLeft);

    //    lineRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    hideLayer(lineRight);
    //    //
    //    pixelsLeft = new GameObject[pixelRes, pixelRes];
    //    //
    //    pixelRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    pixelRight.GetComponent<Renderer>().material.color = Color.blue;
    //    hideLayer(pixelRight);
    //    //
    //    camBodyLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    camBodyLeft.name = "camBodyLeft";
    //    camBodyLeft.transform.position = new Vector3(5, 0, 25 + 10);
    //    camBodyLeft.transform.localScale = new Vector3(.25f, .25f, .25f);
    //    camBodyLeft.transform.eulerAngles = new Vector3(0, 90, 0);
    //    rtLeft = new RenderTexture(pixelRes, pixelRes, 24, RenderTextureFormat.ARGB32);
    //    camLeft = camBodyLeft.AddComponent<Camera>();
    //    camLeft.targetTexture = rtLeft;
    //    camLeft.targetDisplay = 1;
    //    camLeft.cullingMask &= ~(1 << LayerMask.NameToLayer("picturePlane"));
    //    camLeft.clearFlags = CameraClearFlags.SolidColor;
    //    camLeft.nearClipPlane = 3;
    //    picturePlaneLeft = GameObject.CreatePrimitive(PrimitiveType.Quad);
    //    picturePlaneLeft.name = "picturePlaneLeft";
    //    picturePlaneLeft.transform.position = camBodyLeft.transform.position + camBodyLeft.transform.forward * camLeft.nearClipPlane;
    //    ll.transform.position = camLeft.ScreenToWorldPoint(new Vector3(0, 0, camLeft.nearClipPlane));
    //    lr.transform.position = camLeft.ScreenToWorldPoint(new Vector3(pixelRes, 0, camLeft.nearClipPlane));
    //    ul.transform.position = camLeft.ScreenToWorldPoint(new Vector3(0, pixelRes, camLeft.nearClipPlane));
    //    picturePlaneLeft.transform.localScale = new Vector3(Vector3.Distance(ll.transform.position, lr.transform.position), Vector3.Distance(ll.transform.position, ul.transform.position), 1);
    //    picturePlaneLeft.transform.eulerAngles = new Vector3(0, 90, 0);
    //    picturePlaneLeft.GetComponent<Renderer>().material.mainTexture = rtLeft;
    //    hideLayer(picturePlaneLeft);
    //    //
    //    mat = new Material(Shader.Find("Unlit/Texture"));
    //    mat.mainTexture = rtLeft;
    //    //
    //    quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
    //    quad.name = "quad";
    //    quad.transform.position = new Vector3(-10, 0, 10);
    //    quad.transform.eulerAngles = new Vector3(0, 90, 0);
    //    quad.transform.localScale = new Vector3(5, 5, 5);
    //    quad.GetComponent<Renderer>().material = mat;
    //    //
    //    pixel = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    pixel.name = "pixel";
    //    pixel.GetComponent<Renderer>().material.color = Color.red;
    //    float s = quad.transform.localScale.x / pixelRes;
    //    pixel.transform.localScale = new Vector3(s, s, s);
    //    pixel.transform.position = new Vector3(-5, 0, 25);
    //    hideLayer(pixel);
    //    //
    //    ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    ball.name = "ball";
    //    ball.transform.localScale = new Vector3(2, 2, 2);
    //    ball.transform.position = new Vector3(40, 0, 25);
    //    ball.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
    //    ball.GetComponent<Renderer>().material.color = Color.green;
    //    ballNew = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    //
    //    posInt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    //
    //    initWalls();
    //    //
    //    paddle = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    paddle.transform.position = new Vector3(20, 0, 25);
    //    paddle.transform.localScale = new Vector3(1, 2, 10);
    //    //
    //    paddleTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    paddleTarget.GetComponent<Renderer>().material.color = Color.red;
    //    paddleTarget.transform.position = new Vector3(10, 0, 0);
    //}

    //void check()
    //{
    //    RenderTexture.active = rtLeft;
    //    Texture2D tex = new Texture2D(pixelRes, pixelRes, TextureFormat.RGBA32, false);
    //    tex.ReadPixels(new Rect(0, 0, pixelRes, pixelRes), 0, 0);
    //    tex.Apply();
    //    RenderTexture.active = null;
    //    Color[] pix = tex.GetPixels(0, 0, pixelRes, pixelRes);
    //    int bestN = -1;
    //    float bestDist = 1000;
    //    for (int n = 0; n < pix.Length; n++)
    //    {
    //        Color c = pix[n];
    //        float distColor = Vector3.Distance(new Vector3(c.r, c.g, c.b), new Vector3(Color.green.r, Color.green.g, Color.green.b));
    //        if (distColor < bestDist || n == 0)
    //        {
    //            bestN = n;
    //            bestDist = distColor;
    //        }
    //    }
    //    int i = bestN % pixelRes;
    //    int j = bestN / pixelRes;
    //    updatePixel(camLeft, quad, pixel, i, j);
    //    adjustLine(lineLeft, camBodyLeft.transform.position, pixel.transform.position);
    //}


}
