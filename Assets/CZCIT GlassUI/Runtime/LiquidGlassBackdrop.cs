// 在主视角渲染前捕获双眼背景，为玻璃模糊提供与当前追踪姿态匹配的画面。
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace CZCIT.GlassUI
{
    public class LiquidGlassBackdrop : MonoBehaviour
    {
        public Camera viewer;
        [Range(256, 1024)] public int resolution = 512;
        public bool captureEveryFrame = true;
        [Range(5, 30)] public int refreshRate = 12;
        Camera capture;
        RenderTexture left;
        RenderTexture right;
        float nextCapture;
        Canvas canvas;
        Vector3 lastPosition;
        Quaternion lastRotation;
        bool needsCapture = true;
        public int LastCaptureFrame { get; private set; } = -1;
        readonly Vector3[] corners = new Vector3[4];
        readonly Plane[] frustum = new Plane[6];
        readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();

        public void CapturePreview(Camera camera)
        {
            viewer = camera;
            if (!capture)
                OnEnable();
            nextCapture = 0;
            CaptureBackground(force: true);
        }

        public void ReleasePreview()
        {
            OnDisable();
            viewer = null;
        }

        void OnEnable()
        {
            canvas = GetComponent<Canvas>();
            needsCapture = true;
            Camera.onPreCull -= BeforeCamera;
            Camera.onPreCull += BeforeCamera;
            Camera.onPreRender -= BeforeRender;
            Camera.onPreRender += BeforeRender;
            if (capture)
                return;
            var go = new GameObject("GlassBackgroundCapture") { hideFlags = HideFlags.HideAndDontSave };
            capture = go.AddComponent<Camera>();
            capture.enabled = false;
            capture.stereoTargetEye = StereoTargetEyeMask.None;
            left = MakeTexture("GlassLeft");
            right = MakeTexture("GlassRight");
        }

        RenderTexture MakeTexture(string title)
        {
            var rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32) { name = title, filterMode = FilterMode.Bilinear };
            rt.Create();
            return rt;
        }

        void LateUpdate()
        {
            if (!viewer)
                viewer = Camera.main;
        }

        void BeforeCamera(Camera camera)
        {
            if (!viewer)
                viewer = Camera.main;
            // Capture after XR's final pose update, immediately before this
            // viewer renders. Capture cameras cannot recurse into this path.
            if (camera == viewer)
                CaptureBackground();
        }

        void BeforeRender(Camera camera)
        {
            if (camera == viewer)
                Shader.SetGlobalFloat("_LiquidGlassActiveEye", camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right ? 1 : 0);
        }

        public void InvalidateCapture() => needsCapture = true;

        void CaptureBackground(bool force = false)
        {
            if (CZCIT.GlassUI.GlassRenderingBudget.SuspendCaptures)
                return;
            if (!viewer)
                viewer = Camera.main;
            if (!viewer)
                return;
            if (!force && Application.isPlaying && canvas && !canvas.isActiveAndEnabled)
            {
                needsCapture = true;
                return;
            }
            var pose = viewer.transform;
            if (!force && !captureEveryFrame && !needsCapture && Time.unscaledTime < nextCapture &&
                (pose.position - lastPosition).sqrMagnitude < .0001f &&
                Quaternion.Angle(pose.rotation, lastRotation) < .65f)
                return;
            Matrix4x4 leftView = viewer.worldToCameraMatrix, leftProjection = viewer.projectionMatrix;
            Matrix4x4 rightView = leftView, rightProjection = leftProjection;
            if (viewer.stereoEnabled && (!GetEyeMatrices(0, out leftView, out leftProjection) || !GetEyeMatrices(1, out rightView, out rightProjection)))
                return;
            if (Application.isPlaying && !VisibleInEitherEye(leftView, leftProjection, rightView, rightProjection))
                return;
            capture.clearFlags = CameraClearFlags.Skybox;
            // 排除 UI 层，避免背景捕获把玻璃面板自身再绘制进背景纹理。
            capture.cullingMask = viewer.cullingMask & ~(1 << 5);
            capture.nearClipPlane = viewer.nearClipPlane;
            capture.farClipPlane = viewer.farClipPlane;
            capture.allowHDR = false;
            capture.allowMSAA = false;
            Capture(leftView, leftProjection, left);
            if (viewer.stereoEnabled)
                Capture(rightView, rightProjection, right);
            Shader.SetGlobalTexture("_LiquidGlassLeft", left);
            Shader.SetGlobalTexture("_LiquidGlassRight", viewer.stereoEnabled ? right : left);
            Shader.SetGlobalVector("_LiquidGlassTexel", new Vector4(1f / resolution, 1f / resolution, 0, 0));
            Shader.SetGlobalFloat("_LiquidGlassReady", 1);
            LastCaptureFrame = Time.frameCount;
            lastPosition = pose.position;
            lastRotation = pose.rotation;
            nextCapture = Time.unscaledTime + 1f / Mathf.Max(1, refreshRate);
            needsCapture = false;
        }

        bool VisibleInEitherEye(Matrix4x4 lv, Matrix4x4 lp, Matrix4x4 rv, Matrix4x4 rp)
        {
            var rect = transform as RectTransform;
            if (!rect)
                return true;
            rect.GetWorldCorners(corners);
            var bounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < 4; i++)
                bounds.Encapsulate(corners[i]);
            // Conservative margin includes the curved mesh and child controls.
            bounds.Expand(2f);
            GeometryUtility.CalculateFrustumPlanes(lp * lv, frustum);
            if (GeometryUtility.TestPlanesAABB(frustum, bounds))
                return true;
            if (!viewer.stereoEnabled)
                return false;
            GeometryUtility.CalculateFrustumPlanes(rp * rv, frustum);
            return GeometryUtility.TestPlanesAABB(frustum, bounds);
        }

        void Capture(Matrix4x4 view, Matrix4x4 projection, RenderTexture target)
        {
            var world = view.inverse;
            capture.transform.SetPositionAndRotation(world.GetColumn(3), Quaternion.LookRotation(-world.GetColumn(2), world.GetColumn(1)));
            capture.worldToCameraMatrix = view;
            capture.projectionMatrix = projection;
            capture.targetTexture = target;
            capture.Render();
            capture.targetTexture = null;
        }

        bool GetEyeMatrices(int eye, out Matrix4x4 view, out Matrix4x4 projection)
        {
            view = projection = Matrix4x4.identity;
            SubsystemManager.GetInstances(displays);
            foreach (var display in displays)
            {
                if (!display.running)
                    continue;
                int index = 0;
                for (int p = 0; p < display.GetRenderPassCount(); p++)
                {
                    display.GetRenderPass(p, out var pass);
                    for (int e = 0; e < pass.GetRenderParameterCount(); e++, index++)
                    {
                        if (index != eye)
                            continue;
                        pass.GetRenderParameter(viewer, e, out var parameter);
                        view = parameter.view;
                        projection = parameter.projection;
                        return true;
                    }
                }
            }
            return false;
        }

        void OnDisable()
        {
            Camera.onPreCull -= BeforeCamera;
            Camera.onPreRender -= BeforeRender;
            Shader.SetGlobalFloat("_LiquidGlassReady", 0);
            if (capture)
                Release(capture.gameObject);
            if (left)
            {
                left.Release();
                Release(left);
            }
            if (right)
            {
                right.Release();
                Release(right);
            }
            capture = null;
            left = null;
            right = null;
        }

        void Release(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }
    }
}
