using System;
using System.Timers;
namespace auctionServiceAPI.Models;
public class TimerTick
{
    private System.Timers.Timer timer;

   
    public TimerTick()
    {
        // Opret en ny timer med interval på en time (3600000 millisekunder)
        timer = new System.Timers.Timer(600);
        timer.Elapsed += TimerElapsed;
        timer.AutoReset = true; // Hvis du ønsker gentagende udløsninger
    }
    public void Start()
    {
        timer.Start();
    }

    public void Stop()
    {
        timer.Stop();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Kald din metode eller API her
        CallMethodOrApi();
    }

    private void CallMethodOrApi()
    {
        // Implementer logikken for at kalde din metode eller API her
        // f.eks.:
        Console.WriteLine("Timeren udløst!");
    }
}
