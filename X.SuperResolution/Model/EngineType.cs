// -- Function: EngineType.cs
// --- Project: X.SuperResolution
// ---- Remark:
// ---- Author: Lucifer
// ------ Date: 2026-01-24 22:01:27

using System.ComponentModel;
// ReSharper disable InconsistentNaming
namespace X.SuperResolution.Model;

/// <summary>
/// 引擎类型
/// </summary>
public enum EngineType
{
    /// <summary>
    /// waifu2x
    /// </summary>
    [Description("waifu2x")] waifu2x = 1 << 0,

    /// <summary>
    /// Real-ESRGAN
    /// </summary>
    [Description("Real-ESRGAN")] RealESRGAN = 1 << 1,

    /// <summary>
    /// srmd
    /// </summary>
    [Description("srmd")] srmd = 1 << 2
}