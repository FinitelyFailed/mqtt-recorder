using System.Threading.Tasks;

public interface ISubApp 
{
    Task StartAsync();
    Task StopAsync();
}