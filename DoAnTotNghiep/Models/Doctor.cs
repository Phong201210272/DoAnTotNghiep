using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class Doctor
{
    public string DoctorId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateOnly Dob { get; set; }

    public string Gender { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Experience { get; set; }

    public int Status { get; set; }

    public string Images { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
