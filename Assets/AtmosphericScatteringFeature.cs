using BII.URP;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AtmosphericScatteringFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public struct AtmosphericScatteringFeatureSettings
    {
        // we're free to put whatever we want here, public fields will be exposed in the inspector
        public bool IsEnabled;
        public RenderPassEvent WhenToInsert;

        public bool RenderAtmosphericFog;
    }

    // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
    public AtmosphericScatteringFeatureSettings settings = new AtmosphericScatteringFeatureSettings();

    RenderTargetHandle renderTextureHandle;
    AtmosphericScatteringPass scatteringPass;

    public override void Create()
    {
        /*settings.MaterialToBlit = null;
        settings.IsEnabled = true;
        settings.WhenToInsert = RenderPassEvent.AfterRendering;
        settings.RenderAtmosphericFog = false;
        */
        
        scatteringPass = new AtmosphericScatteringPass(settings);
    }
  
    // called every frame once per camera
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.IsEnabled)
        {
            // we can do nothing this frame if we want
            return;
        }

        scatteringPass.renderer = renderer;
        
        // Gather up and pass any extra information our pass will need.
        // In this case we're getting the camera's color buffer target
        //var cameraColorTargetIdent = renderer.cameraColorTarget;
        //scatteringPass.Setup(cameraColorTargetIdent);

        // Ask the renderer to add our pass.
        // Could queue up multiple passes and/or pick passes to use
        renderer.EnqueuePass(scatteringPass);
    }
}