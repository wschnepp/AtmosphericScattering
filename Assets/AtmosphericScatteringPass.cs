using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BII.URP {
    
    public class AtmosphericScatteringPass : ScriptableRenderPass {

        private const string ProfilerTag = "AtmosphericScatteringPass";
        
        public ScriptableRenderer renderer { get; set; }

        private int tempTexture;

        private Material _material;
        private Material _lightShaftsMaterial;

        private CommandBuffer _lightShaftsCommandBuffer;
        private CommandBuffer _cascadeShadowCommandBuffer;

        private bool lightShaftsInit = false;
        
        private AtmosphericScatteringFeature.AtmosphericScatteringFeatureSettings _settings;
        
        public AtmosphericScatteringPass(AtmosphericScatteringFeature.AtmosphericScatteringFeatureSettings settings) {
            /*Shader shader = Shader.Find("Hidden/AtmosphericScattering");
            if (shader == null)
                throw new Exception(
                    "Critical Error: \"Hidden/AtmosphericScattering\" shader is missing. Make sure it is included in \"Always Included Shaders\" in ProjectSettings/Graphics."
                );
            */
            _settings = settings; //new Material(shader);
        }

        
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            Debug.Log("Configure");
            tempTexture = Shader.PropertyToID("_AtmosphericScatteringTarget");
            //tempTexture.Create();
            // create a temporary render texture that matches the camera
            cmd.GetTemporaryRT(tempTexture, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            //renderingData.
            //Debug.Log($"Executing pass on : {renderer.cameraColorTargetHandle.name}");

            //Debug.Log($"Execute {renderingData.}");
            if (!Application.isPlaying)
                return;
            
            _material = AtmosphericScattering._material;
            _lightShaftsMaterial = AtmosphericScattering._lightShaftMaterial;
            
            
            if (_material == null) {
                Debug.Log("Material not set, skipping");
                return;
            }
            if (!_settings.RenderAtmosphericFog) {
                //cmd.Blit(renderer.cameraColorTargetHandle, tempTexture);
                return;
            }

            if (!initializeLightShafts(context, ref renderingData))
            {
                lightShaftsInit = false;
                return;
            }
            else
            {
                context.ExecuteCommandBuffer(_cascadeShadowCommandBuffer);
                context.ExecuteCommandBuffer(_lightShaftsCommandBuffer);
                
            }
            
            var cmd = CommandBufferPool.Get(ProfilerTag);
            
            Blit(cmd,renderer.cameraColorTargetHandle, tempTexture);
            Blit(cmd,tempTexture, tempTexture, _material, 3);
            Blit(cmd,tempTexture, tempTexture, _material, 4);
            Blit(cmd, tempTexture, renderer.cameraColorTargetHandle);
            context.ExecuteCommandBuffer(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(tempTexture);
        }

        private bool initializeLightShafts(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (lightShaftsInit || _lightShaftsMaterial == null) return false;
            if (_cascadeShadowCommandBuffer == null)
            {
                _cascadeShadowCommandBuffer = new CommandBuffer();
                _cascadeShadowCommandBuffer.name = "CascadeShadowCommandBuffer";
                _cascadeShadowCommandBuffer.SetGlobalTexture("_CascadeShadowMapTexture", new UnityEngine.Rendering.RenderTargetIdentifier(UnityEngine.Rendering.BuiltinRenderTextureType.CurrentActive));
            }

            if (_lightShaftsCommandBuffer == null)
            {
                _lightShaftsCommandBuffer = new CommandBuffer();
                _lightShaftsCommandBuffer.name = "LightShaftsCommandBuffer";
            }
            else
            {
                _lightShaftsCommandBuffer.Clear();
            }

            int lightShaftsRT1 = Shader.PropertyToID("_LightShaft1");
            int lightShaftsRT2 = Shader.PropertyToID("_LightShaft2");
            int halfDepthBuffer = Shader.PropertyToID("_HalfResDepthBuffer");
            int halfShaftsRT1 = Shader.PropertyToID("_HalfResColor");
            int halfShaftsRT2 = Shader.PropertyToID("_HalfResColorTemp");

            Texture nullTexture = null;
            // if (LightShaftQuality == LightShaftsQuality.High)
            // {
                _lightShaftsCommandBuffer.GetTemporaryRT(lightShaftsRT1, renderingData.cameraData.camera.pixelWidth, 
                    renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
                _lightShaftsCommandBuffer.Blit(nullTexture, new RenderTargetIdentifier(lightShaftsRT1), _lightShaftsMaterial, 10);

                _lightShaftsCommandBuffer.GetTemporaryRT(lightShaftsRT2, renderingData.cameraData.camera.pixelWidth, 
                    renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
                // horizontal bilateral blur
                _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(lightShaftsRT1), new RenderTargetIdentifier(lightShaftsRT2), _lightShaftsMaterial, 0);
                // vertical bilateral blur
                _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(lightShaftsRT2), new RenderTargetIdentifier(lightShaftsRT1), _lightShaftsMaterial, 1);
            // }
            // else if (LightShaftQuality == LightShaftsQuality.Medium)
            // {
            //     _lightShaftsCommandBuffer.GetTemporaryRT(lightShaftsRT1, _camera.pixelWidth, _camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            //     _lightShaftsCommandBuffer.GetTemporaryRT(halfDepthBuffer, _camera.pixelWidth / 2, _camera.pixelHeight / 2, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            //     _lightShaftsCommandBuffer.GetTemporaryRT(halfShaftsRT1, _camera.pixelWidth / 2, _camera.pixelHeight / 2, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            //     _lightShaftsCommandBuffer.GetTemporaryRT(halfShaftsRT2, _camera.pixelWidth / 2, _camera.pixelHeight / 2, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            //     
            //     // down sample depth to half res
            //     _lightShaftsCommandBuffer.Blit(nullTexture, new RenderTargetIdentifier(halfDepthBuffer), _lightShaftMaterial, 4);
            //     _lightShaftsCommandBuffer.Blit(nullTexture, new RenderTargetIdentifier(halfShaftsRT1), _lightShaftMaterial, 10);
            //
            //     // horizontal bilateral blur at full res
            //     _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(halfShaftsRT1), new RenderTargetIdentifier(halfShaftsRT2), _lightShaftMaterial, 2);
            //     // vertical bilateral blur at full res
            //     _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(halfShaftsRT2), new RenderTargetIdentifier(halfShaftsRT1), _lightShaftMaterial, 3);
            //
            //     // upscale to full res
            //     _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(halfShaftsRT1), new RenderTargetIdentifier(lightShaftsRT1), _lightShaftMaterial, 5);
            // }

            lightShaftsInit = true;
            return lightShaftsInit;
        }
    }
}
