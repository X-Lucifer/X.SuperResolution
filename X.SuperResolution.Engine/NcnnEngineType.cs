// ReSharper disable InconsistentNaming
namespace X.SuperResolution.Engine;

/// <summary>
/// Native engine type exported by lucifer_ncnn_vulkan.dll.
/// </summary>
public enum NcnnEngineType
{
    RealESRGAN = 0,
    SRMD = 1,
    Waifu2x = 2
}