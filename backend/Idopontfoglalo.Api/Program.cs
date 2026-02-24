using System.Text;
using Idopontfoglalo.Api.Middleware;
using Idopontfoglalo.Core.Interfaces;
using Idopontfoglalo.Infrastructure.Data;
using Idopontfoglalo.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();


var jwtKey = builder.Configuration["Jwt:Key"] ?? "SUPER_DEMO_KEY_CHANGE_ME_32_CHARS_MINIMUM";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "idopontfoglalo";
var audience = builder.Configuration["Jwt:Audience"] ?? "idopontfoglalo";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await EnsureLocationSchemaAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowFrontend");    

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

static async Task EnsureLocationSchemaAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SchemaInit");

    var bootstrapCommands = new[]
      {
        """
        CREATE TABLE IF NOT EXISTS locations (
            id INT AUTO_INCREMENT PRIMARY KEY,
            name VARCHAR(120) NOT NULL UNIQUE,
            is_active TINYINT(1) NOT NULL DEFAULT 1
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        """,
    "INSERT IGNORE INTO locations (name, is_active) VALUES ('Fodrászat', 1), ('Kozmetika', 1);"
    };

    foreach (var sql in bootstrapCommands)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Schema init command skipped: {Sql}", sql);
        }
    }
    try
    {
        var connection = (MySqlConnection)db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var schema = connection.Database;

        if (!await ColumnExistsAsync(connection, schema, "employees", "location_id"))
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE employees ADD COLUMN location_id INT NULL;");

        if (!await ForeignKeyExistsAsync(connection, schema, "employees", "fk_employees_location"))
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE employees ADD CONSTRAINT fk_employees_location FOREIGN KEY (location_id) REFERENCES locations(id) ON DELETE SET NULL;");
        }

        await db.Database.ExecuteSqlRawAsync(
            "UPDATE employees SET location_id = (SELECT id FROM locations WHERE name='Fodrászat' LIMIT 1) WHERE location_id IS NULL;");
    }
    catch (Exception ex)
    {
        logger.LogInformation(ex, "Schema verification skipped.");
    }

}
static async Task<bool> ColumnExistsAsync(MySqlConnection connection, string schema, string table, string column)
{
    await using var command = connection.CreateCommand();
    command.CommandText = @"SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table AND COLUMN_NAME = @column;";
    command.Parameters.AddWithValue("@schema", schema);
    command.Parameters.AddWithValue("@table", table);
    command.Parameters.AddWithValue("@column", column);

    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
    return count > 0;
}

static async Task<bool> ForeignKeyExistsAsync(MySqlConnection connection, string schema, string table, string constraintName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = @"SELECT COUNT(*)
FROM information_schema.TABLE_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = @schema
  AND TABLE_NAME = @table
  AND CONSTRAINT_NAME = @constraintName
  AND CONSTRAINT_TYPE = 'FOREIGN KEY';";
    command.Parameters.AddWithValue("@schema", schema);
    command.Parameters.AddWithValue("@table", table);
    command.Parameters.AddWithValue("@constraintName", constraintName);

    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
    return count > 0;
}

