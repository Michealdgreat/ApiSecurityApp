using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiSecurity.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy(PolicyConstantClass.MustHaveEmployeeID, policy =>
    {
        policy.RequireClaim("employeeID");
    });
    opts.AddPolicy(PolicyConstantClass.MustBeTheOwner, policy =>
    {
        policy.RequireClaim("employeeID", "Business Owner"); // policy with more specific value
    });

    opts.AddPolicy(PolicyConstantClass.MustBeAVeteranEmployee, policy =>
    {
        policy.RequireClaim("employeeID", "ES001", "ES002", "ES003");
    });

    opts.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
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
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Authentication:SecretKey")))
                 };
             });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
