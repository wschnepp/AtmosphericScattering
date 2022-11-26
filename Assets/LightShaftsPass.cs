using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BII.URP
{
    public class LightShaftsPass : ScriptableRenderPass
    {
        private int _lightShaftsRT1;
        private int _lightShaftsRT2;

        private readonly Material _lightShaftsMaterial;

        public LightShaftsPass()
        {
            var shader = Shader.Find("Hidden/AtmosphericScattering/LightShafts");
            if (shader == null)
                throw new Exception("Critical Error: \"Hidden/AtmosphericScattering/LightShafts\" shader is missing. Make sure it is included in \"Always Included Shaders\" in ProjectSettings/Graphics.");
            _lightShaftsMaterial = new Material(shader);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            Debug.Log("Configure");

            _lightShaftsRT1 = Shader.PropertyToID("_LightShaft1");
            _lightShaftsRT1 = Shader.PropertyToID("_LightShaft2");
            
            
            // _lightShaftsCommandBuffer.GetTemporaryRT(lightShaftsRT1, renderingData.cameraData.camera.pixelWidth, 
            //     renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            // _lightShaftsCommandBuffer.Blit(nullTexture, new RenderTargetIdentifier(lightShaftsRT1), _lightShaftsMaterial, 10);
            //
            //
            // _lightShaftsCommandBuffer.GetTemporaryRT(lightShaftsRT2, renderingData.cameraData.camera.pixelWidth, 
            //     renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            // // horizontal bilateral blur
            // _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(lightShaftsRT1), new RenderTargetIdentifier(lightShaftsRT2), _lightShaftsMaterial, 0);
            // // vertical bilateral blur
            // _lightShaftsCommandBuffer.Blit(new RenderTargetIdentifier(lightShaftsRT2), new RenderTargetIdentifier(lightShaftsRT1), _lightShaftsMaterial, 1);
            cmd.GetTemporaryRT(_lightShaftsRT1, cameraTextureDescriptor.width, cameraTextureDescriptor.height,0, FilterMode.Bilinear, RenderTextureFormat.RHalf);
            cmd.GetTemporaryRT(_lightShaftsRT2, cameraTextureDescriptor.width, cameraTextureDescriptor.height,0, FilterMode.Bilinear, RenderTextureFormat.RHalf);

        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("Light Shafts");

            cmd.Blit(null, _lightShaftsRT1, _lightShaftsMaterial, 10);
            cmd.Blit(_lightShaftsRT1, _lightShaftsRT2, _lightShaftsMaterial, 0);
            // vertical bilateral blur
            cmd.Blit(_lightShaftsRT2, _lightShaftsRT1, _lightShaftsMaterial, 1);
            
            context.ExecuteCommandBuffer(cmd);
        }
    }
}