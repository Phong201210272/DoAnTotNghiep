using System;
using System.Collections.Generic;

namespace DoAnTotNghiep.Models;

public partial class Admin
{
    public string AdminId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
