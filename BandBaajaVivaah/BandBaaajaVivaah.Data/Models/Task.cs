using System;
using System.Collections.Generic;

namespace BandBaaajaVivaah.Data.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public int WeddingId { get; set; }

    public virtual Wedding Wedding { get; set; } = null!;
}
