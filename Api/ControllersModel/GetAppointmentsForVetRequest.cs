namespace Api.ControllersModel;

public class GetAppointmentsForVetRequest
{
    public Guid VetId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}