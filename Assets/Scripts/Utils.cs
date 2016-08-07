﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BoundsTest
{
    center,
    onScreen,
    offScreen
}

public class Utils : MonoBehaviour {

    public static Bounds _camBounds;

    // Merges two Bounds //

    public static Bounds BoundsUnion(Bounds b0, Bounds b1) 
    {
        if(b0.size == Vector3.zero && b1.size != Vector3.zero)
        {
            return b1;
        }
        else if(b0.size != Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }
        else if (b0.size == Vector3.zero && b1.size == Vector3.zero)
        {
            return b0;
        }

        b0.Encapsulate(b1.min);
        b0.Encapsulate(b1.max);
        return b0;
    }

    // Combine all properties of GameObject, including its children. Recursive. //

    public static Bounds CombineBoundsOfChildren(GameObject go)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero);

        if(go.GetComponent<Renderer>() != null)
        {
            b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
        }

        if (go.GetComponent<Collider>() != null)
        {
            b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
        }

        foreach(Transform t in go.transform)
        {
            b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
        }

        return b;
    }

    public static Bounds camBounds
    {
        get
        {
            if(_camBounds.size == Vector3.zero)
            {
                SetCameraBounds();
            }

            return _camBounds;
        }
    }

    public static void SetCameraBounds(Camera cam = null)
    {
        if(cam == null) cam = Camera.main;

        Vector3 topLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);

        Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
        Vector3 boundBRF = cam.ScreenToWorldPoint(bottomRight);

        boundTLN.z += cam.nearClipPlane;
        boundBRF.z += cam.farClipPlane;

        Vector3 center = (boundTLN + boundBRF) / 2f;

        _camBounds = new Bounds(center, Vector3.zero);
        _camBounds.Encapsulate(boundTLN);
        _camBounds.Encapsulate(boundBRF);
    }

    public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest test = BoundsTest.center)
    {
        return BoundsInBoundsCheck(camBounds, bnd, test);
    }

    public static Vector3 BoundsInBoundsCheck(Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen)
    {
        Vector3 pos = lilB.center;
        Vector3 off = Vector3.zero;

        switch (test)
        {
            case BoundsTest.center:

                if (bigB.Contains(pos))
                {
                    return Vector3.zero;
                }

                // X offset //

                if (pos.x > bigB.max.x)
                {
                    off.x = pos.x - bigB.max.x;
                }
                else if (pos.x < bigB.min.x)
                {
                    off.x = pos.x - bigB.min.x;
                }

                // Y offset //

                if (pos.y > bigB.max.y)
                {
                    off.y = pos.y - bigB.max.y;
                }
                else if (pos.y < bigB.min.y)
                {
                    off.y = pos.y - bigB.min.y;
                }

                // Z offset //

                if (pos.z > bigB.max.z)
                {
                    off.z = pos.z - bigB.max.z;
                }
                else if (pos.z < bigB.min.z)
                {
                    off.z = pos.z - bigB.min.z;
                }

                return off;

            case BoundsTest.onScreen:

                if (bigB.Contains(lilB.min) && bigB.Contains(lilB.max))
                {
                    return Vector3.zero;
                }

                // X offset //

                if (lilB.max.x > bigB.max.x)
                {
                    off.x = lilB.max.x - bigB.max.x;
                }
                else if (lilB.min.x < bigB.min.x)
                {
                    off.x = lilB.min.x - bigB.min.x;
                }

                // Y offset //

                if (lilB.max.y > bigB.max.y)
                {
                    off.y = lilB.max.y - bigB.max.y;
                }
                else if (lilB.min.y < bigB.min.y)
                {
                    off.y = lilB.min.y - bigB.min.y;
                }

                // Z offset //

                if (lilB.max.z > bigB.max.z)
                {
                    off.z = lilB.max.z - bigB.max.z;
                }
                else if (lilB.min.z < bigB.min.z)
                {
                    off.z = lilB.min.z - bigB.min.z;
                }

                return off;

            case BoundsTest.offScreen:

                bool cMin = bigB.Contains(lilB.min);
                bool cMax = bigB.Contains(lilB.max);

                if(cMin || cMax)
                {
                    return Vector3.zero;
                }

                // X Offset //

                if(lilB.min.x > bigB.max.x)
                {
                    off.x = lilB.min.x - bigB.max.x;
                }
                else if(lilB.max.x < bigB.min.x)
                {
                    off.x = lilB.max.x - bigB.min.x;
                }

                // Y Offset //

                if (lilB.min.y > bigB.max.y)
                {
                    off.y = lilB.min.y - bigB.max.y;
                }
                else if (lilB.max.y < bigB.min.y)
                {
                    off.y = lilB.max.y - bigB.min.y;
                }

                // Z Offset //

                if (lilB.min.z > bigB.max.z)
                {
                    off.z = lilB.min.z - bigB.max.z;
                }
                else if (lilB.max.z < bigB.min.z)
                {
                    off.z = lilB.max.z - bigB.min.z;
                }

                return off;
        }

        return Vector3.zero;
    }

    public static GameObject FindTaggedParent(GameObject go)
    {
        if(go.tag != "Untagged")
        {
            return go;
        }

        if(go.transform.parent == null) 
        {
            return null;
        }

        return FindTaggedParent(go.transform.parent.gameObject);
    }

    public static GameObject FindTaggedParent(Transform t)
    {
        return FindTaggedParent(t.gameObject);
    }

    // Material Functions //

    public static Material[] GetAllMaterials(GameObject go)
    {
        List<Material> mats = new List<Material>();

        if(go.GetComponent<Renderer>() != null)
        {
            mats.Add(go.GetComponent<Renderer>().material);
        }

        foreach(Transform t in go.transform)
        {
            mats.AddRange(GetAllMaterials(t.gameObject));
        }

        return mats.ToArray();
    }

    // Linear Interpolation //

    public static Vector3 Lerp(Vector3 vFrom, Vector3 vTo, float u)
    {
        Vector3 res = (1 - u) * vFrom + u * vTo;
        return res;
    }

    public static Vector2 Lerp(Vector2 vFrom, Vector2 vTo, float u)
    {
        Vector2 res = (1 - u) * vFrom + u * vTo;
        return res;
    }

    public static float Lerp(float vFrom, float vTo, float u)
    {
        float res = (1 - u) * vFrom + u * vTo;
        return res; 
    }

    // Bezier Curve, Vector3 //

    public static Vector3 Bezier(float u, List<Vector3> vList)
    {
        if(vList.Count == 1)
        {
            return vList[0];
        }

        List<Vector3> vListR = vList.GetRange(1, vList.Count - 1);
        List<Vector3> vListL = vList.GetRange(0, vList.Count - 1);

        Vector3 res = Lerp(Bezier(u, vListL), Bezier(u, vListR), u);

        return res;
    }

    public static Vector3 Bezier(float u, params Vector3[] vecs)
    {
        return Bezier(u, new List<Vector3>(vecs));
    }

    // Bezier Curve, Vector2 //

    public static Vector2 Bezier(float u, List<Vector2> vList)
    {
        if(vList.Count == 1)
        {
            return vList[0];
        }

        List<Vector2> vListR = vList.GetRange(1, vList.Count - 1);
        List<Vector2> vListL = vList.GetRange(0, vList.Count - 1);

        Vector2 res = Lerp(Bezier(u, vListL), Bezier(u, vListR), u);

        return res;
    }

    public static Vector2 Bezier(float u, params Vector2[] vecs)
    {
        return Bezier(u, new List<Vector2>(vecs));
    }

    // Bezier Curve, Float //

    public static float Bezier(float u, List<float> vList)
    {
        if (vList.Count == 1)
        {
            return vList[0];
        }

        List<float> vListR = vList.GetRange(1, vList.Count - 1);
        List<float> vListL = vList.GetRange(0, vList.Count - 1);

        float res = Lerp(Bezier(u, vListL), Bezier(u, vListR), u);

        return res;
    }

    public static float Bezier(float u, params float[] vecs)
    {
        return Bezier(u, new List<float>(vecs));
    }

    // Trace & Logging functions //

    public static void tr(params object[] objs)
    {
        string s = objs[0].ToString();
        for(int i = 1; i < objs.Length; i++)
        {
            s += "\t" + objs[i].ToString();
        }

        print(s);
    }

    // Math Functions //

    public static float RoundToPlaces(float f, int places = 2)
    {
        float mult = Mathf.Pow(10, places);

        f *= mult;
        f = Mathf.Round(f);
        f /= mult;

        return f;
    }

    public static string AddCommasToNumber(float f, int places = 2)
    {
        int n = Mathf.RoundToInt(f);

        f -= n;
        f = RoundToPlaces(f, places);

        string str = AddCommasToNumber(n);

        str += "," + (f * Mathf.Pow(10, places));

        return str; 
    }

    public static string AddCommasToNumber(int n)
    {
        int rem;
        int div;

        string res = "";
        string rems;

        while (n > 0)
        {
            rem = n % 1000;
            div = n / 1000;

            rems = rem.ToString();

            while (div > 0 && rems.Length < 3)
            {
                rems = "0" + rems;
            }

            if (res == "")
            {
                res = rems;
            }
            else
            {
                res = rems + "," + res.ToString();
            }

            n = div;
        }

        if(res == "")
        {
            res = "0";
        }

        return res; 
    }

    // Easing Classes //

    [System.Serializable]
    public class EasingCachedCurve
    {
        public List<string> curves = new List<string>();
        public List<float> mods = new List<float>();
    }

    public class Easing
    {
        static public string Linear = ",Linear|";
        static public string In = ",In|";
        static public string Out = ",Out|";
        static public string InOut = ",InOut|";
        static public string Sin = ",Sin|";
        static public string SinIn = ",SinIn|";
        static public string SinOut = ",SinOut|";

        static public Dictionary<string, EasingCachedCurve> cache;

        static public float Ease(float u, params string[] curveparams)
        {
            if(cache == null)
            {
                cache = new Dictionary<string, EasingCachedCurve>();
            }

            float u2 = u;

            foreach(string curve in curveparams)
            {
                if(!cache.ContainsKey(curve))
                {
                    EaseParse(curve);
                }

                u2 = EaseP(u2, cache[curve]);
            }

            return u2;
        }

        static private void EaseParse(string curveIn)
        {
            EasingCachedCurve ecc = new EasingCachedCurve();

            string[] curves = curveIn.Split(',');

            foreach(string curve in curves)
            {
                if(curve == "")
                {
                    continue;
                }

                string[] curveA = curve.Split('|');

                ecc.curves.Add(curveA[0]);

                if(curveA.Length == 1 || curveA[1] == "")
                {
                    ecc.mods.Add(float.NaN);
                }
                else
                {
                    float parseRes;

                    if(float.TryParse(curveA[1], out parseRes))
                    {
                        ecc.mods.Add(parseRes);
                    }
                    else
                    {
                        ecc.mods.Add(float.NaN);
                    }
                }
            }

            cache.Add(curveIn, ecc);
        }
        
        public static float Ease(float u, string curve, float mod)
        {
            return EaseP(u, curve, mod);
        }

        private static float EaseP(float u, EasingCachedCurve ec)
        {
            float u2 = u;
            
            for(int i = 0; i < ec.curves.Count; i++)
            {
                u2 = EaseP(u2, ec.curves[i], ec.mods[i]);
            }

            return u2;
        }

        static private float EaseP(float u, string curve, float mod)
        {
            float u2 = 2;

            switch (curve)
            {
                case "In":

                    if (float.IsNaN(mod))
                    {
                        mod = 2;
                    }

                    u2 = Mathf.Pow(u, mod);

                    break;

                case "Out":

                    if (float.IsNaN(mod))
                    {
                        mod = 2;
                    }

                    u2 = 1 - Mathf.Pow(1 - u, mod);

                    break;

                case "InOut":

                    if (float.IsNaN(mod))
                    {
                        mod = 2;
                    }

                    if (u <= 0.5f)
                    {
                        u2 = 0.5f * Mathf.Pow(u * 2, mod);
                    }
                    else
                    {
                        u2 = 0.5f + 0.5f * (1 - Mathf.Pow(1 - (2 * (u - 0.5f)), mod));
                    }

                    break;

                case "Sin":

                    if (float.IsNaN(mod))
                    {
                        mod = 0.15f;
                    }

                    u2 = u + mod * Mathf.Sin(2 * Mathf.PI * u);

                    break;

                case "SinIn":

                    u2 = 1 - Mathf.Cos(u * Mathf.PI * 0.5f);

                    break;

                case "SinOut":

                    u2 = 1 - Mathf.Sin(u * Mathf.PI * 0.5f);

                    break;

                case "Linear":
                default:
                    break;
            }

            return u2;
        }
    }
}