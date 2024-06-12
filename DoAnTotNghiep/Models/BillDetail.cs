using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class BillDetail
{
    public string? NoteMedicine { get; set; }

    public string BillId { get; set; } = null!;
    public string DoctorId {  get; set; } = null!;

    public string ServiceId { get; set; } = null!;

    public virtual Bill Bill { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;
}
