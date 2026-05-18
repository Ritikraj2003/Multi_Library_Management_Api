using Microsoft.AspNetCore.Http;

namespace Multi_Library_Management_Api.Models.DTOs
{
    // ─── Student DTOs ─────────────────────────────────────────────────────────

    public class CreateStudentDto
    {
        public int LibraryId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public IFormFile? StudentImage { get; set; }
        public IFormFile? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DOB { get; set; }
    }

    public class UpdateStudentDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public IFormFile? StudentImage { get; set; }
        public IFormFile? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DOB { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Photo { get; set; }
        public string? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DOB { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StudentListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string LibraryName { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public string? DocumentImage { get; set; }
        public string? DocumentType { get; set; }
        public DateTime? DOB { get; set; }
        public bool IsActive { get; set; }
    }

    // ─── StudentRegistration DTOs ─────────────────────────────────────────────

    public class CreateStudentRegistrationDto
    {
        public int LibraryId { get; set; }
        public int StudentId { get; set; }
        public int TableSeatId { get; set; }
        public int BatchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal SecurityAmount { get; set; }
        public string? Notes { get; set; }
        public string? RFIDCode { get; set; }
        public int CreatedBy { get; set; }
    }

    public class UpdateStudentRegistrationDto
    {
        public int Id { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyAmount { get; set; }
        public string? Notes { get; set; }
        public string? RFIDCode { get; set; }
        public RegistrationStatus Status { get; set; }
    }

    public class StudentRegistrationResponseDto
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TableSeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string TableNumber { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public string BatchTime { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal SecurityAmount { get; set; }
        public string? Notes { get; set; }
        public string? RFIDCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class StudentRegistrationListDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public int TableSeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal MonthlyAmount { get; set; }
        public string? RFIDCode { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RegistrationDetailsDto : StudentRegistrationResponseDto { }

    public class RenewRegistrationDto
    {
        public int LibraryId { get; set; }
        public int RegistrationId { get; set; }
        public int Months { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
    }

    // ─── Payment DTOs ─────────────────────────────────────────────────────────

    public class CreatePaymentDto
    {
        public int RegistrationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
    }

    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime NextDueDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
    }

    public class PaymentListDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
    }

    public class BatchStatusDto
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public string BatchTime { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }
        public bool IsDirectlyOccupied { get; set; }
    }

    public class SeatBatchStatusDto
    {
        public int TableSeatId { get; set; }
        public List<BatchStatusDto> Batches { get; set; } = new();
    }
}
