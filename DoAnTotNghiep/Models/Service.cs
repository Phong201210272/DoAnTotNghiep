using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class Service
{
    public string ServiceId { get; set; } = null!;

    public string ServiceName { get; set; } = null!;

    public int Cost { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
}
