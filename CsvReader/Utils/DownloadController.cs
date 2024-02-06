namespace CsvReader.Utils;

public static class DownloadController
{
    public static async Task DownloadFile(string url, string fileName, int maxMinutesTimeout)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(maxMinutesTimeout);
        using var response = await client.GetAsync(url);
        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(fileName, FileMode.Create);
        await stream.CopyToAsync(fileStream);
        Console.WriteLine($"Successfully downloaded {fileName}.");
    }
}