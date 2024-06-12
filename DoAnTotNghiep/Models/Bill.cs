using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class Bill
{
    public string BillId { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public int Total { get; set; }

    public string UserId { get; set; } = null!;

    public string AdminId { get; set; } = null!;

    public string AppointmentId { get; set; } = null!; // Đảm bảo thuộc tính này không null

    public virtual Admin Admin { get; set; } = null!;

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual User User { get; set; } = null!;

    public virtual Appointment Appointment { get; set; } = null!;
}
