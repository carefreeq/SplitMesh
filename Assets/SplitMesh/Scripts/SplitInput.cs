using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SplitMesh
{
    /// <summary>
    /// 输入类
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SplitInput : MonoBehaviour
    {
        [Range(0.01f, 0.4f)]
        public float raySpace = 0.2f;
        Vector3 from, to, size;
        Camera cam;
        Plane plane;
        List<SplitObject> splits = new List<SplitObject>();
        void Start()
        {
            if (!cam)
                cam = GetComponent<Camera>();
        }
        void Update()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_WEBGL
            if (Input.GetMouseButtonDown(0))
            {
                from = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                to = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                SetSplitObject(from, to);
            }
#elif UNITY_ANDROID || UNITY_IPHONE
            for (int i = 0; i < Input.touchCount; i++)
            {
                if(Input.GetTouch(i).phase== TouchPhase.Began)
                    from = Input.mousePosition;
                if(Input.GetTouch(i).phase == TouchPhase.Moved)
                    to = Input.mousePosition;
                if (Input.GetTouch(i).phase == TouchPhase.Ended)
                    SetSplitObject(from, to);
                
            }
#endif
        }

        void SetSplitObject(Vector3 from, Vector3 to)
        {
            splits.Clear();
            float near = cam.nearClipPlane;
            Vector3 line = from - to;
            float rayLength = new Vector2(line.x / size.x, line.y / size.y).magnitude;
            line = cam.ScreenToWorldPoint(new Vector3(this.to.x, this.to.y, near)) - cam.ScreenToWorldPoint(new Vector3(this.from.x, this.from.y, near));
            if (raySpace > rayLength)
                return;
            for (float i = 0; i <= rayLength; i += raySpace)
            {
                Ray ray = cam.ScreenPointToRay(Vector3.Lerp(from, to, i / rayLength));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    SplitObject split = hit.transform.GetComponent<SplitObject>();
                    if (split)
                    {
                        if (!splits.Contains(split))
                        {
                            plane = new Plane(Vector3.Cross(line, ray.direction).normalized, hit.point);
                            hit.transform.GetComponent<SplitObject>().Split(plane);
                            splits.Add(split);
                        }
                    }
                }
            }
        }
        void OnPostRender()
        {
            GLLine(from, to);
        }
        void GLLine(Vector2 from, Vector2 to)
        {
            size = new Vector2(Screen.width, Screen.height);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            GL.Vertex3(from.x / size.x, from.y / size.y, cam.nearClipPlane);
            GL.Vertex3(to.x / size.x, to.y / size.y, cam.nearClipPlane);
            GL.End();
            GL.PopMatrix();
        }
    }
}