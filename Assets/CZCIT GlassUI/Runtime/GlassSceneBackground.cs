// 为月球场景准备玻璃背景纹理，渲染资源跟随组件启停管理。
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace CZCIT.GlassUI
{
    // Copy the current camera's opaque scene once, without another Camera.Render.
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public class GlassSceneBackground : MonoBehaviour
    {
        Camera source;
        CommandBuffer commands;
        RenderTexture background;
        RenderTextureDescriptor previous;

        void OnEnable()
        {
            source = GetComponent<Camera>();
        }

        void OnPreRender()
        {
            // Multi-pass renders one ordinary 2D target per eye. The XR descriptor
            // can still describe an array; binding it to sampler2D loses an eye.
            bool singlePass = source.stereoEnabled && XRSettings.stereoRenderingMode != XRSettings.StereoRenderingMode.MultiPass;
            var descriptor = singlePass ? XRSettings.eyeTextureDesc :
                new RenderTextureDescriptor(Mathf.Max(1, source.pixelWidth), Mathf.Max(1, source.pixelHeight),
                    source.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32, 0);
            descriptor.width = Mathf.Max(1, descriptor.width / 2);
            descriptor.height = Mathf.Max(1, descriptor.height / 2);
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;
            descriptor.bindMS = false;
            descriptor.useDynamicScale = false;
            if (background && previous.width == descriptor.width && previous.height == descriptor.height &&
                previous.dimension == descriptor.dimension && previous.volumeDepth == descriptor.volumeDepth &&
                previous.colorFormat == descriptor.colorFormat)
                return;
            Release();
            previous = descriptor;
            background = new RenderTexture(descriptor) { name = "GlassUIOpaqueScene", filterMode = FilterMode.Bilinear };
            background.Create();
            commands = new CommandBuffer { name = "Glass UI background copy" };
            if (descriptor.dimension == TextureDimension.Tex2DArray)
            {
                for (int eye = 0; eye < descriptor.volumeDepth; eye++)
                    commands.Blit(BuiltinRenderTextureType.CameraTarget, background, Vector2.one, Vector2.zero, eye, eye);
            }
            else
                commands.Blit(BuiltinRenderTextureType.CameraTarget, background);
            commands.SetGlobalTexture("_CZCITGlassScene", background);
            commands.SetGlobalVector(
                "_CZCITGlassScene_TexelSize",
                new Vector4(1f / descriptor.width, 1f / descriptor.height, descriptor.width, descriptor.height));
            commands.SetGlobalFloat("_CZCITGlassSceneReady", 1);
            source.AddCommandBuffer(CameraEvent.AfterSkybox, commands);
        }

        void OnDisable()
        {
            Release();
            Shader.SetGlobalFloat("_CZCITGlassSceneReady", 0);
        }

        void Release()
        {
            if (commands != null)
            {
                if (source)
                    source.RemoveCommandBuffer(CameraEvent.AfterSkybox, commands);
                commands.Release();
                commands = null;
            }
            if (background)
            {
                background.Release();
                if (Application.isPlaying)
                    Destroy(background);
                else
                    DestroyImmediate(background);
                background = null;
            }
        }
    }
}
