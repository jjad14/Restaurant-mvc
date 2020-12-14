using Restaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Utility
{
    // Static Details
    public static class SD
    {
        public const string DefaultFoodImage = "default_food.png";

        public const string ManagerUser = "Manager";
        public const string KitchenUser = "Kitchen";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustomerUser = "Customer";

        public const string ssShoppingCartCount = "ssCartCount";

        public const string ssCouponCode = "ssCouponCode";

        public const string StatusSubmitted = "Submitted";
        public const string StatusInProcess = "Being Prepared";
        public const string StatusReady = "Ready for Pickup";
        public const string StatusCompleted = "Order Completed";
        public const string StatusCancelled = "Order Cancelled";

        public const string PaymentStatusPending = "Payment Pending";
        public const string PaymentStatusApproved = "Payment Approved";
        public const string PaymentStatusRejected = "Payment Rejected";


		// This function will convert HTML to raw HTML
		// it checks for the syntax for greater than and less then and converts them
		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}

		// Determines the total price of the order after the coupon is applied
		public static double DiscountedPrice(Coupon couponFromDb, double OriginalOrderTotal)
        {
			// no coupon
			if (couponFromDb == null)
            {
				return OriginalOrderTotal;
            }
			else
            {
				// coupon is not valid
				if (couponFromDb.MinimumAmount > OriginalOrderTotal)
                {
					return OriginalOrderTotal;
				}
				else
                {
					// coupon is valid
					if(Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECouponType.Dollar)
                    {
						// $10 off $100 purchase
						return Math.Round(OriginalOrderTotal - couponFromDb.Discount, 2);
                    }

					if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECouponType.Percent)
                    {
						// %10 off $100 purchase
						return Math.Round(OriginalOrderTotal - (OriginalOrderTotal * couponFromDb.Discount / 100), 2);
					}

					
                }
            }
			return OriginalOrderTotal;
        }


	}
}
