using System.Text.Json;

// Récupère une blague sur api.chucknorris.io (appel synchrone, pas d'async)
public static class JokeService
{
    // Un seul HttpClient partagé pour toute l'application
    private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

    private const string Url = "https://api.chucknorris.io/jokes/random";

    // Renvoie la blague (champ JSON "value") ; lève une exception en cas d'échec
    public static string GetJoke()
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url);
        using HttpResponseMessage response = _http.Send(request);
        response.EnsureSuccessStatusCode();
        using Stream body = response.Content.ReadAsStream();
        using JsonDocument doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("value").GetString() ?? "";
    }
}
