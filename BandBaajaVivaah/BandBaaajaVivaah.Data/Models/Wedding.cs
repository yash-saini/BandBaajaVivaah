using System;
using System.Collections.Generic;

namespace BandBaaajaVivaah.Data.Models;

public partial class Wedding
{
    public int WeddingId { get; set; }

    public string WeddingName { get; set; } = null!;

    public DateTime WeddingDate { get; set; }

    public decimal TotalBudget { get; set; }

    public int OwnerUserId { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<Guest> Guests { get; set; } = new List<Guest>();

    public virtual User OwnerUser { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
