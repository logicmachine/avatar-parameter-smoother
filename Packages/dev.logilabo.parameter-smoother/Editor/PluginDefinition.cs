using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(dev.logilabo.parameter_smoother.editor.PluginDefinition))]

// ReSharper disable once CheckNamespace
namespace dev.logilabo.parameter_smoother.editor
{
    public class PluginDefinition : Plugin<PluginDefinition>
    {
        public override string QualifiedName => "dev.logilabo.parameter_smoother";
        public override string DisplayName => "Parameter Smoother";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .Run(ParameterSmootherPass.Instance);
        }
    }
}
