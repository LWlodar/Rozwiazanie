namespace ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var reader = new DataReader();
            reader.ImportAndPrintData("data.csv");
        }
    }
}
