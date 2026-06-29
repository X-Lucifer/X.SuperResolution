// -- Function: UpdateVisitor.cs
// --- Project: X.SuperResolution
// ---- Remark:
// ---- Author: Lucifer
// ------ Date: 2026-01-24 23:01:08

using LibreHardwareMonitor.Hardware;

namespace X.SuperResolution.Services;

public class UpdateVisitor : IVisitor
{
    /// <inheritdoc />
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    /// <inheritdoc />
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (var item in hardware.SubHardware)
        {
            item.Accept(this);
        }
    }

    /// <inheritdoc />
    public void VisitSensor(ISensor sensor)
    {
    }

    /// <inheritdoc />
    public void VisitParameter(IParameter parameter)
    {
    }
}