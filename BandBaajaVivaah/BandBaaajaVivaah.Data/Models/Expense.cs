using System;
using System.Collections.Generic;

namespace BandBaaajaVivaah.Data.Models;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Category { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public int WeddingId { get; set; }

    public virtual Wedding Wedding { get; set; } = null!;
}
