# Parameter Smoother

Non-destructive Parameter Smoothing for Avatars 3.0

## How to use
- Install [Logilabo Avatar Tools VPM repository](https://vpm.logilabo.dev) and import "Parameter Smoother".
- Add "Logilabo Avatar Tools / Parameter Smoother" component to an object in the target avatar.
- Fill configuration parameters.

### Example
| Name                      | Value     |
|---------------------------|-----------|
| Layer Type                | FX        |
| Smoothed Parameter Suffix | /Smoothed |
| Parameter Name (0)        | Foo       |
| Local Smoothness (0)      | 0.1       |
| Remote Smoothness (0)     | 0.7       |

With this configuration, you can use the parameter `Foo/Smoothed` as smoothed `Foo` in the FX layer.

## References
- [Advanced Blend Tree Techniques](https://notes.sleightly.dev/advanced-blendtrees/)
- [NDM Framework](https://ndmf.nadena.dev/)

