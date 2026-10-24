using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MazeOfEgos.Runtime
{
    public static class GraphicsProfile
    {
        private static UniversalRenderPipelineAsset runtime;
        public static void Apply(bool economy)
        {
            if (!runtime)
            {
                runtime = Object.Instantiate((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);
                GraphicsSettings.defaultRenderPipeline = runtime; QualitySettings.renderPipeline = runtime;
            }
            runtime.renderScale = economy ? .75f : 1;
            runtime.shadowDistance = economy ? 12 : 25;
            runtime.msaaSampleCount = economy ? 1 : 2;
        }
    }
}
