using System;
using System.Collections.Generic;

namespace BandBaaajaVivaah.Data.Models;

public partial class Guest
{
    public int GuestId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Side { get; set; } = null!;

    public string Rsvpstatus { get; set; } = null!;

    public int WeddingId { get; set; }

    public virtual Wedding Wedding { get; set; } = null!;
}
