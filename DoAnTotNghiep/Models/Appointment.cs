using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class Appointment
{
    public string AppointmentId { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime? AppointmentTime { get; set; }

    public int Status { get; set; }
    public string PatientPhone { get; set; } = null!;
    public string PatientName { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string ServiceId { get; set; } = null!;

    public string DoctorId { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
