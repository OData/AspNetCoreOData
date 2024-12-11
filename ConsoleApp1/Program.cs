// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


for (int i = 0; i < 100; ++i)
{
    Console.WriteLine($"public int Property{i} {{ get; set; }}");
}

for (int i = 0; true; ++i)
{
    if (i % 100 == 0)
    {
        Console.WriteLine(i);
    }

    using (var httpClient = new HttpClient())
    {
        using (var httpResponse = await httpClient.GetAsync("http://localhost:64771/api/Demo?$filter=Id eq 5"))
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
        }
    }
}