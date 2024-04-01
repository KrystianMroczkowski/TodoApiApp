using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using TodoLibrary.DataAccess;

namespace TodoApi.StartupConfig;

public static class DependencyInjectionExtensions
{
    public static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.AddSwaggerServices();
    }

    private static void AddSwaggerServices(this WebApplicationBuilder builder)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header info using bearer tokens",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        var securityRequirments = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "bearerAuth"
                    }
                },
                new string[] {}
            }
        };

		var title = "Version 1 API";
		var description = "";
		var terms = new Uri("https://localhost:7284/terms");
		var license = new OpenApiLicense()
		{
			Name = "This is my full license information or a link to it."
		};
		var contact = new OpenApiContact()
		{
			Name = "Krys Mro Helpdesk",
			Email = "test@email.com",
			Url = new Uri("https://www.google.com"),
		};

		builder.Services.AddSwaggerGen(opts =>
        {
            opts.AddSecurityDefinition("bearerAuth", securityScheme);
            opts.AddSecurityRequirement(securityRequirments);
			opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
				$"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

			opts.SwaggerDoc("v1", new OpenApiInfo()
			{
				Version = "v1",
				Title = $"{title} v1 (latest version)",
				Description = description,
				TermsOfService = terms,
				License = license,
				Contact = contact
			});
		});
	}
    public static void AddApiVersioningServices(this WebApplicationBuilder builder)
    {
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new(1, 0);
			options.ReportApiVersions = true;
		}).AddMvc().AddApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'VVV";
			options.SubstituteApiVersionInUrl = true;
			options.AddApiVersionParametersWhenVersionNeutral = true;
		});
	}
    public static void AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        builder.Services.AddSingleton<ITodoData, TodoData>();
    }

    public static void AddHealthCheckServices(this WebApplicationBuilder builder)
    {
		builder.Services.AddHealthChecksUI(options =>
		{
			options.AddHealthCheckEndpoint("api", "/health");
			options.SetEvaluationTimeInSeconds(5);
			options.SetMinimumSecondsBetweenFailureNotifications(10);
		}).AddInMemoryStorage();

		builder.Services.AddHealthChecks()
            .AddSqlServer(builder.Configuration.GetConnectionString("Default"));
    }

    public static void AddAuthServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                    ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(
                        builder.Configuration.GetValue<string>("Authentication:SecretKey")))
                };
            });

        builder.Services.AddAuthorization(opts =>
        {
            opts.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();
        });
    }
}
