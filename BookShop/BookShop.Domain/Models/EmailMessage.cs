﻿namespace BookShop.Domain.Models;

public class EmailMessage
{
    public string ToEmail { get; set; } = null!;
    public string ToName { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
}