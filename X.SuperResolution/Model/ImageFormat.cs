// -- Function: ImageFormat.cs
// --- Project: X.SuperResolution
// ---- Remark:
// ---- Author: Lucifer
// ------ Date: 2026-01-24 22:01:53

using System.ComponentModel;
// ReSharper disable InconsistentNaming
namespace X.SuperResolution.Model;

/// <summary>
///图片格式类型 
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// jpg
    /// </summary>
    [Description("jpg")] jpg = 1 << 0,

    /// <summary>
    /// png
    /// </summary>
    [Description("png")] png = 1 << 1,

    /// <summary>
    /// webp
    /// </summary>
    [Description("webp")] webp = 1 << 2
}