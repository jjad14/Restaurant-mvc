using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Models
{
    // order header should contain important information about order like when was the order placed
    // What is the pick up time? was any coupon code used? what was the order total? what was the payment status?
    public class OrderHeader
    {
        public int Id { get; set; }


        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }


        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public double OrderTotalOriginal { get; set; } // total before coupons and discounts applied

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Order Total")]
        public double OrderTotal { get; set; } // after coupons and discounts applied


        [Required]
        [Display(Name = "Pickup Time")]
        public DateTime PickupTime { get; set; }

        [Required]
        [NotMapped]
        public DateTime PickupDate { get; set; }


        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }
        public double CouponCodeDiscount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Comments { get; set; }


        [Display(Name = "Pickup Name")]
        public string PickupName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string TransactionId { get; set; }


    }
}
