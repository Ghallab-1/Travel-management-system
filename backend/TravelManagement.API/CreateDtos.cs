using System;

namespace TravelManagement.API.Models
{
    public class CreateTravelApprovalDto
    {
        public int TravelRequestId { get; set; }
        public int ApproverId { get; set; }
        public string ApprovalLevel { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class CreateTravelBookingDto
    {
        public int TravelRequestId { get; set; }
        public string BookingStatus { get; set; } = "Pending";
        public string? Notes { get; set; }
    }

    public class CreateFlightDto
    {
        public int TravelBookingId { get; set; }
        public int AirlineId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
    }

    public class CreateHotelReservationDto
    {
        public int TravelBookingId { get; set; }
        public int HotelId { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public string BookingReference { get; set; } = string.Empty;
    }

    public class CreateExpenseDto
    {
        public int TravelRequestId { get; set; }
        public int UserId { get; set; }
        public int ExpenseCategoryId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateOnly ExpenseDate { get; set; }
    }
}
