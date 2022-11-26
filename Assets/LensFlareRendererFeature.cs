using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BII.URP
{
    public class LensFlareRendererFeature : ScriptableRendererFeature
    {
        private class LensFlarePass : ScriptableRenderPass
        {
            private readonly Material _material;
            private readonly Mesh _mesh;

            public LensFlarePass(Material material, Mesh mesh)
            {
                _material = material;
                _mesh = mesh;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Lens Flares");
                // Get the Camera data from the renderingData argument.
                Camera camera = renderingData.cameraData.camera;
                // Set the projection matrix so that Unity draws the quad in screen space
                                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                // Add the scale variable, use the Camera aspect ratio for the y coordinate
                                Vector3 scale = new Vector3(1, camera.aspect, 1);
                // Draw a quad for each Light, at the screen space position of the Light.
                foreach (VisibleLight visibleLight in renderingData.lightData.visibleLights)
                {
                    Light light = visibleLight.light;
                    // Convert the position of each Light from world to viewport point.
                    Vector3 position =
                        camera.WorldToViewportPoint(light.transform.position) * 2 - Vector3.one;
                    // Set the z coordinate of the quads to 0 so that Uniy draws them on the same plane.
                    position.z = 0;
                    // Change the Matrix4x4 argument in the cmd.DrawMesh method to use the position and
                    // the scale variables.
                    cmd.DrawMesh(_mesh, Matrix4x4.TRS(position, Quaternion.identity, scale),
                        _material, 0, 0);
                }
                
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Release();
            }
        }
        
        private LensFlarePass pass;
        [SerializeField]
        private Material _material;
        [SerializeField]
        private Mesh _mesh;
        
        public override void Create()
        {
            pass = new LensFlarePass(_material, _mesh);
            pass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(_material != null && _mesh != null)
                renderer.EnqueuePass(pass);
        }
    }
}