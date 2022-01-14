using Microsoft.OpenApi.Models;
using Azure;
using Azure.Data.Tables;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo {
         Title = "l3moni-bike-register-api",
         Version = "v1",
         Description = "This is a API for registering your bike against thefts in Finland",
         Contact = new OpenApiContact
         {
             Name = "Mikko Kasanen",
             Url = new Uri("https://lemoni.cloud"),
             Email = "mikko.kasanen@microsoft.com"
         },
});
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
var connectionString = Environment.GetEnvironmentVariable("connectionString");
var tablename = "l3monibikeregister";

TableClient client = new TableClient(connectionString, tablename);

app.UseHttpsRedirection();

app.MapPost("/post",() => {}).WithName("Post");
app.MapGet("/get",() => {}).WithName("Get");
app.MapDelete("/delete",() => {}).WithName("Delete");
app.MapPut("/put",() => {}).WithName("Put");

app.MapPost("/registerbike", (string serialnumber) =>
{
    var partitionkey = "bicycles";
    var rowkey = serialnumber;
    var alterKey = Guid.NewGuid().ToString("N").Substring(0, 12); 
    var entity = new TableEntity(partitionkey, rowkey){
        {"Color","Black"},
        {"Make","Kona"},
        {"Model","Auris"},
        {"Status","Registered"},
        {"Owner",""},
        {"Location","Helsinki"},
        {"OwnerEmail", ""},
        {"AlterKey", alterKey}
    };
    //var bike = new Bike("1234A","Kona","Black");
    client.AddEntity(entity);
    return entity;
})
.WithName("PostRegisterBike");

app.MapGet("/getallbikes", () =>
{
    Console.WriteLine("Getting items:");

    var partitionkey = "bicycles";
    List<string> bikes = new List<string>();

    Pageable<TableEntity> entities = client.Query<TableEntity>(filter: $"PartitionKey eq '{partitionkey}'");
    foreach (TableEntity entity in entities)
    {

    bikes.Add($"{entity.GetString("RowKey")} : {entity.GetString("Make")} : {entity.GetString("Model")} : {entity.GetString("Color")}");

    Console.WriteLine($"{entity.GetString("Make")}: {entity.GetString("Model")}");
    }

   

  //return Results.Stream(streamResponse, "application/json");
    return bikes;
})
.WithName("GetAllBikes");

app.MapGet("/getstatus/{serialnumber}", (string serialnumber) =>
{
    
    //var partitionkey = "bicycles";
    List<string> results = new List<string>();

    Pageable<TableEntity> entities = client.Query<TableEntity>(ent => ent.PartitionKey == "bicycles" && ent.RowKey == serialnumber);
    foreach (TableEntity entity in entities)
    {
    results.Add($"Serialnumber: {entity.GetString("RowKey")}");
    results.Add($"Created: {entity.GetDateTime("Timestamp")}");
    results.Add($"Owner: {entity.GetString("Owner")}");
    results.Add($"Make: {entity.GetString("Make")}");
    results.Add($"Model: {entity.GetString("Model")}");
    results.Add($"Color: {entity.GetString("Color")}");
    results.Add($"Status: {entity.GetString("Status")}");
    // : {entity.GetString("Owner")} : {entity.GetDateTime("Timestamp")} : {entity.GetString("Make")} : {entity.GetString("Model")} : {entity.GetString("Color")} : {entity.GetString("Status")}");
    Console.WriteLine($"{entity.GetString("Make")}: {entity.GetString("Model")}");
    }

   

  //return Results.Stream(streamResponse, "application/json");
    return results;
})
.WithName("GetStatus");

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Bike(string? Id, string? Name, string? Color)
{

}