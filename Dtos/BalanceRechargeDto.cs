﻿using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class BalanceRechargeDto
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
