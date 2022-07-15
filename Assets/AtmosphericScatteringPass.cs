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

            Debug.Log("Execute");
            if (!Application.isPlaying)
                return;
            
            _material = AtmosphericScattering._material;
            if (_material == null) {
                Debug.Log("Material not set, skipping");
                return;
            }
            if (!_settings.RenderAtmosphericFog) {
                //cmd.Blit(renderer.cameraColorTargetHandle, tempTexture);
                return;
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
    }
}
