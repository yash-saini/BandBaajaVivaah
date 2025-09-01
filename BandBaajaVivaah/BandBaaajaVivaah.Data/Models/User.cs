using System;
using System.Collections.Generic;

namespace BandBaaajaVivaah.Data.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<Wedding> Weddings { get; set; } = new List<Wedding>();
}
